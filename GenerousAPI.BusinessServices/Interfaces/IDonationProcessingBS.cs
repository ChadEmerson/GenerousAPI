using GenerousAPI.BusinessEntities;
using GenerousAPI.BusinessServices;
using GenerousAPI.DataAccessLayer;
using System;
using System.Collections.Generic;

namespace GenerousAPI.BusinessServices
{
    public interface IDonationProcessingBS
    {
        void ProcessDirectDebitDonations(List<TransactionDetail> donationTransList);

        /// <summary>
        /// Send direct debit file for processing based on AwaitingClearance
        /// </summary>
        /// <param name="donationTransList">Collection of transactions to process</param>
        List<TransactionDetail> CheckDirectDebitDonations_AwaitingClearance(List<TransactionDetail> donationTransList);

        /// <summary>
        /// Queue up donations
        /// </summary>
        void QueueUpRegularDonations(TransactionDetail transactionToQueue);

        void SetDonationTransactionsStatus(TransactionDetail donationTransList, PaymentGatewayProcessing.Helpers.Enums.PaymentProcessStatus paymentProcessStatus, DateTime statusUpdateDateTime, string statusUpdateBy);


        void SetDonationTransactionsStatus(List<TransactionDetail> donationTransList, PaymentGatewayProcessing.Helpers.Enums.PaymentProcessStatus paymentProcessStatus, DateTime statusUpdateDateTime, string statusUpdateBy);
    }

}
