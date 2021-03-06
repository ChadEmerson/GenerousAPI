﻿using GenerousAPI.BusinessEntities;
using GenerousAPI.DataAccessLayer;
using System;
using System.Collections.Generic;

namespace GenerousAPI.BusinessServices
{
    public class PaymentGatewayBS : IPaymentGatewayBS
    {
        /// <summary>
        /// Reference to the donation interface
        /// </summary>
        private IPaymentGatewayDAL _IPaymentGatewayDAL = null;

        // <summary>
        /// Ctor
        /// </summary>
        public PaymentGatewayBS()
        {
            _IPaymentGatewayDAL = new PaymentGatewayDAL();
        }


        /// <summary>
        /// Gets payment gateway details for an organisation
        /// </summary>
        /// <param name="organisationId">Organisation ID</param>
        /// <param name="gatewayType">Gateway type</param>
        /// <returns>Payment gateway details for an organisation</returns>
        public IEnumerable<PaymentGatewayDTO> GetPaymentGatewayDetails(byte gatewayType)
        {
            return _IPaymentGatewayDAL.GetPaymentGatewayDetails(gatewayType);
        }
    }
}
