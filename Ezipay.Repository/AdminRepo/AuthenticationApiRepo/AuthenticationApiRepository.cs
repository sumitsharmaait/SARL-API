using ezeePay.Utility.Enums;
using Ezipay.Database;
using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.ViewModel.AdminViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;

namespace Ezipay.Repository.AdminRepo.AuthenticationApiRepo
{
    public class AuthenticationApiRepository : IAuthenticationApiRepository
    {

        /// <summary>
        /// Admin Login using email and password
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<WalletUser> Login(LoginRequest request)
        {
            var response = new WalletUser();

            using (var db = new DB_9ADF60_ewalletEntities())
            {
                var admin = await db.WalletUsers.Where(x => x.EmailId == request.EmailId && (x.UserType == (int)WalletUserTypes.AdminUser || x.UserType == (int)WalletUserTypes.Subadmin)).FirstOrDefaultAsync();

                if (admin != null && admin.WalletUserId > 0 && admin.IsDeleted == false)
                {
                    if (admin.IsActive == true)
                    {
                        response = admin;
                    }
                }
            }
            return response;
        }

        /// <summary>
        /// Change password for admin using old password      
        /// </summary>
        /// <param name="request"></param>
        /// <returns>bool</returns>
        public async Task<ChangePasswordResponse> ChangePassword(ChangePasswordRequest request, string TokenValue)
        {
            var response = new ChangePasswordResponse();
            try
            {
                var adminKeyPair = AES256.AdminKeyPair;
                //HttpRequestMessage httpRequestMessage = HttpContext.Current.Items["MS_HttpRequestMessage"] as HttpRequestMessage;               
                if (!string.IsNullOrEmpty(TokenValue))
                {
                    using (var db = new DB_9ADF60_ewalletEntities())
                    {
                        long WalletUserId = (long)await db.SessionTokens.Where(x => x.TokenValue == TokenValue).Select(x => x.WalletUserId).FirstOrDefaultAsync();


                        var user = db.WalletUsers.Where(x => x.WalletUserId == WalletUserId).FirstOrDefault();
                        //Matched with Email
                        if (user != null)
                        {
                            var hashedObject = SHA256ALGO.HashPasswordDecryption(request.CurrentPassword, user.HashedSalt);
                            var IsPasswordExist = SHA256ALGO.HashPassword(request.NewPassword);
                            var passwords = await GetPasswords(WalletUserId, IsPasswordExist.HashedPassword);
                            if (passwords == false) //NEW PWD
                            {
                                if (hashedObject.HashedPassword == user.HashedPassword)
                                {
                                    var hashedObjectNew = SHA256ALGO.HashPassword(request.NewPassword);
                                    user.HashedPassword = hashedObjectNew.HashedPassword;
                                    user.HashedSalt = hashedObjectNew.SlatBytes;
                                    user.UpdatedDate = DateTime.UtcNow;
                                    if (user.IsTemporaryPassword == true)
                                    {
                                        user.IsTemporaryPassword = false;
                                        // user.OtpHashedSalt = null;
                                        //  user.Otp = string.Empty;
                                    }

                                    int s = db.SaveChanges();
                                    if (s > 0)
                                    {
                                        var reqadminpasswordhistory = new AdminPasswordHistory
                                        {
                                            WalletUserId = user.WalletUserId,
                                            Password = user.HashedPassword,
                                            Hashed = user.HashedSalt,
                                            CreatedDate = DateTime.UtcNow,
                                            UpdatedDate = DateTime.UtcNow
                                        };

                                        db.AdminPasswordHistories.Add(reqadminpasswordhistory);
                                        db.SaveChanges();
                                        //db.Entry(reqadminpasswordhistory).State = EntityState.Modified;
                                        //int s = await db.SaveChangesAsync();

                                    }
                                    //response.IsSuccess = true;
                                    //response.Message = "Password changed successfully.";
                                    response.RstKey = 1;
                                }
                                else
                                {

                                    if (!string.IsNullOrEmpty(user.Otp) && user.OtpHashedSalt != null)
                                    {
                                        hashedObject = SHA256ALGO.HashPasswordDecryption(request.CurrentPassword, user.OtpHashedSalt);
                                        if (hashedObject.HashedPassword == user.Otp)
                                        {

                                            var hashedObjectNew = SHA256ALGO.HashPassword(request.NewPassword);
                                            user.HashedPassword = hashedObjectNew.HashedPassword;
                                            user.HashedSalt = hashedObjectNew.SlatBytes;
                                            user.UpdatedDate = DateTime.UtcNow;
                                            if (user.IsTemporaryPassword == true)
                                            {
                                                user.IsTemporaryPassword = false;
                                                user.OtpHashedSalt = null;
                                                user.Otp = string.Empty;
                                            }
                                            int s = db.SaveChanges();
                                            if (s > 0)
                                            {
                                                var reqadminpasswordhistory = new AdminPasswordHistory
                                                {
                                                    WalletUserId = user.WalletUserId,
                                                    Password = user.HashedPassword,
                                                    Hashed = user.HashedSalt,
                                                    CreatedDate = DateTime.UtcNow,
                                                    UpdatedDate = DateTime.UtcNow
                                                };

                                                db.AdminPasswordHistories.Add(reqadminpasswordhistory);
                                                db.SaveChanges();
                                                //db.Entry(reqadminpasswordhistory).State = EntityState.Modified;
                                                //int s = await db.SaveChangesAsync();

                                            }
                                            //response.IsSuccess = true;
                                            //response.Message = "Password changed successfully.";
                                            response.RstKey = 1;
                                            //new TokenRepository().SendLogoutPush(WalletUserId);

                                        }
                                        else
                                        {
                                            response.RstKey = 2;
                                            //response.Message = "Please enter correct old password.";
                                        }
                                    }
                                    else
                                    {
                                        // response.Message = "Please enter correct old password.";
                                        response.RstKey = 2;
                                    }
                                }
                            }
                            else
                            {
                                response.RstKey = 3;
                            }
                        }
                        else
                        {
                            response.RstKey = 3;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("UserProfileRepository.cs", "ChangePassword", request);
            }
            return response;

        }


        /// <summary>
        /// Get navigation list for logged in user 
        /// </summary>
        /// <returns></returns>
        public async Task<NavigationResponse> NavigationList(NavigationsRequest request)
        {
            NavigationResponse objResponse = new NavigationResponse();

            var adminKeyPair = AES256.AdminKeyPair;
            try { 
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                HttpCookie authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];

                //if (authCookie != null && !string.IsNullOrEmpty(authCookie.Value))
                //{
                //    FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);
                //    LoginResponse model = JsonConvert.DeserializeObject<LoginResponse>(ticket.UserData);

                    var admin = db.WalletUsers.Where(x => x.WalletUserId == request.AdminId).AsQueryable().FirstOrDefault();
                    if (admin != null && admin.WalletUserId > 0)
                    {
                        var result = new List<Navigations>();
                        long UserId = admin.WalletUserId;

                        result = await db.Database.SqlQuery<Navigations>("EXEC usp_UserNavigation @UserId,@UserType",
                             new SqlParameter("@UserId", admin.WalletUserId),
                             new SqlParameter("@UserType", admin.UserType)

                                ).ToListAsync();
                        objResponse.NavigationList = result;
                        result.ForEach(x =>
                        {
                            if (!string.IsNullOrWhiteSpace(x.Functions))
                            {
                                var list = x.Functions.Split(',').Select(a => Convert.ToInt64(a)).ToList();
                                list.ForEach(f =>
                                {
                                    x.FunctionList.Add(
                                        new ModuleFunctionModel
                                        {
                                            Id = f,
                                            FunctionName = Enum.GetName(typeof(EnumModuleFunctionType), f)
                                        });
                                });
                            }
                        });
                    }
                //}
            }
            }
            catch (Exception ex)
            {

            }
            return objResponse;
        }

        /// <summary>
        /// Get crrent user detail 
        /// </summary>
        /// <returns></returns>
        public async Task<string> CrrentUserDetail()
        {
            var UserName = string.Empty;
            var objResponse = new NavigationResponse();

            var adminKeyPair = AES256.AdminKeyPair;

            using (var db = new DB_9ADF60_ewalletEntities())
            {
                HttpCookie authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];

                if (authCookie != null && !string.IsNullOrEmpty(authCookie.Value))
                {
                    FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);
                    LoginResponse model = JsonConvert.DeserializeObject<LoginResponse>(ticket.UserData);

                    var admin = await db.WalletUsers.Where(x => x.WalletUserId == model.AdminId).AsQueryable().FirstOrDefaultAsync();
                    if (admin != null && admin.WalletUserId > 0)
                    {
                        var FirstName = AES256.Decrypt(admin.PrivateKey, admin.FirstName);
                        var LastName = AES256.Decrypt(admin.PrivateKey, admin.LastName);
                        UserName = FirstName + " " + LastName;

                    }
                }
            }
            return UserName;
        }

        //new change
        public async Task<bool> Logout(string token)
        {
            bool IsSuccess = false;
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    var user = db.SessionTokens.Where(x => x.TokenValue == token).FirstOrDefault();
                    if (user != null)
                    {
                        user.IsDeleted = true;
                        db.SaveChanges();
                        IsSuccess = true;
                    }
                    else
                    {
                        IsSuccess = true;
                    }
                }
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("AppUserRepository.cs", "Logout");
            }

            return IsSuccess;
        }


        public async Task<GetPasswordExpiryResponse> GetPasswordExpiry(long request)
        {
            var response = new GetPasswordExpiryResponse();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    response = await db.Database.SqlQuery<GetPasswordExpiryResponse>("EXEC usp_GetPasswordExpiryDate @Walletuserid",
                                 new SqlParameter("@Walletuserid", request)
                                    ).FirstOrDefaultAsync();

                }
            }
            catch (Exception ex)
            {

            }

            return response;
        }

        public async Task<int> InsertWrongPassword(WrongPassword wrongPassword)
        {
            int response = 0;

            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    var isExist = await db.WrongPasswords.Where(x => x.WalletUserId == wrongPassword.WalletUserId).FirstOrDefaultAsync();
                    if (isExist == null || isExist.WrongPasswordCount <= 2)
                    {
                        if (isExist != null)
                        {
                            isExist.WrongPasswordCount = isExist.WrongPasswordCount + 1;
                            response = await db.SaveChangesAsync();
                        }
                        else
                        {
                            db.WrongPasswords.Add(wrongPassword);
                            response = await db.SaveChangesAsync();
                        }
                    }

                }
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("InsertWrongPassword", "InsertWrongPassword", wrongPassword);
            }
            return response;

        }

        public async Task<WrongPassword> GetWrongPasswordCount(long WalletUserId)
        {
            var response = new WrongPassword();

            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    response = await db.WrongPasswords.Where(x => x.WalletUserId == WalletUserId).FirstOrDefaultAsync();

                }
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("InsertWrongPassword", "InsertWrongPassword", WalletUserId);
            }
            return response;

        }

        public async Task<int> DeleteWrongPassword(long WalletUserId)
        {
            int response = 0;

            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    var isExist = await db.WrongPasswords.Where(x => x.WalletUserId == WalletUserId).FirstOrDefaultAsync();
                    db.WrongPasswords.Remove(isExist);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("InsertWrongPassword", "InsertWrongPassword", WalletUserId);
            }
            return response;

        }

        public async Task<bool> GetPasswords(long walletUserId, string password)
        {
            bool response = false;
            var result = new List<AdminPasswordHistory>();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    result = await db.Database.SqlQuery<AdminPasswordHistory>("EXEC usp_GetRecentFivePassword @WalletUserId",
                                new SqlParameter("@WalletUserId", walletUserId)
                                   ).ToListAsync();
                    foreach (var item in result)
                    {
                        if (item.Password == password)
                        {
                            response = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("AdminGetPasswords", "GetPasswords", walletUserId);
            }
            return response;

        }
    }
}
