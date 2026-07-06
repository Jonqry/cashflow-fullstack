using CashFlow.Api.Entities;
using CashFlow.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PessoasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PessoasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CriarPessoa(Pessoa pessoa)
        {
            _context.Pessoas.Add(pessoa);
            await _context.SaveChangesAsync();
            return Ok(pessoa); // Retorna HTTP 200 com os dados salvos
        }

        [HttpGet("totais")]
        public async Task<IActionResult> ListarTotais()
        {
            // Busca todas as pessoas, incluindo suas transações
            var pessoas = await _context.Pessoas
                .Include(p => p.Transacoes)
                .Select(p => new {
                    p.Id,
                    p.Nome,
                    TotalReceitas = p.Transacoes.Where(t => t.Tipo == "Receita").Sum(t => t.Valor),
                    TotalDespesas = p.Transacoes.Where(t => t.Tipo == "Despesa").Sum(t => t.Valor),
                    Saldo = p.Transacoes.Where(t => t.Tipo == "Receita").Sum(t => t.Valor) - 
                            p.Transacoes.Where(t => t.Tipo == "Despesa").Sum(t => t.Valor)
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
    }
}