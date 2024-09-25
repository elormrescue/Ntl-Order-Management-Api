using System.Collections.Generic;

namespace Ntl.AuthSS.OrderManagement.Data.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageName { get; set; }
        public bool IsActive { get; set; }
        public virtual Origin Origin { get; set; }
        public virtual ICollection<ProductPriceHistory> Prices { get; set; }
    }
}
