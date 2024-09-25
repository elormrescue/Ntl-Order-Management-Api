using System.Collections.Generic;
using System.Text;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class ReelChangeListingDto
    {
        public List<MiniReelChangeRequestDto> MiniReelChangeRequeustDtos { get; set; }
        public int SubmittedCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public int TotalCount { get; set; }
        public int TotalRows { get; set; }
    }
}
