using GenerousAPI.BusinessEntities;
using GenerousAPI.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using static GenerousAPI.BusinessEntities.Common;
using static PaymentGatewayProcessing.Helpers.Enums;

namespace GenerousAPI.BusinessServices
{
    public class PaymentToOrganisationBS : IPaymentToOrganisationBS
    {        
        /// <summary>
        /// Reference to the PaymentToOrganisationDAL Interface
        /// </summary>
        private IPaymentToOrganisationDAL _paymentToOrganisationDAL = null;
        private IPaymentProfileDAL _IPaymentProfileDAL = null;
        private IOrganisationFeeProcessingDAL _organisationFeeProcessing = null;
        private ITransactionDetailsDAL _ITransactionDetailsDAL = null;

        //        /// <summary>
        //        /// Reference to the DonationTransactionDAL Interface
        //        /// </summary>
        //        private ITransactionDetailDAL _donationTransactionDAL = null;

        /// <summary>
        /// Ctor
        /// </summary>
        public PaymentToOrganisationBS()
        {
            _paymentToOrganisationDAL = new PaymentToOrganisationDAL();
            //_donationTransactionDAL = new DonationTransactionDAL();
            _organisationFeeProcessing = new OrganisationFeeProcessingDAL();
            _ITransactionDetailsDAL = new TransactionDetailsDAL();
            _IPaymentProfileDAL = new PaymentProfileDAL();
        }

        /// <summary>
        /// Name of the batch process that runs for processing transactions
        /// </summary>
        public static String TransactionProcessBatchName
        {
            get
            {
                return "TX_PROCESS_BATCH";
            }
        }

        /// <summary>
        /// Check if there are any dontaions that are not assigned to a batch
        /// </summary>
        /// <returns>True if donations are not assigned to a batch but are approved, false otherwise</returns>
        public bool AreThereAny_APPROVED_DonationTransactions_NotAssigned_To_Any_Batch()
        {
            return _paymentToOrganisationDAL.AreThereAny_APPROVED_DonationTransactions_NotAssigned_To_Any_Batch();
        }

        /// <summary>
        /// Create a payment batch for an organisatsion
        /// </summary>
        /// <param name="batch">Payment batch details</param>
        public void CreatePaymentToOrganisationBatch(PaymentToOrganisationBatch batch)
        {
            _paymentToOrganisationDAL.CreatePaymentToOrganisationBatch(batch);

            //assign new batch to approved donations
            AssignBatchToApprovedDonations(batch);

            //create batch line items and calculate fees            
            CreatePaymentToOrganisationBatchLineItems(batch);
        }


        /// <summary>
        /// Assigns the donation batch to approved donations
        /// </summary>
        /// <param name="batch">Batch donation details</param>
        public void AssignBatchToApprovedDonations(PaymentToOrganisationBatch batch)
        {
            _paymentToOrganisationDAL.AssignBatchToApprovedDonations(batch);
        }

        //        /// <summary>
        //        /// Create payment to Day3 from organisations batch
        //        /// </summary>
        //        /// <returns></returns>
        //        public PaymentToOrganisationBatch CreatePaymentToDay3FromOrganisation()
        //        {
        //            PaymentToOrganisationBatch batch = new PaymentToOrganisationBatch();
        //            batch.Id = Guid.NewGuid();
        //            batch.BatchNumber = Helper.GetBatchNumberForPaymentToOrganisation();
        //            batch.CreateDateTime = DateTime.Now;
        //            batch.CreatedBy = Common.Config.TransactionProcessBatchName;
        //            batch.isPaymentToDay3 = 1;

        //            bool areThereAny_Organisation_Fee_To_Process = _paymentToOrganisationDAL.AreThereAny_Organisation_Fee_To_Process();

        //            //if there are no APPROVED donation transactions to be batched, then we simply abort the batch creation process
        //            if (!areThereAny_Organisation_Fee_To_Process)
        //                return null;

        //            //create batch record in db
        //            _paymentToOrganisationDAL.CreatePaymentToOrganisationBatch(batch);

        //            //assign new batch to approved donations
        //            AssignBatchToApprovedDonations(batch);

        //            //Create payments to Day3 from organisation
        //            CreatePaymentToDay3BatchLineItems(batch);

        //            return batch;
        //        }
                
        public List<PaymentToOrganisationBatchLineItem> GetBatchListItems(PaymentProcessStatus batchStatus, List<PaymentToOrganisationBatch> batchList)
        {
            List<PaymentToOrganisationBatchLineItem> batchLineItems = new List<PaymentToOrganisationBatchLineItem>();

            foreach (PaymentToOrganisationBatch batch in batchList)
            {
                //get list of unprocessed batch line items
                List<PaymentToOrganisationBatchLineItem> items = GetPaymentToOrganisationBatchLineItemsByStatus_ForProcessing(batch, batchStatus, DateTime.Now);
                batchLineItems.AddRange(items);
            }

            return batchLineItems;
        }

        /// <summary>
        /// Processes a batch payment to an organisation
        /// </summary>
        /// <param name="batchStatus">Status of batch payment</param>
        /// <param name="processPaymentToDay3">True for day 3 payments</param>
        /// <param name="batchnumber">Batch number to look for - conditional</param>
        /// <returns>Batch list details</returns>
        public List<PaymentToOrganisationBatch> BatchProcessPaymentToOrganisation(BatchPaymentToOrganisationStatus batchStatus, bool processPaymentToDay3 = false, string batchnumber = "")
        {
            List<PaymentToOrganisationBatch> batchList = null;

            List<DataAccessLayer.PaymentToOrganisationBatchLineItem> batchLineItems = new List<DataAccessLayer.PaymentToOrganisationBatchLineItem>();
            
            if (batchStatus == BatchPaymentToOrganisationStatus.Unprocessed && !processPaymentToDay3)
            {
                batchList = GetPaymentToOrganisationBatchByStatus(BatchPaymentToOrganisationStatus.Unprocessed) as List<PaymentToOrganisationBatch>;
                batchList = batchList.Where(x => x.isPaymentToDay3 == null || x.isPaymentToDay3 != (int)PaymentType.Day3Payment).ToList();
            }
            else if (batchStatus == BatchPaymentToOrganisationStatus.AwaitingCompletion && !processPaymentToDay3)
            {
                batchList = GetPaymentToOrganisationBatchByStatus(BatchPaymentToOrganisationStatus.AwaitingCompletion) as List<PaymentToOrganisationBatch>;
                batchList = batchList.Where(x => x.isPaymentToDay3 == null || x.isPaymentToDay3 != (int)PaymentType.Day3Payment).ToList();
            }
            else if (batchStatus == BatchPaymentToOrganisationStatus.Unprocessed && processPaymentToDay3)
            {
                batchList = GetPaymentToOrganisationBatchByStatus(BatchPaymentToOrganisationStatus.Unprocessed) as List<PaymentToOrganisationBatch>;
                batchList = batchList.Where(x => x.isPaymentToDay3 == (int)PaymentType.Day3Payment).ToList();
            }
            else if (batchStatus == BatchPaymentToOrganisationStatus.PaymentToHeartBurst)
            {
                batchList = GetPaymentToOrganisationBatchByStatus(BatchPaymentToOrganisationStatus.PaymentToHeartBurst, batchnumber) as List<PaymentToOrganisationBatch>;
            }


            return batchList;
        }

        /// <summary>
        /// Process payment to organisation batch items that are unprocessed
        /// </summary>
        /// <param name="batchList">Collection of items to process</param>
        /// <param name="processDay3Payment">Process payments to Day3</param>
        public void ProcessPaymentToOrganisationBatches_UnProcessed(List<PaymentToOrganisationBatch> batchList, List<PaymentToOrganisationBatchLineItem> batchLineItems, bool processDay3Payment)
        {
            //process batch line items
            if (!processDay3Payment)
                ProcessPaymentToOrganisationBatchLineItems_UnProcessed(batchLineItems);
            else
                ProcessDirectDebitForDay3_UnProcessed(batchLineItems);

            //set batch process status based on batch line items process status
            foreach (PaymentToOrganisationBatch batch in batchList)
            {
                _paymentToOrganisationDAL.UpdatePaymentToOrganisationBatchProcessStatus(batch, DateTime.Now, TransactionProcessBatchName);
            }
        }

        //        /// <summary>
        //        /// Gets a collection of payment to organisation details 
        //        /// </summary>
        //        /// <param name="organisationId">Organisatsion to search</param>
        //        /// <param name="searchCriteria">Any search filters</param>
        //        /// <param name="dataModifiers">Modifiers for sorting</param>
        //        /// <returns>Collection of payment to organisation details</returns>
        //        public List<PaymentToOrganisationListDTO> GetPaymentToOrganisationTransactions(int organisationId, SearchRequest searchCriteria, DataModifiers dataModifiers)
        //        {
        //            return _paymentToOrganisationDAL.GetPaymentToOrganisationTransactions(organisationId, searchCriteria, dataModifiers);
        //        }

        //        /// <summary>
        //        /// Gets a collection of payment to organisation details based on the bank account
        //        /// </summary>
        //        /// <param name="bankAccountId">Bank account id</param>
        //        /// <param name="searchCriteria">Any search filters</param>
        //        /// <param name="dataModifiers">Modifiers for sorting</param>
        //        /// <returns>Collection of payment to organisation details</returns>
        //        public List<PaymentToOrganisationListDTO> GetPaymentToOrganisationTransactionsForBankAccount(Guid bankAccountId, SearchRequest searchCriteria, DataModifiers dataModifiers)
        //        {
        //            return _paymentToOrganisationDAL.GetPaymentToOrganisationTransactionsForBankAccount(bankAccountId, searchCriteria, dataModifiers);
        //        }

        //        /// <summary>
        //        /// Gets a collection of payment to organisation details based on the bank account
        //        /// </summary>
        //        /// <param name="bankAccountId">Bank account id</param>
        //        /// <param name="searchCriteria">Any search filters</param>
        //        /// <param name="dataModifiers">Modifiers for sorting</param>
        //        /// <returns>Collection of payment to organisation details</returns>
        //        public List<PaymentToOrganisationListDTO> GetPaymentToOrganisationTransactionsForBankAccountByOrganisation(Guid bankAccountId, int organisationId, SearchRequest searchCriteria, DataModifiers dataModifiers)
        //        {
        //            return _paymentToOrganisationDAL.GetPaymentToOrganisationTransactionsForBankAccountByOrganisation(bankAccountId, organisationId, searchCriteria, dataModifiers);
        //        }

        //        /// <summary>
        //        /// Gets a collection of payment to organisation details 
        //        /// </summary>
        //        public List<PaymentToOrganisationListDTO> GetPaymentToOrganisationTransactions(SearchRequest searchCriteria, DataModifiers dataModifiers)
        //        {
        //            return _paymentToOrganisationDAL.GetPaymentToOrganisationTransactions(searchCriteria, dataModifiers);
        //        }

        //        /// <summary>
        //        /// Creates a verification payment for the batch line items
        //        /// </summary>
        //        /// <param name="bankAccountId">Bank account id</param>
        //        /// <param name="verificationAmounts">Amounts to verify</param>
        //        public string CreatePaymentToHeartBurstBatchLineItems(Guid bankAccountId, List<decimal> verificationAmounts)
        //        {
        //            IBankAccountBS bankAccountBS = new BankAccountBS(null, null);

        //            BankAccount bankAccount = bankAccountBS.GetBankAccount(bankAccountId);

        //            List<PaymentToOrganisationBatchLineItem> batchLineItemList = new List<PaymentToOrganisationBatchLineItem>();

        //            //create a new batch to process the bank verification request
        //            PaymentToOrganisationBatch batch = new PaymentToOrganisationBatch();
        //            batch.Id = Guid.NewGuid();
        //            batch.BatchNumber = Helper.GetBatchNumberForBankVerification();
        //            batch.IsBankVerificationBatch = true;
        //            batch.CreateDateTime = DateTime.Now;
        //            batch.CreatedBy = Common.Config.TransactionProcessBatchName;


        //            foreach (Decimal verificationAmount in verificationAmounts)
        //            {
        //                PaymentToOrganisationBatchLineItem donationItem = new PaymentToOrganisationBatchLineItem();
        //                donationItem.ProcessStatusId = (byte)Common.PaymentProcessStatus.Unprocessed;
        //                donationItem.Id = Guid.NewGuid();
        //                donationItem.BatchId = batch.Id;
        //                donationItem.BatchNumber = batch.BatchNumber;
        //                donationItem.IsBankVerification = true;
        //                donationItem.OrganisationId = bankAccount.OrganisationId;
        //                donationItem.AccountName = bankAccount.AccountName;
        //                donationItem.AccountNumber = bankAccount.AccountNumber;
        //                donationItem.BankAccountId = bankAccount.Id;
        //                donationItem.BankName = bankAccount.BankName;
        //                donationItem.BSBNumber = bankAccount.BSBNumber;
        //                donationItem.TotalAmountPaidToOrganisation = verificationAmount;
        //                donationItem.TotalAmountReceived = 0;
        //                donationItem.TotalPaymentsReceived = 0;
        //                donationItem.ProcessDateTime = DateTime.Now;
        //                donationItem.CreateDateTime = DateTime.Now;
        //                donationItem.CreatedBy = Common.Config.TransactionProcessBatchName;

        //                batchLineItemList.Add(donationItem);
        //            }

        //            //assign batch line items to batch
        //            batch.PaymentToOrganisationBatchLineItems = batchLineItemList;

        //            //save newly created batch in database
        //            _paymentToOrganisationDAL.CreatePaymentToOrganisationBatch(batch);

        //            return batch.BatchNumber;
        //        }

        //        /// <summary>
        //        /// Creates a verification payment for the batch line items
        //        /// </summary>
        //        /// <param name="bankAccountId">Bank account id</param>
        //        /// <param name="verificationAmounts">Amounts to verify</param>
        //        public void CreateBankVerificationPaymenBatchLineItems(Guid bankAccountId, List<decimal> verificationAmounts)
        //        {
        //            IBankAccountBS bankAccountBS = new BankAccountBS(null, null);

        //            BankAccount bankAccount = bankAccountBS.GetBankAccount(bankAccountId);

        //            List<PaymentToOrganisationBatchLineItem> batchLineItemList = new List<PaymentToOrganisationBatchLineItem>();

        //            //create a new batch to process the bank verification request
        //            PaymentToOrganisationBatch batch = new PaymentToOrganisationBatch();
        //            batch.Id = Guid.NewGuid();
        //            batch.BatchNumber = Helper.GetBatchNumberForBankVerification();
        //            batch.IsBankVerificationBatch = true;
        //            batch.CreateDateTime = DateTime.Now;
        //            batch.CreatedBy = Common.Config.TransactionProcessBatchName;


        //            foreach (Decimal verificationAmount in verificationAmounts)
        //            {
        //                PaymentToOrganisationBatchLineItem donationItem = new PaymentToOrganisationBatchLineItem();
        //                donationItem.ProcessStatusId = (byte)Common.PaymentProcessStatus.Unprocessed;
        //                donationItem.Id = Guid.NewGuid();
        //                donationItem.BatchId = batch.Id;
        //                donationItem.BatchNumber = batch.BatchNumber;
        //                donationItem.IsBankVerification = true;
        //                donationItem.OrganisationId = bankAccount.OrganisationId;
        //                donationItem.AccountName = bankAccount.AccountName;
        //                donationItem.AccountNumber = bankAccount.AccountNumber;
        //                donationItem.BankAccountId = bankAccount.Id;
        //                donationItem.BankName = bankAccount.BankName;
        //                donationItem.BSBNumber = bankAccount.BSBNumber;
        //                donationItem.TotalAmountPaidToOrganisation = verificationAmount;
        //                donationItem.TotalAmountReceived = 0;
        //                donationItem.TotalPaymentsReceived = 0;
        //                donationItem.ProcessDateTime = DateTime.Now;
        //                donationItem.CreateDateTime = DateTime.Now;
        //                donationItem.CreatedBy = Common.Config.TransactionProcessBatchName;

        //                batchLineItemList.Add(donationItem);
        //            }

        //            //assign batch line items to batch
        //            batch.PaymentToOrganisationBatchLineItems = batchLineItemList;

        //            //save newly created batch in database
        //            _paymentToOrganisationDAL.CreatePaymentToOrganisationBatch(batch);
        //        }

        //        /// <summary>
        //        /// Get a payment line item for an organisation
        //        /// </summary>
        //        /// <param name="batchLineItemId">Line Item ID</param>
        //        /// <returns>Payment line item for an organisation</returns>
        //        public PaymentToOrganisationBatchLineItem GetPaymentToOrganisationBatchLineItem(Guid batchLineItemId)
        //        {
        //            return _paymentToOrganisationDAL.GetPaymentToOrganisationBatchLineItem(batchLineItemId);
        //        }

        //        /// <summary>
        //        /// Get a collection of payment line items for an organisation
        //        /// </summary>
        //        /// <param name="listOfIds">List of line item ids</param>
        //        /// <returns>Collection of payment line items</returns>
        //        public List<PaymentToOrganisationBatchLineItem> GetBatchLineItemsByIds(List<Guid> listOfIds)
        //        {
        //            return _paymentToOrganisationDAL.GetPaymentToOrganisationBatchLineItemsByIds(listOfIds);
        //        }

        //        /// <summary>
        //        /// Get a collection of payment line items for an organisation
        //        /// </summary>
        //        /// <param name="listOfLineItemNumbers">List of line item numbers</param>
        //        /// <returns>Collection of payment line items<</returns>
        //        public List<PaymentToOrganisationBatchLineItem> GetBatchLineItemsByLineItemNumbers(List<long> listOfLineItemNumbers)
        //        {
        //            return _paymentToOrganisationDAL.GetPaymentToOrganisationBatchLineItemsByLineItemNumbers(listOfLineItemNumbers);
        //        }

        //        /// <summary>
        //        /// Log failed donations
        //        /// </summary>
        //        /// <param name="failedPaymentToOrganisationList">List of failed donations</param>
        //        /// <param name="processedBy">Name of person donations processed by</param>
        //        public void LogFailedDonations(List<FailedTransaction> failedPaymentToOrganisationList, string processedBy)
        //        {
        //            List<FailedTransactionLog> logEntries = new List<FailedTransactionLog>();

        //            foreach (FailedTransaction failedDonation in failedPaymentToOrganisationList)
        //            {
        //                FailedTransactionLog logEntry = new FailedTransactionLog();
        //                logEntry.Id = Guid.NewGuid();
        //                logEntry.TransactionId = failedDonation.TransactionId;
        //                logEntry.TransactionTypeId = (byte)Common.TransactionType.PaymentToOrg;
        //                logEntry.ResponseMessage = failedDonation.Response;
        //                logEntry.ResponseCode = failedDonation.ReasonCode;
        //                logEntry.CreateDateTime = DateTime.Now;
        //                logEntry.CreatedBy = processedBy;
        //                logEntry.LastNotifiedDateTime = failedDonation.LastNotifiedDateTime;
        //                logEntry.NotificationEmail = failedDonation.NotificationEmail;

        //                logEntries.Add(logEntry);
        //            }

        //            if (logEntries.Count > 0)
        //                _donationTransactionDAL.LogFailedDonations(logEntries);
        //        }

        //        /// <summary>
        //        /// Get a collection of payment line items for an organisation by bank account id
        //        /// </summary>
        //        /// <param name="bankAccountId">bank account id</param>
        //        /// <returns>Collection of payment line items</returns>
        //        public List<PaymentToOrganisationBatchLineItem> GetBankVerificationPaymenBatchLineItems(Guid bankAccountId)
        //        {
        //            return _paymentToOrganisationDAL.GetBankVerificationPaymenBatchLineItems(bankAccountId);
        //        }

        //        #endregion

        //        #region private methods

        //        /// <summary>
        //        /// Send the direct credit file to the configured email
        //        /// </summary>
        //        /// <param name="generatedDirectCreditFileName">Direct credit file contents</param>
        //        /// <param name="batchLineItems">Collection of batch line items</param>
        private void SendDirectCreditFileForProcessing(string generatedDirectCreditFileName, List<PaymentToOrganisationBatchLineItem> batchLineItems)
        {
            //send email to concerned staff
            MailMessage message = new MailMessage();
            message.To.Add(ABAGeneration.AbaConfig.ABA_ProcessingStaffEmailList);

            //fetch generated direct debit file
            FileInfo file = new FileInfo(Path.Combine(ABAGeneration.AbaConfig.ABAPaymentCollectionFilePath, generatedDirectCreditFileName));
            if (file != null)
            {
                decimal totalAmount = batchLineItems.Sum(x => x.TotalAmountPaidToOrganisation);

                message.Subject = String.Format("Please process the direct CREDIT file {0} for donation to organisations", generatedDirectCreditFileName);
                message.Body = String.Format("Total amount paid to organisation: {0}", totalAmount);
                message.Attachments.Add(new Attachment(file.FullName));
            }
            else
            {
                message.Subject = String.Format("The Direct Credit file {0} was not found", generatedDirectCreditFileName);
                message.Body = String.Format("The Direct Credit file {0} was not found and hence could not be emailed. Please alert the system administration regarding this.", generatedDirectCreditFileName);
            }

            Mail.SendMail(message);
        }

        /// <summary>
        /// Get a collection of batch payments to the organisation by the status
        /// </summary>
        /// <param name="batchStatus">Status of batch payment</param>
        /// <returns>Collection of batch payments</returns>
        private List<PaymentToOrganisationBatch> GetPaymentToOrganisationBatchByStatus(BatchPaymentToOrganisationStatus batchStatus)
        {
            return _paymentToOrganisationDAL.GetPaymentToOrganisationBatchByStatus(batchStatus) as List<PaymentToOrganisationBatch>;
        }

        /// <summary>
        /// Get a collection of batch payments to the organisation by the status
        /// </summary>
        /// <param name="batchStatus">Status of batch payment</param>
        /// <param name="batchnumber">Batch number to search for</param>
        /// <returns>Collection of batch payments</returns>
        private List<PaymentToOrganisationBatch> GetPaymentToOrganisationBatchByStatus(BatchPaymentToOrganisationStatus batchStatus, string batchnumber)
        {
            return _paymentToOrganisationDAL.GetPaymentToOrganisationBatchByStatus(batchStatus, batchnumber) as List<PaymentToOrganisationBatch>;
        }

        /// <summary>
        /// Get a collection of donation batch line items for an organisation
        /// </summary>
        /// <param name="batch">Batch item details</param>
        /// <param name="paymentProcessStatus">Status to search for</param>
        /// <param name="dateTimeToBeProcessed">Date time to process donation</param>
        /// <returns>Collection of donation batch line items</returns>
        private List<PaymentToOrganisationBatchLineItem> GetPaymentToOrganisationBatchLineItemsByStatus_ForProcessing(PaymentToOrganisationBatch batch, PaymentProcessStatus paymentProcessStatus, DateTime dateTimeToBeProcessed)
        {
            List<PaymentToOrganisationBatchLineItem> items = _paymentToOrganisationDAL.GetPaymentToOrganisationBatchLineItemsByStatus_ForProcessing(batch, paymentProcessStatus, dateTimeToBeProcessed) as List<PaymentToOrganisationBatchLineItem>;
            return items;
        }       

        /// <summary>
        /// Process payment to organisation batch items that are awaiting completion
        /// </summary>
        /// <param name="batchList">Collection of items to process</param>
        private void ProcessPaymentToOrganisationBatches_AwaitingBatchCompletion(List<PaymentToOrganisationBatch> batchList)
        {
            List<PaymentToOrganisationBatchLineItem> batchLineItemAwaitingClearance = new List<PaymentToOrganisationBatchLineItem>();
            List<PaymentToOrganisationBatchLineItem> batchLineItemDeclined = new List<PaymentToOrganisationBatchLineItem>();
            List<PaymentToOrganisationBatchLineItem> batchLineItemToBeReProcessed = new List<PaymentToOrganisationBatchLineItem>();

            foreach (PaymentToOrganisationBatch batch in batchList)
            {

                //get list of all batch line items of the batch
                List<PaymentToOrganisationBatchLineItem> batchLineItems = _paymentToOrganisationDAL.GetPaymentToOrganisationBatchLineItems_ForProcessing(batch, DateTime.Now) as List<PaymentToOrganisationBatchLineItem>;

                foreach (PaymentToOrganisationBatchLineItem lineItem in batchLineItems)
                {
                    if (lineItem.ProcessStatusId == (byte)PaymentProcessStatus.AwaitingClearance)
                        batchLineItemAwaitingClearance.Add(lineItem);
                    else if (lineItem.ProcessStatusId == (byte)PaymentProcessStatus.Declined)
                        batchLineItemDeclined.Add(lineItem);
                    else if (lineItem.ProcessStatusId == (byte)PaymentProcessStatus.ReProcess)
                        batchLineItemToBeReProcessed.Add(lineItem);
                }

            }

            //process batch line items 
            if (batchLineItemAwaitingClearance.Count() > 0)
                ProcessPaymentToOrganisationBatchLineItems_AwaitingClearance(batchLineItemAwaitingClearance);

            if (batchLineItemDeclined.Count() > 0)
                ProcessPaymentToOrganisationBatchLineItems_Failed(batchLineItemDeclined);

            if (batchLineItemToBeReProcessed.Count() > 0)
                ProcessPaymentToOrganisationBatchLineItems_UnProcessed(batchLineItemToBeReProcessed);

            //set batch process status based on batch line items process status
            foreach (PaymentToOrganisationBatch batch in batchList)
            {
                _paymentToOrganisationDAL.UpdatePaymentToOrganisationBatchProcessStatus(batch, DateTime.Now, TransactionProcessBatchName);
            }

        }

        /// <summary>
        /// Process payment to organisation batch items that are unprocessed
        /// </summary>
        /// <param name="batchLineItems">Collection of batch line items to process</param>
        private void ProcessPaymentToOrganisationBatchLineItems_UnProcessed(List<PaymentToOrganisationBatchLineItem> batchLineItems)
        {
            //Generate ABA file
            String generatedDirectCreditFileName = GenerateDirectCreditFile(batchLineItems);

            //mark the batch line items written to ABA file as awaiting clearance
            SetPaymentToOrganisationBatchLineItemStatus(batchLineItems, PaymentProcessStatus.AwaitingClearance, DateTime.Now, TransactionProcessBatchName);

            if (batchLineItems.Count != 0)
            {
                //send generated direct credit file for processing
                SendDirectCreditFileForProcessing(generatedDirectCreditFileName, batchLineItems);
            }
        }

        private void ProcessDirectDebitForDay3_UnProcessed(List<PaymentToOrganisationBatchLineItem> batchLineItems)
        {
            if (batchLineItems.Count == 0)
                return;

            String generatedDirectDebitFileName = GenerateDirectDebitFile(batchLineItems);

            //Update the processing fee record
            foreach (PaymentToOrganisationBatchLineItem batchItem in batchLineItems)
            {
                try
                {
                    OrganisationFeeProcessing OrgProcessingRecord = _organisationFeeProcessing.GetOrganisationsToProcessFeeRecordByOrganisationId(batchItem.OrganisationId.Value);
                    OrgProcessingRecord.LastRunDate = DateTime.Now;
                    int OrgBillDate = OrgProcessingRecord.OrganisationBillDate != null ? OrgProcessingRecord.OrganisationBillDate.Value.Day : 1;
                    DateTime nextMonthDate = DateTime.Now.AddMonths(1);
                    DateTime nextRunDate = CreateDate(nextMonthDate.Year, nextMonthDate.Month, OrgBillDate, nextMonthDate);
                    OrgProcessingRecord.NextRunDate = nextRunDate;
                    _organisationFeeProcessing.UpdateOrganisationFeeProces(OrgProcessingRecord);
                }
                catch (Exception)
                {
                    // failed to update - can ignore for now
                }
            }

            //send the aba file to concerned staff for bank processsing
            SendDirectDebitFileForProcessing(generatedDirectDebitFileName, batchLineItems);
        }

        public static DateTime CreateDate(int year, int month, int day, DateTime time)
        {
            return new DateTime(year, month, day, 00, 00, 0);
        }

        private string GenerateDirectDebitFile(List<PaymentToOrganisationBatchLineItem> organisationFeesDetails)
        {
            String ABAFileName = String.Format("{0}-{1}.txt", "DD", DateTime.Now.ToString("yyyyMMddHHmm"));
            ABAGeneration.NAB_ABAFileGenerator NAB_ABAFileGen = new ABAGeneration.NAB_ABAFileGenerator(ABAMode.DirectDebit);
            NAB_ABAFileGen.GenerateABAFile(organisationFeesDetails, ABAGeneration.AbaConfig.ABAPaymentCollectionFilePath, ABAFileName);
            //TODO: Exception handling if invalid data etc. 

            return ABAFileName;
        }

        private void SendDirectDebitFileForProcessing(string generatedDirectDebitFileName, List<PaymentToOrganisationBatchLineItem> organisationFeesDetailst)
        {
            //send email to concerned staff
            MailMessage message = new MailMessage();
            message.To.Add(ABAGeneration.AbaConfig.ABA_ProcessingStaffEmailList);

            //fetch generated direct debit file
            FileInfo file = new FileInfo(Path.Combine(ABAGeneration.AbaConfig.ABAPaymentCollectionFilePath, generatedDirectDebitFileName));
            if (file != null)
            {
                //decimal totalAmount = donationTransList.Sum(x => x.Amount);

                message.Subject = String.Format("Please process the direct debit file {0} for organisations", generatedDirectDebitFileName);
                //message.Body = String.Format("Total amount received: {0}", totalAmount);
                message.Attachments.Add(new Attachment(file.FullName));
            }
            else
            {
                message.Subject = String.Format("The Direct Debit file {0} was not found", generatedDirectDebitFileName);
                message.Body = String.Format("The Direct Debit file {0} was not found and hence could not be emailed. "
                    + "Please alert the system administration regarding this.", generatedDirectDebitFileName);
            }

            Mail.SendMail(message);
        }

        /// <summary>
        /// Process payment to organisation batch items that are awating clearance
        /// </summary>
        /// <param name="batchLineItems">Collection of batch line items to process</param>
        private void ProcessPaymentToOrganisationBatchLineItems_AwaitingClearance(List<PaymentToOrganisationBatchLineItem> batchLineItems)
        {
            List<PaymentToOrganisationBatchLineItem> batchLineItemsAssumedClearedList = new List<PaymentToOrganisationBatchLineItem>();

            foreach (PaymentToOrganisationBatchLineItem batchLineItem in batchLineItems)
            {
                DateTime lastDayOfClearanceWaitingPeriod = ((DateTime)batchLineItem.Processed_PaymentSubmittedDateTime).AddDays(ABAGeneration.AbaConfig.ABA_ClearancePeriodInDays);

                if (DateTime.Today > lastDayOfClearanceWaitingPeriod)
                    batchLineItemsAssumedClearedList.Add(batchLineItem);
            }

            //mark the batch line items awaiting clearance as approved if clearance period has passed
            if (batchLineItemsAssumedClearedList.Count() > 0)
                SetPaymentToOrganisationBatchLineItemStatus(batchLineItemsAssumedClearedList, PaymentProcessStatus.Approved, DateTime.Now, TransactionProcessBatchName);
        }

        /// <summary>
        /// Process payment to organisation batch items that are failed
        /// </summary>
        /// <param name="batchLineItems">Collection of batch line items to process</param>
        private void ProcessPaymentToOrganisationBatchLineItems_Failed(List<PaymentToOrganisationBatchLineItem> batchLineItems)
        {
            List<PaymentToOrganisationBatchLineItem> batchLineItemsToBeReProcessed = new List<PaymentToOrganisationBatchLineItem>();
            List<PaymentToOrganisationBatchLineItem> batchLineItemsToBeStoppedFromFurtherProcessing = new List<PaymentToOrganisationBatchLineItem>();

            foreach (PaymentToOrganisationBatchLineItem batchLineItem in batchLineItems)
            {
                if ((batchLineItem.ProcessRetryCounter ?? 0) < ABAGeneration.AbaConfig.MaxRetries)
                {
                    batchLineItemsToBeReProcessed.Add(batchLineItem);
                }
                else
                {
                    batchLineItemsToBeStoppedFromFurtherProcessing.Add(batchLineItem);
                }
            }
            //mark them reprocess status if max retries hasn't reached, otherwise mark them as 'stop processing' status
            if (batchLineItemsToBeReProcessed.Count() > 0)
                SetPaymentToOrganisationBatchLineItemStatus(batchLineItemsToBeReProcessed, PaymentProcessStatus.ReProcess, DateTime.Now, TransactionProcessBatchName);

            if (batchLineItemsToBeStoppedFromFurtherProcessing.Count() > 0)
                SetPaymentToOrganisationBatchLineItemStatus(batchLineItemsToBeStoppedFromFurtherProcessing, PaymentProcessStatus.DoNotProcess, DateTime.Now, TransactionProcessBatchName);
        }

        /// <summary>
        /// Set the status of payments to the organisation
        /// </summary>
        /// <param name="batchLineItemList">Batch line items</param>
        /// <param name="paymentProcessStatus">Payment status</param>
        /// <param name="statusUpdateDateTime">Date/Time status recorded</param>
        /// <param name="statusUpdateBy">Person status updated by</param>
        public void SetPaymentToOrganisationBatchLineItemStatus(List<PaymentToOrganisationBatchLineItem> batchLineItemList, PaymentProcessStatus paymentProcessStatus, DateTime statusUpdateDateTime, string statusUpdateBy)
        {
            foreach (PaymentToOrganisationBatchLineItem batchLineItem in batchLineItemList)
            {
                //update process status or set 'do not process' flag
                if (paymentProcessStatus == PaymentProcessStatus.DoNotProcess)
                {
                    batchLineItem.DoNotProcess = true;
                }
                else
                {
                    batchLineItem.DoNotProcess = false;
                    batchLineItem.ProcessStatusId = (byte)paymentProcessStatus;
                    //batchLineItem.HasProcessStatusChanged = true; //acknowledge that we are updating process status
                }

                batchLineItem.LastProcessedDateTime = statusUpdateDateTime;
                batchLineItem.LastModifiedBy = statusUpdateBy;
                batchLineItem.LastModifiedDateTime = statusUpdateDateTime;

                if (paymentProcessStatus == PaymentProcessStatus.Approved)
                {
                    batchLineItem.Processed_PaymentFinalisedDateTime = DateTime.Now;
                }
                //for DD when ABA file is generated the transaction status is set to awaiting clearance and we set donation submitted date time to current date time
                else if (paymentProcessStatus == PaymentProcessStatus.AwaitingClearance)
                {
                    batchLineItem.Processed_PaymentSubmittedDateTime = DateTime.Now;
                }
                else if (paymentProcessStatus == PaymentProcessStatus.ReProcess)
                {
                    batchLineItem.ProcessRetryCounter = Convert.ToByte((batchLineItem.ProcessRetryCounter ?? 0) + 1);
                    batchLineItem.ProcessDateTime = DateTime.Today.AddDays(ABAGeneration.AbaConfig.RetryPeriodInDays);
                }
                else if (paymentProcessStatus == PaymentProcessStatus.Declined)
                {
                    //do nothing
                }
                else if (paymentProcessStatus == PaymentProcessStatus.DoNotProcess)
                {
                    //not sure
                }

                //update bank account verification status
                if (batchLineItem.IsBankVerification && batchLineItem.BankAccountId.HasValue)
                {
                    IBankAccountBS bankAccountBS = new BankAccountBS();
                    var bankAccount = bankAccountBS.GetBankAccountById(batchLineItem.BankAccountId.Value);
                    if (bankAccount != null)
                    {
                        bankAccount.BankVerificationPaymentStatus = paymentProcessStatus.ToString();

                        bankAccountBS.UpdateBankAccount(bankAccount);
                    }

                }

                // Re-encrypt the bank data
                batchLineItem.BankAccountBSB = EncryptionService.Encrypt(batchLineItem.BankAccountBSB);
                batchLineItem.BankAccountNumber = EncryptionService.Encrypt(batchLineItem.BankAccountNumber);

            }

            if (batchLineItemList.Count() > 0)
                _paymentToOrganisationDAL.UpdatePaymentToOrganisationBatchLineItemList(batchLineItemList);

        }

        /// <summary>
        /// Generate a direct credit file based on line items
        /// </summary>
        /// <param name="batchLineItems">Collection of payment line items</param>
        /// <returns>Direct Credit file</returns>
        private string GenerateDirectCreditFile(List<PaymentToOrganisationBatchLineItem> batchLineItems)
        {
            String ABAFileName = String.Format("{0}-{1}.txt", "DC", DateTime.Now.ToString("yyyyMMddHHmm"));
            ABAGeneration.NAB_ABAFileGenerator NAB_ABAFileGen = new ABAGeneration.NAB_ABAFileGenerator(ABAMode.DirectCredit);
            NAB_ABAFileGen.GenerateABAFile(batchLineItems, ABAGeneration.AbaConfig.ABAPaymentCollectionFilePath, ABAFileName);   //TODO: Exception handling if invalid data etc. 
            return ABAFileName;
        }
        
        private void ProcessLineItemFee(DonationTransactionWithRelatedData donationTransaction, OrganisationFeeProcesingWithRelatedData organisationFeeProcessingSettings)
        {
            if (donationTransaction != null && organisationFeeProcessingSettings != null)
            {
                decimal totalAmount = donationTransaction.TransactionDetail.Amount;
                decimal calcAmount = 0;
                decimal ticketFee = 0;

                if (Convert.ToBoolean(organisationFeeProcessingSettings.OrganisationToProcess.IsActive) &&
                    Convert.ToBoolean(organisationFeeProcessingSettings.OrganisationToProcess.IsPromoBilling))
                {
                    // organisation has promo billing    
                    calcAmount = CalculateFeeAmount(totalAmount, true, donationTransaction, organisationFeeProcessingSettings);
                }
                else
                {
                    // organisation does not have promo billing
                    calcAmount = CalculateFeeAmount(totalAmount, false, donationTransaction, organisationFeeProcessingSettings);
                }

                calcAmount += organisationFeeProcessingSettings.OrganisationStandardFees.TransactionFeeAmount.Value;


                if (donationTransaction.TransactionDetail.NumberOfEventTickets > 0)
                {
                    // Calculate price of tickets
                    try
                    {
                        var ticketPrice = (donationTransaction.TransactionDetail.Amount / donationTransaction.TransactionDetail.NumberOfEventTickets);
                        
                        if (ticketPrice.Value >= organisationFeeProcessingSettings.OrganisationStandardFees.EventTicketBracket1 &&
                            ticketPrice.Value <= organisationFeeProcessingSettings.OrganisationStandardFees.EventTicketBracket2)
                        {
                            ticketFee = organisationFeeProcessingSettings.OrganisationStandardFees.EventTicketBracket1Fee.Value;
                        }
                        else if(ticketPrice.Value >= organisationFeeProcessingSettings.OrganisationStandardFees.EventTicketBracket2 &&
                            ticketPrice.Value <= organisationFeeProcessingSettings.OrganisationStandardFees.EventTicketBracket3)
                        {
                            ticketFee = organisationFeeProcessingSettings.OrganisationStandardFees.EventTicketBracket2Fee.Value;
                        }
                        else if (ticketPrice.Value >= organisationFeeProcessingSettings.OrganisationStandardFees.EventTicketBracket3 &&
                            ticketPrice.Value <= organisationFeeProcessingSettings.OrganisationStandardFees.EventTicketBracket4)
                        {
                            ticketFee = organisationFeeProcessingSettings.OrganisationStandardFees.EventTicketBracket3Fee.Value;
                        }
                        else if (ticketPrice.Value >= organisationFeeProcessingSettings.OrganisationStandardFees.EventTicketBracket4 &&
                            ticketPrice.Value <= organisationFeeProcessingSettings.OrganisationStandardFees.EventTicketBracket5)
                        {
                            ticketFee = organisationFeeProcessingSettings.OrganisationStandardFees.EventTicketBracket4Fee.Value;
                        }
                        else
                        {
                            ticketFee = organisationFeeProcessingSettings.OrganisationStandardFees.EventTicketBracket5Fee.Value;
                        }

                        ticketFee *= donationTransaction.TransactionDetail.NumberOfEventTickets.Value;

                        calcAmount += ticketFee;
                    }
                    catch (Exception)
                    {
                        // No tickets? 
                    }
                }

                //Update the amount paid to organisation after fee deductions
                donationTransaction.TransactionDetail.AmountAfterFeeDeductions = Decimal.Round(totalAmount - calcAmount, 2);
                donationTransaction.TransactionDetail.TransactionFeeAmount = organisationFeeProcessingSettings.OrganisationStandardFees.TransactionFeeAmount;
                donationTransaction.TransactionDetail.ProcessingFeeAmount = calcAmount;
                donationTransaction.TransactionDetail.TicketFeeAmount = ticketFee;

                //Save the updated donation transaction record
                _ITransactionDetailsDAL.UpdateTransactionRecord(donationTransaction.TransactionDetail);

                //Save the fee break down
                // _organisation.SaveDonationTransactionFeeBreakDown(donationTransaction.DonationTransaction.Id, platformFee, directDebitFee, paymentGatewayFee);
            }
        }

        private decimal CalculateFeeAmount(decimal totalAmount, bool PromoFees, DonationTransactionWithRelatedData donationTransaction, OrganisationFeeProcesingWithRelatedData organisationFeeProcessingSettings)
        {
            decimal feeAmount = 0;

            // Determine if Fee (for events) 

           
            // Determine for the transaction if it was direct debit or credit card
            if (donationTransaction.TransactionDetail.PaymentMethodId == (byte)BusinessEntities.TransactionMode.Credit)
            {
                // Determine for the transaction if it was credit card and what kind 
                var paymentProfileDetails = _IPaymentProfileDAL.GetPaymentProfile(donationTransaction.TransactionDetail.PaymentProfileTokenId);

                if (paymentProfileDetails.CardType.ToUpper() == GetEnumDescription(BusinessEntities.Common.CardType.Visa).ToUpper() ||
                    paymentProfileDetails.CardType.ToUpper() == GetEnumDescription(BusinessEntities.Common.CardType.MasterCard).ToUpper())
                {
                    if (PromoFees)
                    {
                        feeAmount = CalcPrecentageAmount(totalAmount, Convert.ToSingle(organisationFeeProcessingSettings.OrganisationPromoFees.VisaFee));

                        if (feeAmount < organisationFeeProcessingSettings.OrganisationPromoFees.VisaMinAmount.Value)
                        {
                            feeAmount = organisationFeeProcessingSettings.OrganisationPromoFees.VisaMinAmount.Value;
                        }
                    }
                    else
                    {
                        feeAmount = CalcPrecentageAmount(totalAmount, Convert.ToSingle(organisationFeeProcessingSettings.OrganisationStandardFees.VisaFee));

                        if (feeAmount < organisationFeeProcessingSettings.OrganisationStandardFees.VisaMinAmount.Value)
                        {
                            feeAmount = organisationFeeProcessingSettings.OrganisationStandardFees.VisaMinAmount.Value;
                        }
                    }
                }
                else if (paymentProfileDetails.CardType.ToUpper() == GetEnumDescription(BusinessEntities.Common.CardType.AMEX).ToUpper())
                {
                    if (PromoFees)
                    {
                        feeAmount = CalcPrecentageAmount(totalAmount, Convert.ToSingle(organisationFeeProcessingSettings.OrganisationPromoFees.AmexFee));

                        if (feeAmount < organisationFeeProcessingSettings.OrganisationPromoFees.AmexMinAmount.Value)
                        {
                            feeAmount = organisationFeeProcessingSettings.OrganisationPromoFees.AmexMinAmount.Value;
                        }
                    }
                    else
                    {
                        feeAmount = CalcPrecentageAmount(totalAmount, Convert.ToSingle(organisationFeeProcessingSettings.OrganisationStandardFees.AmexFee));

                        if (feeAmount < organisationFeeProcessingSettings.OrganisationStandardFees.AmexMinAmount.Value)
                        {
                            feeAmount = organisationFeeProcessingSettings.OrganisationStandardFees.AmexMinAmount.Value;
                        }
                    }
                    
                }
                else
                {
                    if (PromoFees)
                    {
                        feeAmount = CalcPrecentageAmount(totalAmount, Convert.ToSingle(organisationFeeProcessingSettings.OrganisationPromoFees.InternationalFee));

                        if (feeAmount < organisationFeeProcessingSettings.OrganisationPromoFees.InternationalMinAmount.Value)
                        {
                            feeAmount = organisationFeeProcessingSettings.OrganisationPromoFees.InternationalMinAmount.Value;
                        }
                    }
                    else
                    {
                        feeAmount = CalcPrecentageAmount(totalAmount, Convert.ToSingle(organisationFeeProcessingSettings.OrganisationStandardFees.InternationalFee));

                        if (feeAmount < organisationFeeProcessingSettings.OrganisationStandardFees.InternationalMinAmount.Value)
                        {
                            feeAmount = organisationFeeProcessingSettings.OrganisationStandardFees.InternationalMinAmount.Value;
                        }
                    } 
                }                
            }
            else if (donationTransaction.TransactionDetail.PaymentMethodId == (byte)BusinessEntities.TransactionMode.DirectDebit)
            {
                feeAmount = CalcPrecentageAmount(totalAmount, Convert.ToSingle(organisationFeeProcessingSettings.OrganisationStandardFees.DirectDebitFee));

                if (feeAmount < organisationFeeProcessingSettings.OrganisationStandardFees.DirectDebitMin.Value)
                {
                    feeAmount = organisationFeeProcessingSettings.OrganisationStandardFees.DirectDebitMin.Value;
                }
            }
            else if (donationTransaction.TransactionDetail.PaymentMethodId == (byte)BusinessEntities.TransactionMode.Refund)
            {
                feeAmount = organisationFeeProcessingSettings.OrganisationStandardFees.RefundFee.Value;
            }         
                          
            return feeAmount;
        }


        private decimal CalcPrecentageAmount(decimal currentTotal, float precentage)
        {
            return (currentTotal * (decimal)(precentage / 100));
        }

        private decimal DeductFixedAmount(decimal currentTotal, float amount)
        {
            return (decimal)amount;
        }
                
        /// <summary>
        /// Create a new batch item to process for an organisation
        /// </summary>
        /// <param name="batch">Batch detials</param>
        private void CreatePaymentToOrganisationBatchLineItems(PaymentToOrganisationBatch batch)
        {
            IBankAccountBS bankAccountBS = new BankAccountBS();

            Dictionary<Guid, List<TransactionDetail>> donationsTowardsBankAccount = new Dictionary<Guid, List<TransactionDetail>>();

            List<DonationTransactionWithRelatedData> transListOfBatch = _ITransactionDetailsDAL.GetDonationTransactionsOf_Batch_WithRelatedData(batch.Id) as List<DonationTransactionWithRelatedData>;

            List<PaymentToOrganisationBatchLineItem> batchLineItemList = new List<PaymentToOrganisationBatchLineItem>();

            //sorting the approved transactions of a batch by bank account
            foreach (DonationTransactionWithRelatedData trans in transListOfBatch)
            {                
                var OrgFeeProcessingSettings = _organisationFeeProcessing.GetOrganisationFeeProcesingWithRelatedData(trans.TransactionDetail.OrganisationId.Value);

                ProcessLineItemFee(trans, OrgFeeProcessingSettings);

                var bankAccount = bankAccountBS.GetBankAccount(trans.TransactionDetail.BankAccountTokenId);

                List<TransactionDetail> bankAccountDonations = donationsTowardsBankAccount[bankAccount.BankAccountId];

                PaymentToOrganisationBatchLineItem lineItem = new PaymentToOrganisationBatchLineItem();
                lineItem.ProcessStatusId = (byte)PaymentProcessStatus.Unprocessed;
                lineItem.Id = Guid.NewGuid();
                lineItem.BatchId = batch.Id;
                lineItem.BatchNumber = batch.BatchNumber;
                lineItem.OrganisationId = trans.TransactionDetail.OrganisationId;
                lineItem.BankAccountId = bankAccount.BankAccountId;
                lineItem.BankAccountBSB = bankAccount.BankAccountBSB;
                lineItem.BankAccountNumber = bankAccount.BankAccountNumber;
                lineItem.BankAcountName = bankAccount.BankAcountName;
                lineItem.TotalAmountReceived = bankAccountDonations.Sum(x => x.Amount);
                lineItem.TotalPaymentsReceived = bankAccountDonations.Count();
                lineItem.TotalAmountPaidToOrganisation = (decimal)bankAccountDonations.Sum(x => x.AmountAfterFeeDeductions); //lineItem.TotalAmountReceived - CalculateFee(lineItem);
                lineItem.ProcessDateTime = DateTime.Now;
                lineItem.CreateDateTime = DateTime.Now;
                lineItem.CreatedBy = TransactionProcessBatchName;

                batchLineItemList.Add(lineItem);

                // _paymentToOrganisationDAL.RecordPaymentToOrganisationBatchTransactionLog(lineItem.Id, bankAccountDonations);
            }
            
            //save newly created batch line items in database
            _paymentToOrganisationDAL.CreatePaymentToOrganisationBatchLineItems(batchLineItemList);

            //play back batch line item and it's transaction running log
            _paymentToOrganisationDAL.PlayBackPaymentToOrganisationBatchTransactionLog();
        }

        private double NullCheck(double? value)
        {
            if (value == null)
                return 0;
            else
                return value.Value;
        }        

        //        private void CreatePaymentToDay3BatchLineItems(PaymentToOrganisationBatch batch)
        //        {
        //            IBankAccountBS bankAccountBS = new BankAccountBS(null, null);
        //            IOrganisationBS organisationBS = new OrganisationBS(null, null);

        //            Dictionary<Guid, List<DonationTransaction>> donationsTowardsBankAccount = new Dictionary<Guid, List<DonationTransaction>>();

        //            List<OrganisationFeeProcesingWithRelatedData> orgFeeProcessingBatch = _organisationFeeProcessing.GetOrganisationFeeProcesingWithRelatedData() as List<OrganisationFeeProcesingWithRelatedData>;

        //            List<PaymentToOrganisationBatchLineItem> batchLineItemList = new List<PaymentToOrganisationBatchLineItem>();

        //            bool isUsingXero = false;
        //            bool isUsingSalesforce = false;
        //            bool isUsingMailChimp = false;
        //            bool isUsingQuickbooks = false;
        //            bool isUsingCampaignMonitor = false;
        //            bool Emailintegration = false;

        //            foreach (OrganisationFeeProcesingWithRelatedData transOrg in orgFeeProcessingBatch)
        //            {
        //                // Check if Xero configured and active for customer 
        //                var _organisationFinanceGateway = new OrganisationFinanceGatewayBS();

        //                try
        //                {
        //                    var gatewayDetails = _organisationFinanceGateway.GetFinanceGatewayDetails(transOrg.Organisation.Id, Convert.ToByte(FinanceGatewayTypes.Xero)).FirstOrDefault();

        //                    if (gatewayDetails != null && gatewayDetails.IsActive)
        //                    {
        //                        isUsingXero = true;
        //                    }
        //                }
        //                catch (Exception)
        //                {
        //                    // ignored - not in use and not active
        //                }


        //                // Check if Salesforce connected and active
        //                try
        //                {
        //                    var _thirdPartyIntegration = new ThirdPartyIntegrationBS();
        //                    SalesforceIntegrationDTO sfDto = _thirdPartyIntegration.GetSalesforceDetails(transOrg.Organisation.Id).FirstOrDefault();

        //                    if (sfDto != null && sfDto.IsActive)
        //                    {
        //                        isUsingSalesforce = true;
        //                    }
        //                }
        //                catch (Exception)
        //                {
        //                    // ignored - not in use and not active
        //                }

        //                // ### TODO: Fill in when further integrations are made available ###

        //                //Get the sum of total monthly amount to charge from the organisation
        //                double amountToCharge = NullCheck(transOrg.OrganisationToProcess.PlanFeeFixed) +
        //                    (isUsingXero ? NullCheck(transOrg.OrganisationToProcess.XeroIntegrationFixedFee) : 0) +
        //                                        (isUsingQuickbooks ? NullCheck(transOrg.OrganisationToProcess.QuickbooksIntegrationFixedFee) : 0) +
        //                                        (Emailintegration ? NullCheck(transOrg.OrganisationToProcess.EmailintegrationFixedFee) : 0) +
        //                                        (isUsingMailChimp ? NullCheck(transOrg.OrganisationToProcess.MailChipIntegrationFixedFee) : 0) +
        //                                        (isUsingCampaignMonitor ? NullCheck(transOrg.OrganisationToProcess.CampaignMonitorIntegrationFixedFee) : 0) +
        //                                        (isUsingSalesforce ? NullCheck(transOrg.OrganisationToProcess.SalesforceCRMIntegrationFixedFee) : 0);

        //                //Calculate the total donation income of the past month (skip manual transactions - Donations marked as created by 'DATA_IMPORT' )
        //                var totalDonationIncome = _donationTransactionDAL.GetDonationTransactionsOfOrganisation_WithRelatedDataByLastBillDate(transOrg.Organisation.Id)
        //                    //.Where(x=>x.OrganisationFeeProcessing.NextRunDate == null || x.OrganisationFeeProcessing.NextRunDate < DateTime.Now)
        //                    .Where(x => x.DonationTransaction.CreateDateTime > x.OrganisationFeeProcessing.LastRunDate && x.DonationTransaction.IsValidForPaymentToOrganisation && x.DonationTransaction.CreatedBy != CreatedBy.DATA_IMPORT.ToString())
        //                    .Sum(x => x.Donation.Amount);

        //                var orgPaymentGatewaySettings = GetOrganisationPaymentGatewayDetails(transOrg.Organisation.Id);

        //                if (!orgPaymentGatewaySettings.GenerousDefaultGateway.HasValue || (orgPaymentGatewaySettings.GenerousDefaultGateway.HasValue && !orgPaymentGatewaySettings.GenerousDefaultGateway.Value))
        //                {
        //                    // amountToCharge + 1% of all the transactions during the month because they are not using the Generous one 
        //                    if (transOrg.OrganisationToProcess.MF_PG_Client_Fee_Precentage != null && transOrg.OrganisationToProcess.MF_PG_Client_Fee_Precentage > 0 && totalDonationIncome > 0)
        //                        amountToCharge = amountToCharge + (double)totalDonationIncome * (transOrg.OrganisationToProcess.MF_PG_Client_Fee_Precentage.Value / 100);
        //                }

        //                // get customers active bank account
        //                BankAccount bankAccount = bankAccountBS.GetBankAccountsOfOrganisation(transOrg.Organisation.Id, null, null).Where(x => x.IsActive).FirstOrDefault();
        //                //Organisation org = organisationBS.GetOrganisation(bankAccount.OrganisationId);

        //                if (bankAccount != null)
        //                {
        //                    List<DonationTransaction> bankAccountDonations = new List<DonationTransaction>();

        //                    PaymentToOrganisationBatchLineItem lineItem = new PaymentToOrganisationBatchLineItem();
        //                    lineItem.ProcessStatusId = (byte)Common.PaymentProcessStatus.Unprocessed;
        //                    lineItem.Id = Guid.NewGuid();
        //                    lineItem.BatchId = batch.Id;
        //                    lineItem.BatchNumber = batch.BatchNumber;
        //                    lineItem.OrganisationId = transOrg.Organisation.Id;
        //                    lineItem.OrganisationName = transOrg.Organisation.Name;
        //                    lineItem.BankName = bankAccount.BankName; // Config.NAB_BankShortName; 
        //                    lineItem.BSBNumber = bankAccount.BSBNumber; // Config.NAB_TraceBSB; 
        //                    lineItem.AccountNumber = bankAccount.AccountNumber; // Config.NAB_TraceAccountNumber;
        //                    lineItem.AccountName = bankAccount.AccountName;
        //                    lineItem.TotalAmountReceived = (decimal)amountToCharge;
        //                    lineItem.TotalPaymentsReceived = 1;//Always 1
        //                    lineItem.TotalAmountPaidToOrganisation = lineItem.TotalAmountReceived;
        //                    lineItem.ProcessDateTime = DateTime.Now;
        //                    lineItem.CreateDateTime = DateTime.Now;
        //                    lineItem.CreatedBy = Common.Config.TransactionProcessBatchName;

        //                    batchLineItemList.Add(lineItem);

        //                    _paymentToOrganisationDAL.RecordPaymentToOrganisationBatchTransactionLog(lineItem.Id, bankAccountDonations);
        //                }
        //            }

        //            //save newly created batch line items in database
        //            _paymentToOrganisationDAL.CreatePaymentToOrganisationBatchLineItems(batchLineItemList);

        //            //play back batch line item and it's transaction running log
        //            _paymentToOrganisationDAL.PlayBackPaymentToOrganisationBatchTransactionLog();
        //        }

        //        /// <summary>
        //        /// Calculate the organisations fees
        //        /// </summary>
        //        /// <param name="lineItem">Donation line item</param>
        //        /// <returns>Value of fees</returns>
        //        private decimal CalculateFee(PaymentToOrganisationBatchLineItem lineItem)
        //        {
        //            //if (Config.CommissionInPercentage.HasValue)
        //            //{
        //            //    return (lineItem.TotalAmountReceived * (Common.Config.CommissionInPercentage.Value / 100M));
        //            //}
        //            //else if (Config.CommissionInTransactionFeeInDollars.HasValue)
        //            //{
        //            //    return (lineItem.TotalPaymentsReceived * Config.CommissionInTransactionFeeInDollars.Value);
        //            //}
        //            //else
        //            //{
        //            //    return 0M;
        //            //}

        //            //Code above is commented as fee calculation is implemented separately.
        //            return 0M;
        //        }
    }
}
