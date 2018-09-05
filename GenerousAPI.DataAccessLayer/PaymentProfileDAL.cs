namespace GenerousAPI.DataAccessLayer
{
    using BusinessEntities;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Linq;

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
                response.Message = ex.Message +  ex.InnerException;
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

        /// <summary>
        /// Get active payment profile details for cards that are expiring
        /// </summary>
        /// <param name="ExpiryMonth">Expiring month</param>
        /// <param name="ExpiryYear">Expiring year</param>
        /// <returns>Donor payment profile details</returns>
        public List<ContactDetailsDTO> GetExpiringCards(string expiryMonth, string expiryYear)
        {
            try
            {
                using (var db = new GenerousAPIEntities())
                {
                    var paymentProfilesWithExpiringCards = from paymentProfiles in db.PaymentProfiles
                                                           where paymentProfiles.CardExpiryMonth == expiryMonth && 
                                                           paymentProfiles.CardExpiryYear == expiryYear &&
                                                           paymentProfiles.IsActive == true
                                                           select new ContactDetailsDTO
                                                           {
                                                               CustomerFirstName = paymentProfiles.CustomerFirstName,
                                                               CustomerLastName = paymentProfiles.CustomerLastName,
                                                               TokenId = paymentProfiles.TokenId,
                                                               ExpiryMonth = expiryMonth,
                                                               ExpiryYear = expiryYear,
                                                               CardNumberMask = paymentProfiles.CardNumber
                                                           };

                    return paymentProfilesWithExpiringCards.ToList();
                }                
            }
            catch (Exception)
            {
                return null;
            }
        }

        // <summary>
        /// Get when a card is expiring for the token
        /// </summary>
        /// <param name="ExpiryMonth">tokenId of payment profile</param>
        /// <returns>Donor payment profile details</returns>
        public ContactDetailsDTO GetCardExpiryForTokenId(string tokenId)
        {
            try
            {
                using (var db = new GenerousAPIEntities())
                {
                    var paymentProfilesWithExpiringCards = from paymentProfiles in db.PaymentProfiles
                                                           where paymentProfiles.TokenId == tokenId &&
                                                           paymentProfiles.IsActive == true
                                                           select new ContactDetailsDTO
                                                           {
                                                               CustomerFirstName = paymentProfiles.CustomerFirstName,
                                                               CustomerLastName = paymentProfiles.CustomerLastName,
                                                               TokenId = paymentProfiles.TokenId,
                                                               ExpiryMonth = paymentProfiles.CardExpiryMonth,
                                                               ExpiryYear = paymentProfiles.CardExpiryYear,
                                                               CardNumberMask = paymentProfiles.CardNumber
                                                           };

                    return paymentProfilesWithExpiringCards.FirstOrDefault();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Save Expiring Credit Card Detais
        /// </summary>
        /// <param name="expiringCardDetails">Details of cards</param>
        public void SaveExpiringCreditCardDetais(ExpiringCreditCardsForOrganisation expiringCardDetails)
        {
            try
            {
                using (var db = new GenerousAPIEntities())
                {
                    db.ExpiringCreditCardsForOrganisations.Add(expiringCardDetails);
                    db.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                return;
            }
        }

        /// <summary>
        /// Clear out records from table
        /// </summary>
        /// <returns>ProcessorResponse info</returns>
        public ProcessorResponse ClearExpiringCreditCardInfo()
        {
            var response = new ProcessorResponse();

            try
            {
                using (var db = new GenerousAPIEntities())
                {
                    db.Database.ExecuteSqlCommand("DELETE from ExpiringCreditCardsForOrganisation");
                }

                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
            }

            return response;
        }

        /// <summary>
        /// Get list of expiring credit cards for an organisation
        /// </summary>
        /// <param name="organisationId">Organisation Id to get details for</param>
        /// <returns>Collection of contact details</returns>
        public List<ContactDetailsDTO> GetExpiringCreditCardInfoForOrganisation(int organisationId)
        {
            try
            {
                using (var db = new GenerousAPIEntities())
                {
                    var paymentProfilesWithExpiringCards = from cardDetailsForOrganisation in db.ExpiringCreditCardsForOrganisations
                                                           where cardDetailsForOrganisation.OrganisationId == organisationId
                                                           select new ContactDetailsDTO
                                                           {
                                                               CustomerFirstName = cardDetailsForOrganisation.CustomerFirstName,
                                                               CustomerLastName = cardDetailsForOrganisation.CustomerLastName,
                                                               CardNumberMask = cardDetailsForOrganisation.CardNumberMask,
                                                               ExpiryMonth = cardDetailsForOrganisation.ExpiryMonth.ToString(),
                                                               ExpiryYear = cardDetailsForOrganisation.ExpiryYear.ToString(),
                                                               TokenId = cardDetailsForOrganisation.CardTokenId                                                               
                                                           };

                    return paymentProfilesWithExpiringCards.ToList();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
