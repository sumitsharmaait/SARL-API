using System.Collections.Generic;

namespace Ezipay.ViewModel.AdminViewModel
{
    public class CurrencyConvertRequest
    {
        public decimal DollarRate { get; set; }
        public decimal NGNRate { get; set; }
        public double CfaRate { get; set; }
        public decimal CediRate { get; set; }
        public decimal EuroRate { get; set; }
        public decimal SendNGNRate { get; set; }
        public decimal SendGHRate { get; set; }
        public long AdminId { get; set; } //log key
    }
    public class CurrencyConvertResponse
    {
        public decimal DollarRate { get; set; }
        public decimal NGNRate { get; set; }
        public decimal CfaRate { get; set; }
        public decimal CediRate { get; set; }
        public decimal EuroRate { get; set; }
        public decimal SendNGNRate { get; set; }
        public decimal SendGHRate { get; set; }
    }


    public class CurrencyLogsResponce
    {
        public CurrencyLogsResponce()
        {
            TotalCount = 0;
        }
        public int TotalCount { get; set; }
        public List<GetCurrencyConvertLog> CurrencyLogslist { get; set; }

    }

    public class GetCurrencyConvertLog
    {
        public decimal DollarRate { get; set; }
        public decimal CediRate { get; set; }
        public decimal NGNRate { get; set; }
        public decimal CfaRate { get; set; }
        public decimal EuroRate { get; set; }
        public decimal SendNGNRate { get; set; }
        public decimal SendGHRate { get; set; }
        public string CreatedDate { get; set; }
        public string UpdatedDate { get; set; }
        public int TotalCount { get; set; }
    }

    public class CurrencyLogRequest : SearchRequest
    {

    }
}
