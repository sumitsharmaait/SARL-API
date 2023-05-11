using System;

namespace Ezipay.ViewModel.AdminViewModel
{
    public class TItxnresponse
    {
        public long Id { get; set; }
        public Nullable<long> WalletUserId { get; set; }
        public string InvoiceNumber { get; set; }
        public string AfterTransactionBalance { get; set; }
        public string RequestedAmount { get; set; }
        public string JsonResponse { get; set; }

    }
    public class Fluttertxnresponse
    {
        public long WalletTxnid { get; set; }

        public string UpdatebyAdminWalletID { get; set; }

        public int totalCount { get; set; }
        public bool TransactionType { get; set; }
        public long UserId { get; set; }

        public string InvoiceNo { get; set; }
        public long AdminId { get; set; } //log key

    }
    public class WalletTxnRequest
    {
        public long WalletTxnid { get; set; }
        public int? Txnstatus { get; set; }
        public string UpdatebyAdminWalletID { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; }
        public bool TransactionType { get; set; }
        public long UserId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string InvoiceNo { get; set; }
        public long AdminId { get; set; } //log key

    }

    public class WalletTxnResponse
    {
        public int Id { get; set; }
        public string WalletTxnId { get; set; }
        public string TxnId { get; set; }
        public string EmailId { get; set; }
        public string WalletTxnStatus { get; set; }
        public string WalletUserId { get; set; }
        public string InvoiceNo { get; set; }
        public string TransactionType { get; set; }
        public string TotalAmount { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string TxnCountry { get; set; }
        public int? WalletServiceId { get; set; }
    }


}
