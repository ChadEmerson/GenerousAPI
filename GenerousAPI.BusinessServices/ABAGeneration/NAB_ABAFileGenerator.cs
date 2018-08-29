// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NAB_ABAFileGenerator.cs" company="Day3 Solutions">
//   Copyright (c) Day3 Solutions. All rights reserved.
// </copyright>
// <summary>
//  NAB ABA file generator class
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GenerousAPI.BusinessServices.ABAGeneration
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using static BusinessEntities.Common;

    public class NAB_ABAFileGenerator : ABAFileGenerator
    {
        #region private Ctor

        private NAB_ABAFileGenerator()
        {

        }

        #endregion

        #region public Ctor

        public NAB_ABAFileGenerator(ABAMode mode)
        {
            _mode = mode;
        }

        #endregion

        #region protected methods

        /// <summary>
        /// Generates header
        /// </summary>
        /// <returns></returns>
        protected override String GenerateHeader()
        {
            const String RECORD_TYPE = "0";
            const String REEL_SEQUENCE_NUMBER = "01";
            String FILE_DESCRIPTION_TWELVE_CHARS = String.Empty;
            String DIRECT_ENTRY_USERID = String.Empty;

            if (this.Mode == ABAMode.DirectDebit)
            {
                FILE_DESCRIPTION_TWELVE_CHARS = "DIRECT DEBIT";
                DIRECT_ENTRY_USERID = AbaConfig.NAB_APCAUserID.PadLeft(6, '0');
            }
            else if (this.Mode == ABAMode.DirectCredit)
            {
                FILE_DESCRIPTION_TWELVE_CHARS = "Donations   ";
                DIRECT_ENTRY_USERID = AbaConfig.NAB_DeUserID.PadLeft(6, '0');
                //DIRECT_ENTRY_USERID = "000000";
            }

            StringBuilder sb = new StringBuilder();

            sb.Append(RECORD_TYPE); //Record Type
            sb.Append(AbaConfig.Space(17)); //17 blank spaces
            sb.Append(REEL_SEQUENCE_NUMBER); //Reel sequence number
            sb.Append(AbaConfig.NAB_BankShortName); //User financial institution
            sb.Append(AbaConfig.Space(7)); //7 blakn spaces
            sb.Append(AbaConfig.NAB_APCAName.PadRight(26)); //Direct Entry User Name
            sb.Append(DIRECT_ENTRY_USERID); //Direct Entry User ID
            sb.Append(FILE_DESCRIPTION_TWELVE_CHARS); //File description
            sb.Append(DateTime.Today.ToString("ddMMyy")); //Value date
            sb.Append(AbaConfig.Space(40)); //40 blank spaces
            sb.Append(Environment.NewLine); //Carriage Return + Line Feed

            return sb.ToString();
        }

        /// <summary>
        /// Generates footer
        /// </summary>
        /// <returns></returns>
        protected override String GenerateFooter()
        {
            const String RECORD_TYPE = "7";
            const String BSB = "999-999";

            StringBuilder sb = new StringBuilder();

            sb.Append(RECORD_TYPE); //Record Type
            sb.Append(BSB); //BSB
            sb.Append(AbaConfig.Space(12)); //12 blank spaces
            sb.Append(this.TotalNetAmountInCents.ToString().PadLeft(10, '0')); //File net total amount
            sb.Append(this.TotalCreditAmountInCents.ToString().PadLeft(10, '0')); //File credit total amount
            sb.Append(this.TotalDebitAmountInCents.ToString().PadLeft(10, '0')); //File debit total amount
            sb.Append(AbaConfig.Space(24)); //24 blank spaces
            sb.Append(this.TotalRecordCount.ToString().PadLeft(6, '0')); //File total count of record Type 1
            sb.Append(AbaConfig.Space(40)); //40 blank spaces
            sb.Append(Environment.NewLine); //Carriage Return + Line Feed

            return sb.ToString();
        }

        /// <summary>
        /// Genarates item details for a ABA file 
        /// </summary>
        /// <param name="donationTransactions"></param>
        /// <returns>generated details for the line</returns>
        protected override String GenerateDetails(List<DataAccessLayer.DonationTransactionWithRelatedData> donationTransactions)
        {

            List<ABAFileDetailItem> detailLineItemList = new List<ABAFileDetailItem>();
            foreach (DataAccessLayer.DonationTransactionWithRelatedData transWithRelatedData in donationTransactions)
            {
                try
                {
                    ABAFileDetailItem detailLineItem = new ABAFileDetailItem();
                    detailLineItem.Amount = transWithRelatedData.TransactionDetail.Amount;
                    detailLineItem.RecepientBSBNumber = transWithRelatedData.DonorPaymentProfile.BSBNumber;
                    detailLineItem.RecepientAccountNumber = transWithRelatedData.DonorPaymentProfile.BankAccountNumber;
                    detailLineItem.RecepientAccountName = transWithRelatedData.DonorPaymentProfile.BankAccountName;
                    detailLineItem.LodgementReference = transWithRelatedData.TransactionDetail.CustomerReference;

                    //var OrgAndCampaignTrimmedString = string.Empty;
                    //var orgNameTrim = (transWithRelatedData.Organisation.NameInDonorStatement.Length >= 5) ? 
                    //    transWithRelatedData.Organisation.NameInDonorStatement.Substring(0, 5) : transWithRelatedData.Organisation.NameInDonorStatement;
                    //var campaignTrim = (transWithRelatedData.Project.CustomerCampaignId.Length >= 6) ? 
                    //    transWithRelatedData.Project.CustomerCampaignId.Substring(0, 6) : transWithRelatedData.Project.CustomerCampaignId;

                    //OrgAndCampaignTrimmedString = orgNameTrim + " " + campaignTrim + transWithRelatedData.DonationTransaction.TransactionReferenceNumber;
                    //detailLineItem.LodgementReference = OrgAndCampaignTrimmedString;

                    detailLineItem.RemitterName = AbaConfig.NAB_Remitter;

                    detailLineItemList.Add(detailLineItem);
                }
                catch (Exception ex)
                {
                    // Failed to add item to detail line item list
                    //Helper.LogException(ex);
                }
            }

            return GenerateDetailLines(detailLineItemList);
        }

        /// <summary>
        /// Genarates item details for a ABA file 
        /// </summary>
        /// <param name="batchLineItems"></param>
        /// <returns>generated details for the line</returns>
        protected override String GenerateDetails(List<DataAccessLayer.PaymentToOrganisationBatchLineItem> batchLineItems)
        {
            List<ABAFileDetailItem> detailLineItemList = new List<ABAFileDetailItem>();

            foreach (DataAccessLayer.PaymentToOrganisationBatchLineItem batchLineItem in batchLineItems)
            {
                // Get bank details - decrypt then

                ABAFileDetailItem detailLineItem = new ABAFileDetailItem();
                detailLineItem.Amount = batchLineItem.TotalAmountPaidToOrganisation;
                detailLineItem.RecepientBSBNumber = batchLineItem.BankAccountBSB;
                detailLineItem.RecepientAccountNumber = batchLineItem.BankAccountNumber;
                detailLineItem.RecepientAccountName = batchLineItem.BankAcountName;
                detailLineItem.LodgementReference = string.Format("{0}0{1}", batchLineItem.BatchNumber, batchLineItem.LineItemNumber);
                detailLineItem.RemitterName = AbaConfig.NAB_Remitter;
                detailLineItemList.Add(detailLineItem);
            }

            return GenerateDetailLines(detailLineItemList);
        }


        //protected override String GenerateDetails(List<Day3.GivingPortal.BusinessEntities.DataTransferObjects.OrganisationFeeProcesingWithRelatedData> organisationFees)
        //{
        //    List<ABAFileDetailItem> detailLineItemList = new List<ABAFileDetailItem>();

        //    foreach (Day3.GivingPortal.BusinessEntities.DataTransferObjects.OrganisationFeeProcesingWithRelatedData organisationItem in organisationFees)
        //    {
        //        ABAFileDetailItem detailLineItem = new ABAFileDetailItem();
        //        detailLineItem.Amount = organisationItem.TotalAmountToPay;
        //        //detailLineItem.RecepientBSBNumber = batchLineItem.BSBNumber;
        //        //detailLineItem.RecepientAccountNumber = batchLineItem.AccountNumber;
        //        //detailLineItem.RecepientAccountName = batchLineItem.AccountName;
        //        //detailLineItem.LodgementReference = batchLineItem.TransactionReferenceNumber;
        //        detailLineItem.RemitterName = Config.NAB_Remitter;
        //        detailLineItemList.Add(detailLineItem);
        //    }

        //    return GenerateDetailLines(detailLineItemList);
        //}


        private void ProcessTransactionFee()
        {

        }

        #endregion

        #region private methods

        /// <summary>
        /// Genarates item details for a ABA file 
        /// </summary>
        /// <param name="lineItems"></param>
        /// <returns>generated details for the line</returns>
        private string GenerateDetailLines(List<ABAFileDetailItem> lineItems)
        {
            const String RECORD_TYPE = "1";
            const String INDICATOR = " ";
            String TRANSACTION_CODE = String.Empty;

            if (this.Mode == ABAMode.DirectDebit)
                TRANSACTION_CODE = "13";
            else if (this.Mode == ABAMode.DirectCredit)
                TRANSACTION_CODE = "50";


            StringBuilder sb = new StringBuilder();
            int AmountOfWithholdingTax = 0;

            foreach (ABAFileDetailItem lineItem in lineItems)
            {
                String spacer = " ";

                //run validations
                if (!String.IsNullOrEmpty(lineItem.RecepientAccountNumber) && lineItem.RecepientAccountNumber.Length > 9)
                    throw new ApplicationException(String.Format("Bank Account number {0} of transaction : {1} has exceeded it's limit of 9 digits", lineItem.RecepientAccountNumber, lineItem.LodgementReference));

                // Check for 0 dollar amount
                long amountInCents = Convert.ToInt64(lineItem.Amount * 100);

                if (amountInCents != 0)
                {
                    sb.Append(RECORD_TYPE); //Record Type
                    sb.Append(lineItem.RecepientBSBNumber.Insert(3, "-").PadLeft(7)); //7 chars BSB Number in xxx-xxx format
                    sb.Append(lineItem.RecepientAccountNumber.ToString().PadLeft(9)); //9 digit credit/debit account number
                    sb.Append(INDICATOR); //Indicator
                    sb.Append(TRANSACTION_CODE); //Transaction code
                    sb.Append(amountInCents.ToString().PadLeft(10, '0')); //Amount
                    if (!String.IsNullOrEmpty(lineItem.RecepientAccountName))
                        sb.Append(lineItem.RecepientAccountName.PadRight(32)); //Account Name
                    else
                        sb.Append(spacer.PadRight(32)); //Account Name
                    sb.Append(lineItem.LodgementReference.PadRight(18)); //Lodgement reference
                    sb.Append(AbaConfig.NAB_TraceBSB.Insert(3, "-")); //Trace BSB in xxx-xxx format
                    sb.Append(AbaConfig.NAB_TraceAccountNumber.Substring(0, 9).PadLeft(9)); //9 digit trace account number
                    sb.Append(lineItem.RemitterName.PadRight(16)); //name of remitter
                    sb.Append(AmountOfWithholdingTax.ToString().PadLeft(8, '0')); //amount of withholding tax
                    sb.Append(Environment.NewLine); //Carriage Return + Line Feed

                    ++this.TotalRecordCount; //increment record counter
                    this.TotalDebitAmountInCents += amountInCents; //calculate total amount
                }
            }

            this.TotalCreditAmountInCents = this.TotalDebitAmountInCents;
            this.TotalNetAmountInCents = this.TotalDebitAmountInCents - this.TotalCreditAmountInCents;

            return sb.ToString();
        }

        #endregion

    }
}
