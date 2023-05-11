using System;

namespace Ezipay.ViewModel.AdminViewModel
{

    public class TransactionLimitAUResponse
    {
        
        public DateTime? FromDateTime { get; set; }
        public DateTime? ToDateTime { get; set; }
        public string Message { get; set; }
        
        public string Amount { get; set; }

        public string SetAmount { get; set; }
        public string TotalAmount { get; set; }

    }

    public class TransactionLimitAURequest
    {
        public DateTime? FromDateTime { get; set; }
        public DateTime?  ToDateTime { get; set; }
        public string Message { get; set; }
        public int Id { get; set; }
        public int Amount { get; set; }
        public long AdminId { get; set; } //log key
    }
}
