using FluentValidation;
using Supplier.Customers.Dto.Requests;
using Supplier.Customers.Repositories.Interfaces;

namespace Supplier.Customers.Validators
{
    public class CustomerRequestDtoValidator : AbstractValidator<CustomerRequestDto>
    {
        private readonly ICustomerRepository _customerRepository;
        public CustomerRequestDtoValidator(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("O nome não pode estar vazio.")
                .MaximumLength(100).WithMessage("O nome deve ter no máximo 100 caracteres.");

            RuleFor(x => x.Cpf)
                .NotEmpty().WithMessage("O CPF é obrigatório.")
                .Matches(@"^\d{11}$").WithMessage("O CPF deve conter 11 dígitos numéricos.")
                .MustAsync(async (cpf, cancellation) => !(await _customerRepository.ExistsAsync(cpf)))
                .WithMessage("O CPF já está cadastrado.");

            RuleFor(x => x.CreditLimit)
                .GreaterThanOrEqualTo(0).WithMessage("O limite de crédito não pode ser negativo.");
        }
    }
}
