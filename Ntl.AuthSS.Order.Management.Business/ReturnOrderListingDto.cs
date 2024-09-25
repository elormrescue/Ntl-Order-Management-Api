using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business
{
   public class ReturnOrderListingDto
    {
        public IEnumerable<MiniReturnOrderDto> MiniReturnOrderDtos { get; set; }
        public int TotalCount { get; set; }
        public int SubmittedCount { get; set; }
        public int InTransitCount { get; set; }
        public int DeliveredCount { get; set; }
        public int ClosedCount { get; set; }
        public int TotalRows { get; set; }
    }
}
