using Ntl.AuthSS.OrderManagement.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business
{
   public class PackageDto
    {
        public Guid PackageId { get; set; }
        public string Code { get; set; }
        public int StampCount { get; set; }
        public PackageType packageType { get; set; }
        public int ReelCount { get; set; }
        //added for File processing
        public string ProductNameOrigin { get; set; }
        //Added for print
        public string PalletCode { get; set; }
    }
}
