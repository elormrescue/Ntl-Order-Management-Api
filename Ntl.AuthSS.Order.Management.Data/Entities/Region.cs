using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Data.Entities
{
    public class Region
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual Country Country { get; set; }
    }
}
