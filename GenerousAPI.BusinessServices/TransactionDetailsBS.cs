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
        /// Get a collection of donation transactions 
        /// </summary>
        /// <param name="dateTimeToBeProcessed">Date/time when transaction is being processed</param>
        /// <param name="paymentProcessStatus">Enum process status</param>
        /// <param name="paymentMethod">Enum of payment type (credit card, direct debit)</param>
        /// <returns>Collection of donation transaction details</returns>
        public IEnumerable<TransactionDetail> GetDonationTransactionsByStatus_ForProcessing(DateTime dateTimeToBeProcessed, PaymentGatewayProcessing.Helpers.Enums.PaymentProcessStatus processStatus, PaymentGatewayProcessing.Helpers.Enums.PaymentMethod PaymentMethod)
        {
            return _ITransactionDetailsDAL.GetDonationTransactionsByStatus_ForProcessing(dateTimeToBeProcessed, processStatus, PaymentMethod);
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
        /// Update a record for transaction details
        /// </summary>
        /// <param name="transactionDetails">Transaction details</param>
        /// <returns>Response with success, message, and profile token</returns>
        public ProcessorResponse UpdateTransactionRecord(TransactionDetail transactionDetails)
        {
            return _ITransactionDetailsDAL.UpdateTransactionRecord(transactionDetails);
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
        /// Update the Transaction List with the donation details
        /// </summary>
        /// <param name="donationTransList">Donation Transaction List</param>
        public void UpdateDonationTransactionList(IEnumerable<TransactionDetail> donationTransList)
        {
            _ITransactionDetailsDAL.UpdateDonationTransactionList(donationTransList);
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

        /// <summary>
        /// Get donation transaction details based on a transaction id
        /// </summary>
        /// <param name="donationTransactionId">Transaction id to search</param>
        /// <returns>Donation transaction details</returns>
        public List<DonationTransactionWithRelatedData> GetDonationTransaction_WithRelatedData(List<Guid> donationTransactionIds)
        {
            return _ITransactionDetailsDAL.GetDonationTransaction_WithRelatedData(donationTransactionIds);
        }

        
    }
}
