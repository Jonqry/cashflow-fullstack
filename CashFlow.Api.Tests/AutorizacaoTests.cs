using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CashFlow.Api.Tests
{
    public class AutorizacaoTests : IClassFixture<CashFlowApiFactory>
    {
        private const string RotaProtegida = "/api/pessoas/totais";
        private readonly CashFlowApiFactory _factory;

        public AutorizacaoTests(CashFlowApiFactory factory)
        {
            _factory = factory;
        }

        private async Task<HttpStatusCode> GetComToken(string? token)
        {
            var client = _factory.CreateClient();
            if (token is not null)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            var resposta = await client.GetAsync(RotaProtegida);
            return resposta.StatusCode;
        }

        // Monta um token com os parâmetros que o teste quiser, para simular ataques
        private static string ForjarToken(
            string chave = CashFlowApiFactory.ChaveJwt,
            string issuer = CashFlowApiFactory.Issuer,
            string audience = CashFlowApiFactory.Audience,
            DateTime? expira = null)
        {
            var exp = expira ?? DateTime.UtcNow.AddMinutes(10);
            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = issuer,
                Audience = audience,
                IssuedAt = exp.AddHours(-1),
                NotBefore = exp.AddHours(-1),
                Expires = exp,
                Subject = new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub, "1")]),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chave)), SecurityAlgorithms.HmacSha256)
            };
            return new JsonWebTokenHandler().CreateToken(descriptor);
        }

        [Fact]
        public async Task Sem_token_devolve_401()
        {
            Assert.Equal(HttpStatusCode.Unauthorized, await GetComToken(null));
        }

        [Fact]
        public async Task Com_token_valido_devolve_200()
        {
            var client = await _factory.CriarClienteAutenticadoAsync();
            var resposta = await client.GetAsync(RotaProtegida);

            Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        }

        [Fact]
        public async Task Token_forjado_corretamente_e_aceito()
        {
            // Garante que o ForjarToken funciona; assim os testes de rejeição abaixo são confiáveis
            Assert.Equal(HttpStatusCode.OK, await GetComToken(ForjarToken()));
        }

        [Fact]
        public async Task Token_expirado_devolve_401()
        {
            var token = ForjarToken(expira: DateTime.UtcNow.AddMinutes(-5));
            Assert.Equal(HttpStatusCode.Unauthorized, await GetComToken(token));
        }

        [Fact]
        public async Task Token_assinado_com_outra_chave_devolve_401()
        {
            var token = ForjarToken(chave: "chave-do-atacante-com-mais-de-32-bytes-xyz");
            Assert.Equal(HttpStatusCode.Unauthorized, await GetComToken(token));
        }

        [Fact]
        public async Task Token_de_outro_emissor_ou_publico_devolve_401()
        {
            Assert.Equal(HttpStatusCode.Unauthorized, await GetComToken(ForjarToken(issuer: "outro-sistema")));
            Assert.Equal(HttpStatusCode.Unauthorized, await GetComToken(ForjarToken(audience: "outro-app")));
        }

        [Fact]
        public async Task Token_com_payload_adulterado_devolve_401()
        {
            var partes = ForjarToken().Split('.');
            // Troca o payload mantendo a assinatura original
            var payloadFalso = Base64UrlEncoder.Encode("{\"sub\":\"999\",\"iss\":\"CashFlow.Api\",\"aud\":\"CashFlow.Web\",\"exp\":9999999999}");
            var adulterado = $"{partes[0]}.{payloadFalso}.{partes[2]}";

            Assert.Equal(HttpStatusCode.Unauthorized, await GetComToken(adulterado));
        }

        [Fact]
        public async Task Token_com_alg_none_devolve_401()
        {
            var header = Base64UrlEncoder.Encode("{\"alg\":\"none\",\"typ\":\"JWT\"}");
            var payload = Base64UrlEncoder.Encode("{\"sub\":\"1\",\"iss\":\"CashFlow.Api\",\"aud\":\"CashFlow.Web\",\"exp\":9999999999}");
            var semAssinatura = $"{header}.{payload}.";

            Assert.Equal(HttpStatusCode.Unauthorized, await GetComToken(semAssinatura));
        }
    }
}
