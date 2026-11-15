using CashFlow.Api.Dtos;
using CashFlow.Api.Entities;
using CashFlow.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // exige token JWT válido
    public class PessoasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PessoasController(AppDbContext context)
        {
            _context = context;
        }

        // Função responsável por cadastrar uma nova pessoa no banco de dados.
        // Recebe um DTO (e não a entidade) para o cliente não conseguir definir Id nem enviar Transacoes junto.
        [HttpPost]
        public async Task<IActionResult> CriarPessoa(PessoaRequest request)
        {
            var pessoa = new Pessoa
            {
                Nome = request.Nome.Trim(),
                Idade = request.Idade
            };

            _context.Pessoas.Add(pessoa);
            await _context.SaveChangesAsync();
            return Ok(new PessoaResponse(pessoa.Id, pessoa.Nome, pessoa.Idade)); // Retorna HTTP 200 com os dados salvos
        }

        // Função que calcula e retorna os totais (Receitas, Despesas e Saldo) de cada pessoa e o total geral.
        [HttpGet("totais")]
        public async Task<IActionResult> ListarTotais()
        {
            // Busca todas as pessoas, incluindo suas transações para realizar o cálculo na memória
            var pessoas = await _context.Pessoas
                .Include(p => p.Transacoes)
                .Select(p => new {
                    p.Id,
                    p.Nome,
                    TotalReceitas = p.Transacoes.Where(t => t.Tipo == TiposTransacao.Receita).Sum(t => t.Valor),
                    TotalDespesas = p.Transacoes.Where(t => t.Tipo == TiposTransacao.Despesa).Sum(t => t.Valor),
                    Saldo = p.Transacoes.Where(t => t.Tipo == TiposTransacao.Receita).Sum(t => t.Valor) -
                            p.Transacoes.Where(t => t.Tipo == TiposTransacao.Despesa).Sum(t => t.Valor)
                }).ToListAsync();

            var totalReceitasGeral = pessoas.Sum(p => p.TotalReceitas);
            var totalDespesasGeral = pessoas.Sum(p => p.TotalDespesas);

            return Ok(new {
                Pessoas = pessoas,
                TotalGeralReceitas = totalReceitasGeral,
                TotalGeralDespesas = totalDespesasGeral,
                SaldoLiquidoGeral = totalReceitasGeral - totalDespesasGeral
            });
        }

        // Função responsável por deletar a pessoa.
        // A exclusão das transações associadas ocorre automaticamente em cascata via Entity Framework.
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletarPessoa(int id)
        {
            var pessoa = await _context.Pessoas.FindAsync(id);

            if (pessoa == null)
            {
                return Problem(statusCode: StatusCodes.Status404NotFound, title: "Pessoa não encontrada.");
            }

            _context.Pessoas.Remove(pessoa);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Pessoa e suas transações foram excluídas com sucesso." });
        }
    }
}
