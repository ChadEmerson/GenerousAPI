// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PaymentToOrganisationListDTO.cs" company="Day3 Solutions">
//   Copyright (c) Day3 Solutions. All rights reserved.
// </copyright>
// <summary>
//  Payment to Organisation List Data Transfer Object - passed between Controller and Business Service Layer
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace GenerousAPI.DataAccessLayer
{
    using System;
    using System.Collections.Generic;


    public class PaymentToOrganisationListDTO
    {
        /// <summary>
        /// Reference to the payment to organisation line item business entity
        /// </summary>
        public PaymentToOrganisationBatchLineItem PaymentToOrganisationBatchLineItem { get; set; }

        /// <summary>
        /// Earliest Transaction Date/Time
        /// </summary>
        public Nullable<DateTime> MinTransDateTime { get; set; }

        /// <summary>
        /// Latest Transaction Date/Time
        /// </summary>
        public Nullable<DateTime> MaxTransDateTime { get; set; }

        /// <summary>
        /// Payment process status string - Apprioved, Declined
        /// </summary>
        public string PaymentProcessStatus { get; set; }

        /// <summary>
        /// Project name the payment relates to
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// Donation transaction info
        /// </summary>
        public IEnumerable<TransactionDetail> LineItemDonationTransactions { get; set; }

        /// <summary>
        /// Transaction payment method info
        /// </summary>
        public IEnumerable<PaymentMethod> TransactionPaymentMethodInfo { get; set; }
    }

    public class TransactionTotal
    {
        public decimal TotalDonations { get; set; }
        public string PaymentMethod { get; set; }
    }

    public class FeeBreakDown
    {
        public decimal TotalTransactionFee { get; set; }

        public decimal TotalTransactionFeePaidByDonor { get; set; }
        public decimal TotalPaymentGatewayFee { get; set; }
        public decimal TotalDirectDebitFee { get; set; }
        public decimal TotalPlatformFee { get; set; }
    }
}
