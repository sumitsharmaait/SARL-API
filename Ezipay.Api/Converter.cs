using ezeePay.Utility.Enums;
using Ezipay.Service.TokenService;
using Ezipay.Utility.common;
using Ezipay.Utility.LogHandler;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.common;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;

namespace Ezipay.Api
{
    public class Converter : ApiController
    {
        private readonly ILogUtils _logUtils;
        public Converter()
        {
            _logUtils = new LogUtils();
        }
        //plin data

        public IHttpActionResult LogApiResponseMessage<T>(Response<T> dataObject, HttpStatusCode statusCode, bool isencrypt = true, bool isTempToken = false, HttpRequestMessage httpRequestMessage = null) where T : class
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
            IHttpActionResult responseIHttpActionResult;

            //log method for admin panel
            if (GlobalData.AppId == (int)DeviceTypes.Admin)
            {
                if (httpRequestMessage != null)
                {
                    try
                    {
                        var data = JsonConvert.SerializeObject(dataObject);
                        LogMetadata log = new LogMetadata
                        {
                            Type = "Response",
                            RequestMethod = httpRequestMessage.Method.Method,
                            RequestTimestamp = DateTime.Now,
                            RequestUri = httpRequestMessage.RequestUri.ToString(),
                            RequestBody = data
                        };
                        SendToLog(log);
                    }
                    catch { }
                }
            }
            else if (GlobalData.AppId == 2 || GlobalData.AppVersion == 2)
            {
                if (httpRequestMessage != null)
                {
                    try
                    {
                        var data = JsonConvert.SerializeObject(dataObject);
                        LogMetadata log = new LogMetadata
                        {
                            Type = "Response",
                            RequestMethod = httpRequestMessage.Method.Method,
                            RequestTimestamp = DateTime.Now,
                            RequestUri = httpRequestMessage.RequestUri.ToString(),
                            RequestBody = data
                        };
                        SendToLogApps(log);
                    }
                    catch { }
                }
            }
            httpResponseMessage.Content = new ObjectContent<Response<T>>(dataObject, new JsonMediaTypeFormatter());


            httpResponseMessage.StatusCode = statusCode;
            responseIHttpActionResult = ResponseMessage(httpResponseMessage);
            return responseIHttpActionResult;
        }

        //




        public IHttpActionResult ApiResponseMessage<T>(Response<T> dataObject, HttpStatusCode statusCode, bool isencrypt = true, bool isTempToken = false, HttpRequestMessage httpRequestMessage = null) where T : class
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
            IHttpActionResult responseIHttpActionResult;
            if (isencrypt)
            { //log method for admin panel
                if (GlobalData.AppId == (int)DeviceTypes.Admin)
                {
                    if (httpRequestMessage != null)
                    {
                        try
                        {
                            var data = JsonConvert.SerializeObject(dataObject);
                            LogMetadata log = new LogMetadata
                            {
                                Type = "Response",
                                RequestMethod = httpRequestMessage.Method.Method,
                                RequestTimestamp = DateTime.Now,
                                RequestUri = httpRequestMessage.RequestUri.ToString(),
                                RequestBody = data
                            };
                            SendToLog(log);
                        }
                        catch { }
                    }
                }
                else if (GlobalData.AppId == 2 || GlobalData.AppVersion == 2)
                {
                    if (httpRequestMessage != null)
                    {
                        try
                        {
                            var data = JsonConvert.SerializeObject(dataObject);
                            LogMetadata log = new LogMetadata
                            {
                                Type = "Response",
                                RequestMethod = httpRequestMessage.Method.Method,
                                RequestTimestamp = DateTime.Now,
                                RequestUri = httpRequestMessage.RequestUri.ToString(),
                                RequestBody = data
                            };
                            SendToLogApps(log);
                        }
                        catch { }
                    }
                }
                string resultString = new EncrDecr<Response<T>>().Encrypt(dataObject, isTempToken, httpRequestMessage);
                httpResponseMessage.Content = new ObjectContent<string>(resultString, new JsonMediaTypeFormatter());
            }
            else
            { //log method for admin panel
                if (GlobalData.AppId == (int)DeviceTypes.Admin)
                {
                    if (httpRequestMessage != null)
                    {
                        try
                        {
                            var data = JsonConvert.SerializeObject(dataObject);
                            LogMetadata log = new LogMetadata
                            {
                                Type = "Response",
                                RequestMethod = httpRequestMessage.Method.Method,
                                RequestTimestamp = DateTime.Now,
                                RequestUri = httpRequestMessage.RequestUri.ToString(),
                                RequestBody = data
                            };
                            SendToLog(log);
                        }
                        catch { }
                    }
                }
                else if (GlobalData.AppId == 2 || GlobalData.AppVersion == 2)
                {
                    if (httpRequestMessage != null)
                    {
                        try
                        {
                            var data = JsonConvert.SerializeObject(dataObject);
                            LogMetadata log = new LogMetadata
                            {
                                Type = "Response",
                                RequestMethod = httpRequestMessage.Method.Method,
                                RequestTimestamp = DateTime.Now,
                                RequestUri = httpRequestMessage.RequestUri.ToString(),
                                RequestBody = data
                            };
                            SendToLogApps(log);
                        }
                        catch { }
                    }
                }
                httpResponseMessage.Content = new ObjectContent<Response<T>>(dataObject, new JsonMediaTypeFormatter());
            }
            //if (isencrypt)
            //{
            //    string resultString = new EncrDecr<Response<T>>().Encrypt(dataObject, isTempToken);
            //    httpResponseMessage.Content = new ObjectContent<string>(resultString, new JsonMediaTypeFormatter());
            //}
            //else
            //{
            //    httpResponseMessage.Content = new ObjectContent<Response<T>>(dataObject, new JsonMediaTypeFormatter());
            //}
            httpResponseMessage.StatusCode = statusCode;
            responseIHttpActionResult = ResponseMessage(httpResponseMessage);
            return responseIHttpActionResult;
        }

        #region UploadFile

        private HttpContextWrapper GetHttpContext(HttpRequestMessage request = null)
        {
            request = request ?? Request;

            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                return ((HttpContextWrapper)request.Properties["MS_HttpContext"]);
            }
            else if (HttpContext.Current != null)
            {
                return new HttpContextWrapper(HttpContext.Current);
            }
            else
            {
                return null;
            }
        }
        #endregion

        private void SendToLog(LogMetadata logMetadata)
        {
            if (!logMetadata.RequestUri.Contains("swagger"))
            {
                // TODO: Write code here to store the logMetadata instance to a pre-configured log store...
                _logUtils.WriteTextToFile(JsonConvert.SerializeObject(logMetadata));
            }
        }
        private void SendToLogApps(LogMetadata logMetadata)
        {
            if (!logMetadata.RequestUri.Contains("swagger"))
            {
                // TODO: Write code here to store the logMetadata instance to a pre-configured log store...
                _logUtils.WriteTextToFileApps(JsonConvert.SerializeObject(logMetadata));
            }
        }
    }

    public class EncrDecr<T>
    {
        private readonly ILogUtils _logUtils;
        public EncrDecr()
        {
            _logUtils = new LogUtils();
        }
        //public T Decrypt(string value, bool isTempToken = false)
        //{
        //    if (GlobalData.RoleId == 1)
        //    {
        //        if (isTempToken)
        //        {
        //            var keys = new TokenService().KeysByTempToken();
        //            value = new Cryptography2(keys.PrivateKey).Decrypt(value);
        //            return JsonConvert.DeserializeObject<T>(value);
        //        }
        //        else
        //        {
        //            var keys = new TokenService().KeysBySessionToken();
        //            value = new Cryptography2(keys.PrivateKey).Decrypt(value);
        //            // var d = JsonConvert.DeserializeObject<T>(value);
        //            return JsonConvert.DeserializeObject<T>(value);
        //        }
        //    }
        //    else
        //    {
        //        if (isTempToken)
        //        {
        //            var keys = new TokenService().KeysByTempToken();
        //            value = AES256.Decrypt(keys.PrivateKey, value);
        //            return JsonConvert.DeserializeObject<T>(value);
        //        }
        //        else
        //        {
        //            var keys = new TokenService().KeysBySessionToken();
        //            value = AES256.Decrypt(keys.PrivateKey, value);
        //            return JsonConvert.DeserializeObject<T>(value);
        //        }
        //    }
        //}
        public T Decrypt(string value, bool isTempToken = false, HttpRequestMessage httpRequestMessage = null)
        {
            if (GlobalData.RoleId == 1)
            {
                if (isTempToken)
                {
                    var keys = new TokenService().KeysByTempToken();
                    value = new Cryptography2(keys.PrivateKey).Decrypt(value);
                }
                else
                {
                    var keys = new TokenService().KeysBySessionToken();
                    value = new Cryptography2(keys.PrivateKey).Decrypt(value);
                    // var d = JsonConvert.DeserializeObject<T>(value);

                }
                if (httpRequestMessage != null)
                { //log method for admin panel      
                    CreateLogRequest(httpRequestMessage, value);
                }

                return JsonConvert.DeserializeObject<T>(value);
            }
            else
            {
                if (isTempToken)
                {
                    var keys = new TokenService().KeysByTempToken();
                    value = AES256.Decrypt(keys.PrivateKey, value);
                    return JsonConvert.DeserializeObject<T>(value);
                }
                else
                {
                    var keys = new TokenService().KeysBySessionToken();
                    value = AES256.Decrypt(keys.PrivateKey, value);
                    return JsonConvert.DeserializeObject<T>(value);
                }
            }
        }

        public string Encrypt(T response, bool isTempToken = false, HttpRequestMessage httpRequestMessage = null)
        {
            if (GlobalData.RoleId == 1)
            {
                if (isTempToken)
                {
                    var keys = new TokenService().KeysByTempToken();
                    string responseSrting = JsonConvert.SerializeObject(response);
                    return new Cryptography2(keys.PrivateKey).Encrypt(responseSrting);
                }
                else
                {
                    var keys = new TokenService().KeysBySessionToken();
                    string responseSrting = JsonConvert.SerializeObject(response);
                    return new Cryptography2(keys.PrivateKey).Encrypt(responseSrting);
                }
            }
            else
            {
                if (isTempToken)
                {
                    var keys = new TokenService().KeysByTempToken();
                    string responseSrting = JsonConvert.SerializeObject(response);
                    return AES256.Encrypt(keys.PrivateKey, responseSrting);
                }
                else
                {
                    var keys = new TokenService().KeysBySessionToken();
                    string responseSrting = JsonConvert.SerializeObject(response);
                    return AES256.Encrypt(keys.PrivateKey, responseSrting);
                }
            }
        }
        private void SendToLog1(LogMetadata logMetadata)
        {
            if (!logMetadata.RequestUri.Contains("swagger"))
            {
                // TODO: Write code here to store the logMetadata instance to a pre-configured log store...
                _logUtils.WriteTextToFile1(JsonConvert.SerializeObject(logMetadata));
            }
        }
        private void SendToLog(LogMetadata logMetadata)
        {
            if (!logMetadata.RequestUri.Contains("swagger"))
            {
                // TODO: Write code here to store the logMetadata instance to a pre-configured log store...
                _logUtils.WriteTextToFile(JsonConvert.SerializeObject(logMetadata));
            }
        }
        private void SendToLogApps(LogMetadata logMetadata)
        {
            if (!logMetadata.RequestUri.Contains("swagger"))
            {
                // TODO: Write code here to store the logMetadata instance to a pre-configured log store...
                _logUtils.WriteTextToFileApps(JsonConvert.SerializeObject(logMetadata));
            }
        }

        private void CreateLogRequest(HttpRequestMessage httpRequestMessage, string value)
        {
            if (GlobalData.AppId == (int)DeviceTypes.Admin)
            {
                if (httpRequestMessage != null)
                {
                    try
                    {
                        if (httpRequestMessage.RequestUri.ToString().Contains("Login"))
                        {
                            var d = JsonConvert.DeserializeObject<LoginRequest>(value);
                            d.Password = "**********";
                            var changePass = JsonConvert.SerializeObject(d);
                            LogMetadata log = new LogMetadata
                            {
                                Type = "Request",
                                RequestMethod = httpRequestMessage.Method.Method,
                                RequestTimestamp = DateTime.Now,
                                RequestUri = httpRequestMessage.RequestUri.ToString(),
                                RequestBody = changePass
                            };
                            SendToLog(log); //
                        }
                        else if (httpRequestMessage.RequestUri.ToString().Contains("ChangeUserDocumentStatus"))
                        {
                            var d = JsonConvert.DeserializeObject<dynamic>(value);
                            d.Password = "**********";
                            var changePass = JsonConvert.SerializeObject(d);
                            LogMetadata log = new LogMetadata
                            {
                                Type = "Request",
                                RequestMethod = httpRequestMessage.Method.Method,
                                RequestTimestamp = DateTime.Now,
                                RequestUri = httpRequestMessage.RequestUri.ToString(),
                                RequestBody = changePass
                            };
                            SendToLog1(log); //
                        }
                        else
                        {
                            var d = JsonConvert.DeserializeObject<dynamic>(value);
                            d.Password = "**********";
                            var changePass = JsonConvert.SerializeObject(d);
                            LogMetadata log = new LogMetadata
                            {
                                Type = "Request",
                                RequestMethod = httpRequestMessage.Method.Method,
                                RequestTimestamp = DateTime.Now,
                                RequestUri = httpRequestMessage.RequestUri.ToString(),
                                RequestBody = changePass
                            };
                            SendToLog(log);
                        }

                    }
                    catch { }
                }
            }
            else if (GlobalData.AppId == 2 || GlobalData.AppVersion == 2)
            {
                if (httpRequestMessage != null)
                {
                    try
                    {
                        if (httpRequestMessage.RequestUri.ToString().Contains("Login"))
                        {
                            var d = JsonConvert.DeserializeObject<LoginRequest>(value);
                            d.Password = "**********";
                            var changePass = JsonConvert.SerializeObject(d);
                            LogMetadata log = new LogMetadata
                            {
                                Type = "Request",
                                RequestMethod = httpRequestMessage.Method.Method,
                                RequestTimestamp = DateTime.Now,
                                RequestUri = httpRequestMessage.RequestUri.ToString(),
                                RequestBody = changePass
                            };
                            SendToLogApps(log);
                        }
                        else
                        {
                            var d = JsonConvert.DeserializeObject<dynamic>(value);
                            d.Password = "**********";
                            var changePass = JsonConvert.SerializeObject(d);
                            LogMetadata log = new LogMetadata
                            {
                                Type = "Request",
                                RequestMethod = httpRequestMessage.Method.Method,
                                RequestTimestamp = DateTime.Now,
                                RequestUri = httpRequestMessage.RequestUri.ToString(),
                                RequestBody = changePass
                            };
                            SendToLogApps(log);
                        }

                    }
                    catch { }
                }
            }
        }
    }
    public class LogMetadata
    {
        /// <summary>
        /// RequestContentType
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// RequestUri
        /// </summary>
        public string RequestUri { get; set; }
        /// <summary>
        /// RequestMethod
        /// </summary>
        public string RequestMethod { get; set; }
        /// <summary>
        /// RequestTimestamp
        /// </summary>
        public DateTime? RequestTimestamp { get; set; }
        /// <summary>
        /// RequestBody
        /// </summary>
        public string RequestBody { get; set; }
        /// <summary>
        /// ResponseContentType
        /// </summary>
        public string ResponseContentType { get; set; }
        /// <summary>
        /// ResponseStatusCode
        /// </summary>
        public HttpStatusCode ResponseStatusCode { get; set; }
        /// <summary>
        /// ResponseTimestamp
        /// </summary>
        public DateTime? ResponseTimestamp { get; set; }
        /// <summary>
        /// ResponseBody
        /// </summary>
        public string ResponseBody { get; set; }
    }
}