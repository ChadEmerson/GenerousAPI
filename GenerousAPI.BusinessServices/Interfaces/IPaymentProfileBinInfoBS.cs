namespace GenerousAPI.BusinessServices
{
    using DataAccessLayer;
   
    public interface IPaymentProfileBinInfoBS
    {
        /// <summary>
        /// Creates the bin info for the payment profile for a credit card
        /// </summary>
        /// <param name="BinInfo">Bin info DTO details</param>
        /// <returns>True if successfully added, false otherwise</returns>
        bool CreateBinInfo(PaymentProfileBinInfo BinInfo);
    }
}
