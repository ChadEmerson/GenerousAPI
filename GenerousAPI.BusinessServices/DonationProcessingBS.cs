using GenerousAPI.BusinessServices.ABAGeneration;
using GenerousAPI.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using static GenerousAPI.BusinessEntities.Common;

namespace GenerousAPI.BusinessServices
{
    public class DonationProcessingBS : IDonationProcessingBS
    {
        /// <summary>
        /// Reference to the Donation Transaction Interface
        /// </summary>
        private ITransactionDetailsBS _transactionDetailsBS = null;
                
        /// <summary>
        /// Length of time in days before processing a retry
        /// </summary>
        public static byte RetryPeriodInDays
        {
            get { return (System.Configuration.ConfigurationManager.AppSettings["RetryPeriodInDays"] != null ? Convert.ToByte(System.Configuration.ConfigurationManager.AppSettings["RetryPeriodInDays"]) : (byte)0); }
        }

        public DonationProcessingBS()
        {
            _transactionDetailsBS = new TransactionDetailsBS();
        }

        /// <summary>
        /// Process direct debit donations
        /// </summary>
        /// <param name="donationTransList">List of donations to process</param>
        public void ProcessDirectDebitDonations(List<TransactionDetail> donationTransList)
        {
            if (donationTransList.Count == 0)
                return;

            String generatedDirectDebitFileName = GenerateDirectDebitFile(donationTransList);
            
            //mark the transactions written to ABA file as awaiting clearance
            SetDonationTransactionsStatus(
                donationTransList,
                PaymentGatewayProcessing.Helpers.Enums.PaymentProcessStatus.AwaitingClearance,
                DateTime.Now,
                AbaConfig.TransactionProcessBatchName);

            //send the aba file to concerned staff for bank processsing
            SendDirectDebitFileForProcessing(generatedDirectDebitFileName, donationTransList);
        }

        /// <summary>
        /// Send direct debit file for processing based on AwaitingClearance
        /// </summary>
        /// <param name="donationTransList">Collection of transactions to process</param>
        public List<TransactionDetail> CheckDirectDebitDonations_AwaitingClearance(List<TransactionDetail> donationTransList)
        {
            List<TransactionDetail> donationTransAssumedClearedList = new List<TransactionDetail>();

            foreach (TransactionDetail trans in donationTransList)
            {
                //DateTime lastDayOfClearanceWaitingPeriod = ((DateTime)trans.LastProcessedDateTime).AddDays(Common.Config.ABA_ClearancePeriodInDays);
                DateTime lastDayOfClearanceWaitingPeriod = ((DateTime)trans.Processed_DonationSubmittedDateTime).AddDays(AbaConfig.ABA_ClearancePeriodInDays);

                if (DateTime.Today > lastDayOfClearanceWaitingPeriod)
                {
                    donationTransAssumedClearedList.Add(trans);                    
                }

            }

            //mark the DD transactions awaiting clearance as approved if clearance period has passed
            if (donationTransAssumedClearedList.Count > 0)
            {
                SetDonationTransactionsStatus(
                    donationTransAssumedClearedList,
                    PaymentGatewayProcessing.Helpers.Enums.PaymentProcessStatus.Approved,
                    DateTime.Now,
                    AbaConfig.TransactionProcessBatchName);          
            }

            return donationTransAssumedClearedList;
        }

        /// <summary>
        /// Queues up regular donations - sets to "Unprocessed" for next run
        /// </summary>
        public void QueueUpRegularDonations(TransactionDetail transactionToQueue)
        {
            DateTime runDate = DateTime.Today; //NOTE: You can manipulate run date to emulate future runs of this process

            try
            {
                //set process date
                transactionToQueue.ProcessDateTime = DateTime.Now;
                transactionToQueue.ProcessStatusId = (byte)PaymentGatewayProcessing.Helpers.Enums.PaymentProcessStatus.Unprocessed;
                transactionToQueue.ResponseCode = "Not processed";
                transactionToQueue.ResponseText = "Not processed";

                _transactionDetailsBS.CreateTransactionRecord(transactionToQueue);
            }
            catch (Exception ex)
            {
                // This donation cannot be queued - move on so that we don't have issues for all dontations - email so we know issue exists on this one
                //Helper.LogException(ex);
            }

        }

        /// <summary>
        /// Update a collection of donations with a new payment status
        /// </summary>
        /// <param name="donationTrans">Donation to update</param>
        /// <param name="paymentProcessStatus">Status to update transactions with</param>
        /// <param name="statusUpdateDateTime">Date/time of update</param>
        /// <param name="statusUpdateBy">Person updating status</param>
        public void SetDonationTransactionsStatus(TransactionDetail donationTrans, PaymentGatewayProcessing.Helpers.Enums.PaymentProcessStatus paymentProcessStatus, DateTime statusUpdateDateTime, string statusUpdateBy)
        {
            try
            {
                //update process status or set 'do not process' flag
                if (paymentProcessStatus == PaymentGatewayProcessing.Helpers.Enums.PaymentProcessStatus.DoNotProcess)
                {
                    donationTrans.DoNotProcess = true;
                }
                else
                {
                    donationTrans.DoNotProcess = false;
                    donationTrans.ProcessStatusId = (byte)paymentProcessStatus;
                    donationTrans.HasProcessStatusChanged = true; //acknowledge that we have updated process status
                }

                donationTrans.LastProcessedDateTime = statusUpdateDateTime;
                donationTrans.LastModifiedDateTime = statusUpdateDateTime;
                donationTrans.LastModifiedBy = statusUpdateBy;

                if (paymentProcessStatus == PaymentGatewayProcessing.Helpers.Enums.PaymentProcessStatus.Approved)
                {
                    donationTrans.Processed_DonationFinalisedDateTime = DateTime.Now;
                }
                //for DD when ABA file is generated the transaction status is set to awaiting clearance and we set donation submitted date time to current date time
                else if (paymentProcessStatus == PaymentGatewayProcessing.Helpers.Enums.PaymentProcessStatus.AwaitingClearance)
                {
                    donationTrans.Processed_DonationSubmittedDateTime = DateTime.Now;
                }
                else if (paymentProcessStatus == PaymentGatewayProcessing.Helpers.Enums.PaymentProcessStatus.ReProcess)
                {
                    donationTrans.ProcessRetryCounter = Convert.ToByte((donationTrans.ProcessRetryCounter ?? 0) + 1);

                    //allow the transaction to be re-processed after a certain period of time
                    donationTrans.ProcessDateTime = DateTime.Today.AddDays(RetryPeriodInDays);
                }
                else if (paymentProcessStatus == PaymentGatewayProcessing.Helpers.Enums.PaymentProcessStatus.Declined)
                {
                    // Do nothing
                }

                _transactionDetailsBS.UpdateTransactionRecord(donationTrans);
            }
            catch (Exception ex)
            {
                //Common.Helper.LogException(ex);
            }
        }
        

        /// <summary>
        /// Update a collection of donations with a new payment status
        /// </summary>
        /// <param name="donationTransList">Collection of donations to update</param>
        /// <param name="paymentProcessStatus">Status to update transactions with</param>
        /// <param name="statusUpdateDateTime">Date/time of update</param>
        /// <param name="statusUpdateBy">Person updating status</param>
        public void SetDonationTransactionsStatus(List<TransactionDetail> donationTransList, PaymentGatewayProcessing.Helpers.Enums.PaymentProcessStatus paymentProcessStatus, DateTime statusUpdateDateTime, string statusUpdateBy)
        {
            foreach (TransactionDetail trans in donationTransList)
            {
                try
                {
                    SetDonationTransactionsStatus(trans, paymentProcessStatus, statusUpdateDateTime, statusUpdateBy);
                }
                catch (Exception ex)
                {
                    //Common.Helper.LogException(ex);
                }
            }

            //if (donationTransList.Count() > 0)
                //_transactionDetailsBS.UpdateDonationTransactionList(donationTransList);
        }

        /// <summary>
        /// Send direct debit file for processing
        /// </summary>
        /// <param name="generatedDirectDebitFileName">File name for processing</param>
        /// <param name="donationTransList">Collection of transactions to process</param>
        private void SendDirectDebitFileForProcessing(string generatedDirectDebitFileName, List<TransactionDetail> donationTransList)
        {
            //send email to concerned staff
            MailMessage message = new MailMessage();
            message.To.Add(AbaConfig.ABA_ProcessingStaffEmailList);

            //fetch generated direct debit file
            FileInfo file = new FileInfo(Path.Combine(AbaConfig.ABAPaymentCollectionFilePath, generatedDirectDebitFileName));
            if (file != null)
            {
                decimal totalAmount = donationTransList.Sum(x => x.Amount);

                message.Subject = String.Format("Please process the direct debit file {0} for donations received", generatedDirectDebitFileName);
                message.Body = String.Format("Total amount received: {0}", totalAmount);
                message.Attachments.Add(new Attachment(file.FullName));
            }
            else
            {
                message.Subject = String.Format("The Direct Debit file {0} was not found", generatedDirectDebitFileName);               
            }

            Mail.SendMail(message);
        }

        

        /// <summary>
        /// Generate the direct debit dile
        /// </summary>
        /// <param name="donationTransactions">Collection of transactions</param>
        /// <returns>Direct debit file to process</returns>
        private string GenerateDirectDebitFile(List<TransactionDetail> donationTransactions)
        {
            List<DonationTransactionWithRelatedData> donationTransactionsWithDetails = new List<DonationTransactionWithRelatedData>();
            var transactionIds = new List<Guid>();

            //get bank account number and other details for transactions
            foreach (TransactionDetail trans in donationTransactions)
            {
                try
                {
                    transactionIds.Add(trans.Id);
                }
                catch (Exception ex)
                {
                    // Failed to add this transaction, keep going, we do not want to fail all just because of one issue
                    //Helper.LogException(ex);
                }
            }

            try
            {
                donationTransactionsWithDetails = _transactionDetailsBS.GetDonationTransaction_WithRelatedData(transactionIds);
            }
            catch (Exception ex)
            {
                // Failed to add this transaction, keep going, we do not want to fail all just because of one issue
                //Helper.LogException(ex);
            }

            String ABAFileName = String.Format("{0}-{1}.txt", "DD", DateTime.Now.ToString("yyyyMMddHHmm"));
            NAB_ABAFileGenerator NAB_ABAFileGen = new NAB_ABAFileGenerator(ABAMode.DirectDebit);
            NAB_ABAFileGen.GenerateABAFile(donationTransactionsWithDetails, AbaConfig.ABAPaymentCollectionFilePath, ABAFileName);
            //TODO: Exception handling if invalid data etc. 

            return ABAFileName;
        }
    }
}
