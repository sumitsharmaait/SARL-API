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
    [System.Web.Http.RoutePrefix("api/EzipayWebhookController")]
    public class EzipayWebhookController : ApiController
    {

        private IEzipayWebhookService _ez1;
        private ICardPaymentService _cardPaymentService;
        private ILogUtils _logUtils;
        /// <summary>
        /// EziWebHookController
        /// </summary>
        public EzipayWebhookController()
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


            //JavaScriptSerializer js = new JavaScriptSerializer();//for prod they give v2 webhokk output
            //dynamic blogObject = js.Deserialize<dynamic>(txt);

            try
            {
                dynamic data = JsonConvert.DeserializeObject<Dictionary<string, object>>(txt);
                foreach (var item in data["data"])
                {
                    if (item.Name == "status") 
                    {
                        txnreverifystatus = item.Value;
                    }
                    else if (item.Name == "tx_ref") 
                    {
                        tx_ref = item.Value;
                    }
                    else if (item.Name == "payment_type") 
                    {
                        payment_type = item.Value;
                    }
                    else if (item.Name == "currency") 
                    {
                        currency = item.Value;
                    }
                    else if (item.Name == "debit_currency")
                    {
                        debit_currency = item.Value;
                    }
                    else if (item.Name == "reference") 
                    {
                        reference = item.Value;
                    }

                }

                //var txnreverifystatus = blogObject["data"]["status"];//
                //var tx_ref = blogObject["data"]["tx_ref"];//

                //var currency = blogObject["data"]["currency"];//for xof onli 
                //var payment_type = blogObject["data"]["payment_type"];//for xof onli 
                //var debit_currency = blogObject["data"]["debit_currency"];//for xof onli SendBankFlutter
                //string tx_ref1 = tx_ref;

                if (currency == "XOF")
                {
                    "EziWebHookController".ErrorLog("EziWebHookController.cs", "webhookXOFFlutter", txt);
                }
                else if (currency == "USD")
                {
                    "EziWebHookController".ErrorLog("EziWebHookController.cs", "webhookUSDFlutter", txt);
                }
                else if (currency == "EUR")
                {
                    "EziWebHookController".ErrorLog("EziWebHookController.cs", "webhookEUROFlutter", txt);
                }
                else if (currency == "GHS")
                {
                    "EziWebHookController".ErrorLog("EziWebHookController.cs", "webhookGHSFlutter", txt);
                }
                if (currency == "NGN")
                {
                    //if (payment_type == "account")//AddBankFlutter
                    //{
                    //    "EziWebHookController".ErrorLog("EziWebHookController.cs", "webhookAddBankFlutter", txt);
                    //}
                    //else
                    if (payment_type == "bank_transfer")//AddBankFlutter
                    {
                        "EziWebHookController".ErrorLog("EziWebHookController.cs", "webhookAddBankFlutter", txt);
                    }
                    else if (debit_currency == "NGN")//SendBankFlutter
                    {
                        "EziWebHookController".ErrorLog("EziWebHookController.cs", "webhookSendBankFlutter", txt);
                    }
                }

                //
                if (txnreverifystatus == "successful" && tx_ref != null && currency == "XOF")
                {
                    "EziWebHookController".webhookflutterLog("EziWebHookController", "webhookXOFFlutter", tx_ref);
                }
                else if (txnreverifystatus == "successful" && tx_ref != null && currency == "USD")
                {
                    "EziWebHookController".webhookflutterLog("EziWebHookController", "webhookUSDFlutter", tx_ref);
                }
                else if (txnreverifystatus == "successful" && tx_ref != null && currency == "EUR")
                {
                    "EziWebHookController".webhookflutterLog("EziWebHookController", "webhookEUROFlutter", tx_ref);
                }
                //else if (txnreverifystatus == "successful" && tx_ref != null && currency == "NGN" && payment_type == "account")//AddBankFlutter
                //{
                //    "EziWebHookController".webhookflutterLog("EziWebHookController.cs", "webhookAddBankFlutter", tx_ref);
                //}
                else if (txnreverifystatus == "successful" && tx_ref != null && currency == "NGN" && payment_type == "bank_transfer")//AddBankFlutter
                {
                    "EziWebHookController".webhookflutterLog("EziWebHookController.cs", "webhookAddBankFlutter", tx_ref);
                    var xx = await _cardPaymentService.SaveflutteraddmoneNGNBankTransferPaymentResponse(txnreverifystatus, tx_ref, txt);

                }
                else if (reference != null && currency == "NGN" && debit_currency == "NGN")//SendBankFlutter
                {
                    "EziWebHookController".webhookflutterLog("EziWebHookController", "webhookSendBankFlutter", reference);
                    var xx = await _cardPaymentService.SaveflutterPayBankTransferPaymentResponse(txnreverifystatus, reference);
                    
                }
                else if (reference != null && currency == "GHS")//FlutterGhanaMobileMoney
                {
                    "EziWebHookController".webhookflutterLog("EziWebHookController", "webhookGHSFlutter", reference);
                    var xx = await _cardPaymentService.SaveflutterPayBankTransferPaymentResponse(txnreverifystatus, reference);
                }
                //response = await _cardPaymentService.SaveflutterCardPaymentResponsewebhook(_responseModel2);
                //RedirectToRoute("MasterCardPaymentcc", new { InvoiceNo = response.InvoiceNo, Amount = response.Amount, status = response.status, TransactionDate = response.TransactionDate, RstKey = response.RstKey });
            }
            catch (Exception ex)
            {
                "EziWebHookController".ErrorLog("EziWebHookController.cs", "webhookSendBankFlutter", txt + " " + ex.StackTrace + " " + ex.Message);
                _logUtils.WriteTextToFileForBankwebhook(txt + " " + ex.StackTrace + " " + ex.Message);
            }
            //response.RstKey = 0;
            return null;
        }
    }
}
