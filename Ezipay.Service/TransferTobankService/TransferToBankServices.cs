using ezeePay.Utility.CommonClass;
using ezeePay.Utility.Enums;
using Ezipay.Database;
using Ezipay.Repository.CommisionRepo;
using Ezipay.Repository.MasterData;
using Ezipay.Repository.PayMoneyRepo;
using Ezipay.Repository.PushNotificationRepo;
using Ezipay.Repository.TransferToBankRepo;
using Ezipay.Repository.UserRepo;
using Ezipay.Service.Admin.TransactionLimitAU;
using Ezipay.Service.UserService;
using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.Utility.SendEmail;
using Ezipay.Utility.SendPush;
using Ezipay.ViewModel.AirtimeViewModel;
using Ezipay.ViewModel.CommisionViewModel;

using Ezipay.ViewModel.SendEmailViewModel;
using Ezipay.ViewModel.SendPushViewModel;
using Ezipay.ViewModel.TransferToBankViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Ezipay.Service.TransferTobankService
{
    public class TransferToBankServices : ITransferToBankServices
    {

        private ITransferToBankRepository _transferToBankRepository;
        private IWalletUserService _walletUserService;
        private IWalletUserRepository _walletUserRepository;
        private IMasterDataRepository _masterDataRepository;
        private ISetCommisionRepository _setCommisionRepository;
        private ISendEmails _sendEmails;
        private ISendPushNotification _sendPushNotification;
        private IPushNotificationRepository _pushNotificationRepository;
        private IPayMoneyRepository _payMoneyRepository;
        private ITransactionLimitAUService _transactionLimitAUService;
        public TransferToBankServices()
        {
            _walletUserService = new WalletUserService();
            _walletUserRepository = new WalletUserRepository();
            _transferToBankRepository = new TransferToBankRepository();
            _masterDataRepository = new MasterDataRepository();
            _setCommisionRepository = new SetCommisionRepository();
            _sendEmails = new SendEmails();
            _sendPushNotification = new SendPushNotification();
            _pushNotificationRepository = new PushNotificationRepository();
            _payMoneyRepository = new PayMoneyRepository();
            _transactionLimitAUService = new TransactionLimitAUService();
        }


        public async Task<List<IsdCodesResponse1>> GetTransferttobankCountryList()
        {
            var result = new List<IsdCodesResponse1>();
            return result = await _transferToBankRepository.GetTransferttobankCountryList();
        }



        /// <summary>
        /// GetBankList
        /// </summary>
        /// <returns></returns>
        /// 


        public async Task<List<BankListList>> GetBankList()
        {
            var result = new List<BankListList>();
            return result = await _transferToBankRepository.GetBankList();
        }

        public async Task<AddMoneyAggregatorResponse> PayMoneyTransferToBank(PayMoneyAggregatoryRequest request, long WalletUserId = 0)
        {
            var response = new AddMoneyAggregatorResponse();

            var _commissionRequest = new CalculateCommissionRequest();
            var _commission = new CalculateCommissionResponse();
            var transationInitiate = new TransactionInitiateRequest();

            var senderObj = new MobileMoneySenderDetail1();
            var recipientObj = new MobileMoneyReceiverDetail1();


            var sender = await _walletUserRepository.GetUserDetailById(request.WalletUserId);

            var senderIsdCode = await _masterDataRepository.IsdCodesby(sender.StdCode);
            var data = await _walletUserRepository.GetCurrentUser(request.WalletUserId);
            var WalletService = await _masterDataRepository.GetWalletServicesByIdOrChannel(request.channel, request.ServiceCategoryId, request.IsdCode);
            var subcategory = await _masterDataRepository.GetWalletSubCategoriesById(request.ServiceCategoryId);

            bool IsdocVerified = await _walletUserRepository.IsDocVerifiedMOMO((int)sender.DocumetStatus);
            var transactionLimit = await _payMoneyRepository.GetTransactionLimitForPayment(data.WalletUserId);
            var totalTransactionCount = await _payMoneyRepository.GetTotalTransactionCount(data.WalletUserId);
            long count = totalTransactionCount != null ? totalTransactionCount.TotalTransactions : 0;
            int limit = transactionLimit != null ? Convert.ToInt32(transactionLimit.transactionlimit) : 0;
            var transactionHistory = await _payMoneyRepository.GetAllTransactionByDate(data.WalletUserId);
            int totalAmountTransfered = transactionHistory != null ? Convert.ToInt32(transactionHistory.totalAmount) : 0;
            string customer = request.customer;//.Length.ToString();
            //
            var resultTL = await _transactionLimitAUService.CheckTransactionLimitAU(request.WalletUserId.ToString()); //check New TL

            if (request.ISD == "+223" || request.ISD == "+225" || request.ISD == "+245" || request.ISD == "+226" || request.ISD == "+227" || request.ISD == "+228" || request.ISD == "+229")
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
                                    if (transactionLimit == null || limit >= (Convert.ToDecimal(request.Amount) + totalAmountTransfered))
                                    {
                                        if (sender != null && !string.IsNullOrEmpty(sender.MobileNo))
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
                                            decimal currentBalance = Convert.ToDecimal(data.CurrentBalance);

                                            if (resultTL != null) //chk new TL for all user
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
                                                var _mobileMoneyRequest = new AddMobileMoneyAggregatoryRequest();
                                                var invoiceNumber = await _masterDataRepository.GetInvoiceNumber();
                                                string responseString = "";
                                                string transactionInitiate = string.Empty;
                                                #region Prepare the Model for Request
                                                if (request.ServiceCategoryId == 6)
                                                {
                                                    #region Prepare the Model for Request

                                                    _mobileMoneyRequest.servicecategory = "banktransfer";
                                                    _mobileMoneyRequest.ServiceType = AggragatorServiceType.CREDIT;
                                                    _mobileMoneyRequest.Channel = request.channel;
                                                    _mobileMoneyRequest.Amount = Convert.ToString(Convert.ToInt32(_commission.TransactionAmount)); //Request.amount;
                                                                                                                                                   //var invoiceNumber = new ThirdPartyRepository().GetInvoiceNumber();
                                                    if (invoiceNumber != null)
                                                    {
                                                        _mobileMoneyRequest.TransactionId = invoiceNumber.InvoiceNumber;
                                                    }
                                                   
                                                    _mobileMoneyRequest.Customer = customer;
                                                    request.IsdCode = request.IsdCode;
                                                    if (request.IsdCode == "+225")
                                                    {
                                                        _mobileMoneyRequest.Country = "CI";
                                                        _mobileMoneyRequest.Customer = customer;
                                                        recipientObj.BankCode = "414";
                                                    }
                                                    else if (request.IsdCode == "+221")
                                                    {
                                                        _mobileMoneyRequest.Country = "SN";
                                                        _mobileMoneyRequest.Customer = customer;
                                                    }
                                                    else if (request.IsdCode == "+226")
                                                    {
                                                        _mobileMoneyRequest.Country = "BF";
                                                        _mobileMoneyRequest.Customer = customer;
                                                        recipientObj.BankCode = "5091"; 
                                                    }
                                                    else if (request.IsdCode == "+223")
                                                    {
                                                        _mobileMoneyRequest.Country = "ML";
                                                        _mobileMoneyRequest.Customer = customer;
                                                        recipientObj.BankCode = "5101"; 
                                                    }
                                                    if (request.IsdCode == "+228")
                                                    {
                                                        _mobileMoneyRequest.Country = "TG";
                                                        _mobileMoneyRequest.Customer = customer;
                                                        recipientObj.BankCode = "413"; 
                                                    }
                                                    else if (request.IsdCode == "+227")
                                                    {
                                                        _mobileMoneyRequest.Country = "NE";
                                                        _mobileMoneyRequest.Customer = customer;
                                                        recipientObj.BankCode = "407";
                                                    }
                                                    else if (request.IsdCode == "+229")
                                                    {
                                                        _mobileMoneyRequest.Country = "BJ";
                                                        _mobileMoneyRequest.Customer = customer;
                                                        recipientObj.BankCode = "402";
                                                    }
                                                    else if (request.IsdCode == "+245")
                                                    {
                                                        _mobileMoneyRequest.Country = "GW";
                                                        _mobileMoneyRequest.Customer = customer;
                                                    }
                                                    //apikey = ezipay
                                                    var requ = new PayServicesMoneyAggregatoryRequest
                                                    {
                                                        ApiKey = ThirdPartyAggragatorSettings.ApiKey,
                                                        Amount = _commission.TransactionAmount.ToString(),
                                                        Customer = _mobileMoneyRequest.Customer,
                                                        TransactionId = _mobileMoneyRequest.TransactionId
                                                    };
                                                    _mobileMoneyRequest.ApiKey = ThirdPartyAggragatorSettings.ApiKey;
                                                    _mobileMoneyRequest.Signature = new CommonMethods().Sha256Hash(requ);

                                                    ////sender details add to requets api
                                                    var dt = Convert.ToDateTime(request.SenderDateofbirth);
                                                    senderObj.address = request.SenderAddress;
                                                    senderObj.city = request.SenderCity;
                                                    senderObj.dateofBirth = dt.ToString("yyyy-MM-dd");
                                                    senderObj.idNumber = request.SenderIdNumber;
                                                    senderObj.idType = request.SenderIdType;
                                                    senderObj.email = sender.EmailId;
                                                    senderObj.firstName = sender.FirstName;
                                                    senderObj.surname = sender.LastName;                                                    
                                                    senderObj.contact = sender.MobileNo;
                                                    senderObj.country = senderIsdCode.CountryCode;

                                                    recipientObj.firstName = request.ReceiverFirstName;
                                                    recipientObj.surname = request.ReceiverLastName;
                                                    recipientObj.contact = request.ReceiverMobileNo;
                                                    recipientObj.email = request.ReceiverEmail;
                                                    

                                                    _mobileMoneyRequest.Sender = senderObj;
                                                    _mobileMoneyRequest.Recipient = recipientObj;

                                                    //check 
                                                    int senderRequest = await _transferToBankRepository.SaveSenderDetailsRequest(request);
                                                    if (senderRequest != 1)
                                                    {
                                                        response.RstKey = 3;
                                                        response.Message = AggregatoryMESSAGE.FAILED;
                                                        return response;
                                                    }

                                                    //  SHA2Hash
                                                    string RequestString = JsonConvert.SerializeObject(_mobileMoneyRequest);

                                                    #endregion


                                                }
                                                #endregion
                                                var req = JsonConvert.SerializeObject(_mobileMoneyRequest);

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
                                                transationInitiate.CreatedDate = DateTime.UtcNow;
                                                transationInitiate.UpdatedDate = DateTime.UtcNow;
                                                transationInitiate.IsActive = true;
                                                transationInitiate.IsDeleted = false;
                                                transationInitiate.JsonRequest = req;
                                                transationInitiate.JsonResponse = "";
                                                transationInitiate.ReceiverCurrentBalance = _commission.UpdatedCurrentBalance.ToString();
                                                transationInitiate.ReceiverWalletUserId = sender.WalletUserId;
                                                transationInitiate = await _transferToBankRepository.SaveTransactionInitiateRequest(transationInitiate);


                                                //calling pay method insert data in Database
                                                data.CurrentBalance = Convert.ToString(_commission.UpdatedCurrentBalance);
                                                await _walletUserRepository.UpdateUserDetail(data);

                                                //#endregion
                                                string apiUrl = ThirdPartyAggragatorSettings.AddMobileMoney;

                                                //string responseString = "";
                                                //hit 
                                                if (WalletService.HttpVerbs.ToLower() == AggragatorServiceVerbs.HttpPostVerb.ToLower())
                                                {
                                                    var jsonReq = JsonConvert.SerializeObject(_mobileMoneyRequest);
                                                    var responseData = await new CommonApi().PaymentMobileMon(jsonReq, apiUrl);
                                                    responseString = responseData;
                                                }
                                                else
                                                {

                                                    var responseData = Task.Run(() => HttpGetUrlEncodedServiceForMobileMoney(LogTransactionNameTypes.TransferToBank, apiUrl, _mobileMoneyRequest, request, request.channel));
                                                    responseData.Wait();
                                                    responseString = responseData.Result.ToString();
                                                }
                                                var TransactionInitial = await _transferToBankRepository.GetTransactionInitiateRequest(transationInitiate.Id);
                                                TransactionInitial.JsonResponse = "TransferToBank Response" + responseString;
                                                TransactionInitial = await _transferToBankRepository.UpdateTransactionInitiateRequest(TransactionInitial);
                                                LogTransactionTypes.Response.SaveTransactionLog(LogTransactionNameTypes.TransferToBank + subcategory.CategoryName, responseString, "Aggregator Url : ", sender.WalletUserId);
                                                var errorResponse = "The remote server returned an error: (500) Internal Server Error.";
                                                if (!string.IsNullOrEmpty(responseString) && responseString != errorResponse)
                                                {
                                                    var _responseModel = JsonConvert.DeserializeObject<AddMoneyAggregatorResponse>(responseString);

                                                    if (_responseModel != null && !string.IsNullOrEmpty(_responseModel.StatusCode) && (_responseModel.StatusCode == AggregatorySTATUSCODES.SUCCESSFUL || _responseModel.StatusCode == AggregatorySTATUSCODES.PENDING || _responseModel.StatusCode == AggregatorySTATUSCODES.FAILED || _responseModel.StatusCode == AggregatorySTATUSCODES.EXCEPTION))
                                                    {
                                                        var _tranDate = DateTime.UtcNow;
                                                        _responseModel.FormatedTransactionDate = string.Format("{0:d}", DateTime.Now) + "" + string.Format("{0:T}", DateTime.Now);

                                                        _responseModel.AccountNo = request.customer;
                                                        _responseModel.MobileNo = request.customer;

                                                        _responseModel.Amount = request.Amount;
                                                        _responseModel.TransactionDate = _tranDate;
                                                        _responseModel.CurrentBalance = data.CurrentBalance;

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
                                                        tran.TransactionInitiateRequestId = transationInitiate.Id;
                                                        tran.AccountNo = customer;// string.Empty;                                                  
                                                        tran.BankTransactionId = string.Empty;
                                                        tran.IsBankTransaction = false;
                                                        tran.BankBranchCode = string.Empty;
                                                        tran.TransactionId = _responseModel.TransactionId;
                                                        response.TransactionId = tran.TransactionId;
                                                        int _TransactionStatus = 0;
                                                        if (_responseModel.StatusCode == AggregatorySTATUSCODES.SUCCESSFUL)
                                                        {
                                                            _TransactionStatus = (int)TransactionStatus.Completed;

                                                            //-------------sending email after success transaction-----------------
                                                            try
                                                            {
                                                                string filename = CommonSetting.successfullTransaction;
                                                                var FirstName = AES256.Decrypt(sender.PrivateKey, data.FirstName);
                                                                var LastName = AES256.Decrypt(sender.PrivateKey, data.LastName);
                                                                var EmailId = sender.EmailId;//AES256.Decrypt(sender.PrivateKey, data.EmailId);
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
                                                            var refundAmt = Convert.ToDecimal(data.CurrentBalance) + _commission.AmountWithCommission;
                                                            data.CurrentBalance = Convert.ToString(refundAmt);
                                                            await _walletUserRepository.UpdateUserDetail(data);

                                                            var _transactionInitial = await _transferToBankRepository.GetTransactionInitiateRequest(transationInitiate.Id);
                                                            _transactionInitial.AfterTransactionBalance = data.CurrentBalance;
                                                            _transactionInitial.ReceiverCurrentBalance = data.CurrentBalance;
                                                            await _transferToBankRepository.UpdateTransactionInitiateRequest(_transactionInitial);

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
                                                        //if (WalletService.ServiceCategoryId == 6)
                                                        //{
                                                        //    tran.OperatorType = "sample";
                                                        //}
                                                        tran.IsdCode = request.IsdCode;
                                                        _responseModel.CurrentBalance = Convert.ToString(_commission.UpdatedCurrentBalance);
                                                        //   db.WalletTransactions.Add(tran);

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
                                                            tran = await _transferToBankRepository.WalletTransactionSave(tran);
                                                        }
                                                        catch (Exception ex)
                                                        {

                                                        }
                                                        //calling pay method insert data in Database
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
                                                    tran.InvoiceNo = invoiceNumber.InvoiceNumber;
                                                    tran.Comments = request.Comment;
                                                    tran.MerchantCommissionId = _commission.MerchantCommissionId;
                                                    tran.MerchantCommissionAmount = Convert.ToString(_commission.MerchantCommissionAmount);
                                                    tran.CommisionId = _commission.CommissionId;
                                                    tran.CommisionAmount = Convert.ToString(_commission.CommissionAmount);
                                                    tran.CommisionPercent = _commission.CommissionAmount.ToString();
                                                    tran.TotalAmount = Convert.ToString(_commission.AmountWithCommission);
                                                    tran = await _transferToBankRepository.WalletTransactionSave(tran);
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
                                            response.RstKey = 12;
                                            response.Message = ResponseMessages.REQUESTDATA_NOT_EXIST;
                                        }
                                    }
                                    else
                                    {
                                        response.RstKey = 12;
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
                            response.MobileNo = request.customer;
                            response.ToMobileNo = request.customer;
                            response.StatusCode = AggregatorySTATUSCODES.SUCCESSFUL;
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
                    response.Message = ResponseMessages.EMAIL_VERIFICATION_PENDING;// "Please verify your email id.";
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

                        var resData = JsonConvert.DeserializeObject<dynamic>(responseString);
                        if (responseString == null || responseString == "" || resData.StatusCode == AggregatorySTATUSCODES.INVOICEEXIST)
                        {
                            responseString = "{\"StatusCode\":\"300\",\"Message\":\"PENDING\",\"TransactionId\":\"\",\"InvoiceNo\":\"\"}";
                        }
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


        //public async Task<TransferFundResponse> PayMoneyTransferToBank(TransferFundRequest request, string sessionToken)
        //{
        //    var response = new TransferFundResponse();
        //    var _commissionRequest = new CalculateCommissionRequest();
        //    var _commission = new CalculateCommissionResponse();
        //    var transationInitiate = new TransactionInitiateRequest();

        //    var sender = await _walletUserRepository.GetUserDetailById(request.WalletUserId);
        //    var data = await _walletUserRepository.GetCurrentUser(request.WalletUserId);
        //    //var WalletService = await _masterDataRepository.GetWalletServicesByIdOrChannel(request.channel, request.ServiceCategoryId, request.IsdCode);
        //    var WalletService = await _transferToBankRepository.GetBankDetail(request.bankCode);
        //    var subcategory = await _masterDataRepository.GetWalletSubCategoriesById(6);
        //    bool IsdocVerified = await _walletUserRepository.IsDocVerifiedMOMO((int)sender.DocumetStatus);
        //    var transactionLimit = await _payMoneyRepository.GetTransactionLimitForPayment(request.WalletUserId);

        //    int limit = transactionLimit != null ? Convert.ToInt32(transactionLimit.transactionlimit) : 0;
        //    var transactionHistory = await _payMoneyRepository.GetAllTransactionByDate(request.WalletUserId);
        //    int totalAmountTransfered = transactionHistory != null ? Convert.ToInt32(transactionHistory.totalAmount) : 0;
        //    // string customer = request.customer;//.Length.ToString();
        //    //------Get Currency Rate--------------
        //    var currencyDetail = _masterDataRepository.GetCurrencyRate();

        //    decimal NGNRate = Convert.ToDecimal(currencyDetail.NGNRate);
        //    decimal CfaRate = Convert.ToDecimal(currencyDetail.CfaRate);
        //    decimal requestAmount = Convert.ToDecimal(request.amount);// / dollarValue;
        //    var _beneficiaryResponce = new TransferFundResponse();
        //    try
        //    {

        //        if (sender.IsOtpVerified == true) //mobile exist or not then 
        //        {
        //            if (sender.IsEmailVerified == true)
        //            {
        //                if (subcategory != null)
        //                {
        //                    request.serviceCategory = subcategory.CategoryName;//

        //                    if (WalletService != null)
        //                    {

        //                        var adminKeyPair = AES256.AdminKeyPair;
        //                        if (sender.IsDisabledTransaction == false)
        //                        {
        //                            if (IsdocVerified == true)
        //                            {
        //                                if (transactionLimit == null || limit >= (Convert.ToDecimal(request.amount) + totalAmountTransfered))
        //                                {
        //                                    if (sender != null && !string.IsNullOrEmpty(sender.MobileNo))
        //                                    {
        //                                        if (!string.IsNullOrEmpty(sender.CurrentBalance) && !sender.CurrentBalance.IsZero() && Convert.ToDecimal(sender.CurrentBalance) > 0)
        //                                        {
        //                                            _commissionRequest.CurrentBalance = Convert.ToDecimal(sender.CurrentBalance);
        //                                            _commissionRequest.IsRoundOff = true;
        //                                            _commissionRequest.TransactionAmount = Convert.ToDecimal(request.amount);
        //                                            _commissionRequest.WalletServiceId = WalletService.WalletServiceId;
        //                                            _commission = await _setCommisionRepository.CalculateCommission(_commissionRequest);

        //                                        }
        //                                        else
        //                                        {
        //                                            response.RstKey = 6;
        //                                            response.message = ResponseMessages.INSUFICIENT_BALANCE;
        //                                            return response;
        //                                        }



        //                                        decimal amountWithCommision = _commission.AmountWithCommission; //xof
        //                                        decimal currentBalance = Convert.ToDecimal(sender.CurrentBalance);
        //                                        decimal transferAmount = Convert.ToDecimal(_commission.AmountWithCommission); //chk total amount that will deduct through account
        //                                                                                                                      //chk transferAmount
        //                                        if (transferAmount > 0 && Convert.ToDecimal(sender.CurrentBalance) >= transferAmount)
        //                                        {

        //                                            if (currentBalance > 0 && currentBalance >= amountWithCommision && _commission.CurrentBalance > 0 && _commission.CurrentBalance >= amountWithCommision)
        //                                            {

        //                                                var invoiceNumber1 = await _masterDataRepository.GetInvoiceNumber();
        //                                                var txninijson = JsonConvert.SerializeObject(request);
        //                                                //This is for transaction initiate request all---
        //                                                transationInitiate.InvoiceNumber = invoiceNumber1.InvoiceNumber;
        //                                                transationInitiate.ReceiverNumber = request.crAccount; //accountno
        //                                                transationInitiate.ServiceName = WalletService.ServiceName;
        //                                                transationInitiate.RequestedAmount = _commission.TransactionAmount.ToString();
        //                                                transationInitiate.TransactionStatus = (int)TransactionStatus.Pending;
        //                                                transationInitiate.WalletUserId = sender.WalletUserId;
        //                                                transationInitiate.UserReferanceNumber = invoiceNumber1.AutoDigit;
        //                                                transationInitiate.CurrentBalance = sender.CurrentBalance;
        //                                                transationInitiate.AfterTransactionBalance = _commission.UpdatedCurrentBalance.ToString();
        //                                                transationInitiate.ReceiverCurrentBalance = _commission.UpdatedCurrentBalance.ToString();
        //                                                transationInitiate.ReceiverWalletUserId = sender.WalletUserId;
        //                                                transationInitiate.UserName = sender.FirstName + " " + sender.LastName;
        //                                                transationInitiate.CreatedDate = DateTime.UtcNow;
        //                                                transationInitiate.UpdatedDate = DateTime.UtcNow;
        //                                                transationInitiate.IsActive = true;
        //                                                transationInitiate.IsDeleted = false;
        //                                                transationInitiate.JsonRequest = txninijson;
        //                                                transationInitiate.JsonResponse = "";
        //                                                //save txninititaterequest
        //                                                transationInitiate = await _transferToBankRepository.SaveTransactionInitiateRequest(transationInitiate);
        //                                                //Update user's currentbalance amount from wallet
        //                                                var UpdatedCurrentBalance = Convert.ToString(_commission.UpdatedCurrentBalance);
        //                                                //calling pay method insert data in Database
        //                                                await _transferToBankRepository.UpdateCurrentBalance(UpdatedCurrentBalance, sender.WalletUserId);

        //                                                // var responseString = GetWebClient(apiUrl, GetUrl);
        //                                                //var invoiceNumber = await _masterDataRepository.GetInvoiceNumber(12);
        //                                                //Twelve Digit numeric only  save 
        //                                                var _transferToBankRequest = new TransferToBankRequest1();
        //                                                _transferToBankRequest.DebitAcctNumber = request.drAccount;
        //                                                _transferToBankRequest.CreditAcctNumber = request.crAccount;
        //                                                _transferToBankRequest.CreditAcctName = request.crAccountName;
        //                                                _transferToBankRequest.Amount = _commission.AmountWithCommission.ToString(); //total amount 

        //                                                _transferToBankRequest.BankCode = request.bankCode;
        //                                                _transferToBankRequest.bankName = request.bankName;
        //                                                _transferToBankRequest.Remarks = request.remarks;

        //                                                _transferToBankRequest.CategoryCode = request.categoryCode;
        //                                                _transferToBankRequest.WalletNo = request.walletNo;
        //                                                _transferToBankRequest.Narration = request.narration;


        //                                                _transferToBankRequest.CreatedDate = DateTime.UtcNow;
        //                                                _transferToBankRequest.UpdatedDate = DateTime.UtcNow;
        //                                                //save txn initiate request to TransferToBankRequest table
        //                                                _transferToBankRequest = await _transferToBankRepository.SaveTransactionTransferToBankRequest(_transferToBankRequest);
        //                                                #region prod url
        //                                                //string url = TransferToBankApiSetting.TransferToBankUrl + TransferToBankApiMethodList.BeneficiaryName;
        //                                                //HttpWebRequest beneficiaryrequest = (HttpWebRequest)WebRequest.Create(url);

        //                                                ////Trust all certificates
        //                                                //System.Net.ServicePointManager.ServerCertificateValidationCallback =
        //                                                //    ((sender, certificate, chain, sslPolicyErrors) => true);

        //                                                //// trust sender                                                           

        //                                                //// validate cert by calling a function
        //                                                //ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(System.Net.ServicePointManager.ServerCertificateValidationCallback);

        //                                                //beneficiaryrequest.ContentType = "application/json; charset=utf-8";
        //                                                //beneficiaryrequest.Method = "POST";

        //                                                //string svcCredentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(TransferToBankApiSetting.Username + ":" + TransferToBankApiSetting.Password));
        //                                                //beneficiaryrequest.Headers.Add("Authorization", "Basic " + svcCredentials);
        //                                                //beneficiaryRequest requestData = new beneficiaryRequest();
        //                                                //requestData.SrcAcctName = TransferToBankApiSetting.SourceAccountName;
        //                                                //requestData.SrcAcctNumber = TransferToBankApiSetting.SourceAccountNo;
        //                                                //requestData.DestAcctNumber = request.DestAcctNumber;
        //                                                ////requestData.de = request.DestAcctName;
        //                                                //requestData.DestBankCode = request.DestBankCode;
        //                                                //requestData.ServiceTransId = request.ServiceTransId;
        //                                                //string postData = JsonConvert.SerializeObject(requestData);
        //                                                #endregion

        //                                                #region dev 
        //                                                //web.config ->app.setting
        //                                                string url = "https://services.staging.innovectives.group/api/v1/wws/transactions/funds-transfer";

        //                                                string token = "Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6IjEyYmQ5NTRiM2I2OGM4YjI0MzU2MWIyOTQ2OTAwOThlIiwidHlwIjoiSldUIn0.eyJpc3MiOiJhcm46aW52OmlkZW50aXR5Ojo6IiwiYXVkIjpbIkJPZGcwUVBnZnBzNER0QUZrUWxES2JISiJdLCJpYXQiOjE2MTI1MDg4MjAsImV4cCI6MTYxMjU0ODQyMCwiYXV0aF90aW1lIjoxNjEyNTA4ODE3LCJhdF9oYXNoIjoicU5qUXoza3Y3LUZvVWZYcExNSUxwdyIsInN1YiI6IjViYmI4MjVhLTJjMzMtNDc4YS04Mzk2LTgwNzk1YTg2MGNjOCIsImVtYWlsIjoicHJvZmVtenlAZ21haWwuY29tIiwicGhvbmUiOiIwODAzMzU3NDc3OCIsImVtYWlsX3ZlcmlmaWVkIjpmYWxzZSwicHJvZmlsZSI6InByb2ZpbGVfdXJsX2hlcmUiLCJhenAiOiJCT2RnMFFQZ2ZwczREdEFGa1FsREtiSEoifQ.SBOqS9j4HWTwBVBTnuts_ntwzpx9MZeDuDkUFyWpEZ5XQM9YkwLgGlgSQCyOnQSDDsqJd6EClcsfAbllQz62VkwodIDX_RZTqcJJqnY7QophBlwt8XUpN2MTW9OdqwJavOj4yN9LQ1kXMtjbTi9GzHGVC8ggelg3luz2Gb07XKyf-FAHlBoEalCMKa89Kpr4aml4KiQKYIrb8k2GdehGUih6ngqKSn_aOjiwMMAh4EbxHyEsZtj5ADWz6sO4bG7ASj5bgn-efUfookvrJPrcnLhaYxcT0MwI2NMNFZU4j-S6B4dK_Em9VqmEJ0cOU8vKWtpq05WzRCfzzH7nnqynuw";
        //                                                HttpWebRequest request1 = (HttpWebRequest)WebRequest.Create(url);
        //                                                request1.ContentType = "application/json; charset=utf-8";
        //                                                request1.Headers["Authorization"] = token;
        //                                                request1.Method = "POST";
        //                                                //change to appsetting 
        //                                                // string svcCredentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes("claretyoung20@gmail.com" + ":" + "g?4QKuxa"));
        //                                                //request.Headers.Add("Authorization", "Basic " + svcCredentials);
        //                                                request1.PreAuthenticate = true;


        //                                                decimal XOFtoNGNCurrencyConversiontransferAmount = (_commission.AmountWithCommission * NGNRate);           // / cediRate; request.amount;
        //                                                //amount >=1 xof
        //                                                TransferFundRequest requestdatamodel = new TransferFundRequest();
        //                                                requestdatamodel.drAccount = request.drAccount;
        //                                                requestdatamodel.crAccount = request.crAccount;
        //                                                requestdatamodel.crAccountName = request.crAccountName;
        //                                                //xof to NGN when sent amount to fundtransfer api
        //                                                requestdatamodel.amount = XOFtoNGNCurrencyConversiontransferAmount.ToString();
        //                                                requestdatamodel.bankCode = request.bankCode;
        //                                                requestdatamodel.bankName = request.bankName;

        //                                                requestdatamodel.remarks = request.remarks;
        //                                                requestdatamodel.categoryCode = request.categoryCode;
        //                                                requestdatamodel.crAccountName = request.crAccountName;
        //                                                requestdatamodel.walletNo = request.walletNo;
        //                                                requestdatamodel.narration = request.narration;

        //                                                string postData = JsonConvert.SerializeObject(requestdatamodel);
        //                                                #endregion
        //                                                LogTransactionTypes.Request.SaveTransactionLog(LogTransactionNameTypes.TransferToBank + " Api: TransferFundRequest, Branch Code: " + requestdatamodel.bankCode, postData, "TransferFundRequest Request URL : " + url);



        //                                                using (var streamWriter = new StreamWriter(request1.GetRequestStream())) //
        //                                                {
        //                                                    streamWriter.Write(postData);
        //                                                    streamWriter.Flush();
        //                                                    streamWriter.Close();
        //                                                }
        //                                                //hit api
        //                                                try
        //                                                {
        //                                                    using (var bankDetailResponse = (HttpWebResponse)request1.GetResponse())
        //                                                    {
        //                                                        using (var reader = new StreamReader(bankDetailResponse.GetResponseStream()))
        //                                                        {
        //                                                            var objText = reader.ReadToEnd();

        //                                                            //deserlise response
        //                                                            _beneficiaryResponce = JsonConvert.DeserializeObject<TransferFundResponse>(objText);
        //                                                            //when response succesfujl return then amount in kobo not NGN;KOBO to NGN then NGN to XOF

        //                                                            decimal _AmountinKOBO = Convert.ToDecimal(_beneficiaryResponce.amount);// / kobo 
        //                                                                                                                                   //KOBO to NGN
        //                                                            decimal _AmountinNGN = (_AmountinKOBO / 100);
        //                                                            //NGN to XOF
        //                                                            decimal _AmountinXOF = (_AmountinNGN * CfaRate);

        //                                                            _beneficiaryResponce.amount = _AmountinXOF.ToString();

        //                                                            //update initial transaction response
        //                                                            //UpdateTransactionInitiateRequest Response
        //                                                            var TransactionInitial = await _transferToBankRepository.GetTransactionInitiateRequest(transationInitiate.Id);
        //                                                            TransactionInitial.JsonResponse = "Transfer to bank Response" + objText;
        //                                                            await _transferToBankRepository.UpdateTransactionInitiateRequest(TransactionInitial);
        //                                                        }
        //                                                    }
        //                                                }
        //                                                catch(Exception ex)
        //                                                {

        //                                                }

        //                                                DateTime date = DateTime.UtcNow;
        //                                                var Userdata = await _walletUserService.UserProfile(sessionToken); //get user profile by token

        //                                                if (_beneficiaryResponce != null && (_beneficiaryResponce.transactionStatus == "success" || _beneficiaryResponce.responsecode == "00"))
        //                                                {


        //                                                    TransferToBankResponse1 _transaction = new TransferToBankResponse1();
        //                                                    _transaction.AmountXOF = Convert.ToString(_beneficiaryResponce.amount);
        //                                                    _transaction.ResponseDescription = _beneficiaryResponce.responseDescription;
        //                                                    _transaction.TransactionDate = _beneficiaryResponce.transactionDate;
        //                                                    _transaction.TransactionStatus = _beneficiaryResponce.transactionStatus;
        //                                                    _transaction.Responsecode = _beneficiaryResponce.responsecode;
        //                                                    _transaction.Requestdatetime = _beneficiaryResponce.requestdatetime;
        //                                                    _transaction.AccountToDebit = _beneficiaryResponce.drAccount;
        //                                                    _transaction.bankname = _beneficiaryResponce.bankname;
        //                                                    _transaction.NameToCredit = _beneficiaryResponce.crAccountName;
        //                                                    _transaction.AccountToCredit = _beneficiaryResponce.crAccount;
        //                                                    _transaction.Reference = _beneficiaryResponce.reference;
        //                                                    _transaction.Narration = _beneficiaryResponce.narration;
        //                                                    _transaction.Remarks = _beneficiaryResponce.remarks;
        //                                                    _transaction.TransferToBankRequestId = _transferToBankRequest.TransferToBankRequestId;
        //                                                    _transaction.CreatedDate = DateTime.UtcNow;
        //                                                    _transaction.UpdatedDate = DateTime.UtcNow;
        //                                                    var ff = await _transferToBankRepository.SaveTransactionTransferToBankResponse(_transaction);

        //                                                    WalletTransaction wt = new WalletTransaction();
        //                                                    wt.TransactionInitiateRequestId = transationInitiate.Id;
        //                                                    wt.WalletAmount = Convert.ToString(_commission.TransactionAmount);
        //                                                    wt.CommisionId = _commission.CommissionId;
        //                                                    wt.CommisionAmount = Convert.ToString(_commission.CommissionAmount);
        //                                                    wt.ServiceTax = Convert.ToString(_commission.ServiceTaxAmount);
        //                                                    wt.ServiceTaxRate = _commission.ServiceTaxRate;
        //                                                    wt.AccountNo = request.crAccount;
        //                                                    wt.IsBankTransaction = true;
        //                                                    wt.CreatedDate = date;
        //                                                    wt.UpdatedDate = date;
        //                                                    wt.IsdCode = request.Countrycode;
        //                                                    //Self Account 
        //                                                    wt.ReceiverId = Userdata.WalletUserId;
        //                                                    wt.WalletServiceId = WalletService.WalletServiceId;
        //                                                    wt.TransactionType = AggragatorServiceType.CREDIT;
        //                                                    wt.TransactionTypeInfo = (int)TransactionTypeInfo.EWalletToBankTransactions;
        //                                                    wt.SenderId = Userdata.WalletUserId;
        //                                                    wt.TotalAmount = Convert.ToString(_commission.AmountWithCommission);
        //                                                    wt.TransactionId = response.id;
        //                                                    wt.BankTransactionId = response.id;
        //                                                    wt.TransactionStatus = (int)TransactionStatus.Completed;
        //                                                    wt.IsAdminTransaction = true;
        //                                                    wt.IsActive = true;
        //                                                    wt.IsDeleted = false;
        //                                                    wt.InvoiceNo = invoiceNumber1.InvoiceNumber;
        //                                                    //wt.BeneficiaryName = request.;
        //                                                    wt.Comments = request.remarks;
        //                                                    //  wt.Comments = request.SrcAcctName;
        //                                                    wt.MerchantCommissionAmount = Convert.ToString(_commission.MerchantCommissionAmount);
        //                                                    wt.MerchantCommissionId = _commission.MerchantCommissionId;
        //                                                    wt.FlatCharges = _commission.FlatCharges.ToString();
        //                                                    wt.BenchmarkCharges = _commission.BenchmarkCharges.ToString();
        //                                                    wt.CommisionPercent = _commission.CommisionPercent.ToString();
        //                                                    //take id 
        //                                                    // var TransactionInitial = await _transferToBankRepository.GetTransactionInitiateRequest(transationInitiate.Id);
        //                                                    //TransactionInitial.AfterTransactionBalance = CurrentUser.CurrentBalance.ToString();

        //                                                    //await _transferToBankRepository.UpdateTransactionInitiateRequest(TransactionInitial);
        //                                                    await _transferToBankRepository.SaveWalletTransaction(wt);
        //                                                    if (wt.TransactionStatus == (int)TransactionStatus.Completed)
        //                                                    {
        //                                                        //Userdata.CurrentBalance = Convert.ToString(_commission.UpdatedCurrentBalance);
        //                                                        //var walletUser = db.WalletUsers.Where(x => x.WalletUserId == Userdata.WalletUserId).FirstOrDefault();
        //                                                        //if (walletUser != null)
        //                                                        //{
        //                                                        //    walletUser.CurrentBalance = Convert.ToString(_commission.UpdatedCurrentBalance);// Userdata.CurrentBalance;
        //                                                        //}
        //                                                        //db.SaveChanges();
        //                                                        #region PushNotification
        //                                                        if (Userdata.DeviceToken != null && Userdata.DeviceType != 0)
        //                                                        {
        //                                                            PayMoneyPushModel pushModel = new PayMoneyPushModel();
        //                                                            pushModel.TransactionId = response.id;
        //                                                            pushModel.TransactionDate = date;
        //                                                            pushModel.alert = Convert.ToString(_commission.AmountWithCommission) + " XOF has been debited from your account.";
        //                                                            pushModel.Amount = Convert.ToString(_commission.AmountWithCommission);
        //                                                            pushModel.CurrentBalance = Userdata.CurrentBalance;
        //                                                            pushModel.pushType = (int)PushType.TRANSFERTOBANK;
        //                                                            pushModel.AccountNo = wt.AccountNo;

        //                                                            PushNotificationModel push = new PushNotificationModel();
        //                                                            push.deviceType = (int)Userdata.DeviceType;
        //                                                            push.deviceKey = Userdata.DeviceToken;
        //                                                            if ((int)Userdata.DeviceType == (int)DeviceTypes.ANDROID || (int)Userdata.DeviceType == (int)DeviceTypes.Web)
        //                                                            {
        //                                                                PushPayload<PayMoneyPushModel> aps = new PushPayload<PayMoneyPushModel>();
        //                                                                PushPayloadData<PayMoneyPushModel> _data = new PushPayloadData<PayMoneyPushModel>();
        //                                                                _data.notification = pushModel;
        //                                                                aps.data = _data;
        //                                                                aps.to = Userdata.DeviceToken;
        //                                                                aps.collapse_key = string.Empty;
        //                                                                push.message = JsonConvert.SerializeObject(aps);
        //                                                                push.payload = pushModel;

        //                                                            }
        //                                                            if ((int)Userdata.DeviceType == (int)DeviceTypes.IOS)
        //                                                            {
        //                                                                NotificationJsonResponse<PayMoneyIOSPushModel> aps = new NotificationJsonResponse<PayMoneyIOSPushModel>();
        //                                                                PayMoneyIOSPushModel _iosPushModel = new PayMoneyIOSPushModel();
        //                                                                _iosPushModel.alert = pushModel.alert;
        //                                                                _iosPushModel.Amount = pushModel.Amount;
        //                                                                _iosPushModel.CurrentBalance = pushModel.CurrentBalance;
        //                                                                _iosPushModel.MobileNo = pushModel.MobileNo;
        //                                                                _iosPushModel.SenderName = pushModel.SenderName;
        //                                                                _iosPushModel.pushType = pushModel.pushType;
        //                                                                aps.aps = _iosPushModel;

        //                                                                push.message = JsonConvert.SerializeObject(aps);
        //                                                            }
        //                                                            if (!string.IsNullOrEmpty(push.message))
        //                                                            {

        //                                                                //bool IsSuccess = new PushNotificationRepository().sendPushNotification(push);

        //                                                                try
        //                                                                {

        //                                                                    //var AdminKeys = AES256.AdminKeyPair;
        //                                                                    //string FirstName = AES256.Decrypt(Userdata.PrivateKey, Userdata.FirstName);
        //                                                                    //string LastName = AES256.Decrypt(Userdata.PrivateKey, Userdata.LastName);
        //                                                                    //string StdCode = Userdata.StdCode;
        //                                                                    //string MobileNo = AES256.Decrypt(AdminKeys.PrivateKey, Userdata.MobileNo);
        //                                                                    //string EmailId = AES256.Decrypt(AdminKeys.PrivateKey, Userdata.EmailId).Trim().ToLower();

        //                                                                    if (Userdata.EmailId != null)
        //                                                                    {
        //                                                                        string filename = CommonSetting.successfullTransaction;
        //                                                                        var body = _sendEmails.ReadEmailformats(filename);
        //                                                                        body = body.Replace("$$FirstName$$", Userdata.FirstName + " " + Userdata.LastName);
        //                                                                        body = body.Replace("$$DisplayContent$$", WalletService.ServiceName);
        //                                                                        body = body.Replace("$$customer$$", response.crAccount);
        //                                                                        body = body.Replace("$$amount$$", "XOF " + request.amount);
        //                                                                        body = body.Replace("$$ServiceTaxAmount$$", "XOF " + _commission.CommissionAmount);
        //                                                                        body = body.Replace("$$AmountWithCommission$$", "XOF " + _commission.AmountWithCommission);
        //                                                                        body = body.Replace("$$TransactionId$$", Convert.ToString(response.id));
        //                                                                        var req = new EmailModel
        //                                                                        {
        //                                                                            TO = Userdata.EmailId,
        //                                                                            Subject = ResponseEmailMessage.PAYMENT_SUCCESS,
        //                                                                            Body = body
        //                                                                        };
        //                                                                        _sendEmails.SendEmail(req);
        //                                                                    }

        //                                                                }
        //                                                                catch
        //                                                                {

        //                                                                }

        //                                                            }
        //                                                        }
        //                                                        #endregion
        //                                                    }


        //                                                    response.amount = request.amount; //FinalAmount;check
        //                                                    response.CurrentBalance = Userdata.CurrentBalance;
        //                                                    //response.DestAcctNumber = wt.AccountNo;
        //                                                    response.id = wt.TransactionId;
        //                                                    response.DocStatus = IsdocVerified;
        //                                                    ////response.DestBankCode = _result.gipTransaction.DestBank;
        //                                                    //response.transactionDate = DateTime.UtcNow;
        //                                                    response.message = ResponseMessages.TRANSACTION_SUCCESS;
        //                                                    response.RstKey = 1;
        //                                                }
        //                                                else if (_beneficiaryResponce != null && _beneficiaryResponce.transactionStatus == "pending")
        //                                                {
        //                                                    //var Userdata = new AppUserRepository().UserProfile();
        //                                                    //transationInitiate = await _transferToBankRepository.GetTransactionInitiateRequest(transationInitiate.Id);
        //                                                    ////calling pay method insert data in Database
        //                                                    //var Userdatadetail = await _walletUserService.UserProfile(sessionToken); //get user profile by token
        //                                                    //var refundAmt = Convert.ToDecimal(Userdatadetail.CurrentBalance) + _commission.AmountWithCommission;
        //                                                    //Userdatadetail.CurrentBalance = Convert.ToString(refundAmt);
        //                                                    //transationInitiate.AfterTransactionBalance = Userdatadetail.CurrentBalance;
        //                                                    //transationInitiate.ReceiverCurrentBalance = Userdatadetail.CurrentBalance;

        //                                                    //await _transferToBankRepository.UpdateTransactionInitiateRequest(transationInitiate);
        //                                                    //await _walletUserRepository.UpdateUserDetail(data);
        //                                                    //Update user's currentbalance amount from wallet

        //                                                    //await _transferToBankRepository.UpdateCurrentBalance(Userdatadetail.CurrentBalance, Userdatadetail.WalletUserId);

        //                                                    //_responseModel.StatusCode = AggregatorySTATUSCODES.FAILED;

        //                                                    var wt = new WalletTransaction();
        //                                                    wt.WalletAmount = Convert.ToString(_commission.TransactionAmount);
        //                                                    wt.TransactionInitiateRequestId = transationInitiate.Id;
        //                                                    wt.CommisionId = _commission.CommissionId;
        //                                                    wt.CommisionAmount = Convert.ToString(_commission.CommissionAmount);
        //                                                    wt.ServiceTax = Convert.ToString(_commission.ServiceTaxAmount);
        //                                                    wt.ServiceTaxRate = _commission.ServiceTaxRate;
        //                                                    wt.AccountNo = request.crAccount;
        //                                                    wt.IsBankTransaction = true;
        //                                                    //wt.BankBranchCode = _result.gipTransaction.DestBank;
        //                                                    wt.CreatedDate = date;
        //                                                    wt.UpdatedDate = date;
        //                                                    wt.IsdCode = request.Countrycode;
        //                                                    //Self Account 
        //                                                    wt.ReceiverId = Userdata.WalletUserId;
        //                                                    wt.WalletServiceId = WalletService.WalletServiceId;
        //                                                    wt.TransactionType = AggragatorServiceType.CREDIT;
        //                                                    wt.TransactionTypeInfo = (int)TransactionTypeInfo.EWalletToBankTransactions;
        //                                                    wt.SenderId = Userdata.WalletUserId;
        //                                                    wt.TotalAmount = Convert.ToString(_commission.AmountWithCommission);
        //                                                    wt.TransactionId = response.id;
        //                                                    wt.BankTransactionId = response.id;
        //                                                    //wt.TransactionStatus = 2;
        //                                                    wt.TransactionStatus = (int)TransactionStatus.Pending;
        //                                                    wt.IsAdminTransaction = true;
        //                                                    wt.IsActive = true;
        //                                                    wt.IsDeleted = false;

        //                                                    wt.InvoiceNo = invoiceNumber1.InvoiceNumber;
        //                                                    wt.Comments = request.remarks;
        //                                                    wt.MerchantCommissionAmount = Convert.ToString(_commission.MerchantCommissionAmount);
        //                                                    wt.MerchantCommissionId = _commission.MerchantCommissionId;
        //                                                    wt.FlatCharges = _commission.FlatCharges.ToString();
        //                                                    wt.BenchmarkCharges = _commission.BenchmarkCharges.ToString();
        //                                                    wt.CommisionPercent = _commission.CommisionPercent.ToString();

        //                                                    await _transferToBankRepository.SaveWalletTransaction(wt);
        //                                                    //response.StatusCode = (int)TransactionStatus.Failed;
        //                                                    response.RstKey = 6;
        //                                                    response.message = "Transaction Pending";
        //                                                    //response.amount = request.amount; //FinalAmount;check
        //                                                    response.CurrentBalance = Userdata.CurrentBalance;


        //                                                }
        //                                                else
        //                                                {
        //                                                    //fail
        //                                                    transationInitiate = await _transferToBankRepository.GetTransactionInitiateRequest(transationInitiate.Id);
        //                                                    //calling pay method insert data in Database
        //                                                    var Userdatadetail = await _walletUserService.UserProfile(sessionToken); //get user profile by token
        //                                                    var refundAmt = Convert.ToDecimal(Userdatadetail.CurrentBalance) + _commission.AmountWithCommission;
        //                                                    Userdatadetail.CurrentBalance = Convert.ToString(refundAmt);
        //                                                    //transationInitiate.AfterTransactionBalance = Userdatadetail.CurrentBalance;
        //                                                    //transationInitiate.ReceiverCurrentBalance = Userdatadetail.CurrentBalance;

        //                                                    await _transferToBankRepository.UpdateTransactionInitiateRequest(transationInitiate);
        //                                                    //await _walletUserRepository.UpdateUserDetail(data);
        //                                                    //Update user's currentbalance amount from wallet

        //                                                    await _transferToBankRepository.UpdateCurrentBalance(Userdatadetail.CurrentBalance, Userdatadetail.WalletUserId);

        //                                                    //_responseModel.StatusCode = AggregatorySTATUSCODES.FAILED;

        //                                                    var wt = new WalletTransaction();
        //                                                    wt.WalletAmount = Convert.ToString(_commission.TransactionAmount);
        //                                                    wt.TransactionInitiateRequestId = transationInitiate.Id;
        //                                                    wt.CommisionId = _commission.CommissionId;
        //                                                    wt.CommisionAmount = Convert.ToString(_commission.CommissionAmount);
        //                                                    wt.ServiceTax = Convert.ToString(_commission.ServiceTaxAmount);
        //                                                    wt.ServiceTaxRate = _commission.ServiceTaxRate;
        //                                                    wt.AccountNo = request.crAccount;
        //                                                    wt.IsBankTransaction = true;
        //                                                    //wt.BankBranchCode = _result.gipTransaction.DestBank;
        //                                                    wt.CreatedDate = date;
        //                                                    wt.UpdatedDate = date;
        //                                                    wt.IsdCode = request.Countrycode;
        //                                                    //Self Account 
        //                                                    wt.ReceiverId = Userdatadetail.WalletUserId;
        //                                                    wt.WalletServiceId = WalletService.WalletServiceId;
        //                                                    wt.TransactionType = AggragatorServiceType.CREDIT;
        //                                                    wt.TransactionTypeInfo = (int)TransactionTypeInfo.EWalletToBankTransactions;
        //                                                    wt.SenderId = Userdatadetail.WalletUserId;
        //                                                    wt.TotalAmount = Convert.ToString(_commission.AmountWithCommission);
        //                                                    wt.TransactionId = response.id;
        //                                                    wt.BankTransactionId = response.id;
        //                                                    wt.TransactionStatus = (int)TransactionStatus.Failed;
        //                                                    wt.IsAdminTransaction = true;
        //                                                    wt.IsActive = true;
        //                                                    wt.IsDeleted = false;

        //                                                    wt.InvoiceNo = invoiceNumber1.InvoiceNumber;
        //                                                    wt.Comments = request.remarks;
        //                                                    wt.MerchantCommissionAmount = Convert.ToString(_commission.MerchantCommissionAmount);
        //                                                    wt.MerchantCommissionId = _commission.MerchantCommissionId;
        //                                                    wt.FlatCharges = _commission.FlatCharges.ToString();
        //                                                    wt.BenchmarkCharges = _commission.BenchmarkCharges.ToString();
        //                                                    wt.CommisionPercent = _commission.CommisionPercent.ToString();

        //                                                    await _transferToBankRepository.SaveWalletTransaction(wt);
        //                                                    //response.StatusCode = (int)TransactionStatus.Failed;
        //                                                    response.RstKey = 6;
        //                                                    response.message = "Transaction Failed";
        //                                                    //response.amount = request.amount; //FinalAmount;check
        //                                                    response.CurrentBalance = Userdatadetail.CurrentBalance;


        //                                                }
        //                                            }


        //                                            else
        //                                            {
        //                                                response.RstKey = 6;
        //                                                response.message = ResponseMessages.INSUFICIENT_BALANCE;
        //                                                return response;
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            response.RstKey = 6;
        //                                            response.message = ResponseMessages.INSUFICIENT_BALANCE;
        //                                            return response;
        //                                        }

        //                                    }
        //                                    else
        //                                    {
        //                                        response.RstKey = 12;
        //                                        response.message = ResponseMessages.USER_NOT_REGISTERED;
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    response.RstKey = 6;
        //                                    response.message = ResponseMessages.INSUFICIENT_BALANCE;
        //                                    return response;
        //                                }

        //                            }

        //                            else if (sender.DocumetStatus == 0 || sender.DocumetStatus == null)
        //                            {
        //                                response.RstKey = 13;
        //                                response.message = ResponseMessageKyc.FAILED_Doc_NotUploaded;
        //                            }
        //                            else if (sender.DocumetStatus == 1 || sender.DocumetStatus == null)
        //                            {
        //                                response.RstKey = 14;
        //                                response.message = ResponseMessageKyc.FAILED_Doc_Pending;
        //                            }
        //                            else if (sender.DocumetStatus == 4 || sender.DocumetStatus == null)
        //                            {
        //                                response.RstKey = 15;
        //                                response.message = ResponseMessageKyc.Doc_Not_visible;
        //                            }
        //                            else
        //                            {
        //                                response.RstKey = 16;
        //                                response.message = ResponseMessageKyc.Doc_Rejected;
        //                            }


        //                        }
        //                        else
        //                        {
        //                            response.RstKey = 17;
        //                            response.message = ResponseMessageKyc.TRANSACTION_DISABLED;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        response.RstKey = 19;
        //                        response.message = ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND;
        //                    }
        //                }

        //                else
        //                {
        //                    response.RstKey = 19;
        //                    response.message = ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND;
        //                }

        //            }
        //            else
        //            {
        //                response.RstKey = 6;
        //                response.Status = (int)WalletTransactionStatus.FAILED;
        //                response.message = ResponseMessages.EMAIL_VERIFICATION_PENDING;// "Please verify your email id.";
        //            }


        //        }
        //        else
        //        {
        //            response.message = ResponseMessages.MobileNotVerify;// 
        //            response.Status = (int)WalletTransactionStatus.FAILED;
        //            response.RstKey = 20;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        ex.StackTrace.ErrorLog("TransferToBankRepository.cs", "PayMoneyTransferToBank");
        //        response.RstKey = 6;
        //        response.message = ex.StackTrace;
        //        // _response.Create(false, "Exception Occured", HttpStatusCode.NotFound, new TransferToBankResponseModel());
        //        //tran.Rollback();
        //    }

        //    //response.DocStatus = IsdocVerified;

        //    //response.CurrentBalance = re.CurrentBalance;
        //    //return response;
        //    //response.amount = request.amount; //FinalAmount;check
        //    return response;

        //}


        public async Task<List<senderIdTypetbl>> GetsenderidtypeList()
        {
           
            return await _transferToBankRepository.GetsenderidtypeList();
        }
        

    }
}
