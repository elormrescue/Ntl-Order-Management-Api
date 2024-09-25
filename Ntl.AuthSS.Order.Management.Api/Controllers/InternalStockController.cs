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
using Ntl.AuthSS.Order_Management.Api.Models;
using Ntl.AuthSS.OrderManagement.Business;
using Ntl.AuthSS.OrderManagement.Business.Services;
using Utilities.NotificationHelper;
using Microsoft.Extensions.Configuration;

namespace Ntl.AuthSS.Order_Management.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InternalStockController : BaseController
    {
        private readonly IInternalStockService _internalStockService;
        private readonly IConfiguration _configuration;
        private readonly INotificationHelper _notificationHelper;
        private readonly EmailNotificationTemplate _emailNotification;
        private readonly UserClient _userClient;
        private readonly ICosmosDbService _notifcationCosmosDb;
        public InternalStockController(IInternalStockService internalStockService, INotificationHelper notificationHelper, IConfiguration configuration, UserClient userClient, ICosmosDbService notifcationCosmosDb)
        {
            _internalStockService = internalStockService;
            _notificationHelper = notificationHelper;
            _configuration = configuration;
            _userClient = userClient;
            _emailNotification = new EmailNotificationTemplate(_configuration, _userClient); ;
            _notifcationCosmosDb = notifcationCosmosDb;
        }

        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "InternalStockTransfer")]
        public async Task<IActionResult> SaveInternalStockRequest(InternalStockRequestDto internalStockRequestDto)
        {
            internalStockRequestDto.RequestingFacilityId = GetEntityLocation();
            internalStockRequestDto.OrgId = Convert.ToInt32(GetEntityOrgId());

            var internalStockRequest =  await _internalStockService.SaveInternalStockRequest(internalStockRequestDto, GetUserName());

            await _notificationHelper.SendNotificationToQueue(_emailNotification.BuildTemplateForInternalStockTranferRequested(internalStockRequest.Id, internalStockRequest.Organization.Id, internalStockRequest.RequestingFacility.Name));

            if (HttpContext.Request.Headers.TryGetValue("Authorization", out var token))
            {
                    var webContent = new Dictionary<string, string>();
                    webContent.Add("StockOrderNumber", internalStockRequest.Number);
                    webContent.Add("TpsafOrgId", internalStockRequest.Organization.Id.ToString());
                    //webContent.Add("WareHouseId", orderDetails.wareHouseId.ToString());
                    var trimmedToken = token.ToString().Substring(7);
                    //await _webNotificationContent.SendMsgContentToDb(trimmedToken, WebNotificationType.MfOrderApproved, webContent);
                    await _notifcationCosmosDb.AddItemAsync(trimmedToken, WebNotificationType.InternalStockTransferRequested, webContent);
             }
                return Ok("success");
        }

        [HttpGet]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "ViewInternalStockTransfers")]
        public async Task<IActionResult> GetStockRequest(Guid requestId)
        {
            return Ok(await _internalStockService.GetInternalStockRequest(requestId));
        }

        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "ViewInternalStockTransfers")]
        public async Task<IActionResult> GetStockRequests(InternalStockRequestFilterOptions filterOptions)
        {
            var role = GetEntityRoleType();
            int? orgId;
            switch (role)
            {
                case Roles.TsspAdmin:
                case Roles.TsspIntermediate:
                case Roles.TsspWarehouseIncharge:
                    orgId = null;
                    break;
                default:
                    orgId = GetEntityOrgId();
                    break;

            }
            return Ok(await _internalStockService.GetInternalStockRequests(filterOptions, orgId));            
        }

        [HttpPost]
        [Route("[action]")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "InternalStockTransfer")]
        public async Task<IActionResult> ApproveStockRequest(ActionModel stockRequestActionModel)
        {
            var userName = GetUserName();
            var stockRequest = await _internalStockService.ApproveStockRequest(stockRequestActionModel.Id, stockRequestActionModel.Comments, GetUserName(), GetEntityLocation());
            await _notificationHelper.SendNotificationToQueue(_emailNotification.BuildInternalStockTraferApprovedTemplateForRequestorAndTpsafAdmin(stockRequest.Id, stockRequest.Organization.Id, stockRequest.CreatedUser, stockRequest.ApprovingFacility.Name, GetUserName()));
            await _notificationHelper.SendNotificationToQueue(_emailNotification.BuildInternalStockTraferApprovedTemplateForApprover(stockRequest.Id, stockRequest.ModifiedUser));

            if (HttpContext.Request.Headers.TryGetValue("Authorization", out var token))
            {
                var webContent = new Dictionary<string, string>();
                webContent.Add("StockOrderNumber", stockRequest.Number);
                webContent.Add("TpsafOrgId", stockRequest.Organization.Id.ToString());
                webContent.Add("ApproverName", userName);
                webContent.Add("FacilityName", stockRequest.ApprovingFacility.Name);
                //webContent.Add("WareHouseId", orderDetails.wareHouseId.ToString());
                var trimmedToken = token.ToString().Substring(7);
                //await _webNotificationContent.SendMsgContentToDb(trimmedToken, WebNotificationType.MfOrderApproved, webContent);
                await _notifcationCosmosDb.AddItemAsync(trimmedToken, WebNotificationType.InternalStockTransferApproved, webContent);
            }
            return Ok("Success");
        }
        [HttpPost]
        [Route("[action]")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "InternalStockTransfer")]
        public async Task<IActionResult> CloseStockRequest(Guid requestId)
        {
            return Ok(await _internalStockService.CloseStockRequest(requestId, GetUserName(), GetEntityLocation()));
        }


        [HttpGet]
        [Route("[action]")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "InternalStockTransfer")]
        public async Task<IActionResult> GetStockTransferReel(Guid requestId, string reelCode)
        {
            var stockRequest =  await _internalStockService.GetInternalStockRequest(requestId);
            var reel = _internalStockService.GetStockTransferReel(reelCode, stockRequest.ApprovingFacilityId.Value, stockRequest.ProductId);
            if (reel == null)
                return BadRequest("Reel code entered doesn't match the existing reels codes in your warehouse");
            return Ok(reel);
        }

        [HttpGet]
        [Route("[action]")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "InternalStockTransfer")]
        public async Task<IActionResult> GetStockTransferCarton(Guid requestId, string cartonCode)
        {
            var stockRequest = await _internalStockService.GetInternalStockRequest(requestId);
            var carton = _internalStockService.GetStockTransferCarton(cartonCode, stockRequest.ApprovingFacilityId.Value, stockRequest.ProductId);
            if (carton == null)
                return BadRequest("Carton code entered doesn't match the existing carton codes in your warehouse or has been partially used");
            return Ok(carton);
        }

        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "InternalStockTransfer")]
        public async Task<IActionResult> SaveInternalStockTransfer(PostInternalStockTransferModel postInternalStockTransfer)
        {
            var approverName = GetUserName();
            var stockRequest = await _internalStockService.SaveInternalStockTransfer(postInternalStockTransfer.RequestId, postInternalStockTransfer.Packages, postInternalStockTransfer.ShipperId, postInternalStockTransfer.TrackingId, postInternalStockTransfer.ExpectedDate, postInternalStockTransfer.Comments, GetUserName());
            await _notificationHelper.SendNotificationToQueue(_emailNotification.BuildInternalStockTraferFilfilledTemplate(stockRequest.Id, stockRequest.Organization.Id, stockRequest.CreatedUser, stockRequest.ApprovingFacility.Name, GetUserName()));

            if (HttpContext.Request.Headers.TryGetValue("Authorization", out var token))
            {
                var webContent = new Dictionary<string, string>();
                webContent.Add("StockOrderNumber", stockRequest.Number);
                webContent.Add("TpsafOrgId", stockRequest.Organization.Id.ToString());
                webContent.Add("ApproverName", approverName);
                webContent.Add("FacilityName", stockRequest.ApprovingFacility.Name);
                //webContent.Add("WareHouseId", orderDetails.wareHouseId.ToString());
                var trimmedToken = token.ToString().Substring(7);
                //await _webNotificationContent.SendMsgContentToDb(trimmedToken, WebNotificationType.MfOrderApproved, webContent);
                await _notifcationCosmosDb.AddItemAsync(trimmedToken, WebNotificationType.InternalStockTransferFulFilled, webContent);
            }
            return Ok("Success");
        }
        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "ViewInternalStockTransfers")]
        public IActionResult DowloadFilteredInternalStockRequest([FromBody] InternalStockRequestFilterOptions filterOptions)
        {
            var role = GetEntityRoleType();
            int? orgId;
            switch (role)
            {
                case Roles.TsspAdmin:
                case Roles.TsspIntermediate:
                case Roles.TsspWarehouseIncharge:
                    orgId = null;
                    break;
                default:
                    orgId = GetEntityOrgId();
                    break;

            }
            var stockRequestData = _internalStockService.GetInternalStockRequestDownloadList(filterOptions, orgId);
            var result = stockRequestData.Select(s => new { StockRequestNumber = s.Number, RequestedStamp = s.RequestedStamps, FullFilledStamps = s.FulfilledStamps, RequestedOn = s.RequestedOn, Status = s.Status, RequestingFacility = s.RequestingFacility, ApprovingFacility = s.ApprovingFacility }).ToList();
            if(result == null)
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
                    return File(streamedData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "InternalStockRequestList_" + DateTime.Now.Ticks.ToString());
                }
            }
        }
    }
}
