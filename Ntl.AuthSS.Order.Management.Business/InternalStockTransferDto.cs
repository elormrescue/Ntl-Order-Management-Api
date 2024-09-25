using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class InternalStockTransferDto
    {
        public Guid Id { get; set; }
        public virtual int InternalStockRequestId { get; set; }
        public int CartonCount { get; set; }
        public int ReelCount { get; set; }
    }
}
