using CashFlow.Api.Dtos;
using CashFlow.Api.Entities;
using CashFlow.Api.Infrastructure;

namespace CashFlow.Api.Services
{
    // Exceção específica para regra de negócio: a mensagem é segura para mostrar ao usuário
    public class RegraNegocioException : Exception
    {
        public RegraNegocioException(string mensagem) : base(mensagem) { }
    }

    public class TransacaoService
    {
        private readonly AppDbContext _context;

        // Injeta o contexto do banco de dados aqui
        public TransacaoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Transacao> SalvarTransacaoAsync(TransacaoRequest request)
        {
            // Busca a pessoa no banco de dados para checar a idade
            var pessoa = await _context.Pessoas.FindAsync(request.PessoaId);

            if (pessoa == null)
            {
                // Lança um erro que será capturado pelo Controller
                throw new RegraNegocioException("A pessoa informada não existe.");
            }

            // Validação da idade
            if (pessoa.Idade < 18 && request.Tipo == TiposTransacao.Receita)
            {
                throw new RegraNegocioException("Menores de 18 anos apenas podem registrar despesas.");
            }

            // Se passar por todas as regras, salva no banco
            var transacao = new Transacao
            {
                Descricao = request.Descricao.Trim(),
                Valor = request.Valor,
                Tipo = request.Tipo,
                PessoaId = pessoa.Id
            };

            _context.Transacoes.Add(transacao);
            await _context.SaveChangesAsync();

            return transacao;
        }
    }
}
