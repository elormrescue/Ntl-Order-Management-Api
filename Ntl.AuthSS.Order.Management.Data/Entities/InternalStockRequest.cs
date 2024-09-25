using Ntl.AuthSS.OrderManagement.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Data.Entities
{
    public class InternalStockRequest : IAuditable
    {
        public Guid Id { get; set; }
        [MaxLength(50)]
        public string Number { get; set; }
        public virtual Product Product { get; set; }
        public int NoOfStamps { get; set; }
        public int? OrganizationId { get; set; }
        public virtual Organization Organization { get; set; }
        public virtual Warehouse RequestingFacility { get; set; }
        public virtual Warehouse ApprovingFacility { get; set; }
        public InternalStockTransferStatus Status { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public DateTime? ExpectedDate { get; set; }
        [MaxLength(50)]
        public string TrackingId { get; set; }
        public int? ShipperId { get; set; }
        public virtual Organization Shipper { get; set; }
        [MaxLength(500)]
        public string FulfillmentComments { get; set; }
        public int? DeliveryOtp { get; set; }
        [MaxLength(50)]
        public string DeliveryRemarks { get; set; }
        public int CartonCount { get; set; }
        public int ReelCount { get; set; }
        public ICollection<InternalStockRequestReel> InternalStockRequestReels { get; set; }
        public ICollection<InternalStockRequestHistory> InternalStockRequestHistories { get; set; }
        public int CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
