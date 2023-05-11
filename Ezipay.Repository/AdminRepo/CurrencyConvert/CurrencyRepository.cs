using Ezipay.Database;
using Ezipay.ViewModel.AdminViewModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.AdminRepo.CurrencyConvert
{
    public class CurrencyRepository:ICurrencyRepository
    {
        public async Task<int> InsertCurrency(CurrencyRate objCurrencyRate)
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                context.CurrencyRates.Add(objCurrencyRate);
                await context.SaveChangesAsync();
                return 1;
            }
        }

        public async Task<CurrencyConvertResponse> GetCurrencyRate()
        {
            var getCurrencyRate = new CurrencyConvertResponse();
            using (var context = new DB_9ADF60_ewalletEntities())
            {
               
                var response = await context.Database.SqlQuery<CurrencyConvertResponse>
                        ("EXEC usp_GetCurrencyRate").FirstOrDefaultAsync();
                getCurrencyRate.CediRate = response.CediRate;//Add Doller Rate
                getCurrencyRate.DollarRate = response.DollarRate;//Send Doller Rate
                getCurrencyRate.CfaRate = response.CfaRate;//CFA Rate
                getCurrencyRate.NGNRate = response.NGNRate;//bank -ngn
                getCurrencyRate.EuroRate = response.EuroRate;//bank -EuroRate
                getCurrencyRate.SendNGNRate = response.SendNGNRate;//bank -ngn
                getCurrencyRate.SendGHRate = response.SendGHRate;//peyservice -ghana
            }
            return getCurrencyRate;
        }

        public async Task<CurrencyLogsResponce> GetCurrencyRateLog(CurrencyLogRequest request)
        {
            var responce = new CurrencyLogsResponce();
            var list = new List<GetCurrencyConvertLog>();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                list = await db.Database.SqlQuery<GetCurrencyConvertLog>("exec usp_GetCurrencyLog @PageNo,@PageSize",
                    new object[]
                    {
                      new  SqlParameter("@PageNo",request.PageNumber),
                      new  SqlParameter("@PageSize",request.PageSize)
                    }
                    ).ToListAsync();
                if (list != null && list.Count > 0)
                {
                    responce = new CurrencyLogsResponce
                    {
                        TotalCount = list.FirstOrDefault().TotalCount,
                        CurrencyLogslist = list
                    };
                }
                else
                {
                    responce.CurrencyLogslist = new List<GetCurrencyConvertLog>();
                }
            }
            return responce;
        }
    }
}
