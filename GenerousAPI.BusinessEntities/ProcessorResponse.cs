
namespace GenerousAPI.BusinessEntities
{
    public class ProcessorResponse
    {
        public ProcessorResponse()
        {

        }

        public bool IsSuccess { get; set; }
        public string AuthToken { get; set; }
        public string Message { get; set; }

        public int FundId { get; set; }

        public decimal Amount { get; set; }
    }
}
