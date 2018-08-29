using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerousAPI.BusinessEntities
{
    public class Common
    {
        /// <summary>
        /// Defines the status of a batch payment to an organisation
        /// </summary>
        public enum BatchPaymentToOrganisationStatus
        {
            Unprocessed,
            AwaitingCompletion,
            Completed,
            PaymentToHeartBurst
        }

        /// <summary>
        /// Mode of payment
        /// </summary>
        public enum ABAMode
        {
            DirectDebit,
            DirectCredit
        }

        public enum PaymentType
        {
            Day3Payment = 1
        }
    }
}
