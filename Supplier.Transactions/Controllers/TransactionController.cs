using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Supplier.Transactions.Configuration.Interfaces;
using Supplier.Transactions.Dto.Requests;
using Supplier.Transactions.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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
        private readonly IJwtSecurityTokenHandlerWrapper _tokenHandlerWrapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionController"/> class.
        /// </summary>
        /// <param name="transactionService">The transaction service.</param>
        /// <param name="logger">The logger instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when transactionService is null.</exception>
        public TransactionController(ITransactionRequestService transactionService, ILogger<TransactionController> logger, IJwtSecurityTokenHandlerWrapper tokenHandlerWrapper)
        {
            _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
            _logger = logger;
            _tokenHandlerWrapper = tokenHandlerWrapper;
        }

        /// <summary>
        /// Simulates a transaction request.
        /// </summary>
        /// <param name="request">The transaction request data transfer object.</param>
        /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
        [HttpPost("simular")]
        public async Task<IActionResult> RequestTransaction([FromBody] TransactionRequestDto request)
        {
            if (request == null)
            {
                _logger.LogWarning("Request cannot be null");
                return BadRequest("Request cannot be null");
            }

            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Authorization token is missing or empty");
                return Unauthorized("Authorization token is missing or empty");
            }

            var jwtToken = _tokenHandlerWrapper.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub");
            var userId = string.Empty;
            if (userIdClaim != null)
            {
                userId = userIdClaim.Value;
            }

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found");
                return Unauthorized("User ID not found");
            }

            if (!Guid.TryParse(userId, out var userGuid))
            {
                _logger.LogWarning("Invalid User ID format: {UserId}", userId);
                return BadRequest("Invalid User ID format");
            }

            request.UserId = userGuid;

            try
            {
                _logger.LogInformation("Simulating transaction for User ID: {UserId}", userGuid);
                var resultado = await _transactionService.RequestTransactionAsync(request, token);
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
