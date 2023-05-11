using ezeePay.Utility.CommonClass;
using Ezipay.Api.Filters;
using Ezipay.Service.CommonService;
using Ezipay.Service.MerchantPayment;
using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.ViewModel.common;
using Ezipay.ViewModel.MerchantPaymentViewModel;
using Ezipay.ViewModel.PayMoneyViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Ezipay.Api.Controllers
{
    [RoutePrefix("api/MerchantPaymentController")]
    [TransactionsAllowed]
    public class MerchantPaymentController : ApiController
    {
        private IMerchantPaymentService _merchantPaymentService;
        private IHttpActionResult _iHttpActionResult;
        private ICommonServices _commonServices;
        private Converter _converter;

        public MerchantPaymentController(IMerchantPaymentService merchantPaymentService, ICommonServices commonServices)
        {
            _merchantPaymentService = merchantPaymentService;
            _converter = new Converter();
            _commonServices = commonServices;
        }

        /// <summary>
        /// MerchantPayment
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("MerchantPayment")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<WalletTransactionResponse>))]
        public async Task<IHttpActionResult> MerchantPayment(RequestModel request)
        {
            var requestModel = new EncrDecr<MerchantTransactionRequest>().Decrypt(request.Value, false, Request);
            var response = new Response<WalletTransactionResponse>();
            var result = new WalletTransactionResponse();
            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();////
            //int langId = AppUtils.GetLangId(Request);
            bool IsCorrectPassword = false;
            if (ModelState.IsValid)
            {
                try
                {
                    if (request == null)
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
                            if (!string.IsNullOrEmpty(requestModel.Amount) && !requestModel.Amount.IsZero() && requestModel.Amount.IsTwoDigitDecimal())
                            {
                                result = await _merchantPaymentService.MerchantPayment(requestModel, sessionToken);////
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
                                        response = response.Create(true, result.Message, HttpStatusCode.OK, result);

                                        break;
                                    case 7:
                                        response = response.Create(false, ResponseMessages.TRANSACTION_ERROR, HttpStatusCode.OK, result);

                                        break;
                                    case 8:
                                        response = response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.OK, result);

                                        break;
                                    case 9:
                                        response = response.Create(true, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.OK, result);

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
                                        if (requestModel.MerchantId < 0)
                                        {
                                            errorList.Add(new Errorkey { Key = "MerchantId", Val = "Merchant Id is required" });
                                        }
                                        response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.OK, result);

                                        break;
                                    case 19:
                                        response = response.Create(true, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.OK, result);

                                        break;
                                    case 20:
                                        response = response.Create(false, result.Message, HttpStatusCode.OK, result);

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


        [HttpPost]
        [Route("MerchantPaymentEzipayPartner")]
        [ResponseType(typeof(Response<WalletTransactionResponse>))]
        public async Task<IHttpActionResult> MerchantPaymentEzipayPartner(MerchantTransactionForThirdPartyRequest requestModel)
        {
            var response = new Response<WalletTransactionResponse>();
            var result = new WalletTransactionResponse();

            if (ModelState.IsValid)
            {
                try
                {
                    if (requestModel == null)
                    {
                        response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.OK, result);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(requestModel.amount) && !requestModel.amount.IsZero() && requestModel.amount.IsTwoDigitDecimal())
                        {
                            result = await _merchantPaymentService.MerchantPaymentEzipayPartner(requestModel);
                            switch (result.RstKey)
                            {
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
                                    response = response.Create(true, result.Message, HttpStatusCode.OK, result);

                                    break;
                                case 7:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_ERROR, HttpStatusCode.OK, result);

                                    break;
                                case 8:
                                    response = response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.OK, result);

                                    break;
                                case 9:
                                    response = response.Create(true, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.OK, result);

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
                                    if (requestModel.merchantId ==null)
                                    {
                                        errorList.Add(new Errorkey { Key = "MerchantId", Val = "Merchant Id is required" });
                                    }
                                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.OK, result);

                                    break;
                                case 19:
                                    response = response.Create(true, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.OK, result);

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
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, false, false, Request);
            return _iHttpActionResult;


        }
    }
}
