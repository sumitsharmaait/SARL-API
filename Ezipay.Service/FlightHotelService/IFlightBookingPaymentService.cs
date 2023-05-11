using Ezipay.ViewModel.FlightHotelViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.FlightHotelService
{
    public interface IFlightBookingPaymentService
    {
        /// <summary>
        ///Payment By Wallet
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        Task<FlightBookingVerificationResponse> PaymentWalletVerification(FlightBookingVerificationRequest requestModel);

        /// <summary>
        ///Payment By User Wallet
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        Task<FlightBookingPaymentVerifyResponse> PaymentByUserWallet(FlightBookingVerifyRequest requestModel);
    }
}
