using ezeePay.Utility.CommonClass;
using ezeePay.Utility.Enums;
using Ezipay.Database;
using Ezipay.Repository.AdminRepo.ChargeBack;
using Ezipay.Repository.TokenRepo;
using Ezipay.Repository.UserRepo;
using Ezipay.Service.Admin.ShareAndEarn;
using Ezipay.Service.AdminService;
using Ezipay.Utility.AWSS3;
using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.Utility.SendEmail;
using Ezipay.Utility.SendOtp;
using Ezipay.Utility.SendPush;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.common;
using Ezipay.ViewModel.SendEmailViewModel;
using Ezipay.ViewModel.SendOtpViewModel;
using Ezipay.ViewModel.SendPushViewModel;
using Ezipay.ViewModel.TokenViewModel;
using Ezipay.ViewModel.WalletUserVM;
using Newtonsoft.Json;
using QRCoder;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
//using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Ezipay.Service.UserService
{
    public class WalletUserService : IWalletUserService
    {
        private IWalletUserRepository _walletUserRepository;
        private ITokenRepository _tokenRepository;
        private ISendPushNotification _sendPushNotification;
        private ISendEmails _sendEmails;
        private ISendMessage _sendMessage;
        private IS3Uploader _s3Uploader;
        private IShareAndEarnService _shareAndEarnService;
        private IChargeBackRepository _ChargeBackRepository;
        private IUserApiService _userApiService;


        public WalletUserService()
        {
            // _userApiService = new UserApiService();
            _userApiService = new UserApiService();
            _walletUserRepository = new WalletUserRepository();
            _tokenRepository = new TokenRepository();
            _sendEmails = new SendEmails();
            _sendMessage = new SendMessage();
            _s3Uploader = new S3Uploader();
            _sendPushNotification = new SendPushNotification();
            _shareAndEarnService = new ShareAndEarnService();
            _ChargeBackRepository = new ChargeBackRepository();

        }

        public async Task<UserExistanceResponse> IsCredentialsExistance(UserExistanceRequest request)
        {
            var result = new UserExistanceResponse();
            result = await _walletUserRepository.CredentialsExistance(request);
            return result;
        }
        public async Task<UserExistanceResponse> CredentialsExistanceForMobileNumber(UserExistanceRequest request)
        {
            var result = new UserExistanceResponse();
            result = await _walletUserRepository.CredentialsExistanceForMobileNumber(request);
            return result;
        }
        public async Task<UserSignupResponse> SignUp(UserSignupRequest request)
        {
            var response = new UserSignupResponse();
            var req = new UserExistanceRequest { EmailId = request.EmailId, MobileNo = request.MobileNo };
            var res = await _walletUserRepository.CredentialsExistance(req);
            if (res.RstKey == (int)UserExistanceStatus.BothNotExist)
            {
                response = await SignUpWithEmail(request);
                response.RstKey = response.RstKey;
            }
            else
            {
                response.RstKey = res.RstKey;
            }
            return response;
        }

        public async Task<UserSignupResponse> SignUpWithEmail(UserSignupRequest request)
        {
            var response = new UserSignupResponse();
            var adminKeyPair = AES256.AdminKeyPair;
            string Mobile = AES256.Encrypt(adminKeyPair.PublicKey, request.MobileNo.Trim());
            string EmailId = AES256.Encrypt(adminKeyPair.PublicKey, request.EmailId.Trim().ToLower());
            var hashedObject = SHA256ALGO.HashPassword(request.Password);
            var userKeyPair = AES256.UserKeyPair();
            string ddeviceToken = "";
            //var referalData = JsonConvert.DeserializeObject<TagsData>(request.ReferalDetails);
            if (request.DeviceType == 2)
            {
                ddeviceToken = request.DeviceToken.ToLower();
            }
            else
            {
                ddeviceToken = request.DeviceToken;
            }

            var req = new QrCodeRequest
            {
                QrCode = request.IsdCode + "," + request.MobileNo
            };
            var qrCode = await GenerateQrCode(req);
            // IsOtpVerified = true //when otp is in register popup
            try
            {
                var _walletUser = new WalletUser
                {
                    UserType = (int)WalletUserTypes.AppUser,
                    ProfileImage = request.ProfileImage,
                    AdminUserId = 0,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    StdCode = request.IsdCode,
                    IsActive = true,
                    IsDeleted = false,
                    IsOtpVerified = false,
                    IsNotification = true,
                    IsEmailVerified = false,
                    Otp = string.Empty,
                    QrCode = qrCode.QrCodeImage,
                    CurrencyId = (int)CurrencyTypes.Ghanaian_Cedi,
                    CurrentBalance = "0",
                    DeviceToken = ddeviceToken,
                    DeviceType = request.DeviceType,
                    EmailId = EmailId,
                    FirstName = AES256.Encrypt(userKeyPair.PublicKey, request.FirstName),
                    LastName = AES256.Encrypt(userKeyPair.PublicKey, request.LastName),
                    HashedPassword = hashedObject.HashedPassword,
                    HashedSalt = hashedObject.SlatBytes,
                    MobileNo = Mobile,
                    PrivateKey = userKeyPair.PrivateKey,
                    PublicKey = userKeyPair.PublicKey,
                    IsFirstTimeUser = true,
                    DocumetStatus = (int)DocumentStatus.NoDocuments,
                    IsDisabledTransaction = false,
                    EarnedPoints = 0,
                    EarnedAmount = 0,
                    IsNewQrCode = true
                    //  AppVersion= GlobalData.AppVersion
                };

                var result = await _walletUserRepository.SignUp(_walletUser);
                //var resferalUrl = await _shareAndEarnService.GetReferalUrl(result.WalletUserId);
                //var referAmount = await _shareAndEarnService.GetRewardList();
                if (result != null)
                {
                    // var ShareLink = JsonConvert.DeserializeObject<dynamic>(resferalUrl.ToString());
                    response.StatusCode = (int)UserSignUpStatus.Registered;
                    response.IsEmailVerified = (bool)_walletUser.IsEmailVerified;
                    response.IsSuccess = true;
                    response.RstKey = 1;
                    // response.ReferalUrl = resferalUrl;
                }
                if (response.IsSuccess)
                {
                    #region Map session related values
                    var token = new TokenRequest { DeviceUniqueId = request.DeviceUniqueId, WalletUserId = result.WalletUserId };
                    var sessionToken = await _tokenRepository.GenerateToken(token);

                    response.PrivateKey = sessionToken.PrivateKey;
                    response.PublicKey = sessionToken.PublicKey;

                    //string encryptKey = AES256.Encrypt2("4512631236589784", sessionToken.PublicKey);
                    //response.PrivateKey = encryptKey;
                    //response.PublicKey = encryptKey;


                    response.Token = sessionToken.Token;

                    #endregion
                    #region Map user detail

                    response.CurrentBalance = _walletUser.CurrentBalance;

                    response.EmailId = AES256.Decrypt(adminKeyPair.PrivateKey, _walletUser.EmailId);
                    response.MobileNo = AES256.Decrypt(adminKeyPair.PrivateKey, _walletUser.MobileNo);
                    response.StdCode = _walletUser.StdCode;
                    response.FirstName = AES256.Decrypt(_walletUser.PrivateKey, _walletUser.FirstName);
                    response.LastName = AES256.Decrypt(_walletUser.PrivateKey, _walletUser.LastName);
                    response.QrCode = _walletUser.StdCode + AES256.Decrypt(adminKeyPair.PrivateKey, _walletUser.MobileNo);
                    response.QrCodeUrl = CommonSetting.imageUrl + _walletUser.QrCode; // AppSetting.QrCodeUrl + _walletUser.WalletUserId.ToString();
                    response.IsEmailVerified = (bool)_walletUser.IsEmailVerified;
                    response.IsMobileNoVerified = (bool)_walletUser.IsOtpVerified;
                    response.IsNotificationOn = _walletUser.IsNotification ?? false;
                    response.ProfileImage = _walletUser.ProfileImage;
                    response.WalletUserId = _walletUser.WalletUserId;
                    response.DeviceToken = _walletUser.DeviceToken;
                    response.DeviceType = (int)_walletUser.DeviceType;
                    response.Status = (int)UserSignUpStatus.Registered;
                    if (request.ReferalDetails != null)
                    {
                        var referalData = request.ReferalDetails;//JsonConvert.DeserializeObject<TagsData>(request.ReferalDetails);
                        //call function for refer and earn 
                        try
                        {
                            //    await SaveReferalPoints(result.WalletUserId, Convert.ToInt32(referalData), Convert.ToInt32(referAmount.ReceiverPoints), Convert.ToInt32(referAmount.SenderPoints), Convert.ToInt32(referAmount.ConversionPointsValue));
                            //   response.EarnedPoints = Convert.ToDecimal(_walletUser.EarnedPoints);
                        }
                        catch (Exception ex)
                        {

                        }

                    }

                    if (_walletUser.UserType == (int)WalletUserTypes.Merchant)
                    {
                        var ws = await _walletUserRepository.IsUserMerchant(_walletUser.WalletUserId);
                        if (ws != null)
                        {
                            response.FirstName = ws;
                            response.LastName = string.Empty;
                        }
                    }
                    #endregion
                    _walletUser.IsFirstTimeUser = false;

                    if ((int)DeviceTypes.Web != request.DeviceType)
                    {
                        _walletUser.DeviceType = request.DeviceType;
                        _walletUser.DeviceToken = request.DeviceToken;
                    }
                    else
                    {
                        _walletUser.DeviceToken = request.EmailId;
                    }

                    var _EmailVerification = new EmailVerification();
                    _EmailVerification.EmailId = _walletUser.EmailId;
                    _EmailVerification.CreatedDate = DateTime.UtcNow;
                    _EmailVerification.IsVerified = false;
                    _EmailVerification.VerificationDate = DateTime.UtcNow;
                    _EmailVerification.WalletUserId = 0;
                    _EmailVerification.IsMailSent = true;
                    _EmailVerification = await _walletUserRepository.InsertEmailVerification(_EmailVerification);
                    if (_EmailVerification != null)
                    {
                        string uniqueToken = RandomAlphaNumerals(15) + "_" + _EmailVerification.EmailVerificationId.ToString();
                        string VerifyMailLink = CommonSetting.VerifyMailLink + "/" + HttpUtility.UrlEncode(uniqueToken);
                        string filename = CommonSetting.EmailVerificationTemplate;
                        var body = _sendEmails.ReadEmailformats(filename);
                        string Body = string.Format(body, VerifyMailLink);
                        //  body = body.Replace("$$IsVerified$$", VerifyMailLink);
                        //Send Email to user on register
                        var emailModel = new EmailModel
                        {
                            TO = request.EmailId,
                            Subject = ResponseMessages.USER_REGISTERED,//"Registered successfully",
                            Body = Body
                        };
                        _sendEmails.SendEmail(emailModel);
                        response.Message = ResponseEmailMessage.VERIFICATION_EMAIL;

                    }
                }
            }
            catch (Exception ex)
            {
                //tran.Rollback();
                ex.Message.ErrorLog("WalletUserService.cs", "SignUpWithEmail", request);
                response.StatusCode = (int)UserSignUpStatus.NotRegistered;
            }
            return response;
        }


        public async Task<OtpResponse> SendOtp(OtpRequest request, string sessionToken)
        {
            var response = new OtpResponse();
            var UserDetail = await UserProfile(sessionToken);
            var givenMobileNoISDcode = UserDetail.StdCode + UserDetail.MobileNo;
            var requestMobileNoISDcode = request.IsdCode + request.MobileNo;

            if (UserDetail.IsEmailVerified == true)
            {
                if (givenMobileNoISDcode == requestMobileNoISDcode)
                {
                    string Otp = CommonSetting.GetOtp();

                    var req = new SendOtpRequest
                    {
                        IsdCode = request.IsdCode,
                        MobileNo = request.MobileNo,
                        Otp = Otp
                    };
                    response = await _walletUserRepository.SendOtp(req);

                }
                else
                {
                    response.StatusCode = 3;
                }
            }
            else
            {
                response.StatusCode = 5;
            }
            return response;
        }

        public async Task<OtpResponse> CallBackOtp(OtpRequest request, string sessionToken)
        {
            var response = new OtpResponse();
            var UserDetail = await UserProfile(sessionToken);

            var givenMobileNoISDcode = UserDetail.StdCode + UserDetail.MobileNo;
            var requestMobileNoISDcode = request.IsdCode + request.MobileNo;

            if (givenMobileNoISDcode == requestMobileNoISDcode)
            {
                var result = new OneTimePassword();

                var req = new SendOtpCallBackRequest
                {
                    IsdCode = request.IsdCode,
                    MobileNo = request.MobileNo
                };
                result = await _walletUserRepository.GetOtpForCallBack(req);
                if (result != null && result.Callbackcounter == 1)
                {
                    string customer = request.MobileNo.Substring(0, 1);

                    if (request.IsdCode != "+225") //civ con.change 
                    {
                        if (customer == "0")
                        {
                            customer = request.MobileNo.Remove(0, 1);
                        }
                        else
                        {
                            customer = request.MobileNo;
                        }
                    }
                    else
                    {
                        customer = request.MobileNo;
                    }

                    var isd = request.IsdCode.Remove(0, 1);
                    var teleSignReq = new SendOtpTeleSignRequest
                    {
                        phone_number = isd + customer,
                        template = ResponseMessages.CALL_BACK_OTP_MESSAGE + " " + result.Otp,
                        verify_code = result.Otp
                    };
                    var res = await _sendMessage.CallBackTeleSign(teleSignReq);
                    if (res == true)
                    {
                        response.IsSuccess = true;
                        response.StatusCode = 1;
                    }
                }
                else
                {
                    response.StatusCode = 2;
                }
            }
            else
            {
                response.StatusCode = 3;
            }
            return response;
        }

        public async Task<UserExistanceResponse> VerifyOtp(VerifyOtpRequest request)
        {

            UserExistanceResponse response = new UserExistanceResponse();

            response = await _walletUserRepository.VerifyOtp(request);
            if (response.RstKey == 1)
            {
                response.Message = ResponseMessages.VALID_OTP;
                response.Status = (int)OtpStatus.VALID_OTP;
            }
            else
            {
                response.Message = ResponseMessages.INVALID_OTP;
                response.Status = (int)OtpStatus.INVALID_OTP;
            }
            return response;
        }

        public async Task<UserLoginResponse> Login(UserLoginRequest request)
        {

            var response = new UserLoginResponse();
            var adminKeyPair = AES256.AdminKeyPair;
            request.SecretKey = request.SecretKey.Trim().ToLower();
            request.SecretKey = AES256.Encrypt(adminKeyPair.PublicKey, request.SecretKey.ToLower());
            var user = await _walletUserRepository.Login(request);

            if ((user.UserType == 3 && user.DocumetStatus == 2) || user.UserType == 1 || user.UserType == 2)
            {
                var referalUrl = await _walletUserRepository.GetReferalUrl(user.WalletUserId);
                if (user != null && (user.UserType == (int)WalletUserTypes.AppUser || user.UserType == (int)WalletUserTypes.Merchant))
                {
                    //if ((bool)user.IsEmailVerified)
                    //{
                    var hashedObject = SHA256ALGO.HashPasswordDecryption(request.Password, user.HashedSalt);
                    if (hashedObject.HashedPassword == user.HashedPassword)
                    {
                        if ((bool)user.IsActive && (bool)user.IsDeleted == false)
                        {
                            if (referalUrl != null)
                            {
                                var ShareLink = JsonConvert.DeserializeObject<dynamic>(referalUrl.ReferUrl);
                                if (ShareLink != null || ShareLink != "")
                                {
                                    response.ReferalUrl = ShareLink;
                                }
                            }
                            else
                            {
                                response.ReferalUrl = "";
                            }
                            response.WalletUserId = user.WalletUserId;
                            response.IsSuccess = true;
                            response.Status = (int)LoginStatusType.SUCCESS;
                            response.RstKey = 6;
                            if (user.IsFirstTimeUser == true)
                            {
                                response.LoginType = (int)LoginTypes.Tutorial;
                            }
                            else if (user.IsTemporaryPassword == true)
                            {
                                response.LoginType = (int)LoginTypes.ChangePassword;
                            }
                            else
                            {
                                response.LoginType = (int)LoginTypes.Home;
                            }
                        }
                        else
                        {
                            response.IsSuccess = false;
                            response.Status = (int)LoginStatusType.INACTIVE;
                            response.RstKey = (int)LoginStatusType.INACTIVE;
                        }
                    }
                    else
                    {

                        if (!string.IsNullOrEmpty(user.Otp) && user.OtpHashedSalt != null)
                        {
                            hashedObject = SHA256ALGO.HashPasswordDecryption(request.Password, user.OtpHashedSalt);
                            if (hashedObject.HashedPassword == user.Otp)
                            {
                                if ((bool)user.IsActive)
                                {
                                    response.IsSuccess = true;
                                    response.Status = (int)LoginStatusType.SUCCESS;
                                    response.RstKey = 6;
                                    user.IsTemporaryPassword = true;
                                    response.LoginType = (int)LoginTypes.ChangePassword;
                                }
                                else
                                {
                                    response.IsSuccess = false;
                                    response.Status = (int)LoginStatusType.INACTIVE;
                                    response.RstKey = 1;
                                }
                            }
                            else
                            {
                                response.IsSuccess = false;
                                response.Status = (int)LoginStatusType.FAILED;
                            }
                        }
                        else
                        {
                            response.IsSuccess = false;
                            response.Status = (int)LoginStatusType.FAILED;
                        }
                    }
                    if (response.IsSuccess)
                    {
                        #region Map session related values
                        var token = new TokenRequest { DeviceUniqueId = request.DeviceUniqueId, WalletUserId = user.WalletUserId };
                        var sessionToken = await _tokenRepository.GenerateToken(token);
                        if (sessionToken.RstKey == 1)
                        {
                            try
                            {
                                var req = new SendPushRequest
                                {
                                    DeviceUniqueId = request.DeviceUniqueId,
                                    WalletUserId = user.WalletUserId,
                                    DeviceToken = user.DeviceToken,
                                    DeviceType = user.DeviceType,
                                    MobileNo = user.MobileNo
                                };

                                _sendPushNotification.SendLogoutPush(req, user.DeviceToken);
                                if (user.AppVersion < 0 || user.AppVersion == null || user.AppVersion == 0)
                                {
                                    user.AppVersion = GlobalData.AppVersion;
                                    await _walletUserRepository.UpdateUserDetail(user);
                                }
                            }
                            catch
                            {

                            }
                        }
                        response.PrivateKey = sessionToken.PrivateKey;
                        response.PublicKey = sessionToken.PublicKey;

                        //string encryptKey = AES256.Encrypt2("4512631236589784", sessionToken.PublicKey);

                        //response.PrivateKey = encryptKey;
                        //response.PublicKey = encryptKey;



                        response.Token = sessionToken.Token;

                        #endregion
                        if (user.IsNewQrCode == false)
                        {
                            ////GlobalData.Key = response.Token;
                            var reqe = new QrCodeRequest
                            {
                                QrCode = user.StdCode + "," + AES256.Decrypt(adminKeyPair.PrivateKey, user.MobileNo)
                            };
                            var qrCode = await GenerateQrCode(reqe, response.Token);

                            user.QrCode = qrCode.QrCodeImage;
                        }
                        #region Map user detail
                        if (user.DocumetStatus == 2) //kyc pop up 
                        {
                            response.DocumetStatus = (int)user.DocumetStatus;
                            response.DocStatus = true;
                        }
                        else
                        {
                            response.DocumetStatus = (int)user.DocumetStatus;
                            response.DocStatus = false;
                        }

                        response.CurrentBalance = user.CurrentBalance;

                        response.EmailId = AES256.Decrypt(adminKeyPair.PrivateKey, user.EmailId);
                        response.MobileNo = AES256.Decrypt(adminKeyPair.PrivateKey, user.MobileNo);
                        response.StdCode = user.StdCode;
                        response.FirstName = AES256.Decrypt(user.PrivateKey, user.FirstName);
                        response.LastName = AES256.Decrypt(user.PrivateKey, user.LastName);
                        response.QrCode = response.StdCode + "," + response.MobileNo;
                        response.QrCodeUrl = !string.IsNullOrEmpty(user.QrCode) ? CommonSetting.imageUrl + user.QrCode : string.Empty;
                        response.IsEmailVerified = (bool)user.IsEmailVerified;
                        response.IsMobileNoVerified = (bool)user.IsOtpVerified;
                        response.IsNotificationOn = user.IsNotification ?? false;
                        response.ProfileImage = user.ProfileImage;
                        response.UserType = (int)user.UserType;
                        if (user.UserType == (int)WalletUserTypes.Merchant)
                        {
                            var ws = await _walletUserRepository.IsUserMerchant(user.WalletUserId);
                            if (ws != null)
                            {
                                response.FirstName = ws;
                                response.LastName = string.Empty;
                            }
                        }
                        #endregion

                        user.IsFirstTimeUser = false;

                        if ((int)DeviceTypes.Web != request.DeviceType)
                        {
                            user.DeviceType = request.DeviceType;
                            user.DeviceToken = request.DeviceToken;
                        }
                        else
                        {
                            user.DeviceToken = request.SecretKey;
                        }
                        await _walletUserRepository.UpdateUserDetail(user);
                    }
                    //}
                    //else
                    //{
                    //    response.Status = (int)LoginStatusType.EMAILNOTVERIFIED;
                    //}
                }
                else
                {
                    if (!(user.UserType == (int)WalletUserTypes.AppUser || user.UserType == (int)WalletUserTypes.Merchant))
                    {
                        response.Status = (int)LoginStatusType.INVALID_USER_TYPE;
                        response.RstKey = 3;
                    }
                    //Email id not exist
                    else
                    {
                        response.Status = (int)LoginStatusType.EMAILNOTEXIST;
                        response.RstKey = 4;
                    }
                }
            }
            else
            {
                response.Status = (int)LoginStatusType.FAILED;
                response.RstKey = 21;
            }

            return response;
        }

        public async Task<bool> Logout(string tokenHeader)
        {
            bool result = false;
            ////string tokenHeader = GlobalData.Key;

            return result = await _walletUserRepository.Logout(tokenHeader);
        }

        //public async Task<UserDetailResponse> UserProfile()
        //{

        //    string tokenHeader = GlobalData.Key;//This value get from session filter

        //    var response = new UserDetailResponse();
        //    string TokenValue = string.Empty;
        //    try
        //    {
        //        if (tokenHeader != null)
        //        {
        //            if (!string.IsNullOrEmpty(tokenHeader))
        //            {
        //                TokenValue = tokenHeader.ToString();
        //            }

        //            response = await _walletUserRepository.UserProfile(TokenValue);
        //            var d = await _walletUserRepository.GetApiKeysData(response.WalletUserId);

        //            var referalUrl = await _walletUserRepository.GetReferalUrl(response.WalletUserId);
        //            if (referalUrl != null)
        //                response.ReferalUrl = referalUrl.ReferUrl;

        //            if (d.MerchantKey != null && d.WalletUserId > 0)
        //            {
        //                response.ApiKey = d.ApiKey;
        //                response.MerchantKey = d.MerchantKey;
        //                response.WalletUserId = d.WalletUserId;
        //            }
        //            if (response.DocumetStatus == 2)
        //            {
        //                response.RstKey = 1;
        //                response.DocStatus = true;
        //            }
        //            else if (response.DocStatus && (response.DocumetStatus == (int)DocumentStatus.NoDocuments || response.DocumetStatus == (int)DocumentStatus.Pending || response.DocumetStatus == (int)DocumentStatus.Rejected || response.DocumetStatus == (int)DocumentStatus.NotOk))
        //            {
        //                response.DocStatus = false;
        //                response.RstKey = 2;
        //            }
        //            else
        //            {
        //                response.DocStatus = true;
        //                response.RstKey = 1;
        //            }
        //        }
        //        if (string.IsNullOrEmpty(TokenValue))
        //        {
        //            var _currentUser = CommonMethods.GetWebCurrentUser();
        //            if (_currentUser != null)
        //            {
        //                TokenValue = _currentUser.Token;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    return response;
        //}
        public async Task<UserDetailResponse> UserProfile(string token)
        {

            string tokenHeader = token;// GlobalData.Key;//This value get from session filter

            var response = new UserDetailResponse();
            string TokenValue = string.Empty;
            try
            {
                if (tokenHeader != null)
                {
                    if (!string.IsNullOrEmpty(tokenHeader))
                    {
                        TokenValue = tokenHeader.ToString();
                    }

                    response = await _walletUserRepository.UserProfile(TokenValue);
                    var d = await _walletUserRepository.GetApiKeysData(response.WalletUserId);

                    var referalUrl = await _walletUserRepository.GetReferalUrl(response.WalletUserId);
                    if (referalUrl != null)
                        response.ReferalUrl = referalUrl.ReferUrl;

                    if (d.MerchantKey != null && d.WalletUserId > 0)
                    {
                        response.ApiKey = d.ApiKey;
                        response.MerchantKey = d.MerchantKey;
                        response.WalletUserId = d.WalletUserId;
                    }
                    if (response.DocumetStatus == 2)
                    {
                        response.RstKey = 1;
                        response.DocStatus = true;
                    }
                    else
                    {
                        response.DocStatus = false;
                        response.RstKey = 2;
                    }

                    //else if (response.DocStatus && (response.DocumetStatus == (int)DocumentStatus.NoDocuments || response.DocumetStatus == (int)DocumentStatus.Pending || response.DocumetStatus == (int)DocumentStatus.Rejected || response.DocumetStatus == (int)DocumentStatus.NotOk))
                    //{
                    //    response.DocStatus = false;
                    //    response.RstKey = 2;
                    //}
                    //else //
                    //{
                    //    response.DocStatus = true;
                    //    response.RstKey = 1;
                    //}
                }
                if (string.IsNullOrEmpty(TokenValue))
                {
                    var _currentUser = CommonMethods.GetWebCurrentUser();
                    if (_currentUser != null)
                    {
                        TokenValue = _currentUser.Token;
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return response;
        }
        public async Task<UserDetailByQrCodeResponse> UserDetailById(UserDetailByQrCodeRequest request)
        {
            var response = new UserDetailByQrCodeResponse();
            try
            {
                response = await _walletUserRepository.UserDetailById(request);
                if (response != null)
                {
                    response.RstKey = 1;
                }
                else
                {
                    response.RstKey = 2;
                }
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("WalletUserService.cs", "UserDetailById", request);
            }
            return response;

        }

        public async Task<bool> DocumentUpload(DocumentUploadRequest request, string token)
        {
            var adminKeyPair = AES256.AdminKeyPair;
            string ATMCard = AES256.Encrypt(adminKeyPair.PublicKey, request.ATMCard.Trim());
            string IdCard = AES256.Encrypt(adminKeyPair.PublicKey, request.IdCard.Trim());
            var AdminKeys = AES256.AdminKeyPair;
            Documentresponse response = new Documentresponse();
            ////var currentuser = await UserProfile();
            var currentuser = await UserProfile(token);
            bool result = false;
            try
            {
                result = await _walletUserRepository.DocumentUpload(request, currentuser.WalletUserId, IdCard, ATMCard);
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("AppUserRepository.cs", "UpdateUserProfile", request);
                result = false;
            }
            return result;

        }

        public async Task<QrCodeData> GenerateQrCode(QrCodeRequest request, string token = null)
        {
            var qr = new QrCodeData();
            string result = "";
            bool isUploaded = false;
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(request.QrCode, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(qrCodeData);
            var qrCodeImage = qrCode.GetGraphic(20, Color.Black, Color.White, (Bitmap)Bitmap.FromFile(CommonSetting.LogoPath));

            using (var outStream = new MemoryStream())
            {
                qrCodeImage.Save(outStream, System.Drawing.Imaging.ImageFormat.Png);
                qrCodeImage.Dispose();
                result = Guid.NewGuid().ToString() + ".png";
                isUploaded = _s3Uploader.UploadImages(outStream, result);
                var res = await UserProfile(token);
                if (isUploaded == true)
                {
                    var ress = await _walletUserRepository.GetCurrentUser(res.WalletUserId);
                    ress.QrCode = result;
                    if (ress.WalletUserId != null && ress.WalletUserId > 0)
                    {
                        await _walletUserRepository.UpdateUserDetail(ress);
                    }

                    qr.QrCodeUrl = CommonSetting.imageUrl + result;
                    qr.QrCodeImage = result;
                }
            }

            return qr;
        }

        public async Task<UserEmailVerifyResponse> VerfiyByEmailId(string token)
        {
            var result = new UserEmailVerifyResponse();
            result = await _walletUserRepository.VerfiyByEmailId(token);

            return result;


        }

        string RandomAlphaNumerals(int stringLength)
        {
            Random random = new Random();


            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, stringLength)
              .Select(s => s[random.Next(s.Length)]).ToArray());

        }

        //public async Task<bool> UpdateUserProfile(UpdateProfileRequest request)
        //{

        //    bool result = false;
        //    var AdminKeys = AES256.AdminKeyPair;
        //    bool IsEmailVerified = false;
        //    var adminKeyPair = AES256.AdminKeyPair;
        //    string EmailId = AES256.Encrypt(adminKeyPair.PublicKey, request.EmailId.Trim().ToLower());          
        //    var ExistingEmails = await _walletUserRepository.GetCurrentUserByEmailId(EmailId);
        //    var _UserProfile = await UserProfile();
        //    var _walletUser = await _walletUserRepository.GetCurrentUser(_UserProfile.WalletUserId);

        //    try
        //    {
        //        var user = await UserProfile();
        //        if (_walletUser.EmailId != EmailId)
        //        {
        //            if (_walletUser.IsEmailVerified == false)
        //            {
        //                if (ExistingEmails == null || ExistingEmails.EmailId != EmailId)
        //                {
        //                    _walletUser.EmailId = EmailId;
        //                    //  response.status = (int)UserProfileUpdated.EmailSend;
        //                    result = await _walletUserRepository.UpdateUserProfile(request, user.WalletUserId);
        //                    try
        //                    {
        //                        var _EmailVerification = new EmailVerification();
        //                        _EmailVerification.EmailId = _walletUser.EmailId;
        //                        _EmailVerification.CreatedDate = DateTime.UtcNow;
        //                        _EmailVerification.IsVerified = false;
        //                        _EmailVerification.VerificationDate = DateTime.UtcNow;
        //                        _EmailVerification.WalletUserId = 0;
        //                        _EmailVerification.IsMailSent = true;
        //                        _EmailVerification = await _walletUserRepository.InsertEmailVerification(_EmailVerification);
        //                        string uniqueToken = RandomAlphaNumerals(15) + "_" + _EmailVerification.EmailVerificationId.ToString();
        //                        string VerifyMailLink = CommonSetting.VerifyMailLink + "/" + HttpUtility.UrlEncode(uniqueToken);
        //                        string filename = CommonSetting.EmailVerificationTemplate;
        //                        var body = _sendEmails.ReadEmailformats(filename);
        //                        string Body = string.Format(body, VerifyMailLink);
        //                        var emailModel = new EmailModel
        //                        {
        //                            TO = request.EmailId,
        //                            Subject = ResponseMessages.USER_REGISTERED,//"Registered successfully",
        //                            Body = Body
        //                        };
        //                        _sendEmails.SendEmail(emailModel);
        //                    }
        //                    catch (Exception ex)
        //                    {

        //                    }
        //                }
        //                else
        //                {
        //                    result=false;
        //                }
        //            }
        //            else
        //            {
        //                result = false;
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        ex.Message.ErrorLog("WalletUserService.cs", "UpdateUserProfile", request);

        //        return false;
        //    }
        //    return result;
        //}


        public async Task<ChangePasswordResponse> ChangePassword(ChangePasswordRequest request, string TokenValue)
        {
            var response = new ChangePasswordResponse();
            try
            {
                // string TokenValue = GlobalData.Key;
                response = await _walletUserRepository.ChangePassword(request, TokenValue);
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("WalletUserService.cs", "ChangePassword", request);
            }
            return response;

        }

        public async Task<bool> ForgotPassword(ForgotPasswordRequest request)
        {
            var adminKeyPair = AES256.AdminKeyPair;
            var emailId = AES256.Encrypt(adminKeyPair.PublicKey, request.EmailId.Trim().ToLower());
            try
            {
                if (request.EmailId != null)
                {

                    string Otp = CommonSetting.TempPassword();
                    var result = await _walletUserRepository.ForgotPassword(request, Otp);
                    if (result == true)
                    {
                        string filename = CommonSetting.ForgotPasswordEmailTemplate;
                        var body = _sendEmails.ReadEmailformats(filename);
                        string Body = string.Format(body, Otp);

                        //EziPay Temporary Password
                        var emailModel = new EmailModel
                        {
                            TO = request.EmailId,
                            Subject = "EziPay Temporary Password | Reset Password",//"Registered successfully",
                            Body = Body
                        };
                        _sendEmails.SendEmail(emailModel);
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("AppUserRepository.cs", "ForgotPassword", request);
                if (ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message))
                {
                    ex.InnerException.Message.ErrorLog("WalletUserService.cs", "ForgotPassword", request);
                }
                return false;
            }
        }

        public async Task<CurrentBalanceResponse> FindCurrentBalance(string token)
        {
            var response = new CurrentBalanceResponse();
            try
            {
                //// var result = await _walletUserRepository.UserProfile(GlobalData.Key);
                var result = await _walletUserRepository.UserProfile(token);
                response.CurrentBalance = result.CurrentBalance;
                response.WalletUserId = result.WalletUserId;
                response.EarnedPoints = result.EarnedPoints.ToString();
                ////get the user id from whokm we get chargeback :- account block & debit
                //if (result.WalletUserId != 0)
                //{
                //    ChargeBackRequest request1 = new ChargeBackRequest();
                //    request1.Walletuserid = result.WalletUserId;
                //    var user_id = result.WalletUserId;

                //    var userChargeBackresult = await _ChargeBackRepository.GetChargeBackListById(request1);
                //    decimal CurrentBalance = decimal.Parse(response.CurrentBalance);

                //    if (userChargeBackresult.Count > 0)
                //    {
                //        decimal AmountLimit = decimal.Parse(userChargeBackresult[0].AmountLimit);
                //        var ChargeBacksubmitBy = userChargeBackresult[0].Createdby;
                //        if (AmountLimit <= CurrentBalance) 
                //        {
                //            UserManageRequest request2 = new UserManageRequest();
                //            request2.UserId = user_id;
                //            request2.IsActive = false;
                //            request2.Status = 1;
                //            request2.Flag = ChargeBacksubmitBy;

                //            await _userApiService.EnableDisableUser(request2);
                //        }
                //    }
                //}

            }
            catch (Exception ex)
            {
                // throw ex;
            }
            return response;
        }

        public async Task<IsFirstTransactionResponse> IsFirstTransaction(string token)
        {
            var response = new IsFirstTransactionResponse();
            try
            {
                ////var userData = await _walletUserRepository.UserProfile(GlobalData.Key);
                var userData = await _walletUserRepository.UserProfile(token);
                var result = await _walletUserRepository.IsFirstTransaction(userData.WalletUserId);
                response.SenderId = result.SenderId;
                response.WalletUserId = result.WalletUserId;
                response.ResponseId = result.ResponseId;
                response.CardPaymentRequestId = result.CardPaymentRequestId;
                if (result.SenderId > 0 || result.ResponseId > 0)
                {
                    response.IsFirstTransaction = true;
                }
                else
                {
                    response.IsFirstTransaction = false;
                }
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("AppUserRepository.cs", "IsFirstTransaction");
            }
            return response;

        }

        public async Task<UserDataResponse> GetUserDetailById(long WalletUserId)
        {
            var result = new UserDataResponse();

            string TokenValue = string.Empty;
            try
            {
                var resultData = await _walletUserRepository.GetUserDetailById(WalletUserId);
                result.UserName = resultData.FirstName + " " + resultData.LastName;
            }
            catch (Exception ex)
            {
                return null;
            }
            return result;

        }

        //public async Task<bool> SaveReferalData(UserReferalWallet userReferalWallet)
        //{
        //    bool response = false;
        //    var saveReq = new UserReferalWallet()
        //    {
        //        SenderId = userReferalWallet.SenderId,
        //        ReceiverId = userReferalWallet.ReceiverId,
        //        ReferUrl = referalUrl,
        //        ReceiverReferalPoints = "1",
        //        ReceiverReferalAmount = "1",
        //        SenderReferalPoint = "1",
        //        SenderReferalAmount = "1",
        //        CreatedDate = DateTime.UtcNow,
        //        UpdatedDate = DateTime.UtcNow
        //    };
        //    var resultData = await _walletUserRepository.SaveData(saveReq);
        //    if (resultData > 0)
        //    {
        //        response = true;
        //    }
        //    return response;
        //}


        public async Task<ProfileUpdateResponse> UpdateUserProfile(UpdateUserProfileRequest request, string token)
        {
            ProfileUpdateResponse response = new ProfileUpdateResponse();
            // int response = 0;
            // var ExistingEmails = "";
            bool IsEmailVerified = false;
            var adminKeyPair = AES256.AdminKeyPair;
            string EmailId = AES256.Encrypt(adminKeyPair.PublicKey, request.EmailId.Trim().ToLower());
            var AdminKeys = AES256.AdminKeyPair;
            try
            {
                ////var _UserProfile = await UserProfile();
                var _UserProfile = await UserProfile(token);
                var ExistingEmails = await _walletUserRepository.GetCurrentUserByEmailId(EmailId);
                //   var _walletUser = db.WalletUsers.Where(x => x.WalletUserId == _UserProfile.WalletUserId).FirstOrDefault();
                var _walletUser = await _walletUserRepository.GetCurrentUser(_UserProfile.WalletUserId);
                var Email = AES256.Decrypt(adminKeyPair.PrivateKey, _walletUser.EmailId).ToLower();
                if (_walletUser != null)
                {
                    if (!string.IsNullOrEmpty(request.FirstName))
                    {
                        _walletUser.FirstName = AES256.Encrypt(_walletUser.PublicKey, request.FirstName);
                        response.status = (int)UserProfileUpdated.ProfileUpdated;
                    }
                    if (!string.IsNullOrEmpty(request.LastName))
                    {
                        _walletUser.LastName = AES256.Encrypt(_walletUser.PublicKey, request.LastName);
                        response.status = (int)UserProfileUpdated.ProfileUpdated;
                    }
                    if (!string.IsNullOrEmpty(request.ProfileImage))
                    {
                        _walletUser.ProfileImage = request.ProfileImage;
                        response.status = (int)UserProfileUpdated.ProfileUpdated;
                    }
                    if (_walletUser.EmailId != EmailId)
                    {
                        if (_walletUser.IsEmailVerified == false)
                        {
                            if (ExistingEmails == null || ExistingEmails.EmailId != EmailId)
                            {
                                _walletUser.EmailId = EmailId;
                                response.status = (int)UserProfileUpdated.EmailSend;
                                try
                                {
                                    var _EmailVerification = new EmailVerification();
                                    _EmailVerification.EmailId = _walletUser.EmailId;
                                    _EmailVerification.CreatedDate = DateTime.UtcNow;
                                    _EmailVerification.IsVerified = false;
                                    _EmailVerification.VerificationDate = DateTime.UtcNow;
                                    _EmailVerification.WalletUserId = 0;
                                    _EmailVerification.IsMailSent = true;
                                    _EmailVerification = await _walletUserRepository.InsertEmailVerification(_EmailVerification);
                                    string uniqueToken = RandomAlphaNumerals(15) + "_" + _EmailVerification.EmailVerificationId.ToString();
                                    string VerifyMailLink = CommonSetting.VerifyMailLink + "/" + HttpUtility.UrlEncode(uniqueToken);
                                    string filename = CommonSetting.EmailVerificationTemplate;
                                    var body = _sendEmails.ReadEmailformats(filename);
                                    string Body = string.Format(body, VerifyMailLink);
                                    var emailModel = new EmailModel
                                    {
                                        TO = request.EmailId,
                                        Subject = ResponseMessages.USER_REGISTERED,//"Registered successfully",
                                        Body = Body
                                    };
                                    _sendEmails.SendEmail(emailModel);
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                            else
                            {
                                response.status = (int)UserProfileUpdated.EmailAlreadyExist;
                            }
                        }
                        else
                        {
                            response.status = (int)UserProfileUpdated.EmailAlreadyverified;
                        }
                    }
                    await _walletUserRepository.UpdateUserDetail(_walletUser);
                    return response;
                }
                else
                {
                    response.status = (int)UserProfileUpdated.Profile_Not_Updated;
                }
            }
            catch (Exception ex)
            {
                //ex.Message.ErrorLog("AppUserRepository.cs", "UpdateUserProfile", request);              
            }
            return response;
        }


        public async Task<bool> ShareQRCode(QRCodeRequest request, string token)
        {
            try
            {
                ////var user = await UserProfile();
                var user = await UserProfile(token);
                var result = await _walletUserRepository.GetCurrentUser(user.WalletUserId);

                string filename = CommonSetting.qrCodeShare;
                var body = _sendEmails.ReadEmailformats(filename);
                body = body.Replace("$$FirstName$$", user.FirstName + " " + user.LastName);
                body = body.Replace("$$DisplayContent$$", "QrCode");
                body = body.Replace("$$MobileNumber$$", "(" + user.StdCode + "," + user.MobileNo + ")");
                body = body.Replace("$$Qr$$", CommonSetting.imageUrl + result.QrCode);
                var req = new EmailModel
                {
                    Subject = "QR Code",
                    Body = body,
                    TO = request.EmailId
                };
                _sendEmails.SendEmail(req);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public async Task<AuthenticationResponse> Authentication(AuthenticationRequest request)
        {
            var response = new AuthenticationResponse();
            var adminKeyPair = AES256.AdminKeyPair;
            request.emailMobile = request.emailMobile.Trim().ToLower();
            request.emailMobile = AES256.Encrypt(adminKeyPair.PublicKey, request.emailMobile.ToLower());
            var user = await _walletUserRepository.Authentication(request);
            if ((user.UserType == 3 && user.DocumetStatus == 2) || user.UserType == 1 || user.UserType == 2)
            {
                if (user != null && (user.UserType == (int)WalletUserTypes.AppUser || user.UserType == (int)WalletUserTypes.Merchant))
                {
                    //if ((bool)user.IsEmailVerified)
                    //{
                    var hashedObject = SHA256ALGO.HashPasswordDecryption(request.password, user.HashedSalt);
                    if (hashedObject.HashedPassword == user.HashedPassword)
                    {
                        if ((bool)user.IsActive && (bool)user.IsDeleted == false)
                        {
                            response.walletUserId = user.WalletUserId;
                            response.isSuccess = true;
                            response.status = (int)LoginStatusType.SUCCESS;
                            response.RstKey = 6;
                            //if (user.IsFirstTimeUser == true)
                            //{
                            //    response.LoginType = (int)LoginTypes.Tutorial;
                            //}
                            //else if (user.IsTemporaryPassword == true)
                            //{
                            //    response.LoginType = (int)LoginTypes.ChangePassword;
                            //}
                            //else
                            //{
                            //    response.LoginType = (int)LoginTypes.Home;
                            //}
                        }
                        else
                        {
                            response.isSuccess = false;
                            response.status = (int)LoginStatusType.INACTIVE;
                        }
                    }
                    else
                    {

                        if (!string.IsNullOrEmpty(user.Otp) && user.OtpHashedSalt != null)
                        {
                            hashedObject = SHA256ALGO.HashPasswordDecryption(request.password, user.OtpHashedSalt);
                            if (hashedObject.HashedPassword == user.Otp)
                            {
                                if ((bool)user.IsActive)
                                {
                                    response.isSuccess = true;
                                    response.status = (int)LoginStatusType.SUCCESS;
                                    response.RstKey = 6;
                                    //  user.HashedPassword = hashedObject.HashedPassword;
                                    //  user.OtpHashedSalt = hashedObject.SlatBytes;
                                    user.IsTemporaryPassword = true;
                                    //  response.LoginType = (int)LoginTypes.ChangePassword;
                                }
                                else
                                {
                                    response.isSuccess = false;
                                    response.status = (int)LoginStatusType.INACTIVE;
                                    response.RstKey = 1;
                                }
                            }
                            else
                            {
                                response.isSuccess = false;
                                response.status = (int)LoginStatusType.FAILED;
                            }
                        }
                        else
                        {
                            response.isSuccess = false;
                            response.status = (int)LoginStatusType.FAILED;
                        }

                    }
                    if (response.isSuccess)
                    {
                        #region Map session related values
                        var token = new TokenRequest { DeviceUniqueId = request.merchantKey, WalletUserId = user.WalletUserId };
                        var sessionToken = await _tokenRepository.GenerateToken(token);
                        if (sessionToken.RstKey == 1)
                        {
                            try
                            {
                                var req = new SendPushRequest
                                {
                                    DeviceUniqueId = request.merchantKey,
                                    WalletUserId = user.WalletUserId,
                                    DeviceToken = user.DeviceToken,
                                    DeviceType = user.DeviceType,
                                    MobileNo = user.MobileNo
                                };

                                _sendPushNotification.SendLogoutPush(req, user.DeviceToken);
                            }
                            catch
                            {

                            }
                        }
                        response.privateKey = sessionToken.PrivateKey;
                        response.publicKey = sessionToken.PublicKey;

                        //string encryptKey = AES256.Encrypt2("4512631236589784", sessionToken.PublicKey);

                        //response.PrivateKey = encryptKey;
                        //response.PublicKey = encryptKey;

                        response.token = sessionToken.Token;

                        #endregion
                        if (string.IsNullOrWhiteSpace(user.QrCode))
                        {
                            //// GlobalData.Key = response.token;
                            var reqe = new QrCodeRequest
                            {
                                QrCode = user.StdCode + "," + AES256.Decrypt(adminKeyPair.PrivateKey, user.MobileNo)
                            };
                            var qrCode = await GenerateQrCode(reqe);

                            user.QrCode = qrCode.QrCodeImage;
                        }
                        #region Map user detail

                        response.currentBalance = user.CurrentBalance;

                        response.emailId = AES256.Decrypt(adminKeyPair.PrivateKey, user.EmailId);
                        response.mobileNo = AES256.Decrypt(adminKeyPair.PrivateKey, user.MobileNo);
                        response.stdCode = user.StdCode;
                        response.firstName = AES256.Decrypt(user.PrivateKey, user.FirstName);
                        response.lastName = AES256.Decrypt(user.PrivateKey, user.LastName);
                        #endregion

                        user.IsFirstTimeUser = false;

                        //if ((int)DeviceTypes.Web != request.deviceType)
                        //{
                        //    user.DeviceType = request.deviceType;
                        //    user.DeviceToken = request.DeviceToken;
                        //}
                        //else
                        //{
                        //    user.DeviceToken = request.SecretKey;
                        //}
                        await _walletUserRepository.UpdateUserDetail(user);
                    }
                    //}
                    //else
                    //{
                    //    response.Status = (int)LoginStatusType.EMAILNOTVERIFIED;
                    //}
                }
                else
                {
                    if (!(user.UserType == (int)WalletUserTypes.AppUser || user.UserType == (int)WalletUserTypes.Merchant))
                    {
                        response.status = (int)LoginStatusType.INVALID_USER_TYPE;
                        response.RstKey = 3;
                    }
                    //Email id not exist
                    else
                    {
                        response.status = (int)LoginStatusType.EMAILNOTEXIST;
                        response.RstKey = 4;
                    }
                }
            }
            else
            {
                response.status = (int)LoginStatusType.FAILED;
                response.RstKey = 21;
            }
            return response;
        }



        public async Task<int> SaveReferalPoints(long receiverId, long senderId, decimal ReceiverPoints, decimal SenderPoints, decimal ConversionPointsValue)
        {
            var isRefered = new UserReferalWallet
            {
                ReceiverId = receiverId,
                SenderId = Convert.ToInt32(senderId),
                ReceiverReferalAmount = Convert.ToString(ReceiverPoints / ConversionPointsValue),
                SenderReferalAmount = Convert.ToString(SenderPoints / ConversionPointsValue),
                ReceiverReferalPoints = ReceiverPoints.ToString(),
                SenderReferalPoint = SenderPoints.ToString(),
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };
            var receiverWallet = await _walletUserRepository.GetCurrentUser(receiverId);
            decimal receiverReferalBalance = Convert.ToDecimal(receiverWallet.EarnedPoints);
            var senderWallet = await _walletUserRepository.GetCurrentUser(senderId);
            decimal senderReferalBalance = Convert.ToDecimal(senderWallet.EarnedPoints);
            //Amount
            decimal receiverAmt = Convert.ToDecimal(isRefered.ReceiverReferalAmount);
            decimal senderAmt = Convert.ToDecimal(isRefered.SenderReferalAmount);
            //points
            decimal receiverPoint = Convert.ToDecimal(isRefered.ReceiverReferalPoints);
            decimal senderPoint = Convert.ToDecimal(isRefered.SenderReferalPoint);
            int s = await _walletUserRepository.SaveUserReferalData(isRefered);
            if (s > 0)
            {
                //Amount
                receiverWallet.EarnedAmount = receiverReferalBalance + receiverAmt;
                senderWallet.EarnedAmount = senderReferalBalance + senderAmt;
                //Points
                receiverWallet.EarnedPoints = receiverReferalBalance + receiverPoint;
                senderWallet.EarnedPoints = senderReferalBalance + senderPoint;

                var receiver = await _walletUserRepository.UpdateUserDetail(receiverWallet);
                var sender = await _walletUserRepository.UpdateUserDetail(senderWallet);
                if (receiver.EarnedAmount > 0 && sender.EarnedAmount > 0)
                {
                    await InsertRedeemHistory(senderAmt.ToString(), senderWallet.WalletUserId, receiverAmt.ToString(), receiverWallet.WalletUserId);
                }
            }
            return s;
        }



        public async Task<int> InsertRedeemHistory(string senderAmt, long senderId, string receiverAmt, long receiverId)
        {
            int result = 0;
            var senderReq = new RedeemPointsHistory
            {
                RedeemAmount = senderAmt.ToString(),
                WalletUserId = senderId,
                TransactionType = "CREDIT",
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
            };

            result = await _walletUserRepository.InsertEarnedHistory(senderReq);
            if (result > 0)
            {
                var receiverReq = new RedeemPointsHistory
                {
                    RedeemAmount = receiverAmt.ToString(),
                    WalletUserId = receiverId,
                    TransactionType = "CREDIT",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                };

                result = await _walletUserRepository.InsertEarnedHistory(receiverReq);
            }

            return result;
        }




        public async Task<bool> AutoEmailSentForPasswordExipry(QRCodeRequest request)
        {
            try
            {
                //var user = await UserProfile();
                //var result = await _walletUserRepository.GetCurrentUser(user.WalletUserId);

                //string filename = CommonSetting.qrCodeShare;
                //var body = _sendEmails.ReadEmailformats(filename);
                //body = body.Replace("$$FirstName$$", user.FirstName + " " + user.LastName);
                //body = body.Replace("$$DisplayContent$$", "QrCode");
                //body = body.Replace("$$MobileNumber$$", "(" + user.StdCode + "," + user.MobileNo + ")");
                //body = body.Replace("$$Qr$$", CommonSetting.imageUrl + result.QrCode);
                var req = new EmailModel
                {
                    Subject = "Password Expiry",
                    Body = "Change your password",
                    TO = request.EmailId
                };
                _sendEmails.SendEmail(req);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public UserDetailResponse GetUserProfileForTransaction(string tokenHeader)
        {
            // string tokenHeader = GlobalData.Key;//This value get from session filter
            string TokenValue = string.Empty;
            if (!string.IsNullOrEmpty(tokenHeader))
            {
                TokenValue = tokenHeader.ToString();
            }
            var response = new UserDetailResponse();

            try
            {
                if (tokenHeader != null)
                {
                    if (!string.IsNullOrEmpty(tokenHeader))
                    {
                        TokenValue = tokenHeader.ToString();
                    }

                    response = _walletUserRepository.GetUserProfileForTransaction(TokenValue);
                    response.DeviceToken = response.DeviceToken;
                    response.DeviceType = response.DeviceType;
                    response.WalletUserId = response.WalletUserId;

                    if (response.DocumetStatus == 2)
                    {
                        response.RstKey = 1;
                        response.DocStatus = true;
                    }
                    else
                    {
                        response.DocStatus = false;
                        response.RstKey = 2;
                    }
                    //else if (response.DocStatus && (response.DocumetStatus == (int)DocumentStatus.NoDocuments || response.DocumetStatus == (int)DocumentStatus.Pending || response.DocumetStatus == (int)DocumentStatus.Rejected || response.DocumetStatus == (int)DocumentStatus.NotOk))
                    //{
                    //    response.DocStatus = false;
                    //    response.RstKey = 2;
                    //}
                    //else
                    //{
                    //    response.DocStatus = true;
                    //    response.RstKey = 1;
                    //}
                }
                if (string.IsNullOrEmpty(TokenValue))
                {
                    var _currentUser = CommonMethods.GetWebCurrentUser();
                    if (_currentUser != null)
                    {
                        TokenValue = _currentUser.Token;
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return response;
        }
    }
}
