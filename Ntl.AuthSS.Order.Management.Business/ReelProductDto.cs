using System;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class ReelProductDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string ProductName { get; set; }
        public string ProductType { get; set; }
        public string Sku { get; set; }
        public string OldProductName { get; set; }
        public string OldSku { get; set; }

    }
  
}
