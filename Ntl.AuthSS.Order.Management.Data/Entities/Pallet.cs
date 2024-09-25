using Ntl.AuthSS.OrderManagement.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Data.Entities
{
    public class Pallet: IAuditable
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public virtual Container Container { get; set; }
        public ICollection<Carton> Cartons { get; set; }
        public int CartonCount { get; set; }
        public int CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
