// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigXMLParser.cs" company="Day3 Solutions">
//   Copyright (c) Day3 Solutions. All rights reserved.
// </copyright>
// <summary>
//  Config XML Parser class
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace GenerousAPI.Helpers
{
    using System;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Xml.Linq;

    /// <summary>
    /// This parser expects the config xml structure like:
    /// <PaymentGatewayConfig>
    ///    <NetRegistryPaymentGatewayURL>https://paygate.ssllock.net/external2.pl</NetRegistryPaymentGatewayURL>
    ///    <NetRegistryPaymentGatewayLogin>2307</NetRegistryPaymentGatewayLogin>
    ///    <NetRegistryPaymentGatewayPassword>mxHIQGGIvGQIRzm8</NetRegistryPaymentGatewayPassword>
    ///  </PaymentGatewayConfig>
    /// </summary>
    /// 
    public class PaymentGatewayConfigXMLParser
    {
        #region public methods

        /// <summary>
        /// Parse a config file
        /// </summary>
        /// <param name="configXML">config XML file in string format</param>
        /// <returns>name value collection</returns>
        public static NameValueCollection ParseConfigXML(String configXML)
        {
            NameValueCollection configEntriesCollection = new NameValueCollection();

            XDocument doc = XDocument.Parse(configXML);

            var configEntries = doc.Descendants("PaymentGatewayConfig").Elements().Select(config => new
            {
                Name = config.Name.ToString(),
                Value = config.Value
            }).ToList();

            foreach (var configEntry in configEntries)
            {
                configEntriesCollection.Add(configEntry.Name, configEntry.Value);
            }

            return configEntriesCollection;
        }

        #endregion

    }
}