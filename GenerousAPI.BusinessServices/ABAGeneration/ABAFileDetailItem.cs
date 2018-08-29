using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerousAPI.BusinessServices.ABAGeneration
{
    public class ABAFileDetailItem
    {
        #region public propertis

        /// <summary>
        /// Donation amount
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Recepient BSB number
        /// </summary>
        public string RecepientBSBNumber { get; set; }

        /// <summary>
        /// Recepient account number
        /// </summary>
        public string RecepientAccountNumber { get; set; }

        /// <summary>
        /// Recepient account name
        /// </summary>
        public string RecepientAccountName { get; set; }

        /// <summary>
        /// Transaction reference nmber
        /// </summary>
        public string LodgementReference { get; set; }

        /// <summary>
        /// Name in donor statement
        /// </summary>
        public string RemitterName { get; set; }

        #endregion
    }
}
