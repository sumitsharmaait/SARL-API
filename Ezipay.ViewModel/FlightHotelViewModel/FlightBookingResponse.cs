using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.ViewModel.FlightHotelViewModel
{
    public class FlightBookingVerificationRequest
    {
        [Required]
        public long TimeStamp { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public string Amount { get; set; }
    }
    public class FlightBookingVerificationResponse
    {
        public string SecurityCode { get; set; }
        public string SessionId { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
    public class FlightBookingVerifyRequest
    {
        [Required]
        public string SecurityCode { get; set; }
        // public string SessionId { get; set; }
        [Required]
        public long TimeStamp { get; set; }
        public string Otp { get; set; }
        [Required]
        public string UserId { get; set; }
    }

    public class FlightBookingPaymentVerifyResponse
    {
        public long UserId { get; set; }
        public string Amount { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }


}
