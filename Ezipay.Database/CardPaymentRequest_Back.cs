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
    
    public partial class CardPaymentRequest_Back
    {
        public long CardPaymentRequestId { get; set; }
        public Nullable<long> WalletUserId { get; set; }
        public string Amount { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<bool> IsAddDuringPay { get; set; }
        public string CommissionAmount { get; set; }
        public string TotalAmount { get; set; }
        public string OrderNo_Old { get; set; }
        public string TransactionNo_Old { get; set; }
        public string OrderNo { get; set; }
        public string TransactionNo { get; set; }
    
        public virtual WalletUser WalletUser { get; set; }
        public virtual WalletUser WalletUser1 { get; set; }
    }
}
