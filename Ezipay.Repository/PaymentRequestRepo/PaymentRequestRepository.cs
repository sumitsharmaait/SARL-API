using Ezipay.Database;
using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.PayMoneyViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Ezipay.Repository.PaymentRequestRepo
{
    public class PaymentRequestRepository : IPaymentRequestRepository
    {
        public async Task<WalletTransactionResponse> PaymentRequest(MakePaymentRequest request)
        {
            var response = new WalletTransactionResponse();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    response = await db.Database.SqlQuery<WalletTransactionResponse>("exec usp_PaymentRequest @SenderId,@ReceiverId,@Amount,@Comments,@TransactionTypeInfo",
                      new SqlParameter("@SenderId", request.SenderId),
                      new SqlParameter("@ReceiverId", request.RecieverId),
                      new SqlParameter("@Amount", request.Amount),
                      new SqlParameter("@Comments", request.Comment),
                      new SqlParameter("@TransactionTypeInfo", request.TransactionTypeInfo)
                      ).FirstOrDefaultAsync();
                    //string req = "rajdeep shakya has request 1 cedi";//sender.FirstName+""+sender.LastName+"has requested to pay"+request.Amount+" Cedi to his account.";                                         

                }
            }
            catch (Exception ex)
            {
            }
            return response;
        }

        public async Task<List<PayResponse>> ViewPaymentRequests(ViewPaymentRequest request, long walletUserId, int pageSize)
        {
            // var response = new ViewPaymentResponse();
            var response = new List<PayResponse>();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    // var user = new AppUserRepository().UserProfile();
                    if (walletUserId > 0)
                    {
                        // var adminKeys = AES256.AdminKeyPair;
                        //int PageSize = Common.PageSize;
                        // response.CurrentBalance = user.CurrentBalance;
                        response = await db.Database.SqlQuery<PayResponse>("exec usp_UserPayMoneyRequests @WalletUserId,@PageNo,@PageSize",
                         new SqlParameter("@WalletUserId", walletUserId),
                         new SqlParameter("@PageNo", request.PageNo),
                         new SqlParameter("@PageSize", pageSize)
                         ).ToListAsync();

                        //response.PaymentRequests = list;
                        //response.PageSize = PageSize;
                        //response.TotalCount = list.Count();
                    }
                }

            }
            catch (Exception ex)
            {
                //ex.Message.ErrorLog("WalletTransactionRepository.cs", "ViewPaymentRequests");

            }
            return response;
        }

        public async Task<PayMoneyRequest> GetPayMoneyRequests(long payMoneyRequestId)
        {
            var response = new PayMoneyRequest();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {

                    response = await db.PayMoneyRequests.Where(x => x.PayMoneyRequestId == payMoneyRequestId).FirstOrDefaultAsync();
                }
            }
            catch
            {

            }
            return response;
        }

        public async Task<PayMoneyRequest> UpdatePayMoneyRequests(PayMoneyRequest payMoneyRequestId)
        {
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    db.Entry(payMoneyRequestId).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                }
            }
            catch
            {

            }
            return payMoneyRequestId;
        }

        public async Task<int> GetWalletServiceIdBySubType(int walletTransactionSubTypes)
        {
            int WalletServiceId = 0;
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {

                    WalletServiceId = (int)await db.WalletServices.Where(x => x.ServiceCategoryId == walletTransactionSubTypes).Select(x => x.WalletServiceId).FirstOrDefaultAsync();
                }
            }
            catch
            {

            }
            return WalletServiceId;
        }

        public async Task<bool> GetAnyService(long walletUserId)
        {
            bool WalletServiceId = false;
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {

                    WalletServiceId = await db.WalletServices.AnyAsync(x => x.MerchantId == walletUserId);
                }
            }
            catch
            {

            }
            return WalletServiceId;
        }

        public async Task<bool> GetAnyService(long walletUserId, int MerchantCommissionServiceId)
        {
            bool WalletServiceId = false;
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {

                    WalletServiceId = await db.WalletServices.AnyAsync(x => x.WalletServiceId == MerchantCommissionServiceId && x.MerchantId == walletUserId);
                }
            }
            catch
            {

            }
            return WalletServiceId;
        }

        public async Task<MerchantCommisionMaster> GetMerchantCommisionMasters(int MerchantCommissionServiceId)
        {
            var response = new MerchantCommisionMaster();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {

                    response = await db.MerchantCommisionMasters.Where(x => x.WalletServiceId == MerchantCommissionServiceId && (bool)x.IsActive).FirstOrDefaultAsync();
                }
            }
            catch
            {

            }
            return response;
        }

        public async Task<CommisionHistory> SaveCommisionHistory(CommisionHistory request, int save)
        {
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    if (save == 1)
                    {
                        db.CommisionHistories.Add(request);
                    }
                    else if (save == 2)
                    {
                        db.CommisionHistories.Add(request);
                        await db.SaveChangesAsync();
                    }

                }
            }
            catch
            {

            }
            return request;
        }

        public async Task<ViewTransactionResponse> ViewTransactions(ViewTransactionRequest request, long walletUserId)
        {
            var response = new ViewTransactionResponse();
            var list = new List<ViewTransactionResult>();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    //var user = new AppUserRepository().UserProfile();
                    if (walletUserId > 0)
                    {
                        var adminKeys = AES256.AdminKeyPair;
                        int PageSize = CommonSetting.PageSize;

                        list = await db.Database.SqlQuery<ViewTransactionResult>("exec usp_PaymentTransactions_NewApp1 @UserId,@TransactionType,@PageNo,@PageSize",
                         new SqlParameter("@UserId", walletUserId),
                         new SqlParameter("@TransactionType", request.TransactionType),
                         new SqlParameter("@PageNo", request.PageNo),
                         new SqlParameter("@PageSize", PageSize)
                         ).ToListAsync();
                        response.TransactionList = list.Select(x => { x.Pagesize = PageSize; return x; }).ToList();
                    }
                }

            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("PaymentRequestRepository.cs", "ViewPaymentRequests");

            }
            return response;

        }

        public async Task<int> UpdateWalletUser(WalletUser request)
        {
            int result = 0;
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    db.Entry(request).State = EntityState.Modified;
                    result = await db.SaveChangesAsync();
                }
            }
            catch
            {

            }
            return result;
        }

        public async Task<DownloadReportResponse> DownloadReportWithData(DownloadReportApiRequest request)
        {
            var response = new DownloadReportResponse();
            var list = new List<ReportData>();
            list.Add(new ReportData());
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    if (request.WalletUserId > 0)
                    {
                        if (request.DateFrom == null || request.DateFrom == DateTime.MinValue || request.DateTo == null || request.DateTo == DateTime.MinValue)
                        {
                            list = await db.Database.SqlQuery<ReportData>("exec usp_PaymentTransactionReport @UserId,@TransactionType",
                                 new SqlParameter("@UserId", request.WalletUserId),
                                 new SqlParameter("@TransactionType", request.TransactionType)).ToListAsync();
                        }
                        else
                        {
                            list = await db.Database.SqlQuery<ReportData>("exec usp_PaymentTransactionReport @UserId,@TransactionType,@DateFrom,@DateTo",
                             new SqlParameter("@UserId", request.WalletUserId),
                             new SqlParameter("@TransactionType", request.TransactionType),
                             new SqlParameter("@DateFrom", request.DateFrom),
                             new SqlParameter("@DateTo", request.DateTo)
                             ).ToListAsync();
                        }
                        response.ReportData = list;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("WalletTransactionRepository.cs", "DownloadReport");

            }
            return response;
        }


        public async Task<DownloadReportResponse> DownloadReportForApp(DownloadReportApiRequest request)
        {
            DownloadReportResponse response = new DownloadReportResponse();
            List<ReportData> list = new List<ReportData>();
            list.Add(new ReportData());
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    if (request.WalletUserId != null && request.WalletUserId > 0)
                    {
                        if (request.DateFrom == null || request.DateFrom == DateTime.MinValue || request.DateTo == null || request.DateTo == DateTime.MinValue)
                        {
                            list = await db.Database.SqlQuery<ReportData>("exec usp_PaymentTransactionReport @UserId,@TransactionType",
                                 new SqlParameter("@UserId", request.WalletUserId),
                                 new SqlParameter("@TransactionType", request.TransactionType)).ToListAsync();
                        }
                        else
                        {
                            list = await db.Database.SqlQuery<ReportData>("exec usp_PaymentTransactionReport @UserId,@TransactionType,@DateFrom,@DateTo",
                             new SqlParameter("@UserId", request.WalletUserId),
                             new SqlParameter("@TransactionType", request.TransactionType),
                             new SqlParameter("@DateFrom", request.DateFrom),
                             new SqlParameter("@DateTo", request.DateTo)
                             ).ToListAsync();
                        }
                        response.ReportData = list;
                        response.WalletUserId = request.WalletUserId;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("WalletTransactionRepository.cs", "DownloadReport");

            }
            return response;
        }

        public async Task<TransactionInitiateRequest> SaveTransactionInitiateRequest(TransactionInitiateRequest request)
        {
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    db.TransactionInitiateRequests.Add(request);
                    int res = await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {

            }
            return request;
        }

        public async Task<List<WalletUser>> GetWalletUser()
        {
            List<WalletUser> list1 = new List<WalletUser>();
            list1.Add(new WalletUser());
           List<long> foo = new List<long>() { 7315 };//test need to remove

            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    list1 = await db.WalletUsers.Where(x => foo.Contains(x.WalletUserId) && x.IsActive == true
                     && x.IsDeleted == false && x.IsEmailVerified == true
                          && x.IsOtpVerified == true).ToListAsync();

                    //list1 = await db.WalletUsers.Where(x => x.IsActive == true && x.IsDeleted == false && x.IsEmailVerified == true
                    //      && x.IsOtpVerified == true).ToListAsync();

                }

            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("WalletTransactionRepository.cs", "DownloadReporttxn");

            }
            return list1;

        }


        public async Task<DownloadReportResponse> Txndetailperuser(long WalletUserId)
        {
            DownloadReportResponse response = new DownloadReportResponse();
            List<UserTxnReportData> list = new List<UserTxnReportData>();
            list.Add(new UserTxnReportData());
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    list = await db.Database.SqlQuery<UserTxnReportData>("exec usp_txnlist @WalletUserId", new SqlParameter("@WalletUserId", WalletUserId)).ToListAsync();
                    response.UserTxnReportData = list;
                }
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("WalletTransactionRepository.cs", "DownloadReporttxn");

            }
            return response;
        }        
    }
}
