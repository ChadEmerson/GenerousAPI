using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GenerousAPI.BusinessEntities.Common;
using static PaymentGatewayProcessing.Helpers.Enums;

namespace GenerousAPI.DataAccessLayer
{
    public interface IPaymentToOrganisationDAL
    {
        /// <summary>
        /// Create a payment batch for an organisatsion
        /// </summary>
        /// <param name="batch">Payment batch details</param>
        void CreatePaymentToOrganisationBatch(PaymentToOrganisationBatch batch);

        /// <summary>
        /// Assigns the donation batch to approved donations
        /// </summary>
        /// <param name="batch">Batch donation details</param>
        void AssignBatchToApprovedDonations(PaymentToOrganisationBatch batch);

        /// <summary>
        /// Save new batch line items
        /// </summary>
        /// <param name="batchLineItemList">Collection of line items to save</param>
        void CreatePaymentToOrganisationBatchLineItems(List<PaymentToOrganisationBatchLineItem> batchLineItemList);

        /// <summary>
        /// Get a collection of of transactions associated with the batch id
        /// </summary>
        /// <param name="batch">Batch ID</param>
        /// <returns>Donations in the batch</returns>
        IEnumerable<TransactionDetail> GetDonationTransactionsAssignedToBatch(PaymentToOrganisationBatch batch);

        /// <summary>
        /// Get a collection of batch payments to the organisation by the status
        /// </summary>
        /// <param name="batchStatus">Status to search for</param>
        /// <param name="batchNumber">Batch number - conditional</param>
        /// <returns>Collection of batch payments</returns>
        IEnumerable<PaymentToOrganisationBatch> GetPaymentToOrganisationBatchByStatus(BatchPaymentToOrganisationStatus batchStatus, string batchNumber = "");

        /// <summary>
        /// Get a collection of donation batch line items for an organisation
        /// </summary>
        /// <param name="batch">Batch item details</param>
        /// <param name="paymentProcessStatus">Status to search for</param>
        /// <param name="dateTimeToBeProcessed">Date time to process donation</param>
        /// <returns>Collection of donation batch line items</returns>
        IEnumerable<PaymentToOrganisationBatchLineItem> GetPaymentToOrganisationBatchLineItemsByStatus_ForProcessing(PaymentToOrganisationBatch batch, PaymentProcessStatus paymentProcessStatus, DateTime dateTimeToBeProcessed);

        /// <summary>
        /// Updates the the batch line item list
        /// </summary>
        /// <param name="batchLineItemList">Line items</param>
        void UpdatePaymentToOrganisationBatchLineItemList(List<PaymentToOrganisationBatchLineItem> batchLineItemList);

        /// <summary>
        /// Update batch payment details
        /// </summary>
        /// <param name="batch">Batch details</param>
        /// <param name="updatedOn">Date/time batch updated</param>
        /// <param name="updatedBy">Who pdated batch</param>
        void UpdatePaymentToOrganisationBatchProcessStatus(PaymentToOrganisationBatch batch, DateTime updatedOn, String updatedBy);

        /// <summary>
        /// Get collection of batch items to process
        /// </summary>
        /// <param name="batch">Batch items</param>
        /// <param name="dateTimeToBeProcessed">Process date time</param>
        /// <returns>Collection of batch items to process</returns>
        IEnumerable<PaymentToOrganisationBatchLineItem> GetPaymentToOrganisationBatchLineItems_ForProcessing(PaymentToOrganisationBatch batch, DateTime dateTimeToBeProcessed);

        /// <summary>
        /// Check if there are any dontaions that are not assigned to a batch
        /// </summary>
        /// <returns>True if donations are not assigned to a batch but are approved, false otherwise</returns>
        bool AreThereAny_APPROVED_DonationTransactions_NotAssigned_To_Any_Batch();

        bool AreThereAny_Organisation_Fee_To_Process();

        /// <summary>
        /// Gets a collection of payment to organisation details 
        /// </summary>
        /// <param name="organisationId">Organisatsion to search</param>
        /// <returns>Collection of payment to organisation details</returns>
        List<PaymentToOrganisationListDTO> GetPaymentToOrganisationTransactions(int organisationId);

        /// <summary>
        /// Get a payment line item for an organisation
        /// </summary>
        /// <param name="batchLineItemId">Line Item ID</param>
        /// <returns>Payment line item for an organisation</returns>
        PaymentToOrganisationBatchLineItem GetPaymentToOrganisationBatchLineItem(Guid batchLineItemId);

        /// <summary>
        /// Get a collection of payment line items for an organisation
        /// </summary>
        /// <param name="listOfIds">List of line item ids</param>
        /// <returns>Collection of payment line items</returns>
        List<PaymentToOrganisationBatchLineItem> GetPaymentToOrganisationBatchLineItemsByIds(List<Guid> listOfIds);

        /// <summary>
        /// Get a collection of payment line items for an organisation
        /// </summary>
        /// <param name="listOfLineItemNumbers">List of line item numbers</param>
        /// <returns>Collection of payment line items<</returns>
        List<PaymentToOrganisationBatchLineItem> GetPaymentToOrganisationBatchLineItemsByLineItemNumbers(List<long> listOfLineItemNumbers);

        /// <summary>
        /// Get a collection of payment line items for an organisation by bank account id
        /// </summary>
        /// <param name="bankAccountId">bank account id</param>
        /// <returns>Collection of payment line items</returns>
        List<PaymentToOrganisationBatchLineItem> GetBankVerificationPaymenBatchLineItems(Guid bankAccountId);

        /// <summary>
        /// Gets a collection of payment to organisation details based on the bank account
        /// </summary>
        /// <param name="bankAccountId">Bank account id</param>
        /// <returns>Collection of payment to organisation details</returns>
        List<PaymentToOrganisationListDTO> GetPaymentToOrganisationTransactionsForBankAccount(Guid bankAccountId);

        /// <summary>
        /// Gets a collection of payment to organisation details 
        /// </summary>
        List<PaymentToOrganisationListDTO> GetPaymentToOrganisationTransactions();

        /// <summary>
        /// Assigns batch line items to approved Donations 
        /// </summary>
        void PlayBackPaymentToOrganisationBatchTransactionLog();

        /// <summary>
        /// Record association between batch line item and it's transactions as a running log that will be played back later 
        /// to batch update transaction records as a single update statement in stored procedure with log being trimmed post 
        /// play back this is more efficient than updating required transaction records here to store the batch line item id 
        /// against transaction records as we cannot batch it and it will have to be done one by one for each transaction record
        /// </summary>
        /// <param name="batchLineItemId">Line item id</param>
        /// <param name="bankAccountDonations">List of donations</param>
        void RecordPaymentToOrganisationBatchTransactionLog(Guid batchLineItemId, List<TransactionDetail> bankAccountDonations);

        /// <summary>
        /// Gets a collection of payment to organisation details based on the bank account
        /// </summary>
        /// <param name="bankAccountId">Bank account id</param>
        /// <param name="organisationId"></param>
        /// <returns>Collection of payment to organisation details</returns>
        List<PaymentToOrganisationListDTO> GetPaymentToOrganisationTransactionsForBankAccountByOrganisation(Guid bankAccountId, int organisationId);
    }
}
