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
        /// Update a record for transaction details
        /// </summary>
        /// <param name="transactionDetails">Transaction details</param>
        /// <returns>Response with success, message, and profile token</returns>
        ProcessorResponse UpdateTransactionRecord(TransactionDetail transactionDetails);

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

        /// <summary>
        /// Get donation transaction details based on a transaction id
        /// </summary>
        /// <param name="donationTransactionId">Transaction id to search</param>
        /// <returns>Donation transaction details</returns>
        List<DonationTransactionWithRelatedData> GetDonationTransaction_WithRelatedData(List<Guid> donationTransactionIds);

        /// <summary>
        /// Update the Transaction List with the donation details
        /// </summary>
        /// <param name="donationTransList">Donation Transaction List</param>
        void UpdateDonationTransactionList(IEnumerable<TransactionDetail> donationTransList);

        /// <summary>
        /// Get a collection of donation transactions 
        /// </summary>
        /// <param name="dateTimeToBeProcessed">Date/time when transaction is being processed</param>
        /// <param name="paymentProcessStatus">Enum process status</param>
        /// <param name="paymentMethod">Enum of payment type (credit card, direct debit)</param>
        /// <returns>Collection of donation transaction details</returns>
        IEnumerable<TransactionDetail> GetDonationTransactionsByStatus_ForProcessing(DateTime dateTimeToBeProcessed, PaymentGatewayProcessing.Helpers.Enums.PaymentProcessStatus processStatus, PaymentGatewayProcessing.Helpers.Enums.PaymentMethod PaymentMethod);
    }
}
