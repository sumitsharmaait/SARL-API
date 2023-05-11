using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.ViewModel.CheckPasswordViewModel
{
    public class CheckLoginResponse
    {
        public CheckLoginResponse()
        {
            this.HashedPassword = string.Empty;
            this.HashedSalt = new byte[0];

        }

        public string HashedPassword { get; set; }
        public byte[] HashedSalt { get; set; }
        public string Message { get; set; }
        public bool IsPasswordMatched { get; set; }
        public int RstKey { get; set; }
    }

    public class FAQResponse
    {
        public FAQResponse()
        {
            this.QuestionText = string.Empty;
            this.Detail = string.Empty;
            this.FaqDetails = new List<FaqDetailResponse>();
        }
        public int FaqId { get; set; }
        public string QuestionText { get; set; }
        public string Detail { get; set; }
        public List<FaqDetailResponse> FaqDetails { get; set; }
    }
    public class FaqDetailResponse
    {
        public FaqDetailResponse()
        {
            this.Detail = string.Empty;
        }
        public int FaqDetailId { get; set; }
        public string Detail { get; set; }
    }

    public class FeedbackTypeResponse : FeedBackCommon
    {
        public FeedbackTypeResponse()
        {
            this.TypeName = string.Empty;
        }

        public string TypeName { get; set; }

    }
    public class FeedBackCommon
    {
        public FeedBackCommon()
        {
            this.FeedbackTypeId = 0;
        }
        public int FeedbackTypeId { get; set; }
    }   
    public class FeedBackRequest : FeedBackCommon
    {
        public FeedBackRequest()
        {
            this.FeedBackMessage = string.Empty;
            this.FeedbackTypeId = 0;
        }

        public string FeedBackMessage { get; set; }
        public long? UserId { get; set; }
        public string Name { get; set; }
        public string EmailId { get; set; }
        public string MobileNo { get; set; }
    }

    public class FeedBackWebRequest : FeedBackCommon
    {
        public FeedBackWebRequest()
        {
            this.FeedBackMessage = string.Empty;
            this.FeedbackTypeId = 0;
        }

        public string FeedBackMessage { get; set; }
        public string Name { get; set; }
        public string EmailId { get; set; }
        public string MobileNo { get; set; }
    }
    public class DownloadLogReportRequest
    {
        DownloadLogReportRequest()
        {
            this.DateFrom = DateTime.MinValue;
            this.DateTo = DateTime.MinValue;
        }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string name { get; set; }
    }


    public class DownloadLogReportRequest1
    {       
        
        public string Yr { get; set; }
        public string Month { get; set; }
    }

}
