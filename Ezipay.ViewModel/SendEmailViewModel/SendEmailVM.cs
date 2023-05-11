using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.ViewModel.SendEmailViewModel
{
    public class SendVerificationEmailRequest
    {
        public SendVerificationEmailRequest()
        {
            this.EmailId = string.Empty;

        }
        public string EmailId { get; set; }
    }
    public class OtpResponse : OtpCommonRequest
    {
        public OtpResponse()
        {
            this.IsSuccess = false;
            this.StatusCode = 0;
            this.Message = string.Empty;
        }
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
       // public long OtpId { get; set; }


    }
    public class OtpCommonRequest
    {
        public OtpCommonRequest()
        {
            this.Otp = string.Empty;
        }
        public string Otp { get; set; }
    }

    public class EmailModel
    {
        public string TO { get; set; }
        public string CC { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

    }
    public class QRCodeRequest
    {
        [Required]
        public string EmailId { get; set; }
    }

    public class MobileNoListResponse
    {
        public long? WalletUserId { get; set; }
        public string MobileNo { get; set; }
    }


}
