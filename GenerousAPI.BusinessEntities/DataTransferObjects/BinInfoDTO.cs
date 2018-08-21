using System;

namespace GenerousAPI.BusinessEntities
{
    public class BinInfoDTO
    {
        public Guid BinInfoId { get; set; }
        public Guid PaymentProfileId { get; set; }
        public string Brand { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public string BankName { get; set; }
        public string CardType { get; set; }
        public Nullable<int> Latitude { get; set; }
        public Nullable<int> Longitude { get; set; }
    }
}
