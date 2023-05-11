using System;
using System.Collections.Generic;

namespace Ezipay.ViewModel.AdminViewModel
{
    public class DownloadReportResponse
    {
        public DownloadReportResponse()
        {
            this.FileUrl = string.Empty;
            this.ReportData = new List<ReportData>();
            this.UserTxnReportData = new List<UserTxnReportData>();
    
        }

        public bool Status { get; set; }
        public string FileUrl { get; set; }
        public long WalletUserId { get; set; }
        public List<ReportData> ReportData { get; set; }
        public List<UserTxnReportData> UserTxnReportData { get; set; }

    
    }
    public class ReportData
    {

        public ReportData()
        {
            this.WalletTransactionId = 0;
            this.ServiceName = string.Empty;
            this.receiverId = 0;
            this.senderId = 0;
            this.TransactionType = string.Empty;
            this.TransactionCode = 0;
            this.Comments = string.Empty;
            this.BankTransactionId = string.Empty;
            this.CreatedDate = DateTime.UtcNow;
            this.TransactionStatus = 0;
            this.TransactionAmount = string.Empty;
            this.FromMobileNo = string.Empty;
            this.ToMobileNo = string.Empty;
            this.AccountNo = string.Empty;


        }
        public long WalletTransactionId { get; set; }
        public string ServiceName { get; set; }
        public long receiverId { get; set; }
        public long senderId { get; set; }
        public string TransactionType { get; set; }
        public int TransactionCode { get; set; }
        public string Comments { get; set; }
        public string BankTransactionId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int TransactionStatus { get; set; }
        public string TransactionAmount { get; set; }
        public string FromMobileNo { get; set; }
        public string ToMobileNo { get; set; }
        public string AccountNo { get; set; }

    }
    public class DownloadReportRequest
    {
        public DownloadReportRequest()
        {
            this.DateFrom = DateTime.MinValue;
            this.DateTo = DateTime.MinValue;

        }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int DownloadType { get; set; }
        public int TransactionType { get; set; }
    }
    public class DownloadReportApiRequest : DownloadReportRequest
    {
        public long WalletUserId { get; set; }
        public long AdminId { get; set; } //log key
    }
    public class DeatailForDownloadReport
    {
        public long Code { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int DownloadType { get; set; }
        public int TransactionType { get; set; }
    }

    public class UserTxnReportData
    {
        public UserTxnReportData()
        {
            this.TransactionDate = DateTime.UtcNow;
            this.FromDate = DateTime.UtcNow;
            this.ToDate = DateTime.UtcNow;

            this.Walletuserid = string.Empty;
            this.FullName = string.Empty;
            this.Emailid = string.Empty;
            this.IsdMobileno = string.Empty;
            this.UserCountry = string.Empty;
            this.Currentbalance = string.Empty;
            this.AccountNo = string.Empty;
            this.Mainservice = string.Empty;
            this.SubServiceName = string.Empty;
            this.SubServiceCategoryName = string.Empty;
            this.Requestedamount = string.Empty;
            this.CommisionAmount = string.Empty;
            this.TotalAmount = string.Empty;
            this.TransactionStatus = string.Empty;
            this.TransactionType = string.Empty;
            this.Comments = string.Empty;
        }

        public DateTime TransactionDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Walletuserid { get; set; }
        public string FullName { get; set; }
        public string Emailid { get; set; }
        public string IsdMobileno { get; set; }
        public string UserCountry { get; set; }
        public string Currentbalance { get; set; }
        public string AccountNo { get; set; }
        public string Mainservice { get; set; }
        public string SubServiceName { get; set; }
        public string SubServiceCategoryName { get; set; }

        public string Requestedamount { get; set; }
        public string Comments { get; set; }
        public string CommisionAmount { get; set; }
        public string TotalAmount { get; set; }

        public string TransactionStatus { get; set; }
        public string TransactionType { get; set; }

        public string BeforeTxnBalance { get; set; }
        public string AfterTxnBalance { get; set; }

    }



}
