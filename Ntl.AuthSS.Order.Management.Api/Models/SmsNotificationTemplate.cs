using Ntl.AuthSS.Notification.Entities.EmailModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ntl.AuthSS.Order_Management.Api.Models
{
    public class SmsNotificationTemplate
    {
        public NotificationModel BuildSmsNotificationTemplate(string to, string message)
        {
            var smsMessage = new SMSNotificationMessage();
            smsMessage.MessageContent = message;
            smsMessage.To = new List<string> { to };
            
            NotificationModel notificationModel = new NotificationModel();
            notificationModel.EmailContent = smsMessage;
            notificationModel.EmailContentModelType = nameof(SMSNotificationMessage);
            notificationModel.NotificationType = NotificationType.SMS;
            return notificationModel;
        }
    }
}
