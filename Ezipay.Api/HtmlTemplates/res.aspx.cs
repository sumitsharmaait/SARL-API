using Ezipay.Utility.Extention;
using Ezipay.ViewModel.CardPaymentViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Ezipay.Api.HtmlTemplates
{
    public partial class res : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.Form.HasKeys()) // via GIM Uemoa
            {
                // process the request parameters
                // gim paramaters value
                string ResponseCode = (string)Request["ResponseCode"] ?? string.Empty;
                //transref = (string)Request.Form["txnref"] ?? "";
                string transref = (string)Request.Form["OrderID"] ?? "";
                //string defaultresp = (string)Request.Form["desc"] ?? "";
                string ReasonCode = (string)Request.Form["ReasonCode"] ?? "";
                string ReasonCodeDesc = (string)Request.Form["ReasonCodeDesc"] ?? "";
                string gtpayResp = (string)Request.Form["GTPayTxnRef"] ?? "";//GTPay Ibank
                string centralpayresponse = (string)Request.Form["ReasonCode"] ?? "";
                //string centralpayresponse = (string)Request.Form["cpay_ref"] ?? "";
                //string centralpaytranxid = (string)Request.Form["transaction_id"] ?? "" ;
                string centralpaytranxid = (string)Request.Form["ReferenceNo"] ?? "";
                string merchantID = (string)Request.Form["MerID"] ?? "";
                string cardNo = (string)Request.Form["CardNumber"] ?? "";
                string GimSignature = (string)Request.Form["Signature"] ?? "";
                string signatureMethod = (string)Request.Form["SignatureMethod"] ?? "";
                string tokenValue = (string)Request.Form["TokenValue"] ?? "";
                string AcqID = (string)Request.Form["AcqID"] ?? "";
                string AuthCode = (string)Request.Form["AuthorizationCode"] ?? "";

                GTBCIVUrlPaymentResponse obj = new GTBCIVUrlPaymentResponse();
                TextBox1.Text= transref;
                obj.OrderID = transref;
                obj.ResponseCode = ResponseCode;
                obj.ReasonCodeDesc = ReasonCodeDesc;

                obj.ReasonCode = ReasonCode;
                obj.MerID = merchantID;
                obj.Signature = signatureMethod;
                "MasterCardPaymentControllerSuccess".ErrorLog("MasterCardPaymentController.cs", "GTBCIVPaymentResponse", obj);
                Response.RedirectToRoute(new { controller = "MasterCardPayment", action = "GTBCIVPaymentResponse",id = obj });

            }

        }
    }
}