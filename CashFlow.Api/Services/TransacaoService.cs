using CashFlow.Api.Entities;
using CashFlow.Api.Infrastructure;

namespace CashFlow.Api.Services
{
    public class TransacaoService
    {
        private readonly AppDbContext _context;

        // Injeta o contexto do banco de dados aqui
        public TransacaoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Transacao> SalvarTransacaoAsync(Transacao transacao)
        {
            // Busca a pessoa no banco de dados para checar a idade
            var pessoa = await _context.Pessoas.FindAsync(transacao.PessoaId);
            
            if (pessoa == null)
            {
                // Lança um erro que será capturado pelo Controller
                throw new Exception("A pessoa informada não existe.");
            }

            // Validação da idade
            if (pessoa.Idade < 18 && transacao.Tipo == "Receita")
            {
                throw new Exception("Regra violada: Menores de 18 anos apenas podem registrar despesas.");
            }

            // Se passar por todas as regras, salva no banco
            _context.Transacoes.Add(transacao);
            await _context.SaveChangesAsync();

            return transacao;
        }
    }
}