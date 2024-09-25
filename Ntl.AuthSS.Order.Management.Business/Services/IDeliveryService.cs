using Ntl.AuthSS.OrderManagement.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ntl.AuthSS.OrderManagement.Business.Services
{
    public interface IDeliveryService
    {
        Task<DeliveryListingDto> GetDeliveries(DeliveryFilterOptions deliveryFilterOptions, int shipperId, int? userId);
        Task<(string otp, string phoneNo)> PickupDelivery(Guid id, DeliveryType deliveryType, string userName);
        Task Deliver(Guid id, DeliveryType deliveryType, string remarks, string userName);
        bool IsDeliveryOtpValid(Guid id, DeliveryType deliveryType, int otp);
        Task<Order> GetOrderDetails(Guid orderId);
    }
}
