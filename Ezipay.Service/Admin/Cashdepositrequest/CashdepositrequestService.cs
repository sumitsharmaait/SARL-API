using Ezipay.Repository.AdminRepo.Cashdepositrequest;
using Ezipay.ViewModel.AdminViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ezipay.Service.Admin.Cashdepositrequest
{
    public class CashdepositrequestService : ICashdepositrequestService
    {
        private readonly ICashdepositrequestRespository _CashdepositrequestRepository;

        public CashdepositrequestService()
        {
            _CashdepositrequestRepository = new CashdepositrequestRepository();
        }

        public async Task<List<CashdepositrequestResponse>> Getcashdepositrequest(CashdepositrequestRequest cr)
        {
            return await _CashdepositrequestRepository.Getcashdepositrequest(cr);
        }



        public async Task<bool> Updatecashdepositrequest(CashdepositrequestRequest request)
        {
            var result = false;

            int rowAffected = await _CashdepositrequestRepository.Updatecashdepositrequest(request);
            if (rowAffected > 0)
            {
                result = true;
            }

            return result;
        }


    }
}
