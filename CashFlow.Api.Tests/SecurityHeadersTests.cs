using System.Net.Http.Json;

namespace CashFlow.Api.Tests
{
    public class SecurityHeadersTests : IClassFixture<CashFlowApiFactory>
    {
        private readonly CashFlowApiFactory _factory;

        public SecurityHeadersTests(CashFlowApiFactory factory)
        {
            _factory = factory;
        }

        public static TheoryData<string, string> HeadersEsperados => new()
        {
            { "X-Content-Type-Options", "nosniff" },
            { "X-Frame-Options", "DENY" },
            { "Referrer-Policy", "no-referrer" },
            { "Cross-Origin-Opener-Policy", "same-origin" },
            { "Content-Security-Policy", "default-src 'none'; frame-ancestors 'none'; base-uri 'none'; form-action 'none'" },
            { "Permissions-Policy", "camera=(), microphone=(), geolocation=(), payment=()" },
        };

        [Theory]
        [MemberData(nameof(HeadersEsperados))]
        public async Task Resposta_de_sucesso_tem_headers_de_seguranca(string header, string valor)
        {
            var client = await _factory.CriarClienteAutenticadoAsync();
            var resposta = await client.GetAsync("/api/pessoas/totais");

            Assert.True(resposta.Headers.TryGetValues(header, out var valores), $"Header {header} ausente");
            Assert.Equal(valor, valores!.Single());
        }

        [Theory]
        [MemberData(nameof(HeadersEsperados))]
        public async Task Resposta_de_erro_tambem_tem_headers_de_seguranca(string header, string valor)
        {
            var client = _factory.CreateClient();
            var resposta = await client.PostAsJsonAsync("/api/auth/login", new { email = "x@teste.com", senha = "errada-123456" });

            Assert.True(resposta.Headers.TryGetValues(header, out var valores), $"Header {header} ausente");
            Assert.Equal(valor, valores!.Single());
        }

        [Fact]
        public async Task Respostas_da_api_nao_ficam_em_cache()
        {
            var client = await _factory.CriarClienteAutenticadoAsync();
            var resposta = await client.GetAsync("/api/pessoas/totais");

            Assert.True(resposta.Headers.CacheControl?.NoStore);
        }
    }
}
