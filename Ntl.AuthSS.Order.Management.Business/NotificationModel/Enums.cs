using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ntl.AuthSS.Order_Management.Api.Models
{
    public enum WebNotificationType
    {
        MfApplicationSubmitted,
        MfApplicationResubitted,
        MfApplicationApproved,
        MfApplicationRejected,
        MfOrderSubmitted,
        MfOrderApproved,
        MfOrderRejected,
        MfOrderReSubmitted,
        MfOrderFullfilled,
        MfOrderPickedUp,
        MfOrderDelivered,
        MfOrderClosed,
        StockOrderSubmitted,
        StockOrderApproved,
        StockOrderRejected,
        StockOrderReSubmitted,
        StockOrderFullfilled,
        StockOrderPickedUp,
        StockOrderDelivered,
        StockOrderClosed,
        ChangeSkuProductRequested,
        ChangeSkuProductRejected,
        ChangeSkuProductApproved,
        MfReturnOrderSubmitted,
        MfReturnOrderApproved,
        PrintOrderSubmitted,
        PrintOrderApproved,
        PrintOrderRejected,
        PrintOrderInTransit,
        PrintOrderClosed,
        InternalStockTransferRequested,
        InternalStockTransferApproved,
        InternalStockTransferFulFilled,
    }
}
