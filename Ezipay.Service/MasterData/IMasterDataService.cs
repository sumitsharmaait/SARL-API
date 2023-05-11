
using Ezipay.ViewModel.BannerViewModel;
using Ezipay.ViewModel.BundleViewModel;
using Ezipay.ViewModel.ChannelViewModel;
using Ezipay.ViewModel.CheckPasswordViewModel;
using Ezipay.ViewModel.CommisionViewModel;
using Ezipay.ViewModel.MasterDataViewModel;
using Ezipay.ViewModel.WalletUserVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.MasterData
{
    public interface IMasterDataService
    {
        Task<List<IsdCodesResponse>> IsdCodes();

        Task<List<MainCategoryResponse>> MainCategory();

        Task<List<SubCategoryResponse>> SubCategory(SubCategoryRequest request);

        Task<List<WalletServicesList>> WalletServices(WalletServicesRequest reqquest);

        Task<List<ChannelResponce>> GetChannels(ChannelRequest request, string token);////

        Task<BundleResponse> GetBundles(IspBundlesRequest request);

        Task<AppServiceRepositoryResponse> AppServices();

        Task<List<MerchantsResponse>> Merchant(string token);////
        Task<List<commissionOnAmountModel>> ServiceCommissionListForWeb(ChannelRequest request);
        Task<List<FAQResponse>> FAQ();
        Task<bool> CheckPassword(string password, string token);////
        // String MD5Hash(MobileMoneyAggregatoryRequest request);
        //   Task<List<FAQResponse>> FAQ();
        Task<List<FeedbackTypeResponse>> FeedBackTypes();
        Task<bool> SaveFeedBack(FeedBackRequest request);
        Task<bool> ChangeNotification(string token);////
        Task<bool> SendRequest(string token);////
        Task<List<BannerVM>> GetBanner();
        Task<UserDocumentResponse> ViewDocument(UserDocumentRequest request);
        Task<List<RecentReceiverResponse>> RecentReceiver(RecentReceiverRequest request);
        Task<List<IsdCodesResponse>> IsdCodesFrancCountry();
        
        Task<List<IspChannelResponse>> GetChannelsForISP(ChannelRequest request, string token);////

        Task<bool> SaveFeedBackV2(FeedBackWebRequest requestModel);
        Task<List<ViewModel.AdminViewModel.DuplicateCardNoVMResponse>> GetcardnoList(long Walletuserid);

        Task<bool> Chargeback(long Walletuserid);
        Task<List<ManageWalletServicesList>> ManageWalletServices(WalletServicesRequest request);
        Task<int> UpdateWalletServicesStatus(UpdateWalletServicesRequest request);

        Task<List<IsdCodesResponse>> IsdCodesForXAFCountry();
        Task<List<NGNBankResponse>> GetNGNbankList(int flag);
      
        Task<List<CurrencyvalueResponseById>> GetCurrencyValue(CurrencyvalueRequestById request);
        Task<List<IsdCodesResponse>> IsdCodesAddMonMobMonCountry();

        Task<List<IsdCodesResponse>> IsdCodesPayGhanaMobMonCountry(); 
    }
}
