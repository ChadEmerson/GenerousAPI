
namespace GenerousAPI.DataAccessLayer
{
    using BusinessEntities;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class TransactionDetailsDAL : ITransactionDetailsDAL
    {
        /// <summary>
        /// Create a new record for transaction details
        /// </summary>
        /// <param name="transactionDetails">Transaction details</param>
        /// <returns>Response with success, message, and profile token</returns>
        public ProcessorResponse CreateTransactionRecord(TransactionDetail transactionDetails)
        {
            var response = new ProcessorResponse();

            try
            {
                using (var db = new GenerousAPIEntities())
                {
                    db.TransactionDetails.Add(transactionDetails);
                    db.SaveChanges();
                }

                response.Message = "Transaction successfully saved";
                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Update a record for transaction details
        /// </summary>
        /// <param name="transactionDetails">Transaction details</param>
        /// <returns>Response with success, message, and profile token</returns>
        public ProcessorResponse UpdateTransactionRecord(TransactionDetail transactionDetails)
        {
            var response = new ProcessorResponse();

            try
            {
                using (var db = new GenerousAPIEntities())
                {
                    //save changes to database
                    db.Entry(transactionDetails).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }

                response.Message = "Transaction successfully updated";
                response.AuthToken = transactionDetails.PaymentProfileTokenId;
                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Update existing Bank account
        /// </summary>
        /// <param name="paymentProfile">Donor payment profile details</param>
        /// <returns>Bank account details</returns>
        public TransactionDetailsDTO GetTransactionDetails(Guid transactionId)
        {
            using (var db = new GenerousAPIEntities())
            {
                return (from transactionDetails in db.TransactionDetails
                        where transactionDetails.Id == transactionId
                        select new TransactionDetailsDTO
                        {
                            Id = transactionDetails.Id,
                            Amount = transactionDetails.Amount,
                            BankAccountTokenId = transactionDetails.BankAccountTokenId,
                            PaymentMethodId = transactionDetails.PaymentMethodId,
                            PaymentProfileTokenId = transactionDetails.PaymentProfileTokenId,
                            ProcessDateTime = transactionDetails.ProcessDateTime,
                            ResponseCode = transactionDetails.ResponseCode,
                            ResponseText = transactionDetails.ResponseText,
                            AuditNumber = transactionDetails.AuditNumber,
                            CustomerReference = transactionDetails.CustomerReference,
                            OrganisationId = transactionDetails.OrganisationId.Value
                        }).SingleOrDefault();
            }
        }

        /// <summary>
        /// Get collection of transactions
        /// </summary>
        /// <param name="bankAccountTokenId">Bank Account token Id</param>
        /// <returns>Transaction details</returns>
        public IEnumerable<TransactionDetailsDTO> GetTransactionDetailsForBankAccount(string bankAccountTokenId)
        {
            using (var db = new GenerousAPIEntities())
            {
                return (from transactionDetails in db.TransactionDetails
                        where transactionDetails.BankAccountTokenId == bankAccountTokenId
                        select new TransactionDetailsDTO
                        {
                            Id = transactionDetails.Id,
                            Amount = transactionDetails.Amount,
                            BankAccountTokenId = transactionDetails.BankAccountTokenId,
                            PaymentMethodId = transactionDetails.PaymentMethodId,
                            PaymentProfileTokenId = transactionDetails.PaymentProfileTokenId,
                            ProcessDateTime = transactionDetails.ProcessDateTime,
                            ResponseCode = transactionDetails.ResponseCode,
                            ResponseText = transactionDetails.ResponseText,
                            AuditNumber = transactionDetails.AuditNumber,
                            CustomerReference = transactionDetails.CustomerReference,
                            OrganisationId = transactionDetails.OrganisationId.Value
                        }).ToList();
            }

        }

        /// <summary>
        /// Get collection of donations with related data
        /// </summary>
        /// <param name="batchId">Batch ID to lookup</param>
        /// <returns>Collection of donations with related data</returns>
        public List<DonationTransactionWithRelatedData> GetDonationTransactionsOf_Batch_WithRelatedData(Guid batchId)
        {
            using (var db = new GenerousAPIEntities())
            {
                var transQuery = from trans in db.TransactionDetails
                                 join status in db.PaymentProcessStatus on trans.ProcessStatusId equals status.Id
                                 join PaymentMethod in db.PaymentMethods on trans.PaymentMethodId equals PaymentMethod.Id
                                 where trans.PaymentToOrganisationBatchId == batchId
                                 select new DonationTransactionWithRelatedData()
                                 {
                                     TransactionDetail = trans,
                                     DonationTransaction_ProcessStatus = status.Status,
                                     DonationTransaction_PaymentMethod = PaymentMethod.Method,
                                 };

                return transQuery.ToList<DonationTransactionWithRelatedData>();
            }
        }
    }
}
