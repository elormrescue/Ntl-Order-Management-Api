using Ntl.AuthSS.OrderManagement.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Data.Entities
{
    public class OrgBrandProduct
    {
        public int Id { get; set; }
        [MaxLength(100)]
        [Required]
        public string Name { get; set; }
        [Required]
        public virtual Organization Organization { get; set; }
        [Required]
        public virtual Product Product { get; set; }
        public bool IsActive { get; set; }
        public bool IsApproved { get; set; }
    }
}
