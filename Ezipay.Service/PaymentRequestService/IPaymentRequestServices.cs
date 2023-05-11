using Ezipay.Database;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.PayMoneyViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.PaymentRequestService
{
    public interface IPaymentRequestServices
    {
        Task<WalletTransactionResponse> PaymentRequest(WalletTransactionRequest request, string token);////
        Task<ViewPaymentResponse> ViewPaymentRequests(ViewPaymentRequest request, string token);////
        Task<WalletTransactionResponse> ManagePaymentRequest(ManagePayMoneyReqeust request, string token);////
        Task<ViewTransactionResponse> ViewTransactions(ViewTransactionRequest request, string token);////

        Task<DownloadReportResponse> DownloadReport(DownloadReportApiRequest request, string token);////
        Task<DownloadReportResponse> DownloadReportForApp(DownloadReportApiRequest request, string token);////

        Task<List<WalletUser>> GetWalletUser();
        Task<DownloadReportResponse> Txndetailperuser(long WalletUserId);////
        Task<DownloadReportResponse> SendTxndetailperuser(List<UserTxnReportData> model, MemoryStream memoryStream, long WalletUserId);
    }
}
