using Ezipay.Database;
using Ezipay.Repository.AdminRepo.AuthenticationApiRepo;
using Ezipay.Repository.TokenRepo;
using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.TokenViewModel;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Ezipay.Service.AdminService.AuthenticationService
{
    public class AuthenticationApiService : IAuthenticationApiService
    {
        private IAuthenticationApiRepository _authenticationApiRepository;
        private ITokenRepository _tokenRepository;
        public AuthenticationApiService()
        {
            _authenticationApiRepository = new AuthenticationApiRepository();
            _tokenRepository = new TokenRepository();
        }
        /// <summary>
        /// Admin Login using email and password
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<LoginResponse> Login(LoginRequest request)
        {
            double d = 0;
            var response = new LoginResponse();

            var adminKeyPair = AES256.AdminKeyPair;
            request.EmailId = AES256.Encrypt(adminKeyPair.PublicKey, request.EmailId);

            var admin = await _authenticationApiRepository.Login(request);
            //check pwd 
            var passwordExpiryDate = await _authenticationApiRepository.GetPasswordExpiry(admin.WalletUserId);
            //check wrong pwd
            var wrongPasswords = await _authenticationApiRepository.GetWrongPasswordCount(admin.WalletUserId);
            if (wrongPasswords != null)
            {
                DateTime a = Convert.ToDateTime(wrongPasswords.CreatedDate);
                DateTime b = DateTime.UtcNow;
                d = b.Subtract(a).TotalMinutes;
            }
            if (wrongPasswords == null || wrongPasswords != null)
            {
                if (d == 0 || d >= 5 || wrongPasswords.WrongPasswordCount <= 2) //change 3 to 2
                {
                    if (passwordExpiryDate.PasswordDays <= 290)
                    {
                        if (admin != null && admin.WalletUserId > 0 && admin.IsDeleted == false)
                        {
                            if (admin.IsActive == true)
                            {
                                response.IsSuccess = true;

                                var hashedObject = SHA256ALGO.HashPasswordDecryption(request.Password, admin.HashedSalt);
                                if (hashedObject.HashedPassword == admin.HashedPassword)
                                {
                                    //success login here
                                    //& delete count of wrong pwds
                                    await _authenticationApiRepository.DeleteWrongPassword(admin.WalletUserId);

                                    response.FirstName = AES256.Decrypt(admin.PrivateKey, admin.FirstName);
                                    response.LastName = AES256.Decrypt(admin.PrivateKey, admin.LastName);
                                    response.Email = AES256.Decrypt(adminKeyPair.PrivateKey, admin.EmailId);

                                    response.AdminId = (int)admin.WalletUserId;
                                    response.IsSuccess = true;
                                    response.RstKey = 6;

                                    response.PasswordExpiryDay = passwordExpiryDate.PasswordDays;
                                    #region Map session related values
                                    var token = new TokenRequest { DeviceUniqueId = request.DeviceUniqueId, WalletUserId = admin.WalletUserId };
                                    var sessionToken = await _tokenRepository.GenerateToken(token);

                                    response.PrivateKey = sessionToken.PrivateKey;
                                    response.PublicKey = sessionToken.PublicKey;

                                    //string encryptKey = AES256.Encrypt2("4512631236589784", sessionToken.PublicKey);

                                    //response.PrivateKey = encryptKey;
                                    //response.PublicKey = encryptKey;

                                    response.Token = sessionToken.Token;
                                    #endregion

                                }
                                else
                                {

                                    response.RstKey = 5;

                                    response.Email = AES256.Decrypt(adminKeyPair.PrivateKey, admin.EmailId);
                                    response.FirstName = AES256.Decrypt(admin.PrivateKey, admin.FirstName);
                                    response.LastName = AES256.Decrypt(admin.PrivateKey, admin.LastName);
                                    response.AdminId = (int)admin.WalletUserId;
                                    //insert wrong password --3 baar wrong pwd 
                                    var req = new WrongPassword
                                    {
                                        WalletUserId = admin.WalletUserId,
                                        HashedPassword = hashedObject.HashedPassword,
                                        WrongPasswordCount = 1,
                                        CreatedDate = DateTime.UtcNow,
                                        UpdatedDate = DateTime.UtcNow
                                    };
                                    await _authenticationApiRepository.InsertWrongPassword(req);
                                }
                            }
                        }
                        else
                        {
                            response.RstKey = 2;
                        }
                    }
                    else
                    {
                        response.RstKey = 7;

                    }

                }
                else
                {
                    response.RstKey = 8;

                }
            }
            else
            {
                response.RstKey = 8;
            }

            return response;
        }

        /// <summary>
        /// Change password for admin using old password      
        /// </summary>
        /// <param name="request"></param>
        /// <returns>bool</returns>
        public async Task<ChangePasswordResponse> ChangePassword(ChangePasswordRequest request, string token)
        {
            var response = new ChangePasswordResponse();
            try
            {
                string TokenValue = token;
                response = await _authenticationApiRepository.ChangePassword(request, TokenValue);
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("WalletUserService.cs", "ChangePassword", request);
            }
            return response;

        }

        /// <summary>
        /// Get navigation list for logged in user 
        /// </summary>
        /// <returns></returns>
        public async Task<NavigationResponse> NavigationList(NavigationsRequest request)
        {
            var response = new NavigationResponse();

            var adminKeyPair = AES256.AdminKeyPair;
            try
            {
                response = await _authenticationApiRepository.NavigationList(request);

            }
            catch (Exception ex)
            {

            }

            return response;
        }

        /// <summary>
        /// Get crrent user detail 
        /// </summary>
        /// <returns></returns>
        public async Task<string> CrrentUserDetail()
        {
            var UserName = string.Empty;
            try
            {
                UserName = await _authenticationApiRepository.CrrentUserDetail();
            }
            catch (Exception ex)
            {

            }

            return UserName;
        }

        //new change
        public async Task<bool> Logout()
        {
            HttpRequestMessage httpRequestMessage = HttpContext.Current.Items["MS_HttpRequestMessage"] as HttpRequestMessage;
            string token = httpRequestMessage.Headers.GetValues("Token").First();
            bool IsSuccess = false;
            try
            {
                IsSuccess = await _authenticationApiRepository.Logout(token);
            }
            catch (Exception ex)
            {

            }
            return IsSuccess;
        }
    }
}
