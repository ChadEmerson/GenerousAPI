﻿namespace GenerousAPI.BusinessServices
{
    using DataAccessLayer;
    using GenerousAPI.BusinessEntities;

    public class PaymentProfileBS : IPaymentProfileBS
    {
        private IPaymentProfileDAL _IPaymentProfileDAL = null;
        
        /// <summary>
        /// Ctor
        /// </summary>
        public PaymentProfileBS()
        {
            _IPaymentProfileDAL = new PaymentProfileDAL();
        }

        /// <summary>
        /// Create a new donor payment profile for a donor
        /// </summary>
        /// <param name="PaymentProfile">Donor payment profile details</param>
        /// <returns>Payment Response with success, message, and profile token</returns>
        public ProcessorResponse CreatePaymentProfile(PaymentProfile PaymentProfile)
        {
            return _IPaymentProfileDAL.CreatePaymentProfile(PaymentProfile);
        }

        /// <summary>
        /// Update existing donor payment profile for a donor
        /// </summary>
        /// <param name="PaymentProfile">Donor payment profile details</param>
        /// <returns>Payment Response with success, message, and profile token</returns>
        public ProcessorResponse UpdatePaymentProfile(PaymentProfile PaymentProfile)
        {
            return _IPaymentProfileDAL.UpdatePaymentProfile(PaymentProfile);
        }

        /// <summary>
        /// Get donors payment profile details
        /// </summary>
        /// <param name="paymentProfileTokenId">Payment profile token id</param>
        /// <returns>Donor payment profile details</returns>
        public PaymentProfileDTO GetPaymentProfile(string paymentProfileTokenId)
        {
            return _IPaymentProfileDAL.GetPaymentProfile(paymentProfileTokenId);
        }

        /// <summary>
        /// Delete a payment profile for a donor
        /// </summary>
        /// <param name="paymentProfileTokenId">Payment profile token id</param>
        /// <returns>Payment Response with success, message, and profile token</returns>
        public ProcessorResponse DeletePaymentProfile(string paymentProfileTokenId)
        {
            return _IPaymentProfileDAL.DeletePaymentProfile(paymentProfileTokenId);
        }
    }
}