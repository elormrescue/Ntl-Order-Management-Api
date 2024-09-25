using Ntl.AuthSS.OrderManagement.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Data.Entities
{
    public class PrintOrderRequest : IAuditable
    {
        public Guid Id { get; set; }
        public PrintOrder PrintOrder { get; set; }
        public StampGenerationStatus Status { get; set; }
        public int CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
        public virtual Organization? PrintPartner { get; set; }
    }
}
