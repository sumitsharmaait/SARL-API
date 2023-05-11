using Ezipay.ViewModel.common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ezipay.Api.Controllers
{
    public class ErrorController : ApiController
    {
        [HttpGet, HttpPost, HttpPut, HttpDelete, HttpHead, HttpOptions, AcceptVerbs("PATCH")]
        public Response<Object> Handle404()
        {
            var response = new Response<Object>();
            var result = new Object();
            response = response.Create(false, "Please Update your application", HttpStatusCode.Unauthorized, result);
            return response;
        }

    }
}
