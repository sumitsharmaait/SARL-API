using ezeePay.Utility.CommonClass;
using ezeePay.Utility.Enums;
using Ezipay.Database;
using Ezipay.Repository.CommisionRepo;
using Ezipay.Repository.CommonRepo;
using Ezipay.Repository.MasterData;
using Ezipay.Repository.MerchantPaymentRepo;
using Ezipay.Repository.PayMoneyRepo;
using Ezipay.Repository.UserRepo;
using Ezipay.Service.CommisionService;
using Ezipay.Service.UserService;
using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.Utility.LogHandler;
using Ezipay.Utility.SendEmail;
using Ezipay.Utility.SendPush;
using Ezipay.ViewModel.AirtimeViewModel;
using Ezipay.ViewModel.MerchantPaymentViewModel;
using Ezipay.ViewModel.PayMoneyViewModel;
using Ezipay.ViewModel.SendPushViewModel;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Ezipay.Service.MerchantPayment
{
    public class MerchantPaymentService : IMerchantPaymentService
    {
        private IWalletUserRepository _walletUserRepository;
        private IMasterDataRepository _masterDataRepository;
        private IWalletUserService _walletUserService;
        private ISetCommisionRepository _setCommisionRepository;
        private ISendPushNotification _sendPushNotification;
        private ISendEmails _sendEmails;
        private ICommonRepository _commonRepository;
        private ISetCommisionService _setCommisionService;
        private IMerchantPaymentRepository _merchantPaymentRepository;
        private IPayMoneyRepository _payMoneyRepository;
        private ILogUtils _logUtils;

        public MerchantPaymentService()
        {
            _walletUserRepository = new WalletUserRepository();
            _walletUserService = new WalletUserService();
            _masterDataRepository = new MasterDataRepository();
            _setCommisionRepository = new SetCommisionRepository();
            _sendPushNotification = new SendPushNotification();
            _sendEmails = new SendEmails();
            _commonRepository = new CommonRepository();
            _merchantPaymentRepository = new MerchantPaymentRepository();
            _setCommisionService = new SetCommisionService();
            _payMoneyRepository = new PayMoneyRepository();
            _logUtils = new LogUtils();
        }
        public async Task<WalletTransactionResponse> MerchantPayment(MerchantTransactionRequest requestModel, string token, long WalletUserId = 0)
        {
            var response = new WalletTransactionResponse();
            var commission = new CommissionCalculationResponse();
            var merchantCommission = new CommissionCalculationResponse();
            var transationInitiate = new TransactionInitiateRequest();
            var tranDate = DateTime.UtcNow;
            try
            {
                var MerchantDetail = await _walletUserRepository.GetWalletUserByUserType((int)WalletUserTypes.Merchant, requestModel.MerchantId);
                var adminKeyPair = AES256.AdminKeyPair;
                var data = await _walletUserService.UserProfile(token);
                var sender = await _walletUserRepository.GetCurrentUser(data.WalletUserId);

                var transactionLimit = await _payMoneyRepository.GetTransactionLimitForPayment(data.WalletUserId);
                int limit = transactionLimit != null ? Convert.ToInt32(transactionLimit.transactionlimit) : 0;
                var transactionHistory = await _payMoneyRepository.GetAllTransactionByDate(data.WalletUserId);
                int totalAmountTransfered = transactionHistory != null ? Convert.ToInt32(transactionHistory.totalAmount) : 0;
                //var IsdocVerified = await _walletUserRepository.IsDocVerified(data.WalletUserId, data.DocumetStatus);
                var IsdocVerified = await _walletUserRepository.IsDocVerifiedMOMO(data.DocumetStatus);

                response.DocStatus = IsdocVerified;
                response.DocumetStatus = (int)sender.DocumetStatus;

                if (sender.IsEmailVerified == true)
                {
                    if (sender.IsDisabledTransaction == false)
                    {

                        if (MerchantDetail.DocumetStatus == 2) //Merchant document check verified or not
                        {
                            if (IsdocVerified == true) //user document check verified or not
                            {
                                //if (transactionLimit == null || limit >= (Convert.ToDecimal(requestModel.Amount) + totalAmountTransfered))
                                //{
                                if (sender != null && sender.WalletUserId > 0)
                                {

                                    if (MerchantDetail != null && MerchantDetail.WalletUserId > 0)
                                    {
                                        if (MerchantDetail.WalletUserId != sender.WalletUserId)
                                        { //recheck balance for new txn
                                            if (!sender.CurrentBalance.IsZero() && Convert.ToDecimal(sender.CurrentBalance) >= (Convert.ToDecimal(requestModel.Amount) + Convert.ToDecimal(commission.CommissionAmount)))
                                            {

                                                int EWalletServiceId = await _commonRepository.GetWalletServiceId((int)WalletTransactionSubTypes.EWallet_To_Ewallet_Transactions_PayMoney);
                                                int WalletServiceId = await _commonRepository.GetWalletServiceId((int)WalletTransactionSubTypes.Merchants, MerchantDetail.WalletUserId);
                                                if (WalletServiceId > 0)
                                                {

                                                    #region Calculate Commission
                                                    #region Service Commission

                                                    commission.AmountWithCommission = requestModel.Amount;
                                                    commission.AfterDeduction = requestModel.Amount;
                                                    var commissionDetail = await _masterDataRepository.GetCommisionByServiceId(WalletServiceId);
                                                    if (commissionDetail != null && Convert.ToDecimal(commissionDetail.CommisionPercent) > 0)
                                                    {
                                                        commission = await _setCommisionService.CalculateCommission((decimal)commissionDetail.CommisionPercent, (int)commissionDetail.CommisionMasterId, requestModel.Amount, Convert.ToDecimal(commissionDetail.FlatCharges), Convert.ToDecimal(commissionDetail.BenchmarkCharges));

                                                    }
                                                    else if (commissionDetail != null && commissionDetail.FlatCharges > 0) //flatcharges not null
                                                    {
                                                        //commission = await _setCommisionService.CalculateCommission((decimal)commissionDetail.CommisionPercent, (int)commissionDetail.CommisionMasterId, requestModel.Amount, Convert.ToDecimal(commissionDetail.FlatCharges), Convert.ToDecimal(commissionDetail.BenchmarkCharges));

                                                        //user amount send with flat
                                                        var xx = decimal.Parse(requestModel.Amount) + commissionDetail.FlatCharges;
                                                        commission.AmountWithCommission = xx.ToString();
                                                        //merchant ko milega
                                                        commission.AfterDeduction = requestModel.Amount;
                                                        commission.CommissionAmount = commissionDetail.FlatCharges.ToString();
                                                    }
                                                    #endregion

                                                    #region If Receiver is Merchant                                            
                                                    if (await _commonRepository.IsMerchant(MerchantDetail.WalletUserId, WalletServiceId))
                                                    {
                                                        var merchantCommissionDetail = await _commonRepository.MerchantCommisionMasters(WalletServiceId);
                                                        if (merchantCommissionDetail != null && merchantCommissionDetail.CommisionPercent > 0)
                                                        {
                                                            var flat = (commissionDetail != null) ? commissionDetail.FlatCharges : 0;
                                                            var benchmark = (commissionDetail != null) ? commissionDetail.BenchmarkCharges : 0;
                                                            merchantCommission = await _setCommisionService.CalculateCommission((decimal)merchantCommissionDetail.CommisionPercent, (int)merchantCommissionDetail.CommisionMasterId, requestModel.Amount, Convert.ToDecimal(flat), Convert.ToDecimal(benchmark));
                                                        }
                                                    }
                                                    #endregion
                                                    #endregion


                                                    #region transaction initiate request 
                                                    string transactionInitiate = JsonConvert.SerializeObject(requestModel);
                                                    var invoiceNumber = await _masterDataRepository.GetInvoiceNumber();
                                                    //This is for transaction initiate request all---
                                                    transationInitiate.InvoiceNumber = invoiceNumber.InvoiceNumber;
                                                    transationInitiate.ReceiverNumber = "";
                                                    transationInitiate.ServiceName = "Merchant payment";
                                                    transationInitiate.RequestedAmount = commission.AmountWithCommission.ToString();
                                                    transationInitiate.TransactionStatus = (int)TransactionStatus.Pending;
                                                    transationInitiate.WalletUserId = sender.WalletUserId;
                                                    transationInitiate.UserReferanceNumber = invoiceNumber.AutoDigit;
                                                    transationInitiate.CurrentBalance = sender.CurrentBalance;
                                                    transationInitiate.AfterTransactionBalance = "";
                                                    transationInitiate.UserName = data.FirstName + " " + data.LastName;
                                                    transationInitiate.CreatedDate = DateTime.UtcNow;
                                                    transationInitiate.UpdatedDate = DateTime.UtcNow;
                                                    transationInitiate.IsActive = true;
                                                    transationInitiate.IsDeleted = false;
                                                    transationInitiate.JsonRequest = transactionInitiate;
                                                    transationInitiate.JsonResponse = "";
                                                    transationInitiate = await _payMoneyRepository.SaveTransactionInitiateRequest(transationInitiate);
                                                    #endregion

                                                    LogTransactionTypes.Request.SaveTransactionLog(LogTransactionNameTypes.WalletTransaction + "MerchantPayment", requestModel, "");

                                                    if (!sender.CurrentBalance.IsZero() && Convert.ToDecimal(sender.CurrentBalance) >= (Convert.ToDecimal(requestModel.Amount) + Convert.ToDecimal(commission.CommissionAmount)))
                                                    {


                                                        #region Save Transaction
                                                        var tran = new WalletTransaction();
                                                        tran.TransactionInitiateRequestId = transationInitiate.Id;
                                                        tran.Comments = requestModel.Comment;
                                                        tran.InvoiceNo = invoiceNumber.InvoiceNumber;//amit 24/03
                                                                                                     //tran.InvoiceNo = requestModel.Comment;
                                                                                                     //tran.TotalAmount = requestModel.Amount;
                                                        tran.TotalAmount = commission.AmountWithCommission;
                                                        tran.CommisionId = commission.CommissionServiceId;
                                                        tran.WalletAmount = commission.AfterDeduction;
                                                        tran.ServiceTaxRate = 0;
                                                        tran.ServiceTax = "0";
                                                        tran.WalletServiceId = (int)WalletServiceId;
                                                        tran.SenderId = sender.WalletUserId;
                                                        tran.ReceiverId = MerchantDetail.WalletUserId;
                                                        tran.AccountNo = string.Empty;
                                                        tran.TransactionId = "0";
                                                        tran.IsAdminTransaction = false;
                                                        tran.IsActive = true;
                                                        tran.IsDeleted = false;
                                                        tran.CreatedDate = tranDate;
                                                        tran.UpdatedDate = tranDate;
                                                        tran.TransactionTypeInfo = (int)TransactionTypeInfo.EWalletToEwalletTransactionsMerchantPayment;
                                                        tran.TransactionStatus = (int)TransactionStatus.Completed;
                                                        tran.MerchantCommissionId = merchantCommission.CommissionServiceId;
                                                        tran.MerchantCommissionAmount = !string.IsNullOrEmpty(merchantCommission.CommissionAmount) ? merchantCommission.CommissionAmount : "0";
                                                        tran.CommisionAmount = !string.IsNullOrEmpty(commission.CommissionAmount) ? commission.CommissionAmount : "0";
                                                        tran.VoucherCode = string.Empty;
                                                        tran.TransactionType = AggragatorServiceType.DEBIT;
                                                        tran.IsBankTransaction = false;
                                                        tran.BankBranchCode = string.Empty;
                                                        tran.BankTransactionId = string.Empty;
                                                        tran = await _merchantPaymentRepository.SaveWalletTransaction(tran);

                                                        #endregion
                                                        #region Debit and Credit
                                                        #region Debit the Sender
                                                        var debit = new WalletTransactionDetail();
                                                        debit.Amount = commission.AmountWithCommission;
                                                        debit.TransactionType = (int)WalletTransactionDetailTypes.DEBIT;
                                                        debit.WalletUserId = sender.WalletUserId;
                                                        debit.WalletTransactionId = tran.WalletTransactionId;
                                                        debit.IsActive = true;
                                                        debit.IsDeleted = false;
                                                        debit.CreatedDate = tranDate;

                                                        #endregion
                                                        #region Credit the Receiver
                                                        var credit = new WalletTransactionDetail();
                                                        debit.Amount = requestModel.Amount;
                                                        debit.TransactionType = (int)WalletTransactionDetailTypes.CREDIT;
                                                        debit.WalletUserId = MerchantDetail.WalletUserId;
                                                        debit.WalletTransactionId = tran.WalletTransactionId;
                                                        debit.IsActive = true;
                                                        debit.IsDeleted = false;
                                                        debit.CreatedDate = tranDate;
                                                        #endregion
                                                        debit = await _merchantPaymentRepository.SaveWalletTransactionDetail(debit);
                                                        #endregion

                                                        #region Update Commission History
                                                        if (commission.Rate > 0)
                                                        {
                                                            var _CommisionHistory = new CommisionHistory();
                                                            _CommisionHistory.CommisionId = commission.CommissionServiceId;
                                                            _CommisionHistory.WalletTransactionId = tran.WalletTransactionId;
                                                            _CommisionHistory.Amount = commission.CommissionAmount;
                                                            _CommisionHistory.CommisionType = 1;
                                                            _CommisionHistory.IsActive = true;
                                                            _CommisionHistory.IsDeleted = false;
                                                            _CommisionHistory.CreatedDate = tranDate;
                                                            _CommisionHistory.UpdatedDate = tranDate;
                                                            _CommisionHistory = await _merchantPaymentRepository.SaveCommisionHistory(_CommisionHistory);

                                                        }
                                                        if (merchantCommission.Rate > 0)
                                                        {
                                                            var _CommisionHistory = new CommisionHistory();
                                                            _CommisionHistory.CommisionId = merchantCommission.CommissionServiceId;
                                                            _CommisionHistory.WalletTransactionId = tran.WalletTransactionId;
                                                            _CommisionHistory.Amount = merchantCommission.CommissionAmount;
                                                            _CommisionHistory.CommisionType = 2;
                                                            _CommisionHistory.IsActive = true;
                                                            _CommisionHistory.IsDeleted = false;
                                                            _CommisionHistory.CreatedDate = tranDate;
                                                            _CommisionHistory.UpdatedDate = tranDate;
                                                            _CommisionHistory = await _commonRepository.SaveCommisionHistory(_CommisionHistory);

                                                        }
                                                        #endregion
                                                        #region Update Sender Balance

                                                        sender.CurrentBalance = Convert.ToString(Math.Round((Convert.ToDecimal(sender.CurrentBalance) - (Convert.ToDecimal(requestModel.Amount) + Convert.ToDecimal(commission.CommissionAmount))), 2));
                                                        #endregion
                                                        #region Update receiver Balance
                                                        if (!MerchantDetail.CurrentBalance.IsZero())
                                                        {
                                                            MerchantDetail.CurrentBalance = Convert.ToString(Math.Round((Convert.ToDecimal(MerchantDetail.CurrentBalance) + Convert.ToDecimal(requestModel.Amount)) - Convert.ToDecimal(merchantCommission.CommissionAmount), 2));

                                                        }
                                                        else
                                                        {
                                                            MerchantDetail.CurrentBalance = Convert.ToString(Math.Round(Convert.ToDecimal(requestModel.Amount) - Convert.ToDecimal(merchantCommission.CommissionAmount), 2));
                                                        }
                                                        #endregion
                                                        response.StatusCode = (int)WalletTransactionStatus.SUCCESS;
                                                        response.TransactionDate = tranDate;
                                                        response.TransactionAmount = requestModel.Amount;
                                                        response.TransactionId = tran.WalletTransactionId;
                                                        response.Message = "Payment done successfully.";
                                                        response.Amount = requestModel.Amount;
                                                        response.SenderBalance = MerchantDetail.CurrentBalance;
                                                        response.ToMobileNo = MerchantDetail.StdCode + AES256.Decrypt(adminKeyPair.PrivateKey, MerchantDetail.MobileNo);
                                                        response.TransactionAmount = requestModel.Amount;
                                                        response.CurrentBalance = sender.CurrentBalance;
                                                        // response.StatusCode = AggregatorySTATUSCODES.SUCCESSFUL;
                                                        response.RstKey = 1;

                                                        #region PushNotification

                                                        if (!string.IsNullOrEmpty(MerchantDetail.CurrentBalance) && MerchantDetail.DeviceType != null && !string.IsNullOrEmpty(MerchantDetail.DeviceToken))
                                                        {
                                                            var MerchantBalance = await _walletUserRepository.GetUserDetailById(MerchantDetail.WalletUserId);
                                                            if (MerchantBalance != null && response.StatusCode == (int)TransactionStatus.Completed)
                                                            {
                                                                var pushModel = new PayMoneyPushModel();
                                                                pushModel.TransactionDate = response.TransactionDate;
                                                                pushModel.TransactionId = response.TransactionId.ToString();
                                                                pushModel.alert = commission.AfterDeduction + " XOF has been credited to your account by " + data.FirstName + " " + data.LastName;
                                                                pushModel.Amount = response.Amount;
                                                                pushModel.CurrentBalance = MerchantDetail.CurrentBalance;

                                                                pushModel.MobileNo = sender.StdCode + sender.MobileNo;
                                                                pushModel.SenderName = sender.FirstName + " " + sender.LastName;
                                                                pushModel.pushType = (int)PushType.MERCHANTPAYMENT;

                                                                var push = new PushNotificationModel();
                                                                push.deviceType = (int)MerchantDetail.DeviceType;
                                                                push.deviceKey = MerchantDetail.DeviceToken;

                                                                if ((int)MerchantDetail.DeviceType == (int)DeviceTypes.ANDROID || (int)MerchantDetail.DeviceType == (int)DeviceTypes.Web)
                                                                {
                                                                    var aps = new PushPayload<PayMoneyPushModel>();
                                                                    var _data = new PushPayloadData<PayMoneyPushModel>();
                                                                    _data.notification = pushModel;
                                                                    push.payload = pushModel;
                                                                    aps.data = _data;
                                                                    aps.to = MerchantDetail.DeviceToken;
                                                                    aps.collapse_key = string.Empty;
                                                                    push.message = JsonConvert.SerializeObject(aps);

                                                                }
                                                                if ((int)MerchantDetail.DeviceType == (int)DeviceTypes.IOS)
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
                                                            }
                                                        }

                                                        //update intial request with current balance

                                                        transationInitiate = await _payMoneyRepository.GetTransactionInitiateRequest(transationInitiate.Id);
                                                        //transationInitiate = await _payMoneyRepository.GetTransactionInitiateRequestMerchantDetail(transationInitiate.Id, invoiceNumber.InvoiceNumber);
                                                        transationInitiate.AfterTransactionBalance = sender.CurrentBalance;
                                                        transationInitiate.ReceiverCurrentBalance = MerchantDetail.CurrentBalance;
                                                        transationInitiate.ReceiverWalletUserId = MerchantDetail.WalletUserId;
                                                        await _payMoneyRepository.UpdateTransactionInitiateRequest(transationInitiate);
                                                        //Update merchant wallet 
                                                        await _merchantPaymentRepository.UpdateWalletUser(MerchantDetail);
                                                        //Update user wallet 
                                                        await _merchantPaymentRepository.UpdateWalletUser(sender);

                                                    }
                                                    else
                                                    {

                                                        _logUtils.WriteTextToFileForWTxnTableLogs("MerchantPaymentservice :-lineno. 362 InvoiceNumber " + invoiceNumber.InvoiceNumber);
                                                        response.StatusCode = (int)WalletTransactionStatus.FAILED;
                                                        response.Message = "Insuficient Balance.";
                                                        response.RstKey = 3;
                                                    }
                                                    #endregion

                                                }
                                            }
                                            else
                                            {

                                                //_logUtils.WriteTextToFileForWTxnTableLogs("MerchantPaymentservice :-lineno. 362 InvoiceNumber " + invoiceNumber.InvoiceNumber);
                                                response.StatusCode = (int)WalletTransactionStatus.FAILED;
                                                response.Message = "Insuficient Balance.";
                                                response.RstKey = 3;
                                            }
                                        }
                                        else
                                        {
                                            response.StatusCode = (int)WalletTransactionStatus.SELF_WALLET;
                                            response.Message = ResponseMessages.SELF_WALLET;
                                            response.RstKey = 6;
                                        }
                                    }
                                    else
                                    {
                                        response.StatusCode = (int)WalletTransactionStatus.RECEIVER_NOT_EXIST;
                                        response.Message = ResponseMessages.RECEIVER_NOT_EXIST;
                                        response.RstKey = 6;
                                    }
                                }
                                else
                                {
                                    response.RstKey = 11;
                                    response.Message = ResponseMessages.SENDER_NOT_EXIST;
                                    response.StatusCode = (int)WalletTransactionStatus.SENDER_NOT_EXIST;
                                }
                                //}
                                //else
                                //{
                                //    response.RstKey = 12;
                                //    response.Message = ResponseMessageKyc.TRANSACTION_LIMIT;
                                //}

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
                            response.StatusCode = (int)WalletTransactionStatus.FAILED;
                            response.RstKey = 20;
                            response.Message = ResponseMessageKyc.Merchant_TRANSACTION_DISABLED;
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
                    response.StatusCode = (int)WalletTransactionStatus.FAILED;
                    response.Message = ResponseMessages.EMAIL_VERIFICATION_PENDING;
                    response.RstKey = 6;
                }
                response.DocumetStatus = (int)sender.DocumetStatus;
                response.DocStatus = IsdocVerified;
            }
            catch (Exception ex)
            {
                response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                ex.Message.ErrorLog("WalletTransactionRepository.cs", "MerchantPayment", response + " " + ex.StackTrace + " " + ex.Message);

                response.Message = "Exception Occured.";
            }
            LogTransactionTypes.Response.SaveTransactionLog(LogTransactionNameTypes.WalletTransaction + "MerchantPayment", response, "");

            return response;
        }

        public async Task<WalletTransactionResponse> MerchantPaymentEzipayPartner(MerchantTransactionForThirdPartyRequest requestModel)
        {
            var response = new WalletTransactionResponse();
            var commission = new CommissionCalculationResponse();
            var merchantCommission = new CommissionCalculationResponse();
            var tranDate = DateTime.UtcNow;
            var transationInitiate = new TransactionInitiateRequest();
            try
            {//who will provide apikey
                var merchantData = await _walletUserRepository.GetMerchantApiKey(requestModel.apiKey, requestModel.merchantKey);
                //merchantdetail
                var MerchantDetail = await _walletUserRepository.GetWalletUserByUserType((int)WalletUserTypes.Merchant, Convert.ToInt32(requestModel.merchantId));
                var adminKeyPair = AES256.AdminKeyPair;
                //get walletuser active,emailverified,otp verified or not
                var data = await _walletUserRepository.GetWalletUser(Convert.ToInt32(requestModel.senderId));
                //get userdetails
                var sender = await _walletUserRepository.GetCurrentUser(Convert.ToInt32(requestModel.senderId));
                //check transactionlimit
                var transactionLimit = await _payMoneyRepository.GetTransactionLimitForPayment(data.WalletUserId);
                int limit = transactionLimit != null ? Convert.ToInt32(transactionLimit.transactionlimit) : 0;
                //check transactionhistory
                var transactionHistory = await _payMoneyRepository.GetAllTransactionByDate(data.WalletUserId);
                int totalAmountTransfered = transactionHistory != null ? Convert.ToInt32(transactionHistory.totalAmount) : 0;
                //  var data = await _mobileMoneyRepository.GetData(request.WalletUserId);
                //why we use vpc_AuthorizeId to check document verified or not
                var IsdocVerified = await _walletUserRepository.IsDocVerifiedMOMO(Convert.ToInt32(data.DocumetStatus));
                //  bool IsdocVerified = true;
                response.DocStatus = IsdocVerified;
                response.DocumetStatus = (int)sender.DocumetStatus;
                if (merchantData != null)
                {
                    if (sender.IsEmailVerified == true)
                    {
                        if (sender.IsDisabledTransaction == false)
                        {
                            if (IsdocVerified == true)
                            {
                                if (transactionLimit == null || limit >= (Convert.ToDecimal(requestModel.amount) + totalAmountTransfered))
                                {

                                    if (sender != null && sender.WalletUserId > 0)
                                    {
                                        if (MerchantDetail != null && MerchantDetail.WalletUserId > 0)
                                        {
                                            if (MerchantDetail.WalletUserId != sender.WalletUserId)
                                            {
                                                int EWalletServiceId = await _commonRepository.GetWalletServiceId((int)WalletTransactionSubTypes.EWallet_To_Ewallet_Transactions_PayMoney);
                                                int WalletServiceId = await _commonRepository.GetWalletServiceId((int)WalletTransactionSubTypes.Merchants, MerchantDetail.WalletUserId);
                                                if (WalletServiceId > 0)
                                                {
                                                    #region Calculate Commission
                                                    #region Service Commission

                                                    commission.AmountWithCommission = requestModel.amount;
                                                    commission.AfterDeduction = requestModel.amount;
                                                    var commissionDetail = await _masterDataRepository.GetCommisionByServiceId(WalletServiceId);
                                                    if (commissionDetail != null && Convert.ToDecimal(commissionDetail.CommisionPercent) > 0)
                                                    {
                                                        commission = await _setCommisionService.CalculateCommission((decimal)commissionDetail.CommisionPercent, (int)commissionDetail.CommisionMasterId, requestModel.amount, Convert.ToDecimal(commissionDetail.FlatCharges), Convert.ToDecimal(commissionDetail.BenchmarkCharges));

                                                    }
                                                    #endregion
                                                    //if ?
                                                    #region If Receiver is Merchant                                            
                                                    if (await _commonRepository.IsMerchant(MerchantDetail.WalletUserId, WalletServiceId))
                                                    {
                                                        var merchantCommissionDetail = await _commonRepository.MerchantCommisionMasters(WalletServiceId);
                                                        if (merchantCommissionDetail != null && merchantCommissionDetail.CommisionPercent > 0)
                                                        {
                                                            var flat = (commissionDetail != null) ? commissionDetail.FlatCharges : 0;
                                                            var benchmark = (commissionDetail != null) ? commissionDetail.BenchmarkCharges : 0;
                                                            merchantCommission = await _setCommisionService.CalculateCommission((decimal)merchantCommissionDetail.CommisionPercent, (int)merchantCommissionDetail.CommisionMasterId, requestModel.amount, Convert.ToDecimal(flat), Convert.ToDecimal(benchmark));
                                                        }
                                                    }
                                                    #endregion
                                                    #endregion

                                                    //get invoicenumber
                                                    var invoiceNumber = await _masterDataRepository.GetInvoiceNumber();
                                                    #region transaction initiate request 
                                                    string transactionInitiate = JsonConvert.SerializeObject(requestModel);
                                                    //This is for transaction initiate request all---
                                                    transationInitiate.InvoiceNumber = invoiceNumber.InvoiceNumber;
                                                    transationInitiate.ReceiverNumber = "";
                                                    transationInitiate.ServiceName = "Merchant payment";
                                                    transationInitiate.RequestedAmount = commission.AmountWithCommission.ToString();
                                                    transationInitiate.TransactionStatus = (int)TransactionStatus.Pending;
                                                    transationInitiate.WalletUserId = sender.WalletUserId;
                                                    transationInitiate.UserReferanceNumber = invoiceNumber.AutoDigit;
                                                    transationInitiate.CurrentBalance = sender.CurrentBalance;
                                                    transationInitiate.AfterTransactionBalance = "";
                                                    transationInitiate.UserName = sender.FirstName + " " + sender.LastName;//change in data to sender
                                                    transationInitiate.CreatedDate = DateTime.UtcNow;
                                                    transationInitiate.UpdatedDate = DateTime.UtcNow;
                                                    transationInitiate.IsActive = true;
                                                    transationInitiate.IsDeleted = false;
                                                    transationInitiate.JsonRequest = transactionInitiate;
                                                    transationInitiate.JsonResponse = "";
                                                    transationInitiate = await _payMoneyRepository.SaveTransactionInitiateRequest(transationInitiate);
                                                    #endregion

                                                    LogTransactionTypes.Request.SaveTransactionLog(LogTransactionNameTypes.WalletTransaction + "MerchantPayment", requestModel, "");

                                                    if (!sender.CurrentBalance.IsZero() && Convert.ToDecimal(sender.CurrentBalance) >= (Convert.ToDecimal(requestModel.amount) + Convert.ToDecimal(commission.CommissionAmount)))
                                                    {
                                                        #region Save Transaction
                                                        var tran = new WalletTransaction();
                                                        tran.Comments = requestModel.transactionType;
                                                        tran.InvoiceNo = invoiceNumber.InvoiceNumber;
                                                        tran.TotalAmount = requestModel.amount;
                                                        tran.CommisionId = commission.CommissionServiceId;
                                                        tran.WalletAmount = commission.AfterDeduction;
                                                        tran.ServiceTaxRate = 0;
                                                        tran.ServiceTax = "0";
                                                        tran.TransactionInitiateRequestId = transationInitiate.Id;
                                                        tran.WalletServiceId = (int)WalletServiceId;
                                                        tran.SenderId = sender.WalletUserId;
                                                        tran.ReceiverId = MerchantDetail.WalletUserId;
                                                        tran.AccountNo = string.Empty;
                                                        tran.TransactionId = "0";
                                                        tran.IsAdminTransaction = false;
                                                        tran.IsActive = true;
                                                        tran.IsDeleted = false;
                                                        tran.CreatedDate = tranDate;
                                                        tran.UpdatedDate = tranDate;
                                                        tran.TransactionTypeInfo = (int)TransactionTypeInfo.EWalletToEwalletTransactionsMerchantPayment;
                                                        tran.TransactionStatus = (int)TransactionStatus.Completed;
                                                        tran.MerchantCommissionId = merchantCommission.CommissionServiceId;
                                                        tran.MerchantCommissionAmount = !string.IsNullOrEmpty(merchantCommission.CommissionAmount) ? merchantCommission.CommissionAmount : "0";
                                                        tran.CommisionAmount = !string.IsNullOrEmpty(commission.CommissionAmount) ? commission.CommissionAmount : "0";
                                                        tran.VoucherCode = string.Empty;
                                                        tran.TransactionType = AggragatorServiceType.DEBIT;
                                                        tran.IsBankTransaction = false;
                                                        tran.BankBranchCode = string.Empty;
                                                        tran.BankTransactionId = string.Empty;
                                                        tran = await _merchantPaymentRepository.SaveWalletTransaction(tran);

                                                        #endregion
                                                        #region Debit and Credit
                                                        #region Debit the Sender
                                                        var debit = new WalletTransactionDetail();
                                                        debit.Amount = commission.AmountWithCommission;
                                                        debit.TransactionType = (int)WalletTransactionDetailTypes.DEBIT;
                                                        debit.WalletUserId = sender.WalletUserId;
                                                        debit.WalletTransactionId = tran.WalletTransactionId;
                                                        debit.IsActive = true;
                                                        debit.IsDeleted = false;
                                                        debit.CreatedDate = tranDate;

                                                        #endregion
                                                        #region Credit the Receiver 
                                                        var credit = new WalletTransactionDetail();
                                                        debit.Amount = requestModel.amount;
                                                        debit.TransactionType = (int)WalletTransactionDetailTypes.CREDIT;
                                                        debit.WalletUserId = MerchantDetail.WalletUserId;
                                                        debit.WalletTransactionId = tran.WalletTransactionId;
                                                        debit.IsActive = true;
                                                        debit.IsDeleted = false;
                                                        debit.CreatedDate = tranDate;
                                                        #endregion
                                                        debit = await _merchantPaymentRepository.SaveWalletTransactionDetail(debit);
                                                        #endregion

                                                        #region Update Commission History
                                                        if (commission.Rate > 0)
                                                        {
                                                            var _CommisionHistory = new CommisionHistory();
                                                            _CommisionHistory.CommisionId = commission.CommissionServiceId;
                                                            _CommisionHistory.WalletTransactionId = tran.WalletTransactionId;
                                                            _CommisionHistory.Amount = commission.CommissionAmount;
                                                            _CommisionHistory.CommisionType = 1;
                                                            _CommisionHistory.IsActive = true;
                                                            _CommisionHistory.IsDeleted = false;
                                                            _CommisionHistory.CreatedDate = tranDate;
                                                            _CommisionHistory.UpdatedDate = tranDate;
                                                            _CommisionHistory = await _merchantPaymentRepository.SaveCommisionHistory(_CommisionHistory);

                                                        }
                                                        if (merchantCommission.Rate > 0)
                                                        {
                                                            var _CommisionHistory = new CommisionHistory();
                                                            _CommisionHistory.CommisionId = merchantCommission.CommissionServiceId;
                                                            _CommisionHistory.WalletTransactionId = tran.WalletTransactionId;
                                                            _CommisionHistory.Amount = merchantCommission.CommissionAmount;
                                                            _CommisionHistory.CommisionType = 2;
                                                            _CommisionHistory.IsActive = true;
                                                            _CommisionHistory.IsDeleted = false;
                                                            _CommisionHistory.CreatedDate = tranDate;
                                                            _CommisionHistory.UpdatedDate = tranDate;
                                                            _CommisionHistory = await _commonRepository.SaveCommisionHistory(_CommisionHistory);

                                                        }
                                                        #endregion
                                                        #region Update Sender Balance
                                                        sender.CurrentBalance = Convert.ToString(Math.Round((Convert.ToDecimal(sender.CurrentBalance) - (Convert.ToDecimal(requestModel.amount) + Convert.ToDecimal(commission.CommissionAmount))), 2));
                                                        #endregion
                                                        #region Update receiver Balance
                                                        if (!MerchantDetail.CurrentBalance.IsZero())
                                                        {
                                                            MerchantDetail.CurrentBalance = Convert.ToString(Math.Round((Convert.ToDecimal(MerchantDetail.CurrentBalance) + Convert.ToDecimal(requestModel.amount)) - Convert.ToDecimal(merchantCommission.CommissionAmount), 2));
                                                        }
                                                        else
                                                        {
                                                            MerchantDetail.CurrentBalance = Convert.ToString(Math.Round(Convert.ToDecimal(requestModel.amount) - Convert.ToDecimal(merchantCommission.CommissionAmount), 2));
                                                        }
                                                        #endregion
                                                        response.StatusCode = (int)WalletTransactionStatus.SUCCESS;
                                                        response.TransactionDate = tranDate;
                                                        response.TransactionAmount = requestModel.amount;
                                                        response.TransactionId = tran.WalletTransactionId;
                                                        response.Message = "Payment done successfully.";
                                                        response.Amount = requestModel.amount;
                                                        response.SenderBalance = MerchantDetail.CurrentBalance;
                                                        response.ToMobileNo = MerchantDetail.StdCode + AES256.Decrypt(adminKeyPair.PrivateKey, MerchantDetail.MobileNo);
                                                        response.TransactionAmount = requestModel.amount;
                                                        response.CurrentBalance = sender.CurrentBalance;
                                                        // response.StatusCode = AggregatorySTATUSCODES.SUCCESSFUL;
                                                        response.RstKey = 1;

                                                        #region PushNotification

                                                        if (!string.IsNullOrEmpty(MerchantDetail.CurrentBalance) && MerchantDetail.DeviceType != null && !string.IsNullOrEmpty(MerchantDetail.DeviceToken))
                                                        {
                                                            var MerchantBalance = await _walletUserRepository.GetUserDetailById(MerchantDetail.WalletUserId);
                                                            if (MerchantBalance != null && response.StatusCode == (int)TransactionStatus.Completed)
                                                            {
                                                                var pushModel = new PayMoneyPushModel();
                                                                pushModel.TransactionDate = response.TransactionDate;
                                                                pushModel.TransactionId = response.TransactionId.ToString();
                                                                pushModel.alert = commission.AfterDeduction + " XOF has been credited to your account by " + sender.FirstName + " " + sender.LastName;
                                                                pushModel.Amount = response.Amount;
                                                                pushModel.CurrentBalance = MerchantDetail.CurrentBalance;

                                                                pushModel.MobileNo = sender.StdCode + sender.MobileNo;
                                                                pushModel.SenderName = sender.FirstName + " " + sender.LastName;
                                                                pushModel.pushType = (int)PushType.MERCHANTPAYMENT;

                                                                var push = new PushNotificationModel();
                                                                push.deviceType = (int)MerchantDetail.DeviceType;
                                                                push.deviceKey = MerchantDetail.DeviceToken;

                                                                if ((int)MerchantDetail.DeviceType == (int)DeviceTypes.ANDROID || (int)MerchantDetail.DeviceType == (int)DeviceTypes.Web)
                                                                {
                                                                    var aps = new PushPayload<PayMoneyPushModel>();
                                                                    var _data = new PushPayloadData<PayMoneyPushModel>();
                                                                    _data.notification = pushModel;
                                                                    push.payload = pushModel;
                                                                    aps.data = _data;
                                                                    aps.to = MerchantDetail.DeviceToken;
                                                                    aps.collapse_key = string.Empty;
                                                                    push.message = JsonConvert.SerializeObject(aps);

                                                                }
                                                                if ((int)MerchantDetail.DeviceType == (int)DeviceTypes.IOS)
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
                                                            }
                                                        }
                                                        //update intial request with current balance
                                                        transationInitiate = await _payMoneyRepository.GetTransactionInitiateRequest(transationInitiate.Id);
                                                        transationInitiate.AfterTransactionBalance = sender.CurrentBalance;
                                                        transationInitiate.ReceiverCurrentBalance = MerchantDetail.CurrentBalance;
                                                        transationInitiate.ReceiverWalletUserId = MerchantDetail.WalletUserId;
                                                        await _payMoneyRepository.UpdateTransactionInitiateRequest(transationInitiate);
                                                        //Update merchant wallet 
                                                        await _merchantPaymentRepository.UpdateWalletUser(MerchantDetail);
                                                        //Update user wallet 
                                                        await _merchantPaymentRepository.UpdateWalletUser(sender);

                                                    }
                                                    else
                                                    {
                                                        response.StatusCode = (int)WalletTransactionStatus.FAILED;
                                                        response.Message = "Insuficient Balance.";
                                                        response.RstKey = 3;
                                                    }
                                                    #endregion

                                                }
                                            }
                                            else
                                            {
                                                response.StatusCode = (int)WalletTransactionStatus.SELF_WALLET;
                                                response.Message = ResponseMessages.SELF_WALLET;
                                                response.RstKey = 6;
                                            }
                                        }
                                        else
                                        {
                                            response.StatusCode = (int)WalletTransactionStatus.RECEIVER_NOT_EXIST;
                                            response.Message = ResponseMessages.RECEIVER_NOT_EXIST;
                                            response.RstKey = 6;
                                        }
                                    }
                                    else
                                    {
                                        response.RstKey = 11;
                                        response.Message = ResponseMessages.SENDER_NOT_EXIST;
                                        response.StatusCode = (int)WalletTransactionStatus.SENDER_NOT_EXIST;
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
                        response.StatusCode = (int)WalletTransactionStatus.FAILED;
                        response.Message = ResponseMessages.EMAIL_VERIFICATION_PENDING;
                        response.RstKey = 6;
                    }
                }
                else
                {
                    response.StatusCode = (int)WalletTransactionStatus.FAILED;
                    response.Message = "Merchant is not valid";
                    response.RstKey = 6;
                }
                response.DocumetStatus = (int)sender.DocumetStatus;
                response.DocStatus = IsdocVerified;
            }
            catch (Exception ex)
            {
                response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                ex.Message.ErrorLog("WalletTransactionRepository.cs", "PayMoney");
                response.Message = "Exception Occured.";
            }
            LogTransactionTypes.Response.SaveTransactionLog(LogTransactionNameTypes.WalletTransaction + "MerchantPayment", response, "");

            return response;
        }

    }
}
