using ezeePay.Utility.CommonClass;
using ezeePay.Utility.Enums;
using Ezipay.Database;
using Ezipay.Repository.CommisionRepo;
using Ezipay.Repository.CommonRepo;
using Ezipay.Repository.MasterData;
using Ezipay.Repository.PaymentRequestRepo;
using Ezipay.Repository.PayMoneyRepo;
using Ezipay.Repository.UserRepo;
using Ezipay.Service.CommisionService;
using Ezipay.Service.UserService;
using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.Utility.SendEmail;
using Ezipay.Utility.SendPush;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.AirtimeViewModel;
using Ezipay.ViewModel.PayMoneyViewModel;
using Ezipay.ViewModel.SendEmailViewModel;
using Ezipay.ViewModel.SendPushViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ezipay.Service.PaymentRequestService
{
    public class PaymentRequestServices : IPaymentRequestServices
    {
        private IPaymentRequestRepository _paymentRequestRepository;
        private IWalletUserService _walletUserService;
        private IWalletUserRepository _walletUserRepository;
        private ISendPushNotification _sendPushNotification;
        private ISendEmails _sendEmails;
        private IPayMoneyRepository _payMoneyRepository;
        private ISetCommisionRepository _setCommisionRepository;
        private IMasterDataRepository _masterDataRepository;
        private ICommonRepository _commonRepository;
        private ISetCommisionService _setCommisionService;
        public PaymentRequestServices()
        {
            _paymentRequestRepository = new PaymentRequestRepository();
            _walletUserService = new WalletUserService();
            _walletUserRepository = new WalletUserRepository();
            _sendPushNotification = new SendPushNotification();
            _sendEmails = new SendEmails();
            _masterDataRepository = new MasterDataRepository();
            _payMoneyRepository = new PayMoneyRepository();
            _setCommisionRepository = new SetCommisionRepository();
            _commonRepository = new CommonRepository();
            _setCommisionService = new SetCommisionService();
        }
        public async Task<WalletTransactionResponse> PaymentRequest(WalletTransactionRequest request, string token)////
        {
            var data = await _walletUserService.UserProfile(token);
            var sender = await _walletUserRepository.GetCurrentUser(data.WalletUserId);
            // var Isdocverified = await _walletUserRepository.IsDocVerified(data.WalletUserId, data.DocumetStatus);
            var Isdocverified = await _walletUserRepository.IsDocVerifiedMOMO(data.DocumetStatus);
            var response = new WalletTransactionResponse();
            try
            {
                var adminKeyPair = AES256.AdminKeyPair;

                if (!string.IsNullOrEmpty(AES256.Encrypt(adminKeyPair.PublicKey, request.MobileNo)))
                {
                    request.MobileNo = request.IsdCode + AES256.Encrypt(adminKeyPair.PublicKey, request.MobileNo);

                    if (sender != null && sender.WalletUserId > 0)
                    {
                        var receiver = await _walletUserRepository.GetUserDetailByMobile(request.MobileNo);

                        if (sender.IsEmailVerified == true)
                        {

                            if (sender.IsDisabledTransaction == false)
                            {
                                if (Isdocverified == true)
                                {
                                    if (receiver.IsDisabledTransaction == false)
                                    {
                                        if (receiver != null && receiver.WalletUserId > 0)
                                        {
                                            if (receiver.WalletUserId != sender.WalletUserId)
                                            {
                                                LogTransactionTypes.Request.SaveTransactionLog(LogTransactionNameTypes.WalletTransaction + "Pay Money Request", request, "");
                                                var req = new MakePaymentRequest
                                                {
                                                    Amount = request.Amount,
                                                    RecieverId = receiver.WalletUserId,
                                                    SenderId = sender.WalletUserId,
                                                    Comment = request.Comment,
                                                    TransactionTypeInfo = (int)TransactionStatus.NoResponse
                                                };
                                                response = await _paymentRequestRepository.PaymentRequest(req);
                                                #region PushNotification
                                                if (response.TransactionId > 0)
                                                {
                                                    response.RstKey = 1;
                                                }
                                                "Paymoney Request".ErrorLog("Wallet Transaction", "PaymentRequest", request);
                                                RejectPaymentRequestPushModel pushModel = new RejectPaymentRequestPushModel();
                                                pushModel.CurrentBalance = receiver.CurrentBalance;
                                                pushModel.TransactionId = response.TransactionId.ToString();
                                                pushModel.alert = data.FirstName + " " + data.LastName + " has requested to pay " + request.Amount + " XOF to his account.";
                                                pushModel.pushType = 0;
                                                pushModel.Message = "hi";
                                                PushNotificationModel push = new PushNotificationModel();
                                                push.deviceType = (int)receiver.DeviceType;
                                                push.deviceKey = receiver.DeviceToken;
                                                if ((int)receiver.DeviceType == (int)DeviceTypes.ANDROID || (int)receiver.DeviceType == (int)DeviceTypes.Web)
                                                {
                                                    PushPayload<RejectPaymentRequestPushModel> aps = new PushPayload<RejectPaymentRequestPushModel>();

                                                    PushPayloadData<RejectPaymentRequestPushModel> _data = new PushPayloadData<RejectPaymentRequestPushModel>();
                                                    _data.notification = pushModel;
                                                    aps.data = _data;
                                                    aps.to = receiver.DeviceToken;
                                                    aps.collapse_key = string.Empty;
                                                    push.message = JsonConvert.SerializeObject(aps);
                                                    push.payload = pushModel;
                                                }
                                                if ((int)receiver.DeviceType == (int)DeviceTypes.IOS)
                                                {
                                                    NotificationJsonResponse<RejectPaymentRequestPushModel> aps = new NotificationJsonResponse<RejectPaymentRequestPushModel>();
                                                    aps.aps = pushModel;
                                                    push.message = JsonConvert.SerializeObject(aps);
                                                }
                                                if (!string.IsNullOrEmpty(push.message))
                                                {
                                                    _sendPushNotification.sendPushNotification(push);
                                                }
                                                #endregion
                                            }
                                            else
                                            {
                                                response.RstKey = 9;
                                                response.StatusCode = (int)WalletTransactionStatus.SELF_WALLET;
                                                response.Message = ResponseMessages.SELF_WALLET;
                                            }
                                        }
                                        else
                                        {
                                            response.RstKey = 10;
                                            response.Message = ResponseMessages.RECEIVER_NOT_EXIST;
                                            response.StatusCode = (int)WalletTransactionStatus.RECEIVER_NOT_EXIST;
                                        }

                                    }
                                    else
                                    {
                                        response.RstKey = 17;
                                        response.Message = ResponseMessageKyc.TRANSACTION_DISABLED;
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
                            response.RstKey = 6;
                            response.StatusCode = (int)WalletTransactionStatus.FAILED;
                            response.Message = ResponseMessages.EMAIL_VERIFICATION_PENDING;
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
                    response.StatusCode = (int)WalletTransactionStatus.RECEIVER_NOT_EXIST;
                    response.Message = "Receiver's wallet account does not exist.";
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = (int)WalletTransactionStatus.OTHER_ERROR;
                ex.Message.ErrorLog("WalletTransactionRepository.cs", "PaymentRequest", request);
                response.Message = "Exception Occured";
            }
            LogTransactionTypes.Response.SaveTransactionLog(LogTransactionNameTypes.WalletTransaction + "Pay Money Request", response, "");

            response.DocStatus = Isdocverified;
            response.DocumetStatus = (int)sender.DocumetStatus;
            return response;
        }

        public async Task<ViewPaymentResponse> ViewPaymentRequests(ViewPaymentRequest request, string token)////
        {
            var response = new ViewPaymentResponse();
            var list = new List<PayResponse>();
            int pageSize = CommonSetting.PageSize;
            var data = await _walletUserService.UserProfile(token);
            long walletUserId = Convert.ToInt32(data.WalletUserId);
            list = await _paymentRequestRepository.ViewPaymentRequests(request, walletUserId, pageSize);
            if (list != null)
            {
                response.CurrentBalance = data.CurrentBalance;
                response.PaymentRequests = list;
                response.PageSize = pageSize;
                response.TotalCount = list.Count();
                response.RstKey = 1;
            }
            else
            {
                response.RstKey = 2;
            }

            return response;
        }

        public async Task<WalletTransactionResponse> ManagePaymentRequest(ManagePayMoneyReqeust request, string token)////
        {
            var response = new WalletTransactionResponse();
            var commission = new CommissionCalculationResponse();
            var merchantCommission = new CommissionCalculationResponse();
            var transationInitiate = new TransactionInitiateRequest();
            var tranDate = DateTime.UtcNow;
            try
            {

                var data = await _walletUserService.UserProfile(token);
                var CurrentUser = await _walletUserRepository.GetCurrentUser(data.WalletUserId);
                //var IsdocVerified = await _walletUserRepository.IsDocVerified(data.WalletUserId, data.DocumetStatus);
                var IsdocVerified = await _walletUserRepository.IsDocVerifiedMOMO(data.DocumetStatus);

                var adminKeyPair = AES256.AdminKeyPair;
                var _PayRequest = await _paymentRequestRepository.GetPayMoneyRequests(request.PayMoneyRequestId);
                var transactionLimit = await _payMoneyRepository.GetTransactionLimitForPayment(data.WalletUserId);
                int limit = transactionLimit != null ? Convert.ToInt32(transactionLimit.transactionlimit) : 0;
                var transactionHistory = await _payMoneyRepository.GetAllTransactionByDate(data.WalletUserId);
                int totalAmountTransfered = transactionHistory != null ? Convert.ToInt32(transactionHistory.totalAmount) : 0;
                var invoiceNumber = await _masterDataRepository.GetInvoiceNumber();


                string FromMobileNo = string.Empty;
                string ServiceName = string.Empty;
                int WalletServiceId = 0;
                int MerchantCommissionServiceId = 0;
                int _TypeInfo = 0;
                int _PushType = 0;
                var receiverUser = await _walletUserRepository.GetCurrentUser(Convert.ToInt32(_PayRequest.SenderId));
                if (!request.IsAccept && _PayRequest != null)
                {

                    _PayRequest.IsPaid = false;
                    _PayRequest.IsActive = false;
                    _PayRequest.UpdatedDate = DateTime.UtcNow;
                    _PayRequest.TransactionTypeInfo = (int)TransactionStatus.Rejected;
                    response.StatusCode = (int)WalletTransactionStatus.SUCCESS;
                    //db.SaveChanges();
                    await _paymentRequestRepository.UpdatePayMoneyRequests(_PayRequest);
                    response.Message = "Pay money request rejected successfuly";
                    response.CurrentBalance = CurrentUser.CurrentBalance;
                    #region PushNotification


                    var pushModel = new PayMoneyPushModel();
                    pushModel.alert = "Payment Request of " + _PayRequest.Amount + " XOF has been rejected by " + data.FirstName + " " + data.LastName;
                    pushModel.Amount = _PayRequest.Amount;
                    pushModel.CurrentBalance = !string.IsNullOrEmpty(receiverUser.CurrentBalance) ? receiverUser.CurrentBalance : "0";
                    pushModel.MobileNo = data.MobileNo;
                    pushModel.SenderName = data.FirstName + " " + data.LastName;
                    pushModel.pushType = (int)PushType.REJECTMAKEPAYMENTREQUEST;

                    var push = new PushNotificationModel();
                    pushModel.TransactionDate = response.TransactionDate;
                    pushModel.TransactionId = request.PayMoneyRequestId.ToString();
                    push.deviceType = (int)receiverUser.DeviceType;
                    push.deviceKey = receiverUser.DeviceToken;
                    if ((int)receiverUser.DeviceType == (int)DeviceTypes.ANDROID || (int)receiverUser.DeviceType == (int)DeviceTypes.Web)
                    {
                        var aps = new PushPayload<PayMoneyPushModel>();
                        var _data = new PushPayloadData<PayMoneyPushModel>();
                        _data.notification = pushModel;
                        aps.data = _data;

                        push.payload = pushModel;
                        aps.to = receiverUser.DeviceToken;
                        aps.collapse_key = string.Empty;
                        push.message = JsonConvert.SerializeObject(aps);

                    }
                    if ((int)receiverUser.DeviceType == (int)DeviceTypes.IOS)
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
                    await _paymentRequestRepository.UpdatePayMoneyRequests(_PayRequest);
                    response.RstKey = 2;
                    response.Message = "Rejected Succesfully.";
                }
                else if (transactionLimit == null || limit >= (Convert.ToDecimal(_PayRequest.Amount) + totalAmountTransfered))
                {
                    if (CurrentUser.IsEmailVerified == true)
                    {
                        if (IsdocVerified == true)
                        {
                            if (_PayRequest != null && CurrentUser != null && CurrentUser.WalletUserId == _PayRequest.ReceiverId)
                            {
                                FromMobileNo = data.MobileNo;
                                //var receiverUser = db.WalletUsers.FirstOrDefault(x => x.WalletUserId == _PayRequest.SenderId);
                                if (receiverUser != null)
                                {
                                    var AdminKeys = AES256.AdminKeyPair;
                                    response.ToMobileNo = AES256.Decrypt(AdminKeys.PrivateKey, receiverUser.MobileNo);
                                }
                                if (request.IsAccept)
                                {
                                    //WalletServiceId = (int)db.WalletServices.Where(x => x.ServiceCategoryId == (int)WalletTransactionSubTypes.EWallet_To_Ewallet_Transactions_MakePaymentRequest).Select(x => x.WalletServiceId).FirstOrDefault();
                                    WalletServiceId = await _paymentRequestRepository.GetWalletServiceIdBySubType((int)WalletTransactionSubTypes.EWallet_To_Ewallet_Transactions_MakePaymentRequest);
                                    //MerchantCommissionServiceId = (int)db.WalletServices.Where(x => x.ServiceCategoryId == (int)WalletTransactionSubTypes.Merchants).Select(x => x.WalletServiceId).FirstOrDefault();
                                    MerchantCommissionServiceId = await _paymentRequestRepository.GetWalletServiceIdBySubType((int)WalletTransactionSubTypes.Merchants);
                                    if (WalletServiceId > 0)
                                    {
                                        LogTransactionTypes.Request.SaveTransactionLog(LogTransactionNameTypes.WalletTransaction + "Manage Pay Money Request", request, "");

                                        // WalletServiceId = (int)db.WalletServices.Where(x => x.ServiceCategoryId == (int)WalletTransactionSubTypes.EWallet_To_Ewallet_Transactions_PayMoney).Select(x => x.WalletServiceId).FirstOrDefault();
                                        //if (db.WalletServices.Any(x => x.MerchantId == receiverUser.WalletUserId))
                                        if (await _paymentRequestRepository.GetAnyService(receiverUser.WalletUserId))
                                        {
                                            _TypeInfo = (int)TransactionTypeInfo.EWalletToEwalletTransactionsMerchantPayment;
                                            ServiceName = " MerchantPayment";
                                            _PushType = (int)PushType.MERCHANTPAYMENT;

                                        }
                                        else
                                        {
                                            _TypeInfo = (int)TransactionTypeInfo.EWalletToEwalletTransactionsPayMoney;
                                            ServiceName = " PayMoney";
                                            _PushType = (int)PushType.PAYMONEY;
                                        }
                                        #region Calculate Commission
                                        #region Service Commission

                                        commission.AmountWithCommission = _PayRequest.Amount;
                                        commission.AfterDeduction = _PayRequest.Amount;
                                        //var commissionDetail = db.CommisionMasters.Where(x => x.WalletServiceId == WalletServiceId && (bool)x.IsActive).FirstOrDefault();
                                        var commissionDetail = await _masterDataRepository.GetCommisionByServiceId(WalletServiceId);
                                        if (commissionDetail != null && commissionDetail.CommisionPercent > 0)
                                        {
                                            commission = await _setCommisionService.CalculateCommission((decimal)commissionDetail.CommisionPercent, (int)commissionDetail.CommisionMasterId, _PayRequest.Amount, Convert.ToDecimal(commissionDetail.BenchmarkCharges), Convert.ToDecimal(commissionDetail.FlatCharges));

                                        }
                                        #endregion
                                        #region If Receiver is Merchant

                                        //if (db.WalletServices.Any(x => x.WalletServiceId == MerchantCommissionServiceId && x.MerchantId == receiverUser.WalletUserId))
                                        if (await _paymentRequestRepository.GetAnyService(receiverUser.WalletUserId, MerchantCommissionServiceId))
                                        {

                                            //var merchantCommissionDetail = db.MerchantCommisionMasters.Where(x => x.WalletServiceId == MerchantCommissionServiceId && (bool)x.IsActive).FirstOrDefault();
                                            var merchantCommissionDetail = await _paymentRequestRepository.GetMerchantCommisionMasters(MerchantCommissionServiceId);
                                            if (merchantCommissionDetail != null && merchantCommissionDetail.CommisionPercent > 0)
                                            {
                                                merchantCommission = await _setCommisionService.CalculateCommission((decimal)merchantCommissionDetail.CommisionPercent, (int)merchantCommissionDetail.CommisionMasterId, _PayRequest.Amount);
                                            }
                                        }
                                        #endregion
                                        #endregion
                                        var transactionInitiate = JsonConvert.SerializeObject(request);
                                        string amt = Convert.ToString(Math.Round(Convert.ToDecimal(CurrentUser.CurrentBalance) - ((Convert.ToDecimal(_PayRequest.Amount)) + Convert.ToDecimal(commission.CommissionAmount)), 2));

                                        transationInitiate.InvoiceNumber = invoiceNumber.InvoiceNumber;
                                        transationInitiate.ReceiverNumber = "";
                                        transationInitiate.ServiceName = "Make Payment request";
                                        transationInitiate.RequestedAmount = commission.AfterDeduction.ToString();
                                        transationInitiate.TransactionStatus = (int)TransactionStatus.Pending;
                                        transationInitiate.WalletUserId = data.WalletUserId;
                                        transationInitiate.UserReferanceNumber = invoiceNumber.AutoDigit;
                                        transationInitiate.CurrentBalance = CurrentUser.CurrentBalance;
                                        transationInitiate.AfterTransactionBalance = amt;
                                        transationInitiate.UserName = CurrentUser.FirstName + " " + CurrentUser.LastName;
                                        transationInitiate.CreatedDate = DateTime.UtcNow;
                                        transationInitiate.UpdatedDate = DateTime.UtcNow;
                                        transationInitiate.IsActive = true;
                                        transationInitiate.IsDeleted = false;
                                        transationInitiate.JsonRequest = transactionInitiate;
                                        transationInitiate.JsonResponse = "";
                                        transationInitiate = await _paymentRequestRepository.SaveTransactionInitiateRequest(transationInitiate);
                                        if (!CurrentUser.CurrentBalance.IsZero() && Convert.ToDecimal(CurrentUser.CurrentBalance) >= (Convert.ToDecimal(_PayRequest.Amount) + Convert.ToDecimal(commission.CommissionAmount)))
                                        {


                                            #region Save Transaction
                                            var tran = new WalletTransaction();
                                            tran.InvoiceNo = invoiceNumber.InvoiceNumber;
                                            tran.TransactionInitiateRequestId = transationInitiate.Id;
                                            tran.Comments = _PayRequest.Comments;
                                            tran.TotalAmount = _PayRequest.Amount;
                                            tran.CommisionId = commission.CommissionServiceId;
                                            tran.WalletAmount = commission.AfterDeduction;
                                            tran.ServiceTaxRate = 0;
                                            tran.ServiceTax = "0";
                                            tran.WalletServiceId = (int)WalletServiceId;
                                            tran.SenderId = _PayRequest.ReceiverId;
                                            tran.ReceiverId = _PayRequest.SenderId;
                                            tran.AccountNo = string.Empty;
                                            tran.TransactionId = "0";
                                            tran.IsAdminTransaction = false;
                                            tran.IsActive = true;
                                            tran.IsDeleted = false;
                                            tran.CreatedDate = tranDate;
                                            tran.UpdatedDate = tranDate;
                                            tran.TransactionTypeInfo = _TypeInfo;
                                            tran.TransactionStatus = (int)TransactionStatus.Completed;
                                            tran.MerchantCommissionId = merchantCommission.CommissionServiceId;
                                            tran.MerchantCommissionAmount = !string.IsNullOrEmpty(merchantCommission.CommissionAmount) ? merchantCommission.CommissionAmount : "0";
                                            tran.CommisionAmount = !string.IsNullOrEmpty(commission.CommissionAmount) ? commission.CommissionAmount : "0";
                                            tran.VoucherCode = string.Empty;
                                            tran.TransactionType = AggragatorServiceType.DEBIT;
                                            tran.IsBankTransaction = false;
                                            tran.BankBranchCode = string.Empty;
                                            tran.BankTransactionId = string.Empty;
                                            tran.FlatCharges = commissionDetail.FlatCharges.ToString();
                                            tran.BenchmarkCharges = commissionDetail.BenchmarkCharges.ToString();
                                            tran.CommisionPercent = commissionDetail.CommisionPercent.ToString();
                                            //db.WalletTransactions.Add(tran);
                                            //db.SaveChanges();
                                            tran = await _commonRepository.SaveWalletTransaction(tran);
                                            #endregion
                                            #region Debit and Credit
                                            #region Credit the Request Sender
                                            var debit = new WalletTransactionDetail();
                                            debit.Amount = commission.AmountWithCommission;
                                            debit.TransactionType = (int)WalletTransactionDetailTypes.CREDIT;
                                            debit.WalletUserId = _PayRequest.SenderId;
                                            debit.WalletTransactionId = tran.WalletTransactionId;
                                            debit.IsActive = true;
                                            debit.IsDeleted = false;
                                            debit.CreatedDate = tranDate;
                                            //db.WalletTransactionDetails.Add(debit);
                                            debit = await _commonRepository.SaveWalletTransactionDetail(debit);
                                            #endregion
                                            #region Debit the Request Receiver
                                            var credit = new WalletTransactionDetail();
                                            debit.Amount = _PayRequest.Amount;
                                            debit.TransactionType = (int)WalletTransactionDetailTypes.DEBIT;
                                            debit.WalletUserId = _PayRequest.ReceiverId;
                                            debit.WalletTransactionId = tran.WalletTransactionId;
                                            debit.IsActive = true;
                                            debit.IsDeleted = false;
                                            debit.CreatedDate = tranDate;
                                            //db.WalletTransactionDetails.Add(credit);
                                            //#endregion
                                            //db.SaveChanges();
                                            credit = await _commonRepository.SaveWalletTransactionDetail(credit);
                                            //PayMoneyRequest payMoneyRequest = new PayMoneyRequest();
                                            //payMoneyRequest.IsPaid = true;
                                            //db.SaveChanges();
                                            _PayRequest.IsPaid = true;
                                            _PayRequest.TransactionTypeInfo = (int)TransactionStatus.Completed;
                                            // db.SaveChanges();

                                            await _paymentRequestRepository.UpdatePayMoneyRequests(_PayRequest);
                                            #endregion
                                            #region Update Commission History
                                            var _CommisionHistory = new CommisionHistory();
                                            if (commission.Rate > 0)
                                            {
                                                // CommisionHistory _CommisionHistory = new CommisionHistory();
                                                _CommisionHistory.CommisionId = commission.CommissionServiceId;
                                                _CommisionHistory.WalletTransactionId = tran.WalletTransactionId;
                                                _CommisionHistory.Amount = commission.CommissionAmount;
                                                _CommisionHistory.CommisionType = 1;
                                                _CommisionHistory.IsActive = true;
                                                _CommisionHistory.IsDeleted = false;
                                                _CommisionHistory.CreatedDate = tranDate;
                                                _CommisionHistory.UpdatedDate = tranDate;
                                                //db.CommisionHistories.Add(_CommisionHistory);
                                                await _paymentRequestRepository.SaveCommisionHistory(_CommisionHistory, 1);

                                            }
                                            if (merchantCommission.Rate > 0)
                                            {
                                                // CommisionHistory _CommisionHistory = new CommisionHistory();
                                                _CommisionHistory.CommisionId = merchantCommission.CommissionServiceId;
                                                _CommisionHistory.WalletTransactionId = tran.WalletTransactionId;
                                                _CommisionHistory.Amount = merchantCommission.CommissionAmount;
                                                _CommisionHistory.CommisionType = 2;
                                                _CommisionHistory.IsActive = true;
                                                _CommisionHistory.IsDeleted = false;
                                                _CommisionHistory.CreatedDate = tranDate;
                                                _CommisionHistory.UpdatedDate = tranDate;
                                                //  db.CommisionHistories.Add(_CommisionHistory);
                                                await _paymentRequestRepository.SaveCommisionHistory(_CommisionHistory, 1);
                                            }
                                            #endregion

                                            #region Update Sender Balance
                                            CurrentUser.CurrentBalance = Convert.ToString(Math.Round(Convert.ToDecimal(CurrentUser.CurrentBalance) - ((Convert.ToDecimal(_PayRequest.Amount)) + Convert.ToDecimal(commission.CommissionAmount)), 2));
                                            #endregion

                                            #region Update receiver Balance
                                            string ReceiverBalance = "0";
                                            if (!receiverUser.CurrentBalance.IsZero())
                                            {
                                                ReceiverBalance = Convert.ToString(Math.Round((Convert.ToDecimal(receiverUser.CurrentBalance) + Convert.ToDecimal(_PayRequest.Amount)) - Convert.ToDecimal(merchantCommission.CommissionAmount), 2));
                                            }
                                            else
                                            {
                                                ReceiverBalance = Convert.ToString(Math.Round(Convert.ToDecimal(_PayRequest.Amount) - Convert.ToDecimal(merchantCommission.CommissionAmount), 2));
                                            }
                                            receiverUser.CurrentBalance = ReceiverBalance;
                                            #endregion

                                            response.StatusCode = (int)WalletTransactionStatus.SUCCESS;
                                            response.TransactionDate = tranDate;
                                            response.TransactionAmount = _PayRequest.Amount;
                                            response.TransactionId = tran.WalletTransactionId;
                                            response.Message = "Pay Money successfully.";
                                            response.Amount = _PayRequest.Amount;
                                            response.SenderBalance = receiverUser.CurrentBalance;
                                            response.ToMobileNo = receiverUser.StdCode + AES256.Decrypt(adminKeyPair.PrivateKey, receiverUser.MobileNo);
                                            response.TransactionAmount = _PayRequest.Amount;
                                            response.CurrentBalance = CurrentUser.CurrentBalance;
                                            response.RstKey = 1;
                                            _PayRequest.IsPaid = true;
                                            int receiverResult = 0;
                                            int result = await _paymentRequestRepository.UpdateWalletUser(CurrentUser);
                                            if (result > 0)
                                            {
                                                transationInitiate = await _payMoneyRepository.GetTransactionInitiateRequest(transationInitiate.Id);
                                                transationInitiate.AfterTransactionBalance = CurrentUser.CurrentBalance;
                                                transationInitiate.ReceiverCurrentBalance = receiverUser.CurrentBalance;
                                                transationInitiate.ReceiverWalletUserId = receiverUser.WalletUserId;
                                                await _payMoneyRepository.UpdateTransactionInitiateRequest(transationInitiate);
                                                receiverResult = await _paymentRequestRepository.UpdateWalletUser(receiverUser);
                                            }
                                            if (receiverResult > 0)
                                            {
                                                #region PushNotification

                                                if (!string.IsNullOrEmpty(receiverUser.CurrentBalance) && response.StatusCode == (int)WalletTransactionStatus.SUCCESS)
                                                {
                                                    var pushModel = new PayMoneyPushModel();
                                                    pushModel.TransactionDate = response.TransactionDate;
                                                    pushModel.TransactionId = response.TransactionId.ToString();
                                                    string Amount = string.Empty;
                                                    if (merchantCommission.Rate > 0)
                                                    {
                                                        Amount = Math.Round(Convert.ToDecimal(_PayRequest.Amount) - Convert.ToDecimal(merchantCommission.CommissionAmount), 2).ToString();
                                                    }
                                                    else
                                                    {
                                                        Amount = _PayRequest.Amount;
                                                    }
                                                    pushModel.alert = Amount + " XOF has been credited to your account by " + data.FirstName + " " + data.LastName;
                                                    pushModel.Amount = response.Amount;
                                                    pushModel.CurrentBalance = receiverUser.CurrentBalance;

                                                    pushModel.MobileNo = CurrentUser.StdCode + FromMobileNo;
                                                    pushModel.SenderName = data.FirstName + " " + data.LastName;
                                                    pushModel.pushType = _PushType;

                                                    var push = new PushNotificationModel();
                                                    push.deviceType = (int)receiverUser.DeviceType;
                                                    push.deviceKey = receiverUser.DeviceToken;
                                                    if ((int)receiverUser.DeviceType == (int)DeviceTypes.ANDROID || (int)receiverUser.DeviceType == (int)DeviceTypes.Web)
                                                    {
                                                        var aps = new PushPayload<PayMoneyPushModel>();
                                                        var _data = new PushPayloadData<PayMoneyPushModel>();
                                                        _data.notification = pushModel;
                                                        aps.data = _data;
                                                        aps.to = receiverUser.DeviceToken;
                                                        aps.collapse_key = string.Empty;
                                                        push.message = JsonConvert.SerializeObject(aps);
                                                        push.payload = pushModel;

                                                    }
                                                    if ((int)receiverUser.DeviceType == (int)DeviceTypes.IOS)
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

                                                    try
                                                    {
                                                        //--------sending mail on success transaction--------
                                                        var receiverdetail = await _walletUserRepository.GetUserDetailById(receiverUser.WalletUserId);
                                                        var senderdetail = await _walletUserRepository.GetUserDetailById(data.WalletUserId);
                                                        string filename = CommonSetting.successfullTransaction;
                                                        var body = _sendEmails.ReadEmailformats(filename);
                                                        body = body.Replace("$$FirstName$$", senderdetail.FirstName + " " + senderdetail.LastName);
                                                        body = body.Replace("$$DisplayContent$$", ServiceName);
                                                        body = body.Replace("$$customer$$", receiverdetail.MobileNo);
                                                        body = body.Replace("$$amount$$", "XOF " + _PayRequest.Amount);
                                                        body = body.Replace("$$ServiceTaxAmount$$", "XOF " + commission.CommissionAmount);
                                                        body = body.Replace("$$AmountWithCommission$$", "XOF " + commission.AmountWithCommission);
                                                        body = body.Replace("$$TransactionId$$", Convert.ToString(tran.WalletTransactionId));
                                                        var req = new EmailModel
                                                        {
                                                            TO = senderdetail.EmailId,
                                                            Subject = "Transaction Successfull",
                                                            Body = body
                                                        };
                                                        _sendEmails.SendEmail(req);
                                                    }
                                                    catch
                                                    {

                                                    }
                                                }
                                                // db.SaveChanges();
                                                await _paymentRequestRepository.SaveCommisionHistory(_CommisionHistory, 2);
                                                #endregion
                                            }
                                        }
                                        else
                                        {
                                            response.StatusCode = (int)TransactionStatus.Rejected;
                                            response.Message = "Insuficient Balance.";
                                            response.RstKey = 6;
                                        }
                                    }
                                    #endregion
                                }
                                else
                                {
                                    #region Reject the Request

                                    if (_PayRequest != null)
                                    {

                                        _PayRequest.IsPaid = false;
                                        _PayRequest.IsActive = false;
                                        _PayRequest.UpdatedDate = DateTime.UtcNow;
                                        response.StatusCode = (int)WalletTransactionStatus.SUCCESS;
                                        //db.SaveChanges();

                                        response.Message = "Pay money request rejected successfuly";
                                        response.CurrentBalance = CurrentUser.CurrentBalance;
                                        response.RstKey = 6;
                                        #region PushNotification


                                        var pushModel = new PayMoneyPushModel();
                                        pushModel.alert = "Payment Request of " + _PayRequest.Amount + " XOF has been rejected by " + data.FirstName + " " + data.LastName;
                                        pushModel.Amount = _PayRequest.Amount;
                                        pushModel.CurrentBalance = !string.IsNullOrEmpty(receiverUser.CurrentBalance) ? receiverUser.CurrentBalance : "0";
                                        pushModel.MobileNo = data.MobileNo;
                                        pushModel.SenderName = data.FirstName + " " + data.LastName;
                                        pushModel.pushType = (int)PushType.REJECTMAKEPAYMENTREQUEST;

                                        var push = new PushNotificationModel();
                                        pushModel.TransactionDate = response.TransactionDate;
                                        pushModel.TransactionId = request.PayMoneyRequestId.ToString();
                                        push.deviceType = (int)receiverUser.DeviceType;
                                        push.deviceKey = receiverUser.DeviceToken;
                                        if ((int)receiverUser.DeviceType == (int)DeviceTypes.ANDROID || (int)receiverUser.DeviceType == (int)DeviceTypes.Web)
                                        {
                                            var aps = new PushPayload<PayMoneyPushModel>();
                                            var _data = new PushPayloadData<PayMoneyPushModel>();
                                            _data.notification = pushModel;
                                            aps.data = _data;

                                            push.payload = pushModel;
                                            aps.to = receiverUser.DeviceToken;
                                            aps.collapse_key = string.Empty;
                                            push.message = JsonConvert.SerializeObject(aps);

                                        }
                                        if ((int)receiverUser.DeviceType == (int)DeviceTypes.IOS)
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
                                    }
                                    await _paymentRequestRepository.UpdatePayMoneyRequests(_PayRequest);
                                    #endregion

                                }
                            }
                            else
                            {
                                if (_PayRequest == null)
                                {
                                    response.RstKey = 6;
                                    response.Message = "Invalid pay request id.";
                                }
                                else if (CurrentUser == null)
                                {
                                    response.RstKey = 6;
                                    response.Message = "Please login before procced this pay request.";
                                }
                                else
                                {
                                    response.RstKey = 6;
                                    response.Message = "Your are not authorized to procced this payment request.";
                                }
                            }
                        }
                        else if (CurrentUser.DocumetStatus == 0 || CurrentUser.DocumetStatus == null)
                        {
                            response.RstKey = 13;
                            response.Message = ResponseMessageKyc.FAILED_Doc_NotUploaded;
                        }
                        else if (CurrentUser.DocumetStatus == 1 || CurrentUser.DocumetStatus == null)
                        {
                            response.RstKey = 14;
                            response.Message = ResponseMessageKyc.FAILED_Doc_Pending;
                        }
                        else if (CurrentUser.DocumetStatus == 4 || CurrentUser.DocumetStatus == null)
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
                        response.StatusCode = (int)WalletTransactionStatus.FAILED;
                        response.Message = ResponseMessages.EMAIL_VERIFICATION_PENDING;
                    }
                }
                else
                {
                    response.RstKey = 12;
                    response.Message = ResponseMessageKyc.TRANSACTION_LIMIT;
                }
            }
            catch (Exception ex)
            {
                if (request.IsAccept)
                {
                    response.Message = "Amount request rejected.";
                    response.RstKey = 2;
                }
                else
                {
                    response.Message = "Amount request not rejected.";
                    response.RstKey = 2;
                }
                ex.Message.ErrorLog("PaymentRequestServices.cs", "ManagePaymentRequest");

            }
            LogTransactionTypes.Response.SaveTransactionLog(LogTransactionNameTypes.WalletTransaction + "Manage Pay Money Request", response, "");
            return response;

        }

        public async Task<ViewTransactionResponse> ViewTransactions(ViewTransactionRequest request, string token)////
        {
            var response = new ViewTransactionResponse();
            try
            {
                var data = await _walletUserService.UserProfile(token);
                if (data != null && data.WalletUserId > 0)
                {
                    response = await _paymentRequestRepository.ViewTransactions(request, data.WalletUserId);
                    response.RstKey = 1;
                }
                else
                {
                    response.RstKey = 2;
                }
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("PaymentRequestServices.cs", "ViewPaymentRequests");

            }
            return response;

        }

        public async Task<DownloadReportResponse> DownloadReport(DownloadReportApiRequest request, string token)////
        {
            var response = new DownloadReportResponse();
            var data = await _walletUserService.UserProfile(token);
            response = await _paymentRequestRepository.DownloadReportWithData(request);

            return response;
        }


        public async Task<DownloadReportResponse> DownloadReportForApp(DownloadReportApiRequest request, string token)////
        {
            DownloadReportResponse response = new DownloadReportResponse();
            List<ReportData> list = new List<ReportData>();
            if (token != null)
            {
                var data = await _walletUserService.UserProfile(token);
                request.WalletUserId = data.WalletUserId;
            }
            response = await _paymentRequestRepository.DownloadReportForApp(request);
            return response;
        }
        //
        public async Task<List<WalletUser>> GetWalletUser()
        {
            var response = new List<WalletUser>();
            response = await _paymentRequestRepository.GetWalletUser();
            return response;
        }

        public async Task<DownloadReportResponse> Txndetailperuser(long WalletUserId)////
        {
            var response = new DownloadReportResponse();
            response = await _paymentRequestRepository.Txndetailperuser(WalletUserId);
            return response;
        }
        public async Task<DownloadReportResponse> SendTxndetailperuser(List<UserTxnReportData> model, MemoryStream memoryStream, long WalletUserId)
        {
            try
            {
                //--------send mail on success transaction--------
                var senderdata = await _walletUserRepository.GetUserDetailById(WalletUserId);

                //  string filename = AppSetting.successfullTransaction;
                var body = _sendEmails.ReadTemplateEmailformats("template.html");
                body = body.Replace("$$FirstLastName$$", senderdata.FirstName.ToUpper() + " " + senderdata.LastName.ToUpper());

                //body = body.Replace("$$DisplayContent$$", Request.channel);
                //body = body.Replace("$$customer$$", Request.customer);
                //body = body.Replace("$$amount$$", "XOF " + Request.Amount);
                //body = body.Replace("$$ServiceTaxAmount$$", "XOF " + _commission.CommissionAmount);
                //body = body.Replace("$$AmountWithCommission$$", "XOF " + _commission.AmountWithCommission);
                //body = body.Replace("$$TransactionId$$", _responseModel.TransactionId);
                var req = new EmailModel
                {
                    TO = senderdata.EmailId,
                    Subject = "Last Month Transaction Statement",
                    Body = body
                };
                _sendEmails.SendEmailTxn(req, memoryStream);

            }
            catch
            {

            }
            return null;
        }

    }
}
