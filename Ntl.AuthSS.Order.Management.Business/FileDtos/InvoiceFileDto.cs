using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business.FileDtos
{
   public class InvoiceFileDto
    {
        public string InvoiceNumber { get; set; }
        public IEnumerable<OrderItemFileDto> OrderItems { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal AmountDue { get; set; }
        public decimal StampTotalPrice { get; set; }
        public decimal OrderCreditAdded { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingCharges { get; set; }
        public string Organization { get; set; }
        public string TinNumber { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public decimal TotalAfterTaxes { get; set; }
        public decimal TotalAfterCumulativeTax { get; set; }
        public List<OrderPaymentBreakdownDto> BreakdownDtos { get; set; }
        public bool IsOrgImporter { get; set; }
    }
}
