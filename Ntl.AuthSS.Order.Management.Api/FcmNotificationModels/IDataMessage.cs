using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ntl.AuthSS.Order_Management.Api.FcmNotificationModels
{
    public interface IDataMessage
    {
        [JsonProperty("data")]
        public object Data { get; set; }
    }
}
