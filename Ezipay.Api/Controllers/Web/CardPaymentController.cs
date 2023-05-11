using ezeePay.Utility.CommonClass;
using Ezipay.Api.Filters;
using Ezipay.Service.CardPayment;
using Ezipay.Service.CommonService;
using Ezipay.Service.MobileMoneyService;
using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.AirtimeViewModel;
using Ezipay.ViewModel.CardPaymentViewModel;
using Ezipay.ViewModel.common;
using Ezipay.ViewModel.SendEmailViewModel;
using Ezipay.ViewModel.WalletUserVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
//using static Ezipay.Utility.common.AppSetting;

namespace Ezipay.Api.Controllers
{
    [RoutePrefix("api/CardPaymentController")]
   // [TransactionsAllowed]
    public class CardPaymentController : ApiController
    {
        private ICardPaymentService _cardPaymentService;
        private IMobileMoneyServices _mobileMoneyServices;
        private IHttpActionResult _iHttpActionResult;
        private ICommonServices _commonServices;
        private Converter _converter;

        public CardPaymentController(IMobileMoneyServices mobileMoneyServices, ICardPaymentService cardPaymentService, ICommonServices commonServices)
        {
            _mobileMoneyServices = mobileMoneyServices;
            _cardPaymentService = cardPaymentService;
            _converter = new Converter();
            _commonServices = commonServices;
        }


        //[HttpPost]
        //[Route("CardPayment")]
        //[SessionAuthorization]
        //[SessionTokenExceptionFilter]
        //[ResponseType(typeof(Response<CardAddMoneyResponse>))]
        //public async Task<IHttpActionResult> CardPayment(RequestModel request)
        //{
        //    var response = new Response<CardAddMoneyResponse>();
        //    var result = new CardAddMoneyResponse();
        //    var requestModel = new EncrDecr<CardAddMoneyRequest>().Decrypt(request.Value, false, Request);
        //    //int langId = AppUtils.GetLangId(Request);
        //    string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();
        //    //
        //    if (string.IsNullOrEmpty(requestModel.CardNo))
        //    {
        //        response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
        //        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
        //        return _iHttpActionResult;
        //    }

        //    if (requestModel.Amount != null)
        //    {
        //        //checkin MaximumAmount 
        //        AdminMobileMoneyLimitRequest obj = new AdminMobileMoneyLimitRequest
        //        {
        //            MinimumAmount = "0",
        //            Service = "001"
        //        };
        //        var newCommisionMinChargesonGivenAmount = await _mobileMoneyServices.VerifyMobileMoneyLimit(obj);
        //        //decimal d1 = decimal.Parse(newCommisionMinChargesonGivenAmount.MinimumAmount);
        //        decimal d3 = decimal.Parse(newCommisionMinChargesonGivenAmount.MaximumAmount);
        //        decimal d2 = decimal.Parse(requestModel.Amount);
        //        //if (d2 == d1)
        //        //{
        //        //}
        //        //else if (d2 < d1)
        //        //{
        //        //    response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
        //        //    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
        //        //    return _iHttpActionResult;
        //        //}

        //        if (d2 == d3)
        //        {
        //        }
        //        else if (d2 > d3)
        //        {
        //            response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
        //            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
        //            return _iHttpActionResult;
        //        }

        //        try
        //        {
        //            if (request == null)
        //            {
        //                response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotAcceptable, result);
        //                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
        //            }
        //            else
        //            {

        //                if (!string.IsNullOrEmpty(requestModel.Amount) && !requestModel.Amount.IsZero() && requestModel.Amount.IsTwoDigitDecimal())
        //                {
        //                    result = await _cardPaymentService.CardPayment(requestModel, sessionToken);
        //                    switch (result.RstKey)
        //                    {
        //                        case 0:
        //                            response = response.Create(false, ResponseMessages.INVALID_PASSWORD, HttpStatusCode.NotFound, result);
        //                            break;
        //                        case 1:
        //                            response = response.Create(true, AggregatoryMESSAGE.SUCCESSFUL, HttpStatusCode.OK, result);
        //                            break;
        //                        case 2:
        //                            response = response.Create(false, AggregatoryMESSAGE.PENDING, HttpStatusCode.OK, result);
        //                            break;
        //                        case 3:
        //                            response = response.Create(false, AggregatoryMESSAGE.FAILED, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 4:
        //                            response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_ERROR, HttpStatusCode.NotFound, result);
        //                            break;
        //                        case 5:
        //                            response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_EXCEPTION, HttpStatusCode.NotFound, result);
        //                            break;
        //                        case 6:
        //                            response = response.Create(false, result.Message, HttpStatusCode.NotFound, result);
        //                            break;
        //                        case 7:
        //                            response = response.Create(false, ResponseMessages.TRANSACTION_ERROR, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 8:
        //                            response = response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 9:
        //                            response = response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 10:
        //                            response = response.Create(false, ResponseMessages.INSUFICIENT_BALANCE, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 11:
        //                            response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 12:
        //                            response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 13:
        //                            response = response.Create(false, ResponseMessageKyc.FAILED_Doc_NotUploaded, HttpStatusCode.OK, result);

        //                            break;
        //                        case 14:
        //                            response = response.Create(false, ResponseMessageKyc.FAILED_Doc_Pending, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 15:
        //                            response = response.Create(false, ResponseMessageKyc.Doc_Not_visible, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 16:
        //                            response = response.Create(false, ResponseMessageKyc.Doc_Rejected, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 17:
        //                            response = response.Create(false, ResponseMessageKyc.TRANSACTION_DISABLED, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 18:
        //                            var errorList = new List<Errorkey>();
        //                            if (string.IsNullOrWhiteSpace(requestModel.Amount))
        //                            {
        //                                errorList.Add(new Errorkey { Key = "Amount", Val = "Amount can not be null" });
        //                            }

        //                            response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 19:
        //                            response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 21:
        //                            response = response.Create(false, ResponseMessages.MobileNotVerify, HttpStatusCode.OK, result);
        //                            break;

        //                        default:
        //                            response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.NotFound, result);

        //                            break;
        //                    }
        //                }
        //                else
        //                {
        //                    if (string.IsNullOrEmpty(requestModel.Amount))
        //                    {
        //                        response = response.Create(false, ResponseMessages.EMPTY_AMOUNT, HttpStatusCode.NotFound, result);
        //                    }
        //                    else if (requestModel.Amount.IsZero())
        //                    {
        //                        response = response.Create(false, ResponseMessages.ZERO_AMOUNT, HttpStatusCode.NotFound, result);
        //                    }
        //                    else
        //                    {
        //                        response = response.Create(false, ResponseMessages.IMPROPER_AMOUNT, HttpStatusCode.NotFound, result);
        //                    }
        //                }


        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);
        //            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError, true, false, Request);
        //        }
        //    }
        //    else
        //    {
        //        var errorList = new List<Errorkey>();
        //        foreach (var mod in ModelState)
        //        {
        //            Errorkey objkey = new Errorkey();
        //            objkey.Key = mod.Key;
        //            if (mod.Value.Errors.Count > 0)
        //            {
        //                objkey.Val = mod.Value.Errors[0].ErrorMessage;
        //            }
        //            errorList.Add(objkey);
        //        }
        //        response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);

        //    }
        //    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
        //    return _iHttpActionResult;
        //}


        /// <summary>
        /// MobileMoneyServicesAggregator
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("MobileMoneyServicesAggregator")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<AddMoneyAggregatorResponse>))]
        public async Task<IHttpActionResult> MobileMoneyServicesAggregator(RequestModel request)
        {
            var response = new Response<AddMoneyAggregatorResponse>();
            var result = new AddMoneyAggregatorResponse();
            var requestModel = new EncrDecr<AddMoneyAggregatoryRequest>().Decrypt(request.Value, false, Request);
            //int langId = AppUtils.GetLangId(Request);
            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();

            if (requestModel.Amount != null)
            {
                try
                {
                    if (request == null)
                    {
                        response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotAcceptable, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError, true, false, Request);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(requestModel.Amount) && !requestModel.Amount.IsZero() && requestModel.Amount.IsTwoDigitDecimal())
                        {
                            result = await _cardPaymentService.MobileServicesAggregator(requestModel, sessionToken);
                            switch (result.RstKey) //change cond. 
                            {
                                case 0:
                                    response = response.Create(false, ResponseMessages.INVALID_PASSWORD, HttpStatusCode.NotFound, result);
                                    break;
                                case 1:
                                    response = response.Create(true, AggregatoryMESSAGE.SUCCESSFUL, HttpStatusCode.OK, result);

                                    break;
                                case 2:
                                    response = response.Create(false, AggregatoryMESSAGE.PENDING, HttpStatusCode.Ambiguous, result);

                                    break;
                                case 3:
                                    response = response.Create(false, AggregatoryMESSAGE.FAILED, HttpStatusCode.NotFound, result);

                                    break;
                                case 4:
                                    response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_ERROR, HttpStatusCode.NotFound, result);

                                    break;
                                case 5:
                                    response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_EXCEPTION, HttpStatusCode.NotFound, result);

                                    break;
                                case 6:
                                    response = response.Create(false, result.Message, HttpStatusCode.NotFound, result);

                                    break;
                                case 7:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_ERROR, HttpStatusCode.NotFound, result);

                                    break;
                                case 8:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.NotFound, result);

                                    break;
                                case 9:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.NotFound, result);
                                    break;
                                case 10:
                                    response = response.Create(false, ResponseMessages.INSUFICIENT_BALANCE, HttpStatusCode.NotAcceptable, result);
                                    break;
                                case 11:
                                    response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotFound, result);
                                    break;
                                case 12:
                                    response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotFound, result);
                                    break;
                                case 13:
                                    response = response.Create(false, ResponseMessageKyc.FAILED_Doc_NotUploaded, HttpStatusCode.NotFound, result);
                                    break;
                                case 14:
                                    response = response.Create(false, ResponseMessageKyc.FAILED_Doc_Pending, HttpStatusCode.NotFound, result);

                                    break;
                                case 15:
                                    response = response.Create(false, ResponseMessageKyc.Doc_Not_visible, HttpStatusCode.NotFound, result);
                                    break;
                                case 16:
                                    response = response.Create(false, ResponseMessageKyc.Doc_Rejected, HttpStatusCode.NotFound, result);

                                    break;
                                case 17:
                                    response = response.Create(false, ResponseMessageKyc.TRANSACTION_DISABLED, HttpStatusCode.NotFound, result);

                                    break;
                                case 18:
                                    var errorList = new List<Errorkey>();
                                    if (string.IsNullOrWhiteSpace(requestModel.Amount))
                                    {
                                        errorList.Add(new Errorkey { Key = "Amount", Val = "Amount can not be null" });
                                    }

                                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);

                                    break;
                                case 19:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.NotFound, result);

                                    break;

                                default:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.NotFound, result);

                                    break;
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(requestModel.Amount))
                            {
                                response = response.Create(false, ResponseMessages.EMPTY_AMOUNT, HttpStatusCode.NotFound, result);
                            }
                            else if (requestModel.Amount.IsZero())
                            {
                                response = response.Create(false, ResponseMessages.ZERO_AMOUNT, HttpStatusCode.NotFound, result);
                            }
                            else
                            {
                                response = response.Create(false, ResponseMessages.IMPROPER_AMOUNT, HttpStatusCode.NotFound, result);
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);

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
                response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);
                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError, true, false, Request);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }



        /// <summary>
        /// AddCashDepositToBankResponse
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddCashDepositToBankServices")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<AddCashDepositToBankResponse>))]
        public async Task<IHttpActionResult> AddCashDepositToBankServices(RequestModel request)
        {
            var response = new Response<AddCashDepositToBankResponse>();
            var result = new AddCashDepositToBankResponse();
            var requestModel = new EncrDecr<AddCashDepositToBankRequest>().Decrypt(request.Value, false, Request);

            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();
            //bool IsCorrectPassword = false;
            if (requestModel.DepositorCashAmount != null)
            {
                try
                {
                    if (request == null)
                    {
                        response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotAcceptable, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError, true, false, Request);
                    }
                    else
                    {
                        //if (!string.IsNullOrWhiteSpace(requestModel.Password))
                        //{
                        //    IsCorrectPassword = await _commonServices.CheckPassword(requestModel.Password);
                        //}
                        //else
                        //{
                        //    IsCorrectPassword = true;
                        //}

                        //if (IsCorrectPassword)
                        //{
                        if (!string.IsNullOrEmpty(requestModel.DepositorCashAmount) && !requestModel.DepositorCashAmount.IsZero() && requestModel.DepositorCashAmount.IsTwoDigitDecimal())
                        {
                            result = await _cardPaymentService.AddCashDepositToBankServices(requestModel, sessionToken);
                            switch (result.RstKey)
                            {
                                case 0:
                                    response = response.Create(false, ResponseMessages.INVALID_PASSWORD, HttpStatusCode.NotFound, result);
                                    break;
                                case 1:
                                    response = response.Create(true, AggregatoryMESSAGE.SUCCESSFUL, HttpStatusCode.OK, result);

                                    break;
                                case 3:
                                    response = response.Create(false, AggregatoryMESSAGE.FAILED, HttpStatusCode.NotFound, result);

                                    break;
                                case 4:
                                    response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_ERROR, HttpStatusCode.NotFound, result);

                                    break;
                                case 5:
                                    response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_EXCEPTION, HttpStatusCode.NotFound, result);

                                    break;
                                case 6:
                                    response = response.Create(false, result.Message, HttpStatusCode.NotFound, result);

                                    break;

                                case 10:
                                    response = response.Create(false, ResponseMessages.INSUFICIENT_BALANCE, HttpStatusCode.NotAcceptable, result);
                                    break;
                                case 11:
                                    response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotFound, result);
                                    break;
                                case 12:
                                    response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotFound, result);
                                    break;
                                case 13:
                                    response = response.Create(false, ResponseMessageKyc.FAILED_Doc_NotUploaded, HttpStatusCode.NotFound, result);
                                    break;
                                case 14:
                                    response = response.Create(false, ResponseMessageKyc.FAILED_Doc_Pending, HttpStatusCode.NotFound, result);

                                    break;
                                case 15:
                                    response = response.Create(false, ResponseMessageKyc.Doc_Not_visible, HttpStatusCode.NotFound, result);
                                    break;
                                case 16:
                                    response = response.Create(false, ResponseMessageKyc.Doc_Rejected, HttpStatusCode.NotFound, result);

                                    break;


                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(requestModel.DepositorCashAmount))
                            {
                                response = response.Create(false, ResponseMessages.EMPTY_AMOUNT, HttpStatusCode.NotFound, result);
                            }
                            else if (requestModel.DepositorCashAmount.IsZero())
                            {
                                response = response.Create(false, ResponseMessages.ZERO_AMOUNT, HttpStatusCode.NotFound, result);
                            }
                            else
                            {
                                response = response.Create(false, ResponseMessages.IMPROPER_AMOUNT, HttpStatusCode.NotFound, result);
                            }
                        }
                        //}
                        //else
                        //{
                        //    response = response.Create(false, ResponseMessages.INVALID_PASSWORD, HttpStatusCode.NotFound, result);
                        //}
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, AggregatoryMESSAGE.FAILED, HttpStatusCode.NotFound, result);

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
                response = response.Create(false, AggregatoryMESSAGE.FAILED, HttpStatusCode.NotFound, result);
                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError, true, false, Request);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }


        [AcceptVerbs("POST")]
        [Route("AddNewCardNo")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<DuplicateCardNoVMResponse>))]
        public async Task<IHttpActionResult> AddNewCardNo(RequestModel request)
        {
            var response = new Response<DuplicateCardNoVMResponse>();
            var result = new DuplicateCardNoVMResponse();

            var requestModel = new EncrDecr<DuplicateCardNoVMRequest>().Decrypt(request.Value, false, Request);
            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();

            if (ModelState.IsValid)
            {
                try
                {
                    if (request == null)
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError, true, false, Request);
                        return _iHttpActionResult;
                    }

                    result = await _cardPaymentService.AddNewCardNo(requestModel, sessionToken);

                    switch (result.RstKey)
                    {
                        case 1:
                            response = response.Create(true, result.Message, HttpStatusCode.OK, result);
                            break;
                        case 3:
                            response = response.Create(false, AggregatoryMESSAGE.FAILED, HttpStatusCode.NotFound, result);

                            break;
                        case 6:
                            response = response.Create(false, result.Message, HttpStatusCode.NotFound, result);
                            break;

                        case 11:
                            response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotFound, result);
                            break;
                        case 12:
                            response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotFound, result);
                            break;
                        case 13:
                            response = response.Create(false, ResponseMessageKyc.FAILED_Doc_NotUploaded, HttpStatusCode.NotFound, result);
                            break;
                        case 14:
                            response = response.Create(false, ResponseMessageKyc.FAILED_Doc_Pending, HttpStatusCode.NotFound, result);

                            break;
                        case 15:
                            response = response.Create(false, ResponseMessageKyc.Doc_Not_visible, HttpStatusCode.NotFound, result);
                            break;
                        case 16:
                            response = response.Create(false, ResponseMessageKyc.Doc_Rejected, HttpStatusCode.NotFound, result);

                            break;
                        case 21:
                            response = response.Create(false, ResponseMessages.MobileNotVerify, HttpStatusCode.OK, result);
                            break;
                        default:
                            response = response.Create(false, result.Message, HttpStatusCode.NotFound, result);
                            break;
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
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }



        [HttpPost]
        [Route("WalletSendOtp")]
        [SessionAuthorization]
        [ResponseType(typeof(Response<OtpResponse>))]
        [SessionTokenExceptionFilter]
        public async Task<IHttpActionResult> WalletSendOtp(RequestModel request)
        {
            var response = new Response<Object>();
            var result = new OtpResponse();
            var requestModel = new EncrDecr<OtpRequest>().Decrypt(request.Value, false, Request);

            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();

            if (ModelState.IsValid)
            {
                try
                {

                    result = await _cardPaymentService.WalletSendOtp(requestModel, sessionToken);
                    if (result.StatusCode == 1)
                    {
                        response = response.Create(true, ResponseMessages.OTP_SENT, HttpStatusCode.OK, result);
                    }
                    else if (result.StatusCode == 3)
                    {
                        response = response.Create(false, ResponseMessages.REQUEST_SENT_Not, HttpStatusCode.OK, result);

                    }
                    else if (result.StatusCode == 5)
                    {
                        response = response.Create(false, ResponseMessages.Verify_mobileno_SENT_RANGE, HttpStatusCode.OK, result);

                    }
                    else if (result.StatusCode == 6)
                    {
                        response = response.Create(false, ResponseMessages.OTP_Limit_OVER, HttpStatusCode.OK, result);

                    }
                    else if (result.StatusCode == 7)
                    {
                        response = response.Create(false, ResponseMessages.Verify_already_mobileno, HttpStatusCode.OK, result);

                    }

                    else
                    {
                        response = response.Create(false, ResponseMessages.OTP_Limit_OVER, HttpStatusCode.OK, result);
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

        [HttpPost]
        [Route("WalletVerifyOtp")]
        [SessionAuthorization]
        [ResponseType(typeof(Response<Object>))]
        [SessionTokenExceptionFilter]
        public async Task<IHttpActionResult> WalletVerifyOtp(RequestModel request)
        {
            var response = new Response<Object>();
            var result = new UserExistanceResponse();
            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();
            var requestModel = new EncrDecr<VerifyOtpRequest>().Decrypt(request.Value, false, Request);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _cardPaymentService.WalletVerifyOtp(requestModel, sessionToken);
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




        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<List<MobileNoListResponse>>))]
        [Route("GetMobileNoList")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        public async Task<IHttpActionResult> GetMobileNoList()
        {
            var response = new Response<List<MobileNoListResponse>>();
            var result = new List<MobileNoListResponse>();
            //var request = new EncrDecr<OtpRequest>().Decrypt(requestModel.Value, false, Request);
            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _cardPaymentService.GetMobileNoList(sessionToken);
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);

                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError, true, false, Request);
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

            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }


        /// <summary>
        /// GetCardPaymentUrlForNewFlowMasterCard
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetCardPaymentUrlForNewFlowMasterCard")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<MasterCardPaymentUBAResponse>))]
        public async Task<IHttpActionResult> GetCardPaymentUrlForNewFlowMasterCard(RequestModel request)
        {
            var response = new Response<MasterCardPaymentUBAResponse>();
            var result = new MasterCardPaymentUBAResponse();
            var requestModel = new EncrDecr<ThirdpartyPaymentByCardRequest>().Decrypt(request.Value, false, Request);

            var headerToken = Request.Headers.GetValues("token").FirstOrDefault();

            if (requestModel.Amount != null)
            {
                if (string.IsNullOrEmpty(requestModel.CardNo))
                {
                    response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
                    return _iHttpActionResult;
                }


                //checkin MaximumAmount 
                AdminMobileMoneyLimitRequest obj = new AdminMobileMoneyLimitRequest
                {
                    MinimumAmount = "0",
                    Service = "001"
                };
                var newCommisionMinChargesonGivenAmount = await _mobileMoneyServices.VerifyMobileMoneyLimit(obj);
                decimal d1 = decimal.Parse(newCommisionMinChargesonGivenAmount.MinimumAmount);
                decimal d3 = decimal.Parse(newCommisionMinChargesonGivenAmount.MaximumAmount);
                decimal d2 = decimal.Parse(requestModel.Amount);
                if (d2 == d1)
                {
                }
                else if (d2 < d1)
                {
                    response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
                    return _iHttpActionResult;
                }

                if (d2 == d3)
                {
                }
                else if (d2 > d3)
                {
                    response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
                    return _iHttpActionResult;
                }



                try
                {
                    if (request == null)
                    {
                        response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotAcceptable, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(requestModel.Amount) && !requestModel.Amount.IsZero() && requestModel.Amount.IsTwoDigitDecimal())
                        {
                            result = await _cardPaymentService.NewMasterCardPayment(requestModel, headerToken);
                            switch (result.RstKey)
                            {
                                case 0:
                                    response = response.Create(false, ResponseMessages.INVALID_PASSWORD, HttpStatusCode.NotFound, result);
                                    break;
                                case 1:
                                    response = response.Create(false, AggregatoryMESSAGE.PENDING, HttpStatusCode.OK, result);
                                    break;
                                case 2:
                                    response = response.Create(true, AggregatoryMESSAGE.SUCCESSFUL, HttpStatusCode.OK, result);
                                    break;
                                case 3:
                                    response = response.Create(false, AggregatoryMESSAGE.FAILED, HttpStatusCode.NotFound, result);

                                    break;
                                case 4:
                                    response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_ERROR, HttpStatusCode.NotFound, result);
                                    break;
                                case 5:
                                    response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_EXCEPTION, HttpStatusCode.NotFound, result);
                                    break;
                                case 6:
                                    response = response.Create(false, result.Message, HttpStatusCode.NotFound, result);
                                    break;
                                case 7:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_ERROR, HttpStatusCode.NotFound, result);

                                    break;
                                case 8:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.NotFound, result);

                                    break;
                                case 9:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.NotFound, result);

                                    break;
                                case 10:
                                    response = response.Create(false, ResponseMessages.INSUFICIENT_BALANCE, HttpStatusCode.NotFound, result);

                                    break;
                                case 11:
                                    response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotFound, result);

                                    break;
                                case 12:
                                    response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotFound, result);

                                    break;
                                case 13:
                                    response = response.Create(false, ResponseMessageKyc.FAILED_Doc_NotUploaded, HttpStatusCode.OK, result);

                                    break;
                                case 14:
                                    response = response.Create(false, ResponseMessageKyc.FAILED_Doc_Pending, HttpStatusCode.NotFound, result);

                                    break;
                                case 15:
                                    response = response.Create(false, ResponseMessageKyc.Doc_Not_visible, HttpStatusCode.NotFound, result);

                                    break;
                                case 16:
                                    response = response.Create(false, ResponseMessageKyc.Doc_Rejected, HttpStatusCode.NotFound, result);

                                    break;
                                case 17:
                                    response = response.Create(false, ResponseMessageKyc.TRANSACTION_DISABLED, HttpStatusCode.NotFound, result);

                                    break;
                                case 18:
                                    var errorList = new List<Errorkey>();
                                    if (string.IsNullOrWhiteSpace(requestModel.Amount))
                                    {
                                        errorList.Add(new Errorkey { Key = "Amount", Val = "Amount can not be null" });
                                    }
                                    if (string.IsNullOrWhiteSpace(requestModel.Password))
                                    {
                                        errorList.Add(new Errorkey { Key = "Password", Val = "Password is required" });
                                    }

                                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);

                                    break;
                                case 19:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.NotFound, result);

                                    break;
                                default:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.NotFound, result);

                                    break;
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(requestModel.Amount))
                            {
                                response = response.Create(false, ResponseMessages.EMPTY_AMOUNT, HttpStatusCode.NotFound, result);
                            }
                            else if (requestModel.Amount.IsZero())
                            {
                                response = response.Create(false, ResponseMessages.ZERO_AMOUNT, HttpStatusCode.NotFound, result);
                            }
                            else
                            {
                                response = response.Create(false, ResponseMessages.IMPROPER_AMOUNT, HttpStatusCode.NotFound, result);
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);
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
                response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);

            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("VerifyLimit")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<AdminMobileMoneyLimitResponse>))]
        public async Task<IHttpActionResult> VerifyLimit(RequestModel request)
        {
            var response = new Response<AdminMobileMoneyLimitResponse>();
            var result = new AdminMobileMoneyLimitResponse();
            var requestModel = new EncrDecr<AdminMobileMoneyLimitRequest>().Decrypt(request.Value, false, Request);
            if (requestModel.Service != null)
            {
                try
                {
                    result = await _mobileMoneyServices.VerifyMobileMoneyLimit(requestModel);
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);

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

                     
        /// <summary>
        /// GetCardPaymentUrlForNewFlowMasterCard2
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetCardPaymentUrlForNewFlowMasterCard2")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<MasterCardPaymentUBAResponse>))]
        public async Task<IHttpActionResult> GetCardPaymentUrlForNewFlowMasterCard2(RequestModel request)
        {
            var response = new Response<MasterCardPaymentUBAResponse>();
            var result = new MasterCardPaymentUBAResponse();
            var requestModel = new EncrDecr<ThirdpartyPaymentByCardRequest>().Decrypt(request.Value, false, Request);

            var headerToken = Request.Headers.GetValues("token").FirstOrDefault();

            if (requestModel.Amount != null)
            {
                if (string.IsNullOrEmpty(requestModel.CardNo))
                {
                    response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
                    return _iHttpActionResult;
                }


                //checkin MaximumAmount 
                AdminMobileMoneyLimitRequest obj = new AdminMobileMoneyLimitRequest
                {
                    MinimumAmount = "0",
                    Service = "001"
                };
                var newCommisionMinChargesonGivenAmount = await _mobileMoneyServices.VerifyMobileMoneyLimit(obj);
                decimal d1 = decimal.Parse(newCommisionMinChargesonGivenAmount.MinimumAmount);
                decimal d3 = decimal.Parse(newCommisionMinChargesonGivenAmount.MaximumAmount);
                decimal d2 = decimal.Parse(requestModel.Amount);
                if (d2 == d1)
                {
                }
                else if (d2 < d1)
                {
                    response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
                    return _iHttpActionResult;
                }

                if (d2 == d3)
                {
                }
                else if (d2 > d3)
                {
                    response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
                    return _iHttpActionResult;
                }



                try
                {
                    if (request == null)
                    {
                        response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotAcceptable, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(requestModel.Amount) && !requestModel.Amount.IsZero() && requestModel.Amount.IsTwoDigitDecimal())
                        {
                            result = await _cardPaymentService.NewMasterCardPayment2(requestModel, headerToken);
                            switch (result.RstKey)
                            {
                                case 0:
                                    response = response.Create(false, ResponseMessages.INVALID_PASSWORD, HttpStatusCode.NotFound, result);
                                    break;
                                case 1:
                                    response = response.Create(false, AggregatoryMESSAGE.PENDING, HttpStatusCode.OK, result);
                                    break;
                                case 2:
                                    response = response.Create(true, AggregatoryMESSAGE.SUCCESSFUL, HttpStatusCode.OK, result);
                                    break;
                                case 3:
                                    response = response.Create(false, AggregatoryMESSAGE.FAILED, HttpStatusCode.NotFound, result);

                                    break;
                                case 4:
                                    response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_ERROR, HttpStatusCode.NotFound, result);
                                    break;
                                case 5:
                                    response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_EXCEPTION, HttpStatusCode.NotFound, result);
                                    break;
                                case 6:
                                    response = response.Create(false, result.Message, HttpStatusCode.NotFound, result);
                                    break;
                                case 7:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_ERROR, HttpStatusCode.NotFound, result);

                                    break;
                                case 8:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.NotFound, result);

                                    break;
                                case 9:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.NotFound, result);

                                    break;
                                case 10:
                                    response = response.Create(false, ResponseMessages.INSUFICIENT_BALANCE, HttpStatusCode.NotFound, result);

                                    break;
                                case 11:
                                    response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotFound, result);

                                    break;
                                case 12:
                                    response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotFound, result);

                                    break;
                                case 13:
                                    response = response.Create(false, ResponseMessageKyc.FAILED_Doc_NotUploaded, HttpStatusCode.OK, result);

                                    break;
                                case 14:
                                    response = response.Create(false, ResponseMessageKyc.FAILED_Doc_Pending, HttpStatusCode.NotFound, result);

                                    break;
                                case 15:
                                    response = response.Create(false, ResponseMessageKyc.Doc_Not_visible, HttpStatusCode.NotFound, result);

                                    break;
                                case 16:
                                    response = response.Create(false, ResponseMessageKyc.Doc_Rejected, HttpStatusCode.NotFound, result);

                                    break;
                                case 17:
                                    response = response.Create(false, ResponseMessageKyc.TRANSACTION_DISABLED, HttpStatusCode.NotFound, result);

                                    break;
                                case 18:
                                    var errorList = new List<Errorkey>();
                                    if (string.IsNullOrWhiteSpace(requestModel.Amount))
                                    {
                                        errorList.Add(new Errorkey { Key = "Amount", Val = "Amount can not be null" });
                                    }
                                    if (string.IsNullOrWhiteSpace(requestModel.Password))
                                    {
                                        errorList.Add(new Errorkey { Key = "Password", Val = "Password is required" });
                                    }

                                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);

                                    break;
                                case 19:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.NotFound, result);

                                    break;
                                default:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.NotFound, result);

                                    break;
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(requestModel.Amount))
                            {
                                response = response.Create(false, ResponseMessages.EMPTY_AMOUNT, HttpStatusCode.NotFound, result);
                            }
                            else if (requestModel.Amount.IsZero())
                            {
                                response = response.Create(false, ResponseMessages.ZERO_AMOUNT, HttpStatusCode.NotFound, result);
                            }
                            else
                            {
                                response = response.Create(false, ResponseMessages.IMPROPER_AMOUNT, HttpStatusCode.NotFound, result);
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);
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
                response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);

            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }

        [HttpPost]
        [Route("GetCardPaymentUrlForflutterwave")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<flutterPaymentUrlResponse>))]
        public async Task<IHttpActionResult> GetCardPaymentUrlForflutterwave(RequestModel request)
        {
            var response = new Response<flutterPaymentUrlResponse>();
            var result = new flutterPaymentUrlResponse();
            var requestModel = new EncrDecr<ThirdpartyPaymentByCardRequest>().Decrypt(request.Value, false, Request);

            var headerToken = Request.Headers.GetValues("token").FirstOrDefault();

            if (requestModel.Amount != null)
            {
                if (string.IsNullOrEmpty(requestModel.CardNo))
                {
                    response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
                    return _iHttpActionResult;
                }


                //checkin MaximumAmount 
                AdminMobileMoneyLimitRequest obj = new AdminMobileMoneyLimitRequest
                {
                    MinimumAmount = "0",
                    Service = "001"
                };
                var newCommisionMinChargesonGivenAmount = await _mobileMoneyServices.VerifyMobileMoneyLimit(obj);
                decimal d1 = decimal.Parse(newCommisionMinChargesonGivenAmount.MinimumAmount);
                decimal d3 = decimal.Parse(newCommisionMinChargesonGivenAmount.MaximumAmount);
                decimal d2 = decimal.Parse(requestModel.Amount);
                if (d2 == d1)
                {
                }
                else if (d2 < d1)
                {
                    response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
                    return _iHttpActionResult;
                }

                if (d2 == d3)
                {
                }
                else if (d2 > d3)
                {
                    response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
                    return _iHttpActionResult;
                }



                try
                {
                    if (request == null)
                    {
                        response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotAcceptable, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
                    }

                   
                    else
                    {
                        if (!string.IsNullOrEmpty(requestModel.Amount) && !requestModel.Amount.IsZero() && requestModel.Amount.IsTwoDigitDecimal())
                        {
                            result = await _cardPaymentService.GetCardPaymentUrlForflutterwave(requestModel, headerToken);
                            switch (result.RstKey)
                            {
                                case 0:
                                    response = response.Create(false, ResponseMessages.INVALID_PASSWORD, HttpStatusCode.NotFound, result);
                                    break;
                                case 1:
                                    response = response.Create(false, AggregatoryMESSAGE.PENDING, HttpStatusCode.OK, result);
                                    break;
                                case 2:
                                    response = response.Create(true, AggregatoryMESSAGE.SUCCESSFUL, HttpStatusCode.OK, result);
                                    break;
                                case 3:
                                    response = response.Create(false, AggregatoryMESSAGE.FAILED, HttpStatusCode.NotFound, result);

                                    break;
                                case 4:
                                    response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_ERROR, HttpStatusCode.NotFound, result);
                                    break;
                                case 5:
                                    response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_EXCEPTION, HttpStatusCode.NotFound, result);
                                    break;
                                case 6:
                                    response = response.Create(false, result.Message, HttpStatusCode.NotFound, result);
                                    break;
                                case 7:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_ERROR, HttpStatusCode.NotFound, result);

                                    break;
                                case 8:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.NotFound, result);

                                    break;
                                case 9:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.NotFound, result);

                                    break;
                                case 10:
                                    response = response.Create(false, ResponseMessages.INSUFICIENT_BALANCE, HttpStatusCode.NotFound, result);

                                    break;
                                case 11:
                                    response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotFound, result);

                                    break;
                                case 12:
                                    response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotFound, result);

                                    break;
                                case 13:
                                    response = response.Create(false, ResponseMessageKyc.FAILED_Doc_NotUploaded, HttpStatusCode.OK, result);

                                    break;
                                case 14:
                                    response = response.Create(false, ResponseMessageKyc.FAILED_Doc_Pending, HttpStatusCode.NotFound, result);

                                    break;
                                case 15:
                                    response = response.Create(false, ResponseMessageKyc.Doc_Not_visible, HttpStatusCode.NotFound, result);

                                    break;
                                case 16:
                                    response = response.Create(false, ResponseMessageKyc.Doc_Rejected, HttpStatusCode.NotFound, result);

                                    break;
                                case 17:
                                    response = response.Create(false, ResponseMessageKyc.TRANSACTION_DISABLED, HttpStatusCode.NotFound, result);

                                    break;
                                case 18:
                                    var errorList = new List<Errorkey>();
                                    if (string.IsNullOrWhiteSpace(requestModel.Amount))
                                    {
                                        errorList.Add(new Errorkey { Key = "Amount", Val = "Amount can not be null" });
                                    }
                                    if (string.IsNullOrWhiteSpace(requestModel.Password))
                                    {
                                        errorList.Add(new Errorkey { Key = "Password", Val = "Password is required" });
                                    }

                                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);

                                    break;
                                case 19:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.NotFound, result);

                                    break;
                                default:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.NotFound, result);

                                    break;
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(requestModel.Amount))
                            {
                                response = response.Create(false, ResponseMessages.EMPTY_AMOUNT, HttpStatusCode.NotFound, result);
                            }
                            else if (requestModel.Amount.IsZero())
                            {
                                response = response.Create(false, ResponseMessages.ZERO_AMOUNT, HttpStatusCode.NotFound, result);
                            }
                            else
                            {
                                response = response.Create(false, ResponseMessages.IMPROPER_AMOUNT, HttpStatusCode.NotFound, result);
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);
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
                response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);

            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }


        //old addmone:-nigeria debit card
        //[HttpPost]
        //[Route("GetCardPaymentUrlForNGNbankflutter")]
        //[SessionAuthorization]
        //[SessionTokenExceptionFilter]
        //[ResponseType(typeof(Response<flutterbankResponse>))]
        //public async Task<IHttpActionResult> GetCardPaymentUrlForNGNbankflutter(RequestModel request)
        //{
        //    var response = new Response<flutterbankResponse>();
        //    var result = new flutterbankResponse();
        //    var requestModel = new EncrDecr<ThirdpartyPaymentByCardRequest>().Decrypt(request.Value, false, Request);

        //    var headerToken = Request.Headers.GetValues("token").FirstOrDefault();

        //    if (requestModel.Amount != null)
        //    {
        //        string str = requestModel.accountNo; //

        //        if (!Regex.IsMatch(str, "^[0-9]+$"))
        //        {
        //            response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
        //            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
        //            return _iHttpActionResult;
        //        }
        //        else if (string.IsNullOrEmpty(requestModel.ngnbank) && string.IsNullOrEmpty(requestModel.accountNo))
        //        {
        //            response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
        //            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
        //            return _iHttpActionResult;
        //        }
        //        if (requestModel.ngnbank == "057" && string.IsNullOrEmpty(requestModel.zenithdob))
        //        {
        //            response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
        //            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
        //            return _iHttpActionResult;
        //        }

        //        else if (requestModel.ngnbank == "033" && string.IsNullOrEmpty(requestModel.bvn)) //uba bank required paramter
        //        {
        //            response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
        //            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
        //            return _iHttpActionResult;
        //        }
        //        //checkin MaximumAmount 
        //        AdminMobileMoneyLimitRequest obj = new AdminMobileMoneyLimitRequest
        //        {
        //            MinimumAmount = requestModel.Amount.ToString(),
        //            Service = "002"
        //        };
        //        var newCommisionMinChargesonGivenAmount = await _mobileMoneyServices.VerifyMobileMoneyLimit(obj);
        //        decimal d1 = decimal.Parse(newCommisionMinChargesonGivenAmount.MinimumAmount);
        //        decimal d3 = decimal.Parse(newCommisionMinChargesonGivenAmount.MaximumAmount);
        //        decimal d2 = decimal.Parse(requestModel.Amount);
        //        if (d2 == d1)
        //        {
        //        }
        //        else if (d2 < d1)
        //        {
        //            response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
        //            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
        //            return _iHttpActionResult;
        //        }

        //        if (d2 == d3)
        //        {
        //        }
        //        else if (d2 > d3)
        //        {
        //            response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
        //            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
        //            return _iHttpActionResult;
        //        }



        //        try
        //        {
        //            if (request == null)
        //            {
        //                response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotAcceptable, result);
        //                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
        //            }
        //            else
        //            {
        //                if (!string.IsNullOrEmpty(requestModel.Amount) && !requestModel.Amount.IsZero() && requestModel.Amount.IsTwoDigitDecimal())
        //                {
        //                    result = await _cardPaymentService.GetCardPaymentUrlForNGNbankflutter(requestModel, headerToken);
        //                    switch (result.RstKey)
        //                    {
        //                        case 0:
        //                            response = response.Create(false, ResponseMessages.INVALID_PASSWORD, HttpStatusCode.NotFound, result);
        //                            break;
        //                        case 1:
        //                            response = response.Create(false, AggregatoryMESSAGE.PENDING, HttpStatusCode.OK, result);
        //                            break;
        //                        case 2:
        //                            response = response.Create(true, AggregatoryMESSAGE.SUCCESSFUL, HttpStatusCode.OK, result);
        //                            break;
        //                        case 3:
        //                            response = response.Create(false, AggregatoryMESSAGE.FAILED, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 4:
        //                            response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_ERROR, HttpStatusCode.NotFound, result);
        //                            break;
        //                        case 5:
        //                            response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_EXCEPTION, HttpStatusCode.NotFound, result);
        //                            break;
        //                        case 6:
        //                            response = response.Create(false, result.Message, HttpStatusCode.NotFound, result);
        //                            break;
        //                        case 7:
        //                            response = response.Create(false, ResponseMessages.TRANSACTION_ERROR, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 8:
        //                            response = response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 9:
        //                            response = response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 10:
        //                            response = response.Create(false, ResponseMessages.INSUFICIENT_BALANCE, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 11:
        //                            response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 12:
        //                            response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 13:
        //                            response = response.Create(false, ResponseMessageKyc.FAILED_Doc_NotUploaded, HttpStatusCode.OK, result);

        //                            break;
        //                        case 14:
        //                            response = response.Create(false, ResponseMessageKyc.FAILED_Doc_Pending, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 15:
        //                            response = response.Create(false, ResponseMessageKyc.Doc_Not_visible, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 16:
        //                            response = response.Create(false, ResponseMessageKyc.Doc_Rejected, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 17:
        //                            response = response.Create(false, ResponseMessageKyc.TRANSACTION_DISABLED, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 18:
        //                            var errorList = new List<Errorkey>();
        //                            if (string.IsNullOrWhiteSpace(requestModel.Amount))
        //                            {
        //                                errorList.Add(new Errorkey { Key = "Amount", Val = "Amount can not be null" });
        //                            }
        //                            if (string.IsNullOrWhiteSpace(requestModel.Password))
        //                            {
        //                                errorList.Add(new Errorkey { Key = "Password", Val = "Password is required" });
        //                            }

        //                            response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 19:
        //                            response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.NotFound, result);

        //                            break;
        //                        default:
        //                            response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.NotFound, result);

        //                            break;
        //                    }
        //                }
        //                else
        //                {
        //                    if (string.IsNullOrEmpty(requestModel.Amount))
        //                    {
        //                        response = response.Create(false, ResponseMessages.EMPTY_AMOUNT, HttpStatusCode.NotFound, result);
        //                    }
        //                    else if (requestModel.Amount.IsZero())
        //                    {
        //                        response = response.Create(false, ResponseMessages.ZERO_AMOUNT, HttpStatusCode.NotFound, result);
        //                    }
        //                    else
        //                    {
        //                        response = response.Create(false, ResponseMessages.IMPROPER_AMOUNT, HttpStatusCode.NotFound, result);
        //                    }
        //                }

        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);
        //            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
        //        }
        //    }
        //    else
        //    {
        //        var errorList = new List<Errorkey>();
        //        foreach (var mod in ModelState)
        //        {
        //            Errorkey objkey = new Errorkey();
        //            objkey.Key = mod.Key;
        //            if (mod.Value.Errors.Count > 0)
        //            {
        //                objkey.Val = mod.Value.Errors[0].ErrorMessage;
        //            }
        //            errorList.Add(objkey);
        //        }
        //        response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);

        //    }
        //    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
        //    return _iHttpActionResult;
        //}



        //[HttpPost]
        //[Route("GetZenithBankUrlByOTP")]
        //[SessionAuthorization]
        //[SessionTokenExceptionFilter]
        //[ResponseType(typeof(Response<flutterbankResponse>))]
        //public async Task<IHttpActionResult> GetZenithBankUrlByOTP(RequestModel request)
        //{
        //    var response = new Response<flutterbankResponse>();
        //    var result = new flutterbankResponse();
        //    var requestModel = new EncrDecr<ZenithBankOTPRequest>().Decrypt(request.Value, false, Request);

        //    var headerToken = Request.Headers.GetValues("token").FirstOrDefault();

        //    if (requestModel.otp != null)
        //    {
        //        string str = requestModel.otp; //

        //        if (!Regex.IsMatch(str, "^[0-9]+$") && requestModel.flw_ref != null)
        //        {
        //            response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
        //            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
        //            return _iHttpActionResult;
        //        }

        //        try
        //        {
        //            if (request == null)
        //            {
        //                response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotAcceptable, result);
        //                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
        //            }
        //            else
        //            {
        //                result = await _cardPaymentService.GetZenithBankUrlByOTP(requestModel, headerToken);
        //                switch (result.RstKey)
        //                {
        //                    case 0:
        //                        response = response.Create(false, ResponseMessages.INVALID_PASSWORD, HttpStatusCode.NotFound, result);
        //                        break;
        //                    case 1:
        //                        response = response.Create(false, AggregatoryMESSAGE.PENDING, HttpStatusCode.OK, result);
        //                        break;
        //                    case 2:
        //                        response = response.Create(true, AggregatoryMESSAGE.SUCCESSFUL, HttpStatusCode.OK, result);
        //                        break;
        //                    case 3:
        //                        response = response.Create(false, AggregatoryMESSAGE.FAILED, HttpStatusCode.NotFound, result);


        //                        break;
        //                    default:
        //                        response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.NotFound, result);

        //                        break;
        //                }



        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);
        //            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
        //        }
        //    }
        //    else
        //    {
        //        var errorList = new List<Errorkey>();
        //        foreach (var mod in ModelState)
        //        {
        //            Errorkey objkey = new Errorkey();
        //            objkey.Key = mod.Key;
        //            if (mod.Value.Errors.Count > 0)
        //            {
        //                objkey.Val = mod.Value.Errors[0].ErrorMessage;
        //            }
        //            errorList.Add(objkey);
        //        }
        //        response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);

        //    }
        //    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
        //    return _iHttpActionResult;
        //}


        [HttpPost]
        [Route("GetCardPaymentUrlForNGNbankflutterUSD1")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<flutterPaymentUrlResponse>))]
        public async Task<IHttpActionResult> GetCardPaymentUrlForNGNbankflutterUSD(RequestModel request)
        {
            var response = new Response<flutterPaymentUrlResponse>();
            var result = new flutterPaymentUrlResponse();
            var requestModel = new EncrDecr<ThirdpartyPaymentByCardRequest>().Decrypt(request.Value, false, Request);

            var headerToken = Request.Headers.GetValues("token").FirstOrDefault();

            if (requestModel.Amount != null)
            {
                if (string.IsNullOrEmpty(requestModel.CardNo))
                {
                    response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
                    return _iHttpActionResult;
                }


                //checkin MaximumAmount 
                AdminMobileMoneyLimitRequest obj = new AdminMobileMoneyLimitRequest
                {
                    MinimumAmount = "0",
                    Service = "001"
                };
                var newCommisionMinChargesonGivenAmount = await _mobileMoneyServices.VerifyMobileMoneyLimit(obj);
                decimal d1 = decimal.Parse(newCommisionMinChargesonGivenAmount.MinimumAmount);
                decimal d3 = decimal.Parse(newCommisionMinChargesonGivenAmount.MaximumAmount);
                decimal d2 = decimal.Parse(requestModel.Amount);
                if (d2 == d1)
                {
                }
                else if (d2 < d1)
                {
                    response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
                    return _iHttpActionResult;
                }

                if (d2 == d3)
                {
                }
                else if (d2 > d3)
                {
                    response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
                    return _iHttpActionResult;
                }



                try
                {
                    if (request == null)
                    {
                        response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotAcceptable, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(requestModel.Amount) && !requestModel.Amount.IsZero() && requestModel.Amount.IsTwoDigitDecimal())
                        {
                            result = await _cardPaymentService.GetCardPaymentUrlForNGNbankflutterUSD(requestModel, headerToken);
                            switch (result.RstKey)
                            {
                                case 0:
                                    response = response.Create(false, ResponseMessages.INVALID_PASSWORD, HttpStatusCode.NotFound, result);
                                    break;
                                case 1:
                                    response = response.Create(false, AggregatoryMESSAGE.PENDING, HttpStatusCode.OK, result);
                                    break;
                                case 2:
                                    response = response.Create(true, AggregatoryMESSAGE.SUCCESSFUL, HttpStatusCode.OK, result);
                                    break;
                                case 3:
                                    response = response.Create(false, AggregatoryMESSAGE.FAILED, HttpStatusCode.NotFound, result);

                                    break;
                                case 4:
                                    response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_ERROR, HttpStatusCode.NotFound, result);
                                    break;
                                case 5:
                                    response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_EXCEPTION, HttpStatusCode.NotFound, result);
                                    break;
                                case 6:
                                    response = response.Create(false, result.Message, HttpStatusCode.NotFound, result);
                                    break;
                                case 7:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_ERROR, HttpStatusCode.NotFound, result);

                                    break;
                                case 8:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.NotFound, result);

                                    break;
                                case 9:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.NotFound, result);

                                    break;
                                case 10:
                                    response = response.Create(false, ResponseMessages.INSUFICIENT_BALANCE, HttpStatusCode.NotFound, result);

                                    break;
                                case 11:
                                    response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotFound, result);

                                    break;
                                case 12:
                                    response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotFound, result);

                                    break;
                                case 13:
                                    response = response.Create(false, ResponseMessageKyc.FAILED_Doc_NotUploaded, HttpStatusCode.OK, result);

                                    break;
                                case 14:
                                    response = response.Create(false, ResponseMessageKyc.FAILED_Doc_Pending, HttpStatusCode.NotFound, result);

                                    break;
                                case 15:
                                    response = response.Create(false, ResponseMessageKyc.Doc_Not_visible, HttpStatusCode.NotFound, result);

                                    break;
                                case 16:
                                    response = response.Create(false, ResponseMessageKyc.Doc_Rejected, HttpStatusCode.NotFound, result);

                                    break;
                                case 17:
                                    response = response.Create(false, ResponseMessageKyc.TRANSACTION_DISABLED, HttpStatusCode.NotFound, result);

                                    break;
                                case 18:
                                    var errorList = new List<Errorkey>();
                                    if (string.IsNullOrWhiteSpace(requestModel.Amount))
                                    {
                                        errorList.Add(new Errorkey { Key = "Amount", Val = "Amount can not be null" });
                                    }
                                    if (string.IsNullOrWhiteSpace(requestModel.Password))
                                    {
                                        errorList.Add(new Errorkey { Key = "Password", Val = "Password is required" });
                                    }

                                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);

                                    break;
                                case 19:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.NotFound, result);

                                    break;
                                default:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.NotFound, result);

                                    break;
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(requestModel.Amount))
                            {
                                response = response.Create(false, ResponseMessages.EMPTY_AMOUNT, HttpStatusCode.NotFound, result);
                            }
                            else if (requestModel.Amount.IsZero())
                            {
                                response = response.Create(false, ResponseMessages.ZERO_AMOUNT, HttpStatusCode.NotFound, result);
                            }
                            else
                            {
                                response = response.Create(false, ResponseMessages.IMPROPER_AMOUNT, HttpStatusCode.NotFound, result);
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);
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
                response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);

            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }


        [HttpPost]
        [Route("GetCardPaymentUrlForNGNbankflutterEuro1")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<flutterPaymentUrlResponse>))]
        public async Task<IHttpActionResult> GetCardPaymentUrlForNGNbankflutterEuro(RequestModel request)
        {
            var response = new Response<flutterPaymentUrlResponse>();
            var result = new flutterPaymentUrlResponse();
            var requestModel = new EncrDecr<ThirdpartyPaymentByCardRequest>().Decrypt(request.Value, false, Request);

            var headerToken = Request.Headers.GetValues("token").FirstOrDefault();

            if (requestModel.Amount != null)
            {
                if (string.IsNullOrEmpty(requestModel.CardNo))
                {
                    response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
                    return _iHttpActionResult;
                }


                //checkin MaximumAmount 
                AdminMobileMoneyLimitRequest obj = new AdminMobileMoneyLimitRequest
                {
                    MinimumAmount = "0",
                    Service = "001"
                };
                var newCommisionMinChargesonGivenAmount = await _mobileMoneyServices.VerifyMobileMoneyLimit(obj);
                decimal d1 = decimal.Parse(newCommisionMinChargesonGivenAmount.MinimumAmount);
                decimal d3 = decimal.Parse(newCommisionMinChargesonGivenAmount.MaximumAmount);
                decimal d2 = decimal.Parse(requestModel.Amount);
                if (d2 == d1)
                {
                }
                else if (d2 < d1)
                {
                    response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
                    return _iHttpActionResult;
                }

                if (d2 == d3)
                {
                }
                else if (d2 > d3)
                {
                    response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
                    return _iHttpActionResult;
                }



                try
                {
                    if (request == null)
                    {
                        response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotAcceptable, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(requestModel.Amount) && !requestModel.Amount.IsZero() && requestModel.Amount.IsTwoDigitDecimal())
                        {
                            result = await _cardPaymentService.GetCardPaymentUrlForNGNbankflutterEuro(requestModel, headerToken);
                            switch (result.RstKey)
                            {
                                case 0:
                                    response = response.Create(false, ResponseMessages.INVALID_PASSWORD, HttpStatusCode.NotFound, result);
                                    break;
                                case 1:
                                    response = response.Create(false, AggregatoryMESSAGE.PENDING, HttpStatusCode.OK, result);
                                    break;
                                case 2:
                                    response = response.Create(true, AggregatoryMESSAGE.SUCCESSFUL, HttpStatusCode.OK, result);
                                    break;
                                case 3:
                                    response = response.Create(false, AggregatoryMESSAGE.FAILED, HttpStatusCode.NotFound, result);

                                    break;
                                case 4:
                                    response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_ERROR, HttpStatusCode.NotFound, result);
                                    break;
                                case 5:
                                    response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_EXCEPTION, HttpStatusCode.NotFound, result);
                                    break;
                                case 6:
                                    response = response.Create(false, result.Message, HttpStatusCode.NotFound, result);
                                    break;
                                case 7:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_ERROR, HttpStatusCode.NotFound, result);

                                    break;
                                case 8:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.NotFound, result);

                                    break;
                                case 9:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.NotFound, result);

                                    break;
                                case 10:
                                    response = response.Create(false, ResponseMessages.INSUFICIENT_BALANCE, HttpStatusCode.NotFound, result);

                                    break;
                                case 11:
                                    response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotFound, result);

                                    break;
                                case 12:
                                    response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotFound, result);

                                    break;
                                case 13:
                                    response = response.Create(false, ResponseMessageKyc.FAILED_Doc_NotUploaded, HttpStatusCode.OK, result);

                                    break;
                                case 14:
                                    response = response.Create(false, ResponseMessageKyc.FAILED_Doc_Pending, HttpStatusCode.NotFound, result);

                                    break;
                                case 15:
                                    response = response.Create(false, ResponseMessageKyc.Doc_Not_visible, HttpStatusCode.NotFound, result);

                                    break;
                                case 16:
                                    response = response.Create(false, ResponseMessageKyc.Doc_Rejected, HttpStatusCode.NotFound, result);

                                    break;
                                case 17:
                                    response = response.Create(false, ResponseMessageKyc.TRANSACTION_DISABLED, HttpStatusCode.NotFound, result);

                                    break;
                                case 18:
                                    var errorList = new List<Errorkey>();
                                    if (string.IsNullOrWhiteSpace(requestModel.Amount))
                                    {
                                        errorList.Add(new Errorkey { Key = "Amount", Val = "Amount can not be null" });
                                    }
                                    if (string.IsNullOrWhiteSpace(requestModel.Password))
                                    {
                                        errorList.Add(new Errorkey { Key = "Password", Val = "Password is required" });
                                    }

                                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);

                                    break;
                                case 19:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.NotFound, result);

                                    break;
                                default:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.NotFound, result);

                                    break;
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(requestModel.Amount))
                            {
                                response = response.Create(false, ResponseMessages.EMPTY_AMOUNT, HttpStatusCode.NotFound, result);
                            }
                            else if (requestModel.Amount.IsZero())
                            {
                                response = response.Create(false, ResponseMessages.ZERO_AMOUNT, HttpStatusCode.NotFound, result);
                            }
                            else
                            {
                                response = response.Create(false, ResponseMessages.IMPROPER_AMOUNT, HttpStatusCode.NotFound, result);
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);
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
                response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);

            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }


        //ne addmone:-nigeria debit card
        [HttpPost]
        [Route("GetCardPaymentUrlForNGNbanktransferflutter")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<flutterbanktransferauthorization>))]
        public async Task<IHttpActionResult> GetCardPaymentUrlForNGNbanktransferflutter(RequestModel request)
        {
            var response = new Response<flutterbanktransferauthorization>();
            var result = new flutterbanktransferauthorization();
            var requestModel = new EncrDecr<ThirdpartyPaymentByCardRequest>().Decrypt(request.Value, false, Request);

            var headerToken = Request.Headers.GetValues("token").FirstOrDefault();

            if (requestModel.Amount != null)
            {

                //checkin MaximumAmount 
                AdminMobileMoneyLimitRequest obj = new AdminMobileMoneyLimitRequest
                {
                    MinimumAmount = "0",
                    Service = "002"
                };
                var newCommisionMinChargesonGivenAmount = await _mobileMoneyServices.VerifyMobileMoneyLimit(obj);
                decimal d1 = decimal.Parse(newCommisionMinChargesonGivenAmount.MinimumAmount);
                decimal d3 = decimal.Parse(newCommisionMinChargesonGivenAmount.MaximumAmount);
                decimal d2 = decimal.Parse(requestModel.Amount);
                if (d2 == d1)
                {
                }
                else if (d2 < d1)
                {
                    response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
                    return _iHttpActionResult;
                }

                if (d2 == d3)
                {
                }
                else if (d2 > d3)
                {
                    response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
                    return _iHttpActionResult;
                }



                try
                {
                    if (request == null)
                    {
                        response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotAcceptable, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(requestModel.Amount) && !requestModel.Amount.IsZero() && requestModel.Amount.IsTwoDigitDecimal())
                        {
                            result = await _cardPaymentService.GetCardPaymentUrlForNGNbanktransferflutter(requestModel, headerToken);
                            switch (result.RstKey)
                            {
                                case 0:
                                    response = response.Create(false, ResponseMessages.INVALID_PASSWORD, HttpStatusCode.NotFound, result);
                                    break;
                                case 1:
                                    response = response.Create(false, AggregatoryMESSAGE.PENDING, HttpStatusCode.OK, result);
                                    break;
                                case 2:
                                    response = response.Create(true, AggregatoryMESSAGE.SUCCESSFUL, HttpStatusCode.OK, result);
                                    break;
                                case 3:
                                    response = response.Create(false, AggregatoryMESSAGE.FAILED, HttpStatusCode.NotFound, result);

                                    break;
                                case 4:
                                    response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_ERROR, HttpStatusCode.NotFound, result);
                                    break;
                                case 5:
                                    response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_EXCEPTION, HttpStatusCode.NotFound, result);
                                    break;
                                case 6:
                                    response = response.Create(false, result.Message, HttpStatusCode.NotFound, result);
                                    break;
                                case 7:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_ERROR, HttpStatusCode.NotFound, result);

                                    break;
                                case 8:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.NotFound, result);

                                    break;
                                case 9:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.NotFound, result);

                                    break;
                                case 10:
                                    response = response.Create(false, ResponseMessages.INSUFICIENT_BALANCE, HttpStatusCode.NotFound, result);

                                    break;
                                case 11:
                                    response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotFound, result);

                                    break;
                                case 12:
                                    response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotFound, result);

                                    break;
                                case 13:
                                    response = response.Create(false, ResponseMessageKyc.FAILED_Doc_NotUploaded, HttpStatusCode.OK, result);

                                    break;
                                case 14:
                                    response = response.Create(false, ResponseMessageKyc.FAILED_Doc_Pending, HttpStatusCode.NotFound, result);

                                    break;
                                case 15:
                                    response = response.Create(false, ResponseMessageKyc.Doc_Not_visible, HttpStatusCode.NotFound, result);

                                    break;
                                case 16:
                                    response = response.Create(false, ResponseMessageKyc.Doc_Rejected, HttpStatusCode.NotFound, result);

                                    break;
                                case 17:
                                    response = response.Create(false, ResponseMessageKyc.TRANSACTION_DISABLED, HttpStatusCode.NotFound, result);

                                    break;
                                case 18:
                                    var errorList = new List<Errorkey>();
                                    if (string.IsNullOrWhiteSpace(requestModel.Amount))
                                    {
                                        errorList.Add(new Errorkey { Key = "Amount", Val = "Amount can not be null" });
                                    }
                                    if (string.IsNullOrWhiteSpace(requestModel.Password))
                                    {
                                        errorList.Add(new Errorkey { Key = "Password", Val = "Password is required" });
                                    }

                                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);

                                    break;
                                case 19:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.NotFound, result);

                                    break;
                                default:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.NotFound, result);

                                    break;
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(requestModel.Amount))
                            {
                                response = response.Create(false, ResponseMessages.EMPTY_AMOUNT, HttpStatusCode.NotFound, result);
                            }
                            else if (requestModel.Amount.IsZero())
                            {
                                response = response.Create(false, ResponseMessages.ZERO_AMOUNT, HttpStatusCode.NotFound, result);
                            }
                            else
                            {
                                response = response.Create(false, ResponseMessages.IMPROPER_AMOUNT, HttpStatusCode.NotFound, result);
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);
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
                response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);

            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }



        /// <summary>
        /// GetngeniusCardPaymentUrl
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetngeniusCardPaymentUrl")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<NgeniunsResponse>))]
        public async Task<IHttpActionResult> GetngeniusCardPaymentUrl(RequestModel request)
        {
            var response = new Response<NgeniunsResponse>();
            var result = new NgeniunsResponse();
            var requestModel = new EncrDecr<ThirdpartyPaymentByCardRequest>().Decrypt(request.Value, false, Request);

            var headerToken = Request.Headers.GetValues("token").FirstOrDefault();

            if (requestModel.Amount != null)
            {
                if (string.IsNullOrEmpty(requestModel.CardNo))
                {
                    response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
                    return _iHttpActionResult;
                }


                //checkin MaximumAmount 
                AdminMobileMoneyLimitRequest obj = new AdminMobileMoneyLimitRequest
                {
                    MinimumAmount = "0",
                    Service = "001"
                };
                var newCommisionMinChargesonGivenAmount = await _mobileMoneyServices.VerifyMobileMoneyLimit(obj);
                decimal d1 = decimal.Parse(newCommisionMinChargesonGivenAmount.MinimumAmount);
                decimal d3 = decimal.Parse(newCommisionMinChargesonGivenAmount.MaximumAmount);
                decimal d2 = decimal.Parse(requestModel.Amount);
                if (d2 == d1)
                {
                }
                else if (d2 < d1)
                {
                    response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
                    return _iHttpActionResult;
                }

                if (d2 == d3)
                {
                }
                else if (d2 > d3)
                {
                    response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
                    return _iHttpActionResult;
                }



                try
                {
                    if (request == null)
                    {
                        response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotAcceptable, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(requestModel.Amount) && !requestModel.Amount.IsZero() && requestModel.Amount.IsTwoDigitDecimal())
                        {
                            result = await _cardPaymentService.GetngeniusCardPaymentUrl(requestModel, headerToken);
                            switch (result.RstKey)
                            {
                                case 0:
                                    response = response.Create(false, ResponseMessages.INVALID_PASSWORD, HttpStatusCode.NotFound, result);
                                    break;
                                case 1:
                                    response = response.Create(false, AggregatoryMESSAGE.PENDING, HttpStatusCode.OK, result);
                                    break;
                                case 2:
                                    response = response.Create(true, AggregatoryMESSAGE.SUCCESSFUL, HttpStatusCode.OK, result);
                                    break;
                                case 3:
                                    response = response.Create(false, AggregatoryMESSAGE.FAILED, HttpStatusCode.NotFound, result);

                                    break;
                                case 4:
                                    response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_ERROR, HttpStatusCode.NotFound, result);
                                    break;
                                case 5:
                                    response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_EXCEPTION, HttpStatusCode.NotFound, result);
                                    break;
                                case 6:
                                    response = response.Create(false, result.message, HttpStatusCode.NotFound, result);
                                    break;
                                case 7:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_ERROR, HttpStatusCode.NotFound, result);

                                    break;
                                case 8:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.NotFound, result);

                                    break;
                                case 9:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.NotFound, result);

                                    break;
                                case 10:
                                    response = response.Create(false, ResponseMessages.INSUFICIENT_BALANCE, HttpStatusCode.NotFound, result);

                                    break;
                                case 11:
                                    response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotFound, result);

                                    break;
                                case 12:
                                    response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotFound, result);

                                    break;
                                case 13:
                                    response = response.Create(false, ResponseMessageKyc.FAILED_Doc_NotUploaded, HttpStatusCode.OK, result);

                                    break;
                                case 14:
                                    response = response.Create(false, ResponseMessageKyc.FAILED_Doc_Pending, HttpStatusCode.NotFound, result);

                                    break;
                                case 15:
                                    response = response.Create(false, ResponseMessageKyc.Doc_Not_visible, HttpStatusCode.NotFound, result);

                                    break;
                                case 16:
                                    response = response.Create(false, ResponseMessageKyc.Doc_Rejected, HttpStatusCode.NotFound, result);

                                    break;
                                case 17:
                                    response = response.Create(false, ResponseMessageKyc.TRANSACTION_DISABLED, HttpStatusCode.NotFound, result);

                                    break;
                                case 18:
                                    var errorList = new List<Errorkey>();
                                    if (string.IsNullOrWhiteSpace(requestModel.Amount))
                                    {
                                        errorList.Add(new Errorkey { Key = "Amount", Val = "Amount can not be null" });
                                    }
                                    if (string.IsNullOrWhiteSpace(requestModel.Password))
                                    {
                                        errorList.Add(new Errorkey { Key = "Password", Val = "Password is required" });
                                    }

                                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);

                                    break;
                                case 19:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.NotFound, result);

                                    break;
                                default:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.NotFound, result);

                                    break;
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(requestModel.Amount))
                            {
                                response = response.Create(false, ResponseMessages.EMPTY_AMOUNT, HttpStatusCode.NotFound, result);
                            }
                            else if (requestModel.Amount.IsZero())
                            {
                                response = response.Create(false, ResponseMessages.ZERO_AMOUNT, HttpStatusCode.NotFound, result);
                            }
                            else
                            {
                                response = response.Create(false, ResponseMessages.IMPROPER_AMOUNT, HttpStatusCode.NotFound, result);
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);
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
                response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);

            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }


        //[HttpPost]
        //[Route("GetCardPaymentUrlForbinance")]
        //[SessionAuthorization]
        //[SessionTokenExceptionFilter]
        //[ResponseType(typeof(Response<binancePaymentUrlResponse>))]
        //public async Task<IHttpActionResult> GetCardPaymentUrlForbinance(RequestModel request)
        //{
        //    var response = new Response<binancePaymentUrlResponse>();
        //    var result = new binancePaymentUrlResponse();
        //    var requestModel = new EncrDecr<ThirdpartyPaymentByCardRequest>().Decrypt(request.Value, false, Request);

        //    var headerToken = Request.Headers.GetValues("token").FirstOrDefault();

        //    if (requestModel.Amount != null)
        //    {
        //        if (string.IsNullOrEmpty(requestModel.CardNo))
        //        {
        //            response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
        //            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
        //            return _iHttpActionResult;
        //        }


        //        //checkin MaximumAmount 
        //        AdminMobileMoneyLimitRequest obj = new AdminMobileMoneyLimitRequest
        //        {
        //            MinimumAmount = "0",
        //            Service = "001"
        //        };
        //        var newCommisionMinChargesonGivenAmount = await _mobileMoneyServices.VerifyMobileMoneyLimit(obj);
        //        decimal d1 = decimal.Parse(newCommisionMinChargesonGivenAmount.MinimumAmount);
        //        decimal d3 = decimal.Parse(newCommisionMinChargesonGivenAmount.MaximumAmount);
        //        decimal d2 = decimal.Parse(requestModel.Amount);
        //        if (d2 == d1)
        //        {
        //        }
        //        else if (d2 < d1)
        //        {
        //            response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
        //            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
        //            return _iHttpActionResult;
        //        }

        //        if (d2 == d3)
        //        {
        //        }
        //        else if (d2 > d3)
        //        {
        //            response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
        //            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
        //            return _iHttpActionResult;
        //        }



        //        try
        //        {
        //            if (request == null)
        //            {
        //                response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotAcceptable, result);
        //                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
        //            }
        //            else
        //            {
        //                if (!string.IsNullOrEmpty(requestModel.Amount) && !requestModel.Amount.IsZero() && requestModel.Amount.IsTwoDigitDecimal())
        //                {
        //                    result = await _cardPaymentService.GetCardPaymentUrlForbinance(requestModel, headerToken);
        //                    switch (result.RstKey)
        //                    {
        //                        case 0:
        //                            response = response.Create(false, ResponseMessages.INVALID_PASSWORD, HttpStatusCode.NotFound, result);
        //                            break;
        //                        case 1:
        //                            response = response.Create(false, AggregatoryMESSAGE.PENDING, HttpStatusCode.OK, result);
        //                            break;
        //                        case 2:
        //                            response = response.Create(true, AggregatoryMESSAGE.SUCCESSFUL, HttpStatusCode.OK, result);
        //                            break;
        //                        case 3:
        //                            response = response.Create(false, AggregatoryMESSAGE.FAILED, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 4:
        //                            response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_ERROR, HttpStatusCode.NotFound, result);
        //                            break;
        //                        case 5:
        //                            response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_EXCEPTION, HttpStatusCode.NotFound, result);
        //                            break;
        //                        case 6:
        //                            response = response.Create(false, result.Message, HttpStatusCode.NotFound, result);
        //                            break;
        //                        case 7:
        //                            response = response.Create(false, ResponseMessages.TRANSACTION_ERROR, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 8:
        //                            response = response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 9:
        //                            response = response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 10:
        //                            response = response.Create(false, ResponseMessages.INSUFICIENT_BALANCE, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 11:
        //                            response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 12:
        //                            response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 13:
        //                            response = response.Create(false, ResponseMessageKyc.FAILED_Doc_NotUploaded, HttpStatusCode.OK, result);

        //                            break;
        //                        case 14:
        //                            response = response.Create(false, ResponseMessageKyc.FAILED_Doc_Pending, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 15:
        //                            response = response.Create(false, ResponseMessageKyc.Doc_Not_visible, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 16:
        //                            response = response.Create(false, ResponseMessageKyc.Doc_Rejected, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 17:
        //                            response = response.Create(false, ResponseMessageKyc.TRANSACTION_DISABLED, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 18:
        //                            var errorList = new List<Errorkey>();
        //                            if (string.IsNullOrWhiteSpace(requestModel.Amount))
        //                            {
        //                                errorList.Add(new Errorkey { Key = "Amount", Val = "Amount can not be null" });
        //                            }
        //                            if (string.IsNullOrWhiteSpace(requestModel.Password))
        //                            {
        //                                errorList.Add(new Errorkey { Key = "Password", Val = "Password is required" });
        //                            }

        //                            response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 19:
        //                            response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.NotFound, result);

        //                            break;
        //                        default:
        //                            response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.NotFound, result);

        //                            break;
        //                    }
        //                }
        //                else
        //                {
        //                    if (string.IsNullOrEmpty(requestModel.Amount))
        //                    {
        //                        response = response.Create(false, ResponseMessages.EMPTY_AMOUNT, HttpStatusCode.NotFound, result);
        //                    }
        //                    else if (requestModel.Amount.IsZero())
        //                    {
        //                        response = response.Create(false, ResponseMessages.ZERO_AMOUNT, HttpStatusCode.NotFound, result);
        //                    }
        //                    else
        //                    {
        //                        response = response.Create(false, ResponseMessages.IMPROPER_AMOUNT, HttpStatusCode.NotFound, result);
        //                    }
        //                }

        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);
        //            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
        //        }
        //    }
        //    else
        //    {
        //        var errorList = new List<Errorkey>();
        //        foreach (var mod in ModelState)
        //        {
        //            Errorkey objkey = new Errorkey();
        //            objkey.Key = mod.Key;
        //            if (mod.Value.Errors.Count > 0)
        //            {
        //                objkey.Val = mod.Value.Errors[0].ErrorMessage;
        //            }
        //            errorList.Add(objkey);
        //        }
        //        response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);

        //    }
        //    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
        //    return _iHttpActionResult;
        //}

        //[HttpPost]
        //[Route("GetCardPaymentUrlForbinancewallet")]
        //[SessionAuthorization]
        //[SessionTokenExceptionFilter]
        //[ResponseType(typeof(Response<binancewalletResponse>))]
        //public async Task<IHttpActionResult> GetCardPaymentUrlForbinancewallet(RequestModel request)
        //{
        //    var response = new Response<binancewalletResponse>();
        //    var result = new binancewalletResponse();
        //    var requestModel = new EncrDecr<ThirdpartyPaymentByCardRequest>().Decrypt(request.Value, false, Request);

        //    var headerToken = Request.Headers.GetValues("token").FirstOrDefault();

        //    if (requestModel.Amount != null)
        //    {
        //        if (string.IsNullOrEmpty(requestModel.CardNo))
        //        {
        //            response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
        //            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
        //            return _iHttpActionResult;
        //        }


        //        //checkin MaximumAmount 
        //        AdminMobileMoneyLimitRequest obj = new AdminMobileMoneyLimitRequest
        //        {
        //            MinimumAmount = "0",
        //            Service = "001"
        //        };
        //        var newCommisionMinChargesonGivenAmount = await _mobileMoneyServices.VerifyMobileMoneyLimit(obj);
        //        decimal d1 = decimal.Parse(newCommisionMinChargesonGivenAmount.MinimumAmount);
        //        decimal d3 = decimal.Parse(newCommisionMinChargesonGivenAmount.MaximumAmount);
        //        decimal d2 = decimal.Parse(requestModel.Amount);
        //        if (d2 == d1)
        //        {
        //        }
        //        else if (d2 < d1)
        //        {
        //            response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
        //            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
        //            return _iHttpActionResult;
        //        }

        //        if (d2 == d3)
        //        {
        //        }
        //        else if (d2 > d3)
        //        {
        //            response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
        //            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
        //            return _iHttpActionResult;
        //        }



        //        try
        //        {
        //            if (request == null)
        //            {
        //                response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotAcceptable, result);
        //                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
        //            }
        //            else
        //            {
        //                if (!string.IsNullOrEmpty(requestModel.Amount) && !requestModel.Amount.IsZero() && requestModel.Amount.IsTwoDigitDecimal())
        //                {
        //                    result = await _cardPaymentService.GetCardPaymentUrlForbinancewallet(requestModel, headerToken);
        //                    switch (result.RstKey)
        //                    {
        //                        case 0:
        //                            response = response.Create(false, ResponseMessages.INVALID_PASSWORD, HttpStatusCode.NotFound, result);
        //                            break;
        //                        case 1:
        //                            response = response.Create(false, AggregatoryMESSAGE.PENDING, HttpStatusCode.OK, result);
        //                            break;
        //                        case 2:
        //                            response = response.Create(true, AggregatoryMESSAGE.SUCCESSFUL, HttpStatusCode.OK, result);
        //                            break;
        //                        case 3:
        //                            response = response.Create(false, AggregatoryMESSAGE.FAILED, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 4:
        //                            response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_ERROR, HttpStatusCode.NotFound, result);
        //                            break;
        //                        case 5:
        //                            response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_EXCEPTION, HttpStatusCode.NotFound, result);
        //                            break;
        //                        case 6:
        //                            response = response.Create(false, result.Message, HttpStatusCode.NotFound, result);
        //                            break;
        //                        case 7:
        //                            response = response.Create(false, ResponseMessages.TRANSACTION_ERROR, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 8:
        //                            response = response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 9:
        //                            response = response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 10:
        //                            response = response.Create(false, ResponseMessages.INSUFICIENT_BALANCE, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 11:
        //                            response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 12:
        //                            response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 13:
        //                            response = response.Create(false, ResponseMessageKyc.FAILED_Doc_NotUploaded, HttpStatusCode.OK, result);

        //                            break;
        //                        case 14:
        //                            response = response.Create(false, ResponseMessageKyc.FAILED_Doc_Pending, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 15:
        //                            response = response.Create(false, ResponseMessageKyc.Doc_Not_visible, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 16:
        //                            response = response.Create(false, ResponseMessageKyc.Doc_Rejected, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 17:
        //                            response = response.Create(false, ResponseMessageKyc.TRANSACTION_DISABLED, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 18:
        //                            var errorList = new List<Errorkey>();
        //                            if (string.IsNullOrWhiteSpace(requestModel.Amount))
        //                            {
        //                                errorList.Add(new Errorkey { Key = "Amount", Val = "Amount can not be null" });
        //                            }
        //                            if (string.IsNullOrWhiteSpace(requestModel.Password))
        //                            {
        //                                errorList.Add(new Errorkey { Key = "Password", Val = "Password is required" });
        //                            }

        //                            response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 19:
        //                            response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.NotFound, result);

        //                            break;
        //                        default:
        //                            response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.NotFound, result);

        //                            break;
        //                    }
        //                }
        //                else
        //                {
        //                    if (string.IsNullOrEmpty(requestModel.Amount))
        //                    {
        //                        response = response.Create(false, ResponseMessages.EMPTY_AMOUNT, HttpStatusCode.NotFound, result);
        //                    }
        //                    else if (requestModel.Amount.IsZero())
        //                    {
        //                        response = response.Create(false, ResponseMessages.ZERO_AMOUNT, HttpStatusCode.NotFound, result);
        //                    }
        //                    else
        //                    {
        //                        response = response.Create(false, ResponseMessages.IMPROPER_AMOUNT, HttpStatusCode.NotFound, result);
        //                    }
        //                }

        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);
        //            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
        //        }
        //    }
        //    else
        //    {
        //        var errorList = new List<Errorkey>();
        //        foreach (var mod in ModelState)
        //        {
        //            Errorkey objkey = new Errorkey();
        //            objkey.Key = mod.Key;
        //            if (mod.Value.Errors.Count > 0)
        //            {
        //                objkey.Val = mod.Value.Errors[0].ErrorMessage;
        //            }
        //            errorList.Add(objkey);
        //        }
        //        response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);

        //    }
        //    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
        //    return _iHttpActionResult;
        //}













        /////// <summary>
        /////// GetSeerbitCardPaymentUrl
        /////// </summary>
        /////// <param name="request"></param>
        /////// <returns></returns>
        ////[HttpPost]
        ////[Route("GetSeerbitCardPaymentUrl")]
        ////[SessionAuthorization]
        ////[SessionTokenExceptionFilter]
        ////[ResponseType(typeof(Response<SeerbitResponse>))]
        ////public async Task<IHttpActionResult> GetSeerbitCardPaymentUrl(RequestModel request)
        ////{
        ////    var response = new Response<SeerbitResponse>();
        ////    var result = new SeerbitResponse();
        ////    var requestModel = new EncrDecr<ThirdpartyPaymentByCardRequest>().Decrypt(request.Value, false, Request);

        ////    var headerToken = Request.Headers.GetValues("token").FirstOrDefault();

        ////    if (requestModel.Amount != null)
        ////    {
        ////        if (string.IsNullOrEmpty(requestModel.CardNo))
        ////        {
        ////            response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
        ////            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
        ////            return _iHttpActionResult;
        ////        }


        ////        //checkin MaximumAmount 
        ////        AdminMobileMoneyLimitRequest obj = new AdminMobileMoneyLimitRequest
        ////        {
        ////            MinimumAmount = "0",
        ////            Service = "001"
        ////        };
        ////        var newCommisionMinChargesonGivenAmount = await _mobileMoneyServices.VerifyMobileMoneyLimit(obj);
        ////        decimal d1 = decimal.Parse(newCommisionMinChargesonGivenAmount.MinimumAmount);
        ////        decimal d3 = decimal.Parse(newCommisionMinChargesonGivenAmount.MaximumAmount);
        ////        decimal d2 = decimal.Parse(requestModel.Amount);
        ////        if (d2 == d1)
        ////        {
        ////        }
        ////        else if (d2 < d1)
        ////        {
        ////            response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
        ////            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
        ////            return _iHttpActionResult;
        ////        }

        ////        if (d2 == d3)
        ////        {
        ////        }
        ////        else if (d2 > d3)
        ////        {
        ////            response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
        ////            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
        ////            return _iHttpActionResult;
        ////        }



        ////        try
        ////        {
        ////            if (request == null)
        ////            {
        ////                response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotAcceptable, result);
        ////                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
        ////            }
        ////            else
        ////            {
        ////                if (!string.IsNullOrEmpty(requestModel.Amount) && !requestModel.Amount.IsZero() && requestModel.Amount.IsTwoDigitDecimal())
        ////                {
        ////                    result = await _cardPaymentService.GetSeerbitCardPaymentUrl(requestModel, headerToken);
        ////                    switch (result.RstKey)
        ////                    {
        ////                        case 0:
        ////                            response = response.Create(false, ResponseMessages.INVALID_PASSWORD, HttpStatusCode.NotFound, result);
        ////                            break;
        ////                        case 1:
        ////                            response = response.Create(false, AggregatoryMESSAGE.PENDING, HttpStatusCode.OK, result);
        ////                            break;
        ////                        case 2:
        ////                            response = response.Create(true, AggregatoryMESSAGE.SUCCESSFUL, HttpStatusCode.OK, result);
        ////                            break;
        ////                        case 3:
        ////                            response = response.Create(false, AggregatoryMESSAGE.FAILED, HttpStatusCode.NotFound, result);

        ////                            break;
        ////                        case 4:
        ////                            response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_ERROR, HttpStatusCode.NotFound, result);
        ////                            break;
        ////                        case 5:
        ////                            response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_EXCEPTION, HttpStatusCode.NotFound, result);
        ////                            break;
        ////                        case 6:
        ////                            response = response.Create(false, result.message, HttpStatusCode.NotFound, result);
        ////                            break;
        ////                        case 7:
        ////                            response = response.Create(false, ResponseMessages.TRANSACTION_ERROR, HttpStatusCode.NotFound, result);

        ////                            break;
        ////                        case 8:
        ////                            response = response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.NotFound, result);

        ////                            break;
        ////                        case 9:
        ////                            response = response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.NotFound, result);

        ////                            break;
        ////                        case 10:
        ////                            response = response.Create(false, ResponseMessages.INSUFICIENT_BALANCE, HttpStatusCode.NotFound, result);

        ////                            break;
        ////                        case 11:
        ////                            response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotFound, result);

        ////                            break;
        ////                        case 12:
        ////                            response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotFound, result);

        ////                            break;
        ////                        case 13:
        ////                            response = response.Create(false, ResponseMessageKyc.FAILED_Doc_NotUploaded, HttpStatusCode.OK, result);

        ////                            break;
        ////                        case 14:
        ////                            response = response.Create(false, ResponseMessageKyc.FAILED_Doc_Pending, HttpStatusCode.NotFound, result);

        ////                            break;
        ////                        case 15:
        ////                            response = response.Create(false, ResponseMessageKyc.Doc_Not_visible, HttpStatusCode.NotFound, result);

        ////                            break;
        ////                        case 16:
        ////                            response = response.Create(false, ResponseMessageKyc.Doc_Rejected, HttpStatusCode.NotFound, result);

        ////                            break;
        ////                        case 17:
        ////                            response = response.Create(false, ResponseMessageKyc.TRANSACTION_DISABLED, HttpStatusCode.NotFound, result);

        ////                            break;
        ////                        case 18:
        ////                            var errorList = new List<Errorkey>();
        ////                            if (string.IsNullOrWhiteSpace(requestModel.Amount))
        ////                            {
        ////                                errorList.Add(new Errorkey { Key = "Amount", Val = "Amount can not be null" });
        ////                            }
        ////                            if (string.IsNullOrWhiteSpace(requestModel.Password))
        ////                            {
        ////                                errorList.Add(new Errorkey { Key = "Password", Val = "Password is required" });
        ////                            }

        ////                            response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);

        ////                            break;
        ////                        case 19:
        ////                            response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.NotFound, result);

        ////                            break;
        ////                        default:
        ////                            response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.NotFound, result);

        ////                            break;
        ////                    }
        ////                }
        ////                else
        ////                {
        ////                    if (string.IsNullOrEmpty(requestModel.Amount))
        ////                    {
        ////                        response = response.Create(false, ResponseMessages.EMPTY_AMOUNT, HttpStatusCode.NotFound, result);
        ////                    }
        ////                    else if (requestModel.Amount.IsZero())
        ////                    {
        ////                        response = response.Create(false, ResponseMessages.ZERO_AMOUNT, HttpStatusCode.NotFound, result);
        ////                    }
        ////                    else
        ////                    {
        ////                        response = response.Create(false, ResponseMessages.IMPROPER_AMOUNT, HttpStatusCode.NotFound, result);
        ////                    }
        ////                }

        ////            }
        ////        }
        ////        catch (Exception ex)
        ////        {
        ////            response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);
        ////            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
        ////        }
        ////    }
        ////    else
        ////    {
        ////        var errorList = new List<Errorkey>();
        ////        foreach (var mod in ModelState)
        ////        {
        ////            Errorkey objkey = new Errorkey();
        ////            objkey.Key = mod.Key;
        ////            if (mod.Value.Errors.Count > 0)
        ////            {
        ////                objkey.Val = mod.Value.Errors[0].ErrorMessage;
        ////            }
        ////            errorList.Add(objkey);
        ////        }
        ////        response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);

        ////    }
        ////    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
        ////    return _iHttpActionResult;
        ////}



        ////[HttpPost]
        ////[Route("GetGTBCIVPaymentUrl")]
        ////[SessionAuthorization]
        ////[SessionTokenExceptionFilter]
        ////[ResponseType(typeof(Response<MasterCardPaymentUBAResponse>))]
        ////public async Task<IHttpActionResult> GetGTBCIVPaymentUrl(RequestModel request)
        ////{
        ////    var response = new Response<MasterCardPaymentUBAResponse>();
        ////    var result = new MasterCardPaymentUBAResponse();
        ////    var requestModel = new EncrDecr<ThirdpartyPaymentByCardRequest>().Decrypt(request.Value, false, Request);

        ////    var headerToken = Request.Headers.GetValues("token").FirstOrDefault();

        ////    if (requestModel.Amount != null)
        ////    {
        ////        if (string.IsNullOrEmpty(requestModel.CardNo))
        ////        {
        ////            response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
        ////            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
        ////            return _iHttpActionResult;
        ////        }


        ////        //checkin MaximumAmount 
        ////        AdminMobileMoneyLimitRequest obj = new AdminMobileMoneyLimitRequest
        ////        {
        ////            MinimumAmount = "0",
        ////            Service = "001"
        ////        };
        ////        var newCommisionMinChargesonGivenAmount = await _mobileMoneyServices.VerifyMobileMoneyLimit(obj);
        ////        decimal d1 = decimal.Parse(newCommisionMinChargesonGivenAmount.MinimumAmount);
        ////        decimal d3 = decimal.Parse(newCommisionMinChargesonGivenAmount.MaximumAmount);
        ////        decimal d2 = decimal.Parse(requestModel.Amount);
        ////        if (d2 == d1)
        ////        {
        ////        }
        ////        else if (d2 < d1)
        ////        {
        ////            response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
        ////            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
        ////            return _iHttpActionResult;
        ////        }

        ////        if (d2 == d3)
        ////        {
        ////        }
        ////        else if (d2 > d3)
        ////        {
        ////            response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
        ////            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
        ////            return _iHttpActionResult;
        ////        }



        ////        try
        ////        {
        ////            if (request == null)
        ////            {
        ////                response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotAcceptable, result);
        ////                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
        ////            }
        ////            else
        ////            {
        ////                if (!string.IsNullOrEmpty(requestModel.Amount) && !requestModel.Amount.IsZero() && requestModel.Amount.IsTwoDigitDecimal())
        ////                {
        ////                    result = await _cardPaymentService.GetGTBCIVPaymentUrl(requestModel, headerToken);
        ////                    switch (result.RstKey)
        ////                    {
        ////                        case 0:
        ////                            response = response.Create(false, ResponseMessages.INVALID_PASSWORD, HttpStatusCode.NotFound, result);
        ////                            break;
        ////                        case 1:
        ////                            response = response.Create(false, AggregatoryMESSAGE.PENDING, HttpStatusCode.OK, result);
        ////                            break;
        ////                        case 2:
        ////                            response = response.Create(true, AggregatoryMESSAGE.SUCCESSFUL, HttpStatusCode.OK, result);
        ////                            break;
        ////                        case 3:
        ////                            response = response.Create(false, AggregatoryMESSAGE.FAILED, HttpStatusCode.NotFound, result);

        ////                            break;
        ////                        case 4:
        ////                            response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_ERROR, HttpStatusCode.NotFound, result);
        ////                            break;
        ////                        case 5:
        ////                            response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_EXCEPTION, HttpStatusCode.NotFound, result);
        ////                            break;
        ////                        case 6:
        ////                            response = response.Create(false, result.Message, HttpStatusCode.NotFound, result);
        ////                            break;
        ////                        case 7:
        ////                            response = response.Create(false, ResponseMessages.TRANSACTION_ERROR, HttpStatusCode.NotFound, result);

        ////                            break;
        ////                        case 8:
        ////                            response = response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.NotFound, result);

        ////                            break;
        ////                        case 9:
        ////                            response = response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.NotFound, result);

        ////                            break;
        ////                        case 10:
        ////                            response = response.Create(false, ResponseMessages.INSUFICIENT_BALANCE, HttpStatusCode.NotFound, result);

        ////                            break;
        ////                        case 11:
        ////                            response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotFound, result);

        ////                            break;
        ////                        case 12:
        ////                            response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotFound, result);

        ////                            break;
        ////                        case 13:
        ////                            response = response.Create(false, ResponseMessageKyc.FAILED_Doc_NotUploaded, HttpStatusCode.OK, result);

        ////                            break;
        ////                        case 14:
        ////                            response = response.Create(false, ResponseMessageKyc.FAILED_Doc_Pending, HttpStatusCode.NotFound, result);

        ////                            break;
        ////                        case 15:
        ////                            response = response.Create(false, ResponseMessageKyc.Doc_Not_visible, HttpStatusCode.NotFound, result);

        ////                            break;
        ////                        case 16:
        ////                            response = response.Create(false, ResponseMessageKyc.Doc_Rejected, HttpStatusCode.NotFound, result);

        ////                            break;
        ////                        case 17:
        ////                            response = response.Create(false, ResponseMessageKyc.TRANSACTION_DISABLED, HttpStatusCode.NotFound, result);

        ////                            break;
        ////                        case 18:
        ////                            var errorList = new List<Errorkey>();
        ////                            if (string.IsNullOrWhiteSpace(requestModel.Amount))
        ////                            {
        ////                                errorList.Add(new Errorkey { Key = "Amount", Val = "Amount can not be null" });
        ////                            }
        ////                            if (string.IsNullOrWhiteSpace(requestModel.Password))
        ////                            {
        ////                                errorList.Add(new Errorkey { Key = "Password", Val = "Password is required" });
        ////                            }

        ////                            response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);

        ////                            break;
        ////                        case 19:
        ////                            response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.NotFound, result);

        ////                            break;
        ////                        default:
        ////                            response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.NotFound, result);

        ////                            break;
        ////                    }
        ////                }
        ////                else
        ////                {
        ////                    if (string.IsNullOrEmpty(requestModel.Amount))
        ////                    {
        ////                        response = response.Create(false, ResponseMessages.EMPTY_AMOUNT, HttpStatusCode.NotFound, result);
        ////                    }
        ////                    else if (requestModel.Amount.IsZero())
        ////                    {
        ////                        response = response.Create(false, ResponseMessages.ZERO_AMOUNT, HttpStatusCode.NotFound, result);
        ////                    }
        ////                    else
        ////                    {
        ////                        response = response.Create(false, ResponseMessages.IMPROPER_AMOUNT, HttpStatusCode.NotFound, result);
        ////                    }
        ////                }

        ////            }
        ////        }
        ////        catch (Exception ex)
        ////        {
        ////            response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);
        ////            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
        ////        }
        ////    }
        ////    else
        ////    {
        ////        var errorList = new List<Errorkey>();
        ////        foreach (var mod in ModelState)
        ////        {
        ////            Errorkey objkey = new Errorkey();
        ////            objkey.Key = mod.Key;
        ////            if (mod.Value.Errors.Count > 0)
        ////            {
        ////                objkey.Val = mod.Value.Errors[0].ErrorMessage;
        ////            }
        ////            errorList.Add(objkey);
        ////        }
        ////        response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);

        ////    }
        ////    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
        ////    return _iHttpActionResult;
        ////}



        ///// <summary>
        ///// GetCardPaymentUrlForFXKUDI
        ///// </summary>
        ///// <param name="request"></param>
        ///// <returns></returns>
        //[HttpPost]
        //[Route("GetCardPaymentUrlForFXKUDI")]M
        //[SessionAuthorization]
        //[SessionTokenExceptionFilter]
        //[ResponseType(typeof(Response<FXKUDIPaymentUrlResponse>))]
        //public async Task<IHttpActionResult> GetCardPaymentUrlForFXKUDI(RequestModel request)
        //{
        //    var response = new Response<FXKUDIPaymentUrlResponse>();
        //    var result = new FXKUDIPaymentUrlResponse();
        //    var requestModel = new EncrDecr<ThirdpartyPaymentByCardRequest>().Decrypt(request.Value, false, Request);

        //    var headerToken = Request.Headers.GetValues("token").FirstOrDefault();

        //    if (requestModel.Amount != null)
        //    {
        //        //if (string.IsNullOrEmpty(requestModel.CardNo))
        //        //{
        //        //    response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
        //        //    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
        //        //    return _iHttpActionResult;
        //        //}


        //        //checkin MaximumAmount 
        //        AdminMobileMoneyLimitRequest obj = new AdminMobileMoneyLimitRequest
        //        {
        //            MinimumAmount = "0",
        //            Service = "001"
        //        };
        //        var newCommisionMinChargesonGivenAmount = await _mobileMoneyServices.VerifyMobileMoneyLimit(obj);
        //        decimal d1 = decimal.Parse(newCommisionMinChargesonGivenAmount.MinimumAmount);
        //        decimal d3 = decimal.Parse(newCommisionMinChargesonGivenAmount.MaximumAmount);
        //        decimal d2 = decimal.Parse(requestModel.Amount);
        //        if (d2 == d1)
        //        {
        //        }
        //        else if (d2 < d1)
        //        {
        //            response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
        //            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
        //            return _iHttpActionResult;
        //        }

        //        if (d2 == d3)
        //        {
        //        }
        //        else if (d2 > d3)
        //        {
        //            response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
        //            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
        //            return _iHttpActionResult;
        //        }



        //        try
        //        {
        //            if (request == null)
        //            {
        //                response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotAcceptable, result);
        //                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
        //            }
        //            else
        //            {
        //                if (!string.IsNullOrEmpty(requestModel.Amount) && !requestModel.Amount.IsZero() && requestModel.Amount.IsTwoDigitDecimal())
        //                {
        //                    result = await _cardPaymentService.GetCardPaymentUrlForFXKUDI(requestModel, headerToken);
        //                    switch (result.RstKey)
        //                    {
        //                        case 0:
        //                            response = response.Create(false, ResponseMessages.INVALID_PASSWORD, HttpStatusCode.NotFound, result);
        //                            break;
        //                        case 1:
        //                            response = response.Create(false, AggregatoryMESSAGE.PENDING, HttpStatusCode.OK, result);
        //                            break;
        //                        case 2:
        //                            response = response.Create(true, AggregatoryMESSAGE.SUCCESSFUL, HttpStatusCode.OK, result);
        //                            break;
        //                        case 3:
        //                            response = response.Create(false, AggregatoryMESSAGE.FAILED, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 4:
        //                            response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_ERROR, HttpStatusCode.NotFound, result);
        //                            break;
        //                        case 5:
        //                            response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_EXCEPTION, HttpStatusCode.NotFound, result);
        //                            break;
        //                        case 6:
        //                            response = response.Create(false, result.Message, HttpStatusCode.NotFound, result);
        //                            break;
        //                        case 7:
        //                            response = response.Create(false, ResponseMessages.TRANSACTION_ERROR, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 8:
        //                            response = response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 9:
        //                            response = response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 10:
        //                            response = response.Create(false, ResponseMessages.INSUFICIENT_BALANCE, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 11:
        //                            response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 12:
        //                            response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 13:
        //                            response = response.Create(false, ResponseMessageKyc.FAILED_Doc_NotUploaded, HttpStatusCode.OK, result);

        //                            break;
        //                        case 14:
        //                            response = response.Create(false, ResponseMessageKyc.FAILED_Doc_Pending, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 15:
        //                            response = response.Create(false, ResponseMessageKyc.Doc_Not_visible, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 16:
        //                            response = response.Create(false, ResponseMessageKyc.Doc_Rejected, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 17:
        //                            response = response.Create(false, ResponseMessageKyc.TRANSACTION_DISABLED, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 18:
        //                            var errorList = new List<Errorkey>();
        //                            if (string.IsNullOrWhiteSpace(requestModel.Amount))
        //                            {
        //                                errorList.Add(new Errorkey { Key = "Amount", Val = "Amount can not be null" });
        //                            }
        //                            if (string.IsNullOrWhiteSpace(requestModel.Password))
        //                            {
        //                                errorList.Add(new Errorkey { Key = "Password", Val = "Password is required" });
        //                            }

        //                            response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);

        //                            break;
        //                        case 19:
        //                            response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.NotFound, result);

        //                            break;
        //                        default:
        //                            response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.NotFound, result);

        //                            break;
        //                    }
        //                }
        //                else
        //                {
        //                    if (string.IsNullOrEmpty(requestModel.Amount))
        //                    {
        //                        response = response.Create(false, ResponseMessages.EMPTY_AMOUNT, HttpStatusCode.NotFound, result);
        //                    }
        //                    else if (requestModel.Amount.IsZero())
        //                    {
        //                        response = response.Create(false, ResponseMessages.ZERO_AMOUNT, HttpStatusCode.NotFound, result);
        //                    }
        //                    else
        //                    {
        //                        response = response.Create(false, ResponseMessages.IMPROPER_AMOUNT, HttpStatusCode.NotFound, result);
        //                    }
        //                }

        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);
        //            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
        //        }
        //    }
        //    else
        //    {
        //        var errorList = new List<Errorkey>();
        //        foreach (var mod in ModelState)
        //        {
        //            Errorkey objkey = new Errorkey();
        //            objkey.Key = mod.Key;
        //            if (mod.Value.Errors.Count > 0)
        //            {
        //                objkey.Val = mod.Value.Errors[0].ErrorMessage;
        //            }
        //            errorList.Add(objkey);
        //        }
        //        response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);

        //    }
        //    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
        //    return _iHttpActionResult;
        //}


        /// <summary>
        /// GetCardPaymentUrlForNewFlowMasterCard
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("merchantNewFlowPaymentUrl")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<merchantPaymentUrlResponse>))]
        public async Task<IHttpActionResult> merchantNewFlowPaymentUrl(RequestModel request)
        {
            var response = new Response<merchantPaymentUrlResponse>();
            var result = new merchantPaymentUrlResponse();
            var requestModel = new EncrDecr<ThirdpartyPaymentByCardRequest>().Decrypt(request.Value, false, Request);

            var headerToken = Request.Headers.GetValues("token").FirstOrDefault();

            if (requestModel.Amount != null)
            {
                if (string.IsNullOrEmpty(requestModel.CardNo))
                {
                    response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
                    return _iHttpActionResult;
                }


                

                try
                {
                    if (request == null)
                    {
                        response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotAcceptable, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(requestModel.Amount) && !requestModel.Amount.IsZero() && requestModel.Amount.IsTwoDigitDecimal())
                        {
                            result = await _cardPaymentService.merchantNewFlowPaymentUrl(requestModel, headerToken);
                            switch (result.RstKey)
                            {
                                case 0:
                                    response = response.Create(false, ResponseMessages.INVALID_PASSWORD, HttpStatusCode.NotFound, result);
                                    break;
                                case 1:
                                    response = response.Create(false, AggregatoryMESSAGE.PENDING, HttpStatusCode.OK, result);
                                    break;
                                case 2:
                                    response = response.Create(true, AggregatoryMESSAGE.SUCCESSFUL, HttpStatusCode.OK, result);
                                    break;
                                case 3:
                                    response = response.Create(false, AggregatoryMESSAGE.FAILED, HttpStatusCode.NotFound, result);

                                    break;
                                case 4:
                                    response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_ERROR, HttpStatusCode.NotFound, result);
                                    break;
                                case 5:
                                    response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_EXCEPTION, HttpStatusCode.NotFound, result);
                                    break;
                                case 6:
                                    response = response.Create(false, result.Message, HttpStatusCode.NotFound, result);
                                    break;
                                case 7:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_ERROR, HttpStatusCode.NotFound, result);

                                    break;
                                case 8:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.NotFound, result);

                                    break;
                                case 9:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.NotFound, result);

                                    break;
                                case 10:
                                    response = response.Create(false, ResponseMessages.INSUFICIENT_BALANCE, HttpStatusCode.NotFound, result);

                                    break;
                                case 11:
                                    response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotFound, result);

                                    break;
                                case 12:
                                    response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotFound, result);

                                    break;
                                case 13:
                                    response = response.Create(false, ResponseMessageKyc.FAILED_Doc_NotUploaded, HttpStatusCode.OK, result);

                                    break;
                                case 14:
                                    response = response.Create(false, ResponseMessageKyc.FAILED_Doc_Pending, HttpStatusCode.NotFound, result);

                                    break;
                                case 15:
                                    response = response.Create(false, ResponseMessageKyc.Doc_Not_visible, HttpStatusCode.NotFound, result);

                                    break;
                                case 16:
                                    response = response.Create(false, ResponseMessageKyc.Doc_Rejected, HttpStatusCode.NotFound, result);

                                    break;
                                case 17:
                                    response = response.Create(false, ResponseMessageKyc.TRANSACTION_DISABLED, HttpStatusCode.NotFound, result);

                                    break;
                                case 18:
                                    var errorList = new List<Errorkey>();
                                    if (string.IsNullOrWhiteSpace(requestModel.Amount))
                                    {
                                        errorList.Add(new Errorkey { Key = "Amount", Val = "Amount can not be null" });
                                    }
                                    if (string.IsNullOrWhiteSpace(requestModel.Password))
                                    {
                                        errorList.Add(new Errorkey { Key = "Password", Val = "Password is required" });
                                    }

                                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);

                                    break;
                                case 19:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.NotFound, result);

                                    break;
                                default:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.NotFound, result);

                                    break;
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(requestModel.Amount))
                            {
                                response = response.Create(false, ResponseMessages.EMPTY_AMOUNT, HttpStatusCode.NotFound, result);
                            }
                            else if (requestModel.Amount.IsZero())
                            {
                                response = response.Create(false, ResponseMessages.ZERO_AMOUNT, HttpStatusCode.NotFound, result);
                            }
                            else
                            {
                                response = response.Create(false, ResponseMessages.IMPROPER_AMOUNT, HttpStatusCode.NotFound, result);
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);
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
                response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);

            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }

        [HttpPost]
        [Route("notificationsarl")]
        public async Task<IHttpActionResult> notificationsarl()
        {
            var response = new Response<string>();
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _cardPaymentService.notificationsarl();
                    response = response.Create(false, AggregatoryMESSAGE.FAILED, HttpStatusCode.NotFound, null);
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.NotFound, null);
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
                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, null);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, false, false, Request);
            return _iHttpActionResult;
        }

    }
}
