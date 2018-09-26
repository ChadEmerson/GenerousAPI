using GenerousAPI.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerousAPI.BusinessServices
{
    public interface IOrganisationFeeProcessingBS
    {
        IEnumerable<OrganisationFeeProcessing> GetOrganisationsToProcessFee();

        IEnumerable<OrganisationFeeProcesingWithRelatedData> GetOrganisationFeeProcesingWithRelatedData();

        OrganisationFeeProcesingWithRelatedData GetOrganisationFeeProcesingWithRelatedData(int organisationId);

        OrganisationFeeProcesingWithRelatedData GetDefaultOrganisationFeeProcesingWithRelatedData();

        void UpdateOrganisationFeeProces(OrganisationFeeProcessing processedFee);

        void UpdateOrganisationFeePromoPrices(OrganisationPromoFee promoFees);

        void UpdateOrganisationFeeStandardPrices(OrganisationStandardFee standFees);

        void CreateOrganisationFeeProces(OrganisationFeeProcessing processFee);

        void CreateOrganisationPromoFees(OrganisationPromoFee promoFees);

        void CreateOrganisationStandardFees(OrganisationStandardFee standFees);

    }
}
