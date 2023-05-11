using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.ViewModel.ShareAndEarnViewModel
{
    public class InsertShareRewardRequest
    {
        public int ReceiverPoints { get; set; }
        public int SenderPoints { get; set; }
        public decimal ConversionPointsValue { get; set; }
        public decimal ConversionPoint { get; set; }
        public decimal MinimumRedeemablePoint { get; set; }
        public long AdminId { get; set; } //log key
    }
    public class ShareAndEarnMasterResponse
    {
        public int Id { get; set; }
        public Nullable<int> ReceiverPoints { get; set; }
        public Nullable<decimal> ConversionPoint { get; set; }
        public Nullable<int> SenderPoints { get; set; }
        public Nullable<decimal> ConversionPointsValue { get; set; }
        public Nullable<decimal> MinimumRedeemablePoint { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
    }
    public class RedeemPointsRequest
    {
        public decimal RedeemPoints { get; set; }
        public decimal RedeemAmount { get; set; }
        public long WalletUserId { get; set; }
    }
    public class RedeemHistoryRequest
    {      
        public long WalletUserId { get; set; }
    }

    public partial class RedeemPointsHistoryResponse
    {
      
        public Nullable<long> WalletUserId { get; set; }
        public string RedeemAmount { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }       
        public string TransactionType { get; set; }
    }
}
