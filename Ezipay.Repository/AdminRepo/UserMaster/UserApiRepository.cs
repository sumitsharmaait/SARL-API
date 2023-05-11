using Ezipay.Database;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.CheckPasswordViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Ezipay.Repository.AdminRepo
{
    public class UserApiRepository : IUserApiRepository
    {
        public async Task<UserListResponse> UserList(UserListRequest request)
        {
            var objResponse = new UserListResponse();
            var list = new List<UserList>();

            using (var db = new DB_9ADF60_ewalletEntities())
            {

                // list =await db.usp_UserList(request.SearchText, request.PageNumber, request.PageSize).ToListAsync();
                list = await db.Database.SqlQuery<UserList>
                                      ("EXEC usp_UserList @SearchText,@PageNo,@PageSize",
                                      new SqlParameter("@SearchText", request.SearchText),
                                      new SqlParameter("@PageNo", request.PageNumber),
                                      new SqlParameter("@PageSize", request.PageSize)
                                      ).ToListAsync();

                if (list != null && list.Count > 0)
                {
                    objResponse = new UserListResponse
                    {
                        TotalCount = list.FirstOrDefault().TotalCount,
                        UserList = list,

                    };
                }
                else
                {
                    objResponse.UserList = new List<UserList>();
                }
            }
            return objResponse;
        }

        public async Task<bool> EnableDisableUser(UserManageRequest request)
        {
            bool objResponse = false;

            using (var db = new DB_9ADF60_ewalletEntities())
            {
                var user = await db.WalletUsers.Where(x => x.WalletUserId == request.UserId).FirstOrDefaultAsync();
                if (user != null)
                {
                    if (!request.IsActive)
                    {
                        //var repo = new TokenRepository();
                        //repo.RemoveLoginSession(request.UserId);
                        //repo.SendLogoutPush(request.UserId);
                    }
                    user.IsActive = request.IsActive;
                    user.UpdatedDate = DateTime.UtcNow;
                    db.SaveChanges();


                    objResponse = true;
                }
                else
                {
                    objResponse = false;
                }
            }
            return objResponse;
        }

        public async Task<bool> Delete(UserManageRequest request)
        {
            bool objResponse = false;

            using (var db = new DB_9ADF60_ewalletEntities())
            {
                var result = db.usp_WalletUser_Delete(request.UserId);
                if (result != null)
                {
                    objResponse = true;
                }
            }
            return objResponse;
        }

        public async Task<UserEmailVerifyResponse> VerfiyByEmailId(string token)
        {

            var objResponse = new UserEmailVerifyResponse();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                long EmailVerificationId = Convert.ToInt64(token.Split('_')[1]);

                var user = await db.EmailVerifications.Where(x => x.EmailVerificationId == EmailVerificationId).AsQueryable().FirstOrDefaultAsync();
                if (user != null)
                {
                    var walletusr = db.WalletUsers.Where(x => x.EmailId == user.EmailId).AsQueryable().FirstOrDefault();
                    if (user != null && walletusr != null)
                    {
                        if (user.IsVerified == true)
                        {
                            objResponse.VerficationStatus = false;
                            //objResponse.VerficationMessage = ResponseMessages.ALREADY_VERIFIED;
                        }
                        else
                        {
                            user.IsVerified = true;
                            walletusr.IsEmailVerified = true;
                            user.VerificationDate = DateTime.UtcNow;
                            db.SaveChanges();

                            objResponse.VerficationStatus = true;
                            //objResponse.VerficationMessage = ResponseMessages.VERFIED_SUCCESSFULLY;
                        }
                    }
                }
                else
                {
                }
            }
            return objResponse;
        }

        public async Task<ViewUserTransactionResponse> UserTransactions(ViewUserTransactionRequest request)
        {
            var objResponse = new ViewUserTransactionResponse();
            var list = new List<TransactionDetail>();

            using (DB_9ADF60_ewalletEntities db = new DB_9ADF60_ewalletEntities())
            {

                if (request.DateFrom == DateTime.MinValue || request.DateFrom == null || request.DateTo == DateTime.MinValue || request.DateTo == null)
                {
                    list = await db.Database.SqlQuery<TransactionDetail>
                                     ("EXEC usp_PaymentTransactionsDetailWithDateRange @UserId,@TransactionType,@PageNo,@PageSize",
                                     new SqlParameter("@UserId", request.UserId),
                                     new SqlParameter("@TransactionType", request.TransactionType),
                                     new SqlParameter("@PageNo", request.PageNumber),
                                     new SqlParameter("@PageSize", request.PageSize)

                                     ).ToListAsync();
                }
                else
                {
                    list = await db.Database.SqlQuery<TransactionDetail>
                                     ("EXEC usp_PaymentTransactionsDetailWithDateRange @UserId,@TransactionType,@PageNo,@PageSize,@DateFrom,@DateTo",
                                     new SqlParameter("@UserId", request.UserId),
                                     new SqlParameter("@TransactionType", request.TransactionType),
                                     new SqlParameter("@PageNo", request.PageNumber),
                                     new SqlParameter("@PageSize", request.PageSize),
                                     new SqlParameter("@DateFrom", request.DateFrom),
                                     new SqlParameter("@DateTo", request.DateTo)
                                     ).ToListAsync();
                    objResponse = new ViewUserTransactionResponse
                    {

                        DateFrom = request.DateFrom.Value.ToString("yyyy-MM-dd"),
                        DateTo = request.DateTo.Value.ToString("yyyy-MM-dd")

                    };
                }


                if (list != null && list.Count > 0)
                {
                    objResponse.TransactionList = list;
                    objResponse.TotalCount = list.FirstOrDefault().TotalCount;
                }
                else
                {
                    objResponse.TransactionList = new List<TransactionDetail>();
                }
            }
            return objResponse;
        }

        public async Task<CreditDebitResponse> CreditDebitUserAccount(CreditDebitRequest request, int WalletTransactionSubTypesId, long AdminUserType)
        {
            var objResponse = new CreditDebitResponse();

            using (var db = new DB_9ADF60_ewalletEntities())
            {
                int serviceId = (int)db.WalletServices.Where(x => x.ServiceCategoryId == WalletTransactionSubTypesId).Select(x => x.WalletServiceId).FirstOrDefault();
                if (serviceId > 0)
                {
                    var adminUser = db.WalletUsers.Where(x => x.UserType == AdminUserType).FirstOrDefault();
                    if (adminUser != null)
                    {
                        if (WalletTransactionSubTypesId == 4)
                        {

                            if (request.TxnId == null)
                            {
                                request.TxnId = "0";
                            }

                            objResponse = await db.Database.SqlQuery<CreditDebitResponse>
                                          ("EXEC usp_CreditDebitUser @SenderId,@ReceiverId,@TransactionAmount,@Reason,@ServiceId,@TransactionDate,@IsCredit,@TxnId",
                                          new SqlParameter("@SenderId", request.TransactionType ? adminUser.WalletUserId : request.UserId),
                                            new SqlParameter("@ReceiverId", request.TransactionType ? request.UserId : adminUser.WalletUserId),
                                            new SqlParameter("@TransactionAmount", request.Amount),
                                            new SqlParameter("@Reason", request.Reason),
                                            new SqlParameter("@ServiceId", serviceId),
                                            new SqlParameter("@TransactionDate", DateTime.UtcNow),
                                            new SqlParameter("@IsCredit", request.TransactionType),
                                            new SqlParameter("@TxnId", request.TxnId)
                                          ).FirstOrDefaultAsync();
                        }
                        else
                        {
                            objResponse = await db.Database.SqlQuery<CreditDebitResponse>
                                              ("EXEC usp_CreditDebitUser @SenderId,@ReceiverId,@TransactionAmount,@Reason,@ServiceId,@TransactionDate,@IsCredit",
                                              new SqlParameter("@SenderId", request.TransactionType ? adminUser.WalletUserId : request.UserId),
                                                new SqlParameter("@ReceiverId", request.TransactionType ? request.UserId : adminUser.WalletUserId),
                                                new SqlParameter("@TransactionAmount", request.Amount),
                                                new SqlParameter("@Reason", request.Reason),
                                                new SqlParameter("@ServiceId", serviceId),
                                                new SqlParameter("@TransactionDate", DateTime.UtcNow),
                                                new SqlParameter("@IsCredit", request.TransactionType)

                                              ).FirstOrDefaultAsync();
                        }

                    }
                }
            }
            return objResponse;
        }

        public async Task<bool> ManageTransaction(SetTransactionLimitRequest request)
        {
            bool response = false;

            using (var db = new DB_9ADF60_ewalletEntities())
            {
                if (request.UserId != null && Convert.ToInt64(request.UserId) > 0)
                {
                    if (request.Type == 1)
                    {
                        var data = await db.SetTransactionLimits.Where(x => x.UserId == request.UserId).OrderByDescending(x => x.Id).FirstOrDefaultAsync();
                        if (data != null)
                        {
                            data.TransactionLimit = request.TransactionLimit;
                            db.Entry(data).State = EntityState.Modified;
                            await db.SaveChangesAsync();

                            response = true;
                        }
                        else
                        {
                            var req = new SetTransactionLimit
                            {
                                UserId = request.UserId,
                                CreatedOn = DateTime.UtcNow,
                                TransactionLimit = request.TransactionLimit,
                            };
                            db.SetTransactionLimits.Add(req);
                            await db.SaveChangesAsync();
                            response = true;
                        }
                        //var result =await db.Database.SqlQuery<int>("exec usp_SetTransactionLimit @UserId,@TransactionLimit",
                        //                        new object[]
                        //                        {
                        //    new SqlParameter("@UserId",request.UserId),
                        //    new SqlParameter("@TransactionLimit", request.TransactionLimit)
                        //                        }).FirstOrDefaultAsync();
                        //if (result != null)
                        //{
                        //    response = true;
                        //}
                    }
                    else
                    {
                        var data = await db.SetTransactionLimits.Where(x => x.UserId == request.UserId).OrderByDescending(x => x.Id).FirstOrDefaultAsync();
                        //var data = db.SetTransactionLimits.Where(x => x.UserId == request.UserId).LastOrDefault();
                        if (data != null)
                        {
                            data.TransactionLimitForAddMoney = request.TransactionLimit;
                            db.Entry(data).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                            response = true;
                        }
                        else
                        {
                            var req = new SetTransactionLimit
                            {
                                UserId = request.UserId,
                                CreatedOnForAddMoney = DateTime.UtcNow,
                                TransactionLimitForAddMoney = request.TransactionLimit,
                            };
                            db.SetTransactionLimits.Add(req);
                            await db.SaveChangesAsync();
                            response = true;
                        }
                        //var result = await db.Database.SqlQuery<Object>("exec usp_SetAddMoneyLimit @UserId,@TransactionLimit",
                        //new object[]
                        //{
                        //    new SqlParameter("@UserId",request.UserId),
                        //    new SqlParameter("@TransactionLimit", request.TransactionLimit)
                        //}).FirstOrDefaultAsync();
                        //if (result != null)
                        //{
                        //    response = true;
                        //}
                    }

                }
                else
                {
                    response = false;
                }
                return response;
            }
        }

        public async Task<UserTransactionLimitDetailsResponse> GetTransactionLimitDetails(UserDetailsRequest request)
        {
            var response = new UserTransactionLimitDetailsResponse();

            using (var db = new DB_9ADF60_ewalletEntities())
            {
                response = await db.Database.SqlQuery<UserTransactionLimitDetailsResponse>
                                      ("EXEC usp_GetTransactionLimitByUserId @UserId",
                                      new SqlParameter("@UserId", request.UserId)
                                      ).FirstOrDefaultAsync();

            }
            return response;
        }

        public async Task<UserDetailsResponse> UserDetails(UserDetailsRequest request)
        {
            var response = new UserDetailsResponse();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                response = await db.Database.SqlQuery<UserDetailsResponse>
                                      ("EXEC usp_UserDetailById @UserId",
                                      new SqlParameter("@UserId", request.UserId)
                                      ).FirstOrDefaultAsync();

            }
            return response;
        }

        public async Task<DownloadReportResponse> DownloadReportWithData(DownloadReportApiRequest request)
        {
            var response = new DownloadReportResponse();
            var list = new List<ReportData>();
            list.Add(new ReportData());

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

            return response;
        }

        public async Task<List<UserList>> DeletedUserList(UserListRequest request)
        {
            // var objResponse = new UserListResponse();
            var list = new List<UserList>();

            using (var db = new DB_9ADF60_ewalletEntities())
            {
                list = await db.Database.SqlQuery<UserList>
                                      ("EXEC usp_DeletedUserList @SearchText,@PageNo,@PageSize",
                                      new SqlParameter("@SearchText", request.SearchText),
                                      new SqlParameter("@PageNo", request.PageNumber),
                                      new SqlParameter("@PageSize", request.PageSize)
                                      ).ToListAsync();

                //if (list != null && list.Count > 0)
                //{
                //    objResponse = new UserListResponse
                //    {
                //        TotalCount = list.FirstOrDefault().TotalCount,
                //        UserList = list,

                //    };
                //}
                //else
                //{
                //    objResponse.UserList = new List<UserList>();
                //}
            }
            return list;
        }

        public async Task<WalletUser> GetUserById(long userId)
        {
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                return await db.WalletUsers.Where(x => x.WalletUserId == userId).FirstOrDefaultAsync();
            }
        }

        public async Task<UserDocument> GetUserDocumentByUserId(long userId)
        {
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                return await db.UserDocuments.Where(x => x.WalletUserId == userId).FirstOrDefaultAsync();
            }
        }

        public async Task<int> InsertDocument(UserDocument entity)
        {
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                db.UserDocuments.Add(entity);
                return await db.SaveChangesAsync();
            }
        }

        public async Task<int> UpdateUser(WalletUser walletUser)
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                context.Entry(walletUser).State = EntityState.Modified;
                return await context.SaveChangesAsync();
            }
        }

        public async Task<int> UpdateDocument(UserDocument usrDoc)
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                context.Entry(usrDoc).State = EntityState.Modified;
                return await context.SaveChangesAsync();
            }
        }

        public async Task<UserDocumentDetailsResponse> GetUserDocuments(long userId)
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                return await context.Database.SqlQuery<UserDocumentDetailsResponse>
                                   ("EXEC usp_GetDocumentDetails @WalletUserId",
                                   new SqlParameter("@WalletUserId", userId)
                                   ).FirstOrDefaultAsync();

            }
        }

        public async Task<bool> CheckEmail(string emailId, string mobile)
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                var user = await context.WalletUsers.Where(x => x.EmailId == emailId).FirstOrDefaultAsync();
                if (user != null)
                {
                    return true;
                }
                return false;
            }
        }

        public async Task<List<UserList>> PendingKycUserList(UserListRequest request)
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                return await context.Database.SqlQuery<UserList>
                       ("EXEC usp_PendingKycUserList @SearchText,@PageNo,@PageSize",
                       new SqlParameter("@SearchText", request.SearchText),
                       new SqlParameter("@PageNo", request.PageNumber),
                       new SqlParameter("@PageSize", request.PageSize)
                       ).ToListAsync();
            }
        }

        public async Task<UserListResponse> GenerateUserList(DownloadLogReportRequest request)
        {
            UserListResponse objResponse = new UserListResponse();
            List<UserList> list = new List<UserList>();

            using (DB_9ADF60_ewalletEntities db = new DB_9ADF60_ewalletEntities())
            {
                list = await db.Database.SqlQuery<UserList>
                                      ("EXEC usp_UserListDownload  @DateFrom,@DateTo",
                                       new SqlParameter("@DateFrom", request.DateFrom),
                                       new SqlParameter("@DateTo", request.DateTo)
                                      ).ToListAsync();

                if (list != null && list.Count > 0)
                {
                    objResponse = new UserListResponse
                    {
                        TotalCount = list.FirstOrDefault().TotalCount,
                        UserList = list,

                    };
                }
                else
                {
                    objResponse.UserList = new List<UserList>();
                }
            }
            return objResponse;
        }

        public async Task<List<DocModel>> GetMerchantDocuments(long walletUserId)
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                return await (from d in context.MerchantDocuments
                              where d.WalletUserId == walletUserId && d.IsActive == true && d.IsDeleted == false
                              select new DocModel
                              {
                                  DocName = d.DocImage,
                                  DocType = d.DocType
                              }).ToListAsync();
            }
        }


        public async Task<List<DuplicateCardNoVMResponse>> GetduplicatecardnoList(string Cardno, long Walletuserid)
        {
            var list = new List<DuplicateCardNoVMResponse>();

            using (var context = new DB_9ADF60_ewalletEntities())
            {
                //first take current user id submitted
                list = await (from d in context.CardNoDuplicates
                              join x in context.ViewUserLists
                                on d.CreatedBy equals x.WalletUserId.ToString()
                              where d.CardNo == Cardno.Trim()
                              select new DuplicateCardNoVMResponse
                              {
                                  Id = d.Id,
                                  WalletUserId = d.WalletUserId,
                                  CardNo = d.CardNo,
                                  EmailId = d.EmailId,
                                  Mobile_No = d.Mobile_No,
                                  CurrentBalance = d.CurrentBalance,
                                  // CreatedBy = (from x in context.ViewUserLists where x.WalletUserId == SqlFunctions.IsNumeric(d.CreatedBy) select (x.FirstName + " " + x.LastName)).FirstOrDefault(),
                                  CreatedBy = (x.FirstName + " " + x.LastName),
                                  UserCreatedDate = d.UserCreatedDate

                              }).Union(
                            from e in context.CardNoDuplicates
                            join x1 in context.ViewUserLists
                            on e.CreatedBy equals x1.WalletUserId.ToString()
                            where e.WalletUserId == Walletuserid
                            select new DuplicateCardNoVMResponse
                            {
                                Id = e.Id,
                                WalletUserId = e.WalletUserId,
                                CardNo = e.CardNo,
                                EmailId = e.EmailId,
                                Mobile_No = e.Mobile_No,
                                CurrentBalance = e.CurrentBalance,
                                CreatedBy = (x1.FirstName + " " + x1.LastName),
                                // CreatedBy = (from x1 in context.ViewUserLists where SqlFunctions.StringConvert(x1.WalletUserId) == e.CreatedBy select (x1.FirstName + " " + x1.LastName)).FirstOrDefault(),
                                UserCreatedDate = e.UserCreatedDate

                            }).ToListAsync();


                return list;
                // return null;
            }
        }

        public async Task<int> Insertduplicatecardno(DuplicateCardNoVMRequest request)
        {
            if (request.flag == "delete")
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    var isExist = db.CardNoDuplicates.Where(x => x.WalletUserId == request.Walletuserid && x.CardNo == request.Cardno).FirstOrDefault();
                    db.CardNoDuplicates.Remove(isExist);
                    return await db.SaveChangesAsync();

                }

            }
            else
            {
                using (var context = new DB_9ADF60_ewalletEntities())
                {
                    //take first data user
                    var user = await context.ViewUserLists.Where(x => x.WalletUserId == request.Walletuserid).FirstOrDefaultAsync();
                    if (user != null)
                    {
                        var usercount = await context.CardNoDuplicates.Where(x => x.WalletUserId == request.Walletuserid && x.CardNo == request.Cardno).CountAsync();
                        if (usercount == 0)
                        {

                            var entity = new CardNoDuplicate
                            {
                                WalletUserId = request.Walletuserid,
                                CardNo = request.Cardno,
                                EmailId = user.EmailId,
                                Mobile_No = user.MobileNo,
                                CurrentBalance = user.Currentbalance,
                                UserCreatedDate = user.CreatedDate,
                                CreatedDate = DateTime.UtcNow,
                                CreatedBy = request.CreatedBy
                            };
                            context.CardNoDuplicates.Add(entity);
                            await context.SaveChangesAsync();

                            return 1;
                        }
                        return 0;
                    }
                    return 0;
                }
            }
        }

        public async Task<int> SaveBlockUnblockDetails(UserManageRequest request)
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                //take first data user
                if (request.IsActive == true) //unblock by
                {
                    var user = await context.ViewUserLists.Where(x => x.WalletUserId == request.AdminId).FirstOrDefaultAsync();
                    if (user != null)
                    {

                        var entity = new UserBlockUnblockDetail
                        {
                            Walletuserid = request.UserId,
                            BlockUnblock = request.IsActive,
                            UnBlockByEmailid = user.EmailId,
                            Comment = request.Comment,
                            UnBlockdate = DateTime.UtcNow


                        };
                        context.UserBlockUnblockDetails.Add(entity);
                        await context.SaveChangesAsync();

                        return 1;

                    }
                }
                else if (request.IsActive == false) //block by
                {
                    var user = await context.ViewUserLists.Where(x => x.WalletUserId == request.AdminId).FirstOrDefaultAsync();
                    if (user != null)
                    {

                        var entity = new UserBlockUnblockDetail

                        {
                            Walletuserid = request.UserId,
                            BlockUnblock = request.IsActive,
                            BlockByEmailid = user.EmailId,
                            Comment = request.Comment,
                            Blockdate = DateTime.UtcNow

                        };

                        context.UserBlockUnblockDetails.Add(entity);
                        await context.SaveChangesAsync();

                        return 1;

                    }
                    else //for web
                    {
                        var user1 = await context.ViewUserLists.Where(x => x.WalletUserId == request.UserId).FirstOrDefaultAsync();
                        if (user1 != null)
                        {

                            var entity = new UserBlockUnblockDetail
                            {
                                Walletuserid = request.UserId,
                                BlockUnblock = request.IsActive,
                                BlockByEmailid = "ChargeBack_request",
                                Comment = request.Comment,
                                Blockdate = DateTime.UtcNow
                            };
                            context.UserBlockUnblockDetails.Add(entity);
                            await context.SaveChangesAsync();

                            return 1;

                        }

                    }

                }
                return 0;
            }

        }

        //public async Task<List<UserBlockUnblockDetailResponse>> EnableDisableUserList(UserListRequest request)
        //{
        //    var list = new List<UserBlockUnblockDetailResponse>();

        //    using (var context = new DB_9ADF60_ewalletEntities())
        //    {
        //        //first take current user id submitted
        //        list = await (from d in context.UserBlockUnblockDetails
        //                      join x in context.ViewUserLists
        //                        on d.Walletuserid equals x.WalletUserId
        //                      where d.BlockUnblock == false && d.BlockByEmailid != "ChargeBack_request"
        //                      orderby d.Blockdate descending
        //                      select new UserBlockUnblockDetailResponse
        //                      {
        //                          Walletuserid = d.Walletuserid,
        //                          EmailId = x.EmailId,
        //                          BlockByEmailid = d.BlockByEmailid,
        //                          Blockstatus = d.BlockUnblock,
        //                          Blockdate = d.Blockdate,
        //                          Comment = d.Comment

        //                      }).ToListAsync();


        //        return list;
        //        // return null;
        //    }
        //}




        public async Task<UserBlockUnblockDetailResponse> EnableDisableUserList(UserListRequest request)
        {
            var objResponse = new UserBlockUnblockDetailResponse();
            var list = new List<UserBlockUnblockDetail1>();

            using (var db = new DB_9ADF60_ewalletEntities())
            {

                // list =await db.usp_UserList(request.SearchText, request.PageNumber, request.PageSize).ToListAsync();
                list = await db.Database.SqlQuery<UserBlockUnblockDetail1>
                                      ("EXEC usp_UserBlockUnblockDetailList @SearchText,@PageNo,@PageSize",
                                      new SqlParameter("@SearchText", request.SearchText),
                                      new SqlParameter("@PageNo", request.PageNumber),
                                      new SqlParameter("@PageSize", request.PageSize)
                                      ).ToListAsync();

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
