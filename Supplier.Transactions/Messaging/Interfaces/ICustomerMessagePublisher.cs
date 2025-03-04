using Supplier.Contracts.Transactions;

namespace Supplier.Transactions.Messaging.Interfaces
{
    /// <summary>
    /// Interface for publishing customer messages.
    /// </summary>
    public interface ICustomerMessagePublisher
    {
        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="mensagem">The message to be sent.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task Send(MessageWrapper mensagem);
    }
}
