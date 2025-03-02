using FluentValidation;
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
    public class TransactionRequestService(
        IValidator<TransactionRequestDto> validator, 
        ITransactionRequestRepository repository, 
        ITransactionRequestMapper mapper, 
        CustomerMessagePublisher messagePublisher, 
        ICustomerValidationClient customerValidationClient) : ITransactionRequestService
    {
        private readonly IValidator<TransactionRequestDto> _transactionRequestDtoValidator = validator;
        private readonly ITransactionRequestRepository _transactionRequestRepository = repository;
        private readonly ITransactionRequestMapper _transactionRequestMapper = mapper;
        private readonly CustomerMessagePublisher _messagePublisher = messagePublisher;
        private readonly ICustomerValidationClient _customerValidationClient = customerValidationClient;

        public async Task<TransactionResponseDto> SimularTransacaoAsync(TransactionRequestDto dto)
        {
            var validationResult = await _transactionRequestDtoValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var transactionRequest = _transactionRequestMapper.MapToTransactionRequest(dto)
                ?? throw new ArgumentNullException(nameof(dto));

            transactionRequest = await _transactionRequestRepository.RegisterTransactionRequestAsync(transactionRequest) // Pending
                ?? throw new ArgumentNullException(nameof(dto));

            var clientValidationResult = await _customerValidationClient.ValidateCustomerAsync(transactionRequest);

            if (!clientValidationResult.IsValid)
            {
                transactionRequest.Detail = clientValidationResult.Message ?? string.Empty;
                await _transactionRequestRepository.UpdateTransactionRequestAsync(transactionRequest); //Rejected
                //FIX! log    
                return new TransactionResponseDto { Status = "NEGADO" };
            }

            transactionRequest.TransactionId = Guid.NewGuid();
            await _transactionRequestRepository.UpdateTransactionRequestAsync(transactionRequest); // Authorized

            var transactionMessageDataToSend = _transactionRequestMapper.MapToTransactionMessageData(transactionRequest);

            var mensagem = new MessageWrapper
            {
                Version = "V1",
                Data = transactionMessageDataToSend
            };

            await _messagePublisher.Send(mensagem);

            await _transactionRequestRepository.UpdateTransactionRequestAsync(transactionRequest); //Processing

            return new TransactionResponseDto { Status = "APROVADO", TransactionId = transactionRequest.TransactionId };
        }
    }
}
