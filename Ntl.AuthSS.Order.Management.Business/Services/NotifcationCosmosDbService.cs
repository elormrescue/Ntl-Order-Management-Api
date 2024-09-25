using Microsoft.Azure.Cosmos;
using Ntl.AuthSS.Order_Management.Api.Models;
using Ntl.AuthSS.OrderManagement.Business.NotificationModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ntl.AuthSS.OrderManagement.Business.Services
{
    public class NotifcationCosmosDbService : ICosmosDbService
    {
        private Container _container;

        public NotifcationCosmosDbService()
        {
        }

        public NotifcationCosmosDbService(
            CosmosClient dbClient,
            string databaseName,
            string containerName)
        {
            this._container = dbClient.GetContainer(databaseName, containerName);
        }

        public async Task AddItemAsync(string userToken, WebNotificationType notificationType, Dictionary<string, string> content)
        {
            var webNotificationContent = new WebNotificationContent();
            webNotificationContent.id = Guid.NewGuid().ToString();
            webNotificationContent.NotificationType = notificationType.ToString();
            webNotificationContent.UserToken = userToken;
            webNotificationContent.Content = content;
            if (_container != null)
                await _container.CreateItemAsync(webNotificationContent, new PartitionKey(webNotificationContent.UserToken));
        }

    }
}
