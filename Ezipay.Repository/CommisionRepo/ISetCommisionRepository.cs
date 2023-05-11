using Ezipay.Database;
using Ezipay.ViewModel.CommisionViewModel;
using Ezipay.ViewModel.PayMoneyViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.CommisionRepo
{
    public interface ISetCommisionRepository
    {

        Task<bool> SetCommision(CommisionMaster request);

        Task<CalculateCommissionResponse> CalculateCommission(CalculateCommissionRequest request);

        Task<CalculateCommissionResponse> CalculateAddMoneyCommission(CalculateCommissionRequest request);
        Task<CalculateCommissionResponse> CalculateCommissionForMobileMoney(CalculateCommissionRequest request, long UserId, long transactionCount,string isdcode);
        Task<CalculateCommissionResponse> CalculatePayNGNTransferSendMoneyCommission(CalculateCommissionRequest request);
        Task<CalculateCommissionResponse> CalculatePayNGNTransferAddMoneyCommission(CalculateCommissionRequest request);
        //Task<CommissionCalculationResponse> CalculateCommission(decimal rate, int ServiceId, string amount, decimal flatCharges = 0, decimal benchmarkCharges = 0);
    }
}
