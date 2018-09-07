using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerousAPI.BusinessEntities
{
    public class DonorAndOrganisation
    {
        public int OrganisationId { get; set; }

        public string TokenId { get; set; }

        public string ExpiryMonth { get; set; }

        public string ExpiryYear { get; set; }
    }
}
