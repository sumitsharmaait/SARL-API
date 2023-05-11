using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.ViewModel.SendOtpViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Ezipay.Utility.SendOtp
{
    public class SendMessage : ISendMessage
    {
       
        public bool SendMessgeWithISDCode(SendMessageRequest requestModel)
        {
            // HubtelMessageRequest PostData = new HubtelMessageRequest();
            RouteMobileMessageRequest PostData = new RouteMobileMessageRequest();
            PostData.message = requestModel.Message;
            PostData.destination = requestModel.ISD + requestModel.MobileNo.IgnoreZero();

            // PostData.RegisteredDelivery = true;


            bool IsSuccess = false;
            HubtelMessageResponse res = new HubtelMessageResponse();
            string responseString = string.Empty;
            try
            {
                // string Url = ConfigurationManager.AppSettings["HubtelEndPoint"];
                string Url = ConfigurationManager.AppSettings["RouteMobileEndPoint"];
                String QueryString = new CommonMethods().GetQueryString(PostData);

                QueryString = QueryString.Replace("%2b", "");

                ASCIIEncoding ascii = new ASCIIEncoding();
                byte[] postBytes = ascii.GetBytes(QueryString.ToString());
                LogTransactionTypes.Request.SaveTransactionLog("HubSendMessage", PostData, "Request for Route Mobile");
                // set up request object
                HttpWebRequest request;
                string encoded = HttpUtility.UrlEncode(Url + "?" + QueryString, Encoding.UTF8);
                try
                {
                    request = (HttpWebRequest)HttpWebRequest.Create(Url + "?" + QueryString);
                }
                catch (UriFormatException)
                {
                    request = null;
                }

                request.KeepAlive = false;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
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
                            var OtpResult = responseString.Split('|');
                            string MessageId = OtpResult[1];
                            // res = JsonConvert.DeserializeObject<HubtelMessageResponse>(responseString);
                            if (OtpResult != null && !string.IsNullOrEmpty(MessageId))
                            {
                                IsSuccess = true;
                            }
                            LogTransactionTypes.Response.SaveTransactionLog("RouteMobileMessage", OtpResult, "Response from RouteMobile (In case MessageId is not empty we consider the message has been broadcasted.)");
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("HubSendMessage", "HubSendMessage", requestModel);
                LogTransactionTypes.Response.SaveTransactionLog("HubSendMessage", PostData, "Exception :" + ex.Message);

            }

            return IsSuccess;

        }
    
        public async Task<bool> SendOtpTeleSign(SendOtpTeleSignRequest signRequest)
        {
            bool IsSuccess = false;

            var myObjectToBeCreated = new Dictionary<string, string>();
            myObjectToBeCreated.Add("phone_number", signRequest.phone_number);
            myObjectToBeCreated.Add("language", "en-US");
            myObjectToBeCreated.Add("verify_code", signRequest.verify_code);
            myObjectToBeCreated.Add("template",signRequest.template);

            string username = ConfigurationManager.AppSettings["TeleSignUsername"];
            string password = ConfigurationManager.AppSettings["TeleSignPassword"]; 
            //  PayServicesResponseForServices pay = new PayServicesResponseForServices();
            string resourceUri = ConfigurationManager.AppSettings["TeleSignSendOTPEndPoint"];
            string responseString = "";

            FormUrlEncodedContent formBody = new FormUrlEncodedContent(myObjectToBeCreated);
            var httpClient = new HttpClient();         
            try
            {
               
                string urlEncodedFields = await formBody.ReadAsStringAsync().ConfigureAwait(false);

                HttpRequestMessage request;

                request = new HttpRequestMessage(HttpMethod.Post, resourceUri);
                request.Content = formBody;


                Dictionary<string, string> headers = myObjectToBeCreated;// GenerateTelesignHeaders(username, password, "POST", freq, urlEncodedFields, null, null, RestClient.UserAgent);
                var base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64String);
                foreach (KeyValuePair<string, string> header in headers)
                {
                    if (header.Key == "Content-Type")
                        // skip Content-Type, otherwise HttpClient will complain
                        continue;
                    request.Headers.Add(header.Key, header.Value);
                }

                HttpResponseMessage response = await httpClient.SendAsync(request).ConfigureAwait(false);
                responseString = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<TeleSignResponse>(responseString);
                if (result.status.code == 290)
                {
                    IsSuccess = true;
                }              
            }
            catch (Exception ex)
            {
                responseString = "";
            }
            return IsSuccess;
        }


        public async Task<bool> CallBackTeleSign(SendOtpTeleSignRequest signRequest)
        {
            bool IsSuccess = false;
            var myObjectToBeCreated = new Dictionary<string, string>();
            myObjectToBeCreated.Add("phone_number", signRequest.phone_number);
            myObjectToBeCreated.Add("language", "fr-FR");
            myObjectToBeCreated.Add("verify_code", signRequest.verify_code);
            myObjectToBeCreated.Add("tts_message", signRequest.template);

            string username = ConfigurationManager.AppSettings["TeleSignUsername"];
            string password = ConfigurationManager.AppSettings["TeleSignPassword"];
            //  PayServicesResponseForServices pay = new PayServicesResponseForServices();
            string resourceUri = ConfigurationManager.AppSettings["TeleSignCallBackEndPoint"];
            string responseString = "";

            FormUrlEncodedContent formBody = new FormUrlEncodedContent(myObjectToBeCreated);
            var httpClient = new HttpClient();
            try
            {

                string urlEncodedFields = await formBody.ReadAsStringAsync().ConfigureAwait(false);

                HttpRequestMessage request;

                request = new HttpRequestMessage(HttpMethod.Post, resourceUri);
                request.Content = formBody;


                Dictionary<string, string> headers = myObjectToBeCreated;// GenerateTelesignHeaders(username, password, "POST", freq, urlEncodedFields, null, null, RestClient.UserAgent);
                var base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64String);
                foreach (KeyValuePair<string, string> header in headers)
                {
                    if (header.Key == "Content-Type")
                        // skip Content-Type, otherwise HttpClient will complain
                        continue;
                    request.Headers.Add(header.Key, header.Value);
                }

                HttpResponseMessage response = await httpClient.SendAsync(request).ConfigureAwait(false);
                responseString = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<TeleSignCallBackResponse>(responseString);
                if (result.status.code == 103)
                {
                    IsSuccess = true;
                }
            }
            catch (Exception ex)
            {
                IsSuccess =false;
            }
            return IsSuccess;
        }

        //public async Task<bool> SendOtpdimoco(SendOtpTeleSignRequest signRequest)
        //{
        //    string url = "https://api.messaging.dimoco.eu:10081/api?app-id=MobAgGRDIR&password=Nxb39ybt&message=" + signRequest.template + "&to=" + signRequest.phone_number + "&from=EZIPAY&type=0&dlr-mask=24";

        //     try
        //    {

        //        var request = WebRequest.Create(url) as HttpWebRequest;
        //        var response = request.GetResponse();

        //        Stream receiveStream = response.GetResponseStream();
        //        StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);

        //        var result = readStream.ReadToEnd();

        //        var xml = System.Xml.Linq.XElement.Parse(result);

        //        if (xml.Elements("status").FirstOrDefault().Value == "0") //message accepted
        //        {

        //            return true;
        //        }
        //        else
        //        {

        //            return false;
        //        }


        //        //< status > 0 </ status >

        //        //< description > message accepted </ description >

        //        //< msg - id > dim123 - dea440ac - 16d037cc - 72e30d79 </ msg - id >

        //    }

        //    catch (Exception ex)
        //    {
        //        return false;
        //    }


        //}
    }
}
