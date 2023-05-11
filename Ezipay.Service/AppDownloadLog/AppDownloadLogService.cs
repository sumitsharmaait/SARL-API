using ezeePay.Utility.Enums;
using Ezipay.Database;
using Ezipay.Repository;
using Ezipay.Utility.SendPush;
using Ezipay.ViewModel;
using Ezipay.ViewModel.SendPushViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ezipay.Service
{
    public class AppDownloadLogService : IAppDownloadLogService
    {
        private readonly IAppDownloadLogRepository _appDownloadLogRepository;
        private readonly ISendPushNotification _sendPushNotification;

        public AppDownloadLogService()
        {
            _appDownloadLogRepository = new AppDownloadLogRepository();
            _sendPushNotification = new SendPushNotification();
        }

        public async Task<AppDownloadSearchResponse> GetDownloadLogList(AppDownloadSearchVM request)
        {
            return await _appDownloadLogRepository.GetDownloadLogList(request);
        }

        public async Task<int> InsertLog(AppDownloadLogRequest request)
        {
            int result = 0;
            var re = await _appDownloadLogRepository.IsDeviceIdExist(request.DeviceUniqueId, request.DeviceToken);
            if (!re)
            {
                var entity = new AppDownloadLog
                {
                    DeviceId = request.DeviceUniqueId,
                    DeviceToken = request.DeviceToken,
                    DeviceType = request.DeviceType,
                    Status = 0,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };
                await _appDownloadLogRepository.Insert(entity);
                result = 1;
            }
            return result;
        }

        public async Task<int> SendNotification(SendNotificationRequest request)
        {
            int result = 1;



            var dataModel = new AppDownloadSearchVM
            {
                PageNumber = 1,
                PageSize = request.TotalCount
            };
            var data = await _appDownloadLogRepository.GetDownloadLogList(dataModel);

            data.DataList.ForEach(x =>
            {
                if (!string.IsNullOrEmpty(x.DeviceToken) && x.DeviceToken != x.DeviceUniqueId)
                {
                    NotificationDefaultKeys pushModel = new NotificationDefaultKeys();
                    pushModel.alert = request.MessageBody;
                    pushModel.pushType = (int)PushType.ANONYMOUSUSER;

                    PushNotificationModel push = new PushNotificationModel();
                    push.deviceType = (int)x.DeviceType;
                    push.deviceKey = x.DeviceToken;
                    if ((int)x.DeviceType == (int)DeviceTypes.ANDROID)
                    {
                        PushPayload<NotificationDefaultKeys> aps = new PushPayload<NotificationDefaultKeys>();
                        PushPayloadData<NotificationDefaultKeys> _data = new PushPayloadData<NotificationDefaultKeys>();
                        _data.notification = pushModel;
                        aps.data = _data;

                        aps.to = x.DeviceToken;
                        aps.collapse_key = string.Empty;
                        push.message = JsonConvert.SerializeObject(aps);

                    }
                    if ((int)x.DeviceType == (int)DeviceTypes.IOS)
                    {
                        NotificationJsonResponse<NotificationDefaultKeys> aps = new NotificationJsonResponse<NotificationDefaultKeys>();
                        aps.aps = pushModel;

                        push.message = JsonConvert.SerializeObject(aps);
                    }

                    _sendPushNotification.sendPushNotification(push);
                }
            });
            return result;
        }



        public async Task<AppDownloadSearchResponse> GetActiveUserForNotification(AppDownloadSearchVM request)
        {
            return await _appDownloadLogRepository.GetActiveUserForNotification(request);
        }
        public async Task<int> SendNotificationForActiveUser(SendNotificationRequest request)
        {
            int result = 1;
            var dataModel = new AppDownloadSearchVM
            {
                PageNumber = 1,
                PageSize = request.TotalCount
            };
            var data = await _appDownloadLogRepository.GetActiveUserForNotification(dataModel);
            if (data != null) //direct send notofication to web user in ring icon,insert 1 baar
            {
                var entity = new Notificationalert
                {
                    MessageBody = request.MessageBody,
                    Title = request.Title,
                    FileUpload = request.FileUpload,
                    CreatedDate = DateTime.UtcNow
                };
                //
                await _appDownloadLogRepository.InsertNotificationalertforweb(entity);
            }

            data.DataList.ForEach(x =>
            {
                //if (!string.IsNullOrEmpty(x.DeviceToken) && x.DeviceToken != x.DeviceUniqueId)
                if (!string.IsNullOrEmpty(x.DeviceToken))
                {
                    NotificationDefaultKeys pushModel = new NotificationDefaultKeys();
                    pushModel.alert = request.MessageBody;
                    pushModel.pushType = (int)PushType.ANONYMOUSUSER;

                    PushNotificationModel push = new PushNotificationModel();
                    push.deviceType = (int)x.DeviceType;
                    push.deviceKey = x.DeviceToken;
                    if ((int)x.DeviceType == (int)DeviceTypes.ANDROID)
                    {
                        PushPayload<NotificationDefaultKeys> aps = new PushPayload<NotificationDefaultKeys>();
                        PushPayloadData<NotificationDefaultKeys> _data = new PushPayloadData<NotificationDefaultKeys>();
                        _data.notification = pushModel;
                        aps.data = _data;

                        aps.to = x.DeviceToken;
                        aps.collapse_key = string.Empty;
                        push.message = JsonConvert.SerializeObject(aps);

                    }
                    if ((int)x.DeviceType == (int)DeviceTypes.IOS)
                    {
                        NotificationJsonResponse<NotificationDefaultKeys> aps = new NotificationJsonResponse<NotificationDefaultKeys>();
                        aps.aps = pushModel;
                        push.message = JsonConvert.SerializeObject(aps);
                    }

                    _sendPushNotification.sendPushNotification(push);
                }
            });
            return result;
        }


        public async Task<List<SendNotificationResponse>> GetCurrentWebNotification()
        {
            return await _appDownloadLogRepository.GetCurrentWebNotification();
        }

        public async Task<List<CountNotificationRequest>> GetCountCurrentWebNotification(CountNotificationRequest re)
        {
            return await _appDownloadLogRepository.GetCountCurrentWebNotification(re);

        }


        public async Task<int> UpdateCurrentWebNotification(notificationupdateRequest request)
        {
            int result = 0;

            result = await _appDownloadLogRepository.UpdateCurrentWebNotification(request);
            
            return result;
        }

    }
}
