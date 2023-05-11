using Ezipay.ViewModel.CardPaymentViewModel;
using Ezipay.ViewModel.MerchantPaymentViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.ViewModel.BillViewModel
{
    public class BillPayMoneyAggregatoryRequest : BillAddMoneyAggregatoryRequest
    {
        public string serviceCategory { get; set; }
        public int ServiceCategoryId { get; set; }
        public int chennelId { get; set; }
        public string Password { get; set; }
        public long WalletUserId { get; set; }
    }

    public class BillAddMoneyAggregatoryRequest
    {
        public BillAddMoneyAggregatoryRequest()
        {
            this.amount = "0";
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
        public string amount { get; set; }
        public string channel { get; set; }
        public string customer { get; set; }
        public string ISD { get; set; }
        public string Password { get; set; }
        public string invoiceNo { get; set; }
        public string Comment { get; set; }
        public bool IsAddDuringPay { get; set; }
        public string policeNumber { get; set; }
        public string billNumber { get; set; }
        public decimal fees { get; set; }
        public PayMoneyContent PayMoneyContent { get; set; }
        public bool IsMerchant { get; set; }
        public MerchantTransactionRequest MerchantContent { get; set; }
        public long MerchantId { get; set; }
        public decimal netpayble { get; set; }
        public string recipient_invoice_id { get; set; }
        public string recipient_id { get; set; }
        public string customer_reference { get; set; }
        public string numeroFacture { get; set; }




        public string codeExpiration { get; set; }


        public string typeFacture { get; set; }
        public string heureEnreg { get; set; }
        public string refBranch { get; set; }
        public string numFacture { get; set; }
        public string idAbonnement { get; set; }
        public string dateEnreg { get; set; }
        public string perFacture { get; set; }
        public string dateLimite { get; set; }
        public string numero { get; set; }
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

    public class GetBillRequestForServicesIvory
    {
        public string partnerId { get; set; }
        public string loginApi { get; set; }
        public string passwordApi { get; set; }
        public string numeroFacture { get; set; }
        public string facturier { get; set; }
        public string serviceId { get; set; }
    }

    public class GetBillRequestForSN
    {
        public string partnerId { get; set; }
        public string loginApi { get; set; }
        public string passwordApi { get; set; }
        public string meterNo { get; set; }
        public string amount { get; set; }
    }

    public class GetBillReponseForSn
    {
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
        public string status { get; set; }
        public string message { get; set; }
        public double fees { get; set; }
    }

    public class GetBillRequestForSN_SDE
    {
        public string login_api { get; set; }
        public string password_api { get; set; }
        public string partner_id { get; set; }
        public string customer_reference { get; set; }
        public string recipient_country_code { get; set; }
    }

    public class GetBillRequestForSN_Senelec
    {
        public string login_api { get; set; }
        public string password_api { get; set; }
        public string partner_id { get; set; }
        public string recipient_id { get; set; }
        public string recipient_country_code { get; set; }
    }

    public class GetBillRequest_ML
    {
        public string loginApi { get; set; }
        public string passwordApi { get; set; }
        public string partnerId { get; set; }
        public string numero { get; set; }
        public string montant { get; set; }
        //public int walletServiceId { get; set; }
    }

    public class BillPayServicesRequestForServices
    {
        public string login_api { get; set; }
        public string password_api { get; set; }
        public string partner_id { get; set; }
        public string recipient_id { get; set; }
        public string recipient_invoice_id { get; set; }
        public string service_id { get; set; }
        public decimal amount { get; set; }
        public string partner_transaction_id { get; set; }
        public string destinataire { get; set; }
        public string call_back_url { get; set; }
    }

    public class PrepaidBillPayServicesRequestForServices
    {
        public string loginApi { get; set; }
        public string passwordApi { get; set; }
        public string partnerId { get; set; }
        public string recipientId { get; set; }
        public string meterNo { get; set; }
        public string serviceId { get; set; }
        public decimal amount { get; set; }
        public decimal fees { get; set; }
        public string partnerTransactionId { get; set; }
        public string callBackUrl { get; set; }
    }

    public class BillPayRequest
    {
        public string partnerId { get; set; }
        public string loginApi { get; set; }
        public string passwordApi { get; set; }
        public string callBackUrl { get; set; }
        public string telephone { get; set; }
        public string montant { get; set; }
        public string serviceId { get; set; }
        public string partnerTransactionId { get; set; }
        public string dateLimite { get; set; }
        public string codeExpiration { get; set; }
        public string merchant { get; set; }
        public string totAmount { get; set; }
        public string typeFacture { get; set; }
        public string heureEnreg { get; set; }
        public string refBranch { get; set; }
        public string numFacture { get; set; }
        public string idAbonnement { get; set; }
        public string dateEnreg { get; set; }
        public string perFacture { get; set; }
    }

    public class BillPaymentRequestSDE
    {
        public string login_api { get; set; }
        public string password_api { get; set; }
        public string partner_id { get; set; }
        public string customer_reference { get; set; }
        public string recipient_invoice_id { get; set; }
        public string service_id { get; set; }
        public decimal amount { get; set; }
        public string partner_transaction_id { get; set; }
        public string recipient_id { get; set; }
        public string callBackUrl { get; set; }
    }

    public class BillPaymentRequest_ML
    {
        public string numero { get; set; }
        public string telephone { get; set; }
        public string montant { get; set; }
        public string serviceId { get; set; }
        public string partnerTransactionId { get; set; }
        public string partnerId { get; set; }
        public string loginApi { get; set; }
        public string passwordApi { get; set; }
        public string callBackUrl { get; set; }
    }

    public class PayServicesResponseForServices
    {
        public string service_id { get; set; }
        public string gu_transaction_id { get; set; }
        public string status { get; set; }
        public string transaction_date { get; set; }
        public string recipient_phone_number { get; set; }
        public decimal amount { get; set; }
        public string partner_transaction_id { get; set; }
        public int StatusCode { get; set; }
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
        public string recipient_id { get; set; }
        public string customer_reference { get; set; }
        public string recipient_invoice_id { get; set; }
    }

    public class GetFeeRequest
    {
        public string Customer { get; set; }
        public string amount { get; set; }
    }

}