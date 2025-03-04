using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Supplier.Customers.Services.Interfaces;

namespace Supplier.Customers.Controllers
{
    /// <summary>
    /// Controller for customer validation operations.
    /// </summary>
    [ApiController]
    [Route("api/customers")]
    [Authorize]
    public class CustomerValidationController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ILogger<CustomerValidationController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerValidationController"/> class.
        /// </summary>
        /// <param name="customerService">The customer service.</param>
        /// <param name="logger">The logger instance.</param>
        public CustomerValidationController(ICustomerService customerService, ILogger<CustomerValidationController> logger)
        {
            _customerService = customerService;
            _logger = logger;
        }

        /// <summary>
        /// Validates a customer based on the provided customer ID and amount.
        /// </summary>
        /// <param name="customerId">The customer ID.</param>
        /// <param name="amount">The amount to validate.</param>
        /// <returns>An <see cref="IActionResult"/> representing the result of the validation.</returns>
        [HttpGet("{customerId}/validate/{amount}")]
        public async Task<IActionResult> ValidateCustomer(Guid customerId, decimal amount)
        {
            _logger.LogInformation("Starting validation for customer {CustomerId} with amount {Amount}", customerId, amount);

            var response = await _customerService.ValidateCustomerAsync(customerId, amount);
            if (!response.IsValid)
            {
                _logger.LogWarning("Validation failed for customer {CustomerId} with amount {Amount}: {Message}", customerId, amount, response.Message);
                return NotFound(response);
            }

            _logger.LogInformation("Validation succeeded for customer {CustomerId} with amount {Amount}", customerId, amount);
            return Ok(response);
        }
    }
}
