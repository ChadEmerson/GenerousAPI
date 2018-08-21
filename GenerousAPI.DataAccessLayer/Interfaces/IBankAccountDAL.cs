using GenerousAPI.BusinessEntities;

namespace GenerousAPI.DataAccessLayer
{
    public interface IBankAccountDAL
    {
        /// <summary>
        /// Create a new bank account
        /// </summary>
        /// <param name="BankAccount"> bank account details</param>
        /// <returns>Payment Response with success, message, and profile token</returns>
        ProcessorResponse CreateBankAccount(BankAccount BankAccount);

        /// <summary>
        /// Update existing  bank account
        /// </summary>
        /// <param name="BankAccount"> bank account details</param>
        /// <returns>Bank account details</returns>
        ProcessorResponse UpdateBankAccount(BankAccount BankAccount);

        /// <summary>
        /// Delete a  bank account
        /// </summary>
        /// <param name="BankAccountTokenId"> bank account token id</param>
        /// <returns>Bank account details</returns>
        ProcessorResponse DeleteBankAccount(string BankAccountTokenId);

        /// <summary>
        /// Get Bank account details
        /// </summary>
        /// <param name="BankAccountTokenId">Bank account token id</param>
        /// <returns>Bank account details</returns>
        BankAccountDTO GetBankAccount(string BankAccountTokenId);
    }
}
