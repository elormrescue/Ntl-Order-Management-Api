using Ntl.AuthSS.Order_Management.Api.Models;
using Ntl.AuthSS.OrderManagement.Business.NotificationModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ntl.AuthSS.OrderManagement.Business.Services
{
    public interface ICosmosDbService
    {
        Task AddItemAsync(string userToken, WebNotificationType notificationType, Dictionary<string, string> content);
    }
}
