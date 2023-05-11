using ezeePay.Utility.CommonClass;
using ezeePay.Utility.Enums;
using Ezipay.Database;
using Ezipay.Repository.AirtimeRepo;
using Ezipay.Repository.CommisionRepo;
using Ezipay.Repository.CommonRepo;
using Ezipay.Repository.InterNetProviderRepo;
using Ezipay.Repository.MasterData;
using Ezipay.Repository.PayMoneyRepo;
using Ezipay.Repository.UserRepo;
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
using System.Net;
using System.Threading.Tasks;

namespace Ezipay.Service.InterNetProviderService
{
    public class InterNetServiceProviderService : IInterNetProviderService
    {
        private IInterNetProviderRepository _interNetProviderRepository;
        private IWalletUserRepository _walletUserRepository;
        private ISetCommisionRepository _setCommisionRepository;
        private IMasterDataRepository _masterDataRepository;
        private ISendPushNotification _sendPushNotification;
        private ISendEmails _sendEmails;
        private ICommonServices _commonServices;
        private ICommonRepository _commonRepository;
        private IPayMoneyRepository _payMoneyRepository;
        private IAirtimeRepository _airtimeRepository;
        public InterNetServiceProviderService()
        {
            _airtimeRepository = new AirtimeRepository();
            _interNetProviderRepository = new InterNetProviderRepository();
            _walletUserRepository = new WalletUserRepository();
            _setCommisionRepository = new SetCommisionRepository();
            _masterDataRepository = new MasterDataRepository();
            _sendEmails = new SendEmails();
            _sendPushNotification = new SendPushNotification();
            _commonServices = new CommonServices();
            _commonRepository = new CommonRepository();
            _payMoneyRepository = new PayMoneyRepository();
        }

        public async Task<AddMoneyAggregatorResponse> ISPServices(PayMoneyAggregatoryRequest request, long WalletUserId = 0)
        {
            var response = new AddMoneyAggregatorResponse();
            var transationInitiate = new TransactionInitiateRequest();
            var _commissionRequest = new CalculateCommissionRequest();
            var _commission = new CalculateCommissionResponse();
            string BundleId = "";
            string customerMobile = "";
            string customer = request.customer.ToString();


            string responseString = "";
            //var userData = await _commonRepository.GetDetailForBillPayment(request);

            //var sender = userData.sender;
            //var WalletService = userData.WalletService;
            //var subcategory = userData.SubCategory;
            //bool IsdocVerified = userData.IsdocVerified;
            //var transactionLimit = userData.transactionLimit;
            //int limit = transactionLimit != null ? Convert.ToInt32(transactionLimit.transactionlimit) : 0;
            //var transactionHistory = userData.transactionHistory;
            //int totalAmountTransfered = transactionHistory != null ? Convert.ToInt32(transactionHistory.totalAmount) : 0;

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
            if (request.ISD == "+245" || request.ISD == "+227" || request.ISD == "+229" || (request.ISD == "+225" && WalletService.WalletServiceId == 142 || request.ServiceCategoryId == 8) || request.ISD == "+228")
            {
                string isdCode = request.ISD.Trim('+');
                customer = isdCode + request.customer;
            }

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
                                    if (sender != null && !string.IsNullOrEmpty(sender.MobileNo))
                                    {
                                        // var data = db.WalletUsers.Where(x => x.WalletUserId == sender.WalletUserId).FirstOrDefault();

                                        if (sender != null)
                                        {
                                            if (subcategory.CategoryName == "ISP")
                                            {
                                                if (request.channel.ToLower() == "surfline")
                                                {
                                                    customer = customer;
                                                }
                                                else
                                                {
                                                    customer = customer;
                                                }
                                            }

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
                                            string transactionInitiate = string.Empty;
                                            if (currentBalance > 0 && currentBalance >= amountWithCommision && _commission.CurrentBalance > 0 && _commission.CurrentBalance >= amountWithCommision)
                                            {
                                                #region Prepare the Model for Request

                                                var _MobileMoneyRequest = new MobileMoneyAggregatoryRequest();
                                                var apiUrl = ThirdPartyAggragatorSettings.AirtimeArtx;
                                                if ((request.ISD == "+245" && request.ServiceCategoryId == 8) || (request.ISD == "+227" && request.ServiceCategoryId == 8) || (request.ISD == "+229" && request.ServiceCategoryId == 8) || (request.ISD == "+225" && WalletService.WalletServiceId == 142 || request.ServiceCategoryId == 8) || (request.ISD == "+228" && request.ServiceCategoryId == 8))
                                                {
                                                    var invoiceGetProduct = await _masterDataRepository.GetInvoiceNumber();
                                                    var passwordHashedGetProduct = _commonServices.SHA1Hash("eazipayapixof1234");
                                                    string reqGetProduct = invoiceGetProduct.InvoiceNumber + passwordHashedGetProduct;

                                                    var hashedPassGetProduct = _commonServices.SHA1Hash(reqGetProduct);


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


                                                    transactionInitiate = JsonConvert.SerializeObject(_MobileMoneyRequest);
                                                    // transactionInitiate = req;
                                                    #region transaction initiate request 
                                                    //This is for transaction initiate request all---
                                                    transationInitiate.InvoiceNumber = invoiceGetProduct.InvoiceNumber;
                                                    transationInitiate.ReceiverNumber = request.customer;
                                                    transationInitiate.ServiceName = WalletService.ServiceName;
                                                    transationInitiate.RequestedAmount = _commission.TransactionAmount.ToString();
                                                    transationInitiate.TransactionStatus = (int)TransactionStatus.Pending;
                                                    transationInitiate.WalletUserId = sender.WalletUserId;
                                                    transationInitiate.UserReferanceNumber = invoiceGetProduct.AutoDigit;
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

                                                    data.CurrentBalance = Convert.ToString(_commission.UpdatedCurrentBalance);
                                                    //calling pay method insert data in Database
                                                    await _walletUserRepository.UpdateUserDetail(data);
                                                    #endregion
                                                    var jsonReq = JsonConvert.SerializeObject(reqForPayment);
                                                    apiUrl = ThirdPartyAggragatorSettings.AirtimeArtx;
                                                    var payData = PaymentIsp(jsonReq, apiUrl);
                                                    payData.Wait();
                                                    responseString = payData.Result.ToString();
                                                }

                                                //var invoiceNumber = await _masterDataRepository.GetInvoiceNumber();
                                                //if (invoiceNumber != null)
                                                //{
                                                //    _MobileMoneyRequest.invoiceNo = invoiceNumber.InvoiceNumber;
                                                //}
                                                //_MobileMoneyRequest.customer = customer;//Request.customer;                                                
                                                //_MobileMoneyRequest.apiKey = ThirdPartyAggragatorSettings.ApiKey;
                                                //_MobileMoneyRequest.signature = new CommonMethods().MD5Hash(_MobileMoneyRequest);

                                                #endregion
                                                // string apiUrl = ThirdPartyAggragatorSettings.PayMoneyUrl;
                                                // string responseString = "";
                                                //if (WalletService.HttpVerbs.ToLower() == AggragatorServiceVerbs.HttpPostVerb.ToLower())
                                                //{
                                                //    responseString = new CommonMethods().HttpPostUrlEncodedService(LogTransactionNameTypes.PayMoney, apiUrl, _MobileMoneyRequest, request, subcategory.CategoryName);
                                                //}
                                                //else if (request.channel.ToUpper() == "SURFLINE")
                                                //{
                                                //    responseString = new CommonMethods().HttpGetUrlEncodedServiceForSurfline(LogTransactionNameTypes.PayMoney, apiUrl, _MobileMoneyRequest, request, subcategory.CategoryName, BundleId, customerMobile);
                                                //}
                                                //else
                                                //{
                                                //    responseString = new CommonMethods().HttpGetUrlEncodedService(LogTransactionNameTypes.PayMoney, apiUrl, _MobileMoneyRequest, request, subcategory.CategoryName);
                                                //}
                                                var _responseModel = new AddMoneyAggregatorResponse();
                                                AirtimePaymentResponse airtimePayment = new AirtimePaymentResponse();
                                                PayServicesResponseForServices dataSer = new PayServicesResponseForServices();
                                                LogTransactionTypes.Response.SaveTransactionLog(LogTransactionNameTypes.PayMoney + subcategory.CategoryName, responseString, "Aggregator Url : ", sender.WalletUserId);
                                                var TransactionInitial = await _airtimeRepository.GetTransactionInitiateRequest(transationInitiate.Id);
                                                TransactionInitial.JsonResponse = "ISP Response" + responseString;
                                                await _airtimeRepository.UpdateTransactionInitiateRequest(TransactionInitial);

                                                if (!string.IsNullOrEmpty(responseString))
                                                {
                                                    var airtimeFailed = JsonConvert.DeserializeObject<dynamic>(responseString);

                                                    if (airtimeFailed.status.typeName != "Failure")
                                                    {
                                                        //responseString = "{\"status\":{\"id\":0,\"name\":\"Successful\",\"type\":0,\"typeName\":\"Success\"},\"command\":\"execTransaction\",\"timestamp\":1591970488,\"reference\":5749340673,\"result\":{\"id\":7456452105,\"operator\":{\"id\":\"8\",\"name\":\"Benin MTN\",\"reference\":\"2020061215012816201461425\"},\"country\":{\"id\":\"BJ\",\"name\":\"Benin\"},\"amount\":{\"operator\":\"200.00\",\"user\":\"200.00\"},\"currency\":{\"user\":\"XOF\",\"operator\":\"XOF\"},\"productId\":\"17\",\"productType\":\"1\",\"simulation\":false,\"userReference\":\"011620\",\"msisdn\":\"22969364393\",\"balance\":{\"initial\":\"549.60\",\"transaction\":\"200.00\",\"commission\":\"4.00\",\"commissionPercentage\":\"2.00\",\"final\":\"353.60\",\"currency\":\"XOF\"}}}";
                                                        airtimePayment = JsonConvert.DeserializeObject<AirtimePaymentResponse>(responseString);
                                                        if (airtimePayment.status.name.ToUpper() == "SUCCESSFUL")
                                                        {
                                                            _responseModel.StatusCode = AggregatorySTATUSCODES.SUCCESSFUL;
                                                            dataSer.gu_transaction_id = airtimePayment.result.userReference;
                                                            _responseModel.TransactionId = airtimePayment.result.userReference;
                                                            dataSer.status = "SUCCESSFUL";
                                                        }
                                                        else if (airtimePayment.status.name.ToUpper() == "PENDING")
                                                        {
                                                            _responseModel.StatusCode = AggregatorySTATUSCODES.PENDING;
                                                            dataSer.gu_transaction_id = airtimePayment.result.userReference;
                                                            _responseModel.TransactionId = airtimePayment.result.userReference;
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
                                                            _responseModel.TransactionId = airtimePayment.result.userReference;
                                                            dataSer.status = "FAILED";
                                                        }
                                                    }

                                                    // var _responseModel = JsonConvert.DeserializeObject<AddMoneyAggregatorResponse>(responseString);
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
                                                            response.AccountNo = _responseModel.AccountNo;//request.customer;
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
                                                        tran.TransactionInitiateRequestId = transationInitiate.Id;
                                                        tran.BeneficiaryName = request.BeneficiaryName;
                                                        tran.CreatedDate = _tranDate;
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
                                                        tran.AccountNo = request.customer;// string.Empty;                                                   
                                                        tran.BankTransactionId = string.Empty;
                                                        tran.IsBankTransaction = false;
                                                        tran.BankBranchCode = string.Empty;
                                                        tran.TransactionId = _responseModel.TransactionId;
                                                        int _TransactionStatus = 0;
                                                        response.TransactionId = tran.TransactionId;
                                                        response.ToMobileNo = customer;
                                                        response.TransactionDate = DateTime.UtcNow;
                                                        response.Amount = request.Amount;
                                                        response.MobileNo = customer;
                                                        response.StatusCode = AggregatorySTATUSCODES.SUCCESSFUL;
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

                                                        tran.TotalAmount = Convert.ToString(_commission.AmountWithCommission);

                                                        _responseModel.CurrentBalance = Convert.ToString(_commission.UpdatedCurrentBalance);
                                                        //   db.WalletTransactions.Add(tran);


                                                        if (tran.TransactionStatus == (int)TransactionStatus.Completed || tran.TransactionStatus == (int)TransactionStatus.Pending)
                                                        {
                                                            sender.CurrentBalance = Convert.ToString(_commission.UpdatedCurrentBalance);
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
                                                        //calling pay method insert data in Database
                                                        tran = await _interNetProviderRepository.ISPServices(tran);
                                                        // await _walletUserRepository.UpdateUserDetail(sender);
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
                                                    var tran = new WalletTransaction();
                                                    tran.BeneficiaryName = request.BeneficiaryName;
                                                    tran.CreatedDate = DateTime.UtcNow; ;
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
                                                    tran.InvoiceNo = string.Empty;
                                                    tran.Comments = request.Comment;
                                                    tran.MerchantCommissionId = _commission.MerchantCommissionId;
                                                    tran.MerchantCommissionAmount = Convert.ToString(_commission.MerchantCommissionAmount);
                                                    tran.CommisionId = _commission.CommissionId;
                                                    tran.CommisionAmount = Convert.ToString(_commission.CommissionAmount);
                                                    tran.CommisionPercent = _commission.CommissionAmount.ToString();
                                                    tran.TotalAmount = Convert.ToString(_commission.AmountWithCommission);
                                                    tran = await _airtimeRepository.AirtimeServices(tran);

                                                    response.RstKey = 9;
                                                    response.Message = ResponseMessages.TRANSACTION_NULL_ERROR;
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
                    response.RstKey = 6;
                    response.Status = (int)WalletTransactionStatus.FAILED;
                    response.Message = "Please verify your email id.";
                }
            }
            else
            {
                response.RstKey = 21;
                response.Status = (int)WalletTransactionStatus.FAILED;
                response.Message = ResponseMessages.MobileNotVerify;// 
            }
            response.DocStatus = IsdocVerified;
            response.DocumetStatus = (int)sender.DocumetStatus;
            response.CurrentBalance = sender.CurrentBalance;
            response.MobileNo = request.customer;
            return response;
        }

        public async Task<string> PaymentAirtime(string req, string url)
        {
            string resString = "";
            string resBody = "";
            RootObject responseData = new RootObject();
            //using (HttpClient client = new HttpClient())
            //{
            //    // Call asynchronous network methods in a try/catch block to handle exceptions
            //    try
            //    {
            //        var content = new StringContent(req, Encoding.UTF8, "application/json");

            //        HttpResponseMessage response = await client.PostAsync(url, content);
            //        response.EnsureSuccessStatusCode();
            //        resBody = await response.Content.ReadAsStringAsync();
            //        Console.WriteLine(resBody);
            //    }
            //    catch (HttpRequestException e)
            //    {
            //        Console.WriteLine("\nException Caught!");
            //        Console.WriteLine("Message :{0} ", e.Message);
            //    }
            //    return resBody;
            //}
            using (WebClient wc = new WebClient())
            {
                //var content = new StringContent(req, Encoding.UTF8, "application/json");
                try
                {
                    wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                    string HtmlResult = wc.UploadString(url, req);

                    resBody = HtmlResult;
                }
                catch (Exception ex)
                {

                }

            }
            return resBody;
        }

        public async Task<string> PaymentIsp(string req, string url)
        {
            string resString = "";
            string resBody = "";
            RootObject responseData = new RootObject();
            using (WebClient wc = new WebClient())
            {
                //var content = new StringContent(req, Encoding.UTF8, "application/json");
                try
                {
                    wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                    resBody = wc.UploadString(url, req);

                    //resBody = "{\"status\":{\"id\":0,\"name\":\"Successful\",\"type\":0,\"typeName\":\"Success\"},\"command\":\"execTransaction\",\"timestamp\":1595495791,\"reference\":4471888122,\"result\":{\"id\":7772215624,\"operator\":{\"id\":\"17\",\"name\":\"Ivory Coast MTN\",\"reference\":\"GSGR1595495791A000141\"},\"country\":{\"id\":\"CI\",\"name\":\"Ivory Coast\"},\"amount\":{\"operator\":\"60.00\",\"user\":\"60.00\"},\"currency\":{\"user\":\"XOF\",\"operator\":\"XOF\"},\"productId\":\"4690\",\"productType\":\"4\",\"simulation\":false,\"userReference\":\"011817\",\"msisdn\":\"22564690778\",\"balance\":{\"initial\":\"1971370.10\",\"transaction\":\"60.00\",\"commission\":\"1.20\",\"commissionPercentage\":\"2.00\",\"final\":\"1971311.30\",\"currency\":\"XOF\"}}}";
                }
                catch (Exception ex)
                {
                    resBody = "";
                }

            }
            return resBody;
        }

        public async Task<AddMoneyAggregatorResponse> ISPServicesV2(PayMoneyAggregatoryRequest request, long WalletUserId = 0)
        {
            var response = new AddMoneyAggregatorResponse();

            var _commissionRequest = new CalculateCommissionRequest();
            var _commission = new CalculateCommissionResponse();
            string BundleId = "";
            string customerMobile = "";
            string customer = request.customer.Length.ToString();

            //if (customer.Length != 0 && request.ServiceCategoryId == 8)
            //{
            //    if (request.channel.ToUpper() == "BUSY")
            //    {
            //        customer = request.customer;
            //    }
            //    else if (request.channel.ToUpper() == "SURFLINE")
            //    {
            //        var mobileNumber = request.customer.Split(',');
            //        BundleId = mobileNumber[1];
            //        customerMobile = mobileNumber[0];
            //        int idx = customer.IndexOf("0");
            //        if (idx < 1)
            //        {
            //            customer = "0" + customerMobile + "," + BundleId;
            //        }
            //        else
            //        {
            //            customer = customerMobile + "," + BundleId;
            //        }
            //    }
            //    else if (request.channel.ToUpper() == "MTN FIBRE")
            //    {
            //        var mobileNumber = request.customer.Split(',');
            //        BundleId = mobileNumber[1];
            //        customerMobile = mobileNumber[0];

            //        string customerNumber = customerMobile.Substring(0, 1);
            //        int idx = customerMobile.IndexOf("0");
            //        if (customerNumber != "0")
            //        {
            //            customer = "0" + customerMobile + "," + BundleId;
            //        }
            //        else
            //        {
            //            customer = customerMobile + "," + BundleId;
            //        }
            //    }
            //    else
            //    {
            //        customer = request.customer;
            //    }
            //}
            //else
            //{
            //    customer = request.customer;
            //}
            string responseString = "";
            var sender = await _walletUserRepository.GetUserDetailById(request.WalletUserId);
            var data = await _walletUserRepository.GetCurrentUser(request.WalletUserId);
            var WalletService = await _masterDataRepository.GetWalletServicesByIdOrChannel(request.channel, request.ServiceCategoryId, request.ISD);
            var subcategory = await _masterDataRepository.GetWalletSubCategoriesById(request.ServiceCategoryId);
            bool IsdocVerified = await _walletUserRepository.IsDocVerifiedMOMO((int)sender.DocumetStatus);
            if (request.ISD == "+245" || request.ISD == "+227" || request.ISD == "+229" || (request.ISD == "+225" && WalletService.WalletServiceId == 142 || request.ServiceCategoryId == 8) || request.ISD == "+228")
            {
                string isdCode = request.ISD.Trim('+');
                customer = isdCode + request.customer;
            }
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
                                if (sender != null && !string.IsNullOrEmpty(sender.MobileNo))
                                {
                                    // var data = db.WalletUsers.Where(x => x.WalletUserId == sender.WalletUserId).FirstOrDefault();

                                    if (sender != null)
                                    {
                                        if (subcategory.CategoryName == "ISP")
                                        {
                                            if (request.channel.ToLower() == "surfline")
                                            {
                                                customer = customer;
                                            }
                                            else
                                            {
                                                customer = customer;
                                            }
                                        }

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
                                        decimal currentBalance = Convert.ToDecimal(data.CurrentBalance);
                                        if (currentBalance > 0 && currentBalance >= amountWithCommision && _commission.CurrentBalance > 0 && _commission.CurrentBalance >= amountWithCommision)
                                        {
                                            #region Prepare the Model for Request

                                            var _MobileMoneyRequest = new MobileMoneyAggregatoryRequest();
                                            var apiUrl = ThirdPartyAggragatorSettings.AirtimeArtx;
                                            if ((request.ISD == "+245" && request.ServiceCategoryId == 8) || (request.ISD == "+227" && request.ServiceCategoryId == 8) || (request.ISD == "+229" && request.ServiceCategoryId == 8) || (request.ISD == "+225" && WalletService.WalletServiceId == 142 || request.ServiceCategoryId == 8) || (request.ISD == "+228" && request.ServiceCategoryId == 8))
                                            {


                                                var invoiceGetProduct = await _masterDataRepository.GetInvoiceNumber();
                                                var passwordHashedGetProduct = _commonServices.SHA1Hash("eazipayapixof1234");
                                                string reqGetProduct = invoiceGetProduct.InvoiceNumber + passwordHashedGetProduct;

                                                var hashedPassGetProduct = _commonServices.SHA1Hash(reqGetProduct);


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
                                                var payData = PaymentAirtime(jsonReq, apiUrl);
                                                payData.Wait();
                                                responseString = payData.Result.ToString();
                                            }

                                            //var invoiceNumber = await _masterDataRepository.GetInvoiceNumber();
                                            //if (invoiceNumber != null)
                                            //{
                                            //    _MobileMoneyRequest.invoiceNo = invoiceNumber.InvoiceNumber;
                                            //}
                                            //_MobileMoneyRequest.customer = customer;//Request.customer;                                                
                                            //_MobileMoneyRequest.apiKey = ThirdPartyAggragatorSettings.ApiKey;
                                            //_MobileMoneyRequest.signature = new CommonMethods().MD5Hash(_MobileMoneyRequest);

                                            #endregion
                                            // string apiUrl = ThirdPartyAggragatorSettings.PayMoneyUrl;
                                            // string responseString = "";
                                            if (WalletService.HttpVerbs.ToLower() == AggragatorServiceVerbs.HttpPostVerb.ToLower())
                                            {
                                                responseString = new CommonMethods().HttpPostUrlEncodedService(LogTransactionNameTypes.PayMoney, apiUrl, _MobileMoneyRequest, request, subcategory.CategoryName);
                                            }
                                            else if (request.channel.ToUpper() == "SURFLINE")
                                            {
                                                responseString = new CommonMethods().HttpGetUrlEncodedServiceForSurfline(LogTransactionNameTypes.PayMoney, apiUrl, _MobileMoneyRequest, request, subcategory.CategoryName, BundleId, customerMobile);
                                            }
                                            else
                                            {
                                                responseString = new CommonMethods().HttpGetUrlEncodedService(LogTransactionNameTypes.PayMoney, apiUrl, _MobileMoneyRequest, request, subcategory.CategoryName);
                                            }
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
                                                        response.AccountNo = _responseModel.AccountNo;//request.customer;
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
                                                    tran.TransactionId = _responseModel.TransactionId;
                                                    int _TransactionStatus = 0;
                                                    response.TransactionId = tran.TransactionId;
                                                    response.ToMobileNo = customer;
                                                    response.TransactionDate = DateTime.UtcNow;
                                                    response.Amount = request.Amount;
                                                    response.MobileNo = customer;
                                                    response.StatusCode = AggregatorySTATUSCODES.SUCCESSFUL;
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
                                                    //calling pay method insert data in Database
                                                    tran = await _interNetProviderRepository.ISPServices(tran);
                                                    await _walletUserRepository.UpdateUserDetail(data);
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
                                                if (responseString == errorResponse)
                                                {
                                                    response.RstKey = 7;
                                                    response.Message = ResponseMessages.TRANSACTION_ERROR;
                                                }
                                                else
                                                {
                                                    response.RstKey = 8;
                                                    response.Message = ResponseMessages.TRANSACTION_NULL_ERROR;
                                                }
                                                response.RstKey = 9;
                                                response.Message = ResponseMessages.TRANSACTION_NULL_ERROR;
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
                response.RstKey = 6;
                response.Status = (int)WalletTransactionStatus.FAILED;
                response.Message = "Please verify your email id.";
            }

            response.DocStatus = IsdocVerified;
            response.DocumetStatus = (int)sender.DocumetStatus;
            response.CurrentBalance = data.CurrentBalance;
            response.MobileNo = request.customer;
            return response;
        }
    }
}
