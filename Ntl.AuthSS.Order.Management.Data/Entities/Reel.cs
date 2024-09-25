using Ntl.AuthSS.OrderManagement.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Data.Entities
{
    public class Reel: IAuditable
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public int StampCount { get; set; }
        //public Guid PrintOrderId { get; set; }
        public virtual Product Product { get; set; }
        public virtual PrintOrder PrintOrder { get; set; }
        public ReelStatus Status { get; set; }
        public Guid CartonId { get; set; }
        public virtual Carton Carton { get; set; }
        public int ReelSize { get; set; }
        public bool IsUsedForFulfillment { get; set; }
        public int CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
        public virtual Organization PrintPartner { get; set; }
        public DateTime? FulfillLockedDate { get; set; }
        public Guid? LockedOrderItemId { get; set; }

    }

   
}
