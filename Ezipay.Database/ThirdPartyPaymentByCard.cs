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
    
    public partial class ThirdPartyPaymentByCard
    {
        public int Id { get; set; }
        public string Amount { get; set; }
        public string AmountWithCommision { get; set; }
        public string BenificiaryName { get; set; }
        public string Comment { get; set; }
        public string ISD { get; set; }
        public Nullable<long> WalletUserId { get; set; }
        public Nullable<int> ServiceCategoryId { get; set; }
        public string Channel { get; set; }
        public string MobileNo { get; set; }
        public string OrderNumber { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public string SessionTokenPerTransaction { get; set; }
        public string BankCode { get; set; }
        public string MerchantId { get; set; }
        public string MerchantName { get; set; }
        public string Product_Id { get; set; }
        public string AmountInLocalCountry { get; set; }
        public string InvoiceNumber { get; set; }
        public string DisplayContent { get; set; }
        public string AmountInUsd { get; set; }
        public string ExtraField { get; set; }
    }
}
