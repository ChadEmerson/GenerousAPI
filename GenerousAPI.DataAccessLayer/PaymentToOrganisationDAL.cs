using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GenerousAPI.BusinessEntities.Common;
using static PaymentGatewayProcessing.Helpers.Enums;

namespace GenerousAPI.DataAccessLayer
{
    public class PaymentToOrganisationDAL : IPaymentToOrganisationDAL
    {
        #region public methods

        /// <summary>
        /// Create a payment batch for an organisatsion
        /// </summary>
        /// <param name="batch">Payment batch details</param>
        public void CreatePaymentToOrganisationBatch(PaymentToOrganisationBatch batch)
        {
            using (var db = new GenerousAPIEntities())
            {
                using (var trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.PaymentToOrganisationBatches.Add(batch);

                        if (batch.PaymentToOrganisationBatchLineItems != null && batch.PaymentToOrganisationBatchLineItems.Count > 0)
                        {
                            db.PaymentToOrganisationBatchLineItems.AddRange(batch.PaymentToOrganisationBatchLineItems);
                        }

                        db.SaveChanges();

                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw ex;
                    }
                }
            }

        }

        /// <summary>
        /// Assigns the donation batch to approved donations
        /// </summary>
        /// <param name="batch">Batch donation details</param>
        public void AssignBatchToApprovedDonations(PaymentToOrganisationBatch batch)
        {
            using (var db = new GenerousAPIEntities())
            {
                db.AssignBatchToApprovedDonations(batch.Id);
            }
        }

        /// <summary>
        /// Save new batch line items
        /// </summary>
        /// <param name="batchLineItemList">Collection of line items to save</param>
        public void CreatePaymentToOrganisationBatchLineItems(List<PaymentToOrganisationBatchLineItem> batchLineItemList)
        {
            using (var db = new GenerousAPIEntities())
            {
                db.PaymentToOrganisationBatchLineItems.AddRange(batchLineItemList);
                db.SaveChanges();
            }

            //create log entries for multiple transactions
            CreateTransactionLogEntries(batchLineItemList);
        }

        /// <summary>
        /// Get a collection of of transactions associated with the batch id
        /// </summary>
        /// <param name="batch">Batch ID</param>
        /// <returns>Donations in the batch</returns>
        public IEnumerable<TransactionDetail> GetDonationTransactionsAssignedToBatch(PaymentToOrganisationBatch batch)
        {
            using (var db = new GenerousAPIEntities())
            {
                var query = from t in db.TransactionDetails
                            where t.PaymentToOrganisationBatchId == batch.Id
                            select t;

                return query.ToList<TransactionDetail>();
            }

        }

        /// <summary>
        /// Get a collection of batch payments to the organisation by the status
        /// </summary>
        /// <param name="batchStatus">Status to search for</param>
        /// <returns>Collection of batch payments</returns>
        public IEnumerable<PaymentToOrganisationBatch> GetPaymentToOrganisationBatchByStatus(BatchPaymentToOrganisationStatus batchStatus, string batchNumber = "")
        {
            using (var db = new GenerousAPIEntities())
            {
                switch (batchStatus)
                {
                    case BatchPaymentToOrganisationStatus.PaymentToHeartBurst:
                        return db.PaymentToOrganisationBatches.Where(x => x.BatchNumber == batchNumber).ToList();
                    case BatchPaymentToOrganisationStatus.Unprocessed:
                        return db.PaymentToOrganisationBatches.Where(x => x.LastProcessedDateTime == null).ToList();
                    case BatchPaymentToOrganisationStatus.AwaitingCompletion:
                        return db.PaymentToOrganisationBatches.Where(x => x.LastProcessedDateTime != null && x.BatchCompletedDateTime == null).ToList();
                    case BatchPaymentToOrganisationStatus.Completed:
                        return db.PaymentToOrganisationBatches.Where(x => x.BatchCompletedDateTime != null).ToList();
                    default:
                        return null;
                }
            }
        }

        /// <summary>
        /// Get a collection of donation batch line items for an organisation
        /// </summary>
        /// <param name="batch">Batch item details</param>
        /// <param name="paymentProcessStatus">Status to search for</param>
        /// <param name="dateTimeToBeProcessed">Date time to process donation</param>
        /// <returns>Collection of donation batch line items</returns>
        public IEnumerable<PaymentToOrganisationBatchLineItem> GetPaymentToOrganisationBatchLineItemsByStatus_ForProcessing(PaymentToOrganisationBatch batch, PaymentProcessStatus paymentProcessStatus, DateTime dateTimeToBeProcessed)
        {
            byte statusValue = (byte)paymentProcessStatus;

            using (var db = new GenerousAPIEntities())
            {
                var lineItemList = from lineItem in db.PaymentToOrganisationBatchLineItems
                                   where
                                      lineItem.BatchId == batch.Id &&
                                      lineItem.ProcessStatusId == statusValue &&
                                      lineItem.ProcessDateTime <= dateTimeToBeProcessed &&
                                      lineItem.DoNotProcess == false
                                   select lineItem;

                return lineItemList.ToList<PaymentToOrganisationBatchLineItem>();
            }

        }

        /// <summary>
        /// Updates the the batch line item list
        /// </summary>
        /// <param name="batchLineItemList">Line items</param>
        public void UpdatePaymentToOrganisationBatchLineItemList(List<PaymentToOrganisationBatchLineItem> batchLineItemList)
        {
            using (var db = new GenerousAPIEntities())
            {
                foreach (PaymentToOrganisationBatchLineItem lineItem in batchLineItemList)
                {
                    db.Entry(lineItem).State = System.Data.Entity.EntityState.Modified;
                }

                db.SaveChanges(); //batching multiple updates
            }

            //create log entries for multiple transactions
            CreateTransactionLogEntries(batchLineItemList);
        }

        /// <summary>
        /// Update batch payment details
        /// </summary>
        /// <param name="batch">Batch details</param>
        /// <param name="updatedOn">Date/time batch updated</param>
        /// <param name="updatedBy">Who pdated batch</param>
        public void UpdatePaymentToOrganisationBatchProcessStatus(PaymentToOrganisationBatch batch, DateTime updatedOn, string updatedBy)
        {
            using (var db = new GenerousAPIEntities())
            {
                db.UpdatePaymentToOrganisationBatchProcessStatus(batch.Id, updatedOn, updatedBy);
            }
        }

        /// <summary>
        /// Get collection of batch items to process
        /// </summary>
        /// <param name="batch">Batch items</param>
        /// <param name="dateTimeToBeProcessed">Process date time</param>
        /// <returns>Collection of batch items to process</returns>
        public IEnumerable<PaymentToOrganisationBatchLineItem> GetPaymentToOrganisationBatchLineItems_ForProcessing(PaymentToOrganisationBatch batch, DateTime dateTimeToBeProcessed)
        {
            using (var db = new GenerousAPIEntities())
            {
                var lineItemList = from lineItem in db.PaymentToOrganisationBatchLineItems
                                   where
                                      lineItem.BatchId == batch.Id &&
                                      lineItem.ProcessDateTime <= dateTimeToBeProcessed &&
                                      lineItem.DoNotProcess == false
                                   select lineItem;

                return lineItemList.ToList<PaymentToOrganisationBatchLineItem>();
            }
        }

        /// <summary>
        /// Check if there are any dontaions that are not assigned to a batch
        /// </summary>
        /// <returns>True if donations are not assigned to a batch but are approved, false otherwise</returns>
        public bool AreThereAny_APPROVED_DonationTransactions_NotAssigned_To_Any_Batch()
        {
            int count = 0;
            byte approvedStatus = (byte)PaymentProcessStatus.Approved;

            using (var db = new GenerousAPIEntities())
            {
                count = db.TransactionDetails.Where(x => x.PaymentToOrganisationBatchId == null && x.ProcessStatusId.Value == approvedStatus && x.IsValidForPaymentToOrganisation.Value).Count();
            }

            return (count > 0);
        }

        public bool AreThereAny_Organisation_Fee_To_Process()
        {
            int count = 0;

            //using (var db = new GenerousAPIEntities())
            //{
            //    var query = db.OrganisationFeeProcessings.ToList();
            //    count = query.Where(x => (x.NextRunDate == null || x.NextRunDate < DateTime.Now) && x.IsActive).Count();
            //    //query.Where(x => (DateTime.Now - x.LastRunDate).Value.TotalDays > (int)FeeProcessingSettings.DateCountToNextFeeProcessing).Count();
            //}

            return (count > 0);
        }

        /// <summary>
        /// Gets a collection of payment to organisation details 
        /// </summary>
        /// <param name="organisationId">Organisatsion to search</param>        
        /// <returns>Collection of payment to organisation details</returns>
        public List<PaymentToOrganisationListDTO> GetPaymentToOrganisationTransactions(int organisationId)
        {
            using (var db = new GenerousAPIEntities())
            {
                IQueryable<PaymentToOrganisationListDTO> query = from batchLineItem in db.PaymentToOrganisationBatchLineItems
                                                                 join trans in db.TransactionDetails on batchLineItem.BatchId equals trans.PaymentToOrganisationBatchId into transSub
                                                                 where batchLineItem.IsBankVerification == false
                                                                 select new PaymentToOrganisationListDTO
                                                                 {
                                                                     LineItemDonationTransactions = transSub.Where(x => x.PaymentToOrganisationBatchId == batchLineItem.BatchId
                                                                     && x.PaymentToOrganisationBatchLineItemId == batchLineItem.Id),
                                                                     //GetPaymentToOrganisationTransactionsTest(organisationId, db),
                                                                     //transSub.Where(x => x.PaymentToOrganisationBatchId == batchLineItem.BatchId),
                                                                     //transSub.Where(x=>x.PaymentToOrganisationBatchId == batchLineItem.BatchId),
                                                                     PaymentToOrganisationBatchLineItem = batchLineItem,
                                                                     MinTransDateTime = transSub.Min(x => x.ProcessDateTime),
                                                                     MaxTransDateTime = transSub.Max(x => x.ProcessDateTime)
                                                                 };

                
                return query.ToList<PaymentToOrganisationListDTO>();
            }
        }

        public List<TransactionDetail> GetPaymentToOrganisationTransactionsTest(int organisationId, GenerousAPIEntities db)
        {

            var query = (from batchLineItem in db.PaymentToOrganisationBatchLineItems
                         join trans in db.TransactionDetails on batchLineItem.BatchId equals trans.PaymentToOrganisationBatchId
                         where batchLineItem.IsBankVerification == false
                         select trans);

            return query.ToList();
        }


        //public OrganisationFeeProcessing GetOrganisationFeeProcessingSettings(int organisationId)
        //{
        //    using (var db = new GenerousAPIEntities())
        //    {
        //        return db.OrganisationFeeProcessings.Where(x => x.OrganisationId == organisationId).SingleOrDefault<OrganisationFeeProcessing>();
        //    }
        //}

        /// <summary>
        /// Gets a collection of payment to organisation details 
        /// </summary>
        public List<PaymentToOrganisationListDTO> GetPaymentToOrganisationTransactions()
        {
            using (var db = new GenerousAPIEntities())
            {
                IQueryable<PaymentToOrganisationListDTO> query = from batchLineItem in db.PaymentToOrganisationBatchLineItems
                                                                 join trans in db.TransactionDetails on batchLineItem.BatchId equals trans.PaymentToOrganisationBatchId into transSub
                                                                 where batchLineItem.IsBankVerification == false
                                                                 select new PaymentToOrganisationListDTO
                                                                 {
                                                                     PaymentToOrganisationBatchLineItem = batchLineItem,
                                                                     MinTransDateTime = transSub.Min(x => x.ProcessDateTime),
                                                                     MaxTransDateTime = transSub.Max(x => x.ProcessDateTime)
                                                                 };

                return query.ToList<PaymentToOrganisationListDTO>();
            }
        }

        /// <summary>
        /// Gets a collection of payment to organisation details based on the bank account
        /// </summary>
        /// <param name="bankAccountId">Bank account id</param>
        /// <returns>Collection of payment to organisation details</returns>
        public List<PaymentToOrganisationListDTO> GetPaymentToOrganisationTransactionsForBankAccount(Guid bankAccountId)
        {
            using (var db = new GenerousAPIEntities())
            {
                IQueryable<PaymentToOrganisationListDTO> query = from batchLineItem in db.PaymentToOrganisationBatchLineItems
                                                                 join trans in db.TransactionDetails on batchLineItem.BatchId equals trans.PaymentToOrganisationBatchId into transSub
                                                                 where batchLineItem.BankAccountId == bankAccountId
                                                                       && batchLineItem.IsBankVerification == false
                                                                 select new PaymentToOrganisationListDTO
                                                                 {
                                                                     LineItemDonationTransactions = transSub.Where(x => x.PaymentToOrganisationBatchId == batchLineItem.BatchId && x.PaymentToOrganisationBatchLineItemId == batchLineItem.Id),
                                                                     PaymentToOrganisationBatchLineItem = batchLineItem,
                                                                     MinTransDateTime = transSub.Min(x => x.ProcessDateTime),
                                                                     MaxTransDateTime = transSub.Max(x => x.ProcessDateTime)
                                                                 };
                                
                return query.ToList<PaymentToOrganisationListDTO>();
            }
        }


        /// <summary>
        /// Gets a collection of payment to organisation details based on the bank account
        /// </summary>
        /// <param name="bankAccountId">Bank account id</param>
        /// <param name="organisationId">Organisation id</param>
        /// <returns>Collection of payment to organisation details</returns>
        public List<PaymentToOrganisationListDTO> GetPaymentToOrganisationTransactionsForBankAccountByOrganisation(Guid bankAccountId, int organisationId)
        {
            using (var db = new GenerousAPIEntities())
            {
                IQueryable<PaymentToOrganisationListDTO> query = from batchLineItem in db.PaymentToOrganisationBatchLineItems
                                                                 join trans in db.TransactionDetails on batchLineItem.BatchId equals trans.PaymentToOrganisationBatchId into transSub
                                                                 where batchLineItem.BankAccountId == bankAccountId
                                                                       && batchLineItem.IsBankVerification == false
                                                                 select new PaymentToOrganisationListDTO
                                                                 {
                                                                     LineItemDonationTransactions = transSub.Where(x => x.PaymentToOrganisationBatchId == batchLineItem.BatchId && x.PaymentToOrganisationBatchLineItemId == batchLineItem.Id),
                                                                     PaymentToOrganisationBatchLineItem = batchLineItem,
                                                                     MinTransDateTime = transSub.Min(x => x.ProcessDateTime),
                                                                     MaxTransDateTime = transSub.Max(x => x.ProcessDateTime)
                                                                 };

                return query.ToList<PaymentToOrganisationListDTO>();
            }
        }


        /// <summary>
        /// Get a payment line item for an organisation
        /// </summary>
        /// <param name="batchLineItemId">Line Item ID</param>
        /// <returns>Payment line item for an organisation</returns>
        public PaymentToOrganisationBatchLineItem GetPaymentToOrganisationBatchLineItem(Guid batchLineItemId)
        {
            using (var db = new GenerousAPIEntities())
            {
                return db.PaymentToOrganisationBatchLineItems.Where(x => x.Id == batchLineItemId).SingleOrDefault<PaymentToOrganisationBatchLineItem>();
            }
        }

        /// <summary>
        /// Get a collection of payment line items for an organisation
        /// </summary>
        /// <param name="listOfIds">List of line item ids</param>
        /// <returns>Collection of payment line items</returns>
        public List<PaymentToOrganisationBatchLineItem> GetPaymentToOrganisationBatchLineItemsByIds(List<Guid> listOfIds)
        {
            using (var db = new GenerousAPIEntities())
            {
                var query = from lineItem in db.PaymentToOrganisationBatchLineItems
                            where listOfIds.Contains(lineItem.Id)
                            select lineItem;

                return query.ToList<PaymentToOrganisationBatchLineItem>();
            }
        }

        /// <summary>
        /// Get a collection of payment line items for an organisation
        /// </summary>
        /// <param name="listOfLineItemNumbers">List of line item numbers</param>
        /// <returns>Collection of payment line items<</returns>
        public List<PaymentToOrganisationBatchLineItem> GetPaymentToOrganisationBatchLineItemsByLineItemNumbers(List<long> listOfLineItemNumbers)
        {
            using (var db = new GenerousAPIEntities())
            {
                var query = from lineItem in db.PaymentToOrganisationBatchLineItems
                            where listOfLineItemNumbers.Contains(lineItem.LineItemNumber)
                            select lineItem;

                return query.ToList<PaymentToOrganisationBatchLineItem>();
            }
        }

        /// <summary>
        /// Get a collection of payment line items for an organisation by bank account id
        /// </summary>
        /// <param name="bankAccountId">bank account id</param>
        /// <returns>Collection of payment line items</returns>
        public List<PaymentToOrganisationBatchLineItem> GetBankVerificationPaymenBatchLineItems(Guid bankAccountId)
        {
            using (var db = new GenerousAPIEntities())
            {
                var query = from lineItem in db.PaymentToOrganisationBatchLineItems
                            where lineItem.BankAccountId == bankAccountId
                            && lineItem.IsBankVerification == true
                            select lineItem;

                return query.ToList<PaymentToOrganisationBatchLineItem>();
            }
        }

        /// <summary>
        /// Record association between batch line item and it's transactions as a running log that will be played back later 
        /// to batch update transaction records as a single update statement in stored procedure with log being trimmed post 
        /// play back this is more efficient than updating required transaction records here to store the batch line item id 
        /// against transaction records as we cannot batch it and it will have to be done one by one for each transaction record
        /// </summary>
        /// <param name="batchLineItemId">Line item id</param>
        /// <param name="bankAccountDonations">List of donations</param>
        public void RecordPaymentToOrganisationBatchTransactionLog(Guid batchLineItemId, List<TransactionDetail> bankAccountDonations)
        {
            //List<PaymentToOrganisationBatchTransactionLog> logEntries = new List<PaymentToOrganisationBatchTransactionLog>();

            //foreach (DonationTransaction trans in bankAccountDonations)
            //{
            //    logEntries.Add(new PaymentToOrganisationBatchTransactionLog
            //    {
            //        PaymentToOrganisationBatchLineItemId = batchLineItemId,
            //        DonationTransactionId = trans.Id
            //    });
            //}

            //using (var db = new GenerousAPIEntities())
            //{
            //    db.PaymentToOrganisationBatchTransactionLogs.AddRange(logEntries);
            //    db.SaveChanges();
            //}
        }

        /// <summary>
        /// Assigns batch line items to approved Donations 
        /// </summary>
        public void PlayBackPaymentToOrganisationBatchTransactionLog()
        {
            using (var db = new GenerousAPIEntities())
            {
                db.AssignBatchLineItemToApprovedDonations();
            }
        }
        
        /// <summary>
        /// Create a transaction log entry
        /// </summary>
        /// <param name="paymentToOrganisationTransaction">Batch line items</param>
        /// <param name="saveInDatabase">True if saving in db, false otherwise</param>
        /// <returns>Transaction history details</returns>
        private static TransactionHistory CreateTransactionLogEntry(PaymentToOrganisationBatchLineItem paymentToOrganisationTransaction, bool saveInDatabase = true)
        {

            //if (!paymentToOrganisationTransaction.HasProcessStatusChanged || !Common.Config.CreateTransactionAuditLog)
            //    return null;

            //create log entry in history
            TransactionHistory logEntry = new TransactionHistory();

            //logEntry.Id = Guid.NewGuid();
            //logEntry.TransactionTypeId =
            //logEntry.TransactionId = paymentToOrganisationTransaction.Id;
            //logEntry.Status = Enum.GetName(typeof(PaymentProcessStatus), paymentToOrganisationTransaction.ProcessStatusId);

            //if (paymentToOrganisationTransaction.LastModifiedDateTime.HasValue)
            //    logEntry.StatusUpdateDateTime = paymentToOrganisationTransaction.LastModifiedDateTime;
            //else
            //    logEntry.StatusUpdateDateTime = paymentToOrganisationTransaction.CreateDateTime;

            //if (!String.IsNullOrEmpty(paymentToOrganisationTransaction.LastModifiedBy))
            //    logEntry.StatusUpdateBy = paymentToOrganisationTransaction.LastModifiedBy;
            //else
            //    logEntry.StatusUpdateBy = paymentToOrganisationTransaction.CreatedBy;

            //if (saveInDatabase)
            //{
            //    using (var db = new GenerousAPIEntities())
            //    {
            //        db.TransactionHistories.Add(logEntry);
            //        db.SaveChanges();
            //    }
            //}

            return logEntry;

        }

        /// <summary>
        /// Create transaction log entries based on the line items
        /// </summary>
        /// <param name="paymentToOrganisationTransactionList">List of transaction of payment for an organization</param>
        private void CreateTransactionLogEntries(IEnumerable<PaymentToOrganisationBatchLineItem> paymentToOrganisationTransactionList)
        {
            //List<TransactionHistory> logEntries = new List<TransactionHistory>();

            ////create list of log entries
            //foreach (PaymentToOrganisationBatchLineItem trans in paymentToOrganisationTransactionList)
            //{
            //    if (trans.HasProcessStatusChanged && Common.Config.CreateTransactionAuditLog)
            //    {
            //        logEntries.Add(CreateTransactionLogEntry(trans, false));
            //    }
            //}

            ////save multiple log entries as a single batch
            //using (var db = new GenerousAPIEntities())
            //{
            //    db.TransactionHistories.AddRange(logEntries);
            //    db.SaveChanges(); //batching multiple creates
            //}
        }

        #endregion
    }
}
