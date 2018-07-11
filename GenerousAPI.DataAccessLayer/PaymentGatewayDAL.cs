using GenerousAPI.BusinessEntities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GenerousAPI.DataAccessLayer.Interfaces
{
    public class PaymentGatewayDAL : IPaymentGatewayDAL
    {
        /// <summary>
        /// Gets payment gateway details for an organisation
        /// </summary>
        /// <param name="organisationId">Organisation ID</param>
        /// <param name="gatewayType">Gateway type</param>
        /// <returns>Payment gateway details for an organisation</returns>
        public IEnumerable<PaymentGatewayDTO> GetPaymentGatewayDetails(Guid organisationId, byte gatewayType)
        {
            using (var db = new GenerousAPIEntities())
            {
                if (organisationId == Guid.Empty)
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
                else
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
}
