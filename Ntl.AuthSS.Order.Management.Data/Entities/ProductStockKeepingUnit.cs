using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Data.Entities
{
    public class ProductStockKeepingUnit
    {
        public int Id { get; set; }
        public virtual Product Product { get; set; }
        public virtual StockKeepingUnit StockKeepingUnit { get; set; }
        public bool IsActive { get; set; }
    }
}
