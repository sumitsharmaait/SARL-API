using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ezeePay.Utility.Enums;
using Ezipay.Database;
using Ezipay.Utility.common;
using Ezipay.ViewModel.AdminViewModel;

namespace Ezipay.Repository.AdminRepo.SubAdmin
{
    public class SubAdminRepository : ISubAdminRepository
    {
        public async Task<bool> DeleteSubadmin(UserDeleteRequest request)
        {
            bool result = false;
            try
            {
                using (var context = new DB_9ADF60_ewalletEntities())
                {
                    using (var tran = context.Database.BeginTransaction())
                    {
                        var res = await context.WalletUsers.Where(x => x.WalletUserId == request.UserId).FirstOrDefaultAsync();
                        var session = await context.SessionTokens.Where(x => x.WalletUserId == request.UserId).FirstOrDefaultAsync();
                        if (res != null)
                        {
                            res.IsDeleted = true;
                            int s = await context.SaveChangesAsync();
                            if (session != null)
                            {
                                context.SessionTokens.Remove(session);
                                int d = await context.SaveChangesAsync();
                            }
                            if (s > 0)
                            {
                                result = true;
                            }
                        }

                        tran.Commit();
                    }
                    //var res= await context.Database.SqlQuery<object>("exec usp_DeleteSubAdmin @WalletUserId",
                    //         new object[]
                    //         {
                    //         new SqlParameter("@WalletUserId",request.UserId)
                    //         }
                    //         ).FirstOrDefaultAsync();
                    // if (res!=null)
                    // {
                    //     result = true;
                    // }

                }
            }
            catch (Exception ex)
            {

            }
            return result;
        }

        public List<NavigationList> GetNavigationBySubAdmin(long subadminId)
        {
            var list = new List<NavigationList>();
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                list = context.Database.SqlQuery<NavigationList>
                                      ("EXEC usp_SubadminNavigation @SubadminId",
                                      new SqlParameter("@SubadminId", subadminId)
                                      ).ToList();

            }
            list.ForEach(x =>
            {
                if (!string.IsNullOrWhiteSpace(x.Functions))
                {
                    var data = x.Functions.Split(',').Select(a => Convert.ToInt64(a)).ToList();
                    data.ForEach(f =>
                    {
                        x.FunctionList.Add(
                            new ModuleFunctionModel
                            {
                                Id = f,
                                FunctionName = Enum.GetName(typeof(EnumModuleFunctionType), f)
                            });
                    });
                }
                // x.FunctionList = GetModuleFunction(x.Id, dbConnection);
            });
            return list;
        }

        public async Task<List<SubadminList>> GetSubAdminList(SubadminListRequest request)
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                return await context.Database.SqlQuery<SubadminList>
                        ("EXEC usp_SubadminList @SearchText,@PageNo,@PageSize",
                        new SqlParameter("@SearchText", request.SearchText),
                        new SqlParameter("@PageNo", request.PageNumber),
                        new SqlParameter("@PageSize", request.PageSize)
                        ).ToListAsync();
            }
        }

        public async Task<int> isEmailOrPhoneExist(long subadminId, string mobileNo, string emailId)
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                var usrByMobile = await context.WalletUsers.Where(x => x.MobileNo == mobileNo && x.WalletUserId != subadminId).FirstOrDefaultAsync();
                if (usrByMobile != null)
                {
                    return 1;
                }

                var usrByemail = await context.WalletUsers.Where(x => x.EmailId == emailId && x.WalletUserId != subadminId).FirstOrDefaultAsync();
                if (usrByemail != null)
                {
                    return 2;
                }
                return 0;
            }
        }

        //public async Task SaveSubAdmin(SubAdminRequest request)
        //{
        //    using (var context = new DB_9ADF60_ewalletEntities())
        //    {
        //        var adminKeyPair = AES256.AdminKeyPair;
        //        string Mobile = AES256.Encrypt(adminKeyPair.PublicKey, request.MobileNo);
        //        string EmailId = AES256.Encrypt(adminKeyPair.PublicKey, request.EmailId);

        //        var hashedObject = SHA256ALGO.HashPassword(request.Password);
        //        var userKeyPair = AES256.UserKeyPair();

        //        DataTable dtPermissions = new DataTable();
        //        dtPermissions.Columns.Add("UserId", typeof(Int64));
        //        dtPermissions.Columns.Add("NavigationId", typeof(Int64));
        //        dtPermissions.Columns.Add("IsActive", typeof(bool));
        //        dtPermissions.Columns.Add("Functions", typeof(string));             
        //        request.NavigationList.ForEach(nav =>
        //        {
        //            var fnList = nav.FunctionList.Select(f => f.Id).ToList();
        //            string fnStr = string.Join(",",fnList);
        //            dtPermissions.Rows.Add(request.SubadminId, nav.NavigationId, nav.NavigationForUser,fnStr);
        //        });
        //        var permissions = new SqlParameter("@Permissions", SqlDbType.Structured);
        //        permissions.Value = dtPermissions;
        //        permissions.TypeName = "dbo.Permission";
        //        context.Database.ExecuteSqlCommand
        //               ("EXEC usp_SaveSubadmin @UserType,@UserId,@IsdCode,@CurrencyId,@EmailId,@FirstName,@LastName,@Password,@HashedSalt,@MobileNo,@PrivateKey,@PublicKey,@Permissions",
        //               new SqlParameter("@UserType", (int)WalletUserTypes.Subadmin),
        //                 new SqlParameter("@UserId", request.SubadminId),
        //                 new SqlParameter("@IsdCode", request.IsdCode),
        //                 new SqlParameter("@CurrencyId", (int)CurrencyTypes.Ghanaian_Cedi),
        //                 new SqlParameter("@EmailId", EmailId),
        //                 new SqlParameter("@FirstName", AES256.Encrypt(userKeyPair.PublicKey, request.FirstName)),
        //                 new SqlParameter("@LastName", AES256.Encrypt(userKeyPair.PublicKey, request.LastName)),
        //                 new SqlParameter("@Password", hashedObject.HashedPassword),
        //                 new SqlParameter("@HashedSalt", hashedObject.SlatBytes),
        //                 new SqlParameter("@MobileNo", Mobile),
        //                 new SqlParameter("@PrivateKey", userKeyPair.PrivateKey),
        //                 new SqlParameter("@PublicKey", userKeyPair.PublicKey),
        //                permissions
        //               );
        //    }
        //}

        public async Task SaveSubAdmin(List<AdminPermission> request, WalletUser walletUser, long userId, AdminPasswordHistory adminPassword)
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                using (var tran = context.Database.BeginTransaction())
                {
                    try
                    {

                        if (userId == 0)
                        {
                            context.WalletUsers.Add(walletUser);
                            await context.SaveChangesAsync();

                            request.ForEach(x => x.UserId = walletUser.WalletUserId);
                            context.AdminPermissions.AddRange(request);
                            var r = await context.SaveChangesAsync();

                            //maintain admin password history
                            adminPassword.WalletUserId = walletUser.WalletUserId;
                            context.AdminPasswordHistories.Add(adminPassword);
                            await context.SaveChangesAsync();
                        }

                        if (userId > 0 && walletUser != null)
                        {
                            var result = context.AdminPermissions.Where(x => x.UserId == userId).ToList();
                            context.AdminPermissions.RemoveRange(result);
                            await context.SaveChangesAsync();

                            var _data = await context.WalletUsers.Where(x => x.WalletUserId == userId).FirstOrDefaultAsync();
                            _data.EmailId = walletUser.EmailId;
                            _data.FirstName = walletUser.FirstName;
                            _data.LastName = walletUser.LastName;
                            _data.PrivateKey = walletUser.PrivateKey;
                            _data.PublicKey = walletUser.PublicKey;
                            _data.MobileNo = walletUser.MobileNo;
                            _data.UpdatedDate = DateTime.UtcNow;
                            if (walletUser.HashedPassword != null && walletUser.HashedPassword != "")
                            {
                                _data.HashedPassword = walletUser.HashedPassword;
                                _data.HashedSalt = walletUser.HashedSalt;

                                adminPassword.WalletUserId = userId;
                                context.AdminPasswordHistories.Add(adminPassword);
                                await context.SaveChangesAsync();
                            }                                                   
                            context.Entry(_data).State = EntityState.Modified;
                            int s = await context.SaveChangesAsync();

                            if (s > 0)
                            {
                                context.AdminPermissions.AddRange(request);
                                var r = await context.SaveChangesAsync();
                            }

                        }
                        //else
                        //{
                        //    context.WalletUsers.Add(walletUser);
                        //    await context.SaveChangesAsync();

                        //}
                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                    }

                }
            }
        }
        //public async Task<bool> UpdateSubAdmin(long userId, WalletUser walletUser)
        //{
        //    bool res = false;
        //    using (var context = new DB_9ADF60_ewalletEntities())
        //    {
        //        using (var tran = context.Database.BeginTransaction())
        //        {

        //            var result = context.AdminPermissions.Where(x => x.UserId == userId).ToList();
        //            context.AdminPermissions.RemoveRange(result);
        //            var _walletUser = await context.WalletUsers.Where(x => x.WalletUserId == userId).FirstOrDefaultAsync();
        //            context.Entry(walletUser).State = EntityState.Modified;
        //            int s = await context.SaveChangesAsync();
        //            if (s > 0)
        //            {
        //                res = true;
        //            }
        //        }
        //    }
        //    return res;
        //}

        //public async Task<WalletUser> GetSubAdminById(long userId)
        //{
        //    var res = new WalletUser();
        //    using (var context = new DB_9ADF60_ewalletEntities())
        //    {
        //        using (var tran = context.Database.BeginTransaction())
        //        {

        //            res =await context.WalletUsers.Where(x => x.WalletUserId == userId).FirstOrDefaultAsync();

        //        }
        //    }
        //    return res;
        //}
    }
}
