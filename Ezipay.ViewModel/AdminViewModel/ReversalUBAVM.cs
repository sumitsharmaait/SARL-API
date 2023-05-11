namespace Ezipay.ViewModel.AdminViewModel
{


    public class UBATxnVerificationRequest
    {
        public UBATxnVerificationRequest()
        {
            this.CardNumber = 0;
            this.CountryCode = string.Empty;
            this.Apikey = string.Empty;
        }
        public int CardNumber { get; set; }
        public string CountryCode { get; set; }
        public string Apikey { get; set; }

    }



    public class UBATxnVerificationResponse
    {
        public string TXNstatus { get; set; }
    }

    public class UBATxnRefundRequest
    {
        public UBATxnRefundRequest()
        {
            this.CardNumber = 0;
            this.CountryCode = string.Empty;
            this.Amount = string.Empty;
            this.Currency = string.Empty;
            this.Apikey = string.Empty;
        }
        public int CardNumber { get; set; }
        public string CountryCode { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string Apikey { get; set; }

    }
    public class UBATxnRefundResponse
    {
        public string TXNstatus { get; set; }
    }
}
