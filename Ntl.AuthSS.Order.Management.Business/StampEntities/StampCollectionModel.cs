using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business.StampEntities
{
    public class StampCollectionModel
    {
        public Guid id { get; set; }
        public Guid ReelGuid { get; set; }
        public string ReelCode { get; set; }
        public string CollectionName { get; set; }
    }
}
