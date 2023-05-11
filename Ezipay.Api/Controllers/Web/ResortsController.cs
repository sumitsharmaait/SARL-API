using ezeePay.Utility.CommonClass;
using Ezipay.Api.Filters;
using Ezipay.Service.Admin.Resort;
using Ezipay.ViewModel.common;
using Ezipay.ViewModel.PayMoneyViewModel;
using Ezipay.ViewModel.ResortViewModel;
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
    [RoutePrefix("api/ResortsController")]
    [SessionAuthorization]
    [SessionTokenExceptionFilter]
    public class ResortsController : ApiController
    {
        private IResortService _resortService;
        private IHttpActionResult _iHttpActionResult;
        private Converter _converter;
        /// <summary>
        /// ResortController
        /// </summary>
        /// <param name="resortService"></param>
        public ResortsController(IResortService resortService)
        {
            _resortService = resortService;
            _converter = new Converter();
        }

        /// <summary>
        /// GetHotels
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<List<HotelMasterResponse>>))]
        [Route("GetHotels")]
        public async Task<IHttpActionResult> GetHotels()
        {
            var response = new Response<List<HotelMasterResponse>>();
            var result = new List<HotelMasterResponse>();
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _resortService.GetHotels();
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
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
                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError, true, false, Request);
            }
            return _iHttpActionResult;
        }

        /// <summary>
        /// BookHotel
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<object>))]
        [Route("BookHotel")]
        public async Task<IHttpActionResult> BookHotel(RequestModel request)
        {
            var response = new Response<WalletTransactionResponse>();
            var result = new WalletTransactionResponse();
            var requestModel = new EncrDecr<HotelBookingRequest>().Decrypt(request.Value, false, Request);

            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();////
            if (ModelState.IsValid)
            {
                try
                {

                    result = await _resortService.HotelBook(requestModel, sessionToken);
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.HOTEL_BOOKED, HttpStatusCode.OK, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.REQUEST_SENT_FAILD, HttpStatusCode.NotAcceptable, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.REQUEST_SENT_FAILD, HttpStatusCode.NotAcceptable, result);
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
                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError, true, false, Request);
            }
            return _iHttpActionResult;
        }
    }
}
