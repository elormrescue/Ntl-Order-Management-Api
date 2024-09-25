using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business.FileDtos
{
   public class FullFillFileDto
    {
        public string OrderNumber { get; set; }
        public IEnumerable<PackageDto> PackageDtos { get; set; }
        public string TrackingId { get; set; }
        public string Shipper { get; set; }
        public DateTime ExpectedDeliveryDate { get; set; }
        public string Comment { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Organization { get; set; }
    }
}
