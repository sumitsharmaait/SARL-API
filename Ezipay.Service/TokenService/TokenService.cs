using ezeePay.Utility.CommonClass;
using ezeePay.Utility.Enums;
using Ezipay.Database;
using Ezipay.Repository.PushNotificationRepo;
using Ezipay.Repository.TokenRepo;
using Ezipay.Repository.UserRepo;
using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.Utility.SendPush;
using Ezipay.ViewModel.SendPushViewModel;
using Ezipay.ViewModel.TokenViewModel;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.TokenService
{
    public class TokenService : ITokenService
    {
        private ITokenRepository _tokenRepository;
        private ISendPushNotification _sendPushNotification;
        private IWalletUserRepository _walletUserRepository;
        private IPushNotificationRepository _pushNotificationRepository;
        public TokenService()
        {
            _tokenRepository = new TokenRepository();
            _sendPushNotification = new SendPushNotification();
            _pushNotificationRepository = new PushNotificationRepository();
            _walletUserRepository = new WalletUserRepository();
        }

        /// <summary>
        /// To generate the token for authenticating requests
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TokenResponse> GenerateToken(TokenRequest request)
        {
            try
            {
                if (request.WalletUserId > 0 && !string.IsNullOrWhiteSpace(request.DeviceUniqueId))
                {
                    string text = Guid.NewGuid().ToString();
                    var token = SHA256ALGO.HashPassword(text);
                    DateTime issuedOn = DateTime.UtcNow;
                    DateTime expiredOn = GetTokenExpiryTime();
                    var userKeyPair = AES256.UserKeyPair();

                    var res = await _tokenRepository.GetPreviousSessionToken(request.WalletUserId, expiredOn);
                    if (res > 0)
                    {
                        var result = await _walletUserRepository.GetUserDetailById(request.WalletUserId);
                        var req = new SendPushRequest
                        {
                            DeviceToken = result.DeviceToken,
                            DeviceType = result.DeviceType,
                            MobileNo = result.MobileNo,
                            WalletUserId = result.WalletUserId
                        };
                        _sendPushNotification.SendLogoutPush(req, result.DeviceToken);

                        var webPush = new ChatModel();

                        webPush.ReceiverId = result.MobileNo;
                        webPush.pushType = (int)PushType.LOGOUT;
                        webPush.alert = ResponseMessages.UNATHORIZED_REQUEST;
                        webPush.Message = ResponseMessages.UNATHORIZED_REQUEST;

                        var _result = await _pushNotificationRepository.WebLogout(webPush);


                        if (_result == true)
                        {
                            var pushModel = new Notification();
                            pushModel.DeviceToken = request.DeviceUniqueId;
                            pushModel.DeviceType = (int)DeviceTypes.Web;
                            pushModel.AlterMessage = string.Empty;
                            pushModel.NotificationType = (int)PushType.LOGOUT;
                            pushModel.SenderId = request.WalletUserId;
                            // pushModel.NotificationJson = JsonConvert.SerializeObject(webPush);

                            int results = await _pushNotificationRepository.SaveNotification(pushModel);

                            var objToken = new TokenRequest
                            {
                                DeviceUniqueId = request.DeviceUniqueId,
                                WalletUserId = request.WalletUserId,
                                IsSuccess = _result
                            };
                            await _tokenRepository.GenerateToken(objToken);
                        }
                    }

                    //var objToken = new SessionToken
                    //{
                    //    WalletUserId = request.WalletUserId,
                    //    TokenValue = token.HashedPassword,
                    //    IssuedTime = issuedOn,
                    //    ExpiryTime = expiredOn,
                    //    PublicKey = userKeyPair.PublicKey,
                    //    PrivateKey = userKeyPair.PrivateKey,
                    //    IsDeleted = false,
                    //    DeviceUniqueId = request.DeviceUniqueId
                    //};

                    var tokenModel = new TokenResponse()
                    {
                        WalletUserId = request.WalletUserId,
                        IssuedOn = issuedOn,
                        ExpiresOn = expiredOn,
                        PublicKey = userKeyPair.PublicKey,
                        PrivateKey = userKeyPair.PrivateKey,
                        Token = token.HashedPassword,
                    };

                    return tokenModel;
                }
            }
            catch (Exception ex)
            {
                //scope.Rollback();
                ex.Message.ToString();
            }
            return null;
        }

        DateTime GetTokenExpiryTime()
        {
            return DateTime.UtcNow.AddMinutes(Convert.ToInt32(ConfigurationManager.AppSettings["AuthTokenExpiry"]));
        }

        public int ValidateAuthenticaion(ServiceAuthenticationRequest request)
        {
            int response = 0;
            response = _tokenRepository.ValidateAuthenticaion(request);
            return response;
        }

        public TempSessionResponse KeysBySessionToken()
        {
            var response = new TempSessionResponse();
            response = _tokenRepository.KeysBySessionToken();
            return response;
        }

        public TempSessionResponse KeysByTempToken()
        {
            return _tokenRepository.KeysByTempToken();
        }

        public async Task<TempTokenResponse> GenerateTempToken(TempTokenRequest request)
        {
            var response = new TempTokenResponse();
            try
            {
                if (!string.IsNullOrWhiteSpace(request.DeviceUniqueId))
                {
                    response = await _tokenRepository.GenerateTempToken(request);
                    return response;
                }
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("TokenService", "GenerateTempToken", request);
                throw;

            }
            return response;
        }
    }
}
