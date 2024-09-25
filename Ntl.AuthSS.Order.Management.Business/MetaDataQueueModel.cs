using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class MetaDataQueueModel
    {
        public Guid Id { get; set; }
        public MetaDataUpdateEvent EventName { get; set; }
    }
}
