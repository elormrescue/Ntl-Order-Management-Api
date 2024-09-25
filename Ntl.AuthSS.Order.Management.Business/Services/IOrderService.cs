using Ntl.AuthSS.OrderManagement.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ntl.AuthSS.OrderManagement.Business.Services
{
    public interface IOrderService
    {
        Task<Order> SaveOrderAsync(OrderDto orderDto, int entityId, OrderEntityType orderEntityType);
        Task<OrderDto> GetOrderAsync(Guid orderId);
        Task<OrderListingDto> GetOrders(OrderSearchFilterOptions filterOptions);
        Task<(int orgId, string ordNumber, OrgType? orgType, string orgName, string ordWareHouseEmail, int wareHouseId)> ApproveOrder(Guid orderId, string comments, OrderEntityType? orderEntityType);
        Task<(int orgId, string ordNumber, string orgName, OrgType? orgType, string ordWareHouseEmail, int userId, int wareHouseId)> RejectOrder(Guid orderId, string comments, OrderEntityType? orderEntityType);
        Task<(OrderDto orderDto, string errorMessage)> FullFillOrderItem(FullFillOrderDto fullFillOrderDto, OrderEntityType? orderEntityType);
        Boolean GetOrderForDeactivation(int id);
        Boolean GetOrderForProductDeactivation(int productId);
        Boolean GetOrderForReelSizeDeactivation(int reelSizeId);
        Boolean GetOrderForSKUDeactivation(int SkuId);
        List<MiniOrderDto> GetOrderDownloadList(OrderSearchFilterOptions filterOptions);

        Task<(string orgName, string orderNumber, int warehouseId, int orgId)> CloseOrder(Guid orderId, OrderEntityType? orderEntityType);
        Task<(AutoFillFullfillOrderItemDto, string Error)> AutoFillFullfillmentForOrderItem(string orderItemId);
        Task<string> RemovePackageLock(PackageDto packageDto);
        Task<(List<PackageDto> packageDtos, AutoFillFullfillOrderItemDto unFullfilledPackages, OrderItemDto orderItemDto)> FetchPackagesOfOrderItemId(string orderItemId, OrderItem orderItem);
        Task<List<string>> AddPackageLock(LockPackageDto lockPackageDto);
        Task<bool> MakePayment(Guid orderId, PaymentRepsonseDto paymentRepsonseDto);
        (int count, bool IsImporter) GetOrderExpiredStatusDetails(int orgId);
        bool ImporterHasDomesticProducts(int orgId);
        Task<bool> MakeOfflinePayment(Guid orderId, PaymentRepsonseDto paymentRepsonseDto);


    }
}
