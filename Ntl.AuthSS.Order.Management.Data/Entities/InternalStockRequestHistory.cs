using Ntl.AuthSS.OrderManagement.Data.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;

namespace Ntl.AuthSS.OrderManagement.Data.Entities
{
    public class InternalStockRequestHistory : IAuditable
    {
        public Guid Id { get; set; }
        public InternalStockTransferStatus Action { get; set; }
        [MaxLength(1000)]
        public string Notes { get; set; }
        public virtual InternalStockRequest InternalStockRequest { get; set; }
        [MaxLength(50)]
        public string UserName { get; set; }
        public int CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
