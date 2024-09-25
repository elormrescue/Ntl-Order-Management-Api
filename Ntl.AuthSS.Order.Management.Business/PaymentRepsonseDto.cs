using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class PaymentRepsonseDto
    {
        //public string Redirecturl { get; set; }
        //public Guid OrderIdForPayment { get; set; }
        public string TransactionId { get; set; }
        public string Trxref { get; set; }
        public string Reference { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string ResponseInfo { get; set; }
        public int PaymentMode { get; set; }
    }
}
