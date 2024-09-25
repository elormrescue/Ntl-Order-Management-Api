using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class MiniPrintOrderDto
    {
        public Guid PrintOrderId { get; set; }
        public string PoNum { get; set; }
        public string ProductName { get; set; }
        public string Origin { get; set; }
        public string PrintOrderNum { get; set; }
        public string PrintPartnerName { get; set; }
        public string ExpectedDate { get; set; }
        public string RequestedOn { get; set; }
        public string Status { get; set; }
        public string Location { get; set; }

    }
}
