using System;
using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class StampGenerationListingDto
    {
        public IEnumerable<StampGenerationDto> StampGenerationDtos { get; set; }
        public int TotalCount { get; set; }
        public int InProcessCount { get; set; }
        public int QueuedCount { get; set; }
        public int GeneratedCount { get; set; }
        public int TotalRows { get; set; }
    }
}
