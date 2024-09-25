using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Data.Entities
{
    public class OrgSupplier
    {
        public int Id { get; set; }
        public virtual Supplier Supplier { get; set; }
        public virtual Organization Organization { get; set; }
        public bool IsActive { get; set; }
    }
}
