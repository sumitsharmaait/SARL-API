using Ezipay.Repository.AdminRepo.AdminMobileMoneyLimit;
using Ezipay.ViewModel.AdminViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ezipay.Service.Admin.AdminMobileMoneyLimit
{
    public class AdminMobileMoneyLimitService : IAdminMobileMoneyLimitService
    {
        private readonly IAdminMobileMoneyLimitRespository _AdminMobileMoneyLimitRepository;

        public AdminMobileMoneyLimitService()
        {
            _AdminMobileMoneyLimitRepository = new AdminMobileMoneyLimitRepository();
        }

        public async Task<List<AdminMobileMoneyLimitResponse>> GetAdminMobileMoneyLimit()
        {
            return await _AdminMobileMoneyLimitRepository.GetAdminMobileMoneyLimit();
        }

        public async Task<bool> InsertAdminMobileMoneyLimit(AdminMobileMoneyLimitRequest request)
        {
            var result = false;
            var deleteFlag = request.flag;
            if (deleteFlag != null)
            {
                var entity = new Database.AdminMobileMoneyLimit
                {
                    Id = request.Id
                  
                };
                int rowAffected = await _AdminMobileMoneyLimitRepository.InsertAdminMobileMoneyLimit(entity, deleteFlag);
                if (rowAffected > 0)
                {
                    result = true;
                }
            }
            else
            {
                var entity = new Database.AdminMobileMoneyLimit
                {
                    MaximumAmount=request.MaximumAmount.Trim(),
                    ServiceCode=request.Service.Trim(),
                    MinimumAmount = request.MinimumAmount.Trim(),
                    MinimumCharges = request.MinimumCharges.Trim(),
                    
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                };

                int rowAffected = await _AdminMobileMoneyLimitRepository.InsertAdminMobileMoneyLimit(entity, deleteFlag);
                if (rowAffected > 0)
                {
                    result = true;
                }
            }
            return result;
        }


    }
}
