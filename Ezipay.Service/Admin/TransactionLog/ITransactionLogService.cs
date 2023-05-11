using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ezipay.Database;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.CheckPasswordViewModel;

namespace Ezipay.Service.Admin.TransactionLog
{
    public interface ITransactionLogService
    {
        Task<TransactionLogResponse> GetTransactionLogs(TransactionLogRequest requestModel);
        Task<TransactionLogsResponse> GetNewTransactionLogs(TransactionLogsRequest requestModel);
        Task<MemoryStream> GenerateLogReport(DownloadLogReportRequest request);


        Task<List<CardtxndetailsResponse>> Getcardtxndetails(CardtxndetailsRequest request);
      
        Task<MemoryStream> GenerateLogReport1(DownloadLogReportRequest1 request);
        Task<List<WalletTxnResponse>> FlutterCheckTxnNotCaptureOurSide(string InvoiceNumber);
        Task<bool> UpdateFlutterCheckTxnNotCaptureOurSide(WalletTxnRequest request);
        Task<List<Fluttertxnresponse>> UpdateFlutterCheckTxn();

        Task<MemoryStream> GenerateLogReportInfo();
    }
}
