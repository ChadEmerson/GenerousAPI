using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerousAPI.DataAccessLayer
{
    public class OrganisationFeeProcesingWithRelatedData
    {
        public OrganisationFeeProcessing OrganisationToProcess { get; set; }

        public OrganisationPromoFee OrganisationPromoFees { get; set; }

        public OrganisationStandardFee OrganisationStandardFees { get; set; }

        public Decimal TotalAmountToPay { get; set; }
    }
}
