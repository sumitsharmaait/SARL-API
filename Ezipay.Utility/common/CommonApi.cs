using Ezipay.Utility.Extention;
using Ezipay.ViewModel.AirtimeFrVm;
using Ezipay.ViewModel.AirtimeViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Utility.common
{
    public class CommonApi
    {
        public async Task<string> GetUser(string req, string url)
        {
            string resString = "";
            string resBody = "";
            RootObject responseData = new RootObject();
            using (HttpClient client = new HttpClient())
            {
                // Call asynchronous network methods in a try/catch block to handle exceptions
                try
                {
                    var content = new StringContent(req, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(url, content);
                    response.EnsureSuccessStatusCode();
                    resBody = await response.Content.ReadAsStringAsync();
                    // Above three lines can be replaced with new helper method below
                    // string responseBody = await client.GetStringAsync(uri);
                    //GetUserDetailResponse                   
                    var userDetail = JsonConvert.DeserializeObject<RootObject>(resBody);
                    responseData.isSuccess = userDetail.isSuccess;
                    responseData.status = userDetail.status;
                    responseData.message = userDetail.message;
                    responseData.result.WalletUserId = userDetail.result.WalletUserId;
                    responseData.result.FirstName = userDetail.result.FirstName;
                    responseData.result.LastName = userDetail.result.LastName;
                    responseData.result.MobileNo = userDetail.result.MobileNo;
                    responseData.result.StdCode = userDetail.result.StdCode;
                    responseData.result.EmailId = userDetail.result.EmailId;
                    responseData.result.Currentbalance = userDetail.result.Currentbalance;


                    // Console.WriteLine(resBody);
                    resString = JsonConvert.SerializeObject(responseData);
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
                return resString;
            }
        }
        public async Task<string> PayServices(string req, string webUrl, string username, string password)
        {
            //string username = "MTN";
            //string password = "passer";
            PayServicesResponseForServices pay = new PayServicesResponseForServices();
            string responseBody = "";
            string responseString = "";
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    //req= "{'service_id':'AIRTIMEMTN','recipient_phone_number':'45612378','amount':11,'partner_id':'CI1234','partner_transaction_id':'EZZ - 2019 - 85','login_api':'77987654','password_api':'0000','call_back_url':'gutouch.com'}";

                    client.BaseAddress = new Uri(webUrl);
                    // client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    //Set Basic Auth          
                    var content = new StringContent(req, Encoding.UTF8, "application/json");
                    var base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64String);

                    var result = await client.PostAsync(webUrl, content);
                    result.EnsureSuccessStatusCode();


                    responseBody = await result.Content.ReadAsStringAsync();
                    // responseBody = "{\"service_id\":\"AIRTIMEMOOV\",\"gu_transaction_id\":\"1571842511720\",\"status\":\"PENDING\",\"transaction_date\":\"2019/10/23 14:55:11 PM\",\"recipient_phone_number\":\"73374961\",\"amount\":104.0,\"partner_transaction_id\":\"EZZ-2019-6178\",\"StatusCode\":200,\"responseString\":\"{\\\"service_id\\\":\\\"AIRTIMEMOOV\\\",\\\"gu_transaction_id\\\":\\\"1571842511720\\\",\\\"status\\\":\\\"PENDING\\\",\\\"transaction_date\\\":\\\"2019/10/23 14:55:11 PM\\\",\\\"recipient_phone_number\\\":\\\"73374961\\\",\\\"amount\\\":104.0,\\\"partner_transaction_id\\\":\\\"EZZ-2019-6178\\\"}\",\"message\":null,\"meterStatus\":null,\"meterNo\":null,\"krn\":null,\"rspCod\":null,\"custType\":null,\"enelId\":null,\"customerId\":null,\"lastVendDate\":null,\"ti\":null,\"rspMsg\":null,\"address\":null,\"ccy\":null,\"sessionId\":null,\"customerName\":null,\"recipient_id\":null,\"customer_reference\":null,\"recipient_invoice_id\":null}";
                    //  responseString = responseBody.Result.ToString();
                    var dataSer = JsonConvert.DeserializeObject<PayServicesResponseForServices>(responseBody);
                    pay.responseString = responseBody;
                    //pay.StatusCode =Convert.ToInt32(result.StatusCode);
                    pay.service_id = dataSer.service_id;
                    pay.partner_transaction_id = dataSer.partner_transaction_id;
                    pay.recipient_phone_number = dataSer.recipient_phone_number;
                    pay.gu_transaction_id = dataSer.gu_transaction_id;
                    pay.transaction_date = dataSer.transaction_date;
                    pay.status = dataSer.status;
                    pay.amount = dataSer.amount;
                    pay.meterNo = dataSer.meterNo;
                    pay.sessionId = dataSer.sessionId;

                    responseString = JsonConvert.SerializeObject(pay);
                   

                }
                catch (Exception ex)
                {
                    responseString ="";
                }
            }
            return responseString;
        }


        public async Task<bool> PayMoneyToGhanaUser(string req, string url)
        {
            string resString = "";
            string resBody = "";
            RootObject responseData = new RootObject();
            using (HttpClient client = new HttpClient())
            {
                // Call asynchronous network methods in a try/catch block to handle exceptions
                try
                {
                    var content = new StringContent(req, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(url, content);
                    response.EnsureSuccessStatusCode();
                    resBody = await response.Content.ReadAsStringAsync();
                    // Above three lines can be replaced with new helper method below
                    // string responseBody = await client.GetStringAsync(uri);
                    //GetUserDetailResponse                   
                    //var userDetail = JsonConvert.DeserializeObject<RootObject>(resBody);
                    //responseData.isSuccess = userDetail.isSuccess;
                    //responseData.status = userDetail.status;
                    //responseData.message = userDetail.message;
                    //responseData.result.WalletUserId = userDetail.result.WalletUserId;
                    //responseData.result.FirstName = userDetail.result.FirstName;
                    //responseData.result.LastName = userDetail.result.LastName;
                    //responseData.result.MobileNo = userDetail.result.MobileNo;
                    //responseData.result.StdCode = userDetail.result.StdCode;
                    //responseData.result.EmailId = userDetail.result.EmailId;
                    //responseData.result.Currentbalance = userDetail.result.Currentbalance;


                    Console.WriteLine(resBody);
                    //resString = JsonConvert.SerializeObject(responseData);
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
                return true;
            }
        }


        public async Task<string> GetBillDetail(string req, string webUrl, string username, string password)
        {
            //string username = "MTN";
            //string password = "passer";
            GetBillResponse getBillResponse = new GetBillResponse();
            string responseBody = "";
            string responseString = "";
            using (HttpClient client = new HttpClient())
            {
                try
                {


                    client.BaseAddress = new Uri(webUrl);
                    var content = new StringContent(req, Encoding.UTF8, "application/json");
                    var base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64String);

                    var result = await client.PostAsync(webUrl, content);
                    result.EnsureSuccessStatusCode();
                    // Above three lines can be replaced with new helper method below
                    // string respon = await client.GetStringAsync(webUrl);               
                    responseString = await result.Content.ReadAsStringAsync();
                    // responseString = responseBody.Result.ToString();
                    var dataSer = JsonConvert.DeserializeObject<GetBillResponse>(responseBody);
                    //getBillResponse.dateLimite = dataSer.dateLimite;
                    //getBillResponse.codeExpiration = dataSer.codeExpiration;
                    //getBillResponse.merchant = dataSer.merchant;
                    //getBillResponse.totAmount = dataSer.totAmount;
                    //getBillResponse.typeFacture = dataSer.typeFacture;
                    //getBillResponse.heureEnreg = dataSer.heureEnreg;
                    //getBillResponse.refBranch = dataSer.refBranch;
                    //getBillResponse.numFacture = dataSer.numFacture;
                    //getBillResponse.idAbonnement = dataSer.idAbonnement;
                    //getBillResponse.fees = dataSer.fees;
                    //getBillResponse.sms = dataSer.sms;
                    //getBillResponse.dateEnreg = dataSer.dateEnreg;
                    //getBillResponse.perFacture = dataSer.perFacture;
                    //getBillResponse.message = dataSer.message;
                    //getBillResponse.status = dataSer.status;


                    //responseString = JsonConvert.SerializeObject(getBillResponse);

                }
                catch (Exception ex)
                {

                }
            }
            return responseString;
        }

        public async Task<string> BillPayment(string req, string webUrl, string username, string password)
        {
            //string username = "MTN";
            //string password = "passer";
            BillPayServicesResponse response = new BillPayServicesResponse();
            PayServicesResponseForServices payServicesResponseForServices = new PayServicesResponseForServices();
            string responseBody = "";
            string responseString = "";
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    //req= "{'service_id':'AIRTIMEMTN','recipient_phone_number':'45612378','amount':11,'partner_id':'CI1234','partner_transaction_id':'EZZ - 2019 - 85','login_api':'77987654','password_api':'0000','call_back_url':'gutouch.com'}";

                    client.BaseAddress = new Uri(webUrl);
                    // client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    //Set Basic Auth          
                    var content = new StringContent(req, Encoding.UTF8, "application/json");
                    var base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64String);

                    var result = await client.PostAsync(webUrl, content);
                    result.EnsureSuccessStatusCode();

                    // Above three lines can be replaced with new helper method below
                    //string respon = await client.GetStringAsync(webUrl);               
                    responseBody = await result.Content.ReadAsStringAsync();
                    //  responseString = responseBody.Result.ToString();
                    //var dataSer = JsonConvert.DeserializeObject<BillPayServicesResponse>(responseBody);
                    //payServicesResponseForServices.responseString = responseBody;
                    //payServicesResponseForServices.gu_transaction_id = dataSer.guTransactionId;
                    //payServicesResponseForServices.transaction_date = dataSer.transactionDate;
                    //payServicesResponseForServices.recipient_phone_number = dataSer.recipientId;
                    //payServicesResponseForServices.amount =Convert.ToDecimal (dataSer.amount);
                    //payServicesResponseForServices.status = dataSer.status;
                    //payServicesResponseForServices.message = dataSer.message;
                    //payServicesResponseForServices.statusCode =Convert.ToInt32(result.StatusCode);

                    //  responseString = JsonConvert.SerializeObject(payServicesResponseForServices);

                }
                catch (Exception ex)
                {

                }
            }
            return responseBody;
        }

        public async Task<string> GetOperatorAirtime(string req, string url)
        {
            string resString = "";
            string resBody = "";
            RootObject responseData = new RootObject();
            using (HttpClient client = new HttpClient())
            {
                // Call asynchronous network methods in a try/catch block to handle exceptions
                try
                {
                    var content = new StringContent(req, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(url, content);
                    response.EnsureSuccessStatusCode();
                    resBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(resBody);
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
                return resBody;
            }
        }

        public async Task<string> PaymentAirtime(string req, string url)
        {
            string resString = "";
            string resBody = "";
            RootObject responseData = new RootObject();
            using (HttpClient client = new HttpClient())
            {
                // Call asynchronous network methods in a try/catch block to handle exceptions
                try
                {
                    var content = new StringContent(req, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(url, content);
                    response.EnsureSuccessStatusCode();
                    resBody = await response.Content.ReadAsStringAsync();                   
                }
                catch (HttpRequestException e)
                {
                    e.Message.ErrorLog("Airtime",e.Message);
                    LogTransactionTypes.Response.SaveTransactionLog("Airtime exception", "", "Exception Occured : " + e.Message);
                    resBody = "{\"StatusCode\":\"300\",\"Message\":\"PENDING\",\"TransactionId\":\"\",\"InvoiceNo\":\"\"}";
                }
                return resBody;
            }
        }

        public async Task<string> PaymentMobileMon(string req, string url)
        {
            string resString = "";
            string resBody = "";
            RootObject responseData = new RootObject();
            using (HttpClient client = new HttpClient())
            {
                // Call asynchronous network methods in a try/catch block to handle exceptions
                try
                {
                    var content = new StringContent(req, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(url, content);
                    response.EnsureSuccessStatusCode();
                    resBody = await response.Content.ReadAsStringAsync();
                }
                catch (HttpRequestException e)
                {
                    e.Message.ErrorLog("PayMobileMon", e.Message);
                    LogTransactionTypes.Response.SaveTransactionLog("PayMobileMon exception", "", "Exception Occured : " + e.Message);
                    resBody = "{\"StatusCode\":\"300\",\"Message\":\"PENDING\",\"TransactionId\":\"\",\"InvoiceNo\":\"\"}";
                }
                return resBody;
            }
        }
    }
}
