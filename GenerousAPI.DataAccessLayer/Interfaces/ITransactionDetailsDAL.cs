using GenerousAPI.BusinessEntities;
using System;
using System.Collections.Generic;

namespace GenerousAPI.DataAccessLayer
{
    public interface ITransactionDetailsDAL
    {
        /// <summary>
        /// Create a new record for transaction details
        /// </summary>
        /// <param name="transactionDetails">Transaction details</param>
        /// <returns>Response with success, message, and profile token</returns>
        ProcessorResponse CreateTransactionRecord(TransactionDetail transactionDetails);

        /// <summary>
        /// Update a record for transaction details
        /// </summary>
        /// <param name="transactionDetails">Transaction details</param>
        /// <returns>Response with success, message, and profile token</returns>
        ProcessorResponse UpdateTransactionRecord(TransactionDetail transactionDetails);

        /// <summary>
        /// Get collection of donations with related data
        /// </summary>
        /// <param name="batchId">Batch ID to lookup</param>
        /// <returns>Collection of donations with related data</returns>
        List<DonationTransactionWithRelatedData> GetDonationTransactionsOf_Batch_WithRelatedData(Guid batchId);

        /// <summary>
        /// Update existing Bank account
        /// </summary>
        /// <param name="paymentProfile">Donor payment profile details</param>
        /// <returns>Bank account details</returns>
        TransactionDetailsDTO GetTransactionDetails(Guid transactionId);

        /// <summary>
        /// Get collection of transactions
        /// </summary>
        /// <param name="bankAccountTokenId">Bank Account token Id</param>
        /// <returns>Transaction details</returns>
        IEnumerable<TransactionDetailsDTO> GetTransactionDetailsForBankAccount(string bankAccountTokenId);        
    }
}
