using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.ViewModel.SendOtpViewModel
{
    public class SendMessageRequest
    {
        public string MobileNo { get; set; }
        public string ISD { get; set; }
        public string Message { get; set; }
    }
    public class HubtelMessageRequest
    {
        public HubtelMessageRequest()
        {
            this.From = ConfigurationManager.AppSettings["HubtelNumber"];
            this.To = string.Empty;
            this.Content = string.Empty;
            //   this.Time = DateTime.UtcNow;
            this.ClientId = ConfigurationManager.AppSettings["HubtelClientId"];
            this.ClientSecret = ConfigurationManager.AppSettings["HubtelClientSecret"];
        }
        public string From { get; set; }
        public string To { get; set; }
        public string Content { get; set; }
        public bool RegisteredDelivery { get; set; }
        //public DateTime Time { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
    public class HubtelMessageResponse
    {
        public int Status { get; set; }
        public string MessageId { get; set; }
        public decimal Rate { get; set; }
        public string NetworkId { get; set; }
    }


    public class RouteMobileMessageRequest
    {
        public RouteMobileMessageRequest()
        {
            this.source = ConfigurationManager.AppSettings["RouteSource"];
            this.destination = string.Empty;
            this.message = string.Empty;
            this.username = ConfigurationManager.AppSettings["RouteUsername"];
            this.password = ConfigurationManager.AppSettings["RoutePassword"];
            this.type = "0";
            this.dlr = "1";
        }
        public string source { get; set; }
        public string destination { get; set; }
        public string message { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string type { get; set; }
        public string dlr { get; set; }

    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Verify
    {
        public string code_state { get; set; }
        public string code_entered { get; set; }
    }

    public class Status
    {
        public DateTime updated_on { get; set; }
        public int code { get; set; }
        public string description { get; set; }
    }

    public class TeleSignResponse
    {
        public TeleSignResponse()
        {
            verify = new Verify();
            status = new Status();
        }
        public string reference_id { get; set; }
        public string sub_resource { get; set; }
        public List<object> errors { get; set; }
        public Verify verify { get; set; }
        public Status status { get; set; }
    }
    public class SendOtpTeleSignRequest
    {
        public string phone_number { get; set; }
        public string language { get; set; }
        public string verify_code { get; set; }
        public string template { get; set; }
    }

    public class Voice
    {
        public string caller_id { get; set; }
    }

    public class TeleSignCallBackResponse
    {
        public TeleSignCallBackResponse()
        {
            verify = new Verify();
            status = new Status();
            voice = new Voice();
        }
        public string reference_id { get; set; }
        public string sub_resource { get; set; }
        public List<object> errors { get; set; }
        public Verify verify { get; set; }
        public Status status { get; set; }
        public Voice voice { get; set; }
    }


}
