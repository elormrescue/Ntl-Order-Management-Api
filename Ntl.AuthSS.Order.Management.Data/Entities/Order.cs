using Ntl.AuthSS.OrderManagement.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ntl.AuthSS.OrderManagement.Data.Entities
{
    public class Order : IAuditable
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Number { get; set; }
        public virtual OrderStatus Status { get; set; }
        public virtual Payment Payment { get; set; }

        public virtual Warehouse Warehouse { get; set; }
        public int? OrganizationId { get; set; }
        public virtual Organization Organization { get; set; }
        public int? ShipperId { get; set; }
        public virtual Organization Shipper { get; set; }
        public string TrackingId { get; set; }
        public DateTime? ExpectedDate { get; set; }
        public DateTime? PaymentDueDate { get; set; }        
        public int? DeliveryOtp { get; set; }
        [MaxLength(50)]
        public string DeliveryRemarks { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
        public virtual ICollection<OrderHistory> OrderHistories { get; set; }
        public int TotalCoils { get; set; }
        public decimal TotalStampsPrice { get; set; }

        public decimal TotalStamps { get; set; }
        public decimal ShippingCharges { get; set; }
        public decimal Tax { get; set; }
        public decimal TaxPercent { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal CreditsApplied { get; set; }
        public decimal PayableAmount { get; set; }
        public int CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
        public virtual ICollection<OrderPaymentBreakdown> OrderPaymentBreakdowns { get; set; }
    }
}
