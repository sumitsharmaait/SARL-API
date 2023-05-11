using ezeePay.Utility.CommonClass;
using Ezipay.Api.Filters;
using Ezipay.Service.PaymentGetway;
using Ezipay.ViewModel;
using Ezipay.ViewModel.common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Ezipay.Api.Controllers
{
    /// <summary>
    /// PaymentGetway
    /// AccessKey in Header for identify Source
    /// {lendingApp:EziPayVpu3EHooi6Q7f1TD9pnNoPN078dR9Yn+HaQ95i0LSGLbHpYE4Pt+B8rvFdbb9kKaPT
    /// </summary>
    [RoutePrefix("api/PaymentGetway")]
    public class PaymentGetwayController : ApiController
    {
        private IHttpActionResult _iHttpActionResult;
        private Converter _converter;
        private readonly IPaymentGetwayService _paymentGetwayService;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="paymentGetwayService"></param>
        public PaymentGetwayController(IPaymentGetwayService paymentGetwayService)
        {
            _converter = new Converter();
            _paymentGetwayService = paymentGetwayService;
        }

        /// <summary>
        /// GetWalletSessionInfo
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetWalletSessionInfo")]
        [TempSessionAuthorization]
        [TempTokenExceptionFilter]
        [ResponseType(typeof(Response<SessionInfoResponse>))]
        public async Task<IHttpActionResult> GetWalletSessionInfo(RequestModel model)
        {
            var response = new Response<SessionInfoResponse>();
            var result = new SessionInfoResponse();

            try
            {
                var request = new EncrDecr<SessionInfoRequest>().Decrypt(model.Value, true);
                result = await _paymentGetwayService.GetWalletSessionInfo(request);

                if (result != null)
                {
                    response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);
                }
                else
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NoContent, result);

                }
            }
            catch (Exception ex)
            {
                response = response.Create(false, ResponseMessages.EXCEPTION_OCCURED, HttpStatusCode.NoContent, result);

            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, true);
            return _iHttpActionResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PayMoney")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<Object>))]
        public async Task<IHttpActionResult> PayMoney(RequestModel model)
        {
            var response = new Response<Object>();
            var result = new Object();

            try
            {
                var request = new EncrDecr<PGPayMoneyVM>().Decrypt(model.Value);
                result = await _paymentGetwayService.PayMoney(request);

                if ((int)result == 1)
                {
                    response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);

                }
                else
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NoContent, result);

                }
            }
            catch (Exception ex)
            {
                response = response.Create(false, ResponseMessages.EXCEPTION_OCCURED, HttpStatusCode.NoContent, result);

            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
            return _iHttpActionResult;
        }

        /// <summary>
        /// CashInCashOut
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Payment")]
        //[SessionAuthorization]
        //[SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<CashInCashOutResponse>))]
        public async Task<IHttpActionResult> Payment(CashInCashOutRequest request)
        {
            var response = new Response<CashInCashOutResponse>();
            var result = new CashInCashOutResponse();

            try
            {
                // var request = new EncrDecr<PGPayMoneyVM>().Decrypt(model.Value);
                result = await _paymentGetwayService.CashInCashOut(request);

                if (result != null)
                {
                    response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);

                }
                else
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NoContent, result);

                }
            }
            catch (Exception ex)
            {
                response = response.Create(false, ResponseMessages.EXCEPTION_OCCURED, HttpStatusCode.NoContent, result);

            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, false, false);
            return _iHttpActionResult;
        }

    }
}
