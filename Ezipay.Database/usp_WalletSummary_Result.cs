//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Ezipay.Database
{
    using System;
    
    public partial class usp_WalletSummary_Result
    {
        public Nullable<long> TotalUsers { get; set; }
        public Nullable<long> TotalMerchant { get; set; }
        public Nullable<long> PendingKyc { get; set; }
        public Nullable<long> TotalDeletedUsers { get; set; }
        public Nullable<long> TotalMerchants { get; set; }
        public Nullable<int> TotalTransaction { get; set; }
        public Nullable<decimal> TotalLiability { get; set; }
        public Nullable<int> FailedTransaction { get; set; }
        public Nullable<int> PendingTransaction { get; set; }
        public Nullable<decimal> TotalRevenue { get; set; }
        public Nullable<int> TotalSendMoneyTransaction { get; set; }
        public Nullable<int> TotalPayMoneyTransaction { get; set; }
        public Nullable<int> TotalPayServicesTransaction { get; set; }
        public Nullable<bool> TransactionsEnabled { get; set; }
    }
}