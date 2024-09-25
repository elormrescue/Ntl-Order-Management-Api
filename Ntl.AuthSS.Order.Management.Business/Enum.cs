using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public enum MetaDataUpdateEvent
    {
        OrderFulfillment,
        ReturnOrderApproval,
        ChangeSkuApproval,
        InternalStockOrderApproval
    }
}
