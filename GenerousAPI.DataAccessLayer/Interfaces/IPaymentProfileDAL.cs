namespace GenerousAPI.DataAccessLayer
{
    using GenerousAPI.BusinessEntities;

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
    }
}
