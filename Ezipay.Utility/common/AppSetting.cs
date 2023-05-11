using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Ezipay.Utility.common
{
    public class AppSetting
    {
        public static string imageUrl = ConfigurationManager.AppSettings["flagImageurl"];
        public static int LimitAmount = Convert.ToInt32(ConfigurationManager.AppSettings["CommissionAmountLimit"]);
        #region Common Usable Variables
        //public static int PageSize = Convert.ToInt32(ConfigurationManager.AppSettings["PageSize"]);
        public static string HostNameAdmin = ConfigurationManager.AppSettings["HostNameAdmin"];
        public static string QrCodeUrl = HostNameAdmin + "/" + ConfigurationManager.AppSettings["QrCodeUrl"];
        public static string VerifyMailLink = HostNameAdmin + "/" + ConfigurationManager.AppSettings["VerifyMailLink"];
        public static int OtpLimit = Convert.ToInt32(ConfigurationManager.AppSettings["OtpLimit"]);
        public static string EmailVerificationTemplate = "/EmailVerificationTemplate.html";
        public static string ForgotPasswordEmailTemplate = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/EmailTemplate/ForgotPasswordEmailTemplate.html");
        //  public static string successfullTransaction = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/EmailTemplate/EmailVerificationTemplate.html");      
        public static string writeFile = "/test.json";
        //Email Creds 
        public static string EmailFrom = ConfigurationManager.AppSettings["EmailFrom"];
        public static string Host = ConfigurationManager.AppSettings["Host"];
        public static string Port = ConfigurationManager.AppSettings["Port"];
        public static string Username = ConfigurationManager.AppSettings["Username"];
        public static string Password = ConfigurationManager.AppSettings["Password"];
        public static string successfullTransaction = "/email.html";
        public static string TxnStatementPerUsertemplate = "/template.html";
        #endregion

        /// <summary>
        /// Generate random password of 8 digits
        /// One alphabet Capital
        /// Four alphabet small
        /// One special character
        /// Two numeric digits
        /// </summary>
        /// <returns></returns>
        //public static string Password()
        //{
        //    Random random = new Random();
        //    const string WordCapital = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        //    const string WordSmall = "abcdefghijklmnopqrstuvxyz";
        //    const string num = "0123456789";
        //    const string sChars = @"!@#$%^&*()_+=|}{[]:;'/.,?><|\";
        //    string str = new string(Enumerable.Repeat(WordCapital, 1).Select(s => s[random.Next(s.Length)]).ToArray());
        //    str += new string(Enumerable.Repeat(WordSmall, 4).Select(s => s[random.Next(s.Length)]).ToArray());
        //    str += new string(Enumerable.Repeat(sChars, 1).Select(s => s[random.Next(s.Length)]).ToArray());
        //    str += new string(Enumerable.Repeat(num, 2).Select(s => s[random.Next(s.Length)]).ToArray());
        //    return str;

        //}
        /// <summary>
        /// Generate Alpha Numeric String of given size
        /// </summary>
        /// <param name="stringLength"></param>
        /// <returns></returns>
        public static string AlphaNumericString(int stringLength)
        {
            Random random = new Random();


            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!@#$%&*()0123456789";
            return new string(Enumerable.Repeat(chars, stringLength)
              .Select(s => s[random.Next(s.Length)]).ToArray());

        }
        /// <summary>
        /// Generate Alpha Numeric String of given size
        /// </summary>
        /// <param name="stringLength"></param>
        /// <returns></returns>
        public static string QRCode()
        {
            Random random = new Random();

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            StringBuilder sb = new StringBuilder(new string(Enumerable.Repeat(chars, 7).Select(s => s[random.Next(s.Length)]).ToArray()));
            sb.Append("-");
            sb.Append(new string(Enumerable.Repeat(chars, 7).Select(s => s[random.Next(s.Length)]).ToArray()));
            sb.Append("-");
            sb.Append(new string(Enumerable.Repeat(chars, 7).Select(s => s[random.Next(s.Length)]).ToArray()));
            sb.Append("-");
            sb.Append(new string(Enumerable.Repeat(chars, 7).Select(s => s[random.Next(s.Length)]).ToArray()));
            return sb.ToString();

        }
        /// <summary>
        /// Generate Alpha Numeric String of given size
        /// </summary>
        /// <param name="stringLength"></param>
        /// <returns></returns>
        public static string GetOtp()
        {
            Random random = new Random();
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, 4).Select(s => s[random.Next(s.Length)]).ToArray());
        }
        /// <summary>
        /// Generate current time stamp
        /// </summary>
        /// <returns></returns>
        public static String GetTimestamp()
        {
            DateTime dt = DateTime.UtcNow;
            return dt.ToString("yyyyMMddHHmmssffff");
        }
        /// <summary>
        /// Generate new guid
        /// </summary>
        /// <returns></returns>
        public static string GetGUID()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Generate Alpha Numeric String of given size
        /// </summary>
        /// <param name="stringLength"></param>
        /// <returns></returns>
        public static string RandomAlphaNumerals(int stringLength)
        {
            Random random = new Random();


            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, stringLength)
              .Select(s => s[random.Next(s.Length)]).ToArray());

        }

        /// <summary>
        /// Generate unique number of given size
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GetUniqueNumber()
        {
            Random random = new Random();
            const string numbers = "0123456789";

            return "EZ-" + (DateTime.UtcNow.Date.Year).ToString() + "-" + new string(Enumerable.Repeat(numbers, 4).Select(s => s[random.Next(s.Length)]).ToArray());
        }
        /// <summary>
        /// Generate unique number of given size
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GetUniqueNumber(int length)
        {
            Random random = new Random();
            const string numbers = "0123456789";

            return (DateTime.UtcNow.Date.Year).ToString() + new string(Enumerable.Repeat(numbers, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

    }

    public class ApplicationUrl
    {
        public string Domain()
        {
            var Host = HttpContext.Current.Request.Url.Host.ToString().ToLower();
            if (Host != "localhost")
            {

                return "https://www.ezipaygh.com";
            }
            else
            {
                if (HttpContext.Current.Request.Url.Port > 0)
                {
                    return "http://localhost:" + HttpContext.Current.Request.Url.Port.ToString();
                }
                else
                {
                    return "http://localhost";
                }
            }
        }

    }

    public static class ThirdPartyAggragatorSettings
    {

        // public static string wiflixUrl = "https://staging.thirdparty.wi-flix.com/api/ezipay/login?";

        public static string wiflixUrl = "https://thirdparty.wi-flix.com/api/ezipay/login?";
        public static string PayMoneyUrlByPortal1 = "https://api.orange.com/oauth/v2/token";
        public static string PayMoneyUrlByPortal2 = "https://api.orange.com/orange-money-webpay/sl/v1/webpayment";
        public static string customerKey = ConfigurationManager.AppSettings["customerKey"];
        public static string merchant_key = ConfigurationManager.AppSettings["merchant_key"];
        public static string cancel_url = ConfigurationManager.AppSettings["cancel_url"];
        public static string notif_url = ConfigurationManager.AppSettings["notif_url"];
        public static string return_url = ConfigurationManager.AppSettings["return_url"];
        public static string reference = ConfigurationManager.AppSettings["reference"];
        //public static string ApiKey = "b1d7254b-3a3e-4d9d-8255-eaaf256cd97d";
        //url for ghana APIs:- 54.186.218.22 -> 35.84.236.154
        //public static string AddMobileMoney = "http://52.33.132.204/aggregator/api/payment";//nene onli for testin
        public static string AddMobileMoney = "http://35.84.236.154/aggregator/api/payment"; //change url for mfs 16/03 live
        //public static string AddMobileMoney = "http://52.39.144.56/EZCameroon/api/payment"; //intouch checkin for testin
        public static string AddMobileMoneyCameroon = "http://35.84.236.154/EZCameroon/api/payment";
        public static string AddMobileMoney_test = "http://54.186.218.22/aggregatortest/api/payment";
        public static string callBackUrl = "https://newapi.ezipaysarl.com/api/ThridPartyApiController/UpdateTransactionStatus";
        public static string GetUserDetailApi = "http://18.236.158.163/api/User/GetUserDetail";
        public static string PayMoneyUrlForGhana = "http://18.236.158.163/api/WalletTransactions/UpdateUserWalletFromOtherCountry";
        //Urls for payservices
        //public static string MobileMoneyUrl = "http://52.40.89.233/aggregator/api/payment";
        //public static string PayMoneyUrl = "http://52.40.89.233/aggregator/api/payment";
        public static string PayMoneyUrl = "https://api.gutouch.com/v1/EZICI1364/airtime";
        public static string GhanaPayMoneyUrl = "http://52.40.89.233/aggregator/api/payment";//nu 
        //public static string AddMobileMoney = "http://52.13.30.167/aggregator/api/payment";
        //public static string AddMobileMoney = "http://54.186.218.22/aggregator/api/payment";
        public static string GetFeePayMoney = "http://35.84.236.154/aggregator/api/getfees";
        public static string PayMoneyUrl_BF = "https://api.gutouch.com/v1/BFEPY0004/airtime";
        public static string PayMoneyUrl_ML = "https://api.gutouch.com/v1/EZIML1563/airtime";
        public static string ElectricityBillPaymentUrl_SN = "https://dev-api.gutouch.com/v1/INTDK0800/senelec/paidBill";
        public static string WaterBillPaymentUrl_SN = "https://api.gutouch.com/v1/EZISN5651/sde/payBill";
        public static string PayMoneyUrl_SNG = "https://api.gutouch.com/v1/EZISN5651/airtime";
        public static string PrepaidPaymentUrl_SN = "https://api.gutouch.com/v1/EZISN5651/hexing/meterInfos";
        public static string getBillUrl_CIE = "https://api.gutouch.com/v1/EZICI1364/smartmotic/getbill";
        public static string getBillUrl_Sodeci = " https://api.gutouch.com/v1/EZICI1364/smartmotic/getbill";
        public static string payBillUrl_CIE = " https://api.gutouch.com/v1/EZICI1364/smartmotic/paybill";
        public static string GetBill_ML = "https://api.gutouch.com/dist/api/v1/EZIML1563/energia/infosachat";
        public static string PayBill_ML = "https://api.gutouch.com/dist/api/v1/EZIML1563/energia/achat";
        //public static string AirtimeArtx = "https://artx.sochitel.com/staging.php";
        public static string AirtimeArtx = "https://artx.sochitel.com/api.php";
        public static string ElectricityBillPaymentUrl_SEN = "https://api.gutouch.com/v1/EZISN5651/senelec/paidBill/v2";

        //keys      
        public static string BankHashSecret = "GTBANKHASHING";
        public static string service_id = "AIRTIMEMTN";
        //public static string secretKey = "KLgt5$#@sd98ujbVgf96R";
        //public static string ApiKey = "86459B68-264B-44DD-A2B0-BC7EB1EB76D1";
        //public static string MerchantId = "86459B68-264B-44DD-A2B0-BC7EB1EB76D1";
        //public static string secretKey = "YT56$rdf9iu76$ghlopu";
        //public static string ApiKey = "8F6064D4-7149-4AC0-911A-4ED5E5C8165C";
        public static string MerchantId = "86459B68-264B-44DD-A2B0-BC7EB1EB76D1";
        public static string secretKey = "YT56$rdf9iu76$ghlopu";
        public static string ApiKey = "8F6064D4-7149-4AC0-911A-4ED5E5C8165C";
        public static string ApiKeyCamroon = "aa9ebe6f-8182-40bc-b772-9651de112a52";
        public static string secretKeyCamroon = "b96fdc94-6c29-11ec-90d6-0242ac120003";
        
        //key for pay services francophone      
        public static string call_back_url = "gutouch.com";
        public static string login_api = "22222223";
        public static string password_api = "2CK4kbmLGR"; // "0000";
        public static string partner_id = "CI2364";

        //key for pay services burkina
        public static string call_back_url_BF = "";
        public static string login_api_BF = "22666667";
        public static string password_api_BF = "V3wSWEhQTH";// "0000";
        public static string partner_id_BF = "BF0002";
        public static string service_id_BF = "BF_AIRTIME_ORANGE";

        //keys for pay services mali
        public static string call_back_url_ML = "gutouch.com";
        public static string login_api_ML = "55555556";
        public static string password_api_ML = "TqnPquEPAA"; // "0000";
        public static string partner_id_ML = "ML2440";
        public static string service_id_ML = "AIRTIMEMALITEL";
        //keys for pay services SENEGAL electricity BillPayment
        public static string call_back_url_SN = "";
        public static string login_api_SN = "551112234";
        public static string password_api_SN = "0000";
        public static string partner_id_SN = "EP0001";
        public static string service_id_SN = "CASHINSENELEC";
        public static string partner_transaction_id = "1551120921928";
        //keys for pay services SENEGAL prepaid BillPayment
        public static string call_back_url_SN_prepaid = "";
        public static string login_api_SN_prepaid = "72000002";
        public static string password_api_SN_prepaid = "fdNAE6nLLb"; //"TwMhWXkHTf"; // "0000";
        public static string partner_id_SN_prepaid = "PG8413";
        public static string service_id_SN_prepaid = "SN_WOYOFAL_SENELEC";

        //keys for pay services SENEGAL water BillPayment
        public static string call_back_url_SNWater = "";
        public static string login_api_SNWater = "72000002";
        public static string password_api_SNWater = "fdNAE6nLLb"; //"TwMhWXkHTf"; // "0000";
        public static string partner_id_SNWater = "PG8413";
        public static string service_id_SNWater = "CASHINSDE";
        public static string partner_transaction_id_SNWater = "1551120921923";
        //keys for pay services SENEGAL Pay services
        public static string service_id_SNG = "AIRTIMEORANGE";
        public static string partner_id_SNG = "PG8413";
        public static string login_api_SNG = "72000002";
        public static string password_api_SNG = "fdNAE6nLLb"; //"TwMhWXkHTf"; // "0000";
        public static string call_back_url_SNG = "";
        //keys for pay services Ivory coast Get bill detail
        public static string partnerId = "CI2364";
        public static string loginApi = "22222223";
        public static string passwordApi = "2CK4kbmLGR"; // "0000";
        public static string facturier = "CIE";
        public static string serviceId = "CI_FACTURE_CIE";
        //keys for pay services Ivory coast Get bill detail
        public static string partnerId_Cie = "CI2364";
        public static string loginApi_Cie = "22222223";
        public static string passwordApi_Cie = "2CK4kbmLGR"; // "0000";
        public static string facturier_Cie = "CIE";
        public static string serviceId_Cie = "CI_FACTURE_CIE";
        //keys for pay services  Get bill detail For mali
        public static string partnerId_ML = "ML2440";
        public static string loginApi_ML = "55555556";// "0000";
        public static string passwordApi_ML = "TqnPquEPAA";
        public static string facturier_ML = "CIE";
        public static string serviceId_ML = "ML_FC_ENERGIA";

        public static string facturier_Sodeci = "SODECI";
        public static string serviceId_Sodeci = "CI_FACTURE_SODECI";
        //keys for pay services Ivory coast Pay bill detail
        public static string partnerId_Pay = "CI666";
        public static string loginApi_Pay = "66666666";
        public static string passwordApi_Pay = "0000";
        public static string merchant_Pay = "CIE";
        public static string serviceId_Pay = "CI_FACTURE_CIE";

        //keys for pay services Ivory coast Pay bill detail
        public static string partnerId_WtPay = "CI2364";
        public static string loginApi_WtPay = "22222223";
        public static string passwordApi_WtPay = "2CK4kbmLGR"; // "0000";
        public static string merchant_WtPay = "SODECI";
        public static string serviceId_WtPay = "CI_FACTURE_SODECI";

        //keys for pay services senegal Get bill detail
        public static string username_SN = "94E212F6CFF7334FF371B91612DA7AFEE1832C2972C81D6FFAD6C212ED14C1F5";
        public static string password_SN = "B538B1B9407990154271132468CC06FF80D5894DA3ADE0ABDED44CDF7B992C8A";
        public static string partnerId_SN = "PG8413";
        public static string loginApi_SN = "72000002";
        public static string passwordApi_SN = "fdNAE6nLLb"; //"TwMhWXkHTf"; // "0000";
        public static string getBillUrl_SN = "https://api.gutouch.com/v1/EZISN5651/hexing/meterInfos";
        public static string getBillUrl_SDE_SN = "https://api.gutouch.com/v1/EZISN5651/sde/searchBill";
        public static string getBillUrl_SDE_SENELEC = "https://api.gutouch.com/v1/EZISN5651/senelec/searchBill/v2";
        public static string username_CIE = "BF5C0D75E110EDA801651D948BA1E699D707421336CA0B141001F2986ABC54CB";
        public static string password_CIE = "61874536F5E32D89FD82C5209E548BB181E8474F8949016CB7DBD1072A82EB66";
    }
    public static class AggregatorySTATUSCODES
    {
        public static string SUCCESSFUL = "200";
        public static string PENDING = "300";
        public static string PENDINGTxn = "303";
        public static string FAILED = "404";
        public static string EXCEPTION = "506";
        public static string INVOICEEXIST = "113";
        public static string SIGNATURENOTVALID = "112";
        public static string INVALIDINVOICENUMBER = "113";

    }
    public static class InternationalDTHAggregatorySTATUSCODES
    {
        public static string SUCCESSFUL = "200";
        public static string PENDING = "300";
        public static string FAILED = "404";
        public static string EXCEPTION = "506";
        public static string INVOICEEXIST = "113";
        public static string Insufficient_balance_in_your_master_account = "1000777";
        public static string Insufficient_balance_in_your_retailer_account = "1000888";
        public static string Invalid_parameter = "1000999";
        public static string Account_number_incorrect = "1000204";
        public static string Transaction_amount_limit_exceeded = "1000207";
        public static string Transaction_already_paid = "1000212";
        public static string Transaction_repeated = "1000213";
        public static string Transaction_rejected = "1000214";
        public static string Transaction_timeout = "1000218";
        public static string Recipient_reached_maximum_transaction_number = "1000230";
        public static string Product_not_available = "1000301";
        public static string Product_not_compatible_with_transaction_type = "1000302";
        public static string Product_type_incorrect = "1000303";
        public static string Account_verification_not_available_for_this_product = "1000304";
        public static string External_id_already_used = "1000990";
        public static string Unauthorized = "1000401";
        public static string Transaction_not_found = "1000404";
        public static string Internal_server_error = "1000500";
    }


    public static class InternationalAggregatorySTATUSCODES
    {
        public static string SUCCESSFUL = "200";
        public static string PENDING = "300";
        public static string FAILED = "404";
        public static string EXCEPTION = "506";
        public static string INVOICEEXIST = "113";

        public static string destination_number_is_not_avalid = "204";
        public static string input_value_out_of_range = "301";
        public static string transaction_refused_by_the_operator = "214";
        public static string system_not_available = "998";
        public static string unknown_error = "999";
        public static string servicetothisdestinationoperatoristemporarilyunavailable = "215";
        public static string Recipientreachedmaximumtopupamount = "231";


    }
    public static class AggregatoryMESSAGE
    {
        public static string SUCCESSFUL = "SUCCESSFUL";
        public static string PENDING = "PENDING";
        public static string FAILED = "FAILED";

        public static string destination_number_is_not_avalid = "Destination number is not a valid prepaid phone number";
        public static string servicetothisdestinationoperatoristemporarilyunavailable = "Service to this destination operator is temporarily unavailable";
        public static string Recipientreachedmaximumtopupamount = "Recipient reached maximum topup amount";

        public static string input_value_out_of_range = "input value out of range or invalid produc";
        public static string transaction_refused_by_the_operator = "Transaction refused by the operator";
        public static string system_not_available = "System not available, please retry later";
        public static string unknown_error = "Unknown error, please contact support";

        public static string Insufficient_balance_in_your_master_account = "Insufficient balance in your master account";
        public static string Insufficient_balance_in_your_retailer_account = "Insufficient balance in your retailer account";
        public static string Invalid_parameter = "Invalid parameter";
        public static string Account_number_incorrect = "Account number incorrect";
        public static string Transaction_amount_limit_exceeded = "Transaction amount limit exceeded";
        public static string Transaction_already_paid = "Transaction already paid";
        public static string Transaction_repeated = "Transaction repeated";
        public static string Transaction_rejected = "Transaction rejected";
        public static string Transaction_timeout = "Transaction timeout";
        public static string Recipient_reached_maximum_transaction_number = "Recipient reached maximum transaction number";
        public static string Product_not_available = "Product not available";
        public static string Product_not_compatible_with_transaction_type = "Product not compatible with transaction type";
        public static string Product_type_incorrect = "Product type incorrect";
        public static string Account_verification_not_available_for_this_product = "Account verification not available for this product";
        public static string External_id_already_used = "External id already used";
        public static string Unauthorized = "Unauthorized";
        public static string Transaction_not_found = "Transaction not found";
        public static string Internal_server_error = "Internal server error";
    }

    public static class AggragatorServiceCategory
    {
        public static string Airtime = "AIRTIME";
        public static string MobileMoney = "MOBILEMONEY";
        public static string Isp = "ISP";
        public static string General = "GENERAL";
        public static string Merchant = "MERCHANT:";
    }
    public static class AggragatorServiceType
    {
        public static string DEBIT = "DEBIT";
        public static string CREDIT = "CREDIT";
        public static string CASHDEPOSITTOBANK = "CASHDEPOSITTOBANK";
    }
    public static class AggragatorServiceVerbs
    {
        public static string HttpPostVerb = "Post";
        public static string HttpGetVerb = "Get";
    }

    public static class ThirdPartyApiUrl
    {
        public static string GetBundles = "http://52.40.89.233/aggregator/api/bundles?datanumber=";

        public static string GetBundles_Surfline = "http://52.40.89.233/aggregator/api/surflinebundle?customer=";

        public static string GetBundles_DataBundles = "http://52.40.89.233/aggregator/api/databundles?apikey=57F68FC7-97AB-403B-8BD0-7BF50AC13423";
        public static string GetBundles_MTNFIBER = "http://52.40.89.233/aggregator/api/mtn";
    }


    public static class TransferToBankApiSetting
    {
        public static string TransferToBankGTBankUrl = ConfigurationManager.AppSettings["TransferToBankGTBankUrl"];//"http://196.216.228.43/WalletApi/api/user/transfertobank"; //ConfigurationManager.AppSettings["TransferToBankApiURL"];
        public static string TransferToBankUrl = ConfigurationManager.AppSettings["TransferToBankApiURL"];// "https://196.216.228.129/aggregatorservices/ipay/";
        public static string Username = ConfigurationManager.AppSettings["TransferToBankUserName"];
        public static string Password = ConfigurationManager.AppSettings["TransferToBankPassword"];
        public static string GTbankUsername = ConfigurationManager.AppSettings["GTbankTransferToBankUserName"];
        public static string GTbankPassword = ConfigurationManager.AppSettings["GTbankTransferToBankPassword"];
        public static string SourceAccountNo = ConfigurationManager.AppSettings["TransferToBankSourceAccountNo"];
        public static string SourceAccountName = ConfigurationManager.AppSettings["TransferToBankSourceAccountName"];
    }
    public static class TransferToBankApiMethodList
    {
        public static string BankList = "query/ACHBanks";
        public static string BeneficiaryName = "query/BeneficiaryName";
        public static string SubmitCredit = "post/SubmitCredit";
    }
    public class CommonSetting
    {
        public static string binancepaymentUrl = ConfigurationManager.AppSettings["binancepaymentUrl"];
        public static string binancepaymenttransferUrl = ConfigurationManager.AppSettings["binancepaymenttransferUrl"];
        public static string binanceapikee = ConfigurationManager.AppSettings["binanceapikee"];
        public static string binanceSECKee = ConfigurationManager.AppSettings["binanceSECKee"];


        public static string flutterCallBackUrl = ConfigurationManager.AppSettings["flutterCallBackUrl"];
        public static string flutterpaymentUrl = ConfigurationManager.AppSettings["flutterpaymentUrl"];
        public static string flutterFLWSECKey = ConfigurationManager.AppSettings["flutterFLWSECKey"];
        public static string flutterverifypaymentUrl = ConfigurationManager.AppSettings["flutterverifypaymentUrl"];

        public static string flutterbankpaymentUrl = ConfigurationManager.AppSettings["flutterbankpaymentUrl"];
        public static string flutterBankCallBackUrl = ConfigurationManager.AppSettings["flutterBankCallBackUrl"];
        public static string flutterSendBankUrl = ConfigurationManager.AppSettings["flutterSendBankUrl"];

        public static string flutterFLWBankNGN = ConfigurationManager.AppSettings["flutterFLWBankNGN"];
        public static string flutterbankZenithBankOTPRequest = ConfigurationManager.AppSettings["flutterbankZenithBankOTPRequest"];
        public static string flutterbankBanktransfer = ConfigurationManager.AppSettings["flutterbankBanktransfer"];

        
        //NgeniunsResponse

        public static string Ngeniunstokenkey = ConfigurationManager.AppSettings["Ngeniunstokenkey"];
        public static string NgeniunstokenUrl = ConfigurationManager.AppSettings["NgeniunstokenUrl"];
        public static string NgeniunsAPIKEY = ConfigurationManager.AppSettings["NgeniunsAPIKEY"];
        public static string Ngeniunscancel_url = ConfigurationManager.AppSettings["cancel_url"];
        public static string NgeniunspaymentsUrl = ConfigurationManager.AppSettings["NgeniunspaymentsUrl"];

        public static string NgeniunsCallBack = ConfigurationManager.AppSettings["NgeniunsCallBack"];
        public static string GetorderstatusNgeniunspayment = ConfigurationManager.AppSettings["GetorderstatusNgeniunspayment"];
        //seerbit

        public static string Seerbitpublickey = ConfigurationManager.AppSettings["Seerbitpublickey"];
        public static string Seerbitprivatekey = ConfigurationManager.AppSettings["Seerbitprivatekey"];      
        public static string SeerbitGettokenUrl = ConfigurationManager.AppSettings["SeerbitGettokenUrl"];
        public static string SeerbitGethashUrl = ConfigurationManager.AppSettings["SeerbitGethashUrl"];
        public static string SeerbitGetpaymentsUrl = ConfigurationManager.AppSettings["SeerbitGetpaymentsUrl"];
        public static string SeerbitCallBack = ConfigurationManager.AppSettings["SeerbitCallBack"];

        public static string MasterCardUrl2 = ConfigurationManager.AppSettings["MasterCardUrl2"];
        public static string MasterCardCallBackForPayServicesNewFlow2 = ConfigurationManager.AppSettings["HostNameAdmin"] + ConfigurationManager.AppSettings["MasterCardCallBackForPayServicesNewFlow2"];
        public static string MasterCardUserName2 = ConfigurationManager.AppSettings["MasterCardUserName2"];
        public static string MasterCardPassword2 = ConfigurationManager.AppSettings["MasterCardPassword2"];
        public static string apiUsernameMasterCard2 = ConfigurationManager.AppSettings["apiUsernameMasterCard2"];
        public static string MerchantNameMasterCard2 = ConfigurationManager.AppSettings["MerchantNameMasterCard2"];
        public static string MPGSTemplate2 = ConfigurationManager.AppSettings["MPGSTemplate2"];
        public static string operation2 = ConfigurationManager.AppSettings["operation2"];

        //master card detail -29/09 amit uba
        //public static string mastercardCallback = ConfigurationManager.AppSettings["HostNameAdmin"] + ConfigurationManager.AppSettings["MasterCardCallBack"];
        //public static string MasterCardCallBackForPayServices = ConfigurationManager.AppSettings["HostNameAdmin"] + ConfigurationManager.AppSettings["MasterCardCallBackForPayServices"];
        //public static string MerchantPaymentByUba = ConfigurationManager.AppSettings["HostNameAdmin"] + ConfigurationManager.AppSettings["MerchantPaymentByUba"];

        public static string MasterCardUserName = ConfigurationManager.AppSettings["MasterCardUserName"];
        public static string MPGSTemplate = ConfigurationManager.AppSettings["MPGSTemplate"];
        public static string MasterCardPassword = ConfigurationManager.AppSettings["MasterCardPassword"];
        public static string MasterCardUrl = ConfigurationManager.AppSettings["MasterCardUrl"];
        public static string apiUsernameMasterCard = ConfigurationManager.AppSettings["apiUsernameMasterCard"];
        public static string MerchantNameMasterCard = ConfigurationManager.AppSettings["MerchantNameMasterCard"];

        public static string operation = ConfigurationManager.AppSettings["operation"];
        public static string MasterCardCallBackForPayServicesNewFlow = ConfigurationManager.AppSettings["HostNameAdmin"] + ConfigurationManager.AppSettings["MasterCardCallBackForPayServicesNewFlow"];

        public static string API_KEY = ConfigurationManager.AppSettings["API_KEY"];
        public static string secret_KEY = ConfigurationManager.AppSettings["secret_KEY"];
        public static string InternationalDTHUrl = ConfigurationManager.AppSettings["InternationalDTHUrl"];

        ///

        public static string flagImageurl = ConfigurationManager.AppSettings["flagImageurl"];
        public static string imageUrl = ConfigurationManager.AppSettings["ImageUrl"];
        public static string isVerifiedAccount = ConfigurationManager.AppSettings["IsVerifiedAccount"];
        public static string S3ServiceURL = ConfigurationManager.AppSettings["AWSurl"];
        public static string AWS_ACCESS_KEY_ID = ConfigurationManager.AppSettings["AWSAccessKey"];
        public static string AWS_SECRET_ACCESS_KEY = ConfigurationManager.AppSettings["AWSSecretKey"];
        public static string AWS_BUCKET = ConfigurationManager.AppSettings["AWSBucket"];
        public static string LogoPath = ConfigurationManager.AppSettings["LogoPath"];
        public static int PageSize = Convert.ToInt32(ConfigurationManager.AppSettings["PageSize"]);
        public static int LimitAmount = Convert.ToInt32(ConfigurationManager.AppSettings["CommissionAmountLimit"]);
        public static string successfullTransaction = "/email.html";
        public static string resortBooking = "/ResortBooking.html";
        public static string qrCodeShare = "/QrCode.html";
        public static string AlphaNumericString(int stringLength)
        {
            Random random = new Random();


            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!@#$%&*()0123456789";
            return new string(Enumerable.Repeat(chars, stringLength)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static string HostNameAdmin = ConfigurationManager.AppSettings["HostNameAdmin"];
        public static string QrCodeUrl = HostNameAdmin + "/" + ConfigurationManager.AppSettings["QrCodeUrl"];
        public static int OtpLimit = Convert.ToInt32(ConfigurationManager.AppSettings["OtpLimit"]);
        public static string VerifyMailLink = HostNameAdmin + "/" + ConfigurationManager.AppSettings["VerifyMailLink"];
        public static string EmailVerificationTemplate = "/EmailVerificationTemplate.html";
        public static string ForgotPasswordEmailTemplate = "/ForgotPasswordEmailTemplate.html";
        //email creds
        public static string EmailFrom = ConfigurationManager.AppSettings["EmailFrom"];
        public static string Host = ConfigurationManager.AppSettings["Host"];
        public static string Port = ConfigurationManager.AppSettings["Port"];
        public static string Username = ConfigurationManager.AppSettings["Username"];
        public static string Password = ConfigurationManager.AppSettings["Password"];
        public static string GetOtp()
        {
            Random random = new Random();
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, 4).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string GetUniqueNumber()
        {
            Random random = new Random();
            const string numbers = "0123456789";

            return "EZ-" + (DateTime.UtcNow.Date.Year).ToString() + "-" + new string(Enumerable.Repeat(numbers, 4).Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static string TempPassword()
        {
            Random random = new Random();
            const string WordCapital = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string WordSmall = "abcdefghijklmnopqrstuvxyz";
            const string num = "0123456789";
            const string sChars = @"!@#$%^&*()_+=|}{[]:;'/.,?><|\";
            string str = new string(Enumerable.Repeat(WordCapital, 1).Select(s => s[random.Next(s.Length)]).ToArray());
            str += new string(Enumerable.Repeat(WordSmall, 4).Select(s => s[random.Next(s.Length)]).ToArray());
            str += new string(Enumerable.Repeat(sChars, 1).Select(s => s[random.Next(s.Length)]).ToArray());
            str += new string(Enumerable.Repeat(num, 2).Select(s => s[random.Next(s.Length)]).ToArray());
            return str;

        }

        public static string branch_key = ConfigurationManager.AppSettings["branch_key"];
        public static string branchio_url = ConfigurationManager.AppSettings["branchio_url"];
        public static string android_url = ConfigurationManager.AppSettings["android_url"];

        //Service ids for Ci
        public static string Password_Ci = ConfigurationManager.AppSettings["Password_Ci"];
        public static string ServiceId_MTN = ConfigurationManager.AppSettings["ServiceId_MTN"];
        public static string ServiceId_Moov = ConfigurationManager.AppSettings["ServiceId_Moov"];
        public static string ServiceId_Orange = ConfigurationManager.AppSettings["ServiceId_Orange"];


        public static string MerchantToken = ConfigurationManager.AppSettings["MerchantToken"];
        public static string loginkey = ConfigurationManager.AppSettings["loginkey"];
        public static string InternationalAirtimeUrl = ConfigurationManager.AppSettings["InternationalAirtimeUrl"];

        public static string AutoReadOtpId = ConfigurationManager.AppSettings["AutoReadOtpId"];
        
    }

    public class Error
    {
        public int code { get; set; }
        public string message { get; set; }
    }

    public class ErrorResponseForDTH
    {
        public ErrorResponseForDTH()
        {
            errors = new List<Error>();
        }
        public List<Error> errors { get; set; }
    }
}
