using Ezipay.Database;
using Ezipay.ViewModel.AdminViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.AdminRepo.CurrencyConvert
{
    public interface ICurrencyRepository
    {
        Task<int> InsertCurrency(CurrencyRate CurrencyRate);

        Task<CurrencyConvertResponse> GetCurrencyRate();

        Task<CurrencyLogsResponce> GetCurrencyRateLog(CurrencyLogRequest request);
    }
}
