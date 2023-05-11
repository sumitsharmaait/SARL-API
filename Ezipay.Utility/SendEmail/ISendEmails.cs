using Ezipay.ViewModel.SendEmailViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Utility.SendEmail
{
    public interface ISendEmails
    {
        //  Task<OtpResponse> SendVerificationEmail(SendVerificationEmailRequest request);

        string ReadEmailformats(string Filename);

        string SendEmail(EmailModel emailModel);

        string ReadTemplateEmailformats(string Filename);
        string SendEmailTxn(EmailModel emailModel, MemoryStream memoryStream);
        string SendEmailUsingDifferentCard(EmailModel emailModel);
    }
}
