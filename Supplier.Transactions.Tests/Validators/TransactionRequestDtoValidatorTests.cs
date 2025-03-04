using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using Supplier.Transactions.Dto.Requests;
using Supplier.Transactions.Validators;

namespace Supplier.Transactions.Tests.Validators
{
    public class TransactionRequestDtoValidatorTests
    {
        private readonly TransactionRequestDtoValidator _validator;
        private readonly Mock<ILogger<TransactionRequestDtoValidator>> _loggerMock;

        public TransactionRequestDtoValidatorTests()
        {
            _loggerMock = new Mock<ILogger<TransactionRequestDtoValidator>>();
            _validator = new TransactionRequestDtoValidator(_loggerMock.Object);
        }

        [Fact]
        public void Should_Have_Error_When_Amount_Is_Null()
        {
            // Arrange
            var dto = new TransactionRequestDto { Amount = null };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Amount)
                .WithErrorMessage("The transaction amount is required.");
        }

        [Fact]
        public void Should_Have_Error_When_Amount_Is_Less_Than_Or_Equal_To_Zero()
        {
            // Arrange
            var dto = new TransactionRequestDto { Amount = 0 };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Amount)
                .WithErrorMessage("The transaction amount must be greater than zero.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_Amount_Is_Greater_Than_Zero()
        {
            // Arrange
            var dto = new TransactionRequestDto { Amount = 100 };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Amount);
        }

        [Fact]
        public void Should_Have_Error_When_CustomerId_Is_Empty()
        {
            // Arrange
            var dto = new TransactionRequestDto { CustomerId = string.Empty };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CustomerId)
                .WithErrorMessage("The customer ID is required.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_CustomerId_Is_Not_Empty()
        {
            // Arrange
            var dto = new TransactionRequestDto { CustomerId = "123" };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.CustomerId);
        }
    }
}
