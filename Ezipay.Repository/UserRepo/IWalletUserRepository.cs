using Ezipay.Database;
using Ezipay.ViewModel;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.AirtimeViewModel;
using Ezipay.ViewModel.SendEmailViewModel;
using Ezipay.ViewModel.WalletUserVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.UserRepo
{
    public interface IWalletUserRepository
    {
        /// <summary>
        /// SignUp
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<WalletUser> SignUp(WalletUser request);        
        Task<WalletUser> GetUserDetailById(long WalletUserId = 0);

        Task<string> IsUserMerchant(long WalletUserId = 0);
        Task<SessionInfoResponse> GetWalletSessionInfo(long walletUserId);
        Task<WalletUser> GetUserPushDetailById(string deviceKey, int deviceType);

        Task<UserExistanceResponse> CredentialsExistance(UserExistanceRequest request);
        Task<OtpResponse> SendOtp(SendOtpRequest request);

        Task<UserExistanceResponse> VerifyOtp(VerifyOtpRequest request);

        Task<WalletUser> Login(UserLoginRequest request);

        Task<WalletUser> UpdateUserDetail(WalletUser request);
        Task<bool> Logout(string token);

        Task<bool> IsDocVerified(long WalletUserId, int documetStatus);
        Task<bool> IsDocVerifiedMOMO(int documetStatus);

        Task<UserDetailResponse> UserProfile(string TokenValue);

        Task<WalletUser> GetCurrentUser(long walletId);

        Task<WalletUser> GetUserDetailByMobile(string mobileNumber);

        Task<UserDetailByQrCodeResponse> UserDetailById(UserDetailByQrCodeRequest request);
        Task<WalletUser> GetWalletUser(long walletUserId);
        Task<WalletUser> GetWalletUserByUserType(int userType,long walletUserId);
        Task<bool> DocumentUpload(DocumentUploadRequest request, long WalletUserId, string IdCard, string ATMCard);
        Task<UserEmailVerifyResponse> VerfiyByEmailId(string token);
        Task<EmailVerification> InsertEmailVerification(EmailVerification request);
        Task<bool> UpdateUserProfile(UpdateProfileRequest request, long WalletUserId);

        Task<ChangePasswordResponse> ChangePassword(ChangePasswordRequest request, string TokenValue);

        Task<bool> ForgotPassword(ForgotPasswordRequest request, string Otp);
        Task<IsFirstTransactionResponse> IsFirstTransaction(long userId);
        Task<int> InsertWalletTransaction(WalletTransaction transEntity);
        Task<int> SaveData(ShareAndEarnDetail shareAndEarnDetail);
        Task<ApiKeysResponse> GetApiKeysData(long walletUserId);
        Task<WalletUser> GetCurrentUserByEmailId(string emailid);
        Task<UserApiKey> GetMerchantApiKey(string apkiKey, string merchantKey);
        Task<UserExistanceResponse> CredentialsExistanceForMobileNumber(UserExistanceRequest request);
        Task<WalletUser> Authentication(AuthenticationRequest request);
        Task<ShareAndEarnDetail> GetReferalUrl(long request);
        Task<int> SaveUserReferalData(UserReferalWallet request);
        Task<int> InsertEarnedHistory(RedeemPointsHistory transEntity);
        Task<OneTimePassword> GetOtpForCallBack(SendOtpCallBackRequest request);

        Task<AddCashDepositToBankResponse> AddCashDepositToBankServices(AddCashDepositToBankRequest Request);

        UserDetailResponse GetUserProfileForTransaction(string TokenValue);

        Task<OtpResponse> WalletSendOtp(SendOtpRequest request, long userId);
        Task<UserExistanceResponse> WalletVerifyOtp(VerifyOtpRequest request, long userId);
        Task<List<MobileNoListResponse>> GetMobileNoList(long Walletuserid);
        Task<balance161022> GetUserbalancefreezeById(long WalletUserId = 0);

    }
}
