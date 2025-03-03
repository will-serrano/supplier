using FluentValidation;
using Supplier.Contracts.Transactions;
using Supplier.Transactions.Configuration.Interfaces;
using Supplier.Transactions.Dto.Requests;
using Supplier.Transactions.Dto.Responses;
using Supplier.Transactions.HttpClients.Interfaces;
using Supplier.Transactions.Mappers.Interfaces;
using Supplier.Transactions.Messaging.Interfaces;
using Supplier.Transactions.Repositories.Interfaces;
using Supplier.Transactions.Services.Interfaces;
using System.Security.Claims;

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
        private readonly ICustomerMessagePublisher _messagePublisher;
        private readonly ICustomerValidationClient _customerValidationClient;
        private readonly IJwtSecurityTokenHandlerWrapper _tokenHandlerWrapper;
        private readonly ILogger<TransactionRequestService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionRequestService"/> class.
        /// </summary>
        /// <param name="validator">The validator for transaction request DTOs.</param>
        /// <param name="repository">The repository for transaction requests.</param>
        /// <param name="mapper">The mapper for transaction requests.</param>
        /// <param name="messagePublisher">The publisher for customer messages.</param>
        /// <param name="customerValidationClient">The client for customer validation.</param>
        /// <param name="tokenHandlerWrapper">The token handler wrapper.</param>
        /// <param name="logger">The logger for logging information.</param>
        public TransactionRequestService(
            IValidator<TransactionRequestDto> validator,
            ITransactionRequestRepository repository,
            ITransactionRequestMapper mapper,
            ICustomerMessagePublisher messagePublisher,
            ICustomerValidationClient customerValidationClient,
            IJwtSecurityTokenHandlerWrapper tokenHandlerWrapper,
            ILogger<TransactionRequestService> logger)
        {
            _transactionRequestDtoValidator = validator;
            _transactionRequestRepository = repository;
            _transactionRequestMapper = mapper;
            _messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher));
            _customerValidationClient = customerValidationClient ?? throw new ArgumentNullException(nameof(customerValidationClient));
            _tokenHandlerWrapper = tokenHandlerWrapper ?? throw new ArgumentNullException(nameof(tokenHandlerWrapper));
            _logger = logger;
        }

        /// <summary>
        /// Simulates a transaction asynchronously.
        /// </summary>
        /// <param name="dto">The transaction request DTO.</param>
        /// <param name="userId">The user ID.</param>
        /// <param name="token">The authorization token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the transaction response DTO.</returns>
        public async Task<TransactionResponseDto> RequestTransactionAsync(TransactionRequestDto dto, Guid userId, string token)
        {
            _logger.LogInformation("Starting transaction simulation for CustomerId: {CustomerId}", dto.CustomerId);

            var validationResult = await _transactionRequestDtoValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for CustomerId: {CustomerId}. Errors: {Errors}", dto.CustomerId, validationResult.Errors);
                throw new ValidationException(validationResult.Errors);
            }

            if (string.IsNullOrEmpty(dto.CustomerId))
            {
                throw new ArgumentNullException(nameof(dto.CustomerId), "CustomerId cannot be null or empty");
            }

            var isCustomerBlocked = await _transactionRequestRepository.IsCustomerBlockedAsync(dto.CustomerId);
            if (isCustomerBlocked)
            {
                _logger.LogWarning("Customer {CustomerId} is blocked.", dto.CustomerId);
                return new TransactionResponseDto { Status = "NEGADO" };
            }

            var transactionRequest = _transactionRequestMapper.MapToTransactionRequest(dto)
                ?? throw new ArgumentNullException(nameof(dto));

            transactionRequest.RequestedBy = userId.ToString();
            transactionRequest.UpdatedBy = "Supplier.Transactions.API";

            _logger.LogInformation("Customer {CustomerId} blocked for analysis", dto.CustomerId);
            transactionRequest.CustomerBlocked = true;

            transactionRequest = await _transactionRequestRepository.RegisterTransactionRequestAsync(transactionRequest)
                ?? throw new ArgumentNullException(nameof(dto));

            _logger.LogInformation("Transaction request registered for CustomerId: {CustomerId}", dto.CustomerId);

            var clientValidationResult = await _customerValidationClient.ValidateCustomerAsync(transactionRequest, token);

            if (!clientValidationResult.IsValid)
            {
                transactionRequest.Detail = clientValidationResult.Message ?? string.Empty;
                transactionRequest.Status = Enums.TransactionStatus.Rejected;
                transactionRequest.CustomerBlocked = false;
                await _transactionRequestRepository.UpdateTransactionRequestAsync(transactionRequest);
                _logger.LogWarning("Customer validation failed for CustomerId: {CustomerId}. Message: {Message}", dto.CustomerId, clientValidationResult.Message);
                return new TransactionResponseDto { Status = "NEGADO" };
            }

            transactionRequest.TransactionId = Guid.NewGuid();
            transactionRequest.Status = Enums.TransactionStatus.Authorized;
            await _transactionRequestRepository.UpdateTransactionRequestAsync(transactionRequest);

            _logger.LogInformation("Transaction authorized for CustomerId: {CustomerId}, TransactionId: {TransactionId}", dto.CustomerId, transactionRequest.TransactionId);

            var transactionMessageDataToSend = _transactionRequestMapper.MapToTransactionMessageData(transactionRequest);

            var mensagem = new MessageWrapper
            {
                Version = "V1",
                Data = transactionMessageDataToSend,
                Type = Contracts.Transactions.Enums.MessageType.TransactionRequestMessageData
            };

            await _messagePublisher.Send(mensagem);

            _logger.LogInformation("Transaction message sent for TransactionId: {TransactionId}", transactionRequest.TransactionId);

            transactionRequest.Status = Enums.TransactionStatus.Processing;
            await _transactionRequestRepository.UpdateTransactionRequestAsync(transactionRequest);

            _logger.LogInformation("Transaction processing for TransactionId: {TransactionId}", transactionRequest.TransactionId);

            return new TransactionResponseDto { Status = "APROVADO", TransactionId = transactionRequest.TransactionId };
        }

        /// <summary>
        /// Validates the transaction request.
        /// </summary>
        /// <param name="request">The transaction request DTO.</param>
        /// <param name="token">The authorization token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a tuple with validation result, error message, and user ID.</returns>
        public async Task<(bool IsValid, string ErrorMessage, Guid UserId)> ValidateRequest(TransactionRequestDto request, string token)
        {
            return await Task.Run(() =>
            {
                if (request == null)
                {
                    return (false, "Request cannot be null", Guid.Empty);
                }

                if (string.IsNullOrEmpty(token))
                {
                    return (false, "Authorization token is missing or empty", Guid.Empty);
                }

                var jwtToken = _tokenHandlerWrapper.ReadJwtToken(token);
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub");
                var userId = string.Empty;
                if (userIdClaim != null)
                {
                    userId = userIdClaim.Value;
                }

                if (string.IsNullOrEmpty(userId))
                {
                    return (false, "User ID not found", Guid.Empty);
                }

                if (!Guid.TryParse(userId, out var userGuid))
                {
                    return (false, "Invalid User ID format", Guid.Empty);
                }

                return (true, string.Empty, userGuid);
            });
        }
    }
}
