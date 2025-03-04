namespace Supplier.Transactions.Enums
{
    public enum TransactionStatus
    {
        Pending,         // Transação recebida e registrada no banco
        Rejected,        // Transação negada devido à análise
        Authorized,      // GUID Gerado - autorização do débito
        Processing,      // Débito sendo processado
        Completed,       // Débito realizado com sucesso
        Failed           // Erro em qualquer etapa do processo
    }
}
