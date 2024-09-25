using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Data.Entities
{
    public class Country
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string CountryCode { get; set; }
        public ICollection<Region> Regions { get; set; }
    }
}
