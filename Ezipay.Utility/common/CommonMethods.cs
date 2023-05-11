//using Amazon.S3;
using Ezipay.Utility.Extention;
using Ezipay.ViewModel.AirtimeViewModel;
using Ezipay.ViewModel.BundleViewModel;
using Ezipay.ViewModel.ThridPartyApiVIewModel;
using Ezipay.ViewModel.WalletUserVM;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using static Ezipay.Utility.common.AppSetting;

namespace Ezipay.Utility.common
{
    public class CommonMethods
    {
        public string GetQueryString(object obj)
        {
            var properties = from p in obj.GetType().GetProperties()
                             where p.GetValue(obj, null) != null
                             select p.Name + "=" + HttpUtility.UrlEncode(p.GetValue(obj, null).ToString());

            return String.Join("&", properties.ToArray());
        }

        public String MD5Hash(MobileMoneyAggregatoryRequest request)
        {
            StringBuilder sb = new StringBuilder();
            //Url for Payment
            sb.Append(request.apiKey);
            sb.Append(request.customer);
            sb.Append(request.amount);
            sb.Append(request.invoiceNo);
            sb.Append(ThirdPartyAggragatorSettings.secretKey);
            StringBuilder hash = new StringBuilder();
            MD5CryptoServiceProvider md5provider = new MD5CryptoServiceProvider();
            byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(sb.ToString()));

            for (int i = 0; i < bytes.Length; i++)
            {
                hash.Append(bytes[i].ToString("x2"));
            }
            return hash.ToString();
        }

       

        //public async Task<string> Example(string Log, string Url, object PostData, object Request, string CategoryName)
        //{
        //    //The data that needs to be sent. Any object works.
        //    //var pocoObject = new
        //    //{
        //    //    Name = "John Doe",
        //    //    Occupation = "gardener"
        //    //};

        //    //Converting the object to a json string. NOTE: Make sure the object doesn't contain circular references.
        //    string json = JsonConvert.SerializeObject(PostData);

        //    //Needed to setup the body of the request
        //    StringContent data = new StringContent(json, Encoding.UTF8, "application/json");

        //    //The url to post to.
        //    var url = Url;
        //    var client = new HttpClient();

        //    //Pass in the full URL and the json string content
        //    var response = await client.PostAsync(url, data);

        //    //It would be better to make sure this request actually made it through
        //    string result = await response.Content.ReadAsStringAsync();

        //    //close out the client
        //    client.Dispose();

        //    return result;
        //}
        public string HttpPostUrlEncodedService(string Log, string Url, object PostData, object Request, string CategoryName)
        {
            string detail = "Url encoded post request for " + CategoryName;
            string responseString = string.Empty;
            try
            {

                String QueryString = GetQueryString(PostData);

                ASCIIEncoding ascii = new ASCIIEncoding();
                byte[] postBytes = ascii.GetBytes(QueryString.ToString());

                // set up request object
                HttpWebRequest request;
                try
                {
                    request = (HttpWebRequest)HttpWebRequest.Create(Url);
                }
                catch (UriFormatException)
                {
                    request = null;
                }

                LogTransactionTypes.Request.SaveTransactionLog(Log + CategoryName, Request, detail);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = postBytes.Length;


                // add post data to request

                using (Stream postStream = request.GetRequestStream())
                {
                    postStream.Write(postBytes, 0, postBytes.Length);
                    postStream.Flush();
                    postStream.Close();
                    using (var _response = (HttpWebResponse)request.GetResponse())
                    {
                        using (var result = new StreamReader(_response.GetResponseStream()))
                        {

                            responseString = result.ReadToEnd();
                        }
                    }
                }
                //responseString = "{\"StatusCode\":\"506\",\"Message\":\"FAILED\",\"TransactionId\":\"\",\"InvoiceNo\":\"\"}";
                //responseString = "{\"StatusCode\":\"300\",\"Message\":\"PENDING\",\"TransactionId\":\"\",\"InvoiceNo\":\"\"}";
                //responseString = "{\"StatusCode\":\"200\",\"Message\":\"SUCCESS\",\"TransactionId\":\"\",\"InvoiceNo\":\"\"}";
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog(Log + CategoryName, Log + CategoryName, Log + CategoryName);

                LogTransactionTypes.Response.SaveTransactionLog(Log + CategoryName, detail + Request, "Exception Occured : " + ex.Message);
                responseString = "{\"StatusCode\":\"506\",\"Message\":\"FAILED\",\"TransactionId\":\"\",\"InvoiceNo\":\"\"}";
            }
            return responseString;
        }

        public string HttpGetUrlEncodedService(string Log, string Url, object parameters, object Request, string CategoryName)
        {
            string detail = string.Empty;
            string responseString = string.Empty;

            string requestQueryString = string.Empty;
            try
            {
                using (WebClient wc = new WebClient())
                {
                    try
                    {
                        string serviceType = ((PayMoneyAggregatoryRequest)Request).serviceCategory;
                        if (serviceType.ToUpper() == "ISP")
                        {
                            string accountNo = ((MobileMoneyAggregatoryRequest)parameters).customer;
                            string userAmount = ((MobileMoneyAggregatoryRequest)parameters).amount;
                            string channelId = ((MobileMoneyAggregatoryRequest)parameters).channel;
                            string[] result = accountNo.Split(',');
                            string customer = result[0];
                            string planId = result[1];

                            bool checkResult = GetBundles(planId, Convert.ToDecimal(userAmount), channelId, customer);

                            if (!checkResult)
                            {
                                responseString = "{\"StatusCode\":\"5006\",\"Message\":\"Invalid bundle amount\",\"TransactionId\":\"\",\"InvoiceNo\":\"\"}";
                                return responseString;
                            }
                        }
                    }
                    catch
                    {

                    }

                    requestQueryString = GetQueryString(parameters);
                    detail = "Aggregator Url : " + (Url + "?" + requestQueryString);
                    LogTransactionTypes.Request.SaveTransactionLog(Log + CategoryName, Request, detail);
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    responseString = wc.UploadString(Url, requestQueryString);// "{\"StatusCode\":\"300\",\"Message\":\"PENDING\",\"TransactionId\":\"96662\",\"InvoiceNo\":null}";//
                }
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog(Log + CategoryName, Log + CategoryName, Log + CategoryName);
                LogTransactionTypes.Response.SaveTransactionLog(Log + CategoryName, Request, detail + ", Exception Occured : " + ex.Message);
                responseString = "{\"StatusCode\":\"506\",\"Message\":\"FAILED\",\"TransactionId\":\"\",\"InvoiceNo\":\"\"}";
            }
            return responseString;
        }

        public bool GetBundles(string planId, decimal userAmount, string channel, string account)
        {
            //Response<List<IspBundlesResponce>> Response = new Response<List<IspBundlesResponce>>();
            bool result = false;
            string url = "";
            if (channel.ToLower() == "busy")
            {
                url = ThirdPartyApiUrl.GetBundles + account + "&channel=" + channel;
            }
            else if (channel.ToLower() == "surfline")
            {
                url = ThirdPartyApiUrl.GetBundles_Surfline + "233" + account;
            }
            else if (channel.ToUpper() == "MTNFIBRE")
            {
                url = ThirdPartyApiUrl.GetBundles_MTNFIBER;
            }
            else
            {
                url = ThirdPartyApiUrl.GetBundles_DataBundles + "&network=" + channel;
            }
            var m_strFilePath = url;
            string xmlStr;
            try
            {
                using (var wc = new WebClient())
                {
                    xmlStr = wc.DownloadString(m_strFilePath);
                }
                if (!string.IsNullOrEmpty(xmlStr))
                {
                    if (channel.ToLower() == "busy")
                    {
                        var data = JsonConvert.DeserializeObject<List<List<ISPBundleDataObject>>>(xmlStr);
                        List<IspBundlesResponse> objList = new List<IspBundlesResponse>();
                        var accounttype = "";
                        foreach (var list in data)
                        {
                            foreach (var item in list)
                            {
                                IspBundlesResponse obj = new IspBundlesResponse();
                                obj.Amount = item.Amount;
                                obj.BundleId = item.BundleId;
                                obj.Description = item.Description == "null" ? "" : item.Description;
                                obj.Name = item.Name;
                                obj.DisplayContent = item.Amount + " GHS ," + item.Name;
                                obj.AccountType = accounttype;
                                //objList.Add(obj);
                                if (planId == obj.BundleId)
                                {
                                    if (userAmount == Convert.ToDecimal(obj.Amount))
                                    {
                                        result = true;
                                        break;
                                        //return result;
                                    }
                                }
                            }
                        }
                    }
                    else if (channel.ToLower() == "surfline")
                    {
                        dynamic data = JsonConvert.DeserializeObject<Dictionary<string, object>>(xmlStr);
                        List<IspBundlesResponse> objList = new List<IspBundlesResponse>();
                        var accounttype = "";
                        foreach (var item in data)
                        {
                            var key = item.Key;
                            var value = item.Value;
                            if (key == "AccountType")
                            {
                                accounttype = value;
                            }
                        }
                        foreach (var item in data)
                        {
                            var key = item.Key;
                            var value = item.Value;

                            if (key == "AccountType")
                            {
                                accounttype = value;
                            }
                            if (value != null)
                            {
                                if (value != "" && key.Contains("Bundle"))
                                {
                                    var findData = value.Split('|');
                                    IspBundlesResponse obj = new IspBundlesResponse();
                                    obj.Amount = findData[1];
                                    obj.BundleId = findData[3];
                                    obj.Description = "Validity: " + findData[2];
                                    obj.Name = findData[0];
                                    obj.DisplayContent = obj.Amount + "," + obj.Name;
                                    obj.AccountType = accounttype;
                                    if (planId == obj.BundleId)
                                    {
                                        if (userAmount == Convert.ToDecimal(obj.Amount))
                                        {
                                            result = true;
                                            break;
                                            //return result;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (channel.ToLower() == "mtnfibre")
                    {
                        var data = JsonConvert.DeserializeObject<dynamic>(xmlStr);
                        var finalData = JsonConvert.DeserializeObject<MtnBundleResponse>(data);
                        var objList = new List<IspBundlesResponce>();
                        var accounttype = "";

                        //foreach (var d in finalData.bundles)
                        //{
                        foreach (var item in finalData.bundles)
                        {
                            var obj = new IspBundlesResponce();
                            // obj.network_id = item.network_id;
                            obj.BundleId = item.product_id;
                            obj.plan_name = item.name;
                            obj.Amount = item.amount;
                            obj.validity = item.validity;
                            obj.volume = item.product_id;
                            obj.category = item.product_id;
                            obj.DisplayContent = item.amount + " GHS ," + item.product_id + " ," + item.name;
                            objList.Add(obj);
                            if (planId == obj.BundleId)
                            {
                                if (userAmount == Convert.ToDecimal(obj.Amount))
                                {
                                    result = true;
                                    //return result;
                                }
                            }
                        }
                        // }
                        //  Response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, objList);
                    }
                    else
                    {
                        var data = JsonConvert.DeserializeObject<dynamic>(xmlStr);
                        var finalData = JsonConvert.DeserializeObject<BundleResponseForNew>(data);
                        var objList = new List<IspBundlesResponse>();
                        var accounttype = "";
                        foreach (var item in finalData.bundles)
                        {
                            var obj = new IspBundlesResponse();
                            obj.BundleId = item.plan_id;
                            obj.plan_name = item.plan_name;
                            obj.Amount = item.price;
                            obj.validity = item.validity;
                            obj.volume = item.volume;
                            obj.category = item.category;
                            obj.DisplayContent = item.price + " GHS ," + item.volume + " ," + item.category;

                            if (planId == obj.BundleId)
                            {
                                if (userAmount == Convert.ToDecimal(obj.Amount))
                                {
                                    result = true;
                                }
                            }
                        }
                    }
                }
                else
                {

                }
                return result;
            }
            catch (Exception ex)
            {
                return result;
            }

        }

        public static WebUser GetWebCurrentUser()
        {
            WebUser serializeModel = new WebUser();
            try
            {
                HttpCookie authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                if (authCookie != null && !string.IsNullOrEmpty(authCookie.Value))
                {
                    FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);
                    serializeModel = JsonConvert.DeserializeObject<WebUser>(ticket.UserData);
                }
            }
            catch
            {

            }
            return serializeModel;
        }

        public static string GetHeadersToken(HttpRequestMessage request = null)
        {
            string token = "";

            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                token = request.Headers.Where(x => x.Key.ToLower() == ("token")).FirstOrDefault().Value.FirstOrDefault();
                return token;
            }
            else if (HttpContext.Current != null)
            {
                return token;
            }
            else
            {
                return null;
            }
        }

        public bool IsBundles(string planId, decimal userAmount, string channel, string account)
        {
            //Response<List<IspBundlesResponce>> Response = new Response<List<IspBundlesResponce>>();
            bool result = false;
            string url = "";
            if (channel.ToLower() == "busy")
            {
                url = "http://52.40.89.233/aggregator/api/bundles?datanumber=" + account + "&channel=" + channel;
            }
            else if (channel.ToLower() == "surfline")
            {
                url = "http://52.40.89.233/aggregator/api/surflinebundle?customer=" + "233" + account;
            }
            else if (channel.ToUpper() == "MTNFIBRE")
            {
                url = "http://52.40.89.233/aggregator/api/mtn";
            }
            else
            {
                url = "http://52.40.89.233/aggregator/api/databundles?apikey=57F68FC7-97AB-403B-8BD0-7BF50AC13423" + "&network=" + channel;
            }
            var m_strFilePath = url;//"http://52.40.89.233/aggregator/api/bundles?datanumber=000000&channel=busy";
            string xmlStr;
            try
            {
                using (var wc = new WebClient())
                {
                    xmlStr = wc.DownloadString(m_strFilePath);
                }
                if (!string.IsNullOrEmpty(xmlStr))
                {
                    if (channel.ToLower() == "busy")
                    {
                        var data = JsonConvert.DeserializeObject<List<List<ISPBundleDataObject>>>(xmlStr);
                        List<IspBundlesResponce> objList = new List<IspBundlesResponce>();
                        var accounttype = "";
                        foreach (var list in data)
                        {
                            foreach (var item in list)
                            {
                                IspBundlesResponce obj = new IspBundlesResponce();
                                obj.Amount = item.Amount;
                                obj.BundleId = item.BundleId;
                                obj.Description = item.Description == "null" ? "" : item.Description;
                                obj.Name = item.Name;
                                obj.DisplayContent = item.Amount + " GHS ," + item.Name;
                                obj.AccountType = accounttype;
                                //objList.Add(obj);
                                if (planId == obj.BundleId)
                                {
                                    if (userAmount == Convert.ToDecimal(obj.Amount))
                                    {
                                        result = true;
                                        break;
                                        //return result;
                                    }
                                }
                            }
                        }
                    }
                    else if (channel.ToLower() == "surfline")
                    {
                        dynamic data = JsonConvert.DeserializeObject<Dictionary<string, object>>(xmlStr);
                        List<IspBundlesResponce> objList = new List<IspBundlesResponce>();
                        var accounttype = "";

                        foreach (var item in data)
                        {
                            // var myKey = item.FirstOrDefault(x => x.Value == "one").Key;
                            var key = item.Key;
                            var value = item.Value;

                            //if (key == "AccountType")
                            //{
                            //    accounttype = value;
                            //}
                            if (value != null)
                            {
                                if (value != "" && key.Contains("Bundle"))
                                {
                                    var findData = value.Split('|');
                                    IspBundlesResponce obj = new IspBundlesResponce();
                                    obj.Amount = findData[1];
                                    obj.BundleId = findData[3];
                                    obj.Description = "Validity: " + findData[2];
                                    obj.Name = findData[0];
                                    obj.DisplayContent = obj.Amount + "," + obj.Name;
                                    obj.AccountType = accounttype;
                                    if (planId == obj.BundleId)
                                    {
                                        string userAmt = Convert.ToInt32(userAmount) + "GHC".ToString();
                                        if (userAmt == obj.Amount)
                                        {
                                            result = true;
                                            break;
                                            //return result;
                                        }
                                    }
                                    //objList.Add(obj);
                                    //Response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, objList);
                                }
                            }
                        }

                        //Response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, objList);
                    }
                    else if (channel.ToLower() == "mtnfibre")
                    {
                        var data = JsonConvert.DeserializeObject<dynamic>(xmlStr);
                        var finalData = JsonConvert.DeserializeObject<MtnBundleResponse>(data);
                        var objList = new List<IspBundlesResponce>();
                        var accounttype = "";

                        //foreach (var d in finalData.bundles)
                        //{
                        foreach (var item in finalData.bundles)
                        {
                            var obj = new IspBundlesResponce();
                            // obj.network_id = item.network_id;
                            obj.BundleId = item.product_id;
                            obj.plan_name = item.name;
                            obj.Amount = item.amount;
                            obj.validity = item.validity;
                            obj.volume = item.product_id;
                            obj.category = item.product_id;
                            obj.DisplayContent = item.amount + " GHS ," + item.product_id + " ," + item.name;
                            objList.Add(obj);
                            if (planId == obj.BundleId)
                            {
                                if (userAmount == Convert.ToDecimal(obj.Amount))
                                {
                                    result = true;
                                    //return result;
                                }
                            }
                        }
                        // }
                        //  Response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, objList);
                    }
                    else
                    {
                        var data = JsonConvert.DeserializeObject<dynamic>(xmlStr);
                        var finalData = JsonConvert.DeserializeObject<BundleResponseForNew>(data);
                        var objList = new List<IspBundlesResponce>();
                        var accounttype = "";

                        //foreach (var d in finalData.bundles)
                        //{
                        foreach (var item in finalData.bundles)
                        {
                            var obj = new IspBundlesResponce();
                            // obj.network_id = item.network_id;
                            obj.BundleId = item.plan_id;
                            obj.plan_name = item.plan_name;
                            obj.Amount = item.price;
                            obj.validity = item.validity;
                            obj.volume = item.volume;
                            obj.category = item.category;
                            obj.DisplayContent = item.price + " GHS ," + item.volume + " ," + item.category;

                            if (planId == obj.BundleId)
                            {
                                if (userAmount == Convert.ToDecimal(obj.Amount))
                                {
                                    result = true;
                                    //return result;
                                }
                            }
                            //objList.Add(obj);
                        }
                        // }
                        //Response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, objList);
                    }
                }
                else
                {
                    //List<IspBundlesResponce> obj = new List<IspBundlesResponce>();
                    //Response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NoContent, obj);
                    //return result;
                }
                return result;
            }
            catch (Exception ex)
            {
                //List<IspBundlesResponce> obj = new List<IspBundlesResponce>();
                //Response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NoContent, obj);
                return result;
            }

        }

        public string HttpGetUrlEncodedServiceForSurfline(string Log, string Url, object parameters, object Request, string CategoryName, string bundleId, string customerMobile)
        {
            string detail = string.Empty;
            string responseString = string.Empty;

            string requestQueryString = string.Empty;
            try
            {
                using (WebClient wc = new WebClient())
                {
                    try
                    {
                        string serviceType = ((PayMoneyAggregatoryRequest)Request).serviceCategory;
                        if (serviceType.ToUpper() == "ISP")
                        {
                            string accountNo = ((MobileMoneyAggregatoryRequest)parameters).customer;
                            string userAmount = ((MobileMoneyAggregatoryRequest)parameters).amount;
                            string channelId = ((MobileMoneyAggregatoryRequest)parameters).channel;
                            //string[] result = accountNo.Split(',');
                            string customer = customerMobile;
                            string planId = bundleId;

                            bool checkResult = IsBundles(planId, Convert.ToDecimal(userAmount), channelId, customer);
                            //if (channelId.ToUpper() == "SURFLINE")
                            //{

                            //}                                                     
                            if (!checkResult)
                            {
                                responseString = "{\"StatusCode\":\"5006\",\"Message\":\"Invalid bundle amount\",\"TransactionId\":\"\",\"InvoiceNo\":\"\"}";
                                return responseString;
                            }
                        }
                    }
                    catch
                    {

                    }

                    requestQueryString = GetQueryString(parameters);
                    detail = "Aggregator Url : " + (Url + "?" + requestQueryString);
                    LogTransactionTypes.Request.SaveTransactionLog(Log + CategoryName, Request, detail);
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    responseString = wc.UploadString(Url, requestQueryString);
                }
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog(Log + CategoryName, Log + CategoryName, Log + CategoryName);
                LogTransactionTypes.Response.SaveTransactionLog(Log + CategoryName, Request, detail + ", Exception Occured : " + ex.Message);
                responseString = "{\"StatusCode\":\"506\",\"Message\":\"FAILED\",\"TransactionId\":\"\",\"InvoiceNo\":\"\"}";
            }
            return responseString;
        }

        public String Sha512(FlightAndAfroRequest request)
        {
            string data = "";
            // StringBuilder sb = new StringBuilder();
            //Url for Payment           
            string hashedData = request.agentcode + "|" + request.tokenID + "|" + request.tgt + "|" + request.saltkey;
            //sb.Append(hashedData);
            //sb.Append(request.saltkey);           
            //SHA512 sha512 = SHA512Managed.Create();
            //byte[] bytes = Encoding.UTF8.GetBytes(hashedData);
            //byte[] hash = sha512.ComputeHash(bytes);
            using (SHA512 sha512Hash = SHA512.Create())
            {
                //From String to byte array
                byte[] sourceBytes = Encoding.UTF8.GetBytes(hashedData);
                byte[] hashBytes = sha512Hash.ComputeHash(sourceBytes);

                return GetStringFromHash(hashBytes);
            }
        }

        private static string GetStringFromHash(byte[] hash)
        {
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
            {
                result.Append(hash[i].ToString("X2"));
            }
            return result.ToString();
        }

        public String Sha512Ha(GetFlightBookingRequest request)
        {
            string data = "";
            // StringBuilder sb = new StringBuilder();
            //Url for Payment           
            string hashedData = request.merchantcode + "|" + request.agentcode + "|" + request.tokenID + "|" + request.saltkey;
            //sb.Append(hashedData);
            //sb.Append(request.saltkey);           
            //SHA512 sha512 = SHA512Managed.Create();
            //byte[] bytes = Encoding.UTF8.GetBytes(hashedData);
            //byte[] hash = sha512.ComputeHash(bytes);
            using (SHA512 sha512Hash = SHA512.Create())
            {
                //From String to byte array
                byte[] sourceBytes = Encoding.UTF8.GetBytes(hashedData);
                byte[] hashBytes = sha512Hash.ComputeHash(sourceBytes);

                return GetStringFromHash(hashBytes);
            }

        }

        public String Sha512Final(FinalCheckSum request)
        {
            string data = "";
            // StringBuilder sb = new StringBuilder();
            //Url for Payment           
            string hashedData = request.statusCode + "|" + request.statusMessage + "|" + request.merchantCode + "|" + request.agentCode + "|" + request.tokenId + "|" + request.saltKey;
            using (SHA512 sha512Hash = SHA512.Create())
            {
                //From String to byte array
                byte[] sourceBytes = Encoding.UTF8.GetBytes(hashedData);
                byte[] hashBytes = sha512Hash.ComputeHash(sourceBytes);

                return GetStringFromHash(hashBytes);
            }

        }


        public String Sha256Hash(PayServicesMoneyAggregatoryRequest request)
        {
            StringBuilder sb = new StringBuilder();
            //Url for Payment          
            sb.Append(request.ApiKey);
            sb.Append(request.Amount);
            sb.Append(request.Customer);
            sb.Append(request.TransactionId);
            sb.Append(ThirdPartyAggragatorSettings.secretKey);
            StringBuilder hash = new StringBuilder();

            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
                for (int i = 0; i < bytes.Length; i++)
                {
                    hash.Append(bytes[i].ToString("x2"));
                }
            }
            //byte[] bytes = sha256.ComputeHash(new UTF8Encoding().GetBytes(sb.ToString()));

            return hash.ToString();
        }



        public String Sha256HashCamroon(PayServicesMoneyAggregatoryRequestCamroon request)
        {
            StringBuilder sb = new StringBuilder();
            //Url for Payment          
            sb.Append(request.ApiKey);
            sb.Append(request.Customer);
            sb.Append(request.Amount);
            sb.Append(request.InvoiceNo);
            // sb.Append(ThirdPartyAggragatorSettings.secretKeyCamroon);
            sb.Append(ThirdPartyAggragatorSettings.secretKey);
            StringBuilder hash = new StringBuilder();

            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
                for (int i = 0; i < bytes.Length; i++)
                {
                    hash.Append(bytes[i].ToString("x2"));
                }
            }
            //byte[] bytes = sha256.ComputeHash(new UTF8Encoding().GetBytes(sb.ToString()));

            return hash.ToString();
        }
        public String SHA1Hash(string request)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder hash = new StringBuilder();
            //Url for Payment
            hash.Append(request);
            SHA1 sHA1 = SHA1.Create();
            byte[] vs = sHA1.ComputeHash(new UTF8Encoding().GetBytes(hash.ToString()));

            for (int i = 0; i < vs.Length; i++)
            {
                sb.Append(vs[i].ToString("x2"));
            }
            return sb.ToString();
        }


        public async Task<string> HttpGetUrlEncodedServiceForMobileMoney(string Log, string Url, object parameters, object Request, string CategoryName)
        {
            string detail = string.Empty;
            string responseString = string.Empty;

            string requestQueryString = string.Empty;
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    requestQueryString = GetQueryString(parameters);
                    requestQueryString = requestQueryString.Replace("%2c", ",");
                    detail = Url + "?" + requestQueryString;
                    LogTransactionTypes.Request.SaveTransactionLog(Log + CategoryName, Request, detail);
                    HttpResponseMessage response = await httpClient.GetAsync(detail);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        responseString = result.ToString();
                    }
                    // responseString = "{\"StatusCode\":\"300\",\"Message\":\"PENDING\",\"TransactionId\":\"285\",\"InvoiceNo\":null}";
                }
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog(Log + CategoryName, Log + CategoryName, Log + CategoryName);
                LogTransactionTypes.Response.SaveTransactionLog(Log + CategoryName, Request, detail + ", Exception Occured : " + ex.Message);
                responseString = "{\"StatusCode\":\"506\",\"Message\":\"FAILED\",\"TransactionId\":\"\",\"InvoiceNo\":\"\"}";
            }
            return responseString;
        }
        
    }
}
