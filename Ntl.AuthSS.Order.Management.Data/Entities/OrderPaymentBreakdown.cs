using Ntl.AuthSS.OrderManagement.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Data.Entities
{
   public class OrderPaymentBreakdown:IAuditable
    {
        public int Id { get; set; }
        public virtual Order Order { get; set; }
        public string ItemPriceType { get; set; }
        public decimal? Percentage { get; set; }
        public decimal? Amount { get; set; }
        public bool IsTax { get; set; }
        public int CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsCumulative { get; set; }

    }
}
