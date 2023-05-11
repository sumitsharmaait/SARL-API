using Ezipay.Database;
using Ezipay.ViewModel.common;
using Ezipay.ViewModel.SendPushViewModel;
using Ezipay.ViewModel.TokenViewModel;
using Ezipay.ViewModel.WalletUserVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.PushNotificationRepo
{
    public interface IPushNotificationRepository
    {
        Task<bool> WebLogout(ChatModel model);

        Task<int> SaveNotification(Notification notification);

        /// <summary>
        /// Send Notification
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<bool> sendPushNotification(PushNotificationModel objPush);
        /// <summary>
        /// Send Multiple Notifications
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        bool sendPushNotification(List<PushNotificationModel> request);
        /// <summary>
        /// SaveNotification
        /// </summary>
        /// <param name="objPush"></param>
        /// <param name="IsDeliverd"></param>
        Task<int> SaveNotification(PushNotificationModel objPush, bool IsDeliverd, DB_9ADF60_ewalletEntities db);
        /// <summary>
        /// SaveNotification
        /// </summary>
        /// <param name="objPush"></param>
        /// <param name="WalletUserId"></param>
        /// <param name="IsDeliverd"></param>
        Task<int> SaveNotification(PushNotificationModel objPush, long WalletUserId, bool IsDeliverd);
        /// <summary>
        /// SaveNotification
        /// </summary>
        /// <param name="objPush"></param>
        /// <param name="IsDeliverd"></param>
        Task<int> SaveNotification(PushNotificationModel objPush, bool IsDeliverd);


    }
}
