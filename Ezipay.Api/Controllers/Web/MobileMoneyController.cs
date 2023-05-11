using ezeePay.Utility.CommonClass;
using Ezipay.Api.Filters;
using Ezipay.Database;
using Ezipay.Service.Admin.TransactionLimitAU;
using Ezipay.Service.CommonService;
using Ezipay.Service.MobileMoneyService;
using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.AirtimeViewModel;
using Ezipay.ViewModel.common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Ezipay.Service.TransferTobankService;
using Ezipay.ViewModel.CardPaymentViewModel;
using System.Text.RegularExpressions;

namespace Ezipay.Api.Controllers
{
    [RoutePrefix("api/MobileMoneyController")]
    [SessionAuthorization]
    [SessionTokenExceptionFilter]
    [TransactionsAllowed]
    public class MobileMoneyController : ApiController
    {
        private IHttpActionResult _iHttpActionResult;
        private IMobileMoneyServices _mobileMoneyServices;
        private ICommonServices _commonServices;
        private Converter _converter;
        private ITransactionLimitAUService _transactionLimitAUService;
        private ITransferToBankServices _transferToBankServices;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mobileMoneyServices"></param>
        /// <param name="commonServices"></param>
        /// <param name="transactionLimitAUService"></param>
        public MobileMoneyController(ITransferToBankServices transferToBankServices, IMobileMoneyServices mobileMoneyServices, ICommonServices commonServices, ITransactionLimitAUService transactionLimitAUService)
        {
            _mobileMoneyServices = mobileMoneyServices;
            _converter = new Converter();
            _commonServices = commonServices;
            _transactionLimitAUService = transactionLimitAUService;
            _transferToBankServices = transferToBankServices;
        }

        /// <summary>
        /// MobileMobileService
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("MobileMobileService")]
        [ResponseType(typeof(Response<AddMoneyAggregatorResponse>))]
        public async Task<IHttpActionResult> MobileMobileService(RequestModel request)
        {

            var response = new Response<AddMoneyAggregatorResponse>();
            var result = new AddMoneyAggregatorResponse();
            var resultTL = new TransactionLimitAUResponse();

            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();////

            bool IsCorrectPassword = false;
            var requestModel = new EncrDecr<PayMoneyAggregatoryRequest>().Decrypt(request.Value, false, Request);
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
                        //
                        if (requestModel.Amount != null)
                        {
                            AdminMobileMoneyLimitRequest obj = new AdminMobileMoneyLimitRequest
                            {
                                MinimumAmount = requestModel.Amount.ToString(),
                                Service = requestModel.IsdCode
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
                            //except civ and benin countryy code
                            //if (requestModel.IsdCode == "+245" || requestModel.IsdCode == "+221" || requestModel.IsdCode == "+223" || requestModel.IsdCode == "+226" || requestModel.IsdCode == "+227" || requestModel.IsdCode == "+228")
                            //{
                            if (d2 == d3)
                            {
                            }
                            else if (d2 > d3)
                            {
                                response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
                                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
                                return _iHttpActionResult;
                            }
                            //}


                        }

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
                                result = await _mobileMoneyServices.MobileMoneyService(requestModel);
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
                                        response = response.Create(true, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.NotFound, result);

                                        break;
                                    case 10:
                                        response = response.Create(false, ResponseMessages.INSUFICIENT_BALANCE, HttpStatusCode.NotAcceptable, result);

                                        break;
                                    case 11:
                                        response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotFound, result);

                                        break;
                                    case 12:
                                        response = response.Create(false, result.Message, HttpStatusCode.NotFound, result);

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
                                        response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.NotFound, result);

                                        break;
                                    case 20:
                                        response = response.Create(false, ResponseMessages.INVALID_txnAmountREQUEST, HttpStatusCode.OK, result);
                                        break;
                                    case 21:
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
                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.ExpectationFailed, result);

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
        [Route("VerifyMobileMoneyLimit")]
        [ResponseType(typeof(Response<AdminMobileMoneyLimitResponse>))]
        public async Task<IHttpActionResult> VerifyMobileMoneyLimit(RequestModel request)
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



        [HttpPost]
        [Route("VerifySenderIdNumberExistorNot")]
        [ResponseType(typeof(Response<MobileMoneySenderDetail>))]
        public async Task<IHttpActionResult> VerifySenderIdNumberExistorNot(RequestModel request)
        {
            var response = new Response<MobileMoneySenderDetail>();    
            var result = new MobileMoneySenderDetail();
            var requestModel = new EncrDecr<MobileMoneySenderDetailrequest>().Decrypt(request.Value, false, Request);
            if (requestModel != null)
            {
                try
                {
                    result = await _mobileMoneyServices.VerifySenderIdNumberExistorNot(requestModel);  
                                                                    
                    if (result != null)                               
                    {
                        result.SenderAddress = result.SenderAddress.Trim();
                        result.SenderCity = result.SenderCity.Trim();
                        result.SenderDateofbirth = null;
                        result.ReceiverFirstName = string.Empty;
                        result.ReceiverLastName = string.Empty;

                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.OK, result);
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


        /// <summary>
        /// GetsenderidtypeList
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [ResponseType(typeof(Response<List<senderIdTypetbl>>))]
        [Route("GetMobsenderidtypeList")]
        public async Task<IHttpActionResult> GetMobsenderidtypeList()
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



        //peysevrice :- 

        [HttpPost]
        [Route("PayBankTransferServiceForNGNbankflutter")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<AddMoneyAggregatorResponse>))]
        public async Task<IHttpActionResult> PayBankTransferServiceForNGNbankflutter(RequestModel request)
        {
            var response = new Response<AddMoneyAggregatorResponse>();
            var result = new AddMoneyAggregatorResponse();
            var requestModel = new EncrDecr<ThirdpartyPaymentByCardRequest>().Decrypt(request.Value, false, Request);
            //"MobileMoneyController".ErrorLog("MobileMoneyController.cs", "PayBankTransferServiceForNGNbankflutter", requestModel);
            var headerToken = Request.Headers.GetValues("token").FirstOrDefault();

            if (requestModel.Amount != null)
            {
                string str = requestModel.accountNo; //

                if (!Regex.IsMatch(str, "^[0-9]+$"))
                {
                    response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
                    return _iHttpActionResult;
                }
                else if (string.IsNullOrEmpty(requestModel.ngnbank) && string.IsNullOrEmpty(requestModel.accountNo))
                {
                    response = response.Create(false, ResponseMessages.INVALID_REQUEST, HttpStatusCode.NotFound, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
                    return _iHttpActionResult;
                }

                //checkin MaximumAmount 
                AdminMobileMoneyLimitRequest obj = new AdminMobileMoneyLimitRequest
                {
                    MinimumAmount = requestModel.Amount.ToString(),
                    Service = "003"
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
                            result = await _mobileMoneyServices.PayBankTransferServiceForNGNbankflutter(requestModel, headerToken);
                            switch (result.RstKey)
                            {
                                case 0:
                                    response = response.Create(false, ResponseMessages.INVALID_PASSWORD, HttpStatusCode.NotFound, result);
                                    break;
                                case 1:
                                    response = response.Create(true, AggregatoryMESSAGE.SUCCESSFUL, HttpStatusCode.OK, result);
                                    break;
                                case 2:
                                    response = response.Create(false, AggregatoryMESSAGE.PENDING, HttpStatusCode.OK, result);
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


        //ghana 
        /// <summary>
        /// GhanaMobileMobileService
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GhanaMobileMobileService")]
        [ResponseType(typeof(Response<AddMoneyAggregatorResponse>))]
        public async Task<IHttpActionResult> GhanaMobileMobileService(RequestModel request)
        {
            var response = new Response<AddMoneyAggregatorResponse>();
            var result = new AddMoneyAggregatorResponse();

            var resultTL = new TransactionLimitAUResponse();
            bool IsCorrectPassword = false;
            var requestModel = new EncrDecr<PayMoneyAggregatoryRequest>().Decrypt(request.Value, false, Request);
            var headerToken = Request.Headers.GetValues("token").FirstOrDefault();
            if (requestModel.Amount != null)
            {
                try
                {
                    //
                    if (requestModel.Amount != null)
                    {
                        AdminMobileMoneyLimitRequest obj = new AdminMobileMoneyLimitRequest
                        {
                            MinimumAmount = requestModel.Amount.ToString(),
                            Service = requestModel.IsdCode
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

                    }

                    if (request == null)
                    {
                        response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotAcceptable, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(requestModel.Password))
                        {
                            IsCorrectPassword = await _commonServices.CheckPassword(requestModel.Password, headerToken);
                        }
                        else
                        {
                            IsCorrectPassword = true;
                        }
                        if (IsCorrectPassword)
                        {
                            if (!string.IsNullOrEmpty(requestModel.Amount) && !requestModel.Amount.IsZero() && requestModel.Amount.IsTwoDigitDecimal())
                            {
                                result = await _mobileMoneyServices.GhanaMobileMobileService(requestModel);
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
                                        response = response.Create(true, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.NotFound, result);

                                        break;
                                    case 10:
                                        response = response.Create(false, ResponseMessages.INSUFICIENT_BALANCE, HttpStatusCode.NotAcceptable, result);

                                        break;
                                    case 11:
                                        response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.NotFound, result);

                                        break;
                                    case 12:
                                        response = response.Create(false, result.Message, HttpStatusCode.NotFound, result);

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
                                        response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.NotFound, result);

                                        break;
                                    case 20:
                                        response = response.Create(false, ResponseMessages.INVALID_txnAmountREQUEST, HttpStatusCode.OK, result);
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
                            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false);
                            return _iHttpActionResult;
                        }
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.ExpectationFailed, result);

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

    }
}
