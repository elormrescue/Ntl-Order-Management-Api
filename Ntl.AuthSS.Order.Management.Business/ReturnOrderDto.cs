using Ntl.AuthSS.OrderManagement.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class ReturnOrderDto
    {
        public IEnumerable<PackageDto> packageDtos { get; set; }
        public string TrackingId { get; set; }
        public ShipperDto shipper { get; set; }
        public WarehouseDto Warehouse { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public string Comment { get; set; }
        public string Organization { get; set; }
        public string Number { get; set; }
        public ReturnOrderStatus ReturnOrderStatus { get; set; }
        public ICollection<ReturnOrderHistoryDto> ReturnOrderHistories { get; set; }
    }
}
