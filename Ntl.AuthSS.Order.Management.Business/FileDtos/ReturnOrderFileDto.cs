using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business.FileDtos
{
   public class ReturnOrderFileDto
    {
        public IEnumerable<PackageDto> PackageDtos { get; set; }
        public string TrackingId { get; set; }
        public string ShipperName { get; set; }
        public string  WarehouseName { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public string Comment { get; set; }
        public string Organization { get; set; }
        public string Number { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
    }
}
