using GenerousAPI.DataAccessLayer;

namespace GenerousAPI.BusinessServices
{
    public class PaymentProfileBinInfoBS : IPaymentProfileBinInfoBS
    {
        private IPaymentProfileBinInfoDAL _IPaymentProfileBinInfoDAL = null;

        /// <summary>
        /// Ctor
        /// </summary>
        public PaymentProfileBinInfoBS()
        {
            _IPaymentProfileBinInfoDAL = new PaymentProfileBinInfoDAL();
        }


        /// <summary>
        /// Creates the bin info for the payment profile for a credit card
        /// </summary>
        /// <param name="BinInfo">Bin info DTO details</param>
        /// <returns>True if successfully added, false otherwise</returns>
        public bool CreateBinInfo(PaymentProfileBinInfo BinInfo)
        {
            return _IPaymentProfileBinInfoDAL.CreateBinInfo(BinInfo);
        }
    }
}
