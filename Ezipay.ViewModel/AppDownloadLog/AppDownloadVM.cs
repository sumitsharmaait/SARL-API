using Ezipay.ViewModel.AdminViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.ViewModel
{
    public class AppDownloadLogRequest
    {
        public string DeviceUniqueId { get; set; }
        public string DeviceToken { get; set; }
        public int DeviceType { get; set; }
    }

    public class AppDownloadSearchVM : SearchRequest
    {
    }

    public class AppDownloadSearchResponse
    {
        public AppDownloadSearchResponse()
        {
            DataList = new List<AppDownloadLogVM>();
        }
        public List<AppDownloadLogVM> DataList { get; set; }
        public int TotalCount { get; set; }
    }

    public class AppDownloadLogVM
    {
        public long Id { get; set; }
        public string DeviceUniqueId { get; set; }
        public string DeviceToken { get; set; }
        public int DeviceType { get; set; }
    }

    public class SendNotificationRequest
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string MessageBody { get; set; }
        public int TotalCount { get; set; }
        public string FileUpload { get; set; }
       
    }

    public class SendNotificationResponse
    {
        public string Title { get; set; }
        public string MessageBody { get; set; }
        public int Id { get; set; }
        public string FileUpload { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string chkhrmin { get; set; }
        public int TotalCount { get; set; }
        public long WalletUserId { get; set; }
        public string statusflag { get; set; }
    }

    public class CountNotificationRequest
    {
        public int Id { get; set; }
        public string statusflag { get; set; }
        public long WalletUserId { get; set; }
        
    }
    public class notificationupdateRequest
    {       

        public int[] Id { get; set; }
        public string statusflag { get; set; }
        public long walletuserid { get; set; }

    }
}
