using Ezipay.ViewModel.AdminViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.AdminRepo.ChargeBack
{
    public interface IChargeBackService
    {
        Task<bool> InsertChargeBackDetail(ChargeBackRequest request);

        Task<List<Database.ChargeBack>> GetChargeBackList();

        Task<List<Database.ChargeBack>> GetChargeBackListById(ChargeBackRequest request);

        Task<List<Database.freezeuser>> GetfreezeList();
        Task<bool> InsertfreezeDetail(freezeRequest request);
    }
}
