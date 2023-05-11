using ezeePay.Utility.CommonClass;
using Ezipay.Api.Filters;
using Ezipay.Service.Admin.TransactionLog;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.CheckPasswordViewModel;
using Ezipay.ViewModel.common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Ezipay.Api.Controllers.Admin
{
    /// <summary>
    /// Transaction
    /// </summary>
    [RoutePrefix("api/admin")]
    [SessionAuthorization]
    [SessionTokenExceptionFilter]
    public class TransactionLogController : ApiController
    {
        private IHttpActionResult _iHttpActionResult;
        private ITransactionLogService _transactionLogService;
        private Converter _converter;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="transactionLogService"></param>
        public TransactionLogController(ITransactionLogService transactionLogService)
        {
            _transactionLogService = transactionLogService;
            _converter = new Converter();
        }

        /// <summary>
        /// GetMerchantList
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<TransactionLogsResponse>))]
        [Route("GetTransactionLogs")]
        public async Task<IHttpActionResult> GetTransactionLogs(RequestModel request)
        {
            var response = new Response<TransactionLogResponse>();
            var result = new TransactionLogResponse();
            if (ModelState.IsValid)
            {
                try
                {
                    var requestModel = new EncrDecr<TransactionLogRequest>().Decrypt(request.Value, false, Request);
                    result = await _transactionLogService.GetTransactionLogs(requestModel);
                    if (result.TransactionLogs != null)
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
        /// GetNewTransactionLogs
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<TransactionLogsResponse>))]
        [Route("GetNewTransactionLogs")]
        //public async Task<IHttpActionResult> GetNewTransactionLogs(RequestModel request)
        public async Task<IHttpActionResult> GetNewTransactionLogs(TransactionLogsRequest request)
        {
            var response = new Response<TransactionLogsResponse>();
            var result = new TransactionLogsResponse();
            if (ModelState.IsValid)
            {
                try
                {
                    //var requestModel = new EncrDecr<TransactionLogsRequest>().Decrypt(request.Value, false, Request);
                    //result = await _transactionLogService.GetNewTransactionLogs(requestModel);
                    result = await _transactionLogService.GetNewTransactionLogs(request);
                    if (result.TransactionLogslist != null)
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
            _iHttpActionResult = _converter.LogApiResponseMessage(response, HttpStatusCode.OK, false, false, Request);
            return _iHttpActionResult;
        }

        /// <summary>
        /// exportReport
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ExportReport")]
        [Description("ExportReport")]
        public async Task<HttpResponseMessage> ExportReport(DownloadLogReportRequest request)
        {
            // int langId = AppUtils.GetLangId(Request);
            string filename = "EzipayLog";
            MemoryStream memoryStream = null;
            memoryStream = await _transactionLogService.GenerateLogReport(request);
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
            //response.Content.Headers.ContentLength = stream.Length;
            memoryStream.WriteTo(memoryStream);
            memoryStream.Close();
            return response;
        }



        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<List<CardtxndetailsResponse>>))]
        [Route("Getcardtxndetails")]
        public async Task<IHttpActionResult> Getcardtxndetails(RequestModel requestModel)
        {
            var response = new Response<List<CardtxndetailsResponse>>();
            var result = new List<CardtxndetailsResponse>();
            if (ModelState.IsValid)
            {
                try
                {
                    // result = await _transactionLogService.Getcardtxndetails();

                    var request = new EncrDecr<CardtxndetailsRequest>().Decrypt(requestModel.Value, false, Request);
                    result = await _transactionLogService.Getcardtxndetails(request);


                    if (result != null)
                    {

                        response = response.Create(true, AdminResponseMessages.DATA_FOUND, HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, AdminResponseMessages.DATA_NOT_FOUND, HttpStatusCode.NotAcceptable, result);

                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, AdminResponseMessages.DATA_NOT_FOUND, HttpStatusCode.NotAcceptable, result);
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
                // _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;


        }



        [HttpPost]
        [Route("MonthlyreportExportReport")]
        [Description("MonthlyreportExportReport")]
        public async Task<HttpResponseMessage> MonthlyreportExportReport(DownloadLogReportRequest1 request)
        {
            // int langId = AppUtils.GetLangId(Request);
            string filename = "EzipayLog";
            MemoryStream memoryStream = null;
            memoryStream = await _transactionLogService.GenerateLogReport1(request);
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
            //response.Content.Headers.ContentLength = stream.Length;
            memoryStream.WriteTo(memoryStream);
            memoryStream.Close();
            return response;
        }

        //flutter

        //flutter pop invoice one - by-one

        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<List<WalletTxnResponse>>))]
        [Route("FlutterCheckTxnNotCaptureOurSide")]
        public async Task<IHttpActionResult> FlutterCheckTxnNotCaptureOurSide(RequestModel requestModel)
        {
            var response = new Response<List<WalletTxnResponse>>();
            var result = new List<WalletTxnResponse>();
            var request = new EncrDecr<WalletTxnResponse>().Decrypt(requestModel.Value, false, Request);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _transactionLogService.FlutterCheckTxnNotCaptureOurSide(request.InvoiceNo);
                    if (result != null)
                    {
                        response = response.Create(true, AdminResponseMessages.DATA_FOUND, HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, AdminResponseMessages.DATA_NOT_FOUND, HttpStatusCode.NotAcceptable, result);
                        // _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, AdminResponseMessages.DATA_NOT_FOUND, HttpStatusCode.NotAcceptable, result);
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
                // _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }



        [AcceptVerbs("POST")]
        [Route("UpdateFlutterCheckTxnNotCaptureOurSide")]
        [ResponseType(typeof(Response<Object>))]
        public async Task<IHttpActionResult> UpdateFlutterCheckTxnNotCaptureOurSide(RequestModel requestModel)
        {
            var response = new Response<Object>();
            var result = new Object();
            var request = new EncrDecr<WalletTxnRequest>().Decrypt(requestModel.Value, false, Request);
            if (ModelState.IsValid)
            {
                try
                {
                    if (request == null)
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                        return _iHttpActionResult;
                    }
                    result = await _transactionLogService.UpdateFlutterCheckTxnNotCaptureOurSide(request);
                    if ((bool)result)
                    {
                        response = response.Create(true, AdminResponseMessages.DATA_SAVED, HttpStatusCode.OK, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                    }
                    else
                    {
                        response = response.Create(false, AdminResponseMessages.DATA_NOT_SAVED, HttpStatusCode.NotAcceptable, result);
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
        [Route("UpdateFlutterCheckTxn")]
        [ResponseType(typeof(Response<List<Fluttertxnresponse>>))]
        public async Task<IHttpActionResult> UpdateFlutterCheckTxn()
        {
            var response = new Response<List<Fluttertxnresponse>>();
            var result = new List<Fluttertxnresponse>();
          
            if (ModelState.IsValid)
            {
                try
                {
                    //if (request == null)
                    //{
                    //    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    //    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                    //    return _iHttpActionResult;
                    //}
                    result = await _transactionLogService.UpdateFlutterCheckTxn();


                    if (result != null)
                    {

                        response = response.Create(true, AdminResponseMessages.DATA_FOUND, HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, AdminResponseMessages.DATA_NOT_FOUND, HttpStatusCode.NotAcceptable, result);

                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, AdminResponseMessages.DATA_NOT_FOUND, HttpStatusCode.NotAcceptable, result);
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
                // _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;


        }


        [HttpPost]
        [Route("ExportReportinfo")]
        [Description("ExportReportinfo")]
        public async Task<HttpResponseMessage> ExportReportinfo()
        {
            // int langId = AppUtils.GetLangId(Request);
            string filename = "EzipayLog";
            MemoryStream memoryStream = null;
            memoryStream = await _transactionLogService.GenerateLogReportInfo();
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
            //response.Content.Headers.ContentLength = stream.Length;
            memoryStream.WriteTo(memoryStream);
            memoryStream.Close();
            return response;
        }
    }
}
