using ezeePay.Utility.CommonClass;
using ezeePay.Utility.Enums;
using Ezipay.Database;
using Ezipay.Repository.BillPaymentRepository;
using Ezipay.Repository.CommisionRepo;
using Ezipay.Repository.MasterData;
using Ezipay.Repository.PayMoneyRepo;
using Ezipay.Repository.UserRepo;
using Ezipay.Service.CommonService;
using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.Utility.SendEmail;
using Ezipay.Utility.SendPush;
using Ezipay.ViewModel.AirtimeViewModel;
using Ezipay.ViewModel.BillViewModel;
using Ezipay.ViewModel.CommisionViewModel;
using Ezipay.ViewModel.SendPushViewModel;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Ezipay.Service.BillPaymentService
{
    public class BillPaymentServices : IBillPaymentService
    {
        private IBillPaymentRepository _billPaymentRepository;
        private IWalletUserRepository _walletUserRepository;
        private ISetCommisionRepository _setCommisionRepository;
        private IMasterDataRepository _masterDataRepository;
        private ISendPushNotification _sendPushNotification;
        private IPayMoneyRepository _payMoneyRepository;
        private ISendEmails _sendEmails;
        private ICommonServices _commonServices;

        public BillPaymentServices()
        {
            _billPaymentRepository = new BillsPaymentRepository();
            _walletUserRepository = new WalletUserRepository();
            _setCommisionRepository = new SetCommisionRepository();
            _masterDataRepository = new MasterDataRepository();
            _sendEmails = new SendEmails();
            _payMoneyRepository = new PayMoneyRepository();
            _sendPushNotification = new SendPushNotification();
            _commonServices = new CommonServices();
        }
        /// <summary>
        /// BillPaymentServicesAggregator
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="WalletUserId"></param>
        /// <returns></returns>
        public async Task<AddMoneyAggregatorResponse> BillPaymentServicesAggregator(BillPayMoneyAggregatoryRequest request, long WalletUserId = 0)
        {
            var response = new AddMoneyAggregatorResponse();
            string customer = request.customer;
            var _commissionRequest = new CalculateCommissionRequest();
            var _commission = new CalculateCommissionResponse();
            var commonApi = new CommonApi();
            var transationInitiate = new TransactionInitiateRequest();
            var sender = await _walletUserRepository.GetUserDetailById(request.WalletUserId);
            var data = await _walletUserRepository.GetCurrentUser(request.WalletUserId);
            var WalletService = await _masterDataRepository.GetWalletServicesByIdOrChannel(request.channel, request.ServiceCategoryId, request.ISD);
            var subcategory = await _masterDataRepository.GetWalletSubCategoriesById(request.ServiceCategoryId);
            //bool IsdocVerified = await _walletUserRepository.IsDocVerified(sender.WalletUserId, (int)sender.DocumetStatus);
            bool IsdocVerified = await _walletUserRepository.IsDocVerifiedMOMO((int)sender.DocumetStatus);
            var transactionLimit = await _payMoneyRepository.GetTransactionLimitForPayment(data.WalletUserId);
            int limit = transactionLimit != null ? Convert.ToInt32(transactionLimit.transactionlimit) : 0;
            var transactionHistory = await _payMoneyRepository.GetAllTransactionByDate(data.WalletUserId);
            int totalAmountTransfered = transactionHistory != null ? Convert.ToInt32(transactionHistory.totalAmount) : 0;

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
                                    if (transactionLimit == null || limit >= (Convert.ToDecimal(request.amount) + totalAmountTransfered))
                                    {
                                        if (sender != null && !string.IsNullOrEmpty(sender.MobileNo))
                                        {
                                            // var data = db.WalletUsers.Where(x => x.WalletUserId == sender.WalletUserId).FirstOrDefault();

                                            if (data != null)
                                            {
                                                if (subcategory.CategoryName == "ISP")
                                                {
                                                    if (request.channel.ToLower() == "surfline")
                                                    {
                                                        customer = "226" + customer;
                                                    }

                                                }
                                                if (!string.IsNullOrEmpty(data.CurrentBalance) && !data.CurrentBalance.IsZero())
                                                {
                                                    _commissionRequest.CurrentBalance = Convert.ToDecimal(data.CurrentBalance);
                                                    _commissionRequest.IsRoundOff = true;
                                                    if (WalletService.WalletServiceId == 129)
                                                    {
                                                        request.amount = request.amount + request.fees;
                                                    }
                                                    _commissionRequest.TransactionAmount = Convert.ToDecimal(request.amount);
                                                    _commissionRequest.WalletServiceId = WalletService.WalletServiceId;
                                                    _commission = await _setCommisionRepository.CalculateCommission(_commissionRequest);
                                                }
                                                else
                                                {
                                                    response.message = ResponseMessages.INSUFICIENT_BALANCE;
                                                }


                                                //if (_commission.CurrentBalance > 0 && _commission.CurrentBalance >= _commission.AmountWithCommission)
                                                //{
                                                decimal amountWithCommision = _commission.AmountWithCommission;
                                                decimal currentBalance = Convert.ToDecimal(data.CurrentBalance);
                                                if (currentBalance > 0 && currentBalance >= amountWithCommision && _commission.CurrentBalance > 0 && _commission.CurrentBalance >= amountWithCommision)
                                                {
                                                    #region Prepare the Model for Request
                                                    //MobileMoneyAggregatoryRequest _MobileMoneyRequest = new MobileMoneyAggregatoryRequest();
                                                    //_MobileMoneyRequest.serviceCategory = Request.serviceCategory;
                                                    //_MobileMoneyRequest.serviceType = AggragatorServiceType.CREDIT;
                                                    //_MobileMoneyRequest.channel = Request.channel;

                                                    BillPayServicesRequestForServices _MobileMoneyRequest = new BillPayServicesRequestForServices();
                                                    PrepaidBillPayServicesRequestForServices prepaidBillPayServices = new PrepaidBillPayServicesRequestForServices();
                                                    BillPayRequest billPayRequest = new BillPayRequest();
                                                    //_MobileMoneyRequest.apiKey = ThirdPartyAggragatorSettings.ApiKey;
                                                    //_MobileMoneyRequest.signature = new ThirdPartyRepository().MD5Hash(_MobileMoneyRequest);
                                                    //_MobileMoneyRequest.service_id = Request.channel;
                                                    var invoiceNumber = await _masterDataRepository.GetInvoiceNumber();
                                                    string apiUrl = "";
                                                    string username = "";
                                                    string password = "";
                                                    string req = "";
                                                    if (WalletService.WalletServiceId == 124 || WalletService.WalletServiceId == 125)
                                                    {
                                                        username = "94E212F6CFF7334FF371B91612DA7AFEE1832C2972C81D6FFAD6C212ED14C1F5";
                                                        password = "B538B1B9407990154271132468CC06FF80D5894DA3ADE0ABDED44CDF7B992C8A";


                                                        if (invoiceNumber != null)
                                                        {
                                                            _MobileMoneyRequest.partner_transaction_id = invoiceNumber.InvoiceNumber;
                                                        }
                                                        //Request.customer;
                                                        _MobileMoneyRequest.amount = Convert.ToDecimal(request.amount);
                                                        _MobileMoneyRequest.destinataire = customer;
                                                        if (WalletService.WalletServiceId == 124)
                                                        {
                                                            var waterBillReq = new BillPaymentRequestSDE
                                                            {
                                                                login_api = ThirdPartyAggragatorSettings.login_api_SNWater,
                                                                password_api = ThirdPartyAggragatorSettings.password_api_SNWater,
                                                                amount = Convert.ToDecimal(request.amount),
                                                                recipient_invoice_id = request.recipient_invoice_id,
                                                                recipient_id = request.customer,
                                                                customer_reference = request.customer_reference,
                                                                partner_transaction_id = invoiceNumber.InvoiceNumber,
                                                                partner_id = ThirdPartyAggragatorSettings.partner_id_SNWater,
                                                                service_id = "CASHINSDE",
                                                                callBackUrl = ThirdPartyAggragatorSettings.callBackUrl
                                                            };
                                                            //_MobileMoneyRequest.partner_transaction_id = ThirdPartyAggragatorSettings.partner_transaction_id_SNWater;
                                                            apiUrl = ThirdPartyAggragatorSettings.WaterBillPaymentUrl_SN;
                                                            _MobileMoneyRequest.call_back_url = ThirdPartyAggragatorSettings.callBackUrl;
                                                            req = JsonConvert.SerializeObject(waterBillReq);
                                                        }
                                                        else
                                                        {
                                                            _MobileMoneyRequest.login_api = ThirdPartyAggragatorSettings.login_api_SN;
                                                            _MobileMoneyRequest.password_api = ThirdPartyAggragatorSettings.password_api_SN;
                                                            _MobileMoneyRequest.partner_id = ThirdPartyAggragatorSettings.partner_id_SN;
                                                            _MobileMoneyRequest.recipient_id = request.policeNumber;
                                                            _MobileMoneyRequest.recipient_invoice_id = request.billNumber;
                                                            _MobileMoneyRequest.service_id = "CASHINSENELEC";
                                                            _MobileMoneyRequest.amount = Convert.ToDecimal(request.amount);
                                                            apiUrl = ThirdPartyAggragatorSettings.ElectricityBillPaymentUrl_SEN;
                                                            _MobileMoneyRequest.partner_transaction_id = ThirdPartyAggragatorSettings.partner_transaction_id;
                                                            _MobileMoneyRequest.call_back_url = ThirdPartyAggragatorSettings.callBackUrl;
                                                            req = JsonConvert.SerializeObject(_MobileMoneyRequest);
                                                        }

                                                        //apiUrl = ThirdPartyAggragatorSettings.ElectricityBillPaymentUrl_SN;
                                                    }
                                                    else if (WalletService.WalletServiceId == 129)
                                                    {
                                                        username = ThirdPartyAggragatorSettings.username_SN;
                                                        password = ThirdPartyAggragatorSettings.password_SN;
                                                        prepaidBillPayServices.partnerTransactionId = invoiceNumber.InvoiceNumber;
                                                        prepaidBillPayServices.recipientId = customer;
                                                        prepaidBillPayServices.loginApi = ThirdPartyAggragatorSettings.login_api_SN_prepaid;
                                                        prepaidBillPayServices.passwordApi = ThirdPartyAggragatorSettings.password_api_SN_prepaid;
                                                        prepaidBillPayServices.partnerId = ThirdPartyAggragatorSettings.partner_id_SN_prepaid;
                                                        prepaidBillPayServices.amount = Convert.ToDecimal(request.amount);
                                                        prepaidBillPayServices.fees = request.fees;
                                                        prepaidBillPayServices.meterNo = request.policeNumber;
                                                        prepaidBillPayServices.serviceId = ThirdPartyAggragatorSettings.service_id_SN_prepaid;
                                                        apiUrl = ThirdPartyAggragatorSettings.PrepaidPaymentUrl_SN;
                                                        // _MobileMoneyRequest.partner_transaction_id =Request.policeNumber;
                                                        prepaidBillPayServices.callBackUrl = ThirdPartyAggragatorSettings.callBackUrl;
                                                        req = JsonConvert.SerializeObject(prepaidBillPayServices);
                                                    }
                                                    else if (WalletService.WalletServiceId == 132)
                                                    {
                                                        // var r=  GetBillPaymentServicesAggregator().result.ResponseString;
                                                        username = ThirdPartyAggragatorSettings.username_CIE;
                                                        password = ThirdPartyAggragatorSettings.password_CIE;
                                                        billPayRequest.partnerId = ThirdPartyAggragatorSettings.partnerId;
                                                        billPayRequest.loginApi = ThirdPartyAggragatorSettings.loginApi;
                                                        billPayRequest.passwordApi = ThirdPartyAggragatorSettings.passwordApi;
                                                        billPayRequest.telephone = request.customer;
                                                        billPayRequest.montant = request.amount;
                                                        billPayRequest.partnerTransactionId = invoiceNumber.InvoiceNumber;
                                                        billPayRequest.dateLimite = request.dateLimite;
                                                        billPayRequest.codeExpiration = request.codeExpiration;
                                                        billPayRequest.merchant = ThirdPartyAggragatorSettings.facturier;
                                                        billPayRequest.totAmount = request.amount;
                                                        billPayRequest.typeFacture = request.typeFacture;
                                                        billPayRequest.heureEnreg = request.heureEnreg;
                                                        billPayRequest.refBranch = request.refBranch;
                                                        billPayRequest.numFacture = request.numFacture;
                                                        billPayRequest.idAbonnement = request.idAbonnement;
                                                        billPayRequest.dateEnreg = request.dateEnreg;
                                                        billPayRequest.perFacture = request.perFacture;
                                                        billPayRequest.callBackUrl = ThirdPartyAggragatorSettings.callBackUrl;
                                                        billPayRequest.serviceId = ThirdPartyAggragatorSettings.serviceId;
                                                        apiUrl = ThirdPartyAggragatorSettings.payBillUrl_CIE;
                                                        req = JsonConvert.SerializeObject(billPayRequest);

                                                    }
                                                    else if (WalletService.WalletServiceId == 133)
                                                    {
                                                        username = ThirdPartyAggragatorSettings.username_CIE;
                                                        password = ThirdPartyAggragatorSettings.password_CIE;
                                                        billPayRequest.partnerId = ThirdPartyAggragatorSettings.partnerId_WtPay;
                                                        billPayRequest.loginApi = ThirdPartyAggragatorSettings.loginApi_WtPay;
                                                        billPayRequest.passwordApi = ThirdPartyAggragatorSettings.passwordApi_WtPay;
                                                        billPayRequest.telephone = request.customer;
                                                        billPayRequest.montant = request.amount;
                                                        billPayRequest.partnerTransactionId = invoiceNumber.InvoiceNumber;
                                                        billPayRequest.dateLimite = request.Comment;
                                                        billPayRequest.codeExpiration = request.codeExpiration;
                                                        billPayRequest.merchant = ThirdPartyAggragatorSettings.merchant_WtPay;
                                                        billPayRequest.totAmount = request.fees.ToString();
                                                        billPayRequest.typeFacture = request.typeFacture;
                                                        billPayRequest.heureEnreg = request.heureEnreg;
                                                        billPayRequest.refBranch = request.refBranch;
                                                        billPayRequest.numFacture = request.numFacture;
                                                        billPayRequest.idAbonnement = request.idAbonnement;
                                                        billPayRequest.dateEnreg = request.dateEnreg;
                                                        billPayRequest.perFacture = request.perFacture;
                                                        billPayRequest.callBackUrl = ThirdPartyAggragatorSettings.callBackUrl;
                                                        billPayRequest.serviceId = ThirdPartyAggragatorSettings.serviceId_WtPay;
                                                        apiUrl = ThirdPartyAggragatorSettings.payBillUrl_CIE;
                                                        req = JsonConvert.SerializeObject(billPayRequest);
                                                    }
                                                    else if (WalletService.WalletServiceId == 134)
                                                    {
                                                        username = ThirdPartyAggragatorSettings.username_CIE;
                                                        password = ThirdPartyAggragatorSettings.password_CIE;
                                                        var billpay_ml = new BillPaymentRequest_ML
                                                        {
                                                            loginApi = ThirdPartyAggragatorSettings.loginApi_ML,
                                                            passwordApi = ThirdPartyAggragatorSettings.passwordApi_ML,
                                                            montant = request.amount,
                                                            numero = request.numero,
                                                            partnerId = ThirdPartyAggragatorSettings.partnerId_ML,
                                                            partnerTransactionId = invoiceNumber.InvoiceNumber,
                                                            serviceId = ThirdPartyAggragatorSettings.serviceId_ML,
                                                            telephone = request.customer,
                                                            callBackUrl = ThirdPartyAggragatorSettings.callBackUrl
                                                        };


                                                        apiUrl = ThirdPartyAggragatorSettings.PayBill_ML;
                                                        req = JsonConvert.SerializeObject(billpay_ml);
                                                    }
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
                                                    transationInitiate.JsonRequest = req;
                                                    transationInitiate.JsonResponse = "";
                                                    transationInitiate = await _billPaymentRepository.SaveTransactionInitiateRequest(transationInitiate);
                                                    string responseString = string.Empty;
                                                    // var content = new StringContent(req, Encoding.UTF8, "application/json");

                                                    if (WalletService.WalletServiceId == 132 || WalletService.WalletServiceId == 133)
                                                    {
                                                        if (WalletService.HttpVerbs.ToLower() == AggragatorServiceVerbs.HttpPostVerb.ToLower())
                                                        {

                                                            var payData = Task.Run(() => commonApi.BillPayment(req, apiUrl, username, password));
                                                            payData.Wait();
                                                            responseString = payData.Result.ToString();
                                                            //  responseString = HttpPostUrlEncodedService(LogTransactionNameTypes.PayMoney, apiUrl, req, Request, subcategory.CategoryName);

                                                        }
                                                        else
                                                        {
                                                            var payData = Task.Run(() => commonApi.BillPayment(req, apiUrl, username, password));
                                                            payData.Wait();
                                                            responseString = payData.Result.ToString();
                                                            // responseString = HttpGetUrlEncodedService(LogTransactionNameTypes.PayMoney, apiUrl, req, Request, subcategory.CategoryName);                                                    
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (WalletService.HttpVerbs.ToLower() == AggragatorServiceVerbs.HttpPostVerb.ToLower())
                                                        {

                                                            var payData = Task.Run(() => commonApi.PayServices(req, apiUrl, username, password));
                                                            payData.Wait();
                                                            responseString = payData.Result.ToString();
                                                            //  responseString = HttpPostUrlEncodedService(LogTransactionNameTypes.PayMoney, apiUrl, req, Request, subcategory.CategoryName);

                                                        }
                                                        else
                                                        {
                                                            var payData = Task.Run(() => commonApi.PayServices(req, apiUrl, username, password));
                                                            payData.Wait();
                                                            responseString = payData.Result.ToString();
                                                            // responseString = HttpGetUrlEncodedService(LogTransactionNameTypes.PayMoney, apiUrl, req, Request, subcategory.CategoryName);                                                    
                                                        }
                                                    }
                                                    var TransactionInitial = await _billPaymentRepository.GetTransactionInitiateRequest(transationInitiate.Id);
                                                    TransactionInitial.JsonResponse = "Bill payment Response" + responseString;
                                                    TransactionInitial = await _billPaymentRepository.UpdateTransactionInitiateRequest(TransactionInitial);
                                                    var dataSer = JsonConvert.DeserializeObject<PayServicesResponseForServices>(responseString);
                                                    PayServicesResponseForServices payServices = new PayServicesResponseForServices();
                                                    //  payServices.service_id= dataSer.
                                                    LogTransactionTypes.Response.SaveTransactionLog(LogTransactionNameTypes.PayMoney + subcategory.CategoryName, responseString, "Aggregator Url : ");
                                                    var errorResponse = "The remote server returned an error: (500) Internal Server Error.";
                                                    if (!string.IsNullOrEmpty(responseString) && responseString != errorResponse)
                                                    {
                                                        var _responseModel = JsonConvert.DeserializeObject<AddMoneyAggregatorResponse>(responseString);

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
                                                            _responseModel.Amount = request.amount;
                                                            _responseModel.TransactionDate = _tranDate;
                                                            _responseModel.CurrentBalance = data.CurrentBalance;

                                                            WalletTransaction tran = new WalletTransaction();
                                                            tran.CreatedDate = _tranDate;
                                                            tran.UpdatedDate = _tranDate;
                                                            //Self Account 
                                                            tran.ReceiverId = data.WalletUserId;
                                                            //Sender
                                                            tran.TransactionInitiateRequestId = transationInitiate.Id;
                                                            tran.WalletServiceId = WalletService.WalletServiceId;
                                                            tran.TransactionType = AggragatorServiceType.CREDIT;
                                                            tran.TransactionTypeInfo = (int)TransactionTypeInfo.PaidByPayServices;
                                                            tran.VoucherCode = string.Empty;
                                                            tran.SenderId = data.WalletUserId;
                                                            tran.WalletAmount = request.amount;
                                                            tran.ServiceTax = "0";
                                                            tran.ServiceTaxRate = 0;
                                                            tran.UpdatedOn = DateTime.UtcNow;
                                                            if (subcategory.CategoryName == "ISP")
                                                            {
                                                                var rename = customer.Substring(0, customer.IndexOf(','));
                                                                if (rename != null)
                                                                {
                                                                    tran.AccountNo = rename;
                                                                }
                                                                else
                                                                {
                                                                    tran.AccountNo = string.Empty;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                tran.AccountNo = customer;// string.Empty;
                                                            }
                                                            tran.BankTransactionId = string.Empty;
                                                            tran.IsBankTransaction = false;
                                                            tran.BankBranchCode = string.Empty;
                                                            if (WalletService.WalletServiceId == 132 || WalletService.WalletServiceId == 133)
                                                            {
                                                                tran.TransactionId = dataSer.gu_transaction_id;
                                                            }
                                                            else if (WalletService.WalletServiceId == 129)
                                                            {
                                                                tran.TransactionId = dataSer.sessionId;
                                                            }
                                                            else
                                                            {
                                                                tran.TransactionId = dataSer.gu_transaction_id;
                                                            }


                                                            int _TransactionStatus = 0;
                                                            if (_responseModel.StatusCode == AggregatorySTATUSCODES.SUCCESSFUL && dataSer.status.ToUpper() == "SUCCESSFUL")
                                                            {
                                                                _TransactionStatus = (int)TransactionStatus.Completed;
                                                            }
                                                            else if (_responseModel.StatusCode == AggregatorySTATUSCODES.SUCCESSFUL && dataSer.status.ToUpper() == "PENDING")
                                                            {
                                                                _TransactionStatus = (int)TransactionStatus.Pending;
                                                            }
                                                            else if (_responseModel.StatusCode == AggregatorySTATUSCODES.SUCCESSFUL && dataSer.status.ToUpper() == "FAILED")
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
                                                            tran.CommisionAmount = Convert.ToString(_commission.CommissionAmount + _commission.FlatCharges + _commission.BenchmarkCharges);
                                                            tran.FlatCharges = _commission.FlatCharges.ToString();
                                                            tran.CommisionPercent = _commission.CommissionAmount.ToString();
                                                            tran.BenchmarkCharges = _commission.BenchmarkCharges.ToString();

                                                            tran.TotalAmount = Convert.ToString(_commission.AmountWithCommission);
                                                            _responseModel.CurrentBalance = Convert.ToString(_commission.UpdatedCurrentBalance);
                                                            // db.WalletTransactions.Add(tran);
                                                            if (tran.TransactionStatus == (int)TransactionStatus.Completed || tran.TransactionStatus == (int)TransactionStatus.Pending)
                                                            {
                                                                data.CurrentBalance = Convert.ToString(_commission.UpdatedCurrentBalance);
                                                            }
                                                            if (tran.TransactionStatus == (int)TransactionStatus.Completed)
                                                            {

                                                                #region PushNotification


                                                                PayMoneyPushModel pushModel = new PayMoneyPushModel();
                                                                pushModel.TransactionDate = _tranDate;
                                                                pushModel.TransactionId = tran.WalletTransactionId.ToString();
                                                                pushModel.alert = request.amount + " XOF has been debited from your account.";
                                                                pushModel.Amount = request.amount;
                                                                pushModel.CurrentBalance = data.CurrentBalance;
                                                                pushModel.pushType = (int)PushType.PAYSERVICES;
                                                                pushModel.TransactionTypeInfo = (int)TransactionTypeInfo.PaidByPayServices;
                                                                PushNotificationModel push = new PushNotificationModel();
                                                                push.SenderId = data.WalletUserId;
                                                                push.deviceType = (int)data.DeviceType;
                                                                push.deviceKey = data.DeviceToken;
                                                                if ((int)data.DeviceType == (int)DeviceTypes.ANDROID || (int)data.DeviceType == (int)DeviceTypes.Web)
                                                                {
                                                                    PushPayload<PayMoneyPushModel> aps = new PushPayload<PayMoneyPushModel>();
                                                                    PushPayloadData<PayMoneyPushModel> _data = new PushPayloadData<PayMoneyPushModel>();
                                                                    _data.notification = pushModel;
                                                                    aps.data = _data;
                                                                    aps.to = data.DeviceToken;
                                                                    aps.collapse_key = string.Empty;
                                                                    push.message = JsonConvert.SerializeObject(aps);
                                                                    push.payload = pushModel;
                                                                }
                                                                if ((int)data.DeviceType == (int)DeviceTypes.IOS)
                                                                {
                                                                    NotificationJsonResponse<PayMoneyPushModel> aps = new NotificationJsonResponse<PayMoneyPushModel>();
                                                                    aps.aps = pushModel;
                                                                    push.message = JsonConvert.SerializeObject(aps);
                                                                }
                                                                if (!string.IsNullOrEmpty(push.message))
                                                                {
                                                                    _sendPushNotification.sendPushNotification(push);
                                                                }
                                                                #endregion
                                                                response.message = AggregatoryMESSAGE.SUCCESSFUL;
                                                            }
                                                            else
                                                            {
                                                                if (_responseModel.StatusCode == AggregatorySTATUSCODES.SUCCESSFUL && dataSer.status.ToUpper() == "PENDING")
                                                                {
                                                                    response.message = AggregatoryMESSAGE.PENDING;
                                                                }
                                                                else
                                                                {
                                                                    response.message = AggregatoryMESSAGE.FAILED;
                                                                }
                                                            }
                                                            // db.SaveChanges();
                                                            try
                                                            {
                                                                tran = await _billPaymentRepository.InsertWalletTransaction(tran);
                                                            }
                                                            catch (Exception ex)
                                                            {

                                                            }
                                                        }
                                                        else if (_responseModel != null && !string.IsNullOrEmpty(_responseModel.StatusCode) && (_responseModel.StatusCode == AggregatorySTATUSCODES.FAILED))
                                                        {
                                                            response.message = ResponseMessages.AGGREGATOR_FAILED_ERROR;

                                                        }
                                                        else if (_responseModel != null && !string.IsNullOrEmpty(_responseModel.StatusCode) && (_responseModel.StatusCode == AggregatorySTATUSCODES.EXCEPTION))
                                                        {
                                                            response.message = ResponseMessages.AGGREGATOR_FAILED_EXCEPTION;

                                                        }
                                                        else
                                                        {
                                                            response.message = _responseModel.Message;

                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (responseString == errorResponse)
                                                        {
                                                            response.message = ResponseMessages.TRANSACTION_ERROR;

                                                        }
                                                        else
                                                        {
                                                            response.message = ResponseMessages.TRANSACTION_NULL_ERROR;

                                                        }
                                                        // Response.Create(false, ResponseMessages.TRANSACTION_NULL_ERROR, HttpStatusCode.ExpectationFailed, new AddMoneyAggregatorResponse());
                                                        //  Response.Create(false, "Exception occured", HttpStatusCode.ExpectationFailed, new AddMoneyAggregatorResponse());
                                                    }
                                                }
                                                else
                                                {
                                                    response.message = ResponseMessages.INSUFICIENT_BALANCE;

                                                }

                                            }
                                            else
                                            {
                                                response.message = ResponseMessages.USER_NOT_REGISTERED;

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

            }
            else
            {
                response.RstKey = 21;
                response.Status = (int)WalletTransactionStatus.FAILED;
                response.Message = ResponseMessages.MobileNotVerify;//
            }


            return response;
        }

        public async Task<AddMoneyAggregatorResponse> GetBillPaymentServicesAggregator(BillPayMoneyAggregatoryRequest request, long WalletUserId = 0)
        {
            var response = new AddMoneyAggregatorResponse();
            string customer = request.customer;
            var _commissionRequest = new CalculateCommissionRequest();
            var _commission = new CalculateCommissionResponse();
            var commonApi = new CommonApi();
            var sender = await _walletUserRepository.GetUserDetailById(request.WalletUserId);
            var data = await _walletUserRepository.GetCurrentUser(request.WalletUserId);
            var WalletService = await _masterDataRepository.GetWalletServicesByIdOrChannel(request.channel, request.ServiceCategoryId, request.ISD);
            var subcategory = await _masterDataRepository.GetWalletSubCategoriesById(request.ServiceCategoryId);
            bool IsdocVerified = await _walletUserRepository.IsDocVerifiedMOMO((int)sender.DocumetStatus);
            var transactionLimit = await _payMoneyRepository.GetTransactionLimitForPayment(data.WalletUserId);
            int limit = transactionLimit != null ? Convert.ToInt32(transactionLimit.transactionlimit) : 0;
            var transactionHistory = await _payMoneyRepository.GetAllTransactionByDate(data.WalletUserId);
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
                                if (transactionLimit == null || limit >= (Convert.ToDecimal(request.amount) + totalAmountTransfered))
                                {
                                    if (sender != null && !string.IsNullOrEmpty(sender.MobileNo))
                                    {
                                        // var data = db.WalletUsers.Where(x => x.WalletUserId == sender.WalletUserId).FirstOrDefault();

                                        if (sender != null)
                                        {
                                            #region Prepare the Model for Request

                                            // BillPayServicesRequestForServices _MobileMoneyRequest = new BillPayServicesRequestForServices();
                                            // PrepaidBillPayServicesRequestForServices prepaidBillPayServices = new PrepaidBillPayServicesRequestForServices();
                                            GetBillRequestForServicesIvory getBillRequestForServicesIvory = new GetBillRequestForServicesIvory();
                                            GetBillRequestForSN getBillRequestForSN = new GetBillRequestForSN();
                                            GetBillReponseForSn getBillReponseForSn = new GetBillReponseForSn();
                                            var invoiceNumber = await _masterDataRepository.GetInvoiceNumber();
                                            string apiUrl = "";
                                            string username = "";
                                            string password = "";
                                            string req = "";
                                            if (WalletService.WalletServiceId == 129)
                                            {
                                                username = ThirdPartyAggragatorSettings.username_SN;
                                                password = ThirdPartyAggragatorSettings.password_SN;
                                                getBillRequestForSN.partnerId = ThirdPartyAggragatorSettings.partnerId_SN;
                                                getBillRequestForSN.loginApi = ThirdPartyAggragatorSettings.loginApi_SN;
                                                getBillRequestForSN.passwordApi = ThirdPartyAggragatorSettings.passwordApi_SN;
                                                getBillRequestForSN.amount = request.amount;
                                                getBillRequestForSN.meterNo = request.policeNumber;//this is meterNo

                                                apiUrl = ThirdPartyAggragatorSettings.getBillUrl_SN;
                                                req = JsonConvert.SerializeObject(getBillRequestForSN);
                                            }
                                            else if (WalletService.WalletServiceId == 124)
                                            {
                                                username = ThirdPartyAggragatorSettings.username_SN;
                                                password = ThirdPartyAggragatorSettings.password_SN;
                                                var getSdeBillReq = new GetBillRequestForSN_SDE
                                                {
                                                    customer_reference = request.policeNumber,
                                                    login_api = ThirdPartyAggragatorSettings.loginApi_SN,
                                                    password_api = ThirdPartyAggragatorSettings.password_api_SN_prepaid,
                                                    partner_id = ThirdPartyAggragatorSettings.partnerId_SN,
                                                    recipient_country_code = request.ISD
                                                };

                                                apiUrl = ThirdPartyAggragatorSettings.getBillUrl_SDE_SN;
                                                req = JsonConvert.SerializeObject(getSdeBillReq);
                                            }
                                            else if (WalletService.WalletServiceId == 125)
                                            {
                                                username = ThirdPartyAggragatorSettings.username_SN;
                                                password = ThirdPartyAggragatorSettings.password_SN;
                                                var getSdeBillReq = new GetBillRequestForSN_Senelec
                                                {
                                                    recipient_id = request.policeNumber,
                                                    login_api = ThirdPartyAggragatorSettings.loginApi_SN,
                                                    password_api = ThirdPartyAggragatorSettings.password_api_SN_prepaid,
                                                    partner_id = ThirdPartyAggragatorSettings.partnerId_SN,
                                                    recipient_country_code = "SN"
                                                };

                                                apiUrl = ThirdPartyAggragatorSettings.getBillUrl_SDE_SENELEC;
                                                req = JsonConvert.SerializeObject(getSdeBillReq);
                                            }
                                            else if (WalletService.WalletServiceId == 130)
                                            {
                                                getBillRequestForServicesIvory.partnerId = ThirdPartyAggragatorSettings.partnerId;
                                                getBillRequestForServicesIvory.loginApi = ThirdPartyAggragatorSettings.loginApi;
                                                getBillRequestForServicesIvory.passwordApi = ThirdPartyAggragatorSettings.passwordApi;
                                                getBillRequestForServicesIvory.facturier = ThirdPartyAggragatorSettings.facturier;
                                                getBillRequestForServicesIvory.numeroFacture = request.billNumber;
                                                getBillRequestForServicesIvory.serviceId = ThirdPartyAggragatorSettings.serviceId;
                                                apiUrl = ThirdPartyAggragatorSettings.getBillUrl_CIE;
                                                req = JsonConvert.SerializeObject(getBillRequestForServicesIvory);

                                            }
                                            else if (WalletService.WalletServiceId == 132)
                                            {
                                                username = ThirdPartyAggragatorSettings.username_CIE;
                                                password = ThirdPartyAggragatorSettings.password_CIE;
                                                getBillRequestForServicesIvory.partnerId = ThirdPartyAggragatorSettings.partnerId_Cie;
                                                getBillRequestForServicesIvory.loginApi = ThirdPartyAggragatorSettings.loginApi_Cie;
                                                getBillRequestForServicesIvory.passwordApi = ThirdPartyAggragatorSettings.passwordApi_Cie;
                                                getBillRequestForServicesIvory.facturier = ThirdPartyAggragatorSettings.facturier_Cie;
                                                getBillRequestForServicesIvory.numeroFacture = request.numeroFacture;
                                                getBillRequestForServicesIvory.serviceId = ThirdPartyAggragatorSettings.serviceId_Cie;
                                                apiUrl = ThirdPartyAggragatorSettings.getBillUrl_CIE;
                                                req = JsonConvert.SerializeObject(getBillRequestForServicesIvory);
                                            }
                                            else if (WalletService.WalletServiceId == 133)
                                            {
                                                username = ThirdPartyAggragatorSettings.username_CIE;
                                                password = ThirdPartyAggragatorSettings.password_CIE;
                                                getBillRequestForServicesIvory.partnerId = ThirdPartyAggragatorSettings.partnerId_Cie;
                                                getBillRequestForServicesIvory.loginApi = ThirdPartyAggragatorSettings.loginApi_Cie;
                                                getBillRequestForServicesIvory.passwordApi = ThirdPartyAggragatorSettings.passwordApi_Cie;
                                                getBillRequestForServicesIvory.facturier = ThirdPartyAggragatorSettings.facturier_Sodeci;
                                                getBillRequestForServicesIvory.numeroFacture = request.numeroFacture;
                                                getBillRequestForServicesIvory.serviceId = ThirdPartyAggragatorSettings.serviceId_Sodeci;
                                                apiUrl = ThirdPartyAggragatorSettings.getBillUrl_Sodeci;
                                                req = JsonConvert.SerializeObject(getBillRequestForServicesIvory);
                                            }
                                            else if (WalletService.WalletServiceId == 134)
                                            {
                                                username = ThirdPartyAggragatorSettings.username_CIE;
                                                password = ThirdPartyAggragatorSettings.password_CIE;
                                                var req_ML = new GetBillRequest_ML
                                                {
                                                    loginApi = ThirdPartyAggragatorSettings.loginApi_ML,
                                                    passwordApi = ThirdPartyAggragatorSettings.passwordApi_ML,
                                                    montant = request.amount,
                                                    numero = request.numero,
                                                    partnerId = ThirdPartyAggragatorSettings.partnerId_ML
                                                };
                                                apiUrl = ThirdPartyAggragatorSettings.GetBill_ML;
                                                req = JsonConvert.SerializeObject(req_ML);
                                            }
                                            #endregion

                                            string responseString = string.Empty;
                                            // var content = new StringContent(req, Encoding.UTF8, "application/json");                                           

                                            if (WalletService.HttpVerbs.ToLower() == AggragatorServiceVerbs.HttpPostVerb.ToLower())
                                            {
                                                var payData = Task.Run(() => commonApi.GetBillDetail(req, apiUrl, username, password));
                                                payData.Wait();
                                                responseString = payData.Result.ToString();
                                                //  responseString = HttpPostUrlEncodedService(LogTransactionNameTypes.PayMoney, apiUrl, req, Request, subcategory.CategoryName);
                                            }
                                            else
                                            {
                                                var payData = Task.Run(() => commonApi.GetBillDetail(req, apiUrl, username, password));
                                                payData.Wait();
                                                responseString = payData.Result.ToString();
                                                // responseString = HttpGetUrlEncodedService(LogTransactionNameTypes.PayMoney, apiUrl, req, Request, subcategory.CategoryName);                                                    
                                            }
                                            var dataSer = JsonConvert.DeserializeObject<AddMoneyAggregatorResponse>(responseString);

                                            AddMoneyAggregatorResponse payServices = new AddMoneyAggregatorResponse();
                                            //  payServices.service_id= dataSer.
                                            var billDtl = JsonConvert.DeserializeObject(responseString);
                                            LogTransactionTypes.Response.SaveTransactionLog(LogTransactionNameTypes.PayMoney + subcategory.CategoryName, responseString, "Aggregator Url : ");
                                            if (responseString != null)
                                            {
                                                response.DocStatus = IsdocVerified;
                                                response.DocumetStatus = (int)sender.DocumetStatus;
                                                response.IsEmailVerified = (bool)sender.IsEmailVerified;
                                                response.Message = dataSer.message;
                                                response.StatusCode = response.status.ToString();
                                                response.BillDetail = billDtl;
                                            }
                                            else
                                            {
                                                response.message = ResponseMessages.EZEEPAY_FAILED_EXCEPTION;
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

        //public async Task<string> GetFee(PayMoneyAggregatoryRequest Request)
        //{
        //    string responseString = "";
        //    var _mobileMoneyRequest = new GetFeeRequest
        //    {
        //        customer = Request.customer,
        //        amount = Request.Amount
        //    };
        //    var apiUrl = ThirdPartyAggragatorSettings.GetFeePayMoney;
        //    var responseData = Task.Run(() => HttpGetUrlEncodedServiceForMobileMoney(LogTransactionNameTypes.AddMoney, apiUrl, _mobileMoneyRequest, Request, Request.channel));
        //    responseData.Wait();
        //    responseString = responseData.Result.ToString();
        //    var res = JsonConvert.DeserializeObject<object>(responseString);
        //    if (res != null)
        //    {
        //        responseString = res.ToString();
        //    }
        //    return responseString;
        //}

        public async Task<AddMoneyAggregatorResponse> GetBillPaymentServicesAggregator2(BillPayMoneyAggregatoryRequest request, long WalletUserId = 0)
        {
            var response = new AddMoneyAggregatorResponse();
            string customer = request.customer;
            var _commissionRequest = new CalculateCommissionRequest();
            var _commission = new CalculateCommissionResponse();
            var commonApi = new CommonApi();

            var userData = await _billPaymentRepository.GetDetailForBillPayment(request);

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
                                if (transactionLimit == null || limit >= (Convert.ToDecimal(request.amount) + totalAmountTransfered))
                                {
                                    if (sender != null && !string.IsNullOrEmpty(sender.MobileNo))
                                    {
                                        // var data = db.WalletUsers.Where(x => x.WalletUserId == sender.WalletUserId).FirstOrDefault();

                                        if (sender != null)
                                        {
                                            #region Prepare the Model for Request

                                            // BillPayServicesRequestForServices _MobileMoneyRequest = new BillPayServicesRequestForServices();
                                            // PrepaidBillPayServicesRequestForServices prepaidBillPayServices = new PrepaidBillPayServicesRequestForServices();
                                            GetBillRequestForServicesIvory getBillRequestForServicesIvory = new GetBillRequestForServicesIvory();
                                            GetBillRequestForSN getBillRequestForSN = new GetBillRequestForSN();
                                            GetBillReponseForSn getBillReponseForSn = new GetBillReponseForSn();
                                            var invoiceNumber = await _masterDataRepository.GetInvoiceNumber();
                                            string apiUrl = "";
                                            string username = "";
                                            string password = "";
                                            string req = "";
                                            if (WalletService.WalletServiceId == 129)
                                            {
                                                username = ThirdPartyAggragatorSettings.username_SN;
                                                password = ThirdPartyAggragatorSettings.password_SN;
                                                getBillRequestForSN.partnerId = ThirdPartyAggragatorSettings.partnerId_SN;
                                                getBillRequestForSN.loginApi = ThirdPartyAggragatorSettings.loginApi_SN;
                                                getBillRequestForSN.passwordApi = ThirdPartyAggragatorSettings.passwordApi_SN;
                                                getBillRequestForSN.amount = request.amount;
                                                getBillRequestForSN.meterNo = request.policeNumber;//this is meterNo

                                                apiUrl = ThirdPartyAggragatorSettings.getBillUrl_SN;
                                                req = JsonConvert.SerializeObject(getBillRequestForSN);
                                            }
                                            else if (WalletService.WalletServiceId == 124)
                                            {
                                                username = ThirdPartyAggragatorSettings.username_SN;
                                                password = ThirdPartyAggragatorSettings.password_SN;
                                                var getSdeBillReq = new GetBillRequestForSN_SDE
                                                {
                                                    customer_reference = request.policeNumber,
                                                    login_api = ThirdPartyAggragatorSettings.loginApi_SN,
                                                    password_api = ThirdPartyAggragatorSettings.password_api_SN_prepaid,
                                                    partner_id = ThirdPartyAggragatorSettings.partnerId_SN,
                                                    recipient_country_code = request.ISD
                                                };

                                                apiUrl = ThirdPartyAggragatorSettings.getBillUrl_SDE_SN;
                                                req = JsonConvert.SerializeObject(getSdeBillReq);
                                            }
                                            else if (WalletService.WalletServiceId == 125)
                                            {
                                                username = ThirdPartyAggragatorSettings.username_SN;
                                                password = ThirdPartyAggragatorSettings.password_SN;
                                                var getSdeBillReq = new GetBillRequestForSN_Senelec
                                                {
                                                    recipient_id = request.policeNumber,
                                                    login_api = ThirdPartyAggragatorSettings.loginApi_SN,
                                                    password_api = ThirdPartyAggragatorSettings.password_api_SN_prepaid,
                                                    partner_id = ThirdPartyAggragatorSettings.partnerId_SN,
                                                    recipient_country_code = "SN"
                                                };

                                                apiUrl = ThirdPartyAggragatorSettings.getBillUrl_SDE_SENELEC;
                                                req = JsonConvert.SerializeObject(getSdeBillReq);
                                            }
                                            else if (WalletService.WalletServiceId == 130)
                                            {
                                                getBillRequestForServicesIvory.partnerId = ThirdPartyAggragatorSettings.partnerId;
                                                getBillRequestForServicesIvory.loginApi = ThirdPartyAggragatorSettings.loginApi;
                                                getBillRequestForServicesIvory.passwordApi = ThirdPartyAggragatorSettings.passwordApi;
                                                getBillRequestForServicesIvory.facturier = ThirdPartyAggragatorSettings.facturier;
                                                getBillRequestForServicesIvory.numeroFacture = request.billNumber;
                                                getBillRequestForServicesIvory.serviceId = ThirdPartyAggragatorSettings.serviceId;
                                                apiUrl = ThirdPartyAggragatorSettings.getBillUrl_CIE;
                                                req = JsonConvert.SerializeObject(getBillRequestForServicesIvory);

                                            }
                                            else if (WalletService.WalletServiceId == 132)
                                            {
                                                username = ThirdPartyAggragatorSettings.username_CIE;
                                                password = ThirdPartyAggragatorSettings.password_CIE;
                                                getBillRequestForServicesIvory.partnerId = ThirdPartyAggragatorSettings.partnerId_Cie;
                                                getBillRequestForServicesIvory.loginApi = ThirdPartyAggragatorSettings.loginApi_Cie;
                                                getBillRequestForServicesIvory.passwordApi = ThirdPartyAggragatorSettings.passwordApi_Cie;
                                                getBillRequestForServicesIvory.facturier = ThirdPartyAggragatorSettings.facturier_Cie;
                                                getBillRequestForServicesIvory.numeroFacture = request.numeroFacture;
                                                getBillRequestForServicesIvory.serviceId = ThirdPartyAggragatorSettings.serviceId_Cie;
                                                apiUrl = ThirdPartyAggragatorSettings.getBillUrl_CIE;
                                                req = JsonConvert.SerializeObject(getBillRequestForServicesIvory);
                                            }
                                            else if (WalletService.WalletServiceId == 133)
                                            {
                                                username = ThirdPartyAggragatorSettings.username_CIE;
                                                password = ThirdPartyAggragatorSettings.password_CIE;
                                                getBillRequestForServicesIvory.partnerId = ThirdPartyAggragatorSettings.partnerId_Cie;
                                                getBillRequestForServicesIvory.loginApi = ThirdPartyAggragatorSettings.loginApi_Cie;
                                                getBillRequestForServicesIvory.passwordApi = ThirdPartyAggragatorSettings.passwordApi_Cie;
                                                getBillRequestForServicesIvory.facturier = ThirdPartyAggragatorSettings.facturier_Sodeci;
                                                getBillRequestForServicesIvory.numeroFacture = request.numeroFacture;
                                                getBillRequestForServicesIvory.serviceId = ThirdPartyAggragatorSettings.serviceId_Sodeci;
                                                apiUrl = ThirdPartyAggragatorSettings.getBillUrl_Sodeci;
                                                req = JsonConvert.SerializeObject(getBillRequestForServicesIvory);
                                            }
                                            else if (WalletService.WalletServiceId == 134)
                                            {
                                                username = ThirdPartyAggragatorSettings.username_CIE;
                                                password = ThirdPartyAggragatorSettings.password_CIE;
                                                var req_ML = new GetBillRequest_ML
                                                {
                                                    loginApi = ThirdPartyAggragatorSettings.loginApi_ML,
                                                    passwordApi = ThirdPartyAggragatorSettings.passwordApi_ML,
                                                    montant = request.amount,
                                                    numero = request.numero,
                                                    partnerId = ThirdPartyAggragatorSettings.partnerId_ML
                                                };
                                                apiUrl = ThirdPartyAggragatorSettings.GetBill_ML;
                                                req = JsonConvert.SerializeObject(req_ML);
                                            }
                                            #endregion

                                            string responseString = string.Empty;
                                            // var content = new StringContent(req, Encoding.UTF8, "application/json");                                           

                                            if (WalletService.HttpVerbs.ToLower() == AggragatorServiceVerbs.HttpPostVerb.ToLower())
                                            {
                                                var payData = Task.Run(() => commonApi.GetBillDetail(req, apiUrl, username, password));
                                                payData.Wait();
                                                responseString = payData.Result.ToString();
                                                //  responseString = HttpPostUrlEncodedService(LogTransactionNameTypes.PayMoney, apiUrl, req, Request, subcategory.CategoryName);
                                            }
                                            else
                                            {
                                                var payData = Task.Run(() => commonApi.GetBillDetail(req, apiUrl, username, password));
                                                payData.Wait();
                                                responseString = payData.Result.ToString();
                                                // responseString = HttpGetUrlEncodedService(LogTransactionNameTypes.PayMoney, apiUrl, req, Request, subcategory.CategoryName);                                                    
                                            }
                                            var dataSer = JsonConvert.DeserializeObject<AddMoneyAggregatorResponse>(responseString);

                                            AddMoneyAggregatorResponse payServices = new AddMoneyAggregatorResponse();
                                            //  payServices.service_id= dataSer.
                                            var billDtl = JsonConvert.DeserializeObject(responseString);
                                            LogTransactionTypes.Response.SaveTransactionLog(LogTransactionNameTypes.PayMoney + subcategory.CategoryName, responseString, "Aggregator Url : ");
                                            if (responseString != null)
                                            {
                                                response.DocStatus = IsdocVerified;
                                                response.DocumetStatus = (int)sender.DocumetStatus;
                                                response.IsEmailVerified = (bool)sender.IsEmailVerified;
                                                response.Message = dataSer.message;
                                                response.StatusCode = response.status.ToString();
                                                response.BillDetail = billDtl;
                                            }
                                            else
                                            {
                                                response.message = ResponseMessages.EZEEPAY_FAILED_EXCEPTION;
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

    }
}
