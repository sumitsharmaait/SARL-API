using Ezipay.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.AdminRepo.Commission
{
    public interface ICommissionRepository
    {
        Task<List<CommisionMaster>> GetCommissionByWalletServiceId(int walletServiceId);
        Task<int> InsertCommission(CommisionMaster objCommission, List<CommisionMaster> commission);
    }
}
