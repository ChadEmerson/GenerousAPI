//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GenerousAPI.DataAccessLayer
{
    using System;
    using System.Collections.Generic;
    
    public partial class BankAccount
    {
        public System.Guid BankAccountId { get; set; }
        public string BankAcountName { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankAccountBSB { get; set; }
        public string BankAccountTokenId { get; set; }
        public Nullable<bool> Active { get; set; }
        public string BankVerificationAmounts { get; set; }
        public Nullable<System.DateTime> BankVerificationRequestedOn { get; set; }
        public string BankVerificationRequestedBy { get; set; }
        public string BankVerificationPaymentStatus { get; set; }
    }
}
