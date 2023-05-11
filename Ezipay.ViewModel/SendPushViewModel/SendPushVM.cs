using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.ViewModel.SendPushViewModel
{
    public class SendPushRequest
    {
        public int? DeviceType { get; set; }
        public string DeviceToken { get; set; }
        public string DeviceUniqueId { get; set; }
        public string MobileNo { get; set; }
        public long WalletUserId { get; set; }
        
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
    public class PushNotificationModel
    {
        public string message { get; set; }
        public string deviceKey { get; set; }
        public int deviceType { get; set; }
        public int PushType { get; set; }
        public object payload { get; set; }
        public long SenderId { get; set; }
    }
    public class PushPayload<T>
    {

        public string to { get; set; }
        public string collapse_key { get; set; }
        public PushPayloadData<T> data { get; set; }

    }
    public class PushPayloadData<T>
    {
        /// <summary>
        /// notification : Base model of json
        /// </summary>
        public T notification { get; set; }

    }
    public class NotificationJsonResponse<T>
    {
        public T aps { get; set; }

    }

    public class FcmPushResponse
    {
        public FcmPushResponse()
        {
            this.multicast_id = 0;
            this.success = 0;
            this.failure = 0;
            this.results = new List<FcmPushResult>();

        }
        public long multicast_id { get; set; }
        public int success { get; set; }
        public int failure { get; set; }
        public int canonical_ids { get; set; }
        public List<FcmPushResult> results { get; set; }
    }

    public class FcmPushResult
    {
        public FcmPushResult()
        {
            this.message_id = string.Empty;
        }
        public string message_id { get; set; }
    }
    public class WebPushNotificationModel
    {
        public string message { get; set; }
        public string deviceKey { get; set; }
        public int deviceType { get; set; }
        public int PushType { get; set; }
        public object payload { get; set; }
        public string SenderId { get; set; }
        public string RecieverId { get; set; }
        public string Token { get; set; }        
    }
    public class PayMoneyIOSPushModel : NotificationDefaultKeys
    {
        public PayMoneyIOSPushModel()
        {
            this.Amount = string.Empty;
            this.SenderName = string.Empty;
            this.MobileNo = string.Empty;
            this.CurrentBalance = string.Empty;
        }
        public string Amount { get; set; }
        public string SenderName { get; set; }
        public string MobileNo { get; set; }
        public string CurrentBalance { get; set; }
    }
    public class RejectPaymentRequestPushModel : NotificationDefaultKeys
    {
        public RejectPaymentRequestPushModel()
        {          
            this.Message = string.Empty;
        }        
        public string TransactionId { get; set; }       
        public string Message { get; set; }
        public string CurrentBalance { get; set; }
    }

    public class CreditDebitUpdateModel : NotificationDefaultKeys
    {
        public CreditDebitUpdateModel()
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
    }
}
