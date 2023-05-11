using System;

namespace Ezipay.ViewModel.AdminViewModel
{
    public class CashdepositrequestResponse
    {
        public CashdepositrequestResponse()
        {
            TotalCount = 0;
        }

        public int TotalCount { get; set; }
        public int id { get; set; }
        public long WalletUserId { get; set; }
        public long UserId { get; set; }
        public string Name { get; set; }
        public string DepositorName { get; set; }
        public string DepositorCashAmount { get; set; }
        public string DepositorCountry { get; set; }
        public string DepositorCountryCode { get; set; }
        public string DepositorSlipImage { get; set; }
        public DateTime? UploadedDate { get; set; }
        public string DepositStatus { get; set; }
        public string Reason { get; set; }

        public string TotalDepositorAmount { get; set; }
        public string TransactiontID { get; set; }
        public string Isactive { get; set; }


    }

    public class CashdepositrequestRequest : SearchRequest
    {
        public int id { get; set; }
        public long WalletUserId { get; set; }
        public string DepositorName { get; set; }
        public string DepositorCashAmount { get; set; }
        public string DepositorCountry { get; set; }
        public string DepositorCountryCode { get; set; }
        public string DepositorSlipImage { get; set; }
        public DateTime? UploadedDate { get; set; }
        public string DepositStatus { get; set; }
        public string Reason { get; set; }
        public DateTime? DepositStatusUpdateDate { get; set; }
        public string Isactive { get; set; }
        
    }
    
}
