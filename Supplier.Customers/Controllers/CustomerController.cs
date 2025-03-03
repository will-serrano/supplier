using Microsoft.AspNetCore.Mvc;
using Supplier.Customers.Dto.Requests;
using Supplier.Customers.Dto.Responses;
using Supplier.Customers.Services.Interfaces;

namespace Supplier.Customers.Controllers
{
    /// <summary>
    /// Controller for managing customer-related operations.
    /// </summary>
    [ApiController]
    [Route("api/customers")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _service;
        private readonly ILogger<CustomerController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerController"/> class.
        /// </summary>
        /// <param name="service">The customer service.</param>
        /// <exception cref="ArgumentNullException">Thrown when the service is null.</exception>
        public CustomerController(ICustomerService service, ILogger<CustomerController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger;
        }

        /// <summary>
        /// Creates a new customer.
        /// </summary>
        /// <param name="request">The customer request data.</param>
        /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CustomerRequestDto request)
        {
            if (request == null)
            {
                _logger.LogWarning("Create request failed: request data is null.");
                return BadRequest(new ErrorResponseDto("Invalid request data."));
            }

            try
            {
                _logger.LogInformation("Creating a new customer with name: {Name}", request.Name);
                var response = await _service.CreateCustomerAsync(request);
                _logger.LogInformation("Customer created successfully with ID: {Id}", response.IdCliente);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a new customer.");
                return BadRequest(new ErrorResponseDto(ex.Message));
            }
        }

        /// <summary>
        /// Retrieves customers based on the specified criteria.
        /// </summary>
        /// <param name="name">The name of the customer.</param>
        /// <param name="cpf">The CPF of the customer.</param>
        /// <param name="creditLimit">The credit limit of the customer.</param>
        /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? name, [FromQuery] string? cpf, [FromQuery] decimal? creditLimit)
        {
            _logger.LogInformation("Retrieving customers with criteria - Name: {Name}, CPF: {Cpf}, CreditLimit: {CreditLimit}", name, cpf, creditLimit);
            var customers = await _service.GetCustomersAsync(name, cpf, creditLimit);
            _logger.LogInformation("Retrieved {Count} customers", customers.Customers.Count());
            return Ok(customers);
        }
    }
}
