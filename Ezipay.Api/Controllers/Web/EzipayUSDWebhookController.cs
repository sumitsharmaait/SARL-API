using Ezipay.Service.CardPayment;
using Ezipay.Service.EzipayWebhookService;
using Ezipay.Utility.Extention;
using Ezipay.Utility.LogHandler;
using Ezipay.ViewModel.CardPaymentViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Ezipay.Api.Controllers.Web
{/// <summary>
 /// webhook
 /// </summary>
    //[Route("api/[controller]")]
    [System.Web.Http.RoutePrefix("api/EzipayUSDWebhookController")]
    public class EzipayUSDWebhookController : ApiController
    {

        private IEzipayWebhookService _ez1;
        private ICardPaymentService _cardPaymentService;
        private ILogUtils _logUtils;
        /// <summary>
        /// EziWebHookController
        /// </summary>
        public EzipayUSDWebhookController()
        {
            _cardPaymentService = new CardPaymentService();
            _ez1 = new EzipayWebhookService();
            _logUtils = new LogUtils();
        }


        /// <summary>
        /// Index
        /// </summary>
        [HttpPost]
        [Route("webhook")]
        public async Task<IHttpActionResult> Index(FlutterCardPaymentWebResponse webhook)
        {

            string txnreverifystatus = string.Empty;
            string tx_ref = string.Empty; string payment_type = string.Empty;
            string currency = string.Empty; string debit_currency = string.Empty;
            string reference = string.Empty;
            Stream s = HttpContext.Current.Request.InputStream;
            s.Position = 0;
            StreamReader ss = new StreamReader(s);
            string txt = ss.ReadToEnd();

           // string txt = "{\"event\":\"transfer.completed\",\"event.type\":\"Transfer\",\"data\":{\"id\":49438471,\"account_number\":\"2075820532\",\"bank_name\":\"UNITED BANK FOR AFRICA PLC\",\"bank_code\":\"033\",\"fullname\":\"AWERE CHUKWUEMEKA MATHEW\",\"created_at\":\"2023-04-27T08:37:07.000Z\",\"currency\":\"NGN\",\"debit_currency\":null,\"amount\":670,\"fee\":10.75,\"status\":\"SUCCESSFUL\",\"reference\":\"340a2b0eeb8e2973cc88d7736a45d623\",\"meta\":{\"first_name\":\"Chukwuemeka\",\"last_name\":\"Awere\",\"email\":\"emekawere@gmail.com\",\"beneficiary_country\":\"NG\",\"mobile_number\":\"233544681396\",\"sender\":\"Elorm\",\"merchant_name\":\"EziPay\"},\"narration\":null,\"approver\":null,\"complete_message\":\"Transaction was successful\",\"requires_approval\":0,\"is_approved\":1}}";
            try
            {
                dynamic data = JsonConvert.DeserializeObject<Dictionary<string, object>>(txt);
                foreach (var item in data["data"])
                {
                    if (item.Name == "status") //onli firtsbank
                    {
                        txnreverifystatus = item.Value;
                    }
                    else if (item.Name == "tx_ref") //onli firtsbank
                    {
                        tx_ref = item.Value;
                    }
                    else if (item.Name == "payment_type") //onli firtsbank
                    {
                        payment_type = item.Value;
                    }
                    else if (item.Name == "currency") //onli firtsbank
                    {
                        currency = item.Value;
                    }
                    else if (item.Name == "debit_currency") //onli firtsbank
                    {
                        debit_currency = item.Value;
                    }
                    else if (item.Name == "reference") //onli for nigeria peytransfer & ghana mobilemone
                    {
                        reference = item.Value;
                    }

                }


                if (currency == "NGN")
                {

                    if (payment_type == "bank_transfer")//AddBankFlutter
                    {
                        "EziWebHookController".ErrorLog("EziWebHookController.cs", "webhookAddBankglobalFlutter", txt);
                    }
                    else if (debit_currency == null)//SendBankFlutter debit_currency == "NGN"
                    {
                        "EziWebHookController".ErrorLog("EziWebHookController.cs", "webhookSendBankglobalFlutter", txt);
                    }
                }


                if (txnreverifystatus == "successful" && tx_ref != null && currency == "NGN" && payment_type == "bank_transfer")//AddBankFlutter
                {
                    "EziWebHookController".webhookflutterLog("EziWebHookController.cs", "webhookAddBankglobalFlutter", tx_ref);
                    var xx = await _cardPaymentService.SaveflutteraddmoneGlobalNigeriaBankTransferResponse(txnreverifystatus, tx_ref, currency, payment_type);

                }                                               
                else if (txnreverifystatus == "SUCCESSFUL" && reference != null && currency == "NGN" && debit_currency == null)//SendBankFlutter
                {
                    "EziWebHookController".webhookflutterLog("EziWebHookController", "webhookSendBankglobalFlutter", reference);
                    
                    var xx = await _cardPaymentService.SaveflutterPayGlobalNigeriaBankTransferPaymentResponse(txnreverifystatus, reference, currency, payment_type);

                }

            }
            catch (Exception ex)
            {
                "EziWebHookController".ErrorLog("EziWebHookController.cs", "webhookFlutterError", txt + " " + ex.StackTrace + " " + ex.Message);

            }

            return null;
        }
    }
}
