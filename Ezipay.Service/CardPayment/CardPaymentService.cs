using _TNS;
using ezeePay.Utility.CommonClass;
using ezeePay.Utility.Enums;
using Ezipay.Database;
using Ezipay.Repository.CardPayment;
using Ezipay.Repository.CommisionRepo;
using Ezipay.Repository.MasterData;
using Ezipay.Repository.UserRepo;
using Ezipay.Service.Admin.TransactionLimitAU;
using Ezipay.Service.AdminService;
using Ezipay.Service.CommonService;
using Ezipay.Service.MasterData;
using Ezipay.Service.MerchantPayment;
using Ezipay.Service.MobileMoneyService;
using Ezipay.Service.UserService;
using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.Utility.LogHandler;
using Ezipay.Utility.SendEmail;
using Ezipay.Utility.SendPush;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.AirtimeFrVm;
using Ezipay.ViewModel.AirtimeViewModel;
using Ezipay.ViewModel.CardPaymentViewModel;
using Ezipay.ViewModel.CommisionViewModel;
using Ezipay.ViewModel.MerchantPaymentViewModel;
using Ezipay.ViewModel.SendEmailViewModel;
using Ezipay.ViewModel.SendPushViewModel;
using Ezipay.ViewModel.ThridPartyApiVIewModel;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

namespace Ezipay.Service.CardPayment
{
    public class CardPaymentService : ICardPaymentService
    {
        private IUserApiService _userApiService;
        private IWalletUserService _walletUserService;
        private IWalletUserRepository _walletUserRepository;
        private ICommonServices _commonServices;
        private ICardPaymentRepository _cardPaymentRepository;
        private IMasterDataRepository _masterDataRepository;
        private ISetCommisionRepository _setCommisionRepository;
        private ISendPushNotification _sendPushNotification;
        private ISendEmails _sendEmails;
        private IMerchantPaymentService _merchantPaymentService;
        private IMobileMoneyServices _mobileMoneyServices;
        private IMasterDataService _masterDataService;
        private ITransactionLimitAUService _transactionLimitAUService;
        private ILogUtils _logUtils;

        public CardPaymentService()
        {
            _userApiService = new UserApiService();
            _walletUserService = new WalletUserService();
            _walletUserRepository = new WalletUserRepository();
            _cardPaymentRepository = new CardPaymentRepository();
            _masterDataRepository = new MasterDataRepository();
            _setCommisionRepository = new SetCommisionRepository();
            _commonServices = new CommonServices();
            _sendEmails = new SendEmails();
            _sendPushNotification = new SendPushNotification();
            _merchantPaymentService = new MerchantPaymentService();
            _mobileMoneyServices = new MobileMoneyServices();
            _masterDataService = new MasterDataService();
            _transactionLimitAUService = new TransactionLimitAUService();
            _logUtils = new LogUtils();
        }





        public async Task<CardAddMoneyResponse> CardPayment(CardAddMoneyRequest request, string sessionToken)
        {
            var response = new CardAddMoneyResponse();
            var _commission = new CalculateCommissionResponse();
            var _commissionRequest = new CalculateCommissionRequest();
            var transationInitiate = new TransactionInitiateRequest();
            //=======

            try
            {
                var UserDetail = await _walletUserService.UserProfile(sessionToken);
                //var Isdocverified = await _walletUserRepository.IsDocVerified(UserDetail.WalletUserId, UserDetail.DocumetStatus);
                var Isdocverified = await _walletUserRepository.IsDocVerifiedMOMO(UserDetail.DocumetStatus);

                var CurrentUser = await _walletUserRepository.GetCurrentUser(UserDetail.WalletUserId);

                //------Get Currency Rate--------------
                var currencyDetail = _masterDataRepository.GetCurrencyRate();

                decimal cediRate = Convert.ToDecimal(currencyDetail.CediRate);
                decimal CfaRate = Convert.ToDecimal(currencyDetail.CfaRate);
                decimal requestAmount = Convert.ToDecimal(request.Amount);// / dollarValue;

                var transactionLimit = await _masterDataRepository.GetTransactionLimitAddMoney(Convert.ToString(CurrentUser.WalletUserId));
                int limit = transactionLimit != null ? Convert.ToInt32(transactionLimit.TransactionLimitForAddMoney) : 0;

                var transactionHistory = _masterDataRepository.GetAllTransactionsAddMoney(CurrentUser.WalletUserId);
                int totalAmountTransfered = transactionHistory != null ? Convert.ToInt32(transactionHistory.totalAmount) : 0;

                decimal currencyConversion = (requestAmount * CfaRate);// / cediRate;

                #region Calculate commission on request amount               
                int WalletServiceId = await _cardPaymentRepository.GetServiceId();

                if (CurrentUser.IsOtpVerified == true) //mobile exist or not then 
                {
                    if (UserDetail.IsEmailVerified == true)
                    {
                        if (Isdocverified == true)
                        {
                            if (transactionLimit == null || transactionLimit.TransactionLimitForAddMoney == null || limit >= (Convert.ToDecimal(request.Amount) + totalAmountTransfered))
                            {
                                if (WalletServiceId > 0)
                                {
                                    #region Calculate Commission on request amount
                                    _commissionRequest.IsRoundOff = true;
                                    _commissionRequest.TransactionAmount = Convert.ToDecimal(request.Amount);
                                    _commissionRequest.WalletServiceId = WalletServiceId;
                                    _commission = await _setCommisionRepository.CalculateCommission(_commissionRequest);
                                    #endregion
                                }
                                #endregion

                                decimal crdAmount = currencyConversion;
                                var res = Convert.ToDecimal(_commission.TransactionAmount * CfaRate);
                                var cardAmount = Decimal.Parse(res.ToString("0.000"));
                                decimal reqAmt = Convert.ToDecimal(_commission.TransactionAmount / CfaRate);
                                //var requestAmo = Decimal.Parse(reqAmt.ToString("0.00"));

                                CardPaymentRequest _cardRequest = new CardPaymentRequest();
                                _cardRequest.WalletUserId = UserDetail.WalletUserId;
                                _cardRequest.TotalAmount = _commission.AmountWithCommission.ToString();
                                _cardRequest.CommissionAmount = Convert.ToString(_commission.CommissionAmount);
                                _cardRequest.Amount = _commission.TransactionAmount.ToString();// Convert.ToString(requestAmo);
                                _cardRequest.FlatCharges = _commission.FlatCharges;
                                _cardRequest.BenchmarkCharges = _commission.BenchmarkCharges;
                                _cardRequest.CommisionCharges = _commission.CommisionPercent;
                                _cardRequest.CreatedDate = DateTime.UtcNow;
                                _cardRequest.UpdatedDate = DateTime.UtcNow;
                                _cardRequest.AmountInCedi = cardAmount.ToString();

                                if (request.IsAddDuringPay && ((request.IsMerchant && request.MerchantContent != null) || (request.PayMoneyContent != null)))
                                {
                                    _cardRequest.IsAddDuringPay = true;
                                }
                                else
                                {
                                    _cardRequest.IsAddDuringPay = false;
                                }

                                _cardRequest = await _cardPaymentRepository.SaveCardPaymentRequest(_cardRequest);
                                //db.CardPaymentRequests.Add(_cardRequest);
                                //db.SaveChanges();
                                if (request.IsAddDuringPay && request.PayMoneyContent != null)
                                {
                                    if (request.IsMerchant)
                                    {
                                        if (request.MerchantContent != null)
                                        {
                                            var a = _cardPaymentRepository.MerchantContent(request.MerchantContent, _cardRequest.TransactionNo, _cardRequest.OrderNo, UserDetail.WalletUserId, (int)TransactionStatus.Pending);
                                        }
                                    }
                                    else
                                    {
                                        if (request.PayMoneyContent != null)
                                        {
                                            var a = _cardPaymentRepository.PayMoneyContent(request.PayMoneyContent, _cardRequest.TransactionNo, _cardRequest.OrderNo, UserDetail.WalletUserId, (int)TransactionStatus.Pending);

                                        }
                                    }
                                }
                                var _requestPayOrder = new CardPaymentUBARequest
                                {
                                    total = _commission.AmountWithCommission.ToString(), //change
                                    referenceNumber = _cardRequest.OrderNo,
                                    customerEmail = UserDetail.EmailId,
                                    customerFirstName = UserDetail.FirstName,
                                    customerLastname = UserDetail.LastName,
                                    description = "CardPayment",
                                    customerPhoneNumber = UserDetail.MobileNo
                                };
                                var _transactionId = await CardPaymentUBA(_requestPayOrder);
                                var id = JsonConvert.DeserializeObject<UBACardPaymentResponse>(_transactionId);

                                if (id != null)
                                {
                                    var redirectUrl = "https://ucollect.ubagroup.com/cipg-payportal/paytran?id=" + id.registration.transaction.id;
                                    response.Url = redirectUrl;
                                    string PayTranId = id.registration.transaction.id;

                                    //save selected card no. with paytranid like CTNT only requested id savfe not response
                                    await _cardPaymentRepository.SaveCardNo(_cardRequest.OrderNo, UserDetail.WalletUserId, request.CardNo, PayTranId, request.Amount, _commission.AmountWithCommission.ToString(), UserDetail.EmailId);

                                }
                                else
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.Message = "Error from UBA bank.";
                                }

                                transationInitiate.InvoiceNumber = _cardRequest.OrderNo;
                                transationInitiate.ReceiverNumber = UserDetail.MobileNo;
                                transationInitiate.ServiceName = "Card Payment";
                                transationInitiate.RequestedAmount = _commission.TransactionAmount.ToString();
                                transationInitiate.TransactionStatus = (int)TransactionStatus.Pending;
                                transationInitiate.WalletUserId = UserDetail.WalletUserId;
                                transationInitiate.UserReferanceNumber = _cardRequest.TransactionNo;
                                transationInitiate.CurrentBalance = UserDetail.CurrentBalance;
                                transationInitiate.AfterTransactionBalance = "";
                                transationInitiate.UserName = UserDetail.FirstName + " " + UserDetail.LastName;
                                transationInitiate.CreatedDate = DateTime.UtcNow;
                                transationInitiate.UpdatedDate = DateTime.UtcNow;
                                transationInitiate.IsActive = true;
                                transationInitiate.IsDeleted = false;
                                transationInitiate.JsonRequest = response.Url;
                                transationInitiate.JsonResponse = "";
                                transationInitiate = await _cardPaymentRepository.SaveTransactionInitiateRequest(transationInitiate);
                                //decimal amt = (_commission.AmountWithCommission * CfaRate);
                                //var finalAmt = Decimal.Parse(amt.ToString("0.00"));
                                //var _RequestPayOrder = new CardPaymentWebRequest();
                                //_RequestPayOrder.vpc_OrderInfo = _cardRequest.OrderNo;
                                //_RequestPayOrder.vpc_MerchTxnRef = _cardRequest.TransactionNo;                           
                                //_RequestPayOrder.vpc_Amount = Convert.ToString(finalAmt);
                                //response.Url = testGTPay(_RequestPayOrder);

                                response.Amount = _commission.TransactionAmount.ToString();
                                response.StatusCode = UserDetail.DocumetStatus;
                                response.UniqueId = _cardRequest.OrderNo;

                                LogTransactionTypes.Request.SaveTransactionLog(LogTransactionNameTypes.AddMoney + "Credit TO Debit Cards", response.Url, "Request Url : " + response.Url, CurrentUser.WalletUserId);

                                response.RstKey = 1;
                            }
                            else
                            {
                                //response.RstKey = 6;
                                //response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                //response.Message = "You can not add more then " + transactionLimit.TransactionLimitForAddMoney + " cedi";
                                var addLimit = limit - (Convert.ToDecimal(request.Amount) + totalAmountTransfered);
                                if (addLimit < Convert.ToDecimal(request.Amount))
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.Message = "Exceed your transaction limit.";
                                }
                                else
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.Message = "You can not add more then " + transactionLimit.TransactionLimitForAddMoney.ToString() + " XOF in a day";
                                }
                            }
                        }
                        else if (UserDetail.DocumetStatus == 0 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
                        {
                            response.RstKey = 13;
                            response.Message = ResponseMessageKyc.FAILED_Doc_NotUploaded;
                        }
                        else if (UserDetail.DocumetStatus == 1 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
                        {
                            response.RstKey = 14;
                            response.Message = ResponseMessageKyc.FAILED_Doc_Pending;
                        }
                        else if (UserDetail.DocumetStatus == 4 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
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
                        response.RstKey = 6;
                        response.StatusCode = (int)TransactionStatus.Failed;
                        response.Message = ResponseMessages.EMAIL_VERIFICATION_PENDING;
                    }
                }
                else
                {
                    response.RstKey = 21;
                    response.StatusCode = (int)TransactionStatus.Failed;
                    response.Message = ResponseMessages.MobileNotVerify;//
                }
                response.IsEmailVerified = UserDetail.IsEmailVerified;
                response.DocStatus = Isdocverified;
            }

            catch (Exception ex)
            {
                response.RstKey = 6;
                response.StatusCode = (int)TransactionStatus.Failed;
                //tran.Rollback();
            }
            return response;

        }

        private string testGTPay(CardPaymentWebRequest request)
        {
            string acquirerUrl = request.virtualPaymentClientURL;// "https://migs.mastercard.com.au/vpcpay";
            string redirectUrl = "";


            try
            {
                VPCRequest conns = new VPCRequest(acquirerUrl);
                conns.SetSecureSecret(request.vpc_SecureHash);
                // Add the Digital Order Fields for the functionality you wish to use
                // Core Transaction Fields
                conns.AddDigitalOrderField("vpc_Version", request.vpc_Version);
                conns.AddDigitalOrderField("vpc_Command", request.vpc_Command);
                conns.AddDigitalOrderField("vpc_Merchant", request.vpc_Merchant);
                conns.AddDigitalOrderField("vpc_AccessCode", request.vpc_AccessCode);
                conns.AddDigitalOrderField("vpc_MerchTxnRef", request.vpc_MerchTxnRef);

                // conns.AddDigitalOrderField("vpc_MerchTxnRef", "1111111");

                //vpc_MerchTxnRef
                conns.AddDigitalOrderField("vpc_OrderInfo", request.vpc_OrderInfo);
                //conns.AddDigitalOrderField("vpc_Amount", Convert.ToString(Decimal.Parse(request.vpc_Amount)).IgnoreDecimal());
                conns.AddDigitalOrderField("vpc_Amount", Convert.ToString(Decimal.Parse(request.vpc_Amount) * 100).IgnoreDecimal());
                // conns.AddDigitalOrderField("vpc_Amount", Convert.ToString(Decimal.Parse(request.vpc_Amount)).IgnoreDecimal());
                conns.AddDigitalOrderField("vpc_Currency", request.vpc_Currency);
                conns.AddDigitalOrderField("vpc_Card", "");
                conns.AddDigitalOrderField("vpc_CardNum", "");
                conns.AddDigitalOrderField("vpc_CardExp", "");
                conns.AddDigitalOrderField("vpc_Gateway", request.vpc_Gateway);
                conns.AddDigitalOrderField("vpc_ReturnURL", request.vpc_ReturnURL);
                conns.AddDigitalOrderField("vpc_CardSecurityCode", "");
                // Ticket Number
                conns.AddDigitalOrderField("vpc_TicketNo", request.vpc_TicketNo);
                redirectUrl = conns.Create3PartyQueryString();



            }
            catch (Exception ex)
            {

            }
            return redirectUrl;
        }

        public async Task<string> CardPaymentUBA(CardPaymentUBARequest request)
        {
            string responseString = "";
            var dss = DateTime.Now.AddMinutes(55).ToString("dd'/'MM'/'yyyy HH:mm:ss");
            var req = new CardPaymentUBARequest
            {
                countryCurrencyCode = "952",
                customerEmail = request.customerEmail,
                customerFirstName = request.customerFirstName,
                customerLastname = request.customerLastname,
                customerPhoneNumber = request.customerPhoneNumber,
                description = request.description,
                merchantId = "CMCDI10552",
                noOfItems = "1",
                referenceNumber = request.referenceNumber,
                serviceKey = "f5e5aa5b-3e71-479c-80cc-3c8f3b03c13d",
                total = request.total,
                date = dss
            };
            try
            {
                var url = "https://ucollect.ubagroup.com/cipg-payportal/regjtran";
                var jsonReq = JsonConvert.SerializeObject(req);
                //var payData = Task.Run(() => Card(jsonReq, url));
                var payData = await Card(jsonReq, url);
                responseString = payData.ToString();
            }
            catch (Exception ex)
            {

            }

            //payData.Wait();
            // responseString = payData.Result.ToString();


            return responseString;
        }
        //public async Task<string> Card(string req, string url)
        //{
        //    string resString = "";
        //    string resBody = "";
        //    using (HttpClient client = new HttpClient())
        //    {
        //        // Call asynchronous network methods in a try/catch block to handle exceptions
        //        try
        //        {
        //            var content = new StringContent(req, Encoding.UTF8, "application/json");

        //            HttpResponseMessage response = await client.PostAsync(url, content);
        //            response.EnsureSuccessStatusCode();
        //            resBody = await response.Content.ReadAsStringAsync();
        //            Console.WriteLine(resBody);
        //        }
        //        catch (HttpRequestException e)
        //        {
        //            Console.WriteLine("\nException Caught!");
        //            Console.WriteLine("Message :{0} ", e.Message);
        //        }
        //        return resBody;
        //    }
        //}

        public async Task<string> Card(string req, string url)
        {

            string resBody = "";
            using (HttpClient client = new HttpClient())
            {
                // Call asynchronous network methods in a try/catch block to handle exceptions
                try
                {
                    var content = new StringContent(req, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(url, content);
                    response.EnsureSuccessStatusCode();
                    resBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(resBody);
                }
                catch (HttpRequestException e)
                {
                    "EziWebHookController".ErrorLog("EziWebHookController.cs", "webhookFlutterError", url + " " + e.StackTrace + " " + e.Message);
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
                return resBody;
            }
        }

        public async Task<AddMoneyAggregatorResponse> MobileServicesAggregator(AddMoneyAggregatoryRequest Request, string sessionToken, long WalletUserId = 0)
        {
            var response = new AddMoneyAggregatorResponse();
            var _commission = new CalculateCommissionResponse();
            var _commissionRequest = new CalculateCommissionRequest();
            var transationInitiate = new TransactionInitiateRequest();
            var sender = await _walletUserService.UserProfile(sessionToken);
            //var Isdocverified = await _walletUserRepository.IsDocVerified(sender.WalletUserId, sender.DocumetStatus);
            var Isdocverified = await _walletUserRepository.IsDocVerifiedMOMO(sender.DocumetStatus);
            var results = new AddMoneyAggregatorResponse();
            var CurrentUser = await _walletUserRepository.GetCurrentUser(sender.WalletUserId);
            var transactionLimit = await _masterDataRepository.GetTransactionLimitAddMoney(Convert.ToString(CurrentUser.WalletUserId));
            int limit = transactionLimit != null ? Convert.ToInt32(transactionLimit.TransactionLimitForAddMoney) : 0;
            var transactionHistory = _masterDataRepository.GetAllTransactionsAddMoney(CurrentUser.WalletUserId);
            int totalAmountTransfered = transactionHistory != null ? Convert.ToInt32(transactionHistory.totalAmount) : 0;
            string customer = Request.MobileNo;//.Length.ToString();           
            string responseString = string.Empty;

            //if (customer.Length != 0)
            //{
            //    int idx = customer.IndexOf("0");
            //    if (idx < 1)
            //    {
            //        customer = "0" + Request.MobileNo;
            //    }
            //    else
            //    {
            //        customer = Request.MobileNo;
            //    }
            //}
            //else
            //{
            //    customer = Request.MobileNo;
            //}

            customer = Request.MobileNo;
            try
            {
                if (CurrentUser.IsOtpVerified == true) //mobile exist or not then 
                {
                    if (sender.IsEmailVerified == true)
                    {
                        var WalletService = await _cardPaymentRepository.GetWalletService(Request.channel, 3);

                        if (WalletService != null)
                        {
                            var adminKeyPair = AES256.AdminKeyPair;
                            if (Isdocverified == true)
                            {
                                if (transactionLimit == null || limit >= (Convert.ToDecimal(Request.Amount) + totalAmountTransfered))
                                {
                                    if (sender != null && !string.IsNullOrEmpty(sender.MobileNo))
                                    {
                                        string MobileNo = AES256.Encrypt(adminKeyPair.PublicKey, sender.MobileNo);
                                        var data = await _walletUserRepository.GetCurrentUser(sender.WalletUserId);
                                        if (data != null)
                                        {
                                            #region Calculate Commission on request amount

                                            _commissionRequest.CurrentBalance = Convert.ToDecimal(data.CurrentBalance);
                                            _commissionRequest.IsRoundOff = true;
                                            _commissionRequest.TransactionAmount = Convert.ToDecimal(Request.Amount);
                                            _commissionRequest.WalletServiceId = WalletService.WalletServiceId;
                                            _commission = await _setCommisionRepository.CalculateAddMoneyCommission(_commissionRequest);
                                            #endregion
                                            #region If Requested Pay Money
                                            int AddDuringPayRecordId = 0;
                                            if (Request.IsAddDuringPay)
                                            {
                                                if (Request.IsMerchant)
                                                {
                                                    if (Request.MerchantContent != null)
                                                    {

                                                        AddDuringPayRecordId = await MerchantContent(Request.MerchantContent, "0", (long)data.WalletUserId, (int)TransactionStatus.Pending);
                                                    }
                                                }
                                                else
                                                {
                                                    if (Request.PayMoneyContent != null)
                                                    {
                                                        AddDuringPayRecordId = await PayMoneyContent(Request.PayMoneyContent, "0", (long)data.WalletUserId, (int)TransactionStatus.Pending);

                                                    }
                                                }
                                            }
                                            #endregion

                                            #region Prepare the Model for Request
                                            AddMobileMoneyAggregatoryRequest _MobileMoneyRequest = new AddMobileMoneyAggregatoryRequest();

                                            _MobileMoneyRequest.ServiceType = AggragatorServiceType.DEBIT;
                                            _MobileMoneyRequest.Channel = Request.channel;
                                            _MobileMoneyRequest.Amount = Convert.ToString(_commission.AmountWithCommission); //Request.amount;
                                            var invoiceNumber = await _masterDataRepository.GetInvoiceNumber();

                                            if (invoiceNumber != null && Request.IsdCode == "+237")
                                            {
                                                _MobileMoneyRequest.InvoiceNo = invoiceNumber.InvoiceNumber;
                                            }
                                            else if (invoiceNumber != null)
                                            {
                                                _MobileMoneyRequest.TransactionId = invoiceNumber.InvoiceNumber;
                                            }


                                            _MobileMoneyRequest.Customer = customer;
                                            if (Request.IsdCode == "+225")
                                            {
                                                _MobileMoneyRequest.Country = "CI";
                                                _MobileMoneyRequest.Customer = customer;
                                            }
                                            else if (Request.IsdCode == "+221")
                                            {
                                                _MobileMoneyRequest.Country = "SN";
                                                _MobileMoneyRequest.Customer = customer;
                                            }
                                            else if (Request.IsdCode == "+226")
                                            {
                                                _MobileMoneyRequest.Country = "BF";
                                                _MobileMoneyRequest.Customer = customer;
                                            }
                                            else if (Request.IsdCode == "+223")
                                            {
                                                _MobileMoneyRequest.Country = "ML";
                                                _MobileMoneyRequest.Customer = customer;
                                            }
                                            else if (Request.IsdCode == "+229")
                                            {
                                                _MobileMoneyRequest.Country = "BJ";
                                                _MobileMoneyRequest.Customer = "229" + customer;
                                            }
                                            else if (Request.IsdCode == "+237")
                                            {
                                                _MobileMoneyRequest.Country = "CM";
                                                _MobileMoneyRequest.Customer = customer;

                                            }
                                            string apiUrl = string.Empty;

                                            if (Request.IsdCode == "+237")//camroon
                                            {
                                                _MobileMoneyRequest.servicecategory = "MOBILEMONEY";
                                                apiUrl = ThirdPartyAggragatorSettings.AddMobileMoneyCameroon;
                                                var requ = new PayServicesMoneyAggregatoryRequestCamroon
                                                {
                                                    ApiKey = ThirdPartyAggragatorSettings.ApiKeyCamroon,
                                                    Amount = _commission.TransactionAmount.ToString(),
                                                    Customer = _MobileMoneyRequest.Customer,
                                                    InvoiceNo = _MobileMoneyRequest.InvoiceNo
                                                };
                                                _MobileMoneyRequest.ApiKey = ThirdPartyAggragatorSettings.ApiKey;
                                                _MobileMoneyRequest.Signature = new CommonMethods().Sha256HashCamroon(requ);
                                                //_MobileMoneyRequest.Signature = Sha256Hash(requ);
                                            }
                                            else
                                            {
                                                _MobileMoneyRequest.servicecategory = "francophone";

                                                apiUrl = ThirdPartyAggragatorSettings.AddMobileMoney;
                                                var requ = new PayServicesMoneyAggregatoryRequest
                                                {
                                                    ApiKey = ThirdPartyAggragatorSettings.ApiKey,
                                                    Amount = _MobileMoneyRequest.Amount,
                                                    Customer = _MobileMoneyRequest.Customer,
                                                    TransactionId = _MobileMoneyRequest.TransactionId
                                                };
                                                _MobileMoneyRequest.ApiKey = ThirdPartyAggragatorSettings.ApiKey;
                                                _MobileMoneyRequest.Signature = Sha256Hash(requ);
                                            }
                                            //  SHA2Hash
                                            string RequestString = JsonConvert.SerializeObject(_MobileMoneyRequest);
                                            #endregion

                                            transationInitiate.InvoiceNumber = invoiceNumber.InvoiceNumber;
                                            transationInitiate.ReceiverNumber = customer;
                                            transationInitiate.ServiceName = WalletService.ServiceName;
                                            transationInitiate.RequestedAmount = _commission.TransactionAmount.ToString();
                                            transationInitiate.TransactionStatus = (int)TransactionStatus.Pending;
                                            transationInitiate.WalletUserId = sender.WalletUserId;
                                            transationInitiate.UserReferanceNumber = invoiceNumber.AutoDigit;
                                            transationInitiate.CurrentBalance = sender.CurrentBalance;
                                            transationInitiate.AfterTransactionBalance = sender.CurrentBalance;
                                            transationInitiate.ReceiverCurrentBalance = sender.CurrentBalance;
                                            transationInitiate.ReceiverWalletUserId = sender.WalletUserId;
                                            transationInitiate.UserName = sender.FirstName + " " + sender.LastName;
                                            transationInitiate.CreatedDate = DateTime.UtcNow;
                                            transationInitiate.UpdatedDate = DateTime.UtcNow;
                                            transationInitiate.IsActive = true;
                                            transationInitiate.IsDeleted = false;
                                            transationInitiate.JsonRequest = RequestString;
                                            transationInitiate.JsonResponse = "";
                                            transationInitiate = await _cardPaymentRepository.SaveTransactionInitiateRequest(transationInitiate);



                                            if (WalletService.HttpVerbs.ToLower() == AggragatorServiceVerbs.HttpPostVerb.ToLower())
                                            {
                                                var jsonReq = JsonConvert.SerializeObject(_MobileMoneyRequest);
                                                var responseData = await new CommonApi().PaymentMobileMon(jsonReq, apiUrl);

                                                responseString = responseData;
                                            }
                                            else
                                            {
                                                //jsdfklasjdf;
                                                var responseData = Task.Run(() => HttpGetUrlEncodedServiceForMobileMoney(LogTransactionNameTypes.AddMoney, apiUrl, _MobileMoneyRequest, Request, Request.channel));
                                                responseData.Wait();
                                                responseString = responseData.Result.ToString();
                                            }


                                            //responseString = await HttpGetUrlEncodedServiceForMobileMoney(LogTransactionNameTypes.AddMoney, apiUrl, _MobileMoneyRequest, Request, Request.channel);

                                            LogTransactionTypes.Response.SaveTransactionLog(LogTransactionNameTypes.AddMoney + "Mobile money", responseString, "Aggregator Url : ", sender.WalletUserId);
                                            //below three line code for updateing response from third party
                                            var TransactionInitial = await _cardPaymentRepository.GetTransactionInitiateRequest(transationInitiate.Id);
                                            TransactionInitial.JsonResponse = "mobile money Response" + responseString;
                                            TransactionInitial = await _cardPaymentRepository.UpdateTransactionInitiateRequest(TransactionInitial);

                                            if (!string.IsNullOrEmpty(responseString))
                                            {
                                                var _responseModel = JsonConvert.DeserializeObject<AddMoneyAggregatorResponse>(responseString);
                                                if (Request.IsdCode == "+237")//camroon
                                                {

                                                    _responseModel.StatusCode = Convert.ToString(_responseModel.statusCode);
                                                    _responseModel.TransactionId = _responseModel.transactionId;
                                                }
                                                if (WalletService.WalletServiceId == 143)
                                                {
                                                    var msg = _responseModel.Message.Split('|');
                                                    _responseModel.Message = msg[0];
                                                    response.OrangeUrl = msg[1];
                                                }

                                                if (_responseModel != null && !string.IsNullOrEmpty(_responseModel.StatusCode) && (_responseModel.StatusCode == AggregatorySTATUSCODES.SUCCESSFUL || _responseModel.StatusCode == AggregatorySTATUSCODES.PENDING || _responseModel.StatusCode == AggregatorySTATUSCODES.FAILED || _responseModel.StatusCode == AggregatorySTATUSCODES.EXCEPTION))
                                                {

                                                    var _tranDate = DateTime.UtcNow;
                                                    _responseModel.FormatedTransactionDate = string.Format("{0:d}", DateTime.Now) + "" + string.Format("{0:T}", DateTime.Now);
                                                    _responseModel.MobileNo = MobileNo;
                                                    _responseModel.Amount = Request.Amount;
                                                    _responseModel.TransactionDate = _tranDate;

                                                    var tran = new WalletTransaction();
                                                    tran.WalletAmount = Request.Amount;
                                                    tran.TransactionInitiateRequestId = transationInitiate.Id;
                                                    tran.CommisionId = _commission.CommissionId;
                                                    tran.CommisionAmount = Convert.ToString(_commission.CommissionAmount);// "0";
                                                    tran.ServiceTax = Convert.ToString(_commission.ServiceTaxAmount);
                                                    tran.ServiceTaxRate = _commission.ServiceTaxRate;
                                                    tran.IsdCode = Request.IsdCode;
                                                    string AccountNo = customer;
                                                    string VoucherCode = string.Empty;
                                                    if (AccountNo != "VODAFONE" && AccountNo.Contains(","))
                                                    {
                                                        var rename = AccountNo.Split(',');
                                                        if (rename != null && rename.Length > 1)
                                                        {
                                                            tran.AccountNo = rename[0];
                                                            _responseModel.MobileNo = rename[0];
                                                            VoucherCode = rename[1];
                                                        }
                                                        else
                                                        {
                                                            tran.AccountNo = customer;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        _responseModel.MobileNo = customer;
                                                        tran.AccountNo = customer;
                                                    }
                                                    tran.VoucherCode = VoucherCode;
                                                    tran.BankTransactionId = string.Empty;
                                                    tran.MerchantCommissionAmount = Convert.ToString(_commission.MerchantCommissionAmount);
                                                    tran.MerchantCommissionId = _commission.MerchantCommissionId;
                                                    tran.AccountNo = AccountNo;
                                                    tran.IsBankTransaction = false;
                                                    tran.BankBranchCode = string.Empty;
                                                    tran.CreatedDate = _tranDate;
                                                    tran.UpdatedDate = _tranDate;
                                                    tran.ReceiverId = data.WalletUserId;
                                                    tran.WalletServiceId = WalletService.WalletServiceId;
                                                    tran.TransactionType = AggragatorServiceType.DEBIT;
                                                    tran.TransactionTypeInfo = (int)TransactionTypeInfo.AddedByMobileMoney;
                                                    //Self Account
                                                    tran.OperatorType = "sample";
                                                    tran.SenderId = data.WalletUserId;
                                                    tran.TotalAmount = Convert.ToString(_commission.AmountWithCommission);// Request.amount;
                                                    tran.TransactionId = _responseModel.TransactionId;
                                                    int _TransactionStatus = 0;
                                                    if (_responseModel.StatusCode == AggregatorySTATUSCODES.SUCCESSFUL)
                                                    {
                                                        _TransactionStatus = (int)TransactionStatus.Completed;
                                                        try
                                                        {
                                                            //--------send mail on success transaction--------
                                                            var senderdata = await _walletUserRepository.GetUserDetailById(sender.WalletUserId);

                                                            string filename = AppSetting.successfullTransaction;
                                                            var body = _sendEmails.ReadEmailformats(filename);
                                                            body = body.Replace("$$FirstName$$", senderdata.FirstName + " " + senderdata.LastName);
                                                            body = body.Replace("$$DisplayContent$$", Request.channel);
                                                            body = body.Replace("$$customer$$", Request.customer);
                                                            body = body.Replace("$$amount$$", "XOF " + Request.Amount);
                                                            body = body.Replace("$$ServiceTaxAmount$$", "XOF " + _commission.CommissionAmount);
                                                            body = body.Replace("$$AmountWithCommission$$", "XOF " + _commission.AmountWithCommission);
                                                            body = body.Replace("$$TransactionId$$", _responseModel.TransactionId);
                                                            var req = new EmailModel
                                                            {
                                                                TO = senderdata.EmailId,
                                                                Subject = "Transaction Successfull",
                                                                Body = body
                                                            };
                                                            _sendEmails.SendEmail(req);
                                                        }
                                                        catch
                                                        {

                                                        }
                                                    }
                                                    else if (_responseModel.StatusCode == AggregatorySTATUSCODES.PENDING)
                                                    {
                                                        _TransactionStatus = (int)TransactionStatus.Pending;
                                                    }
                                                    else if (_responseModel.StatusCode == AggregatorySTATUSCODES.FAILED)
                                                    {
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
                                                    tran.Comments = string.Empty;
                                                    tran.IsAddDuringPay = Request.IsAddDuringPay;
                                                    tran.FlatCharges = _commission.FlatCharges.ToString();
                                                    tran.BenchmarkCharges = _commission.BenchmarkCharges.ToString();
                                                    tran.CommisionPercent = _commission.CommisionPercent.ToString();
                                                    //db.WalletTransactions.Add(tran);
                                                    //db.SaveChanges();
                                                    tran = await _cardPaymentRepository.MobileMoneyForAddServices(tran);

                                                    if (tran.TransactionStatus == (int)TransactionStatus.Completed)
                                                    {

                                                        #region Update Current Balance
                                                        if (Convert.ToDecimal(data.CurrentBalance) >= 0)
                                                        {
                                                            if (Convert.ToDecimal(data.CurrentBalance) == 0)
                                                            {
                                                                data.CurrentBalance = Math.Round(_commission.TransactionAmount, 2).ToString();
                                                            }
                                                            else
                                                            {
                                                                data.CurrentBalance = Math.Round(Convert.ToDecimal(data.CurrentBalance) + _commission.TransactionAmount, 2).ToString();
                                                            }
                                                        }
                                                        else
                                                        {
                                                            data.CurrentBalance = Math.Round(_commission.TransactionAmount - Convert.ToDecimal(data.CurrentBalance), 2).ToString();
                                                        }
                                                        #endregion

                                                        //db.SaveChanges();
                                                        await _walletUserRepository.UpdateUserDetail(data);
                                                        #region PushNotification

                                                        var pushModel = new PayMoneyPushModel();
                                                        pushModel.TransactionDate = _tranDate;
                                                        pushModel.TransactionId = tran.WalletTransactionId.ToString();
                                                        pushModel.alert = Request.Amount + " XOF has been credited to your account.";
                                                        pushModel.Amount = Request.Amount;
                                                        pushModel.CurrentBalance = data.CurrentBalance;
                                                        pushModel.pushType = (int)PushType.ADDMONEY;
                                                        pushModel.TransactionTypeInfo = (int)TransactionTypeInfo.AddedByMobileMoney;

                                                        var push = new PushNotificationModel();
                                                        push.SenderId = (long)data.WalletUserId;
                                                        push.deviceType = (int)data.DeviceType;
                                                        push.deviceKey = data.DeviceToken;
                                                        if ((int)data.DeviceType == (int)DeviceTypes.ANDROID || (int)data.DeviceType == (int)DeviceTypes.Web)
                                                        {
                                                            var aps = new PushPayload<PayMoneyPushModel>();
                                                            var _data = new PushPayloadData<PayMoneyPushModel>();
                                                            _data.notification = pushModel;
                                                            aps.data = _data;
                                                            aps.to = data.DeviceToken;
                                                            aps.collapse_key = string.Empty;
                                                            push.message = JsonConvert.SerializeObject(aps);
                                                            push.payload = pushModel;
                                                        }
                                                        if ((int)data.DeviceType == (int)DeviceTypes.IOS)
                                                        {
                                                            var aps = new NotificationJsonResponse<PayMoneyIOSPushModel>();
                                                            var _iosPushModel = new PayMoneyIOSPushModel();
                                                            _iosPushModel.alert = pushModel.alert;
                                                            _iosPushModel.Amount = pushModel.Amount;
                                                            _iosPushModel.CurrentBalance = pushModel.CurrentBalance;
                                                            _iosPushModel.MobileNo = pushModel.MobileNo;
                                                            _iosPushModel.SenderName = pushModel.SenderName;
                                                            _iosPushModel.pushType = pushModel.pushType;
                                                            aps.aps = _iosPushModel;

                                                            push.message = JsonConvert.SerializeObject(aps);
                                                        }
                                                        if (!string.IsNullOrEmpty(push.message))
                                                        {
                                                            _sendPushNotification.sendPushNotification(push);
                                                        }
                                                        #endregion

                                                        #region IF Requested Paymoney

                                                        if (Request.IsAddDuringPay && AddDuringPayRecordId > 0)
                                                        {
                                                            var storeddata = await _cardPaymentRepository.GetAddDuringPayRecord(AddDuringPayRecordId, (int)TransactionStatus.Pending);
                                                            if (storeddata != null)
                                                            {
                                                                var _record = new PayMoneyAggregatoryRequest();
                                                                _record.Amount = storeddata.amount;
                                                                _record.channel = storeddata.channel;
                                                                _record.chennelId = (int)storeddata.chennelId;
                                                                _record.Comment = storeddata.Comment;
                                                                _record.customer = storeddata.customer;
                                                                _record.invoiceNo = storeddata.invoiceNo;
                                                                _record.IsAddDuringPay = (bool)storeddata.IsAddDuringPay;
                                                                _record.ISD = storeddata.ISD;
                                                                _record.serviceCategory = storeddata.serviceCategory;
                                                                _record.ServiceCategoryId = (int)storeddata.ServiceCategoryId;
                                                                _record.IsMerchant = (bool)storeddata.IsMerchant;
                                                                _record.MerchantId = (long)storeddata.MerchantId;

                                                                if (_record != null)
                                                                {
                                                                    if (_record.IsAddDuringPay && _record.IsMerchant && _record.MerchantId > 0)
                                                                    {
                                                                        //WalletTransactionRepository obj = new WalletTransactionRepository();
                                                                        var merchantRequest = new MerchantTransactionRequest();
                                                                        merchantRequest.Amount = _record.Amount;
                                                                        merchantRequest.Comment = _record.Comment;
                                                                        merchantRequest.MerchantId = _record.MerchantId;
                                                                        var merchantResponse = await _merchantPaymentService.MerchantPayment(merchantRequest, sessionToken); ////

                                                                        if (merchantResponse.StatusCode == (int)TransactionStatus.Completed)
                                                                        {
                                                                            storeddata.TransactionStatus = (int)TransactionStatus.Completed;
                                                                            //db.SaveChanges();
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        var payResponse = await _mobileMoneyServices.MobileMoneyService(_record);
                                                                        if (payResponse.RstKey == 1)
                                                                        {
                                                                            storeddata.TransactionStatus = (int)TransactionStatus.Completed;
                                                                            // db.SaveChanges();
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {

                                                        }
                                                        #endregion
                                                    }
                                                    else
                                                    {
                                                        //var storeddata = db.AddDuringPayRecords.Where(x => x.AddDuringPayRecordId == AddDuringPayRecordId && x.TransactionStatus == (int)TransactionStatus.Pending).FirstOrDefault();
                                                        var storeddata = await _cardPaymentRepository.GetAddDuringPayRecord(AddDuringPayRecordId, (int)TransactionStatus.Pending);
                                                        if (storeddata != null)
                                                        {
                                                            storeddata.TransactionNo = tran.TransactionId;
                                                            //db.SaveChanges();
                                                            await _cardPaymentRepository.UpdateAddDuringPayRecord(storeddata);
                                                        }
                                                    }

                                                    if (_responseModel.StatusCode == AggregatorySTATUSCODES.SUCCESSFUL)
                                                    {
                                                        //  Response.Create(true, AggregatoryMESSAGE.SUCCESSFUL, HttpStatusCode.OK, _responseModel);
                                                        response.Status = (int)HttpStatusCode.OK;
                                                        response.Message = AggregatoryMESSAGE.SUCCESSFUL;
                                                        response.RstKey = 1;
                                                    }
                                                    else
                                                    {
                                                        if (_responseModel.StatusCode == AggregatorySTATUSCODES.PENDING)
                                                        {

                                                            if (WalletService.WalletServiceId == 143)
                                                            {
                                                                response.Message = AggregatoryMESSAGE.SUCCESSFUL;
                                                                response.RstKey = 1;
                                                            }
                                                            else
                                                            {
                                                                response.Message = AggregatoryMESSAGE.PENDING;
                                                                response.RstKey = 2;
                                                            }

                                                            // Response.Create(false, AggregatoryMESSAGE.PENDING, _responseModel.StatusCode, _responseModel);
                                                        }
                                                        else
                                                        {
                                                            response.Message = AggregatoryMESSAGE.FAILED;
                                                            response.RstKey = 3;
                                                            //Response.Create(false, AggregatoryMESSAGE.FAILED, _responseModel.StatusCode, _responseModel);
                                                        }
                                                    }
                                                }
                                                else if (_responseModel != null && !string.IsNullOrEmpty(_responseModel.StatusCode) && (_responseModel.StatusCode == AggregatorySTATUSCODES.FAILED))
                                                {
                                                    // Response.Create(false, ResponseMessages.AGGREGATOR_FAILED_ERROR, _responseModel.StatusCode, new AddMoneyAggregatorResponse());
                                                    response.Message = ResponseMessages.AGGREGATOR_FAILED_ERROR;
                                                    response.RstKey = 4;
                                                    response.StatusCode = _responseModel.StatusCode;
                                                }
                                                else if (_responseModel != null && !string.IsNullOrEmpty(_responseModel.StatusCode) && (_responseModel.StatusCode == AggregatorySTATUSCODES.EXCEPTION))
                                                {
                                                    //  Response.Create(false, ResponseMessages.AGGREGATOR_FAILED_EXCEPTION, _responseModel.StatusCode, new AddMoneyAggregatorResponse());
                                                    response.Message = ResponseMessages.AGGREGATOR_FAILED_EXCEPTION;
                                                    response.RstKey = 5;
                                                    response.StatusCode = _responseModel.StatusCode;
                                                }
                                                else
                                                {
                                                    response.Message = _responseModel.Message;
                                                    response.RstKey = 6;
                                                    response.StatusCode = _responseModel.StatusCode;
                                                    // Response.Create(false, _responseModel.Message, _responseModel.StatusCode, new AddMoneyAggregatorResponse());
                                                }
                                            }
                                            else
                                            {
                                                var tran = new WalletTransaction();
                                                tran.BeneficiaryName = "";
                                                tran.CreatedDate = DateTime.UtcNow; ;
                                                tran.UpdatedDate = DateTime.UtcNow;
                                                tran.IsAddDuringPay = false;
                                                //Self Account 
                                                tran.ReceiverId = sender.WalletUserId;
                                                //Sender
                                                tran.WalletServiceId = WalletService.WalletServiceId;
                                                tran.TransactionType = AggragatorServiceType.DEBIT;
                                                tran.TransactionTypeInfo = (int)TransactionTypeInfo.PaidByPayServices;
                                                tran.VoucherCode = string.Empty;
                                                tran.SenderId = sender.WalletUserId;
                                                tran.WalletAmount = Request.Amount;
                                                tran.ServiceTax = "0";
                                                tran.ServiceTaxRate = 0;
                                                tran.DisplayContent = Request.DisplayContent;
                                                tran.UpdatedOn = DateTime.UtcNow;
                                                tran.TransactionInitiateRequestId = transationInitiate.Id;
                                                tran.AccountNo = customer;// string.Empty;                                                  
                                                tran.BankTransactionId = string.Empty;
                                                tran.IsBankTransaction = false;
                                                tran.BankBranchCode = string.Empty;
                                                tran.TransactionId = "";
                                                tran.TransactionStatus = (int)TransactionStatus.Pending;
                                                tran.IsAdminTransaction = true;
                                                tran.IsActive = true;
                                                tran.IsDeleted = false;
                                                tran.InvoiceNo = invoiceNumber.InvoiceNumber;
                                                tran.Comments = Request.Comment;
                                                tran.MerchantCommissionId = _commission.MerchantCommissionId;
                                                tran.MerchantCommissionAmount = Convert.ToString(_commission.MerchantCommissionAmount);
                                                tran.CommisionId = _commission.CommissionId;
                                                tran.CommisionAmount = Convert.ToString(_commission.CommissionAmount);
                                                tran.CommisionPercent = _commission.CommissionAmount.ToString();
                                                tran.TotalAmount = Convert.ToString(_commission.AmountWithCommission);
                                                tran = await _cardPaymentRepository.MobileMoneyForAddServices(tran);

                                                response.Message = ResponseMessages.TRANSACTION_NULL_ERROR;
                                                response.RstKey = 7;
                                                response.Status = (int)HttpStatusCode.ExpectationFailed;
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
                                    var addLimit = limit - (Convert.ToDecimal(Request.Amount) + totalAmountTransfered);
                                    var preferAmount = Convert.ToDecimal(Request.Amount) + totalAmountTransfered;
                                    var finalAmt = (preferAmount - limit).ToString();
                                    if (addLimit < Convert.ToDecimal(Request.Amount))
                                    {
                                        response.RstKey = 6;
                                        response.Status = (int)WalletTransactionStatus.OTHER_ERROR;
                                        response.Message = "Exceed your transaction limit.";
                                    }
                                    else
                                    {
                                        response.RstKey = 6;
                                        response.Status = (int)WalletTransactionStatus.OTHER_ERROR;
                                        response.Message = "You can not add more then " + transactionLimit.TransactionLimitForAddMoney.ToString() + " XOF in a day";
                                    }

                                }
                            }
                            else if (string.IsNullOrWhiteSpace(sender.DocumetStatus.ToString()) || sender.DocumetStatus == 0)
                            {
                                response.RstKey = 13;
                                response.Message = ResponseMessageKyc.FAILED_Doc_NotUploaded;
                            }
                            else if (string.IsNullOrWhiteSpace(sender.DocumetStatus.ToString()) || sender.DocumetStatus == 1)
                            {
                                response.RstKey = 14;
                                response.Message = ResponseMessageKyc.FAILED_Doc_Pending;
                            }
                            else if (string.IsNullOrWhiteSpace(sender.DocumetStatus.ToString()) || sender.DocumetStatus == 4)
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
                            response.RstKey = 18;
                            response.Message = ResponseMessages.TRANSACTION_SERVICE_CHANNEL_NOT_REGISTERED;
                        }
                    }
                    else
                    {
                        response.Status = (int)WalletTransactionStatus.FAILED;
                        response.Message = ResponseMessages.EMAIL_VERIFICATION_PENDING;
                        response.RstKey = 6;
                    }
                }
                else
                {
                    response.RstKey = 21;
                    response.Status = (int)WalletTransactionStatus.FAILED;
                    response.Message = ResponseMessages.MobileNotVerify;//
                }


            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.RstKey = 6;
                //Response.Create(false, ResponseMessages.EZEEPAY_FAILED_EXCEPTION, HttpStatusCode.ExpectationFailed, new AddMoneyAggregatorResponse());
                //Response.Create(false, "Exception occured", HttpStatusCode.ExpectationFailed, new AddMoneyAggregatorResponse());
            }

            return response;
        }



        public async Task<int> MerchantContent(MerchantTransactionRequest request, string TransactionId, long? WalletUserId, int? TransactionStatus)
        {

            try
            {
                var _record = new AddDuringPayRecord();
                _record.amount = request.Amount;
                _record.channel = string.Empty;
                _record.chennelId = 0;
                _record.Comment = !string.IsNullOrEmpty(request.Comment) ? request.Comment : string.Empty;
                _record.customer = string.Empty;
                _record.invoiceNo = string.Empty;
                _record.IsAddDuringPay = true;
                _record.IsMerchant = true;
                _record.MerchantId = request.MerchantId;
                _record.ISD = string.Empty;
                _record.OrderNo = string.Empty;
                _record.serviceCategory = string.Empty;
                _record.Password = !string.IsNullOrEmpty(request.Password) ? request.Password : string.Empty;
                _record.ServiceCategoryId = 0;
                _record.TransactionNo = TransactionId;
                _record.TransactionStatus = TransactionStatus;
                _record.WalletUserId = WalletUserId;
                _record.UpdatedDate = DateTime.UtcNow;
                _record.CreatedDate = DateTime.UtcNow;
                _record.IsDeleted = false;
                _record.IsActive = true;
                _record = await _cardPaymentRepository.MerchantContent(_record);
                return _record.AddDuringPayRecordId;
            }
            catch (Exception)
            {
                return 0;

            }

        }

        public async Task<int> PayMoneyContent(PayMoneyContent request, string TransactionId, long? WalletUserId, int? TransactionStatus)
        {

            try
            {
                var _record = new AddDuringPayRecord();
                _record.amount = request.amount;
                _record.channel = request.channel;
                _record.chennelId = request.chennelId;
                _record.Comment = !string.IsNullOrEmpty(request.Comment) ? request.Comment : string.Empty;
                _record.customer = request.customer;
                _record.invoiceNo = !string.IsNullOrEmpty(request.invoiceNo) ? request.invoiceNo : string.Empty;
                _record.IsAddDuringPay = true;
                _record.IsMerchant = false;
                _record.ISD = request.ISD;
                _record.OrderNo = string.Empty;
                _record.serviceCategory = !string.IsNullOrEmpty(request.serviceCategory) ? request.serviceCategory : string.Empty;
                _record.ServiceCategoryId = request.ServiceCategoryId;
                _record.MerchantId = 0;
                _record.Password = !string.IsNullOrEmpty(request.Password) ? request.Password : string.Empty;
                _record.TransactionNo = TransactionId;
                _record.TransactionStatus = TransactionStatus;
                _record.WalletUserId = WalletUserId;
                _record.UpdatedDate = DateTime.UtcNow;
                _record.CreatedDate = DateTime.UtcNow;
                _record.IsDeleted = false;
                _record.IsActive = true;
                _record = await _cardPaymentRepository.PayMoneyContent(_record);
                return _record.AddDuringPayRecordId;

            }
            catch (Exception)
            {
                return 0;
            }

        }

        public async Task<CardPaymentSaveResponse> SavePaymentResponse(CardPaymentWebResponse request)
        {
            CalculateCommissionResponse _commission = new CalculateCommissionResponse();
            CalculateCommissionRequest _commissionRequest = new CalculateCommissionRequest();
            string RequestString = JsonConvert.SerializeObject(request);
            LogTransactionTypes.Response.SaveTransactionLog(LogTransactionNameTypes.AddMoney + "Credit to Debit Cards", RequestString, "Responce by bank detail : ");
            CardPaymentSaveResponse response = new CardPaymentSaveResponse();

            try
            {
                var requestDetail = await _cardPaymentRepository.GetCardPaymentRequest(request.refNo, request.vpc_MerchTxnRef);

                var getInitialTransaction = await _cardPaymentRepository.GetTransactionInitiateRequest(request.refNo);
                //var requestDetail = db.CardPaymentRequests.Where(x => x.OrderNo == request.refNo).FirstOrDefault();

                if (requestDetail != null)
                {
                    // LogTransactionTypes.Request.SaveTransactionLog(LogTransactionNameTypes.PayMoney + subcategory.CategoryName, Request, "Aggregator Url : " + (apiUrl + "?" + GetUrl));

                    CardPaymentHelper _helper = new CardPaymentHelper();

                    //CardPaymentResponse _req = new CardPaymentResponse();
                    //_req.vpc_AVSResultCode = request.vpc_AVSResultCode;
                    //_req.vpc_AcqAVSRespCode = request.vpc_AcqAVSRespCode;
                    //_req.vpc_AcqCSCRespCode = request.vpc_AcqCSCRespCode;
                    //_req.vpc_AcqResponseCode = request.vpc_AcqResponseCode;
                    //_req.vpc_AuthorizeId = request.vpc_AuthorizeId;
                    //_req.vpc_BatchNo = request.vpc_BatchNo;
                    //_req.vpc_CSCResultCode = request.vpc_CSCResultCode;
                    //_req.vpc_Card = request.vpc_Card;
                    //_req.vpc_Command = request.vpc_Command;
                    //_req.vpc_Currency = request.vpc_Currency;
                    //_req.vpc_Locale = request.vpc_Locale;
                    //_req.vpc_Merchant = request.vpc_Merchant;
                    //_req.vpc_Message = request.vpc_Message;
                    //_req.vpc_ReceiptNo = request.vpc_ReceiptNo;
                    //_req.vpc_SecureHash = request.vpc_SecureHash;
                    //_req.vpc_TransactionNo = request.vpc_TransactionNo;
                    //_req.vpc_TxnResponseCode = request.vpc_TxnResponseCode;
                    //_req.vpc_Version = request.vpc_Version;
                    //_req.vpc_VerType = request.vpc_VerType;
                    //_req.vpc_VerStatus = request.vpc_VerStatus;
                    //_req.vpc_VerToken = request.vpc_VerToken;
                    //_req.vpc_VerSecurityLevel = request.vpc_VerSecurityLevel;
                    //_req.vpc_3DSenrolled = request.vpc_3DSenrolled;
                    //_req.vpc_3DSXID = request.vpc_3DSXID;
                    //_req.vpc_3DSECI = request.vpc_3DSECI;
                    //_req.vpc_3DSstatus = request.vpc_3DSstatus;
                    //_req.vpc_hashValidated = request.vpc_hashValidated;
                    //_req.vpc_ResponseCodeDescription = _helper.ResponseDescription(request.vpc_TxnResponseCode);
                    //_req.vpc_StatusCodeDescription = _helper.get3DSstatusDescription(request.vpc_VerStatus);
                    //_req.CardPaymentRequestId = requestDetail.CardPaymentRequestId;
                    CardPaymentResponse _req = new CardPaymentResponse();
                    _req.vpc_AVSResultCode = "unsupported";
                    _req.vpc_AcqAVSRespCode = "unsupported";
                    _req.vpc_AcqCSCRespCode = "M";
                    _req.vpc_AcqResponseCode = "00";
                    _req.vpc_AuthorizeId = "499673UBA";
                    _req.vpc_BatchNo = "20200401UBA";
                    _req.vpc_CSCResultCode = "m";
                    _req.vpc_Card = "VC/MC";
                    _req.vpc_Command = "pay";
                    _req.vpc_Currency = "XOF";
                    _req.vpc_Locale = "fr";
                    _req.vpc_Merchant = "CMCDI10552";
                    _req.vpc_Message = request.status;
                    _req.vpc_ReceiptNo = "009218660583UBA";
                    _req.vpc_SecureHash = "6ed2710c3a7e8c9ae8d99ec3cbb21e0e6917897fdc05bb67";
                    _req.vpc_TransactionNo = request.transactionID;
                    _req.vpc_TxnResponseCode = "0";
                    _req.vpc_Version = "1";
                    _req.vpc_VerType = "UBA";
                    _req.vpc_VerStatus = "y";
                    _req.vpc_VerToken = "aaabbslvfhoigsn4v1uweclsa4g=";
                    _req.vpc_VerSecurityLevel = "1";
                    _req.vpc_3DSenrolled = "1";
                    _req.vpc_3DSXID = "4ztbqo/dp0fxqjler3ookkrxwwk=";
                    _req.vpc_3DSECI = "1";
                    _req.vpc_3DSstatus = request.status;
                    _req.vpc_hashValidated = request.vpc_hashValidated;
                    _req.vpc_ResponseCodeDescription = _helper.ResponseDescription(request.vpc_TxnResponseCode);
                    _req.vpc_StatusCodeDescription = _helper.get3DSstatusDescription(request.vpc_VerStatus);
                    _req.CardPaymentRequestId = requestDetail.CardPaymentRequestId;
                    //db.CardPaymentResponses.Add(_req);
                    //db.SaveChanges();
                    await _cardPaymentRepository.SaveCardPaymentResponse(_req);
                    getInitialTransaction.JsonResponse = JsonConvert.SerializeObject(_req);
                    //check duplicate callback
                    int duplicateOrNotTransactionNo = await _cardPaymentRepository.IsduplicateOrNotTransactionNo(request.transactionID); //take vpc_txnno
                    if (duplicateOrNotTransactionNo == 1) //check double callback 
                    {
                        // if (request.vpc_TxnResponseCode == Convert.ToString((int)CardPaymentHelper.ResponseDescriptionTypes.Success))
                        if (request.status.ToUpper() == "DECLINED" || request.status.ToUpper() == "APPROVED")
                        {
                            response.TransactionRefId = _req.CardPaymentResponseId;
                            response.PaymentTransactionNo = request.refNo;
                            //Math.Round((Amount+Commission)*100,2) Decimal Amount
                            //this line commented due to currentbalance is not added to card expected 
                            //response.TransactionAmount = Convert.ToString(Math.Round((Convert.ToDecimal(requestDetail.TotalAmount) / 100), 2));
                            response.TransactionAmount = Convert.ToString(Math.Round(Convert.ToDecimal(requestDetail.Amount), 2));
                            response.TransactionResponseDescription = request.status;
                            response.TransactionResponseCode = request.transactionID;
                            //response.CurrentBalance=CurrentData.CrrentBalance;
                            AddCardMoneyResponse _transctionResponse = new AddCardMoneyResponse();

                            int WalletServiceId = await _cardPaymentRepository.GetServiceId();
                            if (WalletServiceId > 0)
                            {
                                var adminUser = await _cardPaymentRepository.GetAdminUser();
                                if (adminUser != null)
                                {

                                    // var receiver = await _walletUserRepository.GetCurrentUser(Convert.ToInt32(requestDetail.WalletUserId)); //db.WalletUsers.Where(x => x.WalletUserId == requestDetail.WalletUserId).FirstOrDefault();

                                    long userId = Convert.ToInt32(requestDetail.WalletUserId);
                                    var UserCurrentDetail = await _walletUserRepository.GetCurrentUser(userId);
                                    if (UserCurrentDetail != null && await _cardPaymentRepository.IsWalletTransactions(UserCurrentDetail.WalletUserId, request.refNo))
                                    {
                                        //this line commented due to currentbalance is not added to card expected 
                                        //request.vpc_Amount = Convert.ToString(Math.Round((Convert.ToDecimal(requestDetail.Amount) / 100), 2));
                                        request.vpc_Amount = Convert.ToString(Math.Round(Convert.ToDecimal(requestDetail.Amount), 2));

                                        // to update wallet amount-----

                                        // db.WalletUsers.FirstOrDefault(x => x.WalletUserId == receiver.WalletUserId);

                                        if (UserCurrentDetail != null)
                                        {
                                            _commissionRequest.CurrentBalance = Convert.ToDecimal(UserCurrentDetail.CurrentBalance);
                                            _commissionRequest.IsRoundOff = true;
                                            //_commissionRequest.TransactionAmount = Convert.ToDecimal(request.vpc_Amount);

                                            _commissionRequest.TransactionAmount = Convert.ToDecimal(requestDetail.Amount); //change
                                            _commissionRequest.WalletServiceId = WalletServiceId;
                                            _commission = await _setCommisionRepository.CalculateCommission(_commissionRequest);

                                            if (!string.IsNullOrEmpty(request.refNo) && request.status.ToUpper() == "APPROVED")
                                            {
                                                getInitialTransaction.TransactionStatus = (int)TransactionStatus.Completed;
                                                if (Convert.ToDecimal(UserCurrentDetail.CurrentBalance) >= 0)
                                                {
                                                    if (Convert.ToDecimal(UserCurrentDetail.CurrentBalance) == 0)
                                                    {
                                                        UserCurrentDetail.CurrentBalance = _commission.TransactionAmount.ToString();
                                                        getInitialTransaction.AfterTransactionBalance = UserCurrentDetail.CurrentBalance.ToString();
                                                        getInitialTransaction.ReceiverCurrentBalance = UserCurrentDetail.CurrentBalance.ToString();
                                                    }
                                                    else
                                                    {
                                                        UserCurrentDetail.CurrentBalance = Math.Round(Convert.ToDecimal(UserCurrentDetail.CurrentBalance) + _commission.TransactionAmount, 2).ToString();
                                                        getInitialTransaction.AfterTransactionBalance = UserCurrentDetail.CurrentBalance.ToString();
                                                        getInitialTransaction.ReceiverCurrentBalance = UserCurrentDetail.CurrentBalance.ToString();
                                                    }
                                                }
                                                else
                                                {
                                                    UserCurrentDetail.CurrentBalance = Math.Round(_commission.TransactionAmount - Convert.ToDecimal(UserCurrentDetail.CurrentBalance), 2).ToString();
                                                    getInitialTransaction.AfterTransactionBalance = UserCurrentDetail.CurrentBalance.ToString();
                                                    getInitialTransaction.ReceiverCurrentBalance = UserCurrentDetail.CurrentBalance.ToString();
                                                }
                                            }
                                            await _walletUserRepository.UpdateUserDetail(UserCurrentDetail);
                                            await _cardPaymentRepository.UpdateTransactionInitiateRequest(getInitialTransaction);
                                            // db.SaveChanges();
                                        }
                                        DateTime TDate = DateTime.UtcNow;
                                        #region Save Transaction
                                        var _Transaction = new WalletTransaction();
                                        response.TransactionDate = TDate;
                                        _Transaction.TransactionInitiateRequestId = getInitialTransaction.Id;
                                        _Transaction.MerchantCommissionAmount = Convert.ToString(_commission.MerchantCommissionAmount);
                                        _Transaction.MerchantCommissionId = _commission.MerchantCommissionId;
                                        _Transaction.TransactionType = AggragatorServiceType.CREDIT;
                                        _Transaction.IsBankTransaction = false;
                                        _Transaction.TransactionTypeInfo = (int)TransactionTypeInfo.AddedByCard;
                                        _Transaction.IsBankTransaction = false;
                                        _Transaction.Comments = string.Empty;
                                        _Transaction.InvoiceNo = getInitialTransaction.InvoiceNumber;
                                        _Transaction.CommisionAmount = Convert.ToString(_commission.CommissionAmount);
                                        _Transaction.CommisionId = _commission.CommissionId;
                                        _Transaction.WalletAmount = Convert.ToString(_commission.TransactionAmount);
                                        //  _Transaction.TotalAmount = Convert.ToString(_commission.TransactionAmount);
                                        _Transaction.TotalAmount = Convert.ToString(_commission.AmountWithCommission);
                                        _Transaction.ServiceTaxRate = _commission.ServiceTaxRate;
                                        _Transaction.ServiceTax = Convert.ToString(_commission.ServiceTaxAmount);
                                        _Transaction.OperatorType = "sample";
                                        if (!string.IsNullOrEmpty(request.refNo) && request.status.ToUpper() == "APPROVED")
                                        {

                                            _Transaction.TransactionStatus = (int)TransactionStatus.Completed; ;
                                            try
                                            {
                                                //--------send mail on success transaction--------

                                                var AdminKeys = AES256.AdminKeyPair;
                                                string FirstName = AES256.Decrypt(UserCurrentDetail.PrivateKey, UserCurrentDetail.FirstName);
                                                string LastName = AES256.Decrypt(UserCurrentDetail.PrivateKey, UserCurrentDetail.LastName);
                                                string StdCode = UserCurrentDetail.StdCode;
                                                string MobileNo = AES256.Decrypt(AdminKeys.PrivateKey, UserCurrentDetail.MobileNo);
                                                string EmailId = AES256.Decrypt(AdminKeys.PrivateKey, UserCurrentDetail.EmailId).Trim().ToLower();
                                                // var receiverDetail = new AppUserRepository().GetUserDetailById(receiver.WalletUserId);
                                                string filename = CommonSetting.successfullTransaction;
                                                if (request.vpc_Card.ToLower() == "mc")
                                                {
                                                    _Transaction.AccountNo = "MASTER CARD";
                                                }
                                                else if (request.vpc_Card.ToLower() == "vc")
                                                {
                                                    _Transaction.AccountNo = "VISA CARDS";
                                                }
                                                var body = _sendEmails.ReadEmailformats(filename);
                                                body = body.Replace("$$FirstName$$", FirstName + " " + LastName);
                                                body = body.Replace("$$DisplayContent$$", "VISA CARDS/MASTER CARD");
                                                body = body.Replace("$$customer$$", MobileNo);
                                                body = body.Replace("$$amount$$", "XOF " + requestDetail.Amount);
                                                body = body.Replace("$$ServiceTaxAmount$$", "XOF " + _commission.CommissionAmount);
                                                body = body.Replace("$$AmountWithCommission$$", "XOF " + _commission.AmountWithCommission);
                                                body = body.Replace("$$TransactionId$$", Convert.ToString(requestDetail.CardPaymentRequestId));

                                                var req = new EmailModel()
                                                {
                                                    TO = EmailId,
                                                    Subject = "Transaction Successfull",
                                                    Body = body
                                                };
                                                _sendEmails.SendEmail(req);
                                            }
                                            catch
                                            {

                                            }
                                        }
                                        else
                                        {
                                            _Transaction.TransactionStatus = (int)TransactionStatus.Failed;
                                        }
                                        _Transaction.WalletServiceId = WalletServiceId;
                                        _Transaction.SenderId = UserCurrentDetail.WalletUserId;
                                        _Transaction.ReceiverId = UserCurrentDetail.WalletUserId;
                                        _Transaction.BankBranchCode = string.Empty;
                                        _Transaction.BankTransactionId = request.transactionID;
                                        _Transaction.TransactionId = request.transactionID;
                                        if (request.vpc_Card.ToLower() == "mc")
                                        {
                                            _Transaction.AccountNo = "MASTER CARD";
                                        }
                                        else if (request.vpc_Card.ToLower() == "vc")
                                        {
                                            _Transaction.AccountNo = "VISA CARDS";
                                        }
                                        else
                                        {
                                            _Transaction.AccountNo = request.vpc_Card;
                                        }
                                        // _Transaction.AccountNo = request.vpc_Card;
                                        _Transaction.IsAdminTransaction = false;
                                        _Transaction.IsActive = true;
                                        _Transaction.IsDeleted = false;
                                        _Transaction.CreatedDate = TDate;
                                        _Transaction.UpdatedDate = TDate;
                                        _Transaction.TransactionTypeInfo = (int)TransactionTypeInfo.AddedByCard;
                                        _Transaction.IsAddDuringPay = false;
                                        _Transaction.VoucherCode = string.Empty;
                                        await _cardPaymentRepository.SaveWalletTransactions(_Transaction);
                                        //db.WalletTransactions.Add(_Transaction);
                                        //db.SaveChanges();
                                        #endregion

                                        #region Credit
                                        var _credit = new WalletTransactionDetail();
                                        _credit.Amount = Convert.ToString(_commission.TransactionAmount);
                                        _credit.TransactionType = (int)TransactionDetailType.Credit;
                                        _credit.WalletUserId = adminUser.WalletUserId;
                                        _credit.WalletTransactionId = _Transaction.WalletTransactionId;
                                        _credit.IsActive = true;
                                        _credit.IsDeleted = false;
                                        _credit.CreatedDate = TDate;
                                        _credit.UpdatedDate = TDate;
                                        //db.WalletTransactionDetails.Add(_credit);
                                        //db.SaveChanges();
                                        await _cardPaymentRepository.SaveWalletTransactionDetails(_credit);
                                        #endregion

                                        #region Debit
                                        var _debit = new WalletTransactionDetail();
                                        _debit.Amount = Convert.ToString(_commission.TransactionAmount);
                                        _debit.TransactionType = (int)TransactionDetailType.Debit;
                                        _debit.WalletUserId = UserCurrentDetail.WalletUserId;
                                        _debit.WalletTransactionId = _Transaction.WalletTransactionId;
                                        _debit.IsActive = true;
                                        _debit.IsDeleted = false;
                                        _debit.CreatedDate = TDate;
                                        _debit.UpdatedDate = TDate;
                                        //db.WalletTransactionDetails.Add(_credit);
                                        //db.SaveChanges();
                                        await _cardPaymentRepository.SaveWalletTransactionDetails(_credit);
                                        #endregion

                                        //get UpdateNewCardNoResponseBankCode id
                                        await _cardPaymentRepository.UpdateNewCardNoResponseBankCode(getInitialTransaction.InvoiceNumber, UserCurrentDetail.WalletUserId, request.transactionID);


                                        var adminKeyPair = AES256.AdminKeyPair;
                                        _transctionResponse.ToMobileNo = UserCurrentDetail.StdCode + UserCurrentDetail.MobileNo;
                                        _transctionResponse.TransactionAmount = request.vpc_Amount;
                                        _transctionResponse.StatusCode = 1;
                                        _transctionResponse.TransactionDate = TDate;
                                        //db.SaveChanges();
                                        //tran.Commit();
                                        #region PushNotification

                                        var CurrentUser = await _walletUserRepository.GetCurrentUser(UserCurrentDetail.WalletUserId);//db.WalletUsers.Where(x => x.WalletUserId == receiver.WalletUserId).FirstOrDefault();
                                        if (CurrentUser != null)
                                        {
                                            PushNotificationModel push = new PushNotificationModel();
                                            push.SenderId = UserCurrentDetail.WalletUserId;
                                            push.deviceType = (int)UserCurrentDetail.DeviceType;
                                            push.deviceKey = UserCurrentDetail.DeviceToken;
                                            PayMoneyPushModel pushModel = new PayMoneyPushModel();
                                            pushModel.TransactionDate = TDate;
                                            pushModel.TransactionId = request.vpc_TransactionNo;
                                            pushModel.CurrentBalance = CurrentUser.CurrentBalance;
                                            pushModel.alert = _Transaction.WalletAmount + " XOF has been credited to your account.";
                                            pushModel.Amount = request.vpc_Amount;
                                            pushModel.CurrentBalance = CurrentUser.CurrentBalance;
                                            pushModel.pushType = (int)PushType.ADDMONEY;

                                            if ((int)UserCurrentDetail.DeviceType == (int)DeviceTypes.ANDROID || (int)UserCurrentDetail.DeviceType == (int)DeviceTypes.Web)
                                            {
                                                PushPayload<PayMoneyPushModel> aps = new PushPayload<PayMoneyPushModel>();
                                                PushPayloadData<PayMoneyPushModel> _data = new PushPayloadData<PayMoneyPushModel>();
                                                _data.notification = pushModel;
                                                aps.data = _data;
                                                aps.to = UserCurrentDetail.DeviceToken;
                                                aps.collapse_key = string.Empty;
                                                push.message = JsonConvert.SerializeObject(aps);
                                                push.payload = pushModel;
                                            }
                                            if ((int)UserCurrentDetail.DeviceType == (int)DeviceTypes.IOS)
                                            {
                                                NotificationJsonResponse<PayMoneyIOSPushModel> aps = new NotificationJsonResponse<PayMoneyIOSPushModel>();
                                                PayMoneyIOSPushModel _iosPushModel = new PayMoneyIOSPushModel();
                                                _iosPushModel.alert = pushModel.alert;
                                                _iosPushModel.Amount = pushModel.Amount;
                                                _iosPushModel.CurrentBalance = pushModel.CurrentBalance;
                                                _iosPushModel.MobileNo = pushModel.MobileNo;
                                                _iosPushModel.SenderName = pushModel.SenderName;
                                                _iosPushModel.pushType = pushModel.pushType;
                                                aps.aps = _iosPushModel;

                                                push.message = JsonConvert.SerializeObject(aps);
                                            }
                                            //if (!string.IsNullOrEmpty(push.message))
                                            //{
                                            //    new PushNotificationRepository().sendPushNotification(push);
                                            //}
                                        }
                                        #endregion

                                        if (UserCurrentDetail.MobileNo != null && UserCurrentDetail.MobileNo != "")
                                        {
                                            response.ToMobileNo = AES256.Decrypt(adminKeyPair.PrivateKey, UserCurrentDetail.MobileNo);
                                        }

                                        response.TransactionDate = TDate;
                                        response.CurrentBalance = UserCurrentDetail.CurrentBalance;
                                        // response.ToMobileNo = receiver.StdCode + receiver.MobileNo;

                                        var IsAddduringPay = await _cardPaymentRepository.AddDuringPayRecords(request.vpc_OrderInfo, request.vpc_MerchTxnRef);//db.AddDuringPayRecords.Where(x => x.OrderNo == request.vpc_OrderInfo && x.TransactionNo == request.vpc_MerchTxnRef && x.TransactionStatus == (int)TransactionStatus.Pending).FirstOrDefault();
                                        if (IsAddduringPay != null)
                                        {
                                            #region PayMoneyAfterAdd

                                            //var storeddata = db.AddDuringPayRecords.Where(x => x.OrderNo == request.vpc_OrderInfo && x.TransactionNo == request.vpc_MerchTxnRef && x.TransactionStatus == (int)TransactionStatus.Pending).Select(x => new PayMoneyAggregatoryRequest
                                            //{
                                            //    Amount = x.amount,
                                            //    channel = x.channel,
                                            //    chennelId = x.chennelId ?? 0,
                                            //    Comment = x.Comment,
                                            //    customer = x.customer,
                                            //    invoiceNo = x.invoiceNo,
                                            //    IsAddDuringPay = x.IsAddDuringPay ?? false,
                                            //    ISD = x.ISD,
                                            //    serviceCategory = x.serviceCategory,
                                            //    ServiceCategoryId = x.ServiceCategoryId ?? 0,
                                            //    IsMerchant = x.IsMerchant ?? false,
                                            //    MerchantId = x.MerchantId ?? 0
                                            //}).FirstOrDefault();
                                            var storeddata = await _cardPaymentRepository.AddDuringPayRecord(request.vpc_OrderInfo, request.vpc_MerchTxnRef);
                                            if (storeddata != null)
                                            {
                                                PayMoneyAggregatoryRequest _record = new PayMoneyAggregatoryRequest();
                                                _record.Amount = storeddata.Amount;
                                                _record.channel = storeddata.channel;
                                                _record.chennelId = storeddata.chennelId;
                                                _record.Comment = storeddata.Comment;
                                                _record.customer = storeddata.customer;
                                                _record.invoiceNo = storeddata.invoiceNo;
                                                _record.IsAddDuringPay = storeddata.IsAddDuringPay;
                                                _record.ISD = storeddata.ISD;
                                                _record.serviceCategory = storeddata.serviceCategory;
                                                _record.ServiceCategoryId = storeddata.ServiceCategoryId;
                                                _record.IsMerchant = storeddata.IsMerchant;
                                                _record.MerchantId = storeddata.MerchantId;
                                                if (_record != null)
                                                {
                                                    AddDuringPayResponse payrespone = new AddDuringPayResponse();
                                                    if (_record.IsAddDuringPay && _record.IsMerchant && _record.MerchantId > 0)
                                                    {
                                                        // WalletTransactionRepository obj = new WalletTransactionRepository();
                                                        MerchantTransactionRequest merchantRequest = new MerchantTransactionRequest();
                                                        merchantRequest.Amount = _record.Amount;
                                                        merchantRequest.Comment = _record.Comment;
                                                        merchantRequest.MerchantId = _record.MerchantId;
                                                        var merchantResponse = await _merchantPaymentService.MerchantPayment(merchantRequest, null, UserCurrentDetail.WalletUserId);////pass null it never entry here -token for userprofile
                                                        if (merchantResponse.StatusCode == (int)TransactionStatus.Completed)
                                                        {
                                                            IsAddduringPay.TransactionStatus = (int)TransactionStatus.Completed;
                                                            await _cardPaymentRepository.UpdateAddDuringPayRecord(IsAddduringPay);
                                                            //db.SaveChanges();
                                                        }
                                                        payrespone.Amount = merchantResponse.TransactionAmount;
                                                        payrespone.CurrentBalance = merchantResponse.ToMobileNo;
                                                        payrespone.AccountNo = merchantResponse.ToMobileNo;
                                                        payrespone.MobileNo = merchantResponse.ToMobileNo;
                                                        payrespone.MerchantStatusCode = merchantResponse.StatusCode;
                                                        payrespone.StatusCode = Convert.ToString(merchantResponse.StatusCode);
                                                        payrespone.TransactionDate = merchantResponse.TransactionDate;
                                                        payrespone.TransactionId = Convert.ToString(merchantResponse.TransactionId);
                                                        payrespone.IsMerchant = true;
                                                    }
                                                    else
                                                    {
                                                        // PayServiesRepository obj = new PayServiesRepository();
                                                        var aggregatorResponse = await _mobileMoneyServices.MobileMoneyService(_record, UserCurrentDetail.WalletUserId);
                                                        if (aggregatorResponse.RstKey == 1)
                                                        {
                                                            IsAddduringPay.TransactionStatus = (int)TransactionStatus.Completed;
                                                            await _cardPaymentRepository.UpdateAddDuringPayRecord(IsAddduringPay);
                                                            // db.SaveChanges();
                                                        }
                                                        payrespone.Amount = aggregatorResponse.Amount;
                                                        payrespone.CurrentBalance = aggregatorResponse.CurrentBalance; ;
                                                        payrespone.AccountNo = aggregatorResponse.AccountNo;
                                                        payrespone.MobileNo = aggregatorResponse.MobileNo;
                                                        payrespone.StatusCode = Convert.ToString(aggregatorResponse.StatusCode);
                                                        payrespone.TransactionDate = aggregatorResponse.TransactionDate;
                                                        payrespone.TransactionId = Convert.ToString(aggregatorResponse.TransactionId);
                                                        payrespone.IsMerchant = false;
                                                    }
                                                    response.IsAddDuringPay = true;
                                                    response.AddDuringPayResponse = payrespone;
                                                }
                                            }
                                            else
                                            {
                                                response.AddDuringPayResponse = new AddDuringPayResponse();
                                            }
                                            #endregion
                                        }
                                        else
                                        {
                                            response.AddDuringPayResponse = new AddDuringPayResponse();
                                        }


                                        ///
                                        await _masterDataService.Chargeback(UserCurrentDetail.WalletUserId);
                                    }
                                    else
                                    {
                                        //test

                                    }
                                }
                                else
                                {
                                    //test

                                }
                                //sdfsdfd
                            }
                            else
                            {
                            }
                        }
                        else
                        {
                            response.TransactionResponseDescription = _req.vpc_ResponseCodeDescription;
                            response.TransactionResponseCode = request.vpc_TxnResponseCode;
                        }
                    }
                    else
                    {
                    }



                }
                else
                {
                }

            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("CardPaymentRepository", "SavePaymentResponse", request);
            }
            return response;
        }
        public String Sha256Hash(PayServicesMoneyAggregatoryRequest request)
        {
            StringBuilder sb = new StringBuilder();
            //Url for Payment          
            sb.Append(request.ApiKey);
            sb.Append(request.Amount);
            sb.Append(request.Customer);
            sb.Append(request.TransactionId);
            sb.Append(ThirdPartyAggragatorSettings.secretKey);
            StringBuilder hash = new StringBuilder();

            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
                for (int i = 0; i < bytes.Length; i++)
                {
                    hash.Append(bytes[i].ToString("x2"));
                }
            }
            //byte[] bytes = sha256.ComputeHash(new UTF8Encoding().GetBytes(sb.ToString()));

            return hash.ToString();
        }
        public String MD5Hash(MobileMoneyAggregatoryRequest request)
        {
            StringBuilder sb = new StringBuilder();
            //Url for Payment
            sb.Append(request.apiKey);
            sb.Append(request.customer);
            sb.Append(request.amount);
            sb.Append(request.invoiceNo);
            sb.Append(ThirdPartyAggragatorSettings.secretKey);
            StringBuilder hash = new StringBuilder();
            MD5CryptoServiceProvider md5provider = new MD5CryptoServiceProvider();
            byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(sb.ToString()));

            for (int i = 0; i < bytes.Length; i++)
            {
                hash.Append(bytes[i].ToString("x2"));
            }
            return hash.ToString();
        }

        async Task<string> HttpGetUrlEncodedServiceForMobileMoney(string Log, string Url, object parameters, object Request, string CategoryName)
        {
            string detail = string.Empty;
            string responseString = string.Empty;

            string requestQueryString = string.Empty;
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    requestQueryString = GetQueryString(parameters);
                    requestQueryString = requestQueryString.Replace("%2c", ",");
                    detail = Url + "?" + requestQueryString;
                    LogTransactionTypes.Request.SaveTransactionLog(Log + CategoryName, Request, detail);
                    HttpResponseMessage response = await httpClient.GetAsync(detail);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        responseString = result.ToString();
                    }
                    // responseString = "{\"StatusCode\":\"300\",\"Message\":\"PENDING\",\"TransactionId\":\"285\",\"InvoiceNo\":null}";
                }
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog(Log + CategoryName, Log + CategoryName, Log + CategoryName);
                LogTransactionTypes.Response.SaveTransactionLog(Log + CategoryName, Request, detail + ", Exception Occured : " + ex.Message);
                responseString = "{\"StatusCode\":\"300\",\"Message\":\"PENDING\",\"TransactionId\":\"\",\"InvoiceNo\":\"\"}";
            }
            return responseString;
        }
        public string GetQueryString(object obj)
        {
            var properties = from p in obj.GetType().GetProperties()
                             where p.GetValue(obj, null) != null
                             select p.Name + "=" + HttpUtility.UrlEncode(p.GetValue(obj, null).ToString());

            return String.Join("&", properties.ToArray());
        }


        public async Task<AddCashDepositToBankResponse> AddCashDepositToBankServices(AddCashDepositToBankRequest Request, string sessionToken, long WalletUserId = 0)
        {

            var response = new AddCashDepositToBankResponse();
            var sender = await _walletUserService.UserProfile(sessionToken);
            //var Isdocverified = await _walletUserRepository.IsDocVerified(sender.WalletUserId, sender.DocumetStatus);
            var Isdocverified = await _walletUserRepository.IsDocVerifiedMOMO(sender.DocumetStatus);
            var CurrentUser = await _walletUserRepository.GetCurrentUser(sender.WalletUserId);

            string responseString = string.Empty;

            try
            {
                if (CurrentUser.IsOtpVerified == true) //mobile exist or not then 
                {
                    if (sender.IsEmailVerified == true)
                    {

                        // var adminKeyPair = AES256.AdminKeyPair;
                        if (Isdocverified == true)
                        {
                            if (sender != null)
                            {
                                var data = await _walletUserRepository.GetCurrentUser(sender.WalletUserId);
                                if (data != null)
                                {
                                    response = await _walletUserRepository.AddCashDepositToBankServices(Request);

                                    //
                                    if (response.StatusCode >= 1)
                                    {
                                        response.Status = (int)HttpStatusCode.OK;
                                        response.Message = AggregatoryMESSAGE.SUCCESSFUL;
                                        response.RstKey = 1;
                                    }
                                    else
                                    {
                                        response.Message = AggregatoryMESSAGE.FAILED;
                                        response.RstKey = 3;
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
                        else if (string.IsNullOrWhiteSpace(sender.DocumetStatus.ToString()) || sender.DocumetStatus == 0)
                        {
                            response.RstKey = 13;
                            response.Message = ResponseMessageKyc.FAILED_Doc_NotUploaded;
                        }
                        else if (string.IsNullOrWhiteSpace(sender.DocumetStatus.ToString()) || sender.DocumetStatus == 1)
                        {
                            response.RstKey = 14;
                            response.Message = ResponseMessageKyc.FAILED_Doc_Pending;
                        }
                        else if (string.IsNullOrWhiteSpace(sender.DocumetStatus.ToString()) || sender.DocumetStatus == 4)
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

                        response.Message = ResponseMessages.EMAIL_VERIFICATION_PENDING;
                        response.RstKey = 6;
                    }
                }
                else
                {
                    response.RstKey = 21;
                    response.Status = (int)WalletTransactionStatus.FAILED;
                    response.Message = ResponseMessages.MobileNotVerify;//
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.RstKey = 6;
            }

            return response;
        }



        public async Task<DuplicateCardNoVMResponse> AddNewCardNo(DuplicateCardNoVMRequest Request, string sessionToken)
        {
            int i = 0;
            var response = new DuplicateCardNoVMResponse();
            var sender = await _walletUserService.UserProfile(sessionToken);
            var Isdocverified = await _walletUserRepository.IsDocVerifiedMOMO(sender.DocumetStatus);

            string responseString = string.Empty;

            try
            {
                if (sender.IsEmailVerified == true)
                {
                    if (Isdocverified == true)
                    {
                        if (sender != null)
                        {
                            var data = await _walletUserRepository.GetCurrentUser(sender.WalletUserId);

                            var checkflagduplicatecard = await _cardPaymentRepository.SaveNewCardNo(sender.WalletUserId, Request.Cardno, Request.NewCardImage, "Check");
                            if (data != null)
                            {
                                if (checkflagduplicatecard == 1)
                                {
                                    try
                                    {
                                        //--------send mail on success transaction--------
                                        var senderdata = await _walletUserRepository.GetUserDetailById(sender.WalletUserId);

                                        //string filename = AppSetting.successfullTransaction;
                                        var body = _sendEmails.ReadEmailformats("emailcardNo.html");
                                        body = body.Replace("$$FirstLastName$$", senderdata.FirstName + " " + senderdata.LastName);
                                        body = body.Replace("$$Email$$", senderdata.EmailId);
                                        body = body.Replace("$$MobileNo$$", senderdata.MobileNo);
                                        body = body.Replace("$$CardNo$$", Request.Cardno);
                                        body = body.Replace("$$NewCardImage$$", Request.NewCardImage);

                                        var req = new EmailModel
                                        {
                                            TO = "kyc.support@ezipaysarl.com",
                                            Subject = "Sarl User Update New Card",
                                            Body = body
                                        };
                                        var msg = _sendEmails.SendEmail(req);
                                        if (msg == "Email sent successfully")
                                        {
                                            i = await _cardPaymentRepository.SaveNewCardNo(senderdata.WalletUserId, Request.Cardno, Request.NewCardImage, null);
                                        }
                                    }
                                    catch
                                    {

                                    }
                                    //
                                    if (i == 1)
                                    {
                                        response.Message = "Your New Card Details has reached to Ezipay & will update soon after validation";
                                        response.RstKey = 1;
                                    }
                                    else
                                    {
                                        response.Message = AggregatoryMESSAGE.FAILED;
                                        response.RstKey = 3;
                                    }

                                }
                                else
                                {
                                    response.RstKey = 6;
                                    response.Message = "Duplicate Entry";
                                }
                            }
                            else
                            {
                                response.RstKey = 6;
                                response.Message = ResponseMessages.USER_NOT_REGISTERED;
                            }
                        }
                        else
                        {
                            response.RstKey = 12;
                            response.Message = ResponseMessages.USER_NOT_REGISTERED;
                        }

                    }
                    else if (string.IsNullOrWhiteSpace(sender.DocumetStatus.ToString()) || sender.DocumetStatus == 0)
                    {
                        response.RstKey = 13;
                        response.Message = ResponseMessageKyc.FAILED_Doc_NotUploaded;
                    }
                    else if (string.IsNullOrWhiteSpace(sender.DocumetStatus.ToString()) || sender.DocumetStatus == 1)
                    {
                        response.RstKey = 14;
                        response.Message = ResponseMessageKyc.FAILED_Doc_Pending;
                    }
                    else if (string.IsNullOrWhiteSpace(sender.DocumetStatus.ToString()) || sender.DocumetStatus == 4)
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

                    response.Message = ResponseMessages.EMAIL_VERIFICATION_PENDING;
                    response.RstKey = 6;
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.RstKey = 6;
            }

            return response;
        }

        public async Task<OtpResponse> WalletSendOtp(ViewModel.WalletUserVM.OtpRequest request, string sessionToken)
        {
            var response = new OtpResponse();
            var UserDetail = await _walletUserService.UserProfile(sessionToken);

            if (UserDetail.IsMobileNoVerified == true)
            {
                string Otp = CommonSetting.GetOtp();

                var req = new ViewModel.WalletUserVM.SendOtpRequest
                {
                    IsdCode = request.IsdCode,
                    MobileNo = request.MobileNo,
                    Otp = Otp
                };

                response = await _walletUserRepository.WalletSendOtp(req, UserDetail.WalletUserId);
            }
            else
            {
                response.StatusCode = 3;
            }
            return response;
        }



        public async Task<ViewModel.WalletUserVM.UserExistanceResponse> WalletVerifyOtp(ViewModel.WalletUserVM.VerifyOtpRequest request, string sessionToken)
        {
            ViewModel.WalletUserVM.UserExistanceResponse response = new ViewModel.WalletUserVM.UserExistanceResponse();
            var UserDetail = await _walletUserService.UserProfile(sessionToken);

            response = await _walletUserRepository.WalletVerifyOtp(request, UserDetail.WalletUserId);
            if (response.RstKey == 1)
            {
                response.Message = ResponseMessages.VALID_OTP;
                response.Status = (int)OtpStatus.VALID_OTP;
            }
            else
            {
                response.Message = ResponseMessages.INVALID_OTP;
                response.Status = (int)OtpStatus.INVALID_OTP;
            }
            return response;
        }
        public async Task<List<MobileNoListResponse>> GetMobileNoList(string sessionToken)
        {
            var UserDetail = await _walletUserService.UserProfile(sessionToken);
            return await _walletUserRepository.GetMobileNoList(UserDetail.WalletUserId);
        }



        //master card payment for third party
        public async Task<MasterCardPaymentUBAResponse> NewMasterCardPayment(ThirdpartyPaymentByCardRequest request, string headerToken)
        {
            var response = new MasterCardPaymentUBAResponse();
            var _commission = new CalculateCommissionResponse();
            var _commissionRequest = new CalculateCommissionRequest();

            var transationInitiate = new TransactionInitiateRequest();
            var _thirdPartyPaymentByCard = new ThirdPartyPaymentByCard();

            try
            {

                var UserDetail = await _walletUserService.UserProfile(headerToken);

                var Isdocverified = await _walletUserRepository.IsDocVerifiedMOMO(UserDetail.DocumetStatus);

                var resultTL = await _transactionLimitAUService.CheckTransactionLimitAU(request.WalletUserId.ToString()); //check New TL
                var transactionLimit = await _masterDataRepository.GetTransactionLimitAddMoney(Convert.ToString(UserDetail.WalletUserId));
                int limit = transactionLimit != null ? Convert.ToInt32(transactionLimit.TransactionLimitForAddMoney) : 0;

                var transactionHistory = _masterDataRepository.GetAllTransactionsAddMoney(UserDetail.WalletUserId);
                int totalAmountTransfered = transactionHistory != null ? Convert.ToInt32(transactionHistory.totalAmount) : 0;

                //var currencyDetail = _masterDataRepository.GetCurrencyRate();

                //decimal cediRate = Convert.ToDecimal(currencyDetail.CediRate);
                //decimal CfaRate = Convert.ToDecimal(currencyDetail.CfaRate);
                //decimal requestAmount = Convert.ToDecimal(request.Amount);// / dollarValue;

                //decimal currencyConversion = (requestAmount * CfaRate);// / cediRate;

                if (UserDetail.StdCode == "+229")//check benin user do txn per de
                {

                    int txnno = await _cardPaymentRepository.CheckBeninUserTxnPerde(UserDetail.WalletUserId);
                    if (txnno != 0)//
                    {
                        response.RstKey = 6;
                        response.StatusCode = (int)TransactionStatus.Failed;
                        response.Message = "You can make only one transaction per Day.";
                        return response;
                    }
                }

                #region Calculate commission on request amount               

                //var WalletService = await _masterDataRepository.GetWalletServicesByIdOrChannel(request.channel, request.ServiceCategoryId);
                int WalletServiceId = await _cardPaymentRepository.GetServiceId();
                if (UserDetail.StdCode == "+234" && UserDetail.UserType1 == 3)//merchant nigeria
                {
                    response.RstKey = 6;
                    response.StatusCode = (int)TransactionStatus.Failed;
                    response.Message = ResponseMessages.Merchant_userTYPE;
                }
                else if (UserDetail.IsActive == true)//am
                {
                    if (UserDetail.IsEmailVerified == true)
                    {
                        if (Isdocverified == true)
                        {
                            if (transactionLimit == null || transactionLimit.TransactionLimitForAddMoney == null || limit >= (Convert.ToDecimal(request.Amount) + totalAmountTransfered))
                            {
                                if (WalletServiceId > 0)
                                {
                                    #region Calculate Commission on request amount
                                    _commissionRequest.IsRoundOff = true;
                                    _commissionRequest.TransactionAmount = Convert.ToDecimal(request.Amount);
                                    _commissionRequest.WalletServiceId = WalletServiceId;
                                    _commission = await _setCommisionRepository.CalculateCommission(_commissionRequest);
                                    #endregion
                                }


                                decimal amountWithCommision = decimal.Parse(string.Format("{0:0,0}", _commission.AmountWithCommission));    // "1,234,257";
                                decimal currentBalance = Convert.ToDecimal(UserDetail.CurrentBalance);
                                #endregion
                                if (resultTL != null)
                                {
                                    decimal SetAmount = Convert.ToDecimal(resultTL.SetAmount);// returns decimal
                                    decimal TotalAmount = Convert.ToDecimal(resultTL.TotalAmount);// returns decimal

                                    if (SetAmount != 0) //0 =msg 
                                    {
                                        decimal requestAmountwithcomm = amountWithCommision + TotalAmount;

                                        if (requestAmountwithcomm <= SetAmount)//1000 >= 1000
                                        {

                                        }
                                        else
                                        {
                                            response.RstKey = 6;
                                            response.Message = ResponseMessages.INVALID_txnAmountREQUEST;
                                            return response;
                                        }
                                    }
                                }

                                var uba = await GetPaymentUrlMastrCardUBa(request.Amount, amountWithCommision.ToString(), headerToken);

                                if (uba.SuccessIndicator != null)
                                {


                                    //save selected card no. with paytranid
                                    await _cardPaymentRepository.SaveCardNo(uba.InvoiceNumber, UserDetail.WalletUserId, request.CardNo, uba.SuccessIndicator, request.Amount, amountWithCommision.ToString(), UserDetail.EmailId);

                                    response.SessionId = uba.SessionId;
                                    response.SuccessIndicator = uba.SuccessIndicator;
                                    response.Version = uba.Version;
                                    response.InvoiceNumber = uba.InvoiceNumber;
                                    response.Merchant = uba.Merchant;

                                    response.sessioanData = uba.sessioanData;
                                    //http://apiref.ezipaygh.com/HtmlTemplates/UBATestGetway.html?SessionId=SESSION0002646669248L2590941M50&Merchant=UBAEZIPAYGH&InvoiceNumber=erwewerw3
                                    //response.URL = "http://54.218.162.6:9097/HtmlTemplates/UBATestGetway.html?" + "SessionId=" + uba.SessionId + "&Merchant=UBAEZIPAYGH" + "&InvoiceNumber=" + uba.InvoiceNumber;
                                    response.URL = CommonSetting.MPGSTemplate + "SessionId=" + uba.SessionId + "&Merchant=UBAEZIPAYCDI" + "&InvoiceNumber=" + uba.InvoiceNumber;
                                    _thirdPartyPaymentByCard.Amount = _commission.TransactionAmount.ToString();
                                    _thirdPartyPaymentByCard.AmountWithCommision = amountWithCommision.ToString();
                                    _thirdPartyPaymentByCard.BenificiaryName = request.BeneficiaryName;
                                    _thirdPartyPaymentByCard.Comment = request.Comment;
                                    _thirdPartyPaymentByCard.Channel = request.channel;
                                    _thirdPartyPaymentByCard.MobileNo = request.customer;
                                    _thirdPartyPaymentByCard.ISD = request.ISD;
                                    _thirdPartyPaymentByCard.OrderNumber = uba.SuccessIndicator;
                                    _thirdPartyPaymentByCard.WalletUserId = request.WalletUserId;
                                    _thirdPartyPaymentByCard.IsActive = true;
                                    _thirdPartyPaymentByCard.IsDeleted = false;
                                    _thirdPartyPaymentByCard.CreatedDate = DateTime.UtcNow;
                                    _thirdPartyPaymentByCard.UpdatedDate = DateTime.UtcNow;
                                    _thirdPartyPaymentByCard.ServiceCategoryId = request.ServiceCategoryId;
                                    _thirdPartyPaymentByCard.SessionTokenPerTransaction = headerToken;
                                    _thirdPartyPaymentByCard.MerchantId = request.MerchantId;
                                    _thirdPartyPaymentByCard.MerchantName = request.Name;
                                    _thirdPartyPaymentByCard.Product_Id = request.Product_Id; //use productid -dth & msisdn:- int. recharge
                                    _thirdPartyPaymentByCard.AmountInLocalCountry = request.AmountInLocalCountry; //use AmountInLocalCountry -dth & msisdn:- int. recharge
                                    _thirdPartyPaymentByCard.InvoiceNumber = uba.InvoiceNumber;
                                    _thirdPartyPaymentByCard.DisplayContent = request.DisplayContent;
                                    _thirdPartyPaymentByCard.AmountInUsd = request.AmountInUsd;
                                    _thirdPartyPaymentByCard.ExtraField = string.Empty;

                                    _thirdPartyPaymentByCard = await _cardPaymentRepository.SaveThirdPartyPaymentByCard(_thirdPartyPaymentByCard);
                                    ////save data for initiate transaction 
                                    //transationInitiate.InvoiceNumber = uba.InvoiceNumber;
                                    //transationInitiate.ReceiverNumber = UserDetail.MobileNo;
                                    //transationInitiate.ServiceName = "Third party Payment by card";
                                    //transationInitiate.RequestedAmount = _commission.TransactionAmount.ToString();
                                    //transationInitiate.TransactionStatus = (int)TransactionStatus.Pending;
                                    //transationInitiate.WalletUserId = UserDetail.WalletUserId;
                                    //transationInitiate.UserReferanceNumber = uba.SuccessIndicator;
                                    //transationInitiate.CurrentBalance = UserDetail.CurrentBalance;
                                    //transationInitiate.AfterTransactionBalance = "";
                                    //transationInitiate.ReceiverCurrentBalance = UserDetail.CurrentBalance;
                                    //transationInitiate.UserName = UserDetail.FirstName + " " + UserDetail.LastName;
                                    //transationInitiate.ReceiverWalletUserId = UserDetail.WalletUserId;
                                    //transationInitiate.CreatedDate = DateTime.UtcNow;
                                    //transationInitiate.UpdatedDate = DateTime.UtcNow;
                                    //transationInitiate.IsActive = true;
                                    //transationInitiate.IsDeleted = false;
                                    //transationInitiate.JsonRequest = uba.sessioanData;
                                    //transationInitiate.JsonResponse = "";
                                    //transationInitiate = await _cardPaymentRepository.SaveTransactionInitiateRequest(transationInitiate);


                                    response.RstKey = 2;
                                }
                                else
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.Message = "Please try after some time aggregator error.";
                                }
                                LogTransactionTypes.Request.SaveTransactionLog(LogTransactionNameTypes.AddMoney + "Credit TO Debit Cards", response.sessioanData, "Request Url : " + response.sessioanData);
                            }
                            else
                            {
                                var addLimit = limit - (Convert.ToDecimal(request.Amount) + totalAmountTransfered);
                                if (addLimit < Convert.ToDecimal(request.Amount))
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.Message = "Exceed your transaction limit.";
                                }
                                else
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.Message = "You can not add more then " + transactionLimit.TransactionLimitForAddMoney.ToString() + " cedi in a day";
                                }
                            }
                        }
                        else if (UserDetail.DocumetStatus == 0 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
                        {
                            response.RstKey = 13;
                            response.Message = ResponseMessageKyc.FAILED_Doc_NotUploaded;
                        }
                        else if (UserDetail.DocumetStatus == 1 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
                        {
                            response.RstKey = 14;
                            response.Message = ResponseMessageKyc.FAILED_Doc_Pending;
                        }
                        else if (UserDetail.DocumetStatus == 4 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
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
                        response.RstKey = 6;
                        response.StatusCode = (int)TransactionStatus.Failed;
                        response.Message = ResponseMessages.EMAIL_VERIFICATION_PENDING;
                    }
                }
                else
                {
                    response.RstKey = 6;
                    response.Message = ResponseMessages.TRANSACTION_DISABLED;
                }
            }

            catch (Exception ex)
            {

                //tran.Rollback();
            }
            return response;

        }

        public async Task<MasterCardPaymentUBAResponse> GetPaymentUrlMastrCardUBa(string Amount, string totalAmount, string headerToken)
        {
            CheckoutSessionModel model = new CheckoutSessionModel();
            var _commission = new CalculateCommissionResponse();
            var _commissionRequest = new CalculateCommissionRequest();

            var response = new MasterCardPaymentUBAResponse();
            var _masterCard = new MasterCardPaymentRequest();
            var transationInitiate = new TransactionInitiateRequest();
            var UserDetail = await _walletUserService.UserProfile(headerToken);

            var transactionLimit = await _masterDataRepository.GetTransactionLimitAddMoney(Convert.ToString(UserDetail.WalletUserId));
            int limit = transactionLimit != null ? Convert.ToInt32(transactionLimit.TransactionLimitForAddMoney) : 0;

            var transactionHistory = _masterDataRepository.GetAllTransactionsAddMoney(UserDetail.WalletUserId);
            int totalAmountTransfered = transactionHistory != null ? Convert.ToInt32(transactionHistory.totalAmount) : 0;

            int WalletServiceId = await _cardPaymentRepository.GetServiceId();


            if (transactionLimit == null || limit >= (Convert.ToDecimal(Amount) + totalAmountTransfered))
            {
                if (WalletServiceId > 0)
                {
                    #region Calculate Commission on request amount
                    _commissionRequest.IsRoundOff = true;
                    _commissionRequest.TransactionAmount = Convert.ToDecimal(Amount);
                    _commissionRequest.WalletServiceId = WalletServiceId;
                    _commission = await _setCommisionRepository.CalculateCommission(_commissionRequest);
                    #endregion
                }
                string value = String.Empty;

                string username = CommonSetting.MasterCardUserName;
                string password = CommonSetting.MasterCardPassword;
                var client = new HttpClient();
                client.BaseAddress = new Uri(CommonSetting.MasterCardUrl);
                var invoiceNumber = await _masterDataRepository.GetInvoiceNumber();
                //invoiceNumber.InvoiceNumber = "XXXXXXXX0001";
                //Random rnd = new Random();  //a very common mistake

                var values = new Dictionary<string, string>()
                        {
                            {"apiOperation", "CREATE_CHECKOUT_SESSION"},
                            {"apiPassword",password},
                            {"apiUsername", CommonSetting.apiUsernameMasterCard},
                            {"merchant",CommonSetting.MerchantNameMasterCard},
                            {"interaction.operation",CommonSetting.operation},
                            {"order.amount", totalAmount},
                            {"order.id", invoiceNumber.InvoiceNumber},
                            {"order.currency", "XOF"},
                            {"interaction.returnUrl",CommonSetting.MasterCardCallBackForPayServicesNewFlow}
                        };
                var content = new FormUrlEncodedContent(values);

                var request = new HttpRequestMessage()
                {
                    // RequestUri = new Uri("devnull", UriKind.Relative),
                    Method = HttpMethod.Post,
                    Content = content
                };
                request.Headers.ExpectContinue = false;
                request.Headers.Add("custom-header", "a header value");

                var base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", base64String);
                var result = await client.SendAsync(request);
                var customerJsonString = await result.Content.ReadAsStringAsync();
                List<string> keyValuePairs = customerJsonString.Split('&').ToList();

                foreach (var keyValuePair in keyValuePairs)
                {
                    string key = keyValuePair.Split('=')[0].Trim();
                    if (key == "session.id")
                    {
                        model.Id = keyValuePair.Split('=')[1];
                    }
                    else if (key == "session.version")
                    {
                        model.Version = keyValuePair.Split('=')[1];
                    }
                    else if (key == "successIndicator")
                    {
                        model.SuccessIndicator = keyValuePair.Split('=')[1];
                    }
                    else if (key == "merchant")
                    {
                        model.merchant = keyValuePair.Split('=')[1];
                    }
                }
                _masterCard.SessionId = model.Id;
                _masterCard.Version = model.Version;
                _masterCard.SuccessIndicator = model.SuccessIndicator;
                _masterCard.Merchant = model.merchant;
                _masterCard.IsActive = true;
                _masterCard.IsDeleted = false;
                _masterCard.CreatedDate = DateTime.UtcNow;
                _masterCard.UpdatedDate = DateTime.UtcNow;
                _masterCard.Amount = Amount;
                _masterCard.CommisionCharges = _commission.CommisionPercent;
                _masterCard.TotalAmount = totalAmount;
                _masterCard.WalletUserId = UserDetail.WalletUserId;
                _masterCard.TransactionNo = invoiceNumber.InvoiceNumber;
                _masterCard.FlatCharges = _commission.FlatCharges;
                _masterCard.BenchmarkCharges = _commission.BenchmarkCharges;
                _masterCard.CommissionAmount = _commission.CommissionAmount.ToString();
                await _cardPaymentRepository.SaveMasterCardPaymentRequest(_masterCard);


                response.SessionId = model.Id;
                response.SuccessIndicator = model.SuccessIndicator;
                response.Version = model.Version;
                response.InvoiceNumber = invoiceNumber.InvoiceNumber;
                response.Merchant = model.merchant;
                response.RstKey = 1;
                response.sessioanData = customerJsonString;
                if (response.sessioanData != null)
                {
                    response.RstKey = 1;

                    transationInitiate.InvoiceNumber = invoiceNumber.InvoiceNumber;
                    transationInitiate.ReceiverNumber = UserDetail.MobileNo;
                    transationInitiate.ServiceName = "Card Payment";
                    transationInitiate.RequestedAmount = Amount;
                    transationInitiate.TransactionStatus = (int)TransactionStatus.Pending;
                    transationInitiate.WalletUserId = UserDetail.WalletUserId;
                    transationInitiate.UserReferanceNumber = response.SuccessIndicator;
                    transationInitiate.CurrentBalance = UserDetail.CurrentBalance;
                    transationInitiate.AfterTransactionBalance = "";
                    transationInitiate.ReceiverCurrentBalance = UserDetail.CurrentBalance;
                    transationInitiate.UserName = UserDetail.FirstName + " " + UserDetail.LastName;
                    transationInitiate.ReceiverWalletUserId = UserDetail.WalletUserId;
                    transationInitiate.CreatedDate = DateTime.UtcNow;
                    transationInitiate.UpdatedDate = DateTime.UtcNow;
                    transationInitiate.IsActive = true;
                    transationInitiate.IsDeleted = false;
                    transationInitiate.JsonRequest = customerJsonString;
                    transationInitiate.JsonResponse = "";
                    transationInitiate = await _cardPaymentRepository.SaveTransactionInitiateRequest(transationInitiate);
                }
                LogTransactionTypes.Request.SaveTransactionLog(LogTransactionNameTypes.AddMoney + "Credit TO Debit Cards master card", customerJsonString, "Request Url : " + customerJsonString);
            }
            else
            {
                var addLimit = limit - (Convert.ToDecimal(Amount) + totalAmountTransfered);
                if (addLimit < Convert.ToDecimal(Amount))
                {
                    response.RstKey = 6;
                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                    response.Message = "Exceed your transaction limit.";
                }
                else
                {
                    response.RstKey = 6;
                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                    response.Message = "You can not add more then " + transactionLimit.TransactionLimitForAddMoney.ToString() + " cedi in a day";
                }
            }

            return response;
        }


        public async Task<AddMoneyAggregatorResponse> SaveMasterCardPaymentResponse(MasterCardPaymentResponse request)
        {
            AddMoneyAggregatorResponse response = new AddMoneyAggregatorResponse();
            CalculateCommissionResponse _commission = new CalculateCommissionResponse();
            CalculateCommissionRequest _commissionRequest = new CalculateCommissionRequest();
            string RequestString = JsonConvert.SerializeObject(request);
            LogTransactionTypes.Response.SaveTransactionLog(LogTransactionNameTypes.AddMoney + "Credit to Debit Cards", RequestString, "Responce by bank detail : ");

            try
            {
                var requestDetail = await _cardPaymentRepository.GetMasterCardPaymentRequest(request.resultIndicator);
                var getInitialTransaction = await _cardPaymentRepository.GetTransactionInitiateRequest(requestDetail.TransactionNo);

                int GetWalletTransactionsexist = await _cardPaymentRepository.GetWalletTransactionsexist(requestDetail.WalletUserId, requestDetail.TransactionNo);

                if (requestDetail != null && GetWalletTransactionsexist == 0)
                {

                    getInitialTransaction.JsonResponse = JsonConvert.SerializeObject(request);

                    if (request.resultIndicator != null && request.sessionVersion != null)
                    {
                        response.InvoiceNo = requestDetail.TransactionNo;
                        response.Amount = getInitialTransaction.RequestedAmount;
                        response.status = "MPGS";
                        DateTime TDate = DateTime.UtcNow;
                        response.TransactionDate = TDate;

                        int WalletServiceId = await _cardPaymentRepository.GetServiceId();
                        if (WalletServiceId > 0)
                        {
                            var adminUser = await _cardPaymentRepository.GetAdminUser();
                            if (adminUser != null)
                            {

                                // var receiver = await _walletUserRepository.GetCurrentUser(Convert.ToInt32(requestDetail.WalletUserId)); //db.WalletUsers.Where(x => x.WalletUserId == requestDetail.WalletUserId).FirstOrDefault();

                                long userId = Convert.ToInt32(requestDetail.WalletUserId);
                                var UserCurrentDetail = await _walletUserRepository.GetCurrentUser(userId);
                                if (UserCurrentDetail != null && await _cardPaymentRepository.IsWalletTransactions(UserCurrentDetail.WalletUserId, requestDetail.TransactionNo))
                                {
                                    //this line commented due to currentbalance is not added to card expected 
                                    //request.vpc_Amount = Convert.ToString(Math.Round((Convert.ToDecimal(requestDetail.Amount) / 100), 2));
                                    requestDetail.Amount = Convert.ToString(Math.Round(Convert.ToDecimal(requestDetail.Amount), 2));

                                    // to update wallet amount-----

                                    // db.WalletUsers.FirstOrDefault(x => x.WalletUserId == receiver.WalletUserId);

                                    if (UserCurrentDetail != null)
                                    {
                                        _commissionRequest.CurrentBalance = Convert.ToDecimal(UserCurrentDetail.CurrentBalance);
                                        _commissionRequest.IsRoundOff = true;
                                        //_commissionRequest.TransactionAmount = Convert.ToDecimal(request.vpc_Amount);

                                        _commissionRequest.TransactionAmount = Convert.ToDecimal(requestDetail.Amount); //change
                                        _commissionRequest.WalletServiceId = WalletServiceId;
                                        _commission = await _setCommisionRepository.CalculateCommission(_commissionRequest);

                                        if (!string.IsNullOrEmpty(request.resultIndicator))
                                        {
                                            getInitialTransaction.TransactionStatus = (int)TransactionStatus.Completed;
                                            if (Convert.ToDecimal(UserCurrentDetail.CurrentBalance) >= 0)
                                            {
                                                if (Convert.ToDecimal(UserCurrentDetail.CurrentBalance) == 0)
                                                {
                                                    UserCurrentDetail.CurrentBalance = _commission.TransactionAmount.ToString();
                                                    getInitialTransaction.AfterTransactionBalance = UserCurrentDetail.CurrentBalance.ToString();
                                                    getInitialTransaction.ReceiverCurrentBalance = UserCurrentDetail.CurrentBalance.ToString();
                                                }
                                                else
                                                {
                                                    UserCurrentDetail.CurrentBalance = Math.Round(Convert.ToDecimal(UserCurrentDetail.CurrentBalance) + _commission.TransactionAmount, 2).ToString();
                                                    getInitialTransaction.AfterTransactionBalance = UserCurrentDetail.CurrentBalance.ToString();
                                                    getInitialTransaction.ReceiverCurrentBalance = UserCurrentDetail.CurrentBalance.ToString();
                                                }
                                            }
                                            else
                                            {
                                                UserCurrentDetail.CurrentBalance = Math.Round(_commission.TransactionAmount - Convert.ToDecimal(UserCurrentDetail.CurrentBalance), 2).ToString();
                                                getInitialTransaction.AfterTransactionBalance = UserCurrentDetail.CurrentBalance.ToString();
                                                getInitialTransaction.ReceiverCurrentBalance = UserCurrentDetail.CurrentBalance.ToString();
                                            }
                                        }
                                        await _walletUserRepository.UpdateUserDetail(UserCurrentDetail);
                                        await _cardPaymentRepository.UpdateTransactionInitiateRequest(getInitialTransaction);
                                        // db.SaveChanges();
                                    }

                                    #region Save Transaction
                                    decimal amountWithCommision = decimal.Parse(string.Format("{0:0,0}", _commission.AmountWithCommission));    // "1,234,257";

                                    var _Transaction = new WalletTransaction();

                                    _Transaction.TransactionInitiateRequestId = getInitialTransaction.Id;
                                    _Transaction.MerchantCommissionAmount = Convert.ToString(_commission.MerchantCommissionAmount);
                                    _Transaction.MerchantCommissionId = _commission.MerchantCommissionId;
                                    _Transaction.TransactionType = AggragatorServiceType.CREDIT;
                                    _Transaction.IsBankTransaction = false;
                                    _Transaction.TransactionTypeInfo = (int)TransactionTypeInfo.AddedByCard;
                                    _Transaction.IsBankTransaction = false;
                                    _Transaction.Comments = string.Empty;
                                    _Transaction.InvoiceNo = getInitialTransaction.InvoiceNumber;
                                    _Transaction.CommisionAmount = Convert.ToString(_commission.CommissionAmount);
                                    _Transaction.CommisionId = _commission.CommissionId;
                                    _Transaction.WalletAmount = Convert.ToString(_commission.TransactionAmount);

                                    _Transaction.TotalAmount = Convert.ToString(amountWithCommision);
                                    _Transaction.ServiceTaxRate = _commission.ServiceTaxRate;
                                    _Transaction.ServiceTax = Convert.ToString(_commission.ServiceTaxAmount);
                                    _Transaction.OperatorType = "MPGS";

                                    _Transaction.AccountNo = getInitialTransaction.ReceiverNumber;
                                    if (!string.IsNullOrEmpty(request.resultIndicator))
                                    {

                                        _Transaction.TransactionStatus = (int)TransactionStatus.Completed; ;
                                        try
                                        {
                                            //--------send mail on success transaction--------

                                            var AdminKeys = AES256.AdminKeyPair;
                                            string FirstName = AES256.Decrypt(UserCurrentDetail.PrivateKey, UserCurrentDetail.FirstName);
                                            string LastName = AES256.Decrypt(UserCurrentDetail.PrivateKey, UserCurrentDetail.LastName);
                                            string StdCode = UserCurrentDetail.StdCode;
                                            string MobileNo = AES256.Decrypt(AdminKeys.PrivateKey, UserCurrentDetail.MobileNo);
                                            string EmailId = AES256.Decrypt(AdminKeys.PrivateKey, UserCurrentDetail.EmailId).Trim().ToLower();
                                            // var receiverDetail = new AppUserRepository().GetUserDetailById(receiver.WalletUserId);
                                            string filename = CommonSetting.successfullTransaction;


                                            var body = _sendEmails.ReadEmailformats(filename);
                                            body = body.Replace("$$FirstName$$", FirstName + " " + LastName);
                                            body = body.Replace("$$DisplayContent$$", "VISA CARDS/MASTER CARD");
                                            body = body.Replace("$$customer$$", MobileNo);
                                            body = body.Replace("$$amount$$", "XOF " + requestDetail.Amount);
                                            body = body.Replace("$$ServiceTaxAmount$$", "XOF " + _commission.CommissionAmount);
                                            body = body.Replace("$$AmountWithCommission$$", "XOF " + amountWithCommision);
                                            body = body.Replace("$$TransactionId$$", Convert.ToString(requestDetail.TransactionNo));

                                            var req = new EmailModel()
                                            {
                                                TO = EmailId,
                                                Subject = "Transaction Successfull",
                                                Body = body
                                            };
                                            _sendEmails.SendEmail(req);
                                        }
                                        catch
                                        {

                                        }
                                    }
                                    else
                                    {
                                        _Transaction.TransactionStatus = (int)TransactionStatus.Failed;
                                    }
                                    _Transaction.WalletServiceId = WalletServiceId;
                                    _Transaction.SenderId = UserCurrentDetail.WalletUserId;
                                    _Transaction.ReceiverId = UserCurrentDetail.WalletUserId;
                                    _Transaction.BankBranchCode = string.Empty;
                                    _Transaction.BankTransactionId = request.resultIndicator;
                                    _Transaction.TransactionId = requestDetail.TransactionNo;


                                    _Transaction.IsAdminTransaction = false;
                                    _Transaction.IsActive = true;
                                    _Transaction.IsDeleted = false;
                                    _Transaction.CreatedDate = TDate;
                                    _Transaction.UpdatedDate = TDate;
                                    _Transaction.TransactionTypeInfo = (int)TransactionTypeInfo.AddedByCard;
                                    _Transaction.IsAddDuringPay = false;
                                    _Transaction.VoucherCode = string.Empty;

                                    await _cardPaymentRepository.SaveWalletTransactions(_Transaction);
                                    //db.WalletTransactions.Add(_Transaction);
                                    //db.SaveChanges();
                                    #endregion

                                    #region Credit
                                    var _credit = new WalletTransactionDetail();
                                    _credit.Amount = Convert.ToString(_commission.TransactionAmount);
                                    _credit.TransactionType = (int)TransactionDetailType.Credit;
                                    _credit.WalletUserId = adminUser.WalletUserId;
                                    _credit.WalletTransactionId = _Transaction.WalletTransactionId;
                                    _credit.IsActive = true;
                                    _credit.IsDeleted = false;
                                    _credit.CreatedDate = TDate;
                                    _credit.UpdatedDate = TDate;
                                    //db.WalletTransactionDetails.Add(_credit);
                                    //db.SaveChanges();
                                    await _cardPaymentRepository.SaveWalletTransactionDetails(_credit);
                                    #endregion

                                    #region Debit
                                    var _debit = new WalletTransactionDetail();
                                    _debit.Amount = Convert.ToString(_commission.TransactionAmount);
                                    _debit.TransactionType = (int)TransactionDetailType.Debit;
                                    _debit.WalletUserId = UserCurrentDetail.WalletUserId;
                                    _debit.WalletTransactionId = _Transaction.WalletTransactionId;
                                    _debit.IsActive = true;
                                    _debit.IsDeleted = false;
                                    _debit.CreatedDate = TDate;
                                    _debit.UpdatedDate = TDate;
                                    //db.WalletTransactionDetails.Add(_credit);
                                    //db.SaveChanges();
                                    await _cardPaymentRepository.SaveWalletTransactionDetails(_credit);
                                    #endregion

                                    //get UpdateNewCardNoResponseBankCode id
                                    await _cardPaymentRepository.UpdateNewCardNoResponseBankCode(getInitialTransaction.InvoiceNumber, UserCurrentDetail.WalletUserId, request.resultIndicator);


                                    var adminKeyPair = AES256.AdminKeyPair;


                                    //db.SaveChanges();
                                    //tran.Commit();
                                    #region PushNotification

                                    var CurrentUser = await _walletUserRepository.GetCurrentUser(UserCurrentDetail.WalletUserId);//db.WalletUsers.Where(x => x.WalletUserId == receiver.WalletUserId).FirstOrDefault();
                                    if (CurrentUser != null)
                                    {
                                        PushNotificationModel push = new PushNotificationModel();
                                        push.SenderId = UserCurrentDetail.WalletUserId;
                                        push.deviceType = (int)UserCurrentDetail.DeviceType;
                                        push.deviceKey = UserCurrentDetail.DeviceToken;
                                        PayMoneyPushModel pushModel = new PayMoneyPushModel();
                                        pushModel.TransactionDate = TDate;
                                        pushModel.TransactionId = requestDetail.TransactionNo;
                                        pushModel.CurrentBalance = CurrentUser.CurrentBalance;
                                        pushModel.alert = _Transaction.WalletAmount + " XOF has been credited to your account.";
                                        pushModel.Amount = getInitialTransaction.RequestedAmount;
                                        pushModel.CurrentBalance = CurrentUser.CurrentBalance;
                                        pushModel.pushType = (int)PushType.ADDMONEY;

                                        if ((int)UserCurrentDetail.DeviceType == (int)DeviceTypes.ANDROID || (int)UserCurrentDetail.DeviceType == (int)DeviceTypes.Web)
                                        {
                                            PushPayload<PayMoneyPushModel> aps = new PushPayload<PayMoneyPushModel>();
                                            PushPayloadData<PayMoneyPushModel> _data = new PushPayloadData<PayMoneyPushModel>();
                                            _data.notification = pushModel;
                                            aps.data = _data;
                                            aps.to = UserCurrentDetail.DeviceToken;
                                            aps.collapse_key = string.Empty;
                                            push.message = JsonConvert.SerializeObject(aps);
                                            push.payload = pushModel;
                                        }
                                        if ((int)UserCurrentDetail.DeviceType == (int)DeviceTypes.IOS)
                                        {
                                            NotificationJsonResponse<PayMoneyIOSPushModel> aps = new NotificationJsonResponse<PayMoneyIOSPushModel>();
                                            PayMoneyIOSPushModel _iosPushModel = new PayMoneyIOSPushModel();
                                            _iosPushModel.alert = pushModel.alert;
                                            _iosPushModel.Amount = pushModel.Amount;
                                            _iosPushModel.CurrentBalance = pushModel.CurrentBalance;
                                            _iosPushModel.MobileNo = pushModel.MobileNo;
                                            _iosPushModel.SenderName = pushModel.SenderName;
                                            _iosPushModel.pushType = pushModel.pushType;
                                            aps.aps = _iosPushModel;

                                            push.message = JsonConvert.SerializeObject(aps);
                                        }
                                        //if (!string.IsNullOrEmpty(push.message))
                                        //{
                                        //    new PushNotificationRepository().sendPushNotification(push);
                                        //}
                                    }
                                    #endregion

                                    if (UserCurrentDetail.MobileNo != null && UserCurrentDetail.MobileNo != "")
                                    {
                                        response.ToMobileNo = AES256.Decrypt(adminKeyPair.PrivateKey, UserCurrentDetail.MobileNo);
                                    }


                                    response.CurrentBalance = UserCurrentDetail.CurrentBalance;
                                    response.RstKey = 1;

                                    // response.ToMobileNo = receiver.StdCode + receiver.MobileNo;

                                    //var IsAddduringPay = await _cardPaymentRepository.AddDuringPayRecords(request.vpc_OrderInfo, request.vpc_MerchTxnRef);//db.AddDuringPayRecords.Where(x => x.OrderNo == request.vpc_OrderInfo && x.TransactionNo == request.vpc_MerchTxnRef && x.TransactionStatus == (int)TransactionStatus.Pending).FirstOrDefault();
                                    //if (IsAddduringPay != null)
                                    //{
                                    //    #region PayMoneyAfterAdd

                                    //    //var storeddata = db.AddDuringPayRecords.Where(x => x.OrderNo == request.vpc_OrderInfo && x.TransactionNo == request.vpc_MerchTxnRef && x.TransactionStatus == (int)TransactionStatus.Pending).Select(x => new PayMoneyAggregatoryRequest
                                    //    //{
                                    //    //    Amount = x.amount,
                                    //    //    channel = x.channel,
                                    //    //    chennelId = x.chennelId ?? 0,
                                    //    //    Comment = x.Comment,
                                    //    //    customer = x.customer,
                                    //    //    invoiceNo = x.invoiceNo,
                                    //    //    IsAddDuringPay = x.IsAddDuringPay ?? false,
                                    //    //    ISD = x.ISD,
                                    //    //    serviceCategory = x.serviceCategory,
                                    //    //    ServiceCategoryId = x.ServiceCategoryId ?? 0,
                                    //    //    IsMerchant = x.IsMerchant ?? false,
                                    //    //    MerchantId = x.MerchantId ?? 0
                                    //    //}).FirstOrDefault();
                                    //    //var storeddata = await _cardPaymentRepository.AddDuringPayRecord(request.vpc_OrderInfo, request.vpc_MerchTxnRef);
                                    //    //if (storeddata != null)
                                    //    //{
                                    //    //    PayMoneyAggregatoryRequest _record = new PayMoneyAggregatoryRequest();
                                    //    //    _record.Amount = storeddata.Amount;
                                    //    //    _record.channel = storeddata.channel;
                                    //    //    _record.chennelId = storeddata.chennelId;
                                    //    //    _record.Comment = storeddata.Comment;
                                    //    //    _record.customer = storeddata.customer;
                                    //    //    _record.invoiceNo = storeddata.invoiceNo;
                                    //    //    _record.IsAddDuringPay = storeddata.IsAddDuringPay;
                                    //    //    _record.ISD = storeddata.ISD;
                                    //    //    _record.serviceCategory = storeddata.serviceCategory;
                                    //    //    _record.ServiceCategoryId = storeddata.ServiceCategoryId;
                                    //    //    _record.IsMerchant = storeddata.IsMerchant;
                                    //    //    _record.MerchantId = storeddata.MerchantId;
                                    //    //    if (_record != null)
                                    //    //    {
                                    //    //        AddDuringPayResponse payrespone = new AddDuringPayResponse();
                                    //    //        if (_record.IsAddDuringPay && _record.IsMerchant && _record.MerchantId > 0)
                                    //    //        {
                                    //    //            // WalletTransactionRepository obj = new WalletTransactionRepository();
                                    //    //            MerchantTransactionRequest merchantRequest = new MerchantTransactionRequest();
                                    //    //            merchantRequest.Amount = _record.Amount;
                                    //    //            merchantRequest.Comment = _record.Comment;
                                    //    //            merchantRequest.MerchantId = _record.MerchantId;
                                    //    //            var merchantResponse = await _merchantPaymentService.MerchantPayment(merchantRequest, null, UserCurrentDetail.WalletUserId);////pass null it never entry here -token for userprofile
                                    //    //            if (merchantResponse.StatusCode == (int)TransactionStatus.Completed)
                                    //    //            {
                                    //    //                IsAddduringPay.TransactionStatus = (int)TransactionStatus.Completed;
                                    //    //                await _cardPaymentRepository.UpdateAddDuringPayRecord(IsAddduringPay);
                                    //    //                //db.SaveChanges();
                                    //    //            }
                                    //    //            payrespone.Amount = merchantResponse.TransactionAmount;
                                    //    //            payrespone.CurrentBalance = merchantResponse.ToMobileNo;
                                    //    //            payrespone.AccountNo = merchantResponse.ToMobileNo;
                                    //    //            payrespone.MobileNo = merchantResponse.ToMobileNo;
                                    //    //            payrespone.MerchantStatusCode = merchantResponse.StatusCode;
                                    //    //            payrespone.StatusCode = Convert.ToString(merchantResponse.StatusCode);
                                    //    //            payrespone.TransactionDate = merchantResponse.TransactionDate;
                                    //    //            payrespone.TransactionId = Convert.ToString(merchantResponse.TransactionId);
                                    //    //            payrespone.IsMerchant = true;
                                    //    //        }
                                    //    //        else
                                    //    //        {
                                    //    //            // PayServiesRepository obj = new PayServiesRepository();
                                    //    //            var aggregatorResponse = await _mobileMoneyServices.MobileMoneyService(_record, UserCurrentDetail.WalletUserId);
                                    //    //            if (aggregatorResponse.RstKey == 1)
                                    //    //            {
                                    //    //                IsAddduringPay.TransactionStatus = (int)TransactionStatus.Completed;
                                    //    //                await _cardPaymentRepository.UpdateAddDuringPayRecord(IsAddduringPay);
                                    //    //                // db.SaveChanges();
                                    //    //            }
                                    //    //            payrespone.Amount = aggregatorResponse.Amount;
                                    //    //            payrespone.CurrentBalance = aggregatorResponse.CurrentBalance; ;
                                    //    //            payrespone.AccountNo = aggregatorResponse.AccountNo;
                                    //    //            payrespone.MobileNo = aggregatorResponse.MobileNo;
                                    //    //            payrespone.StatusCode = Convert.ToString(aggregatorResponse.StatusCode);
                                    //    //            payrespone.TransactionDate = aggregatorResponse.TransactionDate;
                                    //    //            payrespone.TransactionId = Convert.ToString(aggregatorResponse.TransactionId);
                                    //    //            payrespone.IsMerchant = false;
                                    //    //        }
                                    //    //        response.IsAddDuringPay = true;
                                    //    //        response.AddDuringPayResponse = payrespone;
                                    //    //    }
                                    //    //}
                                    //    //else
                                    //    //{
                                    //    //    response.AddDuringPayResponse = new AddDuringPayResponse();
                                    //    //}
                                    //    #endregion
                                    //}
                                    //else
                                    //{
                                    //    response.AddDuringPayResponse = new AddDuringPayResponse();
                                    //}


                                    ///
                                    await _masterDataService.Chargeback(UserCurrentDetail.WalletUserId);

                                    //check same card or not in txn -cokmmnet by sumit sir on 29/09 and api update
                                    //if (UserCurrentDetail.StdCode != "+234")
                                    //{
                                    //    var cardNoresponseData = await GethashorUrl(getInitialTransaction.InvoiceNumber, null, "mcbcardverify");
                                    //    JavaScriptSerializer js = new JavaScriptSerializer();
                                    //    dynamic cardNoObject = js.Deserialize<dynamic>(cardNoresponseData);
                                    //    var cardNumber = cardNoObject["sourceOfFunds"]["provided"]["card"]["number"];
                                    //    //var cardNo = cardNumber;
                                    //    var usercardinfo =  await _cardPaymentRepository.GetMasterCarduseinaddmoneyPaymentRequest(UserCurrentDetail.WalletUserId, getInitialTransaction.InvoiceNumber);
                                    //    var userinfo = await _walletUserRepository.GetUserDetailById(UserCurrentDetail.WalletUserId);
                                    //    _logUtils.WriteTextToFileForCardNouseinaddmoneLogs("CardNouseinaddmone :- saveCardNo :-" + usercardinfo.CardNo.ToLower() + ";cardNumber : " + cardNumber + ";walletuserid : " + UserCurrentDetail.WalletUserId + ";Email : " + userinfo.EmailId);
                                    //    //if card is not same then block user email to admin & user and onli user block
                                    //    if (cardNumber != usercardinfo.CardNo.ToLower() && UserCurrentDetail.UserType == 1)
                                    //    {

                                    //        UserManageRequest request2 = new UserManageRequest();
                                    //        request2.UserId = UserCurrentDetail.WalletUserId;
                                    //        request2.IsActive = false;
                                    //        request2.Status = 1;
                                    //        request2.AdminId = 141;
                                    //        request2.Comment = "disable user bcz of not using same card in txn : " + getInitialTransaction.InvoiceNumber + ";User Email : " + userinfo.EmailId + ";saveCardNo :-" + usercardinfo.CardNo.ToLower() + "; cardNumber: " + cardNumber;
                                    //        await _userApiService.EnableDisableUser(request2);                                          


                                    //    }
                                    //}
                                }
                                else
                                {
                                    //test

                                }
                            }
                            else
                            {
                                //test

                            }
                            //sdfsdfd
                        }
                        else
                        {

                            response.RstKey = 2;
                        }
                    }
                    else
                    {
                        await _cardPaymentRepository.UpdateTransactionInitiateRequest(getInitialTransaction);
                        response.RstKey = 3;

                        //response.TransactionResponseDescription = _req.vpc_ResponseCodeDescription;
                        //response.TransactionResponseCode = request.vpc_TxnResponseCode;
                    }



                }
                else
                {
                    response.RstKey = 3;
                }

            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("CardPaymentRepository", "SaveMasterCardPaymentResponse", request);
            }
            return response;
        }


        public async Task<NgeniunsResponse> GetngeniusCardPaymentUrl(ThirdpartyPaymentByCardRequest request, string headerToken)
        {
            var response = new NgeniunsResponse();
            var _commission = new CalculateCommissionResponse();
            var _commissionRequest = new CalculateCommissionRequest();

            var transationInitiate = new TransactionInitiateRequest();
            var _thirdPartyPaymentByCard = new ThirdPartyPaymentByCard();

            try
            {

                var UserDetail = await _walletUserService.UserProfile(headerToken);

                var Isdocverified = await _walletUserRepository.IsDocVerifiedMOMO(UserDetail.DocumetStatus);

                var resultTL = await _transactionLimitAUService.CheckTransactionLimitAU(request.WalletUserId.ToString()); //check New TL
                var transactionLimit = await _masterDataRepository.GetTransactionLimitAddMoney(Convert.ToString(UserDetail.WalletUserId));
                int limit = transactionLimit != null ? Convert.ToInt32(transactionLimit.TransactionLimitForAddMoney) : 0;

                var transactionHistory = _masterDataRepository.GetAllTransactionsAddMoney(UserDetail.WalletUserId);
                int totalAmountTransfered = transactionHistory != null ? Convert.ToInt32(transactionHistory.totalAmount) : 0;


                //------Get Currency Rate--------------
                var currencyDetail = _masterDataRepository.GetCurrencyRate();

                //decimal cediRate = Convert.ToDecimal(currencyDetail.CediRate);
                decimal UsdRate = Convert.ToDecimal(currencyDetail.CediRate);//here we take 2 value of //Add Doller Rate
                //decimal CfaRate = Convert.ToDecimal(currencyDetail.LERate);
                decimal requestAmount = Convert.ToDecimal(request.Amount);// / dollarValue;

                #region Calculate commission on request amount               


                int WalletServiceId = await _cardPaymentRepository.GetServiceId();
                if (UserDetail.IsActive == true)//am
                {
                    if (UserDetail.IsEmailVerified == true)
                    {
                        if (Isdocverified == true)
                        {
                            if (transactionLimit == null || transactionLimit.TransactionLimitForAddMoney == null || limit >= (Convert.ToDecimal(request.Amount) + totalAmountTransfered))
                            {
                                if (WalletServiceId > 0)
                                {
                                    #region Calculate Commission on request amount
                                    _commissionRequest.IsRoundOff = true;
                                    _commissionRequest.TransactionAmount = Convert.ToDecimal(request.Amount);
                                    _commissionRequest.WalletServiceId = WalletServiceId;
                                    _commission = await _setCommisionRepository.CalculateCommission(_commissionRequest);
                                    #endregion
                                }

                                // var res = Convert.ToDecimal(_commission.TransactionAmount * cediRate);
                                //var cardAmount = Decimal.Parse(res.ToString("0.000"));
                                //decimal reqAmt = Convert.ToDecimal(_commission.TransactionAmount / cediRate);

                                decimal amt = (_commission.AmountWithCommission * UsdRate);
                                var finalAmt = Decimal.Parse(amt.ToString("0.00"));
                                var finalAmt1 = decimal.Parse(string.Format("{0:0,0}", finalAmt));    // "1,234,257";

                                CardPaymentRequest _cardRequest = new CardPaymentRequest();
                                _cardRequest.WalletUserId = UserDetail.WalletUserId;
                                _cardRequest.TotalAmount = _commission.AmountWithCommission.ToString();
                                _cardRequest.CommissionAmount = Convert.ToString(_commission.CommissionAmount);
                                _cardRequest.Amount = _commission.TransactionAmount.ToString();// Convert.ToString(requestAmo);
                                _cardRequest.FlatCharges = _commission.FlatCharges;
                                _cardRequest.BenchmarkCharges = _commission.BenchmarkCharges;
                                _cardRequest.CommisionCharges = _commission.CommisionPercent;
                                _cardRequest.CreatedDate = DateTime.UtcNow;
                                _cardRequest.UpdatedDate = DateTime.UtcNow;
                                _cardRequest.AmountInCedi = finalAmt1.ToString();
                                _cardRequest = await _cardPaymentRepository.SaveCardPaymentRequest(_cardRequest);


                                // UBA bank data
                                var finalAmt2 = finalAmt1.ToString() + "00";    // "1,234,257";

                                #endregion
                                if (resultTL != null)
                                {
                                    decimal SetAmount = Convert.ToDecimal(resultTL.SetAmount);// returns decimal
                                    decimal TotalAmount = Convert.ToDecimal(resultTL.TotalAmount);// returns decimal

                                    if (SetAmount != 0) //0 =msg 
                                    {
                                        decimal requestAmountwithcomm = _commission.AmountWithCommission + TotalAmount;

                                        if (requestAmountwithcomm <= SetAmount)//1000 >= 1000
                                        {

                                        }
                                        else
                                        {
                                            response.RstKey = 6;
                                            response.message = ResponseMessages.INVALID_txnAmountREQUEST;
                                            return response;
                                        }
                                    }
                                }

                                //first we have call api encrypt/keys to get beasrfer token

                                var responseData = await GetNgeniunsauthTokenbykey();
                                // { "status":"SUCCESS","data":{ "code":"00","EncryptedSecKey":{ "encryptedKey":"kZaA6N08aiZHnyGa9VTd4ueTkfMUoR7wMk5wjlBuA9vUHbRhseG5gC1Wc1ULWXzpIl0JmSvokIo36dPHISDwD1az6gh8myv2LseitduE3FxVEqzdQWgXdOj/3ZwdNsBO"},"message":"Successful"} }
                                var _responseModel = JsonConvert.DeserializeObject<NgeniunstokenResponse>(responseData);



                                if (_responseModel.access_token != null)
                                {
                                    ////here to get hash of request --
                                    ///
                                    var invoiceNumber = await _masterDataRepository.GetInvoiceNumber();
                                    var _Requestamount = new amount();
                                    _Requestamount.currencyCode = "USD";
                                    _Requestamount.value = Convert.ToString(finalAmt2);
                                    var _RequestmerchantAttributes = new merchantAttributes();
                                    _RequestmerchantAttributes.cancelUrl = CommonSetting.Ngeniunscancel_url;
                                    _RequestmerchantAttributes.merchantOrderReference = invoiceNumber.InvoiceNumber;
                                    _RequestmerchantAttributes.redirectUrl = CommonSetting.NgeniunsCallBack;
                                    _RequestmerchantAttributes.skip3DS = false;
                                    _RequestmerchantAttributes.skipConfirmationPage = true;
                                    var _Request = new NgeniunsRequest();

                                    _Request.action = "PURCHASE";
                                    _Request.emailAddress = UserDetail.EmailId;
                                    _Request.amount = _Requestamount;
                                    _Request.merchantAttributes = _RequestmerchantAttributes;


                                    var req = JsonConvert.SerializeObject(_Request);

                                    JavaScriptSerializer js = new JavaScriptSerializer();

                                    //here to get hash
                                    //var responseData1 = await GethashorUrl(req, _responseModel.data.EncryptedSecKey.encryptedKey, "hash");
                                    //dynamic blogObject = js.Deserialize<dynamic>(responseData1);
                                    ////object hash = blogObject["data"];
                                    ////object hash1 = blogObject["data"]["hash"];
                                    //object hash2 = blogObject["data"]["hash"]["hash"];

                                    //if (hash2 != null)
                                    //{
                                    //here to get psaymenturl



                                    var responseData2 = await GethashorUrl(req, _responseModel.access_token, "NgeniunspaymentsUrl");
                                    var _responseModel2 = JsonConvert.DeserializeObject<NgeniunsResponse>(responseData2);
                                    if (_responseModel2._links.payment.href != null)
                                    {
                                        transationInitiate.InvoiceNumber = invoiceNumber.InvoiceNumber;
                                        transationInitiate.ReceiverNumber = UserDetail.MobileNo;
                                        transationInitiate.ServiceName = "Ngeniuns Card Payment";
                                        transationInitiate.RequestedAmount = _commission.TransactionAmount.ToString();
                                        transationInitiate.TransactionStatus = (int)TransactionStatus.Pending;
                                        transationInitiate.WalletUserId = UserDetail.WalletUserId;
                                        transationInitiate.UserReferanceNumber = _responseModel2.reference;
                                        transationInitiate.CurrentBalance = UserDetail.CurrentBalance;
                                        transationInitiate.AfterTransactionBalance = "";
                                        transationInitiate.ReceiverCurrentBalance = UserDetail.CurrentBalance;
                                        transationInitiate.UserName = UserDetail.FirstName + " " + UserDetail.LastName;
                                        transationInitiate.ReceiverWalletUserId = UserDetail.WalletUserId;
                                        transationInitiate.CreatedDate = DateTime.UtcNow;
                                        transationInitiate.UpdatedDate = DateTime.UtcNow;
                                        transationInitiate.IsActive = true;
                                        transationInitiate.IsDeleted = false;
                                        transationInitiate.JsonRequest = responseData2;
                                        transationInitiate.JsonResponse = "";
                                        transationInitiate = await _cardPaymentRepository.SaveTransactionInitiateRequest(transationInitiate);
                                        response.URL = _responseModel2._links.payment.href;
                                        response.RstKey = 2;
                                    }
                                    else
                                    {
                                        response.RstKey = 6;
                                        response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                        response.message = "Please try after some time aggregator error.";
                                    }


                                }
                                else
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.message = "Please try after some time aggregator error.";
                                }
                                //LogTransactionTypes.Request.SaveTransactionLog(LogTransactionNameTypes.AddMoney + "Credit TO Debit Cards", response.sessioanData, "Request Url : " + response.sessioanData);
                            }
                            else
                            {
                                var addLimit = limit - (Convert.ToDecimal(request.Amount) + totalAmountTransfered);
                                if (addLimit < Convert.ToDecimal(request.Amount))
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.message = "Exceed your transaction limit.";
                                }
                                else
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.message = "You can not add more then " + transactionLimit.TransactionLimitForAddMoney.ToString() + " cedi in a day";
                                }
                            }
                        }
                        else if (UserDetail.DocumetStatus == 0 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
                        {
                            response.RstKey = 13;
                            response.message = ResponseMessageKyc.FAILED_Doc_NotUploaded;
                        }
                        else if (UserDetail.DocumetStatus == 1 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
                        {
                            response.RstKey = 14;
                            response.message = ResponseMessageKyc.FAILED_Doc_Pending;
                        }
                        else if (UserDetail.DocumetStatus == 4 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
                        {
                            response.RstKey = 15;
                            response.message = ResponseMessageKyc.Doc_Not_visible;
                        }
                        else
                        {
                            response.RstKey = 16;
                            response.message = ResponseMessageKyc.Doc_Rejected;
                        }
                    }
                    else
                    {
                        response.RstKey = 6;
                        response.StatusCode = (int)TransactionStatus.Failed;
                        response.message = ResponseMessages.EMAIL_VERIFICATION_PENDING;
                    }
                }
                else
                {
                    response.RstKey = 6;
                    response.message = ResponseMessages.TRANSACTION_DISABLED;
                }
            }

            catch (Exception ex)
            {

                //tran.Rollback();
            }
            return response;

        }

        public async Task<string> GetNgeniunsauthTokenbykey()
        {
            var _Request = new NgeniunstokenRequest();



            _Request.realmName = CommonSetting.Ngeniunstokenkey;

            var jsonReq = JsonConvert.SerializeObject(_Request);


            string resBody = "";
            RootObject responseData = new RootObject();
            using (HttpClient client = new HttpClient())
            {
                // Call asynchronous network methods in a try/catch block to handle exceptions
                try
                {
                    var content = new StringContent(jsonReq, Encoding.UTF8, "application/vnd.ni-identity.v1+json");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", CommonSetting.NgeniunsAPIKEY);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.ni-identity.v1+json"));//ACCEPT header



                    HttpResponseMessage response = await client.PostAsync(CommonSetting.NgeniunstokenUrl, content);

                    response.EnsureSuccessStatusCode();
                    resBody = await response.Content.ReadAsStringAsync();

                }
                catch (HttpRequestException e)
                {
                    e.Message.ErrorLog("GetNgeniunsauthTokenbykey", e.StackTrace + " " + e.Message);

                }

                return resBody;

            }
        }

        public async Task<AddMoneyAggregatorResponse> SavengeniusPaymentResponse(string reference)
        {
            AddMoneyAggregatorResponse response = new AddMoneyAggregatorResponse();
            CalculateCommissionResponse _commission = new CalculateCommissionResponse();
            CalculateCommissionRequest _commissionRequest = new CalculateCommissionRequest();
            string RequestString = JsonConvert.SerializeObject(reference);
            LogTransactionTypes.Response.SaveTransactionLog(LogTransactionNameTypes.AddMoney + "Credit to Debit Cards", RequestString, "Responce by bank detail : ");

            try
            {
                var responseData = await GetNgeniunsauthTokenbykey();

                var _responseModel = JsonConvert.DeserializeObject<NgeniunstokenResponse>(responseData);

                JavaScriptSerializer js = new JavaScriptSerializer();
                var responseData1 = await GethashorUrl(reference, _responseModel.access_token, "Ngeniunspaymentsorderstatus");
                dynamic blogObject = js.Deserialize<dynamic>(responseData1);
                object hash = blogObject["reference"];

                object hash1 = blogObject["_embedded"]["payment"][0]["authResponse"];//stagin
                authResponse obj2 = JsonConvert.DeserializeObject<authResponse>(JsonConvert.SerializeObject(hash1));


                var getInitialTransaction = await _cardPaymentRepository.GetTxnInitiateRequest(reference);
                if (obj2.success == true)
                {

                    int GetWalletTransactionsexist = await _cardPaymentRepository.GetWalletTransactionsexist(getInitialTransaction.WalletUserId, getInitialTransaction.InvoiceNumber);

                    if (getInitialTransaction != null && GetWalletTransactionsexist == 0)
                    {

                        getInitialTransaction.JsonResponse = JsonConvert.SerializeObject(responseData1);

                        if (reference == getInitialTransaction.UserReferanceNumber)
                        {
                            response.InvoiceNo = getInitialTransaction.InvoiceNumber;
                            response.Amount = getInitialTransaction.RequestedAmount;
                            response.status = "Ngenius";
                            DateTime TDate = DateTime.UtcNow;
                            response.TransactionDate = TDate;

                            int WalletServiceId = await _cardPaymentRepository.GetServiceId();
                            if (WalletServiceId > 0)
                            {
                                var adminUser = await _cardPaymentRepository.GetAdminUser();
                                if (adminUser != null)
                                {

                                    // var receiver = await _walletUserRepository.GetCurrentUser(Convert.ToInt32(requestDetail.WalletUserId)); //db.WalletUsers.Where(x => x.WalletUserId == requestDetail.WalletUserId).FirstOrDefault();

                                    long userId = Convert.ToInt32(getInitialTransaction.WalletUserId);
                                    var UserCurrentDetail = await _walletUserRepository.GetCurrentUser(userId);
                                    if (UserCurrentDetail != null && await _cardPaymentRepository.IsWalletTransactions(UserCurrentDetail.WalletUserId, getInitialTransaction.UserReferanceNumber))
                                    {
                                        //this line commented due to currentbalance is not added to card expected 
                                        //request.vpc_Amount = Convert.ToString(Math.Round((Convert.ToDecimal(requestDetail.Amount) / 100), 2));
                                        getInitialTransaction.RequestedAmount = Convert.ToString(Math.Round(Convert.ToDecimal(getInitialTransaction.RequestedAmount), 2));

                                        // to update wallet amount-----

                                        // db.WalletUsers.FirstOrDefault(x => x.WalletUserId == receiver.WalletUserId);

                                        if (UserCurrentDetail != null)
                                        {
                                            _commissionRequest.CurrentBalance = Convert.ToDecimal(UserCurrentDetail.CurrentBalance);
                                            _commissionRequest.IsRoundOff = true;
                                            //_commissionRequest.TransactionAmount = Convert.ToDecimal(request.vpc_Amount);

                                            _commissionRequest.TransactionAmount = Convert.ToDecimal(getInitialTransaction.RequestedAmount); //change
                                            _commissionRequest.WalletServiceId = WalletServiceId;
                                            _commission = await _setCommisionRepository.CalculateCommission(_commissionRequest);

                                            if (!string.IsNullOrEmpty(reference))
                                            {
                                                getInitialTransaction.TransactionStatus = (int)TransactionStatus.Completed;
                                                if (Convert.ToDecimal(UserCurrentDetail.CurrentBalance) >= 0)
                                                {
                                                    if (Convert.ToDecimal(UserCurrentDetail.CurrentBalance) == 0)
                                                    {
                                                        UserCurrentDetail.CurrentBalance = _commission.TransactionAmount.ToString();
                                                        getInitialTransaction.AfterTransactionBalance = UserCurrentDetail.CurrentBalance.ToString();
                                                        getInitialTransaction.ReceiverCurrentBalance = UserCurrentDetail.CurrentBalance.ToString();
                                                    }
                                                    else
                                                    {
                                                        UserCurrentDetail.CurrentBalance = Math.Round(Convert.ToDecimal(UserCurrentDetail.CurrentBalance) + _commission.TransactionAmount, 2).ToString();
                                                        getInitialTransaction.AfterTransactionBalance = UserCurrentDetail.CurrentBalance.ToString();
                                                        getInitialTransaction.ReceiverCurrentBalance = UserCurrentDetail.CurrentBalance.ToString();
                                                    }
                                                }
                                                else
                                                {
                                                    UserCurrentDetail.CurrentBalance = Math.Round(_commission.TransactionAmount - Convert.ToDecimal(UserCurrentDetail.CurrentBalance), 2).ToString();
                                                    getInitialTransaction.AfterTransactionBalance = UserCurrentDetail.CurrentBalance.ToString();
                                                    getInitialTransaction.ReceiverCurrentBalance = UserCurrentDetail.CurrentBalance.ToString();
                                                }
                                            }
                                            await _walletUserRepository.UpdateUserDetail(UserCurrentDetail);
                                            await _cardPaymentRepository.UpdateTransactionInitiateRequest(getInitialTransaction);
                                            // db.SaveChanges();
                                        }

                                        #region Save Transaction
                                        decimal amountWithCommision = decimal.Parse(string.Format("{0:0,0}", _commission.AmountWithCommission));    // "1,234,257";

                                        var _Transaction = new WalletTransaction();

                                        _Transaction.TransactionInitiateRequestId = getInitialTransaction.Id;
                                        _Transaction.MerchantCommissionAmount = Convert.ToString(_commission.MerchantCommissionAmount);
                                        _Transaction.MerchantCommissionId = _commission.MerchantCommissionId;
                                        _Transaction.TransactionType = AggragatorServiceType.CREDIT;
                                        _Transaction.IsBankTransaction = false;
                                        _Transaction.TransactionTypeInfo = (int)TransactionTypeInfo.AddedByCard;
                                        _Transaction.IsBankTransaction = false;
                                        _Transaction.Comments = string.Empty;
                                        _Transaction.InvoiceNo = getInitialTransaction.InvoiceNumber;
                                        _Transaction.CommisionAmount = Convert.ToString(_commission.CommissionAmount);
                                        _Transaction.CommisionId = _commission.CommissionId;
                                        _Transaction.WalletAmount = Convert.ToString(_commission.TransactionAmount);

                                        _Transaction.TotalAmount = Convert.ToString(amountWithCommision);
                                        _Transaction.ServiceTaxRate = _commission.ServiceTaxRate;
                                        _Transaction.ServiceTax = Convert.ToString(_commission.ServiceTaxAmount);
                                        _Transaction.OperatorType = "ngenius";

                                        _Transaction.AccountNo = getInitialTransaction.ReceiverNumber;
                                        if (!string.IsNullOrEmpty(reference))
                                        {

                                            _Transaction.TransactionStatus = (int)TransactionStatus.Completed;
                                            try
                                            {
                                                //--------send mail on success transaction--------

                                                var AdminKeys = AES256.AdminKeyPair;
                                                string FirstName = AES256.Decrypt(UserCurrentDetail.PrivateKey, UserCurrentDetail.FirstName);
                                                string LastName = AES256.Decrypt(UserCurrentDetail.PrivateKey, UserCurrentDetail.LastName);
                                                string StdCode = UserCurrentDetail.StdCode;
                                                string MobileNo = AES256.Decrypt(AdminKeys.PrivateKey, UserCurrentDetail.MobileNo);
                                                string EmailId = AES256.Decrypt(AdminKeys.PrivateKey, UserCurrentDetail.EmailId).Trim().ToLower();
                                                // var receiverDetail = new AppUserRepository().GetUserDetailById(receiver.WalletUserId);
                                                string filename = CommonSetting.successfullTransaction;


                                                var body = _sendEmails.ReadEmailformats(filename);
                                                body = body.Replace("$$FirstName$$", FirstName + " " + LastName);
                                                body = body.Replace("$$DisplayContent$$", "VISA CARDS/MASTER CARD");
                                                body = body.Replace("$$customer$$", MobileNo);
                                                body = body.Replace("$$amount$$", "XOF " + getInitialTransaction.RequestedAmount);
                                                body = body.Replace("$$ServiceTaxAmount$$", "XOF " + _commission.CommissionAmount);
                                                body = body.Replace("$$AmountWithCommission$$", "XOF " + amountWithCommision);
                                                body = body.Replace("$$TransactionId$$", getInitialTransaction.UserReferanceNumber);

                                                var req = new EmailModel()
                                                {
                                                    TO = EmailId,
                                                    Subject = "Transaction Successfull",
                                                    Body = body
                                                };
                                                _sendEmails.SendEmail(req);
                                            }
                                            catch
                                            {

                                            }
                                        }
                                        else
                                        {
                                            _Transaction.TransactionStatus = (int)TransactionStatus.Failed;
                                        }
                                        _Transaction.WalletServiceId = WalletServiceId;
                                        _Transaction.SenderId = UserCurrentDetail.WalletUserId;
                                        _Transaction.ReceiverId = UserCurrentDetail.WalletUserId;
                                        _Transaction.BankBranchCode = string.Empty;
                                        _Transaction.BankTransactionId = reference;
                                        _Transaction.TransactionId = getInitialTransaction.UserReferanceNumber;


                                        _Transaction.IsAdminTransaction = false;
                                        _Transaction.IsActive = true;
                                        _Transaction.IsDeleted = false;
                                        _Transaction.CreatedDate = TDate;
                                        _Transaction.UpdatedDate = TDate;
                                        _Transaction.TransactionTypeInfo = (int)TransactionTypeInfo.AddedByCard;
                                        _Transaction.IsAddDuringPay = false;
                                        _Transaction.VoucherCode = string.Empty;

                                        await _cardPaymentRepository.SaveWalletTransactions(_Transaction);
                                        //db.WalletTransactions.Add(_Transaction);
                                        //db.SaveChanges();
                                        #endregion

                                        #region Credit
                                        var _credit = new WalletTransactionDetail();
                                        _credit.Amount = Convert.ToString(_commission.TransactionAmount);
                                        _credit.TransactionType = (int)TransactionDetailType.Credit;
                                        _credit.WalletUserId = adminUser.WalletUserId;
                                        _credit.WalletTransactionId = _Transaction.WalletTransactionId;
                                        _credit.IsActive = true;
                                        _credit.IsDeleted = false;
                                        _credit.CreatedDate = TDate;
                                        _credit.UpdatedDate = TDate;
                                        //db.WalletTransactionDetails.Add(_credit);
                                        //db.SaveChanges();
                                        await _cardPaymentRepository.SaveWalletTransactionDetails(_credit);
                                        #endregion

                                        #region Debit
                                        var _debit = new WalletTransactionDetail();
                                        _debit.Amount = Convert.ToString(_commission.TransactionAmount);
                                        _debit.TransactionType = (int)TransactionDetailType.Debit;
                                        _debit.WalletUserId = UserCurrentDetail.WalletUserId;
                                        _debit.WalletTransactionId = _Transaction.WalletTransactionId;
                                        _debit.IsActive = true;
                                        _debit.IsDeleted = false;
                                        _debit.CreatedDate = TDate;
                                        _debit.UpdatedDate = TDate;
                                        //db.WalletTransactionDetails.Add(_credit);
                                        //db.SaveChanges();
                                        await _cardPaymentRepository.SaveWalletTransactionDetails(_credit);
                                        #endregion

                                        //get UpdateNewCardNoResponseBankCode id
                                        await _cardPaymentRepository.UpdateNewCardNoResponseBankCode(getInitialTransaction.InvoiceNumber, UserCurrentDetail.WalletUserId, reference);


                                        var adminKeyPair = AES256.AdminKeyPair;


                                        //db.SaveChanges();
                                        //tran.Commit();
                                        #region PushNotification

                                        var CurrentUser = await _walletUserRepository.GetCurrentUser(UserCurrentDetail.WalletUserId);//db.WalletUsers.Where(x => x.WalletUserId == receiver.WalletUserId).FirstOrDefault();
                                        if (CurrentUser != null)
                                        {
                                            PushNotificationModel push = new PushNotificationModel();
                                            push.SenderId = UserCurrentDetail.WalletUserId;
                                            push.deviceType = (int)UserCurrentDetail.DeviceType;
                                            push.deviceKey = UserCurrentDetail.DeviceToken;
                                            PayMoneyPushModel pushModel = new PayMoneyPushModel();
                                            pushModel.TransactionDate = TDate;
                                            pushModel.TransactionId = reference;
                                            pushModel.CurrentBalance = CurrentUser.CurrentBalance;
                                            pushModel.alert = _Transaction.WalletAmount + " XOF has been credited to your account.";
                                            pushModel.Amount = getInitialTransaction.RequestedAmount;
                                            pushModel.CurrentBalance = CurrentUser.CurrentBalance;
                                            pushModel.pushType = (int)PushType.ADDMONEY;

                                            if ((int)UserCurrentDetail.DeviceType == (int)DeviceTypes.ANDROID || (int)UserCurrentDetail.DeviceType == (int)DeviceTypes.Web)
                                            {
                                                PushPayload<PayMoneyPushModel> aps = new PushPayload<PayMoneyPushModel>();
                                                PushPayloadData<PayMoneyPushModel> _data = new PushPayloadData<PayMoneyPushModel>();
                                                _data.notification = pushModel;
                                                aps.data = _data;
                                                aps.to = UserCurrentDetail.DeviceToken;
                                                aps.collapse_key = string.Empty;
                                                push.message = JsonConvert.SerializeObject(aps);
                                                push.payload = pushModel;
                                            }
                                            if ((int)UserCurrentDetail.DeviceType == (int)DeviceTypes.IOS)
                                            {
                                                NotificationJsonResponse<PayMoneyIOSPushModel> aps = new NotificationJsonResponse<PayMoneyIOSPushModel>();
                                                PayMoneyIOSPushModel _iosPushModel = new PayMoneyIOSPushModel();
                                                _iosPushModel.alert = pushModel.alert;
                                                _iosPushModel.Amount = pushModel.Amount;
                                                _iosPushModel.CurrentBalance = pushModel.CurrentBalance;
                                                _iosPushModel.MobileNo = pushModel.MobileNo;
                                                _iosPushModel.SenderName = pushModel.SenderName;
                                                _iosPushModel.pushType = pushModel.pushType;
                                                aps.aps = _iosPushModel;

                                                push.message = JsonConvert.SerializeObject(aps);
                                            }
                                            //if (!string.IsNullOrEmpty(push.message))
                                            //{
                                            //    new PushNotificationRepository().sendPushNotification(push);
                                            //}
                                        }
                                        #endregion

                                        if (UserCurrentDetail.MobileNo != null && UserCurrentDetail.MobileNo != "")
                                        {
                                            response.ToMobileNo = AES256.Decrypt(adminKeyPair.PrivateKey, UserCurrentDetail.MobileNo);
                                        }


                                        response.CurrentBalance = UserCurrentDetail.CurrentBalance;
                                        response.RstKey = 1;



                                        ///
                                        await _masterDataService.Chargeback(UserCurrentDetail.WalletUserId);
                                    }
                                    else
                                    {
                                        //test

                                    }
                                }
                                else
                                {
                                    //test

                                }
                                //sdfsdfd
                            }
                            else
                            {

                                response.RstKey = 2;
                            }
                        }
                        else
                        {
                            await _cardPaymentRepository.UpdateTransactionInitiateRequest(getInitialTransaction);
                            response.RstKey = 3;

                            //response.TransactionResponseDescription = _req.vpc_ResponseCodeDescription;
                            //response.TransactionResponseCode = request.vpc_TxnResponseCode;
                        }



                    }
                    else
                    {
                        response.RstKey = 3;
                    }
                }
                else
                {

                    getInitialTransaction.JsonResponse = JsonConvert.SerializeObject(responseData1);
                    await _cardPaymentRepository.UpdateTransactionInitiateRequest(getInitialTransaction);
                    response.RstKey = 3;
                }

            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("CardPaymentRepository", "SavengeniusPaymentResponse", reference);
            }
            return response;
        }


        public async Task<MasterCardPaymentUBAResponse> NewMasterCardPayment2(ThirdpartyPaymentByCardRequest request, string headerToken)
        {
            var response = new MasterCardPaymentUBAResponse();
            var _commission = new CalculateCommissionResponse();
            var _commissionRequest = new CalculateCommissionRequest();

            var transationInitiate = new TransactionInitiateRequest();
            var _thirdPartyPaymentByCard = new ThirdPartyPaymentByCard();

            try
            {

                var UserDetail = await _walletUserService.UserProfile(headerToken);

                var Isdocverified = await _walletUserRepository.IsDocVerifiedMOMO(UserDetail.DocumetStatus);

                var resultTL = await _transactionLimitAUService.CheckTransactionLimitAU(request.WalletUserId.ToString()); //check New TL
                var transactionLimit = await _masterDataRepository.GetTransactionLimitAddMoney(Convert.ToString(UserDetail.WalletUserId));
                int limit = transactionLimit != null ? Convert.ToInt32(transactionLimit.TransactionLimitForAddMoney) : 0;

                var transactionHistory = _masterDataRepository.GetAllTransactionsAddMoney(UserDetail.WalletUserId);
                int totalAmountTransfered = transactionHistory != null ? Convert.ToInt32(transactionHistory.totalAmount) : 0;


                //------Get Currency Rate--------------
                var currencyDetail = _masterDataRepository.GetCurrencyRate();

                //decimal cediRate = Convert.ToDecimal(currencyDetail.CediRate); 
                decimal UsdRate = Convert.ToDecimal(currencyDetail.CediRate);//Add Doller Rate
                //decimal CfaRate = Convert.ToDecimal(currencyDetail.LERate);
                decimal requestAmount = Convert.ToDecimal(request.Amount);//;


                #region Calculate commission on request amount               

                //var WalletService = await _masterDataRepository.GetWalletServicesByIdOrChannel(request.channel, request.ServiceCategoryId);
                int WalletServiceId = await _cardPaymentRepository.GetServiceId();
                if (UserDetail.IsActive == true)//am
                {
                    if (UserDetail.IsEmailVerified == true)
                    {
                        if (Isdocverified == true)
                        {
                            if (transactionLimit == null || transactionLimit.TransactionLimitForAddMoney == null || limit >= (Convert.ToDecimal(request.Amount) + totalAmountTransfered))
                            {
                                if (WalletServiceId > 0)
                                {
                                    #region Calculate Commission on request amount
                                    _commissionRequest.IsRoundOff = true;
                                    _commissionRequest.TransactionAmount = Convert.ToDecimal(request.Amount);
                                    _commissionRequest.WalletServiceId = WalletServiceId;
                                    _commission = await _setCommisionRepository.CalculateCommission(_commissionRequest);
                                    #endregion
                                }


                                decimal amt = (_commission.AmountWithCommission * UsdRate);
                                var finalAmt = Decimal.Parse(amt.ToString("0.00"));
                                //var finalAmt1 = decimal.Parse(string.Format("{0:0,0}", finalAmt));    // "1,234,257";

                                //CardPaymentRequest _cardRequest = new CardPaymentRequest();
                                //_cardRequest.WalletUserId = UserDetail.WalletUserId;
                                //_cardRequest.TotalAmount = _commission.AmountWithCommission.ToString();
                                //_cardRequest.CommissionAmount = Convert.ToString(_commission.CommissionAmount);
                                //_cardRequest.Amount = _commission.TransactionAmount.ToString();// Convert.ToString(requestAmo);
                                //_cardRequest.FlatCharges = _commission.FlatCharges;
                                //_cardRequest.BenchmarkCharges = _commission.BenchmarkCharges;
                                //_cardRequest.CommisionCharges = _commission.CommisionPercent;
                                //_cardRequest.CreatedDate = DateTime.UtcNow;
                                //_cardRequest.UpdatedDate = DateTime.UtcNow;
                                //_cardRequest.AmountInCedi = finalAmt.ToString();
                                //_cardRequest = await _cardPaymentRepository.SaveCardPaymentRequest(_cardRequest);


                                //decimal amountWithCommision = decimal.Parse(string.Format("{0:0,0}", _commission.AmountWithCommission));    // "1,234,257";

                                #endregion
                                if (resultTL != null)
                                {
                                    decimal SetAmount = Convert.ToDecimal(resultTL.SetAmount);// returns decimal
                                    decimal TotalAmount = Convert.ToDecimal(resultTL.TotalAmount);// returns decimal

                                    if (SetAmount != 0) //0 =msg 
                                    {
                                        decimal requestAmountwithcomm = _commission.AmountWithCommission + TotalAmount;

                                        if (requestAmountwithcomm <= SetAmount)//1000 >= 1000
                                        {

                                        }
                                        else
                                        {
                                            response.RstKey = 6;
                                            response.Message = ResponseMessages.INVALID_txnAmountREQUEST;
                                            return response;
                                        }
                                    }
                                }

                                var uba = await GetPaymentUrlMastrCardUBa2(request.Amount, finalAmt.ToString(), _commission.AmountWithCommission.ToString(), headerToken);

                                if (uba.SuccessIndicator != null)
                                {


                                    //save selected card no. with paytranid
                                    await _cardPaymentRepository.SaveCardNo(uba.InvoiceNumber, UserDetail.WalletUserId, request.CardNo, uba.SuccessIndicator, request.Amount, _commission.AmountWithCommission.ToString(), UserDetail.EmailId);

                                    response.SessionId = uba.SessionId;
                                    response.SuccessIndicator = uba.SuccessIndicator;
                                    response.Version = uba.Version;
                                    response.InvoiceNumber = uba.InvoiceNumber;
                                    response.Merchant = uba.Merchant;

                                    response.sessioanData = uba.sessioanData;
                                    //http://apiref.ezipaygh.com/HtmlTemplates/UBATestGetway.html?SessionId=SESSION0002646669248L2590941M50&Merchant=UBAEZIPAYGH&InvoiceNumber=erwewerw3
                                    //response.URL = "http://54.218.162.6:9097/HtmlTemplates/UBATestGetway.html?" + "SessionId=" + uba.SessionId + "&Merchant=UBAEZIPAYGH" + "&InvoiceNumber=" + uba.InvoiceNumber;
                                    response.URL = CommonSetting.MPGSTemplate2 + "SessionId=" + uba.SessionId + "&Merchant= " + CommonSetting.MerchantNameMasterCard2 + " " + "&InvoiceNumber=" + uba.InvoiceNumber;
                                    _thirdPartyPaymentByCard.Amount = _commission.TransactionAmount.ToString();
                                    _thirdPartyPaymentByCard.AmountWithCommision = _commission.AmountWithCommission.ToString();
                                    _thirdPartyPaymentByCard.BenificiaryName = request.BeneficiaryName;
                                    _thirdPartyPaymentByCard.Comment = request.Comment;
                                    _thirdPartyPaymentByCard.Channel = request.channel;
                                    _thirdPartyPaymentByCard.MobileNo = request.customer;
                                    _thirdPartyPaymentByCard.ISD = request.ISD;
                                    _thirdPartyPaymentByCard.OrderNumber = uba.SuccessIndicator;
                                    _thirdPartyPaymentByCard.WalletUserId = request.WalletUserId;
                                    _thirdPartyPaymentByCard.IsActive = true;
                                    _thirdPartyPaymentByCard.IsDeleted = false;
                                    _thirdPartyPaymentByCard.CreatedDate = DateTime.UtcNow;
                                    _thirdPartyPaymentByCard.UpdatedDate = DateTime.UtcNow;
                                    _thirdPartyPaymentByCard.ServiceCategoryId = request.ServiceCategoryId;
                                    _thirdPartyPaymentByCard.SessionTokenPerTransaction = headerToken;
                                    _thirdPartyPaymentByCard.MerchantId = request.MerchantId;
                                    _thirdPartyPaymentByCard.MerchantName = request.Name;
                                    _thirdPartyPaymentByCard.Product_Id = request.Product_Id; //use productid -dth & msisdn:- int. recharge
                                    _thirdPartyPaymentByCard.AmountInLocalCountry = request.AmountInLocalCountry; //use AmountInLocalCountry -dth & msisdn:- int. recharge
                                    _thirdPartyPaymentByCard.InvoiceNumber = uba.InvoiceNumber;
                                    _thirdPartyPaymentByCard.DisplayContent = request.DisplayContent;
                                    _thirdPartyPaymentByCard.AmountInUsd = finalAmt.ToString();
                                    _thirdPartyPaymentByCard.ExtraField = string.Empty;

                                    _thirdPartyPaymentByCard = await _cardPaymentRepository.SaveThirdPartyPaymentByCard(_thirdPartyPaymentByCard);
                                    ////save data for initiate transaction 
                                    //transationInitiate.InvoiceNumber = uba.InvoiceNumber;
                                    //transationInitiate.ReceiverNumber = UserDetail.MobileNo;
                                    //transationInitiate.ServiceName = "Third party Payment by card";
                                    //transationInitiate.RequestedAmount = _commission.TransactionAmount.ToString();
                                    //transationInitiate.TransactionStatus = (int)TransactionStatus.Pending;
                                    //transationInitiate.WalletUserId = UserDetail.WalletUserId;
                                    //transationInitiate.UserReferanceNumber = uba.SuccessIndicator;
                                    //transationInitiate.CurrentBalance = UserDetail.CurrentBalance;
                                    //transationInitiate.AfterTransactionBalance = "";
                                    //transationInitiate.ReceiverCurrentBalance = UserDetail.CurrentBalance;
                                    //transationInitiate.UserName = UserDetail.FirstName + " " + UserDetail.LastName;
                                    //transationInitiate.ReceiverWalletUserId = UserDetail.WalletUserId;
                                    //transationInitiate.CreatedDate = DateTime.UtcNow;
                                    //transationInitiate.UpdatedDate = DateTime.UtcNow;
                                    //transationInitiate.IsActive = true;
                                    //transationInitiate.IsDeleted = false;
                                    //transationInitiate.JsonRequest = uba.sessioanData;
                                    //transationInitiate.JsonResponse = "";
                                    //transationInitiate = await _cardPaymentRepository.SaveTransactionInitiateRequest(transationInitiate);


                                    response.RstKey = 2;
                                }
                                else
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.Message = "Please try after some time aggregator error.";
                                }
                                LogTransactionTypes.Request.SaveTransactionLog(LogTransactionNameTypes.AddMoney + "Credit TO Debit Cards", response.sessioanData, "Request Url : " + response.sessioanData);
                            }
                            else
                            {
                                var addLimit = limit - (Convert.ToDecimal(request.Amount) + totalAmountTransfered);
                                if (addLimit < Convert.ToDecimal(request.Amount))
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.Message = "Exceed your transaction limit.";
                                }
                                else
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.Message = "You can not add more then " + transactionLimit.TransactionLimitForAddMoney.ToString() + " cedi in a day";
                                }
                            }
                        }
                        else if (UserDetail.DocumetStatus == 0 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
                        {
                            response.RstKey = 13;
                            response.Message = ResponseMessageKyc.FAILED_Doc_NotUploaded;
                        }
                        else if (UserDetail.DocumetStatus == 1 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
                        {
                            response.RstKey = 14;
                            response.Message = ResponseMessageKyc.FAILED_Doc_Pending;
                        }
                        else if (UserDetail.DocumetStatus == 4 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
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
                        response.RstKey = 6;
                        response.StatusCode = (int)TransactionStatus.Failed;
                        response.Message = ResponseMessages.EMAIL_VERIFICATION_PENDING;
                    }
                }
                else
                {
                    response.RstKey = 6;
                    response.Message = ResponseMessages.TRANSACTION_DISABLED;
                }
            }

            catch (Exception ex)
            {

                //tran.Rollback();
            }
            return response;

        }

        public async Task<MasterCardPaymentUBAResponse> GetPaymentUrlMastrCardUBa2(string Amountinxof, string AmountinUsd, string totalAmountxof, string headerToken)
        {
            CheckoutSessionModel model = new CheckoutSessionModel();
            var _commission = new CalculateCommissionResponse();
            var _commissionRequest = new CalculateCommissionRequest();

            var response = new MasterCardPaymentUBAResponse();
            var _masterCard = new MasterCardPaymentRequest();
            var transationInitiate = new TransactionInitiateRequest();
            var UserDetail = await _walletUserService.UserProfile(headerToken);

            var transactionLimit = await _masterDataRepository.GetTransactionLimitAddMoney(Convert.ToString(UserDetail.WalletUserId));
            int limit = transactionLimit != null ? Convert.ToInt32(transactionLimit.TransactionLimitForAddMoney) : 0;

            var transactionHistory = _masterDataRepository.GetAllTransactionsAddMoney(UserDetail.WalletUserId);
            int totalAmountTransfered = transactionHistory != null ? Convert.ToInt32(transactionHistory.totalAmount) : 0;

            int WalletServiceId = await _cardPaymentRepository.GetServiceId();


            if (transactionLimit == null || limit >= (Convert.ToDecimal(Amountinxof) + totalAmountTransfered))
            {
                if (WalletServiceId > 0)
                {
                    #region Calculate Commission on request amount
                    _commissionRequest.IsRoundOff = true;
                    _commissionRequest.TransactionAmount = Convert.ToDecimal(Amountinxof);
                    _commissionRequest.WalletServiceId = WalletServiceId;
                    _commission = await _setCommisionRepository.CalculateCommission(_commissionRequest);
                    #endregion
                }
                string value = String.Empty;

                string username = CommonSetting.MasterCardUserName2;
                string password = CommonSetting.MasterCardPassword2;
                var client = new HttpClient();
                client.BaseAddress = new Uri(CommonSetting.MasterCardUrl2);
                var invoiceNumber = await _masterDataRepository.GetInvoiceNumber();
                //invoiceNumber.InvoiceNumber = "XXXXXXXX0001";
                //Random rnd = new Random();  //a very common mistake

                var values = new Dictionary<string, string>()
                        {
                            {"apiOperation", "CREATE_CHECKOUT_SESSION"},
                            {"apiPassword",password},
                            {"apiUsername", CommonSetting.apiUsernameMasterCard2},
                            {"merchant",CommonSetting.MerchantNameMasterCard2},
                            {"interaction.operation",CommonSetting.operation2},
                            {"order.amount", AmountinUsd},
                            {"order.id", invoiceNumber.InvoiceNumber},
                            {"order.currency", "USD"},
                            {"interaction.returnUrl",CommonSetting.MasterCardCallBackForPayServicesNewFlow2}
                        };
                var content = new FormUrlEncodedContent(values);

                var request = new HttpRequestMessage()
                {
                    // RequestUri = new Uri("devnull", UriKind.Relative),
                    Method = HttpMethod.Post,
                    Content = content
                };
                request.Headers.ExpectContinue = false;
                request.Headers.Add("custom-header", "a header value");

                var base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", base64String);
                var result = await client.SendAsync(request);
                var customerJsonString = await result.Content.ReadAsStringAsync();
                List<string> keyValuePairs = customerJsonString.Split('&').ToList();

                foreach (var keyValuePair in keyValuePairs)
                {
                    string key = keyValuePair.Split('=')[0].Trim();
                    if (key == "session.id")
                    {
                        model.Id = keyValuePair.Split('=')[1];
                    }
                    else if (key == "session.version")
                    {
                        model.Version = keyValuePair.Split('=')[1];
                    }
                    else if (key == "successIndicator")
                    {
                        model.SuccessIndicator = keyValuePair.Split('=')[1];
                    }
                    else if (key == "merchant")
                    {
                        model.merchant = keyValuePair.Split('=')[1];
                    }
                }
                _masterCard.SessionId = model.Id;
                _masterCard.Version = model.Version;
                _masterCard.SuccessIndicator = model.SuccessIndicator;
                _masterCard.Merchant = model.merchant;
                _masterCard.IsActive = true;
                _masterCard.IsDeleted = false;
                _masterCard.CreatedDate = DateTime.UtcNow;
                _masterCard.UpdatedDate = DateTime.UtcNow;
                _masterCard.Amount = Amountinxof;
                _masterCard.CommisionCharges = _commission.CommisionPercent;
                _masterCard.TotalAmount = totalAmountxof;
                _masterCard.WalletUserId = UserDetail.WalletUserId;
                _masterCard.TransactionNo = invoiceNumber.InvoiceNumber;
                _masterCard.FlatCharges = _commission.FlatCharges;
                _masterCard.BenchmarkCharges = _commission.BenchmarkCharges;
                _masterCard.CommissionAmount = _commission.CommissionAmount.ToString();
                await _cardPaymentRepository.SaveMasterCardPaymentRequest(_masterCard);


                response.SessionId = model.Id;
                response.SuccessIndicator = model.SuccessIndicator;
                response.Version = model.Version;
                response.InvoiceNumber = invoiceNumber.InvoiceNumber;
                response.Merchant = model.merchant;
                response.RstKey = 1;
                response.sessioanData = customerJsonString;
                if (response.sessioanData != null)
                {
                    response.RstKey = 1;

                    transationInitiate.InvoiceNumber = invoiceNumber.InvoiceNumber;
                    transationInitiate.ReceiverNumber = UserDetail.MobileNo;
                    transationInitiate.ServiceName = "Card Payment2";
                    transationInitiate.RequestedAmount = Amountinxof;
                    transationInitiate.TransactionStatus = (int)TransactionStatus.Pending;
                    transationInitiate.WalletUserId = UserDetail.WalletUserId;
                    transationInitiate.UserReferanceNumber = response.SuccessIndicator;
                    transationInitiate.CurrentBalance = UserDetail.CurrentBalance;
                    transationInitiate.AfterTransactionBalance = "";
                    transationInitiate.ReceiverCurrentBalance = UserDetail.CurrentBalance;
                    transationInitiate.UserName = UserDetail.FirstName + " " + UserDetail.LastName;
                    transationInitiate.ReceiverWalletUserId = UserDetail.WalletUserId;
                    transationInitiate.CreatedDate = DateTime.UtcNow;
                    transationInitiate.UpdatedDate = DateTime.UtcNow;
                    transationInitiate.IsActive = true;
                    transationInitiate.IsDeleted = false;
                    transationInitiate.JsonRequest = customerJsonString;
                    transationInitiate.JsonResponse = "";
                    transationInitiate = await _cardPaymentRepository.SaveTransactionInitiateRequest(transationInitiate);
                }
                LogTransactionTypes.Request.SaveTransactionLog(LogTransactionNameTypes.AddMoney + "Credit TO Debit Cards master card", customerJsonString, "Request Url : " + customerJsonString);
            }
            else
            {
                var addLimit = limit - (Convert.ToDecimal(Amountinxof) + totalAmountTransfered);
                if (addLimit < Convert.ToDecimal(Amountinxof))
                {
                    response.RstKey = 6;
                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                    response.Message = "Exceed your transaction limit.";
                }
                else
                {
                    response.RstKey = 6;
                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                    response.Message = "You can not add more then " + transactionLimit.TransactionLimitForAddMoney.ToString() + " cedi in a day";
                }
            }

            return response;
        }


        public async Task<AddMoneyAggregatorResponse> SaveMasterCardPayment2Response(MasterCardPaymentResponse request)
        {
            AddMoneyAggregatorResponse response = new AddMoneyAggregatorResponse();
            CalculateCommissionResponse _commission = new CalculateCommissionResponse();
            CalculateCommissionRequest _commissionRequest = new CalculateCommissionRequest();
            string RequestString = JsonConvert.SerializeObject(request);
            LogTransactionTypes.Response.SaveTransactionLog(LogTransactionNameTypes.AddMoney + "Credit to Debit Cards", RequestString, "Responce by bank detail : ");

            try
            {
                var requestDetail = await _cardPaymentRepository.GetMasterCardPaymentRequest(request.resultIndicator);
                var getInitialTransaction = await _cardPaymentRepository.GetTransactionInitiateRequest(requestDetail.TransactionNo);

                int GetWalletTransactionsexist = await _cardPaymentRepository.GetWalletTransactionsexist(requestDetail.WalletUserId, requestDetail.TransactionNo);

                if (requestDetail != null && GetWalletTransactionsexist == 0)
                {

                    getInitialTransaction.JsonResponse = JsonConvert.SerializeObject(request);

                    if (request.resultIndicator != null && request.sessionVersion != null)
                    {
                        response.InvoiceNo = requestDetail.TransactionNo;
                        response.Amount = getInitialTransaction.RequestedAmount;
                        response.status = "MPGS";
                        DateTime TDate = DateTime.UtcNow;
                        response.TransactionDate = TDate;

                        int WalletServiceId = await _cardPaymentRepository.GetServiceId();
                        if (WalletServiceId > 0)
                        {
                            var adminUser = await _cardPaymentRepository.GetAdminUser();
                            if (adminUser != null)
                            {

                                // var receiver = await _walletUserRepository.GetCurrentUser(Convert.ToInt32(requestDetail.WalletUserId)); //db.WalletUsers.Where(x => x.WalletUserId == requestDetail.WalletUserId).FirstOrDefault();

                                long userId = Convert.ToInt32(requestDetail.WalletUserId);
                                var UserCurrentDetail = await _walletUserRepository.GetCurrentUser(userId);
                                if (UserCurrentDetail != null && await _cardPaymentRepository.IsWalletTransactions(UserCurrentDetail.WalletUserId, requestDetail.TransactionNo))
                                {
                                    //this line commented due to currentbalance is not added to card expected 
                                    //request.vpc_Amount = Convert.ToString(Math.Round((Convert.ToDecimal(requestDetail.Amount) / 100), 2));
                                    requestDetail.Amount = Convert.ToString(Math.Round(Convert.ToDecimal(requestDetail.Amount), 2));

                                    // to update wallet amount-----

                                    // db.WalletUsers.FirstOrDefault(x => x.WalletUserId == receiver.WalletUserId);

                                    if (UserCurrentDetail != null)
                                    {
                                        _commissionRequest.CurrentBalance = Convert.ToDecimal(UserCurrentDetail.CurrentBalance);
                                        _commissionRequest.IsRoundOff = true;
                                        //_commissionRequest.TransactionAmount = Convert.ToDecimal(request.vpc_Amount);

                                        _commissionRequest.TransactionAmount = Convert.ToDecimal(requestDetail.Amount); //change
                                        _commissionRequest.WalletServiceId = WalletServiceId;
                                        _commission = await _setCommisionRepository.CalculateCommission(_commissionRequest);

                                        if (!string.IsNullOrEmpty(request.resultIndicator))
                                        {
                                            getInitialTransaction.TransactionStatus = (int)TransactionStatus.Completed;
                                            if (Convert.ToDecimal(UserCurrentDetail.CurrentBalance) >= 0)
                                            {
                                                if (Convert.ToDecimal(UserCurrentDetail.CurrentBalance) == 0)
                                                {
                                                    UserCurrentDetail.CurrentBalance = _commission.TransactionAmount.ToString();
                                                    getInitialTransaction.AfterTransactionBalance = UserCurrentDetail.CurrentBalance.ToString();
                                                    getInitialTransaction.ReceiverCurrentBalance = UserCurrentDetail.CurrentBalance.ToString();
                                                }
                                                else
                                                {
                                                    UserCurrentDetail.CurrentBalance = Math.Round(Convert.ToDecimal(UserCurrentDetail.CurrentBalance) + _commission.TransactionAmount, 2).ToString();
                                                    getInitialTransaction.AfterTransactionBalance = UserCurrentDetail.CurrentBalance.ToString();
                                                    getInitialTransaction.ReceiverCurrentBalance = UserCurrentDetail.CurrentBalance.ToString();
                                                }
                                            }
                                            else
                                            {
                                                UserCurrentDetail.CurrentBalance = Math.Round(_commission.TransactionAmount - Convert.ToDecimal(UserCurrentDetail.CurrentBalance), 2).ToString();
                                                getInitialTransaction.AfterTransactionBalance = UserCurrentDetail.CurrentBalance.ToString();
                                                getInitialTransaction.ReceiverCurrentBalance = UserCurrentDetail.CurrentBalance.ToString();
                                            }
                                        }
                                        await _walletUserRepository.UpdateUserDetail(UserCurrentDetail);
                                        await _cardPaymentRepository.UpdateTransactionInitiateRequest(getInitialTransaction);
                                        // db.SaveChanges();
                                    }

                                    #region Save Transaction
                                    decimal amountWithCommision = decimal.Parse(string.Format("{0:0,0}", _commission.AmountWithCommission));    // "1,234,257";

                                    var _Transaction = new WalletTransaction();

                                    _Transaction.TransactionInitiateRequestId = getInitialTransaction.Id;
                                    _Transaction.MerchantCommissionAmount = Convert.ToString(_commission.MerchantCommissionAmount);
                                    _Transaction.MerchantCommissionId = _commission.MerchantCommissionId;
                                    _Transaction.TransactionType = AggragatorServiceType.CREDIT;
                                    _Transaction.IsBankTransaction = false;
                                    _Transaction.TransactionTypeInfo = (int)TransactionTypeInfo.AddedByCard;
                                    _Transaction.IsBankTransaction = false;
                                    _Transaction.Comments = string.Empty;
                                    _Transaction.InvoiceNo = getInitialTransaction.InvoiceNumber;
                                    _Transaction.CommisionAmount = Convert.ToString(_commission.CommissionAmount);
                                    _Transaction.CommisionId = _commission.CommissionId;
                                    _Transaction.WalletAmount = Convert.ToString(_commission.TransactionAmount);

                                    _Transaction.TotalAmount = Convert.ToString(amountWithCommision);
                                    _Transaction.ServiceTaxRate = _commission.ServiceTaxRate;
                                    _Transaction.ServiceTax = Convert.ToString(_commission.ServiceTaxAmount);
                                    _Transaction.OperatorType = "MPGS2";

                                    _Transaction.AccountNo = getInitialTransaction.ReceiverNumber;
                                    if (!string.IsNullOrEmpty(request.resultIndicator))
                                    {

                                        _Transaction.TransactionStatus = (int)TransactionStatus.Completed; ;
                                        try
                                        {
                                            //--------send mail on success transaction--------

                                            var AdminKeys = AES256.AdminKeyPair;
                                            string FirstName = AES256.Decrypt(UserCurrentDetail.PrivateKey, UserCurrentDetail.FirstName);
                                            string LastName = AES256.Decrypt(UserCurrentDetail.PrivateKey, UserCurrentDetail.LastName);
                                            string StdCode = UserCurrentDetail.StdCode;
                                            string MobileNo = AES256.Decrypt(AdminKeys.PrivateKey, UserCurrentDetail.MobileNo);
                                            string EmailId = AES256.Decrypt(AdminKeys.PrivateKey, UserCurrentDetail.EmailId).Trim().ToLower();
                                            // var receiverDetail = new AppUserRepository().GetUserDetailById(receiver.WalletUserId);
                                            string filename = CommonSetting.successfullTransaction;


                                            var body = _sendEmails.ReadEmailformats(filename);
                                            body = body.Replace("$$FirstName$$", FirstName + " " + LastName);
                                            body = body.Replace("$$DisplayContent$$", "VISA CARDS/MASTER CARD");
                                            body = body.Replace("$$customer$$", MobileNo);
                                            body = body.Replace("$$amount$$", "XOF " + requestDetail.Amount);
                                            body = body.Replace("$$ServiceTaxAmount$$", "XOF " + _commission.CommissionAmount);
                                            body = body.Replace("$$AmountWithCommission$$", "XOF " + amountWithCommision);
                                            body = body.Replace("$$TransactionId$$", Convert.ToString(requestDetail.TransactionNo));

                                            var req = new EmailModel()
                                            {
                                                TO = EmailId,
                                                Subject = "Transaction Successfull",
                                                Body = body
                                            };
                                            _sendEmails.SendEmail(req);
                                        }
                                        catch
                                        {

                                        }
                                    }
                                    else
                                    {
                                        _Transaction.TransactionStatus = (int)TransactionStatus.Failed;
                                    }
                                    _Transaction.WalletServiceId = WalletServiceId;
                                    _Transaction.SenderId = UserCurrentDetail.WalletUserId;
                                    _Transaction.ReceiverId = UserCurrentDetail.WalletUserId;
                                    _Transaction.BankBranchCode = string.Empty;
                                    _Transaction.BankTransactionId = request.resultIndicator;
                                    _Transaction.TransactionId = requestDetail.TransactionNo;


                                    _Transaction.IsAdminTransaction = false;
                                    _Transaction.IsActive = true;
                                    _Transaction.IsDeleted = false;
                                    _Transaction.CreatedDate = TDate;
                                    _Transaction.UpdatedDate = TDate;
                                    _Transaction.TransactionTypeInfo = (int)TransactionTypeInfo.AddedByCard;
                                    _Transaction.IsAddDuringPay = false;
                                    _Transaction.VoucherCode = string.Empty;

                                    await _cardPaymentRepository.SaveWalletTransactions(_Transaction);
                                    //db.WalletTransactions.Add(_Transaction);
                                    //db.SaveChanges();
                                    #endregion

                                    #region Credit
                                    var _credit = new WalletTransactionDetail();
                                    _credit.Amount = Convert.ToString(_commission.TransactionAmount);
                                    _credit.TransactionType = (int)TransactionDetailType.Credit;
                                    _credit.WalletUserId = adminUser.WalletUserId;
                                    _credit.WalletTransactionId = _Transaction.WalletTransactionId;
                                    _credit.IsActive = true;
                                    _credit.IsDeleted = false;
                                    _credit.CreatedDate = TDate;
                                    _credit.UpdatedDate = TDate;
                                    //db.WalletTransactionDetails.Add(_credit);
                                    //db.SaveChanges();
                                    await _cardPaymentRepository.SaveWalletTransactionDetails(_credit);
                                    #endregion

                                    #region Debit
                                    var _debit = new WalletTransactionDetail();
                                    _debit.Amount = Convert.ToString(_commission.TransactionAmount);
                                    _debit.TransactionType = (int)TransactionDetailType.Debit;
                                    _debit.WalletUserId = UserCurrentDetail.WalletUserId;
                                    _debit.WalletTransactionId = _Transaction.WalletTransactionId;
                                    _debit.IsActive = true;
                                    _debit.IsDeleted = false;
                                    _debit.CreatedDate = TDate;
                                    _debit.UpdatedDate = TDate;
                                    //db.WalletTransactionDetails.Add(_credit);
                                    //db.SaveChanges();
                                    await _cardPaymentRepository.SaveWalletTransactionDetails(_credit);
                                    #endregion

                                    //get UpdateNewCardNoResponseBankCode id
                                    await _cardPaymentRepository.UpdateNewCardNoResponseBankCode(getInitialTransaction.InvoiceNumber, UserCurrentDetail.WalletUserId, request.resultIndicator);


                                    var adminKeyPair = AES256.AdminKeyPair;


                                    //db.SaveChanges();
                                    //tran.Commit();
                                    #region PushNotification

                                    var CurrentUser = await _walletUserRepository.GetCurrentUser(UserCurrentDetail.WalletUserId);//db.WalletUsers.Where(x => x.WalletUserId == receiver.WalletUserId).FirstOrDefault();
                                    if (CurrentUser != null)
                                    {
                                        PushNotificationModel push = new PushNotificationModel();
                                        push.SenderId = UserCurrentDetail.WalletUserId;
                                        push.deviceType = (int)UserCurrentDetail.DeviceType;
                                        push.deviceKey = UserCurrentDetail.DeviceToken;
                                        PayMoneyPushModel pushModel = new PayMoneyPushModel();
                                        pushModel.TransactionDate = TDate;
                                        pushModel.TransactionId = requestDetail.TransactionNo;
                                        pushModel.CurrentBalance = CurrentUser.CurrentBalance;
                                        pushModel.alert = _Transaction.WalletAmount + " XOF has been credited to your account.";
                                        pushModel.Amount = getInitialTransaction.RequestedAmount;
                                        pushModel.CurrentBalance = CurrentUser.CurrentBalance;
                                        pushModel.pushType = (int)PushType.ADDMONEY;

                                        if ((int)UserCurrentDetail.DeviceType == (int)DeviceTypes.ANDROID || (int)UserCurrentDetail.DeviceType == (int)DeviceTypes.Web)
                                        {
                                            PushPayload<PayMoneyPushModel> aps = new PushPayload<PayMoneyPushModel>();
                                            PushPayloadData<PayMoneyPushModel> _data = new PushPayloadData<PayMoneyPushModel>();
                                            _data.notification = pushModel;
                                            aps.data = _data;
                                            aps.to = UserCurrentDetail.DeviceToken;
                                            aps.collapse_key = string.Empty;
                                            push.message = JsonConvert.SerializeObject(aps);
                                            push.payload = pushModel;
                                        }
                                        if ((int)UserCurrentDetail.DeviceType == (int)DeviceTypes.IOS)
                                        {
                                            NotificationJsonResponse<PayMoneyIOSPushModel> aps = new NotificationJsonResponse<PayMoneyIOSPushModel>();
                                            PayMoneyIOSPushModel _iosPushModel = new PayMoneyIOSPushModel();
                                            _iosPushModel.alert = pushModel.alert;
                                            _iosPushModel.Amount = pushModel.Amount;
                                            _iosPushModel.CurrentBalance = pushModel.CurrentBalance;
                                            _iosPushModel.MobileNo = pushModel.MobileNo;
                                            _iosPushModel.SenderName = pushModel.SenderName;
                                            _iosPushModel.pushType = pushModel.pushType;
                                            aps.aps = _iosPushModel;

                                            push.message = JsonConvert.SerializeObject(aps);
                                        }
                                        //if (!string.IsNullOrEmpty(push.message))
                                        //{
                                        //    new PushNotificationRepository().sendPushNotification(push);
                                        //}
                                    }
                                    #endregion

                                    if (UserCurrentDetail.MobileNo != null && UserCurrentDetail.MobileNo != "")
                                    {
                                        response.ToMobileNo = AES256.Decrypt(adminKeyPair.PrivateKey, UserCurrentDetail.MobileNo);
                                    }


                                    response.CurrentBalance = UserCurrentDetail.CurrentBalance;
                                    response.RstKey = 1;


                                    ///
                                    await _masterDataService.Chargeback(UserCurrentDetail.WalletUserId);
                                }
                                else
                                {
                                    //test

                                }
                            }
                            else
                            {
                                //test

                            }
                            //sdfsdfd
                        }
                        else
                        {

                            response.RstKey = 2;
                        }
                    }
                    else
                    {
                        await _cardPaymentRepository.UpdateTransactionInitiateRequest(getInitialTransaction);
                        response.RstKey = 3;

                    }



                }
                else
                {
                    response.RstKey = 3;
                }

            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("CardPaymentRepository", "SaveMasterCardPayment2Response", request);
            }
            return response;
        }


        public async Task<flutterPaymentUrlResponse> GetCardPaymentUrlForflutterwave(ThirdpartyPaymentByCardRequest request, string headerToken)
        {
            var response = new flutterPaymentUrlResponse();
            var _commission = new CalculateCommissionResponse();
            var _commissionRequest = new CalculateCommissionRequest();

            var transationInitiate = new TransactionInitiateRequest();
            var _thirdPartyPaymentByCard = new ThirdPartyPaymentByCard();

            try
            {

                var UserDetail = await _walletUserService.UserProfile(headerToken);

                if (UserDetail.StdCode != "+234")
                {
                    response.RstKey = 6;
                    response.Message = "This Option is currently disable.";
                    return response;
                }

                var Isdocverified = await _walletUserRepository.IsDocVerifiedMOMO(UserDetail.DocumetStatus);

                var resultTL = await _transactionLimitAUService.CheckTransactionLimitAU(request.WalletUserId.ToString()); //check New TL
                var transactionLimit = await _masterDataRepository.GetTransactionLimitAddMoney(Convert.ToString(UserDetail.WalletUserId));
                int limit = transactionLimit != null ? Convert.ToInt32(transactionLimit.TransactionLimitForAddMoney) : 0;

                var transactionHistory = _masterDataRepository.GetAllTransactionsAddMoney(UserDetail.WalletUserId);
                int totalAmountTransfered = transactionHistory != null ? Convert.ToInt32(transactionHistory.totalAmount) : 0;

                #region Calculate commission on request amount               

                //int WalletServiceId = await _cardPaymentRepository.GetServiceId();
                var WalletService = await _cardPaymentRepository.GetWalletService("FlutterXOF", 57);


                if (UserDetail.IsActive == true)//am
                {
                    if (UserDetail.IsEmailVerified == true)
                    {
                        if (Isdocverified == true)
                        {
                            if (transactionLimit == null || transactionLimit.TransactionLimitForAddMoney == null || limit >= (Convert.ToDecimal(request.Amount) + totalAmountTransfered))
                            {
                                if (WalletService.WalletServiceId > 0)
                                {
                                    #region Calculate Commission on request amount
                                    _commissionRequest.IsRoundOff = true;
                                    _commissionRequest.TransactionAmount = Convert.ToDecimal(request.Amount);
                                    _commissionRequest.WalletServiceId = WalletService.WalletServiceId;
                                    _commission = await _setCommisionRepository.CalculateCommission(_commissionRequest);
                                    #endregion
                                }


                                decimal amountWithCommision = decimal.Parse(string.Format("{0:0,0}", _commission.AmountWithCommission));    // "1,234,257";

                                #endregion
                                if (resultTL != null)
                                {
                                    decimal SetAmount = Convert.ToDecimal(resultTL.SetAmount);// returns decimal
                                    decimal TotalAmount = Convert.ToDecimal(resultTL.TotalAmount);// returns decimal

                                    if (SetAmount != 0) //0 =msg 
                                    {
                                        decimal requestAmountwithcomm = amountWithCommision + TotalAmount;

                                        if (requestAmountwithcomm <= SetAmount)//1000 >= 1000
                                        {

                                        }
                                        else
                                        {
                                            response.RstKey = 6;
                                            response.Message = ResponseMessages.INVALID_txnAmountREQUEST;
                                            return response;
                                        }
                                    }
                                }
                                var invoiceNumber = await _masterDataRepository.GetInvoiceNumber();
                                var _masterCard = new MasterCardPaymentRequest();

                                _masterCard.SessionId = null;
                                _masterCard.Version = null;
                                _masterCard.SuccessIndicator = null;
                                _masterCard.Merchant = "FlutterXOF";
                                _masterCard.IsActive = true;
                                _masterCard.IsDeleted = false;
                                _masterCard.CreatedDate = DateTime.UtcNow;
                                _masterCard.UpdatedDate = DateTime.UtcNow;
                                _masterCard.Amount = request.Amount;
                                _masterCard.CommisionCharges = _commission.CommisionPercent;
                                _masterCard.TotalAmount = Convert.ToString(amountWithCommision);
                                _masterCard.WalletUserId = UserDetail.WalletUserId;
                                _masterCard.TransactionNo = invoiceNumber.InvoiceNumber;
                                _masterCard.FlatCharges = _commission.FlatCharges;
                                _masterCard.BenchmarkCharges = _commission.BenchmarkCharges;
                                _masterCard.CommissionAmount = _commission.CommissionAmount.ToString();
                                await _cardPaymentRepository.SaveMasterCardPaymentRequest(_masterCard);


                                var _Requestcustomer = new customer();
                                _Requestcustomer.email = UserDetail.EmailId;

                                var _RequestAttributes = new flutterRequest();
                                _RequestAttributes.currency = "XOF";
                                _RequestAttributes.tx_ref = invoiceNumber.InvoiceNumber;
                                // _RequestAttributes.redirect_url = null;
                                _RequestAttributes.redirect_url = CommonSetting.flutterCallBackUrl;
                                _RequestAttributes.amount = Convert.ToString(amountWithCommision);
                                _RequestAttributes.customer = _Requestcustomer;
                                _RequestAttributes.payment_options = "card";

                                var req = JsonConvert.SerializeObject(_RequestAttributes);

                                _logUtils.WriteTextToFileForFlutterPeyLoadLogs("SaveflutterCardRequestDetail :- InvoiceNumber " + invoiceNumber.InvoiceNumber + " " + req);
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                //here to get psaymenturl
                                var responseData2 = await GethashorUrl(req, null, "flutterUrl");
                                var _responseModel2 = JsonConvert.DeserializeObject<flutterPaymentUrlResponse>(responseData2);
                                if (_responseModel2.data.link != null && _responseModel2.status == "success")
                                {
                                    transationInitiate.InvoiceNumber = invoiceNumber.InvoiceNumber;
                                    transationInitiate.ReceiverNumber = UserDetail.MobileNo;
                                    transationInitiate.ServiceName = "FlutterXOF";
                                    transationInitiate.RequestedAmount = _commission.TransactionAmount.ToString();
                                    transationInitiate.TransactionStatus = (int)TransactionStatus.Pending;
                                    transationInitiate.WalletUserId = UserDetail.WalletUserId;
                                    transationInitiate.UserReferanceNumber = invoiceNumber.AutoDigit;
                                    transationInitiate.CurrentBalance = UserDetail.CurrentBalance;
                                    transationInitiate.AfterTransactionBalance = "";
                                    transationInitiate.ReceiverCurrentBalance = UserDetail.CurrentBalance;
                                    transationInitiate.UserName = UserDetail.FirstName + " " + UserDetail.LastName;
                                    transationInitiate.ReceiverWalletUserId = UserDetail.WalletUserId;
                                    transationInitiate.CreatedDate = DateTime.UtcNow;
                                    transationInitiate.UpdatedDate = DateTime.UtcNow;
                                    transationInitiate.IsActive = true;
                                    transationInitiate.IsDeleted = false;
                                    transationInitiate.JsonRequest = responseData2;
                                    transationInitiate.JsonResponse = "";
                                    transationInitiate = await _cardPaymentRepository.SaveTransactionInitiateRequest(transationInitiate);
                                    response.URL = _responseModel2.data.link;
                                    response.RstKey = 2;
                                }

                                else
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.Message = "Please try after some time aggregator error.";
                                }
                                LogTransactionTypes.Request.SaveTransactionLog(LogTransactionNameTypes.AddMoney + "Credit TO Debit Cards", responseData2, "Request Url : " + response.data.link);
                            }
                            else
                            {
                                var addLimit = limit - (Convert.ToDecimal(request.Amount) + totalAmountTransfered);
                                if (addLimit < Convert.ToDecimal(request.Amount))
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.Message = "Exceed your transaction limit.";
                                }
                                else
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.Message = "You can not add more then " + transactionLimit.TransactionLimitForAddMoney.ToString() + " cedi in a day";
                                }
                            }
                        }
                        else if (UserDetail.DocumetStatus == 0 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
                        {
                            response.RstKey = 13;
                            response.Message = ResponseMessageKyc.FAILED_Doc_NotUploaded;
                        }
                        else if (UserDetail.DocumetStatus == 1 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
                        {
                            response.RstKey = 14;
                            response.Message = ResponseMessageKyc.FAILED_Doc_Pending;
                        }
                        else if (UserDetail.DocumetStatus == 4 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
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
                        response.RstKey = 6;
                        response.StatusCode = (int)TransactionStatus.Failed;
                        response.Message = ResponseMessages.EMAIL_VERIFICATION_PENDING;
                    }
                }
                else
                {
                    response.RstKey = 6;
                    response.Message = ResponseMessages.TRANSACTION_DISABLED;
                }
            }

            catch (Exception ex)
            {

                //tran.Rollback();
            }
            return response;

        }

        public async Task<AddMoneyAggregatorResponse> SaveflutterCardPaymentResponse(fluttercallbackResponse request)
        {
            AddMoneyAggregatorResponse response = new AddMoneyAggregatorResponse();
            CalculateCommissionResponse _commission = new CalculateCommissionResponse();
            CalculateCommissionRequest _commissionRequest = new CalculateCommissionRequest();
            string RequestString = JsonConvert.SerializeObject(request);
            LogTransactionTypes.Response.SaveTransactionLog(LogTransactionNameTypes.AddMoney + "Credit to Debit Cards", RequestString, "Responce by bank detail : ");

            try
            {

                var getInitialTransaction = await _cardPaymentRepository.GetTransactionInitiateRequest(request.tx_ref);

                int GetWalletTransactionsexist = await _cardPaymentRepository.GetWalletTransactionsexist(getInitialTransaction.WalletUserId, request.tx_ref);

                if (request.tx_ref != null && GetWalletTransactionsexist == 0)
                {

                    getInitialTransaction.JsonResponse = JsonConvert.SerializeObject(request);
                    var responseData2 = await GethashorUrl(request.tx_ref, null, "flutterUrlverify");

                    JavaScriptSerializer js = new JavaScriptSerializer();
                    dynamic blogObject = js.Deserialize<dynamic>(responseData2);


                    var txnreverifystatus = blogObject["data"]["status"];//stagin

                    //check txn verify flutter --when not succesfujl statsu got from txn verify & suceeful get from cllback 
                    if (txnreverifystatus != "successful" && request.status == "successful")
                    {
                        WalletService WalletService = new WalletService();
                        response.InvoiceNo = request.tx_ref;
                        response.Amount = getInitialTransaction.RequestedAmount;
                        //response.status = "flutter";
                        if (getInitialTransaction.ServiceName == "FlutterXOF")
                        {
                            response.status = "Flutter XOF";
                            WalletService = await _cardPaymentRepository.GetWalletService("FlutterXOF", 57);
                        }
                        else if (getInitialTransaction.ServiceName == "FlutterUSD")
                        {
                            response.status = "Flutter USD";
                            WalletService = await _cardPaymentRepository.GetWalletService("FlutterUSD", 58);
                        }
                        else if (getInitialTransaction.ServiceName == "FlutterEURO")
                        {
                            response.status = "Flutter EURO";
                            WalletService = await _cardPaymentRepository.GetWalletService("FlutterEURO", 59);
                        }

                        DateTime TDate = DateTime.UtcNow;
                        response.TransactionDate = TDate;

                        //int WalletServiceId = await _cardPaymentRepository.GetServiceId();
                        if (WalletService.WalletServiceId > 0)
                        {
                            var adminUser = await _cardPaymentRepository.GetAdminUser();
                            if (adminUser != null)
                            {
                                getInitialTransaction.RequestedAmount = Convert.ToString(Math.Round(Convert.ToDecimal(getInitialTransaction.RequestedAmount), 2));

                                long userId = Convert.ToInt32(getInitialTransaction.WalletUserId);
                                var UserCurrentDetail = await _walletUserRepository.GetCurrentUser(userId);
                                if (UserCurrentDetail != null && await _cardPaymentRepository.IsWalletTransactions(UserCurrentDetail.WalletUserId, request.tx_ref))
                                {
                                    getInitialTransaction.RequestedAmount = Convert.ToString(Math.Round(Convert.ToDecimal(getInitialTransaction.RequestedAmount), 2));

                                    _commissionRequest.CurrentBalance = Convert.ToDecimal(UserCurrentDetail.CurrentBalance);
                                    _commissionRequest.IsRoundOff = true;
                                    _commissionRequest.TransactionAmount = Convert.ToDecimal(getInitialTransaction.RequestedAmount); //change
                                    _commissionRequest.WalletServiceId = WalletService.WalletServiceId;
                                    _commission = await _setCommisionRepository.CalculateCommission(_commissionRequest);

                                    getInitialTransaction.TransactionStatus = (int)TransactionStatus.Pending;
                                    await _cardPaymentRepository.UpdateTransactionInitiateRequest(getInitialTransaction);

                                    #region Save Transaction
                                    decimal amountWithCommision = decimal.Parse(string.Format("{0:0,0}", _commission.AmountWithCommission));    // "1,234,257";

                                    var _Transaction = new WalletTransaction();

                                    _Transaction.TransactionInitiateRequestId = getInitialTransaction.Id;
                                    _Transaction.MerchantCommissionAmount = Convert.ToString(_commission.MerchantCommissionAmount);
                                    _Transaction.MerchantCommissionId = _commission.MerchantCommissionId;
                                    _Transaction.TransactionType = AggragatorServiceType.CREDIT;
                                    _Transaction.IsBankTransaction = false;
                                    _Transaction.TransactionTypeInfo = (int)TransactionTypeInfo.AddedByCard;
                                    _Transaction.IsBankTransaction = false;
                                    _Transaction.Comments = string.Empty;
                                    _Transaction.InvoiceNo = getInitialTransaction.InvoiceNumber;
                                    _Transaction.CommisionAmount = Convert.ToString(_commission.CommissionAmount);
                                    _Transaction.CommisionId = _commission.CommissionId;
                                    _Transaction.WalletAmount = Convert.ToString(_commission.TransactionAmount);

                                    _Transaction.TotalAmount = Convert.ToString(amountWithCommision);
                                    _Transaction.ServiceTaxRate = _commission.ServiceTaxRate;
                                    _Transaction.ServiceTax = Convert.ToString(_commission.ServiceTaxAmount);
                                    _Transaction.OperatorType = WalletService.ServiceName;

                                    _Transaction.AccountNo = getInitialTransaction.ReceiverNumber;

                                    _Transaction.TransactionStatus = (int)TransactionStatus.Pending;

                                    _Transaction.WalletServiceId = WalletService.WalletServiceId;
                                    _Transaction.SenderId = UserCurrentDetail.WalletUserId;
                                    _Transaction.ReceiverId = UserCurrentDetail.WalletUserId;
                                    _Transaction.BankBranchCode = string.Empty;
                                    _Transaction.BankTransactionId = request.transaction_id;
                                    _Transaction.TransactionId = getInitialTransaction.InvoiceNumber;


                                    _Transaction.IsAdminTransaction = false;
                                    _Transaction.IsActive = true;
                                    _Transaction.IsDeleted = false;
                                    _Transaction.CreatedDate = TDate;
                                    _Transaction.UpdatedDate = TDate;
                                    _Transaction.TransactionTypeInfo = (int)TransactionTypeInfo.AddedByCard;
                                    _Transaction.IsAddDuringPay = false;
                                    _Transaction.VoucherCode = string.Empty;

                                    await _cardPaymentRepository.SaveWalletTransactions(_Transaction);
                                    //db.WalletTransactions.Add(_Transaction);
                                    //db.SaveChanges();
                                    #endregion

                                    #region Credit
                                    var _credit = new WalletTransactionDetail();
                                    _credit.Amount = Convert.ToString(_commission.TransactionAmount);
                                    _credit.TransactionType = (int)TransactionDetailType.Credit;
                                    _credit.WalletUserId = adminUser.WalletUserId;
                                    _credit.WalletTransactionId = _Transaction.WalletTransactionId;
                                    _credit.IsActive = true;
                                    _credit.IsDeleted = false;
                                    _credit.CreatedDate = TDate;
                                    _credit.UpdatedDate = TDate;
                                    //db.WalletTransactionDetails.Add(_credit);
                                    //db.SaveChanges();
                                    await _cardPaymentRepository.SaveWalletTransactionDetails(_credit);
                                    #endregion

                                    #region Debit
                                    var _debit = new WalletTransactionDetail();
                                    _debit.Amount = Convert.ToString(_commission.TransactionAmount);
                                    _debit.TransactionType = (int)TransactionDetailType.Debit;
                                    _debit.WalletUserId = UserCurrentDetail.WalletUserId;
                                    _debit.WalletTransactionId = _Transaction.WalletTransactionId;
                                    _debit.IsActive = true;
                                    _debit.IsDeleted = false;
                                    _debit.CreatedDate = TDate;
                                    _debit.UpdatedDate = TDate;
                                    //db.WalletTransactionDetails.Add(_credit);
                                    //db.SaveChanges();
                                    await _cardPaymentRepository.SaveWalletTransactionDetails(_credit);
                                    #endregion

                                    //get UpdateNewCardNoResponseBankCode id
                                    await _cardPaymentRepository.UpdateNewCardNoResponseBankCode(getInitialTransaction.InvoiceNumber, UserCurrentDetail.WalletUserId, request.transaction_id);


                                    var adminKeyPair = AES256.AdminKeyPair;


                                    //db.SaveChanges();
                                    //tran.Commit();


                                    if (UserCurrentDetail.MobileNo != null && UserCurrentDetail.MobileNo != "")
                                    {
                                        response.ToMobileNo = AES256.Decrypt(adminKeyPair.PrivateKey, UserCurrentDetail.MobileNo);
                                    }


                                    response.CurrentBalance = UserCurrentDetail.CurrentBalance;
                                    response.RstKey = 2;



                                }
                                else
                                {
                                    //test

                                }
                            }
                            else
                            {
                                //test

                            }
                            //sdfsdfd
                        }
                        else
                        {

                            response.RstKey = 2;
                        }
                    }
                    //after verify flutter txn then crediht to user --when succesfujl statsu got from both
                    else if (request.tx_ref != null && request.transaction_id != null && request.status == "successful" && txnreverifystatus == "successful")
                    {
                        response.InvoiceNo = request.tx_ref;
                        response.Amount = getInitialTransaction.RequestedAmount;
                        //response.status = "flutter";
                        WalletService WalletService = new WalletService();
                        if (getInitialTransaction.ServiceName == "FlutterXOF")
                        {
                            response.status = "Flutter XOF";
                            WalletService = await _cardPaymentRepository.GetWalletService("FlutterXOF", 57);
                        }
                        else if (getInitialTransaction.ServiceName == "FlutterUSD")
                        {
                            response.status = "Flutter USD";
                            WalletService = await _cardPaymentRepository.GetWalletService("FlutterUSD", 58);
                        }
                        else if (getInitialTransaction.ServiceName == "FlutterEURO")
                        {
                            response.status = "Flutter EURO";
                            WalletService = await _cardPaymentRepository.GetWalletService("FlutterEURO", 59);
                        }
                        DateTime TDate = DateTime.UtcNow;
                        response.TransactionDate = TDate;

                        //int WalletServiceId = await _cardPaymentRepository.GetServiceId();
                        if (WalletService.WalletServiceId > 0)
                        {
                            var adminUser = await _cardPaymentRepository.GetAdminUser();
                            if (adminUser != null)
                            {

                                // var receiver = await _walletUserRepository.GetCurrentUser(Convert.ToInt32(requestDetail.WalletUserId)); //db.WalletUsers.Where(x => x.WalletUserId == requestDetail.WalletUserId).FirstOrDefault();

                                long userId = Convert.ToInt32(getInitialTransaction.WalletUserId);
                                var UserCurrentDetail = await _walletUserRepository.GetCurrentUser(userId);
                                if (UserCurrentDetail != null && await _cardPaymentRepository.IsWalletTransactions(UserCurrentDetail.WalletUserId, request.tx_ref))
                                {
                                    //this line commented due to currentbalance is not added to card expected 
                                    //request.vpc_Amount = Convert.ToString(Math.Round((Convert.ToDecimal(requestDetail.Amount) / 100), 2));
                                    getInitialTransaction.RequestedAmount = Convert.ToString(Math.Round(Convert.ToDecimal(getInitialTransaction.RequestedAmount), 2));

                                    // to update wallet amount-----

                                    // db.WalletUsers.FirstOrDefault(x => x.WalletUserId == receiver.WalletUserId);

                                    if (UserCurrentDetail != null)
                                    {
                                        _commissionRequest.CurrentBalance = Convert.ToDecimal(UserCurrentDetail.CurrentBalance);
                                        _commissionRequest.IsRoundOff = true;
                                        //_commissionRequest.TransactionAmount = Convert.ToDecimal(request.vpc_Amount);

                                        _commissionRequest.TransactionAmount = Convert.ToDecimal(getInitialTransaction.RequestedAmount); //change
                                        _commissionRequest.WalletServiceId = WalletService.WalletServiceId;
                                        _commission = await _setCommisionRepository.CalculateCommission(_commissionRequest);

                                        if (!string.IsNullOrEmpty(request.tx_ref))
                                        {
                                            getInitialTransaction.TransactionStatus = (int)TransactionStatus.Completed;
                                            if (Convert.ToDecimal(UserCurrentDetail.CurrentBalance) >= 0)
                                            {
                                                if (Convert.ToDecimal(UserCurrentDetail.CurrentBalance) == 0)
                                                {
                                                    UserCurrentDetail.CurrentBalance = _commission.TransactionAmount.ToString();
                                                    getInitialTransaction.AfterTransactionBalance = UserCurrentDetail.CurrentBalance.ToString();
                                                    getInitialTransaction.ReceiverCurrentBalance = UserCurrentDetail.CurrentBalance.ToString();
                                                }
                                                else
                                                {
                                                    UserCurrentDetail.CurrentBalance = Math.Round(Convert.ToDecimal(UserCurrentDetail.CurrentBalance) + _commission.TransactionAmount, 2).ToString();
                                                    getInitialTransaction.AfterTransactionBalance = UserCurrentDetail.CurrentBalance.ToString();
                                                    getInitialTransaction.ReceiverCurrentBalance = UserCurrentDetail.CurrentBalance.ToString();
                                                }
                                            }
                                            else
                                            {
                                                UserCurrentDetail.CurrentBalance = Math.Round(_commission.TransactionAmount - Convert.ToDecimal(UserCurrentDetail.CurrentBalance), 2).ToString();
                                                getInitialTransaction.AfterTransactionBalance = UserCurrentDetail.CurrentBalance.ToString();
                                                getInitialTransaction.ReceiverCurrentBalance = UserCurrentDetail.CurrentBalance.ToString();
                                            }
                                        }
                                        await _walletUserRepository.UpdateUserDetail(UserCurrentDetail);
                                        await _cardPaymentRepository.UpdateTransactionInitiateRequest(getInitialTransaction);
                                        // db.SaveChanges();
                                    }

                                    #region Save Transaction
                                    decimal amountWithCommision = decimal.Parse(string.Format("{0:0,0}", _commission.AmountWithCommission));    // "1,234,257";

                                    var _Transaction = new WalletTransaction();

                                    _Transaction.TransactionInitiateRequestId = getInitialTransaction.Id;
                                    _Transaction.MerchantCommissionAmount = Convert.ToString(_commission.MerchantCommissionAmount);
                                    _Transaction.MerchantCommissionId = _commission.MerchantCommissionId;
                                    _Transaction.TransactionType = AggragatorServiceType.CREDIT;
                                    _Transaction.IsBankTransaction = false;
                                    _Transaction.TransactionTypeInfo = (int)TransactionTypeInfo.AddedByCard;
                                    _Transaction.IsBankTransaction = false;
                                    _Transaction.Comments = string.Empty;
                                    _Transaction.InvoiceNo = getInitialTransaction.InvoiceNumber;
                                    _Transaction.CommisionAmount = Convert.ToString(_commission.CommissionAmount);
                                    _Transaction.CommisionId = _commission.CommissionId;
                                    _Transaction.WalletAmount = Convert.ToString(_commission.TransactionAmount);

                                    _Transaction.TotalAmount = Convert.ToString(amountWithCommision);
                                    _Transaction.ServiceTaxRate = _commission.ServiceTaxRate;
                                    _Transaction.ServiceTax = Convert.ToString(_commission.ServiceTaxAmount);
                                    _Transaction.OperatorType = WalletService.ServiceName;

                                    _Transaction.AccountNo = getInitialTransaction.ReceiverNumber;
                                    if (!string.IsNullOrEmpty(request.tx_ref))
                                    {

                                        _Transaction.TransactionStatus = (int)TransactionStatus.Completed; ;
                                        try
                                        {
                                            //--------send mail on success transaction--------

                                            var AdminKeys = AES256.AdminKeyPair;
                                            string FirstName = AES256.Decrypt(UserCurrentDetail.PrivateKey, UserCurrentDetail.FirstName);
                                            string LastName = AES256.Decrypt(UserCurrentDetail.PrivateKey, UserCurrentDetail.LastName);
                                            string StdCode = UserCurrentDetail.StdCode;
                                            string MobileNo = AES256.Decrypt(AdminKeys.PrivateKey, UserCurrentDetail.MobileNo);
                                            string EmailId = AES256.Decrypt(AdminKeys.PrivateKey, UserCurrentDetail.EmailId).Trim().ToLower();
                                            // var receiverDetail = new AppUserRepository().GetUserDetailById(receiver.WalletUserId);
                                            string filename = CommonSetting.successfullTransaction;


                                            var body = _sendEmails.ReadEmailformats(filename);
                                            body = body.Replace("$$FirstName$$", FirstName + " " + LastName);
                                            body = body.Replace("$$DisplayContent$$", "Flutter CARD");
                                            body = body.Replace("$$customer$$", MobileNo);
                                            body = body.Replace("$$amount$$", "XOF " + getInitialTransaction.RequestedAmount);
                                            body = body.Replace("$$ServiceTaxAmount$$", "XOF " + _commission.CommissionAmount);
                                            body = body.Replace("$$AmountWithCommission$$", "XOF " + amountWithCommision);
                                            body = body.Replace("$$TransactionId$$", request.tx_ref);

                                            var req = new EmailModel()
                                            {
                                                TO = EmailId,
                                                Subject = "Transaction Successfull",
                                                Body = body
                                            };
                                            _sendEmails.SendEmail(req);
                                        }
                                        catch
                                        {

                                        }
                                    }
                                    else
                                    {
                                        _Transaction.TransactionStatus = (int)TransactionStatus.Failed;
                                    }
                                    _Transaction.WalletServiceId = WalletService.WalletServiceId;
                                    _Transaction.SenderId = UserCurrentDetail.WalletUserId;
                                    _Transaction.ReceiverId = UserCurrentDetail.WalletUserId;
                                    _Transaction.BankBranchCode = string.Empty;
                                    _Transaction.BankTransactionId = request.transaction_id;
                                    _Transaction.TransactionId = getInitialTransaction.InvoiceNumber;


                                    _Transaction.IsAdminTransaction = false;
                                    _Transaction.IsActive = true;
                                    _Transaction.IsDeleted = false;
                                    _Transaction.CreatedDate = TDate;
                                    _Transaction.UpdatedDate = TDate;
                                    _Transaction.TransactionTypeInfo = (int)TransactionTypeInfo.AddedByCard;
                                    _Transaction.IsAddDuringPay = false;
                                    _Transaction.VoucherCode = string.Empty;

                                    await _cardPaymentRepository.SaveWalletTransactions(_Transaction);
                                    //db.WalletTransactions.Add(_Transaction);
                                    //db.SaveChanges();
                                    #endregion

                                    #region Credit
                                    var _credit = new WalletTransactionDetail();
                                    _credit.Amount = Convert.ToString(_commission.TransactionAmount);
                                    _credit.TransactionType = (int)TransactionDetailType.Credit;
                                    _credit.WalletUserId = adminUser.WalletUserId;
                                    _credit.WalletTransactionId = _Transaction.WalletTransactionId;
                                    _credit.IsActive = true;
                                    _credit.IsDeleted = false;
                                    _credit.CreatedDate = TDate;
                                    _credit.UpdatedDate = TDate;
                                    //db.WalletTransactionDetails.Add(_credit);
                                    //db.SaveChanges();
                                    await _cardPaymentRepository.SaveWalletTransactionDetails(_credit);
                                    #endregion

                                    #region Debit
                                    var _debit = new WalletTransactionDetail();
                                    _debit.Amount = Convert.ToString(_commission.TransactionAmount);
                                    _debit.TransactionType = (int)TransactionDetailType.Debit;
                                    _debit.WalletUserId = UserCurrentDetail.WalletUserId;
                                    _debit.WalletTransactionId = _Transaction.WalletTransactionId;
                                    _debit.IsActive = true;
                                    _debit.IsDeleted = false;
                                    _debit.CreatedDate = TDate;
                                    _debit.UpdatedDate = TDate;
                                    //db.WalletTransactionDetails.Add(_credit);
                                    //db.SaveChanges();
                                    await _cardPaymentRepository.SaveWalletTransactionDetails(_credit);
                                    #endregion

                                    //get UpdateNewCardNoResponseBankCode id
                                    await _cardPaymentRepository.UpdateNewCardNoResponseBankCode(getInitialTransaction.InvoiceNumber, UserCurrentDetail.WalletUserId, request.transaction_id);
                                    //updatfe webhook when callback receive
                                    await _cardPaymentRepository.Updatewebhookflutterflagsuccestxninvoiceno(getInitialTransaction.InvoiceNumber);
                                    var adminKeyPair = AES256.AdminKeyPair;
                                    //db.SaveChanges();
                                    //tran.Commit();
                                    #region PushNotification

                                    var CurrentUser = await _walletUserRepository.GetCurrentUser(UserCurrentDetail.WalletUserId);//db.WalletUsers.Where(x => x.WalletUserId == receiver.WalletUserId).FirstOrDefault();
                                    if (CurrentUser != null)
                                    {
                                        PushNotificationModel push = new PushNotificationModel();
                                        push.SenderId = UserCurrentDetail.WalletUserId;
                                        push.deviceType = (int)UserCurrentDetail.DeviceType;
                                        push.deviceKey = UserCurrentDetail.DeviceToken;
                                        PayMoneyPushModel pushModel = new PayMoneyPushModel();
                                        pushModel.TransactionDate = TDate;
                                        pushModel.TransactionId = request.tx_ref;
                                        pushModel.CurrentBalance = CurrentUser.CurrentBalance;
                                        pushModel.alert = _Transaction.WalletAmount + " XOF has been credited to your account.";
                                        pushModel.Amount = getInitialTransaction.RequestedAmount;
                                        pushModel.CurrentBalance = CurrentUser.CurrentBalance;
                                        pushModel.pushType = (int)PushType.ADDMONEY;

                                        if ((int)UserCurrentDetail.DeviceType == (int)DeviceTypes.ANDROID || (int)UserCurrentDetail.DeviceType == (int)DeviceTypes.Web)
                                        {
                                            PushPayload<PayMoneyPushModel> aps = new PushPayload<PayMoneyPushModel>();
                                            PushPayloadData<PayMoneyPushModel> _data = new PushPayloadData<PayMoneyPushModel>();
                                            _data.notification = pushModel;
                                            aps.data = _data;
                                            aps.to = UserCurrentDetail.DeviceToken;
                                            aps.collapse_key = string.Empty;
                                            push.message = JsonConvert.SerializeObject(aps);
                                            push.payload = pushModel;
                                        }
                                        if ((int)UserCurrentDetail.DeviceType == (int)DeviceTypes.IOS)
                                        {
                                            NotificationJsonResponse<PayMoneyIOSPushModel> aps = new NotificationJsonResponse<PayMoneyIOSPushModel>();
                                            PayMoneyIOSPushModel _iosPushModel = new PayMoneyIOSPushModel();
                                            _iosPushModel.alert = pushModel.alert;
                                            _iosPushModel.Amount = pushModel.Amount;
                                            _iosPushModel.CurrentBalance = pushModel.CurrentBalance;
                                            _iosPushModel.MobileNo = pushModel.MobileNo;
                                            _iosPushModel.SenderName = pushModel.SenderName;
                                            _iosPushModel.pushType = pushModel.pushType;
                                            aps.aps = _iosPushModel;

                                            push.message = JsonConvert.SerializeObject(aps);
                                        }
                                        //if (!string.IsNullOrEmpty(push.message))
                                        //{
                                        //    new PushNotificationRepository().sendPushNotification(push);
                                        //}
                                    }
                                    #endregion

                                    if (UserCurrentDetail.MobileNo != null && UserCurrentDetail.MobileNo != "")
                                    {
                                        response.ToMobileNo = AES256.Decrypt(adminKeyPair.PrivateKey, UserCurrentDetail.MobileNo);
                                    }


                                    response.CurrentBalance = UserCurrentDetail.CurrentBalance;
                                    response.RstKey = 1;

                                    ///
                                    await _masterDataService.Chargeback(UserCurrentDetail.WalletUserId);
                                    //get UpdateNewCardNoResponseBankCode id
                                    await _cardPaymentRepository.UpdateNewCardNoResponseBankCode(getInitialTransaction.InvoiceNumber, UserCurrentDetail.WalletUserId, request.transaction_id);

                                    await _cardPaymentRepository.Updatewebhookflutterflagsuccestxninvoiceno(getInitialTransaction.InvoiceNumber);
                                }
                                else
                                {
                                    //test

                                }
                            }
                            else
                            {
                                //test

                            }
                            //sdfsdfd
                        }
                        else
                        {

                            response.RstKey = 2;
                        }
                    }
                    else
                    {
                        await _cardPaymentRepository.UpdateTransactionInitiateRequest(getInitialTransaction);
                        response.RstKey = 3;

                        //response.TransactionResponseDescription = _req.vpc_ResponseCodeDescription;
                        //response.TransactionResponseCode = request.vpc_TxnResponseCode;
                    }



                }
                else
                {
                    response.RstKey = 3;
                }

            }
            catch (Exception ex)
            {

                // ex.Message.ErrorLog("CardPaymentService", "SaveflutterCardPaymentResponse", request);
                "MasterCardPaymentController".ErrorLog("CardPaymentService", "SaveflutterCardPaymentResponse", ex.StackTrace + " " + ex.Message);
            }
            return response;
        }

        //addmone:-nigeria debit card
        //public async Task<flutterbankResponse> GetCardPaymentUrlForNGNbankflutter(ThirdpartyPaymentByCardRequest request, string headerToken)
        //{
        //    var response = new flutterbankResponse();
        //    var _commission = new CalculateCommissionResponse();
        //    var _commissionRequest = new CalculateCommissionRequest();

        //    var transationInitiate = new TransactionInitiateRequest();
        //    //var _thirdPartyPaymentByCard = new ThirdPartyPaymentByCard();

        //    try
        //    {

        //        var UserDetail = await _walletUserService.UserProfile(headerToken);

        //        var Isdocverified = await _walletUserRepository.IsDocVerifiedMOMO(UserDetail.DocumetStatus);

        //        var resultTL = await _transactionLimitAUService.CheckTransactionLimitAU(request.WalletUserId.ToString()); //check New TL
        //        var transactionLimit = await _masterDataRepository.GetTransactionLimitAddMoney(Convert.ToString(UserDetail.WalletUserId));
        //        int limit = transactionLimit != null ? Convert.ToInt32(transactionLimit.TransactionLimitForAddMoney) : 0;

        //        var transactionHistory = _masterDataRepository.GetAllTransactionsAddMoney(UserDetail.WalletUserId);
        //        int totalAmountTransfered = transactionHistory != null ? Convert.ToInt32(transactionHistory.totalAmount) : 0;

        //        //------Get Currency Rate--------------
        //        var currencyDetail = _masterDataRepository.GetCurrencyRate();

        //        //decimal cediRate = Convert.ToDecimal(currencyDetail.CediRate); 
        //        decimal NGNRate = Convert.ToDecimal(currencyDetail.NGNRate);//
        //        //decimal CfaRate = Convert.ToDecimal(currencyDetail.LERate);
        //        decimal requestAmount = Convert.ToDecimal(request.Amount);//;

        //        #region Calculate commission on request amount               

        //        //int WalletServiceId = await _cardPaymentRepository.GetServiceId();

        //        var WalletServiceId = await _cardPaymentRepository.GetWalletService("AddBankFlutter", 40);


        //        if (UserDetail.IsActive == true)//am
        //        {
        //            if (UserDetail.IsEmailVerified == true)
        //            {
        //                if (Isdocverified == true)
        //                {
        //                    if (transactionLimit == null || transactionLimit.TransactionLimitForAddMoney == null || limit >= (Convert.ToDecimal(request.Amount) + totalAmountTransfered))
        //                    {
        //                        if (WalletServiceId.WalletServiceId > 0)
        //                        {
        //                            #region Calculate Commission on request amount
        //                            _commissionRequest.IsRoundOff = true;
        //                            _commissionRequest.TransactionAmount = Convert.ToDecimal(request.Amount);
        //                            _commissionRequest.WalletServiceId = WalletServiceId.WalletServiceId;
        //                            _commission = await _setCommisionRepository.CalculatePayNGNTransferAddMoneyCommission(_commissionRequest);
        //                            #endregion
        //                        }


        //                        decimal amountWithCommision = decimal.Parse(string.Format("{0:0,0}", _commission.AmountWithCommission));    // "1,234,257";

        //                        decimal amt = (_commission.AmountWithCommission * NGNRate); //xof to NGNRate
        //                        var finalAmt = Decimal.Parse(amt.ToString("0.00"));

        //                        #endregion
        //                        if (resultTL != null)
        //                        {
        //                            decimal SetAmount = Convert.ToDecimal(resultTL.SetAmount);// returns decimal
        //                            decimal TotalAmount = Convert.ToDecimal(resultTL.TotalAmount);// returns decimal

        //                            if (SetAmount != 0) //0 =msg 
        //                            {
        //                                decimal requestAmountwithcomm = amountWithCommision + TotalAmount;

        //                                if (requestAmountwithcomm <= SetAmount)//1000 >= 1000
        //                                {

        //                                }
        //                                else
        //                                {
        //                                    response.RstKey = 20;
        //                                    response.Message = ResponseMessages.INVALID_txnAmountREQUEST;
        //                                    return response;
        //                                }
        //                            }
        //                        }

        //                        var invoiceNumber = await _masterDataRepository.GetInvoiceNumber();
        //                        var _masterCard = new MasterCardPaymentRequest();

        //                        _masterCard.SessionId = null;
        //                        _masterCard.Version = null;
        //                        _masterCard.SuccessIndicator = null;
        //                        _masterCard.Merchant = "AddBankFlutter";
        //                        _masterCard.IsActive = true;
        //                        _masterCard.IsDeleted = false;
        //                        _masterCard.CreatedDate = DateTime.UtcNow;
        //                        _masterCard.UpdatedDate = DateTime.UtcNow;
        //                        _masterCard.Amount = request.Amount;
        //                        _masterCard.CommisionCharges = _commission.CommisionPercent;
        //                        _masterCard.TotalAmount = Convert.ToString(amountWithCommision);
        //                        _masterCard.WalletUserId = UserDetail.WalletUserId;
        //                        _masterCard.TransactionNo = invoiceNumber.InvoiceNumber;
        //                        _masterCard.FlatCharges = _commission.FlatCharges;
        //                        _masterCard.BenchmarkCharges = _commission.BenchmarkCharges;
        //                        _masterCard.CommissionAmount = _commission.CommissionAmount.ToString();
        //                        await _cardPaymentRepository.SaveMasterCardPaymentRequest(_masterCard);


        //                        var _RequestAttributes = new flutterbankRequest();

        //                        _RequestAttributes.amount = Convert.ToString(finalAmt);
        //                        _RequestAttributes.account_bank = request.ngnbank;
        //                        _RequestAttributes.account_number = request.accountNo;
        //                        _RequestAttributes.currency = "NGN";
        //                        _RequestAttributes.tx_ref = invoiceNumber.InvoiceNumber;
        //                        _RequestAttributes.email = UserDetail.EmailId;
        //                        _RequestAttributes.redirect_url = CommonSetting.flutterBankCallBackUrl;

        //                        if (request.ngnbank == "057")
        //                        {
        //                            var SenderDateofbirth = Convert.ToDateTime(request.zenithdob);
        //                            string cc = SenderDateofbirth.ToString("dd-MM-yyyy");
        //                            string myStr = String.Join("", cc.Split('-'));
        //                            _RequestAttributes.passcode = myStr;

        //                        }
        //                        else if (request.ngnbank == "044") //access bank required paramter
        //                        {
        //                            _RequestAttributes.fullname = UserDetail.FirstName + ' ' + UserDetail.LastName;
        //                        }

        //                        else if (request.ngnbank == "033") //uba bank required paramter
        //                        {
        //                            _RequestAttributes.bvn = request.bvn;
        //                        }

        //                        var req = JsonConvert.SerializeObject(_RequestAttributes);

        //                        _logUtils.WriteTextToFileForBankFlutterPeyLoadLogs("SaveBankFlutterRequestDetail :- InvoiceNumber " + invoiceNumber.InvoiceNumber + " " + req);

        //                        //here to get psaymenturl
        //                        //response.flw_ref = "URF_1650880294070_296235";                                
        //                        //response.type = "card";
        //                        //response.URL = "NO-URL";
        //                        //response.RstKey = 2;
        //                        var responseData2 = await GethashorUrl(req, null, "AddBankFlutter");
        //                        var _responseModel2 = JsonConvert.DeserializeObject<flutterbankResponse>(responseData2);

        //                        if (_responseModel2.data.auth_url != null && _responseModel2.status == "success")
        //                        {
        //                            transationInitiate.InvoiceNumber = invoiceNumber.InvoiceNumber;
        //                            transationInitiate.ReceiverNumber = UserDetail.MobileNo;
        //                            transationInitiate.ServiceName = "AddBankFlutter Card Payment";
        //                            transationInitiate.RequestedAmount = _commission.TransactionAmount.ToString();
        //                            transationInitiate.TransactionStatus = (int)TransactionStatus.Pending;
        //                            transationInitiate.WalletUserId = UserDetail.WalletUserId;
        //                            transationInitiate.UserReferanceNumber = invoiceNumber.AutoDigit;
        //                            transationInitiate.CurrentBalance = UserDetail.CurrentBalance;
        //                            transationInitiate.AfterTransactionBalance = "";
        //                            transationInitiate.ReceiverCurrentBalance = UserDetail.CurrentBalance;
        //                            transationInitiate.UserName = UserDetail.FirstName + " " + UserDetail.LastName;
        //                            transationInitiate.ReceiverWalletUserId = UserDetail.WalletUserId;
        //                            transationInitiate.CreatedDate = DateTime.UtcNow;
        //                            transationInitiate.UpdatedDate = DateTime.UtcNow;
        //                            transationInitiate.IsActive = true;
        //                            transationInitiate.IsDeleted = false;
        //                            transationInitiate.JsonRequest = responseData2;
        //                            transationInitiate.JsonResponse = "";
        //                            transationInitiate = await _cardPaymentRepository.SaveTransactionInitiateRequest(transationInitiate);
        //                            //response.URL = _responseModel2.data.auth_url;

        //                            if (_responseModel2.data.auth_url == "NO-URL")
        //                            {
        //                                response.flw_ref = _responseModel2.data.flw_ref;
        //                                response.processor_response = _responseModel2.data.processor_response;
        //                                response.type = "account";
        //                                response.URL = _responseModel2.data.auth_url;
        //                            }
        //                            else
        //                            {

        //                                response.URL = _responseModel2.data.auth_url;
        //                            }
        //                            response.RstKey = 2;
        //                        }

        //                        else
        //                        {
        //                            response.RstKey = 6;
        //                            response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
        //                            response.Message = "Please try after some time aggregator error.";
        //                        }
        //                        LogTransactionTypes.Request.SaveTransactionLog(LogTransactionNameTypes.AddMoney + "Credit TO Debit Cards", responseData2, "Request Url : " + response.data.auth_url);
        //                    }
        //                    else
        //                    {
        //                        var addLimit = limit - (Convert.ToDecimal(request.Amount) + totalAmountTransfered);
        //                        if (addLimit < Convert.ToDecimal(request.Amount))
        //                        {
        //                            response.RstKey = 6;
        //                            response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
        //                            response.Message = "Exceed your transaction limit.";
        //                        }
        //                        else
        //                        {
        //                            response.RstKey = 6;
        //                            response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
        //                            response.Message = "You can not add more then " + transactionLimit.TransactionLimitForAddMoney.ToString() + " cedi in a day";
        //                        }
        //                    }
        //                }
        //                else if (UserDetail.DocumetStatus == 0 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
        //                {
        //                    response.RstKey = 13;
        //                    response.Message = ResponseMessageKyc.FAILED_Doc_NotUploaded;
        //                }
        //                else if (UserDetail.DocumetStatus == 1 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
        //                {
        //                    response.RstKey = 14;
        //                    response.Message = ResponseMessageKyc.FAILED_Doc_Pending;
        //                }
        //                else if (UserDetail.DocumetStatus == 4 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
        //                {
        //                    response.RstKey = 15;
        //                    response.Message = ResponseMessageKyc.Doc_Not_visible;
        //                }
        //                else
        //                {
        //                    response.RstKey = 16;
        //                    response.Message = ResponseMessageKyc.Doc_Rejected;
        //                }
        //            }
        //            else
        //            {
        //                response.RstKey = 6;
        //                response.StatusCode = (int)TransactionStatus.Failed;
        //                response.Message = ResponseMessages.EMAIL_VERIFICATION_PENDING;
        //            }
        //        }
        //        else
        //        {
        //            response.RstKey = 6;
        //            response.Message = ResponseMessages.TRANSACTION_DISABLED;
        //        }
        //    }

        //    catch (Exception ex)
        //    {

        //        //tran.Rollback();
        //    }
        //    return response;

        //}

        //old add_mone :- bank debit reposne

        //public async Task<AddMoneyAggregatorResponse> SaveflutterBankPaymentResponse(BankPaymentWebResponse request)
        //{
        //    AddMoneyAggregatorResponse response = new AddMoneyAggregatorResponse();
        //    CalculateCommissionResponse _commission = new CalculateCommissionResponse();
        //    CalculateCommissionRequest _commissionRequest = new CalculateCommissionRequest();
        //    string RequestString = JsonConvert.SerializeObject(request);
        //    LogTransactionTypes.Response.SaveTransactionLog(LogTransactionNameTypes.AddMoney + "Credit to Debit Cards", RequestString, "Responce by bank detail : ");

        //    try
        //    {

        //        var getInitialTransaction = await _cardPaymentRepository.GetTransactionInitiateRequest(request.txRef);

        //        int GetWalletTransactionsexist = await _cardPaymentRepository.GetWalletTransactionsexist(getInitialTransaction.WalletUserId, request.txRef);

        //        if (request.txRef != null && GetWalletTransactionsexist == 0)
        //        {

        //            getInitialTransaction.JsonResponse = JsonConvert.SerializeObject(request);
        //            var responseData2 = await GethashorUrl(request.txRef, null, "flutterUrlverify");

        //            JavaScriptSerializer js = new JavaScriptSerializer();
        //            dynamic blogObject = js.Deserialize<dynamic>(responseData2);


        //            var txnreverifystatus = blogObject["data"]["status"];//stagin

        //            //check txn verify flutter --when not succesful statsu got from txn verify & suceeful get from cllback 
        //            if (txnreverifystatus != "successful" && request.status == "successful")
        //            {
        //                response.InvoiceNo = request.txRef;
        //                response.Amount = getInitialTransaction.RequestedAmount;
        //                response.status = "AddBankFlutter";
        //                DateTime TDate = DateTime.UtcNow;
        //                response.TransactionDate = TDate;

        //                //int WalletServiceId = await _cardPaymentRepository.GetServiceId();
        //                var WalletServiceId = await _cardPaymentRepository.GetWalletService("AddBankFlutter", 40);

        //                if (WalletServiceId.WalletServiceId > 0)
        //                {
        //                    var adminUser = await _cardPaymentRepository.GetAdminUser();
        //                    if (adminUser != null)
        //                    {
        //                        getInitialTransaction.RequestedAmount = Convert.ToString(Math.Round(Convert.ToDecimal(getInitialTransaction.RequestedAmount), 2));

        //                        long userId = Convert.ToInt32(getInitialTransaction.WalletUserId);
        //                        var UserCurrentDetail = await _walletUserRepository.GetCurrentUser(userId);
        //                        if (UserCurrentDetail != null && await _cardPaymentRepository.IsWalletTransactions(UserCurrentDetail.WalletUserId, request.txRef))
        //                        {
        //                            getInitialTransaction.RequestedAmount = Convert.ToString(Math.Round(Convert.ToDecimal(getInitialTransaction.RequestedAmount), 2));

        //                            _commissionRequest.CurrentBalance = Convert.ToDecimal(UserCurrentDetail.CurrentBalance);
        //                            _commissionRequest.IsRoundOff = true;
        //                            _commissionRequest.TransactionAmount = Convert.ToDecimal(getInitialTransaction.RequestedAmount); //change
        //                            _commissionRequest.WalletServiceId = WalletServiceId.WalletServiceId;
        //                            _commission = await _setCommisionRepository.CalculateCommission(_commissionRequest);

        //                            getInitialTransaction.TransactionStatus = (int)TransactionStatus.Pending;
        //                            await _cardPaymentRepository.UpdateTransactionInitiateRequest(getInitialTransaction);

        //                            #region Save Transaction
        //                            decimal amountWithCommision = decimal.Parse(string.Format("{0:0,0}", _commission.AmountWithCommission));    // "1,234,257";

        //                            var _Transaction = new WalletTransaction();

        //                            _Transaction.TransactionInitiateRequestId = getInitialTransaction.Id;
        //                            _Transaction.MerchantCommissionAmount = Convert.ToString(_commission.MerchantCommissionAmount);
        //                            _Transaction.MerchantCommissionId = _commission.MerchantCommissionId;
        //                            _Transaction.TransactionType = AggragatorServiceType.CREDIT;
        //                            _Transaction.IsBankTransaction = false;
        //                            _Transaction.TransactionTypeInfo = (int)TransactionTypeInfo.AddedByCard;
        //                            _Transaction.IsBankTransaction = false;
        //                            _Transaction.Comments = string.Empty;
        //                            _Transaction.InvoiceNo = getInitialTransaction.InvoiceNumber;
        //                            _Transaction.CommisionAmount = Convert.ToString(_commission.CommissionAmount);
        //                            _Transaction.CommisionId = _commission.CommissionId;
        //                            _Transaction.WalletAmount = Convert.ToString(_commission.TransactionAmount);

        //                            _Transaction.TotalAmount = Convert.ToString(amountWithCommision);
        //                            _Transaction.ServiceTaxRate = _commission.ServiceTaxRate;
        //                            _Transaction.ServiceTax = Convert.ToString(_commission.ServiceTaxAmount);
        //                            _Transaction.OperatorType = "AddBankFlutter";

        //                            _Transaction.AccountNo = getInitialTransaction.ReceiverNumber;

        //                            _Transaction.TransactionStatus = (int)TransactionStatus.Pending;

        //                            _Transaction.WalletServiceId = WalletServiceId.WalletServiceId;
        //                            _Transaction.SenderId = UserCurrentDetail.WalletUserId;
        //                            _Transaction.ReceiverId = UserCurrentDetail.WalletUserId;
        //                            _Transaction.BankBranchCode = string.Empty;
        //                            _Transaction.BankTransactionId = request.orderRef;
        //                            _Transaction.TransactionId = getInitialTransaction.InvoiceNumber;


        //                            _Transaction.IsAdminTransaction = false;
        //                            _Transaction.IsActive = true;
        //                            _Transaction.IsDeleted = false;
        //                            _Transaction.CreatedDate = TDate;
        //                            _Transaction.UpdatedDate = TDate;
        //                            _Transaction.TransactionTypeInfo = (int)TransactionTypeInfo.AddedByCard;
        //                            _Transaction.IsAddDuringPay = false;
        //                            _Transaction.VoucherCode = string.Empty;

        //                            await _cardPaymentRepository.SaveWalletTransactions(_Transaction);
        //                            //db.WalletTransactions.Add(_Transaction);
        //                            //db.SaveChanges();
        //                            #endregion

        //                            #region Credit
        //                            var _credit = new WalletTransactionDetail();
        //                            _credit.Amount = Convert.ToString(_commission.TransactionAmount);
        //                            _credit.TransactionType = (int)TransactionDetailType.Credit;
        //                            _credit.WalletUserId = adminUser.WalletUserId;
        //                            _credit.WalletTransactionId = _Transaction.WalletTransactionId;
        //                            _credit.IsActive = true;
        //                            _credit.IsDeleted = false;
        //                            _credit.CreatedDate = TDate;
        //                            _credit.UpdatedDate = TDate;
        //                            //db.WalletTransactionDetails.Add(_credit);
        //                            //db.SaveChanges();
        //                            await _cardPaymentRepository.SaveWalletTransactionDetails(_credit);
        //                            #endregion

        //                            #region Debit
        //                            var _debit = new WalletTransactionDetail();
        //                            _debit.Amount = Convert.ToString(_commission.TransactionAmount);
        //                            _debit.TransactionType = (int)TransactionDetailType.Debit;
        //                            _debit.WalletUserId = UserCurrentDetail.WalletUserId;
        //                            _debit.WalletTransactionId = _Transaction.WalletTransactionId;
        //                            _debit.IsActive = true;
        //                            _debit.IsDeleted = false;
        //                            _debit.CreatedDate = TDate;
        //                            _debit.UpdatedDate = TDate;
        //                            //db.WalletTransactionDetails.Add(_credit);
        //                            //db.SaveChanges();
        //                            await _cardPaymentRepository.SaveWalletTransactionDetails(_credit);
        //                            #endregion

        //                            //get UpdateNewCardNoResponseBankCode id
        //                            //await _cardPaymentRepository.UpdateNewCardNoResponseBankCode(getInitialTransaction.InvoiceNumber, UserCurrentDetail.WalletUserId, request.orderRef);


        //                            var adminKeyPair = AES256.AdminKeyPair;


        //                            //db.SaveChanges();
        //                            //tran.Commit();


        //                            if (UserCurrentDetail.MobileNo != null && UserCurrentDetail.MobileNo != "")
        //                            {
        //                                response.ToMobileNo = AES256.Decrypt(adminKeyPair.PrivateKey, UserCurrentDetail.MobileNo);
        //                            }


        //                            response.CurrentBalance = UserCurrentDetail.CurrentBalance;
        //                            response.RstKey = 2;



        //                        }
        //                        else
        //                        {
        //                            //test

        //                        }
        //                    }
        //                    else
        //                    {
        //                        //test

        //                    }
        //                    //sdfsdfd
        //                }
        //                else
        //                {

        //                    response.RstKey = 2;
        //                }
        //            }
        //            //after verify flutter txn then crediht to user --when succesfujl statsu got from both
        //            else if (request.txRef != null && request.orderRef != null && request.status == "successful" && txnreverifystatus == "successful")
        //            {
        //                response.InvoiceNo = request.txRef;
        //                response.Amount = getInitialTransaction.RequestedAmount;
        //                response.status = "AddBankFlutter";
        //                DateTime TDate = DateTime.UtcNow;
        //                response.TransactionDate = TDate;

        //                var WalletServiceId = await _cardPaymentRepository.GetWalletService("AddBankFlutter", 40);

        //                if (WalletServiceId.WalletServiceId > 0)
        //                {
        //                    var adminUser = await _cardPaymentRepository.GetAdminUser();
        //                    if (adminUser != null)
        //                    {

        //                        // var receiver = await _walletUserRepository.GetCurrentUser(Convert.ToInt32(requestDetail.WalletUserId)); //db.WalletUsers.Where(x => x.WalletUserId == requestDetail.WalletUserId).FirstOrDefault();

        //                        long userId = Convert.ToInt32(getInitialTransaction.WalletUserId);
        //                        var UserCurrentDetail = await _walletUserRepository.GetCurrentUser(userId);
        //                        if (UserCurrentDetail != null && await _cardPaymentRepository.IsWalletTransactions(UserCurrentDetail.WalletUserId, request.txRef))
        //                        {
        //                            //this line commented due to currentbalance is not added to card expected 
        //                            //request.vpc_Amount = Convert.ToString(Math.Round((Convert.ToDecimal(requestDetail.Amount) / 100), 2));
        //                            getInitialTransaction.RequestedAmount = Convert.ToString(Math.Round(Convert.ToDecimal(getInitialTransaction.RequestedAmount), 2));

        //                            // to update wallet amount-----

        //                            // db.WalletUsers.FirstOrDefault(x => x.WalletUserId == receiver.WalletUserId);

        //                            if (UserCurrentDetail != null)
        //                            {
        //                                _commissionRequest.CurrentBalance = Convert.ToDecimal(UserCurrentDetail.CurrentBalance);
        //                                _commissionRequest.IsRoundOff = true;
        //                                //_commissionRequest.TransactionAmount = Convert.ToDecimal(request.vpc_Amount);

        //                                _commissionRequest.TransactionAmount = Convert.ToDecimal(getInitialTransaction.RequestedAmount); //change
        //                                _commissionRequest.WalletServiceId = WalletServiceId.WalletServiceId;
        //                                _commission = await _setCommisionRepository.CalculatePayNGNTransferAddMoneyCommission(_commissionRequest);

        //                                if (!string.IsNullOrEmpty(request.txRef))
        //                                {
        //                                    getInitialTransaction.TransactionStatus = (int)TransactionStatus.Completed;
        //                                    if (Convert.ToDecimal(UserCurrentDetail.CurrentBalance) >= 0)
        //                                    {
        //                                        if (Convert.ToDecimal(UserCurrentDetail.CurrentBalance) == 0)
        //                                        {
        //                                            UserCurrentDetail.CurrentBalance = _commission.TransactionAmount.ToString();
        //                                            getInitialTransaction.AfterTransactionBalance = UserCurrentDetail.CurrentBalance.ToString();
        //                                            getInitialTransaction.ReceiverCurrentBalance = UserCurrentDetail.CurrentBalance.ToString();
        //                                        }
        //                                        else
        //                                        {
        //                                            UserCurrentDetail.CurrentBalance = Math.Round(Convert.ToDecimal(UserCurrentDetail.CurrentBalance) + _commission.TransactionAmount, 2).ToString();
        //                                            getInitialTransaction.AfterTransactionBalance = UserCurrentDetail.CurrentBalance.ToString();
        //                                            getInitialTransaction.ReceiverCurrentBalance = UserCurrentDetail.CurrentBalance.ToString();
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        UserCurrentDetail.CurrentBalance = Math.Round(_commission.TransactionAmount - Convert.ToDecimal(UserCurrentDetail.CurrentBalance), 2).ToString();
        //                                        getInitialTransaction.AfterTransactionBalance = UserCurrentDetail.CurrentBalance.ToString();
        //                                        getInitialTransaction.ReceiverCurrentBalance = UserCurrentDetail.CurrentBalance.ToString();
        //                                    }
        //                                }
        //                                await _walletUserRepository.UpdateUserDetail(UserCurrentDetail);
        //                                await _cardPaymentRepository.UpdateTransactionInitiateRequest(getInitialTransaction);
        //                                // db.SaveChanges();
        //                            }

        //                            #region Save Transaction
        //                            decimal amountWithCommision = decimal.Parse(string.Format("{0:0,0}", _commission.AmountWithCommission));    // "1,234,257";

        //                            var _Transaction = new WalletTransaction();

        //                            _Transaction.TransactionInitiateRequestId = getInitialTransaction.Id;
        //                            _Transaction.MerchantCommissionAmount = Convert.ToString(_commission.MerchantCommissionAmount);
        //                            _Transaction.MerchantCommissionId = _commission.MerchantCommissionId;
        //                            _Transaction.TransactionType = AggragatorServiceType.CREDIT;
        //                            _Transaction.IsBankTransaction = false;
        //                            _Transaction.TransactionTypeInfo = (int)TransactionTypeInfo.AddedByCard;
        //                            _Transaction.IsBankTransaction = false;
        //                            _Transaction.Comments = string.Empty;
        //                            _Transaction.InvoiceNo = getInitialTransaction.InvoiceNumber;
        //                            _Transaction.CommisionAmount = Convert.ToString(_commission.CommissionAmount);
        //                            _Transaction.CommisionId = _commission.CommissionId;
        //                            _Transaction.WalletAmount = Convert.ToString(_commission.TransactionAmount);

        //                            _Transaction.TotalAmount = Convert.ToString(amountWithCommision);
        //                            _Transaction.ServiceTaxRate = _commission.ServiceTaxRate;
        //                            _Transaction.ServiceTax = Convert.ToString(_commission.ServiceTaxAmount);
        //                            _Transaction.OperatorType = "AddBankFlutter";

        //                            _Transaction.AccountNo = getInitialTransaction.ReceiverNumber;
        //                            if (!string.IsNullOrEmpty(request.txRef))
        //                            {

        //                                _Transaction.TransactionStatus = (int)TransactionStatus.Completed; ;
        //                                try
        //                                {
        //                                    //--------send mail on success transaction--------

        //                                    var AdminKeys = AES256.AdminKeyPair;
        //                                    string FirstName = AES256.Decrypt(UserCurrentDetail.PrivateKey, UserCurrentDetail.FirstName);
        //                                    string LastName = AES256.Decrypt(UserCurrentDetail.PrivateKey, UserCurrentDetail.LastName);
        //                                    string StdCode = UserCurrentDetail.StdCode;
        //                                    string MobileNo = AES256.Decrypt(AdminKeys.PrivateKey, UserCurrentDetail.MobileNo);
        //                                    string EmailId = AES256.Decrypt(AdminKeys.PrivateKey, UserCurrentDetail.EmailId).Trim().ToLower();
        //                                    // var receiverDetail = new AppUserRepository().GetUserDetailById(receiver.WalletUserId);
        //                                    string filename = CommonSetting.successfullTransaction;


        //                                    var body = _sendEmails.ReadEmailformats(filename);
        //                                    body = body.Replace("$$FirstName$$", FirstName + " " + LastName);
        //                                    body = body.Replace("$$DisplayContent$$", "AddBankFlutter CARD");
        //                                    body = body.Replace("$$customer$$", MobileNo);
        //                                    body = body.Replace("$$amount$$", "XOF " + getInitialTransaction.RequestedAmount);
        //                                    body = body.Replace("$$ServiceTaxAmount$$", "XOF " + _commission.CommissionAmount);
        //                                    body = body.Replace("$$AmountWithCommission$$", "XOF " + amountWithCommision);
        //                                    body = body.Replace("$$TransactionId$$", request.txRef);

        //                                    var req = new EmailModel()
        //                                    {
        //                                        TO = EmailId,
        //                                        Subject = "Transaction Successfull",
        //                                        Body = body
        //                                    };
        //                                    _sendEmails.SendEmail(req);
        //                                }
        //                                catch
        //                                {

        //                                }
        //                            }
        //                            else
        //                            {
        //                                _Transaction.TransactionStatus = (int)TransactionStatus.Failed;
        //                            }
        //                            _Transaction.WalletServiceId = WalletServiceId.WalletServiceId;
        //                            _Transaction.SenderId = UserCurrentDetail.WalletUserId;
        //                            _Transaction.ReceiverId = UserCurrentDetail.WalletUserId;
        //                            _Transaction.BankBranchCode = string.Empty;
        //                            _Transaction.BankTransactionId = request.orderRef;
        //                            _Transaction.TransactionId = getInitialTransaction.InvoiceNumber;


        //                            _Transaction.IsAdminTransaction = false;
        //                            _Transaction.IsActive = true;
        //                            _Transaction.IsDeleted = false;
        //                            _Transaction.CreatedDate = TDate;
        //                            _Transaction.UpdatedDate = TDate;
        //                            _Transaction.TransactionTypeInfo = (int)TransactionTypeInfo.AddedByCard;
        //                            _Transaction.IsAddDuringPay = false;
        //                            _Transaction.VoucherCode = string.Empty;

        //                            await _cardPaymentRepository.SaveWalletTransactions(_Transaction);
        //                            //db.WalletTransactions.Add(_Transaction);
        //                            //db.SaveChanges();
        //                            #endregion

        //                            #region Credit
        //                            var _credit = new WalletTransactionDetail();
        //                            _credit.Amount = Convert.ToString(_commission.TransactionAmount);
        //                            _credit.TransactionType = (int)TransactionDetailType.Credit;
        //                            _credit.WalletUserId = adminUser.WalletUserId;
        //                            _credit.WalletTransactionId = _Transaction.WalletTransactionId;
        //                            _credit.IsActive = true;
        //                            _credit.IsDeleted = false;
        //                            _credit.CreatedDate = TDate;
        //                            _credit.UpdatedDate = TDate;
        //                            //db.WalletTransactionDetails.Add(_credit);
        //                            //db.SaveChanges();
        //                            await _cardPaymentRepository.SaveWalletTransactionDetails(_credit);
        //                            #endregion

        //                            #region Debit
        //                            var _debit = new WalletTransactionDetail();
        //                            _debit.Amount = Convert.ToString(_commission.TransactionAmount);
        //                            _debit.TransactionType = (int)TransactionDetailType.Debit;
        //                            _debit.WalletUserId = UserCurrentDetail.WalletUserId;
        //                            _debit.WalletTransactionId = _Transaction.WalletTransactionId;
        //                            _debit.IsActive = true;
        //                            _debit.IsDeleted = false;
        //                            _debit.CreatedDate = TDate;
        //                            _debit.UpdatedDate = TDate;
        //                            //db.WalletTransactionDetails.Add(_credit);
        //                            //db.SaveChanges();
        //                            await _cardPaymentRepository.SaveWalletTransactionDetails(_credit);
        //                            #endregion

        //                            //get UpdateNewCardNoResponseBankCode id
        //                            //await _cardPaymentRepository.UpdateNewCardNoResponseBankCode(getInitialTransaction.InvoiceNumber, UserCurrentDetail.WalletUserId, request.transaction_id);
        //                            //updatfe webhook when callback receive
        //                            await _cardPaymentRepository.Updatewebhookflutterflagsuccestxninvoiceno(getInitialTransaction.InvoiceNumber);
        //                            var adminKeyPair = AES256.AdminKeyPair;
        //                            //db.SaveChanges();
        //                            //tran.Commit();
        //                            #region PushNotification

        //                            var CurrentUser = await _walletUserRepository.GetCurrentUser(UserCurrentDetail.WalletUserId);//db.WalletUsers.Where(x => x.WalletUserId == receiver.WalletUserId).FirstOrDefault();
        //                            if (CurrentUser != null)
        //                            {
        //                                PushNotificationModel push = new PushNotificationModel();
        //                                push.SenderId = UserCurrentDetail.WalletUserId;
        //                                push.deviceType = (int)UserCurrentDetail.DeviceType;
        //                                push.deviceKey = UserCurrentDetail.DeviceToken;
        //                                PayMoneyPushModel pushModel = new PayMoneyPushModel();
        //                                pushModel.TransactionDate = TDate;
        //                                pushModel.TransactionId = request.txRef;
        //                                pushModel.CurrentBalance = CurrentUser.CurrentBalance;
        //                                pushModel.alert = _Transaction.WalletAmount + " XOF has been credited to your account.";
        //                                pushModel.Amount = getInitialTransaction.RequestedAmount;
        //                                pushModel.CurrentBalance = CurrentUser.CurrentBalance;
        //                                pushModel.pushType = (int)PushType.ADDMONEY;

        //                                if ((int)UserCurrentDetail.DeviceType == (int)DeviceTypes.ANDROID || (int)UserCurrentDetail.DeviceType == (int)DeviceTypes.Web)
        //                                {
        //                                    PushPayload<PayMoneyPushModel> aps = new PushPayload<PayMoneyPushModel>();
        //                                    PushPayloadData<PayMoneyPushModel> _data = new PushPayloadData<PayMoneyPushModel>();
        //                                    _data.notification = pushModel;
        //                                    aps.data = _data;
        //                                    aps.to = UserCurrentDetail.DeviceToken;
        //                                    aps.collapse_key = string.Empty;
        //                                    push.message = JsonConvert.SerializeObject(aps);
        //                                    push.payload = pushModel;
        //                                }
        //                                if ((int)UserCurrentDetail.DeviceType == (int)DeviceTypes.IOS)
        //                                {
        //                                    NotificationJsonResponse<PayMoneyIOSPushModel> aps = new NotificationJsonResponse<PayMoneyIOSPushModel>();
        //                                    PayMoneyIOSPushModel _iosPushModel = new PayMoneyIOSPushModel();
        //                                    _iosPushModel.alert = pushModel.alert;
        //                                    _iosPushModel.Amount = pushModel.Amount;
        //                                    _iosPushModel.CurrentBalance = pushModel.CurrentBalance;
        //                                    _iosPushModel.MobileNo = pushModel.MobileNo;
        //                                    _iosPushModel.SenderName = pushModel.SenderName;
        //                                    _iosPushModel.pushType = pushModel.pushType;
        //                                    aps.aps = _iosPushModel;

        //                                    push.message = JsonConvert.SerializeObject(aps);
        //                                }
        //                                //if (!string.IsNullOrEmpty(push.message))
        //                                //{
        //                                //    new PushNotificationRepository().sendPushNotification(push);
        //                                //}
        //                            }
        //                            #endregion

        //                            if (UserCurrentDetail.MobileNo != null && UserCurrentDetail.MobileNo != "")
        //                            {
        //                                response.ToMobileNo = AES256.Decrypt(adminKeyPair.PrivateKey, UserCurrentDetail.MobileNo);
        //                            }


        //                            response.CurrentBalance = UserCurrentDetail.CurrentBalance;
        //                            response.RstKey = 1;

        //                            ///
        //                            await _masterDataService.Chargeback(UserCurrentDetail.WalletUserId);
        //                            //get UpdateNewCardNoResponseBankCode id
        //                            //await _cardPaymentRepository.UpdateNewCardNoResponseBankCode(getInitialTransaction.InvoiceNumber, UserCurrentDetail.WalletUserId, request.transaction_id);

        //                            await _cardPaymentRepository.Updatewebhookflutterflagsuccestxninvoiceno(getInitialTransaction.InvoiceNumber);
        //                        }
        //                        else
        //                        {
        //                            //test

        //                        }
        //                    }
        //                    else
        //                    {
        //                        //test

        //                    }
        //                    //sdfsdfd
        //                }
        //                else
        //                {

        //                    response.RstKey = 2;
        //                }
        //            }
        //            else
        //            {
        //                await _cardPaymentRepository.UpdateTransactionInitiateRequest(getInitialTransaction);
        //                response.RstKey = 3;

        //                //response.TransactionResponseDescription = _req.vpc_ResponseCodeDescription;
        //                //response.TransactionResponseCode = request.vpc_TxnResponseCode;
        //            }



        //        }
        //        else
        //        {
        //            response.RstKey = 3;
        //        }

        //    }
        //    catch (Exception ex)
        //    {

        //        // ex.Message.ErrorLog("CardPaymentService", "SaveflutterCardPaymentResponse", request);
        //        "MasterCardPaymentController".ErrorLog("CardPaymentService", "SaveflutterBankPaymentResponse", ex.StackTrace + " " + ex.Message);
        //    }
        //    return response;
        //}




        //new add_mone :- bank debit reposne                

        public async Task<AddMoneyAggregatorResponse> SaveflutteraddmoneNGNBankTransferPaymentResponse(string txnreverifystatus, string invoiceno, string request)
        {
            AddMoneyAggregatorResponse response = new AddMoneyAggregatorResponse();
            CalculateCommissionResponse _commission = new CalculateCommissionResponse();
            CalculateCommissionRequest _commissionRequest = new CalculateCommissionRequest();

            try
            {

                var getInitialTransaction = await _cardPaymentRepository.GetTransactionInitiateRequest(invoiceno);
                int GetWalletTransactionsexist = await _cardPaymentRepository.GetWalletTransactionsexist(getInitialTransaction.WalletUserId, invoiceno);

                if (invoiceno != null && GetWalletTransactionsexist == 0)
                {

                    getInitialTransaction.JsonResponse = JsonConvert.SerializeObject(request);


                    if (txnreverifystatus != "successful" && invoiceno != null)
                    {
                        DateTime TDate = DateTime.UtcNow;

                        var WalletServiceId = await _cardPaymentRepository.GetWalletService("AddBankFlutter", 40);

                        if (WalletServiceId.WalletServiceId > 0)
                        {
                            var adminUser = await _cardPaymentRepository.GetAdminUser();
                            if (adminUser != null)
                            {
                                getInitialTransaction.RequestedAmount = Convert.ToString(Math.Round(Convert.ToDecimal(getInitialTransaction.RequestedAmount), 2));

                                long userId = Convert.ToInt32(getInitialTransaction.WalletUserId);
                                var UserCurrentDetail = await _walletUserRepository.GetCurrentUser(userId);
                                if (UserCurrentDetail != null && await _cardPaymentRepository.IsWalletTransactions(UserCurrentDetail.WalletUserId, invoiceno))
                                {
                                    getInitialTransaction.RequestedAmount = Convert.ToString(Math.Round(Convert.ToDecimal(getInitialTransaction.RequestedAmount), 2));

                                    _commissionRequest.CurrentBalance = Convert.ToDecimal(UserCurrentDetail.CurrentBalance);
                                    _commissionRequest.IsRoundOff = true;
                                    _commissionRequest.TransactionAmount = Convert.ToDecimal(getInitialTransaction.RequestedAmount); //change
                                    _commissionRequest.WalletServiceId = WalletServiceId.WalletServiceId;
                                    _commission = await _setCommisionRepository.CalculateCommission(_commissionRequest);

                                    getInitialTransaction.TransactionStatus = (int)TransactionStatus.Pending;
                                    await _cardPaymentRepository.UpdateTransactionInitiateRequest(getInitialTransaction);

                                    #region Save Transaction
                                    decimal amountWithCommision = decimal.Parse(string.Format("{0:0,0}", _commission.AmountWithCommission));    // "1,234,257";

                                    var _Transaction = new WalletTransaction();

                                    _Transaction.TransactionInitiateRequestId = getInitialTransaction.Id;
                                    _Transaction.MerchantCommissionAmount = Convert.ToString(_commission.MerchantCommissionAmount);
                                    _Transaction.MerchantCommissionId = _commission.MerchantCommissionId;
                                    _Transaction.TransactionType = AggragatorServiceType.CREDIT;
                                    _Transaction.IsBankTransaction = false;
                                    _Transaction.TransactionTypeInfo = (int)TransactionTypeInfo.AddedByCard;
                                    _Transaction.IsBankTransaction = false;
                                    _Transaction.Comments = string.Empty;
                                    _Transaction.InvoiceNo = getInitialTransaction.InvoiceNumber;
                                    _Transaction.CommisionAmount = Convert.ToString(_commission.CommissionAmount);
                                    _Transaction.CommisionId = _commission.CommissionId;
                                    _Transaction.WalletAmount = Convert.ToString(_commission.TransactionAmount);

                                    _Transaction.TotalAmount = Convert.ToString(amountWithCommision);
                                    _Transaction.ServiceTaxRate = _commission.ServiceTaxRate;
                                    _Transaction.ServiceTax = Convert.ToString(_commission.ServiceTaxAmount);
                                    _Transaction.OperatorType = "AddBankFlutter";

                                    _Transaction.AccountNo = getInitialTransaction.ReceiverNumber;

                                    _Transaction.TransactionStatus = (int)TransactionStatus.Failed;

                                    _Transaction.WalletServiceId = WalletServiceId.WalletServiceId;
                                    _Transaction.SenderId = UserCurrentDetail.WalletUserId;
                                    _Transaction.ReceiverId = UserCurrentDetail.WalletUserId;
                                    _Transaction.BankBranchCode = string.Empty;
                                    _Transaction.BankTransactionId = invoiceno;
                                    _Transaction.TransactionId = getInitialTransaction.InvoiceNumber;


                                    _Transaction.IsAdminTransaction = false;
                                    _Transaction.IsActive = true;
                                    _Transaction.IsDeleted = false;
                                    _Transaction.CreatedDate = TDate;
                                    _Transaction.UpdatedDate = TDate;
                                    _Transaction.TransactionTypeInfo = (int)TransactionTypeInfo.AddedByCard;
                                    _Transaction.IsAddDuringPay = false;
                                    _Transaction.VoucherCode = string.Empty;

                                    await _cardPaymentRepository.SaveWalletTransactions(_Transaction);
                                    //db.WalletTransactions.Add(_Transaction);
                                    //db.SaveChanges();
                                    #endregion

                                    #region Credit
                                    var _credit = new WalletTransactionDetail();
                                    _credit.Amount = Convert.ToString(_commission.TransactionAmount);
                                    _credit.TransactionType = (int)TransactionDetailType.Credit;
                                    _credit.WalletUserId = adminUser.WalletUserId;
                                    _credit.WalletTransactionId = _Transaction.WalletTransactionId;
                                    _credit.IsActive = true;
                                    _credit.IsDeleted = false;
                                    _credit.CreatedDate = TDate;
                                    _credit.UpdatedDate = TDate;
                                    //db.WalletTransactionDetails.Add(_credit);
                                    //db.SaveChanges();
                                    await _cardPaymentRepository.SaveWalletTransactionDetails(_credit);
                                    #endregion

                                    #region Debit
                                    var _debit = new WalletTransactionDetail();
                                    _debit.Amount = Convert.ToString(_commission.TransactionAmount);
                                    _debit.TransactionType = (int)TransactionDetailType.Debit;
                                    _debit.WalletUserId = UserCurrentDetail.WalletUserId;
                                    _debit.WalletTransactionId = _Transaction.WalletTransactionId;
                                    _debit.IsActive = true;
                                    _debit.IsDeleted = false;
                                    _debit.CreatedDate = TDate;
                                    _debit.UpdatedDate = TDate;
                                    //db.WalletTransactionDetails.Add(_credit);
                                    //db.SaveChanges();
                                    await _cardPaymentRepository.SaveWalletTransactionDetails(_credit);
                                    #endregion

                                    //get UpdateNewCardNoResponseBankCode id
                                    //await _cardPaymentRepository.UpdateNewCardNoResponseBankCode(getInitialTransaction.InvoiceNumber, UserCurrentDetail.WalletUserId, request.orderRef);


                                    //var adminKeyPair = AES256.AdminKeyPair;


                                    ////db.SaveChanges();
                                    ////tran.Commit();


                                    //if (UserCurrentDetail.MobileNo != null && UserCurrentDetail.MobileNo != "")
                                    //{
                                    //    response.ToMobileNo = AES256.Decrypt(adminKeyPair.PrivateKey, UserCurrentDetail.MobileNo);
                                    //}


                                    //response.CurrentBalance = UserCurrentDetail.CurrentBalance;
                                    response.RstKey = 2;



                                }
                                else
                                {
                                    //test

                                }
                            }
                            else
                            {
                                //test

                            }
                            //sdfsdfd
                        }
                        else
                        {

                            response.RstKey = 2;
                        }
                    }
                    //after verify flutter txn then crediht to user --when succesfujl statsu got from both
                    else if (invoiceno != null && txnreverifystatus == "successful")
                    {
                        //response.InvoiceNo = invoiceno;
                        //response.Amount = getInitialTransaction.RequestedAmount;
                        //response.status = "AddBankFlutter";
                        DateTime TDate = DateTime.UtcNow;
                        //response.TransactionDate = TDate;

                        var WalletServiceId = await _cardPaymentRepository.GetWalletService("AddBankFlutter", 40);

                        if (WalletServiceId.WalletServiceId > 0)
                        {
                            var adminUser = await _cardPaymentRepository.GetAdminUser();
                            if (adminUser != null)
                            {
                                long userId = Convert.ToInt32(getInitialTransaction.WalletUserId);
                                var UserCurrentDetail = await _walletUserRepository.GetCurrentUser(userId);
                                if (UserCurrentDetail != null && await _cardPaymentRepository.IsWalletTransactions(UserCurrentDetail.WalletUserId, invoiceno))
                                {
                                    //this line commented due to currentbalance is not added to card expected 
                                    //request.vpc_Amount = Convert.ToString(Math.Round((Convert.ToDecimal(requestDetail.Amount) / 100), 2));
                                    getInitialTransaction.RequestedAmount = Convert.ToString(Math.Round(Convert.ToDecimal(getInitialTransaction.RequestedAmount), 2));

                                    // to update wallet amount-----

                                    // db.WalletUsers.FirstOrDefault(x => x.WalletUserId == receiver.WalletUserId);

                                    if (UserCurrentDetail != null)
                                    {
                                        _commissionRequest.CurrentBalance = Convert.ToDecimal(UserCurrentDetail.CurrentBalance);
                                        _commissionRequest.IsRoundOff = true;
                                        //_commissionRequest.TransactionAmount = Convert.ToDecimal(request.vpc_Amount);

                                        _commissionRequest.TransactionAmount = Convert.ToDecimal(getInitialTransaction.RequestedAmount); //change
                                        _commissionRequest.WalletServiceId = WalletServiceId.WalletServiceId;
                                        _commission = await _setCommisionRepository.CalculatePayNGNTransferAddMoneyCommission(_commissionRequest);

                                        if (!string.IsNullOrEmpty(invoiceno))
                                        {
                                            getInitialTransaction.TransactionStatus = (int)TransactionStatus.Completed;
                                            if (Convert.ToDecimal(UserCurrentDetail.CurrentBalance) >= 0)
                                            {
                                                if (Convert.ToDecimal(UserCurrentDetail.CurrentBalance) == 0)
                                                {
                                                    UserCurrentDetail.CurrentBalance = _commission.TransactionAmount.ToString();
                                                    getInitialTransaction.AfterTransactionBalance = UserCurrentDetail.CurrentBalance.ToString();
                                                    getInitialTransaction.ReceiverCurrentBalance = UserCurrentDetail.CurrentBalance.ToString();
                                                }
                                                else
                                                {
                                                    UserCurrentDetail.CurrentBalance = Math.Round(Convert.ToDecimal(UserCurrentDetail.CurrentBalance) + _commission.TransactionAmount, 2).ToString();
                                                    getInitialTransaction.AfterTransactionBalance = UserCurrentDetail.CurrentBalance.ToString();
                                                    getInitialTransaction.ReceiverCurrentBalance = UserCurrentDetail.CurrentBalance.ToString();
                                                }
                                            }
                                            else
                                            {
                                                UserCurrentDetail.CurrentBalance = Math.Round(_commission.TransactionAmount - Convert.ToDecimal(UserCurrentDetail.CurrentBalance), 2).ToString();
                                                getInitialTransaction.AfterTransactionBalance = UserCurrentDetail.CurrentBalance.ToString();
                                                getInitialTransaction.ReceiverCurrentBalance = UserCurrentDetail.CurrentBalance.ToString();
                                            }
                                        }
                                        await _walletUserRepository.UpdateUserDetail(UserCurrentDetail);
                                        await _cardPaymentRepository.UpdateTransactionInitiateRequest(getInitialTransaction);
                                        // db.SaveChanges();
                                    }

                                    #region Save Transaction
                                    decimal amountWithCommision = decimal.Parse(string.Format("{0:0,0}", _commission.AmountWithCommission));    // "1,234,257";

                                    var _Transaction = new WalletTransaction();

                                    _Transaction.TransactionInitiateRequestId = getInitialTransaction.Id;
                                    _Transaction.MerchantCommissionAmount = Convert.ToString(_commission.MerchantCommissionAmount);
                                    _Transaction.MerchantCommissionId = _commission.MerchantCommissionId;
                                    _Transaction.TransactionType = AggragatorServiceType.CREDIT;
                                    _Transaction.IsBankTransaction = false;
                                    _Transaction.TransactionTypeInfo = (int)TransactionTypeInfo.AddedByCard;
                                    _Transaction.IsBankTransaction = false;
                                    _Transaction.Comments = string.Empty;
                                    _Transaction.InvoiceNo = getInitialTransaction.InvoiceNumber;
                                    _Transaction.CommisionAmount = Convert.ToString(_commission.CommissionAmount);
                                    _Transaction.CommisionId = _commission.CommissionId;
                                    _Transaction.WalletAmount = Convert.ToString(_commission.TransactionAmount);

                                    _Transaction.TotalAmount = Convert.ToString(amountWithCommision);
                                    _Transaction.ServiceTaxRate = _commission.ServiceTaxRate;
                                    _Transaction.ServiceTax = Convert.ToString(_commission.ServiceTaxAmount);
                                    _Transaction.OperatorType = "AddBankFlutter";

                                    _Transaction.AccountNo = getInitialTransaction.ReceiverNumber;
                                    if (!string.IsNullOrEmpty(invoiceno))
                                    {

                                        _Transaction.TransactionStatus = (int)TransactionStatus.Completed; ;
                                        try
                                        {
                                            //--------send mail on success transaction--------

                                            var AdminKeys = AES256.AdminKeyPair;
                                            string FirstName = AES256.Decrypt(UserCurrentDetail.PrivateKey, UserCurrentDetail.FirstName);
                                            string LastName = AES256.Decrypt(UserCurrentDetail.PrivateKey, UserCurrentDetail.LastName);
                                            string StdCode = UserCurrentDetail.StdCode;
                                            string MobileNo = AES256.Decrypt(AdminKeys.PrivateKey, UserCurrentDetail.MobileNo);
                                            string EmailId = AES256.Decrypt(AdminKeys.PrivateKey, UserCurrentDetail.EmailId).Trim().ToLower();
                                            // var receiverDetail = new AppUserRepository().GetUserDetailById(receiver.WalletUserId);
                                            string filename = CommonSetting.successfullTransaction;


                                            var body = _sendEmails.ReadEmailformats(filename);
                                            body = body.Replace("$$FirstName$$", FirstName + " " + LastName);
                                            body = body.Replace("$$DisplayContent$$", "AddBankFlutter CARD");
                                            body = body.Replace("$$customer$$", MobileNo);
                                            body = body.Replace("$$amount$$", "XOF " + getInitialTransaction.RequestedAmount);
                                            body = body.Replace("$$ServiceTaxAmount$$", "XOF " + _commission.CommissionAmount);
                                            body = body.Replace("$$AmountWithCommission$$", "XOF " + amountWithCommision);
                                            body = body.Replace("$$TransactionId$$", invoiceno);

                                            var req = new EmailModel()
                                            {
                                                TO = EmailId,
                                                Subject = "Transaction Successfull",
                                                Body = body
                                            };
                                            _sendEmails.SendEmail(req);
                                        }
                                        catch
                                        {

                                        }
                                    }
                                    else
                                    {
                                        _Transaction.TransactionStatus = (int)TransactionStatus.Failed;
                                    }
                                    _Transaction.WalletServiceId = WalletServiceId.WalletServiceId;
                                    _Transaction.SenderId = UserCurrentDetail.WalletUserId;
                                    _Transaction.ReceiverId = UserCurrentDetail.WalletUserId;
                                    _Transaction.BankBranchCode = string.Empty;
                                    _Transaction.BankTransactionId = invoiceno;
                                    _Transaction.TransactionId = getInitialTransaction.InvoiceNumber;


                                    _Transaction.IsAdminTransaction = false;
                                    _Transaction.IsActive = true;
                                    _Transaction.IsDeleted = false;
                                    _Transaction.CreatedDate = TDate;
                                    _Transaction.UpdatedDate = TDate;
                                    _Transaction.TransactionTypeInfo = (int)TransactionTypeInfo.AddedByCard;
                                    _Transaction.IsAddDuringPay = false;
                                    _Transaction.VoucherCode = string.Empty;

                                    await _cardPaymentRepository.SaveWalletTransactions(_Transaction);
                                    //db.WalletTransactions.Add(_Transaction);
                                    //db.SaveChanges();
                                    #endregion

                                    #region Credit
                                    var _credit = new WalletTransactionDetail();
                                    _credit.Amount = Convert.ToString(_commission.TransactionAmount);
                                    _credit.TransactionType = (int)TransactionDetailType.Credit;
                                    _credit.WalletUserId = adminUser.WalletUserId;
                                    _credit.WalletTransactionId = _Transaction.WalletTransactionId;
                                    _credit.IsActive = true;
                                    _credit.IsDeleted = false;
                                    _credit.CreatedDate = TDate;
                                    _credit.UpdatedDate = TDate;
                                    //db.WalletTransactionDetails.Add(_credit);
                                    //db.SaveChanges();
                                    await _cardPaymentRepository.SaveWalletTransactionDetails(_credit);
                                    #endregion

                                    #region Debit
                                    var _debit = new WalletTransactionDetail();
                                    _debit.Amount = Convert.ToString(_commission.TransactionAmount);
                                    _debit.TransactionType = (int)TransactionDetailType.Debit;
                                    _debit.WalletUserId = UserCurrentDetail.WalletUserId;
                                    _debit.WalletTransactionId = _Transaction.WalletTransactionId;
                                    _debit.IsActive = true;
                                    _debit.IsDeleted = false;
                                    _debit.CreatedDate = TDate;
                                    _debit.UpdatedDate = TDate;
                                    //db.WalletTransactionDetails.Add(_credit);
                                    //db.SaveChanges();
                                    await _cardPaymentRepository.SaveWalletTransactionDetails(_credit);
                                    #endregion

                                    //get UpdateNewCardNoResponseBankCode id
                                    //await _cardPaymentRepository.UpdateNewCardNoResponseBankCode(getInitialTransaction.InvoiceNumber, UserCurrentDetail.WalletUserId, request.transaction_id);
                                    //updatfe webhook when callback receive
                                    await _cardPaymentRepository.Updatewebhookflutterflagsuccestxninvoiceno(getInitialTransaction.InvoiceNumber);
                                    var adminKeyPair = AES256.AdminKeyPair;
                                    //db.SaveChanges();
                                    //tran.Commit();
                                    #region PushNotification

                                    var CurrentUser = await _walletUserRepository.GetCurrentUser(UserCurrentDetail.WalletUserId);//db.WalletUsers.Where(x => x.WalletUserId == receiver.WalletUserId).FirstOrDefault();
                                    if (CurrentUser != null)
                                    {
                                        PushNotificationModel push = new PushNotificationModel();
                                        push.SenderId = UserCurrentDetail.WalletUserId;
                                        push.deviceType = (int)UserCurrentDetail.DeviceType;
                                        push.deviceKey = UserCurrentDetail.DeviceToken;
                                        PayMoneyPushModel pushModel = new PayMoneyPushModel();
                                        pushModel.TransactionDate = TDate;
                                        pushModel.TransactionId = invoiceno;
                                        pushModel.CurrentBalance = CurrentUser.CurrentBalance;
                                        pushModel.alert = _Transaction.WalletAmount + " XOF has been credited to your account.";
                                        pushModel.Amount = getInitialTransaction.RequestedAmount;
                                        pushModel.CurrentBalance = CurrentUser.CurrentBalance;
                                        pushModel.pushType = (int)PushType.ADDMONEY;

                                        if ((int)UserCurrentDetail.DeviceType == (int)DeviceTypes.ANDROID || (int)UserCurrentDetail.DeviceType == (int)DeviceTypes.Web)
                                        {
                                            PushPayload<PayMoneyPushModel> aps = new PushPayload<PayMoneyPushModel>();
                                            PushPayloadData<PayMoneyPushModel> _data = new PushPayloadData<PayMoneyPushModel>();
                                            _data.notification = pushModel;
                                            aps.data = _data;
                                            aps.to = UserCurrentDetail.DeviceToken;
                                            aps.collapse_key = string.Empty;
                                            push.message = JsonConvert.SerializeObject(aps);
                                            push.payload = pushModel;
                                        }
                                        if ((int)UserCurrentDetail.DeviceType == (int)DeviceTypes.IOS)
                                        {
                                            NotificationJsonResponse<PayMoneyIOSPushModel> aps = new NotificationJsonResponse<PayMoneyIOSPushModel>();
                                            PayMoneyIOSPushModel _iosPushModel = new PayMoneyIOSPushModel();
                                            _iosPushModel.alert = pushModel.alert;
                                            _iosPushModel.Amount = pushModel.Amount;
                                            _iosPushModel.CurrentBalance = pushModel.CurrentBalance;
                                            _iosPushModel.MobileNo = pushModel.MobileNo;
                                            _iosPushModel.SenderName = pushModel.SenderName;
                                            _iosPushModel.pushType = pushModel.pushType;
                                            aps.aps = _iosPushModel;

                                            push.message = JsonConvert.SerializeObject(aps);
                                        }
                                        //if (!string.IsNullOrEmpty(push.message))
                                        //{
                                        //    new PushNotificationRepository().sendPushNotification(push);
                                        //}
                                    }
                                    #endregion

                                    if (UserCurrentDetail.MobileNo != null && UserCurrentDetail.MobileNo != "")
                                    {
                                        response.ToMobileNo = AES256.Decrypt(adminKeyPair.PrivateKey, UserCurrentDetail.MobileNo);
                                    }


                                    response.CurrentBalance = UserCurrentDetail.CurrentBalance;
                                    response.RstKey = 1;

                                    ///
                                    await _masterDataService.Chargeback(UserCurrentDetail.WalletUserId);
                                    //get UpdateNewCardNoResponseBankCode id
                                    //await _cardPaymentRepository.UpdateNewCardNoResponseBankCode(getInitialTransaction.InvoiceNumber, UserCurrentDetail.WalletUserId, request.transaction_id);

                                    await _cardPaymentRepository.Updatewebhookflutterflagsuccestxninvoiceno(getInitialTransaction.InvoiceNumber);
                                }
                                else
                                {
                                    //test

                                }
                            }
                            else
                            {
                                //test

                            }
                            //sdfsdfd
                        }
                        else
                        {

                            response.RstKey = 2;
                        }
                    }
                    else
                    {
                        await _cardPaymentRepository.UpdateTransactionInitiateRequest(getInitialTransaction);
                        response.RstKey = 3;

                        //response.TransactionResponseDescription = _req.vpc_ResponseCodeDescription;
                        //response.TransactionResponseCode = request.vpc_TxnResponseCode;
                    }



                }
                else
                {
                    response.RstKey = 3;
                }

            }
            catch (Exception ex)
            {

                // ex.Message.ErrorLog("CardPaymentService", "SaveflutterCardPaymentResponse", request);
                "MasterCardPaymentController".ErrorLog("CardPaymentService", "SaveflutterBankPaymentResponse", ex.StackTrace + " " + ex.Message);
            }
            return response;
        }


        //paymone:-nigeria transfer 
        public async Task<UpdateTransactionResponse> SaveflutterPayBankTransferPaymentResponse(string txnreverifystatus, string invoiceno)
        {
            var response = new UpdateTransactionResponse();

            try
            {
                var getInitialTransaction = await _cardPaymentRepository.GetTransactionInitiateRequest(invoiceno);
                //LogTransactionTypes.Response.SaveTransactionLog(LogTransactionNameTypes.UpdateTransaction + "paymone :-nigeria transfer", invoiceno, "Response txnreverifystatus : " + txnreverifystatus + ";" + getInitialTransaction.JsonResponse);
                var GetWalletTransactionsexist = await _cardPaymentRepository.GetWalletTransaction(getInitialTransaction.WalletUserId, invoiceno);
                long WalletUserId = (long)GetWalletTransactionsexist.SenderId;

                var data = await _walletUserRepository.GetCurrentUser(WalletUserId);
                //callback receive for pey service :- tranfertobank txn pending onli 
                if (invoiceno != null && GetWalletTransactionsexist.TransactionStatus == 2)
                {

                    //getInitialTransaction.JsonResponse = "txnreverifystatus :" + txnreverifystatus + ";" + getInitialTransaction.JsonResponse;

                    //check txn verify flutter --when not succesful statsu got from txn verify & suceeful get from cllback 

                    if (txnreverifystatus == "FAILED")
                    {
                        var refundAmt = Convert.ToDecimal(data.CurrentBalance) + Convert.ToDecimal(GetWalletTransactionsexist.TotalAmount);
                        data.CurrentBalance = Convert.ToString(refundAmt);
                        await _walletUserRepository.UpdateUserDetail(data);
                        //_logUtils.WriteTextToFileForWTxnTableLogs("MobileMoneyservice :-lineno. 516 InvoiceNumber :" + invoiceNumber.InvoiceNumber);
                        var _transactionInitial = await _cardPaymentRepository.GetTransactionInitiateRequest(getInitialTransaction.Id);
                        _transactionInitial.UpdatedDate = DateTime.UtcNow;
                        _transactionInitial.AfterTransactionBalance = data.CurrentBalance;
                        _transactionInitial.ReceiverCurrentBalance = data.CurrentBalance;

                        await _cardPaymentRepository.UpdateTransactionInitiateRequest(_transactionInitial);
                        // _logUtils.WriteTextToFileForWTxnTableLogs("MobileMoneyservice :-lineno. 522 InvoiceNumber :" + invoiceNumber.InvoiceNumber);

                        GetWalletTransactionsexist.TransactionStatus = (int)TransactionStatus.Failed;
                        GetWalletTransactionsexist.UpdatedDate = DateTime.UtcNow;
                        await _cardPaymentRepository.UpdateWalletTransaction(GetWalletTransactionsexist);
                    }
                    //after verify flutter txn then credit to user --when succesful statsu got from both
                    else if (txnreverifystatus == "SUCCESSFUL")
                    {
                        var _transactionInitial = await _cardPaymentRepository.GetTransactionInitiateRequest(getInitialTransaction.Id);
                        _transactionInitial.UpdatedDate = DateTime.UtcNow;

                        await _cardPaymentRepository.UpdateTransactionInitiateRequest(_transactionInitial);

                        GetWalletTransactionsexist.TransactionStatus = (int)TransactionStatus.Completed;
                        GetWalletTransactionsexist.UpdatedDate = DateTime.UtcNow;
                        await _cardPaymentRepository.UpdateWalletTransaction(GetWalletTransactionsexist);
                        //await _cardPaymentRepository.SaveWalletTransactions(GetWalletTransactionsexist);



                        //-------------sending email after success transaction-----------------
                        try
                        {
                            string filename = CommonSetting.successfullTransaction;
                            var AdminKeys = AES256.AdminKeyPair;
                            var EmailId = AES256.Decrypt(AdminKeys.PrivateKey, data.EmailId).Trim().ToLower();
                            var body = _sendEmails.ReadEmailformats(filename);
                            body = body.Replace("$$FirstName$$", getInitialTransaction.UserName);
                            body = body.Replace("$$DisplayContent$$", "Flutter Payment");
                            body = body.Replace("$$customer$$", getInitialTransaction.ReceiverNumber);
                            body = body.Replace("$$amount$$", "XOF " + getInitialTransaction.RequestedAmount);
                            body = body.Replace("$$ServiceTaxAmount$$", "XOF " + GetWalletTransactionsexist.CommisionAmount);
                            body = body.Replace("$$AmountWithCommission$$", "XOF " + GetWalletTransactionsexist.TotalAmount);
                            body = body.Replace("$$TransactionId$$", getInitialTransaction.InvoiceNumber);

                            var requ = new EmailModel
                            {
                                TO = data.EmailId,
                                Subject = ResponseEmailMessage.PAYMENT_SUCCESS,
                                Body = body
                            };

                            _sendEmails.SendEmail(requ);
                        }
                        catch
                        {

                        }
                    }
                    else
                    {
                        await _cardPaymentRepository.UpdateTransactionInitiateRequest(getInitialTransaction);
                        //response.RstKey = 3;

                        //response.TransactionResponseDescription = _req.vpc_ResponseCodeDescription;
                        //response.TransactionResponseCode = request.vpc_TxnResponseCode;
                    }
                }
                else
                {
                    //response.RstKey = 3;
                }

            }
            catch (Exception ex)
            {

                // ex.Message.ErrorLog("CardPaymentService", "SaveflutterCardPaymentResponse", request);
                "MasterCardPaymentController".ErrorLog("CardPaymentService", "SaveflutterPayBankTransferPaymentResponse", ex.StackTrace + " " + ex.Message);
            }
            return response;
        }

        //flutter usd
        public async Task<flutterPaymentUrlResponse> GetCardPaymentUrlForNGNbankflutterUSD(ThirdpartyPaymentByCardRequest request, string headerToken)
        {
            var response = new flutterPaymentUrlResponse();
            var _commission = new CalculateCommissionResponse();
            var _commissionRequest = new CalculateCommissionRequest();

            var transationInitiate = new TransactionInitiateRequest();
            var _thirdPartyPaymentByCard = new ThirdPartyPaymentByCard();

            try
            {

                var UserDetail = await _walletUserService.UserProfile(headerToken);

                var Isdocverified = await _walletUserRepository.IsDocVerifiedMOMO(UserDetail.DocumetStatus);

                var resultTL = await _transactionLimitAUService.CheckTransactionLimitAU(request.WalletUserId.ToString()); //check New TL
                var transactionLimit = await _masterDataRepository.GetTransactionLimitAddMoney(Convert.ToString(UserDetail.WalletUserId));
                int limit = transactionLimit != null ? Convert.ToInt32(transactionLimit.TransactionLimitForAddMoney) : 0;

                var transactionHistory = _masterDataRepository.GetAllTransactionsAddMoney(UserDetail.WalletUserId);
                int totalAmountTransfered = transactionHistory != null ? Convert.ToInt32(transactionHistory.totalAmount) : 0;

                #region Calculate commission on request amount               
                //------Get Currency Rate--------------
                var currencyDetail = _masterDataRepository.GetCurrencyRate();
                decimal UsdRate = Convert.ToDecimal(currencyDetail.CediRate);//Add Doller Rate

                decimal requestAmount = Convert.ToDecimal(request.Amount);//;

                //int WalletServiceId = await _cardPaymentRepository.GetServiceId();
                var WalletService = await _cardPaymentRepository.GetWalletService("FlutterUSD", 58);

                if (UserDetail.IsActive == true)//am
                {
                    if (UserDetail.IsEmailVerified == true)
                    {
                        if (Isdocverified == true)
                        {
                            if (transactionLimit == null || transactionLimit.TransactionLimitForAddMoney == null || limit >= (Convert.ToDecimal(request.Amount) + totalAmountTransfered))
                            {
                                if (WalletService.WalletServiceId > 0)
                                {
                                    #region Calculate Commission on request amount
                                    _commissionRequest.IsRoundOff = true;
                                    _commissionRequest.TransactionAmount = Convert.ToDecimal(request.Amount);
                                    _commissionRequest.WalletServiceId = WalletService.WalletServiceId;
                                    _commission = await _setCommisionRepository.CalculateCommission(_commissionRequest);
                                    #endregion
                                }


                                decimal amountWithCommision = decimal.Parse(string.Format("{0:0,0}", _commission.AmountWithCommission));    // "1,234,257";

                                decimal amt = (amountWithCommision * UsdRate);
                                var finalAmt = Decimal.Parse(amt.ToString("0.00"));
                                #endregion
                                if (resultTL != null)
                                {
                                    decimal SetAmount = Convert.ToDecimal(resultTL.SetAmount);// returns decimal
                                    decimal TotalAmount = Convert.ToDecimal(resultTL.TotalAmount);// returns decimal

                                    if (SetAmount != 0) //0 =msg 
                                    {
                                        decimal requestAmountwithcomm = amountWithCommision + TotalAmount;

                                        if (requestAmountwithcomm <= SetAmount)//1000 >= 1000
                                        {

                                        }
                                        else
                                        {
                                            response.RstKey = 6;
                                            response.Message = ResponseMessages.INVALID_txnAmountREQUEST;
                                            return response;
                                        }
                                    }
                                }
                                var invoiceNumber = await _masterDataRepository.GetInvoiceNumber();
                                var _masterCard = new MasterCardPaymentRequest();

                                _masterCard.SessionId = null;
                                _masterCard.Version = null;
                                _masterCard.SuccessIndicator = null;
                                _masterCard.Merchant = "FlutterUSD";
                                _masterCard.IsActive = true;
                                _masterCard.IsDeleted = false;
                                _masterCard.CreatedDate = DateTime.UtcNow;
                                _masterCard.UpdatedDate = DateTime.UtcNow;
                                _masterCard.Amount = request.Amount;
                                _masterCard.CommisionCharges = _commission.CommisionPercent;
                                _masterCard.TotalAmount = Convert.ToString(amountWithCommision);
                                _masterCard.WalletUserId = UserDetail.WalletUserId;
                                _masterCard.TransactionNo = invoiceNumber.InvoiceNumber;
                                _masterCard.FlatCharges = _commission.FlatCharges;
                                _masterCard.BenchmarkCharges = _commission.BenchmarkCharges;
                                _masterCard.CommissionAmount = _commission.CommissionAmount.ToString();
                                await _cardPaymentRepository.SaveMasterCardPaymentRequest(_masterCard);

                                CardPaymentRequest _cardRequest = new CardPaymentRequest();
                                _cardRequest.WalletUserId = UserDetail.WalletUserId;
                                _cardRequest.TotalAmount = _commission.AmountWithCommission.ToString();
                                _cardRequest.CommissionAmount = Convert.ToString(_commission.CommissionAmount);
                                _cardRequest.Amount = _commission.TransactionAmount.ToString();// Convert.ToString(requestAmo);
                                _cardRequest.FlatCharges = _commission.FlatCharges;
                                _cardRequest.BenchmarkCharges = _commission.BenchmarkCharges;
                                _cardRequest.CommisionCharges = _commission.CommisionPercent;
                                _cardRequest.CreatedDate = DateTime.UtcNow;
                                _cardRequest.UpdatedDate = DateTime.UtcNow;
                                _cardRequest.AmountInCedi = finalAmt.ToString(); //usd
                                _cardRequest.OrderNo = invoiceNumber.InvoiceNumber;
                                _cardRequest = await _cardPaymentRepository.SaveCardPaymentRequest(_cardRequest);


                                var _Requestcustomer = new customer();
                                _Requestcustomer.email = UserDetail.EmailId;

                                var _RequestAttributes = new flutterRequest();
                                _RequestAttributes.currency = "USD";
                                _RequestAttributes.tx_ref = invoiceNumber.InvoiceNumber;
                                _RequestAttributes.redirect_url = CommonSetting.flutterCallBackUrl;
                                _RequestAttributes.amount = Convert.ToString(finalAmt); //usd
                                _RequestAttributes.customer = _Requestcustomer;
                                _RequestAttributes.payment_options = "card";

                                var req = JsonConvert.SerializeObject(_RequestAttributes);

                                _logUtils.WriteTextToFileForFlutterPeyLoadLogs("SaveflutterCardRequestDetailUSD :- InvoiceNumber " + invoiceNumber.InvoiceNumber + " " + req);
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                //here to get psaymenturl
                                var responseData2 = await GethashorUrl(req, null, "flutterUrl");
                                var _responseModel2 = JsonConvert.DeserializeObject<flutterPaymentUrlResponse>(responseData2);
                                if (_responseModel2.data.link != null && _responseModel2.status == "success")
                                {
                                    transationInitiate.InvoiceNumber = invoiceNumber.InvoiceNumber;
                                    transationInitiate.ReceiverNumber = UserDetail.MobileNo;
                                    transationInitiate.ServiceName = "FlutterUSD";
                                    transationInitiate.RequestedAmount = _commission.TransactionAmount.ToString();
                                    transationInitiate.TransactionStatus = (int)TransactionStatus.Pending;
                                    transationInitiate.WalletUserId = UserDetail.WalletUserId;
                                    transationInitiate.UserReferanceNumber = invoiceNumber.AutoDigit;
                                    transationInitiate.CurrentBalance = UserDetail.CurrentBalance;
                                    transationInitiate.AfterTransactionBalance = "";
                                    transationInitiate.ReceiverCurrentBalance = UserDetail.CurrentBalance;
                                    transationInitiate.UserName = UserDetail.FirstName + " " + UserDetail.LastName;
                                    transationInitiate.ReceiverWalletUserId = UserDetail.WalletUserId;
                                    transationInitiate.CreatedDate = DateTime.UtcNow;
                                    transationInitiate.UpdatedDate = DateTime.UtcNow;
                                    transationInitiate.IsActive = true;
                                    transationInitiate.IsDeleted = false;
                                    transationInitiate.JsonRequest = responseData2;
                                    transationInitiate.JsonResponse = "";
                                    transationInitiate = await _cardPaymentRepository.SaveTransactionInitiateRequest(transationInitiate);
                                    response.URL = _responseModel2.data.link;
                                    response.RstKey = 2;
                                }

                                else
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.Message = "Please try after some time aggregator error.";
                                }
                                LogTransactionTypes.Request.SaveTransactionLog(LogTransactionNameTypes.AddMoney + "Credit TO Debit Cards", responseData2, "Request Url : " + response.data.link);
                            }
                            else
                            {
                                var addLimit = limit - (Convert.ToDecimal(request.Amount) + totalAmountTransfered);
                                if (addLimit < Convert.ToDecimal(request.Amount))
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.Message = "Exceed your transaction limit.";
                                }
                                else
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.Message = "You can not add more then " + transactionLimit.TransactionLimitForAddMoney.ToString() + " cedi in a day";
                                }
                            }
                        }
                        else if (UserDetail.DocumetStatus == 0 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
                        {
                            response.RstKey = 13;
                            response.Message = ResponseMessageKyc.FAILED_Doc_NotUploaded;
                        }
                        else if (UserDetail.DocumetStatus == 1 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
                        {
                            response.RstKey = 14;
                            response.Message = ResponseMessageKyc.FAILED_Doc_Pending;
                        }
                        else if (UserDetail.DocumetStatus == 4 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
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
                        response.RstKey = 6;
                        response.StatusCode = (int)TransactionStatus.Failed;
                        response.Message = ResponseMessages.EMAIL_VERIFICATION_PENDING;
                    }
                }
                else
                {
                    response.RstKey = 6;
                    response.Message = ResponseMessages.TRANSACTION_DISABLED;
                }
            }

            catch (Exception ex)
            {

                //tran.Rollback();
            }
            return response;

        }

        //flutter euro
        public async Task<flutterPaymentUrlResponse> GetCardPaymentUrlForNGNbankflutterEuro(ThirdpartyPaymentByCardRequest request, string headerToken)
        {
            var response = new flutterPaymentUrlResponse();
            var _commission = new CalculateCommissionResponse();
            var _commissionRequest = new CalculateCommissionRequest();

            var transationInitiate = new TransactionInitiateRequest();
            var _thirdPartyPaymentByCard = new ThirdPartyPaymentByCard();

            try
            {

                var UserDetail = await _walletUserService.UserProfile(headerToken);

                var Isdocverified = await _walletUserRepository.IsDocVerifiedMOMO(UserDetail.DocumetStatus);

                var resultTL = await _transactionLimitAUService.CheckTransactionLimitAU(request.WalletUserId.ToString()); //check New TL
                var transactionLimit = await _masterDataRepository.GetTransactionLimitAddMoney(Convert.ToString(UserDetail.WalletUserId));
                int limit = transactionLimit != null ? Convert.ToInt32(transactionLimit.TransactionLimitForAddMoney) : 0;

                var transactionHistory = _masterDataRepository.GetAllTransactionsAddMoney(UserDetail.WalletUserId);
                int totalAmountTransfered = transactionHistory != null ? Convert.ToInt32(transactionHistory.totalAmount) : 0;

                #region Calculate commission on request amount               
                //------Get Currency Rate--------------
                var currencyDetail = _masterDataRepository.GetCurrencyRate();
                decimal EuroRate = Convert.ToDecimal(currencyDetail.EuroRate);//Add Euro Rate

                decimal requestAmount = Convert.ToDecimal(request.Amount);//;

                //int WalletServiceId = await _cardPaymentRepository.GetServiceId();
                var WalletService = await _cardPaymentRepository.GetWalletService("FlutterEURO", 59);

                if (UserDetail.IsActive == true)//am
                {
                    if (UserDetail.IsEmailVerified == true)
                    {
                        if (Isdocverified == true)
                        {
                            if (transactionLimit == null || transactionLimit.TransactionLimitForAddMoney == null || limit >= (Convert.ToDecimal(request.Amount) + totalAmountTransfered))
                            {
                                if (WalletService.WalletServiceId > 0)
                                {
                                    #region Calculate Commission on request amount
                                    _commissionRequest.IsRoundOff = true;
                                    _commissionRequest.TransactionAmount = Convert.ToDecimal(request.Amount);
                                    _commissionRequest.WalletServiceId = WalletService.WalletServiceId;
                                    _commission = await _setCommisionRepository.CalculateCommission(_commissionRequest);
                                    #endregion
                                }


                                decimal amountWithCommision = decimal.Parse(string.Format("{0:0,0}", _commission.AmountWithCommission));    // "1,234,257";

                                decimal amt = (amountWithCommision * EuroRate);
                                var finalAmt = Decimal.Parse(amt.ToString("0.00"));
                                #endregion
                                if (resultTL != null)
                                {
                                    decimal SetAmount = Convert.ToDecimal(resultTL.SetAmount);// returns decimal
                                    decimal TotalAmount = Convert.ToDecimal(resultTL.TotalAmount);// returns decimal

                                    if (SetAmount != 0) //0 =msg 
                                    {
                                        decimal requestAmountwithcomm = amountWithCommision + TotalAmount;

                                        if (requestAmountwithcomm <= SetAmount)//1000 >= 1000
                                        {

                                        }
                                        else
                                        {
                                            response.RstKey = 6;
                                            response.Message = ResponseMessages.INVALID_txnAmountREQUEST;
                                            return response;
                                        }
                                    }
                                }
                                var invoiceNumber = await _masterDataRepository.GetInvoiceNumber();
                                var _masterCard = new MasterCardPaymentRequest();

                                _masterCard.SessionId = null;
                                _masterCard.Version = null;
                                _masterCard.SuccessIndicator = null;
                                _masterCard.Merchant = "FlutterEuro";
                                _masterCard.IsActive = true;
                                _masterCard.IsDeleted = false;
                                _masterCard.CreatedDate = DateTime.UtcNow;
                                _masterCard.UpdatedDate = DateTime.UtcNow;
                                _masterCard.Amount = request.Amount;
                                _masterCard.CommisionCharges = _commission.CommisionPercent;
                                _masterCard.TotalAmount = Convert.ToString(amountWithCommision);
                                _masterCard.WalletUserId = UserDetail.WalletUserId;
                                _masterCard.TransactionNo = invoiceNumber.InvoiceNumber;
                                _masterCard.FlatCharges = _commission.FlatCharges;
                                _masterCard.BenchmarkCharges = _commission.BenchmarkCharges;
                                _masterCard.CommissionAmount = _commission.CommissionAmount.ToString();
                                await _cardPaymentRepository.SaveMasterCardPaymentRequest(_masterCard);

                                CardPaymentRequest _cardRequest = new CardPaymentRequest();
                                _cardRequest.WalletUserId = UserDetail.WalletUserId;
                                _cardRequest.TotalAmount = _commission.AmountWithCommission.ToString();
                                _cardRequest.CommissionAmount = Convert.ToString(_commission.CommissionAmount);
                                _cardRequest.Amount = _commission.TransactionAmount.ToString();// Convert.ToString(requestAmo);
                                _cardRequest.FlatCharges = _commission.FlatCharges;
                                _cardRequest.BenchmarkCharges = _commission.BenchmarkCharges;
                                _cardRequest.CommisionCharges = _commission.CommisionPercent;
                                _cardRequest.CreatedDate = DateTime.UtcNow;
                                _cardRequest.UpdatedDate = DateTime.UtcNow;
                                _cardRequest.AmountInCedi = finalAmt.ToString(); //Euro
                                _cardRequest.OrderNo = invoiceNumber.InvoiceNumber;
                                _cardRequest = await _cardPaymentRepository.SaveCardPaymentRequest(_cardRequest);


                                var _Requestcustomer = new customer();
                                _Requestcustomer.email = UserDetail.EmailId;

                                var _RequestAttributes = new flutterRequest();
                                _RequestAttributes.currency = "EUR";
                                _RequestAttributes.tx_ref = invoiceNumber.InvoiceNumber;
                                _RequestAttributes.redirect_url = CommonSetting.flutterCallBackUrl;
                                _RequestAttributes.amount = Convert.ToString(finalAmt); //Euro
                                _RequestAttributes.customer = _Requestcustomer;
                                _RequestAttributes.payment_options = "card";

                                var req = JsonConvert.SerializeObject(_RequestAttributes);

                                _logUtils.WriteTextToFileForFlutterPeyLoadLogs("SaveflutterCardRequestDetailEuro :- InvoiceNumber " + invoiceNumber.InvoiceNumber + " " + req);
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                //here to get psaymenturl
                                var responseData2 = await GethashorUrl(req, null, "flutterUrl");
                                var _responseModel2 = JsonConvert.DeserializeObject<flutterPaymentUrlResponse>(responseData2);
                                if (_responseModel2.data.link != null && _responseModel2.status == "success")
                                {
                                    transationInitiate.InvoiceNumber = invoiceNumber.InvoiceNumber;
                                    transationInitiate.ReceiverNumber = UserDetail.MobileNo;
                                    transationInitiate.ServiceName = "FlutterEURO";
                                    transationInitiate.RequestedAmount = _commission.TransactionAmount.ToString();
                                    transationInitiate.TransactionStatus = (int)TransactionStatus.Pending;
                                    transationInitiate.WalletUserId = UserDetail.WalletUserId;
                                    transationInitiate.UserReferanceNumber = invoiceNumber.AutoDigit;
                                    transationInitiate.CurrentBalance = UserDetail.CurrentBalance;
                                    transationInitiate.AfterTransactionBalance = "";
                                    transationInitiate.ReceiverCurrentBalance = UserDetail.CurrentBalance;
                                    transationInitiate.UserName = UserDetail.FirstName + " " + UserDetail.LastName;
                                    transationInitiate.ReceiverWalletUserId = UserDetail.WalletUserId;
                                    transationInitiate.CreatedDate = DateTime.UtcNow;
                                    transationInitiate.UpdatedDate = DateTime.UtcNow;
                                    transationInitiate.IsActive = true;
                                    transationInitiate.IsDeleted = false;
                                    transationInitiate.JsonRequest = responseData2;
                                    transationInitiate.JsonResponse = "";
                                    transationInitiate = await _cardPaymentRepository.SaveTransactionInitiateRequest(transationInitiate);
                                    response.URL = _responseModel2.data.link;
                                    response.RstKey = 2;
                                }

                                else
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.Message = "Please try after some time aggregator error.";
                                }
                                LogTransactionTypes.Request.SaveTransactionLog(LogTransactionNameTypes.AddMoney + "Credit TO Debit Cards", responseData2, "Request Url : " + response.data.link);
                            }
                            else
                            {
                                var addLimit = limit - (Convert.ToDecimal(request.Amount) + totalAmountTransfered);
                                if (addLimit < Convert.ToDecimal(request.Amount))
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.Message = "Exceed your transaction limit.";
                                }
                                else
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.Message = "You can not add more then " + transactionLimit.TransactionLimitForAddMoney.ToString() + " cedi in a day";
                                }
                            }
                        }
                        else if (UserDetail.DocumetStatus == 0 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
                        {
                            response.RstKey = 13;
                            response.Message = ResponseMessageKyc.FAILED_Doc_NotUploaded;
                        }
                        else if (UserDetail.DocumetStatus == 1 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
                        {
                            response.RstKey = 14;
                            response.Message = ResponseMessageKyc.FAILED_Doc_Pending;
                        }
                        else if (UserDetail.DocumetStatus == 4 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
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
                        response.RstKey = 6;
                        response.StatusCode = (int)TransactionStatus.Failed;
                        response.Message = ResponseMessages.EMAIL_VERIFICATION_PENDING;
                    }
                }
                else
                {
                    response.RstKey = 6;
                    response.Message = ResponseMessages.TRANSACTION_DISABLED;
                }
            }

            catch (Exception ex)
            {

                //tran.Rollback();
            }
            return response;

        }

        //flutter bank transfer   :- debit request to get popup
        public async Task<flutterbanktransferauthorization> GetCardPaymentUrlForNGNbanktransferflutter(ThirdpartyPaymentByCardRequest request, string headerToken)
        {
            var response = new flutterbanktransferauthorization();
            var _commission = new CalculateCommissionResponse();
            var _commissionRequest = new CalculateCommissionRequest();

            var transationInitiate = new TransactionInitiateRequest();
            var _thirdPartyPaymentByCard = new ThirdPartyPaymentByCard();

            try
            {

                var UserDetail = await _walletUserService.UserProfile(headerToken);

                var Isdocverified = await _walletUserRepository.IsDocVerifiedMOMO(UserDetail.DocumetStatus);

                var resultTL = await _transactionLimitAUService.CheckTransactionLimitAU(request.WalletUserId.ToString()); //check New TL
                var transactionLimit = await _masterDataRepository.GetTransactionLimitAddMoney(Convert.ToString(UserDetail.WalletUserId));
                int limit = transactionLimit != null ? Convert.ToInt32(transactionLimit.TransactionLimitForAddMoney) : 0;

                var transactionHistory = _masterDataRepository.GetAllTransactionsAddMoney(UserDetail.WalletUserId);
                int totalAmountTransfered = transactionHistory != null ? Convert.ToInt32(transactionHistory.totalAmount) : 0;

                #region Calculate commission on request amount               
                //------Get Currency Rate--------------
                var currencyDetail = _masterDataRepository.GetCurrencyRate();
                decimal NGNRate = Convert.ToDecimal(currencyDetail.NGNRate);//Add NGN Rate

                decimal requestAmount = Convert.ToDecimal(request.Amount);//;

                var WalletServiceId = await _cardPaymentRepository.GetWalletService("AddBankFlutter", 40);

                if (UserDetail.IsActive == true)//am
                {
                    if (UserDetail.IsEmailVerified == true)
                    {
                        if (Isdocverified == true)
                        {
                            if (transactionLimit == null || transactionLimit.TransactionLimitForAddMoney == null || limit >= (Convert.ToDecimal(request.Amount) + totalAmountTransfered))
                            {
                                if (WalletServiceId.WalletServiceId > 0)
                                {
                                    #region Calculate Commission on request amount
                                    _commissionRequest.IsRoundOff = true;
                                    _commissionRequest.TransactionAmount = Convert.ToDecimal(request.Amount);
                                    _commissionRequest.WalletServiceId = WalletServiceId.WalletServiceId;
                                    _commission = await _setCommisionRepository.CalculatePayNGNTransferAddMoneyCommission(_commissionRequest);
                                    #endregion
                                }


                                decimal amountWithCommision = decimal.Parse(string.Format("{0:0,0}", _commission.AmountWithCommission));    // "1,234,257";

                                decimal amt = (amountWithCommision * NGNRate);
                                var finalAmt = Decimal.Parse(amt.ToString("0.00"));
                                #endregion
                                if (resultTL != null)
                                {
                                    decimal SetAmount = Convert.ToDecimal(resultTL.SetAmount);// returns decimal
                                    decimal TotalAmount = Convert.ToDecimal(resultTL.TotalAmount);// returns decimal

                                    if (SetAmount != 0) //0 =msg 
                                    {
                                        decimal requestAmountwithcomm = amountWithCommision + TotalAmount;

                                        if (requestAmountwithcomm <= SetAmount)//1000 >= 1000
                                        {

                                        }
                                        else
                                        {
                                            response.RstKey = 6;
                                            response.Message = ResponseMessages.INVALID_txnAmountREQUEST;
                                            return response;
                                        }
                                    }
                                }
                                var invoiceNumber = await _masterDataRepository.GetInvoiceNumber();
                                var _masterCard = new MasterCardPaymentRequest();

                                _masterCard.SessionId = null;
                                _masterCard.Version = null;
                                _masterCard.SuccessIndicator = null;
                                _masterCard.Merchant = "Flutterbanktransfer";
                                _masterCard.IsActive = true;
                                _masterCard.IsDeleted = false;
                                _masterCard.CreatedDate = DateTime.UtcNow;
                                _masterCard.UpdatedDate = DateTime.UtcNow;
                                _masterCard.Amount = request.Amount;
                                _masterCard.CommisionCharges = _commission.CommisionPercent;
                                _masterCard.TotalAmount = Convert.ToString(amountWithCommision);
                                _masterCard.WalletUserId = UserDetail.WalletUserId;
                                _masterCard.TransactionNo = invoiceNumber.InvoiceNumber;
                                _masterCard.FlatCharges = _commission.FlatCharges;
                                _masterCard.BenchmarkCharges = _commission.BenchmarkCharges;
                                _masterCard.CommissionAmount = _commission.CommissionAmount.ToString();
                                await _cardPaymentRepository.SaveMasterCardPaymentRequest(_masterCard);

                                CardPaymentRequest _cardRequest = new CardPaymentRequest();
                                _cardRequest.WalletUserId = UserDetail.WalletUserId;
                                _cardRequest.TotalAmount = _commission.AmountWithCommission.ToString();
                                _cardRequest.CommissionAmount = Convert.ToString(_commission.CommissionAmount);
                                _cardRequest.Amount = _commission.TransactionAmount.ToString();// Convert.ToString(requestAmo);
                                _cardRequest.FlatCharges = _commission.FlatCharges;
                                _cardRequest.BenchmarkCharges = _commission.BenchmarkCharges;
                                _cardRequest.CommisionCharges = _commission.CommisionPercent;
                                _cardRequest.CreatedDate = DateTime.UtcNow;
                                _cardRequest.UpdatedDate = DateTime.UtcNow;
                                _cardRequest.AmountInCedi = finalAmt.ToString(); //NGN
                                _cardRequest.OrderNo = invoiceNumber.InvoiceNumber;
                                _cardRequest = await _cardPaymentRepository.SaveCardPaymentRequest(_cardRequest);

                                var _Requestcustomer = new customer();
                                _Requestcustomer.email = UserDetail.EmailId;

                                var _RequestAttributes = new flutterbanktransferRequest();
                                _RequestAttributes.currency = "NGN";
                                _RequestAttributes.tx_ref = invoiceNumber.InvoiceNumber;
                                _RequestAttributes.email = UserDetail.EmailId;
                                //_RequestAttributes.amount = Convert.ToString(finalAmt); //Euro
                                var intfinalAmt = int.Parse(amt.ToString("0"));
                                _RequestAttributes.amount = intfinalAmt; //NGN

                                var req = JsonConvert.SerializeObject(_RequestAttributes);

                                _logUtils.WriteTextToFileForFlutterPeyLoadLogs("SaveflutterCardRequestDetailbanktransferNGN :- InvoiceNumber " + invoiceNumber.InvoiceNumber + " " + req);
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                //here to get psaymenturl
                                var responseData2 = await GethashorUrl(req, null, "flutterbanktransfer");
                                var _responseModel2 = JsonConvert.DeserializeObject<flutterbanktransferResponse>(responseData2);
                                if (_responseModel2.meta.authorization.transfer_account != null && _responseModel2.status == "success")
                                {
                                    transationInitiate.InvoiceNumber = invoiceNumber.InvoiceNumber;
                                    transationInitiate.ReceiverNumber = UserDetail.MobileNo;
                                    transationInitiate.ServiceName = "flutter Banktransfer";
                                    transationInitiate.RequestedAmount = _commission.TransactionAmount.ToString();
                                    transationInitiate.TransactionStatus = (int)TransactionStatus.Pending;
                                    transationInitiate.WalletUserId = UserDetail.WalletUserId;
                                    transationInitiate.UserReferanceNumber = invoiceNumber.AutoDigit;
                                    transationInitiate.CurrentBalance = UserDetail.CurrentBalance;
                                    transationInitiate.AfterTransactionBalance = "";
                                    transationInitiate.ReceiverCurrentBalance = UserDetail.CurrentBalance;
                                    transationInitiate.UserName = UserDetail.FirstName + " " + UserDetail.LastName;
                                    transationInitiate.ReceiverWalletUserId = UserDetail.WalletUserId;
                                    transationInitiate.CreatedDate = DateTime.UtcNow;
                                    transationInitiate.UpdatedDate = DateTime.UtcNow;
                                    transationInitiate.IsActive = true;
                                    transationInitiate.IsDeleted = false;
                                    transationInitiate.JsonRequest = responseData2;
                                    transationInitiate.JsonResponse = "";
                                    transationInitiate = await _cardPaymentRepository.SaveTransactionInitiateRequest(transationInitiate);

                                    if (UserDetail != null && transationInitiate.InvoiceNumber == invoiceNumber.InvoiceNumber)
                                    {

                                        //var _tranDate = DateTime.UtcNow;

                                        //var tran = new WalletTransaction();
                                        //tran.BeneficiaryName = request.BeneficiaryName;
                                        //tran.CreatedDate = _tranDate;
                                        //tran.UpdatedDate = _tranDate;
                                        //tran.IsAddDuringPay = false;
                                        ////Self Account 
                                        //tran.ReceiverId = UserDetail.WalletUserId;
                                        ////Sender
                                        //tran.WalletServiceId = 40;
                                        //tran.TransactionType = AggragatorServiceType.CREDIT;
                                        //tran.TransactionTypeInfo = (int)TransactionTypeInfo.AddedByCard;
                                        //tran.VoucherCode = string.Empty;
                                        //tran.SenderId = UserDetail.WalletUserId;
                                        //tran.WalletAmount = request.Amount;
                                        //tran.ServiceTax = "0";
                                        //tran.ServiceTaxRate = 0;
                                        //tran.DisplayContent = request.DisplayContent;
                                        //tran.UpdatedOn = DateTime.UtcNow;
                                        //tran.TransactionInitiateRequestId = transationInitiate.Id;
                                        //tran.AccountNo = _responseModel2.meta.authorization.transfer_account;// string.Empty;                                                  
                                        //tran.BankTransactionId = string.Empty;
                                        //tran.IsBankTransaction = false;
                                        //tran.BankBranchCode = string.Empty;
                                        //tran.TransactionId = invoiceNumber.InvoiceNumber;
                                        //tran.TransactionStatus = (int)TransactionStatus.Pending;
                                        //tran.IsAdminTransaction = true;
                                        //tran.IsActive = true;
                                        //tran.IsDeleted = false;
                                        //tran.InvoiceNo = invoiceNumber.InvoiceNumber;
                                        //tran.Comments = request.Comment;
                                        //tran.MerchantCommissionId = _commission.MerchantCommissionId;
                                        //tran.MerchantCommissionAmount = Convert.ToString(_commission.MerchantCommissionAmount);
                                        //tran.CommisionId = _commission.CommissionId;
                                        //tran.CommisionAmount = Convert.ToString(_commission.CommissionAmount);
                                        //tran.CommisionPercent = _commission.CommissionAmount.ToString();
                                        //tran.TotalAmount = Convert.ToString(_commission.AmountWithCommission);
                                        //tran.OperatorType = "BankTransfer";
                                        //try
                                        //{
                                        //int i = await _walletUserRepository.InsertWalletTransaction(tran);

                                        //if (i == 1)
                                        //{
                                        response.transfer_reference = _responseModel2.meta.authorization.transfer_reference;
                                        response.transfer_account = _responseModel2.meta.authorization.transfer_account;
                                        response.transfer_bank = _responseModel2.meta.authorization.transfer_bank;
                                        response.account_expiration = _responseModel2.meta.authorization.account_expiration;
                                        response.transfer_note = _responseModel2.meta.authorization.transfer_note;
                                        response.transfer_amount = _responseModel2.meta.authorization.transfer_amount;
                                        response.mode = _responseModel2.meta.authorization.mode;

                                        response.RstKey = 2;
                                        //}
                                        //}
                                        //catch (Exception ex)
                                        //{
                                        //    "NGNbanktransferflutter".ErrorLog("CardPaymentService", "GetCardPaymentUrlForNGNbanktransferflutter", ex.StackTrace + " " + ex.Message);
                                        //}
                                    }
                                    else
                                    {
                                        response.RstKey = 9;
                                        response.Message = ResponseMessages.TRANSACTION_NULL_ERROR;
                                    }

                                }

                                else
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.Message = "Please try after some time aggregator error.";
                                }
                                LogTransactionTypes.Request.SaveTransactionLog(LogTransactionNameTypes.AddMoney + "Credit TO Debit Cards", responseData2, "Request Url : " + response.transfer_account);
                            }
                            else
                            {
                                var addLimit = limit - (Convert.ToDecimal(request.Amount) + totalAmountTransfered);
                                if (addLimit < Convert.ToDecimal(request.Amount))
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.Message = "Exceed your transaction limit.";
                                }
                                else
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.Message = "You can not add more then " + transactionLimit.TransactionLimitForAddMoney.ToString() + " cedi in a day";
                                }
                            }
                        }
                        else if (UserDetail.DocumetStatus == 0 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
                        {
                            response.RstKey = 13;
                            response.Message = ResponseMessageKyc.FAILED_Doc_NotUploaded;
                        }
                        else if (UserDetail.DocumetStatus == 1 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
                        {
                            response.RstKey = 14;
                            response.Message = ResponseMessageKyc.FAILED_Doc_Pending;
                        }
                        else if (UserDetail.DocumetStatus == 4 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
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
                        response.RstKey = 6;
                        response.StatusCode = (int)TransactionStatus.Failed;
                        response.Message = ResponseMessages.EMAIL_VERIFICATION_PENDING;
                    }
                }
                else
                {
                    response.RstKey = 6;
                    response.Message = ResponseMessages.TRANSACTION_DISABLED;
                }
            }

            catch (Exception ex)
            {
                //tran.Rollback();
            }
            return response;

        }



        public async Task<string> notificationsarl()
        {
            string resString = "";

            List<long> WalletUserIdlist = new List<long>
{
 8106,
 5586,
19331,
76974,
80785,
34473,
58907,
35334,
47436,
33159,
76366,
58658,
74815,
43607,
80990,
59062,
76213,
16640,
81299,
59242,
76262,
65835,
38393,
80445,
69679,
74695,
21868,
70688,
75806,
67947,
80926,
75831,
41624,
24842,
32980,
43520,
23696,
27177,
11802,
37722,
76701,
62808,
11352,
75824,
57760,
76040,
59976,
78069,
81060,
81064,
80314,
81331,
55729,
65829,
68099,
74970,
62521,
55086,
60245,
53419,
42590,
57515,
58132,
72167,
19754,
21849,
65608,
66772,
12357,
76280,
76462,
31679,
29604,
80912,
81569,
64948,
80914,
62590,
55565,
28849,
44654,
61763,
66208,
24403,
29759,
48552,
80775,
76171,
70301,
58038,
69011,
15604,
76207,
76970,
17130,
80727,
68992,
68007,
68619,
16665,
36281,
12605,
61603,
78419,
8143 ,
22787,
12582,
44906,
56220,
21122,
65953,
75995,
49313,
81138,
58653,
9273 ,
59694,
40678,
55116,
81437,
76586,
80974,
31903,
9236 ,
77429,
58727,
20598,
66873,
76241,
65602,
58292,
76125,
81043,
79272,
15079,
33744,
37048,
46414,
33128,
60672,
74201,
44650,
16594,
69723,
14956,
62787,
75749,
80171,
81321,
77802,
58977,
40205,
41462,
45137,
28768,
22037,
38134,
76063,
14599,
57613,
48840,
65899,
22267,
60236,
43133,
37582,
76088,
20701,
54354,
43496,
81306,
18430,
20163,
74565,
76692,
60222,
76112,
62156,
72079,
81573,
76200,
70932,
80380,
70245,
76183,
66171,
81085,
8603 ,
57222,
75384,
76139,
76085,
70673,
76776,
29649,
51996,
62804,
69924,
27008,
80698,
80612,
37013,
32200,
74573,
33641,
25734,
16668,
76151,
21201,
78398,
61992,
68946,
80082,
75971,
76394,
74966,
38688,
80385,
80931,
81106,
76870,
66188,
80918,
61062,
18079,
76208,
70086,
76604,
81446,
46627,
16680,
9067 ,
15527,
74765,
23007,
64796,
81076,
36063,
78851,
76789,
79794,
71615,
80851,
13480,
68662,
46265,
65090,
77330,
81497,
69159,
18809,
75727,
75823,
66621,
68896,
59836,
80757,
34169,
71604,
76019,
64985,
21632,
21232,
77835,
75799,
76644,
77103,
74602,
62178,
30164,
71424,
81147,
36602,
26535,
72452,
26724,
26085,
59039,
37280,
61998,
58660,
80245,
66623,
76682,
57035,
68193,
78263,
80218,
57732,
66430,
42246,
62201,
62840,
78241,
18289,
70769,
28385,
16988,
65906,
76306,
71524,
49515,
49306,
75492,
34322,
19308,
58738,
25298,
80678,
66923,
80433,
65790,
60308,
12971,
73065,
58609,
74495,
74825,
70007,
39762,
32333,
16822,
8594 ,
80962,
21510,
14712,
59880,
35537,
59426,
71358,
28020,
19713,
46999,
74475,
74568,
81304,
60425,
65480,
68709,
81657,
15926,
71875,
39646,
8719 ,
32367,
80860,
80555,
59924,
74576,
58596,
63181,
67478,
54672,
16868,
80686,
41584,
9658 ,
16635,
65417,
55443,
67498,
70962,
71801,
16870,
70660,
78253,
61714,
57575,
78328,
77828,
80471,
81059,
46381,
16480,
21829,
19604,
66083,
69692,
80241,
76451,
62238,
65832,
73235,
75183,
64901,
68486,
64924,
16752,
76101,
38246,
74081,
76529,
80489,
59777,
72314,
35076,
79840,
67117,
21338,
16894,
54015,
8652 ,
74418,
50415,
80340,
36005,
20284,
74715,
61939,
11917,
27694,
67161,
62668,
46189,
40086,
32623,
27012,
80993,
9856 ,
68862,
80587,
29451,
22611,
44721,
11678,
20876,
20702,
57517,
77088,
25138,
15874,
65733,
77342,
64042,
65369,
66852,
80839,
76030,
79776,
58182,
80627,
75005,
40394,
80281,
8106 ,
75319,
76806,
68008,
80537,
78393,
17748,
69054,
54541,
15200,
25191,
54067,
21320,
20510,
21035,
74742,
15269,
75750,
49173,
76076,
67344,
63362,
74755,
66414,
11139,
23183,
23074,
36559,
80494

};

            DateTime TDate = DateTime.UtcNow;
            foreach (var WalletUserId in WalletUserIdlist)
            {


                var CurrentUser = await _walletUserRepository.GetCurrentUser(WalletUserId);
                if (CurrentUser != null)
                {
                    PushNotificationModel push = new PushNotificationModel();
                    push.SenderId = CurrentUser.WalletUserId;
                    push.deviceType = (int)CurrentUser.DeviceType;
                    push.deviceKey = CurrentUser.DeviceToken;
                    PayMoneyPushModel pushModel = new PayMoneyPushModel();
                    pushModel.TransactionDate = TDate;
                    pushModel.TransactionId = "";
                    pushModel.CurrentBalance = CurrentUser.CurrentBalance;
                    pushModel.alert = "Utilisez www.magremit.com avec les mêmes informations d'identification si vous rencontrez des problèmes et bénéficiez également de meilleurs tarifs. AUCUNE OFFRE REQUISE KYC VALABLE POUR UNE PÉRIODE LIMITÉE !!";
                    pushModel.Amount = "";
                    pushModel.CurrentBalance = CurrentUser.CurrentBalance;
                    pushModel.pushType = (int)PushType.ADDMONEY;

                    if ((int)CurrentUser.DeviceType == (int)DeviceTypes.ANDROID || (int)CurrentUser.DeviceType == (int)DeviceTypes.Web)
                    {
                        PushPayload<PayMoneyPushModel> aps = new PushPayload<PayMoneyPushModel>();
                        PushPayloadData<PayMoneyPushModel> _data = new PushPayloadData<PayMoneyPushModel>();
                        _data.notification = pushModel;
                        aps.data = _data;
                        aps.to = CurrentUser.DeviceToken;
                        aps.collapse_key = string.Empty;
                        push.message = JsonConvert.SerializeObject(aps);
                        push.payload = pushModel;
                    }
                    if ((int)CurrentUser.DeviceType == (int)DeviceTypes.IOS)
                    {
                        NotificationJsonResponse<PayMoneyIOSPushModel> aps = new NotificationJsonResponse<PayMoneyIOSPushModel>();
                        PayMoneyIOSPushModel _iosPushModel = new PayMoneyIOSPushModel();
                        _iosPushModel.alert = pushModel.alert;
                        _iosPushModel.Amount = pushModel.Amount;
                        _iosPushModel.CurrentBalance = pushModel.CurrentBalance;
                        _iosPushModel.MobileNo = pushModel.MobileNo;
                        _iosPushModel.SenderName = pushModel.SenderName;
                        _iosPushModel.pushType = pushModel.pushType;
                        aps.aps = _iosPushModel;

                        push.message = JsonConvert.SerializeObject(aps);
                    }
                    if (!string.IsNullOrEmpty(push.message))
                    {
                        _sendPushNotification.sendPushNotification(push);
                    }
                }

            }
            return "";

        }



        //      public async Task<flutterbankResponse> GetZenithBankUrlByOTP(ZenithBankOTPRequest request, string headerToken)
        //      {
        //          var response1 = new flutterbankResponse();

        //          try
        //          {


        //              var _RequestAttributes = new ZenithBankOTPRequest();
        //              _RequestAttributes.WalletUserId = request.WalletUserId;
        //              _RequestAttributes.otp = request.otp;
        //              _RequestAttributes.flw_ref = request.flw_ref;
        //              _RequestAttributes.type = "account";

        //              var req = JsonConvert.SerializeObject(_RequestAttributes);

        //              _logUtils.WriteTextToFileForBankFlutterZenithBankOTP("ZenithBankOTPRequest :-" + req);

        //              var _RequestAttributes1 = new ZenithBankOTPRequestapi();

        //              _RequestAttributes1.otp = request.otp;
        //              _RequestAttributes1.flw_ref = request.flw_ref;
        //              _RequestAttributes1.type = "account";

        //              var req1 = JsonConvert.SerializeObject(_RequestAttributes1);
        //              //here to get psaymenturl
        //              var responseData2 = await GethashorUrl(req1, null, "ZenithBankOTPRequest");
        //              _logUtils.WriteTextToFileForBankFlutterZenithBankOTP("ZenithBankOTPResponse :-" + responseData2);

        //              var _responseModel2 = JsonConvert.DeserializeObject<FlutterCardPaymentWebResponse>(responseData2);
        //              if (_responseModel2.data.status == "successful")
        //              {
        //                  var requestbnk = new BankPaymentWebResponse();
        //                  var response = new AddMoneyAggregatorResponse();
        //                  requestbnk.txRef = _responseModel2.data.tx_ref;
        //                  requestbnk.status = _responseModel2.data.status;
        //                  requestbnk.orderRef = _responseModel2.data.tx_ref;
        //                  response = await SaveflutterBankPaymentResponse(requestbnk);
        //                  var routeValues = new RouteValueDictionary {
        //{ "InvoiceNo", response.InvoiceNo },  { "Amount",  response.Amount },{ "TransactionDate",  response.TransactionDate },
        //{ "status", response.status },{ "RstKey", response.RstKey }};
        //                  HttpContext.Current.Response.RedirectToRoutePermanent("ZenithBankOTPResponse", routeValues);
        //                  //transationInitiate.InvoiceNumber = invoiceNumber.InvoiceNumber;
        //                  //transationInitiate.ReceiverNumber = UserDetail.MobileNo;
        //                  //transationInitiate.ServiceName = "AddBankFlutter Card Payment";
        //                  //transationInitiate.RequestedAmount = _commission.TransactionAmount.ToString();
        //                  //transationInitiate.TransactionStatus = (int)TransactionStatus.Pending;
        //                  //transationInitiate.WalletUserId = UserDetail.WalletUserId;
        //                  //transationInitiate.UserReferanceNumber = invoiceNumber.AutoDigit;
        //                  //transationInitiate.CurrentBalance = UserDetail.CurrentBalance;
        //                  //transationInitiate.AfterTransactionBalance = "";
        //                  //transationInitiate.ReceiverCurrentBalance = UserDetail.CurrentBalance;
        //                  //transationInitiate.UserName = UserDetail.FirstName + " " + UserDetail.LastName;
        //                  //transationInitiate.ReceiverWalletUserId = UserDetail.WalletUserId;
        //                  //transationInitiate.CreatedDate = DateTime.UtcNow;
        //                  //transationInitiate.UpdatedDate = DateTime.UtcNow;
        //                  //transationInitiate.IsActive = true;
        //                  //transationInitiate.IsDeleted = false;
        //                  //transationInitiate.JsonRequest = responseData2;
        //                  //transationInitiate.JsonResponse = "";
        //                  //transationInitiate = await _cardPaymentRepository.SaveTransactionInitiateRequest(transationInitiate);
        //                  ////response.URL = _responseModel2.data.auth_url;
        //                  //if (_responseModel2.data.auth_url != "NO-URL")
        //                  //{
        //                  //    response.flw_ref = _responseModel2.data.flw_ref;
        //                  //    response.processor_response = _responseModel2.data.processor_response;
        //                  //    response.type = "card";
        //                  //    response.URL = _responseModel2.data.auth_url;
        //                  //}
        //                  //else
        //                  //{

        //                  //    response.URL = _responseModel2.data.auth_url;
        //                  //}

        //              }

        //              else
        //              {
        //                  response1.RstKey = 6;
        //                  response1.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
        //                  response1.Message = "Please try after some time aggregator error.";
        //              }


        //          }

        //          catch (Exception ex)
        //          {
        //              "GetZenithBankUrlByOTP".ErrorLog("CardPaymentService", "GetZenithBankUrlByOTP", ex.StackTrace + " " + ex.Message);
        //          }
        //          return response1;

        //      }

        //public async Task<AddMoneyAggregatorResponse> SaveflutterCardPaymentResponsewebhook(fluttercallbackResponsewebhook request)
        //{
        //    AddMoneyAggregatorResponse response = new AddMoneyAggregatorResponse();
        //    CalculateCommissionResponse _commission = new CalculateCommissionResponse();
        //    CalculateCommissionRequest _commissionRequest = new CalculateCommissionRequest();
        //    string RequestString = JsonConvert.SerializeObject(request);
        //    LogTransactionTypes.Response.SaveTransactionLog(LogTransactionNameTypes.AddMoney + "Credit to Debit Cards", RequestString, "Responce by bank detail : ");

        //    try
        //    {

        //        var getInitialTransaction = await _cardPaymentRepository.GetTransactionInitiateRequest(request.txRef);

        //        int GetWalletTransactionsexist = await _cardPaymentRepository.GetWalletTransactionsexist(getInitialTransaction.WalletUserId, request.txRef);

        //        if (request.txRef != null && GetWalletTransactionsexist == 0)
        //        {

        //            getInitialTransaction.JsonResponse = JsonConvert.SerializeObject(request);
        //            var responseData2 = await GethashorUrl(request.txRef, null, "flutterUrlverify");

        //            JavaScriptSerializer js = new JavaScriptSerializer();
        //            dynamic blogObject = js.Deserialize<dynamic>(responseData2);


        //            var txnreverifystatus = blogObject["data"]["status"];//stagin

        //            //check txn verify flutter --when not succesfujl statsu got from txn verify & suceeful get from cllback 
        //            if (txnreverifystatus != "successful" && request.status == "successful")
        //            {
        //                response.InvoiceNo = request.txRef;
        //                response.Amount = getInitialTransaction.RequestedAmount;
        //                response.status = "flutter";
        //                DateTime TDate = DateTime.UtcNow;
        //                response.TransactionDate = TDate;

        //                int WalletServiceId = await _cardPaymentRepository.GetServiceId();
        //                if (WalletServiceId > 0)
        //                {
        //                    var adminUser = await _cardPaymentRepository.GetAdminUser();
        //                    if (adminUser != null)
        //                    {
        //                        getInitialTransaction.RequestedAmount = Convert.ToString(Math.Round(Convert.ToDecimal(getInitialTransaction.RequestedAmount), 2));

        //                        long userId = Convert.ToInt32(getInitialTransaction.WalletUserId);
        //                        var UserCurrentDetail = await _walletUserRepository.GetCurrentUser(userId);
        //                        if (UserCurrentDetail != null && await _cardPaymentRepository.IsWalletTransactions(UserCurrentDetail.WalletUserId, request.id))
        //                        {
        //                            getInitialTransaction.RequestedAmount = Convert.ToString(Math.Round(Convert.ToDecimal(getInitialTransaction.RequestedAmount), 2));

        //                            _commissionRequest.CurrentBalance = Convert.ToDecimal(UserCurrentDetail.CurrentBalance);
        //                            _commissionRequest.IsRoundOff = true;
        //                            _commissionRequest.TransactionAmount = Convert.ToDecimal(getInitialTransaction.RequestedAmount); //change
        //                            _commissionRequest.WalletServiceId = WalletServiceId;
        //                            _commission = await _setCommisionRepository.CalculateCommission(_commissionRequest);

        //                            getInitialTransaction.TransactionStatus = (int)TransactionStatus.Pending;
        //                            await _cardPaymentRepository.UpdateTransactionInitiateRequest(getInitialTransaction);

        //                            #region Save Transaction
        //                            decimal amountWithCommision = decimal.Parse(string.Format("{0:0,0}", _commission.AmountWithCommission));    // "1,234,257";

        //                            var _Transaction = new WalletTransaction();

        //                            _Transaction.TransactionInitiateRequestId = getInitialTransaction.Id;
        //                            _Transaction.MerchantCommissionAmount = Convert.ToString(_commission.MerchantCommissionAmount);
        //                            _Transaction.MerchantCommissionId = _commission.MerchantCommissionId;
        //                            _Transaction.TransactionType = AggragatorServiceType.CREDIT;
        //                            _Transaction.IsBankTransaction = false;
        //                            _Transaction.TransactionTypeInfo = (int)TransactionTypeInfo.AddedByCard;
        //                            _Transaction.IsBankTransaction = false;
        //                            _Transaction.Comments = string.Empty;
        //                            _Transaction.InvoiceNo = getInitialTransaction.InvoiceNumber;
        //                            _Transaction.CommisionAmount = Convert.ToString(_commission.CommissionAmount);
        //                            _Transaction.CommisionId = _commission.CommissionId;
        //                            _Transaction.WalletAmount = Convert.ToString(_commission.TransactionAmount);

        //                            _Transaction.TotalAmount = Convert.ToString(amountWithCommision);
        //                            _Transaction.ServiceTaxRate = _commission.ServiceTaxRate;
        //                            _Transaction.ServiceTax = Convert.ToString(_commission.ServiceTaxAmount);
        //                            _Transaction.OperatorType = "flutter";

        //                            _Transaction.AccountNo = getInitialTransaction.ReceiverNumber;

        //                            _Transaction.TransactionStatus = (int)TransactionStatus.Pending;

        //                            _Transaction.WalletServiceId = WalletServiceId;
        //                            _Transaction.SenderId = UserCurrentDetail.WalletUserId;
        //                            _Transaction.ReceiverId = UserCurrentDetail.WalletUserId;
        //                            _Transaction.BankBranchCode = string.Empty;
        //                            _Transaction.BankTransactionId = request.id;
        //                            _Transaction.TransactionId = getInitialTransaction.InvoiceNumber;


        //                            _Transaction.IsAdminTransaction = false;
        //                            _Transaction.IsActive = true;
        //                            _Transaction.IsDeleted = false;
        //                            _Transaction.CreatedDate = TDate;
        //                            _Transaction.UpdatedDate = TDate;
        //                            _Transaction.TransactionTypeInfo = (int)TransactionTypeInfo.AddedByCard;
        //                            _Transaction.IsAddDuringPay = false;
        //                            _Transaction.VoucherCode = string.Empty;

        //                            await _cardPaymentRepository.SaveWalletTransactions(_Transaction);
        //                            //db.WalletTransactions.Add(_Transaction);
        //                            //db.SaveChanges();
        //                            #endregion

        //                            #region Credit
        //                            var _credit = new WalletTransactionDetail();
        //                            _credit.Amount = Convert.ToString(_commission.TransactionAmount);
        //                            _credit.TransactionType = (int)TransactionDetailType.Credit;
        //                            _credit.WalletUserId = adminUser.WalletUserId;
        //                            _credit.WalletTransactionId = _Transaction.WalletTransactionId;
        //                            _credit.IsActive = true;
        //                            _credit.IsDeleted = false;
        //                            _credit.CreatedDate = TDate;
        //                            _credit.UpdatedDate = TDate;
        //                            //db.WalletTransactionDetails.Add(_credit);
        //                            //db.SaveChanges();
        //                            await _cardPaymentRepository.SaveWalletTransactionDetails(_credit);
        //                            #endregion

        //                            #region Debit
        //                            var _debit = new WalletTransactionDetail();
        //                            _debit.Amount = Convert.ToString(_commission.TransactionAmount);
        //                            _debit.TransactionType = (int)TransactionDetailType.Debit;
        //                            _debit.WalletUserId = UserCurrentDetail.WalletUserId;
        //                            _debit.WalletTransactionId = _Transaction.WalletTransactionId;
        //                            _debit.IsActive = true;
        //                            _debit.IsDeleted = false;
        //                            _debit.CreatedDate = TDate;
        //                            _debit.UpdatedDate = TDate;
        //                            //db.WalletTransactionDetails.Add(_credit);
        //                            //db.SaveChanges();
        //                            await _cardPaymentRepository.SaveWalletTransactionDetails(_credit);
        //                            #endregion

        //                            //get UpdateNewCardNoResponseBankCode id
        //                            await _cardPaymentRepository.UpdateNewCardNoResponseBankCode(getInitialTransaction.InvoiceNumber, UserCurrentDetail.WalletUserId, request.id);


        //                            var adminKeyPair = AES256.AdminKeyPair;


        //                            //db.SaveChanges();
        //                            //tran.Commit();


        //                            if (UserCurrentDetail.MobileNo != null && UserCurrentDetail.MobileNo != "")
        //                            {
        //                                response.ToMobileNo = AES256.Decrypt(adminKeyPair.PrivateKey, UserCurrentDetail.MobileNo);
        //                            }


        //                            response.CurrentBalance = UserCurrentDetail.CurrentBalance;
        //                            response.RstKey = 2;



        //                        }
        //                        else
        //                        {
        //                            //test

        //                        }
        //                    }
        //                    else
        //                    {
        //                        //test

        //                    }
        //                    //sdfsdfd
        //                }
        //                else
        //                {

        //                    response.RstKey = 2;
        //                }
        //            }
        //            //after verify flutter txn then crediht to user --when succesfujl statsu got from both
        //            else if (request.txRef != null && request.id != null && request.status == "successful" && txnreverifystatus == "successful")
        //            {
        //                response.InvoiceNo = request.txRef;
        //                response.Amount = getInitialTransaction.RequestedAmount;
        //                response.status = "flutter";
        //                DateTime TDate = DateTime.UtcNow;
        //                response.TransactionDate = TDate;

        //                int WalletServiceId = await _cardPaymentRepository.GetServiceId();
        //                if (WalletServiceId > 0)
        //                {
        //                    var adminUser = await _cardPaymentRepository.GetAdminUser();
        //                    if (adminUser != null)
        //                    {

        //                        // var receiver = await _walletUserRepository.GetCurrentUser(Convert.ToInt32(requestDetail.WalletUserId)); //db.WalletUsers.Where(x => x.WalletUserId == requestDetail.WalletUserId).FirstOrDefault();

        //                        long userId = Convert.ToInt32(getInitialTransaction.WalletUserId);
        //                        var UserCurrentDetail = await _walletUserRepository.GetCurrentUser(userId);
        //                        if (UserCurrentDetail != null && await _cardPaymentRepository.IsWalletTransactions(UserCurrentDetail.WalletUserId, request.id))
        //                        {
        //                            //this line commented due to currentbalance is not added to card expected 
        //                            //request.vpc_Amount = Convert.ToString(Math.Round((Convert.ToDecimal(requestDetail.Amount) / 100), 2));
        //                            getInitialTransaction.RequestedAmount = Convert.ToString(Math.Round(Convert.ToDecimal(getInitialTransaction.RequestedAmount), 2));

        //                            // to update wallet amount-----

        //                            // db.WalletUsers.FirstOrDefault(x => x.WalletUserId == receiver.WalletUserId);

        //                            if (UserCurrentDetail != null)
        //                            {
        //                                _commissionRequest.CurrentBalance = Convert.ToDecimal(UserCurrentDetail.CurrentBalance);
        //                                _commissionRequest.IsRoundOff = true;
        //                                //_commissionRequest.TransactionAmount = Convert.ToDecimal(request.vpc_Amount);

        //                                _commissionRequest.TransactionAmount = Convert.ToDecimal(getInitialTransaction.RequestedAmount); //change
        //                                _commissionRequest.WalletServiceId = WalletServiceId;
        //                                _commission = await _setCommisionRepository.CalculateCommission(_commissionRequest);

        //                                if (!string.IsNullOrEmpty(request.id))
        //                                {
        //                                    getInitialTransaction.TransactionStatus = (int)TransactionStatus.Completed;
        //                                    if (Convert.ToDecimal(UserCurrentDetail.CurrentBalance) >= 0)
        //                                    {
        //                                        if (Convert.ToDecimal(UserCurrentDetail.CurrentBalance) == 0)
        //                                        {
        //                                            UserCurrentDetail.CurrentBalance = _commission.TransactionAmount.ToString();
        //                                            getInitialTransaction.AfterTransactionBalance = UserCurrentDetail.CurrentBalance.ToString();
        //                                            getInitialTransaction.ReceiverCurrentBalance = UserCurrentDetail.CurrentBalance.ToString();
        //                                        }
        //                                        else
        //                                        {
        //                                            UserCurrentDetail.CurrentBalance = Math.Round(Convert.ToDecimal(UserCurrentDetail.CurrentBalance) + _commission.TransactionAmount, 2).ToString();
        //                                            getInitialTransaction.AfterTransactionBalance = UserCurrentDetail.CurrentBalance.ToString();
        //                                            getInitialTransaction.ReceiverCurrentBalance = UserCurrentDetail.CurrentBalance.ToString();
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        UserCurrentDetail.CurrentBalance = Math.Round(_commission.TransactionAmount - Convert.ToDecimal(UserCurrentDetail.CurrentBalance), 2).ToString();
        //                                        getInitialTransaction.AfterTransactionBalance = UserCurrentDetail.CurrentBalance.ToString();
        //                                        getInitialTransaction.ReceiverCurrentBalance = UserCurrentDetail.CurrentBalance.ToString();
        //                                    }
        //                                }
        //                                await _walletUserRepository.UpdateUserDetail(UserCurrentDetail);
        //                                await _cardPaymentRepository.UpdateTransactionInitiateRequest(getInitialTransaction);
        //                                // db.SaveChanges();
        //                            }

        //                            #region Save Transaction
        //                            decimal amountWithCommision = decimal.Parse(string.Format("{0:0,0}", _commission.AmountWithCommission));    // "1,234,257";

        //                            var _Transaction = new WalletTransaction();

        //                            _Transaction.TransactionInitiateRequestId = getInitialTransaction.Id;
        //                            _Transaction.MerchantCommissionAmount = Convert.ToString(_commission.MerchantCommissionAmount);
        //                            _Transaction.MerchantCommissionId = _commission.MerchantCommissionId;
        //                            _Transaction.TransactionType = AggragatorServiceType.CREDIT;
        //                            _Transaction.IsBankTransaction = false;
        //                            _Transaction.TransactionTypeInfo = (int)TransactionTypeInfo.AddedByCard;
        //                            _Transaction.IsBankTransaction = false;
        //                            _Transaction.Comments = string.Empty;
        //                            _Transaction.InvoiceNo = getInitialTransaction.InvoiceNumber;
        //                            _Transaction.CommisionAmount = Convert.ToString(_commission.CommissionAmount);
        //                            _Transaction.CommisionId = _commission.CommissionId;
        //                            _Transaction.WalletAmount = Convert.ToString(_commission.TransactionAmount);

        //                            _Transaction.TotalAmount = Convert.ToString(amountWithCommision);
        //                            _Transaction.ServiceTaxRate = _commission.ServiceTaxRate;
        //                            _Transaction.ServiceTax = Convert.ToString(_commission.ServiceTaxAmount);
        //                            _Transaction.OperatorType = "flutter";

        //                            _Transaction.AccountNo = getInitialTransaction.ReceiverNumber;
        //                            if (!string.IsNullOrEmpty(request.id))
        //                            {

        //                                _Transaction.TransactionStatus = (int)TransactionStatus.Completed; ;
        //                                try
        //                                {
        //                                    //--------send mail on success transaction--------

        //                                    var AdminKeys = AES256.AdminKeyPair;
        //                                    string FirstName = AES256.Decrypt(UserCurrentDetail.PrivateKey, UserCurrentDetail.FirstName);
        //                                    string LastName = AES256.Decrypt(UserCurrentDetail.PrivateKey, UserCurrentDetail.LastName);
        //                                    string StdCode = UserCurrentDetail.StdCode;
        //                                    string MobileNo = AES256.Decrypt(AdminKeys.PrivateKey, UserCurrentDetail.MobileNo);
        //                                    string EmailId = AES256.Decrypt(AdminKeys.PrivateKey, UserCurrentDetail.EmailId).Trim().ToLower();
        //                                    // var receiverDetail = new AppUserRepository().GetUserDetailById(receiver.WalletUserId);
        //                                    string filename = CommonSetting.successfullTransaction;


        //                                    var body = _sendEmails.ReadEmailformats(filename);
        //                                    body = body.Replace("$$FirstName$$", FirstName + " " + LastName);
        //                                    body = body.Replace("$$DisplayContent$$", "Flutter CARD");
        //                                    body = body.Replace("$$customer$$", MobileNo);
        //                                    body = body.Replace("$$amount$$", "XOF " + getInitialTransaction.RequestedAmount);
        //                                    body = body.Replace("$$ServiceTaxAmount$$", "XOF " + _commission.CommissionAmount);
        //                                    body = body.Replace("$$AmountWithCommission$$", "XOF " + amountWithCommision);
        //                                    body = body.Replace("$$TransactionId$$", request.txRef);

        //                                    var req = new EmailModel()
        //                                    {
        //                                        TO = EmailId,
        //                                        Subject = "Transaction Successfull",
        //                                        Body = body
        //                                    };
        //                                    _sendEmails.SendEmail(req);
        //                                }
        //                                catch
        //                                {

        //                                }
        //                            }
        //                            else
        //                            {
        //                                _Transaction.TransactionStatus = (int)TransactionStatus.Failed;
        //                            }
        //                            _Transaction.WalletServiceId = WalletServiceId;
        //                            _Transaction.SenderId = UserCurrentDetail.WalletUserId;
        //                            _Transaction.ReceiverId = UserCurrentDetail.WalletUserId;
        //                            _Transaction.BankBranchCode = string.Empty;
        //                            _Transaction.BankTransactionId = request.id;
        //                            _Transaction.TransactionId = getInitialTransaction.InvoiceNumber;


        //                            _Transaction.IsAdminTransaction = false;
        //                            _Transaction.IsActive = true;
        //                            _Transaction.IsDeleted = false;
        //                            _Transaction.CreatedDate = TDate;
        //                            _Transaction.UpdatedDate = TDate;
        //                            _Transaction.TransactionTypeInfo = (int)TransactionTypeInfo.AddedByCard;
        //                            _Transaction.IsAddDuringPay = false;
        //                            _Transaction.VoucherCode = string.Empty;

        //                            await _cardPaymentRepository.SaveWalletTransactions(_Transaction);
        //                            //db.WalletTransactions.Add(_Transaction);
        //                            //db.SaveChanges();
        //                            #endregion

        //                            #region Credit
        //                            var _credit = new WalletTransactionDetail();
        //                            _credit.Amount = Convert.ToString(_commission.TransactionAmount);
        //                            _credit.TransactionType = (int)TransactionDetailType.Credit;
        //                            _credit.WalletUserId = adminUser.WalletUserId;
        //                            _credit.WalletTransactionId = _Transaction.WalletTransactionId;
        //                            _credit.IsActive = true;
        //                            _credit.IsDeleted = false;
        //                            _credit.CreatedDate = TDate;
        //                            _credit.UpdatedDate = TDate;
        //                            //db.WalletTransactionDetails.Add(_credit);
        //                            //db.SaveChanges();
        //                            await _cardPaymentRepository.SaveWalletTransactionDetails(_credit);
        //                            #endregion

        //                            #region Debit
        //                            var _debit = new WalletTransactionDetail();
        //                            _debit.Amount = Convert.ToString(_commission.TransactionAmount);
        //                            _debit.TransactionType = (int)TransactionDetailType.Debit;
        //                            _debit.WalletUserId = UserCurrentDetail.WalletUserId;
        //                            _debit.WalletTransactionId = _Transaction.WalletTransactionId;
        //                            _debit.IsActive = true;
        //                            _debit.IsDeleted = false;
        //                            _debit.CreatedDate = TDate;
        //                            _debit.UpdatedDate = TDate;
        //                            //db.WalletTransactionDetails.Add(_credit);
        //                            //db.SaveChanges();
        //                            await _cardPaymentRepository.SaveWalletTransactionDetails(_credit);
        //                            #endregion

        //                            //get UpdateNewCardNoResponseBankCode id
        //                            await _cardPaymentRepository.UpdateNewCardNoResponseBankCode(getInitialTransaction.InvoiceNumber, UserCurrentDetail.WalletUserId, request.id);


        //                            var adminKeyPair = AES256.AdminKeyPair;


        //                            //db.SaveChanges();
        //                            //tran.Commit();
        //                            #region PushNotification

        //                            var CurrentUser = await _walletUserRepository.GetCurrentUser(UserCurrentDetail.WalletUserId);//db.WalletUsers.Where(x => x.WalletUserId == receiver.WalletUserId).FirstOrDefault();
        //                            if (CurrentUser != null)
        //                            {
        //                                PushNotificationModel push = new PushNotificationModel();
        //                                push.SenderId = UserCurrentDetail.WalletUserId;
        //                                push.deviceType = (int)UserCurrentDetail.DeviceType;
        //                                push.deviceKey = UserCurrentDetail.DeviceToken;
        //                                PayMoneyPushModel pushModel = new PayMoneyPushModel();
        //                                pushModel.TransactionDate = TDate;
        //                                pushModel.TransactionId = request.id;
        //                                pushModel.CurrentBalance = CurrentUser.CurrentBalance;
        //                                pushModel.alert = _Transaction.WalletAmount + " XOF has been credited to your account.";
        //                                pushModel.Amount = getInitialTransaction.RequestedAmount;
        //                                pushModel.CurrentBalance = CurrentUser.CurrentBalance;
        //                                pushModel.pushType = (int)PushType.ADDMONEY;

        //                                if ((int)UserCurrentDetail.DeviceType == (int)DeviceTypes.ANDROID || (int)UserCurrentDetail.DeviceType == (int)DeviceTypes.Web)
        //                                {
        //                                    PushPayload<PayMoneyPushModel> aps = new PushPayload<PayMoneyPushModel>();
        //                                    PushPayloadData<PayMoneyPushModel> _data = new PushPayloadData<PayMoneyPushModel>();
        //                                    _data.notification = pushModel;
        //                                    aps.data = _data;
        //                                    aps.to = UserCurrentDetail.DeviceToken;
        //                                    aps.collapse_key = string.Empty;
        //                                    push.message = JsonConvert.SerializeObject(aps);
        //                                    push.payload = pushModel;
        //                                }
        //                                if ((int)UserCurrentDetail.DeviceType == (int)DeviceTypes.IOS)
        //                                {
        //                                    NotificationJsonResponse<PayMoneyIOSPushModel> aps = new NotificationJsonResponse<PayMoneyIOSPushModel>();
        //                                    PayMoneyIOSPushModel _iosPushModel = new PayMoneyIOSPushModel();
        //                                    _iosPushModel.alert = pushModel.alert;
        //                                    _iosPushModel.Amount = pushModel.Amount;
        //                                    _iosPushModel.CurrentBalance = pushModel.CurrentBalance;
        //                                    _iosPushModel.MobileNo = pushModel.MobileNo;
        //                                    _iosPushModel.SenderName = pushModel.SenderName;
        //                                    _iosPushModel.pushType = pushModel.pushType;
        //                                    aps.aps = _iosPushModel;

        //                                    push.message = JsonConvert.SerializeObject(aps);
        //                                }
        //                                //if (!string.IsNullOrEmpty(push.message))
        //                                //{
        //                                //    new PushNotificationRepository().sendPushNotification(push);
        //                                //}
        //                            }
        //                            #endregion

        //                            if (UserCurrentDetail.MobileNo != null && UserCurrentDetail.MobileNo != "")
        //                            {
        //                                response.ToMobileNo = AES256.Decrypt(adminKeyPair.PrivateKey, UserCurrentDetail.MobileNo);
        //                            }


        //                            response.CurrentBalance = UserCurrentDetail.CurrentBalance;
        //                            response.RstKey = 1;

        //                            ///
        //                            await _masterDataService.Chargeback(UserCurrentDetail.WalletUserId);
        //                        }
        //                        else
        //                        {
        //                            //test

        //                        }
        //                    }
        //                    else
        //                    {
        //                        //test

        //                    }
        //                    //sdfsdfd
        //                }
        //                else
        //                {

        //                    response.RstKey = 2;
        //                }
        //            }
        //            else
        //            {
        //                await _cardPaymentRepository.UpdateTransactionInitiateRequest(getInitialTransaction);
        //                response.RstKey = 3;

        //                //response.TransactionResponseDescription = _req.vpc_ResponseCodeDescription;
        //                //response.TransactionResponseCode = request.vpc_TxnResponseCode;
        //            }



        //        }
        //        else
        //        {
        //            response.RstKey = 3;
        //        }

        //    }
        //    catch (Exception ex)
        //    {

        //        // ex.Message.ErrorLog("CardPaymentService", "SaveflutterCardPaymentResponse", request);
        //        "EziWebHookController".ErrorLog("CardPaymentService", "SaveflutterCardPaymentResponsewebhook", ex.StackTrace + " " + ex.Message);
        //    }
        //    return response;
        //}

        public async Task<binancePaymentUrlResponse> GetCardPaymentUrlForbinance(ThirdpartyPaymentByCardRequest request, string headerToken)
        {
            var response = new binancePaymentUrlResponse();
            var _commission = new CalculateCommissionResponse();
            var _commissionRequest = new CalculateCommissionRequest();

            var transationInitiate = new TransactionInitiateRequest();
            var _thirdPartyPaymentByCard = new ThirdPartyPaymentByCard();

            try
            {

                var UserDetail = await _walletUserService.UserProfile(headerToken);

                var Isdocverified = await _walletUserRepository.IsDocVerifiedMOMO(UserDetail.DocumetStatus);

                var resultTL = await _transactionLimitAUService.CheckTransactionLimitAU(request.WalletUserId.ToString()); //check New TL
                var transactionLimit = await _masterDataRepository.GetTransactionLimitAddMoney(Convert.ToString(UserDetail.WalletUserId));
                int limit = transactionLimit != null ? Convert.ToInt32(transactionLimit.TransactionLimitForAddMoney) : 0;

                var transactionHistory = _masterDataRepository.GetAllTransactionsAddMoney(UserDetail.WalletUserId);
                int totalAmountTransfered = transactionHistory != null ? Convert.ToInt32(transactionHistory.totalAmount) : 0;

                #region Calculate commission on request amount               

                int WalletServiceId = await _cardPaymentRepository.GetServiceId();
                if (UserDetail.IsActive == true)//am
                {
                    if (UserDetail.IsEmailVerified == true)
                    {
                        if (Isdocverified == true)
                        {
                            if (transactionLimit == null || transactionLimit.TransactionLimitForAddMoney == null || limit >= (Convert.ToDecimal(request.Amount) + totalAmountTransfered))
                            {
                                if (WalletServiceId > 0)
                                {
                                    #region Calculate Commission on request amount
                                    _commissionRequest.IsRoundOff = true;
                                    _commissionRequest.TransactionAmount = Convert.ToDecimal(request.Amount);
                                    _commissionRequest.WalletServiceId = WalletServiceId;
                                    _commission = await _setCommisionRepository.CalculateCommission(_commissionRequest);
                                    #endregion
                                }


                                decimal amountWithCommision = decimal.Parse(string.Format("{0:0,0}", _commission.AmountWithCommission));    // "1,234,257";

                                #endregion
                                if (resultTL != null)
                                {
                                    decimal SetAmount = Convert.ToDecimal(resultTL.SetAmount);// returns decimal
                                    decimal TotalAmount = Convert.ToDecimal(resultTL.TotalAmount);// returns decimal

                                    if (SetAmount != 0) //0 =msg 
                                    {
                                        decimal requestAmountwithcomm = amountWithCommision + TotalAmount;

                                        if (requestAmountwithcomm <= SetAmount)//1000 >= 1000
                                        {

                                        }
                                        else
                                        {
                                            response.RstKey = 6;
                                            response.Message = ResponseMessages.INVALID_txnAmountREQUEST;
                                            return response;
                                        }
                                    }
                                }



                                var invoiceNumber = await _masterDataRepository.GetInvoiceNumber();


                                List<transferDetailList> _Requestcustomer = new List<transferDetailList>
                                    {
                                        new transferDetailList { merchantSendId = invoiceNumber.InvoiceNumber, transferAmount = amountWithCommision, receiveType = "BINANCE_ID",transferMethod="SPOT_WALLET",receiver= "186105501"}

                                    };




                                var _RequestAttributes = new binanceaddRequest();
                                _RequestAttributes.currency = "BUSD";
                                _RequestAttributes.requestId = invoiceNumber.InvoiceNumber;
                                _RequestAttributes.batchName = "sample batch";
                                _RequestAttributes.totalAmount = amountWithCommision;
                                _RequestAttributes.totalNumber = 1;
                                _RequestAttributes.bizScene = "MERCHANT_PAYMENT";
                                _RequestAttributes.transferDetailList = _Requestcustomer;

                                var req = JsonConvert.SerializeObject(_RequestAttributes);

                                JavaScriptSerializer js = new JavaScriptSerializer();

                                //here to get psaymenturl



                                var responseData2 = await GethashorUrl(req, null, "binance");
                                var _responseModel2 = JsonConvert.DeserializeObject<binancePaymentUrlResponse>(responseData2);
                                if (_responseModel2.code != null && _responseModel2.status == "success")
                                {
                                    transationInitiate.InvoiceNumber = invoiceNumber.InvoiceNumber;
                                    transationInitiate.ReceiverNumber = UserDetail.MobileNo;
                                    transationInitiate.ServiceName = "binance Card Payment";
                                    transationInitiate.RequestedAmount = _commission.TransactionAmount.ToString();
                                    transationInitiate.TransactionStatus = (int)TransactionStatus.Pending;
                                    transationInitiate.WalletUserId = UserDetail.WalletUserId;
                                    transationInitiate.UserReferanceNumber = invoiceNumber.AutoDigit;
                                    transationInitiate.CurrentBalance = UserDetail.CurrentBalance;
                                    transationInitiate.AfterTransactionBalance = "";
                                    transationInitiate.ReceiverCurrentBalance = UserDetail.CurrentBalance;
                                    transationInitiate.UserName = UserDetail.FirstName + " " + UserDetail.LastName;
                                    transationInitiate.ReceiverWalletUserId = UserDetail.WalletUserId;
                                    transationInitiate.CreatedDate = DateTime.UtcNow;
                                    transationInitiate.UpdatedDate = DateTime.UtcNow;
                                    transationInitiate.IsActive = true;
                                    transationInitiate.IsDeleted = false;
                                    transationInitiate.JsonRequest = responseData2;
                                    transationInitiate.JsonResponse = "";
                                    transationInitiate = await _cardPaymentRepository.SaveTransactionInitiateRequest(transationInitiate);
                                    response.URL = _responseModel2.data.status;
                                    response.RstKey = 2;
                                }

                                else
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.Message = "Please try after some time aggregator error.";
                                }
                                LogTransactionTypes.Request.SaveTransactionLog(LogTransactionNameTypes.AddMoney + "Credit TO Debit Cards", response.data.status, "Request Url : " + response.data.status);
                            }
                            else
                            {
                                var addLimit = limit - (Convert.ToDecimal(request.Amount) + totalAmountTransfered);
                                if (addLimit < Convert.ToDecimal(request.Amount))
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.Message = "Exceed your transaction limit.";
                                }
                                else
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.Message = "You can not add more then " + transactionLimit.TransactionLimitForAddMoney.ToString() + " cedi in a day";
                                }
                            }
                        }
                        else if (UserDetail.DocumetStatus == 0 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
                        {
                            response.RstKey = 13;
                            response.Message = ResponseMessageKyc.FAILED_Doc_NotUploaded;
                        }
                        else if (UserDetail.DocumetStatus == 1 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
                        {
                            response.RstKey = 14;
                            response.Message = ResponseMessageKyc.FAILED_Doc_Pending;
                        }
                        else if (UserDetail.DocumetStatus == 4 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
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
                        response.RstKey = 6;
                        response.StatusCode = (int)TransactionStatus.Failed;
                        response.Message = ResponseMessages.EMAIL_VERIFICATION_PENDING;
                    }
                }
                else
                {
                    response.RstKey = 6;
                    response.Message = ResponseMessages.TRANSACTION_DISABLED;
                }
            }

            catch (Exception ex)
            {

                //tran.Rollback();
            }
            return response;

        }


        public async Task<binancewalletResponse> GetCardPaymentUrlForbinancewallet(ThirdpartyPaymentByCardRequest request, string headerToken)
        {
            var response = new binancewalletResponse();
            var _commission = new CalculateCommissionResponse();
            var _commissionRequest = new CalculateCommissionRequest();

            var transationInitiate = new TransactionInitiateRequest();
            var _thirdPartyPaymentByCard = new ThirdPartyPaymentByCard();

            try
            {

                var UserDetail = await _walletUserService.UserProfile(headerToken);

                var Isdocverified = await _walletUserRepository.IsDocVerifiedMOMO(UserDetail.DocumetStatus);

                var resultTL = await _transactionLimitAUService.CheckTransactionLimitAU(request.WalletUserId.ToString()); //check New TL
                var transactionLimit = await _masterDataRepository.GetTransactionLimitAddMoney(Convert.ToString(UserDetail.WalletUserId));
                int limit = transactionLimit != null ? Convert.ToInt32(transactionLimit.TransactionLimitForAddMoney) : 0;

                var transactionHistory = _masterDataRepository.GetAllTransactionsAddMoney(UserDetail.WalletUserId);
                int totalAmountTransfered = transactionHistory != null ? Convert.ToInt32(transactionHistory.totalAmount) : 0;

                #region Calculate commission on request amount               

                int WalletServiceId = await _cardPaymentRepository.GetServiceId();
                if (UserDetail.IsActive == true)//am
                {
                    if (UserDetail.IsEmailVerified == true)
                    {
                        if (Isdocverified == true)
                        {
                            if (transactionLimit == null || transactionLimit.TransactionLimitForAddMoney == null || limit >= (Convert.ToDecimal(request.Amount) + totalAmountTransfered))
                            {
                                if (WalletServiceId > 0)
                                {
                                    #region Calculate Commission on request amount
                                    _commissionRequest.IsRoundOff = true;
                                    _commissionRequest.TransactionAmount = Convert.ToDecimal(request.Amount);
                                    _commissionRequest.WalletServiceId = WalletServiceId;
                                    _commission = await _setCommisionRepository.CalculateCommission(_commissionRequest);
                                    #endregion
                                }


                                decimal amountWithCommision = decimal.Parse(string.Format("{0:0,0}", _commission.AmountWithCommission));    // "1,234,257";

                                #endregion
                                if (resultTL != null)
                                {
                                    decimal SetAmount = Convert.ToDecimal(resultTL.SetAmount);// returns decimal
                                    decimal TotalAmount = Convert.ToDecimal(resultTL.TotalAmount);// returns decimal

                                    if (SetAmount != 0) //0 =msg 
                                    {
                                        decimal requestAmountwithcomm = amountWithCommision + TotalAmount;

                                        if (requestAmountwithcomm <= SetAmount)//1000 >= 1000
                                        {

                                        }
                                        else
                                        {
                                            response.RstKey = 6;
                                            response.Message = ResponseMessages.INVALID_txnAmountREQUEST;
                                            return response;
                                        }
                                    }
                                }



                                var invoiceNumber = await _masterDataRepository.GetInvoiceNumber();



                                var _RequestAttributes = new binancewalletRequest();
                                _RequestAttributes.currency = "BNB";
                                _RequestAttributes.requestId = invoiceNumber.InvoiceNumber;

                                _RequestAttributes.amount = Convert.ToString(amountWithCommision);

                                _RequestAttributes.transferType = "TO_MAIN";


                                var req = JsonConvert.SerializeObject(_RequestAttributes);

                                JavaScriptSerializer js = new JavaScriptSerializer();

                                //here to get psaymenturl



                                var responseData2 = await GethashorUrl(req, null, "binancewallet");
                                var _responseModel2 = JsonConvert.DeserializeObject<binancewalletResponse>(responseData2);
                                if (_responseModel2.code != null && _responseModel2.status == "success")
                                {
                                    transationInitiate.InvoiceNumber = invoiceNumber.InvoiceNumber;
                                    transationInitiate.ReceiverNumber = UserDetail.MobileNo;
                                    transationInitiate.ServiceName = "binancewallet Payment";
                                    transationInitiate.RequestedAmount = _commission.TransactionAmount.ToString();
                                    transationInitiate.TransactionStatus = (int)TransactionStatus.Pending;
                                    transationInitiate.WalletUserId = UserDetail.WalletUserId;
                                    transationInitiate.UserReferanceNumber = invoiceNumber.AutoDigit;
                                    transationInitiate.CurrentBalance = UserDetail.CurrentBalance;
                                    transationInitiate.AfterTransactionBalance = "";
                                    transationInitiate.ReceiverCurrentBalance = UserDetail.CurrentBalance;
                                    transationInitiate.UserName = UserDetail.FirstName + " " + UserDetail.LastName;
                                    transationInitiate.ReceiverWalletUserId = UserDetail.WalletUserId;
                                    transationInitiate.CreatedDate = DateTime.UtcNow;
                                    transationInitiate.UpdatedDate = DateTime.UtcNow;
                                    transationInitiate.IsActive = true;
                                    transationInitiate.IsDeleted = false;
                                    transationInitiate.JsonRequest = responseData2;
                                    transationInitiate.JsonResponse = "";
                                    transationInitiate = await _cardPaymentRepository.SaveTransactionInitiateRequest(transationInitiate);
                                    response.URL = _responseModel2.data.status;
                                    response.RstKey = 2;
                                }

                                else
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.Message = "Please try after some time aggregator error.";
                                }
                                LogTransactionTypes.Request.SaveTransactionLog(LogTransactionNameTypes.AddMoney + "Credit TO Debit Cards", response.data.status, "Request Url : " + response.data.status);
                            }
                            else
                            {
                                var addLimit = limit - (Convert.ToDecimal(request.Amount) + totalAmountTransfered);
                                if (addLimit < Convert.ToDecimal(request.Amount))
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.Message = "Exceed your transaction limit.";
                                }
                                else
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.Message = "You can not add more then " + transactionLimit.TransactionLimitForAddMoney.ToString() + " cedi in a day";
                                }
                            }
                        }
                        else if (UserDetail.DocumetStatus == 0 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
                        {
                            response.RstKey = 13;
                            response.Message = ResponseMessageKyc.FAILED_Doc_NotUploaded;
                        }
                        else if (UserDetail.DocumetStatus == 1 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
                        {
                            response.RstKey = 14;
                            response.Message = ResponseMessageKyc.FAILED_Doc_Pending;
                        }
                        else if (UserDetail.DocumetStatus == 4 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
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
                        response.RstKey = 6;
                        response.StatusCode = (int)TransactionStatus.Failed;
                        response.Message = ResponseMessages.EMAIL_VERIFICATION_PENDING;
                    }
                }
                else
                {
                    response.RstKey = 6;
                    response.Message = ResponseMessages.TRANSACTION_DISABLED;
                }
            }

            catch (Exception ex)
            {

                //tran.Rollback();
            }
            return response;

        }

        public async Task<FXKUDIPaymentUrlResponse> GetCardPaymentUrlForFXKUDI(ThirdpartyPaymentByCardRequest request, string headerToken)
        {
            var response = new FXKUDIPaymentUrlResponse();
            var _commission = new CalculateCommissionResponse();
            var _commissionRequest = new CalculateCommissionRequest();

            var transationInitiate = new TransactionInitiateRequest();
            var _thirdPartyPaymentByCard = new ThirdPartyPaymentByCard();

            try
            {

                var UserDetail = await _walletUserService.UserProfile(headerToken);

                var Isdocverified = await _walletUserRepository.IsDocVerifiedMOMO(UserDetail.DocumetStatus);

                var resultTL = await _transactionLimitAUService.CheckTransactionLimitAU(request.WalletUserId.ToString()); //check New TL
                var transactionLimit = await _masterDataRepository.GetTransactionLimitAddMoney(Convert.ToString(UserDetail.WalletUserId));
                int limit = transactionLimit != null ? Convert.ToInt32(transactionLimit.TransactionLimitForAddMoney) : 0;

                var transactionHistory = _masterDataRepository.GetAllTransactionsAddMoney(UserDetail.WalletUserId);
                int totalAmountTransfered = transactionHistory != null ? Convert.ToInt32(transactionHistory.totalAmount) : 0;


                //------Get Currency Rate--------------
                var currencyDetail = _masterDataRepository.GetCurrencyRate();

                //decimal cediRate = Convert.ToDecimal(currencyDetail.CediRate); 
                decimal NGNRate = Convert.ToDecimal(currencyDetail.NGNRate);//
                //decimal CfaRate = Convert.ToDecimal(currencyDetail.LERate);
                decimal requestAmount = Convert.ToDecimal(request.Amount);//;


                #region Calculate commission on request amount               

                //var WalletService = await _masterDataRepository.GetWalletServicesByIdOrChannel(request.channel, request.ServiceCategoryId);
                int WalletServiceId = await _cardPaymentRepository.GetServiceId();
                if (UserDetail.IsActive == true)//am
                {
                    if (UserDetail.IsEmailVerified == true)
                    {
                        if (Isdocverified == true)
                        {
                            if (transactionLimit == null || transactionLimit.TransactionLimitForAddMoney == null || limit >= (Convert.ToDecimal(request.Amount) + totalAmountTransfered))
                            {
                                if (WalletServiceId > 0)
                                {
                                    #region Calculate Commission on request amount
                                    _commissionRequest.IsRoundOff = true;
                                    _commissionRequest.TransactionAmount = Convert.ToDecimal(request.Amount);
                                    _commissionRequest.WalletServiceId = WalletServiceId;
                                    _commission = await _setCommisionRepository.CalculateCommission(_commissionRequest);
                                    #endregion
                                }


                                decimal amt = (_commission.AmountWithCommission * NGNRate); //xof to NGNRate
                                var finalAmt = Decimal.Parse(amt.ToString("0.00"));

                                #endregion
                                if (resultTL != null)
                                {
                                    decimal SetAmount = Convert.ToDecimal(resultTL.SetAmount);// returns decimal
                                    decimal TotalAmount = Convert.ToDecimal(resultTL.TotalAmount);// returns decimal

                                    if (SetAmount != 0) //0 =msg 
                                    {
                                        decimal requestAmountwithcomm = _commission.AmountWithCommission + TotalAmount;

                                        if (requestAmountwithcomm <= SetAmount)//1000 >= 1000
                                        {

                                        }
                                        else
                                        {
                                            response.RstKey = 6;
                                            response.Message = ResponseMessages.INVALID_txnAmountREQUEST;
                                            return response;
                                        }
                                    }
                                }


                                var invoiceNumber = await _masterDataRepository.GetInvoiceNumber();

                                var _RequestAttributes = new FXKUDIRequest();
                                _RequestAttributes.currency = "NGN";
                                _RequestAttributes.reference = invoiceNumber.InvoiceNumber;
                                _RequestAttributes.callback_url = CommonSetting.flutterCallBackUrl;
                                _RequestAttributes.amount = Convert.ToString(finalAmt);
                                _RequestAttributes.publickey = "FXKPUB087120068958";
                                _RequestAttributes.customer_name = UserDetail.FirstName + ' ' + UserDetail.LastName;
                                _RequestAttributes.customer_email = UserDetail.EmailId;
                                _RequestAttributes.customer_phone = UserDetail.MobileNo;
                                _RequestAttributes.merchantid = "51";
                                var req = JsonConvert.SerializeObject(_RequestAttributes);

                                JavaScriptSerializer js = new JavaScriptSerializer();
                                //here to get psaymenturl
                                var responseData2 = await GethashorUrl(req, null, "FXKUDI");
                                var _responseModel2 = JsonConvert.DeserializeObject<FXKUDIPaymentUrlResponse>(responseData2);



                                if (_responseModel2.status == "success")
                                {
                                    transationInitiate.InvoiceNumber = invoiceNumber.InvoiceNumber;
                                    transationInitiate.ReceiverNumber = UserDetail.MobileNo;
                                    transationInitiate.ServiceName = "FXKUDI Payment";
                                    transationInitiate.RequestedAmount = _commission.TransactionAmount.ToString();
                                    transationInitiate.TransactionStatus = (int)TransactionStatus.Pending;
                                    transationInitiate.WalletUserId = UserDetail.WalletUserId;
                                    transationInitiate.UserReferanceNumber = invoiceNumber.AutoDigit;
                                    transationInitiate.CurrentBalance = UserDetail.CurrentBalance;
                                    transationInitiate.AfterTransactionBalance = "";
                                    transationInitiate.ReceiverCurrentBalance = UserDetail.CurrentBalance;
                                    transationInitiate.UserName = UserDetail.FirstName + " " + UserDetail.LastName;
                                    transationInitiate.ReceiverWalletUserId = UserDetail.WalletUserId;
                                    transationInitiate.CreatedDate = DateTime.UtcNow;
                                    transationInitiate.UpdatedDate = DateTime.UtcNow;
                                    transationInitiate.IsActive = true;
                                    transationInitiate.IsDeleted = false;
                                    transationInitiate.JsonRequest = responseData2;
                                    transationInitiate.JsonResponse = "";
                                    transationInitiate = await _cardPaymentRepository.SaveTransactionInitiateRequest(transationInitiate);
                                    response.URL = _responseModel2.URL;
                                    response.RstKey = 2;
                                }

                                else
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.Message = "Please try after some time aggregator error.";
                                }
                                LogTransactionTypes.Request.SaveTransactionLog(LogTransactionNameTypes.AddMoney + "Credit TO Debit Cards", response.URL, "Request Url : " + response.URL);
                            }
                            else
                            {
                                var addLimit = limit - (Convert.ToDecimal(request.Amount) + totalAmountTransfered);
                                if (addLimit < Convert.ToDecimal(request.Amount))
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.Message = "Exceed your transaction limit.";
                                }
                                else
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.Message = "You can not add more then " + transactionLimit.TransactionLimitForAddMoney.ToString() + " cedi in a day";
                                }
                            }
                        }
                        else if (UserDetail.DocumetStatus == 0 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
                        {
                            response.RstKey = 13;
                            response.Message = ResponseMessageKyc.FAILED_Doc_NotUploaded;
                        }
                        else if (UserDetail.DocumetStatus == 1 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
                        {
                            response.RstKey = 14;
                            response.Message = ResponseMessageKyc.FAILED_Doc_Pending;
                        }
                        else if (UserDetail.DocumetStatus == 4 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
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
                        response.RstKey = 6;
                        response.StatusCode = (int)TransactionStatus.Failed;
                        response.Message = ResponseMessages.EMAIL_VERIFICATION_PENDING;
                    }
                }
                else
                {
                    response.RstKey = 6;
                    response.Message = ResponseMessages.TRANSACTION_DISABLED;
                }
            }

            catch (Exception ex)
            {

                //tran.Rollback();
            }
            return response;

        }

        public async Task<AddMoneyAggregatorResponse> SaveflutteraddmoneGlobalNigeriaBankTransferResponse(string txnreverifystatus, string invoiceno, string currency, string payment_type)
        {
            try
            {

                var req = new GlobalNigeriaBankTransferRequest
                {
                    status = txnreverifystatus,
                    tx_ref = invoiceno,
                    currency = currency,
                    payment_type = payment_type
                };

                var url = ConfigurationManager.AppSettings["RedirectPaymentUrladdmonebanktransfer"];
                var jsonReq = JsonConvert.SerializeObject(req);

                var payData = await Card(jsonReq, url);
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<AddMoneyAggregatorResponse> SaveflutterPayGlobalNigeriaBankTransferPaymentResponse(string txnreverifystatus, string invoiceno, string currency, string payment_type)
        {
            try
            {


                string str = invoiceno; //

                if (str.Substring(0, 3) == "TNT" && invoiceno != null)
                {
                    var req = new GlobalNigeriaBankTransferRequest
                    {
                        status = txnreverifystatus,
                        tx_ref = invoiceno,
                        currency = currency,
                        payment_type = payment_type
                    };

                    await SaveflutterPayGlobalNigeriaBankrohitResponse(req);
                }
                else if (invoiceno != null)
                {
                    var req = new GlobalNigeriaBankTransferRequestnene
                    {
                        Message = txnreverifystatus,
                        InvoiceNo = invoiceno,
                        StatusCode = "200"
                    };

                    await SaveflutterPayGlobalNigeriaBankNeneResponse(req);
                }



                //var url = ConfigurationManager.AppSettings["RedirectPaymentUrlpaybanktransfer"];
                //var jsonReq = JsonConvert.SerializeObject(req);
                //var payData = await Card(jsonReq, url);
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<AddMoneyAggregatorResponse> SaveflutterPayGlobalNigeriaBankrohitResponse(GlobalNigeriaBankTransferRequest NigeriaBankTransferRequest)
        {
            try
            {

                var url = ConfigurationManager.AppSettings["RedirectPaymentUrlpaybanktransferrohit"];
                var jsonReq = JsonConvert.SerializeObject(NigeriaBankTransferRequest);

                var payData = await Card(jsonReq, url);
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<AddMoneyAggregatorResponse> SaveflutterPayGlobalNigeriaBankNeneResponse(GlobalNigeriaBankTransferRequestnene NigeriaBankTransferRequest)
        {
            try
            {

                var url = ConfigurationManager.AppSettings["RedirectPaymentUrlpaybanktransferNene"];
                var jsonReq = JsonConvert.SerializeObject(NigeriaBankTransferRequest);

                var payData = await Card(jsonReq, url);
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /////option2 
        ///

        public async Task<SeerbitResponse> GetSeerbitCardPaymentUrl(ThirdpartyPaymentByCardRequest request, string headerToken)
        {
            var response = new SeerbitResponse();
            var _commission = new CalculateCommissionResponse();
            var _commissionRequest = new CalculateCommissionRequest();

            var transationInitiate = new TransactionInitiateRequest();
            var _thirdPartyPaymentByCard = new ThirdPartyPaymentByCard();

            try
            {

                var UserDetail = await _walletUserService.UserProfile(headerToken);

                var Isdocverified = await _walletUserRepository.IsDocVerifiedMOMO(UserDetail.DocumetStatus);

                var resultTL = await _transactionLimitAUService.CheckTransactionLimitAU(request.WalletUserId.ToString()); //check New TL
                var transactionLimit = await _masterDataRepository.GetTransactionLimitAddMoney(Convert.ToString(UserDetail.WalletUserId));
                int limit = transactionLimit != null ? Convert.ToInt32(transactionLimit.TransactionLimitForAddMoney) : 0;

                var transactionHistory = _masterDataRepository.GetAllTransactionsAddMoney(UserDetail.WalletUserId);
                int totalAmountTransfered = transactionHistory != null ? Convert.ToInt32(transactionHistory.totalAmount) : 0;


                #region Calculate commission on request amount               


                int WalletServiceId = await _cardPaymentRepository.GetServiceId();
                if (UserDetail.IsActive == true)//am
                {
                    if (UserDetail.IsEmailVerified == true)
                    {
                        if (Isdocverified == true)
                        {
                            if (transactionLimit == null || transactionLimit.TransactionLimitForAddMoney == null || limit >= (Convert.ToDecimal(request.Amount) + totalAmountTransfered))
                            {
                                if (WalletServiceId > 0)
                                {
                                    #region Calculate Commission on request amount
                                    _commissionRequest.IsRoundOff = true;
                                    _commissionRequest.TransactionAmount = Convert.ToDecimal(request.Amount);
                                    _commissionRequest.WalletServiceId = WalletServiceId;
                                    _commission = await _setCommisionRepository.CalculateCommission(_commissionRequest);
                                    #endregion
                                }


                                decimal amountWithCommision = decimal.Parse(string.Format("{0:0,0}", _commission.AmountWithCommission));    // "1,234,257";
                                decimal currentBalance = Convert.ToDecimal(UserDetail.CurrentBalance);
                                #endregion
                                if (resultTL != null)
                                {
                                    decimal SetAmount = Convert.ToDecimal(resultTL.SetAmount);// returns decimal
                                    decimal TotalAmount = Convert.ToDecimal(resultTL.TotalAmount);// returns decimal

                                    if (SetAmount != 0) //0 =msg 
                                    {
                                        decimal requestAmountwithcomm = amountWithCommision + TotalAmount;

                                        if (requestAmountwithcomm <= SetAmount)//1000 >= 1000
                                        {

                                        }
                                        else
                                        {
                                            response.RstKey = 6;
                                            response.message = ResponseMessages.INVALID_txnAmountREQUEST;
                                            return response;
                                        }
                                    }
                                }

                                //first we have call api encrypt/keys to get beasrfer token

                                var responseData = await GetSeerbitauthTokenbykey();
                                // { "status":"SUCCESS","data":{ "code":"00","EncryptedSecKey":{ "encryptedKey":"kZaA6N08aiZHnyGa9VTd4ueTkfMUoR7wMk5wjlBuA9vUHbRhseG5gC1Wc1ULWXzpIl0JmSvokIo36dPHISDwD1az6gh8myv2LseitduE3FxVEqzdQWgXdOj/3ZwdNsBO"},"message":"Successful"} }
                                var _responseModel = JsonConvert.DeserializeObject<GetSeerbitauthTokenbykeyResponse>(responseData);



                                if (_responseModel.status == "SUCCESS")
                                {
                                    ////here to get hash of request --
                                    ///
                                    var _Request = new SeerbitRequest1();
                                    var invoiceNumber = await _masterDataRepository.GetInvoiceNumber();
                                    _Request.publicKey = CommonSetting.Seerbitpublickey;
                                    _Request.amount = amountWithCommision.ToString();
                                    _Request.currency = "XOF";
                                    _Request.country = "CI";
                                    _Request.paymentReference = invoiceNumber.InvoiceNumber;
                                    _Request.email = UserDetail.EmailId;
                                    _Request.productId = "Seerbitoption";
                                    _Request.productDescription = "Seerbitoption";
                                    _Request.callbackUrl = CommonSetting.SeerbitCallBack;


                                    var req = JsonConvert.SerializeObject(_Request);

                                    JavaScriptSerializer js = new JavaScriptSerializer();

                                    //here to get hash
                                    var responseData1 = await GethashorUrl(req, _responseModel.data.EncryptedSecKey.encryptedKey, "hash");
                                    dynamic blogObject = js.Deserialize<dynamic>(responseData1);
                                    //object hash = blogObject["data"];
                                    //object hash1 = blogObject["data"]["hash"];
                                    object hash2 = blogObject["data"]["hash"]["hash"];

                                    if (hash2 != null)
                                    {
                                        //here to get psaymenturl

                                        _Request.hash = hash2.ToString();
                                        _Request.hashType = "sha256";

                                        var req1 = JsonConvert.SerializeObject(_Request);

                                        var responseData2 = await GethashorUrl(req1, _responseModel.data.EncryptedSecKey.encryptedKey, "Url");
                                        var _responseModel2 = JsonConvert.DeserializeObject<GetSeerbitauthTokenbykeyResponse>(responseData2);
                                        if (_responseModel2.status == "SUCCESS" && _responseModel2.data.payments.redirectLink != null)
                                        {
                                            transationInitiate.InvoiceNumber = invoiceNumber.InvoiceNumber;
                                            transationInitiate.ReceiverNumber = UserDetail.MobileNo;
                                            transationInitiate.ServiceName = "Seerbit Card Payment";
                                            transationInitiate.RequestedAmount = _commission.TransactionAmount.ToString();
                                            transationInitiate.TransactionStatus = (int)TransactionStatus.Pending;
                                            transationInitiate.WalletUserId = UserDetail.WalletUserId;
                                            transationInitiate.UserReferanceNumber = invoiceNumber.AutoDigit;
                                            transationInitiate.CurrentBalance = UserDetail.CurrentBalance;
                                            transationInitiate.AfterTransactionBalance = "";
                                            transationInitiate.ReceiverCurrentBalance = UserDetail.CurrentBalance;
                                            transationInitiate.UserName = UserDetail.FirstName + " " + UserDetail.LastName;
                                            transationInitiate.ReceiverWalletUserId = UserDetail.WalletUserId;
                                            transationInitiate.CreatedDate = DateTime.UtcNow;
                                            transationInitiate.UpdatedDate = DateTime.UtcNow;
                                            transationInitiate.IsActive = true;
                                            transationInitiate.IsDeleted = false;
                                            transationInitiate.JsonRequest = responseData2;
                                            transationInitiate.JsonResponse = "";
                                            transationInitiate = await _cardPaymentRepository.SaveTransactionInitiateRequest(transationInitiate);
                                            response.Url = _responseModel2.data.payments.redirectLink;
                                            response.RstKey = 2;
                                        }
                                        else
                                        {
                                            response.RstKey = 6;
                                            response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                            response.message = "Please try after some time aggregator error.";
                                        }
                                    }
                                    else
                                    {
                                        response.RstKey = 6;
                                        response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                        response.message = "Please try after some time aggregator error.";
                                    }

                                }
                                else
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.message = "Please try after some time aggregator error.";
                                }
                                //LogTransactionTypes.Request.SaveTransactionLog(LogTransactionNameTypes.AddMoney + "Credit TO Debit Cards", response.sessioanData, "Request Url : " + response.sessioanData);
                            }
                            else
                            {
                                var addLimit = limit - (Convert.ToDecimal(request.Amount) + totalAmountTransfered);
                                if (addLimit < Convert.ToDecimal(request.Amount))
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.message = "Exceed your transaction limit.";
                                }
                                else
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.message = "You can not add more then " + transactionLimit.TransactionLimitForAddMoney.ToString() + " cedi in a day";
                                }
                            }
                        }
                        else if (UserDetail.DocumetStatus == 0 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
                        {
                            response.RstKey = 13;
                            response.message = ResponseMessageKyc.FAILED_Doc_NotUploaded;
                        }
                        else if (UserDetail.DocumetStatus == 1 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
                        {
                            response.RstKey = 14;
                            response.message = ResponseMessageKyc.FAILED_Doc_Pending;
                        }
                        else if (UserDetail.DocumetStatus == 4 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
                        {
                            response.RstKey = 15;
                            response.message = ResponseMessageKyc.Doc_Not_visible;
                        }
                        else
                        {
                            response.RstKey = 16;
                            response.message = ResponseMessageKyc.Doc_Rejected;
                        }
                    }
                    else
                    {
                        response.RstKey = 6;
                        response.StatusCode = (int)TransactionStatus.Failed;
                        response.message = ResponseMessages.EMAIL_VERIFICATION_PENDING;
                    }
                }
                else
                {
                    response.RstKey = 6;
                    response.message = ResponseMessages.TRANSACTION_DISABLED;
                }
            }

            catch (Exception ex)
            {

                //tran.Rollback();
            }
            return response;

        }

        public async Task<string> GetSeerbitauthTokenbykey()
        {
            var _Request = new GetSeerbitauthTokenbykeyRequest();

            string Seerbitpublickey = CommonSetting.Seerbitpublickey;
            string Seerbitprivatekey = CommonSetting.Seerbitprivatekey;
            _Request.key = Seerbitprivatekey + "." + Seerbitpublickey;

            var jsonReq = JsonConvert.SerializeObject(_Request);


            string resBody = "";
            RootObject responseData = new RootObject();
            using (HttpClient client = new HttpClient())
            {
                // Call asynchronous network methods in a try/catch block to handle exceptions
                try
                {
                    var content = new StringContent(jsonReq, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(CommonSetting.SeerbitGettokenUrl, content);
                    response.EnsureSuccessStatusCode();
                    resBody = await response.Content.ReadAsStringAsync();

                }
                catch (HttpRequestException e)
                {
                    e.Message.ErrorLog("SeerbitGettokenUrl", e.StackTrace + " " + e.Message);

                }

                return resBody;

            }
        }

        public async Task<SeerbitResponse> SaveSeerbitPaymentResponse(SeerbitRequest request)
        {
            SeerbitResponse response = new SeerbitResponse();
            CalculateCommissionResponse _commission = new CalculateCommissionResponse();
            CalculateCommissionRequest _commissionRequest = new CalculateCommissionRequest();
            string RequestString = JsonConvert.SerializeObject(request);
            LogTransactionTypes.Response.SaveTransactionLog(LogTransactionNameTypes.AddMoney + "Credit to Debit Cards2", RequestString, "Responce by bank detail : ");

            try
            {

                var getInitialTransaction = await _cardPaymentRepository.GetTransactionInitiateRequest(request.reference);

                int GetWalletTransactionsexist = await _cardPaymentRepository.GetWalletTransactionsexist(getInitialTransaction.WalletUserId, getInitialTransaction.InvoiceNumber);

                if (request.reference != null && GetWalletTransactionsexist == 0 && request.message == "Successful")
                {
                    getInitialTransaction.JsonResponse = JsonConvert.SerializeObject(request);

                    response.InvoiceNo = getInitialTransaction.InvoiceNumber;
                    response.Amount = getInitialTransaction.RequestedAmount;
                    response.status = "Seerbit";
                    DateTime TDate = DateTime.UtcNow;
                    response.TransactionDate = TDate;

                    var WalletServiceId = await _cardPaymentRepository.GetWalletService("SeerbitCard", (int)WalletTransactionSubTypes.SeerbitCard);
                    if (WalletServiceId != null)
                    {
                        var adminUser = await _cardPaymentRepository.GetAdminUser();
                        if (adminUser != null)
                        {
                            // var receiver = await _walletUserRepository.GetCurrentUser(Convert.ToInt32(requestDetail.WalletUserId)); //db.WalletUsers.Where(x => x.WalletUserId == requestDetail.WalletUserId).FirstOrDefault();

                            long userId = Convert.ToInt32(getInitialTransaction.WalletUserId);
                            var UserCurrentDetail = await _walletUserRepository.GetCurrentUser(userId);
                            if (UserCurrentDetail != null && await _cardPaymentRepository.IsWalletTransactions(UserCurrentDetail.WalletUserId, getInitialTransaction.InvoiceNumber))
                            {
                                //this line commented due to currentbalance is not added to card expected 
                                //request.vpc_Amount = Convert.ToString(Math.Round((Convert.ToDecimal(requestDetail.Amount) / 100), 2));
                                getInitialTransaction.RequestedAmount = Convert.ToString(Math.Round(Convert.ToDecimal(getInitialTransaction.RequestedAmount), 2));

                                // to update wallet amount-----

                                // db.WalletUsers.FirstOrDefault(x => x.WalletUserId == receiver.WalletUserId);

                                if (UserCurrentDetail != null)
                                {
                                    _commissionRequest.CurrentBalance = Convert.ToDecimal(UserCurrentDetail.CurrentBalance);
                                    _commissionRequest.IsRoundOff = true;

                                    _commissionRequest.TransactionAmount = Convert.ToDecimal(getInitialTransaction.RequestedAmount); //change
                                    _commissionRequest.WalletServiceId = WalletServiceId.WalletServiceId;
                                    _commission = await _setCommisionRepository.CalculateCommission(_commissionRequest);

                                    if (!string.IsNullOrEmpty(request.reference))
                                    {
                                        getInitialTransaction.TransactionStatus = (int)TransactionStatus.Completed;
                                        if (Convert.ToDecimal(UserCurrentDetail.CurrentBalance) >= 0)
                                        {
                                            if (Convert.ToDecimal(UserCurrentDetail.CurrentBalance) == 0)
                                            {
                                                UserCurrentDetail.CurrentBalance = _commission.TransactionAmount.ToString();
                                                getInitialTransaction.AfterTransactionBalance = UserCurrentDetail.CurrentBalance.ToString();
                                                getInitialTransaction.ReceiverCurrentBalance = UserCurrentDetail.CurrentBalance.ToString();
                                            }
                                            else
                                            {
                                                UserCurrentDetail.CurrentBalance = Math.Round(Convert.ToDecimal(UserCurrentDetail.CurrentBalance) + _commission.TransactionAmount, 2).ToString();
                                                getInitialTransaction.AfterTransactionBalance = UserCurrentDetail.CurrentBalance.ToString();
                                                getInitialTransaction.ReceiverCurrentBalance = UserCurrentDetail.CurrentBalance.ToString();
                                            }
                                        }
                                        else
                                        {
                                            UserCurrentDetail.CurrentBalance = Math.Round(_commission.TransactionAmount - Convert.ToDecimal(UserCurrentDetail.CurrentBalance), 2).ToString();
                                            getInitialTransaction.AfterTransactionBalance = UserCurrentDetail.CurrentBalance.ToString();
                                            getInitialTransaction.ReceiverCurrentBalance = UserCurrentDetail.CurrentBalance.ToString();
                                        }
                                    }
                                    await _walletUserRepository.UpdateUserDetail(UserCurrentDetail);
                                    await _cardPaymentRepository.UpdateTransactionInitiateRequest(getInitialTransaction);
                                    // db.SaveChanges();
                                }

                                #region Save Transaction
                                decimal amountWithCommision = decimal.Parse(string.Format("{0:0,0}", _commission.AmountWithCommission));    // "1,234,257";

                                var _Transaction = new WalletTransaction();

                                _Transaction.TransactionInitiateRequestId = getInitialTransaction.Id;
                                _Transaction.MerchantCommissionAmount = Convert.ToString(_commission.MerchantCommissionAmount);
                                _Transaction.MerchantCommissionId = _commission.MerchantCommissionId;
                                _Transaction.TransactionType = AggragatorServiceType.CREDIT;
                                _Transaction.IsBankTransaction = false;
                                _Transaction.TransactionTypeInfo = (int)TransactionTypeInfo.AddedByCard;
                                _Transaction.IsBankTransaction = false;
                                _Transaction.Comments = string.Empty;
                                _Transaction.InvoiceNo = getInitialTransaction.InvoiceNumber;
                                _Transaction.CommisionAmount = Convert.ToString(_commission.CommissionAmount);
                                _Transaction.CommisionId = _commission.CommissionId;
                                _Transaction.WalletAmount = Convert.ToString(_commission.TransactionAmount);

                                _Transaction.TotalAmount = Convert.ToString(amountWithCommision);
                                _Transaction.ServiceTaxRate = _commission.ServiceTaxRate;
                                _Transaction.ServiceTax = Convert.ToString(_commission.ServiceTaxAmount);
                                _Transaction.OperatorType = "Seerbit";

                                _Transaction.AccountNo = getInitialTransaction.ReceiverNumber;
                                if (!string.IsNullOrEmpty(request.reference))
                                {

                                    _Transaction.TransactionStatus = (int)TransactionStatus.Completed;
                                    try
                                    {
                                        //--------send mail on success transaction--------

                                        var AdminKeys = AES256.AdminKeyPair;
                                        string FirstName = AES256.Decrypt(UserCurrentDetail.PrivateKey, UserCurrentDetail.FirstName);
                                        string LastName = AES256.Decrypt(UserCurrentDetail.PrivateKey, UserCurrentDetail.LastName);
                                        string StdCode = UserCurrentDetail.StdCode;
                                        string MobileNo = AES256.Decrypt(AdminKeys.PrivateKey, UserCurrentDetail.MobileNo);
                                        string EmailId = AES256.Decrypt(AdminKeys.PrivateKey, UserCurrentDetail.EmailId).Trim().ToLower();

                                        string filename = CommonSetting.successfullTransaction;


                                        var body = _sendEmails.ReadEmailformats(filename);
                                        body = body.Replace("$$FirstName$$", FirstName + " " + LastName);
                                        body = body.Replace("$$DisplayContent$$", "VISA CARDS/MASTER CARD");
                                        body = body.Replace("$$customer$$", MobileNo);
                                        body = body.Replace("$$amount$$", "XOF " + getInitialTransaction.RequestedAmount);
                                        body = body.Replace("$$ServiceTaxAmount$$", "XOF " + _commission.CommissionAmount);
                                        body = body.Replace("$$AmountWithCommission$$", "XOF " + amountWithCommision);
                                        body = body.Replace("$$TransactionId$$", request.reference);

                                        var req = new EmailModel()
                                        {
                                            TO = EmailId,
                                            Subject = "Transaction Successfull",
                                            Body = body
                                        };
                                        _sendEmails.SendEmail(req);
                                    }
                                    catch
                                    {

                                    }
                                }
                                else
                                {
                                    _Transaction.TransactionStatus = (int)TransactionStatus.Failed;
                                }
                                _Transaction.WalletServiceId = WalletServiceId.WalletServiceId;
                                _Transaction.SenderId = UserCurrentDetail.WalletUserId;
                                _Transaction.ReceiverId = UserCurrentDetail.WalletUserId;
                                _Transaction.BankBranchCode = string.Empty;
                                _Transaction.BankTransactionId = request.linkingreference;
                                _Transaction.TransactionId = request.reference;


                                _Transaction.IsAdminTransaction = false;
                                _Transaction.IsActive = true;
                                _Transaction.IsDeleted = false;
                                _Transaction.CreatedDate = TDate;
                                _Transaction.UpdatedDate = TDate;
                                _Transaction.TransactionTypeInfo = (int)TransactionTypeInfo.AddedByCard;
                                _Transaction.IsAddDuringPay = false;
                                _Transaction.VoucherCode = string.Empty;

                                await _cardPaymentRepository.SaveWalletTransactions(_Transaction);
                                //db.WalletTransactions.Add(_Transaction);
                                //db.SaveChanges();
                                #endregion

                                #region Credit
                                var _credit = new WalletTransactionDetail();
                                _credit.Amount = Convert.ToString(_commission.TransactionAmount);
                                _credit.TransactionType = (int)TransactionDetailType.Credit;
                                _credit.WalletUserId = adminUser.WalletUserId;
                                _credit.WalletTransactionId = _Transaction.WalletTransactionId;
                                _credit.IsActive = true;
                                _credit.IsDeleted = false;
                                _credit.CreatedDate = TDate;
                                _credit.UpdatedDate = TDate;
                                //db.WalletTransactionDetails.Add(_credit);
                                //db.SaveChanges();
                                await _cardPaymentRepository.SaveWalletTransactionDetails(_credit);
                                #endregion

                                #region Debit
                                var _debit = new WalletTransactionDetail();
                                _debit.Amount = Convert.ToString(_commission.TransactionAmount);
                                _debit.TransactionType = (int)TransactionDetailType.Debit;
                                _debit.WalletUserId = UserCurrentDetail.WalletUserId;
                                _debit.WalletTransactionId = _Transaction.WalletTransactionId;
                                _debit.IsActive = true;
                                _debit.IsDeleted = false;
                                _debit.CreatedDate = TDate;
                                _debit.UpdatedDate = TDate;
                                //db.WalletTransactionDetails.Add(_credit);
                                //db.SaveChanges();
                                await _cardPaymentRepository.SaveWalletTransactionDetails(_credit);
                                #endregion

                                //get UpdateNewCardNoResponseBankCode id
                                await _cardPaymentRepository.UpdateNewCardNoResponseBankCode(getInitialTransaction.InvoiceNumber, UserCurrentDetail.WalletUserId, request.linkingreference);


                                var adminKeyPair = AES256.AdminKeyPair;


                                //db.SaveChanges();
                                //tran.Commit();
                                #region PushNotification

                                var CurrentUser = await _walletUserRepository.GetCurrentUser(UserCurrentDetail.WalletUserId);//db.WalletUsers.Where(x => x.WalletUserId == receiver.WalletUserId).FirstOrDefault();
                                if (CurrentUser != null)
                                {
                                    PushNotificationModel push = new PushNotificationModel();
                                    push.SenderId = UserCurrentDetail.WalletUserId;
                                    push.deviceType = (int)UserCurrentDetail.DeviceType;
                                    push.deviceKey = UserCurrentDetail.DeviceToken;
                                    PayMoneyPushModel pushModel = new PayMoneyPushModel();
                                    pushModel.TransactionDate = TDate;
                                    pushModel.TransactionId = request.reference;
                                    pushModel.CurrentBalance = CurrentUser.CurrentBalance;
                                    pushModel.alert = _Transaction.WalletAmount + " XOF has been credited to your account.";
                                    pushModel.Amount = getInitialTransaction.RequestedAmount;
                                    pushModel.CurrentBalance = CurrentUser.CurrentBalance;
                                    pushModel.pushType = (int)PushType.ADDMONEY;

                                    if ((int)UserCurrentDetail.DeviceType == (int)DeviceTypes.ANDROID || (int)UserCurrentDetail.DeviceType == (int)DeviceTypes.Web)
                                    {
                                        PushPayload<PayMoneyPushModel> aps = new PushPayload<PayMoneyPushModel>();
                                        PushPayloadData<PayMoneyPushModel> _data = new PushPayloadData<PayMoneyPushModel>();
                                        _data.notification = pushModel;
                                        aps.data = _data;
                                        aps.to = UserCurrentDetail.DeviceToken;
                                        aps.collapse_key = string.Empty;
                                        push.message = JsonConvert.SerializeObject(aps);
                                        push.payload = pushModel;
                                    }
                                    if ((int)UserCurrentDetail.DeviceType == (int)DeviceTypes.IOS)
                                    {
                                        NotificationJsonResponse<PayMoneyIOSPushModel> aps = new NotificationJsonResponse<PayMoneyIOSPushModel>();
                                        PayMoneyIOSPushModel _iosPushModel = new PayMoneyIOSPushModel();
                                        _iosPushModel.alert = pushModel.alert;
                                        _iosPushModel.Amount = pushModel.Amount;
                                        _iosPushModel.CurrentBalance = pushModel.CurrentBalance;
                                        _iosPushModel.MobileNo = pushModel.MobileNo;
                                        _iosPushModel.SenderName = pushModel.SenderName;
                                        _iosPushModel.pushType = pushModel.pushType;
                                        aps.aps = _iosPushModel;

                                        push.message = JsonConvert.SerializeObject(aps);
                                    }
                                    //if (!string.IsNullOrEmpty(push.message))
                                    //{
                                    //    new PushNotificationRepository().sendPushNotification(push);
                                    //}
                                }
                                #endregion

                                if (UserCurrentDetail.MobileNo != null && UserCurrentDetail.MobileNo != "")
                                {
                                    response.ToMobileNo = AES256.Decrypt(adminKeyPair.PrivateKey, UserCurrentDetail.MobileNo);
                                }


                                response.CurrentBalance = UserCurrentDetail.CurrentBalance;
                                response.RstKey = 1;

                                ///
                                await _masterDataService.Chargeback(UserCurrentDetail.WalletUserId);
                            }
                            else
                            {
                                //test

                            }
                        }
                        else
                        {
                            //test

                        }
                        //sdfsdfd
                    }
                    else
                    {

                        response.RstKey = 2;
                    }
                }
                else
                {
                    await _cardPaymentRepository.UpdateTransactionInitiateRequest(getInitialTransaction);
                    response.RstKey = 3;

                    //response.TransactionResponseDescription = _req.vpc_ResponseCodeDescription;
                    //response.TransactionResponseCode = request.vpc_TxnResponseCode;
                }




            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("CardPaymentRepository", "SaveMasterCardPaymentResponse", request);
            }
            return response;
        }

        public async Task<MasterCardPaymentUBAResponse> GetGTBCIVPaymentUrl(ThirdpartyPaymentByCardRequest request, string headerToken)
        {
            var response = new MasterCardPaymentUBAResponse();
            var _commission = new CalculateCommissionResponse();
            var _commissionRequest = new CalculateCommissionRequest();

            var transationInitiate = new TransactionInitiateRequest();
            var _thirdPartyPaymentByCard = new ThirdPartyPaymentByCard();

            try
            {

                var UserDetail = await _walletUserService.UserProfile(headerToken);

                var Isdocverified = await _walletUserRepository.IsDocVerifiedMOMO(UserDetail.DocumetStatus);

                var resultTL = await _transactionLimitAUService.CheckTransactionLimitAU(request.WalletUserId.ToString()); //check New TL
                var transactionLimit = await _masterDataRepository.GetTransactionLimitAddMoney(Convert.ToString(UserDetail.WalletUserId));
                int limit = transactionLimit != null ? Convert.ToInt32(transactionLimit.TransactionLimitForAddMoney) : 0;

                var transactionHistory = _masterDataRepository.GetAllTransactionsAddMoney(UserDetail.WalletUserId);
                int totalAmountTransfered = transactionHistory != null ? Convert.ToInt32(transactionHistory.totalAmount) : 0;
                var invoiceNumber = await _masterDataRepository.GetInvoiceNumber();

                #region Calculate commission on request amount               

                //var WalletService = await _masterDataRepository.GetWalletServicesByIdOrChannel(request.channel, request.ServiceCategoryId);
                int WalletServiceId = await _cardPaymentRepository.GetServiceId();
                if (UserDetail.IsActive == true)//am
                {
                    if (UserDetail.IsEmailVerified == true)
                    {
                        if (Isdocverified == true)
                        {
                            if (transactionLimit == null || transactionLimit.TransactionLimitForAddMoney == null || limit >= (Convert.ToDecimal(request.Amount) + totalAmountTransfered))
                            {
                                if (WalletServiceId > 0)
                                {
                                    #region Calculate Commission on request amount
                                    _commissionRequest.IsRoundOff = true;
                                    _commissionRequest.TransactionAmount = Convert.ToDecimal(request.Amount);
                                    _commissionRequest.WalletServiceId = WalletServiceId;
                                    _commission = await _setCommisionRepository.CalculateCommission(_commissionRequest);
                                    #endregion
                                }


                                decimal amountWithCommision = decimal.Parse(string.Format("{0:0,0}", _commission.AmountWithCommission));    // "1,234,257";

                                string PurchaseAmt = Convert.ToString(amountWithCommision);
                                PurchaseAmt = PurchaseAmt.PadLeft(12, '0');
                                // decimal amountWithCommision1 = decimal.Parse(string.Format("{0:0}", PurchaseAmt));    // "1,234,257";





                                decimal currentBalance = Convert.ToDecimal(UserDetail.CurrentBalance);
                                #endregion
                                if (resultTL != null)
                                {
                                    decimal SetAmount = Convert.ToDecimal(resultTL.SetAmount);// returns decimal
                                    decimal TotalAmount = Convert.ToDecimal(resultTL.TotalAmount);// returns decimal

                                    if (SetAmount != 0) //0 =msg 
                                    {
                                        decimal requestAmountwithcomm = amountWithCommision + TotalAmount;

                                        if (requestAmountwithcomm <= SetAmount)//1000 >= 1000
                                        {

                                        }
                                        else
                                        {
                                            response.RstKey = 6;
                                            response.Message = ResponseMessages.INVALID_txnAmountREQUEST;
                                            return response;
                                        }
                                    }
                                }
                                //8163300091091 281633 12329848634712 000000002050 952 {12329848634712}

                                var requ = new GTBCIVRequestHashRequest
                                {
                                    merchantID = "8163300091091",
                                    acquirerID = "281633",
                                    orderID = invoiceNumber.InvoiceNumber,
                                    formattedPurchaseAmt = PurchaseAmt,
                                    currency = "952"

                                };
                                //XmlDocument SOAPResponseBody = new XmlDocument();
                                var customersignaturevalue = ToSha512(requ);
                                // var jsonReq = JsonConvert.SerializeObject(requ);
                                TransactionInfoResponse responseString = CreateSoapEnvelope(invoiceNumber.InvoiceNumber, customersignaturevalue, PurchaseAmt);
                                var jsonReq = JsonConvert.SerializeObject(responseString);
                                //save data for initiate transaction 
                                transationInitiate.InvoiceNumber = invoiceNumber.InvoiceNumber;
                                transationInitiate.ReceiverNumber = UserDetail.MobileNo;
                                transationInitiate.ServiceName = "GTB Card Payment";
                                transationInitiate.RequestedAmount = _commission.TransactionAmount.ToString();
                                transationInitiate.TransactionStatus = (int)TransactionStatus.Pending;
                                transationInitiate.WalletUserId = UserDetail.WalletUserId;
                                transationInitiate.UserReferanceNumber = invoiceNumber.AutoDigit;
                                transationInitiate.CurrentBalance = UserDetail.CurrentBalance;
                                transationInitiate.AfterTransactionBalance = "";
                                transationInitiate.ReceiverCurrentBalance = UserDetail.CurrentBalance;
                                transationInitiate.UserName = UserDetail.FirstName + " " + UserDetail.LastName;
                                transationInitiate.ReceiverWalletUserId = UserDetail.WalletUserId;
                                transationInitiate.CreatedDate = DateTime.UtcNow;
                                transationInitiate.UpdatedDate = DateTime.UtcNow;
                                transationInitiate.IsActive = true;
                                transationInitiate.IsDeleted = false;
                                transationInitiate.JsonRequest = "GTB Request " + jsonReq;
                                transationInitiate.JsonResponse = "";
                                transationInitiate = await _cardPaymentRepository.SaveTransactionInitiateRequest(transationInitiate);


                                if (responseString.urlPayment != null && responseString.reasonCodeDesc == "Approved")
                                {

                                    response.URL = responseString.urlPayment;
                                    response.RstKey = 2;
                                }
                                else
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.Message = "Please try after some time aggregator error.";
                                }
                                LogTransactionTypes.Request.SaveTransactionLog(LogTransactionNameTypes.AddMoney + "Credit TO Debit Cards", response.sessioanData, "Request Url : " + response.sessioanData);
                            }
                            else
                            {
                                var addLimit = limit - (Convert.ToDecimal(request.Amount) + totalAmountTransfered);
                                if (addLimit < Convert.ToDecimal(request.Amount))
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.Message = "Exceed your transaction limit.";
                                }
                                else
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.Message = "You can not add more then " + transactionLimit.TransactionLimitForAddMoney.ToString() + " cedi in a day";
                                }
                            }
                        }
                        else if (UserDetail.DocumetStatus == 0 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
                        {
                            response.RstKey = 13;
                            response.Message = ResponseMessageKyc.FAILED_Doc_NotUploaded;
                        }
                        else if (UserDetail.DocumetStatus == 1 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
                        {
                            response.RstKey = 14;
                            response.Message = ResponseMessageKyc.FAILED_Doc_Pending;
                        }
                        else if (UserDetail.DocumetStatus == 4 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
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
                        response.RstKey = 6;
                        response.StatusCode = (int)TransactionStatus.Failed;
                        response.Message = ResponseMessages.EMAIL_VERIFICATION_PENDING;
                    }
                }
                else
                {
                    response.RstKey = 6;
                    response.Message = ResponseMessages.TRANSACTION_DISABLED;
                }
            }

            catch (Exception ex)
            {

                //tran.Rollback();
            }
            return response;

        }

        public static string ToSha512(GTBCIVRequestHashRequest request)
        {
            string fullrequest = request.merchantID + request.acquirerID + request.orderID + request.formattedPurchaseAmt + request.currency;
            string input = fullrequest + "{" + request.orderID + "}";
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            using (SHA512 shaM = new SHA512Managed())
            {
                return Convert.ToBase64String(shaM.ComputeHash(Encoding.UTF8.GetBytes(input)));
            }
        }

        public TransactionInfoResponse CreateSoapEnvelope(string InvoiceNumber, string customersignaturevalue, string PurchaseAmt)
        {
            string WsLogin = ConfigurationManager.AppSettings["WsLogin"]; ;
            string WsPassword = ConfigurationManager.AppSettings["WsPassword"];
            string AcceptorPointID = ConfigurationManager.AppSettings["AcceptorPointID"];
            string BankCode = ConfigurationManager.AppSettings["BankCode"];

            string urlCallBack = ConfigurationManager.AppSettings["urlCallBack"];
            string gimServiceUrl = ConfigurationManager.AppSettings["gimServiceUrl"];

            var client = new RestClient(gimServiceUrl); // prod GimServiceUrlProd
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "text/xml");
            request.AddHeader("SOAPAction", "http://www.hps.ma/PowerCARD/PaymentGateway/OnlineSecureServices/ProcessWebPayment");
            request.AddParameter("text/xml", $"<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\"" +
                $" xmlns:onl=\"http://www.hps.ma/PowerCARD/PaymentGateway/OnlineSecureServices\">" +
                $"<soapenv:Header><onl:AuthHeader><onl:WsLogin>{WsLogin}</onl:WsLogin>" +
                $"<onl:WsPassword>{WsPassword}</onl:WsPassword>" +
                $"<onl:AcceptorPointID>{AcceptorPointID}</onl:AcceptorPointID>" +
                $"<onl:BankCode>{BankCode}</onl:BankCode></onl:AuthHeader>" +
                $"</soapenv:Header><soapenv:Body><onl:TransactionInfo><onl:Version>1.0.0</onl:Version>" +
                $"<onl:OrderId>{InvoiceNumber}</onl:OrderId><onl:AuthorType>N</onl:AuthorType><onl:PurchaseAmount>{PurchaseAmt}</onl:PurchaseAmount>" +
                $"<onl:PurchaseCurrency>952</onl:PurchaseCurrency>" +
                $"<onl:CaptureFlag>A</onl:CaptureFlag><onl:Signature>{ customersignaturevalue }</onl:Signature><onl:SignatureMethod>SHA512</onl:SignatureMethod>" +
                $"<onl:MerchantResponseURL>{ urlCallBack }</onl:MerchantResponseURL></onl:TransactionInfo></soapenv:Body></soapenv:Envelope>", ParameterType.RequestBody);
            var response = client.Execute<TransactionInfoResponse>(request);

            return response.Data;



            //string mon = "02";
            //string yr = "24";
            //string card = "5129670105471952";
            //string cvv2 = "788";
            //string ServiceResult = "";
            //GTBCIVUrlPaymentResponse response = new GTBCIVUrlPaymentResponse();
            //HttpWebRequest request = CreateSOAPWebRequest();
            //XmlDocument SOAPReqBody = new XmlDocument();
            //SOAP Body Request    

            //            SOAPReqBody.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
            //<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:onl=""http://www.hps.ma/PowerCARD/PaymentGateway/OnlineSecureServices"">
            //   <soapenv:Header> 
            //       <onl:AuthHeader>  
            //           <onl:WsLogin>ezipay.ci@barakamoney.net</onl:WsLogin>       
            //           <onl:WsPassword>s3b5cr~YF</onl:WsPassword>            
            //           <onl:AcceptorPointID>8163300091091</onl:AcceptorPointID>                 
            //           <onl:BankCode>281633</onl:BankCode>
            //       </onl:AuthHeader>                       
            //       </soapenv:Header>
            //         <soapenv:Body>
            //         <onl:TransactionInfo>
            //            <onl:Version>1.0.0</onl:Version>           
            //            <onl:OrderId>461298485459999</onl:OrderId>            
            //            <onl:PurchaseAmount>" + PurchaseAmt + @"</onl:PurchaseAmount>
            //            <onl:PurchaseCurrency>952</onl:PurchaseCurrency>
            //            <onl:CaptureFlag>A</onl:CaptureFlag>
            //            <onl:Signature>" + customersignaturevalue + @"</onl:Signature>
            //            <onl:SignatureMethod>SHA512</onl:SignatureMethod>
            //         </onl:TransactionInfo>
            //         </soapenv:Body>
            //         </soapenv:Envelope>");

            // <onl:AuthorType>N</onl:AuthorType> < onl:TxtCardNo > " + card + @" </ onl:TxtCardNo >
            //                < onl:CardExpiryMonth > " + mon   + @" </ onl:CardExpiryMonth >
            //                     < onl:CardExpiryYear > " + yr + @" </ onl:CardExpiryYear >
            //                          < onl:CVV2 > " + cvv2 + @" </ onl:CVV2 >
            //using (Stream stream = request.GetRequestStream())
            //{
            //    SOAPReqBody.Save(stream);
            //}
            ////Geting response from request    
            //using (WebResponse Serviceres = request.GetResponse())
            //{
            //    using (StreamReader rd = new StreamReader(Serviceres.GetResponseStream()))
            //    {
            //        //reading stream    
            //         ServiceResult = rd.ReadToEnd();

            //    }
            //}

            //return ServiceResult;
        }


        //public HttpWebRequest CreateSOAPWebRequest()
        //{
        //    //Making Web Request    
        //    HttpWebRequest Req = (HttpWebRequest)WebRequest.Create(@"https://ecom.barakamoney.net/PgWebService/services/OnlineSecureService");
        //    //SOAPAction    
        //    Req.Headers.Add(@"SOAPAction:http://www.hps.ma/PowerCARD/PaymentGateway/OnlineSecureServices/ProcessWebPayment");
        //    //Content_type    
        //    Req.ContentType = "text/xml;charset=\"utf-8\"";
        //    Req.Accept = "text/xml";
        //    //HTTP method    
        //    Req.Method = "POST";
        //    //return HttpWebRequest    
        //    return Req;
        //}


        public async Task<merchantPaymentUrlResponse> merchantNewFlowPaymentUrl(ThirdpartyPaymentByCardRequest request, string headerToken)
        {
            var response = new merchantPaymentUrlResponse();
            var _commission = new CalculateCommissionResponse();
            var _commissionRequest = new CalculateCommissionRequest();

            var transationInitiate = new TransactionInitiateRequest();
            var _thirdPartyPaymentByCard = new ThirdPartyPaymentByCard();

            try
            {

                var UserDetail = await _walletUserService.UserProfile(headerToken);

                var Isdocverified = await _walletUserRepository.IsDocVerifiedMOMO(UserDetail.DocumetStatus);

                var resultTL = await _transactionLimitAUService.CheckTransactionLimitAU(request.WalletUserId.ToString()); //check New TL
                var transactionLimit = await _masterDataRepository.GetTransactionLimitAddMoney(Convert.ToString(UserDetail.WalletUserId));
                int limit = transactionLimit != null ? Convert.ToInt32(transactionLimit.TransactionLimitForAddMoney) : 0;

                var transactionHistory = _masterDataRepository.GetAllTransactionsAddMoney(UserDetail.WalletUserId);
                int totalAmountTransfered = transactionHistory != null ? Convert.ToInt32(transactionHistory.totalAmount) : 0;

                #region Calculate commission on request amount               


                var WalletService = await _cardPaymentRepository.GetWalletService("FlutterXOF", 57);


                if (UserDetail.IsActive == true)//am
                {
                    if (UserDetail.IsEmailVerified == true)
                    {
                        if (Isdocverified == true)
                        {
                            if (transactionLimit == null || transactionLimit.TransactionLimitForAddMoney == null || limit >= (Convert.ToDecimal(request.Amount) + totalAmountTransfered))
                            {
                                if (WalletService.WalletServiceId > 0)
                                {
                                    #region Calculate Commission on request amount
                                    _commissionRequest.IsRoundOff = true;
                                    _commissionRequest.TransactionAmount = Convert.ToDecimal(request.Amount);
                                    _commissionRequest.WalletServiceId = WalletService.WalletServiceId;
                                    _commission = await _setCommisionRepository.CalculateCommission(_commissionRequest);
                                    #endregion
                                }


                                decimal amountWithCommision = decimal.Parse(string.Format("{0:0,0}", _commission.AmountWithCommission));    // "1,234,257";

                                #endregion
                                if (resultTL != null)
                                {
                                    decimal SetAmount = Convert.ToDecimal(resultTL.SetAmount);// returns decimal
                                    decimal TotalAmount = Convert.ToDecimal(resultTL.TotalAmount);// returns decimal

                                    if (SetAmount != 0) //0 =msg 
                                    {
                                        decimal requestAmountwithcomm = amountWithCommision + TotalAmount;

                                        if (requestAmountwithcomm <= SetAmount)//1000 >= 1000
                                        {

                                        }
                                        else
                                        {
                                            response.RstKey = 6;
                                            response.Message = ResponseMessages.INVALID_txnAmountREQUEST;
                                            return response;
                                        }
                                    }
                                }
                                var invoiceNumber = await _masterDataRepository.GetInvoiceNumber();
                                var _masterCard = new MasterCardPaymentRequest();

                                _masterCard.SessionId = null;
                                _masterCard.Version = null;
                                _masterCard.SuccessIndicator = null;
                                _masterCard.Merchant = "FlutterXOF";
                                _masterCard.IsActive = true;
                                _masterCard.IsDeleted = false;
                                _masterCard.CreatedDate = DateTime.UtcNow;
                                _masterCard.UpdatedDate = DateTime.UtcNow;
                                _masterCard.Amount = request.Amount;
                                _masterCard.CommisionCharges = _commission.CommisionPercent;
                                _masterCard.TotalAmount = Convert.ToString(amountWithCommision);
                                _masterCard.WalletUserId = UserDetail.WalletUserId;
                                _masterCard.TransactionNo = invoiceNumber.InvoiceNumber;
                                _masterCard.FlatCharges = _commission.FlatCharges;
                                _masterCard.BenchmarkCharges = _commission.BenchmarkCharges;
                                _masterCard.CommissionAmount = _commission.CommissionAmount.ToString();
                                await _cardPaymentRepository.SaveMasterCardPaymentRequest(_masterCard);



                                var _RequestAttributes = new merchantrequest();
                                _RequestAttributes.api_key = "6192f2b6-918c-4ccd-bdb1-5772060c1ac8";
                                _RequestAttributes.user_id = UserDetail.WalletUserId.ToString();
                                _RequestAttributes.amount = Convert.ToDecimal(request.Amount);
                                _RequestAttributes.currency_code = "USD";
                                _RequestAttributes.user_email = UserDetail.EmailId;
                                _RequestAttributes.user_phone_number = UserDetail.StdCode + UserDetail.MobileNo;

                                _RequestAttributes.transaction_id = invoiceNumber.InvoiceNumber;
                                _RequestAttributes.return_url = "http://localhost:63159/MasterCardPayment/SaveMerchantPaymentResponse";

                                var req = JsonConvert.SerializeObject(_RequestAttributes);

                                _logUtils.WriteTextToFileForFlutterPeyLoadLogs("SavemerchantgloblRequestDetail :- InvoiceNumber " + invoiceNumber.InvoiceNumber + " " + req);
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                //here to get psaymenturl
                                var responseData2 = await GethashorUrl(req, null, "merchantglobl");
                                //{ "status":"success","message":"","link":"http://35.85.177.202:9094/#/sessionId?id=zvCADbQrJKT1eqUOTfYLOWTwyS9T8ndjmsqRBtnSACSnnFaozyP4SPQg20KFLCOoROzR9i7Iucc3nUqdtUQXqbTCRBOoLJyzjNhiXBj%2Bg64%3D"}
                                var _responseModel2 = JsonConvert.DeserializeObject<merchantPaymentUrlResponse>(responseData2);
                                if (_responseModel2.link != null && _responseModel2.status == "success")
                                {
                                    transationInitiate.InvoiceNumber = invoiceNumber.InvoiceNumber;
                                    transationInitiate.ReceiverNumber = UserDetail.MobileNo;
                                    transationInitiate.ServiceName = "merchantglobl";
                                    transationInitiate.RequestedAmount = _commission.TransactionAmount.ToString();
                                    transationInitiate.TransactionStatus = (int)TransactionStatus.Pending;
                                    transationInitiate.WalletUserId = UserDetail.WalletUserId;
                                    transationInitiate.UserReferanceNumber = invoiceNumber.AutoDigit;
                                    transationInitiate.CurrentBalance = UserDetail.CurrentBalance;
                                    transationInitiate.AfterTransactionBalance = "";
                                    transationInitiate.ReceiverCurrentBalance = UserDetail.CurrentBalance;
                                    transationInitiate.UserName = UserDetail.FirstName + " " + UserDetail.LastName;
                                    transationInitiate.ReceiverWalletUserId = UserDetail.WalletUserId;
                                    transationInitiate.CreatedDate = DateTime.UtcNow;
                                    transationInitiate.UpdatedDate = DateTime.UtcNow;
                                    transationInitiate.IsActive = true;
                                    transationInitiate.IsDeleted = false;
                                    transationInitiate.JsonRequest = responseData2;
                                    transationInitiate.JsonResponse = "";
                                    transationInitiate = await _cardPaymentRepository.SaveTransactionInitiateRequest(transationInitiate);
                                    response.URL = _responseModel2.link;
                                    response.RstKey = 2;
                                }

                                else
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.Message = "Please try after some time aggregator error.";
                                }
                                LogTransactionTypes.Request.SaveTransactionLog(LogTransactionNameTypes.AddMoney + "Credit TO Debit Cards", responseData2, "Request Url : " + response.link);
                            }
                            else
                            {
                                var addLimit = limit - (Convert.ToDecimal(request.Amount) + totalAmountTransfered);
                                if (addLimit < Convert.ToDecimal(request.Amount))
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.Message = "Exceed your transaction limit.";
                                }
                                else
                                {
                                    response.RstKey = 6;
                                    response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                                    response.Message = "You can not add more then " + transactionLimit.TransactionLimitForAddMoney.ToString() + " cedi in a day";
                                }
                            }
                        }
                        else if (UserDetail.DocumetStatus == 0 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
                        {
                            response.RstKey = 13;
                            response.Message = ResponseMessageKyc.FAILED_Doc_NotUploaded;
                        }
                        else if (UserDetail.DocumetStatus == 1 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
                        {
                            response.RstKey = 14;
                            response.Message = ResponseMessageKyc.FAILED_Doc_Pending;
                        }
                        else if (UserDetail.DocumetStatus == 4 || string.IsNullOrWhiteSpace(UserDetail.DocumetStatus.ToString()))
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
                        response.RstKey = 6;
                        response.StatusCode = (int)TransactionStatus.Failed;
                        response.Message = ResponseMessages.EMAIL_VERIFICATION_PENDING;
                    }
                }
                else
                {
                    response.RstKey = 6;
                    response.Message = ResponseMessages.TRANSACTION_DISABLED;
                }
            }

            catch (Exception ex)
            {

                //tran.Rollback();
            }
            return response;

        }

        public async Task<AddMoneyAggregatorResponse> SaveMerchantPaymentResponse(string txt_ref)
        {
            AddMoneyAggregatorResponse response = new AddMoneyAggregatorResponse();
            CalculateCommissionResponse _commission = new CalculateCommissionResponse();
            CalculateCommissionRequest _commissionRequest = new CalculateCommissionRequest();
            string RequestString = JsonConvert.SerializeObject(txt_ref);

            LogTransactionTypes.Response.SaveTransactionLog(LogTransactionNameTypes.AddMoney + "Credit to Debit Cards", RequestString, "Responce by bank detail : ");

            try
            {

                var getInitialTransaction = await _cardPaymentRepository.GetTransactionInitiateRequest(txt_ref);

                int GetWalletTransactionsexist = await _cardPaymentRepository.GetWalletTransactionsexist(getInitialTransaction.WalletUserId, txt_ref);

                if (txt_ref != null && GetWalletTransactionsexist == 0)
                {

                    getInitialTransaction.JsonResponse = JsonConvert.SerializeObject(txt_ref);
                    var responseData2 = await GethashorUrl(txt_ref, null, "merchantGlobalverify");

                    JavaScriptSerializer js = new JavaScriptSerializer();
                    dynamic blogObject = js.Deserialize<dynamic>(responseData2);


                    var txnreverifystatus = blogObject["data"]["status"];//stagin

                    //check txn verify flutter --when not succesfujl statsu got from txn verify & suceeful get from cllback 
                    if (txnreverifystatus != "approved")
                    {
                        WalletService WalletService = new WalletService();
                        response.InvoiceNo = txt_ref;
                        response.Amount = getInitialTransaction.RequestedAmount;
                        //response.status = "flutter";
                        if (getInitialTransaction.ServiceName == "FlutterXOF")
                        {
                            response.status = "Flutter XOF";
                            WalletService = await _cardPaymentRepository.GetWalletService("FlutterXOF", 57);
                        }
                        else if (getInitialTransaction.ServiceName == "FlutterUSD")
                        {
                            response.status = "Flutter USD";
                            WalletService = await _cardPaymentRepository.GetWalletService("FlutterUSD", 58);
                        }
                        else if (getInitialTransaction.ServiceName == "FlutterEURO")
                        {
                            response.status = "Flutter EURO";
                            WalletService = await _cardPaymentRepository.GetWalletService("FlutterEURO", 59);
                        }

                        DateTime TDate = DateTime.UtcNow;
                        response.TransactionDate = TDate;

                        //int WalletServiceId = await _cardPaymentRepository.GetServiceId();
                        if (WalletService.WalletServiceId > 0)
                        {
                            var adminUser = await _cardPaymentRepository.GetAdminUser();
                            if (adminUser != null)
                            {
                                getInitialTransaction.RequestedAmount = Convert.ToString(Math.Round(Convert.ToDecimal(getInitialTransaction.RequestedAmount), 2));

                                long userId = Convert.ToInt32(getInitialTransaction.WalletUserId);
                                var UserCurrentDetail = await _walletUserRepository.GetCurrentUser(userId);
                                if (UserCurrentDetail != null && await _cardPaymentRepository.IsWalletTransactions(UserCurrentDetail.WalletUserId, txt_ref))
                                {
                                    getInitialTransaction.RequestedAmount = Convert.ToString(Math.Round(Convert.ToDecimal(getInitialTransaction.RequestedAmount), 2));

                                    _commissionRequest.CurrentBalance = Convert.ToDecimal(UserCurrentDetail.CurrentBalance);
                                    _commissionRequest.IsRoundOff = true;
                                    _commissionRequest.TransactionAmount = Convert.ToDecimal(getInitialTransaction.RequestedAmount); //change
                                    _commissionRequest.WalletServiceId = WalletService.WalletServiceId;
                                    _commission = await _setCommisionRepository.CalculateCommission(_commissionRequest);

                                    getInitialTransaction.TransactionStatus = (int)TransactionStatus.Pending;
                                    await _cardPaymentRepository.UpdateTransactionInitiateRequest(getInitialTransaction);

                                    #region Save Transaction
                                    decimal amountWithCommision = decimal.Parse(string.Format("{0:0,0}", _commission.AmountWithCommission));    // "1,234,257";

                                    var _Transaction = new WalletTransaction();

                                    _Transaction.TransactionInitiateRequestId = getInitialTransaction.Id;
                                    _Transaction.MerchantCommissionAmount = Convert.ToString(_commission.MerchantCommissionAmount);
                                    _Transaction.MerchantCommissionId = _commission.MerchantCommissionId;
                                    _Transaction.TransactionType = AggragatorServiceType.CREDIT;
                                    _Transaction.IsBankTransaction = false;
                                    _Transaction.TransactionTypeInfo = (int)TransactionTypeInfo.AddedByCard;
                                    _Transaction.IsBankTransaction = false;
                                    _Transaction.Comments = string.Empty;
                                    _Transaction.InvoiceNo = getInitialTransaction.InvoiceNumber;
                                    _Transaction.CommisionAmount = Convert.ToString(_commission.CommissionAmount);
                                    _Transaction.CommisionId = _commission.CommissionId;
                                    _Transaction.WalletAmount = Convert.ToString(_commission.TransactionAmount);

                                    _Transaction.TotalAmount = Convert.ToString(amountWithCommision);
                                    _Transaction.ServiceTaxRate = _commission.ServiceTaxRate;
                                    _Transaction.ServiceTax = Convert.ToString(_commission.ServiceTaxAmount);
                                    _Transaction.OperatorType = WalletService.ServiceName;

                                    _Transaction.AccountNo = getInitialTransaction.ReceiverNumber;

                                    _Transaction.TransactionStatus = (int)TransactionStatus.Failed;

                                    _Transaction.WalletServiceId = WalletService.WalletServiceId;
                                    _Transaction.SenderId = UserCurrentDetail.WalletUserId;
                                    _Transaction.ReceiverId = UserCurrentDetail.WalletUserId;
                                    _Transaction.BankBranchCode = string.Empty;
                                    _Transaction.BankTransactionId = txt_ref;
                                    _Transaction.TransactionId = getInitialTransaction.InvoiceNumber;


                                    _Transaction.IsAdminTransaction = false;
                                    _Transaction.IsActive = true;
                                    _Transaction.IsDeleted = false;
                                    _Transaction.CreatedDate = TDate;
                                    _Transaction.UpdatedDate = TDate;
                                    _Transaction.TransactionTypeInfo = (int)TransactionTypeInfo.AddedByCard;
                                    _Transaction.IsAddDuringPay = false;
                                    _Transaction.VoucherCode = string.Empty;

                                    await _cardPaymentRepository.SaveWalletTransactions(_Transaction);
                                    //db.WalletTransactions.Add(_Transaction);
                                    //db.SaveChanges();
                                    #endregion

                                    #region Credit
                                    var _credit = new WalletTransactionDetail();
                                    _credit.Amount = Convert.ToString(_commission.TransactionAmount);
                                    _credit.TransactionType = (int)TransactionDetailType.Credit;
                                    _credit.WalletUserId = adminUser.WalletUserId;
                                    _credit.WalletTransactionId = _Transaction.WalletTransactionId;
                                    _credit.IsActive = true;
                                    _credit.IsDeleted = false;
                                    _credit.CreatedDate = TDate;
                                    _credit.UpdatedDate = TDate;
                                    //db.WalletTransactionDetails.Add(_credit);
                                    //db.SaveChanges();
                                    await _cardPaymentRepository.SaveWalletTransactionDetails(_credit);
                                    #endregion

                                    #region Debit
                                    var _debit = new WalletTransactionDetail();
                                    _debit.Amount = Convert.ToString(_commission.TransactionAmount);
                                    _debit.TransactionType = (int)TransactionDetailType.Debit;
                                    _debit.WalletUserId = UserCurrentDetail.WalletUserId;
                                    _debit.WalletTransactionId = _Transaction.WalletTransactionId;
                                    _debit.IsActive = true;
                                    _debit.IsDeleted = false;
                                    _debit.CreatedDate = TDate;
                                    _debit.UpdatedDate = TDate;
                                    //db.WalletTransactionDetails.Add(_credit);
                                    //db.SaveChanges();
                                    await _cardPaymentRepository.SaveWalletTransactionDetails(_credit);
                                    #endregion

                                    //get UpdateNewCardNoResponseBankCode id
                                    await _cardPaymentRepository.UpdateNewCardNoResponseBankCode(getInitialTransaction.InvoiceNumber, UserCurrentDetail.WalletUserId, txt_ref);


                                    var adminKeyPair = AES256.AdminKeyPair;


                                    //db.SaveChanges();
                                    //tran.Commit();


                                    if (UserCurrentDetail.MobileNo != null && UserCurrentDetail.MobileNo != "")
                                    {
                                        response.ToMobileNo = AES256.Decrypt(adminKeyPair.PrivateKey, UserCurrentDetail.MobileNo);
                                    }


                                    response.CurrentBalance = UserCurrentDetail.CurrentBalance;
                                    response.RstKey = 2;



                                }
                                else
                                {
                                    //test

                                }
                            }
                            else
                            {
                                //test

                            }
                            //sdfsdfd
                        }
                        else
                        {

                            response.RstKey = 2;
                        }
                    }
                    //after verify flutter txn then crediht to user --when succesfujl statsu got from both
                    else if (txt_ref != null && txnreverifystatus == "approved")
                    {
                        response.InvoiceNo = txt_ref;
                        response.Amount = getInitialTransaction.RequestedAmount;
                        //response.status = "flutter";
                        WalletService WalletService = new WalletService();
                        if (getInitialTransaction.ServiceName == "FlutterXOF")
                        {
                            response.status = "Flutter XOF";
                            WalletService = await _cardPaymentRepository.GetWalletService("FlutterXOF", 57);
                        }
                        else if (getInitialTransaction.ServiceName == "FlutterUSD")
                        {
                            response.status = "Flutter USD";
                            WalletService = await _cardPaymentRepository.GetWalletService("FlutterUSD", 58);
                        }
                        else if (getInitialTransaction.ServiceName == "FlutterEURO")
                        {
                            response.status = "Flutter EURO";
                            WalletService = await _cardPaymentRepository.GetWalletService("FlutterEURO", 59);
                        }
                        DateTime TDate = DateTime.UtcNow;
                        response.TransactionDate = TDate;

                        //int WalletServiceId = await _cardPaymentRepository.GetServiceId();
                        if (WalletService.WalletServiceId > 0)
                        {
                            var adminUser = await _cardPaymentRepository.GetAdminUser();
                            if (adminUser != null)
                            {

                                // var receiver = await _walletUserRepository.GetCurrentUser(Convert.ToInt32(requestDetail.WalletUserId)); //db.WalletUsers.Where(x => x.WalletUserId == requestDetail.WalletUserId).FirstOrDefault();

                                long userId = Convert.ToInt32(getInitialTransaction.WalletUserId);
                                var UserCurrentDetail = await _walletUserRepository.GetCurrentUser(userId);
                                if (UserCurrentDetail != null && await _cardPaymentRepository.IsWalletTransactions(UserCurrentDetail.WalletUserId, txt_ref))
                                {
                                    //this line commented due to currentbalance is not added to card expected 
                                    //request.vpc_Amount = Convert.ToString(Math.Round((Convert.ToDecimal(requestDetail.Amount) / 100), 2));
                                    getInitialTransaction.RequestedAmount = Convert.ToString(Math.Round(Convert.ToDecimal(getInitialTransaction.RequestedAmount), 2));

                                    // to update wallet amount-----

                                    // db.WalletUsers.FirstOrDefault(x => x.WalletUserId == receiver.WalletUserId);

                                    if (UserCurrentDetail != null)
                                    {
                                        _commissionRequest.CurrentBalance = Convert.ToDecimal(UserCurrentDetail.CurrentBalance);
                                        _commissionRequest.IsRoundOff = true;
                                        //_commissionRequest.TransactionAmount = Convert.ToDecimal(request.vpc_Amount);

                                        _commissionRequest.TransactionAmount = Convert.ToDecimal(getInitialTransaction.RequestedAmount); //change
                                        _commissionRequest.WalletServiceId = WalletService.WalletServiceId;
                                        _commission = await _setCommisionRepository.CalculateCommission(_commissionRequest);

                                        if (!string.IsNullOrEmpty(txt_ref))
                                        {
                                            getInitialTransaction.TransactionStatus = (int)TransactionStatus.Completed;
                                            if (Convert.ToDecimal(UserCurrentDetail.CurrentBalance) >= 0)
                                            {
                                                if (Convert.ToDecimal(UserCurrentDetail.CurrentBalance) == 0)
                                                {
                                                    UserCurrentDetail.CurrentBalance = _commission.TransactionAmount.ToString();
                                                    getInitialTransaction.AfterTransactionBalance = UserCurrentDetail.CurrentBalance.ToString();
                                                    getInitialTransaction.ReceiverCurrentBalance = UserCurrentDetail.CurrentBalance.ToString();
                                                }
                                                else
                                                {
                                                    UserCurrentDetail.CurrentBalance = Math.Round(Convert.ToDecimal(UserCurrentDetail.CurrentBalance) + _commission.TransactionAmount, 2).ToString();
                                                    getInitialTransaction.AfterTransactionBalance = UserCurrentDetail.CurrentBalance.ToString();
                                                    getInitialTransaction.ReceiverCurrentBalance = UserCurrentDetail.CurrentBalance.ToString();
                                                }
                                            }
                                            else
                                            {
                                                UserCurrentDetail.CurrentBalance = Math.Round(_commission.TransactionAmount - Convert.ToDecimal(UserCurrentDetail.CurrentBalance), 2).ToString();
                                                getInitialTransaction.AfterTransactionBalance = UserCurrentDetail.CurrentBalance.ToString();
                                                getInitialTransaction.ReceiverCurrentBalance = UserCurrentDetail.CurrentBalance.ToString();
                                            }
                                        }
                                        await _walletUserRepository.UpdateUserDetail(UserCurrentDetail);
                                        await _cardPaymentRepository.UpdateTransactionInitiateRequest(getInitialTransaction);
                                        // db.SaveChanges();
                                    }

                                    #region Save Transaction
                                    decimal amountWithCommision = decimal.Parse(string.Format("{0:0,0}", _commission.AmountWithCommission));    // "1,234,257";

                                    var _Transaction = new WalletTransaction();

                                    _Transaction.TransactionInitiateRequestId = getInitialTransaction.Id;
                                    _Transaction.MerchantCommissionAmount = Convert.ToString(_commission.MerchantCommissionAmount);
                                    _Transaction.MerchantCommissionId = _commission.MerchantCommissionId;
                                    _Transaction.TransactionType = AggragatorServiceType.CREDIT;
                                    _Transaction.IsBankTransaction = false;
                                    _Transaction.TransactionTypeInfo = (int)TransactionTypeInfo.AddedByCard;
                                    _Transaction.IsBankTransaction = false;
                                    _Transaction.Comments = string.Empty;
                                    _Transaction.InvoiceNo = getInitialTransaction.InvoiceNumber;
                                    _Transaction.CommisionAmount = Convert.ToString(_commission.CommissionAmount);
                                    _Transaction.CommisionId = _commission.CommissionId;
                                    _Transaction.WalletAmount = Convert.ToString(_commission.TransactionAmount);

                                    _Transaction.TotalAmount = Convert.ToString(amountWithCommision);
                                    _Transaction.ServiceTaxRate = _commission.ServiceTaxRate;
                                    _Transaction.ServiceTax = Convert.ToString(_commission.ServiceTaxAmount);
                                    _Transaction.OperatorType = WalletService.ServiceName;

                                    _Transaction.AccountNo = getInitialTransaction.ReceiverNumber;
                                    if (!string.IsNullOrEmpty(txt_ref))
                                    {

                                        _Transaction.TransactionStatus = (int)TransactionStatus.Completed; ;
                                        try
                                        {
                                            //--------send mail on success transaction--------

                                            var AdminKeys = AES256.AdminKeyPair;
                                            string FirstName = AES256.Decrypt(UserCurrentDetail.PrivateKey, UserCurrentDetail.FirstName);
                                            string LastName = AES256.Decrypt(UserCurrentDetail.PrivateKey, UserCurrentDetail.LastName);
                                            string StdCode = UserCurrentDetail.StdCode;
                                            string MobileNo = AES256.Decrypt(AdminKeys.PrivateKey, UserCurrentDetail.MobileNo);
                                            string EmailId = AES256.Decrypt(AdminKeys.PrivateKey, UserCurrentDetail.EmailId).Trim().ToLower();
                                            // var receiverDetail = new AppUserRepository().GetUserDetailById(receiver.WalletUserId);
                                            string filename = CommonSetting.successfullTransaction;


                                            var body = _sendEmails.ReadEmailformats(filename);
                                            body = body.Replace("$$FirstName$$", FirstName + " " + LastName);
                                            body = body.Replace("$$DisplayContent$$", "Flutter CARD");
                                            body = body.Replace("$$customer$$", MobileNo);
                                            body = body.Replace("$$amount$$", "USD " + getInitialTransaction.RequestedAmount);
                                            body = body.Replace("$$ServiceTaxAmount$$", "USD " + _commission.CommissionAmount);
                                            body = body.Replace("$$AmountWithCommission$$", "USD " + amountWithCommision);
                                            body = body.Replace("$$TransactionId$$", txt_ref);

                                            var req = new EmailModel()
                                            {
                                                TO = EmailId,
                                                Subject = "Transaction Successfull",
                                                Body = body
                                            };
                                            _sendEmails.SendEmail(req);
                                        }
                                        catch
                                        {

                                        }
                                    }
                                    else
                                    {
                                        _Transaction.TransactionStatus = (int)TransactionStatus.Failed;
                                    }
                                    _Transaction.WalletServiceId = WalletService.WalletServiceId;
                                    _Transaction.SenderId = UserCurrentDetail.WalletUserId;
                                    _Transaction.ReceiverId = UserCurrentDetail.WalletUserId;
                                    _Transaction.BankBranchCode = string.Empty;
                                    _Transaction.BankTransactionId = txt_ref;
                                    _Transaction.TransactionId = getInitialTransaction.InvoiceNumber;


                                    _Transaction.IsAdminTransaction = false;
                                    _Transaction.IsActive = true;
                                    _Transaction.IsDeleted = false;
                                    _Transaction.CreatedDate = TDate;
                                    _Transaction.UpdatedDate = TDate;
                                    _Transaction.TransactionTypeInfo = (int)TransactionTypeInfo.AddedByCard;
                                    _Transaction.IsAddDuringPay = false;
                                    _Transaction.VoucherCode = string.Empty;

                                    await _cardPaymentRepository.SaveWalletTransactions(_Transaction);
                                    //db.WalletTransactions.Add(_Transaction);
                                    //db.SaveChanges();
                                    #endregion

                                    #region Credit
                                    var _credit = new WalletTransactionDetail();
                                    _credit.Amount = Convert.ToString(_commission.TransactionAmount);
                                    _credit.TransactionType = (int)TransactionDetailType.Credit;
                                    _credit.WalletUserId = adminUser.WalletUserId;
                                    _credit.WalletTransactionId = _Transaction.WalletTransactionId;
                                    _credit.IsActive = true;
                                    _credit.IsDeleted = false;
                                    _credit.CreatedDate = TDate;
                                    _credit.UpdatedDate = TDate;
                                    //db.WalletTransactionDetails.Add(_credit);
                                    //db.SaveChanges();
                                    await _cardPaymentRepository.SaveWalletTransactionDetails(_credit);
                                    #endregion

                                    #region Debit
                                    var _debit = new WalletTransactionDetail();
                                    _debit.Amount = Convert.ToString(_commission.TransactionAmount);
                                    _debit.TransactionType = (int)TransactionDetailType.Debit;
                                    _debit.WalletUserId = UserCurrentDetail.WalletUserId;
                                    _debit.WalletTransactionId = _Transaction.WalletTransactionId;
                                    _debit.IsActive = true;
                                    _debit.IsDeleted = false;
                                    _debit.CreatedDate = TDate;
                                    _debit.UpdatedDate = TDate;
                                    //db.WalletTransactionDetails.Add(_credit);
                                    //db.SaveChanges();
                                    await _cardPaymentRepository.SaveWalletTransactionDetails(_credit);
                                    #endregion

                                    //get UpdateNewCardNoResponseBankCode id
                                    await _cardPaymentRepository.UpdateNewCardNoResponseBankCode(getInitialTransaction.InvoiceNumber, UserCurrentDetail.WalletUserId, txt_ref);
                                    //updatfe webhook when callback receive
                                    await _cardPaymentRepository.Updatewebhookflutterflagsuccestxninvoiceno(getInitialTransaction.InvoiceNumber);
                                    var adminKeyPair = AES256.AdminKeyPair;
                                    //db.SaveChanges();
                                    //tran.Commit();
                                    #region PushNotification

                                    var CurrentUser = await _walletUserRepository.GetCurrentUser(UserCurrentDetail.WalletUserId);//db.WalletUsers.Where(x => x.WalletUserId == receiver.WalletUserId).FirstOrDefault();
                                    if (CurrentUser != null)
                                    {
                                        PushNotificationModel push = new PushNotificationModel();
                                        push.SenderId = UserCurrentDetail.WalletUserId;
                                        push.deviceType = (int)UserCurrentDetail.DeviceType;
                                        push.deviceKey = UserCurrentDetail.DeviceToken;
                                        PayMoneyPushModel pushModel = new PayMoneyPushModel();
                                        pushModel.TransactionDate = TDate;
                                        pushModel.TransactionId = txt_ref;
                                        pushModel.CurrentBalance = CurrentUser.CurrentBalance;
                                        pushModel.alert = _Transaction.WalletAmount + " XOF has been credited to your account.";
                                        pushModel.Amount = getInitialTransaction.RequestedAmount;
                                        pushModel.CurrentBalance = CurrentUser.CurrentBalance;
                                        pushModel.pushType = (int)PushType.ADDMONEY;

                                        if ((int)UserCurrentDetail.DeviceType == (int)DeviceTypes.ANDROID || (int)UserCurrentDetail.DeviceType == (int)DeviceTypes.Web)
                                        {
                                            PushPayload<PayMoneyPushModel> aps = new PushPayload<PayMoneyPushModel>();
                                            PushPayloadData<PayMoneyPushModel> _data = new PushPayloadData<PayMoneyPushModel>();
                                            _data.notification = pushModel;
                                            aps.data = _data;
                                            aps.to = UserCurrentDetail.DeviceToken;
                                            aps.collapse_key = string.Empty;
                                            push.message = JsonConvert.SerializeObject(aps);
                                            push.payload = pushModel;
                                        }
                                        if ((int)UserCurrentDetail.DeviceType == (int)DeviceTypes.IOS)
                                        {
                                            NotificationJsonResponse<PayMoneyIOSPushModel> aps = new NotificationJsonResponse<PayMoneyIOSPushModel>();
                                            PayMoneyIOSPushModel _iosPushModel = new PayMoneyIOSPushModel();
                                            _iosPushModel.alert = pushModel.alert;
                                            _iosPushModel.Amount = pushModel.Amount;
                                            _iosPushModel.CurrentBalance = pushModel.CurrentBalance;
                                            _iosPushModel.MobileNo = pushModel.MobileNo;
                                            _iosPushModel.SenderName = pushModel.SenderName;
                                            _iosPushModel.pushType = pushModel.pushType;
                                            aps.aps = _iosPushModel;

                                            push.message = JsonConvert.SerializeObject(aps);
                                        }
                                        //if (!string.IsNullOrEmpty(push.message))
                                        //{
                                        //    new PushNotificationRepository().sendPushNotification(push);
                                        //}
                                    }
                                    #endregion

                                    if (UserCurrentDetail.MobileNo != null && UserCurrentDetail.MobileNo != "")
                                    {
                                        response.ToMobileNo = AES256.Decrypt(adminKeyPair.PrivateKey, UserCurrentDetail.MobileNo);
                                    }


                                    response.CurrentBalance = UserCurrentDetail.CurrentBalance;
                                    response.RstKey = 1;

                                    ///
                                    await _masterDataService.Chargeback(UserCurrentDetail.WalletUserId);
                                    //get UpdateNewCardNoResponseBankCode id
                                    await _cardPaymentRepository.UpdateNewCardNoResponseBankCode(getInitialTransaction.InvoiceNumber, UserCurrentDetail.WalletUserId, txt_ref);

                                    await _cardPaymentRepository.Updatewebhookflutterflagsuccestxninvoiceno(getInitialTransaction.InvoiceNumber);
                                }
                                else
                                {
                                    //test

                                }
                            }
                            else
                            {
                                //test

                            }
                            //sdfsdfd
                        }
                        else
                        {

                            response.RstKey = 2;
                        }
                    }
                    else
                    {
                        await _cardPaymentRepository.UpdateTransactionInitiateRequest(getInitialTransaction);
                        response.RstKey = 3;

                        //response.TransactionResponseDescription = _req.vpc_ResponseCodeDescription;
                        //response.TransactionResponseCode = request.vpc_TxnResponseCode;
                    }



                }
                else
                {
                    response.RstKey = 3;
                }

            }
            catch (Exception ex)
            {

                // ex.Message.ErrorLog("CardPaymentService", "SaveflutterCardPaymentResponse", request);
                "MasterCardPaymentController".ErrorLog("CardPaymentService", "SaveflutterCardPaymentResponse", ex.StackTrace + " " + ex.Message);
            }
            return response;
        }

        public async Task<string> GethashorUrl(string jsonReq, string token, string flag)
        {

            string resBody = "";
            RootObject responseData = new RootObject();
            using (HttpClient client = new HttpClient())
            {
                // Call asynchronous network methods in a try/catch block to handle exceptions
                try
                {
                    if (flag == "hash")
                    {
                        var content = new StringContent(jsonReq, Encoding.UTF8, "application/json");
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                        HttpResponseMessage response = await client.PostAsync(CommonSetting.SeerbitGethashUrl, content);
                        response.EnsureSuccessStatusCode();
                        resBody = await response.Content.ReadAsStringAsync();

                    }
                    else if (flag == "Url")
                    {
                        var content = new StringContent(jsonReq, Encoding.UTF8, "application/json");
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                        HttpResponseMessage response = await client.PostAsync(CommonSetting.SeerbitGetpaymentsUrl, content);
                        response.EnsureSuccessStatusCode();
                        resBody = await response.Content.ReadAsStringAsync();

                    }
                    else if (flag == "mcbcardverify")
                    {

                        var username = "merchant.UBAEZIPAYCDI";
                        var password = "32ec3654dbb52a7a3d56e90030f490e7";

                        var base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64String);

                        HttpResponseMessage response = await client.GetAsync("https://ap.gateway.mastercard.com/api/rest/version/61/merchant/UBAEZIPAYCDI/order/" + jsonReq);
                        response.EnsureSuccessStatusCode();
                        resBody = await response.Content.ReadAsStringAsync();


                    }

                    else if (flag == "merchantglobl")
                    {
                        var content = new StringContent(jsonReq, Encoding.UTF8, "application/json");
                        var username = "Merchant.3359";
                        var password = "6192f2b6-918c-4ccd-bdb1-5772060c1ac8";

                        var base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64String);

                        HttpResponseMessage response = await client.PostAsync("https://testnew.ezipay.global/merchant/add-money", content);
                        response.EnsureSuccessStatusCode();
                        resBody = await response.Content.ReadAsStringAsync();


                    }
                    else if (flag == "merchantGlobalverify")
                    {

                        var username = "Merchant.3359";
                        var password = "6192f2b6-918c-4ccd-bdb1-5772060c1ac8";

                        var base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64String);

                        HttpResponseMessage response = await client.GetAsync("https://testnew.ezipay.global/merchant/transactions/verify_by_reference?tx_ref=" + jsonReq);
                        response.EnsureSuccessStatusCode();
                        resBody = await response.Content.ReadAsStringAsync();


                    }
                    else if (flag == "NgeniunspaymentsUrl")
                    {
                        var content = new StringContent(jsonReq, Encoding.UTF8, "application/vnd.ni-payment.v2+json");
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.ni-payment.v2+json"));//ACCEPT header                       


                        HttpResponseMessage response = await client.PostAsync(CommonSetting.NgeniunspaymentsUrl, content);
                        response.EnsureSuccessStatusCode();
                        resBody = await response.Content.ReadAsStringAsync();

                    }

                    else if (flag == "Ngeniunspaymentsorderstatus")
                    {
                        //var content = new StringContent(jsonReq, Encoding.UTF8, "application/json");
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.ni-payment.v2+json"));//ACCEPT header                       

                        var url = CommonSetting.GetorderstatusNgeniunspayment + jsonReq;
                        HttpResponseMessage response = await client.GetAsync(url);
                        response.EnsureSuccessStatusCode();
                        resBody = await response.Content.ReadAsStringAsync();

                    }

                    else if (flag == "flutterUrl")
                    {
                        var content = new StringContent(jsonReq, Encoding.UTF8, "application/json");
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CommonSetting.flutterFLWSECKey);

                        HttpResponseMessage response = await client.PostAsync(CommonSetting.flutterpaymentUrl, content);
                        response.EnsureSuccessStatusCode();
                        resBody = await response.Content.ReadAsStringAsync();

                    }
                    else if (flag == "flutterUrlverify")
                    {

                        // client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
                        //var content = new StringContent(null, Encoding.UTF8, "application/json");
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CommonSetting.flutterFLWSECKey);


                        var url = CommonSetting.flutterverifypaymentUrl + jsonReq;
                        HttpResponseMessage response = await client.GetAsync(url);
                        response.EnsureSuccessStatusCode();
                        resBody = await response.Content.ReadAsStringAsync();

                    }
                    else if (flag == "binance")
                    {
                        var timestamp = DateTime.Now.ToFileTime().ToString();

                        string RandomAlphaNumeralsx = RandomAlphaNumerals(32);

                        string binanceapikee = CommonSetting.binanceapikee;

                        String payload = timestamp + "\n" + RandomAlphaNumeralsx + "\n" + jsonReq + "\n";

                        String signature = SHA512_ComputeHash(payload, CommonSetting.binanceSECKee);

                        //String signature = hex(hmac("sha512", payload, CommonSetting.binanceSECKee)).toUpperCase();
                        //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CommonSetting.flutterFLWSECKey);

                        var content = new StringContent(jsonReq, Encoding.UTF8, "application/json");
                        client.DefaultRequestHeaders.Add("BinancePay-Timestamp", timestamp);
                        client.DefaultRequestHeaders.Add("BinancePay-Nonce", RandomAlphaNumeralsx);

                        client.DefaultRequestHeaders.Add("BinancePay-Certificate-SN", binanceapikee);

                        client.DefaultRequestHeaders.Add("BinancePay-Signature", signature);

                        HttpResponseMessage response = await client.PostAsync(CommonSetting.binancepaymentUrl, content);

                        response.EnsureSuccessStatusCode();
                        resBody = await response.Content.ReadAsStringAsync();


                    }
                    else if (flag == "binancewallet")
                    {
                        var timestamp = DateTime.Now.ToFileTime().ToString();

                        string RandomAlphaNumeralsx = RandomAlphaNumerals(32);

                        string binanceapikee = CommonSetting.binanceapikee;

                        String payload = timestamp + "\n" + RandomAlphaNumeralsx + "\n" + jsonReq + "\n";


                        String signature = SHA512_ComputeHash(payload, CommonSetting.binanceSECKee);

                        //String signature = hex(hmac("sha512", payload, CommonSetting.binanceSECKee)).toUpperCase();
                        //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CommonSetting.flutterFLWSECKey);

                        var content = new StringContent(jsonReq, Encoding.UTF8, "application/json");
                        client.DefaultRequestHeaders.Add("BinancePay-Timestamp", timestamp);
                        client.DefaultRequestHeaders.Add("BinancePay-Nonce", RandomAlphaNumeralsx);

                        client.DefaultRequestHeaders.Add("BinancePay-Certificate-SN", binanceapikee);

                        client.DefaultRequestHeaders.Add("BinancePay-Signature", signature);


                        HttpResponseMessage response = await client.PostAsync(CommonSetting.binancepaymenttransferUrl, content);
                        response.EnsureSuccessStatusCode();
                        resBody = await response.Content.ReadAsStringAsync();


                    }
                    else if (flag == "FXKUDI")
                    {
                        var content = new StringContent(jsonReq, Encoding.UTF8, "application/json");
                        //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                        HttpResponseMessage response = await client.PostAsync("https://www.fxkudipay.com/developer/api/direct-debit", content);
                        response.EnsureSuccessStatusCode();
                        resBody = await response.Content.ReadAsStringAsync();

                    }
                    else if (flag == "AddBankFlutter")
                    {
                        var content = new StringContent(jsonReq, Encoding.UTF8, "application/json");
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CommonSetting.flutterFLWSECKey);

                        HttpResponseMessage response = await client.PostAsync(CommonSetting.flutterbankpaymentUrl, content);
                        response.EnsureSuccessStatusCode();
                        resBody = await response.Content.ReadAsStringAsync();

                    }
                    else if (flag == "ZenithBankOTPRequest")
                    {
                        var content = new StringContent(jsonReq, Encoding.UTF8, "application/json");
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CommonSetting.flutterFLWSECKey);

                        HttpResponseMessage response = await client.PostAsync(CommonSetting.flutterbankZenithBankOTPRequest, content);
                        response.EnsureSuccessStatusCode();
                        resBody = await response.Content.ReadAsStringAsync();

                    }
                    else if (flag == "flutterbanktransfer")
                    {
                        var content = new StringContent(jsonReq, Encoding.UTF8, "application/json");
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CommonSetting.flutterFLWSECKey);

                        HttpResponseMessage response = await client.PostAsync(CommonSetting.flutterbankBanktransfer, content);
                        response.EnsureSuccessStatusCode();
                        resBody = await response.Content.ReadAsStringAsync();

                    }
                }
                catch (HttpRequestException e)
                {
                    if (flag == "hash")
                    {
                        e.Message.ErrorLog("GetSeerbitGethash", e.StackTrace + " " + e.Message);
                    }
                    else if (flag == "Url")
                    { e.Message.ErrorLog("GetSeerbitGetUrl", e.StackTrace + " " + e.Message); }
                    else if (flag == "NgeniunspaymentsUrl")
                    { e.Message.ErrorLog("NgeniunspaymentsUrl", e.StackTrace + " " + e.Message); }
                    else if (flag == "Ngeniunspaymentsorderstatus")
                    { e.Message.ErrorLog("Ngeniunspaymentsorderstatus", e.StackTrace + " " + e.Message); }
                    else if (flag == "flutterUrl")
                    { e.Message.ErrorLog("flutterUrl", e.StackTrace + " " + e.Message); }
                    else if (flag == "flutterUrlverify")
                    { e.Message.ErrorLog("flutterUrlverify", e.StackTrace + " " + e.Message); }
                    else if (flag == "AddBankFlutter")
                    { e.Message.ErrorLog("AddBankFlutter", e.StackTrace + " " + e.Message); }
                    else if (flag == "ZenithBankOTPRequest")
                    { e.Message.ErrorLog("ZenithBankOTPRequest", e.StackTrace + " " + e.Message); }
                    else if (flag == "flutterbanktransfer")
                    { e.Message.ErrorLog("flutterbanktransfer", e.StackTrace + " " + e.Message); }
                }
                return resBody;

            }
        }


        public static string SHA512_ComputeHash(string text, string secretKey)
        {
            var hash = new StringBuilder();
            byte[] secretkeyBytes = Encoding.UTF8.GetBytes(secretKey);
            byte[] inputBytes = Encoding.UTF8.GetBytes(text);
            using (var hmac = new HMACSHA512(secretkeyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }

            return hash.ToString().ToUpper();
        }
        string RandomAlphaNumerals(int stringLength)
        {
            Random random = new Random();


            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, stringLength)
              .Select(s => s[random.Next(s.Length)]).ToArray());

        }
        public static string ReverseString(string myStr)
        {
            char[] myArr = myStr.ToCharArray();
            Array.Reverse(myArr);
            return new string(myArr);
        }


    }
}

