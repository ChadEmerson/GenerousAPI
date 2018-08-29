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
    
    public partial class PaymentToOrganisationBatch
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PaymentToOrganisationBatch()
        {
            this.PaymentToOrganisationBatchLineItems = new HashSet<PaymentToOrganisationBatchLineItem>();
        }
    
        public System.Guid Id { get; set; }
        public string BatchNumber { get; set; }
        public bool IsBankVerificationBatch { get; set; }
        public Nullable<System.DateTime> LastProcessedDateTime { get; set; }
        public Nullable<System.DateTime> BatchCompletedDateTime { get; set; }
        public System.DateTime CreateDateTime { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<short> isPaymentToDay3 { get; set; }
    
        public virtual PaymentToOrganisationBatch PaymentToOrganisationBatch1 { get; set; }
        public virtual PaymentToOrganisationBatch PaymentToOrganisationBatch2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PaymentToOrganisationBatchLineItem> PaymentToOrganisationBatchLineItems { get; set; }
    }
}