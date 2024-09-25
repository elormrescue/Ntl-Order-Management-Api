using Ntl.AuthSS.OrderManagement.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Data.Entities
{
    public class Organization
    {
        public int Id { get; set; }
        [MaxLength(100)]
        [Required]
        public string Name { get; set; }
        public virtual Contact Contact { get; set; }
        [MaxLength(50)]
        [Required]
        public string TinNumber { get; set; }
        [MaxLength(50)]
        [Required]
        public string ExciseLicenseNumber { get; set; }
        public OrgType OrgType { get; set; }
        public OrgStatus Status { get; set; }
        public ICollection<OrgWarehouse> Warehouses { get; set; }
        public bool IsImporter { get; set; }
        public virtual Address Address { get; set; }
    }
}
