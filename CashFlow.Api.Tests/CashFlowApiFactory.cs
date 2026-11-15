using System.Net.Http.Headers;
using System.Net.Http.Json;
using CashFlow.Api.Dtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

namespace CashFlow.Api.Tests
{
    // Sobe a API inteira em memória, com um banco SQLite temporário só para os testes.
    public class CashFlowApiFactory : WebApplicationFactory<Program>
    {
        // Chave usada só nos testes. Os testes também usam para forjar tokens (expirado, etc.).
        public const string ChaveJwt = "chave-de-teste-com-mais-de-32-bytes-0123456789";
        public const string Issuer = "CashFlow.Api";
        public const string Audience = "CashFlow.Web";

        private readonly string _caminhoBanco =
            Path.Combine(Path.GetTempPath(), $"cashflow-testes-{Guid.NewGuid():N}.db");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Default", $"Data Source={_caminhoBanco}");
            builder.UseSetting("Jwt:Key", ChaveJwt);
            builder.UseSetting("Jwt:Issuer", Issuer);
            builder.UseSetting("Jwt:Audience", Audience);

            // Limites altos por padrão para os testes não se atrapalharem.
            // O teste de rate limit sobrescreve com um valor baixo.
            builder.UseSetting("RateLimit:AuthPermitLimit", "1000");
            builder.UseSetting("RateLimit:GlobalPermitLimit", "10000");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                SqliteConnection.ClearAllPools();
                try { File.Delete(_caminhoBanco); } catch (IOException) { }
            }
        }

        // Cria um usuário novo, faz login e devolve um HttpClient já com o token
        public async Task<HttpClient> CriarClienteAutenticadoAsync()
        {
            var client = CreateClient();
            var token = await ObterTokenAsync(client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        public static async Task<string> ObterTokenAsync(HttpClient client)
        {
            var email = $"usuario-{Guid.NewGuid():N}@teste.com";
            const string senha = "senha-segura-123";

            var registro = await client.PostAsJsonAsync("/api/auth/registrar", new { email, senha });
            registro.EnsureSuccessStatusCode();

            var login = await client.PostAsJsonAsync("/api/auth/login", new { email, senha });
            login.EnsureSuccessStatusCode();

            var token = await login.Content.ReadFromJsonAsync<TokenResponse>();
            return token!.Token;
        }
    }
}
