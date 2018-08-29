﻿using GenerousAPI.BusinessEntities;
using GenerousAPI.BusinessServices;
using GenerousAPI.Helpers;
using GenerousAPI.Models;
using PaymentGatewayProcessing.CardAccess;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.Http;

namespace GenerousAPI.Controllers
{
    public class PaymentProcessController : ApiController
    {
        private IPaymentProfileBS _IPaymentProfileBS = null;
        private IPaymentGatewayBS _IPaymentGatewayBS = null;
        private IBankAccountBS _IBankAccountBS = null;
        private ITransactionDetailsBS _ITransactionDetailsBS = null;
        private IPaymentProfileBinInfoBS _IPaymentProfileBinInfoBS = null;
        private IPaymentToOrganisationBS _IPaymentToOrganisationBS = null;

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

                    // Get Card Bin info
                    var binInfo = RetrieveBinInfo(paymentProfileDTO.CardNumber.Substring(0, 8));

                    // Save Binbin info
                    var binInfoDetails = new DataAccessLayer.PaymentProfileBinInfo
                    {
                        BinInfoId = Guid.NewGuid(),
                        PaymentProfileId = paymentProfile.Id,
                        BankName = binInfo.Bank.Name,
                        Brand = binInfo.Brand,
                        CountryCode = binInfo.Country.CountryCode,
                        CountryName = binInfo.Country.CountryName,
                        CardType = binInfo.Type,
                        Latitude = Convert.ToInt32(binInfo.Country.Latitude),
                        Longitude = Convert.ToInt32(binInfo.Country.Longitude),
                        Scheme = binInfo.Scheme
                    };

                    _IPaymentProfileBinInfoBS = new PaymentProfileBinInfoBS();
                    _IPaymentProfileBinInfoBS.CreateBinInfo(binInfoDetails);
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
        public BankVerificationResponse CreateBankAccount([FromBody]BankAccountDTO bankAccountDTO)
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
                var creationResponse = _IBankAccountBS.CreateBankAccount(bankAccount);

                var response = GenerateBankVerificationDonations(bankAccount.BankAccountTokenId);

                // Return a token id
                return response;
            }
            catch (Exception)
            {
                return null;
            }
        }

        [AcceptVerbs("GET")]
        [HttpGet]
        public List<ContactDetailsDTO> GetExpiringCreditCards()
        {
            try
            {
                // Get expiring credit card details member/donor
                var contactDetails = new List<ContactDetailsDTO>();

                // Because credit cards expire at the end of the month, get cards based off this
                var dateIn60Days = DateTime.Now.AddMonths(2);
                var dateIn30Days = DateTime.Now.AddMonths(1);

                // Running on the first of the month, yesterday was last month
                var expired = DateTime.Now.AddDays(-1); 

                // Get expiry for next month
                var paymentProfiles = GetPaymentProfilesWithExpiringCards(dateIn60Days.Month.ToString(), dateIn60Days.Year.ToString());

                // Get expiry for this month
                paymentProfiles.AddRange(GetPaymentProfilesWithExpiringCards(dateIn30Days.Month.ToString(), dateIn30Days.Year.ToString()));

                // Get expired
                paymentProfiles.AddRange(GetPaymentProfilesWithExpiringCards(expired.Month.ToString(), expired.Year.ToString()));

                paymentProfiles = paymentProfiles.Distinct().ToList();

                foreach(var paymentProfile in paymentProfiles)
                {
                    paymentProfile.CardNumberMask = EncryptionService.Decrypt(paymentProfile.CardNumberMask).Substring(12);
                }

                return paymentProfiles;
            }
            catch (Exception)
            {
                return null;
            }
        }
        
        [AcceptVerbs("POST")]
        [HttpPost]
        public ProcessorResponse DeletePaymentProfile(HttpRequestMessage request)
        {
            try
            {
                _IPaymentProfileBS = new PaymentProfileBS();

                // Now pull out the body from the request
                string token = request.Content.ReadAsStringAsync().Result;

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
        public ProcessorResponse DeleteBankAccount(HttpRequestMessage request)
        {
            try
            {
                _IBankAccountBS = new BankAccountBS();

                // Now pull out the body from the request 
                string token = request.Content.ReadAsStringAsync().Result;

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



        [AcceptVerbs("POST")]
        [HttpPost]
        public TransactionDetailsDTO GetTransactionDetails(HttpRequestMessage request)
        {
            try
            {
                _ITransactionDetailsBS = new TransactionDetailsBS();

                // Now pull out the body from the request 
                string token = request.Content.ReadAsStringAsync().Result;

                // Get the transaction details
                var transactionDetails = _ITransactionDetailsBS.GetTransactionDetails(Guid.Parse(token));

                // Return response
                return transactionDetails;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [AcceptVerbs("POST")]
        [HttpPost]
        public IEnumerable<TransactionDetailsDTO> GetTransactionDetailsForBankAccount(HttpRequestMessage request)
        {
            try
            {
                _ITransactionDetailsBS = new TransactionDetailsBS();

                // Now pull out the body from the request 
                string token = request.Content.ReadAsStringAsync().Result;

                // Get the transaction details
                var transactionDetails = _ITransactionDetailsBS.GetTransactionDetailsForBankAccount(token);

                // Return response
                return transactionDetails;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [AcceptVerbs("POST")]
        [HttpPost]
        public void CreatePaymentToOrganisationBatch(HttpRequestMessage request)
        {
            try
            {
                CreatePaymentToOrganisationBatch();
            }
            catch (Exception)
            {
                // Some error occurred
            }
        }

        /// <summary>
        /// Process a refund for the most recent credit card transaction for this person
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [HttpPost]
        public PaymentResponse ProcessRefund(HttpRequestMessage request)
        {
            var refundResponse = new PaymentResponse();

            PaymentGatewayProcessing.PaymentGatewayProcessing paymentGatewayProcessing = null;
            NameValueCollection collection = PaymentGatewayConfigXMLParser.ParseConfigXML(GetGenerousPaymentGatewayDetails((byte)Enums.PaymentGatewayType.GENEROUS).GatewayConfig);
            paymentGatewayProcessing = new ProcessCardAccessPayment(collection);

            try
            {
                PaymentGatewayProcessing.Helpers.RefundRequest cardAccessRefundRequest = new PaymentGatewayProcessing.Helpers.RefundRequest();


                _ITransactionDetailsBS = new TransactionDetailsBS();

                // Now pull out the body from the request 
                string token = request.Content.ReadAsStringAsync().Result;

                // Get the transaction details
                var transactionDetails = _ITransactionDetailsBS.GetTransactionDetails(Guid.Parse(token));
                var paymentProfile = GetPaymentProfileDetails(transactionDetails.PaymentProfileTokenId);

                var auditNumberOfLastTrans = transactionDetails.AuditNumber;

                cardAccessRefundRequest.RefundAuditId = auditNumberOfLastTrans;
                cardAccessRefundRequest.RefundAmount = transactionDetails.Amount;
                cardAccessRefundRequest.DonationTransactionReferenceNumber = transactionDetails.CustomerReference;

                var cardAccessResponse = paymentGatewayProcessing.ProcessRefund(cardAccessRefundRequest);
                
                refundResponse.IsSuccess = cardAccessResponse.TransactionSuccessful;
                refundResponse.Message = cardAccessResponse.ResponseMessage + " " + cardAccessResponse.ResponseText;
                refundResponse.Amount = transactionDetails.Amount;

                var transactionDetail = DataTransformTransactionDetailsDTO(transactionDetails);

                var transactionInformation = CreateTransactionDetailsObject(transactionDetail, refundResponse, TransactionMode.Refund, cardAccessResponse.ResponseCode);

                transactionInformation.CustomerReference = transactionDetails.CustomerReference;
                transactionInformation.AuditNumber = cardAccessResponse.ResponseAuditNumber;
                CreateTransactionRecord(transactionInformation);
                refundResponse.TransactionId = transactionInformation.Id;
            }
            catch (Exception ex)
            {
                refundResponse.IsSuccess = false;
                refundResponse.Message = ex.Message;
            }
        
            return refundResponse;
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

                            paymentResponses.Add(paymentResponse);
                        }
                    }
                    else
                    {
                        var paymentProfile = GetPaymentProfileDetails(transaction.PaymentProfileTokenId);

                        // Convert to cents
                        cardAccessRequest.Amount = transaction.Amount * 100;
                        cardAccessRequest.CardType = paymentProfile.CardType;
                        cardAccessRequest.CardNumber = String.IsNullOrEmpty(paymentProfile.CardNumber) ? paymentProfile.AccountNumber : paymentProfile.CardNumber;
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
                        
                        var transactionInformation = CreateTransactionDetailsObject(transaction, paymentResponse, paymentProfile.TransactionMode, cardAccessResponse.ResponseCode);

                        transactionInformation.CustomerReference = transaction.TransactionReferenceNumber;
                        transactionInformation.AuditNumber = cardAccessResponse.ResponseAuditNumber;
                        CreateTransactionRecord(transactionInformation);
                        paymentResponse.TransactionId = transactionInformation.Id;

                        paymentResponses.Add(paymentResponse);
                    }                    
                }
                catch (Exception ex)
                {
                    var paymentResponse = new PaymentResponse();
                    paymentResponse.IsSuccess = false;
                    paymentResponse.Message = ex.Message;
                    paymentResponse.Amount = transaction.Amount;

                    paymentResponses.Add(paymentResponse);
                }
            }

            return paymentResponses;
        }

        /// <summary>
        /// Returns a batch payment to an organisation
        /// </summary>
        /// <returns>Batch payment to an organisation</returns>
        private DataAccessLayer.PaymentToOrganisationBatch CreatePaymentToOrganisationBatch()
        {
            DataAccessLayer.PaymentToOrganisationBatch batch = new DataAccessLayer.PaymentToOrganisationBatch();
            batch.Id = Guid.NewGuid();
            batch.BatchNumber = BatchNumberHelper.GetBatchNumberForPaymentToOrganisation();
            batch.CreateDateTime = DateTime.Now;
            batch.CreatedBy = PaymentConfiguration.TransactionProcessBatchName;

            _IPaymentToOrganisationBS = new PaymentToOrganisationBS();

            bool areThereAnyAPPROVED_Trans_Not_Assigned_To_Any_Batch = _IPaymentToOrganisationBS.AreThereAny_APPROVED_DonationTransactions_NotAssigned_To_Any_Batch();

            //if there are no APPROVED donation transactions to be batched, then we simply abort the batch creation process
            if (!areThereAnyAPPROVED_Trans_Not_Assigned_To_Any_Batch)
                // return null;

            //create batch record in db
            _IPaymentToOrganisationBS.CreatePaymentToOrganisationBatch(batch);


            // Get batch items so they can be decrypted
            var batchlistDetails = _IPaymentToOrganisationBS.BatchProcessPaymentToOrganisation(Common.BatchPaymentToOrganisationStatus.Unprocessed);
            var batchitems = _IPaymentToOrganisationBS.GetBatchListItems(PaymentGatewayProcessing.Helpers.Enums.PaymentProcessStatus.Unprocessed, batchlistDetails);

            foreach(var batchitem in batchitems)
            {
                batchitem.BankAccountNumber = EncryptionService.Decrypt(batchitem.BankAccountNumber);
                batchitem.BankAccountBSB = EncryptionService.Decrypt(batchitem.BankAccountBSB);
            }

            _IPaymentToOrganisationBS.ProcessPaymentToOrganisationBatches_UnProcessed(batchlistDetails, batchitems, false);


            //_IPaymentToOrganisationBS.ProcessBatchList(Common.BatchPaymentToOrganisationStatus.Unprocessed, batchlistDetails);

            return batch;
        }

        /// <summary>
        /// Creates a verification payment for the batch line items
        /// </summary>
        /// <param name="bankAccountId">Bank account id</param>
        /// <param name="verificationAmounts">Amounts to verify</param>
        private void CreateBankVerificationPaymenBatchLineItems(BankAccountDTO bankAccount, List<decimal> verificationAmounts)
        {
            List<DataAccessLayer.PaymentToOrganisationBatchLineItem> batchLineItemList = new List<DataAccessLayer.PaymentToOrganisationBatchLineItem>();

            //create a new batch to process the bank verification request
            DataAccessLayer.PaymentToOrganisationBatch batch = new DataAccessLayer.PaymentToOrganisationBatch();
            batch.Id = Guid.NewGuid();
            batch.BatchNumber = Helpers.BatchNumberHelper.GetBatchNumberForBankVerification();
            batch.IsBankVerificationBatch = true;
            batch.CreateDateTime = DateTime.Now;
            batch.CreatedBy = PaymentConfiguration.TransactionProcessBatchName;

            foreach (Decimal verificationAmount in verificationAmounts)
            {
                DataAccessLayer.PaymentToOrganisationBatchLineItem donationItem = new DataAccessLayer.PaymentToOrganisationBatchLineItem();
                donationItem.ProcessStatusId = (byte)PaymentGatewayProcessing.Helpers.Enums.PaymentProcessStatus.Unprocessed;
                donationItem.Id = Guid.NewGuid();
                donationItem.BatchId = batch.Id;
                donationItem.BatchNumber = batch.BatchNumber;
                donationItem.IsBankVerification = true;
                
                donationItem.BankAcountName = bankAccount.BankAcountName;
                donationItem.BankAccountNumber = bankAccount.BankAccountNumber;
                donationItem.BankAccountId = bankAccount.BankAccountId;
                
                donationItem.BankAccountBSB = bankAccount.BankAccountBSB;
                donationItem.TotalAmountPaidToOrganisation = verificationAmount;
                donationItem.TotalAmountReceived = 0;
                donationItem.TotalPaymentsReceived = 0;
                donationItem.ProcessDateTime = DateTime.Now;
                donationItem.CreateDateTime = DateTime.Now;
                donationItem.CreatedBy = PaymentConfiguration.TransactionProcessBatchName;

                batchLineItemList.Add(donationItem);
            }

            //assign batch line items to batch
            batch.PaymentToOrganisationBatchLineItems = batchLineItemList;

            _IBankAccountBS = new BankAccountBS();

            _IBankAccountBS.CreatePaymentToOrganisationBatch(batch);            
        }

        /// <summary>
        /// Get the Bin Info from binlist.net for this credit card
        /// </summary>
        /// <param name="BinNumberToSearch"></param>
        /// <returns></returns>
        private IssuerInformation RetrieveBinInfo(string BinNumberToSearch)
        {
            //return BinInfo.BinList.Find(BinNumberToSearch);

            using (WebClient web = new WebClient())
            {
                try
                {
                    string json = web.DownloadString("https://lookup.binlist.net/" + BinNumberToSearch);

                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(IssuerInformation));

                    var issuerInfo =
                        (IssuerInformation)serializer.ReadObject(new MemoryStream(Encoding.Default.GetBytes(json)));

                    return issuerInfo;
                }
                catch (WebException ex)
                {
                    //string addInfo = string.Format("No results for {0}. Make sure you enter a valid BIN/IIN number. --- ", BinNumberToSearch);
                    //throw new WebException(addInfo + ex.Message, ex, ex.Status, ex.Response);
                    return null;
                }
                catch (Exception)
                {
                    throw;
                }
            }

        }

        /// <summary>
        /// Generates a random amount for account verification purposes
        /// </summary>
        /// <param name="tokenId">Unique identifier of the bank account</param>        
        private BankVerificationResponse GenerateBankVerificationDonations(string tokenId)
        {
            _IBankAccountBS = new BankAccountBS();

            var bankAccount = _IBankAccountBS.GetBankAccount(tokenId);

            List<decimal> verificationAmounts = new List<decimal>();

            //generate random amounts less than a dollar
            Random random = new Random();
            verificationAmounts.Add((decimal)random.Next(50, 99) / 100);
            verificationAmounts.Add((decimal)random.Next(50, 99) / 100);

            // Create verification amounts
            CreateBankVerificationPaymenBatchLineItems(bankAccount, verificationAmounts);

            //record bank verification request                
            bankAccount.BankVerificationAmounts = String.Join(",", verificationAmounts.ToArray());
            bankAccount.BankVerificationRequestedOn = DateTime.Now;

            var bankAccountToUpdate = DataTransformBankAccountDTO(bankAccount);

            var response = _IBankAccountBS.UpdateBankAccount(bankAccountToUpdate);

            var bankResponse = new BankVerificationResponse
            {
                Message = response.Message,
                AuthToken = response.AuthToken,
                IsSuccess = response.IsSuccess,
                VerificationAmounts = verificationAmounts,
                BankVerificationRequestedOn = bankAccount.BankVerificationRequestedOn
            };

            return bankResponse;
        }

        /// <summary>
        /// Create the transaction record information
        /// </summary>
        /// <param name="transactionDetails"></param>
        private void CreateTransactionRecord(DataAccessLayer.TransactionDetail transactionDetails)
        {
            try
            {
                _ITransactionDetailsBS = new TransactionDetailsBS();
                _ITransactionDetailsBS.CreateTransactionRecord(transactionDetails);
            }
            catch (Exception ex)
            {
                return;
            }
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
            paymentProfileDTO.ExpirationMonth = paymentProfileDTO.ExpirationMonth;
            paymentProfileDTO.ExpirationYear = paymentProfileDTO.ExpirationYear;
            paymentProfileDTO.SecurityCode = EncryptionService.Decrypt(paymentProfileDTO.SecurityCode);
            return paymentProfileDTO;
        }

        /// <summary>
        /// Get the payment profile details based on the token
        /// </summary>
        /// <param name="ExpiryMonth">Expiring month</param>
        /// <param name="ExpiryYear">Expiring year</param>
        /// <returns>payment profile details</returns>        
        private List<ContactDetailsDTO> GetPaymentProfilesWithExpiringCards(string ExpiryMonth, string ExpiryYear)
        {
            _IPaymentProfileBS = new PaymentProfileBS();

            var paymentProfileDTOs = _IPaymentProfileBS.GetExpiringCards(ExpiryMonth, ExpiryYear);
            
            return paymentProfileDTOs;
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
                CardExpiryMonth =paymentProfileDTO.ExpirationMonth,
                CardExpiryYear = paymentProfileDTO.ExpirationYear,
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
        /// <param name="bankAccountDTO">DTO of the bank account</param>
        /// <returns>Payment Profile object for DAL</returns>
        private DataAccessLayer.BankAccount DataTransformBankAccountDTO(BankAccountDTO bankAccountDTO)
        {
            if (bankAccountDTO.BankVerificationAmounts != null)
            {
                var bankAccount = new DataAccessLayer.BankAccount
                {
                    BankAccountNumber = EncryptionService.Encrypt(bankAccountDTO.BankAccountNumber),
                    BankAccountBSB = EncryptionService.Encrypt(bankAccountDTO.BankAccountBSB),
                    BankAccountId = Guid.NewGuid(),
                    BankAcountName = bankAccountDTO.BankAcountName
                };

                return bankAccount;
            }
            else
            {
                var bankAccount = new DataAccessLayer.BankAccount
                {
                    BankAccountNumber = EncryptionService.Encrypt(bankAccountDTO.BankAccountNumber),
                    BankAccountBSB = EncryptionService.Encrypt(bankAccountDTO.BankAccountBSB),
                    BankAccountId = Guid.NewGuid(),
                    BankAcountName = bankAccountDTO.BankAcountName,
                    BankVerificationAmounts = bankAccountDTO.BankVerificationAmounts,
                    BankVerificationRequestedOn = DateTime.Now
                };

                return bankAccount;
            }
            
        }              

        /// <summary>
        /// Create the transaction detail object and use the DTO to transform the data
        /// </summary>
        /// <param name="transactionDetailsDTO">DTO of the transaction</param>
        /// <returns>Payment Profile object for DAL</returns>
        private TransactionDetails DataTransformTransactionDetailsDTO(TransactionDetailsDTO transactionDetailsDTO)
        {
            var transactionDetail = new TransactionDetails
            {
                Amount = transactionDetailsDTO.Amount,
                BankAccountForFundsTokenId = transactionDetailsDTO.BankAccountTokenId,
                PaymentProfileTokenId = transactionDetailsDTO.PaymentProfileTokenId,
            };

            return transactionDetail;
        }

        /// <summary>
        /// Create the payment profile object and use the DTO to transform the data
        /// </summary>
        /// <param name="paymentProfileDTO">DTO of the payment profile</param>
        /// <returns>Payment Profile object for DAL</returns>
        private DataAccessLayer.TransactionDetail CreateTransactionDetailsObject(TransactionDetails transactionDetails, PaymentResponse paymentResponse, TransactionMode transactionMode, string responseCode)
        {
            var transactionDetail = new DataAccessLayer.TransactionDetail
            {
                Amount = transactionDetails.Amount,
                BankAccountTokenId = transactionDetails.BankAccountForFundsTokenId,
                PaymentProfileTokenId = transactionDetails.PaymentProfileTokenId,
                PaymentMethodId = (byte)transactionMode,
                ProcessDateTime = DateTime.Now,
                ResponseText = paymentResponse.Message,
                ResponseCode = string.IsNullOrEmpty(responseCode) ? "99" : responseCode,
                Id = Guid.NewGuid()
            };

            return transactionDetail;
        }

    }
}
