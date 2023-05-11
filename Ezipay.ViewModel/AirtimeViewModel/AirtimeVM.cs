using Ezipay.Database;
using Ezipay.ViewModel.CardPaymentViewModel;
using Ezipay.ViewModel.MerchantPaymentViewModel;
using Ezipay.ViewModel.PayMoneyViewModel;
using Ezipay.ViewModel.SendPushViewModel;
using System;

namespace Ezipay.ViewModel.AirtimeViewModel
{
    public class AddCashDepositToBankResponse
    {
        public AddCashDepositToBankResponse()
        {

            this.DepositorCashAmount = string.Empty;
            this.DepositorName = string.Empty;
            this.DepositorCountry = string.Empty;
            this.Password = string.Empty;
            this.DepositorCountry = string.Empty;

            this.StatusCode = 0;
            this.Message = string.Empty;
            this.IsEmailVerified = false;

            this.RstKey = 0;
        }

        public string DepositorCountryCode { get; set; }

        public string DepositStatus { get; set; }

        public string Reason { get; set; }
        public int StatusCode { get; set; }
        public int RstKey { get; set; }
        public string Message { get; set; }
        public string DepositorCashAmount { get; set; }
        public string DepositorCountry { get; set; }
        public string DepositorSlipImage { get; set; }
        public string Password { get; set; }
        public long WalletUserId { get; set; }
        public string DepositorName { get; set; }

        public bool IsEmailVerified { get; set; }
        public int Status { get; set; }


        public string FormatedTransactionDate { get; set; }
        public DateTime? TransactionDate { get; set; }
    }

    public class AddCashDepositToBankRequest
    {
        public AddCashDepositToBankRequest()
        {
            this.DepositorCashAmount = string.Empty;
            this.DepositorName = string.Empty;
            this.DepositorCountry = string.Empty;
            this.Password = string.Empty;
            this.DepositorCountry = string.Empty;
            this.TotalDepositorAmount = string.Empty;
        }

        public string TotalDepositorAmount { get; set; }
        public string ServiceType { get; set; }
        public string DepositorCashAmount { get; set; }
        public string DepositorCountry { get; set; }

        public string DepositorCountryCode { get; set; }
        public string DepositorSlipImage { get; set; }
        public string Password { get; set; }
        public long WalletUserId { get; set; }
        public string DepositorName { get; set; }
        public string Message { get; set; }
    }


    public class AddMoneyAggregatorResponse
    {
        public AddMoneyAggregatorResponse()
        {
            this.StatusCode = string.Empty;
            this.Amount = string.Empty;
            this.MobileNo = string.Empty;
            this.Message = string.Empty;
            this.TransactionId = string.Empty;
            this.InvoiceNo = string.Empty;
            this.CurrentBalance = string.Empty;
            this.DocStatus = false;
            this.DocumetStatus = 0;
            this.IsEmailVerified = false;
            airtimePaymentResponse = new AirtimePaymentResponse();
        }
        public DateTime TransactionDate { get; set; }
        public string FormatedTransactionDate { get; set; }
        public string MobileNo { get; set; }
        public string Amount { get; set; }
        public string StatusCode { get; set; }
        public int Status { get; set; }
        public string ToMobileNo { get; set; }
        public string Message { get; set; }
        public string TransactionId { get; set; }
        public string InvoiceNo { get; set; }
        public string CurrentBalance { get; set; }
        public string AccountNo { get; set; }
        public int DocumetStatus { get; set; }
        public bool DocStatus { get; set; }
        public bool IsEmailVerified { get; set; }
        public string ResponseString { get; set; }
        public object BillDetail { get; set; }
        public string service_id { get; set; }
        public string gu_transaction_id { get; set; }
        public string status { get; set; }
        public string transaction_date { get; set; }
        public string recipient_phone_number { get; set; }
        public decimal amount { get; set; }
        public string partner_transaction_id { get; set; }
        public int statusCode { get; set; }
        public string responseString { get; set; }
        public string message { get; set; }
        public string meterStatus { get; set; }
        public string meterNo { get; set; }
        public string krn { get; set; }
        public string rspCod { get; set; }
        public string custType { get; set; }
        public string enelId { get; set; }
        public string customerId { get; set; }
        public string lastVendDate { get; set; }
        public object ti { get; set; }
        public string rspMsg { get; set; }
        public string address { get; set; }
        public string ccy { get; set; }
        public string sessionId { get; set; }
        public string customerName { get; set; }
        public string customer_reference { get; set; }
        public string recipient_invoice_id { get; set; }
        public string customer_name { get; set; }
        public string customer_contact { get; set; }
        public double fee { get; set; }
        public AirtimePaymentResponse airtimePaymentResponse { get; set; }
        public string recipient_id { get; set; }
        public int RstKey { get; set; }
        public string OrangeUrl { get; set; }
        public string transactionId { get; set; }

    }


    public class AirtimePaymentResponse
    {
        public Status status { get; set; }
        public string command { get; set; }
        public int timestamp { get; set; }
        public long reference { get; set; }
        public Result result { get; set; }
    }
    public class Status
    {
        public int id { get; set; }
        public string name { get; set; }
        public int type { get; set; }
        public string typeName { get; set; }
    }
    public class Result
    {
        public long id { get; set; }
        public Operator @operator { get; set; }
        public Country country { get; set; }
        public Amount amount { get; set; }
        public Currency currency { get; set; }
        public string productId { get; set; }
        public string productType { get; set; }
        public bool simulation { get; set; }
        public string userReference { get; set; }
        public string msisdn { get; set; }
        public Balance balance { get; set; }
    }

    public class Operator
    {
        public string id { get; set; }
        public string name { get; set; }
        public string reference { get; set; }
    }
    public class Country
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class Amount
    {
        public string @operator { get; set; }
        public string user { get; set; }
    }

    public class Currency
    {
        public string user { get; set; }
        public string @operator { get; set; }
    }
    public class Balance
    {
        public string initial { get; set; }
        public string transaction { get; set; }
        public string commission { get; set; }
        public string commissionPercentage { get; set; }
        public string final { get; set; }
        public string currency { get; set; }
    }

    public class PayMoneyAggregatoryRequest : AddMoneyAggregatoryRequest
    {
        public string serviceCategory { get; set; }
        public string BeneficiaryName { get; set; }
        public int ServiceCategoryId { get; set; }
        public int? chennelId { get; set; }
        public int? ProductId { get; set; }
        public int? OperatorId { get; set; }
        public object ObjProduct { get; set; }
        public object ObjChannel { get; set; }
    }

    public class AddMoneyAggregatoryRequest
    {
        public AddMoneyAggregatoryRequest()
        {
            this.Amount = string.Empty;
            this.channel = string.Empty;
            this.customer = string.Empty;
            this.ISD = string.Empty;
            this.invoiceNo = string.Empty;
            this.Password = string.Empty;
            this.Comment = string.Empty;
            this.IsAddDuringPay = false;
            this.IsMerchant = false;
            this.MerchantId = 0;
        }
        public string Amount { get; set; }
        public string channel { get; set; }
        public string customer { get; set; }
        public string ISD { get; set; }
        public string Password { get; set; }
        public string invoiceNo { get; set; }
        public string Comment { get; set; }
        public bool IsAddDuringPay { get; set; }
        public string DisplayContent { get; set; }
        public PayMoneyContent PayMoneyContent { get; set; }
        public bool IsMerchant { get; set; }
        public MerchantTransactionRequest MerchantContent { get; set; }
        public long MerchantId { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsDisabledTransaction { get; set; }
        public bool IsdocVerified { get; set; }
        public long WalletUserId { get; set; }
        public string MobileNo { get; set; }
        public string VoucherCode { get; set; }
        public string IsdCode { get; set; }

        public string SenderIdNumber { get; set; }
        public string SenderIdType { get; set; }
        public string SenderDateofbirth { get; set; }
        public string SenderAddress { get; set; }
        public string SenderCity { get; set; }
        public string ReceiverFirstName { get; set; }
        public string ReceiverLastName { get; set; }
        public string ReceiverMobileNo { get; set; }
        public string ReceiverEmail { get; set; }

    }
    public class MobileMoneyAggregatoryRequest
    {
        public MobileMoneyAggregatoryRequest()
        {
            this.apiKey = string.Empty;
            this.invoiceNo = string.Empty;
            this.serviceCategory = string.Empty;
            this.serviceType = string.Empty;
            this.signature = string.Empty;
            this.amount = string.Empty;
            this.channel = string.Empty;
            this.customer = string.Empty;

        }
        /// <summary>
        /// Amount in string
        /// </summary>
        public string amount { get; set; }
        /// <summary>
        /// Channel name like VODAPHONE,AIRTEL etc
        /// </summary>
        public string channel { get; set; }
        /// <summary>
        /// Mobile No without ISD
        /// </summary>
        public string customer { get; set; }
        /// <summary>
        /// Key which provided by Aggregatory
        /// </summary>
        public string apiKey { get; set; }
        /// <summary>
        /// Random number for backend db
        /// </summary>
        public string invoiceNo { get; set; }
        /// <summary>
        /// LIKE AIRTIME,MOBILEMONEY,ISP,GENERAL and GENERAL etc
        /// Airtime (airtime): This is an airtime top up operation.
        /// Mobile Money (mobilemoney): This is a transaction involving a mobile money account.
        /// Isp (isp): This is an isp payment transaction.
        /// General (general): This refers to general transactions that are not categorized, e.g. television subscription payments etc
        /// Merchant: This is a payment to a specific merchant account integrated on the platform.
        /// </summary>
        public string serviceCategory { get; set; }
        /// <summary>
        /// DEBIT or CREDIT
        /// </summary>
        public string serviceType { get; set; }
        /// <summary>
        /// This is an MD5 hash of (ApiKey+Customer+Amount+InvoiceNo+SecretKey)
        /// </summary>
        public string signature { get; set; }
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

    public class TransactionDisabledPushModel : NotificationDefaultKeys
    {
        public TransactionDisabledPushModel()
        {
            this.Message = string.Empty;

        }
        public string Message { get; set; }
    }
    public class OrangeTokenResponse
    {
        public string token_type { get; set; }
        public string access_token { get; set; }
        public string expires_in { get; set; }
    }
    public class OrangeTokenRequest
    {
        public string merchant_key { get; set; }
        public string currency { get; set; }
        public string order_id { get; set; }
        public int amount { get; set; }
        public string return_url { get; set; }
        public string cancel_url { get; set; }
        public string notif_url { get; set; }
        public string lang { get; set; }
        public string reference { get; set; }
    }
    public class PaymentUrlResponse
    {
        public int status { get; set; }
        public string message { get; set; }
        public string pay_token { get; set; }
        public string payment_url { get; set; }
        public string notif_token { get; set; }
    }
    public class AddMobileMoneyAggregatoryRequest
    {
        public AddMobileMoneyAggregatoryRequest()
        {
            this.ApiKey = string.Empty;
            this.TransactionId = string.Empty;
            this.ServiceType = string.Empty;
            this.Signature = string.Empty;
            this.Amount = string.Empty;
            this.Channel = string.Empty;
            this.Customer = string.Empty;
            this.Country = string.Empty;


        }

        /// <summary>
        /// This is an HMAC SHA256 of (MerchantId+Amount+Customer+TransactionId+Secret)
        /// </summary>
        public string Signature { get; set; }
        public string ApiKey { get; set; }
        public string ServiceType { get; set; }
        public string servicecategory { get; set; }
        public string Channel { get; set; }
        public string Amount { get; set; }
        public string Customer { get; set; }
        public string Country { get; set; }
        public string TransactionId { get; set; }
        public string InvoiceNo { get; set; }


        public MobileMoneySenderDetail1 Sender { get; set; }

        public MobileMoneyReceiverDetail1 Recipient { get; set; }

    }

    public class MobileMoneySenderDetail1
    {
        public string address { get; set; }
        public string city { get; set; }
        public string dateofBirth { get; set; }
        public string idNumber { get; set; }
        public string idType { get; set; }
        public string email { get; set; }
        public string firstName { get; set; }
        public string surname { get; set; }
        public string contact { get; set; }
        public string country { get; set; }

        //public string id_country { get; set; }
    }

    public class MobileMoneyReceiverDetail1
    {
        public string firstName { get; set; }
        public string surname { get; set; }
        public string contact { get; set; }
        public string email { get; set; }
        public string BankCode { get; set; }
    }


    public class PayServicesRequestForServices
    {
        public string service_id { get; set; }
        public string recipient_phone_number { get; set; }
        public decimal amount { get; set; }
        public string partner_id { get; set; }
        public string partner_transaction_id { get; set; }
        public string login_api { get; set; }
        public string password_api { get; set; }
        public string call_back_url { get; set; }
    }
    public class PayServicesMoneyAggregatoryRequest
    {
        public PayServicesMoneyAggregatoryRequest()
        {
            this.ApiKey = string.Empty;
            this.Amount = string.Empty;
            this.TransactionId = string.Empty;
            this.Customer = string.Empty;
            this.SecretKey = string.Empty;
        }
        public string ApiKey { get; set; }
        public string Amount { get; set; }
        public string TransactionId { get; set; }
        public string Customer { get; set; }
        public string SecretKey { get; set; }
    }
    public class PayServicesMoneyAggregatoryRequestCamroon
    {
        public PayServicesMoneyAggregatoryRequestCamroon()
        {
            this.ApiKey = string.Empty;
            this.Amount = string.Empty;
            this.InvoiceNo = string.Empty;
            this.Customer = string.Empty;
            this.SecretKey = string.Empty;
        }
        public string ApiKey { get; set; }
        public string Amount { get; set; }
        public string InvoiceNo { get; set; }
        public string Customer { get; set; }
        public string SecretKey { get; set; }
    }

    public class DetailForBillPaymentVM
    {
        public DetailForBillPaymentVM()
        {
            sender = new WalletUser();
            WalletService = new WalletService();
            SubCategory = new SubCategory();
            transactionLimit = new TransactionLimitResponse();
            transactionHistory = new TransactionHistoryAddMoneyReponse();
        }
        public WalletUser sender { get; set; }
        public WalletService WalletService { get; set; }
        public SubCategory SubCategory { get; set; }
        public bool IsdocVerified { get; set; }
        public TransactionLimitResponse transactionLimit { get; set; }
        public TransactionHistoryAddMoneyReponse transactionHistory { get; set; }
    }

    public class MobileMoneySenderDetailresponse
    {
        public string WalletuserId { get; set; }
        public string SenderIdNumber { get; set; }
        public string SenderIdType { get; set; }
        public string SenderIdExpiryMonYr { get; set; }
        public string SenderDateofbirth { get; set; }
        public string SenderAddress { get; set; }
        public string SenderCity { get; set; }
        public string SenderCountry { get; set; }
    }
    public class MobileMoneySenderDetailrequest
    {
        public long WalletuserId { get; set; }
        public string SenderIdNumber { get; set; }

    }

    public class GlobalNigeriaBankTransferRequest
    {
        public string status { get; set; }
        public string tx_ref { get; set; }
        public string currency { get; set; }
        public string payment_type { get; set; }
    }
    public class GlobalNigeriaBankTransferRequestnene
    {
        public string StatusCode { get; set; }

        public string Message { get; set; }

        public string TransactionId { get; set; }

        public string InvoiceNo { get; set; }

        public string OperatorType { get; set; }
    }
    




}
