using Ezipay.ViewModel.SendPushViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Utility.SendPush
{
    public interface ISendPushNotification
    {
        void SendLogoutPush(SendPushRequest request, string DeviceToken);

        bool sendPushNotification(PushNotificationModel objPush);
    }
}
