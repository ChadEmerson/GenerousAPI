using GenerousAPI.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerousAPI.BusinessServices
{
    public class OrganisationFeeProcessingBS : IOrganisationFeeProcessingBS
    {
        IOrganisationFeeProcessingDAL _organisationFeeProcessingDAL;

        public OrganisationFeeProcessingBS()
        {
            _organisationFeeProcessingDAL = new OrganisationFeeProcessingDAL();
        }

        /// <summary>
        /// Get a list of org fee processing reecords with related data
        /// </summary>
        /// <returns>list of fee processing records</returns>
        public IEnumerable<OrganisationFeeProcesingWithRelatedData> GetOrganisationFeeProcesingWithRelatedData()
        {
            return _organisationFeeProcessingDAL.GetOrganisationFeeProcesingWithRelatedData();
        }

        public IEnumerable<OrganisationFeeProcessing> GetOrganisationsToProcessFee()
        {
            return _organisationFeeProcessingDAL.GetOrganisationsToProcessFee();
        }

        public OrganisationFeeProcesingWithRelatedData GetOrganisationFeeProcesingWithRelatedData(int organisationId)
        {
            return _organisationFeeProcessingDAL.GetOrganisationFeeProcesingWithRelatedData(organisationId);
        }

        public OrganisationFeeProcesingWithRelatedData GetDefaultOrganisationFeeProcesingWithRelatedData()
        {
            return _organisationFeeProcessingDAL.GetDefaultOrganisationFeeProcesingWithRelatedData();
        }

        public void UpdateOrganisationFeeProces(OrganisationFeeProcessing processedFee)
        {
            _organisationFeeProcessingDAL.UpdateOrganisationFeeProces(processedFee);
        }

        public void UpdateOrganisationFeePromoPrices(OrganisationPromoFee promoFees)
        {
            _organisationFeeProcessingDAL.UpdateOrganisationFeePromoPrices(promoFees);
        }

        public void UpdateOrganisationFeeStandardPrices(OrganisationStandardFee standFees)
        {
            _organisationFeeProcessingDAL.UpdateOrganisationFeeStandardPrices(standFees);
        }

        public void CreateOrganisationFeeProces(OrganisationFeeProcessing processFee)
        {
            _organisationFeeProcessingDAL.CreateOrganisationFeeProces(processFee);
        }

        public void CreateOrganisationPromoFees(OrganisationPromoFee promoFees)
        {
            _organisationFeeProcessingDAL.CreateOrganisationPromoFees(promoFees);
        }

        public void CreateOrganisationStandardFees(OrganisationStandardFee standFees)
        {
            _organisationFeeProcessingDAL.CreateOrganisationStandardFees(standFees);
        }
    }
}
