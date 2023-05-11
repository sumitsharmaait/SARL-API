using Ezipay.Database;
using Ezipay.Repository.AdminRepo.Commission;
using Ezipay.ViewModel.CommisionViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.Admin.Commission
{
    public class CommissionService : ICommissionService
    {
        private readonly ICommissionRepository _commissionRepository;
        public CommissionService()
        {
            _commissionRepository = new CommissionRepository();
        }
        public async Task<bool> SetCommission(CommissionRequest request)
        {
            bool result = false;

            var commission = await _commissionRepository.GetCommissionByWalletServiceId(request.WalletServiceId);
           
            commission.ForEach(x =>
            {
                x.IsActive = false;
            });

            CommisionMaster objCommission = new CommisionMaster();
            objCommission.CommisionPercent = request.CommisionPercent;
            objCommission.CreatedBy = 0;
            objCommission.CreatedDate = DateTime.UtcNow;
            objCommission.IsActive = true;
            objCommission.UpdatedDate = DateTime.UtcNow;
            objCommission.IsDeleted = false;
            objCommission.FlatCharges = request.FlatCharges;
            objCommission.BenchmarkCharges = request.BenchmarkCharges;
            objCommission.WalletServiceId = request.WalletServiceId;
            objCommission.VATCharges = request.VATCharges;
            if (await _commissionRepository.InsertCommission(objCommission, commission) > 0)
            {
                result = true;
            }

            return result;
        }
    }
}
