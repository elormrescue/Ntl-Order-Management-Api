using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Data.Entities
{
    public class OrgProduct
    {
        public int Id { get; set; }
        public virtual Product Product { get; set; }
        public virtual Organization Organization { get; set; }
        public bool IsActive { get; set; }        
    }
}
