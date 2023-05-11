using ezeePay.Utility.CommonClass;
using Ezipay.Api.Filters;
using Ezipay.Service;
using Ezipay.ViewModel;
using Ezipay.ViewModel.common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Ezipay.Api.Controllers.Web
{
    /// <summary>
    /// AppDownloadLog
    /// </summary>
    [RoutePrefix("api/AppDownloadLog")]
    public class AppDownloadLogController : ApiController
    {
        private IHttpActionResult _iHttpActionResult;
        private Converter _converter;
        private readonly IAppDownloadLogService _appDownloadLogService;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="appDownloadLogService"></param>
        public AppDownloadLogController(IAppDownloadLogService appDownloadLogService)
        {
            _converter = new Converter();
            _appDownloadLogService = appDownloadLogService;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("InsertLog")]
        [TempSessionAuthorization]
        [TempTokenExceptionFilter]
        [ResponseType(typeof(Response<Object>))]
        public async Task<IHttpActionResult> InsertLog(RequestModel model)
        {
            var response = new Response<Object>();
            var result = new Object();

            try
            {
                var request = new EncrDecr<AppDownloadLogRequest>().Decrypt(model.Value, true, Request);
                result = await _appDownloadLogService.InsertLog(request);

                if ((int)result != 0)
                {
                    response = response.Create(true, ResponseMessages.SUCCESS, HttpStatusCode.OK, result);
                }
                else
                {
                    response = response.Create(true, ResponseMessages.REQUEST_SENT_FAILD, HttpStatusCode.OK, result);
                }
            }
            catch (Exception ex)
            {
                response = response.Create(false, ResponseMessages.REQUEST_SENT_FAILD, HttpStatusCode.OK, result);
                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.NoContent, false, false);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, true, Request);
            return _iHttpActionResult;
        }

        /// <summary>
        /// GetDownloadLogList
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetDownloadLogList")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<AppDownloadSearchResponse>))]
        public async Task<IHttpActionResult> GetDownloadLogList(RequestModel model)
        {
            var response = new Response<AppDownloadSearchResponse>();
            var result = new AppDownloadSearchResponse();

            try
            {
                var request = new EncrDecr<AppDownloadSearchVM>().Decrypt(model.Value);
                result = await _appDownloadLogService.GetDownloadLogList(request);

                if (result != null)
                {
                    response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                }
                else
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NoContent, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.NoContent);
                }
            }
            catch (Exception ex)
            {
                response = response.Create(false, ResponseMessages.EXCEPTION_OCCURED, HttpStatusCode.NoContent, result);
                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.NoContent);
            }
            return _iHttpActionResult;
        }


        /// <summary>
        /// GetDownloadLogList
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SendNotification")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<Object>))]
        public async Task<IHttpActionResult> SendNotification(RequestModel model)
        {
            var response = new Response<Object>();
            var result = new Object();

            try
            {
                var request = new EncrDecr<SendNotificationRequest>().Decrypt(model.Value);
                result = await _appDownloadLogService.SendNotification(request);

                if (result != null)
                {
                    response = response.Create(true, ResponseMessages.NOTIFICATIONSENT, HttpStatusCode.OK, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                }
                else
                {
                    response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_ERROR, HttpStatusCode.NoContent, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.NoContent);
                }
            }
            catch (Exception ex)
            {
                response = response.Create(false, ResponseMessages.EXCEPTION_OCCURED, HttpStatusCode.NoContent, result);
                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.NoContent);
            }
            return _iHttpActionResult;
        }


        /// <summary>
        /// GetActiveUserForNotification
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetActiveUserForNotification")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<AppDownloadSearchResponse>))]
        public async Task<IHttpActionResult> GetActiveUserForNotification(RequestModel model)
        {
            var response = new Response<AppDownloadSearchResponse>();
            var result = new AppDownloadSearchResponse();

            try
            {
                var request = new EncrDecr<AppDownloadSearchVM>().Decrypt(model.Value);
                result = await _appDownloadLogService.GetActiveUserForNotification(request);

                if (result != null)
                {
                    response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                }
                else
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NoContent, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.NoContent);
                }
            }
            catch (Exception ex)
            {
                response = response.Create(false, ResponseMessages.EXCEPTION_OCCURED, HttpStatusCode.NoContent, result);
                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.NoContent);
            }
            return _iHttpActionResult;
        }

        /// <summary>
        /// SendNotificationForActiveUser
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SendNotificationForActiveUser")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<Object>))]
        public async Task<IHttpActionResult> SendNotificationForActiveUser(RequestModel model)
        {
            var response = new Response<Object>();
            var result = new Object();

            try
            {
                var request = new EncrDecr<SendNotificationRequest>().Decrypt(model.Value);
                result = await _appDownloadLogService.SendNotificationForActiveUser(request);

                if (result != null)
                {
                    response = response.Create(true, ResponseMessages.NOTIFICATIONSENT, HttpStatusCode.OK, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                }
                else
                {
                    response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_ERROR, HttpStatusCode.NoContent, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.NoContent);
                }
            }
            catch (Exception ex)
            {
                response = response.Create(false, ResponseMessages.EXCEPTION_OCCURED, HttpStatusCode.NoContent, result);
                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.NoContent);
            }
            return _iHttpActionResult;
        }


        [HttpPost]
        [Route("GetCurrentWebNotification")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<List<SendNotificationResponse>>))]
        public async Task<IHttpActionResult> GetCurrentWebNotification()
        {
            var response = new Response<List<SendNotificationResponse>>();
            var result = new List<SendNotificationResponse>();
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _appDownloadLogService.GetCurrentWebNotification();

                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                        // _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
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
                // _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }
                     


        [HttpPost]
        [Route("GetCountCurrentWebNotification")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<List<CountNotificationRequest>>))]
        public async Task<IHttpActionResult> GetCountCurrentWebNotification(RequestModel model)
        {
            var response = new Response<List<CountNotificationRequest>>();
            var result = new List<CountNotificationRequest>();

            try
            {
                
                var request = new EncrDecr<CountNotificationRequest>().Decrypt(model.Value);
                result = await _appDownloadLogService.GetCountCurrentWebNotification(request);

                if (result != null)
                {
                    response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                }
                else
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NoContent, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.NoContent);
                }
            }
            catch (Exception ex)
            {
                response = response.Create(false, ResponseMessages.EXCEPTION_OCCURED, HttpStatusCode.NoContent, result);
                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.NoContent);
            }
            return _iHttpActionResult;
        }


        [HttpPost]
        [Route("UpdateCurrentWebNotification")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<Object>))]
        public async Task<IHttpActionResult> UpdateCurrentWebNotification(RequestModel model)
        {
            var response = new Response<Object>();
            var result = new Object();

            try
            {
                var request = new EncrDecr<notificationupdateRequest>().Decrypt(model.Value);
                result = await _appDownloadLogService.UpdateCurrentWebNotification(request);

                if (result != null)
                {
                    response = response.Create(true, ResponseMessages.NOTIFICATIONSENT, HttpStatusCode.OK, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                }
                else
                {
                    response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_ERROR, HttpStatusCode.NoContent, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.NoContent);
                }
            }
            catch (Exception ex)
            {
                response = response.Create(false, ResponseMessages.EXCEPTION_OCCURED, HttpStatusCode.NoContent, result);
                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.NoContent);
            }
            return _iHttpActionResult;
        }


    }

}
