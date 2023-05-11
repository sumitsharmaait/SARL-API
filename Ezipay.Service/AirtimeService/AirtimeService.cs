using ezeePay.Utility.CommonClass;
using ezeePay.Utility.Enums;
using Ezipay.Database;
using Ezipay.Repository.AirtimeRepo;
using Ezipay.Repository.CommisionRepo;
using Ezipay.Repository.CommonRepo;
using Ezipay.Repository.MasterData;
using Ezipay.Repository.PayMoneyRepo;
using Ezipay.Repository.UserRepo;
using Ezipay.Service.Admin.TransactionLimitAU;
using Ezipay.Service.CommonService;
using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.Utility.SendEmail;
using Ezipay.Utility.SendPush;
using Ezipay.ViewModel.AirtimeFrVm;
using Ezipay.ViewModel.AirtimeViewModel;
using Ezipay.ViewModel.CommisionViewModel;
using Ezipay.ViewModel.SendEmailViewModel;
using Ezipay.ViewModel.SendPushViewModel;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Ezipay.Repository.AdminRepo.ChargeBack;


namespace Ezipay.Service.AirtimeService
{
    public class AirtimeService : IAirtimeService
    {
        private IAirtimeRepository _airtimeRepository;
        private IWalletUserRepository _walletUserRepository;
        private ISetCommisionRepository _setCommisionRepository;
        private IMasterDataRepository _masterDataRepository;
        private ISendPushNotification _sendPushNotification;
        private IPayMoneyRepository _payMoneyRepository;
        private ISendEmails _sendEmails;
        private ICommonRepository _commonRepository;
        private ICommonServices _commonServices;
        private ITransactionLimitAUService _transactionLimitAUService;
        private IChargeBackRepository _ChargeBackRepository;

        public AirtimeService()
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
            _transactionLimitAUService = new TransactionLimitAUService();
            _ChargeBackRepository = new ChargeBackRepository();
        }
        public async Task<AddMoneyAggregatorResponse> AirtimeServicesV2(PayMoneyAggregatoryRequest request, long WalletUserId = 0)
        {
            string responseString = "";
            var response = new AddMoneyAggregatorResponse();
            string customer = request.customer;
            var _commissionRequest = new CalculateCommissionRequest();
            var _commission = new CalculateCommissionResponse();
            var commonApi = new CommonApi();
            var userData = await _commonRepository.GetDetailForBillPayment(request);

            var sender = userData.sender;
            var WalletService = userData.WalletService;
            var subcategory = userData.SubCategory;
            bool IsdocVerified = userData.IsdocVerified;
            var transactionLimit = userData.transactionLimit;
            int limit = transactionLimit != null ? Convert.ToInt32(transactionLimit.transactionlimit) : 0;
            var transactionHistory = userData.transactionHistory;
            int totalAmountTransfered = transactionHistory != null ? Convert.ToInt32(transactionHistory.totalAmount) : 0;

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
                                if (transactionLimit == null || limit >= (Convert.ToDecimal(request.Amount) + totalAmountTransfered))
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
                                            decimal amountWithCommision = _commission.AmountWithCommission;
                                            decimal currentBalance = Convert.ToDecimal(sender.CurrentBalance);
                                            if (currentBalance > 0 && currentBalance >= amountWithCommision && _commission.CurrentBalance > 0 && _commission.CurrentBalance >= amountWithCommision)
                                            {
                                                if (request.ISD == "+245" || request.ISD == "+227" || request.ISD == "+229" || (request.ISD == "+225" && WalletService.WalletServiceId == 142 || request.ServiceCategoryId == 8) || request.ISD == "+228")
                                                {
                                                    string isdCode = request.ISD.Trim('+');
                                                    customer = isdCode + request.customer;
                                                }
                                                #region Prepare the Model for Request
                                                var _mobileMoneyRequest = new AddMobileMoneyAggregatoryRequest();
                                                var _MobileMoneyRequest = new PayServicesRequestForServices();
                                                var invoiceNumber = await _masterDataRepository.GetInvoiceNumber();
                                                //var invoiceNumber = new ThirdPartyRepository().GetInvoiceNumber();
                                                if (invoiceNumber != null)
                                                {
                                                    _MobileMoneyRequest.partner_transaction_id = invoiceNumber.InvoiceNumber;
                                                }
                                                _MobileMoneyRequest.recipient_phone_number = customer;//Request.customer;
                                                _MobileMoneyRequest.amount = Convert.ToDecimal(request.Amount);

                                                string apiUrl = "";
                                                string username = "";
                                                string password = "";
                                                if (WalletService.WalletServiceId == 28 || WalletService.WalletServiceId == 31 || WalletService.WalletServiceId == 61)
                                                {
                                                    //username = AppSetting.Username_Ci;
                                                    //password = AppSetting.Password_Ci;
                                                    username = "BF5C0D75E110EDA801651D948BA1E699D707421336CA0B141001F2986ABC54CB";
                                                    password = "61874536F5E32D89FD82C5209E548BB181E8474F8949016CB7DBD1072A82EB66";
                                                    if (WalletService.WalletServiceId == 28)
                                                    {
                                                        // _MobileMoneyRequest.service_id =AppSetting.ServiceId_MTN;
                                                        _MobileMoneyRequest.service_id = CommonSetting.ServiceId_MTN;
                                                    }
                                                    else if (WalletService.WalletServiceId == 31)
                                                    {
                                                        _MobileMoneyRequest.service_id = CommonSetting.ServiceId_Moov;
                                                    }
                                                    else if (WalletService.WalletServiceId == 61)
                                                    {
                                                        _MobileMoneyRequest.service_id = CommonSetting.ServiceId_Orange;
                                                    }
                                                    _MobileMoneyRequest.call_back_url = ThirdPartyAggragatorSettings.callBackUrl;
                                                    _MobileMoneyRequest.login_api = ThirdPartyAggragatorSettings.login_api;
                                                    _MobileMoneyRequest.password_api = ThirdPartyAggragatorSettings.password_api;
                                                    _MobileMoneyRequest.partner_id = ThirdPartyAggragatorSettings.partner_id;
                                                    apiUrl = ThirdPartyAggragatorSettings.PayMoneyUrl;
                                                }
                                                else if (WalletService.WalletServiceId == 29 || WalletService.WalletServiceId == 30 || WalletService.WalletServiceId == 120)
                                                {
                                                    username = "CA6036929ECEF298A20161C96FFE7E4E87422A85323A22DF3ED9D66129961B92";
                                                    password = "0C5F49EA753B76E288F130B33E3E76C00CF86D516CF05437F351988014546C38";
                                                    if (WalletService.WalletServiceId == 29)
                                                    {
                                                        _MobileMoneyRequest.service_id = "BF_AIRTIME_TELMOB";
                                                        _MobileMoneyRequest.call_back_url = ThirdPartyAggragatorSettings.callBackUrl;
                                                        _MobileMoneyRequest.login_api = ThirdPartyAggragatorSettings.login_api_BF;
                                                        _MobileMoneyRequest.password_api = ThirdPartyAggragatorSettings.password_api_BF;
                                                        _MobileMoneyRequest.partner_id = ThirdPartyAggragatorSettings.partner_id_BF;
                                                    }
                                                    else if (WalletService.WalletServiceId == 30)
                                                    {

                                                        _MobileMoneyRequest.service_id = "BF_AIRTIME_TELECEL";
                                                        _MobileMoneyRequest.call_back_url = ThirdPartyAggragatorSettings.callBackUrl;
                                                        _MobileMoneyRequest.login_api = ThirdPartyAggragatorSettings.login_api_BF;
                                                        _MobileMoneyRequest.password_api = ThirdPartyAggragatorSettings.password_api_BF;
                                                        _MobileMoneyRequest.partner_id = ThirdPartyAggragatorSettings.partner_id_BF;
                                                    }
                                                    else
                                                    {

                                                        _MobileMoneyRequest.service_id = "BF_AIRTIME_ORANGE";
                                                        _MobileMoneyRequest.call_back_url = ThirdPartyAggragatorSettings.callBackUrl;
                                                        _MobileMoneyRequest.login_api = ThirdPartyAggragatorSettings.login_api_BF;
                                                        _MobileMoneyRequest.password_api = ThirdPartyAggragatorSettings.password_api_BF;
                                                        _MobileMoneyRequest.partner_id = ThirdPartyAggragatorSettings.partner_id_BF;
                                                    }
                                                    apiUrl = ThirdPartyAggragatorSettings.PayMoneyUrl_BF;
                                                }
                                                else if (WalletService.WalletServiceId == 121 || WalletService.WalletServiceId == 122 || WalletService.WalletServiceId == 123)
                                                {
                                                    username = "5EF52919A5DDB84EC0B89D7738F74F2DF459DD8BC0E1271DB171B6993F90920F";
                                                    password = "2399B6FE625E6245C3027AB89613B1A911D7D239525BDE6DEEDF4DC199EED5C4";
                                                    if (WalletService.WalletServiceId == 121)
                                                    {
                                                        _MobileMoneyRequest.service_id = "AIRTIMEORANGEMALI";
                                                        _MobileMoneyRequest.call_back_url = ThirdPartyAggragatorSettings.callBackUrl;
                                                        _MobileMoneyRequest.login_api = ThirdPartyAggragatorSettings.login_api_ML;
                                                        _MobileMoneyRequest.password_api = ThirdPartyAggragatorSettings.password_api_ML;
                                                        _MobileMoneyRequest.partner_id = ThirdPartyAggragatorSettings.partner_id_ML;
                                                    }
                                                    else if (WalletService.WalletServiceId == 122)
                                                    {
                                                        _MobileMoneyRequest.service_id = "ML_AIRTIME_TELECEL";
                                                        _MobileMoneyRequest.call_back_url = ThirdPartyAggragatorSettings.callBackUrl;
                                                        _MobileMoneyRequest.login_api = ThirdPartyAggragatorSettings.login_api_ML;
                                                        _MobileMoneyRequest.password_api = ThirdPartyAggragatorSettings.password_api_ML;
                                                        _MobileMoneyRequest.partner_id = ThirdPartyAggragatorSettings.partner_id_ML;
                                                    }
                                                    else
                                                    {
                                                        _MobileMoneyRequest.service_id = ThirdPartyAggragatorSettings.service_id_ML;
                                                        _MobileMoneyRequest.call_back_url = ThirdPartyAggragatorSettings.callBackUrl;
                                                        _MobileMoneyRequest.login_api = ThirdPartyAggragatorSettings.login_api_ML;
                                                        _MobileMoneyRequest.password_api = ThirdPartyAggragatorSettings.password_api_ML;
                                                        _MobileMoneyRequest.partner_id = ThirdPartyAggragatorSettings.partner_id_ML;
                                                    }

                                                    apiUrl = ThirdPartyAggragatorSettings.PayMoneyUrl_ML;
                                                }
                                                else if (WalletService.WalletServiceId == 126 || WalletService.WalletServiceId == 127 || WalletService.WalletServiceId == 128)
                                                {
                                                    username = "94E212F6CFF7334FF371B91612DA7AFEE1832C2972C81D6FFAD6C212ED14C1F5";
                                                    password = "B538B1B9407990154271132468CC06FF80D5894DA3ADE0ABDED44CDF7B992C8A";
                                                    if (WalletService.WalletServiceId == 126)
                                                    {
                                                        _MobileMoneyRequest.service_id = "AIRTIMEORANGE";
                                                    }
                                                    else if (WalletService.WalletServiceId == 127)
                                                    {
                                                        _MobileMoneyRequest.service_id = "AIRTIMEEXPRESSO";
                                                    }
                                                    else
                                                    {
                                                        _MobileMoneyRequest.service_id = "AIRTIMETIGO";
                                                    }

                                                    _MobileMoneyRequest.call_back_url = ThirdPartyAggragatorSettings.callBackUrl;
                                                    _MobileMoneyRequest.login_api = ThirdPartyAggragatorSettings.login_api_SNG;
                                                    _MobileMoneyRequest.password_api = ThirdPartyAggragatorSettings.password_api_SNG;
                                                    _MobileMoneyRequest.partner_id = ThirdPartyAggragatorSettings.partner_id_SNG;
                                                    apiUrl = ThirdPartyAggragatorSettings.PayMoneyUrl_SNG;
                                                }
                                                else
                                                {
                                                    #region Prepare the Model for Request

                                                    _mobileMoneyRequest.servicecategory = "francophone";
                                                    _mobileMoneyRequest.ServiceType = AggragatorServiceType.CREDIT;
                                                    _mobileMoneyRequest.Channel = request.channel;
                                                    _mobileMoneyRequest.Amount = Convert.ToString(Convert.ToInt32(_commission.TransactionAmount)); //Request.amount;
                                                                                                                                                   //var invoiceNumber = new ThirdPartyRepository().GetInvoiceNumber();
                                                    if (invoiceNumber != null)
                                                    {
                                                        _mobileMoneyRequest.TransactionId = invoiceNumber.InvoiceNumber;
                                                    }
                                                    //_MobileMoneyRequest.invoiceNo = Common.GetOtp();
                                                    _mobileMoneyRequest.Customer = customer;
                                                    if (request.ISD == "+225")
                                                    {
                                                        _mobileMoneyRequest.Country = "CI";
                                                        _mobileMoneyRequest.Customer = customer;
                                                    }
                                                    else if (request.ISD == "+221")
                                                    {
                                                        _mobileMoneyRequest.Country = "SN";
                                                        _mobileMoneyRequest.Customer = customer;
                                                    }
                                                    else if (request.ISD == "+226")
                                                    {
                                                        _mobileMoneyRequest.Country = "BF";
                                                        _mobileMoneyRequest.Customer = customer;
                                                    }
                                                    else if (request.ISD == "+223")
                                                    {
                                                        _mobileMoneyRequest.Country = "ML";
                                                        _mobileMoneyRequest.Customer = customer;
                                                    }
                                                    else
                                                    {
                                                        _mobileMoneyRequest.Country = "BJ";
                                                        _mobileMoneyRequest.Customer = customer;
                                                    }
                                                    var requ = new PayServicesMoneyAggregatoryRequest
                                                    {
                                                        ApiKey = ThirdPartyAggragatorSettings.ApiKey,
                                                        Amount = _commission.TransactionAmount.ToString(),
                                                        Customer = _mobileMoneyRequest.Customer,
                                                        TransactionId = _mobileMoneyRequest.TransactionId
                                                    };
                                                    _mobileMoneyRequest.ApiKey = ThirdPartyAggragatorSettings.ApiKey;
                                                    _mobileMoneyRequest.Signature = new CommonMethods().Sha256Hash(requ);

                                                    //  SHA2Hash
                                                    string RequestString = JsonConvert.SerializeObject(_mobileMoneyRequest);
                                                    #endregion

                                                }
                                                #endregion
                                                var req = JsonConvert.SerializeObject(_MobileMoneyRequest);

                                                if (WalletService.HttpVerbs.ToLower() == AggragatorServiceVerbs.HttpPostVerb.ToLower())
                                                {
                                                    var payData = Task.Run(() => commonApi.PayServices(req, apiUrl, username, password));
                                                    payData.Wait();
                                                    responseString = payData.Result.ToString();
                                                    //responseString = HttpPostUrlEncodedService(LogTransactionNameTypes.PayMoney, apiUrl, req, Request, subcategory.CategoryName);
                                                }
                                                else if (request.ISD == "+245" || request.ISD == "+227" || request.ISD == "+229" || (request.ISD == "+225" && WalletService.WalletServiceId == 142 || request.ServiceCategoryId == 8) || request.ISD == "+228")
                                                {


                                                    var invoiceGetProduct = await _masterDataRepository.GetInvoiceNumber();
                                                    var passwordHashedGetProduct = new CommonMethods().SHA1Hash("eazipayapixof1234");
                                                    string reqGetProduct = invoiceGetProduct.InvoiceNumber + passwordHashedGetProduct;

                                                    var hashedPassGetProduct = new CommonMethods().SHA1Hash(reqGetProduct);


                                                    var reqForPayment = new AirtomePaymentRequest();

                                                    reqForPayment.auth = new Auth
                                                    {
                                                        username = "eazipayapixof",
                                                        salt = invoiceGetProduct.InvoiceNumber,
                                                        password = hashedPassGetProduct,
                                                    };
                                                    reqForPayment.command = "execTransaction";
                                                    reqForPayment.version = 5;
                                                    reqForPayment.productId = (int)request.ProductId;
                                                    reqForPayment.amountOperator = Convert.ToDecimal(request.Amount);
                                                    reqForPayment.@operator = (int)request.OperatorId;
                                                    reqForPayment.userReference = invoiceGetProduct.AutoDigit;
                                                    reqForPayment.msisdn = customer;
                                                    reqForPayment.simulate = 0;

                                                    var jsonReq = JsonConvert.SerializeObject(reqForPayment);
                                                    apiUrl = ThirdPartyAggragatorSettings.AirtimeArtx;
                                                    var payData = await commonApi.PaymentAirtime(jsonReq, apiUrl);
                                                    responseString = payData;
                                                }
                                                else
                                                {
                                                    var payData = await commonApi.PayServices(req, apiUrl, username, password);
                                                    responseString = payData;
                                                    // responseString = HttpGetUrlEncodedService(LogTransactionNameTypes.PayMoney, apiUrl, req, Request, subcategory.CategoryName);                                                    
                                                }
                                                AddMoneyAggregatorResponse _responseModel = new AddMoneyAggregatorResponse();

                                                PayServicesResponseForServices payServices = new PayServicesResponseForServices();
                                                PayServicesResponseForServices dataSer = new PayServicesResponseForServices();
                                                AirtimePaymentResponse airtimePayment = new AirtimePaymentResponse();
                                                //  payServices.service_id= dataSer.
                                                LogTransactionTypes.Response.SaveTransactionLog(LogTransactionNameTypes.PayMoney + subcategory.CategoryName, responseString, "Aggregator Url : ");
                                                var errorResponse = "The remote server returned an error: (500) Internal Server Error.";
                                                if (!string.IsNullOrEmpty(responseString) && responseString != errorResponse)
                                                {
                                                    if (request.ISD == "+245" || request.ISD == "+227" || request.ISD == "+229" || (request.ISD == "+225" && WalletService.WalletServiceId == 142 || request.ServiceCategoryId == 8) || request.ISD == "+228")
                                                    {
                                                        airtimePayment = JsonConvert.DeserializeObject<AirtimePaymentResponse>(responseString);
                                                        if (airtimePayment.status.name.ToUpper() == "SUCCESSFUL")
                                                        {
                                                            _responseModel.StatusCode = AggregatorySTATUSCODES.SUCCESSFUL;
                                                            dataSer.gu_transaction_id = airtimePayment.result.userReference;
                                                            dataSer.status = "SUCCESSFUL";
                                                        }
                                                        else if (airtimePayment.status.name.ToUpper() == "PENDING")
                                                        {
                                                            _responseModel.StatusCode = AggregatorySTATUSCODES.PENDING;
                                                            dataSer.gu_transaction_id = airtimePayment.result.userReference;
                                                            dataSer.status = "PENDING";
                                                        }
                                                        else
                                                        {
                                                            _responseModel.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                            dataSer.gu_transaction_id = airtimePayment.result.userReference;
                                                            dataSer.status = "FAILED";
                                                        }
                                                    }
                                                    else
                                                    {
                                                        _responseModel = JsonConvert.DeserializeObject<AddMoneyAggregatorResponse>(responseString);
                                                        dataSer = JsonConvert.DeserializeObject<PayServicesResponseForServices>(responseString);

                                                        if (dataSer.status.ToUpper() == "PENDING")
                                                        {
                                                            _responseModel.StatusCode = AggregatorySTATUSCODES.PENDING;
                                                        }
                                                        else if (dataSer.status.ToUpper() == "FAILED")
                                                        {
                                                            _responseModel.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                        }
                                                        else
                                                        {
                                                            _responseModel.StatusCode = AggregatorySTATUSCODES.SUCCESSFUL;
                                                        }
                                                    }

                                                    if (_responseModel != null && !string.IsNullOrEmpty(_responseModel.StatusCode) && (_responseModel.StatusCode == AggregatorySTATUSCODES.SUCCESSFUL || _responseModel.StatusCode == AggregatorySTATUSCODES.PENDING || _responseModel.StatusCode == AggregatorySTATUSCODES.FAILED || _responseModel.StatusCode == AggregatorySTATUSCODES.EXCEPTION))
                                                    {
                                                        var _tranDate = DateTime.UtcNow;
                                                        _responseModel.FormatedTransactionDate = string.Format("{0:d}", DateTime.Now) + "" + string.Format("{0:T}", DateTime.Now);
                                                        if (subcategory.CategoryName == "ISP")
                                                        {
                                                            var renameCustomer = customer.Split(',');
                                                            if (renameCustomer != null && renameCustomer.Length > 0)
                                                            {
                                                                _responseModel.AccountNo = renameCustomer[0];
                                                                _responseModel.MobileNo = renameCustomer[0];
                                                            }
                                                        }
                                                        else
                                                        {
                                                            _responseModel.AccountNo = customer;
                                                            _responseModel.MobileNo = customer;
                                                        }
                                                        _responseModel.Amount = request.Amount;
                                                        _responseModel.TransactionDate = _tranDate;
                                                        _responseModel.CurrentBalance = sender.CurrentBalance;

                                                        var tran = new WalletTransaction();
                                                        tran.BeneficiaryName = request.BeneficiaryName;
                                                        tran.CreatedDate = _tranDate;
                                                        tran.UpdatedDate = _tranDate;
                                                        tran.IsAddDuringPay = false;
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


                                                        tran.AccountNo = customer;// string.Empty;                                                  
                                                        tran.BankTransactionId = string.Empty;
                                                        tran.IsBankTransaction = false;
                                                        tran.BankBranchCode = string.Empty;
                                                        if (dataSer.gu_transaction_id != null)
                                                        {
                                                            tran.TransactionId = dataSer.gu_transaction_id;
                                                        }
                                                        else
                                                        {
                                                            tran.TransactionId = _responseModel.TransactionId;
                                                        }
                                                        response.TransactionId = tran.TransactionId;
                                                        int _TransactionStatus = 0;
                                                        if (_responseModel.StatusCode == AggregatorySTATUSCODES.SUCCESSFUL)
                                                        {
                                                            _TransactionStatus = (int)TransactionStatus.Completed;

                                                            //-------------sending email after success transaction-----------------
                                                            try
                                                            {
                                                                string filename = CommonSetting.successfullTransaction;
                                                                var FirstName = AES256.Decrypt(sender.PrivateKey, sender.FirstName);
                                                                var LastName = AES256.Decrypt(sender.PrivateKey, sender.LastName);
                                                                var EmailId = AES256.Decrypt(sender.PrivateKey, sender.EmailId);
                                                                var body = _sendEmails.ReadEmailformats(filename);
                                                                body = body.Replace("$$FirstName$$", FirstName + " " + LastName);
                                                                body = body.Replace("$$DisplayContent$$", WalletService.ServiceName);
                                                                body = body.Replace("$$customer$$", request.customer);
                                                                body = body.Replace("$$amount$$", "XOF " + request.Amount);
                                                                body = body.Replace("$$ServiceTaxAmount$$", "XOF " + _commission.CommissionAmount);
                                                                body = body.Replace("$$AmountWithCommission$$", "XOF " + _commission.AmountWithCommission);
                                                                body = body.Replace("$$TransactionId$$", _responseModel.TransactionId);

                                                                var requ = new EmailModel
                                                                {
                                                                    TO = EmailId,
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
                                                        tran.InvoiceNo = string.Empty;
                                                        tran.Comments = request.Comment;
                                                        tran.MerchantCommissionId = _commission.MerchantCommissionId;
                                                        tran.MerchantCommissionAmount = Convert.ToString(_commission.MerchantCommissionAmount);
                                                        tran.CommisionId = _commission.CommissionId;
                                                        tran.CommisionAmount = Convert.ToString(_commission.CommissionAmount);
                                                        tran.CommisionPercent = _commission.CommissionAmount.ToString();
                                                        tran.TotalAmount = Convert.ToString(_commission.AmountWithCommission);

                                                        _responseModel.CurrentBalance = Convert.ToString(_commission.UpdatedCurrentBalance);
                                                        //   db.WalletTransactions.Add(tran);

                                                        if (tran.TransactionStatus == (int)TransactionStatus.Completed || tran.TransactionStatus == (int)TransactionStatus.Pending)
                                                        {
                                                            sender.CurrentBalance = Convert.ToString(_commission.UpdatedCurrentBalance);

                                                        }
                                                        if (tran.TransactionStatus == (int)TransactionStatus.Completed)
                                                        {
                                                            //save transaction detail                                                       
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
                                                        }
                                                        else
                                                        {
                                                            if (_responseModel.StatusCode == AggregatorySTATUSCODES.PENDING)
                                                            {
                                                                response.RstKey = 2;
                                                                response.Message = AggregatoryMESSAGE.PENDING;
                                                            }
                                                            else
                                                            {
                                                                response.RstKey = 3;
                                                                response.Message = AggregatoryMESSAGE.FAILED;
                                                            }
                                                        }
                                                        try
                                                        {
                                                            tran = await _airtimeRepository.AirtimeServices(tran);
                                                        }
                                                        catch (Exception ex)
                                                        {

                                                        }
                                                        //calling pay method insert data in Database
                                                        await _walletUserRepository.UpdateUserDetail(sender);
                                                    }
                                                    else if (_responseModel != null && !string.IsNullOrEmpty(_responseModel.StatusCode) && (_responseModel.StatusCode == AggregatorySTATUSCODES.FAILED))
                                                    {
                                                        // Response.Create(false, ResponseMessages.AGGREGATOR_FAILED_ERROR, _responseModel.StatusCode, new AddMoneyAggregatorResponse());
                                                    }
                                                    else if (_responseModel != null && !string.IsNullOrEmpty(_responseModel.StatusCode) && (_responseModel.StatusCode == AggregatorySTATUSCODES.EXCEPTION))
                                                    {
                                                        // Response.Create(false, ResponseMessages.AGGREGATOR_FAILED_EXCEPTION, _responseModel.StatusCode, new AddMoneyAggregatorResponse());
                                                    }
                                                    else
                                                    {
                                                        //  Response.Create(false, _responseModel.Message, _responseModel.StatusCode, new AddMoneyAggregatorResponse());
                                                    }
                                                }
                                                else
                                                {
                                                    if (responseString == errorResponse)
                                                    {
                                                        // Response.Create(false, ResponseMessages.TRANSACTION_ERROR, HttpStatusCode.ExpectationFailed, new AddMoneyAggregatorResponse());
                                                    }
                                                    else
                                                    {
                                                        // Response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.ExpectationFailed, new AddMoneyAggregatorResponse());
                                                    }
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
                        response.CurrentBalance = sender.CurrentBalance;
                        response.ToMobileNo = request.customer;
                        response.MobileNo = request.customer;

                        response.RstKey = 6;
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
            return response;
        }

        public async Task<AddMoneyAggregatorResponse> AirtimeServices(PayMoneyAggregatoryRequest request, long WalletUserId = 0)
        {
            string responseString = "";
            var response = new AddMoneyAggregatorResponse();
            string customer = request.customer;
            var _commissionRequest = new CalculateCommissionRequest();
            var _commission = new CalculateCommissionResponse();
            var transationInitiate = new TransactionInitiateRequest();
            var commonApi = new CommonApi();
            var reqForPayment = new AirtomePaymentRequest();
            //var userChargeBackresult = await _ChargeBackRepository.GetfreezeById(request.WalletUserId);

            var sender = await _walletUserRepository.GetUserDetailById(request.WalletUserId);

            //if (userChargeBackresult != null)
            //{
            //    decimal AmountLimit = decimal.Parse(userChargeBackresult[0].AmountLimit);
            //    decimal CuBal = decimal.Parse(sender.CurrentBalance);

            //    decimal cc = CuBal - AmountLimit; //here we - 
            //    decimal requestAmount = decimal.Parse(request.Amount);

            //    if (cc < requestAmount) //1000  <= 200
            //    {
            //        response.RstKey = 6;
            //        response.Status = (int)WalletTransactionStatus.FAILED;
            //        response.Message = AmountLimit + ResponseMessages.freeze;
            //        return response;

            //    }

            //}

            var data = await _walletUserRepository.GetCurrentUser(request.WalletUserId);
            var WalletService = await _masterDataRepository.GetWalletServicesByIdOrChannel(request.channel, request.ServiceCategoryId, request.ISD);
            var subcategory = await _masterDataRepository.GetWalletSubCategoriesById(request.ServiceCategoryId);
            //bool IsdocVerified = await _walletUserRepository.IsDocVerified(sender.WalletUserId, (int)sender.DocumetStatus);
            bool IsdocVerified = await _walletUserRepository.IsDocVerifiedMOMO((int)sender.DocumetStatus);
            var transactionLimit = await _payMoneyRepository.GetTransactionLimitForPayment(data.WalletUserId);
            int limit = transactionLimit != null ? Convert.ToInt32(transactionLimit.transactionlimit) : 0;
            var transactionHistory = await _payMoneyRepository.GetAllTransactionByDate(data.WalletUserId);
            int totalAmountTransfered = transactionHistory != null ? Convert.ToInt32(transactionHistory.totalAmount) : 0;

            var resultTL = await _transactionLimitAUService.CheckTransactionLimitAU(request.WalletUserId.ToString()); //check New TL

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
                                    if (transactionLimit == null || limit >= (Convert.ToDecimal(request.Amount) + totalAmountTransfered))
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
                                                decimal amountWithCommision = _commission.AmountWithCommission;
                                                decimal currentBalance = Convert.ToDecimal(sender.CurrentBalance);

                                                //chk new TL for all user
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
                                                            response.RstKey = 20;
                                                            response.Message = ResponseMessages.INVALID_txnAmountREQUEST;
                                                            return response;
                                                        }
                                                    }
                                                }


                                                if (currentBalance > 0 && currentBalance >= amountWithCommision && _commission.CurrentBalance > 0 && _commission.CurrentBalance >= amountWithCommision)
                                                {
                                                    if (request.ISD == "+245" || request.ISD == "+227" || request.ISD == "+229" || (request.ISD == "+225" && WalletService.WalletServiceId == 142 || request.ServiceCategoryId == 8) || request.ISD == "+228")
                                                    {
                                                        string isdCode = request.ISD.Trim('+');
                                                        customer = isdCode + request.customer;
                                                    }
                                                    #region Prepare the Model for Request
                                                    var _mobileMoneyRequest = new AddMobileMoneyAggregatoryRequest();
                                                    var _MobileMoneyRequest = new PayServicesRequestForServices();
                                                    var invoiceNumber = await _masterDataRepository.GetInvoiceNumber();
                                                    string transactionInitiate = string.Empty;
                                                    //var invoiceNumber = new ThirdPartyRepository().GetInvoiceNumber();
                                                    if (invoiceNumber != null)
                                                    {
                                                        _MobileMoneyRequest.partner_transaction_id = invoiceNumber.InvoiceNumber;
                                                    }
                                                    _MobileMoneyRequest.recipient_phone_number = customer;//Request.customer;
                                                    _MobileMoneyRequest.amount = Convert.ToDecimal(request.Amount);

                                                    string apiUrl = "";
                                                    string username = "";
                                                    string password = "";

                                                    if (WalletService.WalletServiceId == 28 || WalletService.WalletServiceId == 31 || WalletService.WalletServiceId == 61)
                                                    {
                                                        //username = AppSetting.Username_Ci;
                                                        //password = AppSetting.Password_Ci;
                                                        username = "BF5C0D75E110EDA801651D948BA1E699D707421336CA0B141001F2986ABC54CB";
                                                        password = "61874536F5E32D89FD82C5209E548BB181E8474F8949016CB7DBD1072A82EB66";
                                                        if (WalletService.WalletServiceId == 28)
                                                        {
                                                            // _MobileMoneyRequest.service_id =AppSetting.ServiceId_MTN;
                                                            _MobileMoneyRequest.service_id = CommonSetting.ServiceId_MTN;
                                                        }
                                                        else if (WalletService.WalletServiceId == 31)
                                                        {
                                                            _MobileMoneyRequest.service_id = CommonSetting.ServiceId_Moov;
                                                        }
                                                        else if (WalletService.WalletServiceId == 61)
                                                        {
                                                            _MobileMoneyRequest.service_id = CommonSetting.ServiceId_Orange;
                                                        }
                                                        _MobileMoneyRequest.call_back_url = ThirdPartyAggragatorSettings.callBackUrl;
                                                        _MobileMoneyRequest.login_api = ThirdPartyAggragatorSettings.login_api;
                                                        _MobileMoneyRequest.password_api = ThirdPartyAggragatorSettings.password_api;
                                                        _MobileMoneyRequest.partner_id = ThirdPartyAggragatorSettings.partner_id;
                                                        apiUrl = ThirdPartyAggragatorSettings.PayMoneyUrl;
                                                    }
                                                    else if (WalletService.WalletServiceId == 29 || WalletService.WalletServiceId == 30 || WalletService.WalletServiceId == 120)
                                                    {
                                                        username = "CA6036929ECEF298A20161C96FFE7E4E87422A85323A22DF3ED9D66129961B92";
                                                        password = "0C5F49EA753B76E288F130B33E3E76C00CF86D516CF05437F351988014546C38";
                                                        if (WalletService.WalletServiceId == 29)
                                                        {
                                                            _MobileMoneyRequest.service_id = "BF_AIRTIME_TELMOB";
                                                            _MobileMoneyRequest.call_back_url = ThirdPartyAggragatorSettings.callBackUrl;
                                                            _MobileMoneyRequest.login_api = ThirdPartyAggragatorSettings.login_api_BF;
                                                            _MobileMoneyRequest.password_api = ThirdPartyAggragatorSettings.password_api_BF;
                                                            _MobileMoneyRequest.partner_id = ThirdPartyAggragatorSettings.partner_id_BF;
                                                        }
                                                        else if (WalletService.WalletServiceId == 30)
                                                        {

                                                            _MobileMoneyRequest.service_id = "BF_AIRTIME_TELECEL";
                                                            _MobileMoneyRequest.call_back_url = ThirdPartyAggragatorSettings.callBackUrl;
                                                            _MobileMoneyRequest.login_api = ThirdPartyAggragatorSettings.login_api_BF;
                                                            _MobileMoneyRequest.password_api = ThirdPartyAggragatorSettings.password_api_BF;
                                                            _MobileMoneyRequest.partner_id = ThirdPartyAggragatorSettings.partner_id_BF;
                                                        }
                                                        else
                                                        {

                                                            _MobileMoneyRequest.service_id = "BF_AIRTIME_ORANGE";
                                                            _MobileMoneyRequest.call_back_url = ThirdPartyAggragatorSettings.callBackUrl;
                                                            _MobileMoneyRequest.login_api = ThirdPartyAggragatorSettings.login_api_BF;
                                                            _MobileMoneyRequest.password_api = ThirdPartyAggragatorSettings.password_api_BF;
                                                            _MobileMoneyRequest.partner_id = ThirdPartyAggragatorSettings.partner_id_BF;
                                                        }
                                                        apiUrl = ThirdPartyAggragatorSettings.PayMoneyUrl_BF;
                                                    }
                                                    else if (WalletService.WalletServiceId == 121 || WalletService.WalletServiceId == 122 || WalletService.WalletServiceId == 123)
                                                    {
                                                        username = "5EF52919A5DDB84EC0B89D7738F74F2DF459DD8BC0E1271DB171B6993F90920F";
                                                        password = "2399B6FE625E6245C3027AB89613B1A911D7D239525BDE6DEEDF4DC199EED5C4";
                                                        if (WalletService.WalletServiceId == 121)
                                                        {
                                                            _MobileMoneyRequest.service_id = "AIRTIMEORANGEMALI";
                                                            _MobileMoneyRequest.call_back_url = ThirdPartyAggragatorSettings.callBackUrl;
                                                            _MobileMoneyRequest.login_api = ThirdPartyAggragatorSettings.login_api_ML;
                                                            _MobileMoneyRequest.password_api = ThirdPartyAggragatorSettings.password_api_ML;
                                                            _MobileMoneyRequest.partner_id = ThirdPartyAggragatorSettings.partner_id_ML;
                                                        }
                                                        else if (WalletService.WalletServiceId == 122)
                                                        {
                                                            _MobileMoneyRequest.service_id = "ML_AIRTIME_TELECEL";
                                                            _MobileMoneyRequest.call_back_url = ThirdPartyAggragatorSettings.callBackUrl;
                                                            _MobileMoneyRequest.login_api = ThirdPartyAggragatorSettings.login_api_ML;
                                                            _MobileMoneyRequest.password_api = ThirdPartyAggragatorSettings.password_api_ML;
                                                            _MobileMoneyRequest.partner_id = ThirdPartyAggragatorSettings.partner_id_ML;
                                                        }
                                                        else
                                                        {
                                                            _MobileMoneyRequest.service_id = ThirdPartyAggragatorSettings.service_id_ML;
                                                            _MobileMoneyRequest.call_back_url = ThirdPartyAggragatorSettings.callBackUrl;
                                                            _MobileMoneyRequest.login_api = ThirdPartyAggragatorSettings.login_api_ML;
                                                            _MobileMoneyRequest.password_api = ThirdPartyAggragatorSettings.password_api_ML;
                                                            _MobileMoneyRequest.partner_id = ThirdPartyAggragatorSettings.partner_id_ML;
                                                        }

                                                        apiUrl = ThirdPartyAggragatorSettings.PayMoneyUrl_ML;
                                                    }
                                                    else if (WalletService.WalletServiceId == 126 || WalletService.WalletServiceId == 127 || WalletService.WalletServiceId == 128)
                                                    {
                                                        username = "94E212F6CFF7334FF371B91612DA7AFEE1832C2972C81D6FFAD6C212ED14C1F5";
                                                        password = "B538B1B9407990154271132468CC06FF80D5894DA3ADE0ABDED44CDF7B992C8A";
                                                        if (WalletService.WalletServiceId == 126)
                                                        {
                                                            _MobileMoneyRequest.service_id = "AIRTIMEORANGE";
                                                        }
                                                        else if (WalletService.WalletServiceId == 127)
                                                        {
                                                            _MobileMoneyRequest.service_id = "AIRTIMEEXPRESSO";
                                                        }
                                                        else
                                                        {
                                                            _MobileMoneyRequest.service_id = "AIRTIMETIGO";
                                                        }

                                                        _MobileMoneyRequest.call_back_url = ThirdPartyAggragatorSettings.callBackUrl;
                                                        _MobileMoneyRequest.login_api = ThirdPartyAggragatorSettings.login_api_SNG;
                                                        _MobileMoneyRequest.password_api = ThirdPartyAggragatorSettings.password_api_SNG;
                                                        _MobileMoneyRequest.partner_id = ThirdPartyAggragatorSettings.partner_id_SNG;
                                                        apiUrl = ThirdPartyAggragatorSettings.PayMoneyUrl_SNG;
                                                    }
                                                    else if ((request.ISD == "+245" && request.ServiceCategoryId == 8) || (request.ISD == "+227" && request.ServiceCategoryId == 8) || (request.ISD == "+229" && request.ServiceCategoryId == 8) || (request.ISD == "+225" && WalletService.WalletServiceId == 142 || request.ServiceCategoryId == 8) || (request.ISD == "+228" && request.ServiceCategoryId == 8) || (request.ISD == "+229" && request.ServiceCategoryId == 7) || (request.ISD == "+227" && request.ServiceCategoryId == 7) || (request.ISD == "+228" && request.ServiceCategoryId == 7))
                                                    {
                                                        /////   

                                                        //niger,togo & benin akll operator service & Ivory :- Ivory Coast Orange operator service here
                                                        var invoiceGetProduct = await _masterDataRepository.GetInvoiceNumber();
                                                        var passwordHashedGetProduct = new CommonMethods().SHA1Hash("eazipayapixof1234");
                                                        string reqGetProduct = invoiceGetProduct.InvoiceNumber + passwordHashedGetProduct;

                                                        var hashedPassGetProduct = new CommonMethods().SHA1Hash(reqGetProduct);

                                                        reqForPayment.auth = new Auth
                                                        {
                                                            username = "eazipayapixof",
                                                            salt = invoiceGetProduct.InvoiceNumber,
                                                            password = hashedPassGetProduct,
                                                        };
                                                        reqForPayment.command = "execTransaction";
                                                        reqForPayment.version = 5;
                                                        reqForPayment.productId = (int)request.ProductId;
                                                        reqForPayment.amountOperator = Convert.ToDecimal(request.Amount);
                                                        reqForPayment.@operator = (int)request.OperatorId;
                                                        reqForPayment.userReference = invoiceGetProduct.AutoDigit;
                                                        reqForPayment.msisdn = customer;
                                                        reqForPayment.simulate = 0;


                                                    }
                                                    else
                                                    {
                                                        #region Prepare the Model for Request

                                                        _mobileMoneyRequest.servicecategory = "francophone";
                                                        _mobileMoneyRequest.ServiceType = AggragatorServiceType.CREDIT;
                                                        _mobileMoneyRequest.Channel = request.channel;
                                                        _mobileMoneyRequest.Amount = Convert.ToString(Convert.ToInt32(_commission.TransactionAmount)); //Request.amount;
                                                                                                                                                       //var invoiceNumber = new ThirdPartyRepository().GetInvoiceNumber();
                                                        if (invoiceNumber != null)
                                                        {
                                                            _mobileMoneyRequest.TransactionId = invoiceNumber.InvoiceNumber;
                                                        }
                                                        //_MobileMoneyRequest.invoiceNo = Common.GetOtp();
                                                        _mobileMoneyRequest.Customer = customer;
                                                        if (request.ISD == "+225")
                                                        {
                                                            _mobileMoneyRequest.Country = "CI";
                                                            _mobileMoneyRequest.Customer = customer;
                                                        }
                                                        else if (request.ISD == "+221")
                                                        {
                                                            _mobileMoneyRequest.Country = "SN";
                                                            _mobileMoneyRequest.Customer = customer;
                                                        }
                                                        else if (request.ISD == "+226")
                                                        {
                                                            _mobileMoneyRequest.Country = "BF";
                                                            _mobileMoneyRequest.Customer = customer;
                                                        }
                                                        else if (request.ISD == "+223")
                                                        {
                                                            _mobileMoneyRequest.Country = "ML";
                                                            _mobileMoneyRequest.Customer = customer;
                                                        }
                                                        else
                                                        {
                                                            _mobileMoneyRequest.Country = "BJ";
                                                            _mobileMoneyRequest.Customer = customer;
                                                        }
                                                        var requ = new PayServicesMoneyAggregatoryRequest
                                                        {
                                                            ApiKey = ThirdPartyAggragatorSettings.ApiKey,
                                                            Amount = _commission.TransactionAmount.ToString(),
                                                            Customer = _mobileMoneyRequest.Customer,
                                                            TransactionId = _mobileMoneyRequest.TransactionId
                                                        };
                                                        _mobileMoneyRequest.ApiKey = ThirdPartyAggragatorSettings.ApiKey;
                                                        _mobileMoneyRequest.Signature = new CommonMethods().Sha256Hash(requ);

                                                        //  SHA2Hash
                                                        transactionInitiate = JsonConvert.SerializeObject(_mobileMoneyRequest);
                                                        #endregion

                                                    }
                                                    #endregion
                                                    if ((request.ISD == "+245" && request.ServiceCategoryId == 8) || (request.ISD == "+227" && request.ServiceCategoryId == 8) || (request.ISD == "+229" && request.ServiceCategoryId == 8) || (request.ISD == "+225" && WalletService.WalletServiceId == 142 || request.ServiceCategoryId == 8) || (request.ISD == "+228" && request.ServiceCategoryId == 8) || (request.ISD == "+229" && request.ServiceCategoryId == 7) || (request.ISD == "+227" && request.ServiceCategoryId == 7) || (request.ISD == "+228" && request.ServiceCategoryId == 7))
                                                    {////
                                                     //niger,togo & benin akll operator service & Ivory :- Ivory Coast Orange operator service here 
                                                        transactionInitiate = JsonConvert.SerializeObject(reqForPayment);
                                                    }
                                                    else
                                                    {
                                                        transactionInitiate = JsonConvert.SerializeObject(_MobileMoneyRequest);
                                                    }

                                                    // transactionInitiate = req;
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
                                                    transationInitiate.JsonRequest = transactionInitiate;
                                                    transationInitiate.JsonResponse = "";
                                                    transationInitiate = await _airtimeRepository.SaveTransactionInitiateRequest(transationInitiate);


                                                    //Update user's currentbalance amount from wallet
                                                    data.CurrentBalance = Convert.ToString(_commission.UpdatedCurrentBalance);
                                                    //calling pay method insert data in Database
                                                    await _walletUserRepository.UpdateUserDetail(data);
                                                    #endregion

                                                    if (WalletService.HttpVerbs.ToLower() == AggragatorServiceVerbs.HttpPostVerb.ToLower())
                                                    {
                                                        var payData = Task.Run(() => commonApi.PayServices(transactionInitiate, apiUrl, username, password));
                                                        payData.Wait();
                                                        responseString = payData.Result.ToString();
                                                        //responseString = HttpPostUrlEncodedService(LogTransactionNameTypes.PayMoney, apiUrl, req, Request, subcategory.CategoryName);
                                                    }
                                                    else if ((request.ISD == "+245" && request.ServiceCategoryId == 8) || (request.ISD == "+227" && request.ServiceCategoryId == 8) || (request.ISD == "+229" && request.ServiceCategoryId == 8) || (request.ISD == "+225" && WalletService.WalletServiceId == 142 || request.ServiceCategoryId == 8) || (request.ISD == "+228" && request.ServiceCategoryId == 8) || (request.ISD == "+229" && request.ServiceCategoryId == 7) || (request.ISD == "+227" && request.ServiceCategoryId == 7) || (request.ISD == "+228" && request.ServiceCategoryId == 7))
                                                    {////
                                                     //niger,togo & benin akll operator service & Ivory :- Ivory Coast Orange operator service here to hit operator api url
                                                        var jsonReq = JsonConvert.SerializeObject(reqForPayment);
                                                        apiUrl = ThirdPartyAggragatorSettings.AirtimeArtx;
                                                        var payData = await commonApi.PaymentAirtime(jsonReq, apiUrl);
                                                        responseString = payData;
                                                    }
                                                    else
                                                    {
                                                        var payData = await commonApi.PayServices(transactionInitiate, apiUrl, username, password);
                                                        responseString = payData;
                                                        // responseString = HttpGetUrlEncodedService(LogTransactionNameTypes.PayMoney, apiUrl, req, Request, subcategory.CategoryName);                                                    
                                                    }

                                                    AddMoneyAggregatorResponse _responseModel = new AddMoneyAggregatorResponse();

                                                    PayServicesResponseForServices payServices = new PayServicesResponseForServices();
                                                    PayServicesResponseForServices dataSer = new PayServicesResponseForServices();
                                                    AirtimePaymentResponse airtimePayment = new AirtimePaymentResponse();

                                                    LogTransactionTypes.Response.SaveTransactionLog(LogTransactionNameTypes.PayMoney + subcategory.CategoryName, responseString, "Aggregator Url : ", sender.WalletUserId);
                                                    #region Belew code for update initial transaction response getting from third party
                                                    //Belew code for update initial transaction response getting from third party

                                                    var TransactionInitial = await _airtimeRepository.GetTransactionInitiateRequest(transationInitiate.Id);
                                                    TransactionInitial.JsonResponse = "Airtime Response" + responseString;
                                                    await _airtimeRepository.UpdateTransactionInitiateRequest(TransactionInitial);
                                                    #endregion
                                                    // responseString = "{\"status\":{\"id\":9,\"name\":\"Transaction is pending\",\"type\":1,\"typeName\":\"Pending\"},\"command\":\"execTransaction\",\"timestamp\":1598276842,\"reference\":1739518307,\"result\":{\"id\":7705694327,\"operator\":{\"id\":\"8\",\"name\":\"Benin MTN\",\"reference\":\"5f43c4da70213500044497ff\"},\"country\":{\"id\":\"BJ\",\"name\":\"Benin\"},\"amount\":{\"operator\":\"200.00\",\"user\":\"200.00\"},\"currency\":{\"user\":\"XOF\",\"operator\":\"XOF\"},\"productId\":\"17\",\"productType\":\"1\",\"simulation\":false,\"userReference\":\"015479\",\"msisdn\":\"22969055515\"}}";
                                                    //responseString = "{\"status\":{\"id\":0,\"name\":\"Successful\",\"type\":0,\"typeName\":\"Success\"},\"command\":\"execTransaction\",\"timestamp\":1592805568,\"reference\":1938877568,\"result\":{\"id\":7991690725,\"operator\":{\"id\":\"142\",\"name\":\"Ivory Coast Orange\",\"reference\":\"933609675\"},\"country\":{\"id\":\"CI\",\"name\":\"Ivory Coast\"},\"amount\":{\"operator\":\"1950.00\",\"user\":\"1950.00\"},\"currency\":{\"user\":\"XOF\",\"operator\":\"XOF\"},\"productId\":\"4516\",\"productType\":\"1\",\"simulation\":false,\"userReference\":\"012161\",\"msisdn\":\"22567500725\",\"balance\":{\"initial\":\"992912.60\",\"transaction\":\"1950.00\",\"commission\":\"97.50\",\"commissionPercentage\":\"5.00\",\"final\":\"991060.10\",\"currency\":\"XOF\"}}}";
                                                    if (!string.IsNullOrEmpty(responseString))
                                                    {
                                                        //171 Niger Airtel  add  17-12-20 & togo=141
                                                        if (WalletService.WalletServiceId == 171 || WalletService.WalletServiceId == 142 || WalletService.WalletServiceId == 61 || WalletService.WalletServiceId == 141 || WalletService.WalletServiceId == 140 || WalletService.WalletServiceId == 139 || WalletService.WalletServiceId == 138 || WalletService.WalletServiceId == 136 || WalletService.WalletServiceId == 137)
                                                        {
                                                            var airtimeFailed = JsonConvert.DeserializeObject<dynamic>(responseString);
                                                            //This condition for failure on sochitel with insufficent fund on thirdparty side
                                                            if (airtimeFailed.status.typeName != "Failure")
                                                            {
                                                                //responseString = "{\"status\":{\"id\":0,\"name\":\"Successful\",\"type\":0,\"typeName\":\"Success\"},\"command\":\"execTransaction\",\"timestamp\":1591970488,\"reference\":5749340673,\"result\":{\"id\":7456452105,\"operator\":{\"id\":\"8\",\"name\":\"Benin MTN\",\"reference\":\"2020061215012816201461425\"},\"country\":{\"id\":\"BJ\",\"name\":\"Benin\"},\"amount\":{\"operator\":\"200.00\",\"user\":\"200.00\"},\"currency\":{\"user\":\"XOF\",\"operator\":\"XOF\"},\"productId\":\"17\",\"productType\":\"1\",\"simulation\":false,\"userReference\":\"011620\",\"msisdn\":\"22969364393\",\"balance\":{\"initial\":\"549.60\",\"transaction\":\"200.00\",\"commission\":\"4.00\",\"commissionPercentage\":\"2.00\",\"final\":\"353.60\",\"currency\":\"XOF\"}}}";
                                                                airtimePayment = JsonConvert.DeserializeObject<AirtimePaymentResponse>(responseString);
                                                                if (airtimePayment.status.name.ToUpper() == "SUCCESSFUL")
                                                                {
                                                                    _responseModel.StatusCode = AggregatorySTATUSCODES.SUCCESSFUL;
                                                                    dataSer.gu_transaction_id = airtimePayment.result.userReference;
                                                                    dataSer.status = "SUCCESSFUL";
                                                                }
                                                                else if (airtimePayment.status.name.ToUpper() == "PENDING")
                                                                {
                                                                    _responseModel.StatusCode = AggregatorySTATUSCODES.PENDING;
                                                                    dataSer.gu_transaction_id = airtimePayment.result.userReference;
                                                                    dataSer.status = "PENDING";
                                                                }
                                                                else if (airtimePayment.status.typeName.ToUpper() == "PENDING")
                                                                {
                                                                    _responseModel.StatusCode = AggregatorySTATUSCODES.PENDING;
                                                                    dataSer.gu_transaction_id = airtimePayment.result.userReference;
                                                                    dataSer.status = "PENDING";
                                                                }
                                                                else
                                                                {
                                                                    transationInitiate = await _airtimeRepository.GetTransactionInitiateRequest(transationInitiate.Id);
                                                                    //calling pay method insert data in Database
                                                                    var refundAmt = Convert.ToDecimal(data.CurrentBalance) + _commission.AmountWithCommission;
                                                                    data.CurrentBalance = Convert.ToString(refundAmt);
                                                                    transationInitiate.AfterTransactionBalance = data.CurrentBalance;
                                                                    transationInitiate.ReceiverCurrentBalance = data.CurrentBalance;

                                                                    await _airtimeRepository.UpdateTransactionInitiateRequest(transationInitiate);
                                                                    await _walletUserRepository.UpdateUserDetail(data);

                                                                    _responseModel.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                                    dataSer.gu_transaction_id = airtimePayment.result.userReference;
                                                                    dataSer.status = "FAILED";
                                                                }
                                                            }
                                                            else
                                                            {
                                                                transationInitiate = await _airtimeRepository.GetTransactionInitiateRequest(transationInitiate.Id);
                                                                //calling pay method insert data in Database
                                                                var refundAmt = Convert.ToDecimal(data.CurrentBalance) + _commission.AmountWithCommission;
                                                                data.CurrentBalance = Convert.ToString(refundAmt);
                                                                transationInitiate.AfterTransactionBalance = data.CurrentBalance;
                                                                transationInitiate.ReceiverCurrentBalance = data.CurrentBalance;

                                                                await _airtimeRepository.UpdateTransactionInitiateRequest(transationInitiate);
                                                                await _walletUserRepository.UpdateUserDetail(data);
                                                                _responseModel.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                                dataSer.gu_transaction_id = airtimeFailed.reference;
                                                                dataSer.status = "FAILED";
                                                            }
                                                        }
                                                        //if (request.ISD == "+245" || request.ISD == "+227" || request.ISD == "+229" || (request.ISD == "+225" && WalletService.WalletServiceId == 142 || request.ServiceCategoryId == 8) || request.ISD == "+228")
                                                        //{
                                                        //    var airtimeFailed = JsonConvert.DeserializeObject<dynamic>(responseString);
                                                        //    //This condition for failure on sochitel with insufficent fund on thirdparty side
                                                        //    if (airtimeFailed.status.typeName != "Failure")
                                                        //    {
                                                        //        // responseString = "{\"status\":{\"id\":0,\"name\":\"Successful\",\"type\":0,\"typeName\":\"Success\"},\"command\":\"execTransaction\",\"timestamp\":1591968364,\"reference\":5756510650,\"result\":{\"id\":7225847919,\"operator\":{\"id\":\"8\",\"name\":\"Benin MTN\",\"reference\":\"2020061214260383001446819\"},\"country\":{\"id\":\"BJ\",\"name\":\"Benin\"},\"amount\":{\"operator\":\"200.00\",\"user\":\"200.00\"},\"currency\":{\"user\":\"XOF\",\"operator\":\"XOF\"},\"productId\":\"17\",\"productType\":\"1\",\"simulation\":false,\"userReference\":\"011609\",\"msisdn\":\"22969364393\",\"balance\":{\"initial\":\"745.60\",\"transaction\":\"200.00\",\"commission\":\"4.00\",\"commissionPercentage\":\"2.00\",\"final\":\"549.60\",\"currency\":\"XOF\"}}}";
                                                        //        airtimePayment = JsonConvert.DeserializeObject<AirtimePaymentResponse>(responseString);
                                                        //        if (airtimePayment.status.name.ToUpper() == "SUCCESSFUL")
                                                        //        {
                                                        //            _responseModel.StatusCode = AggregatorySTATUSCODES.SUCCESSFUL;
                                                        //            dataSer.gu_transaction_id = airtimePayment.result.userReference;
                                                        //            dataSer.status = "SUCCESSFUL";
                                                        //        }
                                                        //        else if (airtimePayment.status.name.ToUpper() == "PENDING")
                                                        //        {
                                                        //            _responseModel.StatusCode = AggregatorySTATUSCODES.PENDING;
                                                        //            dataSer.gu_transaction_id = airtimePayment.result.userReference;
                                                        //            dataSer.status = "PENDING";
                                                        //        }
                                                        //        else
                                                        //        {
                                                        //            _responseModel.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                        //            dataSer.gu_transaction_id = airtimePayment.result.userReference;
                                                        //            dataSer.status = "FAILED";
                                                        //        }
                                                        //    }
                                                        //    else
                                                        //    {
                                                        //        _responseModel.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                        //        dataSer.gu_transaction_id = airtimeFailed.reference;
                                                        //        dataSer.status = "FAILED";
                                                        //    }
                                                        //}
                                                        else
                                                        {
                                                            _responseModel = JsonConvert.DeserializeObject<AddMoneyAggregatorResponse>(responseString);
                                                            dataSer = JsonConvert.DeserializeObject<PayServicesResponseForServices>(responseString);

                                                            if (dataSer.status.ToUpper() == "PENDING")
                                                            {
                                                                _responseModel.StatusCode = AggregatorySTATUSCODES.PENDING;
                                                                _responseModel.TransactionId = _responseModel.gu_transaction_id;
                                                            }
                                                            else if (dataSer.status.ToUpper() == "FAILED")
                                                            {
                                                                transationInitiate = await _airtimeRepository.GetTransactionInitiateRequest(transationInitiate.Id);
                                                                //calling pay method insert data in Database
                                                                var refundAmt = Convert.ToDecimal(data.CurrentBalance) + _commission.AmountWithCommission;
                                                                data.CurrentBalance = Convert.ToString(refundAmt);
                                                                transationInitiate.AfterTransactionBalance = data.CurrentBalance;
                                                                transationInitiate.ReceiverCurrentBalance = data.CurrentBalance;

                                                                await _airtimeRepository.UpdateTransactionInitiateRequest(transationInitiate);
                                                                await _walletUserRepository.UpdateUserDetail(data);

                                                                _responseModel.StatusCode = AggregatorySTATUSCODES.FAILED;
                                                                _responseModel.TransactionId = _responseModel.gu_transaction_id;
                                                            }
                                                            else
                                                            {
                                                                _responseModel.StatusCode = AggregatorySTATUSCODES.SUCCESSFUL;
                                                                _responseModel.TransactionId = _responseModel.gu_transaction_id;
                                                            }
                                                        }

                                                        if (_responseModel != null && !string.IsNullOrEmpty(_responseModel.StatusCode) && (_responseModel.StatusCode == AggregatorySTATUSCODES.SUCCESSFUL || _responseModel.StatusCode == AggregatorySTATUSCODES.PENDING || _responseModel.StatusCode == AggregatorySTATUSCODES.FAILED || _responseModel.StatusCode == AggregatorySTATUSCODES.EXCEPTION))
                                                        {
                                                            var _tranDate = DateTime.UtcNow;
                                                            _responseModel.FormatedTransactionDate = string.Format("{0:d}", DateTime.Now) + "" + string.Format("{0:T}", DateTime.Now);
                                                            if (subcategory.CategoryName == "ISP")
                                                            {
                                                                var renameCustomer = customer.Split(',');
                                                                if (renameCustomer != null && renameCustomer.Length > 0)
                                                                {
                                                                    _responseModel.AccountNo = renameCustomer[0];
                                                                    _responseModel.MobileNo = renameCustomer[0];
                                                                }
                                                            }
                                                            else
                                                            {
                                                                _responseModel.AccountNo = customer;
                                                                _responseModel.MobileNo = customer;
                                                            }
                                                            _responseModel.Amount = request.Amount;
                                                            _responseModel.TransactionDate = _tranDate;
                                                            _responseModel.CurrentBalance = data.CurrentBalance;

                                                            var tran = new WalletTransaction();
                                                            tran.TransactionInitiateRequestId = transationInitiate.Id;
                                                            tran.BeneficiaryName = request.BeneficiaryName;
                                                            tran.CreatedDate = _tranDate;
                                                            tran.UpdatedDate = _tranDate;
                                                            tran.IsAddDuringPay = false;
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
                                                            tran.IsInitialTransction = false;
                                                            tran.AccountNo = request.customer;// string.Empty;                                                  
                                                            tran.BankTransactionId = string.Empty;
                                                            tran.IsBankTransaction = false;
                                                            tran.BankBranchCode = string.Empty;
                                                            if (dataSer.gu_transaction_id != null)
                                                            {
                                                                tran.TransactionId = dataSer.gu_transaction_id;
                                                            }
                                                            else
                                                            {
                                                                tran.TransactionId = _responseModel.TransactionId;
                                                            }
                                                            response.TransactionId = tran.TransactionId;
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

                                                                    var reqEmailModel = new EmailModel
                                                                    {
                                                                        TO = sender.EmailId,
                                                                        Subject = ResponseEmailMessage.PAYMENT_SUCCESS,
                                                                        Body = body
                                                                    };

                                                                    _sendEmails.SendEmail(reqEmailModel);
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
                                                            tran.Comments = request.Comment;
                                                            tran.MerchantCommissionId = _commission.MerchantCommissionId;
                                                            tran.MerchantCommissionAmount = Convert.ToString(_commission.MerchantCommissionAmount);
                                                            tran.CommisionId = _commission.CommissionId;
                                                            tran.CommisionAmount = Convert.ToString(_commission.CommissionAmount);
                                                            tran.CommisionPercent = _commission.CommissionAmount.ToString();
                                                            tran.TotalAmount = Convert.ToString(_commission.AmountWithCommission);
                                                            tran.IsdCode = request.IsdCode;
                                                            _responseModel.CurrentBalance = Convert.ToString(_commission.UpdatedCurrentBalance);
                                                            response.Amount = request.Amount;

                                                            if (tran.TransactionStatus == (int)TransactionStatus.Completed || tran.TransactionStatus == (int)TransactionStatus.Pending)
                                                            {
                                                                data.CurrentBalance = Convert.ToString(_commission.UpdatedCurrentBalance);

                                                            }
                                                            if (tran.TransactionStatus == (int)TransactionStatus.Completed)
                                                            {
                                                                //save transaction detail                                                       
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

                                                            }
                                                            else
                                                            {
                                                                if (_responseModel.StatusCode == AggregatorySTATUSCODES.PENDING)
                                                                {
                                                                    response.RstKey = 2;
                                                                    response.Message = AggregatoryMESSAGE.PENDING;
                                                                }
                                                                else
                                                                {
                                                                    response.RstKey = 3;
                                                                    response.Message = AggregatoryMESSAGE.FAILED;
                                                                }
                                                            }
                                                            try
                                                            {
                                                                tran = await _airtimeRepository.AirtimeServices(tran);
                                                            }
                                                            catch (Exception ex)
                                                            {

                                                            }
                                                            //calling pay method insert data in Database
                                                            //  await _walletUserRepository.UpdateUserDetail(data);
                                                        }
                                                        else if (_responseModel != null && !string.IsNullOrEmpty(_responseModel.StatusCode) && (_responseModel.StatusCode == AggregatorySTATUSCODES.FAILED))
                                                        {
                                                            // Response.Create(false, ResponseMessages.AGGREGATOR_FAILED_ERROR, _responseModel.StatusCode, new AddMoneyAggregatorResponse());
                                                        }
                                                        else if (_responseModel != null && !string.IsNullOrEmpty(_responseModel.StatusCode) && (_responseModel.StatusCode == AggregatorySTATUSCODES.EXCEPTION))
                                                        {
                                                            // Response.Create(false, ResponseMessages.AGGREGATOR_FAILED_EXCEPTION, _responseModel.StatusCode, new AddMoneyAggregatorResponse());
                                                        }
                                                        else
                                                        {
                                                            //  Response.Create(false, _responseModel.Message, _responseModel.StatusCode, new AddMoneyAggregatorResponse());
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var tran = new WalletTransaction();
                                                        tran.BeneficiaryName = request.BeneficiaryName;
                                                        tran.CreatedDate = DateTime.UtcNow;
                                                        tran.UpdatedDate = DateTime.UtcNow;
                                                        tran.IsAddDuringPay = false;
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
                                                        tran.Comments = request.Comment;
                                                        tran.MerchantCommissionId = _commission.MerchantCommissionId;
                                                        tran.MerchantCommissionAmount = Convert.ToString(_commission.MerchantCommissionAmount);
                                                        tran.CommisionId = _commission.CommissionId;
                                                        tran.CommisionAmount = Convert.ToString(_commission.CommissionAmount);
                                                        tran.CommisionPercent = _commission.CommissionAmount.ToString();
                                                        tran.TotalAmount = Convert.ToString(_commission.AmountWithCommission);
                                                        tran = await _airtimeRepository.AirtimeServices(tran);

                                                        response.RstKey = 6;
                                                        response.Message = ResponseMessages.TRANSACTION_NOT_DONE;
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
            response.AccountNo = request.customer;
            response.DocStatus = IsdocVerified;
            response.DocumetStatus = (int)sender.DocumetStatus;
            response.CurrentBalance = data.CurrentBalance;
            response.ToMobileNo = request.customer;
            response.MobileNo = request.customer;
            
            return response;
        }
    }
}
