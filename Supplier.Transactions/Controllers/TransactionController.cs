using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Supplier.Transactions.Dto.Requests;
using Supplier.Transactions.Services.Interfaces;

namespace Supplier.Transactions.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/transacoes")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionRequestService _transactionService;

        public TransactionController(ITransactionRequestService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpPost("simular")]
        public async Task<IActionResult> SimularTransacao([FromBody] TransactionRequestDto request)
        {
            var userId = User.FindFirst("sub")?.Value; // ou use ClaimTypes.NameIdentifier se apropriado
            request.UserId = userId;

            var resultado = await _transactionService.SimularTransacaoAsync(request);
            return Ok(resultado);
        }
    }
}
