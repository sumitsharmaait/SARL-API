using ezeePay.Utility.CommonClass;
using ezeePay.Utility.Enums;
using Ezipay.Database;
using Ezipay.Repository.CardPayment;
using Ezipay.Repository.CommonRepo;
using Ezipay.Repository.MasterData;
using Ezipay.Repository.ThridPartyApiRepo;
using Ezipay.Repository.UserRepo;
using Ezipay.Service.MerchantPayment;
using Ezipay.Service.MobileMoneyService;
using Ezipay.Service.UserService;
using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.Utility.SendEmail;
using Ezipay.Utility.SendPush;
using Ezipay.ViewModel.AirtimeViewModel;
using Ezipay.ViewModel.BillViewModel;
using Ezipay.ViewModel.CommisionViewModel;
using Ezipay.ViewModel.MerchantPaymentViewModel;
using Ezipay.ViewModel.SendEmailViewModel;
using Ezipay.ViewModel.SendPushViewModel;
using Ezipay.ViewModel.ThridPartyApiVIewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace Ezipay.Service.ThridPartyApiService
{
    public class ThridPartyApiServices : IThridPartyApiServices
    {
        private IThridPartyApiRepository _thridPartyApiRepository;
        private IWalletUserRepository _walletUserRepository;
        private ISendPushNotification _sendPushNotification;
        private ISendEmails _sendEmails;
        private ICommonRepository _commonRepository;
        private IMerchantPaymentService _merchantPaymentService;
        private ICardPaymentRepository _cardPaymentRepository;
        private IWalletUserService _walletUserService;
        private IMobileMoneyServices _mobileMoneyServices;
        private IMasterDataRepository _masterDataRepository;
        public ThridPartyApiServices()
        {
            _thridPartyApiRepository = new ThridPartyApiRepository();
            _walletUserRepository = new WalletUserRepository();
            _sendEmails = new SendEmails();
            _commonRepository = new CommonRepository();
            _merchantPaymentService = new MerchantPaymentService();
            _cardPaymentRepository = new CardPaymentRepository();
            _sendPushNotification = new SendPushNotification();
            _walletUserService = new WalletUserService();
            _mobileMoneyServices = new MobileMoneyServices();
            _masterDataRepository = new MasterDataRepository();
        }

        public async Task<UpdateTransactionResponse> UpdateTransactionStatus(UpdateTransactionRequest request)
        {
            var response = new UpdateTransactionResponse();
            var data = new WalletUser();
            string TransactionType = string.Empty;
            decimal currentBalance = 0;
            // decimal CommisionAmount = 0;
            decimal FinalAmount = 0;
            try
            {

                if (request.TransactionId != null)
                {
                    var transaction = await _thridPartyApiRepository.GetWalletTransaction(request.TransactionId, request.OperatorType, request.InvoiceNo);

                    if (transaction != null)
                    {
                        var walletserviceName = await _thridPartyApiRepository.GetWalletService(Convert.ToInt32(transaction.WalletServiceId));
                        var getInitialTransaction = await _cardPaymentRepository.GetTransactionInitiateRequest(request.InvoiceNo);
                        TransactionType = transaction.TransactionType;
                        //
                        //var WalletTxnUpdateList = await _thridPartyApiRepository.WalletTxnUpdateList(request.TransactionId, request.InvoiceNo, request.UpdatebyAdminWalletID);
                        var WalletTxnUpdateList = await _thridPartyApiRepository.WalletTxnUpdateList(request.TransactionId, request.InvoiceNo, request.UpdatebyAdminWalletID, request.StatusCode);
                        LogTransactionTypes.Request.SaveTransactionLog(LogTransactionNameTypes.UpdateTransaction + TransactionType, request, "");
                        int StatusCode = 0;
                        if (request.StatusCode == AggregatorySTATUSCODES.SUCCESSFUL)
                        {
                            StatusCode = (int)TransactionStatus.Completed;
                            try
                            {
                                //--------send mail on success transaction--------                          
                                var senderdata = await _walletUserRepository.GetUserDetailById(Convert.ToInt32(transaction.SenderId));
                                string filename = CommonSetting.successfullTransaction;
                                var body = _sendEmails.ReadEmailformats(filename);
                                body = body.Replace("$$FirstName$$", senderdata.FirstName + " " + senderdata.LastName);
                                body = body.Replace("$$DisplayContent$$", walletserviceName.ServiceName);
                                body = body.Replace("$$customer$$", transaction.AccountNo);
                                body = body.Replace("$$amount$$", "XOF " + transaction.WalletAmount);
                                body = body.Replace("$$ServiceTaxAmount$$", "XOF " + transaction.CommisionAmount);
                                body = body.Replace("$$AmountWithCommission$$", "XOF " + transaction.TotalAmount);
                                body = body.Replace("$$TransactionId$$", transaction.TransactionId);
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
                        if (request.StatusCode == AggregatorySTATUSCODES.SUCCESSFUL && transaction.TransactionStatus == (int)TransactionStatus.Failed)
                        {
                            StatusCode = (int)TransactionStatus.Completed;
                            getInitialTransaction.TransactionStatus = (int)TransactionStatus.Completed;
                            data = await _commonRepository.GetWalletUserById(Convert.ToInt32(transaction.SenderId));
                            if (transaction.TransactionType == AggragatorServiceType.CREDIT)
                            {
                                currentBalance = Math.Round(Convert.ToDecimal(data.CurrentBalance) - Convert.ToDecimal(transaction.TotalAmount), 2);
                                data.CurrentBalance = Convert.ToString(currentBalance);
                                getInitialTransaction.AfterTransactionBalance = currentBalance.ToString();
                                getInitialTransaction.ReceiverCurrentBalance = currentBalance.ToString();
                                getInitialTransaction.ReceiverWalletUserId = data.WalletUserId;
                                await _cardPaymentRepository.UpdateTransactionInitiateRequest(getInitialTransaction);
                                var userResponse = await _walletUserRepository.UpdateUserDetail(data);
                            }
                            try
                            {
                                //--------send mail on success transaction--------                          
                                var senderdata = await _walletUserRepository.GetUserDetailById(Convert.ToInt32(transaction.SenderId));
                                string filename = CommonSetting.successfullTransaction;
                                var body = _sendEmails.ReadEmailformats(filename);
                                body = body.Replace("$$FirstName$$", senderdata.FirstName + " " + senderdata.LastName);
                                body = body.Replace("$$DisplayContent$$", walletserviceName.ServiceName);
                                body = body.Replace("$$customer$$", transaction.AccountNo);
                                body = body.Replace("$$amount$$", "XOF " + transaction.WalletAmount);
                                body = body.Replace("$$ServiceTaxAmount$$", "XOF " + transaction.CommisionAmount);
                                body = body.Replace("$$AmountWithCommission$$", "XOF " + transaction.TotalAmount);
                                body = body.Replace("$$TransactionId$$", transaction.TransactionId);
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
                        if (request.StatusCode == AggregatorySTATUSCODES.PENDING)
                        {
                            StatusCode = (int)TransactionStatus.Pending;
                        }
                        if (request.StatusCode == AggregatorySTATUSCODES.FAILED)
                        {
                            StatusCode = (int)TransactionStatus.Rejected;
                        }
                        if (StatusCode != 0)
                        {
                            if (transaction.TransactionType == AggragatorServiceType.DEBIT)
                            {
                                data = await _commonRepository.GetWalletUserById(Convert.ToInt32(transaction.ReceiverId));
                            }
                            else
                            {
                                data = await _commonRepository.GetWalletUserById(Convert.ToInt32(transaction.SenderId));
                            }
                            if (data != null)
                            {
                                // if (request.StatusCode == AggregatorySTATUSCODES.SUCCESSFUL && transaction.TransactionStatus != (int)TransactionStatus.Completed)
                                if (request.StatusCode == AggregatorySTATUSCODES.SUCCESSFUL && transaction.TransactionStatus == (int)TransactionStatus.Pending)
                                {


                                    //if (transaction.TransactionType == AggragatorServiceType.DEBIT)
                                    //{
                                    //    currentBalance = Math.Round(Convert.ToDecimal(data.CurrentBalance) + Convert.ToDecimal(transaction.WalletAmount), 2);

                                    //}
                                    //else
                                    //{
                                    //    //currentBalance = Math.Round(Convert.ToDecimal(data.CurrentBalance) - Convert.ToDecimal(transaction.WalletAmount), 2);
                                    //}
                                    if (transaction.TransactionType == AggragatorServiceType.DEBIT && request.StatusCode == AggregatorySTATUSCODES.SUCCESSFUL)
                                    {
                                        data = await _commonRepository.GetWalletUserById(Convert.ToInt32(data.WalletUserId));
                                        if (data != null)
                                        {
                                            currentBalance = Math.Round(Convert.ToDecimal(data.CurrentBalance) + Convert.ToDecimal(transaction.WalletAmount), 2);
                                            data.CurrentBalance = Convert.ToString(currentBalance);
                                            getInitialTransaction.AfterTransactionBalance = data.CurrentBalance;
                                            getInitialTransaction.ReceiverCurrentBalance = data.CurrentBalance;
                                            getInitialTransaction.ReceiverWalletUserId = data.WalletUserId;
                                        }
                                    }
                                    //if (transaction.TransactionType == AggragatorServiceType.DEBIT && request.StatusCode == AggregatorySTATUSCODES.SUCCESSFUL)
                                    //{
                                    //    data = await _commonRepository.GetWalletUserById(Convert.ToInt32(data.WalletUserId));
                                    //    if (data != null)
                                    //    {
                                    //        data.CurrentBalance = Convert.ToString(currentBalance);
                                    //    }
                                    //}
                                    //db.SaveChanges();
                                    await _cardPaymentRepository.UpdateTransactionInitiateRequest(getInitialTransaction);
                                    var userResponse = await _walletUserRepository.UpdateUserDetail(data);
                                    #region PayMoneyAfterAdd
                                    if (transaction.IsAddDuringPay && transaction.TransactionType == AggragatorServiceType.DEBIT && transaction.TransactionTypeInfo == (int)TransactionTypeInfo.AddedByMobileMoney)
                                    {
                                        var storeddata = await _thridPartyApiRepository.GetAddDuringPayRecord(transaction.TransactionId, (int)TransactionStatus.Pending);
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
                                                    var merchantRequest = new MerchantTransactionRequest();
                                                    merchantRequest.Amount = _record.Amount;
                                                    merchantRequest.Comment = _record.Comment;
                                                    merchantRequest.MerchantId = _record.MerchantId;
                                                    var merchantResponse = await _merchantPaymentService.MerchantPayment(merchantRequest, null, (long)transaction.SenderId);
                                                    if (merchantResponse.StatusCode == (int)TransactionStatus.Completed)
                                                    {

                                                        storeddata.TransactionStatus = (int)TransactionStatus.Completed;
                                                        //db.SaveChanges();
                                                        await _cardPaymentRepository.UpdateAddDuringPayRecord(storeddata);

                                                    }
                                                }
                                                else
                                                {
                                                    var payResponse = new AddMoneyAggregatorResponse();
                                                    if (storeddata.ServiceCategoryId == 10)
                                                    {
                                                        payResponse = await _mobileMoneyServices.MobileMoneyService(_record, (long)transaction.SenderId);
                                                    }

                                                    if (payResponse.RstKey == 1)
                                                    {
                                                        storeddata.TransactionStatus = (int)TransactionStatus.Completed;
                                                        //db.SaveChanges();
                                                        await _cardPaymentRepository.UpdateAddDuringPayRecord(storeddata);
                                                    }
                                                }

                                            }
                                        }
                                    }
                                    #endregion

                                    #region PushNotification


                                    var pushModel = new PayMoneyPushModel();
                                    if (transaction.TransactionType == AggragatorServiceType.DEBIT)
                                    {
                                        pushModel.alert = transaction.WalletAmount + " XOF has been credited to your account.";
                                    }
                                    else
                                    {
                                        pushModel.alert = transaction.TotalAmount + " XOF has been debited from your account.";
                                    }

                                    pushModel.Amount = transaction.TotalAmount;
                                    pushModel.CurrentBalance = data.CurrentBalance;
                                    if (transaction.TransactionType == AggragatorServiceType.DEBIT)
                                    {
                                        pushModel.pushType = (int)PushType.ADDMONEY;
                                        pushModel.TransactionTypeInfo = transaction.TransactionTypeInfo ?? 0;
                                    }
                                    if (transaction.TransactionType == AggragatorServiceType.CREDIT)
                                    {
                                        pushModel.pushType = (int)PushType.PAYMONEY;
                                        if (transaction.TransactionTypeInfo == (int)TransactionTypeInfo.PaidByPayServices)
                                        {
                                            pushModel.pushType = (int)PushType.PAYSERVICES;
                                        }
                                        pushModel.TransactionTypeInfo = transaction.TransactionTypeInfo ?? 0;
                                    }

                                    var push = new PushNotificationModel();
                                    push.SenderId = (long)transaction.SenderId;
                                    pushModel.TransactionDate = DateTime.UtcNow;
                                    pushModel.TransactionId = transaction.WalletTransactionId.ToString();
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
                                }
                                else if (request.StatusCode == AggregatorySTATUSCODES.SUCCESSFUL && transaction.TransactionStatus == (int)TransactionStatus.Failed)
                                {


                                    if (transaction.TransactionType == AggragatorServiceType.DEBIT)
                                    {
                                        currentBalance = Math.Round(Convert.ToDecimal(data.CurrentBalance) + Convert.ToDecimal(transaction.WalletAmount), 2);

                                        getInitialTransaction.AfterTransactionBalance = currentBalance.ToString();
                                        getInitialTransaction.ReceiverCurrentBalance = currentBalance.ToString();

                                    }
                                    else
                                    {
                                        //currentBalance = Math.Round(Convert.ToDecimal(data.CurrentBalance) - Convert.ToDecimal(transaction.WalletAmount), 2);
                                    }
                                    if (transaction.TransactionType == AggragatorServiceType.DEBIT && request.StatusCode == AggregatorySTATUSCODES.SUCCESSFUL)
                                    {
                                        data = await _commonRepository.GetWalletUserById(Convert.ToInt32(data.WalletUserId));
                                        if (data != null)
                                        {
                                            data.CurrentBalance = Convert.ToString(currentBalance);
                                            getInitialTransaction.AfterTransactionBalance = currentBalance.ToString();
                                            getInitialTransaction.ReceiverCurrentBalance = currentBalance.ToString();
                                        }
                                    }
                                    await _cardPaymentRepository.UpdateTransactionInitiateRequest(getInitialTransaction);
                                    var userResponse = await _walletUserRepository.UpdateUserDetail(data);
                                    #region PayMoneyAfterAdd
                                    if (transaction.IsAddDuringPay && transaction.TransactionType == AggragatorServiceType.DEBIT && transaction.TransactionTypeInfo == (int)TransactionTypeInfo.AddedByMobileMoney)
                                    {
                                        var storeddata = await _thridPartyApiRepository.GetAddDuringPayRecord(transaction.TransactionId, (int)TransactionStatus.Pending);
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
                                                    var merchantRequest = new MerchantTransactionRequest();
                                                    merchantRequest.Amount = _record.Amount;
                                                    merchantRequest.Comment = _record.Comment;
                                                    merchantRequest.MerchantId = _record.MerchantId;
                                                    var merchantResponse = await _merchantPaymentService.MerchantPayment(merchantRequest, null, (long)transaction.SenderId);
                                                    if (merchantResponse.StatusCode == (int)TransactionStatus.Completed)
                                                    {

                                                        storeddata.TransactionStatus = (int)TransactionStatus.Completed;
                                                        //db.SaveChanges();
                                                        await _cardPaymentRepository.UpdateAddDuringPayRecord(storeddata);

                                                    }
                                                }
                                                else
                                                {
                                                    var payResponse = new AddMoneyAggregatorResponse();
                                                    if (storeddata.ServiceCategoryId == 10)
                                                    {
                                                        payResponse = await _mobileMoneyServices.MobileMoneyService(_record, (long)transaction.SenderId);
                                                    }

                                                    if (payResponse.RstKey == 1)
                                                    {
                                                        storeddata.TransactionStatus = (int)TransactionStatus.Completed;
                                                        //db.SaveChanges();
                                                        await _cardPaymentRepository.UpdateAddDuringPayRecord(storeddata);
                                                    }
                                                }

                                            }
                                        }
                                    }
                                    #endregion

                                    #region PushNotification


                                    var pushModel = new PayMoneyPushModel();
                                    if (transaction.TransactionType == AggragatorServiceType.DEBIT)
                                    {
                                        pushModel.alert = transaction.WalletAmount + " XOF has been credited to your account.";
                                    }
                                    else
                                    {
                                        pushModel.alert = transaction.TotalAmount + " XOF has been debited from your account.";
                                    }

                                    pushModel.Amount = transaction.TotalAmount;
                                    pushModel.CurrentBalance = data.CurrentBalance;
                                    if (transaction.TransactionType == AggragatorServiceType.DEBIT)
                                    {
                                        pushModel.pushType = (int)PushType.ADDMONEY;
                                        pushModel.TransactionTypeInfo = transaction.TransactionTypeInfo ?? 0;
                                    }
                                    if (transaction.TransactionType == AggragatorServiceType.CREDIT)
                                    {
                                        pushModel.pushType = (int)PushType.PAYMONEY;
                                        if (transaction.TransactionTypeInfo == (int)TransactionTypeInfo.PaidByPayServices)
                                        {
                                            pushModel.pushType = (int)PushType.PAYSERVICES;
                                        }
                                        pushModel.TransactionTypeInfo = transaction.TransactionTypeInfo ?? 0;
                                    }

                                    var push = new PushNotificationModel();
                                    push.SenderId = (long)transaction.SenderId;
                                    pushModel.TransactionDate = DateTime.UtcNow;
                                    pushModel.TransactionId = transaction.WalletTransactionId.ToString();
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
                                }
                                else if (request.StatusCode != AggregatorySTATUSCODES.SUCCESSFUL)
                                {
                                    #region PushNotification

                                    var pushModel = new PayMoneyPushModel();
                                    pushModel.TransactionDate = DateTime.UtcNow;
                                    pushModel.TransactionId = transaction.WalletTransactionId.ToString();
                                    if (request.StatusCode == AggregatorySTATUSCODES.PENDING)
                                    {
                                        pushModel.alert = transaction.WalletAmount + " XOF transaction has been marked as pending.";
                                    }
                                    else
                                    {
                                        pushModel.alert = transaction.WalletAmount + " XOF transaction has been failed.";
                                        if (transaction.TransactionType == AggragatorServiceType.CREDIT)
                                        {
                                            //var user = db.WalletUsers.Where(x => x.WalletUserId == data.WalletUserId).FirstOrDefault();
                                            var user = await _commonRepository.GetWalletUserById(data.WalletUserId);
                                            if (user != null)
                                            {
                                                FinalAmount = Math.Round(Convert.ToDecimal(user.CurrentBalance) + Convert.ToDecimal(transaction.TotalAmount), 2);
                                                user.CurrentBalance = Convert.ToString(FinalAmount);
                                                getInitialTransaction.AfterTransactionBalance = user.CurrentBalance;
                                                getInitialTransaction.ReceiverCurrentBalance = user.CurrentBalance;

                                                await _cardPaymentRepository.UpdateTransactionInitiateRequest(getInitialTransaction);
                                                var userResponse = await _walletUserRepository.UpdateUserDetail(user);
                                            }
                                        }
                                    }
                                    pushModel.Amount = transaction.WalletAmount;
                                    pushModel.CurrentBalance = data.CurrentBalance;
                                    if (transaction.TransactionType == AggragatorServiceType.DEBIT)
                                    {
                                        pushModel.pushType = (int)PushType.ADDMONEY;
                                        pushModel.TransactionTypeInfo = transaction.TransactionTypeInfo ?? 0;
                                    }
                                    if (transaction.TransactionType == AggragatorServiceType.CREDIT)
                                    {
                                        pushModel.pushType = (int)PushType.PAYMONEY;
                                        pushModel.TransactionTypeInfo = transaction.TransactionTypeInfo ?? 0;
                                    }


                                    var push = new PushNotificationModel();
                                    push.deviceType = (int)data.DeviceType;
                                    push.deviceKey = data.DeviceToken;
                                    if ((int)data.DeviceType == (int)DeviceTypes.ANDROID || (int)data.DeviceType == (int)DeviceTypes.Web)
                                    {
                                        var aps = new PushPayload<PayMoneyPushModel>();
                                        var _data = new PushPayloadData<PayMoneyPushModel>();
                                        _data.notification = pushModel;
                                        push.payload = pushModel;
                                        aps.data = _data;
                                        aps.to = data.DeviceToken;
                                        aps.collapse_key = string.Empty;
                                        push.message = JsonConvert.SerializeObject(aps);
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
                                    _sendPushNotification.sendPushNotification(push);
                                    #endregion
                                }
                                if (request.StatusCode == AggregatorySTATUSCODES.FAILED)
                                {
                                    transaction.Comments = transaction.Comments + ";refund amount :- " + transaction.TotalAmount + " against Txn Id :- " + transaction.TransactionId + " & txn done on :- " + transaction.CreatedDate ;
                                }
                                if (request.StatusCode == AggregatorySTATUSCODES.SUCCESSFUL)
                                {
                                    transaction.Comments = transaction.Comments + ";credited amount :- " + transaction.TotalAmount + " against Txn Id :- " + transaction.TransactionId + " & txn done on :- " + transaction.CreatedDate;
                                }


                                transaction.TransactionStatus = StatusCode;
                                transaction.UpdatedDate = DateTime.UtcNow;
                                //db.SaveChanges();
                                await _cardPaymentRepository.UpdateTransactionInitiateRequest(getInitialTransaction);
                                await _thridPartyApiRepository.UpdateWalletTransaction(transaction);

                                response.isSuccess = true;
                                response.status = (int)HttpStatusCode.OK;
                                response.Message = "Transaction status updated successfully.";
                                // Response.Create(true, "Transaction status updated successfully.", HttpStatusCode.OK, new UpdateTransactionResponse { isSuccess = true, status = (int)HttpStatusCode.OK });
                            }
                            else
                            {
                                response.isSuccess = false;
                                response.status = (int)HttpStatusCode.NotModified;
                                response.Message = "Transaction status not updated.";
                                //Response.Create(true, "Transaction status not updated.", HttpStatusCode.NotModified, new UpdateTransactionResponse { isSuccess = false, status = (int)HttpStatusCode.NotModified });
                            }
                        }
                        else
                        {
                            response.isSuccess = false;
                            response.status = (int)HttpStatusCode.NotModified;
                            response.Message = "Transaction status not updated.";
                            // Response.Create(true, "Transaction status not updated.", HttpStatusCode.NotModified, new UpdateTransactionResponse { isSuccess = false, status = (int)HttpStatusCode.NotModified });
                        }
                    }
                    else
                    {
                        response.isSuccess = false;
                        response.status = (int)HttpStatusCode.NotModified;
                        response.Message = "Incorrect transaction id or transaction status already updated.";
                        //Response.Create(true, "Incorrect transaction id.", HttpStatusCode.NotModified, new UpdateTransactionResponse { isSuccess = false, status = (int)HttpStatusCode.NotModified });
                    }
                }
                else if (request.gu_transaction_id != null)
                {
                    var transaction = await _thridPartyApiRepository.GetSochitelWalletTransaction(request.gu_transaction_id, request.partner_transaction_id);

                    if (transaction != null)
                    {
                        var walletserviceName = await _thridPartyApiRepository.GetWalletService(Convert.ToInt32(transaction.WalletServiceId));
                        var getInitialTransaction = await _cardPaymentRepository.GetTransactionInitiateRequest(request.partner_transaction_id);
                        TransactionType = transaction.TransactionType;
                        LogTransactionTypes.Request.SaveTransactionLog(LogTransactionNameTypes.UpdateTransaction + TransactionType, request, "");
                        int StatusCode = 0;
                        if (request.status.ToUpper() == AggregatoryMESSAGE.SUCCESSFUL && transaction.TransactionStatus == (int)TransactionStatus.Pending && (transaction.TransactionStatus != (int)TransactionStatus.Failed || transaction.TransactionStatus != (int)TransactionStatus.Rejected))
                        {
                            StatusCode = (int)TransactionStatus.Completed;
                            try
                            {
                                //--------send mail on success transaction--------                          
                                var senderdata = await _walletUserRepository.GetUserDetailById(Convert.ToInt32(transaction.SenderId));
                                string filename = CommonSetting.successfullTransaction;
                                var body = _sendEmails.ReadEmailformats(filename);
                                body = body.Replace("$$FirstName$$", senderdata.FirstName + " " + senderdata.LastName);
                                body = body.Replace("$$DisplayContent$$", walletserviceName.ServiceName);
                                body = body.Replace("$$customer$$", transaction.AccountNo);
                                body = body.Replace("$$amount$$", "XOF " + transaction.WalletAmount);
                                body = body.Replace("$$ServiceTaxAmount$$", "XOF " + transaction.CommisionAmount);
                                body = body.Replace("$$AmountWithCommission$$", "XOF " + transaction.TotalAmount);
                                body = body.Replace("$$TransactionId$$", transaction.TransactionId);
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
                        if (request.status.ToUpper() == AggregatoryMESSAGE.SUCCESSFUL && (transaction.TransactionStatus == (int)TransactionStatus.Failed || transaction.TransactionStatus == (int)TransactionStatus.Rejected))
                        {
                            StatusCode = (int)TransactionStatus.Completed;
                            getInitialTransaction.TransactionStatus = (int)TransactionStatus.Completed;
                            data = await _commonRepository.GetWalletUserById(Convert.ToInt32(transaction.SenderId));
                            if (transaction.TransactionType == AggragatorServiceType.CREDIT)
                            {
                                currentBalance = Math.Round(Convert.ToDecimal(data.CurrentBalance) - Convert.ToDecimal(transaction.TotalAmount), 2);
                                data.CurrentBalance = Convert.ToString(currentBalance);
                                getInitialTransaction.AfterTransactionBalance = currentBalance.ToString();
                                getInitialTransaction.ReceiverCurrentBalance = currentBalance.ToString();
                                getInitialTransaction.ReceiverWalletUserId = data.WalletUserId;
                                await _cardPaymentRepository.UpdateTransactionInitiateRequest(getInitialTransaction);
                                var userResponse = await _walletUserRepository.UpdateUserDetail(data);
                            }
                            try
                            {
                                //--------send mail on success transaction--------                          
                                var senderdata = await _walletUserRepository.GetUserDetailById(Convert.ToInt32(transaction.SenderId));
                                string filename = CommonSetting.successfullTransaction;
                                var body = _sendEmails.ReadEmailformats(filename);
                                body = body.Replace("$$FirstName$$", senderdata.FirstName + " " + senderdata.LastName);
                                body = body.Replace("$$DisplayContent$$", walletserviceName.ServiceName);
                                body = body.Replace("$$customer$$", transaction.AccountNo);
                                body = body.Replace("$$amount$$", "GHS " + transaction.WalletAmount);
                                body = body.Replace("$$ServiceTaxAmount$$", "GHS " + transaction.CommisionAmount);
                                body = body.Replace("$$AmountWithCommission$$", "GHS " + transaction.TotalAmount);
                                body = body.Replace("$$TransactionId$$", transaction.TransactionId);
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
                        //if (request.StatusCode == AggregatorySTATUSCODES.SUCCESSFUL)
                        if (request.status.ToUpper() == AggregatoryMESSAGE.SUCCESSFUL)
                        {
                            StatusCode = (int)TransactionStatus.Completed;
                        }
                        if (request.status.ToUpper() == AggregatoryMESSAGE.PENDING)
                        {
                            StatusCode = (int)TransactionStatus.Pending;
                        }
                        if (request.status.ToUpper() == AggregatoryMESSAGE.FAILED)
                        {
                            StatusCode = (int)TransactionStatus.Rejected;
                        }
                        if (StatusCode != 0)
                        {
                            // var data = new WalletUser();
                            if (transaction.TransactionType == AggragatorServiceType.DEBIT)
                            {
                                data = await _commonRepository.GetWalletUserById(Convert.ToInt32(transaction.ReceiverId));
                            }
                            else
                            {
                                data = await _commonRepository.GetWalletUserById(Convert.ToInt32(transaction.SenderId));
                            }
                            if (data != null)
                            {
                                if (request.status == AggregatoryMESSAGE.SUCCESSFUL && transaction.TransactionStatus == (int)TransactionStatus.Pending)
                                {


                                    //if (transaction.TransactionType == AggragatorServiceType.CREDIT)
                                    //{
                                    //    currentBalance = Math.Round(Convert.ToDecimal(data.CurrentBalance) - Convert.ToDecimal(transaction.TotalAmount), 2);
                                    //    data.CurrentBalance = Convert.ToString(currentBalance);
                                    //    getInitialTransaction.AfterTransactionBalance = currentBalance.ToString();
                                    //}
                                    if (transaction.TransactionType == AggragatorServiceType.DEBIT && request.StatusCode == AggregatorySTATUSCODES.SUCCESSFUL)
                                    {
                                        data = await _commonRepository.GetWalletUserById(Convert.ToInt32(data.WalletUserId));
                                        if (data != null)
                                        {
                                            currentBalance = Math.Round(Convert.ToDecimal(data.CurrentBalance) + Convert.ToDecimal(transaction.WalletAmount), 2);
                                            data.CurrentBalance = Convert.ToString(currentBalance);
                                            getInitialTransaction.AfterTransactionBalance = data.CurrentBalance;
                                            getInitialTransaction.ReceiverCurrentBalance = data.CurrentBalance;
                                            getInitialTransaction.ReceiverWalletUserId = data.WalletUserId;
                                        }
                                    }
                                    await _cardPaymentRepository.UpdateTransactionInitiateRequest(getInitialTransaction);
                                    var userResponse = await _walletUserRepository.UpdateUserDetail(data);
                                    #region PayMoneyAfterAdd
                                    if (transaction.IsAddDuringPay && transaction.TransactionType == AggragatorServiceType.DEBIT && transaction.TransactionTypeInfo == (int)TransactionTypeInfo.AddedByMobileMoney)
                                    {
                                        var storeddata = await _thridPartyApiRepository.GetAddDuringPayRecord(transaction.TransactionId, (int)TransactionStatus.Pending);
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
                                                    var merchantRequest = new MerchantTransactionRequest();
                                                    merchantRequest.Amount = _record.Amount;
                                                    merchantRequest.Comment = _record.Comment;
                                                    merchantRequest.MerchantId = _record.MerchantId;
                                                    var merchantResponse = await _merchantPaymentService.MerchantPayment(merchantRequest, null, (long)transaction.SenderId);
                                                    if (merchantResponse.StatusCode == (int)TransactionStatus.Completed)
                                                    {

                                                        storeddata.TransactionStatus = (int)TransactionStatus.Completed;
                                                        //db.SaveChanges();
                                                        await _cardPaymentRepository.UpdateAddDuringPayRecord(storeddata);

                                                    }
                                                }
                                                else
                                                {
                                                    var payResponse = new AddMoneyAggregatorResponse();
                                                    if (storeddata.ServiceCategoryId == 10)
                                                    {
                                                        payResponse = await _mobileMoneyServices.MobileMoneyService(_record, (long)transaction.SenderId);
                                                    }

                                                    if (payResponse.RstKey == 1)
                                                    {
                                                        storeddata.TransactionStatus = (int)TransactionStatus.Completed;
                                                        //db.SaveChanges();
                                                        await _cardPaymentRepository.UpdateAddDuringPayRecord(storeddata);
                                                    }
                                                }

                                            }
                                        }
                                    }
                                    #endregion

                                    #region PushNotification


                                    var pushModel = new PayMoneyPushModel();
                                    if (transaction.TransactionType == AggragatorServiceType.DEBIT)
                                    {
                                        pushModel.alert = transaction.WalletAmount + " XOF has been credited to your account.";
                                    }
                                    else
                                    {
                                        pushModel.alert = transaction.TotalAmount + " XOF has been debited from your account.";
                                    }

                                    pushModel.Amount = transaction.TotalAmount;
                                    pushModel.CurrentBalance = data.CurrentBalance;
                                    if (transaction.TransactionType == AggragatorServiceType.DEBIT)
                                    {
                                        pushModel.pushType = (int)PushType.ADDMONEY;
                                        pushModel.TransactionTypeInfo = transaction.TransactionTypeInfo ?? 0;
                                    }
                                    if (transaction.TransactionType == AggragatorServiceType.CREDIT)
                                    {
                                        pushModel.pushType = (int)PushType.PAYMONEY;
                                        if (transaction.TransactionTypeInfo == (int)TransactionTypeInfo.PaidByPayServices)
                                        {
                                            pushModel.pushType = (int)PushType.PAYSERVICES;
                                        }
                                        pushModel.TransactionTypeInfo = transaction.TransactionTypeInfo ?? 0;
                                    }

                                    var push = new PushNotificationModel();
                                    push.SenderId = (long)transaction.SenderId;
                                    pushModel.TransactionDate = DateTime.UtcNow;
                                    pushModel.TransactionId = transaction.WalletTransactionId.ToString();
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
                                }
                                else if (request.status == AggregatoryMESSAGE.SUCCESSFUL && transaction.TransactionStatus == (int)TransactionStatus.Failed)
                                {
                                    if (transaction.TransactionType == AggragatorServiceType.DEBIT)
                                    {
                                        currentBalance = Math.Round(Convert.ToDecimal(data.CurrentBalance) + Convert.ToDecimal(transaction.WalletAmount), 2);

                                        getInitialTransaction.AfterTransactionBalance = currentBalance.ToString();
                                        getInitialTransaction.ReceiverCurrentBalance = currentBalance.ToString();

                                    }
                                    else
                                    {
                                        //currentBalance = Math.Round(Convert.ToDecimal(data.CurrentBalance) - Convert.ToDecimal(transaction.WalletAmount), 2);
                                    }
                                    if (transaction.TransactionType == AggragatorServiceType.DEBIT && request.StatusCode == AggregatorySTATUSCODES.SUCCESSFUL)
                                    {
                                        data = await _commonRepository.GetWalletUserById(Convert.ToInt32(data.WalletUserId));
                                        if (data != null)
                                        {
                                            data.CurrentBalance = Convert.ToString(currentBalance);
                                            getInitialTransaction.AfterTransactionBalance = currentBalance.ToString();
                                            getInitialTransaction.ReceiverCurrentBalance = currentBalance.ToString();
                                        }
                                    }
                                    await _cardPaymentRepository.UpdateTransactionInitiateRequest(getInitialTransaction);
                                    var userResponse = await _walletUserRepository.UpdateUserDetail(data);
                                    #region PayMoneyAfterAdd
                                    if (transaction.IsAddDuringPay && transaction.TransactionType == AggragatorServiceType.DEBIT && transaction.TransactionTypeInfo == (int)TransactionTypeInfo.AddedByMobileMoney)
                                    {
                                        var storeddata = await _thridPartyApiRepository.GetAddDuringPayRecord(transaction.TransactionId, (int)TransactionStatus.Pending);
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
                                                    var merchantRequest = new MerchantTransactionRequest();
                                                    merchantRequest.Amount = _record.Amount;
                                                    merchantRequest.Comment = _record.Comment;
                                                    merchantRequest.MerchantId = _record.MerchantId;
                                                    var merchantResponse = await _merchantPaymentService.MerchantPayment(merchantRequest, null, (long)transaction.SenderId);
                                                    if (merchantResponse.StatusCode == (int)TransactionStatus.Completed)
                                                    {

                                                        storeddata.TransactionStatus = (int)TransactionStatus.Completed;
                                                        //db.SaveChanges();
                                                        await _cardPaymentRepository.UpdateAddDuringPayRecord(storeddata);

                                                    }
                                                }
                                                else
                                                {
                                                    var payResponse = new AddMoneyAggregatorResponse();
                                                    if (storeddata.ServiceCategoryId == 10)
                                                    {
                                                        payResponse = await _mobileMoneyServices.MobileMoneyService(_record, (long)transaction.SenderId);
                                                    }

                                                    if (payResponse.RstKey == 1)
                                                    {
                                                        storeddata.TransactionStatus = (int)TransactionStatus.Completed;
                                                        //db.SaveChanges();
                                                        await _cardPaymentRepository.UpdateAddDuringPayRecord(storeddata);
                                                    }
                                                }

                                            }
                                        }
                                    }
                                    #endregion

                                    #region PushNotification


                                    var pushModel = new PayMoneyPushModel();
                                    if (transaction.TransactionType == AggragatorServiceType.DEBIT)
                                    {
                                        pushModel.alert = transaction.WalletAmount + " XOF has been credited to your account.";
                                    }
                                    else
                                    {
                                        pushModel.alert = transaction.TotalAmount + " XOF has been debited from your account.";
                                    }

                                    pushModel.Amount = transaction.TotalAmount;
                                    pushModel.CurrentBalance = data.CurrentBalance;
                                    if (transaction.TransactionType == AggragatorServiceType.DEBIT)
                                    {
                                        pushModel.pushType = (int)PushType.ADDMONEY;
                                        pushModel.TransactionTypeInfo = transaction.TransactionTypeInfo ?? 0;
                                    }
                                    if (transaction.TransactionType == AggragatorServiceType.CREDIT)
                                    {
                                        pushModel.pushType = (int)PushType.PAYMONEY;
                                        if (transaction.TransactionTypeInfo == (int)TransactionTypeInfo.PaidByPayServices)
                                        {
                                            pushModel.pushType = (int)PushType.PAYSERVICES;
                                        }
                                        pushModel.TransactionTypeInfo = transaction.TransactionTypeInfo ?? 0;
                                    }

                                    var push = new PushNotificationModel();
                                    push.SenderId = (long)transaction.SenderId;
                                    pushModel.TransactionDate = DateTime.UtcNow;
                                    pushModel.TransactionId = transaction.WalletTransactionId.ToString();
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
                                }
                                #region ------
                                //if (request.status.ToUpper() == AggregatoryMESSAGE.FAILED && transaction.TransactionStatus != (int)TransactionStatus.Completed)
                                //{
                                //    if (transaction.TransactionType == AggragatorServiceType.DEBIT)
                                //    {
                                //        currentBalance = Math.Round(Convert.ToDecimal(data.CurrentBalance) + Convert.ToDecimal(transaction.WalletAmount), 2);

                                //    }
                                //    else
                                //    {
                                //        //currentBalance = Math.Round(Convert.ToDecimal(data.CurrentBalance) - Convert.ToDecimal(transaction.WalletAmount), 2);
                                //    }
                                //    if (transaction.TransactionType == AggragatorServiceType.DEBIT && request.status == "SUCCESSFUL")
                                //    {
                                //        data = await _commonRepository.GetWalletUserById(Convert.ToInt32(data.WalletUserId));
                                //        if (data != null)
                                //        {
                                //            data.CurrentBalance = Convert.ToString(currentBalance);
                                //        }
                                //    }
                                //    //db.SaveChanges();
                                //    var userResponse = await _walletUserRepository.UpdateUserDetail(data);
                                //    #region PayMoneyAfterAdd
                                //    if (transaction.IsAddDuringPay && transaction.TransactionType == AggragatorServiceType.DEBIT && transaction.TransactionTypeInfo == (int)TransactionTypeInfo.AddedByMobileMoney)
                                //    {
                                //        var storeddata = await _thridPartyApiRepository.GetAddDuringPayRecord(transaction.TransactionId, (int)TransactionStatus.Pending);
                                //        if (storeddata != null)
                                //        {
                                //            var _record = new PayMoneyAggregatoryRequest();
                                //            _record.Amount = storeddata.amount;
                                //            _record.channel = storeddata.channel;
                                //            _record.chennelId = (int)storeddata.chennelId;
                                //            _record.Comment = storeddata.Comment;
                                //            _record.customer = storeddata.customer;
                                //            _record.invoiceNo = storeddata.invoiceNo;
                                //            _record.IsAddDuringPay = (bool)storeddata.IsAddDuringPay;
                                //            _record.ISD = storeddata.ISD;
                                //            _record.serviceCategory = storeddata.serviceCategory;
                                //            _record.ServiceCategoryId = (int)storeddata.ServiceCategoryId;
                                //            _record.IsMerchant = (bool)storeddata.IsMerchant;
                                //            _record.MerchantId = (long)storeddata.MerchantId;
                                //            if (_record != null)
                                //            {
                                //                if (_record.IsAddDuringPay && _record.IsMerchant && _record.MerchantId > 0)
                                //                {
                                //                    var merchantRequest = new MerchantTransactionRequest();
                                //                    merchantRequest.Amount = _record.Amount;
                                //                    merchantRequest.Comment = _record.Comment;
                                //                    merchantRequest.MerchantId = _record.MerchantId;
                                //                    var merchantResponse = await _merchantPaymentService.MerchantPayment(merchantRequest, (long)transaction.SenderId);
                                //                    if (merchantResponse.StatusCode == (int)TransactionStatus.Completed)
                                //                    {

                                //                        storeddata.TransactionStatus = (int)TransactionStatus.Completed;
                                //                        //db.SaveChanges();
                                //                        await _cardPaymentRepository.UpdateAddDuringPayRecord(storeddata);

                                //                    }
                                //                }
                                //                else
                                //                {
                                //                    var payResponse = new AddMoneyAggregatorResponse();
                                //                    if (storeddata.ServiceCategoryId == 10)
                                //                    {
                                //                        payResponse = await _mobileMoneyServices.MobileMoneyService(_record, (long)transaction.SenderId);
                                //                    }

                                //                    if (payResponse.RstKey == 1)
                                //                    {
                                //                        storeddata.TransactionStatus = (int)TransactionStatus.Completed;
                                //                        //db.SaveChanges();
                                //                        await _cardPaymentRepository.UpdateAddDuringPayRecord(storeddata);
                                //                    }
                                //                }

                                //            }
                                //        }
                                //    }
                                //    #endregion

                                //    #region PushNotification


                                //    var pushModel = new PayMoneyPushModel();
                                //    if (transaction.TransactionType == AggragatorServiceType.DEBIT)
                                //    {
                                //        pushModel.alert = transaction.WalletAmount + " XOF has been credited to your account.";
                                //    }
                                //    else
                                //    {
                                //        pushModel.alert = transaction.TotalAmount + " XOF has been debited from your account.";
                                //    }

                                //    pushModel.Amount = transaction.TotalAmount;
                                //    pushModel.CurrentBalance = data.CurrentBalance;
                                //    if (transaction.TransactionType == AggragatorServiceType.DEBIT)
                                //    {
                                //        pushModel.pushType = (int)PushType.ADDMONEY;
                                //        pushModel.TransactionTypeInfo = transaction.TransactionTypeInfo ?? 0;
                                //    }
                                //    if (transaction.TransactionType == AggragatorServiceType.CREDIT)
                                //    {
                                //        pushModel.pushType = (int)PushType.PAYMONEY;
                                //        if (transaction.TransactionTypeInfo == (int)TransactionTypeInfo.PaidByPayServices)
                                //        {
                                //            pushModel.pushType = (int)PushType.PAYSERVICES;
                                //        }
                                //        pushModel.TransactionTypeInfo = transaction.TransactionTypeInfo ?? 0;
                                //    }

                                //    var push = new PushNotificationModel();
                                //    push.SenderId = (long)transaction.SenderId;
                                //    pushModel.TransactionDate = DateTime.UtcNow;
                                //    pushModel.TransactionId = transaction.WalletTransactionId.ToString();
                                //    push.deviceType = (int)data.DeviceType;
                                //    push.deviceKey = data.DeviceToken;
                                //    if ((int)data.DeviceType == (int)DeviceTypes.ANDROID || (int)data.DeviceType == (int)DeviceTypes.Web)
                                //    {
                                //        var aps = new PushPayload<PayMoneyPushModel>();
                                //        var _data = new PushPayloadData<PayMoneyPushModel>();
                                //        _data.notification = pushModel;
                                //        aps.data = _data;
                                //        aps.to = data.DeviceToken;
                                //        aps.collapse_key = string.Empty;
                                //        push.message = JsonConvert.SerializeObject(aps);
                                //        push.payload = pushModel;

                                //    }
                                //    if ((int)data.DeviceType == (int)DeviceTypes.IOS)
                                //    {
                                //        var aps = new NotificationJsonResponse<PayMoneyIOSPushModel>();
                                //        var _iosPushModel = new PayMoneyIOSPushModel();
                                //        _iosPushModel.alert = pushModel.alert;
                                //        _iosPushModel.Amount = pushModel.Amount;
                                //        _iosPushModel.CurrentBalance = pushModel.CurrentBalance;
                                //        _iosPushModel.MobileNo = pushModel.MobileNo;
                                //        _iosPushModel.SenderName = pushModel.SenderName;
                                //        _iosPushModel.pushType = pushModel.pushType;
                                //        aps.aps = _iosPushModel;

                                //        push.message = JsonConvert.SerializeObject(aps);
                                //    }
                                //    if (!string.IsNullOrEmpty(push.message))
                                //    {
                                //        _sendPushNotification.sendPushNotification(push);
                                //    }
                                //    #endregion
                                //}
                                #endregion
                                else if (request.status.ToUpper() != AggregatoryMESSAGE.SUCCESSFUL)
                                {
                                    #region PushNotification

                                    var pushModel = new PayMoneyPushModel();
                                    pushModel.TransactionDate = DateTime.UtcNow;
                                    pushModel.TransactionId = transaction.WalletTransactionId.ToString();
                                    if (request.status.ToUpper() == AggregatoryMESSAGE.PENDING)
                                    {
                                        pushModel.alert = transaction.WalletAmount + " XOF transaction has been marked as pending.";
                                    }
                                    else
                                    {
                                        pushModel.alert = transaction.WalletAmount + " XOF transaction has been failed.";
                                        if (transaction.TransactionType == AggragatorServiceType.CREDIT)
                                        {
                                            //var user = db.WalletUsers.Where(x => x.WalletUserId == data.WalletUserId).FirstOrDefault();
                                            var user = await _commonRepository.GetWalletUserById(data.WalletUserId);
                                            if (user != null)
                                            {
                                                FinalAmount = Math.Round(Convert.ToDecimal(user.CurrentBalance) + Convert.ToDecimal(transaction.TotalAmount), 2);
                                                user.CurrentBalance = Convert.ToString(FinalAmount);

                                                getInitialTransaction.AfterTransactionBalance = user.CurrentBalance.ToString();
                                                getInitialTransaction.ReceiverCurrentBalance = user.CurrentBalance.ToString();
                                                await _cardPaymentRepository.UpdateTransactionInitiateRequest(getInitialTransaction);
                                                var userResponse = await _walletUserRepository.UpdateUserDetail(user);
                                            }
                                        }
                                    }

                                    pushModel.Amount = transaction.WalletAmount;
                                    pushModel.CurrentBalance = data.CurrentBalance;
                                    if (transaction.TransactionType == AggragatorServiceType.DEBIT)
                                    {
                                        pushModel.pushType = (int)PushType.ADDMONEY;
                                        pushModel.TransactionTypeInfo = transaction.TransactionTypeInfo ?? 0;
                                    }
                                    if (transaction.TransactionType == AggragatorServiceType.CREDIT)
                                    {
                                        pushModel.pushType = (int)PushType.PAYMONEY;
                                        pushModel.TransactionTypeInfo = transaction.TransactionTypeInfo ?? 0;
                                    }


                                    var push = new PushNotificationModel();
                                    push.deviceType = (int)data.DeviceType;
                                    push.deviceKey = data.DeviceToken;
                                    if ((int)data.DeviceType == (int)DeviceTypes.ANDROID || (int)data.DeviceType == (int)DeviceTypes.Web)
                                    {
                                        var aps = new PushPayload<PayMoneyPushModel>();
                                        var _data = new PushPayloadData<PayMoneyPushModel>();
                                        _data.notification = pushModel;
                                        push.payload = pushModel;
                                        aps.data = _data;
                                        aps.to = data.DeviceToken;
                                        aps.collapse_key = string.Empty;
                                        push.message = JsonConvert.SerializeObject(aps);
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
                                    _sendPushNotification.sendPushNotification(push);
                                    #endregion
                                }
                                if (request.StatusCode == AggregatorySTATUSCODES.FAILED)
                                {
                                    transaction.Comments = transaction.Comments + ";refund amount :- " + transaction.TotalAmount + " against InvoiceNo :- " + transaction.InvoiceNo + " & txn done on :- " + transaction.CreatedDate;
                                }
                                if (request.StatusCode == AggregatorySTATUSCODES.SUCCESSFUL)
                                {
                                    transaction.Comments = transaction.Comments + ";credited amount :- " + transaction.TotalAmount + " against Txn Id :- " + transaction.TransactionId + " & txn done on :- " + transaction.CreatedDate;
                                }
                                transaction.TransactionStatus = StatusCode;
                                transaction.UpdatedDate = DateTime.UtcNow;
                                //db.SaveChanges();
                                await _thridPartyApiRepository.UpdateWalletTransaction(transaction);

                                response.isSuccess = true;
                                response.status = (int)HttpStatusCode.OK;
                                response.Message = "Transaction status updated successfully.";
                                // Response.Create(true, "Transaction status updated successfully.", HttpStatusCode.OK, new UpdateTransactionResponse { isSuccess = true, status = (int)HttpStatusCode.OK });
                            }
                            else
                            {
                                response.isSuccess = false;
                                response.status = (int)HttpStatusCode.NotModified;
                                response.Message = "Transaction status not updated.";
                                //Response.Create(true, "Transaction status not updated.", HttpStatusCode.NotModified, new UpdateTransactionResponse { isSuccess = false, status = (int)HttpStatusCode.NotModified });
                            }
                        }
                        else
                        {
                            response.isSuccess = false;
                            response.status = (int)HttpStatusCode.NotModified;
                            response.Message = "Transaction status not updated.";
                            // Response.Create(true, "Transaction status not updated.", HttpStatusCode.NotModified, new UpdateTransactionResponse { isSuccess = false, status = (int)HttpStatusCode.NotModified });
                        }
                    }
                    else
                    {
                        response.isSuccess = false;
                        response.status = (int)HttpStatusCode.NotModified;
                        response.Message = "Incorrect transaction id or transaction status already updated.";
                        //Response.Create(true, "Incorrect transaction id.", HttpStatusCode.NotModified, new UpdateTransactionResponse { isSuccess = false, status = (int)HttpStatusCode.NotModified });
                    }
                }

            }
            catch (Exception ex)
            {
                response.isSuccess = false;
                response.status = (int)HttpStatusCode.NotModified;
                response.Message = "Transaction updation failed due to exception.";
                //Response.Create(true, "Transaction updation failed due to exception : " + ex.Message, HttpStatusCode.NotModified, new UpdateTransactionResponse { isSuccess = false, status = (int)HttpStatusCode.NotModified });
            }
            LogTransactionTypes.Response.SaveTransactionLog(LogTransactionNameTypes.UpdateTransaction + TransactionType, response, "");
            return response;
        }

        public async Task<List<commissionOnAmountModel>> ServiceCommissionList()
        {
            var response = new List<commissionOnAmountModel>();
            var currencyDetail = _masterDataRepository.GetCurrencyRate();
            response = await _thridPartyApiRepository.ServiceCommissionList();                      
            response.ForEach(x => {
                x.AmountInDollar = Convert.ToDecimal(currencyDetail.CediRate);              
                
            }); 
            //for only option2 add_mone//Add Doller Rate
            response.ForEach(x => {               
                x.AmountInNGN = Convert.ToDecimal(currencyDetail.NGNRate);
            });
            //for only option add_mone
            response.ForEach(x => {
                x.AmountInEuro = Convert.ToDecimal(currencyDetail.EuroRate);
            });
            //for only option paymone:- tranfertobank
            response.ForEach(x => {
                x.AmountInSendNGN = Convert.ToDecimal(currencyDetail.SendNGNRate);
            });
            //for only option paymone:- ghana mobmon
            response.ForEach(x => {
                x.AmountInSendGH = Convert.ToDecimal(currencyDetail.SendGHRate);
            });
           
            return response;
        }

        public async Task<FlightBookingResponse> FlightHotelBooking(string token)////
        {

            CalculateCommissionResponse _commission = new CalculateCommissionResponse();
            CalculateCommissionRequest _commissionRequest = new CalculateCommissionRequest();
            var response = new FlightBookingResponse();
            var data = await _walletUserService.UserProfile(token);
            var sender = await _walletUserRepository.GetCurrentUser(data.WalletUserId);
            // bool Isdocverified = await _walletUserRepository.IsDocVerified(data.WalletUserId, data.DocumetStatus);
            bool Isdocverified = await _walletUserRepository.IsDocVerifiedMOMO(data.DocumetStatus);
            response.DocStatus = Isdocverified;
            response.DocumetStatus = (int)sender.DocumetStatus;

            string responseString = string.Empty;
            var results = new FlightBookingResponse();
            string apiUrl = "";

            response.DocumetStatus = (int)sender.DocumetStatus;
            response.IsEmailVerified = (bool)sender.IsEmailVerified;
            response.DocStatus = Isdocverified;
            results.DocumetStatus = (int)sender.DocumetStatus;
            results.DocStatus = Isdocverified;

            //if (!Isdocverified)
            //{

            //    if (results.DocumetStatus == (int)DocumentStatus.Pending)
            //    {
            //        response.RstKey = 6;
            //        response.Message = ResponseMessageKyc.FAILED_Doc_Pending;
            //    }
            //    else if (results.DocumetStatus == (int)DocumentStatus.Rejected)
            //    {
            //        response.RstKey = 7;
            //        response.Message = ResponseMessageKyc.Doc_Rejected;
            //    }
            //    else if (results.DocumetStatus == (int)DocumentStatus.NotOk)
            //    {
            //        response.RstKey = 8;
            //        response.Message = ResponseMessageKyc.Doc_Not_visible;
            //    }
            //    else
            //    {
            //        response.RstKey = 9;
            //        response.Message = ResponseMessageKyc.FAILED_Doc_NotUploaded;
            //    }
            //    return response;
            //}

            try
            {
                if (sender.IsEmailVerified == true)
                {
                    if (sender != null)
                    {
                        var adminKeyPair = AES256.AdminKeyPair;

                        if (Isdocverified == true)
                        {
                            if (data != null)
                            {
                                #region Prepare the Model for Request

                                var istdate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                                var d = istdate.ToString("ddMMyyyyHHmmss");

                                FlightAndAfroRequest req = new FlightAndAfroRequest();

                                var invoiceNumber = await _masterDataRepository.GetInvoiceNumber();
                                req.agentcode = Convert.ToString(sender.WalletUserId);
                                req.tokenID = invoiceNumber.InvoiceNumber;
                                req.tgt = d;
                                req.saltkey = "hsyd2KDJ29";//MerchandIs=MOSEzee7hdAs    
                                var checkSumm = new CommonMethods().Sha512(req);
                                FlightHotelData flightHotelData = new FlightHotelData();
                                flightHotelData.AgentCode = sender.WalletUserId;
                                flightHotelData.Tgt = req.tgt;
                                flightHotelData.TokenId = invoiceNumber.InvoiceNumber;
                                flightHotelData.CheckSum = checkSumm;
                                flightHotelData.CreatedDate = DateTime.UtcNow;
                                flightHotelData.UpdatedDate = DateTime.UtcNow;
                                flightHotelData.IsActive = true;
                                flightHotelData.IsDeleted = false;
                                int result = await _thridPartyApiRepository.FlightHotelBooking(flightHotelData);


                                #endregion


                                if (result > 0)
                                {
                                    apiUrl = ""; //ThirdPartyAggragatorSettings.FligthHotel + "agentcode=" + req.agentcode + "&" + "tokenID=" + req.tokenID + "&" + "tgt=" + req.tgt + "&" + "Checksum=" + checkSumm;
                                    response.responseString = apiUrl;
                                    response.RstKey = 1;
                                    response.Message = AggregatoryMESSAGE.SUCCESSFUL;
                                }
                                else
                                {
                                    response.RstKey = 11;
                                    response.Message = ResponseMessages.REQUESTDATA_NOT_EXIST;
                                }
                            }
                            else
                            {
                                response.RstKey = 2;
                                response.Message = ResponseMessages.REQUESTDATA_NOT_EXIST;
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
                        response.RstKey = 3;
                        response.Message = ResponseMessages.USER_NOT_REGISTERED;
                    }

                }
                else
                {
                    response.RstKey = 5;
                    response.Message = "Please verify your email id.";
                }
            }

            catch (Exception ex)
            {
                ex.Message.ErrorLog("PayServiesRepository", "FlightHotelBooking", ex);
            }
            return response;
        }

        public async Task<object> DataVerification(VerifyRequest xmlRquest)
        {
            var xml = "";
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {

                if (xmlRquest != null)
                {
                    FlightHotelData re = new FlightHotelData();
                    re.AgentCode = Convert.ToInt32(xmlRquest.agentcode);
                    re.CheckSum = xmlRquest.checksum;
                    re.Tgt = xmlRquest.merchantcode;
                    re.TokenId = xmlRquest.tokenid;
                    re.IsDeleted = true;
                    await _thridPartyApiRepository.FlightHotelBooking(re);
                    //db.FlightHotelDatas.Add(re);
                    //db.SaveChanges();
                }

                var invoiceNumber = await _masterDataRepository.GetInvoiceNumber();
                GetFlightBookingRequest req = new GetFlightBookingRequest();
                req.agentcode = Convert.ToString(xmlRquest.agentcode);
                //req.tokenID = invoiceNumber.InvoiceNumber;
                req.tokenID = xmlRquest.tokenid;
                req.merchantcode = xmlRquest.merchantcode;
                req.saltkey = "hsyd2KDJ29";//MerchandIs=MOSEzee7hdAs    

                //MOSEzee7hdAs
                //SALT:hsyd2KDJ29

                var checkSumm = new CommonMethods().Sha512Ha(req);
                if (checkSumm != null)
                {
                    FlightHotelData res = new FlightHotelData();
                    res.AgentCode = Convert.ToInt32(xmlRquest.agentcode);
                    res.CheckSum = checkSumm;
                    res.Tgt = xmlRquest.merchantcode;
                    res.TokenId = xmlRquest.tokenid;
                    res.IsDeleted = false;
                    res.IsActive = false;
                    //save detail
                    await _thridPartyApiRepository.FlightHotelBooking(res);
                    //db.FlightHotelDatas.Add(res);
                    //db.SaveChanges();
                }
                var subReq = new VerifyResponse();
                XmlSerializer xsSubmit = new XmlSerializer(typeof(VerifyResponse));
                long userId = Convert.ToInt32(xmlRquest.agentcode);
                var data = await _thridPartyApiRepository.GetUserDetailById(userId, xmlRquest.tokenid);//db.FlightHotelDatas.Where(x => x.AgentCode == userId && x.TokenId == xmlRquest.tokenid).FirstOrDefault();
                //if (checkSumm == xmlRquest.checksum)
                if (xmlRquest.checksum != null)
                {
                    var userprofile = await _walletUserRepository.GetUserDetailById(userId);// db.WalletUsers.Where(x => x.WalletUserId == userId);
                                                                                            // var userprofile = new AppUserRepository().GetUserDetailById(userId);

                    if (data != null)
                    {
                        var requset = new FinalCheckSum
                        {
                            agentCode = userId.ToString(),
                            statusMessage = "Success",
                            merchantCode = xmlRquest.merchantcode,
                            tokenId = xmlRquest.tokenid,
                            statusCode = 0.ToString(),
                            saltKey = req.saltkey
                        };

                        var finalCheckSumm = new CommonMethods().Sha512Final(requset);
                        subReq.statusCode = 0;
                        subReq.statusMessage = "Success";
                        subReq.checksum = finalCheckSumm;
                        subReq.merchantcode = xmlRquest.merchantcode;
                        subReq.tokenID = xmlRquest.tokenid;
                        subReq.kcode = userId.ToString();
                        subReq.firstname = userprofile.FirstName;
                        subReq.lastname = userprofile.LastName;
                        subReq.emailid = userprofile.EmailId;
                        subReq.contactphone = userprofile.MobileNo;
                        subReq.city = "GHANA";
                        subReq.mobile = userprofile.MobileNo;
                        subReq.state = "GHANA";
                        subReq.title = "Mr";
                        subReq.zip = "00233";
                        subReq.address = "Plot 3, dade link off of dade street, Cantonments, Accra-Ghana";
                        subReq.companyname = "Ezipaygh";
                    }
                    else
                    {
                        subReq.statusCode = 1;
                        subReq.statusMessage = "Data Not Found.";
                    }
                }
                else if (xmlRquest.agentcode == null || xmlRquest.checksum == null || xmlRquest.merchantcode == null || xmlRquest.tokenid == null)
                {
                    subReq.statusCode = 1;
                    subReq.statusMessage = "Invalid XML.";
                }
                else if (checkSumm != xmlRquest.checksum)
                {
                    subReq.statusCode = 1;
                    subReq.statusMessage = "Checksum Mismatch.";
                }
                else if (userId != Convert.ToInt32(xmlRquest.agentcode) && data.TokenId != xmlRquest.tokenid)
                {
                    subReq.statusCode = 1;
                    subReq.statusMessage = "Verification failed.";
                }
                else
                {
                    subReq.statusCode = 1;
                    subReq.statusMessage = "Unexpected Error.";
                }
                using (var sww = new StringWriter())
                {
                    using (XmlWriter writer = XmlWriter.Create(sww))
                    {
                        xsSubmit.Serialize(writer, subReq);
                        xml = sww.ToString(); // Your XML
                    }
                    string sSyncData = xml;
                    response.Content = new StringContent(sSyncData);
                    return response;
                }
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("PayServiesRepository", "DataVerification", ex);
            }

            return response;
        }

        public async Task<string> GetFee(PayMoneyAggregatoryRequest Request)
        {
            string responseString = "";
            var _mobileMoneyRequest = new GetFeeRequest
            {
                Customer = Request.customer,
                amount = Request.Amount
            };
            var apiUrl = ThirdPartyAggragatorSettings.GetFeePayMoney;
            var responseData = await HttpGetUrlEncodedServiceForMobileMoney(LogTransactionNameTypes.AddMoney, apiUrl, _mobileMoneyRequest, Request, Request.channel);

            responseString = responseData.ToString();
            var res = JsonConvert.DeserializeObject<object>(responseString);
            if (res != null)
            {
                responseString = res.ToString();
            }
            return responseString;
        }

        async Task<string> HttpGetUrlEncodedServiceForMobileMoney(string Log, string Url, object parameters, object Request, string CategoryName)
        {
            string detail = string.Empty;
            string responseString = string.Empty;

            string requestQueryString = string.Empty;
            try
            {
                //using (HttpClient httpClient = new HttpClient())
                //{
                //    requestQueryString = GetQueryString(parameters);
                //    requestQueryString = requestQueryString.Replace("%2c", ",");
                //    detail = Url + "?" + requestQueryString;
                //    LogTransactionTypes.Request.SaveTransactionLog(Log + CategoryName, Request, detail);
                //    HttpResponseMessage response = await httpClient.GetAsync(detail);
                //    if (response.IsSuccessStatusCode)
                //    {
                //        var result = await response.Content.ReadAsStringAsync();
                //        responseString = result.ToString();
                //    }                   
                //}
                requestQueryString = GetQueryString(parameters);
                requestQueryString = requestQueryString.Replace("%2c", ",");
                detail = Url + "?" + requestQueryString;
                LogTransactionTypes.Request.SaveTransactionLog(Log + CategoryName, Request, detail);
                using (WebClient wc = new WebClient())
                {
                    wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                    string HtmlResult = wc.UploadString(Url, requestQueryString);
                }
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog(Log + CategoryName, Log + CategoryName, Log + CategoryName);
                LogTransactionTypes.Response.SaveTransactionLog(Log + CategoryName, Request, detail + ", Exception Occured : " + ex.Message);
                responseString = "{\"StatusCode\":\"506\",\"Message\":\"FAILED\",\"TransactionId\":\"\",\"InvoiceNo\":\"\"}";
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

        public async Task<TransactionStatusResponse> GetTransactionStatus()
        {
            var result = new TransactionStatusResponse();
            var walletPendingTransactions = new List<WalletTransaction>();

            walletPendingTransactions = await _thridPartyApiRepository.GetPendingTransactions();
            try
            {

                string url = "http://54.186.218.22/aggregator/api/status?apikey=8F6064D4-7149-4AC0-911A-4ED5E5C8165C&transactionid=";
                foreach (var item in walletPendingTransactions)
                {
                    url = url + item.TransactionId;
                    var res = await GetIntouchTransactionStatus(url);

                    if (!string.IsNullOrWhiteSpace(res))
                    {
                        result = JsonConvert.DeserializeObject<TransactionStatusResponse>(res);
                        if (result.StatusCode == AggregatorySTATUSCODES.SUCCESSFUL)
                        {
                            item.TransactionStatus = (int)TransactionStatus.Completed;
                            await _thridPartyApiRepository.UpdateStatusOfPendingTransactions(item);
                        }
                        if (result.StatusCode == AggregatorySTATUSCODES.FAILED)
                        {
                            item.TransactionStatus = (int)TransactionStatus.Failed;
                            await _thridPartyApiRepository.UpdateStatusOfPendingTransactions(item);
                        }
                        if (result.StatusCode == AggregatorySTATUSCODES.PENDING)
                        {
                            item.TransactionStatus = (int)TransactionStatus.Pending;
                            await _thridPartyApiRepository.UpdateStatusOfPendingTransactions(item);
                        }
                    }

                    System.Threading.Thread.Sleep(5000);
                }
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("Update Transaction status by crone job", ex.Message);
            }
            return result;
        }
        public async Task<string> GetIntouchTransactionStatus(string url)
        {
            string resString = "";
            using (HttpClient client = new HttpClient())
            {
                // Call asynchronous network methods in a try/catch block to handle exceptions
                try
                {
                    // var content = new StringContent(req, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    resString = await response.Content.ReadAsStringAsync();
                }
                catch (HttpRequestException e)
                {

                }
                return resString;
            }
        }
    }
}
