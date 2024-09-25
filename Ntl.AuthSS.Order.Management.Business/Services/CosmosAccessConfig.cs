

namespace Ntl.AuthSS.OrderManagement.Business.Services
{
    public class CosmosAccessConfig
    {
        public string Account { get; set; }
        public string Container { get; set; }
        public string Database { get; set; }
        public string Key { get; set; }
        public string StampDatabase { get; set; }
        public string StampContainer { get; set; }
        public string NotificationContainer { get; set; }
        public string NotificationUserDetailContainer { get; set; }
        public string NotificationDatabase { get; set; }
    }
}
