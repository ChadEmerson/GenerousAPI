
namespace GenerousAPI.BusinessEntities
{
    public class PaymentProfileDTO
    {
        public TransactionMode TransactionMode { get; set; }
        public string CustomerFirstName { get; set; }
        public string CustomerLastName { get; set; }
        public string BillingAddress { get; set; }
        public string BillingCity { get; set; }
        public string BillingState { get; set; }
        public string Zip { get; set; }
        public string AccountNumber { get; set; }
        public string CardNumber { get; set; }
        public string CardType { get; set; }
        public string ExpirationMonth { get; set; }
        public string ExpirationYear { get; set; }
        public string SecurityCode { get; set; }
        public string BankName { get; set; }
        public string RoutingNumber { get; set; }
        public string AccountType { get; set; }
        public string PaymentProfileAuthToken { get; set; }
        public string MemberAuthToken { get; set; }

        public string CustomerFullName
        {
            get
            {
                return CustomerFirstName + (!string.IsNullOrEmpty(CustomerLastName) ? " " + CustomerLastName : "");
            }
        }
        public string FormattedExpirationDate
        {
            get
            {
                return ExpirationMonth.ToString() + "/" + ExpirationYear.ToString();
            }
        }
    }

}
