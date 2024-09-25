using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class StampGenerationDto
    {
        public Guid Id { get; set; }
        public string PrintOrderNumber { get; set; }
        public string Status { get; set; }
        public string PrintPartner { get; set; }
        public DateTime? RequestedDate { get; set; }
    }
}
