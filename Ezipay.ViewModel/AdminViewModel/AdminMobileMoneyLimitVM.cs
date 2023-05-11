using System;

namespace Ezipay.ViewModel.AdminViewModel
{
    public class AdminMobileMoneyLimitResponse
    {
       public int Id { get; set; }
        public string MaximumAmount { get; set; }
        public string MinimumCharges { get; set; }

        public string MinimumAmount { get; set; }
        public string ServiceCode { get; set; }
    }

    public class AdminMobileMoneyLimitRequest
    {
        public string MaximumAmount { get; set; }
        public string MinimumCharges { get; set; }

        public string MinimumAmount { get; set; }
        public string Service { get; set; }
        public string flag { get; set; }
        public int Id { get; set; }
        public long AdminId { get; set; } //log key
    }
    
}
