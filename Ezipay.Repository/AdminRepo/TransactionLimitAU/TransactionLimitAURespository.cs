using Ezipay.Database;
using Ezipay.ViewModel.AdminViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Data.SqlClient;

namespace Ezipay.Repository.AdminRepo.TransactionLimitAU
{
    public class TransactionLimitAURespository : ITransactionLimitAURespository
    {
        public async Task<int> InsertTransactionLimitAU(Database.TransactionLimitAU entity)
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                context.TransactionLimitAUs.Add(entity);
                return await context.SaveChangesAsync();
            }
        }

        public async Task<List<TransactionLimitAUResponse>> GetTransactionLimitAUResponseList()
        {

            using (var db = new DB_9ADF60_ewalletEntities())
            {
                return await db.Database.SqlQuery<TransactionLimitAUResponse>("exec usp_GetTransactionLimitAU @Flag",
                 new SqlParameter("@Flag", "List")).ToListAsync();
            }

        }

        public async Task<TransactionLimitAUResponse> GetTransactionLimitAUMessage()
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                return await context.Database.SqlQuery<TransactionLimitAUResponse>
                        ("EXEC usp_GetTransactionLimitAU").SingleOrDefaultAsync();
            }
        }
        //chk for airti & mobilemoney tl-au
        public async Task<TransactionLimitAUResponse> CheckTransactionLimitAU(string walletuserid)
        {
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                return await db.Database.SqlQuery<TransactionLimitAUResponse>("exec usp_GetTransactionLimitAU @Flag",
                 new SqlParameter("@Flag", walletuserid)).FirstOrDefaultAsync();
            }
        }
    }
}
