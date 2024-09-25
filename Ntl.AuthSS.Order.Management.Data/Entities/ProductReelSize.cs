using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Data.Entities
{
    public class ProductReelSize
    {
        public int Id { get; set; }
        public virtual Product Product { get; set; }
        public virtual ReelSize ReelSize { get; set; }
        public bool IsActive { get; set; }
        public bool CanManufacturerOrder { get; set; }
        public bool CanTpsafOrder { get; set; }
    }
}
