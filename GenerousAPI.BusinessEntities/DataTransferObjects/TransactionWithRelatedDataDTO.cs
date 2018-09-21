using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerousAPI.BusinessEntities
{
    public class TransactionWithRelatedDataDTO
    {
        public PaymentProfileDTO PaymentProfile { get; set; }

        public TransactionDetailsDTO TransactionDetails { get; set; }
    }
}
