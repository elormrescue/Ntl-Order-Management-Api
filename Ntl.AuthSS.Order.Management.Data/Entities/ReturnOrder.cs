using Ntl.AuthSS.OrderManagement.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Data.Entities
{
    public class ReturnOrder : IAuditable
    {
        public Guid Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Number { get; set; }
        public virtual Organization Organization { get; set; }
        public ReturnOrderStatus Status { get; set; }
        public virtual Warehouse Warehouse { get; set; }
        public virtual Organization Shipper { get; set; }
        [MaxLength(50)]
        public string TrackingId { get; set; }
        public DateTime? ExpectedDate { get; set; }
        public int? DeliveryOtp { get; set; }
        [MaxLength(50)]
        public string DeliveryRemarks { get; set; }
        public virtual ICollection<ReturnOrderHistory> ReturnOrderHistories { get; set; }
        public int CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
