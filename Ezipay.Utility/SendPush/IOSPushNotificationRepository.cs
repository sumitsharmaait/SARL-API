using Ezipay.ViewModel.SendPushViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Ezipay.Utility.SendPush
{
    public class IOSPushNotificationRepository
    {
        #region IOS PushNotfication

        SslStream sslStream;

        public bool IOSPushNotification(PushNotificationModel objPush)
        {
            try
            {
                string p12fileName = ConfigurationManager.AppSettings["iOSPushCertificate"];
                string password = ConfigurationManager.AppSettings["iOSPushPassword"]; // objPush.IOSAuthPassword;


                X509Certificate2Collection certs = new X509Certificate2Collection();
                certs.Add(getServerCert(p12fileName, password));
                string apsHost;

                apsHost = ConfigurationManager.AppSettings["iOSPushApsHost"];

                using (TcpClient tcpClient = new TcpClient(apsHost, 2195))
                {
                    // Create a new SSL stream over the connection
                    sslStream = new SslStream(tcpClient.GetStream());
                    // Authenticate using the Apple cert
                    sslStream.AuthenticateAsClient(apsHost, certs, SslProtocols.Default, false);

                    Int32 messa = objPush.message.Length;
                    Boolean send = PushMessage(objPush);
                    return send;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private X509Certificate getServerCert(string p12File, string password)
        {
            try
            {
                X509Certificate test = new X509Certificate();
                string p12Filename = HttpContext.Current.Server.MapPath("~/" + p12File);
                //string p12Filename = HttpContext.Current.Server.MapPath("/" + p12File);
                test = new X509Certificate2(System.IO.File.ReadAllBytes(p12Filename), password, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
                return test;
            }
            catch (Exception ex)
            {
                //
                throw new Exception(ex.Message + ", " + ex.StackTrace.Substring(100));
            }
        }

        private static byte[] HexToData(string hexString)
        {
            try
            {
                if (hexString == null)
                    return null;

                if (hexString.Length % 2 == 1)
                    hexString = '0' + hexString; // Up to you whether to pad the first or last byte

                byte[] data = new byte[hexString.Length / 2];

                for (int i = 0; i < data.Length; i++)
                    data[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);

                return data;
            }
            catch
            {
                throw new Exception("Token not in correct formate.");
            }
        }

        private bool PushMessage(PushNotificationModel objPush)
        {
            try
            {
                String cToken = objPush.deviceKey;
                String cAlert = objPush.message;


                // Ready to create the push notification
                byte[] buf = new byte[256];
                MemoryStream ms = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(ms);
                bw.Write(new byte[] { 0, 0, 32 });

                byte[] deviceToken = HexToData(cToken);
                bw.Write(deviceToken);

                bw.Write((byte)0);

                // Create the APNS payload - new.caf is an audio file saved in the application bundle on the device                
                string msg = objPush.message;
                //string msg = "{\"aps\":{\"alert\":\"" + objPush.message + "\"}}";
                // Write the data out to the stream
                bw.Write((byte)msg.Length);
                bw.Write(msg.ToCharArray());
                bw.Flush();

                if (sslStream != null)
                {
                    sslStream.Write(ms.ToArray());

                    return true;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ", " + ex.StackTrace.Substring(100));
            }
        }
        #endregion
    }
}
