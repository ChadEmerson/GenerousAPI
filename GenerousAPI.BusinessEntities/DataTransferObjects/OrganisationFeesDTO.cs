using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerousAPI.BusinessEntities
{
    public class OrganisationFeesDTO
    {
        public System.Guid Id { get; set; }
        public int OrganisationId { get; set; }
        public bool IsPromoBilling { get; set; }
        public Nullable<System.DateTime> PromoBillingExpiresOn { get; set; }
        public Nullable<int> CurrencyCode { get; set; }

        public Nullable<int> VisaFee { get; set; }
        public Nullable<decimal> VisaMinAmount { get; set; }
        public Nullable<int> InternationaliFee { get; set; }
        public Nullable<decimal> InternationalMinAmount { get; set; }
        public Nullable<int> AmexFee { get; set; }
        public Nullable<decimal> AmexMinAmount { get; set; }
        public Nullable<int> DirectDebitFee { get; set; }
        public Nullable<decimal> DirectDebitMin { get; set; }
        public Nullable<decimal> TextToGiveFee { get; set; }
        public Nullable<decimal> SmsReminderFee { get; set; }
        public Nullable<decimal> EventTicketBracket1 { get; set; }
        public Nullable<decimal> EventTicketBracket2 { get; set; }
        public Nullable<decimal> EventTicketBracket3 { get; set; }
        public Nullable<decimal> EventTicketBracket4 { get; set; }
        public Nullable<decimal> EventTicketBracket5 { get; set; }
        public Nullable<decimal> RefundFee { get; set; }
        public Nullable<decimal> ChargebackFee { get; set; }
        public Nullable<decimal> GivingModuleFee { get; set; }
        public Nullable<decimal> PaymentModuleFee { get; set; }
        public Nullable<decimal> CampaignPortalModuleFee { get; set; }
        public Nullable<decimal> EventModuleFee { get; set; }
        public Nullable<decimal> SocialMediaModuleFee { get; set; }
        public Nullable<decimal> ChurchManSystemModuleFee { get; set; }
    }
}
