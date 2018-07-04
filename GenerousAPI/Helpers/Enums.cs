
using System.ComponentModel;

namespace GenerousAPI.Helpers
{
    public class Enums
    {
        /// <summary>
        /// Defines current configured and allowed payment gateways
        /// </summary>
        public enum PaymentGatewayType : byte
        {
            GENEROUS = 1,
            CARDACCESS = 2,
            PAYPAL = 3,
            EWAY = 4,
            STRIPE = 5,
        }

        /// <summary>
        /// Defines the types of payment methods accepted in the system
        /// </summary>
        public enum PaymentMethod : byte
        {
            /// <summary>
            /// Credit card payment
            /// </summary>
            [Description("Credit Card")]
            CreditCard = 1,

            /// <summary>
            /// Direct Debit from bank account payment
            /// </summary>
            [Description("Direct Debit")]
            DirectDebit = 2,

            
        }
    }
}