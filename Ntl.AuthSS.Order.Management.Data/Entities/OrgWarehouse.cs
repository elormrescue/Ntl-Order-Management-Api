using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Data.Entities
{
    public class OrgWarehouse
    {
        public int Id { get; set; }
        public virtual Organization Organization { get; set; }
        public virtual Warehouse Warehouse { get; set; }
    }
}