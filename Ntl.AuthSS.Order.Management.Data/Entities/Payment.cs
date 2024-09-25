using Ntl.AuthSS.OrderManagement.Data.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ntl.AuthSS.OrderManagement.Data.Entities
{
    public class Payment: IAuditable
    {
        public Guid Id { get; set; }
        public PaymentMode PaymentMode { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        [Required]
        [MaxLength(50)]
        public string TransactionId { get; set; } //Not sure what it is
        public string PaymentInfo { get; set; }

        public decimal Amount { get; set; }

        public DateTime? PaymentDate { get; set; }

        public PaymentMethod PaymentMethod { get; set; }
        public Guid OrderId { get; set; }
        public Order Order { get; set; }
        public ICollection<PaymentHistory> PaymentHistories { get; set; }
        public int CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedUser { get; set ; }
        public DateTime ModifiedDate { get; set; }
    }
}
