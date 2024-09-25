using System;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class MiniInternalStockRequestDto
    {
        public Guid StockRequestId { get; set; }
        public string Number { get; set; }
        public string ProductName { get; set; }
        public int RequestedStamps { get; set; }
        public int FulfilledStamps { get; set; }
        public string RequestedOn { get; set; }
        public string Status { get; set; }
        public int RequestingFacilityId { get; set; }
        public int? ApprovingFacilityId { get; set; }
        public string RequestingFacility { get; set; }
        public string ApprovingFacility { get; set; }
    }
}
