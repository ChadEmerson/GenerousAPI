// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ABAFileGenerator.cs" company="Day3 Solutions">
//   Copyright (c) Day3 Solutions. All rights reserved.
// </copyright>
// <summary>
//  ABA File Generator class
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GenerousAPI.BusinessServices.ABAGeneration
{
    using BusinessEntities;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public abstract class ABAFileGenerator
    {
        #region protected fields

        protected Common.ABAMode _mode;

        #endregion

        #region protectecd properties

        protected long TotalNetAmountInCents { get; set; }
        protected long TotalCreditAmountInCents { get; set; }
        protected long TotalDebitAmountInCents { get; set; }
        protected int TotalRecordCount { get; set; }
        protected Common.ABAMode Mode { get { return _mode; } }

        #endregion

        #region protected methods

        /// <summary>
        /// Generates header
        /// </summary>
        protected abstract String GenerateHeader();

        /// <summary>
        /// Generates details
        /// </summary>
        /// <param name="donationTransactions">Data related to donation transaction</param>
        protected abstract String GenerateDetails(List<DataAccessLayer.DonationTransactionWithRelatedData> donationTransactions);

        /// <summary>
        /// Generates details
        /// </summary>
        /// <param name="batchLineItems">Data related to payment to organisation batch line items</param>
        protected abstract String GenerateDetails(List<DataAccessLayer.PaymentToOrganisationBatchLineItem> batchLineItems);

        ///// <summary>
        ///// Generates header
        ///// </summary>
        //protected abstract void FeeProcessing();

        //protected abstract String GenerateDetails(List<BusinessEntities.DataTransferObjects.OrganisationFeeProcesingWithRelatedData> organisationFees);

        /// <summary>
        /// Generates footer
        /// </summary>
        protected abstract String GenerateFooter();
        
        /// <summary>
        /// For donation transactions received by Day3
        /// </summary>
        /// <param name="donationTransactions"></param>
        /// <param name="filePath"></param>
        /// <param name="fileName"></param>
        public void GenerateABAFile(List<DataAccessLayer.DonationTransactionWithRelatedData> donationTransactions, String filePath, String fileName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(GenerateHeader());
            sb.Append(GenerateDetails(donationTransactions));
            sb.Append(GenerateFooter());

            using (StreamWriter file = new StreamWriter(Path.Combine(filePath, fileName)))
            {
                file.Write(sb.ToString());
            }

        }

        /// <summary>
        /// For donations to be given from Day3 to Organisations 
        /// </summary>
        /// <param name="batchLineItems"></param>
        /// <param name="filePath"></param>
        /// <param name="fileName"></param>
        public void GenerateABAFile(List<DataAccessLayer.PaymentToOrganisationBatchLineItem> batchLineItems, string filePath, string fileName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(GenerateHeader());
            sb.Append(GenerateDetails(batchLineItems));
            sb.Append(GenerateFooter());

            using (StreamWriter file = new StreamWriter(Path.Combine(filePath, fileName)))
            {
                file.Write(sb.ToString());
            }
        }

        /// <summary>
        /// Organisation fee processing
        /// </summary>
        /// <param name="donationTransactions"></param>
        /// <param name="filePath"></param>
        /// <param name="fileName"></param>
        //public void GenerateABAFile(List<Day3.GivingPortal.BusinessEntities.DataTransferObjects.OrganisationFeeProcesingWithRelatedData> OrgFeeItems, String filePath, String fileName)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    sb.Append(GenerateHeader());
        //    sb.Append(GenerateDetails(OrgFeeItems));
        //    sb.Append(GenerateFooter());

        //    using (StreamWriter file = new StreamWriter(Path.Combine(filePath, fileName)))
        //    {
        //        file.Write(sb.ToString());
        //    }

        //}

        #endregion
    }
}
