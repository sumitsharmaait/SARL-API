using ezeePay.Utility.CommonClass;
using Ezipay.Api.Filters;
using Ezipay.Service.CommonService;
using Ezipay.Service.ThridPartyApiService;
using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.ViewModel.AirtimeViewModel;
using Ezipay.ViewModel.CommisionViewModel;
using Ezipay.ViewModel.common;
using Ezipay.ViewModel.ThridPartyApiVIewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Ezipay.Api.Controllers
{
    [RoutePrefix("api/ThridPartyApiController")]
    public class ThridPartyApiController : ApiController
    {
        private IHttpActionResult _iHttpActionResult;
        private Converter _converter;
        private ICommonServices _commonServices;
        private IThridPartyApiServices _thridPartyApiServices;
        public ThridPartyApiController(IThridPartyApiServices thridPartyApiServices, ICommonServices commonServices)
        {
            _thridPartyApiServices = thridPartyApiServices;
            _converter = new Converter();
            _commonServices = commonServices;
        }

        /// <summary>
        /// UpdateTransactionStatus
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("UpdateTransactionStatus")]
        public async Task<UpdateTransactionResponse> UpdateTransactionStatus(UpdateTransactionRequest request)
        {
            var response = new UpdateTransactionResponse();
            try
            {
               
                response = await _thridPartyApiServices.UpdateTransactionStatus(request);
                "UpdateTransactionStatus".ErrorLog("ThridPartyApiController.cs", "UpdateTransactionStatus Request", request);
                "UpdateTransactionStatus".ErrorLog("ThridPartyApiController.cs", "UpdateTransactionStatus Response", response);
            }
            catch
            {

            }

            return response;
        }

        /// <summary>
        /// IsVerifiedAccount
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<TVVerifyModel>))]
        [SessionTokenExceptionFilter]
        [SessionAuthorization]
        [Route("IsVerifiedAccount")]
        public async Task<IHttpActionResult> IsVerifiedAccount(RequestModel request)
        {
            var response = new Response<TVVerifyModel>();
            var result = new TVVerifyModel();
            var requestModel = new EncrDecr<AccountModel>().Decrypt(request.Value, false, Request);

            if (requestModel.Channel == "KWESE")
            {
                string url = CommonSetting.isVerifiedAccount + requestModel.Account;
                var m_strFilePath = url;
                string xmlStr;
                try
                {
                    using (var wc = new WebClient())
                    {
                        xmlStr = wc.DownloadString(m_strFilePath);
                    }
                    if (!string.IsNullOrEmpty(xmlStr))
                    {
                        var data = JsonConvert.DeserializeObject<DemoVerifyModel>(xmlStr);
                        TVVerifyModel obj = new TVVerifyModel();
                        obj.AccountStatus = data.field9;
                        obj.StatusCode = data.field1;
                        obj.AccountName = data.field6;
                        obj.DateCreated = data.field10;
                        obj.DateModified = data.field11;
                        obj.Currency = data.field8;
                        if (obj.StatusCode == 200 && obj.AccountStatus == "ACTIVE")
                        {
                            response.Create(true, ResponseMessages.UTILITY_ACCOUNT_ACTIVE, HttpStatusCode.OK, obj);
                        }
                        else
                        {
                            response.Create(false, ResponseMessages.UTILITY_ACCOUNT_DEACTIVE, HttpStatusCode.NotAcceptable, obj);
                        }

                    }
                    else
                    {

                        response.Create(false, ResponseMessages.UTILITY_ACCOUNT_NOT_FOUND, HttpStatusCode.NoContent, new TVVerifyModel());
                    }

                    //return Response;
                }
                catch (Exception)
                {
                    response.Create(false, ResponseMessages.UTILITY_ACCOUNT_NOT_FOUND, HttpStatusCode.NotAcceptable, new TVVerifyModel());
                    // return Response;
                }
            }
            else
            {
                response.Create(true, ResponseMessages.UTILITY_ACCOUNT_NOT_FOUND, HttpStatusCode.NotAcceptable, new TVVerifyModel());
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }

        /// <summary>
        /// ServiceCommissionList
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<List<commissionOnAmountModel>>))]
        [Route("ServiceCommissionList")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        public async Task<IHttpActionResult> ServiceCommissionList()
        {
            var response = new Response<List<commissionOnAmountModel>>();
            var result = new List<commissionOnAmountModel>();
            // int langId = AppUtils.GetLangId(Request);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _thridPartyApiServices.ServiceCommissionList();
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
        /// FlightHotelBooking
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<FlightBookingResponse>))]
        [Route("FlightHotelBooking")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        public async Task<IHttpActionResult> FlightHotelBooking(RequestModel request)
        {
            var response = new Response<FlightBookingResponse>();
            var result = new FlightBookingResponse();
            var requestModel = new EncrDecr<FlightBookingPassRequest>().Decrypt(request.Value, false, Request);
            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();////
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
                        result = await _thridPartyApiServices.FlightHotelBooking(sessionToken);
                    }
                    else
                    {
                        response = response.Create(true, ResponseMessages.INVALID_PASSWORD, HttpStatusCode.NotFound, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
                        return _iHttpActionResult;
                    }
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
                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError,true,false, Request);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }

        /// <summary>
        /// DataVerification
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("DataVerification")]
        //[TempSessionAuthorization]
        [ResponseType(typeof(Response<object>))]
        public async Task<IHttpActionResult> DataVerification(VerifyRequest request)
        {
            var response = new Response<object>();
            var result = new object();
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _thridPartyApiServices.DataVerification(request);
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);
                    }
                    else
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);
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
        /// GetFee
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<string>))]
        [Route("GetFee")]
        [SessionAuthorization]
        public async Task<IHttpActionResult> GetFee(RequestModel requestModel)
        {
            var response = new Response<string>();
            string result = "";
            if (ModelState.IsValid)
            {
                try
                {
                    var request = new EncrDecr<PayMoneyAggregatoryRequest>().Decrypt(requestModel.Value);
                    result = await _thridPartyApiServices.GetFee(request);
                    if (result != null)
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


        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<TransactionStatusResponse>))]
        [Route("GetTransactionStatus")]      
        public async Task<IHttpActionResult> GetTransactionStatus()
        {
            var response = new Response<TransactionStatusResponse>();
            var result = new TransactionStatusResponse();
            // int langId = AppUtils.GetLangId(Request);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _thridPartyApiServices.GetTransactionStatus();
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
    }
}
