using Ntl.AuthSS.OrderManagement.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class PaymentDto
    {
        public Guid? Id { get; set; }
        public string TransactionId { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
        public string StatusString { get; set; }
    }
}
