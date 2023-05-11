using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.ViewModel.AirtimeFrVm
{
    public class Result
    {
        public long WalletUserId { get; set; }
        public string EmailId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Currentbalance { get; set; }
        public string MobileNo { get; set; }
        public string StdCode { get; set; }
    }

    public class RootObject
    {
        public RootObject()
        {
            result = new Result();
        }

        public bool isSuccess { get; set; }
        public string message { get; set; }
        public int status { get; set; }
        public Result result { get; set; }
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
    public class GetBillResponse
    {
        public string dateLimite { get; set; }
        public string codeExpiration { get; set; }
        public string merchant { get; set; }
        public string totAmount { get; set; }
        public string typeFacture { get; set; }
        public string heureEnreg { get; set; }
        public string refBranch { get; set; }
        public string numFacture { get; set; }
        public string idAbonnement { get; set; }
        public string fees { get; set; }
        public string sms { get; set; }
        public string dateEnreg { get; set; }
        public string perFacture { get; set; }
        public string status { get; set; }
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
        public double amount { get; set; }
        public double fee { get; set; }
        public string nomProprietaire { get; set; }
        public string numero { get; set; }
        public string volume { get; set; }
        public string recipient_id { get; set; }
        public double frais { get; set; }
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


    public class GetBillPayRequest
    {
        public string loginApi { get; set; }
        public string passwordApi { get; set; }
        public string partnerId { get; set; }
        public string meterNo { get; set; }
        public string amount { get; set; }
        public int walletServiceId { get; set; }
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
    public class BillPayServicesResponse
    {
        public string guTransactionId { get; set; }
        public string transactionDate { get; set; }
        public string recipientId { get; set; }
        public double amount { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        public string responseString { get; set; }
        //   public string call_back_url { get; set; }
    }
    public class AirtomePaymentRequest
    {
        public AirtomePaymentRequest()
        {
            auth = new Auth();
            version = 5;
            command = string.Empty;
            productId = 0;
        }
        public Auth auth { get; set; }
        public int version { get; set; }
        public string command { get; set; }
        public string userReference { get; set; }
        public long productId { get; set; }
        public int simulate { get; set; }
        public string msisdn { get; set; }
        public int @operator { get; set; }
        public decimal amountOperator { get; set; }
    }
    public class Auth
    {
        public Auth()
        {
            username = string.Empty;
            salt = string.Empty;
            password = string.Empty;
        }
        public string username { get; set; }
        public string salt { get; set; }
        public string password { get; set; }
        //public string signature { get; set; }
    }
}
