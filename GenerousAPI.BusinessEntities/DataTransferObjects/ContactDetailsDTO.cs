
namespace GenerousAPI.BusinessEntities
{
    public class ContactDetailsDTO
    {
        public string CustomerFirstName { get; set; }
        public string CustomerLastName { get; set; }

        public string TokenId { get; set; }

        public int DaysUntilExpiry { get; set; }
    }
}

