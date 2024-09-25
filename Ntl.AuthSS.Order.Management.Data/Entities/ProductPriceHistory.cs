using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Data.Entities
{
    public class ProductPriceHistory
    {
        public int Id { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public Decimal Price { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public virtual Product Product { get; set; }
        public PriceStatus Status { get; set; }
    }
}
