using Ntl.AuthSS.OrderManagement.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; }
        public string Notes { get; set; }
        public OrderEntityType OrderEntityType { get; set; }
        public string StatusString { get; set; }
        public ICollection<OrderHistoryDto> OrderHistories { get; set; }        
        public PaymentDto Payment { get; set; }
        public ICollection<OrderItemDto> OrderItems { get; set; }
        public int OrgId { get; set; }
        public int OrgType { get; set; }
        public string OrgName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int TotalCoils { get; set; }
        public decimal TotalStamps { get; set; }
        public decimal ShippingCharges { get; set; }
        public decimal CreditsApplied { get; set; }
        public decimal PayableAmount { get; set; }
        public decimal Tax { get; set; }
        public decimal TotalStampsPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public ICollection<OrderPaymentBreakdownDto> OrderPaymentBreakdowns { get; set; }
        public Organization Shipper { get; set; }
        public Organization Organization { get; set; }
        public DateTime? ExpectedDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
