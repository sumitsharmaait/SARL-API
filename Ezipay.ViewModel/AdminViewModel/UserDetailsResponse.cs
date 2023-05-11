using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.ViewModel.AdminViewModel
{
    public class UserDetailsResponse
    {
        public string UserName { get; set; }
        public string EmailId { get; set; }
        public string MobileNo { get; set; }
        public string AvailableBalance { get; set; }
    }
    
    public class SetTransactionLimitRequest
    {
        public string TransactionLimit { get; set; }
        public string UserId { get; set; }
        /// <summary>
        /// 1-TransactionLimit,2-AddMoneyLimit
        /// </summary>
        public int Type { get; set; }
        public long AdminId { get; set; } //log key
    }
}
