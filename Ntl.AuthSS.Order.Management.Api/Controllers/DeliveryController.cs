using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Azure.Storage;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Ntl.AuthSS.Order_Management.Api.ConfigModels;
using Ntl.AuthSS.Order_Management.Api.Models;
using Ntl.AuthSS.OrderManagement.Business;
using Ntl.AuthSS.OrderManagement.Business.Services;
using Ntl.Tss.Identity.Data;
using Twilio.Rest.Api.V2010.Account;
using Utilities.NotificationHelper;
using Ntl.AuthSS.OrderManagement.Business.NotificationModel;
using Ntl.AuthSS.OrderManagement.Data.Entities;

namespace Ntl.AuthSS.Order_Management.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveryController : BaseController
    {
        private readonly IDeliveryService _deliveryService;
        private StorageAccountConfig _storageAccountConfig;
        private readonly CosmosAccessConfig _cosmosAccessConfig;
        private readonly INotificationHelper _notificationHelper;
        //private readonly WebNotificationContentTemplate _webNotificationContent;
        private readonly EmailNotificationTemplate emailNotification;
        private readonly SmsNotificationTemplate smsNotification;
        private readonly UserClient _userClient;
        private readonly TssIdentityDbContext _tssIdentityDbContext;
        private readonly ICosmosDbService _notifcationCosmosDb;
        private IConfiguration _configuration { get; }

        public DeliveryController(IDeliveryService deliveryService, INotificationHelper notificationHelper, StorageAccountConfig storageAccountConfig, IConfiguration configuration, UserClient userClient, CosmosAccessConfig cosmosAccessConfig, ICosmosDbService notifcationCosmosDb)
        {
            _deliveryService = deliveryService;
            _storageAccountConfig = storageAccountConfig;
            _configuration = configuration;
            _cosmosAccessConfig = cosmosAccessConfig;
            _userClient = userClient;
            _notificationHelper = notificationHelper;
            emailNotification = new EmailNotificationTemplate(_configuration, _userClient);
            smsNotification = new SmsNotificationTemplate();
            _notifcationCosmosDb = notifcationCosmosDb;
            //_webNotificationContent = new WebNotificationContentTemplate(_cosmosAccessConfig);
        }

        [HttpPost]
        [Route("[action]")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "Delivery")]
        public async Task<IActionResult> GetDeliveries(DeliveryFilterOptions deliveryFilterOptions)
        {
            var deliveries = await _deliveryService.GetDeliveries(deliveryFilterOptions, GetEntityOrgId(), null);
            foreach (var delivery in deliveries.Deliveries)
            {
                delivery.PackageURL = GetSASTokenWithURLForFullfillment(delivery.OrderNumber, delivery.DeliveryType);
            }
            return Ok(deliveries);
        }

        [HttpPost]
        [Route("[action]")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "Delivery")]
        public async Task<IActionResult> MyDeliveries(DeliveryFilterOptions deliveryFilterOptions)
        {
            var deliveries = await _deliveryService.GetDeliveries(deliveryFilterOptions, GetEntityOrgId(), GetEntityId());
            foreach (var delivery in deliveries.Deliveries)
            {
                delivery.PackageURL = GetSASTokenWithURLForFullfillment(delivery.OrderNumber, delivery.DeliveryType);
            }
            return Ok(deliveries);
        }

        [HttpPost]
        [Route("[action]")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "Delivery")]
        public async Task<IActionResult> PickupDelivery(PickupRequestModel pickupRequestModel)
        {
            var (otp, phoneNo) = await _deliveryService.PickupDelivery(pickupRequestModel.OrderId, pickupRequestModel.DeliveryType, GetUserName());
            var smsModel = smsNotification.BuildSmsNotificationTemplate(phoneNo, $"Your delivery confirmation Otp is {otp}. Please share it with delivery agent when after you receive the delivery.");
            await _notificationHelper.SendNotificationToQueue(smsModel);
            var order = await _deliveryService.GetOrderDetails(pickupRequestModel.OrderId);
            if (order != null)
            {
                await _notificationHelper.SendNotificationToQueue(emailNotification.BuildPickUpDeliveryTemplateForOrgUsers(order.Id, order.Number, order.Shipper.Name, order.Warehouse.Name, order.Organization.Id, order.Warehouse.Contact.Email));
                await _notificationHelper.SendNotificationToQueue(emailNotification.BuildPickupDeilveryTemplateForPortalAdmins(order.Id, order.Number, order.Shipper.Name, order.Organization.Name));

                //web notifications
                if (HttpContext.Request.Headers.TryGetValue("Authorization", out var token))
                {
                    if(order.Organization.OrgType == OrgType.Manufacturer)
                    {
                        var webContent = new Dictionary<string, string>();
                        webContent.Add("OrderNumber", order.Number);
                        webContent.Add("MfName", order.Organization.Name);
                        webContent.Add("MfOrgId", order.Organization.Id.ToString());
                        webContent.Add("WareHouseId", order.Warehouse.Id.ToString());
                        webContent.Add("DeliveryAgentName", order.Shipper.Name);
                        var trimmedToken = token.ToString().Substring(7);
                        //await _webNotificationContent.SendMsgContentToDb(trimmedToken, WebNotificationType.MfOrderPickedUp, webContent);
                        await _notifcationCosmosDb.AddItemAsync(trimmedToken, WebNotificationType.MfOrderPickedUp, webContent);
                    }
                    else
                    {
                        var webContent = new Dictionary<string, string>();
                        webContent.Add("StockOrderNumber", order.Number);
                        webContent.Add("TpsafName", order.Organization.Name);
                        webContent.Add("TpsafOrgId", order.Organization.Id.ToString());
                        webContent.Add("WareHouseId", order.Warehouse.Id.ToString());
                        webContent.Add("DeliveryAgentName", order.Shipper.Name);
                        var trimmedToken = token.ToString().Substring(7);
                        //await _webNotificationContent.SendMsgContentToDb(trimmedToken, WebNotificationType.MfOrderPickedUp, webContent);
                        await _notifcationCosmosDb.AddItemAsync(trimmedToken, WebNotificationType.StockOrderPickedUp, webContent);
                    }
                }
            }
            return Ok("Pick up successfull");
        }

        [HttpPost]
        [Route("[action]")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "Delivery")]
        public async Task<IActionResult> DeliverOrder(DeliverRequestModel deliverRequestModel)
        {
            if (deliverRequestModel.Otp == null && String.IsNullOrWhiteSpace(deliverRequestModel.Remarks))
                return BadRequest("You need to input either otp or remarks to deliver order");

            if (deliverRequestModel.Otp != null && !_deliveryService.IsDeliveryOtpValid(deliverRequestModel.OrderId, deliverRequestModel.DeliveryType, deliverRequestModel.Otp.Value))
                return BadRequest("Invalid otp");

            await _deliveryService.Deliver(deliverRequestModel.OrderId, deliverRequestModel.DeliveryType, deliverRequestModel.Remarks, GetUserName());

            var order = await _deliveryService.GetOrderDetails(deliverRequestModel.OrderId);
            if (order != null)
            {
                await _notificationHelper.SendNotificationToQueue(emailNotification.BuildOrderDeliverdTemplateForOrgUsers(order.Id, order.Number, order.Shipper.Name, order.Organization.Id, order.Warehouse.Name, order.Warehouse.Contact.Email));
                await _notificationHelper.SendNotificationToQueue(emailNotification.BuildOrderDeliverdTemplateForPortalAdmins(order.Id, order.Number, order.Shipper.Name, order.Organization.Name, order.Warehouse.Name));

                //web notifications
                if (HttpContext.Request.Headers.TryGetValue("Authorization", out var token))
                {
                    if(order.Organization.OrgType == OrgType.Manufacturer)
                    {
                        var webContent = new Dictionary<string, string>();
                        webContent.Add("OrderNumber", order.Number);
                        webContent.Add("MfName", order.Organization.Name);
                        webContent.Add("MfOrgId", order.Organization.Id.ToString());
                        webContent.Add("WareHouseId", order.Warehouse.Id.ToString());
                        webContent.Add("WareHouseName", order.Warehouse.Name);
                        webContent.Add("DeliveryAgentName", order.Shipper.Name);
                        var trimmedToken = token.ToString().Substring(7);
                        //await _webNotificationContent.SendMsgContentToDb(trimmedToken, WebNotificationType.MfOrderDelivered, webContent);
                        await _notifcationCosmosDb.AddItemAsync(trimmedToken, WebNotificationType.MfOrderDelivered, webContent);
                    }
                    else
                    {
                        var webContent = new Dictionary<string, string>();
                        webContent.Add("StockOrderNumber", order.Number);
                        webContent.Add("TpsafName", order.Organization.Name);
                        webContent.Add("TpsafOrgId", order.Organization.Id.ToString());
                        webContent.Add("WareHouseId", order.Warehouse.Id.ToString());
                        webContent.Add("WareHouseName", order.Warehouse.Name);
                        webContent.Add("DeliveryAgentName", order.Shipper.Name);
                        var trimmedToken = token.ToString().Substring(7);
                        //await _webNotificationContent.SendMsgContentToDb(trimmedToken, WebNotificationType.MfOrderDelivered, webContent);
                        await _notifcationCosmosDb.AddItemAsync(trimmedToken, WebNotificationType.StockOrderDelivered, webContent);
                    }
                }
            }
            return Ok("Delivery successfull");
        }

        private string GetSASTokenWithURLForFullfillment(string orderNo, DeliveryType deliveryType)
        {
            var containerName = deliveryType == DeliveryType.Return ? _storageAccountConfig.ReturnOrderDocsContainer : _storageAccountConfig.FullfillOrderDocsContainer;
            var ssk = new StorageSharedKeyCredential(_storageAccountConfig.Name, _storageAccountConfig.Key);
            // Create a SAS token that's valid for 24 hours.
            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = containerName,
                Resource = "b",
                BlobName = orderNo.ToLower().Trim() + ".pdf",
            };

            sasBuilder.StartsOn = DateTimeOffset.UtcNow;
            sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddHours(24);
            sasBuilder.SetPermissions(BlobContainerSasPermissions.Read);

            // Use the key to get the SAS token.
            string sasToken = sasBuilder.ToSasQueryParameters(ssk).ToString();
            string URL = _storageAccountConfig.BlobURL + containerName + "/" + sasBuilder.BlobName + "?" + sasToken;
            return URL;
        }
    }
}
