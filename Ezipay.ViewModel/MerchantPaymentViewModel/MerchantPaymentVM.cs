using Ezipay.ViewModel.SendPushViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.ViewModel.MerchantPaymentViewModel
{   
    public class MerchantTransactionRequest
    {
        public long MerchantId { get; set; }
       
        public string Amount { get; set; }
        public string Comment { get; set; }
      
        public string Password { get; set; }
    }
    public class MerchantTransactionForThirdPartyRequest
    {
        public string amount { get; set; }
        public string emailId { get; set; }
        public string senderId { get; set; }
        public string transactionType { get; set; }
        public string merchantId { get; set; }
        public string merchantKey { get; set; }
        public string apiKey { get; set; }
    }


}
