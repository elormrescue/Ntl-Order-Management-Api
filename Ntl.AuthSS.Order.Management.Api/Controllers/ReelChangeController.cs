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
using Ntl.Tss.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Utilities.NotificationHelper;
using Ntl.AuthSS.OrderManagement.Data.Entities;

namespace Ntl.AuthSS.Order_Management.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReelChangeController : BaseController
    {
        private IReelChangeService _reelChangeService;
        private readonly TssIdentityDbContext _tssIdentityDbContext;
        private readonly IConfiguration _configuration;
        private readonly CosmosAccessConfig _cosmosAccessConfig;
        //private readonly WebNotificationContentTemplate _webNotificationContent;
        private readonly EmailNotificationTemplate _emailNotification;
        private readonly UserClient _userClient;
        private readonly INotificationHelper _notificationHelper;
        private readonly ICosmosDbService _notifcationCosmosDb;
        public ReelChangeController(IReelChangeService reelChangeService, IConfiguration configuration, INotificationHelper notificationHelper, TssIdentityDbContext tssIdentityDbContext, UserClient userClient, CosmosAccessConfig cosmosAccessConfig, ICosmosDbService notifcationCosmosDb)
        {
            _reelChangeService = reelChangeService;
            _userClient = userClient;
            _notificationHelper = notificationHelper;
            _tssIdentityDbContext = tssIdentityDbContext;
            _configuration = configuration;
            _userClient = userClient;
            _cosmosAccessConfig = cosmosAccessConfig;
            _emailNotification = new EmailNotificationTemplate(_configuration, _userClient);
            //_webNotificationContent = new WebNotificationContentTemplate(_cosmosAccessConfig);
            _notifcationCosmosDb = notifcationCosmosDb;
        }

        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "CanCreateReelChangeRequest")]
        public async Task<IActionResult> SaveReelChangeRequest(ReelChangeRequestDto reelChangeRequestDto)
        {
            string productName = reelChangeRequestDto.ChangeToProductId == null ? null : reelChangeRequestDto.ChangeToProductName;
            string sku = reelChangeRequestDto.ChangeToSkuId == null ? null : reelChangeRequestDto.ChangeToSkuName;
            var data = await _reelChangeService.SaveReelChangeRequest(reelChangeRequestDto, GetEntityOrgId());
            await _notificationHelper.SendNotificationToQueue(_emailNotification.BuildChangeSkuRequestedTemplateForOrgUsers(productName, sku, data.Organization.Name, data.Organization.Id));
            await _notificationHelper.SendNotificationToQueue(_emailNotification.BuildChangeSkuRequestedTemplateForPortalAdmins(productName, sku, data.Organization.Name));

            //web notifications
            if (HttpContext.Request.Headers.TryGetValue("Authorization", out var token))
            {
                var webContent = new Dictionary<string, string>();
                webContent.Add("RequestNumber", data.Number);
                webContent.Add("ProductName", productName);
                webContent.Add("Sku", sku);
                webContent.Add("MfName", data.Organization.Name);
                var trimmedToken = token.ToString().Substring(7);
                //await _webNotificationContent.SendMsgContentToDb(trimmedToken, WebNotificationType.ChangeSkuProductRequested, webContent);
                await _notifcationCosmosDb.AddItemAsync(trimmedToken, WebNotificationType.ChangeSkuProductRequested, webContent);
            }
            return Ok("Success");
        }

        [HttpGet]
        [Route("[action]")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "CanCreateReelChangeRequest")]
        public async Task<IActionResult> GetReelProduct(string reelCode, int productId)
        {
            var reelProduct = await _reelChangeService.GetReelProduct(reelCode, GetEntityOrgId(), productId);
            if (reelProduct == null)
                return BadRequest("There are no reels associated with the reel code:" + reelCode);

            return Ok(reelProduct);
        }

        [HttpGet]
        [Route("[action]")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "CanCreateReelChangeRequest")]
        public async Task<IActionResult> GetReelProducts(string cartonCode, int productId)
        {
            var reelProducts = await _reelChangeService.GetReelProducts(cartonCode, GetEntityOrgId(), productId);
            if (reelProducts != null && reelProducts.Count > 0)
                return Ok(reelProducts);

            return BadRequest("There are no cartons associated with the carton code:" + cartonCode);
        }

        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "CanViewReelChangeRequest")]
        public async Task<IActionResult> GetReelChangeRequests(ReelChangeRequestSearchFilterOptions filterOptions)
        {
            var orgType = GetEntityOrgType();
            var orgId = GetEntityOrgId();
            if(orgType == OrgType.Manufacturer.ToString())
            {
                filterOptions.EntityIds = new int[] { orgId };
                return Ok(await _reelChangeService.GetReelChangeRequests(filterOptions));
            }
            else
                return Ok(await _reelChangeService.GetReelChangeRequests(filterOptions));
        }

        [HttpGet]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "CanViewReelChangeRequest")]
        public async Task<IActionResult> GetReelChangeRequest(Guid requestId)
        {
            return Ok(await _reelChangeService.GetReelChangeRequest(requestId));
        }

        [HttpPost]
        [Route("[action]")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "CanApproveRejectReelChangeRequest")]
        public async Task<IActionResult> ApproveReelChangeRequest(ActionModel actionModel)
        {
            var data = await _reelChangeService.ApproveReelChangeRequest(actionModel.Id, actionModel.Comments);
            await _notificationHelper.SendNotificationToQueue(_emailNotification.BuildChangeSkuApprovedTemplateForOrgUsers(actionModel.Id, data.orgId, data.number));
            //_notificationHelper.SendNotificationToQueue(_emailNotification.BuildChangeSkuApprovedTemplateForPortalAdmins(actionModel.Id, res[1], res[0], res[2]));

            //web notifications
            if (HttpContext.Request.Headers.TryGetValue("Authorization", out var token))
            {
                var webContent = new Dictionary<string, string>();
                webContent.Add("RequestNumber", data.number);
                webContent.Add("MfName", data.orgName);
                webContent.Add("MfOrgId", data.orgId.ToString());
                var trimmedToken = token.ToString().Substring(7);
                //await _webNotificationContent.SendMsgContentToDb(trimmedToken, WebNotificationType.ChangeSkuProductApproved, webContent);
                await _notifcationCosmosDb.AddItemAsync(trimmedToken, WebNotificationType.ChangeSkuProductApproved, webContent);
            }
            return Ok("Success");
        }

        [HttpPost]
        [Route("[action]")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "CanApproveRejectReelChangeRequest")]
        public async Task<IActionResult> RejectReelChangeRequest(ActionModel actionModel)
        {
            var data = await _reelChangeService.RejectReelChangeRequest(actionModel.Id, actionModel.Comments);
            //_notificationHelper.SendNotificationToQueue(_emailNotification.BuildChangeSkuRejectedTemplateForOrgUsers(actionModel.Id, res[1], res[0], res[2]));
            //_notificationHelper.SendNotificationToQueue(_emailNotification.BuildChangeSkuRejectedTemplateForPortalAdmins(actionModel.Id, res[1], res[0], res[2]));

            //web notifications
            if (HttpContext.Request.Headers.TryGetValue("Authorization", out var token))
            {
                var webContent = new Dictionary<string, string>();
                webContent.Add("RequestNumber", data.number);
                webContent.Add("MfName", data.orgName);
                webContent.Add("MfOrgId", data.orgId.ToString());
                var trimmedToken = token.ToString().Substring(7);
                //await _webNotificationContent.SendMsgContentToDb(trimmedToken, WebNotificationType.ChangeSkuProductRejected, webContent);
                await _notifcationCosmosDb.AddItemAsync(trimmedToken, WebNotificationType.ChangeSkuProductRejected, webContent);
            }
            return Ok("Success");
        }
        [HttpPost]
        [Route("[action]")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "CanViewReelChangeRequest")]
        public IActionResult DownloadFilteredReelChangeRequestList(ReelChangeRequestSearchFilterOptions filterOptions)
        {
            var reelChangeData = _reelChangeService.GetReelChangeDownloadList(filterOptions);
            var result = reelChangeData.Select(r => new { RequisitionNumber = r.Number, Manufacturer = r.OrgName, Date = r.RequestedDate, Status = r.Status }).ToList();
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
                    return File(streamedData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ReelChangeRequestList_" + DateTime.Now.Ticks.ToString());
                }
            }
        }
    }
}
