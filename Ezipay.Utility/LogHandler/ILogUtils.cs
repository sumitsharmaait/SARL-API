using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Utility.LogHandler
{
    public interface ILogUtils
    {
        void WriteTextToFile(string message);
        void WriteTextToFileApps(string message);
        void InterNationalAirtime(string message);
        void WriteTextToFile1(string message);
        void WriteTextToFileForFlutterPeyLoadLogs(string message);
        void WriteTextToFileForBankFlutterPeyLoadLogs(string message);
        void WriteTextToFileForWTxnTableLogs(string message);
        void WriteTextToFileForBankFlutterZenithBankOTP(string message);
        void WriteTextToFileForBankwebhook(string message);
        void WriteTextToFileForPeyGhanaMobMoneLogs(string message);


        void WriteTextToFileForCardNouseinaddmoneLogs(string message);
    }
}
