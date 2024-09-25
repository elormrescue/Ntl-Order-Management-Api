using System.ComponentModel.DataAnnotations;

namespace Ntl.AuthSS.Order_Management.Api.Models
{
    public class OrderRejectActionModel : OrderActionModel
    {
        [Required]
        public override string Comments { get; set; }
    }

}
