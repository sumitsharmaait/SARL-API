using Ezipay.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.InterNetProviderRepo
{
    public class InterNetProviderRepository : IInterNetProviderRepository
    {
        public async Task<WalletTransaction> ISPServices(WalletTransaction request, long WalletUserId = 0)
        {
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    db.WalletTransactions.Add(request);
                    int res = await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {

            }
            return request;
        }
    }
}
