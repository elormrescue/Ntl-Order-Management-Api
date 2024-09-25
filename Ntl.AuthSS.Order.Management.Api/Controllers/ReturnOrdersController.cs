using System;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Ntl.AuthSS.Order_Management.Api.Models;
using Ntl.AuthSS.OrderManagement.Business;
using Ntl.AuthSS.OrderManagement.Business.Services;
using Ntl.AuthSS.OrderManagement.Data.Entities;
using Ntl.Tss.Identity.Data;
using Ntl.AuthSS.Order_Management.Api.FcmNotificationModels;
using Ntl.AuthSS.Order_Management.Api.HttpClients;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Azure.Storage.Blobs;

namespace Ntl.AuthSS.Order_Management.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReturnOrdersController : BaseController
    {
        private readonly IReturnOrderService _returnOrderService;
        private readonly IOrderMetaService _orderMetaService;
        private readonly TssIdentityDbContext _tssIdentityDbContext;
        private readonly CosmosAccessConfig _cosmosAccessConfig;
        private readonly FcmClient _fcmClient;
        private readonly UserClient _userClient;
        //private readonly WebNotificationContentTemplate _webNotificationContent; 
        private readonly ICosmosDbService _notificationCosmosDb;
        public IConfiguration _configuration { get; }
        public ReturnOrdersController(IReturnOrderService returnOrderService, IOrderMetaService orderMetaService, IConfiguration configuration, TssIdentityDbContext tssIdentityDbContext, FcmClient fcmClient, UserClient userClient, CosmosAccessConfig cosmosAccessConfig, ICosmosDbService notifcationCosmosDb)
        {
            _returnOrderService = returnOrderService;
            _orderMetaService = orderMetaService;
            _configuration = configuration;
            _tssIdentityDbContext = tssIdentityDbContext;
            _fcmClient = fcmClient;
            _userClient = userClient;
            _cosmosAccessConfig = cosmosAccessConfig;
            _notificationCosmosDb = notifcationCosmosDb;
            //_webNotificationContent = new WebNotificationContentTemplate(_cosmosAccessConfig);
        }


        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "CanCreateReturnOrders")]
        public async Task<IActionResult> CreateReturnOrder(ReturnOrderDto returnOrderDto)
        {
            var mfId = GetEntityOrgId();
            var result = await _returnOrderService.ReturnOrder(returnOrderDto, GetOrderEntityType(), mfId);
            if (result.errorMessage != null)
                return BadRequest(result.errorMessage);
            var userDevices = await _userClient.GetUserDevices(returnOrderDto.shipper.ShipperId);
            //var userName = GetUserName();
            foreach (var userDevice in userDevices)
            {
                var fcmNotifyMessage = new FcmNotifyMessage()
                {
                    Notification = new
                    {
                        title = "Return order",
                        body = result.orgName + " has initiated a return order. Please deliver it to Ntl"
                    },
                    Data = new
                    {
                        ntlNotificationType = ntlNotificationType.ReturnOrder,
                        orderNo = result.returnOrderNumber
                    }
                };
                await _fcmClient.SendFcmMessage(fcmNotifyMessage, userDevice.AppToken);
            }
            //web notifications
            if (HttpContext.Request.Headers.TryGetValue("Authorization", out var token))
            {
                var webContent = new Dictionary<string, string>();
                webContent.Add("ReturnOrderNumber", result.returnOrderNumber);
                webContent.Add("MfName", result.orgName);
                var trimmedToken = token.ToString().Substring(7);
                //await _webNotificationContent.SendMsgContentToDb(trimmedToken, WebNotificationType.MfReturnOrderSubmitted, webContent);
                await _notificationCosmosDb.AddItemAsync(trimmedToken, WebNotificationType.MfReturnOrderSubmitted, webContent);
            }
            return Ok(result.orderNumber);
        }

        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "CanViewReturnOrders")]
        public async Task<IActionResult> GetReturnOrders(ReturnOrderSearchFilterOptions filterOptions)
        {
            var role = GetEntityRole();
            switch (role)
            {
                case "MfAdmin": /*Can refer Roles.MfAdmin. Switch doesn't allow non constants*/
                case "MfAccountManager":
                case "MfWarehouseIncharge":
                    var mfId = GetEntityOrgId();
                    filterOptions.EntityIds = new int?[] { mfId };
                    filterOptions.OrgType = OrgType.Manufacturer;
                    return Ok(await _returnOrderService.GetReturnOrders(filterOptions));
                default:
                    return Ok(await _returnOrderService.GetReturnOrders(filterOptions));
            }
        }

        [HttpGet]
        [Route("[action]")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "CanViewReturnOrders")]
        public async Task<IActionResult> GetReturnOrderDetails(Guid returnorderId)
        {
            return Ok(await _returnOrderService.GetReturnOrderDetailsAsync(returnorderId));
        }

        [HttpPost]
        [Route("[action]")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "CanApproveRejectReturnOrders")]
        public async Task<IActionResult> ApproveReturnOrderRequest(ActionModel returnOrderRequestActionModel)
        {
            var resultData = await _returnOrderService.ApproveReturnOrder(returnOrderRequestActionModel.Id, returnOrderRequestActionModel.Comments, GetOrderEntityType());
            //web notifications
            if (HttpContext.Request.Headers.TryGetValue("Authorization", out var token))
            {
                var webContent = new Dictionary<string, string>();
                webContent.Add("ReturnOrderNumber", resultData.returnOrderNumber);
                webContent.Add("MfName", resultData.mfName);
                webContent.Add("MfOrgId", resultData.orgId.ToString());
                var trimmedToken = token.ToString().Substring(7);
                //await _webNotificationContent.SendMsgContentToDb(trimmedToken, WebNotificationType.MfReturnOrderApproved, webContent);
                await _notificationCosmosDb.AddItemAsync(trimmedToken, WebNotificationType.MfReturnOrderApproved, webContent);
            }
            return Ok("Success");
        }

        [HttpGet]
        [Route("[action]")]
        [Authorize(Policy = "CanDownloadPackageList")]
        public async Task<IActionResult> DownloadFile(string returnOrderNumber)
        {
            MemoryStream ms = new MemoryStream();
            BlobServiceClient blobServiceClient = new BlobServiceClient("StorageConnectionString");
            if (blobServiceClient.AccountName != null)
            {
                BlobContainerClient container = blobServiceClient.GetBlobContainerClient("BlobContainerName");
                if (await container.ExistsAsync())
                {
                    BlobClient file = container.GetBlobClient(returnOrderNumber.ToLower() + ".pdf");

                    if (await file.ExistsAsync())
                    {
                        await file.DownloadToAsync(ms);
                        Stream blobStream = file.OpenReadAsync().Result;
                        return File(blobStream, file.GetProperties().Value.ContentType, file.Name);
                    }
                    else
                        return Content("File does not exist");
                }
                else
                    return Content("Container does not exist");
            }
            else
                return Content("Error opening storage");
        }
        [HttpPost]
        [Route("[action]")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "CanApproveRejectReturnOrders")]
        public IActionResult DownloadFilteredReturnOrderList(ReturnOrderSearchFilterOptions filterOptions)
        {
            var returnOrderData = _returnOrderService.GetReturnOrderDownloadList(filterOptions);
            if (returnOrderData == null)
            {
                return BadRequest("not found");
            }
            else
            {
                using (var excelFile = new ExcelPackage())
                {
                    var worksheet = excelFile.Workbook.Worksheets.Add("Sheet1");
                    worksheet.Cells["A1"].LoadFromCollection(Collection: returnOrderData, PrintHeaders: true);
                    var streamedData = new MemoryStream(excelFile.GetAsByteArray());
                    return File(streamedData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ReturnOrderList_" + DateTime.Now.Ticks.ToString());
                }
            }
        }

    }
}
