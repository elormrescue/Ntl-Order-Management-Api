using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Queryable.Entities
{
   public class MFStampDetailsBasedOnProductSku
    {
        public string Sku { get; set; }
        public int SkuId { get; set; }
        public decimal NoOfStamps { get; set; }
        public string ProductNameOrigin { get; set; }
        public int ProductId { get; set; }
    }
}
