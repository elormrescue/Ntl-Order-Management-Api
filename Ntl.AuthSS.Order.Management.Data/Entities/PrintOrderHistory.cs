using Ntl.AuthSS.OrderManagement.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Data.Entities
{
    public class PrintOrderHistory : IAuditable
    {
        public Guid Id { get; set; }
        public virtual PrintOrder PrintOrder { get; set; }
        public PrintOrderStatus Action { get; set; }
        public OrgType ActionedBy { get; set; }
        [MaxLength(1000)]
        public string Comments { get; set; }
        public int CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
