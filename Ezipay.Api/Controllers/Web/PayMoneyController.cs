using ezeePay.Utility.CommonClass;
using Ezipay.Api.Filters;
using Ezipay.Service.CommonService;
using Ezipay.Service.PayMoney;
using Ezipay.Service.TokenService;
using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.ViewModel.CheckPasswordViewModel;
using Ezipay.ViewModel.common;
using Ezipay.ViewModel.PayMoneyViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using static Ezipay.Utility.common.AppSetting;

namespace Ezipay.Api.Controllers
{
    [RoutePrefix("api/PayMoneyController")]
    [TransactionsAllowed]
    public class PayMoneyController : ApiController
    {
        private IPayMoneyService _payMoneyService;
        private IHttpActionResult _iHttpActionResult;
        private ICommonServices _commonServices;
        private ITokenService _tokenService;
        private Converter _converter;

        public PayMoneyController(IPayMoneyService payMoneyService, ICommonServices commonServices, ITokenService tokenService)
        {
            _payMoneyService = payMoneyService;
            _converter = new Converter();
            _commonServices = commonServices;
            _tokenService = tokenService;
        }

        /// <summary>
        /// PayMoney
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PayMoney")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<WalletTransactionResponse>))]
        public async Task<IHttpActionResult> PayMoney(RequestModel request)
        {

            var requestModel = new EncrDecr<WalletTransactionRequest>().Decrypt(request.Value, false, Request);
          
            var response = new Response<WalletTransactionResponse>();
            var result = new WalletTransactionResponse();
            //int langId = AppUtils.GetLangId(Request);
            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();////
            var passwordResponse = new CheckLoginResponse();
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
                                result = await _payMoneyService.PayMoney(requestModel, sessionToken);
                                switch (result.RstKey)
                                {
                                    case 0:
                                        response = response.Create(false, ResponseMessages.INVALID_PASSWORD, HttpStatusCode.NotFound, result);

                                        break;
                                    case 1:
                                        response = response.Create(true, AggregatoryMESSAGE.SUCCESSFUL, HttpStatusCode.OK, result);
                                        break;
                                    case 2:
                                        response = response.Create(false, AggregatoryMESSAGE.PENDING, HttpStatusCode.NotFound, result);
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
                                        response = response.Create(false, ResponseMessages.SELF_WALLET, HttpStatusCode.NotFound, result);
                                        break;
                                    case 10:
                                        response = response.Create(false, ResponseMessages.RECEIVER_NOT_EXIST, HttpStatusCode.NotFound, result);
                                        break;
                                    case 11:
                                        response = response.Create(false, ResponseMessages.SENDER_NOT_EXIST, HttpStatusCode.NotFound, result);

                                        break;
                                    case 12:
                                        response = response.Create(false, ResponseMessageKyc.TRANSACTION_LIMIT, HttpStatusCode.NotFound, result);
                                        break;
                                    case 13:
                                        response = response.Create(false, ResponseMessageKyc.FAILED_Doc_NotUploaded, HttpStatusCode.NotFound, result);
                                        break;
                                    case 14:
                                        response = response.Create(false, ResponseMessageKyc.FAILED_Doc_Pending, HttpStatusCode.NotFound, result);
                                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.NotFound, true, false, Request);
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
                                        if (string.IsNullOrWhiteSpace(requestModel.IsdCode))
                                        {
                                            errorList.Add(new Errorkey { Key = "CountryCode", Val = "CountryCode is required" });
                                        }
                                        if (string.IsNullOrWhiteSpace(requestModel.MobileNo))
                                        {
                                            errorList.Add(new Errorkey { Key = "PhoneNumber", Val = "PhoneNumber is required" });
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
            return _iHttpActionResult;//AES256.Encrypt(keys.PublicKey, JsonConvert.SerializeObject(_iHttpActionResult));//_iHttpActionResult;
        }
    }
}
