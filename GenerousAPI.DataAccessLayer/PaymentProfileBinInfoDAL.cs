
namespace GenerousAPI.DataAccessLayer
{
    using System;

    public class PaymentProfileBinInfoDAL : IPaymentProfileBinInfoDAL
    {
        /// <summary>
        /// Creates the bin info for the payment profile for a credit card
        /// </summary>
        /// <param name="BinInfo">Bin info DTO details</param>
        /// <returns>True if successfully added, false otherwise</returns>
        public bool CreateBinInfo(PaymentProfileBinInfo BinInfo)
        {
            try
            {
                using (var db = new GenerousAPIEntities())
                {
                    db.PaymentProfileBinInfoes.Add(BinInfo);
                    db.SaveChanges();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
