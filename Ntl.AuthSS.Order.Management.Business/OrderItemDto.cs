using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class OrderItemDto
    {
        public Guid Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ImageName { get; set; }
        public int? BrandProductId { get; set; }
        public string BrandProductName { get; set; }
        public int? StockKeepingUnitId { get; set; }
        public string StockKeepingUnitName { get; set; }
        public int ReelSizeId { get; set; }
        public string ReelSize { get; set; }
        public int? SupplierId { get; set; }
        public string SupplierName { get; set; }
        public int NoOfCoils { get; set; }
        public decimal NoOfStamps { get; set; }
        public decimal? StampPrice { get; set; }
        public decimal? TotalStampsPrice { get; set; }
        public decimal? TotalPrice { get; set; }
        public bool IsFulfilled { get; set; }
        public int UsedReelCount { get; set; }
        public int UsedCartonCount { get; set; }
        public ICollection<PackageDto> FullfilledPackages { get; set; }
        public AutoFillFullfillOrderItemDto UnFullfilledPackages { get; set; }
        //newly added for warehouse details for print order details
        public string ManufactureName { get; set; }
        public string WarehouseName { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string PostalAddress { get; set; }
        public string Description { get; set; }
    }
}
