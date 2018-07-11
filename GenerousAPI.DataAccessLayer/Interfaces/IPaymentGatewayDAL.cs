using GenerousAPI.BusinessEntities;
using System;
using System.Collections.Generic;

namespace GenerousAPI.DataAccessLayer.Interfaces
{
    public interface IPaymentGatewayDAL
    {
        /// <summary>
        /// Gets payment gateway details for an organisation
        /// </summary>
        /// <param name="organisationId">Organisation ID</param>
        /// <param name="gatewayType">Gateway type</param>
        /// <returns>Payment gateway details for an organisation</returns>
        IEnumerable<PaymentGatewayDTO> GetPaymentGatewayDetails(Guid organisationId, byte gatewayType);
    }
}
