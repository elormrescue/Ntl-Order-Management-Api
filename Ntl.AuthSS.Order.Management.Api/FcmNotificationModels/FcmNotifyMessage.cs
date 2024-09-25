using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ntl.AuthSS.Order_Management.Api.FcmNotificationModels
{
    public class FcmNotifyMessage : IDataMessage, INotificationMessage
    {
        [JsonProperty("data")]
        public object Data { set; get; }
        [JsonProperty("notification")]
        public object Notification { get; set; }
    }
}
