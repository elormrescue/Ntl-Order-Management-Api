using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class FullFillOrderItemDto
    {
        public Guid OrderItemId { get; set; }
        public IEnumerable<PackageDto> packageDtos { get; set; }
    }
}
