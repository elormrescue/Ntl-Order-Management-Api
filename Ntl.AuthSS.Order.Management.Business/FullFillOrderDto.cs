using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business.Services
{
   public class FullFillOrderDto
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; }
        public IEnumerable<FullFillOrderItemDto> fullFillOrderItemDtos { get; set; }

        public string TrackingId { get; set; }
        public ShipperDto shipper { get; set; }
        public DateTime ExpectedDeliveryDate { get; set; }
        public string Comment { get; set; }
    }
}
