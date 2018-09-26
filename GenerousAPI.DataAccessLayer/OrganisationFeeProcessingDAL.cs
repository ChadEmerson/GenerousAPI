using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerousAPI.DataAccessLayer
{
    public class OrganisationFeeProcessingDAL : IOrganisationFeeProcessingDAL
    {
        /// <summary>
        /// List of active records to process the fees
        /// </summary>
        /// <returns></returns>
        public IEnumerable<OrganisationFeeProcessing> GetOrganisationsToProcessFee()
        {
            using (var db = new GenerousAPIEntities())
            {
                return db.OrganisationFeeProcessings.Where(x => x.IsActive.Value).ToList<OrganisationFeeProcessing>();
            }
        }

        /// <summary>
        /// Get a list of org fee processing reecords with related data
        /// </summary>
        /// <returns>list of fee processing records</returns>
        public IEnumerable<OrganisationFeeProcesingWithRelatedData> GetOrganisationFeeProcesingWithRelatedData()
        {
            using (var db = new GenerousAPIEntities())
            {
                var orgFeeProcessing = from orgFee in db.OrganisationFeeProcessings
                                       join promofees in db.OrganisationPromoFees on orgFee.OrganisationId equals promofees.OrganisationId
                                       join standardfees in db.OrganisationStandardFees on orgFee.OrganisationId equals standardfees.OrganisationId
                                       select new OrganisationFeeProcesingWithRelatedData()
                                       {
                                           OrganisationToProcess = orgFee,
                                           OrganisationPromoFees = promofees,
                                           OrganisationStandardFees = standardfees
                                       };

                var list = orgFeeProcessing.ToList<OrganisationFeeProcesingWithRelatedData>();
                return list.Where(x => (x.OrganisationToProcess.NextRunDate.Value == null || x.OrganisationToProcess.NextRunDate.Value < DateTime.Now) && x.OrganisationToProcess.IsActive.Value).ToList();
            }
        }

        /// <summary>
        /// Get default fees 
        /// </summary>
        /// <returns>Default fees</returns>
        public OrganisationFeeProcesingWithRelatedData GetDefaultOrganisationFeeProcesingWithRelatedData()
        {
            using (var db = new GenerousAPIEntities())
            {
                var orgFeeProcessing = from orgFee in db.OrganisationFeeProcessings
                                       where orgFee.OrganisationId == 0 && orgFee.SystemDefault == true 
                                       join promofees in db.OrganisationPromoFees on orgFee.OrganisationId equals promofees.OrganisationId
                                       join standardfees in db.OrganisationStandardFees on orgFee.OrganisationId equals standardfees.OrganisationId
                                       select new OrganisationFeeProcesingWithRelatedData()
                                       {
                                           OrganisationToProcess = orgFee,
                                           OrganisationPromoFees = promofees,
                                           OrganisationStandardFees = standardfees
                                       };

                return orgFeeProcessing.SingleOrDefault();
            }
        }

        /// <summary>
        /// Get a list of org fee processing records with related data
        /// </summary>
        /// <returns>list of fee processing records</returns>
        public OrganisationFeeProcesingWithRelatedData GetOrganisationFeeProcesingWithRelatedData(int organisationId)
        {
            using (var db = new GenerousAPIEntities())
            {
                var orgFeeProcessing = from orgFee in db.OrganisationFeeProcessings
                                       where orgFee.OrganisationId == organisationId
                                       join promofees in db.OrganisationPromoFees on orgFee.OrganisationId equals promofees.OrganisationId
                                       join standardfees in db.OrganisationStandardFees on orgFee.OrganisationId equals standardfees.OrganisationId
                                       select new OrganisationFeeProcesingWithRelatedData()
                                       {
                                           OrganisationToProcess = orgFee,
                                           OrganisationPromoFees = promofees,
                                           OrganisationStandardFees = standardfees
                                       };

                return orgFeeProcessing.SingleOrDefault();
            }
        }

        /// <summary>
        /// Get an organisation record to process fees
        /// </summary>
        /// <param name="OrgId">org Id</param>
        /// <returns>Org record</returns>
        public OrganisationFeeProcessing GetOrganisationsToProcessFeeRecordByOrganisationId(int OrgId)
        {
            using (var db = new GenerousAPIEntities())
            {
                return db.OrganisationFeeProcessings.Where(x => x.OrganisationId == OrgId).FirstOrDefault<OrganisationFeeProcessing>();
            }
        }

        /// <summary>
        /// Update the organisation fee processing record
        /// </summary>
        /// <param name="processedFee"></param>
        public void UpdateOrganisationFeeProces(OrganisationFeeProcessing processedFee)
        {
            try
            {
                using (var db = new GenerousAPIEntities())
                {
                    db.Entry(processedFee).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                //Helper.LogException(ex);
            }
        }

        /// <summary>
        /// Update the organisation fee processing record
        /// </summary>
        /// <param name="processedFee"></param>
        public void UpdateOrganisationFeePromoPrices(OrganisationPromoFee promoFees)
        {
            try
            {
                using (var db = new GenerousAPIEntities())
                {
                    db.Entry(promoFees).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                //Helper.LogException(ex);
            }
        }

        /// <summary>
        /// Update the organisation fee processing record
        /// </summary>
        /// <param name="processedFee"></param>
        public void UpdateOrganisationFeeStandardPrices(OrganisationStandardFee standFees)
        {
            try
            {
                using (var db = new GenerousAPIEntities())
                {
                    db.Entry(standFees).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                //Helper.LogException(ex);
            }
        }

        /// <summary>
        /// Create a new fee processing record
        /// </summary>
        /// <param name="processFee"></param>
        public void CreateOrganisationFeeProces(OrganisationFeeProcessing processFee)
        {
            try
            {
                using (var db = new GenerousAPIEntities())
                {
                    db.OrganisationFeeProcessings.Add(processFee);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                //Helper.LogException(ex);
            }
        }

        /// <summary>
        /// Create a new fee processing record
        /// </summary>
        /// <param name="processFee"></param>
        public void CreateOrganisationPromoFees(OrganisationPromoFee promoFees)
        {
            try
            {
                using (var db = new GenerousAPIEntities())
                {
                    db.OrganisationPromoFees.Add(promoFees);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                //Helper.LogException(ex);
            }
        }

        /// <summary>
        /// Create a new fee processing record
        /// </summary>
        /// <param name="processFee"></param>
        public void CreateOrganisationStandardFees(OrganisationStandardFee standFees)
        {
            try
            {
                using (var db = new GenerousAPIEntities())
                {
                    db.OrganisationStandardFees.Add(standFees);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                //Helper.LogException(ex);
            }
        }
    }
}
