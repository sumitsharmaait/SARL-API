using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ezipay.Database;
using Ezipay.ViewModel.AdminViewModel;

namespace Ezipay.Repository.AdminRepo.Subscription
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        public async Task<List<SubscriptionLogRecord>> GetSubscriptionLogs(SearchRequest request)
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                return await context.Database.SqlQuery<SubscriptionLogRecord>("exec usp_SubscriptionLogs @PageNo,@PageSize",
                     new SqlParameter("@PageNo", request.PageNumber),
                     new SqlParameter("@PageSize", request.PageSize)
                     ).ToListAsync();
            }
        }
    }
}
