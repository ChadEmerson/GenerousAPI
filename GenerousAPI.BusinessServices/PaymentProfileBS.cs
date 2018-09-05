namespace GenerousAPI.BusinessServices
{
    using DataAccessLayer;
    using GenerousAPI.BusinessEntities;
    using System.Collections.Generic;

    public class PaymentProfileBS : IPaymentProfileBS
    {
        private IPaymentProfileDAL _IPaymentProfileDAL = null;
        
        /// <summary>
        /// Ctor
        /// </summary>
        public PaymentProfileBS()
        {
            _IPaymentProfileDAL = new PaymentProfileDAL();
        }

        /// <summary>
        /// Create a new donor payment profile for a donor
        /// </summary>
        /// <param name="PaymentProfile">Donor payment profile details</param>
        /// <returns>Payment Response with success, message, and profile token</returns>
        public ProcessorResponse CreatePaymentProfile(PaymentProfile PaymentProfile)
        {
            return _IPaymentProfileDAL.CreatePaymentProfile(PaymentProfile);
        }

        /// <summary>
        /// Update existing donor payment profile for a donor
        /// </summary>
        /// <param name="PaymentProfile">Donor payment profile details</param>
        /// <returns>Payment Response with success, message, and profile token</returns>
        public ProcessorResponse UpdatePaymentProfile(PaymentProfile PaymentProfile)
        {
            return _IPaymentProfileDAL.UpdatePaymentProfile(PaymentProfile);
        }

        /// <summary>
        /// Get donors payment profile details
        /// </summary>
        /// <param name="paymentProfileTokenId">Payment profile token id</param>
        /// <returns>Donor payment profile details</returns>
        public PaymentProfileDTO GetPaymentProfile(string paymentProfileTokenId)
        {
            return _IPaymentProfileDAL.GetPaymentProfile(paymentProfileTokenId);
        }

        /// <summary>
        /// Delete a payment profile for a donor
        /// </summary>
        /// <param name="paymentProfileTokenId">Payment profile token id</param>
        /// <returns>Payment Response with success, message, and profile token</returns>
        public ProcessorResponse DeletePaymentProfile(string paymentProfileTokenId)
        {
            return _IPaymentProfileDAL.DeletePaymentProfile(paymentProfileTokenId);
        }

        /// <summary>
        /// Get payment profile details for cards that are expiring
        /// </summary>
        /// <param name="ExpiryMonth">Expiring month</param>
        /// <param name="ExpiryYear">Expiring year</param>
        /// <returns>Donor payment profile details</returns>
        public List<ContactDetailsDTO> GetExpiringCards(string expiryMonth, string expiryYear)
        {
            return _IPaymentProfileDAL.GetExpiringCards(expiryMonth, expiryYear);
        }

        // <summary>
        /// Get when a card is expiring for the token
        /// </summary>
        /// <param name="ExpiryMonth">tokenId of payment profile</param>
        /// <returns>Donor payment profile details</returns>
        public ContactDetailsDTO GetCardExpiryForTokenId(string tokenId)
        {
            return _IPaymentProfileDAL.GetCardExpiryForTokenId(tokenId);
        }

        /// <summary>
        /// Save Expiring Credit Card Detais
        /// </summary>
        /// <param name="expiringCardDetails">Details of cards</param>
        public void SaveExpiringCreditCardDetais(ExpiringCreditCardsForOrganisation expiringCardDetails)
        {
            _IPaymentProfileDAL.SaveExpiringCreditCardDetais(expiringCardDetails);
        }

        /// <summary>
        /// Clear out records from table
        /// </summary>
        /// <returns>ProcessorResponse info</returns>
        public ProcessorResponse ClearExpiringCreditCardInfo()
        {
            return _IPaymentProfileDAL.ClearExpiringCreditCardInfo();
        }

        /// <summary>
        /// Get list of expiring credit cards for an organisation
        /// </summary>
        /// <param name="organisationId">Organisation Id to get details for</param>
        /// <returns>Collection of contact details</returns>
        public List<ContactDetailsDTO> GetExpiringCreditCardInfoForOrganisation(int organisationId)
        {
            return _IPaymentProfileDAL.GetExpiringCreditCardInfoForOrganisation(organisationId);
        }
    }
}
