using ezeePay.Utility.CommonClass;
using Ezipay.Api.Filters;
using Ezipay.Service.BillPaymentService;
using Ezipay.Service.CommonService;
using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.ViewModel.AirtimeViewModel;
using Ezipay.ViewModel.BillViewModel;
using Ezipay.ViewModel.common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Ezipay.Api.Controllers.Web
{
    /// <summary>
    /// BillPaymentController
    /// </summary>
    [RoutePrefix("api/BillPaymentController")]
    [TransactionsAllowed]
    public class BillPaymentController : ApiController
    {
        private IBillPaymentService _billpaymentService;
        private ICommonServices _commonServices;
        private IHttpActionResult _iHttpActionResult;
        private Converter _converter;

        /// <summary>
        /// BillPaymentController
        /// </summary>
        /// <param name="billpaymentService"></param>
        /// <param name="commonServices"></param>
        public BillPaymentController(IBillPaymentService billpaymentService, ICommonServices commonServices)
        {
            _billpaymentService = billpaymentService;
            _converter = new Converter();
            _commonServices = commonServices;
        }

        /// <summary>
        /// GetBillAggregator
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetBillAggregator")]
        [ResponseType(typeof(Response<AddMoneyAggregatorResponse>))]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        public async Task<IHttpActionResult> GetBillAggregator(RequestModel request)
        {
            var requestModel = new EncrDecr<BillPayMoneyAggregatoryRequest>().Decrypt(request.Value, false, Request);
            var response = new Response<AddMoneyAggregatorResponse>();
            var result = new AddMoneyAggregatorResponse();
            //int langId = AppUtils.GetLangId(Request);    
            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();////
            bool IsCorrectPassword = false;
            if (requestModel.Password != null)
            {
                try
                {
                    if (request == null)
                    {
                        response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotAcceptable, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
                    }
                    else
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
                            result = await _billpaymentService.GetBillPaymentServicesAggregator(requestModel);
                            switch (result.RstKey)
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
                                    response = response.Create(false, AggregatoryMESSAGE.FAILED, HttpStatusCode.OK, result);

                                    break;
                                case 4:
                                    response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_ERROR, HttpStatusCode.OK, result);

                                    break;
                                case 5:
                                    response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_EXCEPTION, HttpStatusCode.OK, result);

                                    break;
                                case 6:
                                    response = response.Create(false, result.Message, HttpStatusCode.OK, result);

                                    break;
                                case 7:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_ERROR, HttpStatusCode.OK, result);

                                    break;
                                case 8:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.OK, result);

                                    break;
                                case 9:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.OK, result);

                                    break;
                                case 10:
                                    response = response.Create(false, ResponseMessages.INSUFICIENT_BALANCE, HttpStatusCode.NotAcceptable, result);

                                    break;
                                case 11:
                                    response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.OK, result);

                                    break;
                                case 12:
                                    response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.OK, result);

                                    break;
                                case 13:
                                    response = response.Create(false, ResponseMessageKyc.FAILED_Doc_NotUploaded, HttpStatusCode.OK, result);

                                    break;
                                case 14:
                                    response = response.Create(true, ResponseMessageKyc.FAILED_Doc_Pending, HttpStatusCode.OK, result);

                                    break;
                                case 15:
                                    response = response.Create(false, ResponseMessageKyc.Doc_Not_visible, HttpStatusCode.OK, result);

                                    break;
                                case 16:
                                    response = response.Create(false, ResponseMessageKyc.Doc_Rejected, HttpStatusCode.OK, result);

                                    break;
                                case 17:
                                    response = response.Create(false, ResponseMessageKyc.TRANSACTION_DISABLED, HttpStatusCode.OK, result);

                                    break;
                                case 18:
                                    var errorList = new List<Errorkey>();
                                    if (string.IsNullOrWhiteSpace(requestModel.amount))
                                    {
                                        errorList.Add(new Errorkey { Key = "Amount", Val = "Amount can not be null" });
                                    }
                                    if (string.IsNullOrWhiteSpace(requestModel.ISD))
                                    {
                                        errorList.Add(new Errorkey { Key = "CountryCode", Val = "CountryCode is required" });
                                    }
                                    //if (string.IsNullOrWhiteSpace(requestModel.MobileNo))
                                    //{
                                    //    errorList.Add(new Errorkey { Key = "PhoneNumber", Val = "PhoneNumber is required" });
                                    //}

                                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.OK, result);
                                    break;
                                case 19:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.OK, result);
                                    break;
                                default:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.OK, result);
                                    break;
                            }
                        }
                        else
                        {
                            response = response.Create(false, ResponseMessages.INVALID_PASSWORD, HttpStatusCode.NotFound, result);
                            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
                            return _iHttpActionResult;
                        }
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.OK, result);
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
                response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.OK, result);

            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }

        /// <summary>
        /// PayBillAggregator
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PayBillAggregator")]
        [ResponseType(typeof(Response<AddMoneyAggregatorResponse>))]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        public async Task<IHttpActionResult> PayBillAggregator(RequestModel request)
        {
            var requestModel = new EncrDecr<BillPayMoneyAggregatoryRequest>().Decrypt(request.Value, false, Request);
            var response = new Response<AddMoneyAggregatorResponse>();
            var result = new AddMoneyAggregatorResponse();
            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();////

            //int langId = AppUtils.GetLangId(Request);           
            bool IsCorrectPassword = false;
            if (requestModel.Password != null && requestModel.amount != null)
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
                            if (!string.IsNullOrEmpty(requestModel.amount) && !requestModel.amount.IsZero() && requestModel.amount.IsTwoDigitDecimal())
                            {
                                result = await _billpaymentService.BillPaymentServicesAggregator(requestModel);
                                switch (result.RstKey)
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
                                        response = response.Create(false, AggregatoryMESSAGE.FAILED, HttpStatusCode.OK, result);

                                        break;
                                    case 4:
                                        response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_ERROR, HttpStatusCode.OK, result);

                                        break;
                                    case 5:
                                        response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_EXCEPTION, HttpStatusCode.OK, result);

                                        break;
                                    case 6:
                                        response = response.Create(false, result.Message, HttpStatusCode.OK, result);

                                        break;
                                    case 7:
                                        response = response.Create(false, ResponseMessages.TRANSACTION_ERROR, HttpStatusCode.OK, result);

                                        break;
                                    case 8:
                                        response = response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.OK, result);

                                        break;
                                    case 9:
                                        response = response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.OK, result);

                                        break;
                                    case 10:
                                        response = response.Create(false, ResponseMessages.INSUFICIENT_BALANCE, HttpStatusCode.NotAcceptable, result);

                                        break;
                                    case 11:
                                        response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.OK, result);

                                        break;
                                    case 12:
                                        response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.OK, result);

                                        break;
                                    case 13:
                                        response = response.Create(false, ResponseMessageKyc.FAILED_Doc_NotUploaded, HttpStatusCode.OK, result);

                                        break;
                                    case 14:
                                        response = response.Create(true, ResponseMessageKyc.FAILED_Doc_Pending, HttpStatusCode.OK, result);

                                        break;
                                    case 15:
                                        response = response.Create(false, ResponseMessageKyc.Doc_Not_visible, HttpStatusCode.OK, result);

                                        break;
                                    case 16:
                                        response = response.Create(false, ResponseMessageKyc.Doc_Rejected, HttpStatusCode.OK, result);

                                        break;
                                    case 17:
                                        response = response.Create(false, ResponseMessageKyc.TRANSACTION_DISABLED, HttpStatusCode.OK, result);

                                        break;
                                    case 18:
                                        var errorList = new List<Errorkey>();
                                        if (string.IsNullOrWhiteSpace(requestModel.amount))
                                        {
                                            errorList.Add(new Errorkey { Key = "Amount", Val = "Amount can not be null" });
                                        }
                                        if (string.IsNullOrWhiteSpace(requestModel.ISD))
                                        {
                                            errorList.Add(new Errorkey { Key = "CountryCode", Val = "CountryCode is required" });
                                        }
                                        //if (string.IsNullOrWhiteSpace(requestModel.MobileNo))
                                        //{
                                        //    errorList.Add(new Errorkey { Key = "PhoneNumber", Val = "PhoneNumber is required" });
                                        //}

                                        response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.OK, result);
                                        break;
                                    case 19:
                                        response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.OK, result);
                                        break;
                                    case 21:
                                        response = response.Create(false, ResponseMessages.MobileNotVerify, HttpStatusCode.OK, result);
                                        break;

                                    default:
                                        response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.OK, result);
                                        break;
                                }
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(requestModel.amount))
                                {
                                    response = response.Create(false, ResponseMessages.EMPTY_AMOUNT, HttpStatusCode.NotFound, result);
                                }
                                else if (requestModel.amount.IsZero())
                                {
                                    response = response.Create(false, ResponseMessages.ZERO_AMOUNT, HttpStatusCode.NotFound, result);
                                }
                                else
                                {
                                    response = response.Create(false, ResponseMessages.IMPROPER_AMOUNT, HttpStatusCode.NotFound, result);
                                }
                            }
                        }
                        else
                        {
                            response = response.Create(false, ResponseMessages.INVALID_PASSWORD, HttpStatusCode.NotFound, result);
                            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
                            return _iHttpActionResult;
                        }
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.OK, result);
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
                response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.OK, result);

            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }

        ///// <summary>
        ///// GetFee
        ///// </summary>
        ///// <param name="request"></param>
        ///// <returns></returns>
        //[HttpPost]
        //[Route("GetFee")]
        //[ResponseType(typeof(Response<AddMoneyAggregatorResponse>))]
        //[SessionAuthorization]
        //[SessionTokenExceptionFilter]
        //public async Task<IHttpActionResult> GetFee(RequestModel request)
        //{
        //    var requestModel = new EncrDecr<PayMoneyAggregatoryRequest>().Decrypt(request.Value, false);
        //    var response = new Response<string>();
        //    var  result = await _billpaymentService.GetFee(requestModel);
        //    if (result != null)
        //    {
        //        response = response.Create(true, AggregatoryMESSAGE.SUCCESSFUL, HttpStatusCode.OK, result);
        //    }
        //    else
        //    {

        //    }

        //    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false);
        //    return _iHttpActionResult;
        //}
    }
}
