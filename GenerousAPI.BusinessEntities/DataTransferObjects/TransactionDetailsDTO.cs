using System;

namespace GenerousAPI.BusinessEntities
{
    public class TransactionDetailsDTO
    {
        public Guid Id { get; set; }
        public string BankAccountTokenId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentProfileTokenId { get; set; }
        public int PaymentMethodId { get; set; }
        public DateTime ProcessDateTime { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseText { get; set; }
    }
}

