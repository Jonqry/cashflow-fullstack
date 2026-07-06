using CashFlow.Api.Entities;
using CashFlow.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CashFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransacoesController : ControllerBase
    {
        private readonly TransacaoService _transacaoService;

        // O Controller recebe o serviço pronto para uso
        public TransacoesController(TransacaoService transacaoService)
        {
            _transacaoService = transacaoService;
        }

        [HttpPost]
        public async Task<IActionResult> CriarTransacao(Transacao transacao)
        {
            try
            {
                // Tenta salvar usando as regras
                var novaTransacao = await _transacaoService.SalvarTransacaoAsync(transacao);
                return Ok(novaTransacao);
            }
            catch (Exception ex)
            {
                // Se a regra da idade falhar, captura o erro e devolve HTTP 400 (Bad Request)
                return BadRequest(ex.Message);
            }
        }
    }
}