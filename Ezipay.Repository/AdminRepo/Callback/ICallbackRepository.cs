using Ezipay.Database;
using Ezipay.ViewModel.AdminViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.AdminRepo.Callback
{
    public interface ICallbackRepository
    {
        Task<List<CallbackRecord>> GetCallbackList(SearchRequest request);
        Task<Database.Callback> GetCallbackById(int callbackId);
        Task<int> UpdateCallback(Database.Callback callback);
        Task<int> InsertCallbackLog(CallbackListTracking entity);
    }
}
