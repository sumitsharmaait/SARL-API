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
    
    public partial class TransactionLimitAU
    {
        public int Id { get; set; }
        public Nullable<System.DateTime> FromDateTime { get; set; }
        public Nullable<System.DateTime> ToDateTime { get; set; }
        public Nullable<int> Amount { get; set; }
        public string Message { get; set; }
        public Nullable<System.DateTime> Createddate { get; set; }
    }
}