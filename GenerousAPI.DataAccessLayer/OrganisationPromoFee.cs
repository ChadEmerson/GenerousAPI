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
    
    public partial class OrganisationPromoFee
    {
        public System.Guid FeeProcessingId { get; set; }
        public Nullable<double> OrganisationId { get; set; }
        public Nullable<double> VisaFee { get; set; }
        public Nullable<decimal> VisaMinAmount { get; set; }
        public Nullable<double> InternationalFee { get; set; }
        public Nullable<decimal> InternationalMinAmount { get; set; }
        public Nullable<double> AmexFee { get; set; }
        public Nullable<decimal> AmexMinAmount { get; set; }
        public Nullable<double> DirectDebitFee { get; set; }
        public Nullable<decimal> DirectDebitMin { get; set; }
    }
}
