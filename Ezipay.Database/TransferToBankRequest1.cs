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
    
    public partial class TransferToBankRequest1
    {
        public long TransferToBankRequestId { get; set; }
        public string OrderNo { get; set; }
        public string TransactionNo { get; set; }
        public Nullable<long> WalletUserId { get; set; }
        public string Amount { get; set; }
        public string DebitAcctNumber { get; set; }
        public string CreditAcctNumber { get; set; }
        public string CreditAcctName { get; set; }
        public string BankCode { get; set; }
        public string bankName { get; set; }
        public string Narration { get; set; }
        public string Remarks { get; set; }
        public string Countrycode { get; set; }
        public string CategoryCode { get; set; }
        public string WalletNo { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
    
        public virtual WalletUser WalletUser { get; set; }
    }
}
