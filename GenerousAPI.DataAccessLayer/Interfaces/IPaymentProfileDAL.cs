namespace GenerousAPI.DataAccessLayer
{
    using GenerousAPI.BusinessEntities;
    using System.Collections.Generic;

    public interface IPaymentProfileDAL
    {
        /// <summary>
        /// Create a new donor payment profile for a donor
        /// </summary>
        /// <param name="paymentProfile">Donor payment profile details</param>
        /// <returns>Payment Response with success, message, and profile token</returns>
        ProcessorResponse CreatePaymentProfile(PaymentProfile paymentProfile);

        /// <summary>
        /// Update existing donor payment profile for a donor
        /// </summary>
        /// <param name="paymentProfile">Donor payment profile details</param>
        /// <returns>Donor payment profile details</returns>
        ProcessorResponse UpdatePaymentProfile(PaymentProfile PaymentProfile);

        /// <summary>
        /// Delete a payment profile for a donor
        /// </summary>
        /// <param name="paymentProfileTokenId">Payment profile token id</param>
        /// <returns>Donor payment profile details</returns>
        ProcessorResponse DeletePaymentProfile(string paymentProfileTokenId);

        /// <summary>
        /// Get donors payment profile details
        /// </summary>
        /// <param name="paymentProfileTokenId">Payment profile token ID of donor</param>
        /// <returns>Donor payment profile details</returns>
        PaymentProfileDTO GetPaymentProfile(string paymentProfileTokenId);

        /// <summary>
        /// Get payment profile details for cards that are expiring
        /// </summary>
        /// <param name="ExpiryMonth">Expiring month</param>
        /// <param name="ExpiryYear">Expiring year</param>
        /// <returns>Donor payment profile details</returns>
        List<ContactDetailsDTO> GetExpiringCards(string expiryMonth, string expiryYear);

        // <summary>
        /// Get when a card is expiring for the token
        /// </summary>
        /// <param name="ExpiryMonth">tokenId of payment profile</param>
        /// <returns>Donor payment profile details</returns>
        ContactDetailsDTO GetCardExpiryForTokenId(string tokenId);

        /// <summary>
        /// Save Expiring Credit Card Detais
        /// </summary>
        /// <param name="expiringCardDetails">Details of cards</param>
        void SaveExpiringCreditCardDetais(ExpiringCreditCardsForOrganisation expiringCardDetails);

        /// <summary>
        /// Clear out records from table
        /// </summary>
        /// <returns>ProcessorResponse info</returns>
        ProcessorResponse ClearExpiringCreditCardInfo();

        /// <summary>
        /// Get list of expiring credit cards for an organisation
        /// </summary>
        /// <param name="organisationId">Organisation Id to get details for</param>
        /// <returns>Collection of contact details</returns>
        List<ContactDetailsDTO> GetExpiringCreditCardInfoForOrganisation(int organisationId);
    }
}
