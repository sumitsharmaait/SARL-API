using ezeePay.Utility.CommonClass;
using ezeePay.Utility.Enums;
using Ezipay.Api.Filters;
using Ezipay.Service.CommonService;
using Ezipay.Service.PaymentRequestService;
using Ezipay.Service.UserService;
using Ezipay.Utility.common;
using Ezipay.Utility.ExcelGenerate;
using Ezipay.Utility.Extention;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.common;
using Ezipay.ViewModel.PayMoneyViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Ezipay.Api.Controllers
{
    [RoutePrefix("api/PaymentRequestController")]
    [TransactionsAllowed]
    public class PaymentRequestController : ApiController
    {
        private IHttpActionResult _iHttpActionResult;
        private IWalletUserService _walletUserService;
        private ICommonServices _commonServices;
        private Converter _converter;
        private IPaymentRequestServices _paymentRequestServices;


        public PaymentRequestController(IPaymentRequestServices paymentRequestServices, ICommonServices commonServices, IWalletUserService walletUserService)
        {
            _paymentRequestServices = paymentRequestServices;
            _converter = new Converter();
            _commonServices = commonServices;
            _walletUserService = walletUserService;

        }

        /// <summary>
        /// MakePaymentRequest
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("MakePaymentRequest")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<WalletTransactionResponse>))]
        public async Task<IHttpActionResult> MakePaymentRequest(RequestModel request)
        {

            var response = new Response<WalletTransactionResponse>();
            var result = new WalletTransactionResponse();
            //int langId = AppUtils.GetLangId(Request);
            //  var passwordResponse = new CheckLoginResponse();
            bool IsCorrectPassword = false;
            var requestModel = new EncrDecr<WalletTransactionRequest>().Decrypt(request.Value, false, Request);
            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();////
            if (ModelState.IsValid)
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
                            if (!string.IsNullOrEmpty(requestModel.Amount) && !requestModel.Amount.IsZero() && requestModel.Amount.IsTwoDigitDecimal())
                            {
                                result = await _paymentRequestServices.PaymentRequest(requestModel, sessionToken);
                                switch (result.RstKey)
                                {
                                    case 0:
                                        response = response.Create(false, ResponseMessages.INVALID_PASSWORD, HttpStatusCode.OK, result);
                                        break;
                                    case 1:
                                        response = response.Create(true, ResponseMessages.PAY_MONEY_REQUEST_SUCCESS, HttpStatusCode.OK, result);

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
                                        response = response.Create(true, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.OK, result);
                                        break;
                                    case 10:
                                        response = response.Create(false, ResponseMessages.RECEIVER_NOT_EXIST, HttpStatusCode.NotAcceptable, result);
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
                                        response = response.Create(true, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.OK, result);
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
                            response = response.Create(true, ResponseMessages.INVALID_PASSWORD, HttpStatusCode.NotFound, result);
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
        /// ViewPaymentRequests
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ViewPaymentRequests")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<ViewPaymentResponse>))]
        public async Task<IHttpActionResult> ViewPaymentRequests(RequestModel request)
        {

            var response = new Response<ViewPaymentResponse>();
            var result = new ViewPaymentResponse();
            var requestModel = new EncrDecr<ViewPaymentRequest>().Decrypt(request.Value, false, Request);
            //int langId = AppUtils.GetLangId(Request);
            //  var passwordResponse = new CheckLoginResponse();
            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();////
            if (ModelState.IsValid)
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

                        result = await _paymentRequestServices.ViewPaymentRequests(requestModel, sessionToken);
                        switch (result.RstKey)
                        {
                            case 1:
                                response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);

                                break;
                            case 2:
                                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.OK, result);
                                break;
                            case 3:
                                response = response.Create(false, AggregatoryMESSAGE.FAILED, HttpStatusCode.OK, result);
                                break;
                            case 4:
                                response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_ERROR, HttpStatusCode.OK, result);
                                break;
                            case 18:
                                var errorList = new List<Errorkey>();
                                if (string.IsNullOrWhiteSpace(requestModel.Password))
                                {
                                    errorList.Add(new Errorkey { Key = "Password", Val = "Password can not be null" });
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
        /// ManagePaymentRequest
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ManagePaymentRequest")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<WalletTransactionResponse>))]
        public async Task<IHttpActionResult> ManagePaymentRequest(RequestModel request)
        {
            var response = new Response<WalletTransactionResponse>();
            var result = new WalletTransactionResponse();
            //int langId = AppUtils.GetLangId(Request);
            bool IsCorrectPassword = false;
            var requestModel = new EncrDecr<ManagePayMoneyReqeust>().Decrypt(request.Value, false, Request);
            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();////
            if (ModelState.IsValid)
            {
                try
                {
                    if (requestModel == null)
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
                            result = await _paymentRequestServices.ManagePaymentRequest(requestModel, sessionToken);

                            switch (result.RstKey)
                            {
                                case 0:
                                    response = response.Create(false, ResponseMessages.INVALID_PASSWORD, HttpStatusCode.OK, result);
                                    break;
                                case 1:
                                    response = response.Create(true, AggregatoryMESSAGE.SUCCESSFUL, HttpStatusCode.OK, result);

                                    break;
                                case 2:
                                    response = response.Create(true, ResponseMessages.REJECTED, HttpStatusCode.OK, result);
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
                                    response = response.Create(true, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.OK, result);
                                    break;
                                case 10:
                                    response = response.Create(false, ResponseMessages.INSUFICIENT_BALANCE, HttpStatusCode.OK, result);
                                    break;
                                case 11:
                                    response = response.Create(false, ResponseMessages.USER_NOT_REGISTERED, HttpStatusCode.OK, result);
                                    break;
                                case 12:
                                    response = response.Create(false, ResponseMessageKyc.TRANSACTION_LIMIT, HttpStatusCode.OK, result);
                                    break;
                                case 13:
                                    response = response.Create(false, ResponseMessageKyc.FAILED_Doc_NotUploaded, HttpStatusCode.OK, result);
                                    break;
                                case 14:
                                    response = response.Create(false, ResponseMessageKyc.FAILED_Doc_Pending, HttpStatusCode.OK, result);
                                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
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
                                    if (requestModel.PayMoneyRequestId < 0)
                                    {
                                        errorList.Add(new Errorkey { Key = "PayMoneyRequestId", Val = "PayMoneyRequestId can not be null" });
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
                            response = response.Create(false, ResponseMessages.INVALID_PASSWORD, HttpStatusCode.NotFound, result);
                            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
                            return _iHttpActionResult;
                        }
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.OK, result);
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
                response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED, HttpStatusCode.OK, result);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }

        /// <summary>
        /// ViewTransactions
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ViewTransactions")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<ViewTransactionResponse>))]
        public async Task<IHttpActionResult> ViewTransactions(RequestModel request)
        {

            var response = new Response<ViewTransactionResponse>();
            var result = new ViewTransactionResponse();
            var requestModel = new EncrDecr<ViewTransactionRequest>().Decrypt(request.Value, false, Request);
            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();////
            if (ModelState.IsValid)
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
                        result = await _paymentRequestServices.ViewTransactions(requestModel, sessionToken);

                        switch (result.RstKey)
                        {
                            case 0:
                                response = response.Create(false, ResponseMessages.INVALID_PASSWORD, HttpStatusCode.NotFound, result);

                                break;
                            case 1:
                                response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);
                                break;
                            case 2:
                                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.OK, result);
                                break;
                            case 3:
                                response = response.Create(false, AggregatoryMESSAGE.FAILED, HttpStatusCode.OK, result);
                                break;
                            case 4:
                                response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_ERROR, HttpStatusCode.OK, result);
                                break;
                            default:
                                response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.OK, result);
                                break;
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
        /// TransactionStatement
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [Route("TransactionStatement")]
        public async Task<HttpResponseMessage> TransactionStatement(DeatailForDownloadReport requestModel)
        {
            // var requestModel = new EncrDecr<DeatailForDownloadReport>().Decrypt(request.Value, false);
            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();////
            string MediaType = MediaTypes.ApplicationPdf;
            if ((int)DownloadFileType.EXCEL == requestModel.DownloadType)
            {
                MediaType = MediaTypes.ApplicationMsExcel;
            }
            var result = new HttpResponseMessage(HttpStatusCode.OK);
            try
            {
                var DATA = await _paymentRequestServices.DownloadReport(new DownloadReportApiRequest { DateFrom = requestModel.DateFrom, DateTo = requestModel.DateTo, DownloadType = requestModel.DownloadType, TransactionType = requestModel.TransactionType, WalletUserId = requestModel.Code }, sessionToken);
                if (DATA.ReportData != null && DATA.ReportData != null && DATA.ReportData.Count > 0)
                {
                    if ((int)DownloadFileType.PDF == requestModel.DownloadType)
                    {
                        var UserDetail = await _walletUserService.GetUserDetailById(requestModel.Code);
                        using (MemoryStream memoryStream = new TransactionHelper().WritePdfForTransactionList(DATA.ReportData, UserDetail.UserName))
                        {
                            byte[] bytes = memoryStream.ToArray();
                            result.Content = new ByteArrayContent(bytes);
                            result.Content.Headers.ContentType = new MediaTypeHeaderValue(MediaType);
                            result.Content.Headers.Add("Content-Disposition", "attachment; filename=TransactionStatement.pdf");
                            memoryStream.Close();
                            return result;
                        }
                    }
                    else if ((int)DownloadFileType.EXCEL == requestModel.DownloadType)
                    {
                        using (MemoryStream memoryStream = new TransactionHelper().GenerateStreamFromString(DATA.ReportData))
                        {
                            string filename = "EzipayLog";
                            var response = new HttpResponseMessage
                            {
                                StatusCode = HttpStatusCode.OK,
                                Content = new ByteArrayContent(memoryStream.ToArray())
                            };
                            response.Content.Headers.ContentType = new MediaTypeHeaderValue
                                      ("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                            response.Content.Headers.ContentDisposition =
                                   new ContentDispositionHeaderValue("attachment")
                                   {
                                       FileName = $"{filename}_{DateTime.Now.Ticks.ToString()}.xls"
                                   };
                            memoryStream.WriteTo(memoryStream);
                            memoryStream.Close();

                            return response;
                        }
                    }
                    else
                    {
                        result = new HttpResponseMessage(HttpStatusCode.NotFound);
                        result.Content.Headers.ContentType = new MediaTypeHeaderValue(MediaType);
                        return result;
                    }
                }
                else
                {
                    result = new HttpResponseMessage(HttpStatusCode.NotFound);
                    result.Content.Headers.ContentType = new MediaTypeHeaderValue(MediaType);
                    return result;
                }
            }
            catch (Exception ex)
            {
                result = new HttpResponseMessage(HttpStatusCode.NotFound);
                result.Content.Headers.ContentType = new MediaTypeHeaderValue(MediaType);
                return result;
            }
        }

        /// <summary>
        /// DownloadReport
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [Route("DownloadReport")]
        [ResponseType(typeof(Response<DownloadReportResponse>))]
        public async Task<IHttpActionResult> DownloadReport(RequestModel request)
        {

            var response = new Response<DownloadReportResponse>();
            var _response = new DownloadReportResponse();
            var _request = new EncrDecr<DownloadReportApiRequest>().Decrypt(request.Value, false);
            //var keys = _IToken.KeysBySessionToken();
            //request.request = AES256.Decrypt(keys.PrivateKey, request.request);
            //DownloadReportRequest _request = JsonConvert.DeserializeObject<DownloadReportRequest>(request.request);
            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();////
            _response = await _paymentRequestServices.DownloadReportForApp(_request, sessionToken);


            var UserDetail = await _walletUserService.UserProfile(sessionToken);////
            var hostName = ConfigurationManager.AppSettings["hostName"].ToString();
            if (_response != null && _response.ReportData != null && _response.ReportData.Count > 0)
            {

                _response.Status = true;
                _response.FileUrl = hostName + "/api/PaymentRequestController/TransactionStatementForApp?DateFrom=" + _request.DateFrom.ToString("yyyy-MMM-dd") + "&DateTo=" + _request.DateTo.ToString("yyyy-MMM-dd") + "&DownloadType=" + _request.DownloadType.ToString() + "&TransactionType=" + _request.TransactionType.ToString() + "&Code=" + _response.WalletUserId.ToString() + "&name=" + UserDetail.FirstName + "-" + UserDetail.LastName;
                _response.WalletUserId = 0;
                response = response.Create(true, ResponseMessages.DATA_RECEIVED.Replace("Data", " ReportData"), HttpStatusCode.OK, _response);
            }
            else
            {
                _response.Status = false;
                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED.Replace("Data", "ReportData"), HttpStatusCode.NotFound, new DownloadReportResponse());
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false);
            return _iHttpActionResult;


        }

        /// <summary>
        /// TransactionStatementForApp
        /// </summary>
        /// <param name="DateFrom"></param>
        /// <param name="DateTo"></param>
        /// <param name="DownloadType"></param>
        /// <param name="TransactionType"></param>
        /// <param name="Code"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("TransactionStatementForApp")]
        public async Task<HttpResponseMessage> TransactionStatementForApp(string DateFrom, string DateTo, int DownloadType, int TransactionType, long Code, string name)
        {
            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();////

            string MediaType = MediaTypes.ApplicationPdf;
            if ((int)DownloadFileType.EXCEL == DownloadType)
            {
                MediaType = MediaTypes.ApplicationMsExcel;
            }
            DownloadReportResponse Data = new DownloadReportResponse();
            var result = new HttpResponseMessage(HttpStatusCode.OK);
            try
            {
                if ((string.IsNullOrEmpty(DateFrom) || DateFrom == "null") && (string.IsNullOrEmpty(DateTo) || DateTo == "null"))
                {
                    Data = await _paymentRequestServices.DownloadReportForApp(new DownloadReportApiRequest { DownloadType = DownloadType, TransactionType = TransactionType, WalletUserId = Code }, sessionToken);
                }
                else
                {
                    Data = await _paymentRequestServices.DownloadReportForApp(new DownloadReportApiRequest { DateFrom = Convert.ToDateTime(DateFrom), DateTo = Convert.ToDateTime(DateTo), DownloadType = DownloadType, TransactionType = TransactionType, WalletUserId = Code }, sessionToken);

                }
                if (Data != null && Data.ReportData != null && Data.ReportData.Count > 0)
                {

                    if ((int)DownloadFileType.PDF == DownloadType)
                    {

                        using (MemoryStream memoryStream = new TransactionHelper().WritePdfForTransactionList(Data.ReportData, name))
                        {

                            byte[] bytes = memoryStream.ToArray();
                            result.Content = new ByteArrayContent(bytes);
                            result.Content.Headers.ContentType = new MediaTypeHeaderValue(MediaType);
                            result.Content.Headers.Add("Content-Disposition", "attachment; filename=TransactionStatement.pdf");
                            result.Content.Headers.ContentLength = memoryStream.Length;
                            memoryStream.Close();
                            return result;
                        }
                    }
                    else if ((int)DownloadFileType.EXCEL == DownloadType)
                    {
                        using (MemoryStream memoryStream = new TransactionHelper().GenerateStreamFromString(Data.ReportData))
                        {

                            byte[] bytes = memoryStream.ToArray();
                            result.Content = new ByteArrayContent(bytes);
                            //result.Content.Headers.ContentType = new MediaTypeHeaderValue(MediaType);
                            result.Content.Headers.Add("Content-Disposition", "attachment; filename=TransactionStatement.xls");

                            //result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = "TransactionStatement.xls" };
                            result.Content.Headers.ContentType = new MediaTypeHeaderValue(MediaType);
                            result.Content.Headers.ContentLength = memoryStream.Length;
                            memoryStream.Close();
                            return result;
                        }
                    }
                    else
                    {

                        result = new HttpResponseMessage(HttpStatusCode.NotFound);
                        result.Content.Headers.ContentType = new MediaTypeHeaderValue(MediaType);
                        return result;
                    }

                }
                else
                {

                    result = new HttpResponseMessage(HttpStatusCode.NotFound);
                    result.Content.Headers.ContentType = new MediaTypeHeaderValue(MediaType);
                    return result;
                }
            }
            catch (Exception ex)
            {
                result = new HttpResponseMessage(HttpStatusCode.NotFound);
                result.Content.Headers.ContentType = new MediaTypeHeaderValue(MediaType);
                return result;

            }

        }



        /// <summary>
        /// TransactionStatementPerUser
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [Route("TransactionStatementPerUser")]
        public async Task<IHttpActionResult> TransactionStatementPerUser()
        {
            
            var response = new Response<UserTxnReportData>();
            try
            {
                //get all user in active list
                var Getactiveuserlist = await _paymentRequestServices.GetWalletUser();
                foreach (var item in Getactiveuserlist) //get one by one id 
                {
                    //get akll txn details of given user
                    var DATA = await _paymentRequestServices.Txndetailperuser(item.WalletUserId);
                   
                    //
                    if (DATA.UserTxnReportData != null && DATA.UserTxnReportData.Count > 0)
                    {   //WRITE & take memorystream OF PDF here 
                        using (MemoryStream memoryStream = new TransactionHelper().WritePdfForTransactionListPerUser(DATA.UserTxnReportData, item.WalletUserId))
                        {
                            memoryStream.Position = 0;     // read from the start of what was written                                                          
                            var DATA1 = await _paymentRequestServices.SendTxndetailperuser(DATA.UserTxnReportData, memoryStream,item.WalletUserId);
                            memoryStream.Close();
                        }

                    }

                }
                response = response.Create(true, ResponseMessages.SUCCESS, HttpStatusCode.OK, null);
                return _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false);
                 
            }
            catch (Exception ex)
            {
                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.OK, null);
                return _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false);

            }
        }


    }
}
