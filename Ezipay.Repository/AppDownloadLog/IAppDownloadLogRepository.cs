using Ezipay.Database;
using Ezipay.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository
{
    public interface IAppDownloadLogRepository
    {
        Task<int> Insert(AppDownloadLog entity);
        Task<AppDownloadSearchResponse> GetDownloadLogList(AppDownloadSearchVM request);
        Task<bool> IsDeviceIdExist(string deviceId, string deviceToken);
        Task<AppDownloadSearchResponse> GetActiveUserForNotification(AppDownloadSearchVM request);

        Task<int> InsertNotificationalertforweb(Notificationalert entity);

        Task<List<SendNotificationResponse>> GetCurrentWebNotification();
        Task<List<CountNotificationRequest>> GetCountCurrentWebNotification(CountNotificationRequest re);

        Task<int> UpdateCurrentWebNotification(notificationupdateRequest request);
    }
}
