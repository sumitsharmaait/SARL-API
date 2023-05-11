using ezeePay.Utility.CommonClass;
using Ezipay.Api.Filters;
using Ezipay.Service.UserService;
using Ezipay.ViewModel.common;
using Ezipay.ViewModel.WalletUserVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace Ezipay.Api.Controllers
{
    [RoutePrefix("api/TestController")]
    public class TestController : ApiController
    {
        private IHttpActionResult _iHttpActionResult;
        private Converter _converter;
        private IWalletUserService _walletUserService;

      

        public TestController(IWalletUserService walletUserService)
        {
            _walletUserService = walletUserService;
            _converter = new Converter();
        }
        //[HttpGet]
        //[Route("GetUserDetailByToken")]
        //[SessionAuthorization]
        //[SessionTokenExceptionFilter]
        //[ResponseType(typeof(Response<UserDetailResponse>))]
        //public async Task<IHttpActionResult> GetUserDetailByToken(HttpRequestMessage request = null)
        //{
        //    var response = new Response<UserDetailResponse>();
        //    var result = new UserDetailResponse();
        // //    var ee = GetHttpContext(request);          
        //    result = await _walletUserService.UserProfile();

        //    if (result != null)
        //    {
        //        response = response.Create(false, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);
        //        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
        //    }
        //    return _iHttpActionResult;
        //}

        //[HttpGet]
        //[Route("GetHttpContext")]
        //[SessionAuthorization]
        //[SessionTokenExceptionFilter]
        //public string GetHttpContext(HttpRequestMessage request = null)
        //{
        //    string token = "";
        //    request = request;// ?? Request;

        //    if (request.Properties.ContainsKey("MS_HttpContext"))
        //    {
        //        token = request.Headers.Where(x => x.Key.ToLower() == ("token")).FirstOrDefault().Value.FirstOrDefault();
        //        return token;//((HttpContextWrapper)request.Properties["MS_HttpContext"]);
        //    }
        //    else if (HttpContext.Current != null)
        //    {
        //        return token;// new HttpContextWrapper(HttpContext.Current);
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

    }
}
