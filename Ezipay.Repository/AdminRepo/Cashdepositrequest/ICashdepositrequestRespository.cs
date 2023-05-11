
using Ezipay.Database;
using Ezipay.ViewModel.AdminViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.AdminRepo.Cashdepositrequest
{
    public interface ICashdepositrequestRespository
    {
        Task<int> Updatecashdepositrequest(CashdepositrequestRequest entity);
        
        Task<List<CashdepositrequestResponse>> Getcashdepositrequest(CashdepositrequestRequest cr);
  
    }
}
