using Ezipay.ViewModel.CommisionViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.ViewModel.AdminViewModel
{
    public class UserListResponse
    {
        public UserListResponse()
        {
            TotalCount = 0;
        }

        public int TotalCount { get; set; }
        public List<UserList> UserList { get; set; }
        //public string TotalBalance{ get; set; }
    }

    public class UserList
    {
        public long UserId { get; set; }
        public string Name { get; set; }
        public string EmailId { get; set; }
        public string MobileNo { get; set; }
        public string Country { get; set; }
        public bool IsActive { get; set; }
        public int TotalCount { get; set; }
        public decimal Currentbalance { get; set; }
        public int DocumetStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public long RowNumber { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsOtpVerified { get; set; }
    }

    public class UserListRequest : SearchRequest
    {

    }
    public class UserManageRequest
    {
        [Required]
        public long UserId { get; set; }
        [Required]
        public bool IsActive { get; set; }
        [Required]
        public int Status { get; set; }
        public long AdminId { get; set; } //log key

        public string Flag { get; set; }
        public string Comment { get; set; }
    }

    public class UserDeleteRequest
    {
        [Required]
        public long UserId { get; set; }
        public long AdminId { get; set; } //log key
    }

    public class UserEmailVerifyResponse
    {
        public UserEmailVerifyResponse()
        {
            this.VerficationMessage = string.Empty;
            this.VerficationStatus = false;
        }
        public string VerficationMessage { get; set; }
        public bool VerficationStatus { get; set; }
        public int RstKey { get; set; }
    }
    public class ViewUserTransactionResponse
    {
        public int TotalCount { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public List<TransactionDetail> TransactionList { get; set; }
    }

    public class TransactionDetail
    {
        public int Id { get; set; }
        public string RowLabel { get; set; }
        public string TransactionType { get; set; }
        public string TransactionAmount { get; set; }
        public int TransactionCode { get; set; }
        public string FromMobileNo { get; set; }
        public string ToMobileNo { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Comments { get; set; }
        public long WalletTransactionId { get; set; }
        public int TotalCount { get; set; }
        public int DataType { get; set; }
        public int Pagesize { get; set; }
        public string BankTransactionId { get; set; }
        public int TransactionTypeInfo { get; set; }
        public string AccountNo { get; set; }
        public int TransactionStatus { get; set; }
        public string ServiceName { get; set; }
        //public string SubCategory { get; set; }
        public string AfterTransactionBalance { get; set; }
        public long receiverId { get; set; }
        public long senderId { get; set; }
        public string TotalAmount { get; set; }
        public string CommisionAmount { get; set; }
        public string TxnCountry { get; set; }
    }

    public class ViewUserTransactionRequest : SearchRequest
    {
        public long UserId { get; set; }
        public Nullable<DateTime> DateFrom { get; set; }
        public Nullable<DateTime> DateTo { get; set; }
        public int TransactionType { get; set; }
    }
    public class CreditDebitResponse
    {
        public int RstKey { get; set; }
    }
    public class CreditDebitRequest
    {
        public decimal Amount { get; set; }
        public string Reason { get; set; }
        public bool TransactionType { get; set; }
        public long UserId { get; set; }
        public string CashDepositFlag { get; set; }
        public string TxnId { get; set; }
        public long AdminId { get; set; } //log key
    }

    public class UserTransactionLimitDetailsResponse
    {
        public string UserName { get; set; }
        public string EmailId { get; set; }
        public string MobileNo { get; set; }
        public string TransactionLimit { get; set; }
        public string TransactionLimitForAddMoney { get; set; }
        public int RstKey { get; set; }
    }
    public class UserDetailsRequest
    {
        public long UserId { get; set; }
        public long AdminId { get; set; } //log key
    }

    public class UserDocumentDetailsResponse
    {
        public UserDocumentDetailsResponse()
        {
            Documents = new List<DocModel>();
        }
        public long WalletUserId { get; set; }
        public string IdProofImage { get; set; }
        public string CardImage { get; set; }
        public string DocImage { get; set; }
        public int DocumentStatus { get; set; }
        public List<DocModel> Documents { get; set; }
    }

    public class DocumentChangeRequest
    {
        [Required]
        public long UserId { get; set; }
        [Range(0, 4)]
        public int Status { get; set; }
        public long AdminId { get; set; } //log key
    }

    public class UserBlockUnblockDetailResponse
    {
        public UserBlockUnblockDetailResponse()
        {
            TotalCount = 0;
        }

        public int TotalCount { get; set; }
        public List<UserBlockUnblockDetail1> UserList { get; set; }
        
    }

    public class UserBlockUnblockDetail1
    {
        public long RowNumber { get; set; }
        public int TotalCount { get; set; }
        public string EmailId { get; set; }
        public long? Walletuserid { get; set; }
        public bool? Blockstatus { get; set; }
        public string BlockByEmailid { get; set; }
        public Nullable<System.DateTime> Blockdate { get; set; }
        public string UnBlockByEmailid { get; set; }
        public Nullable<System.DateTime> UnBlockdate { get; set; }
        public string Comment { get; set; }
    }
}
