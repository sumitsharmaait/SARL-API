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
    
    public partial class usp_UserDetailByToken_Result
    {
        public long WalletUserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string StdCode { get; set; }
        public string MobileNo { get; set; }
        public string EmailId { get; set; }
        public string QrCode { get; set; }
        public string CurrentBalance { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        public string ProfileImage { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsMobileNoVerified { get; set; }
        public bool IsNotification { get; set; }
        public string HashedPassword { get; set; }
        public byte[] HashedSalt { get; set; }
        public Nullable<int> DeviceType { get; set; }
        public string DeviceToken { get; set; }
        public Nullable<int> DocumetStatus { get; set; }
        public bool IsDisabledTransaction { get; set; }
        public decimal EarnedPoints { get; set; }
        public decimal EarnedAmount { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<int> UserType1 { get; set; }
    }
}
