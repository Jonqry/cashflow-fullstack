using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;

namespace CashFlow.Api.Tests
{
    public class RateLimitTests : IClassFixture<CashFlowApiFactory>
    {
        private const int Limite = 3;
        private readonly CashFlowApiFactory _factory;

        public RateLimitTests(CashFlowApiFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Login_bloqueia_apos_exceder_o_limite_de_tentativas()
        {
            // Nova instância da API com limite baixo, para o teste ser rápido
            using var api = _factory.WithWebHostBuilder(b => b.UseSetting("RateLimit:AuthPermitLimit", Limite.ToString()));
            var client = api.CreateClient();
            var tentativa = new { email = "alvo@teste.com", senha = "chute-errado-123" };

            for (var i = 0; i < Limite; i++)
            {
                var resposta = await client.PostAsJsonAsync("/api/auth/login", tentativa);
                Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
            }

            var bloqueada = await client.PostAsJsonAsync("/api/auth/login", tentativa);

            Assert.Equal(HttpStatusCode.TooManyRequests, bloqueada.StatusCode);
            Assert.NotNull(bloqueada.Headers.RetryAfter);
        }

        [Fact]
        public async Task Rotas_comuns_nao_usam_o_limite_do_login()
        {
            using var api = _factory.WithWebHostBuilder(b => b.UseSetting("RateLimit:AuthPermitLimit", "1"));
            var client = api.CreateClient();

            // Rota protegida sem token: deve dar 401 sempre, nunca 429
            for (var i = 0; i < 5; i++)
            {
                var resposta = await client.GetAsync("/api/pessoas/totais");
                Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
            }
        }
    }
}
