using Ezipay.ViewModel.AirtimeViewModel;
using Ezipay.ViewModel.BannerViewModel;
using Ezipay.ViewModel.CheckPasswordViewModel;
using Ezipay.ViewModel.WalletUserVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.CommonService
{
    public interface ICommonServices
    {
        Task<bool> CheckPassword(string password, string token);
        //  // String MD5Hash(MobileMoneyAggregatoryRequest request);
        ////   Task<List<FAQResponse>> FAQ();
        //   Task<List<FeedbackTypeResponse>> FeedBackTypes();
        //   Task<bool> SaveFeedBack(FeedBackRequest request);
        //   Task<bool> ChangeNotification();
        //   Task<bool> SendRequest();
        //   Task<List<BannerVM>> GetBanner();
        //   Task<UserDocumentResponse> ViewDocument(UserDocumentRequest request);
        //   Task<List<RecentReceiverResponse>> RecentReceiver(RecentReceiverRequest request);
        String SHA1Hash(string request);
        Task<bool> IsUserValid(string token, long walletUserId, decimal RequestedAmount);
    }
}
