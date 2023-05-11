using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ezeePay.Utility.Enums;
using Ezipay.Database;
using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.CheckPasswordViewModel;

namespace Ezipay.Repository.AdminRepo.Merchant
{
    public class MerchantRepository : IMerchantRepository
    {
        public async Task<bool> DeleteSubadmin(MarchantDeleteRequest request)
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                return await context.Database.SqlQuery<bool>("exec usp_DeleteMarchant @WalletUserId",
                        new object[]
                        {
                            new SqlParameter("@WalletUserId",request.UserId)
                        }
                        ).FirstOrDefaultAsync();
            }
        }

        public async Task<List<MerchantCommisionMaster>> GetCommissionByServiceId(int walletServiceId)
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                return await context.MerchantCommisionMasters.Where(y => y.WalletServiceId == walletServiceId).ToListAsync();
            }
        }

        public async Task<List<MerchantList>> GetMerchantList(MerchantListRequest request)
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                return await context.Database.SqlQuery<MerchantList>
                        ("EXEC usp_MerchantList @SearchText,@PageNo,@PageSize,@type",
                        new SqlParameter("@SearchText", request.SearchText),
                        new SqlParameter("@PageNo", request.PageNumber),
                        new SqlParameter("@PageSize", request.PageSize),
                        new SqlParameter("@type", request.Type)
                        ).ToListAsync();
            }
        }

        public async Task<WalletService> GetWalletServiceByUserId(long walletUserId)
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                return await context.WalletServices.Where(y => y.MerchantId == walletUserId).FirstOrDefaultAsync();
            }
        }

        public async Task<int> InsertMerchant(WalletUser walletUser, WalletService walletService, MerchantCommisionMaster commisionMaster)
        {
            int statusCode = 0;
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                using (var tran = context.Database.BeginTransaction())
                {
                    try
                    {
                        context.WalletUsers.Add(walletUser);
                        await context.SaveChangesAsync();

                        walletService.MerchantId = walletUser.WalletUserId;
                        context.WalletServices.Add(walletService);
                        await context.SaveChangesAsync();

                        commisionMaster.WalletServiceId = walletService.WalletServiceId;
                        context.MerchantCommisionMasters.Add(commisionMaster);
                        await context.SaveChangesAsync();
                        tran.Commit();
                        statusCode = 1;
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        statusCode = 0;
                    }
                }
            }
            return statusCode;
        }

        public async Task<int> UpdateMerchant(WalletUser walletUser, WalletService walletService, MerchantCommisionMaster objCommission, List<MerchantCommisionMaster> commisionList)
        {
            int statusCode = 0;
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                using (var tran = context.Database.BeginTransaction())
                {
                    try
                    {
                        context.Entry(walletUser).State = EntityState.Modified;
                        await context.SaveChangesAsync();

                        if (walletService != null)
                        {
                            context.Entry(walletService).State = EntityState.Modified;
                            await context.SaveChangesAsync();
                        }

                        if (commisionList.Count > 0)
                        {
                            commisionList.ForEach(x =>
                            {
                                context.Entry(x).State = EntityState.Modified;
                                context.SaveChanges();
                            });
                        }

                        if (objCommission != null)
                        {
                            context.MerchantCommisionMasters.Add(objCommission);
                            await context.SaveChangesAsync();
                        }
                        tran.Commit();
                        statusCode = 1;
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        statusCode = 0;
                    }
                }
            }
            return statusCode;
        }

        public async Task<List<TransactionDetails>> ViewMerchantTransactions(ViewMarchantTransactionRequest request)
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                if (request.DateFrom == DateTime.MinValue || request.DateFrom == null || request.DateTo == DateTime.MinValue || request.DateTo == null)
                {
                    return await context.Database.SqlQuery<TransactionDetails>
                                     ("EXEC usp_MarchantPaymentTransactionsDetailWithDateRange @UserId,@TransactionType,@PageNo,@PageSize",
                                     new SqlParameter("@UserId", request.UserId),
                                     new SqlParameter("@TransactionType", request.TransactionType),
                                     new SqlParameter("@PageNo", request.PageNumber),
                                     new SqlParameter("@PageSize", request.PageSize)

                                     ).ToListAsync();
                }
                else
                {
                    return await context.Database.SqlQuery<TransactionDetails>
                                     ("EXEC usp_MarchantPaymentTransactionsDetailWithDateRange @UserId,@TransactionType,@PageNo,@PageSize,@DateFrom,@DateTo",
                                     new SqlParameter("@UserId", request.UserId),
                                     new SqlParameter("@TransactionType", request.TransactionType),
                                     new SqlParameter("@PageNo", request.PageNumber),
                                     new SqlParameter("@PageSize", request.PageSize),
                                     new SqlParameter("@DateFrom", request.DateFrom),
                                     new SqlParameter("@DateTo", request.DateTo)
                                     ).ToListAsync();
                }
            }
        }

        public async Task<TransactionLogsResponce> GenerateLogReport(DownloadLogReportRequest request)
        {
            TransactionLogsResponce response = new TransactionLogsResponce();
            List<TransactionLogslist> list = new List<TransactionLogslist>();
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

        public async Task<MerchantListResponse> DownLoadMerchantLogList(DownloadLogReportRequest request)
        {
            MerchantListResponse objResponse = new MerchantListResponse();
            List<MerchantList> list = new List<MerchantList>();

            using (DB_9ADF60_ewalletEntities db = new DB_9ADF60_ewalletEntities())
            {
                list = await db.Database.SqlQuery<MerchantList>
                                      ("EXEC usp_DownloadMerchantList  @DateFrom,@DateTo",
                                      new SqlParameter("@DateFrom", request.DateFrom),
                                      new SqlParameter("@DateTo", request.DateTo)
                                      ).ToListAsync();

                if (list != null && list.Count > 0)
                {
                    objResponse = new MerchantListResponse
                    {
                        TotalCount = list.FirstOrDefault().TotalCount,
                        MerchantList = list
                    };
                }
                else
                {
                    objResponse.MerchantList = new List<MerchantList>();
                }
            }
            return objResponse;
        }

        public async Task<int> OnBoardRequest(WalletUser walletUser, UserDocument docEntity, List<MerchantDocument> docs, BankDetail bankEntity, WalletService walletService, MerchantCommisionMaster commisionMaster, UserApiKey userApiKey)
        {
            int statusCode = 0;
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                using (var tran = context.Database.BeginTransaction())
                {
                    try
                    {
                        context.WalletUsers.Add(walletUser);
                        await context.SaveChangesAsync();

                        docEntity.WalletUserId = walletUser.WalletUserId;
                        context.UserDocuments.Add(docEntity);

                        bankEntity.WalletUserId = walletUser.WalletUserId;
                        context.BankDetails.Add(bankEntity);

                        walletService.MerchantId = walletUser.WalletUserId;
                        context.WalletServices.Add(walletService);
                        await context.SaveChangesAsync();

                        commisionMaster.WalletServiceId = walletService.WalletServiceId;
                        context.MerchantCommisionMasters.Add(commisionMaster);
                        await context.SaveChangesAsync();

                        if (docs.Count > 0)
                        {
                            docs.ForEach(x => x.WalletUserId = walletUser.WalletUserId);
                            context.MerchantDocuments.AddRange(docs);

                        }

                        userApiKey.WalletUserId = walletUser.WalletUserId;
                        context.UserApiKeys.Add(userApiKey);

                        await context.SaveChangesAsync();

                        tran.Commit();
                        statusCode = 1;
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        statusCode = 0;
                    }
                }
            }
            return statusCode;
        }

        public async Task<int> InsertStore(MerchantStore storeEntity)
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                context.MerchantStores.Add(storeEntity);
                return await context.SaveChangesAsync();
            }
        }

        public async Task<int> UpdateStore(MerchantStore storeEntity)
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                context.Entry(storeEntity).State = EntityState.Modified;
                return await context.SaveChangesAsync();
            }
        }

        public async Task<MerchantStore> GetStoreById(long storeId)
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                return await context.MerchantStores.Where(x => x.Id == storeId).FirstOrDefaultAsync();
            }
        }

        public async Task<List<StoreResponse>> GetStores(StoreSearchRequest request)
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                return await (from d in context.MerchantStores
                              where d.WalletUserId == request.WalletUserId && d.IsDeleted == false
                              select new StoreResponse
                              {
                                  WalletUserId = d.WalletUserId,
                                  Location = d.Location,
                                  QrCode = CommonSetting.imageUrl + d.QrCode,
                                  StoreId = d.Id,
                                  StoreName = d.StoreName,
                                  IsActive = d.IsActive,
                                  CreatedOn = d.CreatedDate
                              }).OrderByDescending(x=>x.StoreId).ToListAsync();
            }
        }
    }
}
