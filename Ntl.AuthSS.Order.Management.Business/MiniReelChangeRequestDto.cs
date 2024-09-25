using System;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class MiniReelChangeRequestDto
    {
        public Guid Id { get; set; }
        public string Number { get; set; }
        public string OrgName { get; set; }
        public string RequestedDate { get; set; }
        public string Status { get; set; }
        
    }
}
