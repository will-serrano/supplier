namespace Supplier.Customers.Dto.Responses
{
    public class ErrorResponseDto : CustomerResponseBase
    {
        public ErrorResponseDto(string errorDetail)
            : base(errorDetail)
        {
        }
    }
}
