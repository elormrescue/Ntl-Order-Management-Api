using Ntl.AuthSS.OrderManagement.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Data.Entities
{
    public class ReelChangeRequest : IAuditable
    {
        public Guid Id { get; set; }
        [MaxLength(50)]
        public string Number { get; set; }
        public virtual Organization Organization { get; set; }
        public ReelChangeType ReelChangeType { get; set; }
        public virtual Product ReelChangeProduct { get; set; }
        public virtual OrgBrandProduct ChangeToProduct { get; set; }
        public virtual StockKeepingUnit ChangeToSku { get; set; }
        public virtual ICollection<ReelChangeRequestReel> ReelChangeRequestReels { get; set; }
        public virtual ICollection<ReelChangeRequestHistory> ReelChangeRequestHistories { get; set; }
        public ReelChangeRequestStatus Status { get; set; }
        public int CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
