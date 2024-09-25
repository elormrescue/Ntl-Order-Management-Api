using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business
{
   public class MiniReturnOrderDto
    {
        public Guid OrderId { get; set; }
        public string OrgName { get; set; }
        public string OrderNumber { get; set; }
        public string RequestedOn { get; set; }
        public string Status { get; set; }
    }
}
