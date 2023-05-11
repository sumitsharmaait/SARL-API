using ezeePay.Utility.CommonClass;
using Ezipay.Api.Filters;
using Ezipay.Service.AfroBasket;
using Ezipay.Service.CommonService;
using Ezipay.Service.EzipayPartner;
using Ezipay.Utility.common;
using Ezipay.ViewModel.AfroBasketViewModel;
using Ezipay.ViewModel.common;
using Ezipay.ViewModel.ThridPartyApiVIewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Ezipay.Api.Controllers.Web
{
    [RoutePrefix("api/EzipayPartner")]
    public class EzipayPartnerController : ApiController
    {
        private IHttpActionResult _iHttpActionResult;
        private Converter _converter;
        private IEzipayPartnerService _ezipayPartnerService;
        private ICommonServices _commonServices;

        /// <summary>
        /// EzipayPartnerController
        /// </summary>
        /// <param name="ezipayPartnerService"></param>
        /// <param name="commonServices"></param>
        public EzipayPartnerController(IEzipayPartnerService ezipayPartnerService, ICommonServices commonServices)
        {
            _ezipayPartnerService = ezipayPartnerService;
            _commonServices = commonServices;
            _converter = new Converter();
        }

        /// <summary>
        /// PaymentWalletVerification for afrobasket
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<AfroBasketVerificationResponse>))]
        [Description("PaymentWalletVerification")]
        [Route("PaymentWalletVerification")]
        public async Task<IHttpActionResult> PaymentWalletVerification(AfroBasketVerificationRequest request)
        {
            var response = new Response<AfroBasketVerificationResponse>();
            var _response = new AfroBasketVerificationResponse();
            if (ModelState.IsValid)
            {
                _response = await _ezipayPartnerService.PaymentWalletVerification(request);
                if (_response.IsSuccess)
                {
                    response = response.Create(true, _response.Message, HttpStatusCode.OK, _response);
                }
                else
                {
                    response = response.Create(false, _response.Message, HttpStatusCode.NotFound, _response);
                }
            }
            else
            {
                if (request.UserId == null)
                {
                    response = response.Create(false, _response.Message, HttpStatusCode.NotAcceptable, _response);
                }
                else
                {
                    response = response.Create(false, _response.Message, HttpStatusCode.ExpectationFailed, _response);
                }
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, false, false);
            return _iHttpActionResult;
        }

        /// <summary>
        /// PaymentByUserWallet
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<AfroBasketPaymentVerifyResponse>))]
        [Description("PaymentByUserWallet")]
        [Route("PaymentByUserWallet")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        public async Task<IHttpActionResult> PaymentByUserWallet(AfroBasketVerifyRequest request)
        {
            var response = new Response<AfroBasketPaymentVerifyResponse>();
            var _response = new AfroBasketPaymentVerifyResponse();
            var headerToken = Request.Headers.GetValues("token").FirstOrDefault();
            if (ModelState.IsValid)
            {
                _response = await _ezipayPartnerService.PaymentByUserWallet(request, headerToken);
                if (_response.IsSuccess)
                {
                    response = response.Create(true, _response.Message, HttpStatusCode.OK, _response);
                }
                else
                {
                    response = response.Create(false, _response.Message, HttpStatusCode.NotFound, _response);
                }
            }
            else
            {
                if (request.UserId == null)
                {
                    response = response.Create(false, _response.Message, HttpStatusCode.NotAcceptable, _response);
                }
                else
                {
                    response = response.Create(false, _response.Message, HttpStatusCode.ExpectationFailed, _response);
                }
            }

            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, false, false);
            return _iHttpActionResult;
        }

        /// <summary>
        /// AfroDataVerification
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// client side verification they use
        [HttpPost]
        [Route("EzipayPartnerDataVerification")]
        [ResponseType(typeof(Response<VerifyResponse>))]
        public async Task<IHttpActionResult> EzipayPartnerDataVerification(VerifyAfroBasketRequest request)
        {
            var response = new Response<VerifyResponse>();
            var result = new VerifyResponse();
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _ezipayPartnerService.DataVerification(request);
                    if (result != null && result.emailid != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);
                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.OK, result);
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
                response = response.Create(false, ResponseMessages.DATA_RECEIVED, HttpStatusCode.InternalServerError, result);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, false, false);
            return _iHttpActionResult;
        } 


        /// <summary>
        /// FlightHotelBooking
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<AfroBasketLoginResponse>))]
        [Route("LoginWithEzipayPartner")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        public async Task<IHttpActionResult> LoginWithEzipayPartner(RequestModel request)
        {
            var response = new Response<AfroBasketLoginResponse>();
            var result = new AfroBasketLoginResponse();
            var requestModel = new EncrDecr<EzipayPartnerLoginRequest>().Decrypt(request.Value, false);
            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();
            bool IsCorrectPassword = false;
            if (ModelState.IsValid)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(requestModel.Password))
                    {
                        IsCorrectPassword = await _commonServices.CheckPassword(requestModel.Password, sessionToken);
                    }
                    else
                    {
                        IsCorrectPassword = true;
                    }
                    if (IsCorrectPassword)
                    {
                        result = await _ezipayPartnerService.LoginWithEzipayPartner(requestModel, sessionToken);
                        switch (result.RstKey)
                        {
                            case 0:
                                response = response.Create(true, ResponseMessages.INVALID_PASSWORD, HttpStatusCode.NotFound, result);
                                break;
                            case 1:
                                response = response.Create(true, AggregatoryMESSAGE.SUCCESSFUL, HttpStatusCode.OK, result);
                                break;
                            case 2:
                                response = response.Create(true, AggregatoryMESSAGE.PENDING, HttpStatusCode.OK, result);
                                break;
                            case 3:
                                response = response.Create(true, AggregatoryMESSAGE.FAILED, HttpStatusCode.OK, result);
                                break;
                            case 4:
                                response = response.Create(true, ResponseMessages.AGGREGATOR_FAILED_ERROR, HttpStatusCode.OK, result);
                                break;
                            case 5:
                                response = response.Create(true, ResponseMessages.AGGREGATOR_FAILED_EXCEPTION, HttpStatusCode.OK, result);
                                break;
                            case 6:
                                response = response.Create(true, result.Message, HttpStatusCode.OK, result);
                                break;
                            case 7:
                                response = response.Create(true, ResponseMessages.TRANSACTION_ERROR, HttpStatusCode.OK, result);
                                break;
                            case 8:
                                response = response.Create(true, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.OK, result);
                                break;
                            case 9:
                                response = response.Create(true, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.OK, result);
                                break;
                            case 10:
                                response = response.Create(true, ResponseMessages.INSUFICIENT_BALANCE, HttpStatusCode.NotAcceptable, result);
                                break;
                            case 11:
                                response = response.Create(true, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.OK, result);
                                break;
                            case 12:
                                response = response.Create(true, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.OK, result);
                                break;
                            case 13:
                                response = response.Create(true, ResponseMessageKyc.FAILED_Doc_NotUploaded, HttpStatusCode.OK, result);
                                break;
                            case 14:
                                response = response.Create(true, ResponseMessageKyc.FAILED_Doc_Pending, HttpStatusCode.OK, result);
                                break;
                            case 15:
                                response = response.Create(true, ResponseMessageKyc.Doc_Not_visible, HttpStatusCode.OK, result);
                                break;
                            case 16:
                                response = response.Create(true, ResponseMessageKyc.Doc_Rejected, HttpStatusCode.OK, result);
                                break;
                            case 17:
                                response = response.Create(true, ResponseMessageKyc.TRANSACTION_DISABLED, HttpStatusCode.OK, result);
                                break;
                            case 18:
                                var errorList = new List<Errorkey>();
                                response = response.Create(true, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.OK, result);
                                break;
                            case 19:
                                response = response.Create(true, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.OK, result);
                                break;
                            default:
                                response = response.Create(true, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.OK, result);
                                break;
                        }
                    }
                    else
                    {
                        response = response.Create(true, ResponseMessages.INVALID_PASSWORD, HttpStatusCode.NotFound, result);
                        //_iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false);
                        //return _iHttpActionResult;
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
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
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false);
            return _iHttpActionResult;
        } //



        /// <summary>
        /// GetUserCurrentBalance for wi-flix
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetUserCurrentBalance")]
        [ResponseType(typeof(Response<GetUserCurrentBalanceResponse>))]
        public async Task<IHttpActionResult> GetUserCurrentBalance(GetUserCurrentBalanceRequest request)
        {
            var response = new Response<GetUserCurrentBalanceResponse>();
            var result = new GetUserCurrentBalanceResponse();
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _ezipayPartnerService.GetWalletUser(request);
                    if (result != null && result.CurrentBalance != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);
                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.OK, result);
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
                if (request.WalletUserId == 0)
                {
                    errorList.Add(new Errorkey { Key = "WalletUserId", Val = "WalletUserId can not be null" });
                }
                if (string.IsNullOrWhiteSpace(request.EmailId))
                {
                    errorList.Add(new Errorkey { Key = "Email", Val = "EmailId is required" });
                }
                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.InternalServerError, result);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, false, false);
            return _iHttpActionResult;
        }
    }
}
