using GenerousAPI.BusinessEntities;
using System;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace GenerousAPI.DataAccessLayer
{
    public class PaymentProfileDAL : IPaymentProfileDAL
    {
        /// <summary>
        /// Create a new donor payment profile for a donor
        /// </summary>
        /// <param name="PaymentProfile">Donor payment profile details</param>
        /// <returns>Payment Response with success, message, and profile token</returns>
        public ProcessorResponse CreatePaymentProfile(PaymentProfile PaymentProfile)
        {
            var response = new ProcessorResponse();

            try
            {
                using (var db = new GenerousAPIEntities())
                {
                    db.PaymentProfiles.Add(PaymentProfile);
                    db.SaveChanges();
                }

                response.Message = "Payment profile successfully saved";
                response.AuthToken = PaymentProfile.TokenId;
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
        /// <param name="PaymentProfile">Donor payment profile details</param>
        /// <returns>Payment Response with success, message, and profile token</returns>
        public ProcessorResponse UpdatePaymentProfile(PaymentProfile PaymentProfile)
        {
            var response = new ProcessorResponse();

            try
            {
                using (var db = new GenerousAPIEntities())
                {
                    //save changes to database
                    db.Entry(PaymentProfile).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }

                response.Message = "Payment profile successfully updated";
                response.AuthToken = PaymentProfile.TokenId;
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
        /// <param name="paymentProfileTokenId">Payment profile token id</param>
        /// <returns>Donor payment profile details</returns>
        public PaymentProfileDTO GetPaymentProfile(string paymentProfileTokenId)
        {
            using (var db = new GenerousAPIEntities())
            {
                return (from paymentProfile in db.PaymentProfiles
                        where paymentProfile.TokenId == paymentProfileTokenId
                        select new PaymentProfileDTO
                        {
                            TransactionMode = (BusinessEntities.TransactionMode)paymentProfile.TransactionMode,
                            CustomerFirstName = paymentProfile.CustomerFirstName,
                            CustomerLastName = paymentProfile.CustomerLastName,
                            BillingAddress = paymentProfile.BillingAddress,
                            BillingCity = paymentProfile.BillingCity,
                            BillingState = paymentProfile.BillingState,
                            Zip = paymentProfile.PostalCode,
                            AccountNumber = paymentProfile.BankAccountNumber,
                            CardNumber = paymentProfile.CardNumber,
                            ExpirationMonth = paymentProfile.CardExpiryMonth,
                            ExpirationYear = paymentProfile.CardExpiryYear,
                            BankName = paymentProfile.BankName,
                            RoutingNumber = paymentProfile.BSBNumber,
                            AccountType = paymentProfile.AccountType,
                            CardType = paymentProfile.CardType,
                            SecurityCode = paymentProfile.CardSerurityNumber
                        }).SingleOrDefault();
            }
        }

        /// <summary>
        /// Delete a payment profile for a donor
        /// </summary>
        /// <param name="paymentProfileTokenId">Payment profile token id</param>
        /// <returns>Payment Response with success, message, and profile token</returns>
        public ProcessorResponse DeletePaymentProfile(string paymentProfileTokenId)
        {
            var response = new ProcessorResponse();

            try
            {
                using (var db = new GenerousAPIEntities())
                {
                    var paymentProfile = db.PaymentProfiles.Where(x => x.TokenId == paymentProfileTokenId).SingleOrDefault();
                    DbEntityEntry dbEntityEntry = db.Entry(paymentProfile);

                    if (dbEntityEntry.State == System.Data.Entity.EntityState.Detached)
                    {
                        db.PaymentProfiles.Attach(paymentProfile);
                    }

                    db.PaymentProfiles.Remove(paymentProfile);
                    db.SaveChanges();
                }

                response.Message = "Payment profile successfully deleted";
                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;

        }
    }
}
