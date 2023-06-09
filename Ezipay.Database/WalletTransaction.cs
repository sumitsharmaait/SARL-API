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
    
    public partial class WalletTransaction
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public WalletTransaction()
        {
            this.Commisions = new HashSet<Commision>();
            this.Commisions1 = new HashSet<Commision>();
            this.Commisions2 = new HashSet<Commision>();
            this.Commisions3 = new HashSet<Commision>();
        }
    
        public long WalletTransactionId { get; set; }
        public string AmountInCedi { get; set; }
        public string TotalAmount { get; set; }
        public Nullable<int> CommisionId { get; set; }
        public string CommisionAmount { get; set; }
        public string WalletAmount { get; set; }
        public Nullable<decimal> ServiceTaxRate { get; set; }
        public string ServiceTax { get; set; }
        public Nullable<int> WalletServiceId { get; set; }
        public Nullable<long> SenderId { get; set; }
        public Nullable<long> ReceiverId { get; set; }
        public string CountryCode { get; set; }
        public string AccountNo { get; set; }
        public string TransactionId { get; set; }
        public Nullable<bool> IsAdminTransaction { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public string Comments { get; set; }
        public string InvoiceNo { get; set; }
        public Nullable<int> TransactionStatus { get; set; }
        public string TransactionType { get; set; }
        public Nullable<int> TransactionTypeInfo { get; set; }
        public Nullable<bool> IsBankTransaction { get; set; }
        public string BankBranchCode { get; set; }
        public string BankTransactionId { get; set; }
        public bool IsAddDuringPay { get; set; }
        public string MerchantCommissionAmount { get; set; }
        public Nullable<long> MerchantCommissionId { get; set; }
        public string VoucherCode { get; set; }
        public Nullable<System.DateTime> UpdatedOn { get; set; }
        public string FlatCharges { get; set; }
        public string BenchmarkCharges { get; set; }
        public string CommisionPercent { get; set; }
        public string OperatorType { get; set; }
        public string DisplayContent { get; set; }
        public Nullable<long> StoreId { get; set; }
        public string BeneficiaryName { get; set; }
        public string IsdCode { get; set; }
        public Nullable<bool> IsInitialTransction { get; set; }
        public Nullable<long> CronId { get; set; }
        public Nullable<long> TransactionInitiateRequestId { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Commision> Commisions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Commision> Commisions1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Commision> Commisions2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Commision> Commisions3 { get; set; }
    }
}
