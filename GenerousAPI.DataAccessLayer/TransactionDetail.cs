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
    
    public partial class TransactionDetail
    {
        public System.Guid Id { get; set; }
        public string BankAccountTokenId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentProfileTokenId { get; set; }
        public int PaymentMethodId { get; set; }
        public System.DateTime ProcessDateTime { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseText { get; set; }
        public string AuditNumber { get; set; }
        public string CustomerReference { get; set; }
        public Nullable<System.Guid> PaymentToOrganisationBatchId { get; set; }
        public Nullable<System.Guid> PaymentToOrganisationBatchLineItemId { get; set; }
        public Nullable<bool> IsValidForPaymentToOrganisation { get; set; }
        public Nullable<byte> ProcessStatusId { get; set; }
        public Nullable<byte> TransactionTypeId { get; set; }
        public Nullable<decimal> AmountAfterFeeDeductions { get; set; }
        public Nullable<decimal> ProcessingFeeAmount { get; set; }
        public Nullable<decimal> TransactionFeeAmount { get; set; }
        public Nullable<int> OrganisationId { get; set; }
        public Nullable<int> NumberOfEventTickets { get; set; }
        public Nullable<decimal> TicketFeeAmount { get; set; }
        public Nullable<bool> DoNotProcess { get; set; }
        public Nullable<System.DateTime> Processed_DonationFinalisedDateTime { get; set; }
        public Nullable<System.DateTime> Processed_DonationSubmittedDateTime { get; set; }
        public Nullable<int> ProcessRetryCounter { get; set; }
        public string LastModifiedBy { get; set; }
        public Nullable<System.DateTime> LastModifiedDateTime { get; set; }
        public Nullable<System.DateTime> LastProcessedDateTime { get; set; }
        public Nullable<bool> HasProcessStatusChanged { get; set; }
        public Nullable<System.Guid> TransactionGroupId { get; set; }
    
        public virtual TransactionType TransactionType { get; set; }
    }
}
