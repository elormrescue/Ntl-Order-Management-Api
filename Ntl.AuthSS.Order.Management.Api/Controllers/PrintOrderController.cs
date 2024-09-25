using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using OfficeOpenXml;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Ntl.AuthSS.Order_Management.Api.Models;
using Ntl.AuthSS.OrderManagement.Business;
using Ntl.AuthSS.OrderManagement.Business.Services;
using Utilities.NotificationHelper;
using Azure.Storage.Queues;

namespace Ntl.AuthSS.Order_Management.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrintOrderController : BaseController
    {
        private readonly IPrintOrderService _printOrderService;
        private readonly IConfiguration _configuration;
        private readonly CosmosAccessConfig _cosmosAccessConfig;
        private readonly INotificationHelper _notificationHelper;
        //private readonly WebNotificationContentTemplate _webNotificationContent;
        private readonly ICosmosDbService _notifcationCosmosDb;
        private readonly EmailNotificationTemplate _emailNotification;
        private readonly UserClient _userClient;
        public PrintOrderController(IPrintOrderService printOrderService, IConfiguration configuration, CosmosAccessConfig cosmosAccessConfig,ICosmosDbService notifcationCosmosDb, INotificationHelper notificationHelper, UserClient userClient)
        {
            _printOrderService = printOrderService;
            _configuration = configuration;
            _cosmosAccessConfig = cosmosAccessConfig;
            //_webNotificationContent = new WebNotificationContentTemplate(_cosmosAccessConfig);
            _notifcationCosmosDb = notifcationCosmosDb;
            _notificationHelper = notificationHelper;
            _userClient = userClient;
            _emailNotification = new EmailNotificationTemplate(_configuration, _userClient);
        }
        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "CanCreateClosePrintOrders")]
        public async Task<IActionResult> SavePrintOrder(PrintOrderDto printOrder)
        {
            var orgId = GetEntityOrgId();
            var resData = await _printOrderService.SavePrintOrderAsync(printOrder);
            //foreach (var data in resData)
            //{
                await _notificationHelper.SendNotificationToQueue(_emailNotification.BuildPrintOrderPlacedTemplateForAdmins(/*data.printOrderId,*/orgId, resData.Count().ToString()));
                await _notificationHelper.SendNotificationToQueue(_emailNotification.BuildPrintOrderPlacedTemplateForPrintPartner(/*data.printOrderId,*/ resData.Count().ToString(), resData.FirstOrDefault().printPartnerId));

            //}
            //web notifications
            if (HttpContext.Request.Headers.TryGetValue("Authorization", out var token))
            {
                //foreach (var data in resData)
                //{
                    var webContent = new Dictionary<string, string>();
                    webContent.Add("PrintOrderNumber", resData.Count().ToString());
                    webContent.Add("WareHouseId", resData.FirstOrDefault().warehouseId.ToString());
                    webContent.Add("PrintPartnerId", resData.FirstOrDefault().printPartnerId.ToString());
                    var trimmedToken = token.ToString().Substring(7);
                    //await _webNotificationContent.SendMsgContentToDb(trimmedToken, WebNotificationType.PrintOrderSubmitted, webContent);
                    await _notifcationCosmosDb.AddItemAsync(trimmedToken, WebNotificationType.PrintOrderSubmitted, webContent);
                //}
            }
            return Ok("Success");
        }

        [HttpGet]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "CanViewPrintOrders")]
        public async Task<IActionResult> GetPrintOrder(Guid printOrderId)
        {
            return Ok(await _printOrderService.GetPrintOrderAsync(printOrderId));
        }

        [HttpPost]
        [Route("[action]")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "CanViewPrintOrders")]
        public async Task<IActionResult> GetPrintOrders(PrintOrderSearchFilterOptions options)
        {
            var role = GetEntityRole();
            switch (role)
            {
                case "PrintPartner":
                    var ppId = User.Claims.Single(x => x.Type == "OrgId").Value;
                    options.PrintPartnerId = Convert.ToInt32(ppId);
                    return Ok(await _printOrderService.GetPrintOrdersAsync(options));
                case "TsspAdmin":
                case "TsspIntermediate":
                case "TsspWarehouseIncharge":
                    options.PrintPartnerId = null;
                    return Ok(await _printOrderService.GetPrintOrdersAsync(options));
                default:
                    return BadRequest("Cannot match print orders for requesting role.");

            }
        }

        [HttpPost]
        [Route("[action]")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "CanApproveRejectPrintOrders")]
        public async Task<IActionResult> ApprovePrintOrder(OrderApprovalActionModel orderApprovalActionModel)
        {
            var id = GetEntityOrgId();
            var printOrderQueueDto = await _printOrderService.ApprovePrintOrder(orderApprovalActionModel.OrderId, orderApprovalActionModel.Comments, id);
            await _notificationHelper.SendNotificationToQueue(_emailNotification.BuildPrintOrderApprovedTemplateForAdmins(orderApprovalActionModel.OrderId, printOrderQueueDto.Item1.PrintOrderNo, printOrderQueueDto.orgId, GetUserName()));
            //web notifications
            if (HttpContext.Request.Headers.TryGetValue("Authorization", out var token))
            {
                var webContent = new Dictionary<string, string>();
                webContent.Add("PrintOrderNumber", printOrderQueueDto.Item1.PrintOrderNo);
                webContent.Add("WareHouseId", printOrderQueueDto.Item1.WareHouseId.ToString());
                webContent.Add("PrintPartnerId", printOrderQueueDto.Item1.PrintPartnerId.ToString());
                var trimmedToken = token.ToString().Substring(7);
                //await _webNotificationContent.SendMsgContentToDb(trimmedToken, WebNotificationType.PrintOrderApproved, webContent);
                await _notifcationCosmosDb.AddItemAsync(trimmedToken, WebNotificationType.PrintOrderApproved, webContent);
            }
            QueueServiceClient account = new QueueServiceClient(_configuration.GetConnectionString("StorageAccount"));
            QueueClient cloudQueue = account.GetQueueClient(_configuration.GetValue<string>("StampGenerationRequestQueue"));
            var messageAsJson = JsonConvert.SerializeObject(printOrderQueueDto.Item1);
            await cloudQueue.SendMessageAsync(messageAsJson);
            //CloudStorageAccount account = CloudStorageAccount.Parse(_configuration.GetConnectionString("StorageAccount"));
            //CloudQueueClient client = account.CreateCloudQueueClient();
            //CloudQueue cloudQueue = client.GetQueueReference(_configuration.GetValue<string>("StampGenerationRequestQueue"));
            //var messageAsJson = JsonConvert.SerializeObject(printOrderQueueDto.Item1);
            //var cloudQueueMessage = new CloudQueueMessage(messageAsJson);
            //cloudQueue.EncodeMessage = false;
            //await cloudQueue.AddMessageAsync(cloudQueueMessage);
            return Ok();
        }

        [HttpPost]
        [Route("[action]")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "CanApproveRejectPrintOrders")]
        public async Task<IActionResult> RejectOrder(OrderRejectActionModel orderRejectActionModel)
        {
            var resData=await _printOrderService.RejectPrintOrder(orderRejectActionModel.OrderId, orderRejectActionModel.Comments);

            await _notificationHelper.SendNotificationToQueue(_emailNotification.BuildPrintOrderRejectedTemplate(orderRejectActionModel.OrderId, orderRejectActionModel.Comments, resData.printOrderNumber, resData.userId, resData.printPartnerName));
            //web notifications
            if (HttpContext.Request.Headers.TryGetValue("Authorization", out var token))
            {
                var webContent = new Dictionary<string, string>();
                webContent.Add("PrintOrderNumber", resData.printOrderNumber);
                webContent.Add("WareHouseId",  resData.warehouseId.ToString());
                webContent.Add("PrintPartnerId", resData.printPartnerId.ToString());
                var trimmedToken = token.ToString().Substring(7);
                //await _webNotificationContent.SendMsgContentToDb(trimmedToken, WebNotificationType.PrintOrderRejected, webContent);
                await _notifcationCosmosDb.AddItemAsync(trimmedToken, WebNotificationType.PrintOrderRejected, webContent);
            }
            return Ok("success");
        }

        [HttpPost]
        [Route("[action]")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "CanCreateClosePrintOrders")]
        public async Task<IActionResult> CloseOrder(OrderRejectActionModel orderRejectActionModel)
        {
            var resData=await _printOrderService.ClosePrintOrder(orderRejectActionModel.OrderId, orderRejectActionModel.Comments);
            await _notificationHelper.SendNotificationToQueue(_emailNotification.BuildPrintOrderClosedTemplate(resData.printOrderId, resData.printOrderNumber, resData.printPartnerId));
            //web notifications
            if (HttpContext.Request.Headers.TryGetValue("Authorization", out var token))
            {
                var webContent = new Dictionary<string, string>();
                webContent.Add("PrintOrderNumber", resData.printOrderNumber);
                webContent.Add("WareHouseId", resData.warehouseId.ToString());
                webContent.Add("PrintPartnerId", resData.printPartnerId.ToString());
                var trimmedToken = token.ToString().Substring(7);
                //await _webNotificationContent.SendMsgContentToDb(trimmedToken, WebNotificationType.PrintOrderClosed, webContent);
                await _notifcationCosmosDb.AddItemAsync(trimmedToken, WebNotificationType.PrintOrderClosed, webContent);
            }
            return Ok("success");
        }
        [HttpPost]
        [Route("[action]")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult DownloadPrintOrderList(PrintOrderSearchFilterOptions filterOptions)
        {
            var printOrderData = _printOrderService.GetPrintOrderDownloadList(filterOptions);
            var result = printOrderData.Select(p => new { PurchaseOrderNumber = p.PoNum, ProductName = p.ProductName, PrintOrderNumber = p.PrintOrderNum, ExpectedDate = p.ExpectedDate, Status = p.Status }).ToList();
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
                    return File(streamedData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PrintOrderList_" + DateTime.Now.Ticks.ToString());
                }
            }
        }
    }
}
