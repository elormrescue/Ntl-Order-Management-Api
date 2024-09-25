using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business
{
   public class LockPackageDto
    {
       public List<PackageDto> PackageDtos { get; set; }
       public Guid OrderItemId { get; set; }
    }
}
