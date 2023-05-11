
using Ezipay.Database;
using Ezipay.ViewModel.AdminViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ezipay.Service.Admin.Cashdepositrequest
{
    public interface ICashdepositrequestService
    {

        Task<List<CashdepositrequestResponse>> Getcashdepositrequest(CashdepositrequestRequest request);
        Task<bool> Updatecashdepositrequest(CashdepositrequestRequest request);    
    }
}
