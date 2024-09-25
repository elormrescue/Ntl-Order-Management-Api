using Ntl.AuthSS.OrderManagement.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class PrintOrderHistoryDto
    {
        public Guid Id { get; set; }
        public string Action { get; set; }
        public string ActionedBy { get; set; }
        public string Comments { get; set; }
        public string ModifiedDate { get; set; }
    }
}
