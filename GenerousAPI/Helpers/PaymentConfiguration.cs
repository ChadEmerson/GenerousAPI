using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GenerousAPI.Helpers
{
    public class PaymentConfiguration
    {
        /// <summary>
        /// prefix of payment to an organisation
        /// </summary>
        public static string PaymentToOrganisationBatchNumberPrefix
        {
            get
            {
                return "PO"; //Payment To Organisation
            }
        }

        /// <summary>
        /// Prefix for a Bank Verification payment
        /// </summary>
        public static string BankVerificationBatchNumberPrefix
        {
            get
            {
                return "BV"; //Bank Verification
            }
        }

        /// <summary>
        /// Name of the batch process that runs for processing transactions
        /// </summary>
        public static String TransactionProcessBatchName
        {
            get
            {
                return "TX_PROCESS_BATCH";
            }
        }
    }
}