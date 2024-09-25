using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business.NotificationModel
{
    public class WebNotificationContent
    {
        public string id { get; set; } //cosmos reference
        public string NotificationType { get; set; }
        public string UserToken { get; set; }
        public Dictionary<string, string> Content { get; set; }
    }
}
