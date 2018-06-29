using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GenerousAPI.Models
{
    public class TransactionDetails
    {
        public decimal Amount { get; set; }

        public string PaymentProfileTokenId { get; set; }

        public int FundId { get; set; }

        public string TransactionReferenceNumber { get; set; }

        public bool IsTest { get; set; }

    }
}