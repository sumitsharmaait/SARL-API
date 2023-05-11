using ezeePay.Utility.CommonClass;
using ezeePay.Utility.Enums;
using Ezipay.Database;
using Ezipay.Repository.CommisionRepo;
using Ezipay.Repository.MasterData;
using Ezipay.Repository.PayMoneyRepo;
using Ezipay.Repository.UserRepo;
using Ezipay.Service.CommisionService;
using Ezipay.Service.UserService;
using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.Utility.SendEmail;
using Ezipay.Utility.SendPush;
using Ezipay.ViewModel.AirtimeViewModel;
using Ezipay.ViewModel.PayMoneyViewModel;
using Ezipay.ViewModel.SendEmailViewModel;
using Ezipay.ViewModel.SendPushViewModel;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Ezipay.Service.PayMoney
{
    public class PayMoneyService : IPayMoneyService
    {
        private IWalletUserService _walletUserService;
        private IWalletUserRepository _walletUserRepository;
        private IPayMoneyRepository _payMoneyRepository;
        private IMasterDataRepository _masterDataRepository;
        private ISetCommisionRepository _setCommisionRepository;
        private ISendPushNotification _sendPushNotification;
        private ISendEmails _sendEmails;
        private ISetCommisionService _setCommisionService;
        public PayMoneyService()
        {
            _walletUserService = new WalletUserService();
            _walletUserRepository = new WalletUserRepository();
            _payMoneyRepository = new PayMoneyRepository();
            _masterDataRepository = new MasterDataRepository();
            _setCommisionRepository = new SetCommisionRepository();
            _sendPushNotification = new SendPushNotification();
            _sendEmails = new SendEmails();
            _setCommisionService = new SetCommisionService();
        }
        public async Task<WalletTransactionResponse> PayMoney(WalletTransactionRequest request, string sessionToken)
        {
            var response = new WalletTransactionResponse();
            var commission = new CommissionCalculationResponse();
            var merchantCommission = new CommissionCalculationResponse();
            var transationInitiate = new TransactionInitiateRequest();
            var invoiceNumber = await _masterDataRepository.GetInvoiceNumber();
            string ReceiverMobileNo = string.Empty;
            var tranDate = DateTime.UtcNow;
            try
            {
                var adminKeyPair = AES256.AdminKeyPair;
                string FromMobileNo = request.MobileNo;
                string ServiceName = string.Empty;
                int WalletServiceId = 0;
                int MerchantCommissionServiceId = 0;
                int _TypeInfo = 0;
                int _PushType = 0;

                #region Some other issue related to descryption
                if (!string.IsNullOrEmpty(AES256.Encrypt(adminKeyPair.PublicKey, request.MobileNo)))
                {
                    ReceiverMobileNo = request.MobileNo;
                    request.MobileNo = request.IsdCode + AES256.Encrypt(adminKeyPair.PublicKey, request.MobileNo);
                    var data = await _walletUserService.UserProfile(sessionToken);
                    var sender = await _walletUserRepository.GetCurrentUser(data.WalletUserId);


                    //new change                   
                    var transactionLimit = await _payMoneyRepository.GetTransactionLimitForPayment(data.WalletUserId);
                    int limit = transactionLimit != null ? Convert.ToInt32(transactionLimit.transactionlimit) : 0;
                    var transactionHistory = await _payMoneyRepository.GetAllTransactionByDate(data.WalletUserId);
                    int totalAmountTransfered = transactionHistory != null ? Convert.ToInt32(transactionHistory.totalAmount) : 0;
                    //var Isdocverified = await _walletUserRepository.IsDocVerified(data.WalletUserId, data.DocumetStatus);
                    var Isdocverified = await _walletUserRepository.IsDocVerifiedMOMO(data.DocumetStatus);
                    response.DocStatus = Isdocverified;
                    response.DocumetStatus = (int)sender.DocumetStatus;
                    response.CurrentBalance = sender.CurrentBalance;
                    #region Sender Existance

                    if (sender.IsEmailVerified == true)
                    {
                        if (sender.IsDisabledTransaction == false)
                        {
                            if (Isdocverified == true)
                            {
                                //if (transactionLimit == null || limit >= (Convert.ToDecimal(request.Amount) + totalAmountTransfered))
                                //{
                                if (sender != null && sender.WalletUserId > 0)
                                {
                                    var receiver = await _walletUserRepository.GetUserDetailByMobile(request.MobileNo);

                                    #region Receiver Existance
                                    if (receiver != null && receiver.WalletUserId > 0)
                                    {
                                        #region Check Is the users are same
                                        //if (receiver.UserType == 1) //user not to send merchant onli to user
                                        //{ 
                                            if (receiver.WalletUserId != sender.WalletUserId)
                                            {
                                                if (receiver.DocumetStatus == 2) //user or Merchant document check verified or not
                                                {

                                                    WalletServiceId = await _payMoneyRepository.GetServiceId();
                                                    MerchantCommissionServiceId = await _payMoneyRepository.GetMerchantId();
                                                    if (await _payMoneyRepository.IsMerchant(receiver.WalletUserId))
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

                                                    commission.AmountWithCommission = request.Amount;
                                                    commission.AfterDeduction = request.Amount;
                                                    var commissionDetail = await _masterDataRepository.GetCommisionByServiceId(WalletServiceId);
                                                    if (commissionDetail != null && commissionDetail.CommisionPercent > 0)
                                                    {
                                                        commission = await _setCommisionService.CalculateCommission((decimal)commissionDetail.CommisionPercent, (int)commissionDetail.CommisionMasterId, request.Amount, (decimal)commissionDetail.FlatCharges, (decimal)commissionDetail.BenchmarkCharges);
                                                    }
                                                    #endregion
                                                    #region If Receiver is Merchant                                               
                                                    if (await _payMoneyRepository.IsService(MerchantCommissionServiceId, receiver.WalletUserId))
                                                    {
                                                        var merchantCommissionDetail = await _payMoneyRepository.MerchantCommisionMasters(MerchantCommissionServiceId);
                                                        if (merchantCommissionDetail != null && merchantCommissionDetail.CommisionPercent > 0)
                                                        {
                                                            merchantCommission = await _setCommisionService.CalculateCommission((decimal)merchantCommissionDetail.CommisionPercent, (int)merchantCommissionDetail.CommisionMasterId, request.Amount, (decimal)commissionDetail.FlatCharges, (decimal)commissionDetail.BenchmarkCharges);
                                                        }
                                                    }
                                                    #endregion
                                                    #endregion
                                                    //recheck balance for new txn
                                                    if (!sender.CurrentBalance.IsZero() && Convert.ToDecimal(sender.CurrentBalance) >= (Convert.ToDecimal(request.Amount) + Convert.ToDecimal(commission.CommissionAmount)))
                                                    {
                                                        #region transaction initiate request 
                                                        string transactionInitiate = JsonConvert.SerializeObject(request);
                                                        //This is for transaction initiate request all---
                                                        transationInitiate.InvoiceNumber = invoiceNumber.InvoiceNumber;
                                                        transationInitiate.ReceiverNumber = ReceiverMobileNo;
                                                        transationInitiate.ServiceName = ServiceName;
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
                                                        if (!sender.CurrentBalance.IsZero() && Convert.ToDecimal(sender.CurrentBalance) >= (Convert.ToDecimal(request.Amount) + Convert.ToDecimal(commission.CommissionAmount)))
                                                        {
                                                            #region Save Transaction
                                                            var tran = new WalletTransaction();
                                                            tran.Comments = request.Comment;
                                                            tran.InvoiceNo = invoiceNumber.InvoiceNumber;
                                                            tran.BeneficiaryName = request.BeneficiaryName;
                                                            tran.TotalAmount = request.Amount;
                                                            tran.TransactionType = AggragatorServiceType.DEBIT;
                                                            tran.IsBankTransaction = false;
                                                            tran.BankBranchCode = string.Empty;
                                                            tran.BankTransactionId = string.Empty;
                                                            tran.CommisionId = commission.CommissionServiceId;
                                                            tran.WalletAmount = commission.AfterDeduction;
                                                            tran.ServiceTaxRate = 0;
                                                            tran.ServiceTax = "0";
                                                            tran.WalletServiceId = (int)WalletServiceId;
                                                            tran.SenderId = sender.WalletUserId;
                                                            tran.ReceiverId = receiver.WalletUserId;
                                                            tran.AccountNo = ReceiverMobileNo;
                                                            tran.TransactionId = "0";
                                                            tran.IsAdminTransaction = false;
                                                            tran.IsActive = true;
                                                            tran.IsDeleted = false;
                                                            tran.CreatedDate = tranDate;
                                                            tran.UpdatedDate = tranDate;
                                                            tran.TransactionTypeInfo = _TypeInfo;
                                                            tran.TransactionStatus = (int)TransactionStatus.Completed;
                                                            tran.MerchantCommissionAmount = !string.IsNullOrEmpty(merchantCommission.CommissionAmount) ? merchantCommission.CommissionAmount : "0";
                                                            tran.CommisionAmount = !string.IsNullOrEmpty(commission.CommissionAmount) ? commission.CommissionAmount : "0";
                                                            tran.VoucherCode = string.Empty;
                                                            tran.MerchantCommissionId = merchantCommission.CommissionServiceId;
                                                            tran.UpdatedOn = DateTime.Now;
                                                            tran.BenchmarkCharges = commissionDetail.BenchmarkCharges.ToString();
                                                            tran.FlatCharges = commissionDetail.FlatCharges.ToString();
                                                            tran.CommisionPercent = commissionDetail.CommisionPercent.ToString();
                                                            tran.StoreId = request.StoreId;
                                                            tran.TransactionInitiateRequestId = transationInitiate.Id;
                                                            tran = await _payMoneyRepository.SaveWalletTransaction(tran);
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
                                                            debit.Amount = request.Amount;
                                                            debit.TransactionType = (int)WalletTransactionDetailTypes.CREDIT;
                                                            debit.WalletUserId = receiver.WalletUserId;
                                                            debit.WalletTransactionId = tran.WalletTransactionId;
                                                            debit.IsActive = true;
                                                            debit.IsDeleted = false;
                                                            debit.CreatedDate = tranDate;
                                                            //db.WalletTransactionDetails.Add(credit);
                                                            #endregion
                                                            //db.SaveChanges();
                                                            debit = await _payMoneyRepository.SaveWalletTransactionDetail(debit);
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
                                                                _CommisionHistory = await _payMoneyRepository.SaveCommisionHistory(_CommisionHistory);
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
                                                                _CommisionHistory = await _payMoneyRepository.SaveCommisionHistory(_CommisionHistory);
                                                            }
                                                            #endregion
                                                            string ReceiverBalance = "0";

                                                            #region Update Sender Balance
                                                            sender.CurrentBalance = Convert.ToString(Math.Round((Convert.ToDecimal(sender.CurrentBalance) - (Convert.ToDecimal(request.Amount) + Convert.ToDecimal(commission.CommissionAmount))), 2));
                                                            #endregion

                                                            #region Update receiver Balance
                                                            if (!receiver.CurrentBalance.IsZero())
                                                            {
                                                                ReceiverBalance = Convert.ToString(Math.Round((Convert.ToDecimal(receiver.CurrentBalance) + Convert.ToDecimal(request.Amount)) - Convert.ToDecimal(merchantCommission.CommissionAmount), 2));
                                                            }
                                                            else
                                                            {
                                                                ReceiverBalance = Convert.ToString(Math.Round(Convert.ToDecimal(request.Amount) - Convert.ToDecimal(merchantCommission.CommissionAmount), 2));
                                                            }
                                                            receiver.CurrentBalance = ReceiverBalance;
                                                            #endregion

                                                            response.StatusCode = (int)WalletTransactionStatus.SUCCESS;
                                                            response.TransactionDate = tranDate;
                                                            response.TransactionAmount = request.Amount;
                                                            response.TransactionId = tran.WalletTransactionId;
                                                            response.Message = "Pay Money successfully.";
                                                            response.Amount = request.Amount;
                                                            response.SenderBalance = receiver.CurrentBalance;
                                                            response.ToMobileNo = receiver.StdCode + AES256.Decrypt(adminKeyPair.PrivateKey, receiver.MobileNo);
                                                            response.TransactionAmount = request.Amount;
                                                            response.CurrentBalance = sender.CurrentBalance;
                                                            response.RstKey = 1;
                                                            #region PushNotification

                                                            if (!string.IsNullOrEmpty(receiver.CurrentBalance) && response.StatusCode == (int)WalletTransactionStatus.SUCCESS)
                                                            {
                                                                var pushModel = new PayMoneyPushModel();
                                                                pushModel.TransactionDate = response.TransactionDate;
                                                                pushModel.TransactionId = response.TransactionId.ToString();
                                                                if (merchantCommission.Rate > 0)
                                                                {
                                                                    pushModel.alert = Math.Round(Convert.ToDecimal(request.Amount) - Convert.ToDecimal(merchantCommission.CommissionAmount), 2).ToString() + " XOF has been credited to your account by " + data.FirstName + " " + data.LastName;
                                                                }
                                                                else
                                                                {
                                                                    pushModel.alert = request.Amount + " XOF has been credited to your account by " + data.FirstName + " " + data.LastName;
                                                                }
                                                                pushModel.Amount = response.Amount;
                                                                pushModel.CurrentBalance = receiver.CurrentBalance;

                                                                pushModel.MobileNo = request.IsdCode + FromMobileNo;
                                                                pushModel.SenderName = sender.FirstName + " " + sender.LastName;
                                                                pushModel.pushType = _PushType;

                                                                var push = new PushNotificationModel();
                                                                push.deviceType = (int)receiver.DeviceType;
                                                                push.deviceKey = receiver.DeviceToken;
                                                                if ((int)receiver.DeviceType == (int)DeviceTypes.ANDROID || (int)receiver.DeviceType == (int)DeviceTypes.Web)
                                                                {
                                                                    var aps = new PushPayload<PayMoneyPushModel>();
                                                                    var _data = new PushPayloadData<PayMoneyPushModel>();
                                                                    _data.notification = pushModel;
                                                                    aps.data = _data;
                                                                    aps.to = receiver.DeviceToken;
                                                                    aps.collapse_key = string.Empty;
                                                                    push.message = JsonConvert.SerializeObject(aps);
                                                                    push.payload = pushModel;
                                                                }
                                                                if ((int)receiver.DeviceType == (int)DeviceTypes.IOS)
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

                                                                    //  res.userPushDetail = push;
                                                                }
                                                                try
                                                                {//--------sending mail on success transaction--------
                                                                    var receiverdetail = await _walletUserRepository.GetUserDetailById(receiver.WalletUserId);
                                                                    var senderdetail = await _walletUserRepository.GetUserDetailById(sender.WalletUserId);
                                                                    string filename = CommonSetting.successfullTransaction;
                                                                    var body = _sendEmails.ReadEmailformats(filename);
                                                                    body = body.Replace("$$FirstName$$", senderdetail.FirstName + " " + senderdetail.LastName);
                                                                    body = body.Replace("$$DisplayContent$$", ServiceName);
                                                                    body = body.Replace("$$customer$$", receiverdetail.MobileNo);
                                                                    body = body.Replace("$$amount$$", "XOF " + request.Amount);
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
                                                            //Update sender or receiver wallet with current balance
                                                            transationInitiate = await _payMoneyRepository.GetTransactionInitiateRequest(transationInitiate.Id);
                                                            transationInitiate.AfterTransactionBalance = sender.CurrentBalance;
                                                            transationInitiate.ReceiverCurrentBalance = receiver.CurrentBalance;
                                                            transationInitiate.ReceiverWalletUserId = receiver.WalletUserId;
                                                            await _payMoneyRepository.UpdateTransactionInitiateRequest(transationInitiate);
                                                            //  db.SaveChanges();
                                                            await _payMoneyRepository.UpdateWalletUser(receiver);
                                                            await _payMoneyRepository.UpdateWalletUser(sender);
                                                            #endregion
                                                        }
                                                        else
                                                        {
                                                            response.RstKey = 6;
                                                            response.StatusCode = (int)TransactionStatus.Rejected;
                                                            response.Message = "Insuficient Balance.";
                                                        }
                                                    }
                                                    else
                                                    {
                                                        response.RstKey = 6;
                                                        response.StatusCode = (int)TransactionStatus.Rejected;
                                                        response.Message = "Insuficient Balance.";
                                                    }
                                                }
                                                else
                                                {
                                                    response.RstKey = 6;
                                                    response.StatusCode = (int)WalletTransactionStatus.FAILED;
                                                    response.Message = "Receiver Document verification is pending";
                                                }
                                            }
                                            else
                                            {
                                                response.RstKey = 9;
                                                response.StatusCode = (int)WalletTransactionStatus.SELF_WALLET;
                                                response.Message = ResponseMessages.SELF_WALLET;
                                            }
                                           


                                        //}
                                        //else
                                        //{
                                        //    response.RstKey = 6;
                                        //    response.StatusCode = (int)WalletTransactionStatus.FAILED;
                                        //    response.Message = "This service is currently disable.";
                                        //}






                                        #endregion

                                    }
                                    else
                                    {
                                        response.RstKey = 10;
                                        response.Message = ResponseMessages.RECEIVER_NOT_EXIST;
                                        response.StatusCode = (int)WalletTransactionStatus.RECEIVER_NOT_EXIST;
                                    }
                                    #endregion



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
                    response.RstKey = 6;
                    response.StatusCode = (int)WalletTransactionStatus.FAILED;
                    response.Message = "This service is currently disable.";
                }


                #endregion
                #endregion
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("WalletTransactionRepository.cs", "PayMoney", invoiceNumber + " " + ex.StackTrace + " " + ex.Message);
                response.Message = ex.Message;
                response.RstKey = 6;
            }
            return response;
        }
    }
}
