using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using Supplier.Customers.Dto.Requests;
using Supplier.Customers.Repositories.Interfaces;
using Supplier.Customers.Validators;

namespace Supplier.Customers.Tests.Validators
{
    public class CustomerRequestDtoValidatorTests
    {
        private readonly Mock<ICustomerRepository> _customerRepositoryMock;
        private readonly CustomerRequestDtoValidator _validator;
        private readonly ILogger<CustomerRequestDtoValidator> _logger;

        public CustomerRequestDtoValidatorTests()
        {
            _customerRepositoryMock = new Mock<ICustomerRepository>();
            _logger = new Mock<ILogger<CustomerRequestDtoValidator>>().Object;
            _validator = new CustomerRequestDtoValidator(_customerRepositoryMock.Object, _logger);
        }

        [Fact]
        public async Task Should_Have_Error_When_Name_Is_Empty()
        {
            var model = new CustomerRequestDto { Name = string.Empty };
            var result = await _validator.TestValidateAsync(model);
            result.ShouldHaveValidationErrorFor(x => x.Name).WithErrorMessage("Name cannot be empty.");
        }

        [Fact]
        public async Task Should_Have_Error_When_Name_Exceeds_MaxLength()
        {
            var model = new CustomerRequestDto { Name = new string('a', 101) };
            var result = await _validator.TestValidateAsync(model);
            result.ShouldHaveValidationErrorFor(x => x.Name).WithErrorMessage("Name must be at most 100 characters long.");
        }

        [Fact]
        public async Task Should_Have_Error_When_Cpf_Is_Empty()
        {
            var model = new CustomerRequestDto { Cpf = string.Empty };
            var result = await _validator.TestValidateAsync(model);
            result.ShouldHaveValidationErrorFor(x => x.Cpf).WithErrorMessage("CPF is required.");
        }

        [Fact]
        public async Task Should_Have_Error_When_Cpf_Is_Invalid_Format()
        {
            var model = new CustomerRequestDto { Cpf = "1234567890" };
            var result = await _validator.TestValidateAsync(model);
            result.ShouldHaveValidationErrorFor(x => x.Cpf).WithErrorMessage("CPF must contain 11 numeric digits.");
        }

        [Fact]
        public async Task Should_Have_Error_When_Cpf_Already_Exists()
        {
            _customerRepositoryMock.Setup(repo => repo.ExistsAsync(It.IsAny<string>())).ReturnsAsync(true);
            var model = new CustomerRequestDto { Cpf = "12345678901" };
            var result = await _validator.TestValidateAsync(model);
            result.ShouldHaveValidationErrorFor(x => x.Cpf).WithErrorMessage("CPF is already registered.");
        }

        [Fact]
        public async Task Should_Have_Error_When_CreditLimit_Is_Negative()
        {
            var model = new CustomerRequestDto { CreditLimit = -1 };
            var result = await _validator.TestValidateAsync(model);
            result.ShouldHaveValidationErrorFor(x => x.CreditLimit).WithErrorMessage("Credit limit cannot be negative.");
        }

        [Fact]
        public async Task Should_Not_Have_Error_When_Model_Is_Valid()
        {
            var model = new CustomerRequestDto { Name = "Valid Name", Cpf = "12345678901", CreditLimit = 100 };
            var result = await _validator.TestValidateAsync(model);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
