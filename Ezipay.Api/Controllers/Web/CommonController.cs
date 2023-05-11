using ezeePay.Utility.CommonClass;
using Ezipay.Api.Filters;
using Ezipay.Service.Admin.ShareAndEarn;
using Ezipay.ViewModel.common;
using Ezipay.ViewModel.ShareAndEarnViewModel;
using Ezipay.ViewModel.WalletUserVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Ezipay.Api.Controllers.Web
{
    [RoutePrefix("api/CommonController")]
    public class CommonController : ApiController
    {
        private IShareAndEarnService _shareAndEarnService;
        private IHttpActionResult _iHttpActionResult;
        private Converter _converter;
        /// <summary>
        /// CommonController
        /// </summary>
        /// <param name="shareAndEarnService"></param>
        public CommonController(IShareAndEarnService shareAndEarnService)
        {
            _shareAndEarnService = shareAndEarnService;
            _converter = new Converter();
        }

        /// <summary>
        /// GetReferalUrl
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<Object>))]
        [Route("GetReferalUrl")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        public async Task<IHttpActionResult> GetReferalUrl(RequestModel request)
        {
            var response = new Response<Object>();
            var result = new Object();
            // int langId = AppUtils.GetLangId(Request);
            var requestModel = new EncrDecr<UserDocumentRequest>().Decrypt(request.Value, false, Request);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _shareAndEarnService.GetReferalUrl(requestModel.WalletUserId);
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
        /// RedeemPoints
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<Object>))]
        [Route("RedeemPoints")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        public async Task<IHttpActionResult> RedeemPoints(RequestModel request)
        {
            var response = new Response<Object>();
            var result = new Object();
            // int langId = AppUtils.GetLangId(Request);
            var requestModel = new EncrDecr<RedeemPointsRequest>().Decrypt(request.Value, false, Request);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _shareAndEarnService.RedeemPoints(requestModel);
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.POINT_REDEEM_SUCCESS, HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_ERROR, HttpStatusCode.NotAcceptable, result);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_ERROR, HttpStatusCode.NotAcceptable, result);
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
                response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_ERROR, HttpStatusCode.NotAcceptable, result);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }

        /// <summary>
        /// RedeemPoints
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<List<RedeemPointsHistoryResponse>>))]
        [Route("GetRedeemHistory")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        public async Task<IHttpActionResult> GetRedeemHistory(RequestModel request)
        {
            var response = new Response<List<RedeemPointsHistoryResponse>>();
            var result = new List<RedeemPointsHistoryResponse>();
            // int langId = AppUtils.GetLangId(Request);
            var requestModel = new EncrDecr<RedeemHistoryRequest>().Decrypt(request.Value, false, Request);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _shareAndEarnService.GetRedeemHistory(requestModel.WalletUserId);
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
    }
}
