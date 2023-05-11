using Ezipay.ViewModel.AdminViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.AdminRepo.ChargeBack
{
    public interface IChargeBackRepository
    {
        Task<int> InsertChargeBackDetail(ChargeBackRequest request);
        Task<List<Database.ChargeBack>> GetChargeBackList();
        Task<List<Database.ChargeBack>> GetChargeBackListById(ChargeBackRequest request);

        Task<List<Database.freezeuser>> GetfreezeList();
        Task<int> InsertfreezeDetail(freezeRequest request);
        Task<List<Database.freezeuser>> GetfreezeById(long Walletuserid);
    }
}
