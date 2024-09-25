using CorePush.Google;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ntl.AuthSS.Order_Management.Api.HttpClients
{
    public class FcmClient
    {
        private readonly HttpClient _httpClient;
        private readonly FcmSettings _fcmSettings;

        public FcmClient(HttpClient httpClient, FcmSettings fcmSettings)
        {
            _httpClient = httpClient;
            _fcmSettings = fcmSettings;
        }

        public async Task SendFcmMessage(object fcmMessage, string deviceToken)
        {
            var fcm = new FcmSender(_fcmSettings, _httpClient);
            await fcm.SendAsync(deviceToken, fcmMessage);
        }
    }
}
