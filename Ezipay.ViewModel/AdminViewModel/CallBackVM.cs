using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.ViewModel.AdminViewModel
{
    public class CallbackResponse
    {
        public CallbackResponse()
        {
            this.TotalCount = 0;
            CallbackList = new List<CallbackRecord>();
        }
        public int TotalCount { get; set; }
        public List<CallbackRecord> CallbackList { get; set; }
    }

    public class UpdateCallbackRequest
    {
        public int CallbackId { get; set; }
        public int Status { get; set; }
         public long AdminId { get; set; } //log key
    }

    public class CallbackRecord
    {
        CallbackRecord()
        {
            this.CallbackId = 0;
            this.FirstName = string.Empty;
            this.LastName = string.Empty;
            this.MobileNo = string.Empty;
            this.EmailId = string.Empty;
            this.CreatedDate = DateTime.UtcNow;
            this.RequestNumber = string.Empty;
            this.IsActive = true;
            this.Pagesize = 0;
            this.TotalCount = 0;
            this.Status = 0;
            this.AcceptedBy = string.Empty;
        }
        public long CallbackId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public String MobileNo { get; set; }
        public string EmailId { get; set; }
        public string RequestNumber { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public int TotalCount { get; set; }
        public int Pagesize { get; set; }
        public int Status { get; set; }
        public String AcceptedBy { get; set; }
    }
    public class SearchRequest
    {
        public SearchRequest()
        {
            this.SearchText = "";
            this.PageNumber = 1;
            this.PageSize = 10;
            FromDate = null;
            ToDate = null;
        }
        public string SearchText { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
         public long AdminId { get; set; } //log key
    }
}
