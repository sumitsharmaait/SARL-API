using Ezipay.ViewModel.AdminViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.Admin.Callback
{
    public interface ICallbackService
    {
        Task<CallbackResponse> GetCallbackList(SearchRequest request);
        Task<int> UpdateCallBackStatus(UpdateCallbackRequest request);
    }
}
