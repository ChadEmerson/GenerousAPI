﻿using GenerousAPI.BusinessEntities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GenerousAPI.DataAccessLayer
{
    public class PaymentGatewayDAL : IPaymentGatewayDAL
    {
        /// <summary>
        /// Gets payment gateway details for an organisation
        /// </summary>
        /// <param name="gatewayType">Gateway type</param>
        /// <returns>Payment gateway details for an organisation</returns>
        public IEnumerable<PaymentGatewayDTO> GetPaymentGatewayDetails(byte gatewayType)
        {
            using (var db = new GenerousAPIEntities())
            {
                var result = (from paymentgateway in db.PaymentGatewayConfigs
                                where paymentgateway.GatewayTypeId == gatewayType
                                select new PaymentGatewayDTO()
                                {
                                    id = paymentgateway.Id,
                                    GatewayTypeId = paymentgateway.GatewayTypeId,
                                    GatewayConfig = paymentgateway.GatewayConfig,
                                    IsActive = paymentgateway.IsActive
                                }).ToList();
                return result;
            }
        }
    }
}
