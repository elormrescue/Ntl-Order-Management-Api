using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business
{
   public class AutoFillFullfillOrderItemDto
    {
       public string OrderItemId { get; set; }
       public List<CartonDto> CartonDtos { get; set; }
       public List<ReelDto> ReelDtos { get; set; }
    }
}
