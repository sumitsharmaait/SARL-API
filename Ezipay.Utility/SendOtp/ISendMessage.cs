using Ezipay.ViewModel.SendOtpViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Utility.SendOtp
{
    public interface ISendMessage
    { 
       
        bool SendMessgeWithISDCode(SendMessageRequest PostData);
        Task<bool> SendOtpTeleSign(SendOtpTeleSignRequest signRequest);
        Task<bool> CallBackTeleSign(SendOtpTeleSignRequest signRequest);
    }
}
