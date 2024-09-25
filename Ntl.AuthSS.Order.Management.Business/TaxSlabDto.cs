using Ntl.AuthSS.OrderManagement.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class TaxSlabDto
    {
        public int Id { get; set; }
        public string TaxType { get; set; }
        public decimal? Percentage { get; set; }
        public bool IsCumulative { get; set; }
        public TaxSlabStatus Status { get; set; }
        public decimal? NewPercentage { get; set; }
        public DateTime? EffectiveFrom { get; set; }

    }
}
