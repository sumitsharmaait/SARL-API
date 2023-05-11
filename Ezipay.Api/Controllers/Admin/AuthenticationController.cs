using ezeePay.Utility.CommonClass;
using Ezipay.Api.Filters;
using Ezipay.Service.AdminService.AuthenticationService;
using Ezipay.Service.CommonService;
using Ezipay.Service.TokenService;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Ezipay.Api.Controllers.Admin
{
    /// <summary>
    /// Authentication
    /// </summary>
    [RoutePrefix("api/admin")]
    public class AuthenticationController : ApiController
    {
        private IHttpActionResult _iHttpActionResult;
        private readonly Converter _converter;
        private readonly IAuthenticationApiService _authenticationApiService;
        private readonly ITokenService _tokenService;
        private readonly ICommonServices _commonServices;
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="authenticationApiService"></param>
        /// <param name="tokenService"></param>
        public AuthenticationController(IAuthenticationApiService authenticationApiService, ITokenService tokenService, ICommonServices commonServices)
        {
            _authenticationApiService = authenticationApiService;
            _tokenService = tokenService;
            _converter = new Converter();
            _commonServices = commonServices;
        }

        /// <summary>
        /// Login
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Login")]
        [TempSessionAuthorization]
        [ResponseType(typeof(Response<LoginResponse>))]
        public async Task<IHttpActionResult> Login(RequestModel request)
        {
            var response = new Response<LoginResponse>();
            var result = new LoginResponse();

            if (ModelState.IsValid)
            {
                try
                {
                    var requestModel = new EncrDecr<LoginRequest>().Decrypt(request.Value, true);
                    result = await _authenticationApiService.Login(requestModel);

                    switch (result.RstKey)
                    {
                        case 0:
                            response = response.Create(false, ResponseMessages.INACTIVE_USER, HttpStatusCode.NotFound, result);
                            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, true);
                            break;
                        case 1:
                            response = response.Create(false, ResponseMessages.INACTIVE_USER, HttpStatusCode.NotFound, result);
                            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, true);
                            break;
                        case 2:
                            response = response.Create(false, ResponseMessages.INVALID_EMAIL, HttpStatusCode.NotFound, result);
                            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, true);
                            break;
                        case 3:
                            response = response.Create(false, ResponseMessages.INVALID_USER_TYPE, HttpStatusCode.NotFound, result);
                            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, true);
                            break;
                        case 4:
                            response = response.Create(false, ResponseMessages.USER_NOT_EXIST, HttpStatusCode.NotFound, result);
                            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, true);
                            break;
                        case 5:
                            response = response.Create(false, ResponseMessages.INVALID_PASSWORD, HttpStatusCode.NotFound, result);
                            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, true);
                            break;
                        case 6:
                            response = response.Create(true, ResponseMessages.USER_LOGIN, HttpStatusCode.OK, result);
                            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, true);
                            break;
                        case 7:
                            //response = response.Create(true, ResponseMessages.USER_LOGIN_PasswordExpiry, HttpStatusCode.OK, result);
                            response = response.Create(false, ResponseMessages.USER_LOGIN_PasswordExpiry, HttpStatusCode.OK, result);
                            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, true);
                            break;
                        case 8:
                            //response = response.Create(true, ResponseMessages.USER_LOGIN_ENTERED_WRONG_PASSWORD, HttpStatusCode.OK, result);
                            response = response.Create(false, ResponseMessages.USER_LOGIN_ENTERED_WRONG_PASSWORD, HttpStatusCode.OK, result);
                            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, true);
                            break;
                        default:
                            response = response.Create(false, ResponseMessages.LOGIN_FAILED, HttpStatusCode.NotFound, result);
                            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, true);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.LOGIN_FAILED, HttpStatusCode.NotFound, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError, true, true);
                }
            }
            else
            {
                var errorList = new List<Errorkey>();
                foreach (var mod in ModelState)
                {
                    Errorkey objkey = new Errorkey();
                    objkey.Key = mod.Key;
                    if (mod.Value.Errors.Count > 0)
                    {
                        objkey.Val = mod.Value.Errors[0].ErrorMessage;
                    }
                    errorList.Add(objkey);
                }
                response = response.Create(false, ResponseMessages.LOGIN_FAILED, HttpStatusCode.InternalServerError, result);
                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
            }
            return _iHttpActionResult;
        }

        /// <summary>
        /// Logout
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("Logout")]
        [ResponseType(typeof(Response<Object>))]
        public async Task<IHttpActionResult> Logout()
        {
            var response = new Response<Object>();
            var result = new Object();
            // int langId = AppUtils.GetLangId(Request);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _authenticationApiService.Logout();
                    if ((bool)result == true)
                    {
                        response = response.Create(true, ResponseMessages.LOGOUT_SUCCESS, HttpStatusCode.OK, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.LOGOUT_UNSUCCESS, HttpStatusCode.NotAcceptable, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.LOGOUT_UNSUCCESS, HttpStatusCode.NotAcceptable, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
                }
            }
            else
            {
                var errorList = new List<Errorkey>();
                foreach (var mod in ModelState)
                {
                    Errorkey objkey = new Errorkey();
                    objkey.Key = mod.Key;
                    if (mod.Value.Errors.Count > 0)
                    {
                        objkey.Val = mod.Value.Errors[0].ErrorMessage;
                    }
                    errorList.Add(objkey);
                }
                response = response.Create(false, ResponseMessages.LOGOUT_UNSUCCESS, HttpStatusCode.NotAcceptable, result);
                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
            }
            return _iHttpActionResult;
        }

        /// <summary>
        /// NavigationList
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<NavigationResponse>))]
        [Route("NavigationList")]
        public async Task<IHttpActionResult> NavigationList(RequestModel request)
        {
            var response = new Response<NavigationResponse>();
            var result = new NavigationResponse();

            if (ModelState.IsValid)
            {
                try
                {
                    var requestModel = new EncrDecr<NavigationsRequest>().Decrypt(request.Value,false,Request);
                    result = await _authenticationApiService.NavigationList(requestModel);
                    if (result.NavigationList != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
                }
            }
            else
            {
                var errorList = new List<Errorkey>();
                foreach (var mod in ModelState)
                {
                    Errorkey objkey = new Errorkey();
                    objkey.Key = mod.Key;
                    if (mod.Value.Errors.Count > 0)
                    {
                        objkey.Val = mod.Value.Errors[0].ErrorMessage;
                    }
                    errorList.Add(objkey);
                }
                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
            }
            return _iHttpActionResult;
        }

        [HttpPost]
        [Route("ChangePassword")]
        [SessionAuthorization]
        [ResponseType(typeof(Response<ChangePasswordResponse>))]
        public async Task<IHttpActionResult> ChangePassword(RequestModel request)
        {
            var response = new Response<ChangePasswordResponse>();
            var result = new ChangePasswordResponse();
            var requestModel = new EncrDecr<ChangePasswordRequest>().Decrypt(request.Value, false, Request);
            var headerToken = Request.Headers.GetValues("token").FirstOrDefault(); //pfi 
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _authenticationApiService.ChangePassword(requestModel, headerToken);
                    switch (result.RstKey)
                    {
                        case 1:
                            response = response.Create(true, ResponseMessages.PASSWORD_CHANGED, HttpStatusCode.OK, result);
                            break;
                        case 2:
                            response = response.Create(false, ResponseMessages.PASSWORD_NOT_CURRECT, HttpStatusCode.OK, result);
                            break;
                        case 3:
                            response = response.Create(false, ResponseMessages.PASSWORD_NOT_CHANGED, HttpStatusCode.OK, result);
                            break;
                        default:
                            response = response.Create(false, ResponseMessages.PASSWORD_NOT_CHANGED, HttpStatusCode.OK, result);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.LOGIN_FAILED, HttpStatusCode.OK, result);
                }
            }
            else
            {
                var errorList = new List<Errorkey>();
                foreach (var mod in ModelState)
                {
                    Errorkey objkey = new Errorkey();
                    objkey.Key = mod.Key;
                    if (mod.Value.Errors.Count > 0)
                    {
                        objkey.Val = mod.Value.Errors[0].ErrorMessage;
                    }
                    errorList.Add(objkey);
                }
                response = response.Create(false, ResponseMessages.LOGIN_FAILED, HttpStatusCode.InternalServerError, result);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }
        [AcceptVerbs("GET")]
        [ResponseType(typeof(Response<Object>))]
        [Route("CrrentUserDetail")]
        public async Task<IHttpActionResult> CrrentUserDetail()
        {
            var response = new Response<Object>();
            var result = new Object();
            // int langId = AppUtils.GetLangId(Request);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _authenticationApiService.CrrentUserDetail();
                    if ((bool)result == true)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
                }
            }
            else
            {
                var errorList = new List<Errorkey>();
                foreach (var mod in ModelState)
                {
                    Errorkey objkey = new Errorkey();
                    objkey.Key = mod.Key;
                    if (mod.Value.Errors.Count > 0)
                    {
                        objkey.Val = mod.Value.Errors[0].ErrorMessage;
                    }
                    errorList.Add(objkey);
                }
                response = response.Create(false, ResponseMessages.LOGOUT_UNSUCCESS, HttpStatusCode.NotAcceptable, result);
                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
            }
            return _iHttpActionResult;
        }
    }
}
