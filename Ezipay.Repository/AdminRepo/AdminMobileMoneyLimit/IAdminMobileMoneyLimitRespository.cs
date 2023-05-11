
using Ezipay.ViewModel.AdminViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.AdminRepo.AdminMobileMoneyLimit
{
    public interface IAdminMobileMoneyLimitRespository
    {
        Task<int> InsertAdminMobileMoneyLimit(Database.AdminMobileMoneyLimit entity, string deleteFlag);
        Task<List<AdminMobileMoneyLimitResponse>> GetAdminMobileMoneyLimit();
    }
}
