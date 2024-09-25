using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ntl.AuthSS.Order_Management.Api.ConfigModels
{
    public class StorageAccountConfig
    {
        public string Name { get; set; }
        public string Key { get; set; }

        public string FullfillOrderDocsContainer { get; set; }
        public string ReturnOrderDocsContainer { get; set; }
        public string BlobURL { get; set; }
    }
}
