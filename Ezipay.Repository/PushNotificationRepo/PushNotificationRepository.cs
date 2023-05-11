using ezeePay.Utility.Enums;
using Ezipay.Database;
using Ezipay.Repository.TokenRepo;
using Ezipay.Repository.UserRepo;
using Ezipay.Utility.Extention;
using Ezipay.Utility.SendPush;
using Ezipay.ViewModel.common;
using Ezipay.ViewModel.SendPushViewModel;
using Ezipay.ViewModel.TokenViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.PushNotificationRepo
{
    public class PushNotificationRepository : IPushNotificationRepository
    {
        private ITokenRepository _tokenRepository;
        private IWalletUserRepository _walletUserRepository;
        public PushNotificationRepository()
        {
            _tokenRepository = new TokenRepository();
            _walletUserRepository = new WalletUserRepository();
        }

        /// <summary>
        /// WebLogout
        /// </summary>
        /// <returns></returns>
        public async Task<bool> WebLogout(ChatModel model)
        {
            bool _result = new bool();
            string url = ConfigurationManager.AppSettings["WebSocketUrl"];
            try
            {

                HttpWebRequest saveCreditrequest = (HttpWebRequest)WebRequest.Create(url);
                var TokenData = await _tokenRepository.GenerateTempToken(new TempTokenRequest { DeviceUniqueId = model.ReceiverId });
                if (TokenData != null)
                {
                    saveCreditrequest.Headers.Add("Token", TokenData.Token);
                }
                saveCreditrequest.ContentType = "application/json; charset=utf-8";
                saveCreditrequest.Method = "POST";
                string postSaveCreditData = JsonConvert.SerializeObject(model);
                using (var streamWriter = new StreamWriter(saveCreditrequest.GetRequestStream()))
                {
                    streamWriter.Write(postSaveCreditData);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                using (var finalResponse = (HttpWebResponse)saveCreditrequest.GetResponse())
                {
                    using (var ResponseReader = new StreamReader(finalResponse.GetResponseStream()))
                    {
                        var ResponseString = ResponseReader.ReadToEnd();

                        //TransferToBankSubmitCreditResponce _result = (TransferToBankSubmitCreditResponce)js.Deserialize(submitCredit_objText, typeof(TransferToBankSubmitCreditResponce));
                        _result = JsonConvert.DeserializeObject<bool>(ResponseString);

                    }
                }


            }
            catch (Exception ex)
            {
                //(ex.Message).ErrorLog("TokenRepository", "WebLogout", model);
                //(ex.Message).ErrorLog("TokenRepository", "WebLogout", url);

            }
            return _result;



        }


        /// <summary>
        /// SaveNotification
        /// </summary>
        /// <param name="objPush"></param>
        /// <param name="IsDeliverd"></param>
        public async Task<int> SaveNotification(Notification notification)
        {
            int result = 0;
            try
            {
                using (DB_9ADF60_ewalletEntities db = new DB_9ADF60_ewalletEntities())
                {

                    db.Notifications.Add(notification);
                    result = await db.SaveChangesAsync();
                }

            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("PushNotificationRepository", "SaveNotification", notification);
            }
            return result;
        }


        /// <summary>
        /// Send Notification
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<bool> sendPushNotification(PushNotificationModel objPush)
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
                        var deviceUniqueId = new TempTokenRequest { DeviceUniqueId = objPush.deviceKey };
                        var reciever = await _walletUserRepository.GetUserPushDetailById(objPush.deviceKey, objPush.deviceType);
                        var Sender = await _walletUserRepository.GetUserDetailById(objPush.SenderId);
                        var token = await _tokenRepository.GenerateTempToken(deviceUniqueId);
                        var req = new WebPushNotificationModel
                        {
                            deviceType = (int)reciever.DeviceType,
                            deviceKey = reciever.DeviceToken,
                            message = objPush.message,
                            RecieverId = reciever.MobileNo,
                            payload = "",
                            PushType = 0,
                            SenderId = Sender.MobileNo
                        };
                        var res = new WebPushNotification().WebPush(req);
                        isSuccess = res.isSuccess;


                    }
                    SaveNotification(objPush, isSuccess);
                }


            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("PushNotificationRepository.cs", "sendPushNotification", objPush);
            }
            return isSuccess;
        }

        /// <summary>
        /// Send Multiple Notifications
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public bool sendPushNotification(List<PushNotificationModel> request)
        {
            bool isSuccess = false;
            try
            {
                foreach (var objPush in request)
                {

                    if (objPush.deviceType == (int)DeviceTypes.IOS)
                    {
                        isSuccess = (new IOSPushNotificationRepository().IOSPushNotification(objPush));
                    }
                    else if (objPush.deviceType == (int)DeviceTypes.ANDROID)
                    {
                        //isSuccess = (new AndroidPushNotificationRespository().AndroidPushNotification(objPush));
                        var data = (new AndroidPushNotification().FireBasePush(objPush));
                        isSuccess = (data != null && data.success == 1);

                    }

                }
                if (isSuccess)
                {

                }
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("PushNotificationRepository.cs", "sendPushNotification", request);
            }
            return isSuccess;
        }

        /// <summary>
        /// SaveNotification
        /// </summary>
        /// <param name="objPush"></param>
        /// <param name="IsDeliverd"></param>
        public async Task<int> SaveNotification(PushNotificationModel objPush, bool IsDeliverd, DB_9ADF60_ewalletEntities db)
        {
            int res = 0;
            try
            {

                var sender = await _walletUserRepository.GetUserDetailById(objPush.SenderId);
                if (sender != null && sender.WalletUserId > 0)
                {
                    var receiver = db.WalletUsers.Where(x => x.DeviceToken == objPush.deviceKey && x.DeviceType == objPush.deviceType).FirstOrDefault();
                    if (receiver != null)
                    {
                        var keys = JsonConvert.DeserializeObject<NotificationDefaultKeys>(objPush.message);
                        if (keys != null)
                        {
                            DateTime date = DateTime.UtcNow;
                            var _notification = new Notification();
                            _notification.AlterMessage = keys.alert;
                            _notification.CreatedDate = date;
                            _notification.UpdatedDate = date;
                            _notification.ReceiverId = receiver.WalletUserId;
                            _notification.SenderId = sender.WalletUserId;
                            _notification.NotificationType = keys.pushType;
                            _notification.IsActive = true;
                            _notification.IsRead = false;
                            _notification.IsDelivered = IsDeliverd;
                            _notification.NotificationJson = objPush.message;
                            _notification.IsActive = true;
                            db.Notifications.Add(_notification);
                            res = db.SaveChanges();
                        }
                    }
                }

            }
            catch (Exception ex)
            {


            }
            return res;
        }

        /// <summary>
        /// SaveNotification
        /// </summary>
        /// <param name="objPush"></param>
        /// <param name="IsDeliverd"></param>
        public async Task<int> SaveNotification(PushNotificationModel objPush, bool IsDeliverd)
        {
            int res = 0;
            try
            {
                using (DB_9ADF60_ewalletEntities db = new DB_9ADF60_ewalletEntities())
                {

                    var sender = await _walletUserRepository.GetUserDetailById(objPush.SenderId);
                    if (sender != null && sender.WalletUserId > 0)
                    {
                        var receiver = db.WalletUsers.Where(x => x.DeviceToken == objPush.deviceKey && x.DeviceType == objPush.deviceType).FirstOrDefault();
                        if (receiver != null)
                        {
                            var keys = JsonConvert.DeserializeObject<NotificationDefaultKeys>(objPush.message);
                            if (keys != null)
                            {
                                DateTime date = DateTime.UtcNow;
                                var _notification = new Notification();
                                _notification.AlterMessage = keys.alert;
                                _notification.DeviceToken = objPush.deviceKey;
                                _notification.DeviceType = objPush.deviceType;
                                _notification.CreatedDate = date;
                                _notification.UpdatedDate = date;
                                _notification.IsDeleted = false;
                                _notification.ReceiverId = receiver.WalletUserId;
                                _notification.SenderId = sender.WalletUserId;
                                _notification.NotificationType = keys.pushType;
                                _notification.IsActive = true;
                                _notification.IsRead = false;
                                _notification.IsDelivered = IsDeliverd;
                                _notification.NotificationJson = objPush.message;
                                _notification.IsActive = true;
                                db.Notifications.Add(_notification);
                                res = db.SaveChanges();
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("PushNotificationRepository", "SaveNotification", objPush);

            }
            return res;
        }

        /// <summary>
        /// SaveNotification
        /// </summary>
        /// <param name="objPush"></param>
        /// <param name="IsDeliverd"></param>
        public async Task<int> SaveNotification(PushNotificationModel objPush, long WalletUserId, bool IsDeliverd)
        {
            int res = 0;
            try
            {
                using (DB_9ADF60_ewalletEntities db = new DB_9ADF60_ewalletEntities())
                {



                    var keys = JsonConvert.DeserializeObject<NotificationDefaultKeys>(objPush.message);
                    if (keys != null)
                    {
                        DateTime date = DateTime.UtcNow;
                        var _notification = new Notification();
                        _notification.AlterMessage = keys.alert;
                        _notification.DeviceToken = objPush.deviceKey;
                        _notification.DeviceType = objPush.deviceType;
                        _notification.CreatedDate = date;
                        _notification.UpdatedDate = date;
                        _notification.IsDeleted = false;
                        _notification.ReceiverId = WalletUserId;
                        _notification.SenderId = WalletUserId;
                        _notification.NotificationType = keys.pushType;
                        _notification.IsActive = true;
                        _notification.IsRead = false;
                        _notification.IsDelivered = IsDeliverd;
                        _notification.NotificationJson = objPush.message;
                        _notification.IsActive = true;
                        db.Notifications.Add(_notification);
                        res = await db.SaveChangesAsync();
                    }


                }

            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("PushNotificationRepository", "SaveNotification", objPush);

            }
            return res;
        }
    }
}
