using ezeePay.Utility.CommonClass;
using Ezipay.Api.Filters;
using Ezipay.Service.Admin.Commission;
using Ezipay.ViewModel.CommisionViewModel;
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
    public class ManageCommissionController : ApiController
    {
        private IHttpActionResult _iHttpActionResult;
        private Converter _converter;
        private ICommissionService _commissionService;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="commissionService"></param>
        public ManageCommissionController(ICommissionService commissionService)
        {
            _converter = new Converter();
            _commissionService = commissionService;
        }

        /// <summary>
        /// SetCommission
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<Object>))]
        [Route("SetCommission")]
        public async Task<IHttpActionResult> SetCommission(RequestModel model)
        {
            var response = new Response<Object>();
            var result = new Object();
            if (ModelState.IsValid)
            {
                try
                {
                    var request = new EncrDecr<CommissionRequest>().Decrypt(model.Value,false,Request);
                    result = await _commissionService.SetCommission(request);
                    if ((bool)result)
                    {
                        response = response.Create(true, AdminResponseMessages.COMMISSION_SET, HttpStatusCode.OK, result);
                     
                    }
                    else
                    {
                        response = response.Create(false, AdminResponseMessages.COMMISSION_NOT_SET, HttpStatusCode.NotAcceptable, result);
                        //_iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    //_iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
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
