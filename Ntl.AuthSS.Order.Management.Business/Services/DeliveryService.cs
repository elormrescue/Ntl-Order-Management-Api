using Microsoft.EntityFrameworkCore;
using Ntl.AuthSS.OrderManagement.Data;
using Ntl.AuthSS.OrderManagement.Data.Entities;
using Ntl.AuthSS.OrderManagement.Business.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ntl.AuthSS.OrderManagement.Business.Services
{
    public class DeliveryService : IDeliveryService
    {
        private readonly OrderManagementDbContext _orderMgmtDbContext;
        public DeliveryService(OrderManagementDbContext orderManagementDbContext)
        {
            _orderMgmtDbContext = orderManagementDbContext;
        }
        public async Task<DeliveryListingDto> GetDeliveries(DeliveryFilterOptions deliveryFilterOptions, int shipperId, int? userId)
        {
            var deliveryDtos = new List<DeliveryDto>();
            int readyToDeliverCount = 0;
            int transitCount = 0;
            int deliveredCount = 0;
            int totalCount = 0;

            switch (deliveryFilterOptions.DeliveryType)
            {
                case DeliveryType.Mf:
                case DeliveryType.Tpsaf:
                    var mfTpsafDeliveries = await GetMfTpsafOrderDeliveries(deliveryFilterOptions, shipperId, userId);
                    deliveryDtos.AddRange(mfTpsafDeliveries.deliveries);

                    readyToDeliverCount = mfTpsafDeliveries.readyToDeliverCount;
                    transitCount = mfTpsafDeliveries.transitCount;
                    deliveredCount = mfTpsafDeliveries.deliveredCount;
                    totalCount = mfTpsafDeliveries.totalCount;
                    break;
                case DeliveryType.Return:
                    var returnOrderDeliveries = await GetReturnOrderDeliveries(deliveryFilterOptions, shipperId, userId);
                    deliveryDtos.AddRange(returnOrderDeliveries.deliveries);

                    readyToDeliverCount = returnOrderDeliveries.readyToDeliverCount;
                    transitCount = returnOrderDeliveries.transitCount;
                    deliveredCount = returnOrderDeliveries.deliveredCount;
                    totalCount = returnOrderDeliveries.totalCount;
                    break;
            }


            var deliveryListingDto = new DeliveryListingDto();
            deliveryListingDto.DeliveryType = deliveryFilterOptions.DeliveryType;
            deliveryListingDto.ReadyToBeDeliveredCount = readyToDeliverCount;
            deliveryListingDto.TransitCount = transitCount;
            deliveryListingDto.DeliveredCount = deliveredCount;
            deliveryListingDto.TotalCount = totalCount;
            deliveryListingDto.Deliveries = deliveryDtos;

            return deliveryListingDto;
        }
        public async Task<(string otp, string phoneNo)> PickupDelivery(Guid id, DeliveryType deliveryType, string userName)
        {
            string otp = string.Empty;
            string phoneNo = string.Empty;
            switch (deliveryType)
            {
                case DeliveryType.Mf:
                case DeliveryType.Tpsaf:
                    var order = await _orderMgmtDbContext.Orders.Include(x => x.Warehouse).ThenInclude(x => x.Contact).Include(x => x.OrderHistories).SingleOrDefaultAsync(x => x.Id == id);
                    if (order != null)
                    {
                        order.Status = OrderStatus.InTransit;
                        order.DeliveryOtp = new Random().Next(1000, 9999);
                        otp = order.DeliveryOtp.ToString();
                        phoneNo = $"{order.Warehouse.Contact.CountryCode}{order.Warehouse.Contact.Phone}";
                        var orderHistory = new OrderHistory();
                        orderHistory.Action = OrderStatus.InTransit;
                        orderHistory.ActionedBy = OrgType.Shipper;
                        orderHistory.Comments = string.Empty;
                        orderHistory.Order = order;
                        order.OrderHistories.Add(orderHistory);
                        await _orderMgmtDbContext.SaveChangesAsync();
                    }
                    break;
                case DeliveryType.Return:
                    var returnOrder = await _orderMgmtDbContext.ReturnOrders.Include(x => x.Warehouse).ThenInclude(x => x.Contact).Include(x => x.ReturnOrderHistories).SingleOrDefaultAsync(x => x.Id == id);
                    if (returnOrder != null)
                    {
                        returnOrder.Status = ReturnOrderStatus.InTransit;
                        returnOrder.DeliveryOtp = new Random().Next(1000, 9999);
                        otp = returnOrder.DeliveryOtp.ToString();
                        phoneNo = $"{returnOrder.Warehouse.Contact.CountryCode}{returnOrder.Warehouse.Contact.Phone}";
                        var returnOrderHistory = new ReturnOrderHistory();
                        returnOrderHistory.Action = ReturnOrderStatus.InTransit;
                        returnOrderHistory.ActionedBy = OrgType.Shipper;
                        returnOrderHistory.Comments = string.Empty;
                        returnOrderHistory.ReturnOrder = returnOrder;
                        returnOrder.ReturnOrderHistories.Add(returnOrderHistory);
                        await _orderMgmtDbContext.SaveChangesAsync();
                    }
                    break;
            }
            return (otp, phoneNo);
        }

        public async Task Deliver(Guid id, DeliveryType deliveryType, string remarks, string userName)
        {
            switch (deliveryType)
            {
                case DeliveryType.Mf:
                case DeliveryType.Tpsaf:
                    var order = await _orderMgmtDbContext.Orders.Include(x => x.OrderHistories).SingleAsync(x => x.Id == id);
                    order.DeliveryRemarks = remarks;
                    order.Status = OrderStatus.Closed;
                    var orderHistory = new OrderHistory();
                    orderHistory.Action = OrderStatus.Closed;
                    orderHistory.ActionedBy = OrgType.Shipper;
                    orderHistory.Comments = remarks;
                    orderHistory.Order = order;
                    order.OrderHistories.Add(orderHistory);
                    await _orderMgmtDbContext.SaveChangesAsync();
                    break;
                case DeliveryType.Return:
                    var returnOrder = await _orderMgmtDbContext.ReturnOrders.Include(x => x.ReturnOrderHistories).SingleAsync(x => x.Id == id);
                    returnOrder.DeliveryRemarks = remarks;
                    returnOrder.Status = ReturnOrderStatus.Closed;
                    var returnOrderHistory = new ReturnOrderHistory();
                    returnOrderHistory.Action = ReturnOrderStatus.Closed;
                    returnOrderHistory.ActionedBy = OrgType.Shipper;
                    returnOrderHistory.Comments = remarks;
                    returnOrderHistory.ReturnOrder = returnOrder;
                    returnOrder.ReturnOrderHistories.Add(returnOrderHistory);
                    await _orderMgmtDbContext.SaveChangesAsync();
                    break;
            }

        }

        public bool IsDeliveryOtpValid(Guid id, DeliveryType deliveryType, int otp)
        {
            int deliveryOtp = 0;
            switch (deliveryType)
            {
                case DeliveryType.Mf:
                case DeliveryType.Tpsaf:
                    deliveryOtp = _orderMgmtDbContext.Orders.Single(x => x.Id == id).DeliveryOtp.Value;
                    break;
                case DeliveryType.Return:
                    deliveryOtp = _orderMgmtDbContext.ReturnOrders.Single(x => x.Id == id).DeliveryOtp.Value;
                    break;
            }

            return deliveryOtp == otp;
        }

        private async Task<(ICollection<DeliveryDto> deliveries, int readyToDeliverCount, int transitCount, int deliveredCount, int totalCount)> GetMfTpsafOrderDeliveries(DeliveryFilterOptions deliveryFilterOptions, int shipperId, int? userId)
        {
            var deliveryStatuses = new List<OrderStatus>();
            if (userId == null)
                deliveryStatuses.Add(OrderStatus.Fullfilled);
            else
            {
                deliveryStatuses.Add(OrderStatus.InTransit);
                //deliveryStatuses.Add(OrderStatus.Delivered);
                deliveryStatuses.Add(OrderStatus.Closed);
            }


            var deliveries = _orderMgmtDbContext.Orders.Where(x => deliveryStatuses.Contains(x.Status) && x.Shipper.Id == shipperId);

            if (userId != null)
            {
                deliveries = _orderMgmtDbContext.Orders.Where(x => x.ModifiedUser == userId);
            }

            if (deliveryFilterOptions.OrderId != Guid.Empty)
            {
                deliveries = _orderMgmtDbContext.Orders.Where(x => x.Id == deliveryFilterOptions.OrderId);
            }

            if (deliveryFilterOptions.DeliveryType == DeliveryType.Mf)
            {
                deliveries = deliveries.Where(x => x.Organization.OrgType == OrgType.Manufacturer);
            }

            else if (deliveryFilterOptions.DeliveryType == DeliveryType.Tpsaf)
            {
                deliveries = deliveries.Where(x => x.Organization.OrgType == OrgType.Tpsaf);
            }

            if (!string.IsNullOrEmpty(deliveryFilterOptions.SearchText))
            {
                deliveries = deliveries.Where(x => x.Organization.Name.Contains(deliveryFilterOptions.SearchText)
                || x.Organization.Warehouses.Any(x => x.Warehouse.Name.Contains(deliveryFilterOptions.SearchText)
                || x.Warehouse.Address.Description.Contains(deliveryFilterOptions.SearchText)
                || x.Warehouse.Contact.Name.Contains(deliveryFilterOptions.SearchText)
                || x.Warehouse.Contact.Phone.Contains(deliveryFilterOptions.SearchText)
                || x.Warehouse.Contact.Email.Contains(deliveryFilterOptions.SearchText)));
            }

            deliveries = deliveries.OrderByDescending(x => x.ModifiedDate);

            var readyToDeliverCount = deliveries.Count(x => x.Status == OrderStatus.Fullfilled);
            var transitCount = deliveries.Count(x => x.Status == OrderStatus.InTransit);
            var deliveredCount = deliveries.Count(x => x.Status == OrderStatus.Closed);
            var totalCount = deliveries.Count();

            deliveryFilterOptions.PageNo = deliveryFilterOptions.PageNo == 0 ? 1 : deliveryFilterOptions.PageNo;
            deliveryFilterOptions.PageSize = deliveryFilterOptions.PageSize == 0 ? 10 : deliveryFilterOptions.PageSize;
            deliveries = deliveries.Skip((deliveryFilterOptions.PageNo - 1) * deliveryFilterOptions.PageSize).Take(deliveryFilterOptions.PageSize);

            var deliveriesMaterialised = await deliveries.Select(x => new
            {
                OrderId = x.Id,
                OrderNumber = x.Number,
                OrderStatus = x.Status,
                OrgType = x.Organization.OrgType,
                FullfilledDate = x.OrderHistories.FirstOrDefault(y => y.Action == OrderStatus.Fullfilled).ModifiedDate,
                OrgName = x.Organization.Name,
                RecepientName = x.Warehouse.Contact.Name,
                Address = x.Warehouse.Address.Description,
                x.Warehouse.Contact.Phone,
                x.ModifiedDate
            }).ToListAsync();

            var deliveryDtoList = new List<DeliveryDto>();
            foreach (var materialisedDelivery in deliveriesMaterialised)
            {
                var deliveryDto = new DeliveryDto();
                deliveryDto.Address = materialisedDelivery.Address;
                deliveryDto.ContactNo = materialisedDelivery.Phone;
                deliveryDto.DeliveryType = materialisedDelivery.OrgType == OrgType.Manufacturer ? DeliveryType.Mf : DeliveryType.Tpsaf;
                deliveryDto.DeliveryTypeString = deliveryDto.DeliveryType.ToString();
                deliveryDto.FullfilledDateTime = materialisedDelivery.FullfilledDate;
                deliveryDto.FullfilledDate = deliveryDto.FullfilledDateTime == null ? string.Empty : deliveryDto.FullfilledDateTime.Value.ToDisplayDate();
                deliveryDto.OrderId = materialisedDelivery.OrderId;
                deliveryDto.OrderNumber = materialisedDelivery.OrderNumber;
                deliveryDto.OrgName = materialisedDelivery.OrgName;
                deliveryDto.RecepientName = materialisedDelivery.RecepientName;
                deliveryDto.OrderStatusString = materialisedDelivery.OrderStatus.ToString();
                deliveryDto.DeliveryStatus = materialisedDelivery.OrderStatus == OrderStatus.Fullfilled ? DeliveryStatus.ReadyToBeDelivered : materialisedDelivery.OrderStatus == OrderStatus.InTransit ? DeliveryStatus.InTransit : DeliveryStatus.Delivered;
                deliveryDto.DeliveryStatusString = deliveryDto.DeliveryStatus.ToString();
                deliveryDto.ModifiedDate = materialisedDelivery.ModifiedDate;
                deliveryDto.ModifiedDateString = materialisedDelivery.ModifiedDate.ToDisplayDate();
                deliveryDtoList.Add(deliveryDto);
            }

            return (deliveryDtoList, readyToDeliverCount, transitCount, deliveredCount, totalCount);
        }
        private async Task<(ICollection<DeliveryDto> deliveries, int readyToDeliverCount, int transitCount, int deliveredCount, int totalCount)> GetReturnOrderDeliveries(DeliveryFilterOptions deliveryFilterOptions, int shipperId, int? userId)
        {
            var deliveryStatuses = new List<ReturnOrderStatus>();
            if (userId == null)
                deliveryStatuses.Add(ReturnOrderStatus.Submitted);
            else
            {
                deliveryStatuses.Add(ReturnOrderStatus.InTransit);
                //deliveryStatuses.Add(ReturnOrderStatus.Delivered);
                deliveryStatuses.Add(ReturnOrderStatus.Closed);
            }

            var deliveries = _orderMgmtDbContext.ReturnOrders.Where(x => deliveryStatuses.Contains(x.Status) && x.Shipper.Id == shipperId);

            if (userId != null)
            {
                deliveries = _orderMgmtDbContext.ReturnOrders.Where(x => x.ModifiedUser == userId);
            }

            if (deliveryFilterOptions.OrderId != Guid.Empty)
            {
                deliveries = _orderMgmtDbContext.ReturnOrders.Where(x => x.Id == deliveryFilterOptions.OrderId);
            }

            if (!string.IsNullOrEmpty(deliveryFilterOptions.SearchText))
            {
                deliveries = deliveries.Where(x => x.Organization.Name.Contains(deliveryFilterOptions.SearchText)
                || x.Organization.Warehouses.Any(x => x.Warehouse.Name.Contains(deliveryFilterOptions.SearchText)
                || x.Warehouse.Address.Description.Contains(deliveryFilterOptions.SearchText)
                || x.Warehouse.Contact.Name.Contains(deliveryFilterOptions.SearchText)
                || x.Warehouse.Contact.Phone.Contains(deliveryFilterOptions.SearchText)
                || x.Warehouse.Contact.Email.Contains(deliveryFilterOptions.SearchText)));
            }

            deliveries = deliveries.OrderByDescending(x => x.ModifiedDate);

            var readyToDeliverCount = deliveries.Count(x => x.Status == ReturnOrderStatus.Submitted);
            var transitCount = deliveries.Count(x => x.Status == ReturnOrderStatus.InTransit);
            var deliveredCount = deliveries.Count(x => x.Status == ReturnOrderStatus.Closed);
            var totalCount = deliveries.Count();

            deliveryFilterOptions.PageNo = deliveryFilterOptions.PageNo == 0 ? 1 : deliveryFilterOptions.PageNo;
            deliveryFilterOptions.PageSize = deliveryFilterOptions.PageSize == 0 ? 10 : deliveryFilterOptions.PageSize;
            deliveries = deliveries.Skip((deliveryFilterOptions.PageNo - 1) * deliveryFilterOptions.PageSize).Take(deliveryFilterOptions.PageSize);

            var deliveriesMaterialised = await deliveries.Select(x => new
            {
                OrderId = x.Id,
                OrderNumber = x.Number,
                OrderStatus = x.Status,
                OrgType = x.Organization.OrgType,
                FullfilledDate = x.ReturnOrderHistories.FirstOrDefault(y => y.Action == ReturnOrderStatus.Submitted).ModifiedDate,
                OrgName = x.Organization.Name,
                RecepientName = x.Warehouse.Contact.Name,
                Address = x.Warehouse.Address.Description,
                x.Warehouse.Contact.Phone,
                x.ModifiedDate
            }).ToListAsync();

            var deliveryDtoList = new List<DeliveryDto>();
            foreach (var materialisedDelivery in deliveriesMaterialised)
            {
                var deliveryDto = new DeliveryDto();
                deliveryDto.Address = materialisedDelivery.Address;
                deliveryDto.ContactNo = materialisedDelivery.Phone;
                deliveryDto.DeliveryType = DeliveryType.Return;
                deliveryDto.DeliveryTypeString = deliveryDto.DeliveryType.ToString();
                deliveryDto.FullfilledDateTime = materialisedDelivery.FullfilledDate;
                deliveryDto.FullfilledDate = deliveryDto.FullfilledDateTime == null ? string.Empty : deliveryDto.FullfilledDateTime.Value.ToShortDateString();
                deliveryDto.OrderId = materialisedDelivery.OrderId;
                deliveryDto.OrderNumber = materialisedDelivery.OrderNumber;
                deliveryDto.OrgName = materialisedDelivery.OrgName;
                deliveryDto.RecepientName = materialisedDelivery.RecepientName;
                deliveryDto.OrderStatusString = materialisedDelivery.OrderStatus.ToString();
                deliveryDto.DeliveryStatus = materialisedDelivery.OrderStatus == ReturnOrderStatus.Submitted ? DeliveryStatus.ReadyToBeDelivered : materialisedDelivery.OrderStatus == ReturnOrderStatus.InTransit ? DeliveryStatus.InTransit : DeliveryStatus.Delivered;
                deliveryDto.DeliveryStatusString = deliveryDto.DeliveryStatus.ToString();
                deliveryDto.ModifiedDate = materialisedDelivery.ModifiedDate;
                deliveryDto.ModifiedDateString = materialisedDelivery.ModifiedDate.ToDisplayDate();
                deliveryDtoList.Add(deliveryDto);
            }

            return (deliveryDtoList, readyToDeliverCount, transitCount, deliveredCount, totalCount);
        }

        public async Task<Order> GetOrderDetails(Guid orderId)
        {
            return await _orderMgmtDbContext.Orders.Include(o => o.Shipper).Include(o => o.Organization).Include(o => o.Organization.Contact).Include(o => o.Warehouse).Include(o => o.Warehouse.Contact).Include(o => o.Shipper).Where(o => o.Id == orderId).SingleOrDefaultAsync();
        }
    }
}
