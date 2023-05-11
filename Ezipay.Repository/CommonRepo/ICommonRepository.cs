using Ezipay.Database;
using Ezipay.ViewModel.AirtimeViewModel;
using Ezipay.ViewModel.BannerViewModel;
using Ezipay.ViewModel.BillViewModel;
using Ezipay.ViewModel.CardPaymentViewModel;
using Ezipay.ViewModel.CheckPasswordViewModel;
using Ezipay.ViewModel.WalletUserVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.CommonRepo
{
    public interface ICommonRepository
    {
        Task<SetTransactionLimit> GetTransactionLimitForPayment(long walletUserId);

        Task<TransactionHistoryAddMoneyReponse> GetAllTransactionByDate(long walletUserId);

        Task<int> GetWalletServiceId(int walletTransactionSubTypes);
        Task<int> GetMerchantId();
        Task<bool> IsMerchant(long walletUserId, int walletServiceId);
        Task<bool> IsService(long MerchantCommissionServiceId, long WalletUserId);
        Task<MerchantCommisionMaster> MerchantCommisionMasters(long MerchantCommissionServiceId);
        Task<WalletTransaction> SaveWalletTransaction(WalletTransaction request);
        Task<WalletTransactionDetail> SaveWalletTransactionDetail(WalletTransactionDetail request);
        Task<CommisionHistory> SaveCommisionHistory(CommisionHistory request);
        Task<WalletUser> UpdateWalletUser(WalletUser request);
        Task<WalletUser> GetWalletUserByUserType(int userType, long walletUserId);

        Task<int> GetWalletServiceId(int walletTransactionSubTypes, long walletUserId);
        Task<CheckLoginResponse> CheckPassword(string token); ////
        Task<WalletUser> GetWalletUserById(long walletUserId);
       // Task<List<FAQResponse>> FAQ();
        Task<List<FeedbackTypeResponse>> FeedBackTypes();
        Task<bool> SaveFeedBack(Feedback request);
        Task<bool> ChangeNotification(WalletUser walletUser);
        Task<Callback> SendRequest(Callback callback);
        Task<bool> InsertCallbackListTracking(CallbackListTracking callbackListTracking);
        Task<List<BannerVM>> GetBanner();
        Task<UserDocumentResponse> ViewDocument(UserDocumentRequest request);
        Task<List<RecentReceiverResponse>> RecentReceiver(RecentReceiverRequest request);
        Task<DetailForBillPaymentVM> GetDetailForBillPayment(PayMoneyAggregatoryRequest request);
        Task<SessionToken> IsValidToken(string token);
        Task<SessionToken> UpdateTokenTime(string token);
    }
}
