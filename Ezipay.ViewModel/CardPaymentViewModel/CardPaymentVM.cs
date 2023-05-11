using Ezipay.ViewModel.MerchantPaymentViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Ezipay.ViewModel.CardPaymentViewModel
{
    public class AddMoneyTransavtionLimitResponse
    {
        public string userid { get; set; }
        public string TransactionLimitForAddMoney { get; set; }
    }
    public class CardAddMoneyResponse
    {
        public CardAddMoneyResponse()
        {
            this.Url = string.Empty;
            this.StatusCode = 0;
            this.Message = string.Empty;
            this.IsEmailVerified = false;
            this.DocStatus = false;
            this.RstKey = 0;
        }

        public string Url { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool DocStatus { get; set; }
        public string AmountIncedi { get; set; }
        public string Amount { get; set; }
        public string UniqueId { get; set; }
        public int RstKey { get; set; }

    }
    public class CardAddMoneyRequest
    {
        public CardAddMoneyRequest()
        {
            this.Amount = string.Empty;
            this.Password = string.Empty;
            this.IsAddDuringPay = false;
            this.CardNo = string.Empty;
        }
        public string Amount { get; set; }
        public string Password { get; set; }
        public bool IsAddDuringPay { get; set; }
        public string CardNo { get; set; }
        public PayMoneyContent PayMoneyContent { get; set; }
        public bool IsMerchant { get; set; }
        public MerchantTransactionRequest MerchantContent { get; set; }
    }

    public class PayMoneyContent
    {
        public string amount { get; set; }
        public string channel { get; set; }
        public string customer { get; set; }
        public string ISD { get; set; }
        public string Password { get; set; }
        public string invoiceNo { get; set; }
        public string Comment { get; set; }
        public string serviceCategory { get; set; }
        public int ServiceCategoryId { get; set; }
        public int chennelId { get; set; }
        public bool IsAddDuringPay { get; set; }
    }
    //public class MerchantTransactionRequest
    //{

    //    public long MerchantId { get; set; }
    //    [Required]
    //    public string Amount { get; set; }
    //    public string Comment { get; set; }
    //    [Required]
    //    public string Password { get; set; }
    //}

    public class TransactionHistoryAddMoneyReponse
    {
        public decimal totalAmount { get; set; }
        public long WalletUserId { get; set; }
    }
    public class CardPaymentWebRequest
    {
        public CardPaymentWebRequest()
        {
            string OrderInfo = Guid.NewGuid().ToString().Substring(0, 10);

            this.Title = "Card Payment";
            this.virtualPaymentClientURL = ConfigurationManager.AppSettings["PaymentUrl"];
            this.vpc_Version = ConfigurationManager.AppSettings["Version"];
            this.vpc_Command = ConfigurationManager.AppSettings["CardCommand"];
            this.vpc_AccessCode = ConfigurationManager.AppSettings["AccessCode"];

            this.vpc_Merchant = ConfigurationManager.AppSettings["MerchantId"];
            this.vpc_OrderInfo = "OI-" + OrderInfo;
            this.vpc_MerchTxnRef = "MTR-" + OrderInfo + "|" + DateTime.Now.ToString("ssfff");
            this.vpc_Amount = string.Empty;
            this.vpc_Locale = ConfigurationManager.AppSettings["Locale"];
            this.vpc_Currency = ConfigurationManager.AppSettings["Currency"];
            this.vpc_ReturnURL = ConfigurationManager.AppSettings["HostNameAdmin"] + ConfigurationManager.AppSettings["ResponseUrl"];
            this.vpc_TicketNo = "TN-" + OrderInfo;
            this.vpc_TxSourceSubType = ConfigurationManager.AppSettings["SourceSubType"];
            this.vpc_SecureHash = ConfigurationManager.AppSettings["SecureHash"];
            this.vpc_OperatorId = ConfigurationManager.AppSettings["OperatorId"];
            this.AgainLink = "";
            this.vpc_SecureHashType = ConfigurationManager.AppSettings["SecureHashType"];
            this.vpc_Gateway = ConfigurationManager.AppSettings["Gateway"];
        }
        public string Title { get; set; }
        public string virtualPaymentClientURL { get; set; }
        public string vpc_Version { get; set; }
        public string vpc_Command { get; set; }
        public string vpc_AccessCode { get; set; }
        public string vpc_MerchTxnRef { get; set; }
        public string vpc_Merchant { get; set; }
        public string vpc_OrderInfo { get; set; }
        public string vpc_Amount { get; set; }
        public string vpc_OperatorId { get; set; }
        public string vpc_Locale { get; set; }
        public string vpc_Currency { get; set; }
        public string vpc_ReturnURL { get; set; }
        public string vpc_TicketNo { get; set; }
        public string vpc_TxSourceSubType { get; set; }
        public string AgainLink { get; set; }
        public string vpc_SecureHash { get; set; }
        public string vpc_SecureHashType { get; set; }
        public string vpc_Gateway { get; set; }
    }

    public class CardPaymentWebResponse
    {
        public CardPaymentWebResponse()
        {
            this.Title = string.Empty;
            this.vpc_AVSResultCode = string.Empty;
            this.vpc_AcqAVSRespCode = string.Empty;
            this.vpc_AcqCSCRespCode = string.Empty;
            this.vpc_AcqResponseCode = string.Empty;
            this.vpc_Amount = string.Empty;
            this.vpc_AuthorizeId = string.Empty;
            this.vpc_BatchNo = string.Empty;
            this.vpc_CSCResultCode = string.Empty;
            this.vpc_Card = string.Empty;
            this.vpc_Command = string.Empty;
            this.vpc_Currency = string.Empty;
            this.vpc_Locale = string.Empty;
            this.vpc_MerchTxnRef = string.Empty;
            this.vpc_Merchant = string.Empty;
            this.vpc_Message = string.Empty;
            this.vpc_OrderInfo = string.Empty;
            this.vpc_ReceiptNo = string.Empty;
            this.vpc_SecureHash = string.Empty;
            this.vpc_TransactionNo = string.Empty;
            this.vpc_TxnResponseCode = string.Empty;
            this.vpc_Version = string.Empty;
            this.vpc_VerType = string.Empty;
            this.vpc_VerStatus = string.Empty;
            this.vpc_VerToken = string.Empty;
            this.vpc_VerSecurityLevel = string.Empty;
            this.vpc_3DSenrolled = string.Empty;
            this.vpc_3DSXID = string.Empty;
            this.vpc_3DSECI = string.Empty;
            this.vpc_3DSstatus = string.Empty;
            this.vpc_hashValidated = string.Empty;
            this.vpc_StatusCodeDescription = string.Empty;
            this.vpc_ResponseCodeDescription = string.Empty;
        }
        public string Title { get; set; }
        public string vpc_AVSResultCode { get; set; }
        public string vpc_AcqAVSRespCode { get; set; }
        public string vpc_AcqCSCRespCode { get; set; }
        public string vpc_AcqResponseCode { get; set; }
        public string vpc_Amount { get; set; }
        public string vpc_AuthorizeId { get; set; }
        public string vpc_BatchNo { get; set; }
        public string vpc_CSCResultCode { get; set; }
        public string vpc_Card { get; set; }
        public string vpc_Command { get; set; }
        public string vpc_Currency { get; set; }
        public string vpc_Locale { get; set; }
        public string vpc_MerchTxnRef { get; set; }
        public string vpc_Merchant { get; set; }
        public string vpc_Message { get; set; }
        public string vpc_OrderInfo { get; set; }
        public string vpc_ReceiptNo { get; set; }
        public string vpc_SecureHash { get; set; }
        public string vpc_TransactionNo { get; set; }
        public string vpc_TxnResponseCode { get; set; }
        public string vpc_Version { get; set; }
        public string vpc_VerType { get; set; }
        public string vpc_VerStatus { get; set; }
        public string vpc_VerToken { get; set; }
        public string vpc_VerSecurityLevel { get; set; }
        public string vpc_3DSenrolled { get; set; }
        public string vpc_3DSXID { get; set; }
        public string vpc_3DSECI { get; set; }
        public string vpc_3DSstatus { get; set; }
        public string vpc_hashValidated { get; set; }
        public string vpc_ResponseCodeDescription { get; set; }
        public string vpc_StatusCodeDescription { get; set; }
        public string refNo { get; set; }
        public string transactionID { get; set; }
        public string status { get; set; }
        public string bnk { get; set; }

    }

    public class PaymentConfirmationModelOther
    {
        public PaymentConfirmationModelOther()
        {
            this.StatusCode = string.Empty;
            this.Amount = string.Empty;
            this.MobileNo = string.Empty;
            this.Message = string.Empty;
            this.TransactionId = string.Empty;
            this.InvoiceNo = string.Empty;
            this.CurrentBalance = string.Empty;
            this.Status = 0;
            this.PaymentTransactionNo = string.Empty;
            this.ToMobileNo = string.Empty;
            this.TransactionAmount = string.Empty;
            this.CurrentBalance = string.Empty;
            this.TransactionResponseDescription = string.Empty;
            this.TransactionResponseCode = string.Empty;

            this.IsAddDuringPay = false;
        }
        public int Status { get; set; }
        public string PaymentTransactionNo { get; set; }
        public string ToMobileNo { get; set; }
        public string TransactionAmount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string CurrentBalance { get; set; }
        public string TransactionResponseDescription { get; set; }
        public string TransactionResponseCode { get; set; }
        public bool IsAddDuringPay { get; set; }
        public bool IsMerchant { get; set; }
        public int MerchantStatusCode { get; set; }
        public string FormatedTransactionDate { get; set; }
        public string MobileNo { get; set; }
        public string Amount { get; set; }
        public string StatusCode { get; set; }
        public string Message { get; set; }
        public string TransactionId { get; set; }
        public string InvoiceNo { get; set; }

        public string AccountNo { get; set; }
    }

    public class CardPaymentSaveResponse
    {
        public CardPaymentSaveResponse()
        {
            this.PaymentTransactionNo = string.Empty;
            this.TransactionRefId = 0;
            this.ToMobileNo = string.Empty;
            this.TransactionAmount = string.Empty;
            this.CurrentBalance = string.Empty;
            this.TransactionResponseDescription = string.Empty;
            this.TransactionResponseCode = string.Empty;
            this.AddDuringPayResponse = null;
            this.IsAddDuringPay = false;

        }
        public long TransactionRefId { get; set; }
        public string PaymentTransactionNo { get; set; }
        public string ToMobileNo { get; set; }
        public string TransactionAmount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string CurrentBalance { get; set; }
        public string TransactionResponseDescription { get; set; }
        public string TransactionResponseCode { get; set; }
        public bool IsAddDuringPay { get; set; }
        public AddDuringPayResponse AddDuringPayResponse { get; set; }

    }
    public class AddDuringPayResponse
    {
        public AddDuringPayResponse()
        {
            this.StatusCode = string.Empty;
            this.Amount = string.Empty;
            this.MobileNo = string.Empty;
            this.Message = string.Empty;
            this.TransactionId = string.Empty;
            this.InvoiceNo = string.Empty;
            this.CurrentBalance = string.Empty;
        }
        public DateTime TransactionDate { get; set; }
        public string FormatedTransactionDate { get; set; }
        public string MobileNo { get; set; }
        public string Amount { get; set; }
        public string StatusCode { get; set; }
        public string Message { get; set; }
        public string TransactionId { get; set; }
        public string InvoiceNo { get; set; }
        public string CurrentBalance { get; set; }
        public string AccountNo { get; set; }
        public bool IsMerchant { get; set; }
        public int MerchantStatusCode { get; set; }
    }
    public class AddCardMoneyResponse
    {
        public AddCardMoneyResponse()
        {
            this.TransactionId = string.Empty;
            this.StatusCode = 0;
            this.Message = string.Empty;
            this.CurrentBalance = string.Empty;
            this.TransactionDate = DateTime.Now;
        }
        public string TransactionId { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public string CurrentBalance { get; set; }
        public DateTime TransactionDate { get; set; }
        public string ToMobileNo { get; set; }
        public string TransactionAmount { get; set; }
    }
    public class CardPaymentUBARequest
    {
        public string merchantId { get; set; }
        public string description { get; set; }
        public string total { get; set; }
        public string date { get; set; }
        public string countryCurrencyCode { get; set; }
        public string noOfItems { get; set; }
        public string customerFirstName { get; set; }
        public string customerLastname { get; set; }
        public string customerEmail { get; set; }
        public string customerPhoneNumber { get; set; }
        public string referenceNumber { get; set; }
        public string serviceKey { get; set; }
    }
    public class Transaction
    {
        public string id { get; set; }
    }

    public class Registration
    {
        public Transaction transaction { get; set; }
    }

    public class UBACardPaymentResponse
    {
        public Registration registration { get; set; }
    }


    public class MasterCardPaymentResponse
    {
        public string resultIndicator { get; set; }
        public string sessionVersion { get; set; }
    }

    public class MasterCardUBAPaymentRequest
    {
        public string Amount { get; set; }
    }

    public class GetSeerbitauthTokenbykeyRequest
    {
        public string key { get; set; }
    }
    public class GetSeerbitauthTokenbykeyResponse
    {
        public string status { get; set; }
        public data1 data { get; set; }


    }
    public class data1
    {
        public string code { get; set; }
        public EncryptedSecKey EncryptedSecKey { get; set; }
        public string message { get; set; }

        public payments payments { get; set; }
    }

    public class EncryptedSecKey
    {
        public string encryptedKey { get; set; }
    }

    public class payments
    {
        public string redirectLink { get; set; }
    }
    public partial class MasterCardPaymentUBAResponse
    {
        public string sessioanData { get; set; }
        public string SessionId { get; set; }
        public string Version { get; set; }
        public string SuccessIndicator { get; set; }
        public string Merchant { get; set; }
        public string InvoiceNumber { get; set; }
        public int RstKey { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public string URL { get; set; }
    }

    public class ThirdpartyPaymentByCardRequest
    {
        public ThirdpartyPaymentByCardRequest()
        {
            this.Amount = string.Empty;
            this.Password = string.Empty;

        }
        public string CardNo { get; set; }
        public string serviceCategory { get; set; }
        public string BeneficiaryName { get; set; }
        public int ServiceCategoryId { get; set; }
        public int chennelId { get; set; }
        public string Amount { get; set; }
        public string channel { get; set; }
        public string customer { get; set; }
        public string ISD { get; set; }
        public string AmountInUsd { get; set; }
        public string Password { get; set; }
        public string invoiceNo { get; set; }
        public string Comment { get; set; }
        public string DisplayContent { get; set; }
        public bool IsMerchant { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsDisabledTransaction { get; set; }
        public bool IsdocVerified { get; set; }
        public long WalletUserId { get; set; }
        public string MobileNo { get; set; }
        public string MerchantId { get; set; }
        public string Name { get; set; }
        public string Product_Id { get; set; }
        public string AmountInLocalCountry { get; set; }
        public int ServiceCategoryIdFrom { get; set; }
        public string MobileNoFrom { get; set; }
        public string channelFrom { get; set; }
        public string VoucherCode { get; set; }

        public string ngnbank { get; set; }
        public string accountNo { get; set; }
        public string zenithdob { get; set; } //userbankdob
        public string bvn { get; set; }

    }
    public class MasterCardPaymentSaveResponse
    {
        public MasterCardPaymentSaveResponse()
        {
            this.PaymentTransactionNo = string.Empty;
            this.TransactionRefId = 0;
            this.ToMobileNo = string.Empty;
            this.TransactionAmount = string.Empty;
            this.CurrentBalance = string.Empty;
            this.TransactionResponseDescription = string.Empty;
            this.TransactionResponseCode = string.Empty;
            this.AddDuringPayResponse = null;
            this.IsAddDuringPay = false;
            this.IsMasterCard = true;
        }
        public long TransactionRefId { get; set; }
        public string PaymentTransactionNo { get; set; }
        public string TransactionId { get; set; }
        public string ToMobileNo { get; set; }
        public string TransactionAmount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string CurrentBalance { get; set; }
        public string TransactionResponseDescription { get; set; }
        public string TransactionResponseCode { get; set; }
        public bool IsAddDuringPay { get; set; }
        public bool IsMasterCard { get; set; }
        public AddDuringPayResponse AddDuringPayResponse { get; set; }
        public long WalletUserId { get; set; }
        public int ServiceCategoryId { get; set; }
        public string Amount { get; set; }
        public string ISD { get; set; }
        public string customer { get; set; }
        public string Comment { get; set; }
        public string BeneficiaryName { get; set; }
        public string channel { get; set; }
        public string headerToken { get; set; }
        public int RstKey { get; set; }
        public string transactionBy { get; set; }
        public string cardTransactionId { get; set; }
        public string DisplayContent { get; set; }
        public string AmountInUsd { get; set; }
        public string AmountInLocalCountry { get; set; }
        public string MerchantId { get; set; }
        public string ProductId { get; set; }
    }

    public class CheckoutSessionModel
    {
        public string Id { get; set; }
        public string Version { get; set; }
        public string SuccessIndicator { get; set; }
        public string merchant { get; set; }
        public static CheckoutSessionModel toCheckoutSessionModel(string response)
        {
            //JObject jObject = JObject.Parse(response.);
            //CheckoutSessionModel model = jObject["session"].ToObject<CheckoutSessionModel>();
            //model.SuccessIndicator = jObject["successIndicator"] != null ? jObject["successIndicator"].ToString() : "";
            //return model;
            CheckoutSessionModel model = new CheckoutSessionModel();
            string value = String.Empty;
            List<string> keyValuePairs = response.Split('&').ToList();

            foreach (var keyValuePair in keyValuePairs)
            {
                string key = keyValuePair.Split('=')[0].Trim();
                if (key == "session.id")
                {
                    model.Id = keyValuePair.Split('=')[1];
                }
                else if (key == "session.version")
                {
                    model.Version = keyValuePair.Split('=')[1];
                }
                else if (key == "successIndicator")
                {
                    model.SuccessIndicator = keyValuePair.Split('=')[1];
                }
            }
            return model;
        }
    }
    public class SeerbitResponse
    {
        public string code { get; set; }
        public string message { get; set; }
        public string reference { get; set; }
        public string linkingreference { get; set; }

        public string InvoiceNumber { get; set; }
        public int RstKey { get; set; }
        public string redirectLink { get; set; }
        public string status { get; set; }
        public string paymentStatus { get; set; }
        public int StatusCode { get; set; }
        public string Url { get; set; }

        public string InvoiceNo { get; set; }
        public DateTime TransactionDate { get; set; }

        public string Amount { get; set; }
        public string ToMobileNo { get; set; }
        public string CurrentBalance { get; set; }
    }

    public class SeerbitRequest
    {
        public string code { get; set; }
        public string message { get; set; }
        public string reference { get; set; }
        public string linkingreference { get; set; }

        public string InvoiceNumber { get; set; }
        public int RstKey { get; set; }
    }
    public class SeerbitRequest1
    {
        public string hash { get; set; }
        public string hashType { get; set; }

        public string amount { get; set; }
        public string callbackUrl { get; set; }
        public string country { get; set; }
        public string currency { get; set; }
        public string email { get; set; }
        public string paymentReference { get; set; }

        public string productDescription { get; set; }
        public string productId { get; set; }
        public string publicKey { get; set; }
    }


    public class GTBCIVRequest
    {
        public string Version { get; set; }
        public string OrderID { get; set; }
        public int PurchaseAmount { get; set; }
        public int PurchaseCurrency { get; set; }
        public string CaptureFlag { get; set; }
        public string Signature { get; set; }
        public string SignatureMethod { get; set; }

        public string InvoiceNumber { get; set; }
        public int RstKey { get; set; }
    }

    //public class GTBCIVRequestHashRequest
    //{
    //    public GTBCIVRequestHashRequest()
    //    {
    //        this.merchantID = string.Empty;
    //        this.acquirerID = string.Empty;
    //        this.orderID = string.Empty;
    //        this.formattedPurchaseAmt = string.Empty;
    //        this.currency = string.Empty;
    //    }
    //    public string merchantID { get; set; }
    //    public string acquirerID { get; set; }
    //    public string orderID { get; set; }
    //    public string formattedPurchaseAmt { get; set; }
    //    public string currency { get; set; }


    //}

    public class GTBCIVRequestHashRequest
    {
        public GTBCIVRequestHashRequest()
        {
            this.orderID = string.Empty;

            this.merchantID = string.Empty;
            this.acquirerID = string.Empty;
            this.formattedPurchaseAmt = string.Empty;
            this.currency = string.Empty;


        }
        public string orderID { get; set; }

        public string merchantID { get; set; }
        public string acquirerID { get; set; }
        public string formattedPurchaseAmt { get; set; }
        public string currency { get; set; }




    }


    public class GTBCIVUrlPaymentResponse
    {

        public string OrderID { get; set; }

        public string Signature { get; set; }
        public string ResponseCode { get; set; }

        public string ReasonCode { get; set; }

        public string ReasonCodeDesc { get; set; }


        public string MerID { get; set; }
        public string AcqID { get; set; }

        public string ReferenceNo { get; set; }



    }
    public class TransactionInfoResponse
    {
        public int responseCode { get; set; }
        public int reasonCode { get; set; }
        public string reasonCodeDesc { get; set; }
        public string accessKey { get; set; }
        public string urlPayment { get; set; }
    }

    public class NgeniunstokenRequest
    {
        public string realmName { get; set; }
    }
    public class NgeniunstokenResponse
    {

        public string access_token { get; set; }

    }
    public class NgeniunsResponse
    {
        public string URL { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool DocStatus { get; set; }
        public string message { get; set; }
        public int RstKey { get; set; }
        public _links _links { get; set; }
        public string reference { get; set; }
        public string SuccessIndicator { get; set; }
    }
    public class authResponse
    {

        // "string":{"authorizationCode":"AB0012","success":true,"resultCode":"00","resultMessage":"Successful approval/completion or that VIP PIN verification is valid","mid":"10052283000","rrn":"000000123456"}
        public string authorizationCode { get; set; }
        public Boolean success { get; set; }
        public string resultCode { get; set; }
        public string resultMessage { get; set; }
        public string mid { get; set; }
        public string rrn { get; set; }

    }
    public class _links
    {
        public payment payment { get; set; }


    }
    public class payment
    {
        public string href { get; set; }


    }
    public class merchantAttributes
    {
        public string merchantOrderReference { get; set; }

        public string cancelUrl { get; set; }
        public string redirectUrl { get; set; }
        public Boolean skipConfirmationPage { get; set; }
        public Boolean skip3DS { get; set; }
    }
    public class amount
    {
        public string currencyCode { get; set; }
        public string value { get; set; }

    }
    public class NgeniunsRequest
    {
        public merchantAttributes merchantAttributes { get; set; }
        public amount amount { get; set; }
        public string emailAddress { get; set; }
        public string action { get; set; }


    }

    public class customer
    {
        public string email { get; set; }


    }
    public class flutterRequest
    {
        public customer customer { get; set; }
        public string currency { get; set; }
        public string tx_ref { get; set; }
        public string amount { get; set; }
        public string redirect_url { get; set; }
        public string payment_options { get; set; }


    }
    public class fluttercallbackResponse
    {
        public string status { get; set; }
        public string tx_ref { get; set; }
        public string transaction_id { get; set; }
        public string flag { get; set; }
    }
    public class fluttercallbackResponsewebhook
    {
        public string status { get; set; }
        public string txRef { get; set; }

        public string @event { get; set; }
        public string flag { get; set; }
        public string id { get; set; }
    }

    public class flutterPaymentUrlResponse
    {
        public string status { get; set; }
        public string message { get; set; }
        public int RstKey { get; set; }
        public data data { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string URL { get; set; }
    }

    public class merchantPaymentUrlResponse
    {
        public string status { get; set; }
        public string message { get; set; }
        public int RstKey { get; set; }
       
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string URL { get; set; }
        public string link { get; set; }
    }
    public class data
    {
        public string link { get; set; }
    }



    public class binancePaymentUrlResponse
    {
        public string status { get; set; }
        public string code { get; set; }
        public string errorMessage { get; set; }

        public databi data { get; set; }
        public int RstKey { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string URL { get; set; }
    }

    public class databi
    {
        public string status { get; set; }
        public string code { get; set; }

    }

    public class transferDetailList
    {
        public transferDetailList()
        {
            this.merchantSendId = string.Empty;
            this.transferAmount = 0;
            this.receiveType = string.Empty;
            this.transferMethod = string.Empty;
            this.receiver = string.Empty;
        }
        public string merchantSendId { get; set; }
        public decimal transferAmount { get; set; }
        public string receiveType { get; set; }
        public string transferMethod { get; set; }
        public string receiver { get; set; }

    }
    public class binanceaddRequest
    {
        public binanceaddRequest()
        {
            transferDetailList = new List<transferDetailList>();

        }

        public string requestId { get; set; }
        public string batchName { get; set; }
        public string currency { get; set; }
        public decimal totalAmount { get; set; }
        public int totalNumber { get; set; }

        public string bizScene { get; set; }
        public List<transferDetailList> transferDetailList { get; set; }
    }

    public class binancewalletRequest
    {

        public string requestId { get; set; }

        public string currency { get; set; }
        public string amount { get; set; }


        public string transferType { get; set; }

    }
    public class binancewalletResponse
    {
        public string status { get; set; }
        public string code { get; set; }
        public string errorMessage { get; set; }

        public databinancewallet data { get; set; }
        public int RstKey { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string URL { get; set; }
    }

    public class databinancewallet
    {
        public string tranId { get; set; }
        public string amount { get; set; }
        public string status { get; set; }
        public string currency { get; set; }
    }

    public class FXKUDIcallbackResponse
    {
        public string status { get; set; }
        public string tx_ref { get; set; }


    }

    public class FXKUDIPaymentUrlResponse
    {
        public string status { get; set; }
        public string message { get; set; }
        public string amount { get; set; }
        public string currency { get; set; }
        public int RstKey { get; set; }

        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string URL { get; set; }

        public string account_name { get; set; }
        public string account_number { get; set; }
        public string bank_name { get; set; }
        public string payment_instruction { get; set; }
        public string reference { get; set; }
        public string expiry_date { get; set; }
    }
    public class FXKUDIRequest
    {
        public string merchantid { get; set; }
        public string publickey { get; set; }
        public string amount { get; set; }
        public string currency { get; set; }
        public int RstKey { get; set; }

        public int StatusCode { get; set; }
        public string customer_name { get; set; }
        public string customer_email { get; set; }

        public string customer_phone { get; set; }
        public string callback_url { get; set; }
        public string reference { get; set; }

    }

    //webhook
    public class FlutterCardPaymentWebResponse
    {
        public string @event { get; set; }
        public f_data data { get; set; }
    }
    public class f_data
    {
        public int id { get; set; }
        public string tx_ref { get; set; }

        public string flw_ref { get; set; }

        public string device_fingerprint { get; set; }

        public int amount { get; set; }
        public string currency { get; set; }
        public int charged_amount { get; set; }

        public decimal app_fee { get; set; }
        public int merchant_fee { get; set; }
        public string processor_response { get; set; }


        public string auth_model { get; set; }

        public string ip { get; set; }
        public string narration { get; set; }
        public string status { get; set; }
        public string debit_currency { get; set; }
        public string reference { get; set; }
        
        
        public decimal payment_type { get; set; }
        public DateTime created_at { get; set; }
        public int account_id { get; set; }
        public f_customer customer { get; set; }
        public f_card card { get; set; }
    }
    public class f_card
    {
        public string first_6digits { get; set; }
        public string last_4digits { get; set; }
        public string issuer { get; set; }
        public string country { get; set; }
        public string type { get; set; }
        public string expiry { get; set; }
    }

    public class f_customer
    {
        public int id { get; set; }
        public string name { get; set; }
        public string phone_number { get; set; }

        public string email { get; set; }
        public DateTime created_at { get; set; }

    }
    public class flutterbankRequest
    {
        public string email { get; set; }
        public string currency { get; set; }
        public string account_bank { get; set; }
        public string account_number { get; set; }
        public string amount { get; set; }
        public string passcode { get; set; } //userbankdob
        public string bvn { get; set; }
        public string tx_ref { get; set; }
        public string redirect_url { get; set; }
        public string fullname { get; set; }
    }
    public class flutterbankResponse
    {

        public string status { get; set; }
        public string message { get; set; }
        public int RstKey { get; set; }
        public dataauth_url data { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string URL { get; set; }
        public string flw_ref { get; set; }
        public string type { get; set; }
        public string processor_response { get; set; }


    }

    public class dataauth_url
    {
        public string auth_url { get; set; }
        public string flw_ref { get; set; }
        public string type { get; set; }
        public string processor_response { get; set; }

    }

    //bank flutter


    public class BankPaymentWebResponse
    {

        public int id { get; set; }
        public string txRef { get; set; }
        public string orderRef { get; set; }
        public string flwRef { get; set; }
        public string redirectUrl { get; set; }
        public string device_fingerprint { get; set; }
        public string settlement_token { get; set; }
        public string cycle { get; set; }
        public string amount { get; set; }
        public string charged_amount { get; set; }
        public string appfee { get; set; }
        public string merchantfee { get; set; }
        public string merchantbearsfee { get; set; }
        public string chargeResponseCode { get; set; }
        public string raveRef { get; set; }
        public string chargeResponseMessage { get; set; }
        public string authModelUsed { get; set; }
        public string currency { get; set; }
        public string IP { get; set; }
        public string narration { get; set; }
        public string status { get; set; }

    }

    public class flutterSendBankRequest
    {
        public string narration { get; set; }
        public string currency { get; set; }
        public string account_bank { get; set; }
        public string account_number { get; set; }
        public int amount { get; set; }


        public string reference { get; set; }
        public string callback_url { get; set; }
        public string debit_currency { get; set; }
    }
    public class ZenithBankOTPRequest
    {
        public string otp { get; set; }
        public string flw_ref { get; set; }
        public string type { get; set; }
        public long WalletUserId { get; set; }

    }
    public class ZenithBankOTPRequestapi
    {
        public string otp { get; set; }
        public string flw_ref { get; set; }
        public string type { get; set; }


    }

    public class flutterbanktransferRequest
    {
        public string currency { get; set; }
        public string tx_ref { get; set; }
        public int amount { get; set; }
        public string email { get; set; }
    }

    public class flutterbanktransferResponse
    {
        public string status { get; set; }
        public string message { get; set; }
        public int RstKey { get; set; }
        public meta meta { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }

    }


    public class meta
    {
        public authorization authorization { get; set; }

    }


    public class authorization
    {
        public string transfer_reference { get; set; }
        public string transfer_account { get; set; }
        public string transfer_bank { get; set; }
        public string account_expiration { get; set; }
        public string transfer_note { get; set; }
        public string transfer_amount { get; set; }
        public string mode { get; set; }

    }

    public class flutterbanktransferauthorization
    {
        public string status { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public int RstKey { get; set; }
        public string transfer_reference { get; set; }
        public string transfer_account { get; set; }
        public string transfer_bank { get; set; }
        public string account_expiration { get; set; }
        public string transfer_note { get; set; }
        public string transfer_amount { get; set; }
        public string mode { get; set; }

    }

    public class flutterGhanaSendMobMonRequest
    {

        public string account_bank { get; set; }
        public string account_number { get; set; }
        public int amount { get; set; }
        public string currency { get; set; }

        public string reference { get; set; }
        public string beneficiary_name { get; set; }
        public meta1 meta { get; set; }
    }

    public class meta1
    {
        public meta1()
        {
            this.sender = string.Empty;
            this.sender_country = string.Empty;
            this.mobile_number = string.Empty;
        }
        public string sender { get; set; }
        public string sender_country { get; set; }
        public string mobile_number { get; set; }

    }


    public class merchantrequest
    {
        public string api_key { get; set; }
        public string user_id { get; set; }
        public decimal amount { get; set; }
        public string currency_code { get; set; }
        public string user_email { get; set; }
        public string user_phone_number { get; set; }
        public string transaction_id { get; set; }
        public string return_url { get; set; }
    }

    //public class FlutterCardPaymentWebResponse
    //{
    //    public FlutterCardPaymentWebResponse()
    //    {
    //        this.txRef = string.Empty;
    //        this.flwRef = string.Empty;
    //        this.orderRef = string.Empty;
    //        this.paymentPlan = string.Empty;
    //        this.status = string.Empty;
    //        this.IP = string.Empty;
    //        this.currency = string.Empty;

    //    }
    //   // public string event { get; set; }

    //    public int id { get; set; }
    //    public string txRef { get; set; }
    //    public string flwRef { get; set; }
    //    public string orderRef { get; set; }
    //    public string paymentPlan { get; set; }
    //    public DateTime createdAt { get; set; }
    //    public int amount { get; set; }
    //    public int charged_amount { get; set; }
    //    public string status { get; set; }
    //    public string IP { get; set; }
    //    public string currency { get; set; }

    //    public f_customer customer { get; set; }


    //    public entity entity { get; set; }


    //}
    //public class webhookrequest
    //{
    //    public string wehook { get; set; }
    //}

    //    public class f_customer
    //{
    //    public int id { get; set; }
    //    public string phone { get; set; }
    //    public string fullName { get; set; }
    //    public string customertoken { get; set; }
    //    public string email { get; set; }
    //    public DateTime createdAt { get; set; }
    //    public DateTime updatedAt { get; set; }
    //    public string deletedAt { get; set; }
    //    public int AccountId { get; set; }
    //}

    //public class entity
    //{
    //    public string card6 { get; set; }
    //    public string card_last4 { get; set; }
    //}

}
