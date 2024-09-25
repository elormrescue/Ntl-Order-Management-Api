using System.ComponentModel.DataAnnotations;

namespace Ntl.AuthSS.OrderManagement.Data.Entities
{
    public class StockKeepingUnit
    {
        public int Id { get; set; }

        [MaxLength(50)]
        [Required]
        public string Unit { get; set; }

        [MaxLength(100)]
        [Required]
        public string Description { get; set; }

        public bool IsActive { get; set; }
    }
}
