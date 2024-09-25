using System.Collections.Generic;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class StockRequestListingDto
    {
        public IEnumerable<MiniInternalStockRequestDto> MiniStockRequestDtos { get; set; }
        public int TotalCount { get; set; }
        public int RequestedCount { get; set; }
        public int ApprovedCount { get; set; }
        public int FulfilledCount { get; set; }
        public int TransitCount { get; set; }
        public int DeliveredCount { get; set; }
        public int ClosedCount { get; set; }
        public int ExpiredCount { get; set; }
        public int TotalRows { get; set; }
    }
}
