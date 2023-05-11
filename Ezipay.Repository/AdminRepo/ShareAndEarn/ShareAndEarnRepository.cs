using Ezipay.Database;
using Ezipay.ViewModel.ShareAndEarnViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.AdminRepo.ShareAndEarn
{
    public class ShareAndEarnRepository : IShareAndEarnRepository
    {
        public async Task<int> InsertReward(ShareAndEarnMaster shareAndEarnMaster)
        {
            int result = 0;
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                db.ShareAndEarnMasters.Add(shareAndEarnMaster);
                result = await db.SaveChangesAsync();
            }
            return result;
        }

        public async Task<ShareAndEarnMaster> GetReward()
        {
            var result = new ShareAndEarnMaster();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                result = await db.ShareAndEarnMasters.Where(x => x.IsActive == true).FirstOrDefaultAsync();
            }
            return result;
        }
        public async Task<int> UpdateRewards(ShareAndEarnMaster shareAndEarnMaster)
        {
            int result = 0;
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                db.Entry(shareAndEarnMaster).State = EntityState.Modified;
                result = await db.SaveChangesAsync();
            }
            return result;
        }

        public async Task<ShareAndEarnMaster> GetRewardById(int id)
        {
            var result = new ShareAndEarnMaster();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                result = await db.ShareAndEarnMasters.Where(x => x.Id == id).FirstOrDefaultAsync();
            }
            return result;
        }

        public async Task<int> SaveData(ShareAndEarnDetail shareAndEarnDetail)
        {
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                db.ShareAndEarnDetails.Add(shareAndEarnDetail);
                return await db.SaveChangesAsync();
            }
        }

        public async Task<List<RedeemPointsHistoryResponse>> GetRedeemHistory(long walletUserId)
        {
            var result = new List<RedeemPointsHistoryResponse>();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                result = await db.Database.SqlQuery<RedeemPointsHistoryResponse>("exec usp_GetRedeem_History @WalletUserId",
                    new object[]
                    {
                        new SqlParameter("@WalletUserId",walletUserId)
                    }
                    ).ToListAsync();
            }
            return result;
        }
    }
}
