using ezeePay.Utility.CommonClass;
using Ezipay.Api.Filters;
using Ezipay.Service.Admin.Subscription;
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
    /// Merchant Management
    /// </summary>
    [RoutePrefix("api/admin")]
    [SessionAuthorization]
    [SessionTokenExceptionFilter]
    public class SubscriptionController : ApiController
    {
        private IHttpActionResult _iHttpActionResult;
        private ISubscriptionService _subscriptionService;
        private Converter _converter;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="subscriptionService"></param>
        public SubscriptionController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
            _converter = new Converter();
        }

        /// <summary>
        /// GetMerchantList
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<SubscriptionLogResponse>))]
        [Route("GetSubscriptionLogs")]
        public async Task<IHttpActionResult> GetSubscriptionLogs(RequestModel requestModel)
        {
            var response = new Response<SubscriptionLogResponse>();
            var result = new SubscriptionLogResponse();
            if (ModelState.IsValid)
            {
                try
                {
                    var request = new EncrDecr<SearchRequest>().Decrypt(requestModel.Value,false,Request);
                    result = await _subscriptionService.GetSubscriptionLogs(request);
                    if (result.SubscriptionLogs != null)
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
