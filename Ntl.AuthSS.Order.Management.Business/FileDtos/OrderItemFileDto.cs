using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business.FileDtos
{
   public class OrderItemFileDto
    {
        public string ProductDescription { get; set; }
        public int Quantity { get; set; }
        public decimal? StampPrice { get; set; }
        public decimal? TotalStamps { get; set; }
        public decimal? TotalPrice { get; set; }
    }
}
