using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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

        public enum CardType
        {
            [Description("Visa")]
            Visa,

            [Description("MasterCard")]
            MasterCard,

            [Description("AMEX")]
            AMEX,

            [Description("International")]
            International
        }


        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }
    }
}
