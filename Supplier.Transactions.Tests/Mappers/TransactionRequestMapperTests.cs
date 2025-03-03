using Supplier.Transactions.Dto.Requests;
using Supplier.Transactions.Mappers;
using Supplier.Transactions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supplier.Transactions.Tests.Mappers
{
    public class TransactionRequestMapperTests
    {
        private readonly TransactionRequestMapper _mapper;

        public TransactionRequestMapperTests()
        {
            _mapper = new TransactionRequestMapper();
        }

        [Fact]
        public void MapToTransactionMessageData_ShouldMapCorrectly()
        {
            // Arrange
            var transactionRequest = new TransactionRequest
            {
                Amount = 100.50m,
                CustomerId = Guid.NewGuid(),
                TransactionId = Guid.NewGuid()
            };

            // Act
            var result = _mapper.MapToTransactionMessageData(transactionRequest);

            // Assert
            Assert.Equal(transactionRequest.Amount, result.Amount);
            Assert.Equal(transactionRequest.CustomerId, result.CustomerId);
            Assert.Equal(transactionRequest.TransactionId, result.TransactionId);
        }

        [Fact]
        public void MapToTransactionRequest_ShouldMapCorrectly()
        {
            // Arrange
            var requestDto = new TransactionRequestDto
            {
                CustomerId = Guid.NewGuid().ToString(),
                Amount = 200.75m,
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _mapper.MapToTransactionRequest(requestDto);

            // Assert
            Assert.Equal(Guid.Parse(requestDto.CustomerId), result.CustomerId);
            Assert.Equal(requestDto.Amount.GetValueOrDefault(), result.Amount);
            Assert.Equal(requestDto.UserId.ToString(), result.RequestedBy);
        }

        [Fact]
        public void MapToTransactionRequest_ShouldHandleNullCustomerId()
        {
            // Arrange
            var requestDto = new TransactionRequestDto
            {
                CustomerId = null,
                Amount = 200.75m,
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _mapper.MapToTransactionRequest(requestDto);

            // Assert
            Assert.Equal(Guid.Empty, result.CustomerId);
            Assert.Equal(requestDto.Amount.GetValueOrDefault(), result.Amount);
            Assert.Equal(requestDto.UserId.ToString(), result.RequestedBy);
        }

        [Fact]
        public void MapToTransactionRequest_ShouldHandleNullAmount()
        {
            // Arrange
            var requestDto = new TransactionRequestDto
            {
                CustomerId = Guid.NewGuid().ToString(),
                Amount = null,
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _mapper.MapToTransactionRequest(requestDto);

            // Assert
            Assert.Equal(Guid.Parse(requestDto.CustomerId), result.CustomerId);
            Assert.Equal(0, result.Amount);
            Assert.Equal(requestDto.UserId.ToString(), result.RequestedBy);
        }

        [Fact]
        public void MapToTransactionRequest_ShouldHandleNullUserId()
        {
            // Arrange
            var requestDto = new TransactionRequestDto
            {
                CustomerId = Guid.NewGuid().ToString(),
                Amount = 200.75m,
                UserId = null
            };

            // Act
            var result = _mapper.MapToTransactionRequest(requestDto);

            // Assert
            Assert.Equal(Guid.Parse(requestDto.CustomerId), result.CustomerId);
            Assert.Equal(requestDto.Amount.GetValueOrDefault(), result.Amount);
            Assert.Equal(string.Empty, result.RequestedBy);
        }
    }
}
