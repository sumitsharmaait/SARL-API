using Ezipay.ViewModel.AdminViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.Admin.Currency
{
    public interface ICurrencyConvertService
    {
        Task<bool> CurrencyConversion(CurrencyConvertRequest request);
        Task<CurrencyConvertResponse> GetCurrencyRate();
        Task<CurrencyLogsResponce> GetCurrencyRateLog(CurrencyLogRequest request);
    }
}
