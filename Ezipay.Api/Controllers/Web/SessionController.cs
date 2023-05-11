using ezeePay.Utility.CommonClass;
using Ezipay.Api.Filters;
using Ezipay.Service.TokenService;
using Ezipay.ViewModel.common;
using Ezipay.ViewModel.TokenViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Ezipay.Api.Controllers
{
    [RoutePrefix("api/SessionController")]
    public class SessionController : ApiController
    {
        private ITokenService _tokenService;
        private IHttpActionResult _iHttpActionResult;
        private Converter _converter;

        public SessionController(ITokenService tokenService)
        {
            _tokenService = tokenService;
            _converter = new Converter();
        }
        /// <summary>
        /// Generate Temp Token
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("TempToken")]
        // [TempTokenExceptionFilter]
        [ResponseType(typeof(Response<TempTokenResponse>))]
        public async Task<IHttpActionResult> TempToken(TempTokenRequest request)
        {
            var response = new Response<TempTokenResponse>();
            var result = new TempTokenResponse();

            try
            {
                GlobalData.RoleId = new TempSessionAuthorization().GetRoleId(Request);
                //if (request.AppType==2)
                //{
                    //    var ee = GetHttpContext(request);          
                    result = await _tokenService.GenerateTempToken(request);

                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, false);
                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NoContent, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.NoContent, false, false);
                    }
                //}
                //else
                //{
                //    response = response.Create(false,"Please update your application", HttpStatusCode.NotFound, result);
                //    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, false);
                //}
            }
            catch (Exception ex)
            {
                response = response.Create(false, ResponseMessages.EXCEPTION_OCCURED, HttpStatusCode.NoContent, result);
                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.NoContent, false, false);
            }
            return _iHttpActionResult;
        }
    }
}
