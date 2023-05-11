using ezeePay.Utility.CommonClass;
using ezeePay.Utility.Enums;
using Ezipay.Database;
using Ezipay.Repository.PushNotificationRepo;
using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
//using Ezipay.ViewModel.AirtimeViewModel;
using Ezipay.ViewModel.SendPushViewModel;
using Ezipay.ViewModel.TokenViewModel;
using Newtonsoft.Json;
using System;
//using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
//using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Ezipay.Repository.TokenRepo
{
    public class TokenRepository : ITokenRepository
    {

        /// <summary>
        /// GetPreviousSessionToken
        /// </summary>
        /// <param name="walletUserId"></param>
        /// <param name="expiredOn"></param>
        /// <returns></returns>
        public async Task<int> GetPreviousSessionToken(long walletUserId, DateTime expiredOn)
        {
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                var previousToken = await db.SessionTokens.Where(x => x.WalletUserId == walletUserId && (bool)!x.IsDeleted).ToListAsync();
                if (previousToken != null && previousToken.Count > 0)
                {
                    foreach (var item in previousToken)
                    {
                        item.IsDeleted = true;
                        item.ExpiryTime = expiredOn;
                    }
                    db.SaveChanges();
                    //SendLogoutPush(request.WalletUserId, request.DeviceUniqueId);
                }
            }
            return 1;
        }

        /// <summary>
        /// GenerateTempToken
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TempTokenResponse> GenerateTempToken(TempTokenRequest request)
        {
            TempTokenResponse response = new TempTokenResponse();
            try
            {
                if (!string.IsNullOrWhiteSpace(request.DeviceUniqueId))
                {
                    using (DB_9ADF60_ewalletEntities DbContext = new DB_9ADF60_ewalletEntities())
                    {
                        RemoveATempToken(request.DeviceUniqueId);
                        var userKeyPair = AES256.UserKeyPair();
                        var isExist = await DbContext.TempTokens.FirstOrDefaultAsync(x => x.DeviceUniqueId == request.DeviceUniqueId.Trim());

                        if (isExist == null)
                        {
                            string text = Guid.NewGuid().ToString();
                            var token = SHA256ALGO.HashPassword(text);
                            string TokenValue = token.HashedPassword;
                            TempToken objTemp = new TempToken();
                            objTemp.TokenValue = TokenValue;
                            objTemp.DeviceUniqueId = request.DeviceUniqueId;
                            objTemp.PrivateKey = userKeyPair.PrivateKey;
                            objTemp.PublicKey = userKeyPair.PublicKey;
                            objTemp.IsDeleted = false;
                            objTemp.CreatedDate = DateTime.UtcNow;
                            DbContext.TempTokens.Add(objTemp);
                            DbContext.SaveChanges();
                            response.Token = token.HashedPassword;
                            response.PrivateKey = userKeyPair.PrivateKey;
                            response.PublicKey = userKeyPair.PublicKey;

                            //string encryptKey = AES256.Encrypt2("4512631236589784", userKeyPair.PublicKey);
                            //response.PrivateKey = encryptKey;
                            //response.PublicKey = encryptKey;

                        }
                        await InsertAppDownloadLog(request);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("TokenRepository", "GenerateTempToken", request);
                throw;

            }
            return response;
        }

        private async Task InsertAppDownloadLog(TempTokenRequest request)
        {
            try
            {
                if (request.IsFirstTimeLaunch)
                {
                    using (DB_9ADF60_ewalletEntities dbContext = new DB_9ADF60_ewalletEntities())
                    {
                        var log = dbContext.AppDownloadLogs.Where(x => x.DeviceId == request.DeviceUniqueId).FirstOrDefault();
                        if (log == null)
                        {
                            var entity = new AppDownloadLog
                            {
                                DeviceToken = request.DeviceToken,
                                DeviceType = request.DeviceType,
                                Status = 0,
                                IsActive = true,
                                IsDeleted = false,
                                CreatedDate = DateTime.UtcNow,
                                UpdatedDate = DateTime.UtcNow
                            };
                            dbContext.AppDownloadLogs.Add(entity);
                        }
                        else
                        {
                            log.DeviceToken = request.DeviceToken;
                            log.DeviceType = request.DeviceType;
                        }
                        await dbContext.SaveChangesAsync();
                    }
                }
            }
            catch { }
        }


        /// <summary>
        /// RemoveATempToken
        /// </summary>
        /// <param name="DeviceUniqueId"></param>
        public void RemoveATempToken(string DeviceUniqueId)
        {
            try
            {
                using (DB_9ADF60_ewalletEntities DbContext = new DB_9ADF60_ewalletEntities())
                {
                    var tempTokens = DbContext.TempTokens.Where(x => x.DeviceUniqueId == DeviceUniqueId).ToList();
                    if (tempTokens != null && tempTokens.Count > 0)
                    {
                        DbContext.TempTokens.RemoveRange(tempTokens);
                        DbContext.SaveChanges();
                    }
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// GetDeviceUniqueIdByTempToken
        /// </summary>
        /// <param name="TokenValue"></param>
        /// <returns></returns>
        public async Task<string> GetDeviceUniqueIdByTempToken(string TokenValue)
        {
            string uniqueId = string.Empty;
            try
            {
                using (DB_9ADF60_ewalletEntities DbContext = new DB_9ADF60_ewalletEntities())
                {
                    var isExist = await DbContext.TempTokens.FirstOrDefaultAsync(x => x.TokenValue == TokenValue.Trim());
                    if (isExist != null)
                    {
                        uniqueId = isExist.DeviceUniqueId;
                    }
                }
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("TokenRepository.cs", "GetUniqueIdByTempToken", TokenValue);
            }
            return uniqueId;

        }

        /// <summary>
        /// ValidateAuthenticaion
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public int ValidateAuthenticaion(ServiceAuthenticationRequest request)
        {
            int res = (int)TokenStatusCode.Invalid;
            if (!string.IsNullOrWhiteSpace(request.Token) && request.Type > 0)
            {
                using (DB_9ADF60_ewalletEntities DbContext = new DB_9ADF60_ewalletEntities())
                {
                    if (request.Type == (int)TokenType.TempToken)
                    {

                        var tokenValPre = DbContext.TempTokens.Where(x => x.TokenValue == request.Token).FirstOrDefault();
                        // var tokenValPre = DbContext.TempTokens.Where(a => a.TokenValue == request.Token).First();
                        if (tokenValPre != null)
                        {
                            res = (int)TokenStatusCode.Success;
                        }
                        else
                        {
                            res = (int)TokenStatusCode.Failed;
                        }

                    }
                    else //SessionAuthorization
                    {
                        var tokenVal = DbContext.SessionTokens.Where(x => x.TokenValue == request.Token && x.IsDeleted == false).FirstOrDefault();

                        var walletUser = DbContext.WalletUsers.Where(a => a.WalletUserId == tokenVal.WalletUserId).First();
                        if (tokenVal != null && walletUser.IsActive == true && walletUser.IsDeleted == false)//(tokenVal.ExpiryTime > DateTime.UtcNow)
                        {
                            var previousToken = DbContext.SessionTokens.Where(x => x.WalletUserId == tokenVal.WalletUserId && x.TokenValue != request.Token).ToList();
                            if (previousToken != null && previousToken.Count > 0)
                            {
                                foreach (var item in previousToken)
                                {
                                    item.IsDeleted = true;

                                }
                                DbContext.SaveChanges();
                            }
                            res = (int)TokenStatusCode.Success;
                            tokenVal.ExpiryTime = GetTokenExpiryTime();
                            DbContext.SaveChanges();
                        }
                        else
                        {
                            res = (int)TokenStatusCode.Failed;
                        }


                    }

                }
            }
            return res;
        }

        DateTime GetTokenExpiryTime()
        {
            return DateTime.UtcNow.AddMinutes(Convert.ToInt32(ConfigurationManager.AppSettings["AuthTokenExpiry"]));
        }

        /// <summary>
        /// ValidateMacAddress
        /// </summary>
        /// <param name="DeviceUniqueId"></param>
        /// <returns></returns>
        public async Task<string> ValidateMacAddress(string DeviceUniqueId)
        {
            DeviceUniqueId = DeviceUniqueId.Replace(" ", "").Replace(":", "").Replace("-", "");
            Regex r = new Regex("^[a-fA-F0-9]{12}$");

            if (r.IsMatch(DeviceUniqueId))
            {
                return "Valid Mac";
            }
            else
            {
                return "Invalid Mac";
            }
        }

        /// <summary>
        /// GetWalletUserIdBySession
        /// </summary>
        /// <returns></returns>
        public async Task<SessionResponse> GetWalletUserIdBySession()
        {
            SessionResponse response = new SessionResponse();
            HttpRequestMessage httpRequestMessage = HttpContext.Current.Items["MS_HttpRequestMessage"] as HttpRequestMessage;

            try
            {
                string token = httpRequestMessage.Headers.GetValues("Token").First();
                using (DB_9ADF60_ewalletEntities db = new DB_9ADF60_ewalletEntities())
                {
                    var user = await db.SessionTokens.Where(x => x.TokenValue == token).FirstOrDefaultAsync();
                    if (user != null)
                    {
                        response.WalletUserId = (long)user.WalletUserId;
                        response.PrivateKey = user.PrivateKey;
                        response.PublicKey = user.PublicKey;
                    }
                }

            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("TokenRepository.cs", "GetPatientIdByToken");
            }
            return response;
        }

        /// <summary>
        /// KeysByTempToken
        /// </summary>
        /// <returns></returns>
        public TempSessionResponse KeysByTempToken()
        {
            SessionResponse response = new SessionResponse();
            //TempSessionResponse response = new TempSessionResponse();
            HttpRequestMessage httpRequestMessage = HttpContext.Current.Items["MS_HttpRequestMessage"] as HttpRequestMessage;

            try
            {

                string token = httpRequestMessage.Headers.GetValues("Token").FirstOrDefault();

                using (DB_9ADF60_ewalletEntities db = new DB_9ADF60_ewalletEntities())
                {
                    var user = db.TempTokens.Where(x => x.TokenValue == token).FirstOrDefault();
                    if (user != null)
                    {
                        response.PrivateKey = user.PrivateKey;
                        response.PublicKey = user.PublicKey;
                        response.DeviceUniqueId = user.DeviceUniqueId;
                        response.Token = user.TokenValue;
                        //response.WalletUserId = user.WalletUserId;
                    }
                }

            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("TokenRepository.cs", "KeysByTempToken");
            }
            return response;
        }

        /// <summary>
        /// KeysByTempToken
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<TempSessionResponse> KeysByTempToken(string token)
        {
            SessionResponse response = new SessionResponse();


            try
            {

                using (DB_9ADF60_ewalletEntities db = new DB_9ADF60_ewalletEntities())
                {
                    var user = await db.TempTokens.Where(x => x.TokenValue == token).FirstOrDefaultAsync();
                    if (user != null)
                    {
                        response.PrivateKey = user.PrivateKey;
                        response.PublicKey = user.PublicKey;
                        response.DeviceUniqueId = user.DeviceUniqueId;
                    }
                }

            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("TokenRepository.cs", "KeysByTempToken");
            }
            return response;
        }

        /// <summary>
        /// KeysBySessionToken
        /// </summary>
        /// <returns></returns>
        public TempSessionResponse KeysBySessionToken()
        {
            SessionResponse response = new SessionResponse();
            HttpRequestMessage httpRequestMessage = HttpContext.Current.Items["MS_HttpRequestMessage"] as HttpRequestMessage;

            try
            {
                string token = httpRequestMessage.Headers.GetValues("Token").First();
                using (DB_9ADF60_ewalletEntities db = new DB_9ADF60_ewalletEntities())
                {
                    var user = db.SessionTokens.Where(x => x.TokenValue == token).FirstOrDefault();
                    if (user != null)
                    {
                        response.Token = user.TokenValue;
                        response.PrivateKey = user.PrivateKey;
                        response.PublicKey = user.PublicKey;
                        response.DeviceUniqueId = user.DeviceUniqueId;
                    }
                }

            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("TokenRepository.cs", "KeysByTempToken");
            }
            return response;
        }

        /// <summary>
        /// KeysBySessionToken
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<TempSessionResponse> KeysBySessionToken(string token)
        {
            SessionResponse response = new SessionResponse();


            try
            {

                using (DB_9ADF60_ewalletEntities db = new DB_9ADF60_ewalletEntities())
                {
                    var user = await db.SessionTokens.Where(x => x.TokenValue == token).FirstOrDefaultAsync();
                    if (user != null)
                    {
                        response.PrivateKey = user.PrivateKey;
                        response.PublicKey = user.PublicKey;
                    }
                }

            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("TokenRepository.cs", "KeysByTempToken");
            }
            return response;
        }

        /// <summary>
        /// WebLogout
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> WebLogout(ChatModel model)
        {
            //Response<bool> _result = new Response<bool>();

            bool _result = false;
            string url = ConfigurationManager.AppSettings["WebSocketUrl"];

            try
            {

                HttpWebRequest saveCreditrequest = (HttpWebRequest)WebRequest.Create(url);
                var Token = await GenerateTempToken(new TempTokenRequest { DeviceUniqueId = model.ReceiverId });
                if (Token != null)
                {
                    saveCreditrequest.Headers.Add("Token", Token.Token);
                }
                saveCreditrequest.ContentType = "application/json; charset=utf-8";
                saveCreditrequest.Method = "POST";
                string postSaveCreditData = JsonConvert.SerializeObject(model);
                using (var streamWriter = new StreamWriter(saveCreditrequest.GetRequestStream()))
                {
                    streamWriter.Write(postSaveCreditData);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                using (var finalResponse = (HttpWebResponse)saveCreditrequest.GetResponse())
                {
                    using (var ResponseReader = new StreamReader(finalResponse.GetResponseStream()))
                    {
                        var ResponseString = ResponseReader.ReadToEnd();

                        //TransferToBankSubmitCreditResponce _result = (TransferToBankSubmitCreditResponce)js.Deserialize(submitCredit_objText, typeof(TransferToBankSubmitCreditResponce));
                        _result = JsonConvert.DeserializeObject<bool>(ResponseString);

                    }
                }
            }
            catch (Exception ex)
            {
                (ex.Message).ErrorLog("TokenRepository", "WebLogout", model);
                (ex.Message).ErrorLog("TokenRepository", "WebLogout", url);
            }
            return _result;



        }

        /// <summary>
        /// RemoveLoginSession
        /// </summary>
        /// <param name="WalletUserId"></param>
        public void RemoveLoginSession(long WalletUserId)
        {
            using (DB_9ADF60_ewalletEntities DbContext = new DB_9ADF60_ewalletEntities())
            {
                var previousToken = DbContext.SessionTokens.Where(x => x.WalletUserId == WalletUserId && (bool)!x.IsDeleted).ToList();
                if (previousToken != null && previousToken.Count > 0)
                {
                    foreach (var item in previousToken)
                    {
                        item.IsDeleted = true;
                        item.ExpiryTime = DateTime.UtcNow;
                    }
                    DbContext.SaveChanges();
                }
            }
        }

        public async Task<TokenResponse> GenerateToken(TokenRequest request)
        {
            var tokenModel = new TokenResponse();
//            string userIpAddress = System.Web.HttpContext.Current.Request.UserHostAddress;
            string userIpAddress = null;
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    userIpAddress = ip.ToString();
                }
            }

            try
            {
                using (DB_9ADF60_ewalletEntities DbContext = new DB_9ADF60_ewalletEntities())
                {
                    if (request.WalletUserId > 0 && !string.IsNullOrWhiteSpace(request.DeviceUniqueId))
                    {
                        string text = Guid.NewGuid().ToString();
                        var token = SHA256ALGO.HashPassword(text);
                        DateTime issuedOn = DateTime.UtcNow;
                        DateTime expiredOn = GetTokenExpiryTime();
                        var userKeyPair = AES256.UserKeyPair();
                        var previousToken = await DbContext.SessionTokens.Where(x => x.WalletUserId == request.WalletUserId && (bool)!x.IsDeleted).ToListAsync();
                        if (previousToken != null && previousToken.Count > 0)
                        {
                            foreach (var item in previousToken)
                            {
                                item.IsDeleted = true;
                                item.ExpiryTime = expiredOn;
                            }
                            //DbContext.Entry(previousToken).State = EntityState.Modified;
                            DbContext.SaveChanges();
                            tokenModel.RstKey = 1;
                        }



                        var objToken = new SessionToken
                        {
                            WalletUserId = request.WalletUserId,
                            TokenValue = token.HashedPassword,
                            IssuedTime = issuedOn,
                            ExpiryTime = expiredOn,
                            PublicKey = userKeyPair.PublicKey,
                            PrivateKey = userKeyPair.PrivateKey,
                            IsDeleted = false,
                            DeviceUniqueId = request.DeviceUniqueId,
                            IPaddress =userIpAddress
                        };

                        DbContext.SessionTokens.Add(objToken);
                        DbContext.SaveChanges();


                        tokenModel.WalletUserId = (long)request.WalletUserId;
                        tokenModel.IssuedOn = issuedOn;
                        tokenModel.ExpiresOn = expiredOn;
                        tokenModel.PublicKey = userKeyPair.PublicKey;
                        tokenModel.PrivateKey = userKeyPair.PrivateKey;
                        tokenModel.Token = token.HashedPassword;


                        return tokenModel;
                    }
                }
            }
            catch (Exception ex)
            {
                //scope.Rollback();
                ex.Message.ToString();
            }
            return null;
        }

        /// <summary>
        /// Send Logout push
        /// </summary>
        /// <param name="WalletUserId"></param>
        public void SendLogoutPush(long WalletUserId)
        {
            using (DB_9ADF60_ewalletEntities DbContext = new DB_9ADF60_ewalletEntities())
            {
                var UserData = DbContext.WalletUsers.Where(x => x.WalletUserId == WalletUserId).FirstOrDefault();
                if (UserData != null)
                {
                    #region PushNotification

                    if (!string.IsNullOrEmpty(UserData.DeviceToken))
                    {
                        if (UserData.DeviceType != (int)DeviceTypes.Web)
                        {
                            NotificationDefaultKeys pushModel = new NotificationDefaultKeys();
                            pushModel.alert = ResponseMessages.UNATHORIZED_REQUEST;
                            pushModel.pushType = (int)PushType.LOGOUT;

                            PushNotificationModel push = new PushNotificationModel();
                            push.deviceType = (int)UserData.DeviceType;
                            push.deviceKey = UserData.DeviceToken;
                            if ((int)UserData.DeviceType == (int)DeviceTypes.ANDROID)
                            {
                                PushPayload<NotificationDefaultKeys> aps = new PushPayload<NotificationDefaultKeys>();
                                PushPayloadData<NotificationDefaultKeys> _data = new PushPayloadData<NotificationDefaultKeys>();
                                _data.notification = pushModel;
                                aps.data = _data;

                                aps.to = UserData.DeviceToken;
                                aps.collapse_key = string.Empty;
                                push.message = JsonConvert.SerializeObject(aps);

                            }
                            if ((int)UserData.DeviceType == (int)DeviceTypes.IOS)
                            {
                                NotificationJsonResponse<NotificationDefaultKeys> aps = new NotificationJsonResponse<NotificationDefaultKeys>();
                                aps.aps = pushModel;

                                push.message = JsonConvert.SerializeObject(aps);
                            }
                            new PushNotificationRepository().sendPushNotification(push);
                        }
                        else
                        {
                            var keys = AES256.AdminKeyPair;
                            WebLogout(new ChatModel { ReceiverId = UserData.StdCode + AES256.Decrypt(keys.PrivateKey, UserData.MobileNo) });
                        }

                    }
                    #endregion
                }
            }
        }

    }
}
