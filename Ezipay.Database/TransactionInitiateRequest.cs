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
    
    public partial class TransactionInitiateRequest
    {
        public long Id { get; set; }
        public Nullable<long> WalletUserId { get; set; }
        public Nullable<long> ReceiverWalletUserId { get; set; }
        public string UserName { get; set; }
        public string InvoiceNumber { get; set; }
        public string UserReferanceNumber { get; set; }
        public string ServiceName { get; set; }
        public string CurrentBalance { get; set; }
        public string ReceiverCurrentBalance { get; set; }
        public string AfterTransactionBalance { get; set; }
        public string RequestedAmount { get; set; }
        public string JsonRequest { get; set; }
        public string JsonResponse { get; set; }
        public Nullable<int> TransactionStatus { get; set; }
        public string ReceiverNumber { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
    }
}
