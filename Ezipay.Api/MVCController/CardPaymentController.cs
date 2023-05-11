using Ezipay.Service.CardPayment;
using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.ViewModel.CardPaymentViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Ezipay.Api.MVCController
{
    /// <summary>
    /// CardPaymentController
    /// </summary>
    public class CardPaymentController : Controller
    {
        private ICardPaymentService _cardPaymentService;
        /// <summary>
        /// CardPaymentController
        /// </summary>
        public CardPaymentController()
        {
            _cardPaymentService = new CardPaymentService();
        }
        /// <summary>
        /// card Payment Response
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> PaymentResponse(CardPaymentWebResponse request)
        {
            //string d = "{\"Title\":\"\",\"vpc_AVSResultCode\":\"Unsupported\",\"vpc_AcqAVSRespCode\":\"Unsupported\",\"vpc_AcqCSCRespCode\":\"M\",\"vpc_AcqResponseCode\":\"00\",\"vpc_Amount\":\"50\",\"vpc_AuthorizeId\":\"527236\",\"vpc_BatchNo\":\"20200207\",\"vpc_CSCResultCode\":\"M\",\"vpc_Card\":\"MC\",\"vpc_Command\":\"pay\",\"vpc_Currency\":\"GHS\",\"vpc_Locale\":\"en_GH\",\"vpc_MerchTxnRef\":\"EZZT-2020-TR10354\",\"vpc_Merchant\":\"GTB111030B01\",\"vpc_Message\":\"Approved\",\"vpc_OrderInfo\":\"EZZT-2020-OR10354\",\"vpc_ReceiptNo\":\"003817461079\",\"vpc_SecureHash\":\"B9AE7A1E761B1500AE085E97E5979BF6A63DD86D77CD7AFC4E9AE59F3B579FB2\",\"vpc_TransactionNo\":\"2070006001\",\"vpc_TxnResponseCode\":\"0\",\"vpc_Version\":\"1\",\"vpc_VerType\":\"3DS\",\"vpc_VerStatus\":\"Y\",\"vpc_VerToken\":\"jJtH/66wqsh0CBEEGqNbBYEAAAA=\",\"vpc_VerSecurityLevel\":\"05\",\"vpc_3DSenrolled\":\"Y\",\"vpc_3DSXID\":\"qs7ABZXDW7s1MxoiN0E+LO5GKkE=\",\"vpc_3DSECI\":\"02\",\"vpc_3DSstatus\":\"Y\",\"vpc_hashValidated\":\"\",\"vpc_ResponseCodeDescription\":\"\",\"vpc_StatusCodeDescription\":\"\"}";
            //request = JsonConvert.DeserializeObject<CardPaymentWebResponse>(d);

            "Card Payment Success".ErrorLog("CardPaymentController.cs", "PaymentResponse", request);
            var response = await _cardPaymentService.SavePaymentResponse(request);
            if (response != null && response.TransactionRefId > 0 && !string.IsNullOrEmpty(response.PaymentTransactionNo) && response.TransactionResponseDescription.ToUpper() == "APPROVED")
            {
                if (response.AddDuringPayResponse == null)
                {

                }
                else
                {
                    "AddDuringPayResponse obj".ErrorLog("CardPaymentController", "PaymentResponse", response.AddDuringPayResponse);
                }
                PaymentConfirmationModelOther paymentConfirmationModel = new PaymentConfirmationModelOther();
                paymentConfirmationModel.TransactionResponseDescription = response.TransactionResponseDescription;
                paymentConfirmationModel.TransactionResponseCode = response.TransactionResponseCode;
                paymentConfirmationModel.Status = 1;
                paymentConfirmationModel.PaymentTransactionNo = response.PaymentTransactionNo;
                paymentConfirmationModel.ToMobileNo = response.ToMobileNo;
                paymentConfirmationModel.TransactionAmount = response.TransactionAmount;
                paymentConfirmationModel.TransactionDate = response.TransactionDate;
                paymentConfirmationModel.IsAddDuringPay = response.IsAddDuringPay;
                paymentConfirmationModel.IsMerchant = response.AddDuringPayResponse != null ? response.AddDuringPayResponse.IsMerchant : false;
                paymentConfirmationModel.MerchantStatusCode = response.AddDuringPayResponse.MerchantStatusCode;
                paymentConfirmationModel.AccountNo = response.AddDuringPayResponse.AccountNo;
                paymentConfirmationModel.Amount = response.AddDuringPayResponse.Amount;
                paymentConfirmationModel.CurrentBalance = response.AddDuringPayResponse.CurrentBalance;
                paymentConfirmationModel.InvoiceNo = response.AddDuringPayResponse.InvoiceNo;
                paymentConfirmationModel.Message = response.AddDuringPayResponse.Message;
                paymentConfirmationModel.MobileNo = response.AddDuringPayResponse.MobileNo;
                paymentConfirmationModel.StatusCode = response.AddDuringPayResponse.StatusCode;
                paymentConfirmationModel.TransactionDate = response.AddDuringPayResponse.TransactionDate;
                paymentConfirmationModel.TransactionId = response.AddDuringPayResponse.AccountNo;
                paymentConfirmationModel.TransactionId = response.AddDuringPayResponse.TransactionId;
                paymentConfirmationModel.CurrentBalance = response.CurrentBalance;
                return RedirectToAction("PaymentConfirmation", paymentConfirmationModel);
            }
            else
            {
                PaymentConfirmationModelOther paymentConfirmationModel = new PaymentConfirmationModelOther();
                paymentConfirmationModel.TransactionResponseDescription = response.TransactionResponseDescription;
                paymentConfirmationModel.TransactionResponseCode = response.TransactionResponseCode;
                paymentConfirmationModel.Status = 0;
                paymentConfirmationModel.PaymentTransactionNo = string.Empty;

                return RedirectToAction("PaymentConfirmation", paymentConfirmationModel);
            }
            //PaymentConfirmationModelOther paymentConfirmationModel = new PaymentConfirmationModelOther();
            //return RedirectToAction("PaymentConfirmation", paymentConfirmationModel);
        }

        [HttpGet]
        public ActionResult PaymentConfirmation(PaymentConfirmationModelOther model)
        {
            var adminKeyPair = AES256.AdminKeyPair;
            // var s = "{\"Status\":1,\"PaymentTransactionNo\":\"2070006001\",\"ToMobileNo\":\"\",\"TransactionAmount\":\"0.5\",\"TransactionDate\":\"0001-01-01T00:00:00\",\"CurrentBalance\":\"91524.35\",\"TransactionResponseDescription\":\"Transaction Successful\",\"TransactionResponseCode\":\"0\",\"IsAddDuringPay\":false,\"IsMerchant\":false,\"MerchantStatusCode\":0,\"FormatedTransactionDate\":null,\"MobileNo\":\"\",\"Amount\":\"\",\"StatusCode\":\"\",\"Message\":\"\",\"TransactionId\":\"\",\"InvoiceNo\":\"\",\"AccountNo\":null}"; //JsonConvert.SerializeObject(model);
            var s =JsonConvert.SerializeObject(model);
            var CardPaymentPage = ConfigurationManager.AppSettings["CardPaymentPage"];
            // string encData = AES256.Encrypt(adminKeyPair.PublicKey, s);
            return Redirect(CardPaymentPage + s);
            // return View(model);
        }
    }
}