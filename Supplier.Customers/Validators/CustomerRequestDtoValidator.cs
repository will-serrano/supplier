using FluentValidation;
using Supplier.Customers.Dto.Requests;
using Supplier.Customers.Repositories.Interfaces;

namespace Supplier.Customers.Validators
{
    /// <summary>
    /// Validator for CustomerRequestDto.
    /// </summary>
    public class CustomerRequestDtoValidator : AbstractValidator<CustomerRequestDto>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<CustomerRequestDtoValidator> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerRequestDtoValidator"/> class.
        /// </summary>
        /// <param name="customerRepository">The customer repository.</param>
        /// <param name="logger">The logger instance.</param>
        public CustomerRequestDtoValidator(ICustomerRepository customerRepository, ILogger<CustomerRequestDtoValidator> logger)
        {
            _customerRepository = customerRepository;
            _logger = logger;

            _logger.LogInformation("Initializing CustomerRequestDtoValidator");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name cannot be empty.")
                .MaximumLength(100).WithMessage("Name must be at most 100 characters long.");

            RuleFor(x => x.Cpf)
                .NotEmpty().WithMessage("CPF is required.")
                .Matches(@"^\d{11}$").WithMessage("CPF must contain 11 numeric digits.")
                .MustAsync(async (cpf, cancellation) =>
                {
                    var exists = await _customerRepository.ExistsAsync(cpf);
                    if (exists)
                    {
                        _logger.LogWarning("CPF {Cpf} already exists.", cpf);
                    }
                    return !exists;
                })
                .WithMessage("CPF is already registered.");

            RuleFor(x => x.CreditLimit)
                .GreaterThanOrEqualTo(0).WithMessage("Credit limit cannot be negative.");
        }
    }
}
