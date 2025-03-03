﻿using FluentValidation;
using Supplier.Contracts.Transactions;
using Supplier.Transactions.Dto.Requests;
using Supplier.Transactions.Dto.Responses;
using Supplier.Transactions.HttpClients.Interfaces;
using Supplier.Transactions.Mappers.Interfaces;
using Supplier.Transactions.Messaging;
using Supplier.Transactions.Repositories.Interfaces;
using Supplier.Transactions.Services.Interfaces;

namespace Supplier.Transactions.Services
{
    /// <summary>
    /// Service for handling transaction requests.
    /// </summary>
    public class TransactionRequestService : ITransactionRequestService
    {
        private readonly IValidator<TransactionRequestDto> _transactionRequestDtoValidator;
        private readonly ITransactionRequestRepository _transactionRequestRepository;
        private readonly ITransactionRequestMapper _transactionRequestMapper;
        private readonly CustomerMessagePublisher _messagePublisher;
        private readonly ICustomerValidationClient _customerValidationClient;
        private readonly ILogger<TransactionRequestService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionRequestService"/> class.
        /// </summary>
        /// <param name="validator">The validator for transaction request DTOs.</param>
        /// <param name="repository">The repository for transaction requests.</param>
        /// <param name="mapper">The mapper for transaction requests.</param>
        /// <param name="messagePublisher">The publisher for customer messages.</param>
        /// <param name="customerValidationClient">The client for customer validation.</param>
        /// <param name="logger">The logger for logging information.</param>
        public TransactionRequestService(
            IValidator<TransactionRequestDto> validator,
            ITransactionRequestRepository repository,
            ITransactionRequestMapper mapper,
            CustomerMessagePublisher messagePublisher,
            ICustomerValidationClient customerValidationClient,
            ILogger<TransactionRequestService> logger)
        {
            _transactionRequestDtoValidator = validator;
            _transactionRequestRepository = repository;
            _transactionRequestMapper = mapper;
            _messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher));
            _customerValidationClient = customerValidationClient ?? throw new ArgumentNullException(nameof(customerValidationClient));
            _logger = logger;
        }

        /// <summary>
        /// Simulates a transaction asynchronously.
        /// </summary>
        /// <param name="dto">The transaction request DTO.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the transaction response DTO.</returns>
        public async Task<TransactionResponseDto> RequestTransactionAsync(TransactionRequestDto dto)
        {
            _logger.LogInformation("Starting transaction simulation for CustomerId: {CustomerId}", dto.CustomerId);

            var validationResult = await _transactionRequestDtoValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for CustomerId: {CustomerId}. Errors: {Errors}", dto.CustomerId, validationResult.Errors);
                throw new ValidationException(validationResult.Errors);
            }

            var transactionRequest = _transactionRequestMapper.MapToTransactionRequest(dto)
                ?? throw new ArgumentNullException(nameof(dto));

            transactionRequest = await _transactionRequestRepository.RegisterTransactionRequestAsync(transactionRequest)
                ?? throw new ArgumentNullException(nameof(dto));

            _logger.LogInformation("Transaction request registered for CustomerId: {CustomerId}", dto.CustomerId);

            var clientValidationResult = await _customerValidationClient.ValidateCustomerAsync(transactionRequest);

            if (!clientValidationResult.IsValid)
            {
                transactionRequest.Detail = clientValidationResult.Message ?? string.Empty;
                await _transactionRequestRepository.UpdateTransactionRequestAsync(transactionRequest);
                _logger.LogWarning("Customer validation failed for CustomerId: {CustomerId}. Message: {Message}", dto.CustomerId, clientValidationResult.Message);
                return new TransactionResponseDto { Status = "NEGADO" };
            }

            transactionRequest.TransactionId = Guid.NewGuid();
            await _transactionRequestRepository.UpdateTransactionRequestAsync(transactionRequest);

            _logger.LogInformation("Transaction authorized for CustomerId: {CustomerId}, TransactionId: {TransactionId}", dto.CustomerId, transactionRequest.TransactionId);

            var transactionMessageDataToSend = _transactionRequestMapper.MapToTransactionMessageData(transactionRequest);

            var mensagem = new MessageWrapper
            {
                Version = "V1",
                Data = transactionMessageDataToSend
            };

            await _messagePublisher.Send(mensagem);

            _logger.LogInformation("Transaction message sent for TransactionId: {TransactionId}", transactionRequest.TransactionId);

            await _transactionRequestRepository.UpdateTransactionRequestAsync(transactionRequest);

            _logger.LogInformation("Transaction processing for TransactionId: {TransactionId}", transactionRequest.TransactionId);

            return new TransactionResponseDto { Status = "APROVADO", TransactionId = transactionRequest.TransactionId };
        }
    }
}
