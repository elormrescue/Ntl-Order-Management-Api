using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class DeliveryListingDto
    {
        public ICollection<DeliveryDto> Deliveries { get; set; }
        public DeliveryType DeliveryType;
        public int ReadyToBeDeliveredCount { get; set; }
        public int TransitCount { get; set; }
        public int DeliveredCount { get; set; }
        public int TotalCount { get; set; }
    }
}
