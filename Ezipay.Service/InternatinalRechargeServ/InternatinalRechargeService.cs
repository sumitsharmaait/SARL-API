using ezeePay.Utility.CommonClass;
using ezeePay.Utility.Enums;
using Ezipay.Database;
using Ezipay.Repository.AirtimeRepo;
using Ezipay.Repository.CommisionRepo;
using Ezipay.Repository.CommonRepo;
using Ezipay.Repository.MasterData;
using Ezipay.Repository.PayMoneyRepo;
using Ezipay.Repository.UserRepo;
using Ezipay.Service.CommonService;
using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.Utility.SendEmail;
using Ezipay.Utility.SendPush;
using Ezipay.ViewModel.AirtimeViewModel;
using Ezipay.ViewModel.CommisionViewModel;
using Ezipay.ViewModel.InternatinalRechargeViewModel;
using Ezipay.ViewModel.SendEmailViewModel;
using Ezipay.ViewModel.SendPushViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Twilio.TwiML;


namespace Ezipay.Service.InternatinalRechargeServ
{
    public class InternatinalRechargeService : IInternatinalRechargeService
    {
        private IMasterDataRepository _masterDataRepository;
        private IAirtimeRepository _airtimeRepository;
        private IWalletUserRepository _walletUserRepository;
        private ISetCommisionRepository _setCommisionRepository;
        private ISendPushNotification _sendPushNotification;
        private IPayMoneyRepository _payMoneyRepository;
        private ISendEmails _sendEmails;
        private ICommonRepository _commonRepository;
        private ICommonServices _commonServices;
        // private ILogUtils _logUtils;
        public InternatinalRechargeService()
        {
            _airtimeRepository = new AirtimeRepository();
            _walletUserRepository = new WalletUserRepository();
            _setCommisionRepository = new SetCommisionRepository();
            _masterDataRepository = new MasterDataRepository();
            _sendEmails = new SendEmails();
            _payMoneyRepository = new PayMoneyRepository();
            _sendPushNotification = new SendPushNotification();
            _commonRepository = new CommonRepository();
            _commonServices = new CommonServices();

            //   _logUtils = new LogUtils();
        }
        public async Task<InternationalAirtimeResponse> GetProductList(InternationalAirtimeRequest request)
        {
            var res = new InternationalAirtimeResponse();
            var currenyRates = new List<InternationalAirtimeAmountResponse>();
            // XmlSerializer xsSubmit = new XmlSerializer(typeof(xml));
            //StringContent response = new StringContent();
            string xml = string.Empty;
            string url = CommonSetting.InternationalAirtimeUrl;
            var invoiceNumber = await _masterDataRepository.GetInvoiceNumber();
            var currencyDetail = _masterDataRepository.GetCurrencyRate();
            try
            {
                // _logUtils.InterNationalAirtime("Request " + request.MobileNo);
                decimal dollarRate = Convert.ToDecimal(currencyDetail.DollarRate);

                var token = CommonSetting.MerchantToken;
                string md5Generate = CommonSetting.loginkey + token + invoiceNumber.AutoDigit;
                var result = MD5(md5Generate);
                string customer = request.MobileNo.Substring(0, 1);
                if (customer == "0")
                {
                    customer = request.MobileNo.Remove(0, 1);
                }
                else
                {
                    customer = request.MobileNo;
                }
                //var req = new xml
                //{
                //    login = CommonSetting.loginkey,
                //    key = invoiceNumber.AutoDigit,
                //    md5 = result,
                //    destination_msisdn = request.MobileNumber,
                //    delivered_amount_info = request.Amount,
                //    return_service_fee = request.Amount,
                //    action = "msisdn_info"
                //};

                XDocument xmlDocument = new XDocument(new XDeclaration("1.0", "UTF-8", null));
                XElement xBillFetchReqElement = new XElement("xml");
                xBillFetchReqElement.Add(new XElement("login", CommonSetting.loginkey));
                xBillFetchReqElement.Add(new XElement("key", invoiceNumber.AutoDigit));
                xBillFetchReqElement.Add(new XElement("md5", result));
                xBillFetchReqElement.Add(new XElement("destination_msisdn", request.IsdCode + request.MobileNo));
                xBillFetchReqElement.Add(new XElement("delivered_amount_info", "1"));
                xBillFetchReqElement.Add(new XElement("return_service_fee", "1"));
                xBillFetchReqElement.Add(new XElement("action", "msisdn_info"));
                xmlDocument.Add(xBillFetchReqElement);
                StringWriter stringWriter = new Utf8StringWriter();
                xmlDocument.Save(stringWriter, SaveOptions.None);


                var response = new StringContent(stringWriter.ToString(), Encoding.UTF8, "application/xml");
                var rr = postXMLData(url, stringWriter.ToString());
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(rr);
                string jsonText = JsonConvert.SerializeXmlNode(doc);
                string display = string.Empty;
                //  _logUtils.InterNationalAirtime(jsonText);
                var productResult = JsonConvert.DeserializeObject<GetProductListResponse>(jsonText);
                if (productResult.TransferTo.error_code == "0")
                {
                    var products = productResult.TransferTo.product_list.Split(',');
                    var retail_price_list = productResult.TransferTo.retail_price_list.Split(',');
                    var wholesale_price_list = productResult.TransferTo.wholesale_price_list.Split(',');
                    // To convert JSON text contained in string json into an XML node
                    res.Products = products.ToList();
                    res.retail_price_list = retail_price_list.ToList();
                    res.wholesale_price_list = wholesale_price_list.ToList();

                    for (var i = 0; i < res.Products.Count; i++)
                    {
                        var amt = Convert.ToDecimal(res.retail_price_list[i]) / dollarRate;
                        var finalAmt = amt.ToString("0.000");
                        display = res.Products[i] + " " + productResult.TransferTo.destination_currency + ", $ " + res.retail_price_list[i] + "(XOF " + finalAmt + ")";
                        res.DisplayContent.Add(display);

                        currenyRates.Add(new InternationalAirtimeAmountResponse
                        {
                            DisplayContent = display,
                            AmountInUsd = res.retail_price_list[i],
                            AmountInLocalCountry = res.Products[i],
                            msisdn = productResult.TransferTo.destination_msisdn,
                            AmountInLe = finalAmt
                        });
                    }
                    res.internationalAirtimeAmountResponses.AddRange(currenyRates);
                    res.TransferTo = productResult.TransferTo;
                    res.RstKey = 1;
                }
                else if (productResult.TransferTo.error_code == InternationalAggregatorySTATUSCODES.destination_number_is_not_avalid)
                {
                    res.RstKey = 6;
                    res.Message = AggregatoryMESSAGE.destination_number_is_not_avalid;
                }
                //amit add
                else if (productResult.TransferTo.error_code == InternationalAggregatorySTATUSCODES.servicetothisdestinationoperatoristemporarilyunavailable)
                {
                    res.RstKey = 6;
                    res.Message = AggregatoryMESSAGE.servicetothisdestinationoperatoristemporarilyunavailable;
                }
                else if (productResult.TransferTo.error_code == InternationalAggregatorySTATUSCODES.input_value_out_of_range)
                {
                    res.RstKey = 6;
                    res.Message = AggregatoryMESSAGE.input_value_out_of_range;
                }
                else if (productResult.TransferTo.error_code == InternationalAggregatorySTATUSCODES.system_not_available)
                {
                    res.RstKey = 6;
                    res.Message = AggregatoryMESSAGE.system_not_available;
                }
                else if (productResult.TransferTo.error_code == InternationalAggregatorySTATUSCODES.transaction_refused_by_the_operator)
                {
                    res.RstKey = 6;
                    res.Message = AggregatoryMESSAGE.transaction_refused_by_the_operator;
                }
                else if (productResult.TransferTo.error_code == InternationalAggregatorySTATUSCODES.unknown_error)
                {
                    res.RstKey = 6;
                    res.Message = AggregatoryMESSAGE.unknown_error;
                }
                else
                {
                    res.RstKey = 3;
                }
            }
            catch (Exception ex)
            {
                // _logUtils.InterNationalAirtime(ex.Message);
                ex.Message.ErrorLog("InternatinalRechargeService", "GetProductList", ex.StackTrace + " ," + ex.Message);
                res.RstKey = 6;
                res.Message = ex.Message;
            }

            return res;
        }

        public string postXMLData(string destinationUrl, string requestXml)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(destinationUrl);
            string responseStr = string.Empty;
            byte[] bytes;
            bytes = System.Text.Encoding.ASCII.GetBytes(requestXml);
            request.ContentType = "text/xml; encoding='utf-8'";
            request.ContentLength = bytes.Length;
            request.Method = "POST";
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(bytes, 0, bytes.Length);
            requestStream.Close();
            HttpWebResponse response;
            response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream responseStream = response.GetResponseStream();
                responseStr = new StreamReader(responseStream).ReadToEnd();

            }
            return responseStr;
        }

        public static string MD5(string s)
        {
            using (var provider = System.Security.Cryptography.MD5.Create())
            {
                StringBuilder builder = new StringBuilder();

                foreach (byte b in provider.ComputeHash(Encoding.UTF8.GetBytes(s)))
                    builder.Append(b.ToString("x2").ToLower());

                return builder.ToString();
            }
        }

        public async Task<AddMoneyAggregatorResponse> InternationalAirtimeServices(RechargeAirtimeInternationalAggregatorRequest request, string sessionToken, long WalletUserId = 0)
        {
            var response = new AddMoneyAggregatorResponse();

            var _commissionRequest = new CalculateCommissionRequest();
            var _commission = new CalculateCommissionResponse();
            var transationInitiate = new TransactionInitiateRequest();
            var _responseModel = new AddMoneyAggregatorResponse();
            string customer = request.customer.Length.ToString();
            string BundleId = "";
            string customerMobile = "";

            var sender = await _walletUserRepository.GetUserDetailById(request.WalletUserId);
            var data = await _walletUserRepository.GetCurrentUser(request.WalletUserId);
            var WalletService = await _masterDataRepository.GetWalletServicesByIdOrChannel(request.channel, request.ServiceCategoryId);
            var subcategory = await _masterDataRepository.GetWalletSubCategoriesById(request.ServiceCategoryId);
            // bool IsdocVerified = await _walletUserRepository.IsDocVerified(sender.WalletUserId, (int)sender.DocumetStatus);
            bool IsdocVerified = await _walletUserRepository.IsDocVerifiedMOMO((int)sender.DocumetStatus);
            var transactionLimit = await _payMoneyRepository.GetTransactionLimitForPayment(data.WalletUserId);
            int limit = transactionLimit != null ? Convert.ToInt32(transactionLimit.transactionlimit) : 0;
            var transactionHistory = await _payMoneyRepository.GetAllTransactionByDate(data.WalletUserId);
            int totalAmountTransfered = transactionHistory != null ? Convert.ToInt32(transactionHistory.totalAmount) : 0;
            var invoiceNumber = await _masterDataRepository.GetInvoiceNumber();

            //checking requested product is currect or not
            var requestForCheckingProduct = new InternationalAirtimeRequest
            {
                Amount = request.AmountInLocalCountry,
                MobileNo = request.MobileNo,
                IsdCode = request.ISD
            };
            var isCurrectProduct = await IsCurrectProduct(requestForCheckingProduct);
            //Get currency rate for internation recharge usd.
            var currencyDetail = _masterDataRepository.GetCurrencyRate();

            decimal dollarRate = Convert.ToDecimal(currencyDetail.DollarRate);
            //decimal CfaRate = Convert.ToDecimal(currencyDetail.LERate);
            decimal requestAmount = Convert.ToDecimal(isCurrectProduct.AmountInUsd);
            // string AmountInUsd = request.Amount;
            decimal currencyConversion = (requestAmount / dollarRate);
            request.Amount = currencyConversion.ToString("0.000");

            if (sender.IsOtpVerified == true) //mobile exist or not then 
            {
                if (sender.IsEmailVerified == true)
                {
                    if (isCurrectProduct.RstKey == 1 && isCurrectProduct.AmountInUsd != null && isCurrectProduct.AmountInLocalCountry != null)
                    {

                        if (subcategory != null)
                        {
                            request.serviceCategory = subcategory.CategoryName;
                            if (WalletService != null)
                            {
                                var adminKeyPair = AES256.AdminKeyPair;


                                if (sender.IsDisabledTransaction == false)
                                {
                                    if (IsdocVerified == true)
                                    {
                                        if (transactionLimit == null || transactionLimit.transactionlimit == null || limit >= (Convert.ToDecimal(request.Amount) + totalAmountTransfered))
                                        {
                                            if (sender != null && !string.IsNullOrEmpty(sender.MobileNo))
                                            {
                                                // var data = db.WalletUsers.Where(x => x.WalletUserId == sender.WalletUserId).FirstOrDefault();

                                                if (sender != null)
                                                {
                                                    if (!string.IsNullOrEmpty(sender.CurrentBalance) && !sender.CurrentBalance.IsZero() && Convert.ToDecimal(sender.CurrentBalance) > 0)
                                                    {
                                                        _commissionRequest.CurrentBalance = Convert.ToDecimal(sender.CurrentBalance);
                                                        _commissionRequest.IsRoundOff = true;
                                                        _commissionRequest.TransactionAmount = Convert.ToDecimal(request.Amount);
                                                        _commissionRequest.WalletServiceId = WalletService.WalletServiceId;
                                                        _commission = await _setCommisionRepository.CalculateCommission(_commissionRequest);
                                                    }
                                                    else
                                                    {
                                                        response.RstKey = 6;
                                                        response.Message = ResponseMessages.INSUFICIENT_BALANCE;
                                                        return response;
                                                    }
                                                    string transactionInitiate = string.Empty;
                                                    decimal amountWithCommision = _commission.AmountWithCommission;
                                                    decimal currentBalance = Convert.ToDecimal(sender.CurrentBalance);
                                                    //
                                                    if (await _commonServices.IsUserValid(sessionToken, request.WalletUserId, amountWithCommision))
                                                    {
                                                        if (currentBalance > 0 && currentBalance >= amountWithCommision && _commission.CurrentBalance > 0 && _commission.CurrentBalance >= amountWithCommision)
                                                        {
                                                            #region Prepare the Model for Request
                                                            string xml = string.Empty;
                                                            string url = CommonSetting.InternationalAirtimeUrl;
                                                            var token = CommonSetting.MerchantToken;
                                                            string md5Generate = CommonSetting.loginkey + token + invoiceNumber.AutoDigit;
                                                            var result = MD5(md5Generate);

                                                            XDocument xmlDocument = new XDocument(new XDeclaration("1.0", "UTF-8", null));
                                                            XElement xBillFetchReqElement = new XElement("xml");
                                                            xBillFetchReqElement.Add(new XElement("login", CommonSetting.loginkey));
                                                            xBillFetchReqElement.Add(new XElement("key", invoiceNumber.AutoDigit));
                                                            xBillFetchReqElement.Add(new XElement("md5", result));
                                                            xBillFetchReqElement.Add(new XElement("msisdn", request.msisdn));
                                                            xBillFetchReqElement.Add(new XElement("sms", request.Comment));
                                                            xBillFetchReqElement.Add(new XElement("destination_msisdn", request.ISD + request.MobileNo));
                                                            xBillFetchReqElement.Add(new XElement("product", isCurrectProduct.AmountInLocalCountry));
                                                            xBillFetchReqElement.Add(new XElement("cid1", "Ezipay Sarl"));
                                                            xBillFetchReqElement.Add(new XElement("sender_sms", request.Comment));
                                                            xBillFetchReqElement.Add(new XElement("sender_text", request.Comment));
                                                            xBillFetchReqElement.Add(new XElement("delivered_amount_info", isCurrectProduct.AmountInUsd));

                                                            xBillFetchReqElement.Add(new XElement("return_timestamp", "1"));
                                                            xBillFetchReqElement.Add(new XElement("return_version", "1"));
                                                            xBillFetchReqElement.Add(new XElement("return_service_fee", "1"));
                                                            xBillFetchReqElement.Add(new XElement("action", "topup"));
                                                            xmlDocument.Add(xBillFetchReqElement);
                                                            StringWriter stringWriter = new Utf8StringWriter();
                                                            xmlDocument.Save(stringWriter, SaveOptions.None);

                                                            #region transaction initiate request 
                                                            //This is for transaction initiate request all---
                                                            transationInitiate.InvoiceNumber = invoiceNumber.InvoiceNumber;
                                                            transationInitiate.ReceiverNumber = request.customer;
                                                            transationInitiate.ServiceName = WalletService.ServiceName;
                                                            transationInitiate.RequestedAmount = _commission.TransactionAmount.ToString();
                                                            transationInitiate.TransactionStatus = (int)TransactionStatus.Pending;
                                                            transationInitiate.WalletUserId = sender.WalletUserId;
                                                            transationInitiate.UserReferanceNumber = invoiceNumber.AutoDigit;
                                                            transationInitiate.CurrentBalance = sender.CurrentBalance;
                                                            transationInitiate.AfterTransactionBalance = _commission.UpdatedCurrentBalance.ToString();
                                                            transationInitiate.UserName = sender.FirstName + " " + sender.LastName;
                                                            transationInitiate.ReceiverCurrentBalance = _commission.UpdatedCurrentBalance.ToString();
                                                            transationInitiate.ReceiverWalletUserId = sender.WalletUserId;
                                                            transationInitiate.CreatedDate = DateTime.UtcNow;
                                                            transationInitiate.UpdatedDate = DateTime.UtcNow;
                                                            transationInitiate.IsActive = true;
                                                            transationInitiate.IsDeleted = false;
                                                            transationInitiate.JsonRequest = stringWriter.ToString();
                                                            transationInitiate.JsonResponse = "";
                                                            transationInitiate = await _airtimeRepository.SaveTransactionInitiateRequest(transationInitiate);


                                                            //Update user's currentbalance amount from wallet
                                                            data.CurrentBalance = Convert.ToString(_commission.UpdatedCurrentBalance);
                                                            //calling pay method insert data in Database
                                                            await _walletUserRepository.UpdateUserDetail(data);
                                                            #endregion

                                                            #endregion
                                                            string responseString = "";
                                                            if (WalletService.HttpVerbs.ToLower() == AggragatorServiceVerbs.HttpPostVerb.ToLower())
                                                            {
                                                                var response2 = new StringContent(stringWriter.ToString(), Encoding.UTF8, "application/xml");
                                                                var rr = postXMLData(url, stringWriter.ToString());
                                                                XmlDocument doc = new XmlDocument();
                                                                doc.LoadXml(rr);
                                                                responseString = JsonConvert.SerializeXmlNode(doc);
                                                                var finalResult = JsonConvert.DeserializeObject<GetProductListResponse>(responseString);
                                                                if (finalResult.TransferTo.error_code == "0")
                                                                {
                                                                    _responseModel.StatusCode = AggregatorySTATUSCODES.SUCCESSFUL;
                                                                    //_responseModel.TransactionId = finalResult.TransferTo.authentication_key;
                                                                   //replace authentication_key : - transactionid 17/11/21
                                                                    _responseModel.TransactionId = finalResult.TransferTo.transactionid;
                                                                }
                                                                if (finalResult.TransferTo.error_code == "204")
                                                                {
                                                                    _responseModel.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                                    _responseModel.TransactionId = finalResult.TransferTo.transactionid;
                                                                }
                                                                //
                                                                if (finalResult.TransferTo.error_code == "215")
                                                                {
                                                                    _responseModel.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                                    _responseModel.TransactionId = finalResult.TransferTo.transactionid;
                                                                }
                                                                if (finalResult.TransferTo.error_code == "301")
                                                                {
                                                                    _responseModel.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                                    _responseModel.TransactionId = finalResult.TransferTo.transactionid;
                                                                }
                                                                if (finalResult.TransferTo.error_code == "214")
                                                                {
                                                                    _responseModel.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                                    _responseModel.TransactionId = finalResult.TransferTo.transactionid;
                                                                }
                                                                if (finalResult.TransferTo.error_code == "998")
                                                                {
                                                                    _responseModel.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                                    _responseModel.TransactionId = finalResult.TransferTo.transactionid;
                                                                }
                                                                if (finalResult.TransferTo.error_code == "999")
                                                                {
                                                                    _responseModel.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                                    _responseModel.TransactionId = finalResult.TransferTo.transactionid;
                                                                }
                                                                if (finalResult.TransferTo.error_code == "231")
                                                                {
                                                                    _responseModel.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                                    _responseModel.TransactionId = finalResult.TransferTo.transactionid;
                                                                }
                                                                //else
                                                                //{
                                                                //    _responseModel.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                                //    _responseModel.TransactionId = finalResult.TransferTo.authentication_key;
                                                                //}
                                                            }
                                                            else
                                                            {
                                                                var response2 = new StringContent(stringWriter.ToString(), Encoding.UTF8, "application/xml");
                                                                var rr = postXMLData(url, stringWriter.ToString());
                                                                XmlDocument doc = new XmlDocument();
                                                                doc.LoadXml(rr);
                                                                responseString = JsonConvert.SerializeXmlNode(doc);
                                                                var finalResult = JsonConvert.DeserializeObject<GetProductListResponse>(responseString);
                                                                if (finalResult.TransferTo.error_code == "0")
                                                                {
                                                                    _responseModel.StatusCode = InternationalAggregatorySTATUSCODES.SUCCESSFUL;
                                                                    _responseModel.TransactionId = finalResult.TransferTo.transactionid;
                                                                }
                                                                if (finalResult.TransferTo.error_code == "204")
                                                                {
                                                                    _responseModel.StatusCode = InternationalAggregatorySTATUSCODES.destination_number_is_not_avalid;
                                                                    _responseModel.TransactionId = finalResult.TransferTo.transactionid;
                                                                }
                                                                if (finalResult.TransferTo.error_code == "215")
                                                                {
                                                                    _responseModel.StatusCode = InternationalAggregatorySTATUSCODES.servicetothisdestinationoperatoristemporarilyunavailable;
                                                                    _responseModel.TransactionId = finalResult.TransferTo.transactionid;
                                                                }
                                                                if (finalResult.TransferTo.error_code == "301")
                                                                {
                                                                    _responseModel.StatusCode = InternationalAggregatorySTATUSCODES.input_value_out_of_range;
                                                                    _responseModel.TransactionId = finalResult.TransferTo.transactionid;
                                                                }
                                                                if (finalResult.TransferTo.error_code == "214")
                                                                {
                                                                    _responseModel.StatusCode = InternationalAggregatorySTATUSCODES.transaction_refused_by_the_operator;
                                                                    _responseModel.TransactionId = finalResult.TransferTo.transactionid;
                                                                }
                                                                if (finalResult.TransferTo.error_code == "998")
                                                                {
                                                                    _responseModel.StatusCode = InternationalAggregatorySTATUSCODES.system_not_available;
                                                                    _responseModel.TransactionId = finalResult.TransferTo.transactionid;
                                                                }
                                                                if (finalResult.TransferTo.error_code == "999")
                                                                {
                                                                    _responseModel.StatusCode = InternationalAggregatorySTATUSCODES.unknown_error;
                                                                    _responseModel.TransactionId = finalResult.TransferTo.transactionid;
                                                                }
                                                                if (finalResult.TransferTo.error_code == "231")
                                                                {
                                                                    _responseModel.StatusCode = InternationalAggregatorySTATUSCODES.Recipientreachedmaximumtopupamount;
                                                                    _responseModel.TransactionId = finalResult.TransferTo.transactionid;
                                                                }
                                                                //else
                                                                //{
                                                                //    _responseModel.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                                //    _responseModel.TransactionId = finalResult.TransferTo.authentication_key;
                                                                //}
                                                            }
                                                            var TransactionInitial = await _airtimeRepository.GetTransactionInitiateRequest(transationInitiate.Id);
                                                            TransactionInitial.JsonResponse = "InternationalRecharge Response" + responseString;
                                                            await _airtimeRepository.UpdateTransactionInitiateRequest(TransactionInitial);
                                                            LogTransactionTypes.Response.SaveTransactionLog(LogTransactionNameTypes.PayMoney + subcategory.CategoryName, responseString, "Aggregator Url : ");
                                                            var errorResponse = "The remote server returned an error: (500) Internal Server Error.";
                                                            if (!string.IsNullOrEmpty(responseString) && responseString != errorResponse)
                                                            {

                                                                if (_responseModel != null && !string.IsNullOrEmpty(_responseModel.StatusCode) &&
                                                                    (_responseModel.StatusCode == AggregatorySTATUSCODES.SUCCESSFUL
                                                                    || _responseModel.StatusCode == AggregatorySTATUSCODES.PENDING
                                                                    || _responseModel.StatusCode == AggregatorySTATUSCODES.FAILED
                                                                    || _responseModel.StatusCode == AggregatorySTATUSCODES.EXCEPTION
                                                                    || _responseModel.StatusCode == InternationalAggregatorySTATUSCODES.destination_number_is_not_avalid
                                                                    || _responseModel.StatusCode == InternationalAggregatorySTATUSCODES.input_value_out_of_range
                                                                    || _responseModel.StatusCode == InternationalAggregatorySTATUSCODES.transaction_refused_by_the_operator
                                                                    || _responseModel.StatusCode == InternationalAggregatorySTATUSCODES.system_not_available
                                                                    || _responseModel.StatusCode == InternationalAggregatorySTATUSCODES.unknown_error
                                                                    || _responseModel.StatusCode == InternationalAggregatorySTATUSCODES.servicetothisdestinationoperatoristemporarilyunavailable
                                                                    || _responseModel.StatusCode == InternationalAggregatorySTATUSCODES.Recipientreachedmaximumtopupamount
                                                                    ))
                                                                {
                                                                    var _tranDate = DateTime.UtcNow;
                                                                    _responseModel.FormatedTransactionDate = string.Format("{0:d}", DateTime.Now) + "" + string.Format("{0:T}", DateTime.Now);

                                                                    _responseModel.AccountNo = request.customer;
                                                                    _responseModel.ToMobileNo = request.customer;

                                                                    _responseModel.Amount = request.Amount;
                                                                    _responseModel.TransactionDate = _tranDate;
                                                                    _responseModel.CurrentBalance = sender.CurrentBalance;

                                                                    var tran = new WalletTransaction();
                                                                    tran.BeneficiaryName = request.BeneficiaryName;
                                                                    tran.CreatedDate = _tranDate;
                                                                    tran.TransactionInitiateRequestId = transationInitiate.Id;
                                                                    tran.UpdatedDate = _tranDate;
                                                                    //Self Account 
                                                                    tran.ReceiverId = sender.WalletUserId;
                                                                    //Sender
                                                                    tran.WalletServiceId = WalletService.WalletServiceId;
                                                                    tran.TransactionType = AggragatorServiceType.CREDIT;
                                                                    tran.TransactionTypeInfo = (int)TransactionTypeInfo.PaidByPayServices;
                                                                    tran.VoucherCode = string.Empty;
                                                                    tran.SenderId = sender.WalletUserId;
                                                                    tran.WalletAmount = request.Amount;
                                                                    tran.ServiceTax = "0";
                                                                    tran.ServiceTaxRate = 0;
                                                                    tran.DisplayContent = request.DisplayContent;
                                                                    tran.UpdatedOn = DateTime.UtcNow;
                                                                    tran.IsdCode = request.ISD;
                                                                    tran.AccountNo = request.customer;// string.Empty;                                                  
                                                                    tran.BankTransactionId = string.Empty;
                                                                    tran.IsBankTransaction = false;
                                                                    tran.BankBranchCode = string.Empty;
                                                                    tran.TransactionId = _responseModel.TransactionId;
                                                                    response.TransactionId = tran.TransactionId;
                                                                    response.TransactionDate = DateTime.UtcNow;
                                                                    response.Amount = request.Amount;
                                                                    response.Message = "Pay Money successfully.";
                                                                    response.AccountNo = request.customer;
                                                                    response.CurrentBalance = sender.CurrentBalance;
                                                                    int _TransactionStatus = 0;
                                                                    if (_responseModel.StatusCode == AggregatorySTATUSCODES.SUCCESSFUL)
                                                                    {
                                                                        _TransactionStatus = (int)TransactionStatus.Completed;


                                                                        //-------------sending email after success transaction-----------------
                                                                        try
                                                                        {
                                                                            string filename = CommonSetting.successfullTransaction;

                                                                            var body = _sendEmails.ReadEmailformats(filename);
                                                                            body = body.Replace("$$FirstName$$", sender.FirstName + " " + sender.LastName);
                                                                            body = body.Replace("$$DisplayContent$$", WalletService.ServiceName);
                                                                            body = body.Replace("$$customer$$", request.customer);
                                                                            body = body.Replace("$$amount$$", "XOF " + request.Amount);
                                                                            body = body.Replace("$$ServiceTaxAmount$$", "XOF " + _commission.CommissionAmount);
                                                                            body = body.Replace("$$AmountWithCommission$$", "XOF " + _commission.AmountWithCommission);
                                                                            body = body.Replace("$$TransactionId$$", _responseModel.TransactionId);

                                                                            var requ = new EmailModel
                                                                            {
                                                                                TO = sender.EmailId,
                                                                                Subject = ResponseEmailMessage.PAYMENT_SUCCESS,
                                                                                Body = body
                                                                            };

                                                                            _sendEmails.SendEmail(requ);
                                                                        }
                                                                        catch
                                                                        {

                                                                        }
                                                                    }
                                                                    else if (_responseModel.StatusCode == AggregatorySTATUSCODES.PENDING)
                                                                    {
                                                                        _TransactionStatus = (int)TransactionStatus.Pending;
                                                                    }
                                                                    else if (_responseModel.StatusCode == InternationalAggregatorySTATUSCODES.Recipientreachedmaximumtopupamount)
                                                                    {
                                                                        transationInitiate = await _airtimeRepository.GetTransactionInitiateRequest(transationInitiate.Id);

                                                                        var refundAmt = Convert.ToDecimal(data.CurrentBalance) + _commission.AmountWithCommission;
                                                                        data.CurrentBalance = Convert.ToString(refundAmt);
                                                                        transationInitiate.AfterTransactionBalance = data.CurrentBalance;
                                                                        transationInitiate.ReceiverCurrentBalance = data.CurrentBalance;
                                                                        await _airtimeRepository.UpdateTransactionInitiateRequest(transationInitiate);
                                                                        await _walletUserRepository.UpdateUserDetail(data);
                                                                        _TransactionStatus = (int)TransactionStatus.Failed;
                                                                    }
                                                                    //
                                                                    else if (_responseModel.StatusCode == InternationalAggregatorySTATUSCODES.servicetothisdestinationoperatoristemporarilyunavailable)
                                                                    {
                                                                        transationInitiate = await _airtimeRepository.GetTransactionInitiateRequest(transationInitiate.Id);

                                                                        var refundAmt = Convert.ToDecimal(data.CurrentBalance) + _commission.AmountWithCommission;
                                                                        data.CurrentBalance = Convert.ToString(refundAmt);
                                                                        transationInitiate.AfterTransactionBalance = data.CurrentBalance;
                                                                        transationInitiate.ReceiverCurrentBalance = data.CurrentBalance;
                                                                        await _airtimeRepository.UpdateTransactionInitiateRequest(transationInitiate);
                                                                        await _walletUserRepository.UpdateUserDetail(data);
                                                                        _TransactionStatus = (int)TransactionStatus.Failed;
                                                                    }
                                                                    else if (_responseModel.StatusCode == InternationalAggregatorySTATUSCODES.destination_number_is_not_avalid)
                                                                    {
                                                                        transationInitiate = await _airtimeRepository.GetTransactionInitiateRequest(transationInitiate.Id);

                                                                        var refundAmt = Convert.ToDecimal(data.CurrentBalance) + _commission.AmountWithCommission;
                                                                        data.CurrentBalance = Convert.ToString(refundAmt);
                                                                        transationInitiate.AfterTransactionBalance = data.CurrentBalance;
                                                                        transationInitiate.ReceiverCurrentBalance = data.CurrentBalance;
                                                                        await _airtimeRepository.UpdateTransactionInitiateRequest(transationInitiate);
                                                                        await _walletUserRepository.UpdateUserDetail(data);
                                                                        _TransactionStatus = (int)TransactionStatus.Failed;
                                                                    }
                                                                    else if (_responseModel.StatusCode == InternationalAggregatorySTATUSCODES.input_value_out_of_range)
                                                                    {
                                                                        transationInitiate = await _airtimeRepository.GetTransactionInitiateRequest(transationInitiate.Id);

                                                                        var refundAmt = Convert.ToDecimal(data.CurrentBalance) + _commission.AmountWithCommission;
                                                                        data.CurrentBalance = Convert.ToString(refundAmt);
                                                                        transationInitiate.AfterTransactionBalance = data.CurrentBalance;
                                                                        transationInitiate.ReceiverCurrentBalance = data.CurrentBalance;
                                                                        await _airtimeRepository.UpdateTransactionInitiateRequest(transationInitiate);
                                                                        await _walletUserRepository.UpdateUserDetail(data);
                                                                        _TransactionStatus = (int)TransactionStatus.Failed;
                                                                    }
                                                                    else if (_responseModel.StatusCode == InternationalAggregatorySTATUSCODES.transaction_refused_by_the_operator)
                                                                    {
                                                                        transationInitiate = await _airtimeRepository.GetTransactionInitiateRequest(transationInitiate.Id);

                                                                        var refundAmt = Convert.ToDecimal(data.CurrentBalance) + _commission.AmountWithCommission;
                                                                        data.CurrentBalance = Convert.ToString(refundAmt);
                                                                        transationInitiate.AfterTransactionBalance = data.CurrentBalance;
                                                                        transationInitiate.ReceiverCurrentBalance = data.CurrentBalance;
                                                                        await _airtimeRepository.UpdateTransactionInitiateRequest(transationInitiate);
                                                                        await _walletUserRepository.UpdateUserDetail(data);
                                                                        _TransactionStatus = (int)TransactionStatus.Failed;
                                                                    }
                                                                    else if (_responseModel.StatusCode == InternationalAggregatorySTATUSCODES.system_not_available)
                                                                    {
                                                                        transationInitiate = await _airtimeRepository.GetTransactionInitiateRequest(transationInitiate.Id);

                                                                        var refundAmt = Convert.ToDecimal(data.CurrentBalance) + _commission.AmountWithCommission;
                                                                        data.CurrentBalance = Convert.ToString(refundAmt);
                                                                        transationInitiate.AfterTransactionBalance = data.CurrentBalance;
                                                                        transationInitiate.ReceiverCurrentBalance = data.CurrentBalance;
                                                                        await _airtimeRepository.UpdateTransactionInitiateRequest(transationInitiate);
                                                                        await _walletUserRepository.UpdateUserDetail(data);
                                                                        _TransactionStatus = (int)TransactionStatus.Failed;
                                                                    }
                                                                    else if (_responseModel.StatusCode == InternationalAggregatorySTATUSCODES.unknown_error)
                                                                    {
                                                                        transationInitiate = await _airtimeRepository.GetTransactionInitiateRequest(transationInitiate.Id);

                                                                        var refundAmt = Convert.ToDecimal(data.CurrentBalance) + _commission.AmountWithCommission;
                                                                        data.CurrentBalance = Convert.ToString(refundAmt);
                                                                        transationInitiate.AfterTransactionBalance = data.CurrentBalance;
                                                                        transationInitiate.ReceiverCurrentBalance = data.CurrentBalance;
                                                                        await _airtimeRepository.UpdateTransactionInitiateRequest(transationInitiate);
                                                                        await _walletUserRepository.UpdateUserDetail(data);
                                                                        _TransactionStatus = (int)TransactionStatus.Failed;
                                                                    }
                                                                    else if (_responseModel.StatusCode == AggregatorySTATUSCODES.FAILED)
                                                                    {
                                                                        transationInitiate = await _airtimeRepository.GetTransactionInitiateRequest(transationInitiate.Id);

                                                                        var refundAmt = Convert.ToDecimal(data.CurrentBalance) + _commission.AmountWithCommission;
                                                                        data.CurrentBalance = Convert.ToString(refundAmt);
                                                                        transationInitiate.AfterTransactionBalance = data.CurrentBalance;
                                                                        transationInitiate.ReceiverCurrentBalance = data.CurrentBalance;
                                                                        await _airtimeRepository.UpdateTransactionInitiateRequest(transationInitiate);
                                                                        await _walletUserRepository.UpdateUserDetail(data);
                                                                        _TransactionStatus = (int)TransactionStatus.Failed;
                                                                    }
                                                                    else if (_responseModel.StatusCode == AggregatorySTATUSCODES.EXCEPTION)
                                                                    {
                                                                        _TransactionStatus = (int)TransactionStatus.Failed;
                                                                    }
                                                                    tran.TransactionStatus = _TransactionStatus;
                                                                    tran.IsAdminTransaction = true;
                                                                    tran.IsActive = true;
                                                                    tran.IsDeleted = false;
                                                                    tran.InvoiceNo = invoiceNumber.InvoiceNumber;
                                                                    tran.Comments = request.Comment;
                                                                    tran.MerchantCommissionId = _commission.MerchantCommissionId;
                                                                    tran.MerchantCommissionAmount = Convert.ToString(_commission.MerchantCommissionAmount);
                                                                    tran.CommisionId = _commission.CommissionId;
                                                                    tran.CommisionAmount = Convert.ToString(_commission.CommissionAmount);

                                                                    tran.TotalAmount = Convert.ToString(_commission.AmountWithCommission);

                                                                    _responseModel.CurrentBalance = Convert.ToString(_commission.UpdatedCurrentBalance);
                                                                    //   db.WalletTransactions.Add(tran);


                                                                    if (tran.TransactionStatus == (int)TransactionStatus.Completed || tran.TransactionStatus == (int)TransactionStatus.Pending)
                                                                    {
                                                                        data.CurrentBalance = Convert.ToString(_commission.UpdatedCurrentBalance);
                                                                    }

                                                                    if (tran.TransactionStatus == (int)TransactionStatus.Completed)
                                                                    {

                                                                        #region PushNotification

                                                                        var pushModel = new PayMoneyPushModel();
                                                                        pushModel.TransactionDate = _tranDate;
                                                                        pushModel.TransactionId = tran.WalletTransactionId.ToString();
                                                                        pushModel.alert = request.Amount + " XOF has been debited from your account.";
                                                                        pushModel.Amount = request.Amount;
                                                                        pushModel.CurrentBalance = sender.CurrentBalance;
                                                                        pushModel.pushType = (int)PushType.PAYSERVICES;
                                                                        pushModel.TransactionTypeInfo = (int)TransactionTypeInfo.PaidByPayServices;
                                                                        var push = new PushNotificationModel();
                                                                        push.SenderId = sender.WalletUserId;
                                                                        push.deviceType = (int)sender.DeviceType;
                                                                        push.deviceKey = sender.DeviceToken;
                                                                        if ((int)sender.DeviceType == (int)DeviceTypes.ANDROID || (int)sender.DeviceType == (int)DeviceTypes.Web)
                                                                        {
                                                                            var aps = new PushPayload<PayMoneyPushModel>();
                                                                            var _data = new PushPayloadData<PayMoneyPushModel>();
                                                                            _data.notification = pushModel;
                                                                            aps.data = _data;
                                                                            aps.to = sender.DeviceToken;
                                                                            aps.collapse_key = string.Empty;
                                                                            push.message = JsonConvert.SerializeObject(aps);
                                                                            push.payload = pushModel;
                                                                        }
                                                                        if ((int)sender.DeviceType == (int)DeviceTypes.IOS)
                                                                        {
                                                                            var aps = new NotificationJsonResponse<PayMoneyPushModel>();
                                                                            aps.aps = pushModel;

                                                                            push.message = JsonConvert.SerializeObject(aps);
                                                                        }
                                                                        if (!string.IsNullOrEmpty(push.message))
                                                                        {
                                                                            _sendPushNotification.sendPushNotification(push);
                                                                        }
                                                                        #endregion
                                                                        response.RstKey = 1;
                                                                        response.Message = AggregatoryMESSAGE.SUCCESSFUL;
                                                                        response.StatusCode = AggregatorySTATUSCODES.SUCCESSFUL;
                                                                    }
                                                                    else
                                                                    {
                                                                        if (_responseModel.StatusCode == AggregatorySTATUSCODES.PENDING)
                                                                        {
                                                                            response.RstKey = 2;
                                                                            response.Message = AggregatoryMESSAGE.PENDING;
                                                                            response.StatusCode = AggregatorySTATUSCODES.PENDING;
                                                                        }
                                                                        //
                                                                        else if (_responseModel.StatusCode == InternationalAggregatorySTATUSCODES.Recipientreachedmaximumtopupamount)
                                                                        {
                                                                            response.RstKey = 6;
                                                                            response.Message = AggregatoryMESSAGE.Recipientreachedmaximumtopupamount;
                                                                            response.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                                        }
                                                                        else if (_responseModel.StatusCode == InternationalAggregatorySTATUSCODES.servicetothisdestinationoperatoristemporarilyunavailable)
                                                                        {
                                                                            response.RstKey = 6;
                                                                            response.Message = AggregatoryMESSAGE.servicetothisdestinationoperatoristemporarilyunavailable;
                                                                            response.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                                        }
                                                                        else if (_responseModel.StatusCode == InternationalAggregatorySTATUSCODES.destination_number_is_not_avalid)
                                                                        {
                                                                            response.RstKey = 6;
                                                                            response.Message = AggregatoryMESSAGE.destination_number_is_not_avalid;
                                                                            response.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                                        }
                                                                        else if (_responseModel.StatusCode == InternationalAggregatorySTATUSCODES.input_value_out_of_range)
                                                                        {
                                                                            response.RstKey = 6;
                                                                            response.Message = AggregatoryMESSAGE.input_value_out_of_range;
                                                                            response.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                                        }
                                                                        else if (_responseModel.StatusCode == InternationalAggregatorySTATUSCODES.system_not_available)
                                                                        {
                                                                            response.RstKey = 6;
                                                                            response.Message = AggregatoryMESSAGE.system_not_available;
                                                                            response.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                                        }
                                                                        else if (_responseModel.StatusCode == InternationalAggregatorySTATUSCODES.transaction_refused_by_the_operator)
                                                                        {
                                                                            response.RstKey = 6;
                                                                            response.Message = AggregatoryMESSAGE.transaction_refused_by_the_operator;
                                                                            response.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                                        }
                                                                        else if (_responseModel.StatusCode == InternationalAggregatorySTATUSCODES.unknown_error)
                                                                        {
                                                                            response.RstKey = 6;
                                                                            response.Message = AggregatoryMESSAGE.unknown_error;
                                                                            response.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                                        }
                                                                        else
                                                                        {
                                                                            response.RstKey = 3;
                                                                            response.Message = AggregatoryMESSAGE.FAILED;
                                                                            response.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                                        }
                                                                    }
                                                                    //calling pay method insert data in Database
                                                                    tran = await _airtimeRepository.AirtimeServices(tran);
                                                                    // await _walletUserRepository.UpdateUserDetail(data);
                                                                }
                                                                else if (_responseModel != null && !string.IsNullOrEmpty(_responseModel.StatusCode) && (_responseModel.StatusCode == AggregatorySTATUSCODES.FAILED))
                                                                {
                                                                    response.RstKey = 4;
                                                                    response.Message = ResponseMessages.AGGREGATOR_FAILED_ERROR;
                                                                }
                                                                else if (_responseModel != null && !string.IsNullOrEmpty(_responseModel.StatusCode) && (_responseModel.StatusCode == AggregatorySTATUSCODES.EXCEPTION))
                                                                {
                                                                    response.RstKey = 5;
                                                                    response.Message = ResponseMessages.AGGREGATOR_FAILED_EXCEPTION;
                                                                }
                                                                else
                                                                {
                                                                    response.RstKey = 6;
                                                                    response.Message = _responseModel.Message;
                                                                }
                                                            }
                                                            else
                                                            {

                                                                response.RstKey = 7;
                                                                response.Message = ResponseMessages.TRANSACTION_ERROR;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            response.RstKey = 10;
                                                            response.Message = ResponseMessages.INSUFICIENT_BALANCE;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        response.RstKey = 6;
                                                        response.Message = ResponseMessages.RequestIsnot;
                                                    }
                                                }
                                                else
                                                {
                                                    response.RstKey = 11;
                                                    response.Message = ResponseMessages.USER_NOT_REGISTERED;
                                                }
                                            }
                                            else
                                            {
                                                response.RstKey = 12;
                                                response.Message = ResponseMessages.USER_NOT_REGISTERED;
                                            }
                                        }
                                        else
                                        {
                                            response.RstKey = 6;
                                            response.Message = ResponseMessageKyc.TRANSACTION_LIMIT;
                                        }
                                    }
                                    else if (sender.DocumetStatus == 0 || sender.DocumetStatus == null)
                                    {
                                        response.RstKey = 13;
                                        response.Message = ResponseMessageKyc.FAILED_Doc_NotUploaded;
                                    }
                                    else if (sender.DocumetStatus == 1 || sender.DocumetStatus == null)
                                    {
                                        response.RstKey = 14;
                                        response.Message = ResponseMessageKyc.FAILED_Doc_Pending;
                                    }
                                    else if (sender.DocumetStatus == 4 || sender.DocumetStatus == null)
                                    {
                                        response.RstKey = 15;
                                        response.Message = ResponseMessageKyc.Doc_Not_visible;
                                    }
                                    else
                                    {
                                        response.RstKey = 16;
                                        response.Message = ResponseMessageKyc.Doc_Rejected;
                                    }
                                }
                                else
                                {
                                    response.RstKey = 17;
                                    response.Message = ResponseMessageKyc.TRANSACTION_DISABLED;
                                }
                                response.AccountNo = request.customer;
                                response.DocStatus = IsdocVerified;
                                response.DocumetStatus = (int)sender.DocumetStatus;
                                response.CurrentBalance = data.CurrentBalance;
                                response.ToMobileNo = request.customer;
                                response.MobileNo = request.customer;
                                response.Amount = request.Amount;
                                response.TransactionDate = DateTime.UtcNow;
                                //response.RstKey = 6;
                            }
                            else
                            {
                                response.RstKey = 18;
                                response.Message = ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED;
                            }
                        }
                        else
                        {
                            response.RstKey = 19;
                            response.Message = ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND;
                        }
                    }
                    else
                    {
                        response.Status = (int)WalletTransactionStatus.FAILED;
                        response.Message = ResponseMessages.VERIFICATION_PRODUCT;
                        response.RstKey = 6;
                    }
                }
                else
                {
                    response.Status = (int)WalletTransactionStatus.FAILED;
                    response.Message = ResponseMessages.EMAIL_VERIFICATION_PENDING;
                    response.RstKey = 6;
                    // "Please verify your email id.";
                }
            }
            else
            {
                response.RstKey = 21;
                response.Status = (int)WalletTransactionStatus.FAILED;
                response.Message = ResponseMessages.MobileNotVerify;
            }
            return response;
        }

        public async Task<IsCurrectProduct> IsCurrectProduct(InternationalAirtimeRequest request)
        {
            var res = new IsCurrectProduct();
            //   var res = new InternationalAirtimeResponse();
            // var currenyRates = new List<InternationalAirtimeAmountResponse>();
            // XmlSerializer xsSubmit = new XmlSerializer(typeof(xml));
            //StringContent response = new StringContent();
            string xml = string.Empty;
            string url = CommonSetting.InternationalAirtimeUrl;
            var invoiceNumber = await _masterDataRepository.GetInvoiceNumber();
            var currencyDetail = _masterDataRepository.GetCurrencyRate();
            try
            {
                decimal dollarRate = Convert.ToDecimal(currencyDetail.DollarRate);

                var token = CommonSetting.MerchantToken;
                string md5Generate = CommonSetting.loginkey + token + invoiceNumber.AutoDigit;
                var result = MD5(md5Generate);
                string customer = request.MobileNo.Substring(0, 1);
                if (customer == "0")
                {
                    customer = request.MobileNo.Remove(0, 1);
                }
                else
                {
                    customer = request.MobileNo;
                }

                XDocument xmlDocument = new XDocument(new XDeclaration("1.0", "UTF-8", null));
                XElement xBillFetchReqElement = new XElement("xml");
                xBillFetchReqElement.Add(new XElement("login", CommonSetting.loginkey));
                xBillFetchReqElement.Add(new XElement("key", invoiceNumber.AutoDigit));
                xBillFetchReqElement.Add(new XElement("md5", result));
                xBillFetchReqElement.Add(new XElement("destination_msisdn", request.IsdCode + request.MobileNo));
                xBillFetchReqElement.Add(new XElement("delivered_amount_info", "1"));
                xBillFetchReqElement.Add(new XElement("return_service_fee", "1"));
                xBillFetchReqElement.Add(new XElement("action", "msisdn_info"));
                xmlDocument.Add(xBillFetchReqElement);
                StringWriter stringWriter = new Utf8StringWriter();
                xmlDocument.Save(stringWriter, SaveOptions.None);

                var response = new StringContent(stringWriter.ToString(), Encoding.UTF8, "application/xml");
                var rr = postXMLData(url, stringWriter.ToString());
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(rr);
                string jsonText = JsonConvert.SerializeXmlNode(doc);
                string display = string.Empty;
                var productResult = JsonConvert.DeserializeObject<GetProductListResponse>(jsonText);
                if (productResult.TransferTo.error_code == "0")
                {
                    var products = productResult.TransferTo.product_list.Split(',');
                    var retail_price_list = productResult.TransferTo.retail_price_list.Split(',');
                    var wholesale_price_list = productResult.TransferTo.wholesale_price_list.Split(',');

                    var index = Array.FindIndex(products, row => row == request.Amount);
                    if (index == 0 || index > 0)
                    {
                        var UsdAmt = retail_price_list[index];
                        var amtLocalCountry = products[index];
                        res.AmountInLocalCountry = amtLocalCountry;
                        res.AmountInUsd = UsdAmt;
                        res.RstKey = 1;
                    }
                    else
                    {
                        res.RstKey = 2;
                    }
                    // To convert JSON text contained in string json into an XML node                  
                }
                else
                {
                    res.RstKey = 3;
                }
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("InternatinalRechargeService", "GetProductList", ex.StackTrace + " ," + ex.Message);
                res.RstKey = 6;
                res.Message = ex.Message;
            }

            return res;
        }
        //dth


        public static HttpRequestMessage GetHttpRequestMessage(string url, HttpMethod method = null, HttpContent content = null)
        {
            string api_key = CommonSetting.API_KEY;
            string api_secret = CommonSetting.secret_KEY;

            int epoch = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            string nonce = epoch.ToString();
            string message = api_key + nonce;

            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(api_secret);
            HMACSHA256 hmac = new HMACSHA256(keyByte);
            byte[] messageBytes = encoding.GetBytes(message);
            byte[] hashmessage = hmac.ComputeHash(messageBytes);

            string hmac_base64 = Convert.ToBase64String(hashmessage);
            if (method == null)
                method = HttpMethod.Get;
            HttpRequestMessage request = new HttpRequestMessage(method, url);
            request.Headers.Add("X-TransferTo-apikey", api_key);
            request.Headers.Add("X-TransferTo-nonce", nonce);
            request.Headers.Add("X-TransferTo-hmac", hmac_base64);
            if (content != null)
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                request.Content = content;
            }
            return request;
        }
        public async Task<string> Ping(string url)
        {
            using (var client = new HttpClient())
            {
                HttpRequestMessage request = GetHttpRequestMessage(url);
                HttpResponseMessage response = await client.SendAsync(request);
                string result = await response.Content.ReadAsStringAsync();

                return result;
            }
        }
        public async Task<string> PingForTransaction(string url, string jsonRequest)
        {
            using (var client = new HttpClient())
            {
                var method = HttpMethod.Post;
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                HttpRequestMessage request = GetHttpRequestMessage(url, method, content);
                HttpResponseMessage response = await client.SendAsync(request);
                string result = await response.Content.ReadAsStringAsync();

                return result;
            }
        }
        public async Task<InternationalDTHResponse> GetCountryList()
        {
            var response = new InternationalDTHResponse();
            var result = new CountryListResponse();
            var urlForCountry = CommonSetting.InternationalDTHUrl + "countries";
            try
            {
                var res = await Ping(urlForCountry);
                result = JsonConvert.DeserializeObject<CountryListResponse>(res);
                if (result.countries.Count > 0)
                {
                    response.RstKey = 1;
                    response.countryListResponse = result;
                }
                else
                {
                    response.RstKey = 6;
                    response.countryListResponse = result;
                }
            }
            catch (Exception ex)
            {
                response.RstKey = 6;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<InternationalDTHResponse> GetServiceList(GetServiceListRequest request)
        {
            var response = new InternationalDTHResponse();
            var result = new GetServiceListResponse();
            var urlForCountry = CommonSetting.InternationalDTHUrl + "services?country_id=" + request.country_id;
            try
            {
                var res = await Ping(urlForCountry);
                result = JsonConvert.DeserializeObject<GetServiceListResponse>(res);
                if (result != null)
                {
                    response.RstKey = 1;
                    response.getServiceListResponse = result;
                }
                else
                {
                    response.RstKey = 2;
                }
            }
            catch (Exception ex)
            {

            }
            return response;
        }

        public async Task<InternationalDTHResponse> GetOperatorList(GetServiceListRequest request)
        {
            var response = new InternationalDTHResponse();
            var result = new OperatorListReqponse();
            var ress = new List<OperatorListWithUrlReqponse>();
            var urlForCountry = CommonSetting.InternationalDTHUrl + "operators?service_id=" + request.service_id + "&country_id=" + request.country_id;

            try
            {
                var res = await Ping(urlForCountry);
                result = JsonConvert.DeserializeObject<OperatorListReqponse>(res);
                if (result != null)
                {
                    foreach (var item in result.operators)
                    {
                        string logoUrl = "https://operator-logo.dtone.com/logo-" + item.operator_id + "-3.png";
                        ress.Add(new OperatorListWithUrlReqponse
                        {
                            country = item.country,
                            country_id = item.country_id,
                            ImageUrl = logoUrl,
                            @operator = item.@operator,
                            operator_id = item.operator_id

                        });
                    }
                    //result.operators.ForEach({ })

                    response.RstKey = 1;
                    response.operatorListReqponse.AddRange(ress);
                }
                else
                {
                    response.RstKey = 2;
                }
            }
            catch (Exception ex)
            {

            }
            return response;
        }

        public async Task<InternationalDTHProductResponse> GetProductList(GetServiceListRequest request)
        {
            var response = new InternationalDTHProductResponse();
            var result = new GetDTHProductListResponse();
            var ress = new List<OperatorListWithUrlReqponse>();
            var currentRates = new List<ProductListDisplayResponse>();
            var urlForCountry = CommonSetting.InternationalDTHUrl + "operators/" + request.operator_id + "/products";
            string display = string.Empty;
            var currencyDetail = _masterDataRepository.GetCurrencyRate();
            decimal dollarRate = Convert.ToDecimal(currencyDetail.DollarRate);
            try
            {
                var res = await Ping(urlForCountry);
                result = JsonConvert.DeserializeObject<GetDTHProductListResponse>(res);
                if (result.fixed_value_recharges.Count > 0 && result.fixed_value_recharges.Count != 0)
                {
                    foreach (var item in result.fixed_value_recharges)
                    {
                        var amt = Convert.ToDecimal(item.wholesale_price) / dollarRate;
                        var finalAmt = amt.ToString("0.000");
                        display = item.product_value.ToString() + " " + item.product_currency + ", $ " + item.wholesale_price + "(XOF " + finalAmt + ")";

                        currentRates.Add(new ProductListDisplayResponse
                        {
                            DisplayContent = display,
                            AmountInUsd = item.wholesale_price.ToString(),
                            AmountInLocalCountry = item.product_value.ToString(),
                            operator_id = item.operator_id.ToString(),
                            country_id = item.country_id.ToString(),
                            product_id = item.product_id.ToString(),
                            product_value = item.product_value.ToString(),
                            retail_price = item.retail_price.ToString(),
                            service_id = item.service_id.ToString(),
                            wholesale_price = item.wholesale_price.ToString(),
                            AmountInLe = finalAmt
                        });
                    }
                    response.productListDisplayResponse.AddRange(currentRates);
                    response.RstKey = 1;
                    response.getDTHProductListResponse = result;
                }
                else if (result.fixed_value_vouchers.Count > 0)
                {
                    foreach (var item in result.fixed_value_vouchers)
                    {
                        var amt = Convert.ToDecimal(item.wholesale_price) / dollarRate;
                        var finalAmt = amt.ToString("0.000");
                        display = item.product_value.ToString() + " " + item.product_currency + ", $ " + item.wholesale_price + "(XOF " + finalAmt + ")";

                        currentRates.Add(new ProductListDisplayResponse
                        {
                            DisplayContent = display,
                            AmountInUsd = item.wholesale_price.ToString(),
                            AmountInLocalCountry = item.product_value.ToString(),
                            operator_id = item.operator_id.ToString(),
                            country_id = item.country_id.ToString(),
                            product_id = item.product_id.ToString(),
                            product_value = item.product_value.ToString(),
                            retail_price = item.retail_price.ToString(),
                            service_id = item.service_id.ToString(),
                            wholesale_price = item.wholesale_price.ToString(),
                            AmountInLe = finalAmt
                        });
                    }
                    response.productListDisplayResponse.AddRange(currentRates);
                    response.RstKey = 1;
                    response.getDTHProductListResponse = result;
                }
                else if (result.variable_value_recharges.Count > 0)
                {
                    foreach (var item in result.variable_value_recharges)
                    {
                        ////var amt = Convert.ToDecimal(item.wholesale_price) / dollarRate;
                        ////var finalAmt = amt.ToString("0.000");
                        //display = item.service.ToString() + " " + item.product_currency + ", " + item.@operator+"("+item.product_short_desc+")";

                        //currentRates.Add(new ProductListDisplayResponse
                        //{
                        //    DisplayContent = display,
                        //    //AmountInUsd = item.wholesale_price.ToString(),
                        //    //AmountInLocalCountry = item.product_value.ToString(),
                        //    operator_id = item.operator_id.ToString(),
                        //    country_id = item.country_id.ToString(),
                        //    product_id = item.product_id.ToString(),
                        //    //product_value = item.product_value.ToString(),
                        //    //retail_price = item.retail_price.ToString(),
                        //    service_id = item.service_id.ToString(),
                        //    //wholesale_price = item.wholesale_price.ToString(),
                        //    //AmountInLe = finalAmt
                        //});
                    }
                    //response.productListDisplayResponse.AddRange(currentRates);
                    response.RstKey = 6;
                    response.Message = "Servive not available from aggregator side";
                    // response.getDTHProductListResponse = result;
                }

                else
                {
                    response.RstKey = 3;
                }
            }
            catch (Exception ex)
            {

            }
            return response;
        }


        public async Task<AddMoneyAggregatorResponse> InternationalDTHServices(RechargeDthInternationalAggregatorRequest request, long WalletUserId = 0)
        {
            var response = new AddMoneyAggregatorResponse();
            var req = new DTHRechargeRequest();
            var _commissionRequest = new CalculateCommissionRequest();
            var _commission = new CalculateCommissionResponse();
            var transationInitiate = new TransactionInitiateRequest();
            var _responseModel = new AddMoneyAggregatorResponse();
            string customer = request.customer.Length.ToString();
            string BundleId = "";
            string customerMobile = "";

            var sender = await _walletUserRepository.GetUserDetailById(request.WalletUserId);
            var data = await _walletUserRepository.GetCurrentUser(request.WalletUserId);
            var WalletService = await _masterDataRepository.GetWalletServicesByIdOrChannel(request.channel, request.ServiceCategoryId);
            var subcategory = await _masterDataRepository.GetWalletSubCategoriesById(request.ServiceCategoryId);
            //bool IsdocVerified = await _walletUserRepository.IsDocVerified(sender.WalletUserId, (int)sender.DocumetStatus);

            bool IsdocVerified = await _walletUserRepository.IsDocVerifiedMOMO((int)sender.DocumetStatus);
            var transactionLimit = await _payMoneyRepository.GetTransactionLimitForPayment(data.WalletUserId);
            int limit = transactionLimit != null ? Convert.ToInt32(transactionLimit.transactionlimit) : 0;
            var transactionHistory = await _payMoneyRepository.GetAllTransactionByDate(data.WalletUserId);
            int totalAmountTransfered = transactionHistory != null ? Convert.ToInt32(transactionHistory.totalAmount) : 0;
            var invoiceNumber = await _masterDataRepository.GetInvoiceNumber();
            //Get currency rate for internation recharge usd.
            var currencyDetail = _masterDataRepository.GetCurrencyRate();

            decimal dollarRate = Convert.ToDecimal(currencyDetail.DollarRate);
            //decimal CfaRate = Convert.ToDecimal(currencyDetail.LERate);
            decimal requestAmount = Convert.ToDecimal(request.AmountInUsd);
            string AmountInUsd = request.Amount;
            decimal currencyConversion = (requestAmount / dollarRate);
            request.Amount = currencyConversion.ToString("0.000");

            if (sender.IsOtpVerified == true) //mobile exist or not then 
            {
                if (sender.IsEmailVerified == true)
                {
                    if (subcategory != null)
                    {
                        request.serviceCategory = subcategory.CategoryName;
                        if (WalletService != null)
                        {
                            var adminKeyPair = AES256.AdminKeyPair;
                            if (sender.IsDisabledTransaction == false)
                            {
                                if (IsdocVerified == true)
                                {
                                    if (transactionLimit == null || transactionLimit.transactionlimit == null || limit >= (Convert.ToDecimal(request.Amount) + totalAmountTransfered))
                                    {
                                        if (sender != null && !string.IsNullOrEmpty(sender.MobileNo))
                                        {
                                            // var data = db.WalletUsers.Where(x => x.WalletUserId == sender.WalletUserId).FirstOrDefault();

                                            if (sender != null)
                                            {
                                                if (!string.IsNullOrEmpty(sender.CurrentBalance) && !sender.CurrentBalance.IsZero() && Convert.ToDecimal(sender.CurrentBalance) > 0)
                                                {
                                                    _commissionRequest.CurrentBalance = Convert.ToDecimal(sender.CurrentBalance);
                                                    _commissionRequest.IsRoundOff = true;
                                                    _commissionRequest.TransactionAmount = Convert.ToDecimal(request.Amount);
                                                    _commissionRequest.WalletServiceId = WalletService.WalletServiceId;
                                                    _commission = await _setCommisionRepository.CalculateCommission(_commissionRequest);
                                                }
                                                else
                                                {
                                                    response.RstKey = 6;
                                                    response.Message = ResponseMessages.INSUFICIENT_BALANCE;
                                                    return response;
                                                }
                                                string transactionInitiate = string.Empty;
                                                decimal amountWithCommision = _commission.AmountWithCommission;
                                                decimal currentBalance = Convert.ToDecimal(sender.CurrentBalance);
                                                if (currentBalance > 0 && currentBalance >= amountWithCommision && _commission.CurrentBalance > 0 && _commission.CurrentBalance >= amountWithCommision)
                                                {
                                                    var urlForCountry = CommonSetting.InternationalDTHUrl + "transactions/fixed_value_recharges";
                                                    #region Prepare the Model for Request
                                                    req.account_number = request.account_number;
                                                    req.product_id = request.product_id;
                                                    req.external_id = invoiceNumber.AutoDigit;
                                                    req.simulation = "0";
                                                    req.sender_sms_notification = "1";
                                                    req.sender_sms_text = request.Comment;
                                                    req.recipient_sms_notification = "1";
                                                    req.recipient_sms_text = "Test";
                                                    req.sender.last_name = sender.LastName;
                                                    req.sender.middle_name = sender.LastName;
                                                    req.sender.first_name = sender.FirstName;
                                                    req.sender.email = sender.EmailId;
                                                    req.sender.mobile = sender.MobileNo;
                                                    req.recipient.last_name = "test";
                                                    req.recipient.middle_name = "test";
                                                    req.recipient.first_name = sender.FirstName;
                                                    req.recipient.mobile = request.account_number;

                                                    var jsonReq = JsonConvert.SerializeObject(req);

                                                    #region transaction initiate request 
                                                    //This is for transaction initiate request all---
                                                    transationInitiate.InvoiceNumber = invoiceNumber.InvoiceNumber;
                                                    transationInitiate.ReceiverNumber = request.customer;
                                                    transationInitiate.ServiceName = WalletService.ServiceName;
                                                    transationInitiate.RequestedAmount = _commission.TransactionAmount.ToString();
                                                    transationInitiate.TransactionStatus = (int)TransactionStatus.Pending;
                                                    transationInitiate.WalletUserId = sender.WalletUserId;
                                                    transationInitiate.UserReferanceNumber = invoiceNumber.AutoDigit;
                                                    transationInitiate.CurrentBalance = sender.CurrentBalance;
                                                    transationInitiate.AfterTransactionBalance = _commission.UpdatedCurrentBalance.ToString();
                                                    transationInitiate.UserName = sender.FirstName + " " + sender.LastName;
                                                    transationInitiate.ReceiverCurrentBalance = _commission.UpdatedCurrentBalance.ToString();
                                                    transationInitiate.ReceiverWalletUserId = sender.WalletUserId;
                                                    transationInitiate.CreatedDate = DateTime.UtcNow;
                                                    transationInitiate.UpdatedDate = DateTime.UtcNow;
                                                    transationInitiate.IsActive = true;
                                                    transationInitiate.IsDeleted = false;
                                                    transationInitiate.JsonRequest = jsonReq;
                                                    transationInitiate.JsonResponse = "";
                                                    transationInitiate = await _airtimeRepository.SaveTransactionInitiateRequest(transationInitiate);


                                                    //Update user's currentbalance amount from wallet
                                                    data.CurrentBalance = Convert.ToString(_commission.UpdatedCurrentBalance);
                                                    //calling pay method insert data in Database
                                                    await _walletUserRepository.UpdateUserDetail(data);
                                                    #endregion

                                                    #endregion
                                                    string responseString = "";
                                                    if (WalletService.HttpVerbs.ToLower() == AggragatorServiceVerbs.HttpPostVerb.ToLower())
                                                    {
                                                        responseString = await PingForTransaction(urlForCountry, jsonReq);
                                                    }
                                                    else
                                                    {
                                                        responseString = await PingForTransaction(urlForCountry, jsonReq);
                                                        // responseString = "{\"transaction_id\":\"7133049\",\"simulation\":0,\"status\":\"0\",\"status_message\":\"Transaction successful\",\"date\":\"2020-09-07 11:00:13\",\"account_number\":\"1152812333\",\"external_id\":\"039968\",\"operator_reference\":\"\",\"product_id\":\"2254\",\"product\":\"420 INR\",\"product_desc\":\"\",\"product_currency\":\"INR\",\"product_value\":420,\"local_currency\":\"INR\",\"local_value\":420,\"operator_id\":\"1809\",\"operator\":\"DTH Tata-Sky India\",\"country_id\":\"766\",\"country\":\"India\",\"account_currency\":\"USD\",\"wholesale_price\":6.27,\"retail_price\":6.97,\"fee\":0,\"sender\":{\"last_name\":\"test2\",\"middle_name\":\"test2\",\"first_name\":\"test2\",\"email\":\"test2@yopmail.com\",\"mobile\":\"2424242424\",\"custom_field_1\":\"\",\"custom_field_2\":\"\",\"custom_field_3\":\"\"},\"recipient\":{\"last_name\":\"test\",\"middle_name\":\"test\",\"first_name\":\"test2\",\"email\":\"\",\"mobile\":\"1152812333\",\"custom_field_1\":\"\",\"custom_field_2\":\"\",\"custom_field_3\":\"\"},\"sender_sms_notification\":1,\"sender_sms_text\":\"test\",\"recipient_sms_notification\":1,\"recipient_sms_text\":\"Test\",\"custom_field_1\":\"\",\"custom_field_2\":\"\",\"custom_field_3\":\"\"}";
                                                    }
                                                    var TransactionInitial = await _airtimeRepository.GetTransactionInitiateRequest(transationInitiate.Id);
                                                    TransactionInitial.JsonResponse = "DTH Response" + responseString;
                                                    await _airtimeRepository.UpdateTransactionInitiateRequest(TransactionInitial);
                                                    LogTransactionTypes.Response.SaveTransactionLog(LogTransactionNameTypes.PayMoney + subcategory.CategoryName, responseString, "Aggregator Url : ");
                                                    var finalResult = JsonConvert.DeserializeObject<DTHRechargeResponse>(responseString);
                                                    if (finalResult.status != null)
                                                    {
                                                        if (finalResult.status == "0")
                                                        {
                                                            _responseModel.StatusCode = InternationalDTHAggregatorySTATUSCODES.SUCCESSFUL;
                                                            _responseModel.TransactionId = finalResult.transaction_id + "," + finalResult.external_id;
                                                        }
                                                        if (finalResult.status == "1000777")
                                                        {
                                                            _responseModel.StatusCode = InternationalDTHAggregatorySTATUSCODES.Insufficient_balance_in_your_master_account;
                                                            _responseModel.TransactionId = finalResult.transaction_id + "," + finalResult.external_id;
                                                        }
                                                        if (finalResult.status == "1000888")
                                                        {
                                                            _responseModel.StatusCode = InternationalDTHAggregatorySTATUSCODES.Insufficient_balance_in_your_retailer_account;
                                                            _responseModel.TransactionId = finalResult.transaction_id + "," + finalResult.external_id;
                                                        }
                                                        if (finalResult.status == "1000999")
                                                        {
                                                            _responseModel.StatusCode = InternationalDTHAggregatorySTATUSCODES.Invalid_parameter;
                                                            _responseModel.TransactionId = finalResult.transaction_id + "," + finalResult.external_id;
                                                        }
                                                        if (finalResult.status == "1000204")
                                                        {
                                                            _responseModel.StatusCode = InternationalDTHAggregatorySTATUSCODES.Account_number_incorrect;
                                                            _responseModel.TransactionId = finalResult.transaction_id + "," + finalResult.external_id;
                                                        }
                                                        if (finalResult.status == "1000207")
                                                        {
                                                            _responseModel.StatusCode = InternationalDTHAggregatorySTATUSCODES.Transaction_amount_limit_exceeded;
                                                            _responseModel.TransactionId = finalResult.transaction_id + "," + finalResult.external_id;
                                                        }
                                                        if (finalResult.status == "1000212")
                                                        {
                                                            _responseModel.StatusCode = InternationalDTHAggregatorySTATUSCODES.Transaction_already_paid;
                                                            _responseModel.TransactionId = finalResult.transaction_id + "," + finalResult.external_id;
                                                        }
                                                        if (finalResult.status == "1000213")
                                                        {
                                                            _responseModel.StatusCode = InternationalDTHAggregatorySTATUSCODES.Transaction_repeated;
                                                            _responseModel.TransactionId = finalResult.transaction_id + "," + finalResult.external_id;
                                                        }
                                                        if (finalResult.status == "1000214")
                                                        {
                                                            _responseModel.StatusCode = InternationalDTHAggregatorySTATUSCODES.Transaction_rejected;
                                                            _responseModel.TransactionId = finalResult.transaction_id + "," + finalResult.external_id;
                                                        }
                                                        if (finalResult.status == "1000218")
                                                        {
                                                            _responseModel.StatusCode = InternationalDTHAggregatorySTATUSCODES.Transaction_timeout;
                                                            _responseModel.TransactionId = finalResult.transaction_id + "," + finalResult.external_id;
                                                        }
                                                        if (finalResult.status == "1000230")
                                                        {
                                                            _responseModel.StatusCode = InternationalDTHAggregatorySTATUSCODES.Recipient_reached_maximum_transaction_number;
                                                            _responseModel.TransactionId = finalResult.transaction_id + "," + finalResult.external_id;
                                                        }
                                                        if (finalResult.status == "1000301")
                                                        {
                                                            _responseModel.StatusCode = InternationalDTHAggregatorySTATUSCODES.Product_not_available;
                                                            _responseModel.TransactionId = finalResult.transaction_id + "," + finalResult.external_id;
                                                        }
                                                        if (finalResult.status == "1000302")
                                                        {
                                                            _responseModel.StatusCode = InternationalDTHAggregatorySTATUSCODES.Product_not_compatible_with_transaction_type;
                                                            _responseModel.TransactionId = finalResult.transaction_id + "," + finalResult.external_id;
                                                        }
                                                        if (finalResult.status == "1000303")
                                                        {
                                                            _responseModel.StatusCode = InternationalDTHAggregatorySTATUSCODES.Product_type_incorrect;
                                                            _responseModel.TransactionId = finalResult.transaction_id + "," + finalResult.external_id;
                                                        }
                                                        if (finalResult.status == "1000304")
                                                        {
                                                            _responseModel.StatusCode = InternationalDTHAggregatorySTATUSCODES.Account_verification_not_available_for_this_product;
                                                            _responseModel.TransactionId = finalResult.transaction_id + "," + finalResult.external_id;
                                                        }
                                                        if (finalResult.status == "1000990")
                                                        {
                                                            _responseModel.StatusCode = InternationalDTHAggregatorySTATUSCODES.External_id_already_used;
                                                            _responseModel.TransactionId = finalResult.transaction_id + "," + finalResult.external_id;
                                                        }
                                                        if (finalResult.status == "1000401")
                                                        {
                                                            _responseModel.StatusCode = InternationalDTHAggregatorySTATUSCODES.Unauthorized;
                                                            _responseModel.TransactionId = finalResult.transaction_id + "," + finalResult.external_id;
                                                        }
                                                        if (finalResult.status == "1000404")
                                                        {
                                                            _responseModel.StatusCode = InternationalDTHAggregatorySTATUSCODES.Transaction_not_found;
                                                            _responseModel.TransactionId = finalResult.transaction_id + "," + finalResult.external_id;
                                                        }
                                                        if (finalResult.status == "1000500")
                                                        {
                                                            _responseModel.StatusCode = InternationalDTHAggregatorySTATUSCODES.Internal_server_error;
                                                            _responseModel.TransactionId = finalResult.transaction_id + "," + finalResult.external_id;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var finalErrorResult = JsonConvert.DeserializeObject<ErrorResponseForDTH>(responseString);
                                                        foreach (var item in finalErrorResult.errors)
                                                        {
                                                            if (item.code == Convert.ToInt32(InternationalDTHAggregatorySTATUSCODES.Invalid_parameter))
                                                            {
                                                                _responseModel.StatusCode = InternationalDTHAggregatorySTATUSCODES.Invalid_parameter;
                                                                _responseModel.TransactionId = finalResult.transaction_id + "," + finalResult.external_id;
                                                            }
                                                        }
                                                    }
                                                    var errorResponse = "The remote server returned an error: (500) Internal Server Error.";
                                                    if (!string.IsNullOrEmpty(responseString) && responseString != errorResponse)
                                                    {

                                                        if (_responseModel != null && !string.IsNullOrEmpty(_responseModel.StatusCode) &&
                                                            (_responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.SUCCESSFUL
                                                            || _responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.PENDING
                                                            || _responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.FAILED
                                                            || _responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.EXCEPTION
                                                            || _responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Insufficient_balance_in_your_master_account
                                                             || _responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Insufficient_balance_in_your_retailer_account
                                                              || _responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Invalid_parameter
                                                               || _responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Account_number_incorrect
                                                                || _responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Transaction_amount_limit_exceeded
                                                                   || _responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Transaction_already_paid
                                                            || _responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Transaction_repeated
                                                            || _responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Transaction_rejected
                                                             || _responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Transaction_timeout
                                                              || _responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Recipient_reached_maximum_transaction_number
                                                               || _responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Product_not_available
                                                                || _responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Product_not_compatible_with_transaction_type
                                                              || _responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Product_type_incorrect
                                                            || _responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Account_verification_not_available_for_this_product
                                                             || _responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.External_id_already_used
                                                              || _responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Unauthorized
                                                               || _responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Transaction_not_found
                                                                || _responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Internal_server_error
                                                            ))
                                                        {
                                                            var _tranDate = DateTime.UtcNow;
                                                            _responseModel.FormatedTransactionDate = string.Format("{0:d}", DateTime.Now) + "" + string.Format("{0:T}", DateTime.Now);

                                                            _responseModel.AccountNo = request.customer;
                                                            _responseModel.ToMobileNo = request.customer;

                                                            _responseModel.Amount = request.Amount;
                                                            _responseModel.TransactionDate = _tranDate;
                                                            _responseModel.CurrentBalance = sender.CurrentBalance;

                                                            var tran = new WalletTransaction();
                                                            tran.BeneficiaryName = request.BeneficiaryName;
                                                            tran.CreatedDate = _tranDate;
                                                            tran.TransactionInitiateRequestId = transationInitiate.Id;
                                                            tran.UpdatedDate = _tranDate;
                                                            //Self Account 
                                                            tran.ReceiverId = sender.WalletUserId;
                                                            //Sender
                                                            tran.WalletServiceId = WalletService.WalletServiceId;
                                                            tran.TransactionType = AggragatorServiceType.CREDIT;
                                                            tran.TransactionTypeInfo = (int)TransactionTypeInfo.PaidByPayServices;
                                                            tran.VoucherCode = string.Empty;
                                                            tran.SenderId = sender.WalletUserId;
                                                            tran.WalletAmount = request.Amount;
                                                            tran.ServiceTax = "0";
                                                            tran.ServiceTaxRate = 0;
                                                            tran.DisplayContent = request.DisplayContent;
                                                            tran.UpdatedOn = DateTime.UtcNow;
                                                            tran.IsdCode = request.ISD;
                                                            tran.AccountNo = request.customer;// string.Empty;                                                  
                                                            tran.BankTransactionId = string.Empty;
                                                            tran.IsBankTransaction = false;
                                                            tran.BankBranchCode = string.Empty;
                                                            tran.TransactionId = _responseModel.TransactionId;
                                                            response.TransactionId = tran.TransactionId;
                                                            response.TransactionDate = DateTime.UtcNow;
                                                            response.Amount = request.Amount;
                                                            response.Message = "Pay Money successfully.";
                                                            response.AccountNo = request.customer;
                                                            response.CurrentBalance = sender.CurrentBalance;
                                                            int _TransactionStatus = 0;
                                                            if (_responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.SUCCESSFUL)
                                                            {
                                                                _TransactionStatus = (int)TransactionStatus.Completed;


                                                                //-------------sending email after success transaction-----------------
                                                                try
                                                                {
                                                                    string filename = CommonSetting.successfullTransaction;

                                                                    var body = _sendEmails.ReadEmailformats(filename);
                                                                    body = body.Replace("$$FirstName$$", sender.FirstName + " " + sender.LastName);
                                                                    body = body.Replace("$$DisplayContent$$", WalletService.ServiceName);
                                                                    body = body.Replace("$$customer$$", request.customer);
                                                                    body = body.Replace("$$amount$$", "XOF " + request.Amount);
                                                                    body = body.Replace("$$ServiceTaxAmount$$", "XOF " + _commission.CommissionAmount);
                                                                    body = body.Replace("$$AmountWithCommission$$", "XOF " + _commission.AmountWithCommission);
                                                                    body = body.Replace("$$TransactionId$$", _responseModel.TransactionId);

                                                                    var requ = new EmailModel
                                                                    {
                                                                        TO = sender.EmailId,
                                                                        Subject = ResponseEmailMessage.PAYMENT_SUCCESS,
                                                                        Body = body
                                                                    };

                                                                    _sendEmails.SendEmail(requ);
                                                                }
                                                                catch
                                                                {

                                                                }
                                                            }
                                                            else if (_responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.PENDING)
                                                            {
                                                                _TransactionStatus = (int)TransactionStatus.Pending;
                                                            }
                                                            else if (_responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Insufficient_balance_in_your_master_account
                                                             || _responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Insufficient_balance_in_your_retailer_account
                                                              || _responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Invalid_parameter
                                                               || _responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Account_number_incorrect
                                                                || _responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Transaction_amount_limit_exceeded
                                                                   || _responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Transaction_already_paid
                                                            || _responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Transaction_repeated
                                                            || _responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Transaction_rejected
                                                             || _responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Transaction_timeout
                                                              || _responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Recipient_reached_maximum_transaction_number
                                                               || _responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Product_not_available
                                                                || _responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Product_not_compatible_with_transaction_type
                                                              || _responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Product_type_incorrect
                                                            || _responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Account_verification_not_available_for_this_product
                                                             || _responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.External_id_already_used
                                                              || _responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Unauthorized
                                                               || _responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Transaction_not_found
                                                                || _responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Internal_server_error)
                                                            {
                                                                transationInitiate = await _airtimeRepository.GetTransactionInitiateRequest(transationInitiate.Id);

                                                                var refundAmt = Convert.ToDecimal(data.CurrentBalance) + _commission.AmountWithCommission;
                                                                data.CurrentBalance = Convert.ToString(refundAmt);
                                                                transationInitiate.AfterTransactionBalance = data.CurrentBalance;
                                                                transationInitiate.ReceiverCurrentBalance = data.CurrentBalance;
                                                                await _airtimeRepository.UpdateTransactionInitiateRequest(transationInitiate);
                                                                await _walletUserRepository.UpdateUserDetail(data);
                                                                _TransactionStatus = (int)TransactionStatus.Failed;
                                                            }
                                                            else if (_responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.FAILED)
                                                            {
                                                                transationInitiate = await _airtimeRepository.GetTransactionInitiateRequest(transationInitiate.Id);

                                                                var refundAmt = Convert.ToDecimal(data.CurrentBalance) + _commission.AmountWithCommission;
                                                                data.CurrentBalance = Convert.ToString(refundAmt);
                                                                transationInitiate.AfterTransactionBalance = data.CurrentBalance;
                                                                transationInitiate.ReceiverCurrentBalance = data.CurrentBalance;
                                                                await _airtimeRepository.UpdateTransactionInitiateRequest(transationInitiate);
                                                                await _walletUserRepository.UpdateUserDetail(data);
                                                                _TransactionStatus = (int)TransactionStatus.Failed;
                                                            }
                                                            else if (_responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.EXCEPTION)
                                                            {
                                                                transationInitiate = await _airtimeRepository.GetTransactionInitiateRequest(transationInitiate.Id);

                                                                var refundAmt = Convert.ToDecimal(data.CurrentBalance) + _commission.AmountWithCommission;
                                                                data.CurrentBalance = Convert.ToString(refundAmt);
                                                                transationInitiate.AfterTransactionBalance = data.CurrentBalance;
                                                                transationInitiate.ReceiverCurrentBalance = data.CurrentBalance;
                                                                await _airtimeRepository.UpdateTransactionInitiateRequest(transationInitiate);
                                                                await _walletUserRepository.UpdateUserDetail(data);
                                                                _TransactionStatus = (int)TransactionStatus.Failed;
                                                            }
                                                            tran.TransactionStatus = _TransactionStatus;
                                                            tran.IsAdminTransaction = true;
                                                            tran.IsActive = true;
                                                            tran.IsDeleted = false;
                                                            tran.InvoiceNo = invoiceNumber.InvoiceNumber;
                                                            tran.Comments = request.Comment;
                                                            tran.MerchantCommissionId = _commission.MerchantCommissionId;
                                                            tran.MerchantCommissionAmount = Convert.ToString(_commission.MerchantCommissionAmount);
                                                            tran.CommisionId = _commission.CommissionId;
                                                            tran.CommisionAmount = Convert.ToString(_commission.CommissionAmount);

                                                            tran.TotalAmount = Convert.ToString(_commission.AmountWithCommission);

                                                            _responseModel.CurrentBalance = Convert.ToString(_commission.UpdatedCurrentBalance);
                                                            //   db.WalletTransactions.Add(tran);


                                                            if (tran.TransactionStatus == (int)TransactionStatus.Completed || tran.TransactionStatus == (int)TransactionStatus.Pending)
                                                            {
                                                                data.CurrentBalance = Convert.ToString(_commission.UpdatedCurrentBalance);
                                                            }

                                                            if (tran.TransactionStatus == (int)TransactionStatus.Completed)
                                                            {

                                                                #region PushNotification

                                                                var pushModel = new PayMoneyPushModel();
                                                                pushModel.TransactionDate = _tranDate;
                                                                pushModel.TransactionId = tran.WalletTransactionId.ToString();
                                                                pushModel.alert = request.Amount + " XOF has been debited from your account.";
                                                                pushModel.Amount = request.Amount;
                                                                pushModel.CurrentBalance = sender.CurrentBalance;
                                                                pushModel.pushType = (int)PushType.PAYSERVICES;
                                                                pushModel.TransactionTypeInfo = (int)TransactionTypeInfo.PaidByPayServices;
                                                                var push = new PushNotificationModel();
                                                                push.SenderId = sender.WalletUserId;
                                                                push.deviceType = (int)sender.DeviceType;
                                                                push.deviceKey = sender.DeviceToken;
                                                                if ((int)sender.DeviceType == (int)DeviceTypes.ANDROID || (int)sender.DeviceType == (int)DeviceTypes.Web)
                                                                {
                                                                    var aps = new PushPayload<PayMoneyPushModel>();
                                                                    var _data = new PushPayloadData<PayMoneyPushModel>();
                                                                    _data.notification = pushModel;
                                                                    aps.data = _data;
                                                                    aps.to = sender.DeviceToken;
                                                                    aps.collapse_key = string.Empty;
                                                                    push.message = JsonConvert.SerializeObject(aps);
                                                                    push.payload = pushModel;
                                                                }
                                                                if ((int)sender.DeviceType == (int)DeviceTypes.IOS)
                                                                {
                                                                    var aps = new NotificationJsonResponse<PayMoneyPushModel>();
                                                                    aps.aps = pushModel;

                                                                    push.message = JsonConvert.SerializeObject(aps);
                                                                }
                                                                if (!string.IsNullOrEmpty(push.message))
                                                                {
                                                                    _sendPushNotification.sendPushNotification(push);
                                                                }
                                                                #endregion
                                                                response.RstKey = 1;
                                                                response.Message = AggregatoryMESSAGE.SUCCESSFUL;
                                                                response.StatusCode = AggregatorySTATUSCODES.SUCCESSFUL;
                                                            }
                                                            else
                                                            {
                                                                if (_responseModel.StatusCode == AggregatorySTATUSCODES.PENDING)
                                                                {
                                                                    response.RstKey = 2;
                                                                    response.Message = AggregatoryMESSAGE.PENDING;
                                                                    response.StatusCode = AggregatorySTATUSCODES.PENDING;
                                                                }
                                                                else if (_responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Insufficient_balance_in_your_master_account)
                                                                {
                                                                    response.RstKey = 6;
                                                                    response.Message = AggregatoryMESSAGE.destination_number_is_not_avalid;
                                                                    response.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                                }
                                                                else if (_responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Insufficient_balance_in_your_retailer_account)
                                                                {
                                                                    response.RstKey = 6;
                                                                    response.Message = AggregatoryMESSAGE.Insufficient_balance_in_your_retailer_account;
                                                                    response.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                                }
                                                                else if (_responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Invalid_parameter)
                                                                {
                                                                    response.RstKey = 6;
                                                                    response.Message = AggregatoryMESSAGE.Invalid_parameter;
                                                                    response.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                                }
                                                                else if (_responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Account_number_incorrect)
                                                                {
                                                                    response.RstKey = 6;
                                                                    response.Message = AggregatoryMESSAGE.Account_number_incorrect;
                                                                    response.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                                }
                                                                else if (_responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Transaction_amount_limit_exceeded)
                                                                {
                                                                    response.RstKey = 6;
                                                                    response.Message = AggregatoryMESSAGE.Transaction_amount_limit_exceeded;
                                                                    response.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                                }
                                                                else if (_responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Transaction_already_paid)
                                                                {
                                                                    response.RstKey = 6;
                                                                    response.Message = AggregatoryMESSAGE.Transaction_already_paid;
                                                                    response.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                                }
                                                                else if (_responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Transaction_repeated)
                                                                {
                                                                    response.RstKey = 6;
                                                                    response.Message = AggregatoryMESSAGE.Transaction_repeated;
                                                                    response.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                                }
                                                                else if (_responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Transaction_rejected)
                                                                {
                                                                    response.RstKey = 6;
                                                                    response.Message = AggregatoryMESSAGE.Transaction_rejected;
                                                                    response.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                                }
                                                                else if (_responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Transaction_timeout)
                                                                {
                                                                    response.RstKey = 6;
                                                                    response.Message = AggregatoryMESSAGE.Transaction_timeout;
                                                                    response.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                                }
                                                                else if (_responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Recipient_reached_maximum_transaction_number)
                                                                {
                                                                    response.RstKey = 6;
                                                                    response.Message = AggregatoryMESSAGE.Recipient_reached_maximum_transaction_number;
                                                                    response.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                                }
                                                                else if (_responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Product_not_available)
                                                                {
                                                                    response.RstKey = 6;
                                                                    response.Message = AggregatoryMESSAGE.Product_not_available;
                                                                    response.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                                }
                                                                else if (_responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Product_not_compatible_with_transaction_type)
                                                                {
                                                                    response.RstKey = 6;
                                                                    response.Message = AggregatoryMESSAGE.Product_not_compatible_with_transaction_type;
                                                                    response.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                                }
                                                                else if (_responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Product_type_incorrect)
                                                                {
                                                                    response.RstKey = 6;
                                                                    response.Message = AggregatoryMESSAGE.Product_type_incorrect;
                                                                    response.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                                }
                                                                else if (_responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Account_verification_not_available_for_this_product)
                                                                {
                                                                    response.RstKey = 6;
                                                                    response.Message = AggregatoryMESSAGE.Account_verification_not_available_for_this_product;
                                                                    response.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                                }
                                                                else if (_responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.External_id_already_used)
                                                                {
                                                                    response.RstKey = 6;
                                                                    response.Message = AggregatoryMESSAGE.External_id_already_used;
                                                                    response.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                                }
                                                                //--------------------------------------------
                                                                else if (_responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Unauthorized)
                                                                {
                                                                    response.RstKey = 6;
                                                                    response.Message = AggregatoryMESSAGE.Unauthorized;
                                                                    response.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                                }
                                                                else if (_responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Transaction_not_found)
                                                                {
                                                                    response.RstKey = 6;
                                                                    response.Message = AggregatoryMESSAGE.Transaction_not_found;
                                                                    response.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                                }
                                                                else if (_responseModel.StatusCode == InternationalDTHAggregatorySTATUSCODES.Internal_server_error)
                                                                {
                                                                    response.RstKey = 6;
                                                                    response.Message = AggregatoryMESSAGE.Internal_server_error;
                                                                    response.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                                }
                                                                else
                                                                {
                                                                    response.RstKey = 3;
                                                                    response.Message = AggregatoryMESSAGE.FAILED;
                                                                    response.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                                }
                                                            }
                                                            //calling pay method insert data in Database
                                                            tran = await _airtimeRepository.AirtimeServices(tran);
                                                            // await _walletUserRepository.UpdateUserDetail(data);
                                                        }
                                                        else if (_responseModel != null && !string.IsNullOrEmpty(_responseModel.StatusCode) && (_responseModel.StatusCode == AggregatorySTATUSCODES.FAILED))
                                                        {
                                                            response.RstKey = 4;
                                                            response.Message = ResponseMessages.AGGREGATOR_FAILED_ERROR;
                                                        }
                                                        else if (_responseModel != null && !string.IsNullOrEmpty(_responseModel.StatusCode) && (_responseModel.StatusCode == AggregatorySTATUSCODES.EXCEPTION))
                                                        {
                                                            response.RstKey = 5;
                                                            response.Message = ResponseMessages.AGGREGATOR_FAILED_EXCEPTION;
                                                        }
                                                        else
                                                        {
                                                            response.RstKey = 6;
                                                            response.Message = _responseModel.Message;
                                                        }
                                                    }
                                                    else
                                                    {

                                                        response.RstKey = 7;
                                                        response.Message = ResponseMessages.TRANSACTION_ERROR;
                                                    }
                                                }
                                                else
                                                {
                                                    response.RstKey = 10;
                                                    response.Message = ResponseMessages.INSUFICIENT_BALANCE;
                                                }
                                            }
                                            else
                                            {
                                                response.RstKey = 11;
                                                response.Message = ResponseMessages.USER_NOT_REGISTERED;
                                            }
                                        }
                                        else
                                        {
                                            response.RstKey = 12;
                                            response.Message = ResponseMessages.USER_NOT_REGISTERED;
                                        }
                                    }
                                    else
                                    {
                                        response.RstKey = 6;
                                        response.Message = ResponseMessageKyc.TRANSACTION_LIMIT;
                                    }
                                }
                                else if (sender.DocumetStatus == 0 || sender.DocumetStatus == null)
                                {
                                    response.RstKey = 13;
                                    response.Message = ResponseMessageKyc.FAILED_Doc_NotUploaded;
                                }
                                else if (sender.DocumetStatus == 1 || sender.DocumetStatus == null)
                                {
                                    response.RstKey = 14;
                                    response.Message = ResponseMessageKyc.FAILED_Doc_Pending;
                                }
                                else if (sender.DocumetStatus == 4 || sender.DocumetStatus == null)
                                {
                                    response.RstKey = 15;
                                    response.Message = ResponseMessageKyc.Doc_Not_visible;
                                }
                                else
                                {
                                    response.RstKey = 16;
                                    response.Message = ResponseMessageKyc.Doc_Rejected;
                                }
                            }
                            else
                            {
                                response.RstKey = 17;
                                response.Message = ResponseMessageKyc.TRANSACTION_DISABLED;
                            }
                            response.AccountNo = request.customer;
                            response.DocStatus = IsdocVerified;
                            response.DocumetStatus = (int)sender.DocumetStatus;
                            response.CurrentBalance = data.CurrentBalance;
                            response.ToMobileNo = request.customer;
                            response.MobileNo = request.customer;

                            //response.RstKey = 6;
                        }
                        else
                        {
                            response.RstKey = 18;
                            response.Message = ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED;
                        }
                    }
                    else
                    {
                        response.RstKey = 19;
                        response.Message = ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND;
                    }
                }
                else
                {
                    response.Status = (int)WalletTransactionStatus.FAILED;
                    response.Message = ResponseMessages.EMAIL_VERIFICATION_PENDING;
                    response.RstKey = 6;
                    // "Please verify your email id.";
                }


            }
            else
            {
                response.RstKey = 21;
                response.Status = (int)WalletTransactionStatus.FAILED;
                response.Message = ResponseMessages.MobileNotVerify;//
            }
            return response;
        }



    }
}
