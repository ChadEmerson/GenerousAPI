using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace GenerousAPI.Helpers
{
    public class BatchNumberHelper
    {

        /// <summary>
        /// Generates a batch reference number
        /// </summary>
        /// <returns>A randomly generated batch reference number</returns>
        public static string GetBatchNumberForPaymentToOrganisation()
        {
            return PaymentConfiguration.PaymentToOrganisationBatchNumberPrefix + GetUniqueKey(6);
        }

        /// <summary>
        /// Generates a batch reference number for bank verification
        /// </summary>
        /// <returns>A randomly generated batch reference number</returns>
        public static string GetBatchNumberForBankVerification()
        {
            return PaymentConfiguration.BankVerificationBatchNumberPrefix + GetUniqueKey(6);
        }

        /// <summary>
        /// Generate a key that is reasonably unique for transaction reference numbers 
        /// </summary>
        /// <param name="maxSize">Max size of new unique string</param>
        /// <returns>Unique string</returns>
        public static string GetUniqueKey(int maxSize)
        {
            char[] chars = new char[62];
            chars =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            byte[] data = new byte[1];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetNonZeroBytes(data);
                data = new byte[maxSize];
                crypto.GetNonZeroBytes(data);
            }
            StringBuilder result = new StringBuilder(maxSize);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }
    }
}