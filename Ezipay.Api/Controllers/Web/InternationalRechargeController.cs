using ezeePay.Utility.CommonClass;
using Ezipay.Api.Filters;
using Ezipay.Service.CommonService;
using Ezipay.Service.InternatinalRechargeServ;
using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.ViewModel.AirtimeViewModel;
using Ezipay.ViewModel.common;
using Ezipay.ViewModel.InternatinalRechargeViewModel;
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
    [RoutePrefix("api/InternationalRechargeController")]
    [TransactionsAllowed]
    public class InternationalRechargeController : ApiController
    {
        private IHttpActionResult _iHttpActionResult;
        private ICommonServices _commonServices;
        private IInternatinalRechargeService _internatinalRechargeService;
        private Converter _converter;
        // private ILogUtils _logUtils;
        public InternationalRechargeController(IInternatinalRechargeService internatinalRechargeService, ICommonServices commonServices)
        {
            _internatinalRechargeService = internatinalRechargeService;
            _converter = new Converter();
            _commonServices = commonServices;
        }

        /// <summary>
        /// GetProductList
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetProductList")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<InternationalAirtimeResponse>))]
        public async Task<IHttpActionResult> GetProductList(RequestModel request)
        {
            var requestModel = new EncrDecr<InternationalAirtimeRequest>().Decrypt(request.Value, false, Request);
            var response = new Response<InternationalAirtimeResponse>();
            var result = new InternationalAirtimeResponse();
            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();////
            //int langId = AppUtils.GetLangId(Request);  
            bool IsCorrectPassword = false;
            if (requestModel != null)
            {
                try
                {
                    if (requestModel == null)
                    {
                        response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.OK, result);
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
                            if (!string.IsNullOrEmpty(requestModel.MobileNo))
                            {
                                result = await _internatinalRechargeService.GetProductList(requestModel);
                                switch (result.RstKey)
                                {
                                    case 0:
                                        response = response.Create(false, ResponseMessages.INVALID_PASSWORD, HttpStatusCode.OK, result);
                                        break;
                                    case 1:
                                        response = response.Create(true, AggregatoryMESSAGE.SUCCESSFUL, HttpStatusCode.OK, result);

                                        break;
                                    case 2:
                                        response = response.Create(false, AggregatoryMESSAGE.PENDING, HttpStatusCode.OK, result);
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
                                        response = response.Create(false, ResponseMessages.INSUFICIENT_BALANCE, HttpStatusCode.OK, result);
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
                                        response = response.Create(false, ResponseMessageKyc.FAILED_Doc_Pending, HttpStatusCode.OK, result);
                                        break;
                                    case 15:
                                        response = response.Create(false, ResponseMessageKyc.Doc_Not_visible, HttpStatusCode.OK, result);
                                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                                        break;
                                    case 16:
                                        response = response.Create(false, ResponseMessageKyc.Doc_Rejected, HttpStatusCode.OK, result);
                                        break;
                                    case 17:
                                        response = response.Create(false, ResponseMessageKyc.TRANSACTION_DISABLED, HttpStatusCode.OK, result);
                                        break;
                                    case 18:
                                        var errorList = new List<Errorkey>();
                                        if (string.IsNullOrWhiteSpace(requestModel.Amount))
                                        {
                                            errorList.Add(new Errorkey { Key = "Amount", Val = "Amount can not be null" });
                                        }
                                        if (string.IsNullOrWhiteSpace(requestModel.IsdCode))
                                        {
                                            errorList.Add(new Errorkey { Key = "CountryCode", Val = "CountryCode is required" });
                                        }
                                        if (string.IsNullOrWhiteSpace(requestModel.MobileNo))
                                        {
                                            errorList.Add(new Errorkey { Key = "PhoneNumber", Val = "PhoneNumber is required" });
                                        }

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
                    response = response.Create(false, ex.Message, HttpStatusCode.OK, result);
                    // _logUtils.InterNationalAirtime(ex.Message);
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
        /// InternationalAirtimeServices
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("InternationalAirtimeServices")]
        [ResponseType(typeof(Response<AddMoneyAggregatorResponse>))]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        public async Task<IHttpActionResult> InternationalAirtimeServices(RequestModel request)
        {
            var requestModel = new EncrDecr<RechargeAirtimeInternationalAggregatorRequest>().Decrypt(request.Value, false, Request);
            var response = new Response<AddMoneyAggregatorResponse>();
            var result = new AddMoneyAggregatorResponse();
            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();

            //int langId = AppUtils.GetLangId(Request);           
            bool IsCorrectPassword = false;
            if (requestModel.Password != null && requestModel.Amount != null)
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
                            if (!string.IsNullOrEmpty(requestModel.Amount) && !requestModel.Amount.IsZero() && requestModel.Amount.IsTwoDigitDecimal())
                            {
                                result = await _internatinalRechargeService.InternationalAirtimeServices(requestModel, sessionToken);
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
                                        if (string.IsNullOrWhiteSpace(requestModel.Amount))
                                        {
                                            errorList.Add(new Errorkey { Key = "Amount", Val = "Amount can not be null" });
                                        }
                                        if (string.IsNullOrWhiteSpace(requestModel.ISD))
                                        {
                                            errorList.Add(new Errorkey { Key = "CountryCode", Val = "CountryCode is required" });
                                        }
                                        if (string.IsNullOrWhiteSpace(requestModel.MobileNo))
                                        {
                                            errorList.Add(new Errorkey { Key = "PhoneNumber", Val = "PhoneNumber is required" });
                                        }

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
        /// InternationalDTHServices
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("InternationalDTHServices")]
        [ResponseType(typeof(Response<AddMoneyAggregatorResponse>))]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        public async Task<IHttpActionResult> InternationalDTHServices(RequestModel request)
        {
            var requestModel = new EncrDecr<RechargeDthInternationalAggregatorRequest>().Decrypt(request.Value, false, Request);
            var response = new Response<AddMoneyAggregatorResponse>();
            var result = new AddMoneyAggregatorResponse();
            //int langId = AppUtils.GetLangId(Request);     
            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();
            bool IsCorrectPassword = false;
            if (requestModel.Password != null && requestModel.Amount != null)
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
                            if (!string.IsNullOrEmpty(requestModel.Amount) && !requestModel.Amount.IsZero() && requestModel.Amount.IsTwoDigitDecimal())
                            {                               
                                result = await _internatinalRechargeService.InternationalDTHServices(requestModel);
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
                                        if (string.IsNullOrWhiteSpace(requestModel.Amount))
                                        {
                                            errorList.Add(new Errorkey { Key = "Amount", Val = "Amount can not be null" });
                                        }
                                        if (string.IsNullOrWhiteSpace(requestModel.ISD))
                                        {
                                            errorList.Add(new Errorkey { Key = "CountryCode", Val = "CountryCode is required" });
                                        }
                                        if (string.IsNullOrWhiteSpace(requestModel.MobileNo))
                                        {
                                            errorList.Add(new Errorkey { Key = "PhoneNumber", Val = "PhoneNumber is required" });
                                        }

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
        /// GetCountryList
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [ResponseType(typeof(Response<InternationalDTHResponse>))]
        [Route("GetCountryList")]
        public async Task<IHttpActionResult> GetCountryList()
        {
            var response = new Response<InternationalDTHResponse>();
            var result = new InternationalDTHResponse();
            // int langId = AppUtils.GetLangId(Request);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _internatinalRechargeService.GetCountryList();
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
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError, false, false, Request);
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
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, false, false, Request);
            return _iHttpActionResult;
        }


        /// <summary>
        /// GetServiceList
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("GetServiceList")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<InternationalDTHResponse>))]
        public async Task<IHttpActionResult> GetServiceList(RequestModel request)
        {
            var requestModel = new EncrDecr<GetServiceListRequest>().Decrypt(request.Value, false, Request);
            var response = new Response<InternationalDTHResponse>();
            var result = new InternationalDTHResponse();
            //int langId = AppUtils.GetLangId(Request);             
            try
            {

                if (requestModel != null)
                {
                    result = await _internatinalRechargeService.GetServiceList(requestModel);
                    switch (result.RstKey)
                    {
                        case 0:
                            response = response.Create(false, ResponseMessages.INVALID_PASSWORD, HttpStatusCode.OK, result);
                            break;
                        case 1:
                            response = response.Create(true, AggregatoryMESSAGE.SUCCESSFUL, HttpStatusCode.OK, result);

                            break;
                        case 2:
                            response = response.Create(false, AggregatoryMESSAGE.PENDING, HttpStatusCode.OK, result);
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
                            response = response.Create(false, ResponseMessages.INSUFICIENT_BALANCE, HttpStatusCode.OK, result);
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
                            response = response.Create(false, ResponseMessageKyc.FAILED_Doc_Pending, HttpStatusCode.OK, result);
                            break;
                        case 15:
                            response = response.Create(false, ResponseMessageKyc.Doc_Not_visible, HttpStatusCode.OK, result);
                            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                            break;
                        case 16:
                            response = response.Create(false, ResponseMessageKyc.Doc_Rejected, HttpStatusCode.OK, result);
                            break;
                        case 17:
                            response = response.Create(false, ResponseMessageKyc.TRANSACTION_DISABLED, HttpStatusCode.OK, result);
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
                    response = response.Create(false, "Request is required", HttpStatusCode.OK, result);
                }
            }
            catch (Exception ex)
            {
                response = response.Create(false, ex.Message, HttpStatusCode.OK, result);
                // _logUtils.InterNationalAirtime(ex.Message);
            }

            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }

        /// <summary>
        /// GetOperatorList
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("GetOperatorList")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<InternationalDTHResponse>))]
        public async Task<IHttpActionResult> GetOperatorList(RequestModel request)
        {
            var requestModel = new EncrDecr<GetServiceListRequest>().Decrypt(request.Value, false, Request);
            var response = new Response<InternationalDTHResponse>();
            var result = new InternationalDTHResponse();
            //int langId = AppUtils.GetLangId(Request);              
            try
            {
                if (requestModel != null)
                {

                    result = await _internatinalRechargeService.GetOperatorList(requestModel);
                    switch (result.RstKey)
                    {
                        case 0:
                            response = response.Create(false, ResponseMessages.INVALID_PASSWORD, HttpStatusCode.OK, result);
                            break;
                        case 1:
                            response = response.Create(true, AggregatoryMESSAGE.SUCCESSFUL, HttpStatusCode.OK, result);

                            break;
                        case 2:
                            response = response.Create(false, AggregatoryMESSAGE.PENDING, HttpStatusCode.OK, result);
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
                            response = response.Create(false, ResponseMessages.INSUFICIENT_BALANCE, HttpStatusCode.OK, result);
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
                            response = response.Create(false, ResponseMessageKyc.FAILED_Doc_Pending, HttpStatusCode.OK, result);
                            break;
                        case 15:
                            response = response.Create(false, ResponseMessageKyc.Doc_Not_visible, HttpStatusCode.OK, result);
                            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                            break;
                        case 16:
                            response = response.Create(false, ResponseMessageKyc.Doc_Rejected, HttpStatusCode.OK, result);
                            break;
                        case 17:
                            response = response.Create(false, ResponseMessageKyc.TRANSACTION_DISABLED, HttpStatusCode.OK, result);
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
                    response = response.Create(false, ResponseMessages.UNATHORIZED_REQUEST, HttpStatusCode.NotFound, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
                    return _iHttpActionResult;
                }
            }
            catch (Exception ex)
            {
                response = response.Create(false, ex.Message, HttpStatusCode.OK, result);
                // _logUtils.InterNationalAirtime(ex.Message);
            }

            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetDTHProductList")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<InternationalDTHResponse>))]
        public async Task<IHttpActionResult> GetDTHProductList(RequestModel request)
        {
            var requestModel = new EncrDecr<GetServiceListRequest>().Decrypt(request.Value, false, Request);
            var response = new Response<InternationalDTHProductResponse>();
            var result = new InternationalDTHProductResponse();
            //int langId = AppUtils.GetLangId(Request);             
            try
            {
                if (requestModel != null)
                {

                    result = await _internatinalRechargeService.GetProductList(requestModel);
                    switch (result.RstKey)
                    {
                        case 0:
                            response = response.Create(false, ResponseMessages.INVALID_PASSWORD, HttpStatusCode.OK, result);
                            break;
                        case 1:
                            response = response.Create(true, AggregatoryMESSAGE.SUCCESSFUL, HttpStatusCode.OK, result);

                            break;
                        case 2:
                            response = response.Create(false, AggregatoryMESSAGE.PENDING, HttpStatusCode.OK, result);
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
                            response = response.Create(false, ResponseMessages.INSUFICIENT_BALANCE, HttpStatusCode.OK, result);
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
                            response = response.Create(false, ResponseMessageKyc.FAILED_Doc_Pending, HttpStatusCode.OK, result);
                            break;
                        case 15:
                            response = response.Create(false, ResponseMessageKyc.Doc_Not_visible, HttpStatusCode.OK, result);
                            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                            break;
                        case 16:
                            response = response.Create(false, ResponseMessageKyc.Doc_Rejected, HttpStatusCode.OK, result);
                            break;
                        case 17:
                            response = response.Create(false, ResponseMessageKyc.TRANSACTION_DISABLED, HttpStatusCode.OK, result);
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
                    response = response.Create(false, ResponseMessages.REQUEST_SENT_FAILD, HttpStatusCode.NotFound, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
                    return _iHttpActionResult;
                }
            }
            catch (Exception ex)
            {
                response = response.Create(false, ex.Message, HttpStatusCode.OK, result);
                // _logUtils.InterNationalAirtime(ex.Message);
            }

            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }

    }
}
