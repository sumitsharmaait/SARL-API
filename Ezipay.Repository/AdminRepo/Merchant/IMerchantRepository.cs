using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ezipay.Database;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.CheckPasswordViewModel;

namespace Ezipay.Repository.AdminRepo.Merchant
{
    public interface IMerchantRepository
    {
        Task<List<MerchantList>> GetMerchantList(MerchantListRequest request);
        Task<int> InsertMerchant(WalletUser walletUser, WalletService walletService, MerchantCommisionMaster commisionMaster);
        Task<WalletService> GetWalletServiceByUserId(long walletUserId);
        Task<List<MerchantCommisionMaster>> GetCommissionByServiceId(int walletServiceId);
        Task<int> UpdateMerchant(WalletUser walletUser, WalletService walletService, MerchantCommisionMaster objCommission, List<MerchantCommisionMaster> commisionList);
        Task<bool> DeleteSubadmin(MarchantDeleteRequest request);
        Task<List<TransactionDetails>> ViewMerchantTransactions(ViewMarchantTransactionRequest request);
        Task<MerchantListResponse> DownLoadMerchantLogList(DownloadLogReportRequest request);
        Task<int> InsertStore(MerchantStore storeEntity);
        Task<int> UpdateStore(MerchantStore storeEntity);
        Task<MerchantStore> GetStoreById(long storeId);
        Task<List<StoreResponse>> GetStores(StoreSearchRequest request);
        Task<int> OnBoardRequest(WalletUser walletUser, UserDocument docEntity, List<MerchantDocument> docs, BankDetail bankEntity, WalletService walletService, MerchantCommisionMaster commisionMaster, UserApiKey userApiKey);
    }
}
