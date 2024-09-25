using System.ComponentModel.DataAnnotations;

namespace Ntl.AuthSS.OrderManagement.Data.Entities
{
    public class Warehouse
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public virtual Contact Contact { get; set; }
        public virtual Address Address { get; set; }
    }
}
