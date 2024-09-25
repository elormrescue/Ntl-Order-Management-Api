using Ntl.AuthSS.OrderManagement.Data.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;

namespace Ntl.AuthSS.OrderManagement.Data.Entities
{
    public class ReturnOrderHistory: IAuditable
    {
        public Guid Id { get; set; }
        public virtual ReturnOrder ReturnOrder { get; set; }
        public ReturnOrderStatus Action { get; set; }
        public OrgType ActionedBy { get; set; }
        [MaxLength(500)]
        public string Comments { get; set; }
        public int CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
