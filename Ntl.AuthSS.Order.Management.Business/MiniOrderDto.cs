using System;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class MiniOrderDto
    {
        public Guid OrderId { get; set; }
        public string OrgName { get; set; }
        public string OrderNumber { get; set; }
        public string RequestedOn { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public string Location { get; set; }
        public bool IsImporter { get; set; }
        public bool ManufactureHasDomesticProducts { get; set; }
        public decimal PayableAmount { get; set; }
        public int DueDays { get; set; }
    }
}
