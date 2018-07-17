using System;
using GenerousAPI.BusinessEntities;
using GenerousAPI.DataAccessLayer;
using System.Collections.Generic;

namespace GenerousAPI.BusinessServices
{
    public class TransactionDetailsBS : ITransactionDetailsBS
    {
        private ITransactionDetailsDAL _ITransactionDetailsDAL = null;
    
        /// <summary>
        /// Ctor
        /// </summary>
        public TransactionDetailsBS()
        {
            _ITransactionDetailsDAL = new TransactionDetailsDAL();
        }

        /// <summary>
        /// Create a new record for transaction details
        /// </summary>
        /// <param name="transactionDetails">Transaction details</param>
        /// <returns>Response with success, message, and profile token</returns>
        public ProcessorResponse CreateTransactionRecord(TransactionDetail transactionDetails)
        {
            return _ITransactionDetailsDAL.CreateTransactionRecord(transactionDetails);
        }

        /// <summary>
        /// Update existing Bank account
        /// </summary>
        /// <param name="paymentProfile">Donor payment profile details</param>
        /// <returns>Bank account details</returns>
        public TransactionDetailsDTO GetTransactionDetails(Guid transactionId)
        {
            return _ITransactionDetailsDAL.GetTransactionDetails(transactionId);
        }

        /// <summary>
        /// Get collection of transactions
        /// </summary>
        /// <param name="bankAccountTokenId">Bank Account token Id</param>
        /// <returns>Transaction details</returns>
        public IEnumerable<TransactionDetailsDTO> GetTransactionDetailsForBankAccount(string bankAccountTokenId)
        {
            return _ITransactionDetailsDAL.GetTransactionDetailsForBankAccount(bankAccountTokenId);

        }
    }
}
