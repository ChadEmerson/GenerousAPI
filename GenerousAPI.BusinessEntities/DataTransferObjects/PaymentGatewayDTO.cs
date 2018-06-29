// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OrganisationPaymentGatewayDTO.cs" company="Day3 Solutions">
//   Copyright (c) Day3 Solutions. All rights reserved.
// </copyright>
// <summary>
//  Organisation Payment Gateway(s) Data Transfer Object - passed between Controller and Business Service Layer
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace GenerousAPI.BusinessEntities
{
    using System;

    public class PaymentGatewayDTO
    {
        /// <summary>
        /// Unique identifier of payment gateway
        /// </summary>
        public Guid? id { get; set; }

        /// <summary>
        /// Payment gateway type
        /// </summary>
        public Byte GatewayTypeId { get; set; }

        /// <summary>
        /// Payment gateway config details
        /// </summary>
        public string GatewayConfig { get; set; }

        /// <summary>
        /// Payment gateway config details
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Payment gateway is the defualt generous Gateway
        /// </summary>
        public bool GenerousDefaultGateway { get; set; }
        
    }
}
