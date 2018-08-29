using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerousAPI.DataAccessLayer
{
    public class DonationTransactionWithRelatedData
    {
        /// <summary>
        /// Reference to the donation transaction Business Entity 
        /// </summary>
        public TransactionDetail TransactionDetail { get; set; }

        /// <summary>
        /// Reference to the donation transaction Business Entity 
        /// </summary>
        public PaymentProfile DonorPaymentProfile { get; set; }

        /// <summary>
        /// Status of the donation transaction
        /// </summary>
        public string DonationTransaction_ProcessStatus { get; set; }

        /// <summary>
        /// Payment method of the donation
        /// </summary>
        public string DonationTransaction_PaymentMethod { get; set; }

        /// <summary>
        /// Regular donation or a once off
        /// </summary>
        public string Donation_OneOff_Regular { get; set; }
    }
}
