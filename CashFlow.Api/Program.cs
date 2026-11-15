using System.Globalization;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using CashFlow.Api.Entities;
using CashFlow.Api.Infrastructure;
using CashFlow.Api.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Não anuncia o servidor (Kestrel) no header "Server"
builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

// Adiciona suporte a Controllers (rotas)
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;

    // Campos que não existem no DTO fazem a requisição falhar com 400 em vez de serem ignorados
    options.JsonSerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
});

// Erros saem no formato padrão ProblemDetails (RFC 9457), sem stack trace
builder.Services.AddProblemDetails();

// Configura o Banco de Dados (SQLite). A connection string é lida na hora de criar o contexto,
// o que permite aos testes usarem um banco temporário.
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
    options.UseSqlite(sp.GetRequiredService<IConfiguration>().GetConnectionString("Default") ?? "Data Source=cashflow.db"));

// Registra os serviços para poderem ser usados nos Controllers
builder.Services.AddScoped<CashFlow.Api.Services.TransacaoService>();
builder.Services.AddSingleton<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
builder.Services.AddSingleton<SigningKeyProvider>();
builder.Services.AddSingleton<TokenService>();

// ---------- Autenticação JWT ----------
builder.Services.AddOptions<JwtSettings>().BindConfiguration("Jwt");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtSettings>, SigningKeyProvider>((options, jwt, keys) =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Value.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Value.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = keys.Key,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            RequireSignedTokens = true,
            // Só aceita HS256: bloqueia tokens com "alg": "none" ou algoritmo trocado
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = "email"
        };
    });

// Seguro por padrão: todo endpoint exige login, a não ser que esteja marcado com [AllowAnonymous]
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// ---------- Limite de tentativas (rate limiting) ----------
builder.Services.AddOptions<RateLimitSettings>().BindConfiguration("RateLimit");

static string ChaveCliente(HttpContext context) =>
    context.Connection.RemoteIpAddress?.ToString() ?? "desconhecido";

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, cancellationToken) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter =
                ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
        }

        await context.HttpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status429TooManyRequests,
            Title = "Muitas tentativas. Aguarde um pouco e tente novamente."
        }, cancellationToken);
    };

    // Login e cadastro: poucas tentativas por IP numa janela fixa (contra força bruta e credential stuffing)
    options.AddPolicy(RateLimitPolicies.Auth, context =>
    {
        var cfg = context.RequestServices.GetRequiredService<IOptions<RateLimitSettings>>().Value;
        return RateLimitPartition.GetFixedWindowLimiter(ChaveCliente(context), _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = cfg.AuthPermitLimit,
            Window = TimeSpan.FromSeconds(cfg.AuthJanelaSegundos),
            QueueLimit = 0
        });
    });

    // Limite geral por IP para a API inteira
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var cfg = context.RequestServices.GetRequiredService<IOptions<RateLimitSettings>>().Value;
        return RateLimitPartition.GetFixedWindowLimiter(ChaveCliente(context), _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = cfg.GlobalPermitLimit,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        });
    });
});

// ---------- CORS ----------
// Antes: AllowAnyOrigin. Agora só as origens listadas em "Cors:Origens" podem chamar a API pelo navegador.
builder.Services.AddCors();
builder.Services.AddOptions<CorsOptions>().Configure<IConfiguration>((options, config) =>
{
    var origens = config.GetSection("Cors:Origens").Get<string[]>() ?? [];
    options.AddPolicy("PermitirFrontend", policy => policy
        .WithOrigins(origens)
        .WithMethods("GET", "POST", "DELETE")
        .WithHeaders("Authorization", "Content-Type"));
});

// OpenAPI (Swagger) Para facilitar seus testes
builder.Services.AddOpenApi();

var app = builder.Build();

// Valida a configuração do JWT na subida (fail fast): sem chave em produção, a API nem inicia
app.Services.GetRequiredService<SigningKeyProvider>();

// Automação do Banco de Dados
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Headers de segurança em todas as respostas
app.UseSecurityHeaders();

// Exceções não tratadas viram um 500 genérico (sem vazar detalhes internos)
app.UseExceptionHandler();

// Configurações de ambiente
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();

// Aplica a regra do CORS
app.UseCors("PermitirFrontend");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// Mapeia os Controllers para que as rotas funcionem
app.MapControllers();

app.Run();

// Permite que os testes de integração (WebApplicationFactory) acessem a classe Program
public partial class Program { }
