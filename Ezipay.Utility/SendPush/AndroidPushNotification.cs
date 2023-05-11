using Ezipay.ViewModel.SendPushViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Utility.SendPush
{
    public class AndroidPushNotification
    {
        public bool AndroidPushNotifications(PushNotificationModel objPush)
        {
            // msg = "order confirmed";
            WebRequest tRequest;
            tRequest = WebRequest.Create(ConfigurationManager.AppSettings["androidPushDomain"]);
            tRequest.Method = "post";
            tRequest.ContentType = "application/x-www-form-urlencoded";
            string resistrationId = objPush.deviceKey;
            tRequest.Headers.Add(string.Format("Authorization: key={0}", ConfigurationManager.AppSettings["androidPushPassword"]));
            String collaspeKey = Guid.NewGuid().ToString("n");
            string messagetosend = objPush.message;
            String postData = string.Format("registration_id={0}&data.payload={1}&collapse_key={2}", resistrationId, "" + messagetosend, collaspeKey);
            Byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            tRequest.ContentLength = byteArray.Length;

            Stream dataStream = tRequest.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            WebResponse tResponse = tRequest.GetResponse();

            dataStream = tResponse.GetResponseStream();

            StreamReader tReader = new StreamReader(dataStream);

            String sResponseFromServer = tReader.ReadToEnd();

            tReader.Close();
            dataStream.Close();
            tResponse.Close();
            if (!string.IsNullOrEmpty(sResponseFromServer) && sResponseFromServer.Contains("id"))
            {
                return true;

            }
            else
            {
                return false;
            }
        }

        public FcmPushResponse FireBasePush(PushNotificationModel objPush)
        {
            //"Request".ErrorLog("PushNotificationRepository.cs", "FireBasePush Request", objPush);
            FcmPushResponse response = new FcmPushResponse();
            try
            {
                WebRequest tRequest = WebRequest.Create(ConfigurationManager.AppSettings["FCM_HOST_URL"]);
                tRequest.Method = "post";
                tRequest.ContentType = "application/json";
                Byte[] byteArray = Encoding.UTF8.GetBytes(objPush.message);
                tRequest.Headers.Add(string.Format("Authorization: key={0}", ConfigurationManager.AppSettings["FCM_APPLICATION_KEY"]));
                tRequest.Headers.Add(string.Format("Sender: id={0}", ConfigurationManager.AppSettings["FCM_SERVER_KEY"]));
                tRequest.ContentLength = byteArray.Length;
                tRequest.ContentType = "application/json";
                using (Stream dataStream = tRequest.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    using (WebResponse tResponse = tRequest.GetResponse())
                    {
                        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                        {
                            using (StreamReader tReader = new StreamReader(dataStreamResponse))
                            {
                                String sResponseFromServer = tReader.ReadToEnd();
                                response = JsonConvert.DeserializeObject<FcmPushResponse>(sResponseFromServer);
                                // "Response".ErrorLog("PushNotificationRepository.cs", "FireBasePush Response", response);
                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                var sss = ex.Message;
                if (ex.InnerException != null)
                {
                    var ss = ex.InnerException;
                }
                // ex.Message.ErrorLog("PushNotificationRepository.cs", "FireBasePush EXCEPTION", objPush);                
            }
            return response;
        }
    }
}
