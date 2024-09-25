using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class OrderListingDto
    {
        public IEnumerable<MiniOrderDto> MiniOrderDtos { get; set; }
        public int TotalCount { get; set; }
        public int SubmittedCount { get; set; }
        public int InConsiderationCount { get; set; }
        public int InTransitCount { get; set; }
        public int FullFilledCount { get; set; }
        public int RejectedCount { get; set; }
        public int ResubmittedCount { get; set; }
        public int DeliveredCount { get; set; }
        public int TotalRows { get; set; }
    }
}
