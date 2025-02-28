using FluentValidation;
using Supplier.Transactions.Dto.Requests;

namespace Supplier.Transactions.Validators
{
    public class TransactionRequestDtoValidator : AbstractValidator<TransactionRequestDto>
    {
        public TransactionRequestDtoValidator()
        {
            RuleFor(x => x.CustomerId)
                .NotEmpty()
                .WithMessage("CustomerId is required.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero.");
        }
    }
}
