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

namespace Ezipay.Api.Controllers.Admin
{
    /// <summary>
    /// ShareAndEarnController
    /// </summary>
    [RoutePrefix("api/admin")]
    [SessionAuthorization]
    [SessionTokenExceptionFilter]
    public class ShareAndEarnController : ApiController
    {
        private IShareAndEarnService _shareAndEarnService;
        private IHttpActionResult _iHttpActionResult;
        private Converter _converter;

        /// <summary>
        /// ShareAndEarnController
        /// </summary>
        /// <param name="shareAndEarnService"></param>
        public ShareAndEarnController(IShareAndEarnService shareAndEarnService)
        {
            _shareAndEarnService = shareAndEarnService;
            _converter = new Converter();
        }

        /// <summary>
        /// InsertReward
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<object>))]
        [Route("InsertReward")]
        public async Task<IHttpActionResult> InsertReward(RequestModel model)
        {
            var response = new Response<object>();
            int result = 0;
            if (ModelState.IsValid)
            {
                try
                {
                    var request = new EncrDecr<InsertShareRewardRequest>().Decrypt(model.Value, false, Request);
                    result = await _shareAndEarnService.InsertReward(request);
                    if (result > 0)
                    {
                        response = response.Create(true, AdminResponseMessages.DATA_SAVED, HttpStatusCode.OK, result);
                    }
                    else
                    {
                        response = response.Create(true, ResponseMessages.DATA_NOT_SAVED, HttpStatusCode.OK, result);
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
                response = response.Create(false, ResponseMessages.DATA_NOT_SAVED, HttpStatusCode.NotAcceptable, result);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }

        /// <summary>
        /// GetRewardList
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<ShareAndEarnMasterResponse>))]
        [Route("GetRewardList")]
        public async Task<IHttpActionResult> GetRewardList()
        {
            var response = new Response<ShareAndEarnMasterResponse>();
            var result = new ShareAndEarnMasterResponse();
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _shareAndEarnService.GetRewardList();
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                        //_iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    //  _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError, true, false);
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
                // _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError, true, false);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }

    }
}
