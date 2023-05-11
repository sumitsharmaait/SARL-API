using ezeePay.Utility.CommonClass;
using ezeePay.Utility.Enums;
using Ezipay.Service.ApiHelpPage;
using Ezipay.Service.TokenService;
using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.ViewModel.common;
using Ezipay.ViewModel.TokenViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Ezipay.Api.Filters
{
    public class TempSessionAuthorization : ActionFilterAttribute
    {
        private ITokenService _tokenService;
        private IApiHelpPageService _apiHelpPageService;
        public TempSessionAuthorization()
        {
            _tokenService = new TokenService();
            _apiHelpPageService = new ApiHelpPageService();
        }
        public override void OnActionExecuting(HttpActionContext context)
        {
            bool isAuthorized = false;



            try
            {
                if (context.Request.Headers.Any(x => x.Key.ToLower() == ("token")))
                {

                    var tokenHeader = context.Request.Headers.Where(x => x.Key.ToLower() == "token").FirstOrDefault().Value.FirstOrDefault();
                    GlobalData.RoleId = GetRoleId(context.Request);
                    GlobalData.AppVersion = GetAppVersion(context.Request);

                    if (!string.IsNullOrEmpty(tokenHeader))
                    {
                        string token = tokenHeader.ToString();
                        if (!string.IsNullOrEmpty(token))
                        {

                            string actionName = context.ActionDescriptor.ActionName;

                            int tokenStatus = _tokenService.ValidateAuthenticaion(new ServiceAuthenticationRequest { Token = token, Type = (int)TokenType.TempToken });
                            if (tokenStatus == (int)TokenStatusCode.Success)
                            {
                                isAuthorized = true;
                            }
                            if (actionName == "IsdCodes")
                            {
                                tokenStatus = _tokenService.ValidateAuthenticaion(new ServiceAuthenticationRequest { Token = token, Type = (int)TokenType.Session });
                                if (tokenStatus == (int)TokenStatusCode.Success)
                                {
                                    isAuthorized = true;
                                }
                            }
                        }

                    }
                }



            }
            catch (Exception ex)
            {


            }



            if (!isAuthorized)
            {

                Response<string> response = new Response<string>();
                string result = _apiHelpPageService.ApiList().Where(x => x.ApiName == context.ActionDescriptor.ActionName).Select(x => x.Response).FirstOrDefault();
                response.Create(false, ResponseMessages.UNATHORIZED_REQUEST, HttpStatusCode.Unauthorized, result);
                string responseString = JsonConvert.SerializeObject(response);
                var tokenPair = _tokenService.KeysByTempToken();
                responseString = AES256.Encrypt(tokenPair.PublicKey, responseString);
                context.Response = context.Request.CreateResponse(HttpStatusCode.Unauthorized, responseString);

                base.OnActionExecuting(context);
            }

        }
        public int GetRoleId(HttpRequestMessage request)
        {
            int roleId = 0;
            try
            {
                var data = request.Headers.GetValues("roleId").FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(data))
                {
                    roleId = Convert.ToInt16(data);
                }
            }
            catch (Exception ex)
            {
                roleId = 0;
            }
            return roleId;

        }
        public int GetAppVersion(HttpRequestMessage request)
        {
            int roleId = 0;
            try
            {
                var data = request.Headers.GetValues("appVersion").FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(data))
                {
                    roleId = Convert.ToInt16(data);
                }
            }
            catch (Exception ex)
            {
                roleId = 0;
            }
            return roleId;

        }
    }
    public class SessionAuthorization : ActionFilterAttribute
    {
        private ITokenService _tokenService;
        private IApiHelpPageService _apiHelpPageService;
        public SessionAuthorization()
        {
            _tokenService = new TokenService();
            _apiHelpPageService = new ApiHelpPageService();
        }
        public override void OnActionExecuting(HttpActionContext context)
        {
            bool isAuthorized = false;
            try
            {
                if (context.Request.Headers.Any(x => x.Key.ToLower() == ("token")))
                {

                    var tokenHeader = context.Request.Headers.Where(x => x.Key.ToLower() == "token").FirstOrDefault().Value.FirstOrDefault();
                    //Set token value for getting user profile
                    //GlobalData.Key = tokenHeader;
                    GlobalData.RoleId = GetRoleId(context.Request);
                    GlobalData.AppVersion = GetAppVersion(context.Request);
                    GlobalData.AppId = GetAppId(context.Request);
                    if (!string.IsNullOrEmpty(tokenHeader))
                    {
                        string token = tokenHeader.ToString();
                        if (!string.IsNullOrEmpty(token))
                        {

                            int tokenStatus = _tokenService.ValidateAuthenticaion(new ServiceAuthenticationRequest { Token = token, Type = (int)TokenType.Session });
                            if (tokenStatus == (int)TokenStatusCode.Success)
                            {

                                isAuthorized = true;
                            }
                        }
                    }


                }
            }
            catch (Exception ex)
            {


            }
            if (!isAuthorized)
            {

                if (GlobalData.AppVersion == 2)
                {
                    Response<string> response = new Response<string>();
                    string result = _apiHelpPageService.ApiList().Where(x => x.ApiName == context.ActionDescriptor.ActionName).Select(x => x.Response).FirstOrDefault();
                    response.Create(false, ResponseMessages.UNATHORIZED_REQUEST, HttpStatusCode.Unauthorized, result);
                    string responseString = JsonConvert.SerializeObject(response);
                    var tokenPair = _tokenService.KeysBySessionToken();
                    try
                    {
                        responseString = AES256.Encrypt(tokenPair.PublicKey, responseString);
                        context.Response = context.Request.CreateResponse(HttpStatusCode.Unauthorized, responseString);

                    }
                    catch (Exception ex)
                    {
                        ex.Message.ErrorLog("SessionAuthorization.cs", "Filter Exception Token Value", tokenPair.Token);
                        context.Response = context.Request.CreateResponse(HttpStatusCode.InternalServerError, response);


                    }
                    base.OnActionExecuting(context);
                }
                else
                {
                    Response<string> response = new Response<string>();
                    string result = _apiHelpPageService.ApiList().Where(x => x.ApiName == context.ActionDescriptor.ActionName).Select(x => x.Response).FirstOrDefault();
                    response = response.Create(false, ResponseMessages.UNATHORIZED_REQUEST2, HttpStatusCode.InternalServerError, result);
                    string responseString = JsonConvert.SerializeObject(response);
                    var tokenPair = _tokenService.KeysBySessionToken();
                    try
                    {
                        responseString = AES256.Encrypt(tokenPair.PublicKey, responseString);
                        context.Response = context.Request.CreateResponse(HttpStatusCode.InternalServerError, responseString);

                    }
                    catch (Exception ex)
                    {
                        ex.Message.ErrorLog("SessionAuthorization.cs", "Filter Exception Token Value", tokenPair.Token);
                        context.Response = context.Request.CreateResponse(HttpStatusCode.InternalServerError, response);


                    }
                    base.OnActionExecuting(context);
                }
            }

        }

        public int GetRoleId(HttpRequestMessage request)
        {
            int roleId = 0;
            try
            {
                var data = request.Headers.GetValues("roleId").FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(data))
                {
                    roleId = Convert.ToInt16(data);
                }
            }
            catch (Exception ex)
            {
                roleId = 0;
            }
            return roleId;

        }

        public int GetAppVersion(HttpRequestMessage request)
        {
            int roleId = 0;
            try
            {
                var data = request.Headers.GetValues("appVersion").FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(data))
                {
                    roleId = Convert.ToInt16(data);
                }
            }
            catch (Exception ex)
            {
                roleId = 0;
            }
            return roleId;

        }

        public int GetAppId(HttpRequestMessage request)
        {
            int appId = 0;//(int)DeviceTypes.Admin;
            try
            {
                var data = request.Headers.GetValues("appId").FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(data))
                {
                    appId = Convert.ToInt16(data);
                }
            }
            catch (Exception ex)
            {
            }
            return appId;

        }
    }
}