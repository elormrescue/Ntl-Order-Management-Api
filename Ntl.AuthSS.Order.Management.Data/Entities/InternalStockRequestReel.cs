using Ntl.AuthSS.OrderManagement.Data.Interfaces;
using System;

namespace Ntl.AuthSS.OrderManagement.Data.Entities
{
    public class InternalStockRequestReel : IAuditable
    {
        public Guid Id { get; set; }
        public virtual InternalStockRequest InternalStockRequest { get; set; }
        public virtual Reel Reel { get; set; }
        public int CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
