using System;
using System.IO;
using OfficeOpenXml;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ntl.AuthSS.Order_Management.Api.Models;
using Ntl.AuthSS.OrderManagement.Business;
using Ntl.AuthSS.OrderManagement.Business.Services;
using Ntl.AuthSS.OrderManagement.Data.Entities;
using Utilities.NotificationHelper;
using Microsoft.Extensions.Configuration;
using Ntl.AuthSS.Order_Management.Api.FcmNotificationModels;
using Ntl.AuthSS.Order_Management.Api.HttpClients;
using Ntl.AuthSS.OrderManagement.Business.NotificationModel;

namespace Ntl.AuthSS.Order_Management.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : BaseController
    {
        private readonly IOrderService _orderService;
        private readonly IConfiguration _configuration;
        private readonly IOrderMetaService _orderMetaService;
        private readonly INotificationHelper _notificationHelper;
        private readonly CosmosAccessConfig _cosmosAccessConfig;
        //private readonly WebNotificationContentTemplate _webNotificationContent;
        private readonly EmailNotificationTemplate _emailNotification;
        private readonly UserClient _userClient;
        private readonly FcmClient _fcmClient;
        private readonly ICosmosDbService _notifcationCosmosDb;
        public OrdersController(IOrderService orderService, IOrderMetaService orderMetaService, INotificationHelper notificationHelper, IConfiguration configuration, UserClient userClient, FcmClient fcmClient, CosmosAccessConfig cosmosAccessConfig, ICosmosDbService notifcationCosmosDb)
        {
            _orderService = orderService;
            _orderMetaService = orderMetaService;
            _configuration = configuration;
            _notificationHelper = notificationHelper;
            _userClient = userClient;
            _fcmClient = fcmClient;
            _cosmosAccessConfig = cosmosAccessConfig;
            _emailNotification = new EmailNotificationTemplate(_configuration, _userClient);
            //_webNotificationContent = new WebNotificationContentTemplate(_cosmosAccessConfig);
            _notifcationCosmosDb = notifcationCosmosDb;
        }

        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "CanCreateOrders")]
        public async Task<IActionResult> SaveOrder(OrderDto orderDto)
        {
            var order = await _orderService.SaveOrderAsync(orderDto, GetEntityOrgId(), GetOrderEntityType().Value);

            var mailDataForOrgUser = order.Status == OrderStatus.Submitted ?
                _emailNotification.BuildOrderPlacedTemplateForOrgUsers(order.Id, order.Number, order.Organization.Id, order.Warehouse.Contact.Email)
                : _emailNotification.BuildOrderResubmitedTemplateForOrgAdmins(order.Id, order.Number, order.Organization.Id, order.Warehouse.Contact.Email);
            var mailDataForPortalAdmin = order.Status == OrderStatus.Submitted ?
                _emailNotification.BuildOrderPlacedTemplateForPortalAdmins(order.Id, order.Number, order.Organization.Name)
                : _emailNotification.BuildOrderResubmitedTemplateForPortalAdmins(order.Id, order.Number, GetUserName());

            //email notifications
            await _notificationHelper.SendNotificationToQueue(mailDataForOrgUser);
            await _notificationHelper.SendNotificationToQueue(mailDataForPortalAdmin);

            //web notifications
            var orgType = GetOrderEntityType();
            if (HttpContext.Request.Headers.TryGetValue("Authorization", out var token))
            {
                if (orgType == OrderEntityType.Manufacturer)
                {
                    var webContent = new Dictionary<string, string>();
                    webContent.Add("OrderNumber", order.Number);
                    webContent.Add("MfName", order.Organization.Name);
                    webContent.Add("WareHouseId", order.Warehouse.Id.ToString());
                    var trimmedToken = token.ToString().Substring(7);
                    if (order.Status == OrderStatus.Submitted)
                        //await _webNotificationContent.SendMsgContentToDb(trimmedToken, WebNotificationType.MfOrderSubmitted, webContent);
                        await _notifcationCosmosDb.AddItemAsync(trimmedToken, WebNotificationType.MfOrderSubmitted, webContent);
                    else if (order.Status == OrderStatus.Resubmitted)
                        //await _webNotificationContent.SendMsgContentToDb(trimmedToken, WebNotificationType.MfOrderReSubmitted, webContent);
                        await _notifcationCosmosDb.AddItemAsync(trimmedToken, WebNotificationType.MfOrderReSubmitted, webContent);
                }
                else
                {
                    var webContent = new Dictionary<string, string>();
                    webContent.Add("StockOrderNumber", order.Number);
                    webContent.Add("TpsafName", order.Organization.Name);
                    webContent.Add("WareHouseId", order.Warehouse.Id.ToString());
                    var trimmedToken = token.ToString().Substring(7);
                    if (order.Status == OrderStatus.Submitted)
                        //await _webNotificationContent.SendMsgContentToDb(trimmedToken, WebNotificationType.MfOrderSubmitted, webContent);
                        await _notifcationCosmosDb.AddItemAsync(trimmedToken, WebNotificationType.StockOrderSubmitted, webContent);
                    else if (order.Status == OrderStatus.Resubmitted)
                        //await _webNotificationContent.SendMsgContentToDb(trimmedToken, WebNotificationType.MfOrderReSubmitted, webContent);
                        await _notifcationCosmosDb.AddItemAsync(trimmedToken, WebNotificationType.StockOrderReSubmitted, webContent);
                }
            }
            return Ok("success");
        }

        [HttpPost]
        [Route("[action]")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "CanApproveRejectOrders")]
        public async Task<IActionResult> ApproveOrder(OrderApprovalActionModel orderApprovalActionModel)
        {
            var orderDetails = await _orderService.ApproveOrder(orderApprovalActionModel.OrderId, orderApprovalActionModel.Comments, GetOrderEntityType());

            await _notificationHelper.SendNotificationToQueue(_emailNotification.BuildOrderApprovedTemplateForOrgUsers(orderApprovalActionModel.OrderId, orderDetails.orgId, orderDetails.ordNumber, orderDetails.ordWareHouseEmail));
            await _notificationHelper.SendNotificationToQueue(_emailNotification.BuildOrderApprovedTemplateForPortalAdmins(orderApprovalActionModel.OrderId, orderDetails.ordNumber, orderDetails.orgName));
            await _notificationHelper.SendNotificationToQueue(_emailNotification.BuildOrderApprovedTemplateForFinanceOfficer(orderApprovalActionModel.OrderId, orderDetails.ordNumber, orderDetails.orgName));

            //web notifications
            if (HttpContext.Request.Headers.TryGetValue("Authorization", out var token))
            {
                if(orderDetails.orgType == OrgType.Manufacturer)
                {
                    var webContent = new Dictionary<string, string>();
                    webContent.Add("OrderNumber", orderDetails.ordNumber);
                    webContent.Add("MfName", orderDetails.orgName);
                    webContent.Add("MfOrgId", orderDetails.orgId.ToString());
                    webContent.Add("WareHouseId", orderDetails.wareHouseId.ToString());
                    var trimmedToken = token.ToString().Substring(7);
                    //await _webNotificationContent.SendMsgContentToDb(trimmedToken, WebNotificationType.MfOrderApproved, webContent);
                    await _notifcationCosmosDb.AddItemAsync(trimmedToken, WebNotificationType.MfOrderApproved, webContent);
                }
                else
                {
                    var webContent = new Dictionary<string, string>();
                    webContent.Add("StockOrderNumber", orderDetails.ordNumber);
                    webContent.Add("TpsafName", orderDetails.orgName);
                    webContent.Add("TpsafOrgId", orderDetails.orgId.ToString());
                    webContent.Add("WareHouseId", orderDetails.wareHouseId.ToString());
                    var trimmedToken = token.ToString().Substring(7);
                    //await _webNotificationContent.SendMsgContentToDb(trimmedToken, WebNotificationType.MfOrderApproved, webContent);
                    await _notifcationCosmosDb.AddItemAsync(trimmedToken, WebNotificationType.StockOrderApproved, webContent);
                }
            }
            return Ok("Success");
        }

        [HttpPost]
        [Route("[action]")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "CanApproveRejectOrders")]
        public async Task<IActionResult> RejectOrder(OrderRejectActionModel orderRejectActionModel)
        {
            var orderDetails = await _orderService.RejectOrder(orderRejectActionModel.OrderId, orderRejectActionModel.Comments, GetOrderEntityType());

            //email notifications
            await _notificationHelper.SendNotificationToQueue(_emailNotification.BuildOrderRejectedTemplateForOrgUsers(orderRejectActionModel.OrderId, orderDetails.orgId, orderDetails.ordNumber, orderRejectActionModel.Comments, orderDetails.ordWareHouseEmail));
            await _notificationHelper.SendNotificationToQueue(_emailNotification.BuildOrderRejectedTemplateForPortalAdmins(orderRejectActionModel.OrderId, orderDetails.ordNumber, orderRejectActionModel.Comments));
            await _notificationHelper.SendNotificationToQueue(_emailNotification.BuildOrderRejectedTemplateForSubmittedUser(orderRejectActionModel.OrderId, orderDetails.ordNumber, orderRejectActionModel.Comments, orderDetails.userId));

            //web notifications
            if (HttpContext.Request.Headers.TryGetValue("Authorization", out var token))
            {
                if(orderDetails.orgType == OrgType.Manufacturer)
                {
                    var webContent = new Dictionary<string, string>();
                    webContent.Add("OrderNumber", orderDetails.ordNumber);
                    webContent.Add("MfName", orderDetails.orgName);
                    webContent.Add("MfOrgId", orderDetails.orgId.ToString());
                    webContent.Add("WareHouseId", orderDetails.wareHouseId.ToString());
                    var trimmedToken = token.ToString().Substring(7);
                    //await _webNotificationContent.SendMsgContentToDb(trimmedToken, WebNotificationType.MfOrderRejected, webContent);
                    await _notifcationCosmosDb.AddItemAsync(trimmedToken, WebNotificationType.StockOrderRejected, webContent);
                }
                else
                {
                    var webContent = new Dictionary<string, string>();
                    webContent.Add("StockOrderNumber", orderDetails.ordNumber);
                    webContent.Add("TpsafName", orderDetails.orgName);
                    webContent.Add("TpsafOrgId", orderDetails.orgId.ToString());
                    webContent.Add("WareHouseId", orderDetails.wareHouseId.ToString());
                    var trimmedToken = token.ToString().Substring(7);
                    //await _webNotificationContent.SendMsgContentToDb(trimmedToken, WebNotificationType.MfOrderRejected, webContent);
                    await _notifcationCosmosDb.AddItemAsync(trimmedToken, WebNotificationType.StockOrderRejected, webContent);
                }
            }
            return Ok("Success");
        }

        [HttpGet]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "CanViewOrders")]
        public async Task<IActionResult> GetOrder(Guid orderId)
        {
            return Ok(await _orderService.GetOrderAsync(orderId));
        }

        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "CanViewOrders")]
        public async Task<IActionResult> GetOrders(OrderSearchFilterOptions filterOptions)
        {
            var role = GetEntityRole();
            var orgId = GetEntityOrgId();
            switch (role)
            {
                case "MfAdmin":
                case "MfAccountManager":
                    filterOptions.EntityIds = new int?[] { orgId };
                    filterOptions.OrgType = OrgType.Manufacturer;
                    return Ok(await _orderService.GetOrders(filterOptions));
                case "MfWarehouseIncharge":
                    filterOptions.EntityIds = new int?[] { orgId };
                    filterOptions.Locations = new int?[] { GetEntityLocation() };
                    filterOptions.OrgType = OrgType.Manufacturer;
                    return Ok(await _orderService.GetOrders(filterOptions));
                case "TpsafAdmin":
                    filterOptions.EntityIds = new int?[] { orgId };
                    filterOptions.OrgType = OrgType.Tpsaf;
                    return Ok(await _orderService.GetOrders(filterOptions));
                case "TpsafFacilityAdmin":
                case "TpsafFacilityIncharge":
                    filterOptions.EntityIds = new int?[] { orgId };
                    filterOptions.Locations = new int?[] { GetEntityLocation() };
                    filterOptions.OrgType = OrgType.Tpsaf;
                    return Ok(await _orderService.GetOrders(filterOptions));
                default:
                    return Ok(await _orderService.GetOrders(filterOptions));
            }
        }

        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "CanFulfillOrders")]
        public async Task<IActionResult> FullFillOrder(FullFillOrderDto fullFillOrder)
        {
            var result = await _orderService.FullFillOrderItem(fullFillOrder, GetOrderEntityType());
            if (result.errorMessage != null)
                return BadRequest(result.errorMessage);
            var userDevices = await _userClient.GetUserDevices(fullFillOrder.shipper.ShipperId);
            foreach (var userDevice in userDevices)
            {
                var fcmNotifyMessage = new FcmNotifyMessage()
                {
                    Notification = new
                    {
                        title = "Order FullFilled",
                        body = "Order with Number " + fullFillOrder.OrderNumber + " is fullfilled by NTL and has to be Delivered to " + result.orderDto.OrgName
                    },
                    Data = new
                    {
                        ntlNotificationType = ntlNotificationType.OrderFullFill,
                        orderNo = fullFillOrder.OrderNumber
                    }
                };
                await _fcmClient.SendFcmMessage(fcmNotifyMessage, userDevice.AppToken);
            }

            //web notifications
            if (HttpContext.Request.Headers.TryGetValue("Authorization", out var token))
            {
                if(result.orderDto.OrgType == (int)OrgType.Manufacturer)
                {
                    var webContent = new Dictionary<string, string>();
                    webContent.Add("OrderNumber", result.orderDto.OrderNumber);
                    webContent.Add("MfName", result.orderDto.OrgName);
                    webContent.Add("MfOrgId", result.orderDto.OrgId.ToString());
                    webContent.Add("WareHouseId", result.orderDto.WarehouseId.ToString());
                    var trimmedToken = token.ToString().Substring(7);
                    //await _webNotificationContent.SendMsgContentToDb(trimmedToken, WebNotificationType.MfOrderRejected, webContent);
                    await _notifcationCosmosDb.AddItemAsync(trimmedToken, WebNotificationType.MfOrderFullfilled, webContent);
                }
                else
                {
                    var webContent = new Dictionary<string, string>();
                    webContent.Add("StockOrderNumber", result.orderDto.OrderNumber);
                    webContent.Add("TpsafName", result.orderDto.OrgName);
                    webContent.Add("TpsafOrgId", result.orderDto.OrgId.ToString());
                    webContent.Add("WareHouseId", result.orderDto.WarehouseId.ToString());
                    var trimmedToken = token.ToString().Substring(7);
                    //await _webNotificationContent.SendMsgContentToDb(trimmedToken, WebNotificationType.MfOrderRejected, webContent);
                    await _notifcationCosmosDb.AddItemAsync(trimmedToken, WebNotificationType.StockOrderFullfilled, webContent);
                }
            }
            return Ok("Success");
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public Boolean GetOrderForDeactivation(int orgId)
        {
            return _orderService.GetOrderForDeactivation(orgId);
        }


        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public Boolean GetProductOrders(int productId)
        {
            return _orderService.GetOrderForProductDeactivation(productId);
        }
        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public Boolean GetReelSizeOrders(int reelSizeId)
        {
            return _orderService.GetOrderForReelSizeDeactivation(reelSizeId);
        }
        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public Boolean GetSkuOrders(int SkuId)
        {
            return _orderService.GetOrderForSKUDeactivation(SkuId);
        }
        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "CanViewOrders")]
        public IActionResult DownloadFilteredOrders(OrderSearchFilterOptions filterOptions)
        {
            var role = GetEntityRole();
            var orgId = GetEntityOrgId();
            var orderData = new List<MiniOrderDto>();
            switch (role)
            {
                case "MfAdmin":
                case "MfAccountManager":
                    filterOptions.EntityIds = new int?[] { orgId };
                    filterOptions.OrgType = OrgType.Manufacturer;
                    orderData = _orderService.GetOrderDownloadList(filterOptions);
                    break;
                case "MfWarehouseIncharge":
                    filterOptions.EntityIds = new int?[] { orgId };
                    filterOptions.Locations = new int?[] { GetEntityLocation() };
                    filterOptions.OrgType = OrgType.Manufacturer;
                    orderData = _orderService.GetOrderDownloadList(filterOptions);
                    break;
                case "TpsafAdmin":
                case "TpsafFacilityAdmin":
                case "TpsafFacilityIncharge":
                    filterOptions.EntityIds = new int?[] { orgId };
                    filterOptions.Locations = new int?[] { GetEntityLocation() };
                    filterOptions.OrgType = OrgType.Tpsaf;
                    orderData = _orderService.GetOrderDownloadList(filterOptions);
                    break;
                default:
                    orderData = _orderService.GetOrderDownloadList(filterOptions);
                    break;
            }
            var result = orderData.Select(o => new { OrderNumber = o.OrderNumber, Organization = o.OrgName, RequestedOn = o.RequestedOn, Status = o.Status, PaymentStatus = o.PaymentStatus, PayableAmount = o.PayableAmount }).ToList(); ;
            if (result == null)
            {
                return BadRequest("not found");
            }
            else
            {
                using (var excelFile = new ExcelPackage())
                {
                    var worksheet = excelFile.Workbook.Worksheets.Add("Sheet1");
                    worksheet.Cells["A1"].LoadFromCollection(Collection: result, PrintHeaders: true);
                    var streamedData = new MemoryStream(excelFile.GetAsByteArray());
                    return File(streamedData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "OrderList_" + DateTime.Now.Ticks.ToString());
                }
            }
        }
        [HttpPost]
        [Route("[action]")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "CanCreateOrders")]
        public async Task<IActionResult> CloseOrder(Guid orderId)
        {
            var orgType = GetOrderEntityType();
            var orderDetails = await _orderService.CloseOrder(orderId, orgType);
            //web notifications
            if (HttpContext.Request.Headers.TryGetValue("Authorization", out var token))
            {
                if(orgType == OrderEntityType.Manufacturer)
                {
                    var webContent = new Dictionary<string, string>();
                    webContent.Add("OrderNumber", orderDetails.orderNumber);
                    webContent.Add("MfName", orderDetails.orgName);
                    webContent.Add("MfOrgId", orderDetails.orgId.ToString());
                    webContent.Add("WareHouseId", orderDetails.warehouseId.ToString());
                    var trimmedToken = token.ToString().Substring(7);
                    //await _webNotificationContent.SendMsgContentToDb(trimmedToken, WebNotificationType.MfOrderClosed, webContent);
                    await _notifcationCosmosDb.AddItemAsync(trimmedToken, WebNotificationType.MfOrderClosed, webContent);
                }
                else
                {
                    var webContent = new Dictionary<string, string>();
                    webContent.Add("StockOrderNumber", orderDetails.orderNumber);
                    webContent.Add("TpsafName", orderDetails.orgName);
                    webContent.Add("TpsafOrgId", orderDetails.orgId.ToString());
                    webContent.Add("WareHouseId", orderDetails.warehouseId.ToString());
                    var trimmedToken = token.ToString().Substring(7);
                    //await _webNotificationContent.SendMsgContentToDb(trimmedToken, WebNotificationType.MfOrderClosed, webContent);
                    await _notifcationCosmosDb.AddItemAsync(trimmedToken, WebNotificationType.StockOrderClosed, webContent);
                }
            }
            return Ok("Success");
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "CanFulfillOrders")]
        public async Task<IActionResult> AutoSelectAndFetchReelsForFullfillmentForOrderItemAsync(string orderItemId)
        {
            var result =await _orderService.AutoFillFullfillmentForOrderItem(orderItemId);
            if (result.Error != null)
                return BadRequest(result.Error);
            return Ok(result.Item1);
        }

        [HttpPost]
        [Route("[action]")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "CanFulfillOrders")]
        public async Task<IActionResult> RemovePackageForFullfillment(PackageDto packageDto)
        {
            var result = await _orderService.RemovePackageLock(packageDto);
            if (result != null)
                return BadRequest(result);
            return Ok("Package removed successfully");
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "ViewPackageTypes")]
        public async Task<IActionResult> GetPackageDataForPrintOrderItemBased(string orderItemId)
        {
            var result = await _orderService.FetchPackagesOfOrderItemId(orderItemId,null);
            if (result.orderItemDto == null)
                return BadRequest("No packages for the requested order item");
            return Ok(result.orderItemDto);
        }

        [HttpPost]
        [Route("[action]")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "CanFulfillOrders")]
        public async Task<IActionResult> AddPackageLock(LockPackageDto lockPackageDto)
        {
            var result = await _orderService.AddPackageLock(lockPackageDto);
            if (result.Count != 0)
                return BadRequest(result);
            return Ok("Package Locked successfully");
        }

        [HttpPost]
        [Route("[action]")]
        [Authorize(Policy = "CanCreateOrders")]
        public IActionResult ProcessOrderPayment(Guid orderId, PaymentRepsonseDto paymentRepsonseDto)
        {
            var res = _orderService.MakePayment(orderId,paymentRepsonseDto);
            return Ok(res);
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult GetOrderExpiredStatusWithOrgOrigin(int orgId)
        {
            var res = _orderService.GetOrderExpiredStatusDetails(orgId);
            return Ok(res);
        }
        [HttpGet]
        [Route("[action]")]
        public IActionResult GetIsImporterHasDomesticProducts(int orgId)
        {
            var res = _orderService.ImporterHasDomesticProducts(orgId);
            return Ok(res);
        }
        [HttpPost]
        [Route("[action]")]
        [Authorize(Policy = "CanCreateOrders")]
        public IActionResult ProcessOrderOfflinePayment(Guid orderId, PaymentRepsonseDto paymentRepsonseDto)
        {
            var res = _orderService.MakeOfflinePayment(orderId, paymentRepsonseDto);
            return Ok(res);
        }
    }
}
