using Ezipay.ViewModel.CommisionViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.Admin.Commission
{
    public interface ICommissionService
    {
        Task<bool> SetCommission(CommissionRequest request);
    }
}
