
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
    }
}