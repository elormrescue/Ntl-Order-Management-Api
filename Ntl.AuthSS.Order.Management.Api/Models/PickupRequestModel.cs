using Ntl.AuthSS.OrderManagement.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ntl.AuthSS.Order_Management.Api.Models
{
    public class PickupRequestModel
    {
        public Guid OrderId { get; set; }
        public DeliveryType DeliveryType { get; set; }
    }

    public class DeliverRequestModel
    {
        public Guid OrderId { get; set; }
        public DeliveryType DeliveryType { get; set; }
        public int? Otp { get; set; }
        public string Remarks { get; set; }
    }
}
