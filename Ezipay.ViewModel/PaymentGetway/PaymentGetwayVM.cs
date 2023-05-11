using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.ViewModel
{
    public class PGPayMoneyVM
    {
        public long WalletUserId { get; set; }
        public double Amount { get; set; }
        public string LoanNumber { get; set; }
    }

    public class SessionInfoRequest
    {
        public long WalletUserId { get; set; }
        public string DeviceUniqueId { get; set; }
    }

    public class SessionInfoResponse
    {
        public string Token { get; set; }
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
        public string CurrentBalance { get; set; }
        public double WalletBalance { get; set; }
    }

    public class CashInCashOutRequest
    {
        public string amount { get; set; }
        public string emailId { get; set; }
        public string senderId { get; set; }
        public string transactionType { get; set; }
        public string merchantId { get; set; }
        public string merchantKey { get; set; }
        public string apiKey { get; set; }
    }

    public class CashInCashOutResponse
    {
        public string Amount { get; set; }
        public string UserId { get; set; }
        public string EmailId { get; set; }
        public string Sender { get; set; }
        public string TransactionType { get; set; }
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
    }
}
