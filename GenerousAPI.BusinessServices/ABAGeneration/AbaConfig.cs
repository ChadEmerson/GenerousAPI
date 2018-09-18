using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerousAPI.BusinessServices.ABAGeneration
{
    public class AbaConfig
    {
        /// <summary>
        /// Name of the batch process that runs for processing transactions
        /// </summary>
        public static string TransactionProcessBatchName
        {
            get { return (System.Configuration.ConfigurationManager.AppSettings["TransactionProcessBatchName"] != null ? System.Configuration.ConfigurationManager.AppSettings["TransactionProcessBatchName"] : string.Empty); }
        }

        /// <summary>
        /// Adds the specified number of blank spaces to a string
        /// </summary>
        /// <param name="count">Number of blank spaces to add</param>
        /// <returns>String value with added spaces</returns>
        public static string Space(int count)
        {
            return String.Empty.PadLeft(count);
        }

        /// <summary>
        /// Max number of retries allowed for processing a donation
        /// </summary>
        public static byte MaxRetries
        {
            get { return (System.Configuration.ConfigurationManager.AppSettings["MaxRetries"] != null ? Convert.ToByte(System.Configuration.ConfigurationManager.AppSettings["MaxRetries"]) : (byte)0); }
        }

        /// <summary>
        /// Length of time in days before processing a retry
        /// </summary>
        public static byte RetryPeriodInDays
        {
            get { return (System.Configuration.ConfigurationManager.AppSettings["RetryPeriodInDays"] != null ? Convert.ToByte(System.Configuration.ConfigurationManager.AppSettings["RetryPeriodInDays"]) : (byte)0); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static string ABA_ProcessingStaffEmailList
        {
            get
            {
                return (System.Configuration.ConfigurationManager.AppSettings["ABA_ProcessingStaffEmailList"]);
            }
        }

        #region "ABA config"
        /// <summary>
        /// Bank short name for payments
        /// </summary>
        public static string NAB_BankShortName
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["NAB_BankShortName"]; }
        }

        /// <summary>
        /// Australian Payments Clearing Association name for payments
        /// </summary>
        public static string NAB_APCAName
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["NAB_APCAName"]; }
        }

        /// <summary>
        /// Australian Payments Clearing Association user ID for payments
        /// </summary>
        public static string NAB_APCAUserID
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["NAB_APCAUserID"]; }
        }

        public static string NAB_DeUserID
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["NAB_DeUserID"]; }
        }

        /// <summary>
        /// Bank Trace BSB number
        /// </summary>
        public static string NAB_TraceBSB
        {
            get
            {
                var bsb = System.Configuration.ConfigurationManager.AppSettings["NAB_TraceBSB"];
                if (bsb != null)
                    return bsb.ToString().Replace("-", "");
                else
                    throw new ApplicationException("BSB Number not found");
            }
        }

        /// <summary>
        /// Bank Trace Account number
        /// </summary>
        public static string NAB_TraceAccountNumber
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["NAB_TraceAccountNumber"]; }
        }

        /// <summary>
        /// HeartBurst Bank Trace BSB number
        /// </summary>
        public static string HeartBurst_TraceBSB
        {
            get
            {
                var bsb = System.Configuration.ConfigurationManager.AppSettings["HEARTBURST_TraceBSB"];
                if (bsb != null)
                    return bsb.ToString().Replace("-", "");
                else
                    throw new ApplicationException("BSB Number not found");
            }
        }

        /// <summary>
        /// Unique Id of an anonymous donor relative to the organisation Id
        /// </summary>
        public static Guid HeartBurst_OrganisationId
        {
            get { return Guid.Parse(System.Configuration.ConfigurationManager.AppSettings["HeartBurst_OrganisationId"]); }
        }

        /// <summary>
        /// HeartBurst Bank Trace Account number
        /// </summary>
        public static string HeartBurst_TraceAccountNumber
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["HEARTBURST_TraceAccountNumber"]; }
        }

        /// <summary>
        /// Bank remitter name
        /// </summary>
        public static string NAB_Remitter
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["NAB_Remitter"]; }
        }


        /// <summary>
        /// Bank clearance period for payments
        /// </summary>
        public static double ABA_ClearancePeriodInDays
        {
            get { return Convert.ToDouble(System.Configuration.ConfigurationManager.AppSettings["ABA_ClearancePeriodInDays"]); }
        }

        /// <summary>
        /// ABA file prefix info for payments
        /// </summary>
        public static string ABAPaymentCollectionFilePrefix
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["ABAPaymentCollectionFilePrefix"]; }
        }

        /// <summary>
        /// ABA file path
        /// </summary>
        public static string ABAPaymentCollectionFilePath
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["ABAPaymentCollectionFilePath"]; }
        }

        #endregion
    }
}
