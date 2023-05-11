using System;

namespace Ezipay.ViewModel.AdminViewModel
{
    public class DuplicateCardNoVMResponse
    {
        public long? WalletUserId { get; set; }
        public string CardNo { get; set; }
        public string EmailId { get; set; }
        public string Mobile_No { get; set; }
        public string CurrentBalance { get; set; }
        public DateTime? UserCreatedDate { get; set; }
        public long Id { get; set; }
        public string Message { get; set; }
        public string CreatedBy { get; set; }      
        public int RstKey { get; set; }
    }

    public class DuplicateCardNoVMRequest
    {
        public string Name { get; set; }
        public string Cardno { get; set; }
        public long Walletuserid { get; set; }     
        public string CreatedBy { get; set; } 
        public string flag { get; set; }
        public string NewCardImage { get; set; }
    }
    

}
