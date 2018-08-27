
namespace GenerousAPI.BusinessEntities
{
    public class ContactDetailsDTO
    {
        public string CustomerFirstName { get; set; }
        public string CustomerLastName { get; set; }

        public string TokenId { get; set; }

        public string ExpiryMonth { get; set; }

        public string ExpiryYear { get; set; }

        public string CardNumberMask { get; set; }
    }
}

