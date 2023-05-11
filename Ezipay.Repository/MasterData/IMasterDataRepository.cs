using Ezipay.Database;

using Ezipay.ViewModel.CardPaymentViewModel;
using Ezipay.ViewModel.ChannelViewModel;
using Ezipay.ViewModel.CheckPasswordViewModel;
using Ezipay.ViewModel.CommisionViewModel;
using Ezipay.ViewModel.MasterDataViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.MasterData
{
    public interface IMasterDataRepository
    {
        Task<List<IsdCodesResponse>> IsdCodes();

        Task<List<MainCategoryResponse>> MainCategory();

        Task<List<SubCategoryResponse>> SubCategory(SubCategoryRequest request);

        Task<List<WalletServicesList>> WalletServices(WalletServicesRequest reqquest);

        Task<WalletService> GetWalletServicesByIdOrChannel(string channel, long ServiceCategoryId, string ISD);
        Task<SubCategory> GetWalletSubCategoriesById(long ServiceCategoryId);

        Task<InvoiceNumberResponse> GetInvoiceNumber(int digit = 6);

        Task<List<ChannelResponce>> GetChannels(ChannelRequest request);
        Task<AddMoneyTransavtionLimitResponse> GetTransactionLimitAddMoney(string WalletUserId);
        TransactionHistoryAddMoneyReponse GetAllTransactionsAddMoney(long WalletUserId);
        Task<CommisionMaster> GetCommisionByServiceId(long serviceId);
        Task<AppServiceRepositoryResponse> AppServices();

        Task<List<MerchantsResponse>> Merchant(long WalletUserId);
        Task<List<commissionOnAmountModel>> ServiceCommissionListForWeb(ChannelRequest request);
        Task<List<FAQResponse>> FAQ();
        usp_GetCurrencyRate_Result GetCurrencyRate();
        Task<List<IsdCodesResponse>> IsdCodesFrancCountry();
        Task<List<IspChannelResponse>> GetChannelsForISP(ChannelRequest request);
        Task<WalletService> GetWalletServicesByIdOrChannel(string channel, long ServiceCategoryId);

        Task<List<ViewModel.AdminViewModel.DuplicateCardNoVMResponse>> GetcardnoList(long Walletuserid);
        Task<Country> IsdCodesby(string IsdCode);
        Task<List<ManageWalletServicesList>> ManageWalletServices(WalletServicesRequest request);
        Task<WalletService> GetWalletServicesForUpdate(long walletServiceId);
        Task<WalletService> UpdateWalletServicesStatus(WalletService walletService);
        Task<List<IsdCodesResponse>> IsdCodesForXAFCountry();
        Task<List<IsdCodesResponse>> IsdCodesAddMonMobMonCountry();
        Task<List<IsdCodesResponse>> IsdCodesPayGhanaMobMonCountry();

    }
}
