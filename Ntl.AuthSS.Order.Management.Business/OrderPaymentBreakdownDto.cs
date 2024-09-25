using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class OrderPaymentBreakdownDto
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public decimal? Percentage { get; set; }
        public bool IsTax { get; set; }
        public decimal? SlabAmount { get; set; }
        public bool IsCumulative { get; set; }
    }
}
