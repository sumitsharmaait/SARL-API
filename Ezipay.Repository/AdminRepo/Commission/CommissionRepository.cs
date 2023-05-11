using Ezipay.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.AdminRepo.Commission
{
    public class CommissionRepository : ICommissionRepository
    {
        public async Task<List<CommisionMaster>> GetCommissionByWalletServiceId(int walletServiceId)
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                return await context.CommisionMasters.AsQueryable().Where(x => x.WalletServiceId == walletServiceId && x.IsActive==true).ToListAsync();
            }
        }

        public async Task<int> InsertCommission(CommisionMaster objCommission, List<CommisionMaster> commission)
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                context.CommisionMasters.Add(objCommission);
                await context.SaveChangesAsync();

                commission.ForEach(x =>
                {
                    context.Entry(x).State = EntityState.Modified;
                    context.SaveChanges();
                });
                return 1;
            }
        }
    }
}
