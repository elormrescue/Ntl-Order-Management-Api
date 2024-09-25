//using Microsoft.Azure.Cosmos;
//using Ntl.AuthSS.OrderManagement.Business.Services;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace Ntl.AuthSS.Order_Management.Api.Models
//{
//    public class WebNotificationContentTemplate
//    {
//        private readonly Container _container;
//        private static CosmosClient _cosmosClient;
//        //private readonly CosmosDbService _cosmosDbService;
//        public static WebNotificationContentTemplate()
//        {

//        }
//        public WebNotificationContentTemplate(CosmosAccessConfig cosmosAccessConfig)
//        {
//            CosmosClient cosmosClient = new CosmosClient(cosmosAccessConfig.Account, cosmosAccessConfig.Key);
//            _container = cosmosClient.GetContainer(cosmosAccessConfig.NotificationDatabase, cosmosAccessConfig.NotificationUserDetailContainer);
//        }

//        public async Task SendMsgContentToDb(string userToken, WebNotificationType notificationType, Dictionary<string, string> content)
//        {
//            var webNotificationContent = new WebNotificationContent();
//            webNotificationContent.id = Guid.NewGuid().ToString();
//            webNotificationContent.NotificationType = notificationType.ToString();
//            webNotificationContent.UserToken = userToken;
//            webNotificationContent.Content = content;
//            if (_container != null)
//                await _container.CreateItemAsync(webNotificationContent, new PartitionKey(webNotificationContent.UserToken));
//        }
//    }
//}
