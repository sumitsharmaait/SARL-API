using Ezipay.ViewModel.CommisionViewModel;
using Ezipay.ViewModel.PayMoneyViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.CommisionService
{
    public interface ISetCommisionService
    {
       
        Task<bool> SetCommision(CommissionRequest request);

        Task<CalculateCommissionResponse> CalculateCommission(CalculateCommissionRequest request);

        Task<CalculateCommissionResponse> CalculateAddMoneyCommission(CalculateCommissionRequest request);

        Task<CommissionCalculationResponse> CalculateCommission(decimal rate, int ServiceId, string amount, decimal flatCharges = 0, decimal benchmarkCharges = 0);
    }
}
