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
    
    public partial class UserDocument
    {
        public int Id { get; set; }
        public Nullable<long> WalletUserId { get; set; }
        public string IdProofImage { get; set; }
        public string CardImage { get; set; }
        public Nullable<System.DateTime> CreateOn { get; set; }
        public Nullable<System.DateTime> UpdatedOn { get; set; }
        public Nullable<int> DocumentStatus { get; set; }
    }
}