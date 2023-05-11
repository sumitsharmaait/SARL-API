using Ezipay.Database;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.SendEmailViewModel;
using Ezipay.ViewModel.WalletUserVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.UserService
{
    public interface IWalletUserService
    {
        /// <summary>
        /// SignUp
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<UserSignupResponse> SignUpWithEmail(UserSignupRequest request);
        Task<UserSignupResponse> SignUp(UserSignupRequest request);

        Task<OtpResponse> SendOtp(OtpRequest request, string sessionToken);
        Task<UserExistanceResponse> VerifyOtp(VerifyOtpRequest request);

        Task<UserLoginResponse> Login(UserLoginRequest request);

        Task<bool> Logout(string token);////

        ////Task<UserDetailResponse> UserProfile();
        Task<UserDetailResponse> UserProfile(string token);
        Task<UserDetailByQrCodeResponse> UserDetailById(UserDetailByQrCodeRequest request);
        Task<bool> DocumentUpload(DocumentUploadRequest request, string token);
        Task<QrCodeData> GenerateQrCode(QrCodeRequest request,string token=null);
        Task<UserEmailVerifyResponse> VerfiyByEmailId(string token);
        //Task<bool> UpdateUserProfile(UpdateProfileRequest request);
        Task<ChangePasswordResponse> ChangePassword(ChangePasswordRequest request,string TokenValue);
        Task<bool> ForgotPassword(ForgotPasswordRequest request);
        Task<CurrentBalanceResponse> FindCurrentBalance(string token); ////
        Task<IsFirstTransactionResponse> IsFirstTransaction(string token); ////
        Task<UserDataResponse> GetUserDetailById(long WalletUserId);
        Task<ProfileUpdateResponse> UpdateUserProfile(UpdateUserProfileRequest request, string token);
        Task<UserExistanceResponse> IsCredentialsExistance(UserExistanceRequest request);
        Task<bool> ShareQRCode(QRCodeRequest request, string token);
        Task<UserExistanceResponse> CredentialsExistanceForMobileNumber(UserExistanceRequest request);
        Task<AuthenticationResponse> Authentication(AuthenticationRequest request);
        Task<int> SaveReferalPoints(long receiverId, long senderId, decimal ReceiverPoints, decimal SenderPoints, decimal ConversionPointsValue);
        // Task<string> SendOtpTeleSign();
        Task<OtpResponse> CallBackOtp(OtpRequest request, string sessionToken);
        Task<bool> AutoEmailSentForPasswordExipry(QRCodeRequest request);

        UserDetailResponse GetUserProfileForTransaction(string tokenHeader);

    }
}
