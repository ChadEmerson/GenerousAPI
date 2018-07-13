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

    }
}
