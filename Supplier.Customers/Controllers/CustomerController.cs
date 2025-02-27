using Microsoft.AspNetCore.Mvc;
using Supplier.Customers.Dto.Requests;
using Supplier.Customers.Dto.Responses;
using Supplier.Customers.Services;

namespace Supplier.Customers.Controllers
{
    [ApiController]
    [Route("api/customers")]
    public class CustomerController : ControllerBase
    {
        private readonly CustomerService _service;

        public CustomerController(CustomerService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CustomerRequestDto request)
        {
            try
            {
                var response = await _service.CreateCustomerAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponseDto(ex.Message));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? name, [FromQuery] string? cpf, [FromQuery] decimal? creditLimit)
        {
            var customers = await _service.GetCustomersAsync(name, cpf, creditLimit);
            return Ok(customers);
        }
    }
}
