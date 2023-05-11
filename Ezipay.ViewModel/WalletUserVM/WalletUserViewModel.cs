using Ezipay.ViewModel.AdminViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.ViewModel.WalletUserVM
{

    public class UserSignupResponse : UserDetailResponse
    {
        public UserSignupResponse()
        {
            this.StatusCode = 0;
            this.IsEmailVerified = false;
            this.IsSuccess = false;
            this.Token = string.Empty;
            this.QrCodeUrl = string.Empty;
            this.LoginType = 0;
            this.Status = 0;
            this.IsNotificationOn = false;
        }
        public int StatusCode { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsSuccess { get; set; }
        public int Status { get; set; }
        public string Token { get; set; }
        public int LoginType { get; set; }
        public string QrCodeUrl { get; set; }
        public bool IsNotificationOn { get; set; }
        public int RstKey { get; set; }
        public string Message { get; set; }
        public object ReferalUrl { get; set; }
    }
    public class UpdateUserProfileRequest
    {
        public string ProfileImage { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }
    }
    public class QrCodeData
    {
        public string QrCodeUrl { get; set; }
        public string QrCodeImage { get; set; }
    }
    public class UserDetailResponse
    {
        public UserDetailResponse()
        {
            this.CurrentBalance = string.Empty;
            this.QrCode = string.Empty;
            this.EmailId = string.Empty;
            this.MobileNo = string.Empty;
            this.StdCode = string.Empty;
            this.FirstName = string.Empty;
            this.LastName = string.Empty;
            this.EmailId = string.Empty;
            this.Otp = string.Empty;
            this.QrCode = string.Empty;
            this.CurrentBalance = string.Empty;
            this.PrivateKey = string.Empty;
            this.PublicKey = string.Empty;
            this.IsEmailVerified = false;
            this.IsMobileNoVerified = false;
            this.ProfileImage = string.Empty;
            this.DeviceToken = string.Empty;
            this.DocumetStatus = 0;
            this.DocStatus = false;
            this.UserType1 = 0;
        }
        public long WalletUserId { get; set; }
        public string ProfileImage { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string StdCode { get; set; }
        public string MobileNo { get; set; }
        public string EmailId { get; set; }
        public string Otp { get; set; }
        public string QrCode { get; set; }
        public string CurrentBalance { get; set; }
        public decimal EarnedPoints { get; set; }
        public decimal EarnedAmount { get; set; }
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsMobileNoVerified { get; set; }
        public bool IsNotification { get; set; }
        public int DeviceType { get; set; }
        public string DeviceToken { get; set; }
        public int DocumetStatus { get; set; }
        public bool DocStatus { get; set; }
        public bool IsDisabledTransaction { get; set; }
        public int RstKey { get; set; }
        public long Id { get; set; }
        public string ApiKey { get; set; }
        public string MerchantKey { get; set; }
        public string PreImage { get; set; }
        public string ReferalUrl { get; set; }

        public string MobileNoISDcode { get; set; }//otp
        public bool IsActive { get; set; }
        public int UserType1 { get; set; }
    }

    public class UserSignupRequest
    {
        public UserSignupRequest()
        {
            this.FirstName = string.Empty;
            this.LastName = string.Empty;
            this.EmailId = string.Empty;
            this.IsdCode = string.Empty;
            this.MobileNo = string.Empty;
            this.Otp = string.Empty;
            this.Password = string.Empty;
            this.DeviceToken = string.Empty;
            this.DeviceType = 0;
            this.ProfileImage = string.Empty;
            this.DeviceUniqueId = string.Empty;
        }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [EmailAddress]
        public string EmailId { get; set; }
        [Required]
        public string IsdCode { get; set; }
        [Required]
        [Phone]
        public string MobileNo { get; set; }
        [Required]
        public string Otp { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string DeviceToken { get; set; }
        [Required]
        public int DeviceType { get; set; }
        public string ProfileImage { get; set; }
        [Required]
        public string DeviceUniqueId { get; set; }
        public long IsReferal { get; set; }
        public string Referaled { get; set; }
        public string ReferalDetails { get; set; }
    }


    //public class ChatModel : PayMoneyPushModel
    //{
    //    public ChatModel()
    //    {

    //        this.ReceiverId = string.Empty;
    //        this.SenderId = string.Empty;

    //    }
    //    public string ReceiverId { get; set; }
    //    public string SenderId { get; set; }

    //}
    public class PayMoneyPushModel : NotificationDefaultKeys
    {
        public PayMoneyPushModel()
        {
            this.Amount = string.Empty;
            this.SenderName = string.Empty;
            this.MobileNo = string.Empty;

            this.CurrentBalance = string.Empty;
            this.Message = string.Empty;
            this.AccountNo = string.Empty;

        }
        public string Amount { get; set; }
        public string SenderName { get; set; }
        public string MobileNo { get; set; }

        public string CurrentBalance { get; set; }
        public string Message { get; set; }
        public string AccountNo { get; set; }
        public int TransactionTypeInfo { get; set; }
        public string TransactionId { get; set; }
        public DateTime TransactionDate { get; set; }
    }
    public class NotificationDefaultKeys
    {

        public NotificationDefaultKeys()
        {
            this.alert = string.Empty;
            this.sound = "default";
            this.badge = this.pushType = 0;
        }

        public string alert { get; set; }
        public int badge { get; set; }
        public int pushType { get; set; }
        public string sound { get; set; }

    }

    public class UserExistanceResponse
    {
        public UserExistanceResponse()
        {
            this.Status = 0;
            this.Message = string.Empty;
        }
        public int Status { get; set; }
        public string Message { get; set; }
        public int RstKey { get; set; }
       

    }
    public class UserExistanceRequest
    {
        public UserExistanceRequest()
        {
            this.WalletUserId = 0;
            this.EmailId = string.Empty;
            this.MobileNo = string.Empty;
        }
        public long WalletUserId { get; set; }
        public string EmailId { get; set; }
        public string MobileNo { get; set; }
    }

    public class OtpRequest
    {
        public OtpRequest()
        {
            this.MobileNo = string.Empty;
        }
        [Required]
        [Phone]
        public string MobileNo { get; set; }
        [Required]
        public string IsdCode { get; set; }
    }

    public class SendOtpRequest
    {
        public SendOtpRequest()
        {
            this.MobileNo = string.Empty;
        }
        [Required]
        [Phone]
        public string MobileNo { get; set; }
        [Required]
        public string IsdCode { get; set; }
        [Required]
        public string Otp { get; set; }
    }
    public class SendOtpCallBackRequest
    {
        public SendOtpCallBackRequest()
        {
            this.MobileNo = string.Empty;
        }
        [Required]
        [Phone]
        public string MobileNo { get; set; }
        [Required]
        public string IsdCode { get; set; }       
    }

    public class VerifyOtpRequest : OtpCommonRequest
    {
        public VerifyOtpRequest()
        {
            this.MobileNo = string.Empty;
        }
        public string MobileNo { get; set; }
        [Required]
        public string IsdCode { get; set; }

    }
    public class OtpCommonRequest
    {
        public OtpCommonRequest()
        {
            this.Otp = string.Empty;
        }
        public string Otp { get; set; }
    }

    public class UserLoginResponse : UserDetailResponse
    {
        public UserLoginResponse()
        {
            this.IsSuccess = false;
            this.Token = string.Empty;
            this.PrivateKey = string.Empty;
            this.PublicKey = string.Empty;
            this.QrCode = string.Empty;
            this.QrCodeUrl = string.Empty;
            this.IsEmailVerified = false;
        }
        public bool IsSuccess { get; set; }
        public int Status { get; set; }
        public string Token { get; set; }
        //public string PrivateKey { get; set; }
        public int RstKey { get; set; }
        public int LoginType { get; set; }
        public string QrCodeUrl { get; set; }
        public bool IsNotificationOn { get; set; }
        public bool IsEmailVerified { get; set; }
        public int UserType { get; set; }
        public bool IsCheckedDoc { get; set; }
        public long WalletUserId { get; set; }
        public object ReferalUrl { get; set; }
    }


    public class AuthenticationRequest
    {
        public string emailMobile { get; set; }
        public string password { get; set; }
        public string deviceType { get; set; }
        public string merchantId { get; set; }
        public string merchantKey { get; set; }
        public string apiKey { get; set; }
    }
    public class AuthenticationResponse
    {
        public bool isSuccess { get; set; }
        public int status { get; set; }
        public int RstKey { get; set; }
        public string token { get; set; }
        public long walletUserId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string stdCode { get; set; }
        public string mobileNo { get; set; }
        public string emailId { get; set; }
        public string currentBalance { get; set; }
        public string privateKey { get; set; }
        public string publicKey { get; set; }
    }
    public class UserLoginRequest
    {
        public UserLoginRequest()
        {
            this.SecretKey = string.Empty;
            this.Password = string.Empty;
            this.DeviceUniqueId = string.Empty;
            this.DeviceToken = string.Empty;
            this.DeviceType = 0;

        }
        [Required]

        public string SecretKey { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string DeviceUniqueId { get; set; }
        [Required]
        public string DeviceToken { get; set; }
        [Required]
        public int DeviceType { get; set; }
    }

    public class WebUser
    {
        public WebUser()
        {
            this.UserId = 0;
            this.LastName = this.FirstName = this.Email = this.Token = string.Empty;
            this.CurrentBalance = "0";
            this.MobileNo = string.Empty;
            this.StdCode = string.Empty;
            this.ProfilePic = string.Empty;
            this.QRCodeUrl = string.Empty;
            this.IsPasswordChanged = false;
        }
        public long UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Token { get; set; }
        public string CurrentBalance { get; set; }
        public string MobileNo { get; set; }
        public string StdCode { get; set; }
        public string ProfilePic { get; set; }
        public string QRCodeUrl { get; set; }
        public bool IsPasswordChanged { get; set; }
        public int DocumetStatus { get; set; }
    }
    public class UserDetailByQrCodeResponse
    {
        public UserDetailByQrCodeResponse()
        {
            this.UserName = string.Empty;

        }
        public string UserName { get; set; }
        public int RstKey { get; set; }
    }
    public class UserDetailByQrCodeRequest
    {
        public UserDetailByQrCodeRequest()
        {
            this.MobileNo = string.Empty;
            this.QrCode = string.Empty;
        }
        public string QrCode { get; set; }
        public string MobileNo { get; set; }
    }
    public class DocumentUploadRequest
    {
        public DocumentUploadRequest()
        {
            this.ATMCard = string.Empty;
            this.IdCard = string.Empty;
            this.UserId = 0;
        }
        [Required]
        public string ATMCard { get; set; }
        [Required]
        public string IdCard { get; set; }
        [Required]
        public long UserId { get; set; }

        public long AdminId { get; set; } //log key
    }
    public class Documentresponse
    {
        public long WalletUserId { get; set; }
    }
    public class QrCodeRequest
    {
        public string QrCode { get; set; }
    }
    public class UpdateProfileRequest
    {
        public UpdateProfileRequest()
        {
            this.PreImage = string.Empty;
            this.FirstName = string.Empty;
            this.LastName = string.Empty;
            this.ProfileImage = string.Empty;
        }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PreImage { get; set; }
        public string ProfileImage { get; set; }
        public string EmailId { get; set; }
    }
    public class ForgotPasswordRequest
    {
        public ForgotPasswordRequest()
        {
            this.EmailId = string.Empty;
        }
        [Required]
        [EmailAddress]
        public string EmailId { get; set; }
    }

    public class CurrentBalanceResponse
    {
        public CurrentBalanceResponse()
        {
            this.CurrentBalance = "0";
        }
        public long WalletUserId { get; set; }
        public string CurrentBalance { get; set; }
        public string EarnedPoints { get; set; }
    }
    public class IsFirstTransactionResponse
    {
        public long? SenderId { get; set; }
        public long? WalletUserId { get; set; }
        public long? CardPaymentRequestId { get; set; }
        public long? ResponseId { get; set; }
        public bool IsFirstTransaction { get; set; }
    }
    public class UserDocumentResponse
    {
        public long WalletUserId { get; set; }
        public string IdProofImage { get; set; }
        public string CardImage { get; set; }
        public int DocumentStatus { get; set; }
        //public string MobileNo { get; set; }
        //public string AvailableBalance { get; set; }
    }
    public class UserDocumentRequest
    {
        public long WalletUserId { get; set; }
    }
    public class UserDataResponse
    {
        public string UserName { get; set; }
    }
    public class ShareAndEarnResponse
    {
        public string ShareLink { get; set; }
    }

    public class TagsData
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string WalletUserId { get; set; }
        public string MobileNo { get; set; }
    }
    public class ShareAndEarnRequest
    {
        public string branch_key { get; set; }
        public string channel { get; set; }
        public string feature { get; set; }
        public string campaign { get; set; }
        public string stage { get; set; }
        public string tags { get; set; }
        public Data data { get; set; }
    }
    public class Data
    {
        public string canonical_identifier { get; set; }
        public string og_title { get; set; }
        public string og_description { get; set; }
        public string og_image_url { get; set; }
        public string desktop_url { get; set; }
        public string android_url { get; set; }
        public bool custom_boolean { get; set; }
        public int custom_integer { get; set; }
        public string custom_string { get; set; }
        public int[] custom_array { get; set; }
        public Custom_Object custom_object { get; set; }
    }

    public class Custom_Object
    {
        public string random { get; set; }
    }

    public class CustomObject
    {
        public string random { get; set; }
    }

    public class DeepLinkData
    {
        public List<int> custom_array { get; set; }
        public bool custom_boolean { get; set; }
        public int custom_integer { get; set; }
        public string og_title { get; set; }
        public int creation_source { get; set; }
        public string custom_string { get; set; }
        public string url { get; set; }
        public CustomObject custom_object { get; set; }
        public List<string> tags { get; set; }
        public string desktop_url { get; set; }
        public string og_description { get; set; }
        public string id { get; set; }
        public bool one_time_use { get; set; }
        public string og_image_url { get; set; }
        public string canonical_identifier { get; set; }
    }

    public class DeepLinkResponse
    {
        public DeepLinkData data { get; set; }
        public int type { get; set; }
        public List<string> tags { get; set; }
    }

    public class RecentReceiverRequest
    {
        public long ServiceId { get; set; }
        public long SenderId { get; set; }
    }
    public class RecentReceiverResponse : CashdepositrequestRequest
    {
        public int WalletServiceId { get; set; }
        public long SenderId { get; set; }
        public long ReceiverId { get; set; }
        public string Name { get; set; }
        public string MobileNo { get; set; }
        public string ProfileImage { get; set; }
        public string ServiceName { get; set; }
        public string TotalAmount { get; set; }
        public string StdCode { get; set; }
        public string ImageUrl { get; set; }
        public string AccountNo { get; set; }
        public string BankCode { get; set; }
        public string BeneficiaryName { get; set; }
        public string CountryCode { get; set; }
        

    }

    public class ApiKeysResponse
    {
        public long Id { get; set; }
        public long WalletUserId { get; set; }
        public string ApiKey { get; set; }
        public string MerchantKey { get; set; }
    }
    public class ProfileUpdateResponse
    {
        public int status { get; set; }
    }


}
