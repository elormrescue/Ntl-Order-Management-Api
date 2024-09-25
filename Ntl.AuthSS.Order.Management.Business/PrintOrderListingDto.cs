using System.Collections.Generic;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class PrintOrderListingDto
    {
        public IEnumerable<MiniPrintOrderDto> MiniPrintOrderDtos { get; set; }
        public int TotalCount { get; set; }
        public int SubmittedCount { get; set; }
        public int ProcessingCount { get; set; }
        public int InTransitCount { get; set; }
        public int ClosedCount { get; set; }
        public int RejectedCount { get; set; }
        public int TotalRows { get; set; }
    }
}
