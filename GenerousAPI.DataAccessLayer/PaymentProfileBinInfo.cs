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
    
    public partial class PaymentProfileBinInfo
    {
        public System.Guid BinInfoId { get; set; }
        public System.Guid PaymentProfileId { get; set; }
        public string Brand { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public string BankName { get; set; }
        public string CardType { get; set; }
        public Nullable<int> Latitude { get; set; }
        public Nullable<int> Longitude { get; set; }
        public string Scheme { get; set; }
    }
}
