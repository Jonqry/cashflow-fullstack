using CashFlow.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Adiciona suporte a Controllers (rotas)
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// Configura o Banco de Dados (SQLite)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=cashflow.db"));

// Registra o serviço para poder ser usado nos Controllers
builder.Services.AddScoped<CashFlow.Api.Services.TransacaoService>();

// Configura o CORS (Permite que o React na porta 5173 acesse a API na porta 5000/5001)
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

// OpenAPI (Swagger) Para facilitar seus testes
builder.Services.AddOpenApi();

var app = builder.Build();

// Automação do Banco de Dados
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate(); 
}

// Configurações de ambiente
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Aplica a regra do CORS
app.UseCors("PermitirFrontend");

app.UseAuthorization();

// Mapeia os Controllers para que as rotas funcionem
app.MapControllers();

app.Run();