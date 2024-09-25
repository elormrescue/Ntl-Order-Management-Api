using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class PrintOrderQueueDto
    {
        public Guid RequestGuid { get; set; }
        public string PrintOrderNo { get; set; }
        public Guid PrintOrderGuid { get; set; }
        public int ProductType { get; set; }
        public int WareHouseId { get; set; }
        public int PrintPartnerId { get; set; }
        public int ReelSize { get; set; }
        public int TotalReels { get; set; }
        public int ExcessPercentage { get; set; }
        public bool IsExcessNewFile { get; set; }
    }
}
