using Ezipay.Service.AirtimeService;
using Ezipay.Service.CardPayment;
using Ezipay.Service.InternatinalRechargeServ;
using Ezipay.Service.InterNetProviderService;
using Ezipay.Service.MasterData;
using Ezipay.Service.MerchantPayment;
using Ezipay.Service.MobileMoneyService;

using Ezipay.Service.TvService;
using Ezipay.Utility.Extention;
using Ezipay.Utility.LogHandler;
using Ezipay.ViewModel.AirtimeViewModel;
using Ezipay.ViewModel.CardPaymentViewModel;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Ezipay.Api.MVCController
{
    /// <summary>
    /// MasterCardPaymentController
    /// </summary>
    public class MasterCardPaymentController : Controller
    {
        // GET: MasterCardPayment
        private ICardPaymentService _cardPaymentService;

        private IMasterDataService _masterDataService;
        private IMobileMoneyServices _mobileMoneyServices;
        private IAirtimeService _airtimeService;
        private IInternatinalRechargeService _internatinalRechargeService;
        private IInterNetProviderService _interNetProviderService;
        private ITvServices _tvServices;
        private IMerchantPaymentService _merchantPaymentService;
        private ILogUtils _logUtils;
        /// <summary>
        /// MasterCardPaymentController
        /// </summary>
        public MasterCardPaymentController()
        {
            _cardPaymentService = new CardPaymentService();

            _masterDataService = new MasterDataService();
            _mobileMoneyServices = new MobileMoneyServices();
            _airtimeService = new AirtimeService();
            _internatinalRechargeService = new InternatinalRechargeService();
            _interNetProviderService = new InterNetServiceProviderService();
            _tvServices = new TvServices();
            _merchantPaymentService = new MerchantPaymentService();
            _logUtils = new LogUtils();

        }

        /// <summary>
        /// SaveImaginaryPaymentResponse
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> SaveMasterCardPaymentResponse(MasterCardPaymentResponse request)
        {
            "MasterCardPaymentControllerSuccess".ErrorLog("MasterCardPaymentController.cs", "SaveMasterCardResponse", request);
            var response = new AddMoneyAggregatorResponse();

            try
            {

                response = await _cardPaymentService.SaveMasterCardPaymentResponse(request);

                return RedirectToAction("CardPaymentConfirmationFromUBA", "MasterCardPayment", new { InvoiceNo = response.InvoiceNo, Amount = response.Amount, status = response.status, TransactionDate = response.TransactionDate, RstKey = response.RstKey });
            }
            catch (Exception ex)
            {

            }
            response.RstKey = 0;
            return View(response);
        }

        //[HttpGet]
        //public async Task<ActionResult> SaveSeerbitPaymentResponse(SeerbitRequest request)
        //{
        //    "MasterCardPaymentControllerSuccess".ErrorLog("MasterCardPaymentController.cs", "SaveSeerbitPaymentResponse", request);
        //    var response = new SeerbitResponse();

        //    try
        //    {

        //        response = await _cardPaymentService.SaveSeerbitPaymentResponse(request);

        //        return RedirectToAction("CardPaymentConfirmationFromUBA", "MasterCardPayment", new { InvoiceNo = response.InvoiceNo, Amount = response.Amount, status = response.status, TransactionDate = response.TransactionDate, RstKey = response.RstKey });
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    response.RstKey = 0;
        //    return View(response);
        //}

        //[HttpGet]
        //public async Task<ActionResult> GTBCIVPaymentResponse(GTBCIVUrlPaymentResponse id)
        //{
        //    "MasterCardPaymentControllerSuccess".ErrorLog("MasterCardPaymentController.cs", "GTBCIVPaymentResponse", id);
        //    var response = new SeerbitResponse();

        //    try
        //    {

        //        //response = await _cardPaymentService.SaveSeerbitPaymentResponse(request);

        //        //return RedirectToAction("CardPaymentConfirmationFromUBA", "MasterCardPayment", new { InvoiceNo = response.InvoiceNo, Amount = response.Amount, status = response.status, TransactionDate = response.TransactionDate, RstKey = response.RstKey });
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    response.RstKey = 0;
        //    var obj = new AddMoneyAggregatorResponse();
        //    obj.Amount = id.AcqID;
        //    obj.InvoiceNo = id.OrderID;
        //    obj.status = id.Signature;
        //    return View("CardPaymentConfirmationFromUBA",obj);
        //    //return null;
        //}

        [HttpGet]
        public async Task<ActionResult> SavengeniusPaymentResponse()
        {
            string reference = Request.QueryString["ref"];
            "MasterCardPaymentControllerSuccess".ErrorLog("MasterCardPaymentController.cs", "SavengeniusPaymentResponse", reference);
            var response = new AddMoneyAggregatorResponse();

            try
            {
                if (reference != null)
                {
                    response = await _cardPaymentService.SavengeniusPaymentResponse(reference);

                    return RedirectToAction("CardPaymentConfirmationFromUBA", "MasterCardPayment", new { InvoiceNo = response.InvoiceNo, Amount = response.Amount, status = response.status, TransactionDate = response.TransactionDate, RstKey = response.RstKey });
                }
            }
            catch (Exception ex)
            {

            }
            response.RstKey = 0;
            return View(response);
        }

        [HttpGet]
        public async Task<ActionResult> SaveMasterCardPayment2Response(MasterCardPaymentResponse request)
        {
            "MasterCardPaymentControllerSuccess".ErrorLog("MasterCardPaymentController.cs", "SaveMasterCardPayment2Response", request);
            var response = new AddMoneyAggregatorResponse();

            try
            {

                response = await _cardPaymentService.SaveMasterCardPayment2Response(request);

                return RedirectToAction("CardPaymentConfirmationFromUBA", "MasterCardPayment", new { InvoiceNo = response.InvoiceNo, Amount = response.Amount, status = response.status, TransactionDate = response.TransactionDate, RstKey = response.RstKey });
            }
            catch (Exception ex)
            {

            }
            response.RstKey = 0;
            return View(response);
        }

        [HttpGet]
        public async Task<ActionResult> SaveflutterCardPaymentResponse(fluttercallbackResponse request)
        {
            "MasterCardPaymentControllerSuccess".ErrorLog("MasterCardPaymentController.cs", "SaveflutterCardPaymentResponse", request);
            var response = new AddMoneyAggregatorResponse();

            try
            {
                response = await _cardPaymentService.SaveflutterCardPaymentResponse(request);
                return RedirectToAction("CardPaymentConfirmationFromUBA", "MasterCardPayment", new { InvoiceNo = response.InvoiceNo, Amount = response.Amount, status = response.status, TransactionDate = response.TransactionDate, RstKey = response.RstKey });
            }
            catch (Exception ex)
            {

            }
            response.RstKey = 0;
            return View(response);
            
        }


        [HttpGet]
        public async Task<ActionResult> SaveMerchantPaymentResponse()
        {
            string txt_ref = Request.QueryString["txt_ref"];
            "MasterCardPaymentControllerSuccess".ErrorLog("MasterCardPaymentController.cs", "SaveMerchantPaymentResponse", txt_ref);
            var response = new AddMoneyAggregatorResponse();

            try
            {

                response = await _cardPaymentService.SaveMerchantPaymentResponse(txt_ref);

                return RedirectToAction("CardPaymentConfirmationFromUBA", "MasterCardPayment", new { InvoiceNo = response.InvoiceNo, Amount = response.Amount, status = response.status, TransactionDate = response.TransactionDate, RstKey = response.RstKey });
            }
            catch (Exception ex)
            {

            }
            response.RstKey = 0;
            return View(response);
        }


        //add_mone nigeria bank debit
        //[HttpGet]
        //public async Task<ActionResult> SaveflutterBankPaymentResponse()
        //{
        //    string reference = Request.QueryString["response"];
        //    "MasterCardPaymentControllerSuccess".ErrorLog("MasterCardPaymentController.cs", "SaveflutterBankPaymentResponse", reference);
        //    //string d = "{\"Title\":\"\",\"vpc_AVSResultCode\":\"Unsupported\",\"vpc_AcqAVSRespCode\":\"Unsupported\",\"vpc_AcqCSCRespCode\":\"M\",\"vpc_AcqResponseCode\":\"00\",\"vpc_Amount\":\"50\",\"vpc_AuthorizeId\":\"527236\",\"vpc_BatchNo\":\"20200207\",\"vpc_CSCResultCode\":\"M\",\"vpc_Card\":\"MC\",\"vpc_Command\":\"pay\",\"vpc_Currency\":\"GHS\",\"vpc_Locale\":\"en_GH\",\"vpc_MerchTxnRef\":\"EZZT-2020-TR10354\",\"vpc_Merchant\":\"GTB111030B01\",\"vpc_Message\":\"Approved\",\"vpc_OrderInfo\":\"EZZT-2020-OR10354\",\"vpc_ReceiptNo\":\"003817461079\",\"vpc_SecureHash\":\"B9AE7A1E761B1500AE085E97E5979BF6A63DD86D77CD7AFC4E9AE59F3B579FB2\",\"vpc_TransactionNo\":\"2070006001\",\"vpc_TxnResponseCode\":\"0\",\"vpc_Version\":\"1\",\"vpc_VerType\":\"3DS\",\"vpc_VerStatus\":\"Y\",\"vpc_VerToken\":\"jJtH/66wqsh0CBEEGqNbBYEAAAA=\",\"vpc_VerSecurityLevel\":\"05\",\"vpc_3DSenrolled\":\"Y\",\"vpc_3DSXID\":\"qs7ABZXDW7s1MxoiN0E+LO5GKkE=\",\"vpc_3DSECI\":\"02\",\"vpc_3DSstatus\":\"Y\",\"vpc_hashValidated\":\"\",\"vpc_ResponseCodeDescription\":\"\",\"vpc_StatusCodeDescription\":\"\"}";
        //    var request = JsonConvert.DeserializeObject<BankPaymentWebResponse>(reference);

        //    var response = new AddMoneyAggregatorResponse();
        //    try
        //    {
        //        response = await _cardPaymentService.SaveflutterBankPaymentResponse(request);
        //        return RedirectToAction("CardPaymentConfirmationFromUBA", "MasterCardPayment", new { InvoiceNo = response.InvoiceNo, Amount = response.Amount, status = response.status, TransactionDate = response.TransactionDate, RstKey = response.RstKey });
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    response.RstKey = 0;
        //    return View(response);


        //}

        //[HttpGet]
        //public async Task<ActionResult> SaveflutterPayBankTransferPaymentResponse()
        //{
        //    Stream s = System.Web.HttpContext.Current.Request.InputStream;
        //    s.Position = 0;
        //    StreamReader ss = new StreamReader(s);
        //    string txt = ss.ReadToEnd();

        //    JavaScriptSerializer js = new JavaScriptSerializer();
        //    dynamic blogObject = js.Deserialize<dynamic>(txt);

        //    var txnreverifystatus = blogObject["data"]["status"];//
        //    var tx_ref = blogObject["data"]["tx_ref"];//

        //    var currency = blogObject["data"]["currency"];//for xof onli 
        //    string tx_ref1 = tx_ref;

        //    "MasterCardPaymentControllerSuccess".ErrorLog("MasterCardPaymentController.cs", "SaveflutterPayBankTransferPaymentResponse", txt);

        //    var response = new UpdateTransactionResponse();

        //    try
        //    {
        //        response = await _cardPaymentService.SaveflutterPayBankTransferPaymentResponse(txnreverifystatus, tx_ref);
        //        //return RedirectToAction("CardPaymentConfirmationFromUBA", "MasterCardPayment", new { InvoiceNo = response.InvoiceNo, Amount = response.Amount, status = response.status, TransactionDate = response.TransactionDate, RstKey = response.RstKey });
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    //response.RstKey = 0;
        //    return null;
        //}

        public ActionResult CardPaymentConfirmationFromUBA(string InvoiceNo, string Amount, string status, DateTime TransactionDate, int RstKey)
        {
            var obj = new AddMoneyAggregatorResponse();
            obj.Amount = Amount;
            obj.InvoiceNo = InvoiceNo;
            obj.status = status;
            obj.RstKey = RstKey;

            obj.TransactionDate = TransactionDate;
            return View(obj);
        }


        [HttpGet]
        [Route("ZenithBankOTPResponse")]
        public ActionResult ZenithBankOTPResponse(string InvoiceNo, string Amount, string status, DateTime TransactionDate, int RstKey)
        {
            var obj = new AddMoneyAggregatorResponse();
            obj.Amount = Amount;
            obj.InvoiceNo = InvoiceNo;
            obj.status = status;
            obj.RstKey = RstKey;

            obj.TransactionDate = TransactionDate;
            return View(obj);
        }




      
    }
}
