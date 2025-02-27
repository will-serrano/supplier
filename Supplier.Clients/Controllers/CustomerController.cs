using Microsoft.AspNetCore.Mvc;
using Supplier.Customers.Dto.Requests;
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
        public async Task<IActionResult> Register([FromBody] CustomerRequestDto request)
        {
            try
            {
                var response = await _service.RegisterCustomerAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = "ERRO", detalheErro = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomers()
        {
            var customers = await _service.GetCustomersAsync();
            return Ok(customers);
        }
    }

}
