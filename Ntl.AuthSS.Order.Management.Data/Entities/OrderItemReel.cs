using Ntl.AuthSS.OrderManagement.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Data.Entities
{
    public class OrderItemReel : IAuditable
    {
        public Guid Id { get; set; }
        public virtual OrderItem OrderItem { get; set; }
        public virtual Reel Reel { get; set; }
        public virtual Carton Carton { get; set; }
        public string CartonCode { get; set; }
        public bool IsReturned { get; set; }
        public ReelConsumptionType ReelConsumptionType { get; set; }
        public virtual InternalStockRequest InternalStockRequest { get; set; }
        public virtual ReturnOrder ReturnOrder { get; set; }
        public virtual PrintOrder PrintOrder { get; set; }
        public virtual Organization Organization { get; set; }
        [MaxLength(50)]
        public string OrganizationName { get; set; }
        public virtual Warehouse Warehouse { get; set; }
        public virtual Product Product { get; set; }
        public virtual OrgBrandProduct BrandProduct { get; set; }
        [MaxLength(50)]
        public string BrandProductName { get; set; }
        [MaxLength(50)]
        public string WarehouseName { get; set; }
        public virtual StockKeepingUnit Sku { get; set; }
        [MaxLength(50)]
        public string SkuName { get; set; }
        public int? NewBrandProductId { get; set; }
        public virtual OrgBrandProduct NewBrandProduct { get; set; }
        [MaxLength(50)]
        public string NewBrandProductName { get; set; }
        public int? NewSkuId { get; set; }
        public virtual StockKeepingUnit NewSku { get; set; }
        [MaxLength(50)]
        public string NewSkuName { get; set; }
        public int CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
