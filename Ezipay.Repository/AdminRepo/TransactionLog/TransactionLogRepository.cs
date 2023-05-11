using Ezipay.Database;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.CheckPasswordViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Ezipay.Repository.AdminRepo.TransactionLog
{
    public class TransactionLogRepository : ITransactionLogRepository
    {
        public async Task<List<TransactionLogslist>> GetNewTransactionLogs(TransactionLogsRequest request)
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {

                return await context.Database.SqlQuery<TransactionLogslist>("exec usp_NewFormate_TransactionLogs @PageNo,@PageSize,@transactionsType,@WalletTransactionId,@transactionid,@Date,@Time,@categoryname,@servicename,@totalAmount,@walletAmount,@name,@accountNo,@walletuserid",
                     new SqlParameter("@PageNo", request.PageNumber),
                     new SqlParameter("@PageSize", request.PageSize),
                     new SqlParameter("@transactionsType", request.transactionsType),
                     new SqlParameter("@WalletTransactionId", request.WalletTransactionId),
                     new SqlParameter("@transactionid", request.transactionid),
                     new SqlParameter("@Date", request.Date),
                     new SqlParameter("@Time", request.Time),
                     new SqlParameter("@categoryname", request.categoryname),
                     new SqlParameter("@servicename", request.servicename),
                     new SqlParameter("@totalAmount", request.totalAmount),
                     new SqlParameter("@walletAmount", request.walletAmount),
                     new SqlParameter("@name", request.name),
                     new SqlParameter("@accountNo", request.accountNo),
                     new SqlParameter("@walletuserid", request.walletuserid)
                     ).ToListAsync();
            }
        }

        public async Task<List<TransactionLogRecord>> GetTransactionLogs(TransactionLogRequest request)
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                return await context.Database.SqlQuery<TransactionLogRecord>("exec usp_TransactionLogs @TransactionType,@PageNo,@PageSize",
                     new SqlParameter("@TransactionType", request.TransactionType),
                     new SqlParameter("@PageNo", request.PageNumber),
                     new SqlParameter("@PageSize", request.PageSize)
                     ).ToListAsync();
            }
        }


        public async Task<TransactionLogsResponce> GenerateLogReport(DownloadLogReportRequest request)
        {
            var response = new TransactionLogsResponce();
            var list = new List<TransactionLogslist>();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {

                    list = await db.Database.SqlQuery<TransactionLogslist>("exec usp_NewFormate_TransactionLogs_Download @DateFrom,@DateTo",
                     new SqlParameter("@DateFrom", request.DateFrom),
                     new SqlParameter("@DateTo", request.DateTo)
                     ).ToListAsync();

                    if (list != null && list.Count > 0)
                    {

                        response = new TransactionLogsResponce
                        {
                            TotalCount = list.FirstOrDefault().TotalCount,
                            TransactionLogslist = list
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                //  ex.Message.ErrorLog("WalletTransactionRepository.cs", "ViewPaymentRequests");
            }
            return response;

        }

        public async Task<List<Carduseinaddmoney>> Getcardtxndetails(CardtxndetailsRequest request)
        {
            //var result = new CardtxndetailsResponseo();
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                //return await context.Carduseinaddmoneys.OrderByDescending(x => x.CreatedDate).ToListAsync();


                return await context.Database.SqlQuery<Carduseinaddmoney>
                       ("EXEC usp_CardTransactionDetailsUseList @SearchText,@PageNo,@PageSize",
                       new SqlParameter("@SearchText", request.SearchText),
                       new SqlParameter("@PageNo", request.PageNumber),
                       new SqlParameter("@PageSize", request.PageSize)
                       ).ToListAsync();


                // result.TotalCount = await context.Carduseinaddmoneys.OrderByDescending(x => x.CreatedDate).CountAsync();
            }
            //return result;

        }

        public async Task<MonthlyreportResponce> GenerateLogReport1(DownloadLogReportRequest1 request)
        {
            var response = new MonthlyreportResponce();
            var list = new List<Monthlyreportlist>();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    list = await db.Database.SqlQuery<Monthlyreportlist>("exec sp_Monthlyreport @Month, @Yr",
                     new SqlParameter("@Month", request.Month),
                     new SqlParameter("@Yr", request.Yr)
                     ).ToListAsync();

                    if (list != null && list.Count > 0)
                    {

                        response = new MonthlyreportResponce
                        {
                            //TotalCount = list.FirstOrDefault().TotalCount,
                            Monthlyreportlist = list
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                //  ex.Message.ErrorLog("WalletTransactionRepository.cs", "ViewPaymentRequests");
            }
            return response;

        }

        public async Task<int> UpdateFlutterCheckTxn(Fluttertxnresponse Request)
        {
            var objResponse = new CreditDebitResponse();
            try
            {
                using (var db1 = new DB_9ADF60_ewalletEntities())
                {
                    var getInitialTransaction = await db1.TransactionInitiateRequests.Where(x => x.InvoiceNumber == Request.InvoiceNo).FirstOrDefaultAsync();
                    var txnstatus = db1.WalletTransactions.Where(x => x.SenderId == Request.UserId && x.InvoiceNo == Request.InvoiceNo).FirstOrDefault();
                    if (txnstatus == null)
                    {

                        int i = await UpdateFlagonwebhookflutter(Request.InvoiceNo);

                        if (i == 1)
                        {
                            int serviceId = db1.WalletServices.Where(x => x.ServiceCategoryId == 4).Select(x => x.WalletServiceId).FirstOrDefault();
                            if (serviceId > 0)
                            {
                                var adminUser = db1.WalletUsers.Where(x => x.UserType == 2).FirstOrDefault();
                                if (adminUser != null)
                                {


                                    var Reason = "fluttercredited amount :- " + getInitialTransaction.RequestedAmount + " against Txn Id :- " + getInitialTransaction.InvoiceNumber;
                                    objResponse = await db1.Database.SqlQuery<CreditDebitResponse>
                                                                                          ("EXEC usp_CreditDebitUser @SenderId,@ReceiverId,@TransactionAmount,@Reason,@ServiceId,@TransactionDate,@IsCredit,@TxnId",
                                                                                          new SqlParameter("@SenderId", Request.TransactionType ? adminUser.WalletUserId : getInitialTransaction.WalletUserId),
                                                                                            new SqlParameter("@ReceiverId", Request.TransactionType ? getInitialTransaction.WalletUserId : adminUser.WalletUserId),
                                                                                            new SqlParameter("@TransactionAmount", getInitialTransaction.RequestedAmount),
                                                                                            new SqlParameter("@Reason", Reason),
                                                                                            new SqlParameter("@ServiceId", serviceId),
                                                                                            new SqlParameter("@TransactionDate", DateTime.UtcNow),
                                                                                            new SqlParameter("@IsCredit", Request.TransactionType),
                                                                                            new SqlParameter("@TxnId", Request.InvoiceNo)
                                                                                          ).FirstOrDefaultAsync();

                                    //emailuser
                                }
                                else
                                {
                                    return -1;
                                }
                            }
                            else
                            {
                                return -1;
                            }
                            //
                            if (objResponse.RstKey == 1)
                            {
                                return 1;
                            }
                            else
                            {
                                return -1;
                            }
                        }
                        else
                        {
                            return -1;
                        }

                    }
                    else
                    {
                        return -1;
                    }
                }
            }

            catch (Exception ex)
            {

                return -1;

            }



        }


        public async Task<int> UpdateFlutterCheckTxnNotCaptureOurSide(WalletTxnRequest Request)
        {
            var objResponse = new CreditDebitResponse();
            try
            {
                using (var db1 = new DB_9ADF60_ewalletEntities())
                {
                    var getInitialTransaction = await db1.TransactionInitiateRequests.Where(x => x.InvoiceNumber == Request.InvoiceNo).FirstOrDefaultAsync();
                    var txnstatus = db1.WalletTransactions.Where(x => x.SenderId == Request.UserId && x.InvoiceNo == Request.InvoiceNo).FirstOrDefault();
                    if (txnstatus == null)
                    {
                        var entity = new Database.WalletTxnUpdateList
                        {
                            WalletTxn = 00,
                            UpdatebyAdminWalletID = Request.UpdatebyAdminWalletID,
                            WalletTxnStatus = "1",
                            CreatedDate = DateTime.UtcNow,


                        };
                        db1.WalletTxnUpdateLists.Add(entity);
                        int i = await db1.SaveChangesAsync();
                        if (i == 1)
                        {
                            int serviceId = db1.WalletServices.Where(x => x.ServiceCategoryId == 4).Select(x => x.WalletServiceId).FirstOrDefault();
                            if (serviceId > 0)
                            {
                                var adminUser = db1.WalletUsers.Where(x => x.UserType == 2).FirstOrDefault();
                                if (adminUser != null)
                                {


                                    Request.Reason = "flutter credited amount :- " + getInitialTransaction.RequestedAmount + " against Txn Id :- " + getInitialTransaction.InvoiceNumber;
                                    objResponse = await db1.Database.SqlQuery<CreditDebitResponse>
                                                                                          ("EXEC usp_CreditDebitUser @SenderId,@ReceiverId,@TransactionAmount,@Reason,@ServiceId,@TransactionDate,@IsCredit,@TxnId",
                                                                                          new SqlParameter("@SenderId", Request.TransactionType ? adminUser.WalletUserId : getInitialTransaction.WalletUserId),
                                                                                            new SqlParameter("@ReceiverId", Request.TransactionType ? getInitialTransaction.WalletUserId : adminUser.WalletUserId),
                                                                                            new SqlParameter("@TransactionAmount", getInitialTransaction.RequestedAmount),
                                                                                            new SqlParameter("@Reason", Request.Reason),
                                                                                            new SqlParameter("@ServiceId", serviceId),
                                                                                            new SqlParameter("@TransactionDate", DateTime.UtcNow),
                                                                                            new SqlParameter("@IsCredit", Request.TransactionType),
                                                                                            new SqlParameter("@TxnId", Request.InvoiceNo)
                                                                                          ).FirstOrDefaultAsync();

                                    //emailuser
                                }
                                else
                                {
                                    return -1;
                                }
                            }
                            else
                            {
                                return -1;
                            }
                            //
                            if (objResponse.RstKey == 1)
                            {
                                return 1;
                            }
                            else
                            {
                                return -1;
                            }
                        }
                        else
                        {
                            return -1;
                        }

                    }
                    else
                    {
                        return -1;
                    }
                }
            }

            catch (Exception ex)
            {

                return -1;

            }



        }


        public async Task<int> UpdateFlagonwebhookflutter(string InvoiceNumber)
        {
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    var Data = db.webhookflutters.Where(x => x.JsonData == InvoiceNumber && x.flag == "0").FirstOrDefault();
                    if (Data != null)
                    {
                        Data.flag = "1";
                        db.Entry(Data).State = EntityState.Modified;
                        await db.SaveChangesAsync();
                    }
                    else
                    {
                        return -1;
                    }
                    return 1;
                }


            }
            catch (Exception ex)
            {
                return -1;
            }

        }



        public async Task<TransactionLogsResponse2> GenerateLogReportInfo()
        {
            var response = new TransactionLogsResponse2();
            var list = new List<TransactionLogslist2>();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    string[] obj = new string[] { "0", "0.00" }; //an array to check with


                    list = await (from x in db.ViewUserLists
                                  where !obj.Contains(x.Currentbalance) && x.WalletUserId != 141
                                
                                  select new TransactionLogslist2
                                  {

                                      WalletUserId = x.WalletUserId,
                                      FirstName = x.FirstName,
                                      LastName = x.LastName,
                                      EmailId = x.EmailId,
                                      StdCode = x.StdCode,
                                      MobileNo = x.MobileNo,
                                      IsActive = x.IsActive,
                                      CreatedDate = x.CreatedDate,
                                      UserType = x.UserType,
                                      Country = x.Country,
                                      Currentbalance = x.Currentbalance,
                                      DeviceType = x.DeviceType,
                                      IsDeleted = x.IsDeleted,
                                      DocumetStatus = x.DocumetStatus,
                                      IsEmailVerified = x.IsEmailVerified,
                                      IsOtpVerified = x.IsOtpVerified
                                  }).ToListAsync();
                    //list = await db.Database.SqlQuery<TransactionLogslist>("exec usp_NewFormate_TransactionLogs_Download @DateFrom,@DateTo",
                    // new SqlParameter("@DateFrom", request.DateFrom),
                    // new SqlParameter("@DateTo", request.DateTo)
                    // ).ToListAsync();

                    if (list != null && list.Count > 0)
                    {

                        response = new TransactionLogsResponse2
                        {
                            TotalCount = list.Count,
                            TransactionLogslist2 = list
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                //  ex.Message.ErrorLog("WalletTransactionRepository.cs", "ViewPaymentRequests");
            }
            return response;

        }
    }
}
