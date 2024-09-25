using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ntl.AuthSS.Order_Management.Api.FcmNotificationModels
{
    public class FcmDataMessage : IDataMessage
    {
        public object Data { get; set; }
    }
}
