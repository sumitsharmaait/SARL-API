using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.ViewModel.DashBoardViewModel
{
    public class DashboardRequest
    {        
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public long AdminId { get; set; } //log key
    }
    public class DashboardResponse
    {
        public long TotalUsers { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalSendMoneyTransaction { get; set; }
        public int TotalPayMoneyTransaction { get; set; }
        public int TotalPayServicesTransaction { get; set; }
        public int TotalTransaction { get; set; }
        public bool TransactionsEnabled { get; set; }
        public int FailedTransaction { get; set; }
        public int PendingTransaction { get; set; }
        public decimal TotalLiability { get; set; }
        public long TotalDeletedUsers { get; set; }
        public long PendingKyc { get; set; }
        public long TotalMerchant { get; set; }
    }

    public class CheckUBATxnNotCaptureOurSideResponse
    {

        public long? WalletUserId { get; set; }
        public string RequestedAmount { get; set; }
        public string EmailId { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
    public class CheckUBATxnNotCaptureOurSide
    {       
        public string InvoiceNumber { get; set; }      
     
    }


}
