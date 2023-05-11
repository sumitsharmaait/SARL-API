using ezeePay.Utility.CommonClass;
using Ezipay.Api.Filters;
using Ezipay.Service.Admin.Currency;
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
    /// Banner
    /// </summary>
    [RoutePrefix("api/admin")]
    [SessionAuthorization]
    [SessionTokenExceptionFilter]
    public class CurrencyConvertController : ApiController
    {
        private IHttpActionResult _iHttpActionResult;
        private Converter _converter;
        private ICurrencyConvertService _currencyConvertService;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="currencyConvertService"></param>

        public CurrencyConvertController(ICurrencyConvertService currencyConvertService)
        {
            _converter = new Converter();
            _currencyConvertService = currencyConvertService;
        }

        /// <summary>
        /// CurrencyConversion
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<Object>))]
        [Route("CurrencyConversion")]
        public async Task<IHttpActionResult> CurrencyConversion(RequestModel model)
        {
            var response = new Response<Object>();
            var result = new Object();
            if (ModelState.IsValid)
            {
                try
                {
                    var request = new EncrDecr<CurrencyConvertRequest>().Decrypt(model.Value, false, Request);
                    result = await _currencyConvertService.CurrencyConversion(request);
                    if ((bool)result)
                    {
                        response = response.Create(true, AdminResponseMessages.CURRENCY_SET, HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, AdminResponseMessages.CURRENCY_NOT_SET, HttpStatusCode.NotAcceptable, result);
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

        /// <summary>
        /// GetCurrencyRate
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<CurrencyConvertResponse>))]
        [Route("GetCurrencyRate")]
        public async Task<IHttpActionResult> GetCurrencyRate()
        {
            var response = new Response<CurrencyConvertResponse>();
            var result = new CurrencyConvertResponse();
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _currencyConvertService.GetCurrencyRate();
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
                //_iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }

        /// <summary>
        /// GetCurrencyRateLog
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<CurrencyLogsResponce>))]
        [Route("GetCurrencyRateLog")]
        public async Task<IHttpActionResult> GetCurrencyRateLog(RequestModel request)
        {
            var response = new Response<CurrencyLogsResponce>();
            var result = new CurrencyLogsResponce();
            if (ModelState.IsValid)
            {
                try
                {
                    var requestModel = new EncrDecr<CurrencyLogRequest>().Decrypt(request.Value, false, Request);
                    result = await _currencyConvertService.GetCurrencyRateLog(requestModel);
                    if (result.CurrencyLogslist != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                        //_iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
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
                //  _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }
    }
}
