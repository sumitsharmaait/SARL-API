using ezeePay.Utility.CommonClass;
using Ezipay.Api.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Ezipay.Service.Admin.TransactionLimitAU;
using System.Web.Http.Description;
using System.Threading.Tasks;
using Ezipay.ViewModel.common;
using Ezipay.ViewModel.AdminViewModel;

namespace Ezipay.Api.Controllers.Admin
{
    [RoutePrefix("api/adminvi")]
    [SessionAuthorization]
    [SessionTokenExceptionFilter]
    public class TransactionLimitAUController : ApiController
    {
        private IHttpActionResult _iHttpActionResult;
        private ITransactionLimitAUService _transactionLimitAUService;
        private Converter _converter;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="transactionLimitAUService"></param>
        public TransactionLimitAUController(ITransactionLimitAUService transactionLimitAUService)
        {
            _transactionLimitAUService = transactionLimitAUService;
            _converter = new Converter();
        }

        [AcceptVerbs("POST")]
        [Route("InsertTransactionLimitAU")]
        [ResponseType(typeof(Response<Object>))]
        public async Task<IHttpActionResult> InsertTransactionLimitAU(RequestModel requestModel)
        {
            var response = new Response<Object>();
            var result = new Object();
            var request = new EncrDecr<TransactionLimitAURequest>().Decrypt(requestModel.Value, false, Request);
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
                    result = await _transactionLimitAUService.InsertTransactionLimitAU(request);
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
        [ResponseType(typeof(Response<List<TransactionLimitAUResponse>>))]
        [Route("GetTransactionLimitAUResponseList")]
        public async Task<IHttpActionResult> GetTransactionLimitAUResponseList()
        {
            var response = new Response<List<TransactionLimitAUResponse>>();
            var result = new List<TransactionLimitAUResponse>();
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _transactionLimitAUService.GetTransactionLimitAUResponseList();
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
        [ResponseType(typeof(Response<TransactionLimitAUResponse>))]
        [Route("GetTransactionLimitAUMessage")]
        public async Task<IHttpActionResult> GetTransactionLimitAUMessage()
        {
            var response = new Response<TransactionLimitAUResponse>();
            var result = new TransactionLimitAUResponse();
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _transactionLimitAUService.GetTransactionLimitAUMessage();
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


        
    }
}
