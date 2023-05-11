using ezeePay.Utility.CommonClass;
using Ezipay.Api.Filters;
using Ezipay.Repository.Admin.ReversalUBA;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Ezipay.Api.Controllers.Admin
{
    [RoutePrefix("api/adminvini")]
    [SessionAuthorization]
    [SessionTokenExceptionFilter]
    public class ReversalUBAController : ApiController
    {
        private IHttpActionResult _iHttpActionResult;
        private IReversalUBAService _ReversalUBAService;
        private Converter _converter;


        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="ReversalUBAService"></param>
        public ReversalUBAController(IReversalUBAService ReversalUBAService)
        {
            _ReversalUBAService = ReversalUBAService;
            _converter = new Converter();
        }


        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<List<UBATxnVerificationResponse>>))]
        [Route("Getresponse")]
        public async Task<IHttpActionResult> Getresponse(RequestModel requestModel)
        {
            var response = new Response<List<UBATxnVerificationResponse>>();
            var result = new List<UBATxnVerificationResponse>();
            if (ModelState.IsValid)
            {
                try
                {
                    var request = new EncrDecr<UBATxnVerificationRequest>().Decrypt(requestModel.Value, false, Request);
                    result = await _ReversalUBAService.Getresponse(request);
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
