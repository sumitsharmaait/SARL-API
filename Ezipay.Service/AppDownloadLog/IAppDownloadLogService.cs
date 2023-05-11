using Ezipay.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service
{
    public interface IAppDownloadLogService
    {
        Task<int> InsertLog(AppDownloadLogRequest request);
        Task<AppDownloadSearchResponse> GetDownloadLogList(AppDownloadSearchVM request);
        Task<int> SendNotification(SendNotificationRequest request);
        Task<AppDownloadSearchResponse> GetActiveUserForNotification(AppDownloadSearchVM request);
        Task<int> SendNotificationForActiveUser(SendNotificationRequest request);

        Task<List<SendNotificationResponse>> GetCurrentWebNotification();

        Task<List<CountNotificationRequest>> GetCountCurrentWebNotification(CountNotificationRequest request);
        Task<int> UpdateCurrentWebNotification(notificationupdateRequest request);

    }
}
