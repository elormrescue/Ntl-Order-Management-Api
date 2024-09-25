using Ntl.AuthSS.OrderManagement.Data.Interfaces;
using System;

namespace Ntl.AuthSS.OrderManagement.Data.Entities
{
    public class ReturnOrderReels: IAuditable
    {
        public Guid Id { get; set; }
        public virtual ReturnOrder ReturnOrder { get; set; }
        public virtual Reel Reel { get; set; }
        public string ReelCode { get; set; }
        public virtual Carton Carton { get; set; }
        public string CartonCode { get; set; }
        public virtual Product Product { get; set; }
        public string ProductName { get; set; }
        public int CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
