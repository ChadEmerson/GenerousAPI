using GenerousAPI.BusinessEntities;
using GenerousAPI.BusinessServices;
using GenerousAPI.Helpers;
using GenerousAPI.Models;
using PaymentGatewayProcessing.CardAccess;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web.Http;

namespace GenerousAPI.Controllers
{
    public class PaymentProcessController : ApiController
    {
        private IPaymentProfileBS _IPaymentProfileBS = null;
        private IPaymentGatewayBS _IPaymentGatewayBS = null;
        private IBankAccountBS _IBankAccountBS = null;

        public const string CardAccessMerchantId = "2004";
        public const string CardAccessPassword = "password1234";


        public string[] Get()
        {
            return new string[]
            {
        "Hello",
        "World"
            };
        }

        [AcceptVerbs("POST")]
        [HttpPost]
        public ProcessorResponse CreatePaymentProfile([FromBody]PaymentProfileDTO paymentProfileDTO)
        {
            try
            {
                // Create the new token
                var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

                // Create Data Access object 
                var paymentProfile = DataTransformPaymentProfile(paymentProfileDTO);

                if (!string.IsNullOrEmpty(paymentProfileDTO.CardNumber) && 
                    !string.IsNullOrEmpty(paymentProfileDTO.ExpirationMonth) && 
                    !string.IsNullOrEmpty(paymentProfileDTO.ExpirationYear))
                {
                    paymentProfile.PaymentMethodId = (byte)Enums.PaymentMethod.CreditCard;
                }
                else
                {
                    paymentProfile.PaymentMethodId = (byte)Enums.PaymentMethod.DirectDebit;
                }

                paymentProfile.TokenId = token;

                _IPaymentProfileBS = new PaymentProfileBS();
                // Save the payment profile
                var response = _IPaymentProfileBS.CreatePaymentProfile(paymentProfile);

                // Return a token id
                return response;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [AcceptVerbs("POST")]
        [HttpPost]
        public ProcessorResponse CreateBankAccount([FromBody]BankAccountDTO bankAccountDTO)
        {
            try
            {
                // Create the new token
                var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

                // Create Data Access object 
                var bankAccount = DataTransformBankAccountDTO(bankAccountDTO);
                bankAccount.BankAccountTokenId = token;

                _IBankAccountBS = new BankAccountBS();

                // Save the bank account
                var response = _IBankAccountBS.CreateBankAccount(bankAccount);

                // Return a token id
                return response;
            }
            catch (Exception)
            {
                return null;
            }
        }

        [AcceptVerbs("POST")]
        [HttpPost]
        public ProcessorResponse DeletePaymentProfile([FromBody]string token)
        {
            try
            {
                _IPaymentProfileBS = new PaymentProfileBS();

                // Delete the payment profile
                var response = _IPaymentProfileBS.DeletePaymentProfile(token);

                // Return response
                return response;
            }
            catch (Exception)
            {
                return null;
            }
            
        }

        [AcceptVerbs("POST")]
        [HttpPost]
        public ProcessorResponse DeleteBankAccount([FromBody]string token)
        {
            try
            {
                _IBankAccountBS = new BankAccountBS();
                
                // Delete the bank account
                var response = _IBankAccountBS.DeleteBankAccount(token);

                // Return response
                return response;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="transactionDetails">Collection of transaction details</param>
        /// <returns>Collection of responses for each payment</returns>
        [HttpPost]
        [AcceptVerbs("POST")]
        public IEnumerable<PaymentResponse> ProcessPayment([FromBody]IEnumerable<TransactionDetails> transactionDetails)
        {
            PaymentGatewayProcessing.PaymentGatewayProcessing paymentGatewayProcessing = null;
            NameValueCollection collection = PaymentGatewayConfigXMLParser.ParseConfigXML(GetGenerousPaymentGatewayDetails((byte)Enums.PaymentGatewayType.GENEROUS).GatewayConfig);
            paymentGatewayProcessing = new ProcessCardAccessPayment(collection);

            PaymentGatewayProcessing.Helpers.PaymentRequestDetails cardAccessRequest = new PaymentGatewayProcessing.Helpers.PaymentRequestDetails();

            var paymentResponses = new List<PaymentResponse>();

            // Get payment profile details for each transaction and process           
            foreach (var transaction in transactionDetails)
            {
                try
                {
                    if (transaction.IsTest)
                    {
                        var targetUrl = ProcessCardAccessPayment.TestUrl;
                        PaymentGatewayProcessing.Interfaces.IAuthentication authentication = new PaymentGatewayProcessing.Helpers.HashAuthentication(CardAccessPassword);

                        var paymentGateway = new ProcessCardAccessPayment(targetUrl, CardAccessMerchantId, authentication);

                        var auditId = paymentGateway.GetAudit();

                        if (!string.IsNullOrEmpty(auditId))
                        {
                            var paymentResponse = new PaymentResponse();
                            paymentResponse.IsSuccess = true;
                            paymentResponse.Message = "Test successful";
                            paymentResponse.Amount = transaction.Amount;
                            paymentResponse.FundId = transaction.FundId;

                            paymentResponses.Add(paymentResponse);
                        }
                    }
                    else
                    {
                        var paymentProfile = GetPaymentProfileDetails(transaction.PaymentProfileTokenId);

                        // Convert to cents
                        cardAccessRequest.Amount = transaction.Amount * 100;
                        cardAccessRequest.CardType = paymentProfile.CardType;
                        cardAccessRequest.CardNumber = paymentProfile.CardNumber;
                        cardAccessRequest.CardExpiryMonth = paymentProfile.ExpirationMonth;
                        cardAccessRequest.CardExpiryYear = paymentProfile.ExpirationYear;
                        cardAccessRequest.CCV = paymentProfile.SecurityCode;
                        cardAccessRequest.NameOnCard = paymentProfile.CustomerFullName;
                        cardAccessRequest.DonationTransactionReferenceNumber = transaction.TransactionReferenceNumber;

                        var cardAccessResponse = paymentGatewayProcessing.ProcessCreditCardDonation(cardAccessRequest);

                        var paymentResponse = new PaymentResponse();
                        paymentResponse.IsSuccess = cardAccessResponse.TransactionSuccessful;
                        paymentResponse.Message = cardAccessResponse.ResponseMessage + " " + cardAccessResponse.ResponseText;
                        paymentResponse.Amount = transaction.Amount;
                        paymentResponse.FundId = transaction.FundId;

                        paymentResponses.Add(paymentResponse);
                    }
                    
                }
                catch (Exception ex)
                {
                    var paymentResponse = new PaymentResponse();
                    paymentResponse.IsSuccess = false;
                    paymentResponse.Message = ex.Message;
                    paymentResponse.Amount = transaction.Amount;
                    transaction.FundId = transaction.FundId;

                    paymentResponses.Add(paymentResponse);
                }
            }

            return paymentResponses;
        }

        /// <summary>
        /// Get the payment profile details based on the token
        /// </summary>
        /// <param name="paymentProfiletokenID">Token Id</param>
        /// <returns>payment profile details</returns>        
        private PaymentProfileDTO GetPaymentProfileDetails(string paymentProfiletokenID)
        {
            _IPaymentProfileBS = new PaymentProfileBS();

            var paymentProfileDTO = _IPaymentProfileBS.GetPaymentProfile(paymentProfiletokenID);

            paymentProfileDTO.RoutingNumber = EncryptionService.Decrypt(paymentProfileDTO.RoutingNumber);
            paymentProfileDTO.AccountNumber = EncryptionService.Decrypt(paymentProfileDTO.AccountNumber);
            paymentProfileDTO.CardNumber = EncryptionService.Decrypt(paymentProfileDTO.CardNumber);
            paymentProfileDTO.ExpirationMonth = EncryptionService.Decrypt(paymentProfileDTO.ExpirationMonth.ToString());
            paymentProfileDTO.ExpirationYear = EncryptionService.Decrypt(paymentProfileDTO.ExpirationYear.ToString());
            paymentProfileDTO.SecurityCode = EncryptionService.Decrypt(paymentProfileDTO.SecurityCode.ToString());
            return paymentProfileDTO;
        }

        /// <summary>
        /// Get the payment gateway type
        /// </summary>
        /// <param name="paymentGatewayType"></param>
        /// <returns></returns>
        private PaymentGatewayDTO GetGenerousPaymentGatewayDetails(byte paymentGatewayType)
        {
            _IPaymentGatewayBS = new PaymentGatewayBS();

            var paymentGateways = _IPaymentGatewayBS.GetPaymentGatewayDetails(paymentGatewayType);

            return paymentGateways.FirstOrDefault();
        }

        /// <summary>
        /// Create the payment profile object and use the DTO to transform the data
        /// </summary>
        /// <param name="paymentProfileDTO">DTO of the payment profile</param>
        /// <returns>Payment Profile object for DAL</returns>
        private DataAccessLayer.PaymentProfile DataTransformPaymentProfile(PaymentProfileDTO paymentProfileDTO)
        {
            var paymentProfile = new DataAccessLayer.PaymentProfile
            {
                Id = Guid.NewGuid(),
                CustomerFirstName = paymentProfileDTO.CustomerFirstName,
                CustomerLastName = paymentProfileDTO.CustomerLastName,
                BillingAddress = paymentProfileDTO.BillingAddress,
                BillingCity = paymentProfileDTO.BillingCity,
                BillingState = paymentProfileDTO.BillingState,
                PostalCode = paymentProfileDTO.Zip,
                BSBNumber = EncryptionService.Encrypt(paymentProfileDTO.RoutingNumber),
                BankAccountNumber = EncryptionService.Encrypt(paymentProfileDTO.AccountNumber),
                CardType = paymentProfileDTO.CardType,
                CardNumber = EncryptionService.Encrypt(paymentProfileDTO.CardNumber),
                CardExpiryMonth = EncryptionService.Encrypt(paymentProfileDTO.ExpirationMonth),
                CardExpiryYear = EncryptionService.Encrypt(paymentProfileDTO.ExpirationYear),
                CardSerurityNumber = EncryptionService.Encrypt(paymentProfileDTO.SecurityCode),
                BankName = paymentProfileDTO.BankName,
                TransactionMode = (byte)paymentProfileDTO.TransactionMode,
                AccountType = paymentProfileDTO.AccountType,
                CreatedBy = System.Configuration.ConfigurationManager.AppSettings["SystemAddedName"],
                CreateDateTime = DateTime.Now                
            };

            return paymentProfile;
        }

        /// <summary>
        /// Create the payment profile object and use the DTO to transform the data
        /// </summary>
        /// <param name="paymentProfileDTO">DTO of the payment profile</param>
        /// <returns>Payment Profile object for DAL</returns>
        private DataAccessLayer.BankAccount DataTransformBankAccountDTO(BankAccountDTO bankAccountDTO)
        {
            var bankAccount = new DataAccessLayer.BankAccount
            {
                BankAccountNumber = EncryptionService.Encrypt(bankAccountDTO.BankAccountNumber),
                BankAccountBSB = EncryptionService.Encrypt(bankAccountDTO.BankAccountBSB),
                BankAccountId = Guid.NewGuid(),
                BankAcountName = bankAccountDTO.BankAcountName,
                FundId = bankAccountDTO.FundId
            };

            return bankAccount;
        }

    }
}
