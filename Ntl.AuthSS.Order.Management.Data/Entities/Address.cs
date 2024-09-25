using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Data.Entities
{
    public class Address
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string City { get; set; }
        public virtual Region Region { get; set; }
        public virtual Country Country { get; set; }
        public string PostalAddress { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? Latitude { get; set; }
    }
}
