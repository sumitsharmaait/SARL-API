using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.ViewModel.InternatinalRechargeViewModel
{
    public class TransferTo
    {
        public string country { get; set; }
        public string countryid { get; set; }
        public string @operator { get; set; }
        public string operatorid { get; set; }
        public string connection_status { get; set; }
        public string destination_msisdn { get; set; }
        public string destination_currency { get; set; }
        public string product_list { get; set; }
        public string retail_price_list { get; set; }
        public string wholesale_price_list { get; set; }
        public string authentication_key { get; set; }
        public string error_code { get; set; }
        public string error_txt { get; set; }
        //--------------------------------------
        public string transactionid { get; set; }
        public string msisdn { get; set; }
        public string reference_operator { get; set; }
        public string originating_currency { get; set; }
        public string product_requested { get; set; }
        public string actual_product_sent { get; set; }
        public string wholesale_price { get; set; }
        public string retail_price { get; set; }
        public string service_fee { get; set; }
        public string balance { get; set; }
        public string sms_sent { get; set; }
        public string sms { get; set; }
        public string cid1 { get; set; }
        public string cid2 { get; set; }
        public string cid3 { get; set; }
        public string return_timestamp { get; set; }
        public string return_version { get; set; }

    }

    public class GetProductListResponse
    {
        public GetProductListResponse()
        {
            TransferTo = new TransferTo();
        }
        public TransferTo TransferTo { get; set; }
    }

    public class InternationalAirtimeRequest
    {
        public string MobileNo { get; set; }
        public string Amount { get; set; }
        public string Password { get; set; }
        public string IsdCode { get; set; }
    }
    public class InternationalAirtimeResponse
    {
        public InternationalAirtimeResponse()
        {
            TransferTo = new TransferTo();
            DisplayContent = new List<string>();

            internationalAirtimeAmountResponses = new List<InternationalAirtimeAmountResponse>();
        }
        public int RstKey { get; set; }
        public string Message { get; set; }
        public string responsestring { get; set; }
        public string AmountInCedi { get; set; }
        public List<string> DisplayContent { get; set; }
        public List<string> Products { get; set; }
        public List<string> retail_price_list { get; set; }
        public List<string> wholesale_price_list { get; set; }
        public TransferTo TransferTo { get; set; }
        public List<InternationalAirtimeAmountResponse> internationalAirtimeAmountResponses { get; set; }
    }

    public class InternationalAirtimeAmountResponse
    {
        public string AmountInLocalCountry { get; set; }
        public string AmountInUsd { get; set; }
        public string msisdn { get; set; }
        public string DisplayContent { get; set; }
        public string AmountInLe { get; set; }
    }

    public class @xml
    {
        public string login { get; set; }
        public string key { get; set; }
        public string md5 { get; set; }
        public string destination_msisdn { get; set; }
        public string delivered_amount_info { get; set; }
        public string return_service_fee { get; set; }
        public string action { get; set; }
    }
    public class GetReverseIdAirtimeInternationalRequest
    {
        public string login { get; set; }
        public string key { get; set; }
        public string md5 { get; set; }
        public string action { get; set; }
    }

    public class RechargeAirtimeInternationalRequest
    {
        public string login { get; set; }
        public string key { get; set; }
        public string md5 { get; set; }
        public string msisdn { get; set; }
        public string sms { get; set; }
        public string destination_msisdn { get; set; }
        public string product { get; set; }
        public string cid1 { get; set; }
        public string sender_sms { get; set; }
        public string sender_text { get; set; }
        public string delivered_amount_info { get; set; }
        public string return_timestamp { get; set; }
        public string return_version { get; set; }
        public string return_service_fee { get; set; }
        public string action { get; set; }
    }
    public class RechargeAirtimeInternationalResponse
    {
        public string transactionid { get; set; }
        public string msisdn { get; set; }
        public string destination_msisdn { get; set; }
        public string country { get; set; }
        public string countryid { get; set; }
        public string @operator { get; set; }
        public string operatorid { get; set; }
        public string reference_operator { get; set; }
        public string originating_currency { get; set; }
        public string destination_currency { get; set; }
        public string product_requested { get; set; }
        public string actual_product_sent { get; set; }
        public string wholesale_price { get; set; }
        public string retail_price { get; set; }
        public string service_fee { get; set; }
        public string balance { get; set; }
        public string sms_sent { get; set; }
        public string sms { get; set; }
        public string cid1 { get; set; }
        public string cid2 { get; set; }
        public string cid3 { get; set; }
        public string local_info_value { get; set; }
        public string local_info_amount { get; set; }
        public string local_info_currency { get; set; }
        public string return_timestamp { get; set; }
        public string return_version { get; set; }
        public string sender_sms { get; set; }
        public string sender_text { get; set; }
        public string authentication_key { get; set; }
        public string error_code { get; set; }
        public string error_txt { get; set; }
    }
    public class RechargeAirtimeInternationalAggregatorRequest
    {
        public string serviceCategory { get; set; }
        public string BeneficiaryName { get; set; }
        public int ServiceCategoryId { get; set; }
        public int chennelId { get; set; }
        public string Amount { get; set; }
        public string channel { get; set; }
        public string customer { get; set; }
        public string ISD { get; set; }
        public string Password { get; set; }
        public string Comment { get; set; }
        public string DisplayContent { get; set; }
        public long WalletUserId { get; set; }
        public string MobileNo { get; set; }
        public string msisdn { get; set; }
        public string amount { get; set; }
        public string AmountInUsd { get; set; }
        public string AmountInLocalCountry { get; set; }
    }
    //------------
    public class IntCountry
    {
        public int country_id { get; set; }
        public string country { get; set; }
    }

    public class CountryListResponse
    {
        public CountryListResponse()
        {
            countries = new List<IntCountry>();
        }
        public List<IntCountry> countries { get; set; }
    }

    public class InternationalDTHResponse
    {
        public InternationalDTHResponse()
        {
            countryListResponse = new CountryListResponse();
            getServiceListResponse = new GetServiceListResponse();
            operatorListReqponse = new List<OperatorListWithUrlReqponse>();
        }
        public int RstKey { get; set; }
        public string Message { get; set; }
        public CountryListResponse countryListResponse { get; set; }
        public GetServiceListResponse getServiceListResponse { get; set; }
        public List<OperatorListWithUrlReqponse> operatorListReqponse { get; set; }
    }
    public class GetCountryListRequest
    {
        public string Password { get; set; }
    }
    public class GetServiceListRequest
    {
        public string Password { get; set; }
        public string country_id { get; set; }
        public string service_id { get; set; }
        public string operator_id { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Service
    {
        public int service_id { get; set; }
        public string service { get; set; }
    }


    public class GetServiceListResponse
    {
        public GetServiceListResponse()
        {
            services = new List<Service>();
        }
        public List<Service> services { get; set; }
    }

    public class Operator
    {
        public int operator_id { get; set; }
        public string @operator { get; set; }
        public int country_id { get; set; }
        public string country { get; set; }
    }

    public class OperatorListReqponse
    {
        public OperatorListReqponse()
        {
            operators = new List<Operator>();
        }
        public List<Operator> operators { get; set; }
    }


    public class OperatorListWithUrlReqponse
    {
        public int operator_id { get; set; }
        public string @operator { get; set; }
        public int country_id { get; set; }
        public string country { get; set; }
        public string ImageUrl { get; set; }
    }



    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class FixedValueRecharge
    {
        public int product_id { get; set; }
        public string product_name { get; set; }
        public object product_short_desc { get; set; }
        public int operator_id { get; set; }
        public string @operator { get; set; }
        public int country_id { get; set; }
        public string country { get; set; }
        public int service_id { get; set; }
        public string service { get; set; }
        public string account_currency { get; set; }
        public double wholesale_price { get; set; }
        public double retail_price { get; set; }
        public int fee { get; set; }
        public string product_currency { get; set; }
        public int product_value { get; set; }
        public string local_currency { get; set; }
        public int local_value { get; set; }
    }

    public class VariableValueRecharge
    {
        public int product_id { get; set; }
        public string product_name { get; set; }
        public string product_short_desc { get; set; }
        public int operator_id { get; set; }
        public string @operator { get; set; }
        public int country_id { get; set; }
        public string country { get; set; }
        public int service_id { get; set; }
        public string service { get; set; }
        public string account_currency { get; set; }
        public string product_currency { get; set; }
    }

    public class GetDTHProductListResponse
    {
        public GetDTHProductListResponse()
        {
            fixed_value_recharges = new List<FixedValueRecharge>();
            variable_value_recharges = new List<VariableValueRecharge>();
            fixed_value_vouchers = new List<Fixed_value_vouchers>();
            fixed_value_payments = new List<object>();
        }
        public List<Fixed_value_vouchers> fixed_value_vouchers { get; set; }
        public List<FixedValueRecharge> fixed_value_recharges { get; set; }
        public List<object> variable_value_payments { get; set; }
        public List<object> fixed_value_payments { get; set; }
        public List<object> variable_value_vouchers { get; set; }
        public List<VariableValueRecharge> variable_value_recharges { get; set; }
    }



    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Fixed_value_vouchers
    {
        public int product_id { get; set; }
        public string product_name { get; set; }
        public string product_short_desc { get; set; }
        public int operator_id { get; set; }
        public string @operator { get; set; }
        public int country_id { get; set; }
        public string country { get; set; }
        public int service_id { get; set; }
        public string service { get; set; }
        public string account_currency { get; set; }
        public double wholesale_price { get; set; }
        public double retail_price { get; set; }
        public int fee { get; set; }
        public string product_currency { get; set; }
        public int product_value { get; set; }
        public string local_currency { get; set; }
        public int local_value { get; set; }
        public object info1 { get; set; }
        public object info2 { get; set; }
        public object info3 { get; set; }
    }


    public class InternationalDTHProductResponse
    {
        public InternationalDTHProductResponse()
        {
            getDTHProductListResponse = new GetDTHProductListResponse();
            productListDisplayResponse = new List<ProductListDisplayResponse>();
        }
        public int RstKey { get; set; }
        public string Message { get; set; }
        public GetDTHProductListResponse getDTHProductListResponse { get; set; }
        public List<ProductListDisplayResponse> productListDisplayResponse { get; set; }
    }

    public class ProductListDisplayResponse
    {
        public string AmountInLocalCountry { get; set; }
        public string AmountInUsd { get; set; }
        public string product_id { get; set; }
        public string DisplayContent { get; set; }
        public string AmountInLe { get; set; }

        public string operator_id { get; set; }
        public string country_id { get; set; }
        public string service_id { get; set; }
        public string wholesale_price { get; set; }
        public string retail_price { get; set; }
        public string product_value { get; set; }
    }

    public class RechargeDthInternationalAggregatorRequest
    {
        public string serviceCategory { get; set; }
        public string BeneficiaryName { get; set; }
        public int ServiceCategoryId { get; set; }
        public int chennelId { get; set; }
        public string Amount { get; set; }
        public string channel { get; set; }
        public string customer { get; set; }
        public string ISD { get; set; }
        public string Password { get; set; }
        public string Comment { get; set; }
        public string DisplayContent { get; set; }
        public long WalletUserId { get; set; }
        public string MobileNo { get; set; }
        public string msisdn { get; set; }
        public string amount { get; set; }
        public string AmountInUsd { get; set; }
        public string AmountInLocalCountry { get; set; }

        public string account_number { get; set; }
        public string product_id { get; set; }
    }



    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Sender
    {
        public string last_name { get; set; }
        public string middle_name { get; set; }
        public string first_name { get; set; }
        public string email { get; set; }
        public string mobile { get; set; }
    }

    public class Recipient
    {
        public string last_name { get; set; }
        public string middle_name { get; set; }
        public string first_name { get; set; }
        public string email { get; set; }
        public string mobile { get; set; }
    }

    public class DTHRechargeRequest
    {
        public DTHRechargeRequest()
        {
            sender = new Sender();
            recipient = new Recipient();
        }
        public string account_number { get; set; }
        public string product_id { get; set; }
        public string external_id { get; set; }
        public string simulation { get; set; }
        public string sender_sms_notification { get; set; }
        public string sender_sms_text { get; set; }
        public string recipient_sms_notification { get; set; }
        public string recipient_sms_text { get; set; }
        public Sender sender { get; set; }
        public Recipient recipient { get; set; }
    }

    public class DTHRechargeResponse
    {
        public DTHRechargeResponse()
        {
            sender = new Sender();
            recipient = new Recipient();
        }
        public string transaction_id { get; set; }
        public int simulation { get; set; }
        public string status { get; set; }
        public string status_message { get; set; }
        public string date { get; set; }
        public string account_number { get; set; }
        public string external_id { get; set; }
        public string operator_reference { get; set; }
        public string product_id { get; set; }
        public string product { get; set; }
        public string product_desc { get; set; }
        public string product_currency { get; set; }
        public int product_value { get; set; }
        public string local_currency { get; set; }
        public int local_value { get; set; }
        public string operator_id { get; set; }
        public string @operator { get; set; }
        public string country_id { get; set; }
        public string country { get; set; }
        public string account_currency { get; set; }
        public double wholesale_price { get; set; }
        public double retail_price { get; set; }
        public int fee { get; set; }
        public Sender sender { get; set; }
        public Recipient recipient { get; set; }
        public int sender_sms_notification { get; set; }
        public string sender_sms_text { get; set; }
        public int recipient_sms_notification { get; set; }
        public string recipient_sms_text { get; set; }
        public string custom_field_1 { get; set; }
        public string custom_field_2 { get; set; }
        public string custom_field_3 { get; set; }
    }
    public class IsCurrectProduct
    {
        public string AmountInLocalCountry { get; set; }
        public string AmountInUsd { get; set; }
        public string msisdn { get; set; }
        public string DisplayContent { get; set; }
        public string AmountInLe { get; set; }
        public int RstKey { get; set; }
        public string Message { get; set; }
    }

}
