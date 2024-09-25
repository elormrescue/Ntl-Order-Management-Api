using Ntl.AuthSS.OrderManagement.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Data.Entities
{
    public class OrgWallet : IAuditable
    {
        public Guid Id { get; set; }
        [Required]
        public Organization Organization { get; set; }
        [Required]
        public TransactionType TransactionType { get; set; }
        [Required]
        public WalletOrderType WalletOrderType { get; set; }
        [Required]
        public decimal TransactionAmount { get; set; }
        [Required]
        public decimal BalanceAmount { get; set; }
        [MaxLength(1000)]
        public string Description { get; set; }
        public virtual ReturnOrder ReturnOrder { get; set; }
        public virtual Order Order { get; set; }
        public int CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
