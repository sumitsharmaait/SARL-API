using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Ezipay.Api.Filters
{
    /// <summary>
    /// CustomLogHandler
    /// </summary>
    public class CustomMessageHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string uri = request.RequestUri.ToString();
            #region Decrypt Request
            if (!uri.Contains("swagger") && uri.Contains("api"))
            {
                string JsonContent = string.Empty;
                try
                {
                    JsonContent = request.Content.ReadAsStringAsync().Result;
                }
                catch { }
            }
            #endregion
            var response = await base.SendAsync(request, cancellationToken);

            #region Encrypt Response
            if (!uri.Contains("swagger") && uri.Contains("api"))
            {
                string JsonContent2 = string.Empty;
                try
                {
                    JsonContent2 = response.Content.ReadAsStringAsync().Result;
                    var newresponse = new HttpResponseMessage(HttpStatusCode.OK)
                    {

                        Content = new StringContent("Inside the IDG message handler...")

                    };
                    response = newresponse;
                }
                catch { }
            }
            #endregion

            return response;
        }
    }
}