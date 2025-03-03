using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Rebus.Bus;
using Supplier.Contracts.Transactions;
using Supplier.Contracts.Transactions.Requests;
using Supplier.Transactions.Dto.Requests;
using Supplier.Transactions.HttpClients.Dto;
using Supplier.Transactions.HttpClients.Interfaces;
using Supplier.Transactions.Mappers.Interfaces;
using Supplier.Transactions.Messaging;
using Supplier.Transactions.Messaging.Interfaces;
using Supplier.Transactions.Models;
using Supplier.Transactions.Repositories.Interfaces;
using Supplier.Transactions.Services;

namespace Supplier.Transactions.Tests.Services
{
    public class TransactionRequestServiceTests
    {
        private readonly Mock<IValidator<TransactionRequestDto>> _validatorMock;
        private readonly Mock<ITransactionRequestRepository> _repositoryMock;
        private readonly Mock<ITransactionRequestMapper> _mapperMock;
        private readonly Mock<ICustomerMessagePublisher> _messagePublisherMock;
        private readonly Mock<ICustomerValidationClient> _customerValidationClientMock;
        private readonly Mock<ILogger<TransactionRequestService>> _loggerMock;
        private readonly TransactionRequestService _service;
        private readonly string _token = "test-token";

        public TransactionRequestServiceTests()
        {
            _validatorMock = new Mock<IValidator<TransactionRequestDto>>();
            _repositoryMock = new Mock<ITransactionRequestRepository>();
            _mapperMock = new Mock<ITransactionRequestMapper>();
            _messagePublisherMock = new Mock<ICustomerMessagePublisher>();
            _customerValidationClientMock = new Mock<ICustomerValidationClient>();
            _loggerMock = new Mock<ILogger<TransactionRequestService>>();

            _service = new TransactionRequestService(
                _validatorMock.Object,
                _repositoryMock.Object,
                _mapperMock.Object,
                _messagePublisherMock.Object,
                _customerValidationClientMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task RequestTransactionAsync_ValidRequest_ReturnsApprovedResponse()
        {
            // Arrange
            var dto = new TransactionRequestDto { CustomerId = "123", Amount = 100, UserId = Guid.NewGuid() };
            var validationResult = new ValidationResult();
            var transactionRequest = new TransactionRequest { CustomerId = Guid.NewGuid(), Amount = 100 };
            var clientValidationResult = new CustomerValidationResultDto { IsValid = true };
            var transactionMessageData = new TransactionRequestMessageData { Amount = 100, CustomerId = Guid.NewGuid(), TransactionId = Guid.NewGuid() };

            _validatorMock.Setup(v => v.ValidateAsync(dto, default)).ReturnsAsync(validationResult);
            _mapperMock.Setup(m => m.MapToTransactionRequest(dto)).Returns(transactionRequest);
            _repositoryMock.Setup(r => r.RegisterTransactionRequestAsync(transactionRequest)).ReturnsAsync(transactionRequest);
            _customerValidationClientMock.Setup(c => c.ValidateCustomerAsync(transactionRequest, _token)).ReturnsAsync(clientValidationResult);
            _mapperMock.Setup(m => m.MapToTransactionMessageData(transactionRequest)).Returns(transactionMessageData);
            _messagePublisherMock.Setup(m => m.Send(It.IsAny<MessageWrapper>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.RequestTransactionAsync(dto, _token);

            // Assert
            Assert.Equal("APROVADO", result.Status);
            Assert.NotEqual(Guid.Empty, result.TransactionId);
            _repositoryMock.Verify(r => r.UpdateTransactionRequestAsync(transactionRequest), Times.Exactly(2));
            _messagePublisherMock.Verify(m => m.Send(It.IsAny<MessageWrapper>()), Times.Once);
        }

        [Fact]
        public async Task RequestTransactionAsync_InvalidRequest_ThrowsValidationException()
        {
            // Arrange
            var dto = new TransactionRequestDto { CustomerId = "123", Amount = 100, UserId = Guid.NewGuid() };
            var validationResult = new ValidationResult(new[] { new ValidationFailure("CustomerId", "Invalid CustomerId") });

            _validatorMock.Setup(v => v.ValidateAsync(dto, default)).ReturnsAsync(validationResult);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.RequestTransactionAsync(dto, _token));
        }

        [Fact]
        public async Task RequestTransactionAsync_CustomerValidationFails_ReturnsDeniedResponse()
        {
            // Arrange
            var dto = new TransactionRequestDto { CustomerId = "123", Amount = 100, UserId = Guid.NewGuid() };
            var validationResult = new ValidationResult();
            var transactionRequest = new TransactionRequest { CustomerId = Guid.NewGuid(), Amount = 100 };
            var clientValidationResult = new CustomerValidationResultDto { IsValid = false, Message = "Customer validation failed" };

            _validatorMock.Setup(v => v.ValidateAsync(dto, default)).ReturnsAsync(validationResult);
            _mapperMock.Setup(m => m.MapToTransactionRequest(dto)).Returns(transactionRequest);
            _repositoryMock.Setup(r => r.RegisterTransactionRequestAsync(transactionRequest)).ReturnsAsync(transactionRequest);
            _customerValidationClientMock.Setup(c => c.ValidateCustomerAsync(transactionRequest, _token)).ReturnsAsync(clientValidationResult);

            // Act
            var result = await _service.RequestTransactionAsync(dto, _token);

            // Assert
            Assert.Equal("NEGADO", result.Status);
            _repositoryMock.Verify(r => r.UpdateTransactionRequestAsync(transactionRequest), Times.Once);
        }
    }
}
