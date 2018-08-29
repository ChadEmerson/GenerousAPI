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
    
    public partial class PaymentToOrganisationBatchLineItem
    {
        public System.Guid Id { get; set; }
        public long LineItemNumber { get; set; }
        public System.Guid BatchId { get; set; }
        public string BatchNumber { get; set; }
        public Nullable<System.Guid> BankAccountId { get; set; }
        public bool IsBankVerification { get; set; }
        public long TotalPaymentsReceived { get; set; }
        public decimal TotalAmountReceived { get; set; }
        public decimal TotalAmountPaidToOrganisation { get; set; }
        public Nullable<System.DateTime> ProcessDateTime { get; set; }
        public byte ProcessStatusId { get; set; }
        public Nullable<System.DateTime> Processed_PaymentSubmittedDateTime { get; set; }
        public Nullable<System.DateTime> Processed_PaymentFinalisedDateTime { get; set; }
        public Nullable<System.DateTime> LastProcessedDateTime { get; set; }
        public Nullable<byte> ProcessRetryCounter { get; set; }
        public System.DateTime CreateDateTime { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> LastModifiedDateTime { get; set; }
        public string LastModifiedBy { get; set; }
        public bool DoNotProcess { get; set; }
        public string BankAccountBSB { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankAcountName { get; set; }
    
        public virtual PaymentProcessStatu PaymentProcessStatu { get; set; }
        public virtual PaymentToOrganisationBatch PaymentToOrganisationBatch { get; set; }
    }
}
