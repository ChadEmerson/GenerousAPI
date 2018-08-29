using GenerousAPI.BusinessEntities;
using GenerousAPI.DataAccessLayer;

namespace GenerousAPI.BusinessServices
{
    public interface IBankAccountBS
    {
        /// <summary>
        /// Create a new Bank account
        /// </summary>
        /// <param name="paymentProfile">Bank account details</param>
        /// <returns>Payment Response with success, message, and profile token</returns>
        ProcessorResponse CreateBankAccount(BankAccount paymentProfile);

        /// <summary>
        /// Update existing Bank account
        /// </summary>
        /// <param name="paymentProfile">Donor payment profile details</param>
        /// <returns>Bank account details</returns>
        ProcessorResponse UpdateBankAccount(BankAccount PaymentProfile);

        /// <summary>
        /// Delete a Bank account
        /// </summary>
        /// <param name="paymentProfileTokenId">Payment profile token id</param>
        /// <returns>Bank account details</returns>
        ProcessorResponse DeleteBankAccount(string bankAccountTokenId);

        /// <summary>
        /// Get Bank account details
        /// </summary>
        /// <param name="paymentProfileTokenId">Payment profile token ID of donor</param>
        /// <returns>Bank account details</returns>
        BankAccountDTO GetBankAccount(string bankAccountTokenId);

        /// <summary>
        /// Get Bank account details
        /// </summary>
        /// <param name="bankAccountId">Guid iD of bank</param>
        /// <returns>Bank account details</returns>
        BankAccount GetBankAccountById(System.Guid bankAccountId);

        /// <summary>
        /// Create a payment batch for an organisatsion
        /// </summary>
        /// <param name="batch">Payment batch details</param>
        void CreatePaymentToOrganisationBatch(PaymentToOrganisationBatch batch);


    }
}
