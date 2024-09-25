using Ntl.AuthSS.OrderManagement.Data.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;

namespace Ntl.AuthSS.OrderManagement.Data.Entities
{
    public class ReelChangeRequestHistory: IAuditable
    {
        public Guid Id { get; set; }
        public virtual ReelChangeRequest ReelChangeRequest { get; set; }
        public ReelChangeRequestStatus Action { get; set; }
        public OrgType ActionedBy { get; set; }
        [MaxLength(500)]
        public string Comments { get; set; }
        public int CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
