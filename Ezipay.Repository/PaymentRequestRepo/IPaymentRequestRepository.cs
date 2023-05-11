using Ezipay.Database;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.PayMoneyViewModel;
using Ezipay.ViewModel.WalletUserVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.PaymentRequestRepo
{
    public interface IPaymentRequestRepository
    {
        Task<WalletTransactionResponse> PaymentRequest(MakePaymentRequest request);
        Task<List<PayResponse>> ViewPaymentRequests(ViewPaymentRequest request, long walletUserId, int pageSize);
        Task<PayMoneyRequest> GetPayMoneyRequests(long payMoneyRequestId);
        Task<PayMoneyRequest> UpdatePayMoneyRequests(PayMoneyRequest payMoneyRequestId);
        Task<int> GetWalletServiceIdBySubType(int walletTransactionSubTypes);
        Task<bool> GetAnyService(long walletUserId);
        Task<bool> GetAnyService(long walletUserId, int MerchantCommissionServiceId);
        Task<MerchantCommisionMaster> GetMerchantCommisionMasters(int MerchantCommissionServiceId);
        Task<CommisionHistory> SaveCommisionHistory(CommisionHistory request, int save);
        Task<ViewTransactionResponse> ViewTransactions(ViewTransactionRequest request, long walletUserId);
        Task<int> UpdateWalletUser(WalletUser request);
        Task<DownloadReportResponse> DownloadReportWithData(DownloadReportApiRequest request);
        Task<DownloadReportResponse> DownloadReportForApp(DownloadReportApiRequest request);
        Task<TransactionInitiateRequest> SaveTransactionInitiateRequest(TransactionInitiateRequest request);

        Task<List<WalletUser>> GetWalletUser();
        Task<DownloadReportResponse> Txndetailperuser(long WalletUserId);

        
    }
}
