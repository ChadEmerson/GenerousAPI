using System;

namespace GenerousAPI.BusinessEntities
{
    public class BankAccountDTO
    {
        public Guid BankAccountId { get; set; }
        public string BankAcountName { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankAccountBSB { get; set; }
        public string BankAccountTokenId { get; set; }

        public string BankVerificationAmounts { get; set; }
        public Nullable<System.DateTime> BankVerificationRequestedOn { get; set; }
        public string BankVerificationRequestedBy { get; set; }
        public string BankVerificationPaymentStatus { get; set; }

    }
}
