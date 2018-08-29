// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPaymentToOrganisationBS.cs" company="Day3 Solutions">
//   Copyright (c) Day3 Solutions. All rights reserved.
// </copyright>
// <summary>
//   Interface for the Payment To Organisation BS
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace GenerousAPI.BusinessServices
{
    using BusinessEntities;
    using DataAccessLayer;
    using System;
    using System.Collections.Generic;
    using static PaymentGatewayProcessing.Helpers.Enums;

    /*
* Donations to org will be processed in batches.
* A record needs to be created for every organisation project's donation in ABA file since each project can have a different bank acc
* Hence we have concept of batch line item - a batch line item = organisation project's donation record
*/

    public interface IPaymentToOrganisationBS
    {
        /// <summary>
        /// Processes a batch payment to an organisation
        /// </summary>
        /// <param name="batchStatus">Status of batch payment</param>
        /// <param name="processPaymentToDay3">True for day 3 payments</param>
        /// <param name="batchnumber">Batch number to look for - conditional</param>
        /// <returns>Batch list details</returns>
        List<PaymentToOrganisationBatch> BatchProcessPaymentToOrganisation(Common.BatchPaymentToOrganisationStatus batchStatus, bool processPaymentToDay3 = false, string batchnumber = "");

        void ProcessPaymentToOrganisationBatches_UnProcessed(List<PaymentToOrganisationBatch> batchList, List<PaymentToOrganisationBatchLineItem> batchLineItems, bool processDay3Payment);

        List<PaymentToOrganisationBatchLineItem> GetBatchListItems(PaymentProcessStatus batchStatus, List<PaymentToOrganisationBatch> batchList);

        /// <summary>
        /// Processes a batch payment to an organisation
        /// </summary>
        /// <param name="batchStatus">Status of batch payment</param>
        /// <param name="batchList">batch list details</param>
        /// <param name="processPaymentToDay3">True for day 3 payments</param>
        /// <param name="batchnumber">Batch number (if one)</param>
        //void ProcessBatchList(Common.BatchPaymentToOrganisationStatus batchStatus, List<PaymentToOrganisationBatch> batchList, bool processPaymentToDay3 = false, string batchnumber = "");
            
        /// <summary>
        /// Check if there are any dontaions that are not assigned to a batch
        /// </summary>
        /// <returns>True if donations are not assigned to a batch but are approved, false otherwise</returns>
        bool AreThereAny_APPROVED_DonationTransactions_NotAssigned_To_Any_Batch();

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
        /// Gets a collection of payment to organisation details 
        /// </summary>
        //List<PaymentToOrganisationListDTO> GetPaymentToOrganisationTransactions();

        /// <summary>
        /// Gets a collection of payment to organisation details 
        /// </summary>
        /// <param name="organisationId">Organisatsion to search</param>
        /// <returns>Collection of payment to organisation details</returns>
        //List<PaymentToOrganisationListDTO> GetPaymentToOrganisationTransactions(int organisationId);

        /// <summary>
        /// Gets a collection of payment to organisation details based on the bank account
        /// </summary>
        /// <param name="bankAccountId">Bank account id</param>
        /// <returns>Collection of payment to organisation details</returns>
        //List<PaymentToOrganisationListDTO> GetPaymentToOrganisationTransactionsForBankAccount(Guid bankAccountId);


        /// <summary>
        /// Gets a collection of payment to organisation details based on the bank account
        /// </summary>
        /// <param name="bankAccountId">Bank account id</param>
        /// <param name="organisationId">Organisatsion to search</param>
        /// <returns>Collection of payment to organisation details</returns>
        //List<PaymentToOrganisationListDTO> GetPaymentToOrganisationTransactionsForBankAccountByOrganisation(Guid bankAccountId, int organisationId);


        /// <summary>
        /// Creates a verification payment for the batch line items
        /// </summary>
        /// <param name="bankAccountId">Bank account id</param>
        /// <param name="verificationAmounts">Amounts to verify</param>
        //void CreateBankVerificationPaymenBatchLineItems(Guid bankAccountId, List<decimal> verificationAmounts);

        /// <summary>
        /// Creates a verification payment for the batch line items
        /// </summary>
        /// <param name="bankAccountId">Bank account id</param>
        /// <param name="verificationAmounts">Amounts to verify</param>
        //string CreatePaymentToHeartBurstBatchLineItems(Guid bankAccountId, List<decimal> verificationAmounts);

        /// <summary>
        /// Get a payment line item for an organisation
        /// </summary>
        /// <param name="batchLineItemId">Line Item ID</param>
        /// <returns>Payment line item for an organisation</returns>
        //PaymentToOrganisationBatchLineItem GetPaymentToOrganisationBatchLineItem(Guid batchLineItemId);

        /// <summary>
        /// Get a collection of payment line items for an organisation
        /// </summary>
        /// <param name="listOfIds">List of line item ids</param>
        /// <returns>Collection of payment line items</returns>
        //List<PaymentToOrganisationBatchLineItem> GetBatchLineItemsByIds(List<Guid> listOfIds);

        /// <summary>
        /// Get a collection of payment line items for an organisation
        /// </summary>
        /// <param name="listOfLineItemNumbers">List of line item numbers</param>
        /// <returns>Collection of payment line items<</returns>
        //List<PaymentToOrganisationBatchLineItem> GetBatchLineItemsByLineItemNumbers(List<long> listOfLineItemNumbers);

        /// <summary>
        /// Get a collection of payment line items for an organisation by bank account id
        /// </summary>
        /// <param name="bankAccountId">bank account id</param>
        /// <returns>Collection of payment line items</returns>
        //List<PaymentToOrganisationBatchLineItem> GetBankVerificationPaymenBatchLineItems(Guid bankAccountId);

        /// <summary>
        /// Set the status of payments to the organisation
        /// </summary>
        /// <param name="batchLineItemList">Batch line items</param>
        /// <param name="paymentProcessStatus">Payment status</param>
        /// <param name="statusUpdateDateTime">Date/Time status recorded</param>
        /// <param name="statusUpdateBy">Person status updated by</param>
        void SetPaymentToOrganisationBatchLineItemStatus(List<PaymentToOrganisationBatchLineItem> batchLineItemList, PaymentProcessStatus paymentProcessStatus, DateTime statusUpdateDateTime, string statusUpdateBy);
    }
}
