using ezeePay.Utility.CommonClass;
using ezeePay.Utility.Enums;
using Ezipay.Database;
using Ezipay.Repository.CommisionRepo;
using Ezipay.Repository.MasterData;
using Ezipay.Repository.PayMoneyRepo;
using Ezipay.Repository.TvRepo;
using Ezipay.Repository.UserRepo;
using Ezipay.Service.CommonService;
using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.Utility.SendEmail;
using Ezipay.Utility.SendPush;
using Ezipay.ViewModel.AirtimeViewModel;
using Ezipay.ViewModel.CommisionViewModel;
using Ezipay.ViewModel.SendEmailViewModel;
using Ezipay.ViewModel.SendPushViewModel;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Ezipay.Service.TvService
{
    public class TvServices : ITvServices
    {
        private ITvRepository _tvRepository;
        private IWalletUserRepository _walletUserRepository;
        private ISetCommisionRepository _setCommisionRepository;
        private IMasterDataRepository _masterDataRepository;
        private ISendPushNotification _sendPushNotification;
        private ISendEmails _sendEmails;
        private IPayMoneyRepository _payMoneyRepository;
        private ICommonServices _commonServices;
        public TvServices()
        {
            _tvRepository = new TvRepository();
            _walletUserRepository = new WalletUserRepository();
            _setCommisionRepository = new SetCommisionRepository();
            _masterDataRepository = new MasterDataRepository();
            _sendEmails = new SendEmails();
            _sendPushNotification = new SendPushNotification();
            _payMoneyRepository = new PayMoneyRepository();
            _commonServices = new CommonServices();
        }
        public async Task<AddMoneyAggregatorResponse> TvService(PayMoneyAggregatoryRequest request, long WalletUserId = 0)
        {
            var response = new AddMoneyAggregatorResponse();

            var _commissionRequest = new CalculateCommissionRequest();
            var _commission = new CalculateCommissionResponse();

            var sender = await _walletUserRepository.GetUserDetailById(request.WalletUserId);
            var data = await _walletUserRepository.GetCurrentUser(request.WalletUserId);

            var transactionLimit = await _payMoneyRepository.GetTransactionLimitForPayment(data.WalletUserId);
            int limit = transactionLimit != null ? Convert.ToInt32(transactionLimit.transactionlimit) : 0;
            var transactionHistory = await _payMoneyRepository.GetAllTransactionByDate(data.WalletUserId);
            int totalAmountTransfered = transactionHistory != null ? Convert.ToInt32(transactionHistory.totalAmount) : 0;

            var WalletService = await _masterDataRepository.GetWalletServicesByIdOrChannel(request.channel, request.ServiceCategoryId, request.ISD);
            var subcategory = await _masterDataRepository.GetWalletSubCategoriesById(request.ServiceCategoryId);
            bool IsdocVerified = await _walletUserRepository.IsDocVerifiedMOMO((int)sender.DocumetStatus);

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
                                            decimal currentBalance = Convert.ToDecimal(data.CurrentBalance);

                                            if (currentBalance > 0 && currentBalance >= amountWithCommision && _commission.CurrentBalance > 0 && _commission.CurrentBalance >= amountWithCommision)
                                            {
                                                #region Prepare the Model for Request

                                                var _MobileMoneyRequest = new MobileMoneyAggregatoryRequest();

                                                _MobileMoneyRequest.serviceCategory = request.serviceCategory;
                                                _MobileMoneyRequest.serviceType = AggragatorServiceType.CREDIT;
                                                _MobileMoneyRequest.channel = request.channel;
                                                _MobileMoneyRequest.amount = request.Amount;
                                                var invoiceNumber = await _masterDataRepository.GetInvoiceNumber();
                                                if (invoiceNumber != null)
                                                {
                                                    _MobileMoneyRequest.invoiceNo = invoiceNumber.InvoiceNumber;
                                                }
                                                _MobileMoneyRequest.customer = request.customer;
                                                _MobileMoneyRequest.apiKey = ThirdPartyAggragatorSettings.ApiKey;
                                                _MobileMoneyRequest.signature = new CommonMethods().MD5Hash(_MobileMoneyRequest);

                                                #endregion
                                                string apiUrl = ThirdPartyAggragatorSettings.PayMoneyUrl;
                                                string responseString = "";
                                                if (WalletService.HttpVerbs.ToLower() == AggragatorServiceVerbs.HttpPostVerb.ToLower())
                                                {
                                                    responseString = new CommonMethods().HttpPostUrlEncodedService(LogTransactionNameTypes.PayMoney, apiUrl, _MobileMoneyRequest, request, subcategory.CategoryName);
                                                }
                                                else
                                                {
                                                    responseString = new CommonMethods().HttpGetUrlEncodedService(LogTransactionNameTypes.PayMoney, apiUrl, _MobileMoneyRequest, request, subcategory.CategoryName);
                                                }
                                                LogTransactionTypes.Response.SaveTransactionLog(LogTransactionNameTypes.PayMoney + subcategory.CategoryName, responseString, "Aggregator Url : ", sender.WalletUserId);
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


                                                        tran.AccountNo = request.customer;// string.Empty;                                                  
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
                                                                string filename = AppSetting.successfullTransaction;

                                                                var body = _sendEmails.ReadEmailformats(filename);
                                                                body = body.Replace("$$FirstName$$", sender.FirstName + " " + sender.LastName);
                                                                body = body.Replace("$$DisplayContent$$", WalletService.ServiceName);
                                                                body = body.Replace("$$customer$$", request.customer);
                                                                body = body.Replace("$$amount$$", "XOF " + request.Amount);
                                                                body = body.Replace("$$ServiceTaxAmount$$", "XOF " + _commission.CommissionAmount);
                                                                body = body.Replace("$$AmountWithCommission$$", "XOF " + _commission.AmountWithCommission);
                                                                body = body.Replace("$$TransactionId$$", _responseModel.TransactionId);

                                                                var req = new EmailModel
                                                                {
                                                                    TO = sender.EmailId,
                                                                    Subject = ResponseEmailMessage.PAYMENT_SUCCESS,
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
                                                        tran = await _tvRepository.TvService(tran);

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
            response.AccountNo = request.customer;
            response.DocStatus = IsdocVerified;
            response.DocumetStatus = (int)sender.DocumetStatus;
            response.CurrentBalance = data.CurrentBalance;
            response.MobileNo = request.customer;
            response.StatusCode = AggregatorySTATUSCODES.SUCCESSFUL;
            return response;
        }
    }
}
