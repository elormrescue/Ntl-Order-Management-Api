using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Queryable.Entities
{
   public class DashboardOrderDetailsResponse
    {
       public int? OrgType { get; set; }
        public int? TotalNoOfOrder { get; set; }
        public int? Submitted { get; set; }
       public int? InConsideration { get; set; }
       public int? Rejected { get; set; }
       public int? InProcessing { get; set; }
       public int? InTransit { get; set; }
       public int? Delivered { get; set; }
       public int? Closed { get; set; }
       public int? LatestSubmitted { get; set; }
       public int? LatestInConsideration { get; set; }
       public int? LatestRejected { get; set; }
       public int? LatestInProcessing { get; set; }
       public int? LatestInTransit { get; set; }
       public int? LatestDelivered { get; set; }
       public int? LatestClosed { get; set; }
    }
}
