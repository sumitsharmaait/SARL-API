using Ezipay.ViewModel.SendPushViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.ViewModel.PayMoneyViewModel
{
    public class WalletTransactionRequest
    {
        public WalletTransactionRequest()
        {
            this.MobileNo = string.Empty;
            this.IsdCode = string.Empty;
            this.Amount = string.Empty;
            this.Comment = string.Empty;
            this.Password = string.Empty;
        }
        [Required]
        [Phone]
        public string MobileNo { get; set; }
        public string IsdCode { get; set; }
        [Required]
        public string Amount { get; set; }
        public string Comment { get; set; }
        public string Password { get; set; }
        public long StoreId { get; set; }
        public string BeneficiaryName { get; set; }
    }
    public class WalletTransactionResponse
    {
        public WalletTransactionResponse()
        {
            this.TransactionId = 0;
            this.StatusCode = 0;
            this.Message = string.Empty;
            this.CurrentBalance = string.Empty;
            this.TransactionDate = DateTime.Now;
            this.TransactionAmount = string.Empty;
            this.AccountNo = string.Empty;
            this.Amount = string.Empty;
            this.ToMobileNo = string.Empty;
            this.SenderBalance = string.Empty;
            this.RstKey = 0;
        }
        public long TransactionId { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public string CurrentBalance { get; set; }
        public DateTime TransactionDate { get; set; }
        public string ToMobileNo { get; set; }
        public string TransactionAmount { get; set; }
        public string Amount { get; set; }
        public string AccountNo { get; set; }
        public string SenderBalance { get; set; }
        public bool DocStatus { get; set; }
        public int DocumetStatus { get; set; }
        public PushNotificationModel userPushDetail { get; set; }
        public int RstKey { get; set; }
    }
    public class CommissionCalculationResponse
    {
        public CommissionCalculationResponse()
        {
            this.CommissionAmount = "0";
            this.AfterDeduction = "0";
            this.AmountWithCommission = "0";
        }
        public string CommissionAmount { get; set; }
        public decimal Rate { get; set; }
        public string AmountWithCommission { get; set; }
        public string AfterDeduction { get; set; }
        public int CommissionServiceId { get; set; }

    }

    public class MakePaymentRequest
    {
        public long SenderId { get; set; }
        public long RecieverId { get; set; }
        public string Amount { get; set; }
        public string Comment { get; set; }
        public int TransactionTypeInfo { get; set; }
    }

    public class ViewPaymentResponse
    {
        public ViewPaymentResponse()
        {
            this.CurrentBalance = string.Empty;
            this.TotalCount = this.PageSize = 0;
            this.RstKey = 0;
        }
        public string CurrentBalance { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int RstKey { get; set; }
        public List<PayResponse> PaymentRequests { get; set; }
    }

    public class PayResponse
    {
        public PayResponse()
        {
            this.PayMoneyRequestId = 0;
            this.Amount = string.Empty;
            this.Comments = string.Empty;
            this.SenderId = 0;
            this.ReceiverId = 0;
            this.CreatedDate = DateTime.Now;
            this.IsAccept = false;
            this.FirstName = string.Empty;
            this.LastName = string.Empty;
            this.MobileNo = string.Empty;
            this.StdCode = string.Empty;
            this.PrivateKey = string.Empty;
            this.PublicKey = string.Empty;
            this.TotalCount = 0;
        }
        public long PayMoneyRequestId { get; set; }
        public string Amount { get; set; }
        public string Comments { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MobileNo { get; set; }
        public string StdCode { get; set; }
        public long SenderId { get; set; }
        public long ReceiverId { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsAccept { get; set; }
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
        public int TotalCount { get; set; }
    }

    public class ViewPaymentRequest
    {
        public int PageNo { get; set; }
        public string Password { get; set; }
    }
    public class ManagePayMoneyReqeust
    {
        public ManagePayMoneyReqeust()
        {
            this.PayMoneyRequestId = 0;
            this.IsAccept = false;
        }
        public long PayMoneyRequestId { get; set; }
        public bool IsAccept { get; set; }
        public string Password { get; set; }
    }
    public class ViewTransactionResponse
    {
        public int RstKey { get; set; }
        public List<ViewTransactionResult> TransactionList { get; set; }
    }
    public class ViewTransactionResult
    {
        public ViewTransactionResult()
        {
            this.Id = 0;
            this.RowLabel = string.Empty;
            this.WalletTransactionId = 0;
            this.ServiceName = string.Empty;
            //this.SubCategory = string.Empty;
            //this.MainCategory = string.Empty;
            this.receiverId = 0;
            this.senderId = 0;
            this.TransactionType = string.Empty;
            this.TransactionCode = 0;
            this.Comments = string.Empty;
            this.BankTransactionId = string.Empty;
            this.CreatedDate = string.Empty;
            //this.CreatedDate = DateTime.UtcNow;
            this.TransactionStatus = 0;
            this.TransactionAmount = string.Empty;
            this.FromMobileNo = string.Empty;
            this.ToMobileNo = string.Empty;
            this.AccountNo = string.Empty;
            this.DataType = 0;
            this.TotalCount = 0;
            this.Pagesize = 0;
            this.TotalAmount = string.Empty;
            this.CommisionAmount = string.Empty;

        }
        public int Id { get; set; }
        public string RowLabel { get; set; }
        public long WalletTransactionId { get; set; }
        public string ServiceName { get; set; }
        //public string SubCategory { get; set; }
        //public string MainCategory { get; set; }
        public long receiverId { get; set; }
        public long senderId { get; set; }
        //blank in api
        public string TransactionType { get; set; }
        public int TransactionCode { get; set; }
        public string Comments { get; set; }
        public string BankTransactionId { get; set; }
        //public DateTime CreatedDate { get; set; }
        public string CreatedDate { get; set; }
        public int TransactionStatus { get; set; }
        public string TransactionAmount { get; set; }
        public string TotalAmount { get; set; }
        public string CommisionAmount { get; set; }
        public string FromMobileNo { get; set; }
        public string ToMobileNo { get; set; }
        public string AccountNo { get; set; }
        public int DataType { get; set; }
        public int TotalCount { get; set; }
        public int Pagesize { get; set; }
        public int TransactionTypeInfo { get; set; }
        public string DisplayContent { get; set; }
        public string TxnCountry { get; set; }
    }
    public class ViewTransactionRequest
    {
        public int PageNo { get; set; }
        public int TransactionType { get; set; }

    }

    public class TransactionLimitResponse
    {
        public string userid { get; set; }
        public string transactionlimit { get; set; }

    }
    public class TotalTransactionCountResponse
    {
        public int TotalTransactions { get; set; }           
    }
}
