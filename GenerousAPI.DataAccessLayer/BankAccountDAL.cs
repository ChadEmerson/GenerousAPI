using GenerousAPI.BusinessEntities;
using System;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace GenerousAPI.DataAccessLayer
{
    public class BankAccountDAL : IBankAccountDAL
    {
        /// <summary>
        /// Create a new donor payment profile for a donor
        /// </summary>
        /// <param name="BankAccount">Donor payment profile details</param>
        /// <returns>Payment Response with success, message, and profile token</returns>
        public ProcessorResponse CreateBankAccount(BankAccount BankAccount)
        {
            var response = new ProcessorResponse();

            try
            {
                using (var db = new GenerousAPIEntities())
                {
                    db.BankAccounts.Add(BankAccount);
                    db.SaveChanges();
                }

                response.Message = "Bank Account successfully saved";
                response.AuthToken = BankAccount.BankAccountTokenId;
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
        /// Update existing donor payment profile for a donor
        /// </summary>
        /// <param name="BankAccount">Donor payment profile details</param>
        /// <returns>Payment Response with success, message, and profile token</returns>
        public ProcessorResponse UpdateBankAccount(BankAccount BankAccount)
        {
            var response = new ProcessorResponse();

            try
            {
                using (var db = new GenerousAPIEntities())
                {
                    //save changes to database
                    db.Entry(BankAccount).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }

                response.Message = "Bank Account successfully updated";
                response.AuthToken = BankAccount.BankAccountTokenId;
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
        /// Get donors payment profile details
        /// </summary>
        /// <param name="BankAccountTokenId">Payment profile token id</param>
        /// <returns>Donor payment profile details</returns>
        public BankAccountDTO GetBankAccount(string BankAccountTokenId)
        {
            using (var db = new GenerousAPIEntities())
            {
                return (from bankAccount in db.BankAccounts
                        where bankAccount.BankAccountTokenId == BankAccountTokenId
                        select new BankAccountDTO
                        {
                            BankAccountId = bankAccount.BankAccountId,
                            BankAccountTokenId = bankAccount.BankAccountTokenId,
                            BankAccountBSB = bankAccount.BankAccountBSB,
                            BankAccountNumber = bankAccount.BankAccountNumber,
                            BankAcountName = bankAccount.BankAcountName,
                            BankVerificationAmounts = bankAccount.BankVerificationAmounts
                        }).SingleOrDefault();
            }
        }

        /// <summary>
        /// Get Bank account details
        /// </summary>
        /// <param name="bankAccountId">Guid iD of bank</param>
        /// <returns>Bank account details</returns>
        public BankAccount GetBankAccountById(System.Guid bankAccountId)
        {
            try
            {
                using (var db = new GenerousAPIEntities())
                {
                    var bankAccount = db.BankAccounts.SingleOrDefault(x => x.BankAccountId == bankAccountId);
                    return bankAccount;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            
        }

        /// <summary>
        /// Delete a payment profile for a donor
        /// </summary>
        /// <param name="BankAccountTokenId">Payment profile token id</param>
        /// <returns>Payment Response with success, message, and profile token</returns>
        public ProcessorResponse DeleteBankAccount(string BankAccountTokenId)
        {
            var response = new ProcessorResponse();

            try
            {
                using (var db = new GenerousAPIEntities())
                {
                    var BankAccount = db.BankAccounts.Where(x => x.BankAccountTokenId == BankAccountTokenId).SingleOrDefault();
                    DbEntityEntry dbEntityEntry = db.Entry(BankAccount);

                    if (dbEntityEntry.State == System.Data.Entity.EntityState.Detached)
                    {
                        db.BankAccounts.Attach(BankAccount);
                    }

                    BankAccount.Active = false;
                    db.SaveChanges();
                }
                
                response.AuthToken = BankAccountTokenId;
                response.Message = "Bank Account successfully deleted";
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
    }
}
