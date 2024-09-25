using Ntl.AuthSS.OrderManagement.Data.Entities;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Ntl.AuthSS.OrderManagement.Business.Services
{
    public interface IReturnOrderService
    {
        Task<(string orderNumber,string orgName,string returnOrderNumber, string errorMessage)> ReturnOrder(ReturnOrderDto returnOrderDto, OrderEntityType? orderEntityType, int orgId);
        Task<ReturnOrderListingDto> GetReturnOrders(ReturnOrderSearchFilterOptions filterOptions);
        Task<ReturnOrderDto> GetReturnOrderDetailsAsync(Guid returnOrderId);
        Task<(string mfName, string returnOrderNumber, int orgId)> ApproveReturnOrder(Guid orderId, string comments, OrderEntityType? orderEntityType);
        IList<MiniReturnOrderDto> GetReturnOrderDownloadList(ReturnOrderSearchFilterOptions filterOptions);
    }
}