using Ezipay.Database;
using Ezipay.Repository.CommisionRepo;
using Ezipay.Utility.common;
using Ezipay.ViewModel.CommisionViewModel;
using Ezipay.ViewModel.PayMoneyViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.CommisionService
{
    public class SetCommisionService : ISetCommisionService
    {
        private ISetCommisionRepository _setCommisionRepository;
        public SetCommisionService()
        {
            _setCommisionRepository = new SetCommisionRepository();
        }

        public async Task<bool> SetCommision(CommissionRequest request)
        {
            var response = new bool();
            var objCommission = new CommisionMaster();
            objCommission.CommisionPercent = request.CommisionPercent;
            objCommission.CreatedBy = 0;
            objCommission.CreatedDate = DateTime.UtcNow;
            objCommission.IsActive = true;
            objCommission.UpdatedDate = DateTime.UtcNow;
            objCommission.IsDeleted = false;
            objCommission.FlatCharges = request.FlatCharges;
            objCommission.BenchmarkCharges = request.BenchmarkCharges;
            objCommission.WalletServiceId = request.WalletServiceId;

            response = await _setCommisionRepository.SetCommision(objCommission);
            return response;
        }

        public async Task<CalculateCommissionResponse> CalculateCommission(CalculateCommissionRequest request)
        {
            var response = new CalculateCommissionResponse();
            response = await _setCommisionRepository.CalculateCommission(request);
            return response;
        }

        public async Task<CalculateCommissionResponse> CalculateAddMoneyCommission(CalculateCommissionRequest request)
        {
            var response = new CalculateCommissionResponse();
            response = await _setCommisionRepository.CalculateAddMoneyCommission(request);
            return response;
        }

        public async Task<CommissionCalculationResponse> CalculateCommission(decimal rate, int ServiceId, string amount, decimal flatCharges = 0, decimal benchmarkCharges = 0)
        {
            var res = new CommissionCalculationResponse();
            res.CommissionServiceId = ServiceId;
            decimal Amount = Convert.ToDecimal(amount);
            res.Rate = rate;
            try
            {

                if (Amount > 0)
                {
                    res.CommissionAmount = Convert.ToString(Math.Round(((Amount * rate) / 100 + flatCharges + benchmarkCharges), 2));
                    res.AmountWithCommission = Convert.ToString(Math.Round(Convert.ToDecimal(Amount) + Convert.ToDecimal(res.CommissionAmount), 2));
                    res.AfterDeduction = Convert.ToString(Math.Round(Convert.ToDecimal(Amount), 2));// - Convert.ToDecimal(res.CommissionAmount), 2));
                }
            }
            catch (Exception ex)
            {
                res.AmountWithCommission = Convert.ToString(Math.Round(Convert.ToDecimal(Amount), 2));
                res.AfterDeduction = res.AmountWithCommission;
            }
            return res;
        }

       
    }
}
