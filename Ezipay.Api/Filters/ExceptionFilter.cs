using Ezipay.Service.ApiHelpPage;
using Ezipay.Service.TokenService;
using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.ViewModel.common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;

namespace Ezipay.Api.Filters
{
    /// <summary>
    /// SessionTokenExceptionFilter
    /// </summary>
    public class SessionTokenExceptionFilter : ExceptionFilterAttribute
    {
        private ITokenService _tokenService;
        private IApiHelpPageService _apiHelpPageService;
        /// <summary>
        /// SessionTokenExceptionFilter
        /// </summary>
        public SessionTokenExceptionFilter()
        {
            _tokenService = new TokenService();
            _apiHelpPageService = new ApiHelpPageService();
        }
        /// <summary>
        /// OnException
        /// </summary>
        /// <param name="context"></param>
        public override void OnException(HttpActionExecutedContext context)
        {
            var response = new Response<string>();
            string result = _apiHelpPageService.ApiList().Where(x => x.ApiName == context.ActionContext.ActionDescriptor.ActionName).Select(x => x.Response).FirstOrDefault();
            response.Create(false, context.Exception.Message, HttpStatusCode.InternalServerError, result);
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
            base.OnException(context);
        }
    }
    /// <summary>
    /// TempTokenExceptionFilter
    /// </summary>
    public class TempTokenExceptionFilter : ExceptionFilterAttribute
    {
        private ITokenService _tokenService;
        private IApiHelpPageService _apiHelpPageService;

        /// <summary>
        /// TempTokenExceptionFilter
        /// </summary>
        public TempTokenExceptionFilter()
        {
            _tokenService = new TokenService();
            _apiHelpPageService = new ApiHelpPageService();
        }
        /// <summary>
        /// OnException
        /// </summary>
        /// <param name="context"></param>
        public override void OnException(HttpActionExecutedContext context)
        {
            bool IsAuthenticated = HttpContext.Current.Request.IsAuthenticated;
            var response = new Response<string>();
            string errorMessage = context.Exception.Message;
            if (context.Exception.InnerException != null && !string.IsNullOrEmpty(context.Exception.InnerException.Message))
            {
                errorMessage = errorMessage + context.Exception.InnerException.Message;
            }
            string result = _apiHelpPageService.ApiList().Where(x => x.ApiName == context.ActionContext.ActionDescriptor.ActionName).Select(x => x.Response).FirstOrDefault();
            response.Create(false, context.Exception.Message, HttpStatusCode.InternalServerError, result);

            if (context.ActionContext.ActionDescriptor.ActionName == "TempToken")
            {
                context.Response = context.Request.CreateResponse(HttpStatusCode.InternalServerError, response);
            }
            else
            {
                string responseString = JsonConvert.SerializeObject(response);
                var tokenPair = _tokenService.KeysByTempToken();
                responseString = AES256.Encrypt(tokenPair.PublicKey, responseString);
                context.Response = context.Request.CreateResponse(HttpStatusCode.InternalServerError, responseString);
            }
            "Exception Filter".ErrorLog("ExceptionFilter.cs", "OnException", errorMessage);
            base.OnException(context);
        }
    }
}