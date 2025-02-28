using Microsoft.AspNetCore.Mvc;
using Supplier.Customers.Dto.Responses;
using Supplier.Customers.Repositories.Interfaces;
using System.Linq;

namespace Supplier.Customers.Controllers
{
    [ApiController]
    [Route("api/customers")]
    public class CustomerValidationController : ControllerBase
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerValidationController(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        [HttpGet("{customerId}/validate/{amount}")]
        public async Task<IActionResult> ValidateCustomer(Guid customerId, decimal amount)
        {
            var customers = await _customerRepository.GetAllAsync();
            var customer = customers.FirstOrDefault(c => c.Id == customerId);
            if (customer == null)
            {
                return NotFound(new CustomerValidationResponseDto
                {
                    IsValid = false,
                    Message = "Cliente não encontrado."
                });
            }

            if (customer.CreditLimit < amount)
            {
                return Ok(new CustomerValidationResponseDto
                {
                    IsValid = false,
                    Message = "Limite insuficiente."
                });
            }

            return Ok(new CustomerValidationResponseDto
            {
                IsValid = true,
                Message = "Cliente validado com sucesso."
            });
        }
    }
}
