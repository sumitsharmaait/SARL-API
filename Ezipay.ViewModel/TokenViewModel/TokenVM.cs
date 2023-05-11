using Ezipay.ViewModel.SendPushViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.ViewModel.TokenViewModel
{
    public class TokenRequest
    {
        public TokenRequest()
        {
            this.IsSuccess = false;
            this.WalletUserId = 0;
            this.DeviceUniqueId = string.Empty;
        }
        public bool IsSuccess { get; set; }
        public long WalletUserId { get; set; }
        public string DeviceUniqueId { get; set; }
    }
    /// <summary>
    /// To generate the autherization token based on temp token
    /// </summary>
    public class TokenResponse : TempTokenResponse
    {
        public TokenResponse()
        {
            this.TokenId = 0;
            this.WalletUserId = 0;
            this.PublicKey = string.Empty;
            this.PrivateKey = string.Empty;
            this.Token = string.Empty;
            this.IssuedOn = DateTime.UtcNow;
            this.ExpiresOn = DateTime.UtcNow;
        }
        public long TokenId { get; set; }
        public long WalletUserId { get; set; }
        public DateTime IssuedOn { get; set; }
        public DateTime ExpiresOn { get; set; }
        public int RstKey { get; set; }
    }
    /// <summary>
    /// Yo generate token based on mac address of the device
    /// </summary>
    public class TempTokenRequest
    {
        public TempTokenRequest()
        {
            this.DeviceUniqueId = string.Empty;
        }
        public string DeviceUniqueId { get; set; }
        public string DeviceToken { get; set; }
        public int DeviceType { get; set; }
        public bool IsFirstTimeLaunch { get; set; }
        public int AppType { get; set; }
    }
    public class UtilityRequest
    {

        public object Json { get; set; }
        public string Password { get; set; }

    }


    /// <summary>
    /// Yo generate token based on mac address of the device
    /// </summary>
    public class TempTokenResponse
    {
        public TempTokenResponse()
        {
            this.PublicKey = string.Empty;
            this.PrivateKey = string.Empty;
            this.Token = string.Empty;
        }
        public string Token { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }

    }
    public class SessionResponse : TempSessionResponse
    {

        public long WalletUserId { get; set; }
    }
    public class TempSessionResponse
    {
        public string Token { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        public string DeviceUniqueId { get; set; }

    }
    /// <summary>
    /// To check whether the token is valid or not
    /// </summary>
    public class ServiceAuthenticationRequest
    {
        public ServiceAuthenticationRequest()
        {
            this.Token = string.Empty;
            this.TokenExpiry = string.Empty;
            this.Type = 0;
        }
        public string Token { get; set; }
        public string TokenExpiry { get; set; }
        public int Type { get; set; }

    }

    //web logout
    public class ChatModel : PayMoneyPushModel
    {
        public ChatModel()
        {

            this.ReceiverId = string.Empty;
            this.SenderId = string.Empty;

        }
        public string ReceiverId { get; set; }
        public string SenderId { get; set; }

    }
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

    //public class NotificationDefaultKeys
    //{

    //    public NotificationDefaultKeys()
    //    {
    //        this.alert = string.Empty;
    //        this.sound = "default";
    //        this.badge = this.pushType = 0;
    //    }

    //    public string alert { get; set; }
    //    public int badge { get; set; }
    //    public int pushType { get; set; }
    //    public string sound { get; set; }

    //}
}
