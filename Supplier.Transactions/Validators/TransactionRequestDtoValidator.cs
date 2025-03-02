using FluentValidation;
using Supplier.Transactions.Dto.Requests;

namespace Supplier.Transactions.Validators
{
    public class TransactionRequestDtoValidator : AbstractValidator<TransactionRequestDto>
    {
        public TransactionRequestDtoValidator()
        {
            // Exemplo de regra para validar o valor da transação (Amount)
            RuleFor(x => x.Amount)
                .NotNull().WithMessage("O valor da transação é obrigatório.")
                .GreaterThan(0).WithMessage("O valor da transação deve ser maior que zero.");

            // Exemplo de regra para validar o identificador do cliente
            RuleFor(x => x.CustomerId)
                .NotEmpty().WithMessage("O ID do cliente é obrigatório.");

        }
    }
}
