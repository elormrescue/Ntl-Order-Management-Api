using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class TotalPymentDetailsDto
    {
        public string TotalAmount { get; set; }
        public string PaidAmount { get; set; }
        public string UnpaidAmount { get; set; }
        public int TotalRows { get; set; }
        public IList<OrgPaymentDetails> OrgPaymentDetails { get; set; }
        public long DomesticAffixAccount { get; set; }
        public long ImportedAffixCount { get; set; }
        public long TransitionAffixCount { get; set; }

    }
    public class OrgPaymentDetails
    {
        public int? OrganizationId { get; set; }
        public string Name { get; set; }
        public string totalAmount { get; set; }
        public string PaidAmount { get; set; }
        public string UnpaidAmount { get; set; }
    }
}
