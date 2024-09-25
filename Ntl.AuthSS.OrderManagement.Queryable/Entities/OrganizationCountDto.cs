namespace Ntl.AuthSS.OrderManagement.Queryable.Entities
{
   public class OrganizationCountDto
    {
        public int? ActiveMfCount { get; set; }
        public int? TotalMfCount { get; set; }
        public int? MfImporterCount { get; set; }
        public int? MfDomesticCount { get; set; }
        public int? TpsafCount { get; set; }
        public int? PrintPartnerCount { get; set; }
        public int? ShipperCount { get; set; }
    }
}
