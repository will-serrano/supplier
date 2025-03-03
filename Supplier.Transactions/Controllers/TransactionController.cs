using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Supplier.Transactions.Dto.Requests;
using Supplier.Transactions.Services.Interfaces;

namespace Supplier.Transactions.Controllers
{
    /// <summary>
    /// Controller for handling transaction-related requests.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/transacoes")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionRequestService _transactionService;
        private readonly ILogger<TransactionController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionController"/> class.
        /// </summary>
        /// <param name="transactionService">The transaction service.</param>
        /// <param name="logger">The logger instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when transactionService is null.</exception>
        public TransactionController(ITransactionRequestService transactionService, ILogger<TransactionController> logger)
        {
            _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
            _logger = logger;
        }

        /// <summary>
        /// Simulates a transaction request.
        /// </summary>
        /// <param name="request">The transaction request data transfer object.</param>
        /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
        [HttpPost("requesttransaction")]
        public async Task<IActionResult> RequestTransaction([FromBody] TransactionRequestDto request)
        {
            if (request == null)
            {
                return BadRequest("Request cannot be null");
            }

            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var (isValid, errorMessage, userGuid) = await _transactionService.ValidateRequest(request, token);

            if (!isValid)
            {
                _logger.LogWarning(errorMessage);
                return BadRequest(errorMessage);
            }

            if (userGuid == Guid.Empty)
            {
                return Unauthorized("User ID not found");
            }

            try
            {
                _logger.LogInformation("Simulating transaction for User ID: {UserId}", userGuid);
                var resultado = await _transactionService.RequestTransactionAsync(request, userGuid, token);
                if (resultado == null)
                {
                    _logger.LogError("An error occurred while processing the transaction for User ID: {UserId}", userGuid);
                    return StatusCode(500, "An error occurred while processing the transaction");
                }

                _logger.LogInformation("Transaction simulation successful for User ID: {UserId}", userGuid);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An internal server error occurred while processing the transaction for User ID: {UserId}", userGuid);
                return StatusCode(500, "An internal server error occurred");
            }
        }
    }
}
