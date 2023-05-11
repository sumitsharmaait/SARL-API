using System;
using System.IO;
using System.Text;
using System.Web;

namespace Ezipay.Utility.LogHandler
{
    public class LogUtils : ILogUtils
    {
        public void WriteTextToFile(string message)
        {
            //string filePath = (HttpContext.Current.Server.MapPath("~/Logs/APILog.txt"));
            //using (var stream = File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            //{
            //    byte[] newline = Encoding.ASCII.GetBytes(Environment.NewLine);
            //    stream.Write(newline, 0, newline.Length);
            //    Byte[] info = new UTF8Encoding(true).GetBytes(message);
            //    stream.Write(info, 0, info.Length);
            //    stream.Close();
            //}

            var date = DateTime.UtcNow.ToString("dd-MM-yyyy");
            string fileName = "~/Logs/" + date + "_APILog" + ".txt";
            string filePath = (HttpContext.Current.Server.MapPath(fileName));
            using (var stream = File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                byte[] newline = Encoding.ASCII.GetBytes(Environment.NewLine);
                stream.Write(newline, 0, newline.Length);
                Byte[] info = new UTF8Encoding(true).GetBytes(message);
                stream.Write(info, 0, info.Length);
                stream.Close();
            }
                                 
        }

        public void WriteTextToFile1(string message)
        {
            
            var date = DateTime.UtcNow.ToString("dd-MM-yyyy");
            string fileName = "~/Logs/" + date + "_AdminLog" + ".txt";
            string filePath = (HttpContext.Current.Server.MapPath(fileName));
            using (var stream = File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                byte[] newline = Encoding.ASCII.GetBytes(Environment.NewLine);
                stream.Write(newline, 0, newline.Length);
                Byte[] info = new UTF8Encoding(true).GetBytes(message);
                stream.Write(info, 0, info.Length);
                stream.Close();
            }

        }

        public void WriteTextToFileForFlutterPeyLoadLogs(string message)
        {
            var date = DateTime.UtcNow.ToString("dd-MM-yyyy");
            string fileName = "~/Logs/" + date + "_FlutterPayLoadLog" + ".txt";
            string filePath = (HttpContext.Current.Server.MapPath(fileName));
            using (var stream = File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                byte[] newline = Encoding.ASCII.GetBytes(Environment.NewLine);
                stream.Write(newline, 0, newline.Length);
                Byte[] info = new UTF8Encoding(true).GetBytes(message);
                stream.Write(info, 0, info.Length);
                stream.Close();
            }
        }

        public void WriteTextToFileForBankFlutterPeyLoadLogs(string message)
        {
            var date = DateTime.UtcNow.ToString("dd-MM-yyyy");
            string fileName = "~/Logs/" + date + "_BankFlutterPayLoadLog" + ".txt";
            string filePath = (HttpContext.Current.Server.MapPath(fileName));
            using (var stream = File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                byte[] newline = Encoding.ASCII.GetBytes(Environment.NewLine);
                stream.Write(newline, 0, newline.Length);
                Byte[] info = new UTF8Encoding(true).GetBytes(message);
                stream.Write(info, 0, info.Length);
                stream.Close();
            }
        }

        public void WriteTextToFileApps(string message)
        {
            //string filePath = (HttpContext.Current.Server.MapPath("~/Logs/UserLogs.txt"));
            //using (var stream = File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            //{
            //    byte[] newline = Encoding.ASCII.GetBytes(Environment.NewLine);
            //    stream.Write(newline, 0, newline.Length);
            //    Byte[] info = new UTF8Encoding(true).GetBytes(message);
            //    stream.Write(info, 0, info.Length);
            //    stream.Close();
            //}

            var date = DateTime.UtcNow.ToString("dd-MM-yyyy");
            string fileName = "~/Logs/" + date + "_UserLogs" + ".txt";

            string filePath = (HttpContext.Current.Server.MapPath(fileName));
            using (var stream = File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                byte[] newline = Encoding.ASCII.GetBytes(Environment.NewLine);
                stream.Write(newline, 0, newline.Length);
                Byte[] info = new UTF8Encoding(true).GetBytes(message);
                stream.Write(info, 0, info.Length);
                stream.Close();
            }

        }

        public void InterNationalAirtime(string message)
        {
            var date = DateTime.UtcNow.ToString("dd-MM-yyyy");
            string fileName = "~/Logs/" + date + "_Airtime" + ".txt";
            string filePath = (HttpContext.Current.Server.MapPath(fileName));
            using (var stream = File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                byte[] newline = Encoding.ASCII.GetBytes(Environment.NewLine);
                stream.Write(newline, 0, newline.Length);
                Byte[] info = new UTF8Encoding(true).GetBytes(message);
                stream.Write(info, 0, info.Length);
                stream.Close();
            }

            //string filePath = (HttpContext.Current.Server.MapPath("~/Logs/Airtime.txt"));
            //using (var stream = File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            //{
            //    byte[] newline = Encoding.ASCII.GetBytes(Environment.NewLine);
            //    stream.Write(newline, 0, newline.Length);
            //    Byte[] info = new UTF8Encoding(true).GetBytes(message);
            //    stream.Write(info, 0, info.Length);
            //    stream.Close();
            //}
        }


        public void WriteTextToFileForWTxnTableLogs(string message)
        {
            var date = DateTime.UtcNow.ToString("dd-MM-yyyy");
            string fileName = "~/Logs/" + date + "_WTxnTableLog" + ".txt";
            string filePath = (HttpContext.Current.Server.MapPath(fileName));
            using (var stream = File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                byte[] newline = Encoding.ASCII.GetBytes(Environment.NewLine);
                stream.Write(newline, 0, newline.Length);
                Byte[] info = new UTF8Encoding(true).GetBytes(message);
                stream.Write(info, 0, info.Length);
                stream.Close();
            }
        }
        public void WriteTextToFileForBankFlutterZenithBankOTP(string message)
        {
            var date = DateTime.UtcNow.ToString("dd-MM-yyyy");
            string fileName = "~/Logs/" + date + "_ZenithBankOTPLog" + ".txt";
            string filePath = (HttpContext.Current.Server.MapPath(fileName));
            using (var stream = File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                byte[] newline = Encoding.ASCII.GetBytes(Environment.NewLine);
                stream.Write(newline, 0, newline.Length);
                Byte[] info = new UTF8Encoding(true).GetBytes(message);
                stream.Write(info, 0, info.Length);
                stream.Close();
            }
        }


        public void WriteTextToFileForBankwebhook(string message)
        {
            var date = DateTime.UtcNow.ToString("dd-MM-yyyy");
            string fileName = "~/Logs/" + date + "_BankwebhookLog" + ".txt";
            string filePath = (HttpContext.Current.Server.MapPath(fileName));
            using (var stream = File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                byte[] newline = Encoding.ASCII.GetBytes(Environment.NewLine);
                stream.Write(newline, 0, newline.Length);
                Byte[] info = new UTF8Encoding(true).GetBytes(message);
                stream.Write(info, 0, info.Length);
                stream.Close();
            }
        }



        public void WriteTextToFileForPeyGhanaMobMoneLogs(string message)
        {
            var date = DateTime.UtcNow.ToString("dd-MM-yyyy");
            string fileName = "~/Logs/" + date + "WriteTextToFileForPeyGhanaMobMoneLogs" + ".txt";
            string filePath = (HttpContext.Current.Server.MapPath(fileName));
            using (var stream = File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                byte[] newline = Encoding.ASCII.GetBytes(Environment.NewLine);
                stream.Write(newline, 0, newline.Length);
                Byte[] info = new UTF8Encoding(true).GetBytes(message);
                stream.Write(info, 0, info.Length);
                stream.Close();
            }
        }
        public void WriteTextToFileForCardNouseinaddmoneLogs(string message)
        {
            var date = DateTime.UtcNow.ToString("dd-MM-yyyy");
            string fileName = "~/Logs/" + date + "WriteTextToFileForCardNouseinaddmoneLogs" + ".txt";
            string filePath = (HttpContext.Current.Server.MapPath(fileName));
            using (var stream = File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                byte[] newline = Encoding.ASCII.GetBytes(Environment.NewLine);
                stream.Write(newline, 0, newline.Length);
                Byte[] info = new UTF8Encoding(true).GetBytes(message);
                stream.Write(info, 0, info.Length);
                stream.Close();
            }
        }
    }
}
