using CashFlow.Api.Dtos;
using CashFlow.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CashFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // exige token JWT válido
    public class TransacoesController : ControllerBase
    {
        private readonly TransacaoService _transacaoService;

        // O Controller recebe o serviço pronto para uso
        public TransacoesController(TransacaoService transacaoService)
        {
            _transacaoService = transacaoService;
        }

        [HttpPost]
        public async Task<IActionResult> CriarTransacao(TransacaoRequest request)
        {
            try
            {
                // Tenta salvar usando as regras
                var nova = await _transacaoService.SalvarTransacaoAsync(request);
                return Ok(new TransacaoResponse(nova.Id, nova.Descricao, nova.Valor, nova.Tipo, nova.PessoaId));
            }
            catch (RegraNegocioException ex)
            {
                // Só erros de regra de negócio viram 400 com mensagem.
                // Qualquer outra exceção cai no tratador global e devolve um 500 genérico, sem detalhes internos.
                return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Regra de negócio violada.", detail: ex.Message);
            }
        }
    }
}
