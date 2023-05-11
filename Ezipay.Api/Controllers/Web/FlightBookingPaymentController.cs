using Ezipay.Service.FlightHotelService;
using Ezipay.ViewModel.common;
using Ezipay.ViewModel.FlightHotelViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Ezipay.Api.Controllers.Web
{
    /// <summary>
    /// FlightBookingPaymentController
    /// </summary>
    public class FlightBookingPaymentController : ApiController
    {
        private IFlightBookingPaymentService _FlightBookingPayment;
        //ITokenRepository _IToken;
        //IAppUser _IAppUser;
        public FlightBookingPaymentController(IFlightBookingPaymentService _flightBookingPayment)
        {
            _FlightBookingPayment = _flightBookingPayment;
            //_IToken = _iToken;
            //_IAppUser = _iAppUser;
        }


        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<FlightBookingVerificationResponse>))]
        [Description("PaymentWalletVerification")]
        public async Task<Response<FlightBookingVerificationResponse>> PaymentWalletVerification(FlightBookingVerificationRequest request)
        {
            Response<FlightBookingVerificationResponse> response = new Response<FlightBookingVerificationResponse>();
            FlightBookingVerificationResponse _response = new FlightBookingVerificationResponse();
            if (ModelState.IsValid)
            {
                _response = await _FlightBookingPayment.PaymentWalletVerification(request);
                if (_response.IsSuccess)
                {
                    response.Create(true, _response.Message, HttpStatusCode.OK, _response);
                }
                else
                {
                    response.Create(false, _response.Message, HttpStatusCode.NotFound, _response);
                }
            }
            else
            {
                if (request.UserId == null)
                {
                    response.Create(false, _response.Message, HttpStatusCode.NotAcceptable, _response);
                }
                else
                {
                    response.Create(false, _response.Message, HttpStatusCode.ExpectationFailed, _response);
                }
            }
            return response;
        }

        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<FlightBookingPaymentVerifyResponse>))]
        [Description("CardPayment")]
        public async Task<Response<FlightBookingPaymentVerifyResponse>> PaymentByUserWallet(FlightBookingVerifyRequest request)
        {
            var response = new Response<FlightBookingPaymentVerifyResponse>();
            var _response = new FlightBookingPaymentVerifyResponse();
            if (ModelState.IsValid)
            {
                _response =await _FlightBookingPayment.PaymentByUserWallet(request);
                if (_response.IsSuccess)
                {
                    response.Create(true, _response.Message, HttpStatusCode.OK, _response);
                }
                else
                {
                    response.Create(false, _response.Message, HttpStatusCode.NotFound, _response);
                }
            }
            else
            {
                if (request.UserId == null)
                {
                    response.Create(false, _response.Message, HttpStatusCode.NotAcceptable, _response);
                }
                else
                {
                    response.Create(false, _response.Message, HttpStatusCode.ExpectationFailed, _response);
                }
            }

            return response;
        }
    }
}
