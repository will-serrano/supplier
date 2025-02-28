using FluentValidation;
using Newtonsoft.Json;
using Supplier.Transactions.Dto.Requests;
using Supplier.Transactions.Dto.Responses;
using Supplier.Transactions.HttpClients.Interfaces;
using Supplier.Transactions.Mappers.Interfaces;
using Supplier.Transactions.Messaging;
using Supplier.Transactions.Messaging.Contracts;
using Supplier.Transactions.Repositories.Interfaces;
using Supplier.Transactions.Services.Interfaces;

namespace Supplier.Transactions.Services
{
    public class TransactionRequestService : ITransactionRequestService
    {
        private readonly IValidator<TransactionRequestDto> _transactionRequestDtoValidator;
        private readonly ITransactionRequestRepository _transactionRequestRepository;
        private readonly ITransactionRequestMapper _transactionRequestMapper;
        private readonly CustomerMessagePublisher _messagePublisher;
        private readonly ICustomerValidationClient _customerValidationClient;

        public TransactionRequestService(IValidator<TransactionRequestDto> validator, ITransactionRequestRepository repository, ITransactionRequestMapper mapper, CustomerMessagePublisher messagePublisher, ICustomerValidationClient customerValidationClient)
        {
            _transactionRequestDtoValidator = validator;
            _transactionRequestRepository = repository;
            _transactionRequestMapper = mapper;
            _messagePublisher = messagePublisher;
            _customerValidationClient = customerValidationClient;
        }

        public async Task<TransactionResponseDto> SimularTransacaoAsync(TransactionRequestDto dto)
        {
            var validationResult = await _transactionRequestDtoValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var transactionRequest = _transactionRequestMapper.MapToTransactionRequest(dto);

            transactionRequest = await _transactionRequestRepository.RegisterTransactionRequestAsync(transactionRequest); // Pending
            
            var clientValidationResult = await _customerValidationClient.ValidateCustomerAsync(transactionRequest);

            if (!clientValidationResult.IsValid)
            {
                transactionRequest.Detail = clientValidationResult.Message;
                await _transactionRequestRepository.UpdateTransactionRequestAsync(transactionRequest); //Rejected
                //FIX! log    
                return new TransactionResponseDto { Status = "NEGADO" };
            }

            transactionRequest.TransactionId = Guid.NewGuid();
            await _transactionRequestRepository.UpdateTransactionRequestAsync(transactionRequest); // Authorized

            var transactionMessageDataToSend = _transactionRequestMapper.MapToTransactionMessageData(transactionRequest);

            var mensagem = new TransactionMessage
            {
                Version = "V1",
                Data = JsonConvert.SerializeObject(transactionMessageDataToSend)
            };

            await _messagePublisher.Send(mensagem);

            await _transactionRequestRepository.UpdateTransactionRequestAsync(transactionRequest); //Processing

            return new TransactionResponseDto { Status = "APROVADO", TransactionId = transactionRequest.TransactionId };
        }
    }
}
