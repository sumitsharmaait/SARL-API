using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.ViewModel.common;
using Ezipay.ViewModel.SendPushViewModel;
using Ezipay.ViewModel.TokenViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static Ezipay.Utility.common.AppSetting;

namespace Ezipay.Utility.SendPush
{
    public class WebPushNotification
    {
       
        public Response<bool> WebPush(WebPushNotificationModel model)
        {

            Response<bool> res = new Response<bool>();

            try
            {
               
                   // var sender = new AppUserRepository().UserProfile(model.SenderId);
                    if (model.deviceKey!=null && model!=null)
                    {
                       // var receiver = db.WalletUsers.Where(x => x.DeviceToken == model.deviceKey && x.DeviceType == model.deviceType).FirstOrDefault();
                        if (model != null)
                        {
                            string resString = JsonConvert.SerializeObject(model.payload);
                            var keys = JsonConvert.DeserializeObject<ChatModel>(resString);
                            if (keys != null)
                            {

                                var AdminKeys = AES256.AdminKeyPair;
                                keys.ReceiverId = model.RecieverId;
                                keys.SenderId = model.SenderId;

                                var _Application = new ApplicationUrl();
                                string url = _Application.Domain() + "/api/PushNotification";
                                HttpWebRequest saveCreditrequest = (HttpWebRequest)WebRequest.Create(url);
                              //  var Token = new TokenRepository().GenerateTempToken(new TempTokenRequest { DeviceUniqueId = keys.ReceiverId });
                                if (model.Token != null)
                                {
                                    saveCreditrequest.Headers.Add("Token", model.Token);
                                }
                                saveCreditrequest.ContentType = "application/json; charset=utf-8";
                                saveCreditrequest.Method = "POST";
                                var postData = JsonConvert.SerializeObject(keys);
                                using (var streamWriter = new StreamWriter(saveCreditrequest.GetRequestStream()))
                                {
                                    streamWriter.Write(postData);
                                    streamWriter.Flush();
                                    streamWriter.Close();
                                }
                                using (var _response = (HttpWebResponse)saveCreditrequest.GetResponse())
                                {
                                    using (var result = new StreamReader(_response.GetResponseStream()))
                                    {
                                        var data = result.ReadToEnd();
                                        if (!string.IsNullOrEmpty(data))
                                        {
                                            res = JsonConvert.DeserializeObject<Response<bool>>(data);

                                        }
                                    }
                                }
                            }
                        }
                    }
                

            }
            catch (Exception ex)
            {
                (ex.Message).ErrorLog("WebPushNotification", "WebPush exception");
                res.message = ex.Message;

            }

            return res;

        }
    }
}
