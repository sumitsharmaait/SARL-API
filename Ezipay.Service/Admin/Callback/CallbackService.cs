using Ezipay.Database;
using Ezipay.Repository.AdminRepo.Callback;
using Ezipay.ViewModel.AdminViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.Admin.Callback
{
    public class CallbackService : ICallbackService
    {
        private ICallbackRepository _callbackRepository;
        public CallbackService()
        {
            _callbackRepository = new CallbackRepository();
        }

        public async Task<CallbackResponse> GetCallbackList(SearchRequest request)
        {
            var result = new CallbackResponse();

            result.CallbackList = await _callbackRepository.GetCallbackList(request);
            if (result.CallbackList.Count > 0)
            {
                result.TotalCount = result.CallbackList[0].TotalCount;
            }
            return result;

        }

        public async Task<int> UpdateCallBackStatus(UpdateCallbackRequest request)
        {
            int result = 0;
            var callback = await _callbackRepository.GetCallbackById(request.CallbackId);
            if (callback != null)
            {
                callback.AcceptedBy = request.AdminId;
                callback.Status = request.Status;
                callback.UpdatedDate = DateTime.UtcNow;
                result = await _callbackRepository.UpdateCallback(callback);
                if (result > 0)
                {
                   await InsertTracking(callback);
                }
            }

            return result;
        }

        private async Task InsertTracking(Database.Callback callback)
        {
            var entity = new CallbackListTracking
            {
                CallBackId = callback.CallbackId,
                CeatedBy = callback.AcceptedBy,
                CreatedOn = DateTime.UtcNow,
                Status = callback.Status
            };
           int rowAffected= await _callbackRepository.InsertCallbackLog(entity);
        }
    }
}
