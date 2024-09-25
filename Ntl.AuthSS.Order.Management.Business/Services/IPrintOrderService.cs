using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ntl.AuthSS.OrderManagement.Business.Services
{
    public interface IPrintOrderService
    {
        Task<IList<(string printOrderNumber, int warehouseId, int printPartnerId, Guid printOrderId)>> SavePrintOrderAsync(PrintOrderDto printOrderDto);
        Task<PrintOrderDto> GetPrintOrderAsync(Guid printOrderId);
        Task<PrintOrderListingDto> GetPrintOrdersAsync(PrintOrderSearchFilterOptions filterOptions);
        Task<(PrintOrderQueueDto, int orgId)> ApprovePrintOrder(Guid printOrderId, string comments, int id);
        Task<(string printOrderNumber, int warehouseId, int printPartnerId, int userId, string printPartnerName)> RejectPrintOrder(Guid printOrderId, string comments);
        Task<(Guid printOrderId,string printOrderNumber, int warehouseId, int printPartnerId)> ClosePrintOrder(Guid printOrderId, string comments);
        IList<MiniPrintOrderDto> GetPrintOrderDownloadList(PrintOrderSearchFilterOptions filterOptions);
    }
}
