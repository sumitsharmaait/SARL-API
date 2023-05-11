using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.DashBoardViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.AdminService.DashBoardService
{
    public interface IDashBoardServices
    {
        Task<DashboardResponse> DashboardDetails(DashboardRequest request);
        Task<bool?> EnableTransactions(string sessionToken);
        Task<List<CheckUBATxnNotCaptureOurSideResponse>> CheckUBATxnNotCaptureOurSide(string InvoiceNumber);

        Task<UserBlockUnblockDetailResponse> Emailuser();
    }
}
