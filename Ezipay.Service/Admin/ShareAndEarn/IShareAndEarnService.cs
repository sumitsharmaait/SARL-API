using Ezipay.Database;
using Ezipay.ViewModel.ShareAndEarnViewModel;
using Ezipay.ViewModel.WalletUserVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.Admin.ShareAndEarn
{
    public interface IShareAndEarnService
    {
        Task<int> InsertReward(InsertShareRewardRequest request);
        Task<ShareAndEarnMasterResponse> GetRewardList();
        Task<Object> GetReferalUrl(long walletuserId);
        Task<object> GetUserData(string url);
        Task<bool> RedeemPoints(RedeemPointsRequest request);
        Task<List<RedeemPointsHistoryResponse>> GetRedeemHistory(long walletUserId);
    }
}
