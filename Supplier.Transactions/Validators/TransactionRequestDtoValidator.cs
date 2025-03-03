using FluentValidation;
using Supplier.Transactions.Dto.Requests;

namespace Supplier.Transactions.Validators
{
    /// <summary>
    /// Validator for TransactionRequestDto.
    /// </summary>
    public class TransactionRequestDtoValidator : AbstractValidator<TransactionRequestDto>
    {
        private readonly ILogger<TransactionRequestDtoValidator> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionRequestDtoValidator"/> class.
        /// </summary>
        /// <param name="logger">The logger instance to use for logging.</param>
        public TransactionRequestDtoValidator(ILogger<TransactionRequestDtoValidator> logger)
        {
            _logger = logger;

            // Log the start of validation
            _logger.LogInformation("Initializing TransactionRequestDtoValidator");

            // Example rule to validate the transaction amount (Amount)
            RuleFor(x => x.Amount)
                .NotNull().WithMessage("The transaction amount is required.")
                .GreaterThan(0).WithMessage("The transaction amount must be greater than zero.");

            // Example rule to validate the customer identifier
            RuleFor(x => x.CustomerId)
                .NotEmpty().WithMessage("The customer ID is required.");
        }
    }
}
