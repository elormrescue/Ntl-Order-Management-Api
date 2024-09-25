using Ntl.AuthSS.OrderManagement.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class DeliveryDto
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string OrderStatusString { get; set; }
        public DeliveryStatus DeliveryStatus { get; set; }
        public string DeliveryStatusString { get; set; }
        public DeliveryType DeliveryType { get; set; }
        public string DeliveryTypeString { get; set; }
        public DateTime? FullfilledDateTime { get; set; }
        public string FullfilledDate { get; set; }
        public string OrgName { get; set; }
        public string RecepientName { get; set; }
        public string Address { get; set; }
        public string ContactNo { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ModifiedDateString { get; set; }
        public string PackageURL { get; set; }
    }
}
