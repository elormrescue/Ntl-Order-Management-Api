using Ntl.AuthSS.OrderManagement.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ntl.AuthSS.Order_Management.Api.Models
{
    public class PostInternalStockTransferModel
    {
        public Guid RequestId { get; set; }
        public PackageDto[] Packages { get; set; }
        public int ShipperId { get; set; }
        public string TrackingId { get; set; }
        public DateTime ExpectedDate { get; set; }
        public string Comments { get; set; }
    }
}
