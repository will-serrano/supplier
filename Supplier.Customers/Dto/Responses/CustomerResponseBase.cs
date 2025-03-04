using Supplier.Customers.Models;

namespace Supplier.Customers.Dto.Responses
{
    public abstract class CustomerResponseBase
    {
        private static readonly string LABEL_OK = "OK";
        private static readonly string LABEL_ERROR = "ERRO";

        public string Status { get; protected set; }
        public string? ErrorDetail { get; protected set; }

        protected CustomerResponseBase()
        {
            Status = LABEL_OK;
        }

        protected CustomerResponseBase(string errorDetail)
        {
            Status = LABEL_ERROR;
            ErrorDetail = errorDetail;
        }
    }
}
