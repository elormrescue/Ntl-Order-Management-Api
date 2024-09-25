using Ntl.AuthSS.OrderManagement.Data.Interfaces;
using System;

namespace Ntl.AuthSS.OrderManagement.Data.Entities
{
    public class ReelChangeRequestReel: IAuditable
    {
        public Guid Id { get; set; }
        public virtual ReelChangeRequest ReelChangeRequest { get; set; }
        public virtual Reel Reel { get; set; }
        public int CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
