using Ezipay.Database;
using Ezipay.ViewModel.AdminViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.AdminRepo.Callback
{
    public class CallbackRepository : ICallbackRepository
    {
        public async Task<Database.Callback> GetCallbackById(int callbackId)
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                return await context.Callbacks.Where(x => x.CallbackId == callbackId).FirstOrDefaultAsync();
            }
        }

        public async Task<List<CallbackRecord>> GetCallbackList(SearchRequest request)
        {

            using (var db = new DB_9ADF60_ewalletEntities())
            {
                return await db.Database.SqlQuery<CallbackRecord>("exec usp_CallbackList @PageNo,@PageSize",
                 new SqlParameter("@PageNo", request.PageNumber),
                 new SqlParameter("@PageSize", request.PageSize)
                 ).ToListAsync();


            }

        }

        public async Task<int> InsertCallbackLog(CallbackListTracking entity)
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                context.CallbackListTrackings.Add(entity);
                return await context.SaveChangesAsync();
            }
        }

        public async Task<int> UpdateCallback(Database.Callback callback)
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                context.Entry(callback).State = EntityState.Modified;
                return await context.SaveChangesAsync();
            }
        }
    }
}
