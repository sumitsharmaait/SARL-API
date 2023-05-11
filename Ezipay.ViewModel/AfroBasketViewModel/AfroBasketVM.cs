using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.ViewModel.AfroBasketViewModel
{
    public class GetUserCurrentBalanceResponse
    {
        public GetUserCurrentBalanceResponse()
        {
            this.RstKey = 0;
        }

        public int RstKey { get; set; }
        public string CurrentBalance { get; set; }
        public string EmailId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long WalletUserId { get; set; }
    }


    public class GetUserCurrentBalanceRequest
    {
        [Required]
        public string EmailId { get; set; }
        [Required]
        public long WalletUserId { get; set; }
    }
    public class AfroBasketVerificationRequest
    {
        [Required]
        public long TimeStamp { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public string Amount { get; set; }
    }
    public class AfroBasketVerificationResponse
    {
        public string SecurityCode { get; set; }
        public string SessionId { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }

    }
    public class AfroBasketVerifyRequest
    {
        [Required]
        public string SecurityCode { get; set; }
        // public string SessionId { get; set; }
        [Required]
        public long TimeStamp { get; set; }
        // public string Otp { get; set; }
        [Required]
        public string UserId { get; set; }
        public string ServiceName { get; set; }
    }

    public class AfroBasketPaymentVerifyResponse
    {
        public long UserId { get; set; }
        public string Amount { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }

    public class VerifyAfroBasketRequest
    {
        public string merchantcode { get; set; }
        public string agentcode { get; set; }
        public string tokenid { get; set; }
        public string checksum { get; set; }
        public string countrycode { get; set; }
    }
    public class GetAfroBasketRequest
    {
        public GetAfroBasketRequest()
        {
        }
        public string agentcode { get; set; }
        public string tokenID { get; set; }
        public string merchantcode { get; set; }
        public string saltkey { get; set; }
    }
    public class AfroBasketLoginRequest
    {
        public AfroBasketLoginRequest()
        {
        }
        public string agentcode { get; set; }
        public string tokenID { get; set; }
        public string tgt { get; set; }
        public string saltkey { get; set; }
    }
    public class AfroBasketLoginResponse
    {
        public AfroBasketLoginResponse()
        {
            this.RstKey = 0;
            this.Message = string.Empty;
            this.DocStatus = false;
            this.DocumetStatus = 0;
            this.IsEmailVerified = false;
        }

        public int RstKey { get; set; }
        public string Message { get; set; }
        public int DocumetStatus { get; set; }
        public bool? DocStatus { get; set; }
        public bool IsEmailVerified { get; set; }
        public string responseString { get; set; }

    }


    public class EzipayPartnerLoginRequest
    {
        public string Password { get; set; }
        public string ServiceType { get; set; }
    }


}
