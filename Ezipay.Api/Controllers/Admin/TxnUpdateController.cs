using ezeePay.Utility.CommonClass;
using Ezipay.Api.Filters;
using Ezipay.Database;
using Ezipay.Service.Admin.TxnUpdate;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.common;
using Ezipay.ViewModel.DashBoardViewModel;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Ezipay.Api.Controllers.Admin
{
    /// <summary>
    /// 
    /// </summary>
    [RoutePrefix("api/adminvin")]
    [SessionAuthorization]
    [SessionTokenExceptionFilter]
    public class TxnUpdateController : ApiController
    {
        private IHttpActionResult _iHttpActionResult;
        private ITxnUpdateService _TxnUpdateService;
        private Converter _converter;


        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="TxnUpdateService"></param>
        public TxnUpdateController(ITxnUpdateService TxnUpdateService)
        {
            _TxnUpdateService = TxnUpdateService;
            _converter = new Converter();
        }

        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<List<WalletTxnResponse>>))]
        [Route("GetWalletTxnPendingList")]
        public async Task<IHttpActionResult> GetWalletTxnPendingList()
        {
            var response = new Response<List<WalletTxnResponse>>();
            var result = new List<WalletTxnResponse>();
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _TxnUpdateService.GetWalletTxnPendingList();
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
        [Route("UpdatePendingWalletTxn")]
        [ResponseType(typeof(Response<Object>))]
        public async Task<IHttpActionResult> UpdatePendingWalletTxn(RequestModel requestModel)
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
                    result = await _TxnUpdateService.UpdatePendingWalletTxn(request);
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
        [Route("UpdateBankPendingWalletTxn")]
        [ResponseType(typeof(Response<Object>))]
        public async Task<IHttpActionResult> UpdateBankPendingWalletTxn(RequestModel requestModel)
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
                    result = await _TxnUpdateService.UpdateBankPendingWalletTxn(request);
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

        
    }
}
