using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class TraceOrderDetailDto
    {
        public string OrganizationName { get; set; }
        public string BrandProductName { get; set; }
        public string NewBrandProductName { get; set; }
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string SkuName { get; set; }
        public string NewSkuName { get; set; }
        public string WarehouseName { get; set; }
        public bool IsReturned { get; set; }
        public string StockRequestNumber { get; set; }
        public string StockEarlierFacility { get; set; }
        public string StockCurrentFacility { get; set; }

    }
}
