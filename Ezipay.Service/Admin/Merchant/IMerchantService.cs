using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.CheckPasswordViewModel;

namespace Ezipay.Service.Admin.Merchant
{
    public interface IMerchantService
    {
        Task<MerchantListResponse> GetMerchantList(MerchantListRequest request);
        Task<MerchantSaveResponse> SaveMerchant(MerchantRequest request);
        Task<bool> EnableDisableMerchant(MerchantManageRequest request);
        Task<bool> DeleteMarchant(MarchantDeleteRequest request);
        Task<bool> EnableDisableTransaction(MerchantEnableTransactionRequest request);
        Task<ViewMarchantTransactionResponse> ViewMerchantTransactions(ViewMarchantTransactionRequest request);
        Task<MemoryStream> ExportMerchantListReport(DownloadLogReportRequest request);
        Task<MerchantSaveResponse> MerchantOnBoardRequest(MerchantRequest request);
        Task<int> SaveStore(AddStoreRequest request);
        Task<List<StoreResponse>> GetStores(StoreSearchRequest request);
        Task<int> DeleteStore(StoreDeleteRequest requestModel);
        Task<int> EnableDisableStore(StoreManageRequest request);
    }
}
