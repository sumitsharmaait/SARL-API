using ezeePay.Utility.CommonClass;
using Ezipay.Api.Filters;
using Ezipay.Service.AdminService.DashBoardService;
using Ezipay.ViewModel.common;
using Ezipay.ViewModel.DashBoardViewModel;
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
    [RoutePrefix("api/admin")]
    [SessionAuthorization]
    [SessionTokenExceptionFilter]
    public class DashboardController : ApiController
    {
        private IHttpActionResult _iHttpActionResult;
        private IDashBoardServices _dashBoardServices;
        private Converter _converter;
        // private object _walletUserService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dashBoardServices"></param>
        public DashboardController(IDashBoardServices dashBoardServices)
        {
            _dashBoardServices = dashBoardServices;
            _converter = new Converter();
        }

        /// <summary>
        /// Dashboard
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<DashboardResponse>))]
        [Route("Dashboard")]
        public async Task<IHttpActionResult> Dashboard(RequestModel request)
        {
            var response = new Response<DashboardResponse>();
            var result = new DashboardResponse();

            try
            {
                var requestModel = new EncrDecr<DashboardRequest>().Decrypt(request.Value, false, Request);
                result = await _dashBoardServices.DashboardDetails(requestModel);
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
                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError, true, false, Request);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }

        /// <summary>
        /// EnableTransactions
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("EnableTransactions")]
        [ResponseType(typeof(Response<Object>))]
        public async Task<IHttpActionResult> EnableTransactions()
        {
            var response = new Response<Object>();
            var result = new Object();
            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();////

            if (ModelState.IsValid)
            {
                try
                {
                    result = await _dashBoardServices.EnableTransactions(sessionToken);
                    if ((bool)result == true)
                    {
                        response = response.Create(true, AdminResponseMessages.TRANSACTION_ENABLED, HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, AdminResponseMessages.TRANSACTION_NOT_ENABLED, HttpStatusCode.NotAcceptable, result);
                        // _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, AdminResponseMessages.TRANSACTION_NOT_ENABLED, HttpStatusCode.NotAcceptable, result);
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
                response = response.Create(false, AdminResponseMessages.TRANSACTION_NOT_ENABLED, HttpStatusCode.NotAcceptable, result);
                //_iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }



        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<CheckUBATxnNotCaptureOurSideResponse>))]
        [Route("CheckUBATxnNotCaptureOurSide")]
        public async Task<IHttpActionResult> CheckUBATxnNotCaptureOurSide(RequestModel requestModel)
        {
            var response = new Response<List<CheckUBATxnNotCaptureOurSideResponse>>();
            var result = new List<CheckUBATxnNotCaptureOurSideResponse>();
            var request = new EncrDecr<CheckUBATxnNotCaptureOurSide>().Decrypt(requestModel.Value, false, Request);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _dashBoardServices.CheckUBATxnNotCaptureOurSide(request.InvoiceNumber);
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
        [Route("Emailuser")]
        [ResponseType(typeof(Response<Object>))]
        public async Task<IHttpActionResult> Emailuser()
        {
            var response = new Response<Object>();
            var result = new Object();
            

            if (ModelState.IsValid)
            {
                try
                {
                    result = await _dashBoardServices.Emailuser();
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);

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
                response = response.Create(false, AdminResponseMessages.DATA_NOT_FOUND, HttpStatusCode.NotAcceptable, result);
                //_iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }


    }
}
