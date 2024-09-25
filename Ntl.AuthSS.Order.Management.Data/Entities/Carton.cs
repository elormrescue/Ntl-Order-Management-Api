using Ntl.AuthSS.OrderManagement.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Data.Entities
{
    public class Carton: IAuditable
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public virtual Product Product { get; set; }
        public int ReelCount { get; set; }
        public Guid PalletId { get; set; }
        public virtual Pallet Pallet { get; set; }
        public ICollection<Reel> Reels { get; set; }
        public int CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
