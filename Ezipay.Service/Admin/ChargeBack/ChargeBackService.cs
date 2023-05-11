using Ezipay.Database;
using Ezipay.ViewModel.AdminViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ezipay.Repository.AdminRepo.ChargeBack
{
    public class ChargeBackService : IChargeBackService
    {
        private readonly IChargeBackRepository _ChargeBackRepository;

        public ChargeBackService()
        {
            _ChargeBackRepository = new ChargeBackRepository();
        }

        public async Task<List<Database.ChargeBack>> GetChargeBackList()
        {
            return await _ChargeBackRepository.GetChargeBackList();
        }

        public async Task<List<Database.ChargeBack>> GetChargeBackListById(ChargeBackRequest request)
        {
            return await _ChargeBackRepository.GetChargeBackListById(request);
        }

        public async Task<bool> InsertChargeBackDetail(ChargeBackRequest request)
        {
            var result = false;

            int rowAffected = await _ChargeBackRepository.InsertChargeBackDetail(request);
            if (rowAffected > 0)
            {
                result = true;
            }
            return result;
        }

        /////
        ///
        public async Task<List<Database.freezeuser>> GetfreezeList()
        {
            return await _ChargeBackRepository.GetfreezeList();
        }


        public async Task<bool> InsertfreezeDetail(freezeRequest request)
        {
            var result = false;

            int rowAffected = await _ChargeBackRepository.InsertfreezeDetail(request);
            if (rowAffected > 0)
            {
                result = true;
            }
            return result;
        }
        public async Task<List<Database.freezeuser>> GetfreezeById(long Walletuserid)
        {
            return await _ChargeBackRepository.GetfreezeById(Walletuserid);
        }


    }
}

