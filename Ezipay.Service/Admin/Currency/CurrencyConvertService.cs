using Ezipay.Database;
using Ezipay.Repository.AdminRepo.CurrencyConvert;
using Ezipay.ViewModel.AdminViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.Admin.Currency
{
    public class CurrencyConvertService:ICurrencyConvertService
    {
        private readonly ICurrencyRepository _currencyRepository;

        public CurrencyConvertService()
        {
            _currencyRepository = new CurrencyRepository();
        }

        public async Task<bool> CurrencyConversion(CurrencyConvertRequest request)
        {
            bool result = false;

            var currencyRate = new CurrencyRate();

            currencyRate.DollarRate = Convert.ToDecimal(request.DollarRate);//Send Doller Rate
            currencyRate.CediRate = Convert.ToDecimal(request.CediRate);//Add Doller Rate
            currencyRate.CfaRate = Convert.ToDecimal(request.CfaRate); //CFA Rate
            currencyRate.NGNRate = Convert.ToDecimal(request.NGNRate);//Add -ngn
            currencyRate.EuroRate = Convert.ToDecimal(request.EuroRate);//Add 
            currencyRate.SendNGNRate = Convert.ToDecimal(request.SendNGNRate);//bank -ngn
            currencyRate.SendGHRate = Convert.ToDecimal(request.SendGHRate);//peyservice -ghana
            currencyRate.CreatedDate = DateTime.UtcNow;
            currencyRate.UpdatedDate = DateTime.UtcNow;

            if (await _currencyRepository.InsertCurrency(currencyRate) > 0)
            {
                result = true;
            }

            return result;
        }

        public async Task<CurrencyConvertResponse> GetCurrencyRate()
        {
            return await _currencyRepository.GetCurrencyRate();
        }

        public async Task<CurrencyLogsResponce> GetCurrencyRateLog(CurrencyLogRequest request)
        {
            var result = new CurrencyLogsResponce();
            result = await _currencyRepository.GetCurrencyRateLog(request);
            if (result.CurrencyLogslist.Count > 0)
            {
                result.TotalCount = result.CurrencyLogslist[0].TotalCount;
            }
            return result;
        }
    }
}
