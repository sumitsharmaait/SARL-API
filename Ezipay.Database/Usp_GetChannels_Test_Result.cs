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
    
    public partial class Usp_GetChannels_Test_Result
    {
        public int WalletServiceId { get; set; }
        public string ServiceName { get; set; }
        public string DisplayServiceName { get; set; }
        public Nullable<int> ServiceCategoryId { get; set; }
        public string ImageUrl { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<long> MerchantId { get; set; }
        public decimal CommisionPercent { get; set; }
        public decimal FlatCharges { get; set; }
        public decimal BenchmarkCharges { get; set; }
    }
}
