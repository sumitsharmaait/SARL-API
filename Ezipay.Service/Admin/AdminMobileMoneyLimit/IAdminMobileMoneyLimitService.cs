using Ezipay.ViewModel.AdminViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ezipay.Service.Admin.AdminMobileMoneyLimit
{
    public interface IAdminMobileMoneyLimitService
    {
        Task<bool> InsertAdminMobileMoneyLimit(AdminMobileMoneyLimitRequest request);

        Task<List<AdminMobileMoneyLimitResponse>> GetAdminMobileMoneyLimit();

    }
}
