using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Queryable.Entities
{
   public class PrintOrderStampDetailsBasedOnProduct
    {
        public int? NoOfStamps { get; set; }
        public string ProductNameOrigin { get; set; }
        public int? ProductId { get; set; }
    }
}
