using GenerousAPI.BusinessEntities;
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
        private IOrganisationFeeProcessingBS _IOrganisationFeeProcessingBS = null;

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

                    // Save card type
                    paymentProfile.CardType = binInfo.Scheme;

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

                var response = GenerateBankVerificationDonations(bankAccount.BankAccountTokenId, bankAccountDTO.BankAccountOrganisationId);

                // Return a token id
                return response;
            }
            catch (Exception)
            {
                return null;
            }
        }

        [AcceptVerbs("POST")]
        [HttpGet]
        public ProcessorResponse ClearExpiringCreditCardInfo()
        {
            // Clear out records from table           
            _IPaymentProfileBS = new PaymentProfileBS();

            return _IPaymentProfileBS.ClearExpiringCreditCardInfo();
        }

        [AcceptVerbs("POST")]
        [HttpGet]
        public List<ContactDetailsDTO> GetExpiringCreditCardInfoForOrganisation(DonorAndOrganisation donorAndOrganisation)
        {
            // Always get last month so it doesn't matter when this is checked
            var expired = DateTime.Now.AddMonths(-1);

            // Get expiring credit cards for an organisation based on id
            _IPaymentProfileBS = new PaymentProfileBS();

            if (donorAndOrganisation.ExpiryMonth == null && donorAndOrganisation.ExpiryYear == null)
            {
                return _IPaymentProfileBS.GetExpiringCreditCardInfoForOrganisation(donorAndOrganisation.OrganisationId);
            }
            else
            {
                // Create expiry based on date passed in
                var expirydate = new DateTime(Convert.ToInt32(donorAndOrganisation.ExpiryYear), Convert.ToInt32(donorAndOrganisation.ExpiryMonth), 1);

                // Expiry date passed in is the same as expired, or prior
                if (expirydate <= expired)
                {
                    return _IPaymentProfileBS.GetExpiringCreditCardInfoForOrganisation(donorAndOrganisation.OrganisationId, expired.Month, expired.Year);
                }
                else
                {                    
                    return _IPaymentProfileBS.GetExpiringCreditCardInfoForOrganisation(donorAndOrganisation.OrganisationId, expirydate.Month, expirydate.Year);
                }
            }
        }

        [AcceptVerbs("POST")]
        [HttpGet]
        public OrganisationFeesDTO GetOrganisationFees(HttpRequestMessage request)
        {
            int organisationId = Convert.ToInt32(request.Content.ReadAsStringAsync().Result);
            _IOrganisationFeeProcessingBS = new OrganisationFeeProcessingBS();

            var orgFeesDetail = new OrganisationFeesDTO();

            // Get organisation standard and promo fees
            var orgFeesDetailCollection = _IOrganisationFeeProcessingBS.GetOrganisationFeeProcesingWithRelatedData(organisationId);

            try
            {
                orgFeesDetail = DataTransformOrgFees(orgFeesDetailCollection, organisationId);

                orgFeesDetail.OrganisationId = organisationId;
                orgFeesDetail.CurrencyCode = orgFeesDetailCollection.OrganisationToProcess.CurrencyCode;

                return orgFeesDetail;
            }
            catch (Exception)
            {
                return null;
            }
        }

        [AcceptVerbs("POST")]
        [HttpGet]
        public ProcessorResponse SetOrganisationFees(OrganisationFeesDTO orgFees)
        {
            int organisationId = orgFees.OrganisationId;
            _IOrganisationFeeProcessingBS = new OrganisationFeeProcessingBS();
            var response = new ProcessorResponse();

            try
            {
                // Create promo billing
                var promoBilling = CreatePromoBilling(orgFees, orgFees.OrganisationId);

                // Create standing billing
                var standardBilling = CreateStandardBilling(orgFees, orgFees.OrganisationId);

                // create fee process
                var feeProcess = CreateFeeProcessing(orgFees, orgFees.OrganisationId);

                // Check if creating new or updating existing
                try
                {
                    var orgFeesDetailCollection = _IOrganisationFeeProcessingBS.GetOrganisationFeeProcesingWithRelatedData(organisationId);

                    if (orgFeesDetailCollection == null)
                    {
                        // New ones
                        _IOrganisationFeeProcessingBS.CreateOrganisationPromoFees(promoBilling);
                        _IOrganisationFeeProcessingBS.CreateOrganisationStandardFees(standardBilling);
                        _IOrganisationFeeProcessingBS.CreateOrganisationFeeProces(feeProcess);
                    }
                    else
                    {
                        // Existing
                        promoBilling.FeeProcessingId = orgFeesDetailCollection.OrganisationPromoFees.FeeProcessingId;
                        standardBilling.FeeProcessingId = orgFeesDetailCollection.OrganisationStandardFees.FeeProcessingId;
                        feeProcess.Id = orgFeesDetailCollection.OrganisationToProcess.Id;

                        _IOrganisationFeeProcessingBS.UpdateOrganisationFeePromoPrices(promoBilling);
                        _IOrganisationFeeProcessingBS.UpdateOrganisationFeeStandardPrices(standardBilling);
                        _IOrganisationFeeProcessingBS.UpdateOrganisationFeeProces(feeProcess);
                    }
                }
                catch (Exception)
                {
                    _IOrganisationFeeProcessingBS.CreateOrganisationPromoFees(promoBilling);
                    _IOrganisationFeeProcessingBS.CreateOrganisationStandardFees(standardBilling);
                    _IOrganisationFeeProcessingBS.CreateOrganisationFeeProces(feeProcess);
                }
                
                response.IsSuccess = true;

            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }

        [AcceptVerbs("POST")]
        [HttpGet]
        public void SaveExpiringCreditCardInfoForDonor(DonorAndOrganisation donorAndOrganisation)
        {
            // Get expiring credit card details member/donor
            var contactDetails = new List<ContactDetailsDTO>();

            var paymentProfile = GetCardExpiryForTokenId(donorAndOrganisation.TokenId);

            // Transform details
            var expiringCardDetails = DataTransformExpiringCCDetails(paymentProfile, donorAndOrganisation.OrganisationId);

            // Save details
            _IPaymentProfileBS = new PaymentProfileBS();

            _IPaymentProfileBS.SaveExpiringCreditCardDetais(expiringCardDetails);
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
                        paymentGateway.IsTest = transaction.IsTest;

                        // Convert to cents
                        cardAccessRequest.Amount = Convert.ToInt64(transaction.Amount * 100);                        
                        cardAccessRequest.AccountNumber = "001030230";
                        cardAccessRequest.AccountName = "Card Access Services";
                        cardAccessRequest.BsbNumber = "123456";
                        cardAccessRequest.DonationTransactionReferenceNumber = "Order #1234";

                        var cardAccessResponse = paymentGatewayProcessing.ProcessDirectDebitDonation(cardAccessRequest);
                                               
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
                        cardAccessRequest.Amount = Convert.ToInt64(transaction.Amount * 100);
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
        private void CreateBankVerificationPaymenBatchLineItems(BankAccountDTO bankAccount, List<decimal> verificationAmounts, int organisationId)
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
                donationItem.OrganisationId = organisationId;
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
        private BankVerificationResponse GenerateBankVerificationDonations(string tokenId, int organisationId)
        {
            _IBankAccountBS = new BankAccountBS();

            var bankAccount = _IBankAccountBS.GetBankAccount(tokenId);

            List<decimal> verificationAmounts = new List<decimal>();

            //generate random amounts less than a dollar
            Random random = new Random();
            verificationAmounts.Add((decimal)random.Next(50, 99) / 100);
            verificationAmounts.Add((decimal)random.Next(50, 99) / 100);

            // Create verification amounts
            CreateBankVerificationPaymenBatchLineItems(bankAccount, verificationAmounts, organisationId);

            //record bank verification request                
            var bankAccountToUpdate = DataTransformBankAccountDTO(bankAccount, false);

            bankAccountToUpdate.BankVerificationAmounts = string.Join(",", verificationAmounts.ToArray());
            bankAccountToUpdate.BankVerificationRequestedOn = DateTime.Now;

            var response = _IBankAccountBS.UpdateBankAccount(bankAccountToUpdate);

            var bankResponse = new BankVerificationResponse
            {
                Message = response.Message,
                AuthToken = response.AuthToken,
                IsSuccess = response.IsSuccess,
                VerificationAmounts = verificationAmounts,
                BankVerificationRequestedOn = bankAccountToUpdate.BankVerificationRequestedOn
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
                // Log error message - email it
                Mail.LogException(ex);
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
        /// Get the payment profile details based on the token
        /// </summary>
        /// <param name="ExpiryMonth">Expiring month</param>
        /// <param name="ExpiryYear">Expiring year</param>
        /// <returns>payment profile details</returns>        
        private ContactDetailsDTO GetCardExpiryForTokenId(string TokenId)
        {
            _IPaymentProfileBS = new PaymentProfileBS();

            var paymentProfileDTOs = _IPaymentProfileBS.GetCardExpiryForTokenId(TokenId);

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
        private DataAccessLayer.ExpiringCreditCardsForOrganisation DataTransformExpiringCCDetails(ContactDetailsDTO contactDetails, int organisationId)
        {
            var expiringCCDetails = new DataAccessLayer.ExpiringCreditCardsForOrganisation
            {
                    ExpiringCCId = Guid.NewGuid(),
                    ExpiryMonth = Convert.ToInt32(contactDetails.ExpiryMonth),
                    ExpiryYear = Convert.ToInt32(contactDetails.ExpiryYear),
                    CardNumberMask = EncryptionService.Decrypt(contactDetails.CardNumberMask).Substring(12),
                    CustomerFirstName = contactDetails.CustomerFirstName,
                    CustomerLastName = contactDetails.CustomerLastName,
                    OrganisationId = organisationId,
                    CardTokenId = contactDetails.TokenId
            };

            return expiringCCDetails;
        }

        /// <summary>
        /// Create the payment profile object and use the DTO to transform the data
        /// </summary>
        /// <param name="bankAccountDTO">DTO of the bank account</param>
        /// <returns>Payment Profile object for DAL</returns>
        private DataAccessLayer.BankAccount DataTransformBankAccountDTO(BankAccountDTO bankAccountDTO, bool encrypt = true)
        {
            if (encrypt)
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
            else
            {
                var bankAccount = new DataAccessLayer.BankAccount
                {
                    BankAccountNumber = bankAccountDTO.BankAccountNumber,
                    BankAccountBSB = bankAccountDTO.BankAccountBSB,
                    BankAccountId = bankAccountDTO.BankAccountId,
                    BankAcountName = bankAccountDTO.BankAcountName,
                    BankAccountTokenId = bankAccountDTO.BankAccountTokenId
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
                OrganisationId = transactionDetailsDTO.OrganisationId
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
                Id = Guid.NewGuid(),
                OrganisationId = transactionDetails.OrganisationId,
                NumberOfEventTickets = transactionDetails.NumberOfEventTickets
            };

            return transactionDetail;
        }

        /// <summary>
        /// Create CreateFeeProcessing
        /// </summary>
        /// <param name="organisationFeeDetails">Fee Details</param>
        /// <param name="organisationId">Organisation id</param>
        /// <returns></returns>
        private DataAccessLayer.OrganisationFeeProcessing CreateFeeProcessing(OrganisationFeesDTO organisationFeeDetails, int organisationId)
        {
            try
            {
                var feeprocessing = new DataAccessLayer.OrganisationFeeProcessing
                {
                    Id = Guid.NewGuid(),
                    OrganisationId = organisationId,
                    IsPromoBilling = organisationFeeDetails.IsPromoBilling,
                    PromoBillingExpiresOn = organisationFeeDetails.PromoBillingExpiresOn,
                    CurrencyCode = organisationFeeDetails.CurrencyCode,
                    IsActive = organisationFeeDetails.IsActive,
                    NextRunDate = organisationFeeDetails.NextRunDate,
                    LastRunDate = organisationFeeDetails.LastRunDate,
                    OrganisationBillDate = organisationFeeDetails.BillDate
                };

                return feeprocessing;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Create Promo Fees
        /// </summary>
        /// <param name="organisationFeeDetails">Fee Details</param>
        /// <param name="organisationId">Organisation id</param>
        /// <returns></returns>
        private DataAccessLayer.OrganisationPromoFee CreatePromoBilling(OrganisationFeesDTO organisationFeeDetails, int organisationId)
        {
            try
            {
                var promoFees = new DataAccessLayer.OrganisationPromoFee
                {
                    FeeProcessingId = Guid.NewGuid(),
                    OrganisationId = organisationId,
                    VisaFee = organisationFeeDetails.VisaFeePromo,
                    VisaMinAmount = organisationFeeDetails.VisaMinAmountPromo,
                    InternationalFee = organisationFeeDetails.InternationalFeePromo,
                    InternationalMinAmount = organisationFeeDetails.InternationalMinAmountPromo,
                    AmexFee = organisationFeeDetails.AmexFeePromo,
                    AmexMinAmount = organisationFeeDetails.AmexMinAmountPromo,
                    DirectDebitFee = organisationFeeDetails.DirectDebitFee,
                    DirectDebitMin = organisationFeeDetails.DirectDebitMin
                };

                return promoFees;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Create Standard Fees
        /// </summary>
        /// <param name="organisationFeeDetails">Fee Details</param>
        /// <param name="organisationId">Organisation id</param>
        /// <returns></returns>
        private DataAccessLayer.OrganisationStandardFee CreateStandardBilling(OrganisationFeesDTO organisationFeeDetails, int organisationId)
        {
            try
            {
                var standardFees = new DataAccessLayer.OrganisationStandardFee
                {
                    FeeProcessingId = Guid.NewGuid(),
                    OrganisationId = organisationId,
                    VisaFee = organisationFeeDetails.VisaFee,
                    VisaMinAmount = organisationFeeDetails.VisaMinAmount,
                    InternationalFee = organisationFeeDetails.InternationalFee,
                    InternationalMinAmount = organisationFeeDetails.InternationalMinAmount,
                    AmexFee = organisationFeeDetails.AmexFee,
                    AmexMinAmount = organisationFeeDetails.AmexMinAmount,
                    DirectDebitFee = organisationFeeDetails.DirectDebitFee,
                    DirectDebitMin = organisationFeeDetails.DirectDebitMin,
                    TextToGiveFee = organisationFeeDetails.TextToGiveFee,
                    SmsReminderFee = organisationFeeDetails.SmsReminderFee,
                    EventTicketBracket1 = organisationFeeDetails.EventTicketBracket1,
                    EventTicketBracket2 = organisationFeeDetails.EventTicketBracket2,
                    EventTicketBracket3 = organisationFeeDetails.EventTicketBracket3,
                    EventTicketBracket4 = organisationFeeDetails.EventTicketBracket4,
                    EventTicketBracket5 = organisationFeeDetails.EventTicketBracket5,
                    RefundFee = organisationFeeDetails.RefundFee,
                    ChargebackFee = organisationFeeDetails.ChargebackFee,
                    GivingModuleFee = organisationFeeDetails.GivingModuleFee,
                    PaymentModuleFee = organisationFeeDetails.PaymentModuleFee,
                    CampaignPortalModuleFee = organisationFeeDetails.CampaignPortalModuleFee,
                    EventModuleFee = organisationFeeDetails.EventModuleFee,
                    SocialMediaModuleFee = organisationFeeDetails.SocialMediaModuleFee,
                    ChurchManSystemModuleFee = organisationFeeDetails.ChurchManSystemModuleFee,
                    TransactionFeeAmount = organisationFeeDetails.TransactionFeeAmount,
                    EventTicketBracket1Fee = organisationFeeDetails.EventTicketBracket1Fee,
                    EventTicketBracket2Fee = organisationFeeDetails.EventTicketBracket2Fee,
                    EventTicketBracket3Fee = organisationFeeDetails.EventTicketBracket3Fee,
                    EventTicketBracket4Fee = organisationFeeDetails.EventTicketBracket4Fee,
                    EventTicketBracket5Fee = organisationFeeDetails.EventTicketBracket5Fee
                };

                return standardFees;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Data transform organisation fees with the data from the database
        /// </summary>
        /// <param name="orgFeesWithRelatedData">Organisation fees collection</param>
        /// <param name="organisationId">Organisation Id</param>
        /// <returns>DTO of organisation fees</returns>
        private OrganisationFeesDTO DataTransformOrgFees(GenerousAPI.DataAccessLayer.OrganisationFeeProcesingWithRelatedData orgFeesWithRelatedData, int organisationId)
        {
            try
            {
                var organisationFeesDetail = new OrganisationFeesDTO
                {
                    TextToGiveFee = orgFeesWithRelatedData.OrganisationStandardFees.TextToGiveFee,
                    SmsReminderFee = orgFeesWithRelatedData.OrganisationStandardFees.SmsReminderFee,
                    EventTicketBracket1 = orgFeesWithRelatedData.OrganisationStandardFees.EventTicketBracket1,
                    EventTicketBracket2 = orgFeesWithRelatedData.OrganisationStandardFees.EventTicketBracket2,
                    EventTicketBracket3 = orgFeesWithRelatedData.OrganisationStandardFees.EventTicketBracket3,
                    EventTicketBracket4 = orgFeesWithRelatedData.OrganisationStandardFees.EventTicketBracket4,
                    EventTicketBracket5 = orgFeesWithRelatedData.OrganisationStandardFees.EventTicketBracket5,
                    RefundFee = orgFeesWithRelatedData.OrganisationStandardFees.RefundFee,
                    ChargebackFee = orgFeesWithRelatedData.OrganisationStandardFees.ChargebackFee,
                    GivingModuleFee = orgFeesWithRelatedData.OrganisationStandardFees.GivingModuleFee,
                    PaymentModuleFee = orgFeesWithRelatedData.OrganisationStandardFees.PaymentModuleFee,
                    CampaignPortalModuleFee = orgFeesWithRelatedData.OrganisationStandardFees.CampaignPortalModuleFee,
                    EventModuleFee = orgFeesWithRelatedData.OrganisationStandardFees.EventModuleFee,
                    SocialMediaModuleFee = orgFeesWithRelatedData.OrganisationStandardFees.SocialMediaModuleFee,
                    ChurchManSystemModuleFee = orgFeesWithRelatedData.OrganisationStandardFees.ChurchManSystemModuleFee,
                    TransactionFeeAmount = orgFeesWithRelatedData.OrganisationStandardFees.TransactionFeeAmount,
                    EventTicketBracket1Fee = orgFeesWithRelatedData.OrganisationStandardFees.EventTicketBracket1Fee,
                    EventTicketBracket2Fee = orgFeesWithRelatedData.OrganisationStandardFees.EventTicketBracket2Fee,
                    EventTicketBracket3Fee = orgFeesWithRelatedData.OrganisationStandardFees.EventTicketBracket3Fee,
                    EventTicketBracket4Fee = orgFeesWithRelatedData.OrganisationStandardFees.EventTicketBracket4Fee,
                    EventTicketBracket5Fee = orgFeesWithRelatedData.OrganisationStandardFees.EventTicketBracket5Fee,
                    VisaFeePromo = orgFeesWithRelatedData.OrganisationPromoFees.VisaFee,
                    VisaMinAmountPromo = orgFeesWithRelatedData.OrganisationPromoFees.VisaMinAmount,
                    InternationalFeePromo = orgFeesWithRelatedData.OrganisationPromoFees.InternationalFee,
                    InternationalMinAmountPromo = orgFeesWithRelatedData.OrganisationPromoFees.InternationalMinAmount,
                    AmexFeePromo = orgFeesWithRelatedData.OrganisationPromoFees.AmexFee,
                    AmexMinAmountPromo = orgFeesWithRelatedData.OrganisationPromoFees.AmexMinAmount,
                    VisaFee = orgFeesWithRelatedData.OrganisationStandardFees.VisaFee,
                    VisaMinAmount = orgFeesWithRelatedData.OrganisationStandardFees.VisaMinAmount,
                    InternationalFee = orgFeesWithRelatedData.OrganisationStandardFees.InternationalFee,
                    InternationalMinAmount = orgFeesWithRelatedData.OrganisationStandardFees.InternationalMinAmount,
                    AmexFee = orgFeesWithRelatedData.OrganisationStandardFees.AmexFee,
                    AmexMinAmount = orgFeesWithRelatedData.OrganisationStandardFees.AmexMinAmount,
                    DirectDebitFee = orgFeesWithRelatedData.OrganisationStandardFees.DirectDebitFee,
                    DirectDebitMin = orgFeesWithRelatedData.OrganisationStandardFees.DirectDebitMin,
                    IsPromoBilling = orgFeesWithRelatedData.OrganisationToProcess.IsPromoBilling.Value,
                    PromoBillingExpiresOn = orgFeesWithRelatedData.OrganisationToProcess.PromoBillingExpiresOn,
                    CurrencyCode = orgFeesWithRelatedData.OrganisationToProcess.CurrencyCode,
                    IsActive = orgFeesWithRelatedData.OrganisationToProcess.IsActive.Value,
                    NextRunDate = orgFeesWithRelatedData.OrganisationToProcess.NextRunDate.Value,
                    LastRunDate = orgFeesWithRelatedData.OrganisationToProcess.LastRunDate == null ? DateTime.MinValue : orgFeesWithRelatedData.OrganisationToProcess.LastRunDate.Value,
                    BillDate = orgFeesWithRelatedData.OrganisationToProcess.OrganisationBillDate.Value
                };

                return organisationFeesDetail;
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
