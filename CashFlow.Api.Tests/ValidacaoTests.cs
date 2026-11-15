using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CashFlow.Api.Dtos;

namespace CashFlow.Api.Tests
{
    public class ValidacaoTests : IClassFixture<CashFlowApiFactory>
    {
        private readonly CashFlowApiFactory _factory;

        public ValidacaoTests(CashFlowApiFactory factory)
        {
            _factory = factory;
        }

        private static async Task<PessoaResponse> CriarPessoa(HttpClient client, string nome, int idade)
        {
            var resposta = await client.PostAsJsonAsync("/api/pessoas", new { nome, idade });
            resposta.EnsureSuccessStatusCode();
            return (await resposta.Content.ReadFromJsonAsync<PessoaResponse>())!;
        }

        [Theory]
        [InlineData("", 30)]                              // nome vazio
        [InlineData("A", 30)]                             // nome curto demais
        [InlineData("<script>alert(1)</script>", 30)]     // caracteres fora da lista permitida
        [InlineData("Maria", -1)]                         // idade negativa
        [InlineData("Maria", 200)]                        // idade absurda
        public async Task Pessoa_invalida_devolve_400(string nome, int idade)
        {
            var client = await _factory.CriarClienteAutenticadoAsync();
            var resposta = await client.PostAsJsonAsync("/api/pessoas", new { nome, idade });

            Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        }

        [Fact]
        public async Task Nome_com_acento_e_apostrofo_e_aceito()
        {
            var client = await _factory.CriarClienteAutenticadoAsync();
            var pessoa = await CriarPessoa(client, "Conceição D'Ávila", 40);

            Assert.Equal("Conceição D'Ávila", pessoa.Nome);
        }

        [Fact]
        public async Task Mass_assignment_de_transacoes_na_pessoa_e_bloqueado()
        {
            // Ataque: menor de idade cadastrada já com uma "Receita" embutida, burlando a regra de negócio
            var client = await _factory.CriarClienteAutenticadoAsync();
            const string nome = "Menor Infiltrada";

            var resposta = await client.PostAsJsonAsync("/api/pessoas", new
            {
                nome,
                idade = 15,
                transacoes = new[] { new { descricao = "Salário", valor = 5000m, tipo = "Receita" } }
            });

            Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);

            // E a pessoa não foi criada
            var totais = await client.GetFromJsonAsync<JsonElement>("/api/pessoas/totais");
            var nomes = totais.GetProperty("pessoas").EnumerateArray().Select(p => p.GetProperty("nome").GetString());
            Assert.DoesNotContain(nome, nomes);
        }

        [Fact]
        public async Task Cliente_nao_consegue_definir_o_id()
        {
            var client = await _factory.CriarClienteAutenticadoAsync();
            var resposta = await client.PostAsJsonAsync("/api/pessoas", new { id = 9999, nome = "Paulo", idade = 30 });

            Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        }

        [Theory]
        [InlineData("Mercado", 50.0, "Investimento")]   // tipo fora da lista
        [InlineData("Mercado", 0.0, "Despesa")]          // valor zero
        [InlineData("Mercado", -10.0, "Despesa")]        // valor negativo
        [InlineData("Mercado", 10.555, "Despesa")]       // mais de 2 casas decimais
        [InlineData("", 10.0, "Despesa")]                // descrição vazia
        public async Task Transacao_invalida_devolve_400(string descricao, double valor, string tipo)
        {
            var client = await _factory.CriarClienteAutenticadoAsync();
            var pessoa = await CriarPessoa(client, "Ana", 30);

            var resposta = await client.PostAsJsonAsync("/api/transacoes", new
            {
                descricao,
                valor = (decimal)valor,
                tipo,
                pessoaId = pessoa.Id
            });

            Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        }

        [Fact]
        public async Task Regra_de_negocio_menor_de_idade_continua_valendo()
        {
            var client = await _factory.CriarClienteAutenticadoAsync();
            var menor = await CriarPessoa(client, "Lucas", 16);

            var receita = await client.PostAsJsonAsync("/api/transacoes", new { descricao = "Mesada", valor = 100m, tipo = "Receita", pessoaId = menor.Id });
            var despesa = await client.PostAsJsonAsync("/api/transacoes", new { descricao = "Lanche", valor = 20m, tipo = "Despesa", pessoaId = menor.Id });

            Assert.Equal(HttpStatusCode.BadRequest, receita.StatusCode);
            Assert.Equal(HttpStatusCode.OK, despesa.StatusCode);
        }

        [Fact]
        public async Task Erro_nao_vaza_detalhes_internos()
        {
            var client = await _factory.CriarClienteAutenticadoAsync();
            var resposta = await client.PostAsJsonAsync("/api/transacoes", new { descricao = "Teste", valor = 10m, tipo = "Despesa", pessoaId = 987654 });
            var corpo = await resposta.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
            Assert.DoesNotContain("Exception", corpo);
            Assert.DoesNotContain("   at ", corpo); // stack trace
        }
    }
}
