using Ezipay.Database;
using Ezipay.ViewModel.AdminViewModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ezipay.Repository.AdminRepo.AdminMobileMoneyLimit
{
    public class AdminMobileMoneyLimitRepository : IAdminMobileMoneyLimitRespository
    {
        public async Task<List<AdminMobileMoneyLimitResponse>> GetAdminMobileMoneyLimit()
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                return await context.Database.SqlQuery<AdminMobileMoneyLimitResponse>
                        ("EXEC usp_GetAdminMobileMoneyLimit").ToListAsync();
            }
        }

        public async Task<int> InsertAdminMobileMoneyLimit(Database.AdminMobileMoneyLimit entity, string deleteFlag)
        {
            if (deleteFlag == "delete")
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    var isExist = db.AdminMobileMoneyLimits.Where(x => x.Id == entity.Id).FirstOrDefault();
                    db.AdminMobileMoneyLimits.Remove(isExist);
                    return await db.SaveChangesAsync();

                }

            }
            else
            {
                using (var context = new DB_9ADF60_ewalletEntities())
                {
                    context.AdminMobileMoneyLimits.Add(entity);
                    return await context.SaveChangesAsync();
                }
            }
        }

    }
}
