using Ntl.AuthSS.OrderManagement.Data.Interfaces;
using System;

namespace Ntl.AuthSS.OrderManagement.Data.Entities
{
    public class OrderItem: IAuditable
    {
        public Guid Id { get; set; }
        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
        public virtual OrgBrandProduct BrandProduct { get; set; }
        public virtual StockKeepingUnit StockKeepingUnit { get; set; }
        public virtual ReelSize ReelSize { get; set; }
        public virtual Supplier Supplier { get; set; }
        public int NoOfCoils { get; set; }
        public decimal NoOfStamps { get; set; }
        public decimal? StampPrice { get; set; }
        public decimal? TotalPrice { get; set; }
        public int UsedReelCount { get; set; }
        public bool IsFulfilled { get; set; }
        public int UsedCartonCount { get; set; }
        public int CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
