
namespace GenerousAPI.Helpers
{
    using System.Diagnostics;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents all the information returned by binlist.net.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("Bin: {Bin}, Brand: {Brand}")]
    public class IssuerInformation
    {
        //private IssuerInformation() { }

        /// <summary>
        /// BIN/IIN number.
        /// </summary>
        [DataMember(Name = "bin")]
        public string Bin { get; set; }

        /// <summary>
        /// Type of card
        /// </summary>
        [DataMember(Name = "type")]
        public string Type { get; set; }

        /// <summary>
        /// BIN/IIN number.
        /// </summary>
        [DataMember(Name = "scheme")]
        public string Scheme { get; set; }

        /// <summary>
        /// Card brand.
        /// </summary>
        [DataMember(Name = "brand")]
        public string Brand { get; set; }

        [DataMember(Name = "sub_brand")]
        public string SubBrand { get; set; }

        /// <summary>
        /// Country Code (ISO3166-1)
        /// </summary>
        [DataMember(Name = "country")]
        public Country Country { get; set; }

        /// <summary>
        /// Country Code (ISO3166-1)
        /// </summary>
        [DataMember(Name = "bank")]
        public Bank Bank { get; set; } 
       
        [DataMember(Name = "query_time")]
        public string QueryTime { get; set; }
    }

    [DataContract]
    public class Country
    {   
        [DataMember(Name = "name")]
        public string CountryName { get; set; }

        /// <summary>
        /// Country Code (ISO3166-1)
        /// </summary>
        [DataMember(Name = "alpha2")]
        public string CountryCode { get; set; }

        [DataMember(Name = "latitude")]
        public string Latitude { get; set; }

        [DataMember(Name = "longitude")]
        public string Longitude { get; set; }
    }


    [DataContract]
    public class Bank
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

    }
}