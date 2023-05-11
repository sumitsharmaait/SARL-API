using ezeePay.Utility.CommonClass;
using Ezipay.Api.Filters;
using Ezipay.Database;
using Ezipay.Service.CommonService;
using Ezipay.Service.TransferTobankService;
using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.ViewModel.AirtimeViewModel;
using Ezipay.ViewModel.common;

using Ezipay.ViewModel.TransferToBankViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Ezipay.Api.Controllers
{
    [RoutePrefix("api/TransferTobankController")]  
    [TransactionsAllowed]
    public class TransferTobankController : ApiController
    {
        private ITransferToBankServices _transferToBankServices;
        private IHttpActionResult _iHttpActionResult;
        private ICommonServices _commonServices;
        private Converter _converter;

        /// <summary>
        /// TransferTobankController
        /// </summary>
        /// <param name="transferToBankServices"></param>
        public TransferTobankController(ITransferToBankServices transferToBankServices, ICommonServices commonServices)
        {
            _transferToBankServices = transferToBankServices;
            _converter = new Converter();
            _commonServices = commonServices;
        }

        /// <summary>
        /// IsdCodes
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [ResponseType(typeof(Response<List<IsdCodesResponse1>>))]
        [Route("GetTransferttobankCountryList")]
        public async Task<IHttpActionResult> GetTransferttobankCountryList()
        {
            var response = new Response<List<IsdCodesResponse1>>();
            var result = new List<IsdCodesResponse1>();
            // int langId = AppUtils.GetLangId(Request);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _transferToBankServices.GetTransferttobankCountryList();
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
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, false, false);
            return _iHttpActionResult;
        }




        /// <summary>
        /// GetBankList
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<BankListList>))]
        [Route("GetBankList")]
        public async Task<IHttpActionResult> GetBankList()
        {
            var response = new Response<List<BankListList>>();
            var result = new List<BankListList>();
            // int langId = AppUtils.GetLangId(Request);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _transferToBankServices.GetBankList();
                    if (result.Count > 0)
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
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }


     
        /// <summary>
        /// PayMoneyTransferToBank
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PayMoneyTransferToBank")]
        [ResponseType(typeof(Response<TransferFundResponse>))]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        public async Task<IHttpActionResult> PayMoneyTransferToBank(RequestModel request)
        {
            var requestModel = new EncrDecr<PayMoneyAggregatoryRequest>().Decrypt(request.Value, false, Request);
            var result = new AddMoneyAggregatorResponse();
            var response = new Response<AddMoneyAggregatorResponse>();
            // var requestModel = new EncrDecr<TransferFundRequest>().Decrypt(request.Value, false, Request);
           // var response = new Response<TransferFundResponse>();
           // var result = new TransferFundResponse();
            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();
            //int langId = AppUtils.GetLangId(Request);
            bool IsCorrectPassword = false;
            if (requestModel.Amount != null)
            {
                try
                {
                    if (requestModel == null)
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
                            if (!string.IsNullOrEmpty(requestModel.Amount) && !requestModel.Amount.IsZero() && requestModel.Amount.IsTwoDigitDecimal())
                            {
                                result = await _transferToBankServices.PayMoneyTransferToBank(requestModel);
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
                                    case 20:
                                        response = response.Create(false, ResponseMessages.MobileNotVerify, HttpStatusCode.OK, result);
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
                    //response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.OK, result);
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
                response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.NotFound, result);

            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }


        /// <summary>
        /// GetBankList
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<BankListResponse>))]
        [Route("GetBankListForApp")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        public async Task<IHttpActionResult> GetBankListForApp()
        {
            var response = new Response<List<BankListList>>();
            var result = new List<BankListList>();
            // int langId = AppUtils.GetLangId(Request);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _transferToBankServices.GetBankList();
                    if (result.Count > 0)
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
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false);
            return _iHttpActionResult;
        }


        /// <summary>
        /// GetsenderidtypeList
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [ResponseType(typeof(Response<List<senderIdTypetbl>>))]
        [Route("GetsenderidtypeList")]
       
        public async Task<IHttpActionResult> GetsenderidtypeList()
        {
            var response = new Response<List<senderIdTypetbl>>();
            var result = new List<senderIdTypetbl>();
            // int langId = AppUtils.GetLangId(Request);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _transferToBankServices.GetsenderidtypeList();
                    if (result.Count > 0)
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
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, false, false, Request);
            return _iHttpActionResult;
        }

    }
}
