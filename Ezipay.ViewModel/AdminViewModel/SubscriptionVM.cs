using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.ViewModel.AdminViewModel
{
    public class SubscriptionLogResponse
    {
        public SubscriptionLogResponse()
        {
            this.TotalCount = 0;
            SubscriptionLogs = new List<SubscriptionLogRecord>();
        }
        public int TotalCount { get; set; }
        public List<SubscriptionLogRecord> SubscriptionLogs { get; set; }
    }

    public class SubscriptionLogRecord
    {
        SubscriptionLogRecord()
        {
            this.SubscriptionId = 0;
            this.EmailId = string.Empty;
            this.CreatedDate = DateTime.UtcNow;
            this.RequestNumber = string.Empty;
            this.IsActive = true;
            this.Pagesize = 0;
            this.TotalCount = 0;
        }
        public long SubscriptionId { get; set; }
        public string EmailId { get; set; }
        public string RequestNumber { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public int TotalCount { get; set; }
        public int Pagesize { get; set; }
    }
}
