using Ezipay.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.TvRepo
{
    public class TvRepository:ITvRepository
    {
        public async Task<WalletTransaction> TvService(WalletTransaction request, long WalletUserId = 0)
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
