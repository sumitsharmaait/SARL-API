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
    using System.Collections.Generic;
    
    public partial class Vw_wallettrans
    {
        public long WalletTransactionId { get; set; }
        public string TransactionId { get; set; }
        public Nullable<System.DateTime> TransactionDate { get; set; }
        public string TransactionTime { get; set; }
        public string Category { get; set; }
        public string ServiceName { get; set; }
        public string TransactionType { get; set; }
        public string TotalAmount { get; set; }
        public string CommisionAmount { get; set; }
        public string WalletAmount { get; set; }
        public string Name { get; set; }
        public string AccountNo { get; set; }
        public string TransactionStatus { get; set; }
        public string Comments { get; set; }
        public Nullable<long> Walletuserno { get; set; }
    }
}
