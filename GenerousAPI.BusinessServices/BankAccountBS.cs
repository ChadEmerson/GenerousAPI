using GenerousAPI.BusinessEntities;
using GenerousAPI.DataAccessLayer;

namespace GenerousAPI.BusinessServices
{
    public class BankAccountBS : IBankAccountBS
    {
        private IBankAccountDAL _IBankAccountDAL = null;

        /// <summary>
        /// Ctor
        /// </summary>
        public BankAccountBS()
        {
            _IBankAccountDAL = new BankAccountDAL();
        }

        /// <summary>
        /// Create a new donor payment profile for a donor
        /// </summary>
        /// <param name="BankAccount">Donor payment profile details</param>
        /// <returns>Payment Response with success, message, and profile token</returns>
        public ProcessorResponse CreateBankAccount(BankAccount BankAccount)
        {
            return _IBankAccountDAL.CreateBankAccount(BankAccount);
        }

        /// <summary>
        /// Update existing donor payment profile for a donor
        /// </summary>
        /// <param name="BankAccount">Donor payment profile details</param>
        /// <returns>Payment Response with success, message, and profile token</returns>
        public ProcessorResponse UpdateBankAccount(BankAccount BankAccount)
        {
            return _IBankAccountDAL.UpdateBankAccount(BankAccount);
        }

        /// <summary>
        /// Get donors payment profile details
        /// </summary>
        /// <param name="BankAccountTokenId">Payment profile token id</param>
        /// <returns>Donor payment profile details</returns>
        public BankAccountDTO GetBankAccount(string BankAccountTokenId)
        {
            return _IBankAccountDAL.GetBankAccount(BankAccountTokenId);
        }

        /// <summary>
        /// Get Bank account details
        /// </summary>
        /// <param name="bankAccountId">Guid iD of bank</param>
        /// <returns>Bank account details</returns>
        public BankAccount GetBankAccountById(System.Guid bankAccountId)
        {
            return _IBankAccountDAL.GetBankAccountById(bankAccountId);
        }

        /// <summary>
        /// Delete a payment profile for a donor
        /// </summary>
        /// <param name="BankAccountTokenId">Payment profile token id</param>
        /// <returns>Payment Response with success, message, and profile token</returns>
        public ProcessorResponse DeleteBankAccount(string BankAccountTokenId)
        {
            return _IBankAccountDAL.DeleteBankAccount(BankAccountTokenId);
        }

        /// <summary>
        /// Create a payment batch for an organisatsion
        /// </summary>
        /// <param name="batch">Payment batch details</param>
        public void CreatePaymentToOrganisationBatch(PaymentToOrganisationBatch batch)
        {
            _IBankAccountDAL.CreatePaymentToOrganisationBatch(batch);
        }
    }
}
