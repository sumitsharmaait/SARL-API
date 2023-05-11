using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.ViewModel.ApiHelpPage
{
    public class ApiHelpPageModel
    {
        public ApiHelpPageModel()
        {
            this.ApiName = string.Empty;
            this.Request = string.Empty;
            this.Response = string.Empty;
            this.ControllerName = string.Empty;
            this.HttpVerb = "POST";
        }
        public string ControllerName { get; set; }
        public string ApiName { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public string HttpVerb { get; set; }
    }
}
