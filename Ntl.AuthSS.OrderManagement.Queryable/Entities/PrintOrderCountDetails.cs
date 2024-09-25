using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Queryable.Entities
{
   public class PrintOrderCountDetails
    {
      public int? Submitted { get; set; }
      public int? Processing { get; set; }
      public int? Rejected { get; set; }
      public int? InTransit { get; set; }
      public int? Closed { get; set; }
      public int? LatestSubmitted { get; set; }
      public int? LatestProcessing { get; set; }
      public int? LatestRejected { get; set; }
      public int? LatestInTransit { get; set; }
      public int? LatestClosed { get; set; }
      public int? TotalNoOfOrder { get; set; } 
      public int? IsRecent { get; set; }
    }
}
