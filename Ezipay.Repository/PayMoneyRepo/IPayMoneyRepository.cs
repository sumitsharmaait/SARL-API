using Ezipay.Database;
using Ezipay.ViewModel.CardPaymentViewModel;
using Ezipay.ViewModel.PayMoneyViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.PayMoneyRepo
{
    public interface IPayMoneyRepository
    {
        Task<WalletTransactionResponse> PayMoney(WalletTransactionRequest request);
        Task<TransactionLimitResponse> GetTransactionLimitForPayment(long walletUserId);

        Task<TransactionHistoryAddMoneyReponse> GetAllTransactionByDate(long walletUserId);

        Task<int> GetServiceId();
        Task<int> GetMerchantId();
        Task<bool> IsMerchant(long walletUserId);
        Task<bool> IsService(long MerchantCommissionServiceId, long WalletUserId);
        Task<MerchantCommisionMaster> MerchantCommisionMasters(long MerchantCommissionServiceId);
        Task<WalletTransaction> SaveWalletTransaction(WalletTransaction request);
        Task<WalletTransactionDetail> SaveWalletTransactionDetail(WalletTransactionDetail request);
        Task<CommisionHistory> SaveCommisionHistory(CommisionHistory request);
        Task<WalletUser> UpdateWalletUser(WalletUser request);
        Task<TransactionInitiateRequest> SaveTransactionInitiateRequest(TransactionInitiateRequest request);
        Task<TransactionInitiateRequest> GetTransactionInitiateRequest(long InvoiceNumber);
        Task<TransactionInitiateRequest> UpdateTransactionInitiateRequest(TransactionInitiateRequest request);
        Task<TotalTransactionCountResponse> GetTotalTransactionCount(long walletUserId);
        Task<TransactionInitiateRequest> GetTransactionInitiateRequestMerchantDetail(long Id, string InvoiceNumber);
    }
}
