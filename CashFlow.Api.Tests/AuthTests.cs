using System.Net;
using System.Net.Http.Json;
using CashFlow.Api.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace CashFlow.Api.Tests
{
    public class AuthTests : IClassFixture<CashFlowApiFactory>
    {
        private readonly HttpClient _client;

        public AuthTests(CashFlowApiFactory factory)
        {
            _client = factory.CreateClient();
        }

        private static string NovoEmail() => $"auth-{Guid.NewGuid():N}@teste.com";

        [Fact]
        public async Task Registrar_e_logar_devolve_token()
        {
            var email = NovoEmail();

            var registro = await _client.PostAsJsonAsync("/api/auth/registrar", new { email, senha = "senha-segura-123" });
            Assert.Equal(HttpStatusCode.Created, registro.StatusCode);

            var login = await _client.PostAsJsonAsync("/api/auth/login", new { email, senha = "senha-segura-123" });
            Assert.Equal(HttpStatusCode.OK, login.StatusCode);

            var token = await login.Content.ReadFromJsonAsync<TokenResponse>();
            Assert.False(string.IsNullOrWhiteSpace(token!.Token));
            Assert.True(token.ExpiraEm > DateTime.UtcNow);
        }

        [Fact]
        public async Task Resposta_do_registro_nao_expoe_hash_da_senha()
        {
            var resposta = await _client.PostAsJsonAsync("/api/auth/registrar", new { email = NovoEmail(), senha = "senha-segura-123" });
            var corpo = await resposta.Content.ReadAsStringAsync();

            Assert.DoesNotContain("senha", corpo, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Email_e_normalizado_e_nao_permite_cadastro_duplicado()
        {
            var email = NovoEmail();
            await _client.PostAsJsonAsync("/api/auth/registrar", new { email, senha = "senha-segura-123" });

            // Mesmo e-mail com maiúsculas e espaços
            var duplicado = await _client.PostAsJsonAsync("/api/auth/registrar", new { email = $"  {email.ToUpperInvariant()} ", senha = "outra-senha-123" });

            Assert.Equal(HttpStatusCode.Conflict, duplicado.StatusCode);
        }

        [Theory]
        [InlineData("nao-e-email", "senha-segura-123")]
        [InlineData("valido@teste.com", "curta")]
        [InlineData("", "senha-segura-123")]
        public async Task Registro_com_dados_invalidos_devolve_400(string email, string senha)
        {
            var resposta = await _client.PostAsJsonAsync("/api/auth/registrar", new { email, senha });

            Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        }

        [Fact]
        public async Task Login_nao_revela_se_o_email_existe()
        {
            var email = NovoEmail();
            await _client.PostAsJsonAsync("/api/auth/registrar", new { email, senha = "senha-segura-123" });

            var senhaErrada = await _client.PostAsJsonAsync("/api/auth/login", new { email, senha = "senha-errada-123" });
            var emailInexistente = await _client.PostAsJsonAsync("/api/auth/login", new { email = NovoEmail(), senha = "senha-segura-123" });

            Assert.Equal(HttpStatusCode.Unauthorized, senhaErrada.StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, emailInexistente.StatusCode);

            var erro1 = await senhaErrada.Content.ReadFromJsonAsync<ProblemDetails>();
            var erro2 = await emailInexistente.Content.ReadFromJsonAsync<ProblemDetails>();
            Assert.Equal(erro1!.Title, erro2!.Title);
        }
    }
}
