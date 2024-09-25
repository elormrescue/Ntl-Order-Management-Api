using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ntl.AuthSS.Order_Management.Api.Models
{
    public class OrderActionModel
    {
        public Guid OrderId { get; set; }
        public virtual string Comments { get; set; }
    }

}
