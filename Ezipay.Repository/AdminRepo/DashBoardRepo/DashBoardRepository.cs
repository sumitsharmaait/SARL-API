using Ezipay.Database;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.DashBoardViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Ezipay.Repository.AdminRepo.DashBoardRepo
{
    public class DashBoardRepository : IDashBoardRepository
    {
        public async Task<DashboardResponse> DashboardDetails(DashboardRequest request)
        {
            var response = new DashboardResponse();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                if (request.DateFrom == DateTime.MinValue || request.DateTo == DateTime.MinValue || request.DateFrom == null || request.DateTo == null)
                {
                    response = await db.Database.SqlQuery<DashboardResponse>
                           ("EXEC usp_WalletSummary"
                            ).FirstOrDefaultAsync();
                }
                else
                {
                    response = await db.Database.SqlQuery<DashboardResponse>

                          ("EXEC usp_WalletSummary @DateFrom,@DateTo",
                             new SqlParameter("@DateFrom", request.DateFrom),
                                         new SqlParameter("@DateTo", request.DateTo)
                           ).FirstOrDefaultAsync();
                }
            }
            return response;
        }

        public async Task<bool?> EnableTransactions()
        {
            bool? objResponse = null;
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                var setting = await db.WalletSettings.Where(w => w.SettingKey == "TransactionsEnabled").FirstOrDefaultAsync();
                if (setting != null)
                {
                    var enabled = Convert.ToInt32(setting.SettingValue);
                    enabled = 1 - enabled;
                    setting.SettingValue = enabled.ToString();
                    db.SaveChanges();
                    objResponse = Convert.ToBoolean(enabled);
                }
            }
            return objResponse;
        }


        public async Task<List<CheckUBATxnNotCaptureOurSideResponse>> CheckUBATxnNotCaptureOurSide(string InvoiceNumber)
        {
            var result = new List<CheckUBATxnNotCaptureOurSideResponse>();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {

                    result = await (from x in db.TransactionInitiateRequests
                                    join sa in db.ViewUserLists on x.WalletUserId equals sa.WalletUserId
                                    where x.InvoiceNumber == InvoiceNumber.Trim()
                                    select new CheckUBATxnNotCaptureOurSideResponse
                                    {
                                        EmailId = sa.EmailId,
                                        WalletUserId = x.WalletUserId,
                                        RequestedAmount = x.RequestedAmount,
                                        CreatedDate = x.CreatedDate,
                                        InvoiceNumber = x.InvoiceNumber
                                    }).ToListAsync();
                }
            }
            catch (Exception ex)
            {

            }
            return result;
        }


        public async Task<UserBlockUnblockDetailResponse> Emailuser()
        {
           
            var list = new List<UserBlockUnblockDetail1>();
            var objResponse = new UserBlockUnblockDetailResponse();
          

            using (var db = new DB_9ADF60_ewalletEntities())
            {
                // list =await db.usp_UserList(request.SearchText, request.PageNumber, request.PageSize).ToListAsync();
                list = await db.Database.SqlQuery<UserBlockUnblockDetail1>
                                      ("EXEC usp_UserBlockUnblockDetailEmailList").ToListAsync();

                if (list != null && list.Count > 0)
                {
                    objResponse = new UserBlockUnblockDetailResponse
                    {
                        TotalCount = list.FirstOrDefault().TotalCount,
                        UserList = list,

                    };
                }
                else
                {
                    objResponse.UserList = new List<UserBlockUnblockDetail1>();
                }
            }

            return objResponse;
        }

    }
}
