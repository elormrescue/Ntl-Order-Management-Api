using Ntl.AuthSS.OrderManagement.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Data.Entities
{
    public class TaxSlab: IAuditable
    {
        public int Id { get; set; }
        public string TaxType { get; set; }
        public decimal? Percentage { get; set; }
        public bool IsCumulative { get; set; }
        public TaxSlabStatus Status { get; set; }
        public int CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime? ReflectingFrom { get; set; }

    }
}
