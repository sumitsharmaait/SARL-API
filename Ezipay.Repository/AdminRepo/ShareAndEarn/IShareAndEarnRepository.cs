using Ezipay.Database;
using Ezipay.ViewModel.ShareAndEarnViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.AdminRepo.ShareAndEarn
{
    public interface IShareAndEarnRepository
    {
        Task<int> InsertReward(ShareAndEarnMaster shareAndEarnMaster);
        Task<ShareAndEarnMaster> GetReward();
        Task<int> UpdateRewards(ShareAndEarnMaster shareAndEarnMaster);
        Task<ShareAndEarnMaster> GetRewardById(int id);
        Task<int> SaveData(ShareAndEarnDetail shareAndEarnDetail);
        Task<List<RedeemPointsHistoryResponse>> GetRedeemHistory(long walletUserId);
    }
}
