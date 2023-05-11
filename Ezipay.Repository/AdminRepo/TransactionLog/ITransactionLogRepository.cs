using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ezipay.Database;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.CheckPasswordViewModel;

namespace Ezipay.Repository.AdminRepo.TransactionLog
{
    public interface ITransactionLogRepository
    {
        Task<List<TransactionLogRecord>> GetTransactionLogs(TransactionLogRequest request);
        Task<List<TransactionLogslist>> GetNewTransactionLogs(TransactionLogsRequest request);
        Task<TransactionLogsResponce> GenerateLogReport(DownloadLogReportRequest request);


        Task<List<Carduseinaddmoney>> Getcardtxndetails(CardtxndetailsRequest cc);


        Task<MonthlyreportResponce> GenerateLogReport1(DownloadLogReportRequest1 request);
        Task<int> UpdateFlutterCheckTxn(Fluttertxnresponse entity);
        Task<int> UpdateFlutterCheckTxnNotCaptureOurSide(WalletTxnRequest entity);

        Task<int> UpdateFlagonwebhookflutter(string InvoiceNumber);

        Task<TransactionLogsResponse2> GenerateLogReportInfo();
    }
}
