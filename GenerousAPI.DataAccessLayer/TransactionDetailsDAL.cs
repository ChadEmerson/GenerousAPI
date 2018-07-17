using AutoMapper;
using GenerousAPI.BusinessEntities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GenerousAPI.DataAccessLayer
{
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
                            ResponseText = transactionDetails.ResponseText
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
                            ResponseText = transactionDetails.ResponseText
                        }).ToList();
            }

        }
    }
}
