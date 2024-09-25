using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business.Services
{
    public interface IStampGenerationService
    {
        StampGenerationListingDto GetStampsGenerate(StampGenerationFilterDto filter);
        List<StampGenerationDto> GetFilteredStampGenerationDownloadList(StampGenerationFilterDto filter);
    }
}
