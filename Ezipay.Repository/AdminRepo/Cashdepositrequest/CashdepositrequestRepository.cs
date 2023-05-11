using Ezipay.Database;
using Ezipay.Utility.Extention;
using Ezipay.ViewModel.AdminViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Ezipay.Repository.AdminRepo.Cashdepositrequest
{
    public class CashdepositrequestRepository : ICashdepositrequestRespository
    {
        
        public async Task<List<CashdepositrequestResponse>> Getcashdepositrequest(CashdepositrequestRequest cr)
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                return await context.Database.SqlQuery<CashdepositrequestResponse>
                        ("EXEC usp_DepositorList @SearchText,@PageNo,@PageSize",
                        new SqlParameter("@SearchText", cr.SearchText),
                        new SqlParameter("@PageNo", cr.PageNumber),
                        new SqlParameter("@PageSize", cr.PageSize)
                        ).ToListAsync();
            }
        }
        

        public async Task<int> Updatecashdepositrequest(CashdepositrequestRequest Request)
        {         
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    var Data = db.Cashdeposituser_addmoney.Where(x => x.id == Request.id && x.WalletUserId == Request.WalletUserId).FirstOrDefault();
                    if (Data != null)
                    {
                        Data.DepositStatus = Request.DepositStatus;
                        Data.Isactive = Request.Isactive;
                        Data.Reason = Request.Reason;
                        Data.DepositStatusUpdateDate = DateTime.UtcNow;

                        db.Entry(Data).State = EntityState.Modified;
                        await db.SaveChangesAsync();
                    }
                    else
                    {

                        return -1;
                    }
                    return  1;
                }
            }

            catch (Exception ex)
            {
                ex.Message.ErrorLog("CashdepositrequestRepository.cs", "Updatecashdepositrequest");
                return -1;

            }


        }

    }
}
