using GenerousAPI.BusinessEntities;
using GenerousAPI.DataAccessLayer;
using System;
using System.Collections.Generic;

namespace GenerousAPI.BusinessServices
{
    public interface ITransactionDetailsBS
    {
        /// <summary>
        /// Create a new record for transaction details
        /// </summary>
        /// <param name="transactionDetails">Transaction details</param>
        /// <returns>Response with success, message, and profile token</returns>
        ProcessorResponse CreateTransactionRecord(TransactionDetail transactionDetails);

        /// <summary>
        /// Get transaction details
        /// </summary>
        /// <param name="transactionId">Transaction Id</param>
        /// <returns>Transaction details</returns>
        TransactionDetailsDTO GetTransactionDetails(Guid transactionId);

        /// <summary>
        /// Get collection of transactions
        /// </summary>
        /// <param name="bankAccountTokenId">Bank Account token Id</param>
        /// <returns>Transaction details</returns>
        IEnumerable<TransactionDetailsDTO> GetTransactionDetailsForBankAccount(string bankAccountTokenId); 
    }
}
