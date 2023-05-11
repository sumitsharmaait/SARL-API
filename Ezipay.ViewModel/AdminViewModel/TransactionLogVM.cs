using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.ViewModel.AdminViewModel
{

    public class TransactionLogResponse
    {
        public TransactionLogResponse()
        {
            this.TotalCount = 0;
            TransactionLogs = new List<TransactionLogRecord>();
        }
        public int TotalCount { get; set; }
        public List<TransactionLogRecord> TransactionLogs { get; set; }
    }



    public class TransactionLogRecord
    {
        TransactionLogRecord()
        {
            this.RowLabel = string.Empty;
            this.TransactionLogId = 0;
            this.TransactionName = string.Empty;
            this.LogDate = DateTime.UtcNow;
            this.LogJson = string.Empty;
            this.LogType = string.Empty;
            this.Pagesize = 0;
            this.TotalCount = 0;
        }
        public string RowLabel { get; set; }
        public long TransactionLogId { get; set; }
        public string LogType { get; set; }
        public string TransactionName { get; set; }
        public string LogJson { get; set; }
        public string Detail { get; set; }
        public DateTime LogDate { get; set; }
        public int TotalCount { get; set; }
        public int Pagesize { get; set; }
    }

    public class TransactionLogRequest : SearchRequest
    {
        public TransactionLogRequest()
        {
            this.TransactionType = TransactionType;
        }
        // public int PageNo { get; set; }
        public string TransactionType { get; set; }
        public long WalletTransactionId { get; set; }
        public string transactionid { get; set; }
        public string categoryname { get; set; }
        public string servicename { get; set; }
        public string totalAmount { get; set; }
        public string walletAmount { get; set; }
        public string name { get; set; }
        public string accountNo { get; set; }
        public long walletuserid { get; set; }
    }

    public class TransactionLogsRequest : SearchRequest
    {
        public TransactionLogsRequest()
        {
            this.TransactionType = 0;
            this.transactionsType = "";
            this.transactionid = "";
            this.categoryname = "";
            this.servicename = "";
            this.totalAmount = "";
            this.walletAmount = "";
            this.name = "";
            this.accountNo = "";
            this.walletuserid = 0;
            this.Date = "";
            this.Time = "";
        }
        // public int PageNo { get; set; }
        public int TransactionType { get; set; }
        public long WalletTransactionId { get; set; }
        public string transactionid { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string categoryname { get; set; }
        public string servicename { get; set; }
        public string transactionsType { get; set; }
        public string totalAmount { get; set; }
        public string commisionAmount { get; set; }
        public string walletAmount { get; set; }
        public string name { get; set; }
        public string accountNo { get; set; }
        public int transactionStatus { get; set; }
        public string comments { get; set; }
        public long walletuserid { get; set; }
    }

    public class TransactionLogsResponse
    {
        public TransactionLogsResponse()
        {
            this.TotalCount = 0;
            TransactionLogslist = new List<TransactionLogslist>();
        }
        public int TotalCount { get; set; }
        public List<TransactionLogslist> TransactionLogslist { get; set; }
    }
    public class TransactionLogslist
    {
        public long WalletTransactionId { get; set; }
        public string transactionid { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string categoryname { get; set; }
        public string servicename { get; set; }
        public string transactionType { get; set; }
        public string totalAmount { get; set; }
        public string commisionAmount { get; set; }
        public string walletAmount { get; set; }
        public string name { get; set; }
        public string accountNo { get; set; }
        public int transactionStatus { get; set; }
        public string comments { get; set; }
        public long walletuserid { get; set; }
        public int TotalCount { get; set; }
        public string CountryName { get; set; }
        public string ReceiverCountryName { get; set; }
        public string CurrentBalance { get; set; }
        public string RequestedAmount { get; set; }
        public string AfterTransactionBalance { get; set; }
        public string SenderCountryName { get; set; }
    }

    public class TransactionLogsResponce
    {
        public TransactionLogsResponce()
        {
            this.TotalCount = 0;
        }
        public int TotalCount { get; set; }
        public List<TransactionLogslist> TransactionLogslist { get; set; }
    }
    
    public class CardtxndetailsResponse
    {

        public CardtxndetailsResponse()
        {
            TotalCount = 0;
        }
        public long? WalletUserId { get; set; }
        public long TotalCount { get; set; }
        public string CardNo { get; set; }
        public string RequestedBankTxnId { get; set; }
        public string EmailId { get; set; }
        public string ResponseBankTxnId { get; set; }
        public string InvoiceNumber { get; set; }
        public string UserName { get; set; }
        public DateTime? CreatedDate { get; set; }
    }

    public class CardtxndetailsRequest: SearchRequest
    { }




    public class Monthlyreportlist
    {
        
        public int TotalCount { get; set; }
        public string Particulars { get; set; }
        public double TOTAL { get; set; }
        public double F2 { get; set; }
        public double F3 { get; set; }
        public double F4 { get; set; }
        public double F5 { get; set; }
        public double F6 { get; set; }
        public double F7 { get; set; }
        public double F8 { get; set; }
        public double F9 { get; set; }
        public double F10 { get; set; }
        public double F11 { get; set; }
        public double F12 { get; set; }
        public double F13 { get; set; }
        public double F14 { get; set; }
        public double F15 { get; set; }
        public double F16 { get; set; }
        public double F17 { get; set; }
        public double F18 { get; set; }
        public double F19 { get; set; }
        public double F20 { get; set; }
        public double F21 { get; set; }       
        public double F22 { get; set; }
        public double F23 { get; set; }
        public double F24 { get; set; }
        public double F25 { get; set; }
        public double F26 { get; set; }
        public double F27 { get; set; }
        public double F28 { get; set; }
        public double F29 { get; set; }
        public double F30 { get; set; }
        public double F31 { get; set; }
        public double F32 { get; set; }
        public string Month { get; set; }
        public string Yr { get; set; }
    }
    public class MonthlyreportResponce
    {
        public MonthlyreportResponce()
        {
            this.TotalCount = 0;
        }
        public int TotalCount { get; set; }
        public List<Monthlyreportlist> Monthlyreportlist { get; set; }
    }


    public class TransactionLogsResponse2
    {
        public TransactionLogsResponse2()
        {
            this.TotalCount = 0;
            TransactionLogslist2 = new List<TransactionLogslist2>();
        }
        public int TotalCount { get; set; }
        public List<TransactionLogslist2> TransactionLogslist2 { get; set; }
    }
    public class TransactionLogslist2
    {
         

        public long WalletUserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }
        public string StdCode { get; set; }
        public string MobileNo { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> UserType { get; set; }
        public string Country { get; set; }
        
        public string Currentbalance { get; set; }
        
        public Nullable<int> DeviceType { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public Nullable<int> DocumetStatus { get; set; }
        public Nullable<bool> IsEmailVerified { get; set; }
        public Nullable<bool> IsOtpVerified { get; set; }

    }

}
