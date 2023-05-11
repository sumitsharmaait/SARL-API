using ezeePay.Utility.CommonClass;
using Ezipay.Api.Filters;
using Ezipay.Service.Admin.Callback;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Ezipay.Api.Controllers.Admin
{
    /// <summary>
    /// Callback Management
    /// </summary>
    [RoutePrefix("api/admin")]
    [SessionAuthorization]
    [SessionTokenExceptionFilter]
    public class CallbackController : ApiController
    {
        private IHttpActionResult _iHttpActionResult;
        private ICallbackService _callbackService;
        private Converter _converter;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="callbackService"></param>
        public CallbackController(ICallbackService callbackService)
        {
            _callbackService = callbackService;
            _converter = new Converter();
        }

        /// <summary>
        /// GetCallbackList
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<CallbackResponse>))]
        [Route("GetCallbackList")]
        public async Task<IHttpActionResult> GetCallbackList(RequestModel request)
        {
            var response = new Response<CallbackResponse>();
            var result = new CallbackResponse();
            if (ModelState.IsValid)
            {
                try
                {
                    var requestModel = new EncrDecr<SearchRequest>().Decrypt(request.Value,false,Request);
                    result = await _callbackService.GetCallbackList(requestModel);
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
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK,true,false,Request);
            return _iHttpActionResult;
        }

        /// <summary>
        /// GetCallbackList
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<Object>))]
        [Route("UpdateCallBackStatus")]
        public async Task<IHttpActionResult> UpdateCallBackStatus(RequestModel request)
        {
            var response = new Response<Object>();
            var result = new Object();
            if (ModelState.IsValid)
            {
                try
                {
                    var requestModel = new EncrDecr<UpdateCallbackRequest>().Decrypt(request.Value,false,Request);
                    result = await _callbackService.UpdateCallBackStatus(requestModel);
                    if (result != null)
                    {
                        response = response.Create(true, AdminResponseMessages.DATA_FOUND, HttpStatusCode.OK, result);
                        
                    }
                    else
                    {
                        response = response.Create(false, AdminResponseMessages.DATA_NOT_FOUND, HttpStatusCode.NotAcceptable, result);
                        //_iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, AdminResponseMessages.DATA_NOT_FOUND, HttpStatusCode.NotAcceptable, result);
                   // _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError, true, false, Request);
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
    }
}
