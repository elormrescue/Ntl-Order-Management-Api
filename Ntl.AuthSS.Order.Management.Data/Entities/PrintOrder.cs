using Ntl.AuthSS.OrderManagement.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Data.Entities
{
    public class PrintOrder : IAuditable
    {
        public Guid Id { get; set; }
        [MaxLength(50)]
        public string PoNum { get; set; }
        [MaxLength(50)]
        public string Number { get; set; }
        public DateTime ExpectedDate { get; set; }
        public virtual Product Product { get; set; }
        public virtual ReelSize ReelSize { get; set; }
        public int NoOfReels { get; set; }
        public int TotalStamps { get; set; }
        public virtual Organization PrintPartner { get; set; }
        public virtual Warehouse Warehouse { get; set; }
        public PrintOrderStatus Status { get; set; }
        public virtual ICollection<PrintOrderHistory> PrintOrderHistories { get; set; }
        public int CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
