using ezeePay.Utility.CommonClass;
using Ezipay.Api.Filters;
using Ezipay.Service.AdminService;
using Ezipay.Service.TokenService;
using Ezipay.Service.UserService;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.common;
using Ezipay.ViewModel.SendEmailViewModel;
using Ezipay.ViewModel.WalletUserVM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace Ezipay.Api.Controllers
{
    [RoutePrefix("api/UserController")]
    public class UserController : ApiController
    {
        private IWalletUserService _walletUserService;
        private IHttpActionResult _iHttpActionResult;
        private Converter _converter;
        private ITokenService _tokenService;
        private IUserApiService _userApiService;
        /// <summary>
        ///  Ctor
        /// </summary>
        /// <param name="walletUserService"></param>
        public UserController(IWalletUserService walletUserService, ITokenService tokenService, IUserApiService userApiService)
        {
            _walletUserService = walletUserService;
            _converter = new Converter();
            _tokenService = tokenService;
            _userApiService = userApiService;
        }

        /// <summary>
        /// SignUp
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SignUp")]
        [TempSessionAuthorization]
        [TempTokenExceptionFilter]
        [ResponseType(typeof(Response<UserSignupResponse>))]
        public async Task<IHttpActionResult> SignUp(RequestModel request)
        {

            var response = new Response<UserSignupResponse>();
            var result = new UserSignupResponse();
            var requestModel = new EncrDecr<UserSignupRequest>().Decrypt(request.Value, true, Request);
            if (ModelState.IsValid)
            {
                try
                {
                    //if (requestModel.IsdCode == "+234")
                    //{
                    // result.Message = "We are currently not accepting new user registration at the moment as we approving previous enrolled users.";
                    //result.Message = "New registration are close due  to system Upgradation.";
                    //response = response.Create(false, result.Message, HttpStatusCode.NotFound, result);
                    //}
                    //else
                    //{
                    if (requestModel.FirstName == null && (string.IsNullOrWhiteSpace(requestModel.EmailId) || string.IsNullOrWhiteSpace(requestModel.Password) || string.IsNullOrWhiteSpace(requestModel.MobileNo) || string.IsNullOrWhiteSpace(requestModel.IsdCode)))
                    {
                        response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotAcceptable, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError, true, true, Request);
                    }


                    else if (Regex.IsMatch(requestModel.MobileNo, "^[a-zA-Z]+$"))
                    {
                        response = response.Create(false, ResponseMessages.INVALID_MOBILE_NO, HttpStatusCode.NotAcceptable, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError, true, true, Request);

                    }
                    else
                    {
                        //var OtpVerification = await _walletUserService.VerifyOtp(new VerifyOtpRequest { MobileNo = requestModel.MobileNo, IsdCode = requestModel.IsdCode, Otp = requestModel.Otp });

                        //if (OtpVerification.Status == 2)
                        //{
                        result = await _walletUserService.SignUp(requestModel);
                        switch (result.RstKey)
                        {
                            case 1:
                                response = response.Create(true, ResponseMessages.USER_REGISTERED, HttpStatusCode.OK, result);
                                break;
                            case 2:
                                response = response.Create(false, ResponseMessages.EXIST_MOBILE_NO, HttpStatusCode.NotAcceptable, result);
                                break;
                            case 3:
                                response = response.Create(false, ResponseMessages.EXIST_EMAIL, HttpStatusCode.OK, result);

                                break;
                            case 4:
                                response = response.Create(false, ResponseMessages.BothExist, HttpStatusCode.OK, result);

                                break;
                            case 5:
                                response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.OK, result);

                                break;
                            case 6:
                                response = response.Create(false, ResponseMessages.DUPLICATE_CREDENTIALS, HttpStatusCode.OK, result);

                                break;
                            case 7:
                                response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.OK, result);

                                break;
                            case 8:
                                response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.OK, result);

                                break;
                            case 10:
                                var errorList = new List<Errorkey>();

                                if (string.IsNullOrWhiteSpace(requestModel.EmailId))
                                {
                                    errorList.Add(new Errorkey { Key = "Email", Val = "Email is required" });
                                }

                                if (string.IsNullOrWhiteSpace(requestModel.IsdCode))
                                {
                                    errorList.Add(new Errorkey { Key = "CountryCode", Val = "CountryCode is required" });
                                }
                                if (string.IsNullOrWhiteSpace(requestModel.MobileNo))
                                {
                                    errorList.Add(new Errorkey { Key = "PhoneNumber", Val = "PhoneNumber is required" });
                                }

                                response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.OK, result);

                                break;
                            case 11:
                                response = response.Create(true, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.OK, result);

                                break;
                            default:
                                response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.OK, result);

                                break;
                        }
                        //}
                        //else
                        //{
                        //    response = response.Create(false, ResponseMessages.OTP_NOT_VERIFIED, HttpStatusCode.OK, result);

                        //}
                        //    }
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ex.Message, HttpStatusCode.OK, result);

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
                response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.OK, result);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, true, Request);
            return _iHttpActionResult;
        }


        /// <summary>
        /// SendOtp
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SendOtp")]
        [SessionAuthorization]
        [ResponseType(typeof(Response<OtpResponse>))]
        [SessionTokenExceptionFilter]
        public async Task<IHttpActionResult> SendOtp(RequestModel request)
        {
            var response = new Response<Object>();
            var result = new OtpResponse();
            var requestModel = new EncrDecr<OtpRequest>().Decrypt(request.Value, false, Request);
            // int langId = AppUtils.GetLangId(Request);
            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();////

            if (ModelState.IsValid)
            {
                try
                {
                    var req = new UserExistanceRequest { MobileNo = requestModel.MobileNo };
                    var resultCreds = await _walletUserService.CredentialsExistanceForMobileNumber(req); //check mobile 
                    if (resultCreds.RstKey != 2)
                    {
                        result = await _walletUserService.SendOtp(requestModel, sessionToken);
                        if ((int)result.StatusCode == 1)
                        {
                            response = response.Create(true, ResponseMessages.OTP_SENT, HttpStatusCode.OK, result);
                        }
                        else if ((int)result.StatusCode == 3)
                        {
                            response = response.Create(false, ResponseMessages.REQUEST_SENT_Not, HttpStatusCode.OK, result);

                        }
                        else if ((int)result.StatusCode == 5)
                        {
                            response = response.Create(false, ResponseMessages.VERIFICATION_EMAIL, HttpStatusCode.OK, result);

                        }
                        else
                        {
                            response = response.Create(false, ResponseMessages.OTP_NOT_SENT, HttpStatusCode.OK, result);
                        }
                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.EXIST_MOBILE_NO, HttpStatusCode.OK, result);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.OTP_NOT_SENT, HttpStatusCode.OK, result);
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
                response = response.Create(false, ResponseMessages.OTP_NOT_SENT, HttpStatusCode.InternalServerError, result);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);//change
            return _iHttpActionResult;

        }

        // <summary>
        // SendOtp
        // </summary>
        // <param name = "request" ></ param >
        // < returns ></ returns >
        [HttpPost]
        [Route("CallBackOtp")]
        [SessionAuthorization]
        [ResponseType(typeof(Response<OtpResponse>))]
        [SessionTokenExceptionFilter]
        public async Task<IHttpActionResult> CallBackOtp(RequestModel request)
        {
            var response = new Response<Object>();
            var result = new OtpResponse();
            var requestModel = new EncrDecr<OtpRequest>().Decrypt(request.Value, false, Request);
            // int langId = AppUtils.GetLangId(Request);
            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();////

            if (ModelState.IsValid)
            {
                try
                {
                    var req = new UserExistanceRequest { MobileNo = requestModel.MobileNo };
                    var resultCreds = await _walletUserService.CredentialsExistanceForMobileNumber(req);
                    if (resultCreds.RstKey != 2)
                    {
                        result = await _walletUserService.CallBackOtp(requestModel, sessionToken);
                        if ((int)result.StatusCode == 1)
                        {
                            response = response.Create(true, ResponseMessages.OTP_SENT, HttpStatusCode.OK, result);

                        }
                        else if ((int)result.StatusCode == 2)
                        {
                            response = response.Create(false, ResponseMessages.REQUEST_SENT_ONE, HttpStatusCode.OK, result);

                        }
                        else if ((int)result.StatusCode == 3)
                        {
                            response = response.Create(false, ResponseMessages.REQUEST_SENT_Not, HttpStatusCode.OK, result);

                        }
                        else
                        {
                            response = response.Create(false, ResponseMessages.OTP_NOT_SENT, HttpStatusCode.OK, result);
                        }
                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.EXIST_MOBILE_NO, HttpStatusCode.OK, result);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.OTP_NOT_SENT, HttpStatusCode.OK, result);
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
                response = response.Create(false, ResponseMessages.OTP_NOT_SENT, HttpStatusCode.InternalServerError, result);
            }
            // _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, true);
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);//change
            return _iHttpActionResult;
        }

        /// <summary>
        /// Verify OTP
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("VerifyOTP")]
        // [TempSessionAuthorization]
        [SessionAuthorization]
        [ResponseType(typeof(Response<Object>))]
        public async Task<IHttpActionResult> VerifyOTP(RequestModel request)
        {
            var response = new Response<Object>();
            var result = new UserExistanceResponse();
            //  int langId = AppUtils.GetLangId(Request);
            var requestModel = new EncrDecr<VerifyOtpRequest>().Decrypt(request.Value, false, Request);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _walletUserService.VerifyOtp(requestModel);
                    if (result.RstKey == 1)
                    {
                        response = response.Create(true, ResponseMessages.OTP_VERIFIED, HttpStatusCode.OK, true);

                    }
                    else if (result.RstKey == 2)
                    {
                        response = response.Create(false, ResponseMessages.OTP_NOT_VERIFIED, HttpStatusCode.OK, false);

                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.OTP_NOT_VERIFIED, HttpStatusCode.OK, false);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.EXCEPTION_OCCURED, HttpStatusCode.InternalServerError, false);
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
                response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.InternalServerError, false);
            }
            // _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, true, Request);
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);//change
            return _iHttpActionResult;
        }


        [HttpPost]
        [Route("Login")]
        [TempSessionAuthorization]
        //[SessionAuthorization]      
        [ResponseType(typeof(Response<UserLoginResponse>))]
        public async Task<IHttpActionResult> Login(RequestModel request)
        {
            var response = new Response<UserLoginResponse>();
            var result = new UserLoginResponse();
            var requestModel = new EncrDecr<UserLoginRequest>().Decrypt(request.Value, true, Request);


            if (ModelState.IsValid)
            {
                try
                {

                    result = await _walletUserService.Login(requestModel);
                    switch (result.RstKey)
                    {
                        case 1:
                            response = response.Create(false, ResponseMessages.INACTIVE_USER, HttpStatusCode.NotFound, result);
                            break;
                        case 2:
                            response = response.Create(false, ResponseMessages.INACTIVE_USER, HttpStatusCode.NotFound, result);
                            break;
                        case 3:
                            response = response.Create(false, ResponseMessages.INVALID_USER_TYPE, HttpStatusCode.NotFound, result);
                            break;
                        case 4:
                            response = response.Create(false, ResponseMessages.USER_NOT_EXIST, HttpStatusCode.NotFound, result);
                            break;
                        case 5:
                            response = response.Create(false, ResponseMessages.INACTIVE_USER, HttpStatusCode.NotFound, result);
                            break;
                        case 6:
                            response = response.Create(true, ResponseMessages.USER_LOGIN, HttpStatusCode.OK, result);
                            break;
                        case 21:
                            response = response.Create(false, ResponseMessages.Merchant_Login_Failed, HttpStatusCode.OK, result);
                            break;
                        case 22:
                            response = response.Create(false, ResponseMessages.USER_PASSWORD_EXPIRED, HttpStatusCode.OK, result);
                            break;
                        default:
                            response = response.Create(false, ResponseMessages.LOGIN_FAILED, HttpStatusCode.NotFound, result);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.LOGIN_FAILED, HttpStatusCode.NotFound, result);
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
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, true, Request);
            return _iHttpActionResult;
        }

        /// <summary>
        /// Logout
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("Logout")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<Object>))]
        public async Task<IHttpActionResult> Logout()
        {
            var response = new Response<Object>();
            var result = new Object();
            // int langId = AppUtils.GetLangId(Request);
            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();////
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _walletUserService.Logout(sessionToken);
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


        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<UserDetailByQrCodeResponse>))]
        [SessionAuthorization]
        [Route("UserDetailById")]
        [Description("UserDetailBy Id(Mobile no or Qr Code) using mobile no or qr code.")]
        public async Task<IHttpActionResult> UserDetailById(UserDetailByQrCodeRequest request)
        {
            var response = new Response<UserDetailByQrCodeResponse>();
            var result = new UserDetailByQrCodeResponse();
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _walletUserService.UserDetailById(request);
                    switch (result.RstKey)
                    {
                        case 1:
                            response = response.Create(false, ResponseMessages.DATA_RECEIVED, HttpStatusCode.NotFound, result);
                            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                            break;
                        case 2:
                            response = response.Create(false, ResponseMessages.INACTIVE_USER, HttpStatusCode.NotFound, result);
                            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                            break;
                        case 3:
                            response = response.Create(false, ResponseMessages.INVALID_USER_TYPE, HttpStatusCode.NotFound, result);
                            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                            break;
                        case 4:
                            response = response.Create(false, ResponseMessages.USER_NOT_EXIST, HttpStatusCode.NotFound, result);
                            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                            break;
                        case 5:
                            response = response.Create(false, ResponseMessages.INACTIVE_USER, HttpStatusCode.NotFound, result);
                            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                            break;
                        case 6:
                            response = response.Create(true, ResponseMessages.USER_LOGIN, HttpStatusCode.OK, result);
                            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                            break;
                        default:
                            response = response.Create(false, ResponseMessages.LOGIN_FAILED, HttpStatusCode.NotFound, result);
                            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.LOGIN_FAILED, HttpStatusCode.NotFound, result);
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
                response = response.Create(false, ResponseMessages.LOGIN_FAILED, HttpStatusCode.InternalServerError, result);
                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
            }
            return _iHttpActionResult;
        }

        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<UserDetailResponse>))]
        [SessionAuthorization]
        [Route("UserProfile")]
        [Description("user detail by token")]
        public async Task<IHttpActionResult> UserProfile()
        {
            var response = new Response<UserDetailResponse>();
            var result = new UserDetailResponse();
            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();

            if (ModelState.IsValid)
            {
                try
                {
                    result = await _walletUserService.UserProfile(sessionToken); ////change
                    switch (result.RstKey)
                    {
                        case 1:
                            response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);
                            break;
                        case 2:
                            response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);
                            break;
                        default:
                            response = response.Create(true, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.OK, result);
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
                response = response.Create(false, ResponseMessages.LOGIN_FAILED, HttpStatusCode.OK, result);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false);
            return _iHttpActionResult;
        }

        [AcceptVerbs("POST")]
        [SessionAuthorization]
        [Route("DocumentUpload")]
        [Description("Document Upload")]
        [ResponseType(typeof(Response<Object>))]
        [SessionAuthorization]
        public async Task<IHttpActionResult> DocumentUpload(RequestModel request)
        {
            var response = new Response<Object>();
            var result = new Object();
            // int langId = AppUtils.GetLangId(Request);
            var requestModel = new EncrDecr<DocumentUploadRequest>().Decrypt(request.Value, false);
            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _walletUserService.DocumentUpload(requestModel, sessionToken);
                    if ((bool)result == true)
                    {
                        response = response.Create(true, ResponseMessages.DOCUMENT_UPLOADED, HttpStatusCode.OK, result);
                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DOCUMENT_NOT_UPLOADED, HttpStatusCode.NotAcceptable, result);
                    }
                }
                catch (Exception ex)
                {
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
                response = response.Create(false, ResponseMessages.DOCUMENT_NOT_UPLOADED, HttpStatusCode.NotAcceptable, result);
                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false);
            return _iHttpActionResult;
        }


        [AcceptVerbs("POST")]
        [SessionAuthorization]
        [Route("GenerateQrCode")]
        [Description("GenerateQrCode")]
        [ResponseType(typeof(Response<Object>))]
        public async Task<IHttpActionResult> GenerateQrCode(QrCodeRequest QrCodeRequest)
        {
            var response = new Response<Object>();
            var result = new Object();
            // int langId = AppUtils.GetLangId(Request);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _walletUserService.GenerateQrCode(QrCodeRequest);
                    if (result != null)
                    {
                        response = response.Create(true, "Success", HttpStatusCode.OK, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                    }
                    else
                    {
                        response = response.Create(false, "Failed", HttpStatusCode.NotAcceptable, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, "Failed", HttpStatusCode.NotAcceptable, result);
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
                response = response.Create(false, "Failed", HttpStatusCode.NotAcceptable, result);
                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
            }
            return _iHttpActionResult;
        }


        //[AcceptVerbs("POST")]
        //[ResponseType(typeof(Response<bool>))]
        //[Description("UpdateUserProfile.")]
        //[Route("UpdateUserProfile")]
        //[SessionAuthorization]
        //[ResponseType(typeof(Response<Object>))]
        //public async Task<IHttpActionResult> UpdateUserProfile()
        //{
        //    var response = new Response<Object>();
        //    var result = new Object();
        //    try
        //    {
        //        HttpContextWrapper objwrapper = GetHttpContext(this.Request);
        //        HttpPostedFileBase collection = objwrapper.Request.Files["UploadedImage"];
        //        string firstName = objwrapper.Request.Form["firstName"];
        //        string lastName = objwrapper.Request.Form["lastName"];
        //        string emailId = objwrapper.Request.Form["emailId"];
        //        // int langId = AppUtils.GetLangId(Request);
        //        //HttpContextWrapper objwrapper = GetHttpContext(this.Request);
        //        //HttpPostedFileBase collection = objwrapper.Request.Files["UploadedImage"];
        //        //string jsonvalue = objwrapper.Request.Form["json"];
        //        var request = new UserDetailResponse();
        //        request.FirstName = firstName;
        //        request.LastName = lastName;
        //        request.EmailId = emailId;
        //        request.ProfileImage = string.Empty;
        //        if (collection != null)
        //        {
        //            request.ProfileImage = await _userApiService.SaveImage(collection, request.PreImage);
        //            if (!string.IsNullOrEmpty(request.ProfileImage))
        //            {
        //                request.ProfileImage = ConfigurationManager.AppSettings["ImageUrl"] + request.ProfileImage;
        //                result = await _walletUserService.UpdateUserProfile(request);
        //            }
        //        }
        //        else
        //        {
        //            result = await _walletUserService.UpdateUserProfile(request);
        //        }

        //        if (result != null)
        //        {
        //            response = response.Create(true, ResponseMessages.PROFILE_UPDATED, HttpStatusCode.OK, result);

        //        }
        //        else
        //        {
        //            response = response.Create(false, ResponseMessages.PROFILE_NOT_UPDATED, HttpStatusCode.OK, result);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        response = response.Create(false, ResponseMessages.PROFILE_NOT_UPDATED, HttpStatusCode.OK, result);
        //    }

        //    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false);
        //    return _iHttpActionResult;
        //}

        HttpContextWrapper GetHttpContext(HttpRequestMessage request = null)
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


        /// <summary>
        /// ChangePassword
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ChangePassword")]
        [SessionAuthorization]
        [ResponseType(typeof(Response<ChangePasswordResponse>))]
        public async Task<IHttpActionResult> ChangePassword(RequestModel request)
        {
            var response = new Response<ChangePasswordResponse>();
            var result = new ChangePasswordResponse();
            var requestModel = new EncrDecr<ChangePasswordRequest>().Decrypt(request.Value, false);
            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _walletUserService.ChangePassword(requestModel, sessionToken);
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
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false);
            return _iHttpActionResult;
        }

        /// <summary>
        /// ChangePassword
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ForgotPassword")]
        [TempSessionAuthorization]
        [ResponseType(typeof(Response<object>))]
        public async Task<IHttpActionResult> ForgotPassword(RequestModel request)
        {
            var response = new Response<object>();
            var result = new object();
            var requestModel = new EncrDecr<ForgotPasswordRequest>().Decrypt(request.Value, true);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _walletUserService.ForgotPassword(requestModel);
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.FORGOT_PASSWORD_SUCCESS, HttpStatusCode.OK, result);
                    }
                    else
                    {
                        response = response.Create(true, ResponseMessages.FORGOT_PASSWORD_UNSUCCESS, HttpStatusCode.OK, result);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.FORGOT_PASSWORD_UNSUCCESS, HttpStatusCode.OK, result);
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
                response = response.Create(false, ResponseMessages.FORGOT_PASSWORD_UNSUCCESS, HttpStatusCode.InternalServerError, result);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, true);
            return _iHttpActionResult;
        }

        /// <summary>
        /// FindCurrentBalance
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("FindCurrentBalance")]
        [SessionAuthorization]
        [ResponseType(typeof(Response<CurrentBalanceResponse>))]
        public async Task<IHttpActionResult> FindCurrentBalance()
        {
            var response = new Response<CurrentBalanceResponse>();
            var result = new CurrentBalanceResponse();
            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();////
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _walletUserService.FindCurrentBalance(sessionToken);
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);
                    }
                    else
                    {
                        response = response.Create(true, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.OK, result);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.OK, result);
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
                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.InternalServerError, result);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false);
            return _iHttpActionResult;
        }


        /// <summary>
        /// IsFirstTransaction
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("IsFirstTransaction")]
        [SessionAuthorization]
        [ResponseType(typeof(Response<IsFirstTransactionResponse>))]
        public async Task<IHttpActionResult> IsFirstTransaction()
        {
            var response = new Response<IsFirstTransactionResponse>();
            var result = new IsFirstTransactionResponse();
            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();////
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _walletUserService.IsFirstTransaction(sessionToken);
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);
                    }
                    else
                    {
                        response = response.Create(true, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.OK, result);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.OK, result);
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
                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.InternalServerError, result);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false);
            return _iHttpActionResult;
        }

        /// <summary>
        /// UpdateUserProfileForApp
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [SessionAuthorization]
        [Route("UpdateUserProfileForApp")]
        [Description("UpdateUserProfileForApp")]
        [ResponseType(typeof(Response<ProfileUpdateResponse>))]
        [SessionAuthorization]
        public async Task<IHttpActionResult> UpdateUserProfileForApp(RequestModel request)
        {
            var response = new Response<ProfileUpdateResponse>();
            var result = new ProfileUpdateResponse();
            // int langId = AppUtils.GetLangId(Request);
            var requestModel = new EncrDecr<UpdateUserProfileRequest>().Decrypt(request.Value, false);
            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();////
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _walletUserService.UpdateUserProfile(requestModel, sessionToken);
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.PROFILE_UPDATED, HttpStatusCode.OK, result);
                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.PROFILE_UPDATED, HttpStatusCode.NotAcceptable, result);
                    }
                }
                catch (Exception ex)
                {
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
                response = response.Create(false, ResponseMessages.DOCUMENT_NOT_UPLOADED, HttpStatusCode.NotAcceptable, result);
                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false);
            return _iHttpActionResult;
        }

        /// <summary>
        /// ShareQRCode
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<object>))]
        [Description("Share QR Code")]
        [SessionAuthorization]
        [Route("sharecode")]
        public async Task<IHttpActionResult> ShareQRCode(RequestModel request)
        {
            var response = new Response<object>();
            var result = new object();
            // int langId = AppUtils.GetLangId(Request);
            var requestModel = new EncrDecr<QRCodeRequest>().Decrypt(request.Value, false);
            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();////
            if (ModelState.IsValid)
            {
                result = await _walletUserService.ShareQRCode(requestModel, sessionToken);
                if (result != null)
                {
                    response = response.Create(true, ResponseMessages.SHARED_SUCCESS, HttpStatusCode.OK, true);
                }
                else
                {
                    response = response.Create(true, ResponseMessages.SHARED_FAILED, HttpStatusCode.NotFound, false);
                }
            }
            else
            {
                response = response.Create(false, ResponseMessages.INVALID_FORM_DATA, HttpStatusCode.NotAcceptable, false);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false);
            return _iHttpActionResult;
        }

        /// <summary>
        /// Authentication for cash in cash out
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Authentication")]
        // [TempSessionAuthorization]
        //[SessionAuthorization]      
        [ResponseType(typeof(Response<AuthenticationResponse>))]
        public async Task<IHttpActionResult> Authentication(AuthenticationRequest requestModel)
        {
            var response = new Response<AuthenticationResponse>();
            var result = new AuthenticationResponse();
            //var requestModel = new EncrDecr<AuthenticationRequest>().Decrypt(request.Value, true);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _walletUserService.Authentication(requestModel);
                    switch (result.RstKey)
                    {
                        case 1:
                            response = response.Create(false, ResponseMessages.INACTIVE_USER, HttpStatusCode.NotFound, result);
                            break;
                        case 2:
                            response = response.Create(false, ResponseMessages.INACTIVE_USER, HttpStatusCode.NotFound, result);
                            break;
                        case 3:
                            response = response.Create(false, ResponseMessages.INVALID_USER_TYPE, HttpStatusCode.NotFound, result);
                            break;
                        case 4:
                            response = response.Create(false, ResponseMessages.USER_NOT_EXIST, HttpStatusCode.NotFound, result);
                            break;
                        case 5:
                            response = response.Create(false, ResponseMessages.INACTIVE_USER, HttpStatusCode.NotFound, result);
                            break;
                        case 6:
                            response = response.Create(true, ResponseMessages.USER_LOGIN, HttpStatusCode.OK, result);
                            break;
                        case 21:
                            response = response.Create(false, ResponseMessages.Merchant_Login_Failed, HttpStatusCode.OK, result);
                            break;
                        default:
                            response = response.Create(false, ResponseMessages.LOGIN_FAILED, HttpStatusCode.NotFound, result);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.LOGIN_FAILED, HttpStatusCode.NotFound, result);
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
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, false, true);
            return _iHttpActionResult;
        }


        /// <summary>
        /// UpdateUserProfile
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<bool>))]
        [Description("UpdateUserProfile.")]
        [Route("UpdateUserProfile")]
        [SessionAuthorization]
        [ResponseType(typeof(Response<Object>))]
        public async Task<IHttpActionResult> UpdateUserProfile(RequestModel request)
        {
            var requestModel = new EncrDecr<UpdateUserProfileRequest>().Decrypt(request.Value, false, Request);
            var response = new Response<Object>();
            var result = new Object();
            try
            {
                //HttpContextWrapper objwrapper = GetHttpContext(this.Request);
                //HttpPostedFileBase collection = objwrapper.Request.Files["UploadedImage"];
                //string firstName = objwrapper.Request.Form["firstName"];
                //string lastName = objwrapper.Request.Form["lastName"];
                //string emailId = objwrapper.Request.Form["emailId"];

                //request.FirstName = firstName;
                //request.LastName = lastName;
                //request.EmailId = emailId;
                //request.ProfileImage = string.Empty;
                string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();////
                if (requestModel.ProfileImage != null)
                {
                    //request.ProfileImage = await _userApiService.SaveImage(collection, request.PreImage);
                    if (!string.IsNullOrEmpty(requestModel.ProfileImage))
                    {
                        //request.ProfileImage = ConfigurationManager.AppSettings["ImageUrl"] + request.ProfileImage;
                        result = await _walletUserService.UpdateUserProfile(requestModel, sessionToken);
                    }
                }
                else
                {
                    result = await _walletUserService.UpdateUserProfile(requestModel, sessionToken);
                }

                if (result != null)
                {
                    response = response.Create(true, ResponseMessages.PROFILE_UPDATED, HttpStatusCode.OK, result);

                }
                else
                {
                    response = response.Create(false, ResponseMessages.PROFILE_NOT_UPDATED, HttpStatusCode.OK, result);
                }
            }
            catch (Exception ex)
            {
                response = response.Create(false, ResponseMessages.PROFILE_NOT_UPDATED, HttpStatusCode.OK, result);
            }

            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }


        [AcceptVerbs("POST")]
        [Route("AutoEmailSentForPasswordExipry")]
        [ResponseType(typeof(Response<Object>))]
        [SessionAuthorization]
        public async Task<IHttpActionResult> AutoEmailSentForPasswordExipry(RequestModel request)
        {
            var response = new Response<Object>();
            var result = new Object();
            var requestModel = new EncrDecr<QRCodeRequest>().Decrypt(request.Value, false, Request);
            // int langId = AppUtils.GetLangId(Request);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _walletUserService.AutoEmailSentForPasswordExipry(requestModel);
                    if ((bool)result == true)
                    {
                        response = response.Create(true, ResponseMessages.PASSWORD_CHANGED, HttpStatusCode.OK, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.PASSWORD_NOT_CHANGED, HttpStatusCode.NotAcceptable, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.PASSWORD_NOT_CHANGED, HttpStatusCode.NotAcceptable, result);
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
