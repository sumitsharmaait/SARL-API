using ezeePay.Utility.CommonClass;
using ezeePay.Utility.Enums;
using Ezipay.Utility.common;
using Ezipay.ViewModel.SendPushViewModel;
using Ezipay.ViewModel.TokenViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Utility.SendPush
{
    public class SendPushNotification : ISendPushNotification
    {
        
        public void SendLogoutPush(SendPushRequest request,string DeviceToken)
        {
            // var UserData = DbContext.WalletUsers.Where(x => x.WalletUserId == WalletUserId).FirstOrDefault();
            if (request != null)
            {
                #region PushNotification

                if (!string.IsNullOrEmpty(request.DeviceToken) && DeviceToken != request.DeviceUniqueId)
                {
                    if (request.DeviceType != (int)DeviceTypes.Web)
                    {
                        NotificationDefaultKeys pushModel = new NotificationDefaultKeys();
                        pushModel.alert = ResponseMessages.UNATHORIZED_REQUEST;
                        pushModel.pushType = (int)PushType.LOGOUT;

                        PushNotificationModel push = new PushNotificationModel();
                        push.deviceType = (int)request.DeviceType;
                        push.deviceKey = request.DeviceToken;
                        if ((int)request.DeviceType == (int)DeviceTypes.ANDROID)
                        {
                            PushPayload<NotificationDefaultKeys> aps = new PushPayload<NotificationDefaultKeys>();
                            PushPayloadData<NotificationDefaultKeys> _data = new PushPayloadData<NotificationDefaultKeys>();
                            _data.notification = pushModel;
                            aps.data = _data;

                            aps.to = request.DeviceToken;
                            aps.collapse_key = string.Empty;
                            push.message = JsonConvert.SerializeObject(aps);

                        }
                        if ((int)request.DeviceType == (int)DeviceTypes.IOS)
                        {
                            NotificationJsonResponse<NotificationDefaultKeys> aps = new NotificationJsonResponse<NotificationDefaultKeys>();
                            aps.aps = pushModel;

                            push.message = JsonConvert.SerializeObject(aps);
                        }
                       sendPushNotification(push);
                    }
                    else
                    {
                        var keys = AES256.AdminKeyPair;
                        var webPush = new ChatModel();

                        webPush.ReceiverId = request.MobileNo;
                        webPush.pushType = (int)PushType.LOGOUT;
                        webPush.alert = ResponseMessages.UNATHORIZED_REQUEST;
                        webPush.Message = ResponseMessages.UNATHORIZED_REQUEST;




                        //var _result = WebLogout(webPush);
                        //if (_result != null)
                        //{
                        //    var pushModel = new PushNotificationModel();
                        //    pushModel.deviceKey = request.DeviceUniqueId;
                        //    pushModel.deviceType = (int)DeviceTypes.Web;
                        //    pushModel.message = string.Empty;
                        //    pushModel.PushType = (int)PushType.LOGOUT;
                        //    pushModel.SenderId = request.WalletUserId;
                        //    pushModel.message = JsonConvert.SerializeObject(webPush);

                        //    // SaveNotification(pushModel, request.WalletUserId, _result.isSuccess);
                        //}
                    }

                }
                #endregion
            }
        }
        
        public bool sendPushNotification(PushNotificationModel objPush)
        {

            bool isSuccess = false;
            try
            {
                objPush.message = objPush.message.Replace("\\", "");

                if (!string.IsNullOrEmpty(objPush.deviceKey))
                {
                    if (objPush.deviceType == (int)DeviceTypes.IOS)
                    {
                        isSuccess = (new IOSPushNotificationRepository().IOSPushNotification(objPush));
                        //isSuccess = new PushNotificationIOS().IOSPushNotification(objPush);
                    }
                    else if (objPush.deviceType == (int)DeviceTypes.ANDROID)
                    {

                        var data = (new AndroidPushNotification().FireBasePush(objPush));
                        isSuccess = (data != null && data.success == 1);

                    }
                    else if (objPush.deviceType == (int)DeviceTypes.Web)
                    {

                        //var res = new WebPushNotification().WebPush(objPush);
                        //isSuccess = res.isSuccess;
                    }
                   // SaveNotification(objPush, isSuccess);
                }


            }
            catch (Exception ex)
            {

               // ex.Message.ErrorLog("PushNotificationRepository.cs", "sendPushNotification", objPush);
            }
            return isSuccess;
        }
    }
}
