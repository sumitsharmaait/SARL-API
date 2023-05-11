using ezeePay.Utility.CommonClass;
using ezeePay.Utility.Enums;
using Ezipay.Database;
using Ezipay.Repository.AdminRepo.ChargeBack;
using Ezipay.Repository.CardPayment;
using Ezipay.Repository.CommisionRepo;
using Ezipay.Repository.CommonRepo;
using Ezipay.Repository.MasterData;
using Ezipay.Repository.MobileMoneyRepo;
using Ezipay.Repository.PayMoneyRepo;
using Ezipay.Repository.UserRepo;
using Ezipay.Service.Admin.TransactionLimitAU;
using Ezipay.Service.CommonService;
using Ezipay.Service.UserService;
using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.Utility.LogHandler;
using Ezipay.Utility.SendEmail;
using Ezipay.Utility.SendPush;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.AirtimeViewModel;
using Ezipay.ViewModel.CardPaymentViewModel;
using Ezipay.ViewModel.CommisionViewModel;
using Ezipay.ViewModel.SendEmailViewModel;
using Ezipay.ViewModel.SendPushViewModel;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

namespace Ezipay.Service.MobileMoneyService
{
    public class MobileMoneyServices : IMobileMoneyServices
    {
        private IMobileMoneyRepository _mobileMoneyRepository;
        private IWalletUserRepository _walletUserRepository;
        private ISetCommisionRepository _setCommisionRepository;
        private IMasterDataRepository _masterDataRepository;
        private ISendPushNotification _sendPushNotification;
        private IPayMoneyRepository _payMoneyRepository;
        private ISendEmails _sendEmails;
        private ICommonRepository _commonRepository;
        private ICommonServices _commonServices;
        private ITransactionLimitAUService _transactionLimitAUService;
        private IWalletUserService _walletUserService;
        private IChargeBackRepository _ChargeBackRepository;
        private ILogUtils _logUtils;
        private ICardPaymentRepository _cardPaymentRepository;
        public MobileMoneyServices()
        {
            _walletUserRepository = new WalletUserRepository();
            _setCommisionRepository = new SetCommisionRepository();
            _masterDataRepository = new MasterDataRepository();
            _sendEmails = new SendEmails();
            _sendPushNotification = new SendPushNotification();
            _mobileMoneyRepository = new MobileMoneyRepository();
            _payMoneyRepository = new PayMoneyRepository();
            _commonRepository = new CommonRepository();
            _commonServices = new CommonServices();
            _transactionLimitAUService = new TransactionLimitAUService();
            _walletUserService = new WalletUserService();
            _ChargeBackRepository = new ChargeBackRepository();
            _logUtils = new LogUtils();
            _cardPaymentRepository = new CardPaymentRepository();
        }

        public async Task<AddMoneyAggregatorResponse> MobileMoneyService(PayMoneyAggregatoryRequest request, long WalletUserId = 0)
        {

            var response = new AddMoneyAggregatorResponse();

            var _commissionRequest = new CalculateCommissionRequest();
            var _commission = new CalculateCommissionResponse();
            var transationInitiate = new TransactionInitiateRequest();

            var senderObj = new MobileMoneySenderDetail1();
            var recipientObj = new MobileMoneyReceiverDetail1();
            string responseString = "";
            // var token = GlobalData.Key;

            //var userChargeBackresult = await _ChargeBackRepository.GetfreezeById(request.WalletUserId);
            try
            {
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
                string apiUrl = string.Empty;

                var data = await _walletUserRepository.GetCurrentUser(request.WalletUserId);
                var WalletService = await _masterDataRepository.GetWalletServicesByIdOrChannel(request.channel, request.ServiceCategoryId, request.IsdCode);
                var subcategory = await _masterDataRepository.GetWalletSubCategoriesById(request.ServiceCategoryId);
                //bool IsdocVerified = await _walletUserRepository.IsDocVerified(sender.WalletUserId, (int)sender.DocumetStatus);
                bool IsdocVerified = await _walletUserRepository.IsDocVerifiedMOMO((int)sender.DocumetStatus);
                var transactionLimit = await _payMoneyRepository.GetTransactionLimitForPayment(data.WalletUserId);
                var totalTransactionCount = await _payMoneyRepository.GetTotalTransactionCount(data.WalletUserId);
                long count = totalTransactionCount != null ? totalTransactionCount.TotalTransactions : 0;
                int limit = transactionLimit != null ? Convert.ToInt32(transactionLimit.transactionlimit) : 0;
                var transactionHistory = await _payMoneyRepository.GetAllTransactionByDate(data.WalletUserId);
                int totalAmountTransfered = transactionHistory != null ? Convert.ToInt32(transactionHistory.totalAmount) : 0;
                string customer = request.customer;//.Length.ToString();
                var _tranDate = DateTime.UtcNow;
                //
                var resultTL = await _transactionLimitAUService.CheckTransactionLimitAU(request.WalletUserId.ToString()); //check New TL

                if (request.ISD == "+245" || request.ISD == "+227" || request.ISD == "+229" || request.ISD == "+228" || request.ISD == "+242" || request.IsdCode == "+235" || request.IsdCode == "+241" || request.IsdCode == "+236" || request.IsdCode == "+240" || request.ISD == "+237" || (request.ISD == "+225" && WalletService.WalletServiceId == 142 || request.ServiceCategoryId == 8))
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
                                                //if (sender != null)
                                                //{
                                                if (!string.IsNullOrEmpty(sender.CurrentBalance) && !sender.CurrentBalance.IsZero() && Convert.ToDecimal(sender.CurrentBalance) > 0)
                                                {
                                                    _commissionRequest.CurrentBalance = Convert.ToDecimal(sender.CurrentBalance);
                                                    _commissionRequest.IsRoundOff = true;
                                                    _commissionRequest.TransactionAmount = Convert.ToDecimal(request.Amount);

                                                    _commissionRequest.WalletServiceId = WalletService.WalletServiceId;
                                                    _commission = await _setCommisionRepository.CalculateCommissionForMobileMoney(_commissionRequest, sender.WalletUserId, count, request.IsdCode);
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

                                                    string transactionInitiate = string.Empty;
                                                    #region Prepare the Model for Request
                                                    if (request.ServiceCategoryId == 10)
                                                    {
                                                        #region Prepare the Model for Request


                                                        _mobileMoneyRequest.ServiceType = AggragatorServiceType.CREDIT;
                                                        _mobileMoneyRequest.Channel = request.channel;
                                                        _mobileMoneyRequest.Amount = Convert.ToString(Convert.ToInt32(_commission.TransactionAmount)); //Request.amount;

                                                        //if (invoiceNumber != null && request.IsdCode == "+237")
                                                        //{
                                                        //    _mobileMoneyRequest.InvoiceNo = invoiceNumber.InvoiceNumber;
                                                        //}
                                                        //else if (invoiceNumber != null)
                                                        //{
                                                        _mobileMoneyRequest.TransactionId = invoiceNumber.InvoiceNumber;
                                                        //}

                                                        _mobileMoneyRequest.Customer = customer;
                                                        request.IsdCode = request.IsdCode;

                                                        if (request.IsdCode == "+225")
                                                        {
                                                            _mobileMoneyRequest.Country = "CI";
                                                            _mobileMoneyRequest.Customer = customer;

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

                                                        }
                                                        else if (request.IsdCode == "+223")
                                                        {
                                                            _mobileMoneyRequest.Country = "ML";
                                                            _mobileMoneyRequest.Customer = customer;

                                                        }
                                                        else if (request.IsdCode == "+228")
                                                        {
                                                            _mobileMoneyRequest.Country = "TG";
                                                            _mobileMoneyRequest.Customer = customer;

                                                        }
                                                        else if (request.IsdCode == "+227")
                                                        {
                                                            _mobileMoneyRequest.Country = "NE";
                                                            _mobileMoneyRequest.Customer = customer;

                                                        }
                                                        else if (request.IsdCode == "+229")
                                                        {
                                                            _mobileMoneyRequest.Country = "BJ";
                                                            _mobileMoneyRequest.Customer = customer;

                                                        }
                                                        else if (request.IsdCode == "+245")
                                                        {
                                                            _mobileMoneyRequest.Country = "GW";
                                                            _mobileMoneyRequest.Customer = customer;

                                                        }
                                                        else if (request.IsdCode == "+242") //xaf
                                                        {
                                                            _mobileMoneyRequest.Country = "CG";
                                                            _mobileMoneyRequest.Customer = customer;

                                                        }
                                                        else if (request.IsdCode == "+237")
                                                        {
                                                            _mobileMoneyRequest.Country = "CM";
                                                            _mobileMoneyRequest.Customer = customer;

                                                        }
                                                        else if (request.IsdCode == "+235")
                                                        {
                                                            _mobileMoneyRequest.Country = "TD";
                                                            _mobileMoneyRequest.Customer = customer;

                                                        }
                                                        else if (request.IsdCode == "+241")
                                                        {
                                                            _mobileMoneyRequest.Country = "GA";
                                                            _mobileMoneyRequest.Customer = customer;

                                                        }
                                                        else if (request.IsdCode == "+236")
                                                        {
                                                            _mobileMoneyRequest.Country = "CF";
                                                            _mobileMoneyRequest.Customer = customer;

                                                        }
                                                        else if (request.IsdCode == "+240")
                                                        {
                                                            _mobileMoneyRequest.Country = "GQ";
                                                            _mobileMoneyRequest.Customer = customer;

                                                        }
                                                        //apikey = ezipay

                                                        //if (request.IsdCode == "+237")//camroon
                                                        //{
                                                        //    _mobileMoneyRequest.servicecategory = "MOBILEMONEY";
                                                        //    apiUrl = ThirdPartyAggragatorSettings.AddMobileMoneyCameroon;
                                                        //    var requ = new PayServicesMoneyAggregatoryRequestCamroon
                                                        //    {
                                                        //        ApiKey = ThirdPartyAggragatorSettings.ApiKeyCamroon,
                                                        //        Amount = _commission.TransactionAmount.ToString(),
                                                        //        Customer = _mobileMoneyRequest.Customer,
                                                        //        InvoiceNo = _mobileMoneyRequest.InvoiceNo
                                                        //    };
                                                        //    _mobileMoneyRequest.ApiKey = ThirdPartyAggragatorSettings.ApiKeyCamroon;
                                                        //    _mobileMoneyRequest.Signature = new CommonMethods().Sha256HashCamroon(requ);

                                                        //}
                                                        //else
                                                        //{
                                                        _mobileMoneyRequest.servicecategory = "francophone";
                                                        apiUrl = ThirdPartyAggragatorSettings.AddMobileMoney;
                                                        var requ = new PayServicesMoneyAggregatoryRequest
                                                        {
                                                            ApiKey = ThirdPartyAggragatorSettings.ApiKey,
                                                            Amount = _commission.TransactionAmount.ToString(),
                                                            Customer = _mobileMoneyRequest.Customer,
                                                            TransactionId = _mobileMoneyRequest.TransactionId
                                                        };
                                                        _mobileMoneyRequest.ApiKey = ThirdPartyAggragatorSettings.ApiKey;
                                                        _mobileMoneyRequest.Signature = new CommonMethods().Sha256Hash(requ);

                                                        //}
                                                        ////sender details add to requets api
                                                        if (request.IsdCode == "+237" || request.IsdCode == "+242" || request.IsdCode == "+229" || request.IsdCode == "+245" || request.IsdCode == "+227" || request.IsdCode == "+228" || request.IsdCode == "+242" || request.IsdCode == "+235" || request.IsdCode == "+241" || request.IsdCode == "+236" || request.IsdCode == "+240")
                                                        {
                                                            //to get isdcode country code
                                                            var countryCode = await _masterDataRepository.IsdCodesby(sender.StdCode);

                                                            senderObj.address = request.SenderAddress;
                                                            senderObj.city = request.SenderCity;
                                                            senderObj.dateofBirth = request.SenderDateofbirth;
                                                            senderObj.idNumber = request.SenderIdNumber;
                                                            senderObj.idType = request.SenderIdType;
                                                            senderObj.email = sender.EmailId;
                                                            senderObj.firstName = sender.FirstName;
                                                            senderObj.surname = sender.LastName;
                                                            senderObj.contact = sender.MobileNo;
                                                            senderObj.country = countryCode.CountryCode;

                                                            recipientObj.firstName = request.ReceiverFirstName;
                                                            recipientObj.surname = request.ReceiverLastName;

                                                            //check 
                                                            int senderRequest = await _mobileMoneyRepository.SaveMobileMoneySenderDetailsRequest(request);
                                                            if (senderRequest != 1)
                                                            {
                                                                response.RstKey = 3;
                                                                response.Message = AggregatoryMESSAGE.FAILED;
                                                                return response;
                                                            }

                                                            var SenderDateofbirth1 = Convert.ToDateTime(request.SenderDateofbirth);
                                                            senderObj.dateofBirth = SenderDateofbirth1.ToString("yyyy-MM-dd");
                                                            _mobileMoneyRequest.Sender = senderObj;
                                                            _mobileMoneyRequest.Recipient = recipientObj;
                                                        }
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
                                                    transationInitiate = await _mobileMoneyRepository.SaveTransactionInitiateRequest(transationInitiate);

                                                    if (transationInitiate.Id != 0)
                                                    {
                                                        //calling pay method insert data in Database
                                                        data.CurrentBalance = Convert.ToString(_commission.UpdatedCurrentBalance);
                                                        var walletUser = await _walletUserRepository.UpdateUserDetail(data);

                                                        if (walletUser != null)
                                                        {
                                                            //hit 
                                                            if (WalletService.HttpVerbs.ToLower() == AggragatorServiceVerbs.HttpPostVerb.ToLower())
                                                            {
                                                                var jsonReq = JsonConvert.SerializeObject(_mobileMoneyRequest);
                                                                _logUtils.WriteTextToFileForWTxnTableLogs("MobileMoneyservice :-jsonReq " + jsonReq);
                                                                var responseData = await new CommonApi().PaymentMobileMon(jsonReq, apiUrl);

                                                                responseString = responseData;
                                                            }
                                                            else
                                                            {
                                                                //jsdfklasjdf;
                                                                _logUtils.WriteTextToFileForWTxnTableLogs("MobileMoneyservice :-jsonReq :" + _mobileMoneyRequest);
                                                                var responseData = Task.Run(() => HttpGetUrlEncodedServiceForMobileMoney(LogTransactionNameTypes.AddMoney, apiUrl, _mobileMoneyRequest, request, request.channel));

                                                                responseData.Wait();
                                                                responseString = responseData.Result.ToString();
                                                            }

                                                            var TransactionInitial = await _mobileMoneyRepository.GetTransactionInitiateRequest(transationInitiate.Id);
                                                            TransactionInitial.JsonResponse = "mobile money Response" + responseString;
                                                            TransactionInitial = await _mobileMoneyRepository.UpdateTransactionInitiateRequest(TransactionInitial);
                                                            LogTransactionTypes.Response.SaveTransactionLog(LogTransactionNameTypes.PayMoney + subcategory.CategoryName, responseString, "Aggregator Url : ", sender.WalletUserId);
                                                            var errorResponse = "The remote server returned an error: (500) Internal Server Error.";
                                                            if (!string.IsNullOrEmpty(responseString) && responseString != errorResponse)
                                                            {

                                                                var _responseModel = JsonConvert.DeserializeObject<AddMoneyAggregatorResponse>(responseString);
                                                                _logUtils.WriteTextToFileForWTxnTableLogs("MobileMoneyservice :-lineno. 425 InvoiceNumber :" + invoiceNumber.InvoiceNumber);

                                                                //if (request.IsdCode == "+237")//camroon
                                                                //{
                                                                //    _responseModel.StatusCode = Convert.ToString(_responseModel.statusCode); 
                                                                //    _responseModel.TransactionId = _responseModel.transactionId;
                                                                //}
                                                                if (_responseModel != null && !string.IsNullOrEmpty(_responseModel.StatusCode) && (_responseModel.StatusCode == AggregatorySTATUSCODES.SUCCESSFUL || _responseModel.StatusCode == AggregatorySTATUSCODES.PENDING || _responseModel.StatusCode == AggregatorySTATUSCODES.FAILED || _responseModel.StatusCode == AggregatorySTATUSCODES.EXCEPTION))
                                                                {

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

                                                                        _logUtils.WriteTextToFileForWTxnTableLogs("MobileMoneyservice :-lineno. 511 InvoiceNumber :" + invoiceNumber.InvoiceNumber);

                                                                        var refundAmt = Convert.ToDecimal(data.CurrentBalance) + _commission.AmountWithCommission;
                                                                        data.CurrentBalance = Convert.ToString(refundAmt);
                                                                        await _walletUserRepository.UpdateUserDetail(data);
                                                                        _logUtils.WriteTextToFileForWTxnTableLogs("MobileMoneyservice :-lineno. 516 InvoiceNumber :" + invoiceNumber.InvoiceNumber);
                                                                        var _transactionInitial = await _mobileMoneyRepository.GetTransactionInitiateRequest(transationInitiate.Id);

                                                                        _transactionInitial.AfterTransactionBalance = data.CurrentBalance;
                                                                        _transactionInitial.ReceiverCurrentBalance = data.CurrentBalance;
                                                                        await _mobileMoneyRepository.UpdateTransactionInitiateRequest(_transactionInitial);
                                                                        _logUtils.WriteTextToFileForWTxnTableLogs("MobileMoneyservice :-lineno. 522 InvoiceNumber :" + invoiceNumber.InvoiceNumber);

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
                                                                    if (WalletService.ServiceCategoryId == 10)
                                                                    {
                                                                        tran.OperatorType = "sample";
                                                                    }
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
                                                                        tran = await _mobileMoneyRepository.MobileMoneyService(tran);
                                                                    }
                                                                    catch (Exception ex)
                                                                    {
                                                                        _logUtils.WriteTextToFileForWTxnTableLogs("MobileMoneyservice :-lineno. 616 InvoiceNumber :" + invoiceNumber.InvoiceNumber + " " + ex.StackTrace + " " + ex.Message);
                                                                    }
                                                                    //calling pay method insert data in Database
                                                                    // await _walletUserRepository.UpdateUserDetail(data);

                                                                }

                                                                //303
                                                                else if (_responseModel != null && !string.IsNullOrEmpty(_responseModel.StatusCode) && (_responseModel.StatusCode == AggregatorySTATUSCODES.PENDINGTxn))
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
                                                                    tran.TransactionId = _responseModel.TransactionId;
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
                                                                    tran.OperatorType = "sample";
                                                                    tran.IsdCode = request.IsdCode;
                                                                    // tran = await _mobileMoneyRepository.MobileMoneyService(tran);

                                                                    try
                                                                    {
                                                                        tran = await _mobileMoneyRepository.MobileMoneyService(tran);
                                                                    }
                                                                    catch (Exception ex)
                                                                    {
                                                                        _logUtils.WriteTextToFileForWTxnTableLogs("MobileMoneyservice :-lineno. 616 InvoiceNumber :" + invoiceNumber.InvoiceNumber + " " + ex.StackTrace + " " + ex.Message);
                                                                    }
                                                                    response.RstKey = 6;
                                                                    response.Message = _responseModel.Message;
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
                                                                tran = await _mobileMoneyRepository.MobileMoneyService(tran);
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
                                                            response.RstKey = 5;
                                                            response.Message = ResponseMessages.AGGREGATOR_FAILED_EXCEPTION;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        response.RstKey = 5;
                                                        response.Message = ResponseMessages.AGGREGATOR_FAILED_EXCEPTION;
                                                    }

                                                }
                                                else
                                                {
                                                    response.RstKey = 10;
                                                    response.Message = ResponseMessages.INSUFICIENT_BALANCE;
                                                }

                                                //}
                                                //else
                                                //{
                                                //    response.RstKey = 11;
                                                //    response.Message = ResponseMessages.REQUESTDATA_NOT_EXIST;
                                                //}
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
                                response.Amount = request.Amount;
                                response.TransactionDate = DateTime.UtcNow;
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
            }
            catch (Exception ex)
            {
                "MobileMoneyController".ErrorLog("MobileMoneyController.cs", "MobileMoneyService", response + " " + ex.StackTrace + " " + ex.Message);

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

        public async Task<AdminMobileMoneyLimitResponse> VerifyMobileMoneyLimit(AdminMobileMoneyLimitRequest request)
        {
            return await _mobileMoneyRepository.VerifyMobileMoneyLimit(request);

        }

        public async Task<MobileMoneySenderDetail> VerifySenderIdNumberExistorNot(MobileMoneySenderDetailrequest request)
        {
            return await _mobileMoneyRepository.VerifySenderIdNumberExistorNot(request);

        }


        //transfertobank nigeria

        public async Task<AddMoneyAggregatorResponse> PayBankTransferServiceForNGNbankflutter(ThirdpartyPaymentByCardRequest request, string headerToken)
        {
            var response = new AddMoneyAggregatorResponse();
            var _commissionRequest = new CalculateCommissionRequest();
            var _commission = new CalculateCommissionResponse();
            var transationInitiate = new TransactionInitiateRequest();
            string responseString = "";

            try
            {
                var sender = await _walletUserRepository.GetUserDetailById(request.WalletUserId);

                string apiUrl = string.Empty;

                var data = await _walletUserRepository.GetCurrentUser(request.WalletUserId);
                //var WalletService = await _masterDataRepository.GetWalletServicesByIdOrChannel(request.channel, request.ServiceCategoryId, request.IsdCode);
                var subcategory = await _masterDataRepository.GetWalletSubCategoriesById(41);

                bool IsdocVerified = await _walletUserRepository.IsDocVerifiedMOMO((int)sender.DocumetStatus);
                var transactionLimit = await _payMoneyRepository.GetTransactionLimitForPayment(data.WalletUserId);
                var totalTransactionCount = await _payMoneyRepository.GetTotalTransactionCount(data.WalletUserId);
                long count = totalTransactionCount != null ? totalTransactionCount.TotalTransactions : 0;
                int limit = transactionLimit != null ? Convert.ToInt32(transactionLimit.transactionlimit) : 0;
                var transactionHistory = await _payMoneyRepository.GetAllTransactionByDate(data.WalletUserId);
                int totalAmountTransfered = transactionHistory != null ? Convert.ToInt32(transactionHistory.totalAmount) : 0;

                var _tranDate = DateTime.UtcNow;
                //
                var resultTL = await _transactionLimitAUService.CheckTransactionLimitAU(request.WalletUserId.ToString()); //check New TL


                //------Get Currency Rate--------------
                var currencyDetail = _masterDataRepository.GetCurrencyRate();

                //decimal cediRate = Convert.ToDecimal(currencyDetail.CediRate); 
                decimal SendNGNRate = Convert.ToDecimal(currencyDetail.SendNGNRate);//
                //decimal CfaRate = Convert.ToDecimal(currencyDetail.LERate);
                decimal requestAmount = Convert.ToDecimal(request.Amount);//;


                if (sender.IsOtpVerified == true) //mobile exist or not then 
                {
                    if (sender.IsEmailVerified == true)
                    {
                        if (subcategory != null)
                        {
                            request.serviceCategory = subcategory.CategoryName;
                            var WalletService = await _cardPaymentRepository.GetWalletService("SendBankFlutter", 41);
                            if (WalletService != null)
                            {
                                if (WalletService.IsActive == true)
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
                                                        //CalculatePayNGNTransferSendMoneyCommission
                                                        _commission = await _setCommisionRepository.CalculateCommission(_commissionRequest);
                                                    }
                                                    else
                                                    {
                                                        response.RstKey = 6;
                                                        response.Message = ResponseMessages.INSUFICIENT_BALANCE;
                                                        return response;
                                                    }
                                                    //decimal amountWithCommision = _commission.AmountWithCommission;
                                                    // requestamount:- 1000 xof ,deductAmountWithCommission:- 1025 xof ,user ko mil 906.75 ngn
                                                    decimal amountWithCommision = decimal.Parse(string.Format("{0:0,0}", _commission.AmountWithCommission));    // "1,234,257";

                                                    //decimal amt = (_commission.AmountWithCommission * NGNRate); //xof to NGNRate
                                                    //var finalAmt = Decimal.Parse(amt.ToString("0.00"));


                                                    decimal amt = (_commission.TransactionAmount * SendNGNRate); //xof to NGNRate
                                                    var finalAmt = Decimal.Parse(amt.ToString("0.00"));


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

                                                        string transactionInitiate = string.Empty;
                                                        #region Prepare the Model for Request

                                                        var _masterCard = new MasterCardPaymentRequest();

                                                        _masterCard.SessionId = null;
                                                        _masterCard.Version = null;
                                                        _masterCard.SuccessIndicator = null;
                                                        _masterCard.Merchant = "SendBankFlutter";
                                                        _masterCard.IsActive = true;
                                                        _masterCard.IsDeleted = false;
                                                        _masterCard.CreatedDate = DateTime.UtcNow;
                                                        _masterCard.UpdatedDate = DateTime.UtcNow;
                                                        _masterCard.Amount = request.Amount;
                                                        _masterCard.CommisionCharges = _commission.CommisionPercent;
                                                        _masterCard.TotalAmount = Convert.ToString(amountWithCommision);
                                                        _masterCard.WalletUserId = sender.WalletUserId;
                                                        _masterCard.TransactionNo = invoiceNumber.InvoiceNumber;
                                                        _masterCard.FlatCharges = _commission.FlatCharges;
                                                        _masterCard.BenchmarkCharges = _commission.BenchmarkCharges;
                                                        _masterCard.CommissionAmount = _commission.CommissionAmount.ToString();
                                                        await _cardPaymentRepository.SaveMasterCardPaymentRequest(_masterCard);

                                                        var _RequestAttributes = new flutterSendBankRequest();
                                                        _RequestAttributes.account_bank = request.ngnbank;
                                                        _RequestAttributes.account_number = request.accountNo;
                                                        _RequestAttributes.amount = Convert.ToInt32(finalAmt);
                                                        _RequestAttributes.narration = "SendBankFlutter " + sender.FirstName + ' ' + sender.LastName;

                                                        _RequestAttributes.currency = "NGN";
                                                        _RequestAttributes.reference = invoiceNumber.InvoiceNumber;

                                                        //_RequestAttributes.callback_url = CommonSetting.flutterSendBankCallBackUrl;
                                                        _RequestAttributes.debit_currency = "NGN";

                                                        var req = JsonConvert.SerializeObject(_RequestAttributes);

                                                        transationInitiate.InvoiceNumber = invoiceNumber.InvoiceNumber;
                                                        transationInitiate.ReceiverNumber = request.accountNo;
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
                                                        transationInitiate = await _mobileMoneyRepository.SaveTransactionInitiateRequest(transationInitiate);


                                                        //calling pay method insert data in Database
                                                        data.CurrentBalance = Convert.ToString(_commission.UpdatedCurrentBalance);
                                                        await _walletUserRepository.UpdateUserDetail(data);

                                                        #endregion

                                                        _logUtils.WriteTextToFileForBankFlutterPeyLoadLogs("PayBankTransferServiceForNGNSendbankflutter :- InvoiceNumber " + invoiceNumber.InvoiceNumber + " " + req);

                                                        //here to get psaymenturl
                                                        responseString = await GethashorUrl(req, "SendBankFlutter");

                                                        if (responseString != "")
                                                        {

                                                            var TransactionInitial = await _mobileMoneyRepository.GetTransactionInitiateRequest(transationInitiate.Id);
                                                            TransactionInitial.JsonResponse = "SendBankFlutter Response" + responseString;
                                                            TransactionInitial = await _mobileMoneyRepository.UpdateTransactionInitiateRequest(TransactionInitial);
                                                            LogTransactionTypes.Response.SaveTransactionLog(LogTransactionNameTypes.PayMoney + subcategory.CategoryName, responseString, "Aggregator Url : ", sender.WalletUserId);

                                                            var _responseModel = JsonConvert.DeserializeObject<AddMoneyAggregatorResponse>(responseString);
                                                            // _logUtils.WriteTextToFileForWTxnTableLogs("MobileMoneyservice :-lineno. 425 InvoiceNumber :" + invoiceNumber.InvoiceNumber);
                                                            JavaScriptSerializer js = new JavaScriptSerializer();
                                                            dynamic blogObject = js.Deserialize<dynamic>(responseString);
                                                            var txnreverifystatus = blogObject["data"]["status"];//stagin

                                                            if (_responseModel.status == "success" && (txnreverifystatus == "NEW" || txnreverifystatus == "SUCCESSFUL" || txnreverifystatus == "PENDING" || txnreverifystatus == "FAILED"))
                                                            {

                                                                _responseModel.FormatedTransactionDate = string.Format("{0:d}", DateTime.Now) + "" + string.Format("{0:T}", DateTime.Now);

                                                                _responseModel.AccountNo = request.accountNo;
                                                                _responseModel.MobileNo = request.accountNo;

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
                                                                tran.AccountNo = request.accountNo;// string.Empty;                                                  
                                                                tran.BankTransactionId = string.Empty;
                                                                tran.IsBankTransaction = false;
                                                                tran.BankBranchCode = string.Empty;
                                                                tran.TransactionId = invoiceNumber.InvoiceNumber;
                                                                response.TransactionId = invoiceNumber.InvoiceNumber;
                                                                int _TransactionStatus = 0;
                                                                if (txnreverifystatus == "SUCCESSFUL")
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
                                                                        body = body.Replace("$$customer$$", request.accountNo);
                                                                        //body = body.Replace("$$amount$$", "NGN " + finalAmt);
                                                                        //body = body.Replace("$$ServiceTaxAmount$$", "NGN 0");
                                                                        //body = body.Replace("$$AmountWithCommission$$", "NGN 0");

                                                                        body = body.Replace("$$amount$$", "XOF " + _commission.TransactionAmount);
                                                                        body = body.Replace("$$ServiceTaxAmount$$", "XOF " + _commission.CommissionAmount);
                                                                        body = body.Replace("$$AmountWithCommission$$", "XOF " + _commission.AmountWithCommission);
                                                                        body = body.Replace("$$TransactionId$$", invoiceNumber.InvoiceNumber);

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
                                                                else if (txnreverifystatus == "NEW" || txnreverifystatus == "PENDING")
                                                                {
                                                                    _TransactionStatus = (int)TransactionStatus.Pending;
                                                                }
                                                                else if (txnreverifystatus == "FAILED")
                                                                {

                                                                    // _logUtils.WriteTextToFileForWTxnTableLogs("MobileMoneyservice :-lineno. 511 InvoiceNumber :" + invoiceNumber.InvoiceNumber);

                                                                    var refundAmt = Convert.ToDecimal(data.CurrentBalance) + _commission.AmountWithCommission;
                                                                    data.CurrentBalance = Convert.ToString(refundAmt);
                                                                    await _walletUserRepository.UpdateUserDetail(data);
                                                                    //_logUtils.WriteTextToFileForWTxnTableLogs("MobileMoneyservice :-lineno. 516 InvoiceNumber :" + invoiceNumber.InvoiceNumber);
                                                                    var _transactionInitial = await _mobileMoneyRepository.GetTransactionInitiateRequest(transationInitiate.Id);

                                                                    _transactionInitial.AfterTransactionBalance = data.CurrentBalance;
                                                                    _transactionInitial.ReceiverCurrentBalance = data.CurrentBalance;
                                                                    await _mobileMoneyRepository.UpdateTransactionInitiateRequest(_transactionInitial);
                                                                    // _logUtils.WriteTextToFileForWTxnTableLogs("MobileMoneyservice :-lineno. 522 InvoiceNumber :" + invoiceNumber.InvoiceNumber);

                                                                    _TransactionStatus = (int)TransactionStatus.Failed;
                                                                }
                                                                //else if (_responseModel.StatusCode == AggregatorySTATUSCODES.EXCEPTION)
                                                                //{
                                                                //    _TransactionStatus = (int)TransactionStatus.Failed;
                                                                //}
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

                                                                tran.OperatorType = "SendBankFlutter";

                                                                tran.IsdCode = null;
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
                                                                    if (txnreverifystatus == "NEW" || txnreverifystatus == "PENDING")
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
                                                                    tran = await _mobileMoneyRepository.MobileMoneyService(tran);
                                                                }
                                                                catch (Exception ex)
                                                                {
                                                                    _logUtils.WriteTextToFileForWTxnTableLogs("MobileMoneyservice :-PayBankTransferServiceForNGNbankflutter InvoiceNumber :" + invoiceNumber.InvoiceNumber + " " + ex.StackTrace + " " + ex.Message);
                                                                }
                                                                //calling pay method insert data in Database
                                                                // await _walletUserRepository.UpdateUserDetail(data);

                                                            }


                                                            else
                                                            {
                                                                response.RstKey = 6;
                                                                response.Message = _responseModel.Message;
                                                            }

                                                        }
                                                        else
                                                        {
                                                            var refundAmt = Convert.ToDecimal(data.CurrentBalance) + _commission.AmountWithCommission;
                                                            _logUtils.WriteTextToFileForWTxnTableLogs("PayBankTransferServiceForNGNbankflutter :- InvoiceNumber :" + invoiceNumber.InvoiceNumber + ";refundAmt :" + refundAmt + ";CurrentBalance :" + data.CurrentBalance);
                                                            data.CurrentBalance = Convert.ToString(refundAmt);
                                                            await _walletUserRepository.UpdateUserDetail(data);

                                                            var _transactionInitial = await _mobileMoneyRepository.GetTransactionInitiateRequest(transationInitiate.Id);

                                                            _transactionInitial.AfterTransactionBalance = data.CurrentBalance;
                                                            _transactionInitial.ReceiverCurrentBalance = data.CurrentBalance;
                                                            await _mobileMoneyRepository.UpdateTransactionInitiateRequest(_transactionInitial);
                                                            response.RstKey = 8;
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
                                    response.AccountNo = request.accountNo;
                                    response.DocStatus = IsdocVerified;
                                    response.DocumetStatus = (int)sender.DocumetStatus;
                                    response.CurrentBalance = sender.CurrentBalance;
                                    response.MobileNo = request.accountNo;
                                    response.ToMobileNo = request.accountNo;
                                    response.StatusCode = AggregatorySTATUSCODES.SUCCESSFUL;
                                    response.Amount = request.Amount;
                                    response.TransactionDate = DateTime.UtcNow;
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
                "MobileMoneyController".ErrorLog("MobileMoneyController.cs", "PayBankTransferServiceForNGNbankflutter", response + " " + ex.StackTrace + " " + ex.Message);

            }
            return response;
        }

        //ghana mobilemone
        public async Task<AddMoneyAggregatorResponse> GhanaMobileMobileService(PayMoneyAggregatoryRequest request, long WalletUserId = 0)
        {
            var response = new AddMoneyAggregatorResponse();

            var _commissionRequest = new CalculateCommissionRequest();
            var _commission = new CalculateCommissionResponse();
            var transationInitiate = new TransactionInitiateRequest();


            string responseString = "";
            try
            {
                var sender = await _walletUserRepository.GetUserDetailById(request.WalletUserId);

                string apiUrl = string.Empty;

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

                string customer = request.customer;//accountno 
                var _tranDate = DateTime.UtcNow;
                //
                var resultTL = await _transactionLimitAUService.CheckTransactionLimitAU(request.WalletUserId.ToString()); //check New TL


                //------Get Currency Rate--------------
                var currencyDetail = _masterDataRepository.GetCurrencyRate();
                decimal SendGHRate = Convert.ToDecimal(currencyDetail.SendGHRate);//
                decimal requestAmount = Convert.ToDecimal(request.Amount);//;


                if (customer.Length != 0 && request.ServiceCategoryId == 10)
                {
                    customer = request.customer.Substring(0, 1);
                    if (customer == "0")
                    {
                        customer = request.customer;
                    }
                    else
                    {
                        customer = "0" + request.customer;
                    }
                }

                string isdCode = request.IsdCode.Trim('+');
                customer = isdCode + request.customer;

                if (sender.IsOtpVerified == true)
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

                                                    _commission = await _setCommisionRepository.CalculateCommissionForMobileMoney(_commissionRequest, sender.WalletUserId, count, request.IsdCode);
                                                }
                                                else
                                                {
                                                    response.RstKey = 6;
                                                    response.Message = ResponseMessages.INSUFICIENT_BALANCE;
                                                    return response;
                                                }

                                                // requestamount:- 1000 xof ,deductAmountWithCommission:- 1025 xof ,user ko mil cedi 
                                                decimal amountWithCommision = decimal.Parse(string.Format("{0:0,0}", _commission.AmountWithCommission));    // "1,234,257";


                                                decimal amt = (_commission.TransactionAmount * SendGHRate); //xof to GHRate
                                                var finalAmt = Decimal.Parse(amt.ToString("0.00"));


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

                                                    string transactionInitiate = string.Empty;
                                                    #region Prepare the Model for Request

                                                    var _masterCard = new MasterCardPaymentRequest();

                                                    _masterCard.SessionId = null;
                                                    _masterCard.Version = null;
                                                    _masterCard.SuccessIndicator = null;
                                                    _masterCard.Merchant = "GhanaMobileMoney";
                                                    _masterCard.IsActive = true;
                                                    _masterCard.IsDeleted = false;
                                                    _masterCard.CreatedDate = DateTime.UtcNow;
                                                    _masterCard.UpdatedDate = DateTime.UtcNow;
                                                    _masterCard.Amount = request.Amount;
                                                    _masterCard.CommisionCharges = _commission.CommisionPercent;
                                                    _masterCard.TotalAmount = Convert.ToString(amountWithCommision);
                                                    _masterCard.WalletUserId = sender.WalletUserId;
                                                    _masterCard.TransactionNo = invoiceNumber.InvoiceNumber;
                                                    _masterCard.FlatCharges = _commission.FlatCharges;
                                                    _masterCard.BenchmarkCharges = _commission.BenchmarkCharges;
                                                    _masterCard.CommissionAmount = _commission.CommissionAmount.ToString();
                                                    await _cardPaymentRepository.SaveMasterCardPaymentRequest(_masterCard);

                                                    string ServiceName = string.Empty;

                                                    if (WalletService.ServiceName == "Vodafone Cash")
                                                    {
                                                        ServiceName = "VODAFONE";
                                                    }
                                                    else if (WalletService.ServiceName == "Airtel Money")
                                                    {
                                                        ServiceName = "AIRTEL";
                                                    }
                                                    else if (WalletService.ServiceName == "MTN Mobile Money")
                                                    {
                                                        ServiceName = "MTN";
                                                    }
                                                    var cc = await _masterDataRepository.IsdCodes();
                                                    var xx = cc.Where(x => x.IsdCode == sender.StdCode).FirstOrDefault();
                                                    var _Requestcustomer = new meta1();
                                                    _Requestcustomer.sender = sender.FirstName + ' ' + sender.LastName;
                                                    _Requestcustomer.sender_country = xx.CountryCode;
                                                    _Requestcustomer.mobile_number = sender.StdCode.Trim('+') + sender.MobileNo;

                                                    var _RequestAttributes = new flutterGhanaSendMobMonRequest();
                                                    _RequestAttributes.account_bank = ServiceName;
                                                    _RequestAttributes.account_number = customer;
                                                    _RequestAttributes.amount = Convert.ToInt32(finalAmt);
                                                    _RequestAttributes.currency = "GHS";
                                                    _RequestAttributes.reference = invoiceNumber.InvoiceNumber;
                                                    _RequestAttributes.beneficiary_name = request.BeneficiaryName;

                                                    _RequestAttributes.meta = _Requestcustomer;

                                                    var req = JsonConvert.SerializeObject(_RequestAttributes);

                                                    transationInitiate.InvoiceNumber = invoiceNumber.InvoiceNumber;
                                                    transationInitiate.ReceiverNumber = customer;
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
                                                    transationInitiate = await _mobileMoneyRepository.SaveTransactionInitiateRequest(transationInitiate);


                                                    //calling pay method insert data in Database
                                                    data.CurrentBalance = Convert.ToString(_commission.UpdatedCurrentBalance);
                                                    await _walletUserRepository.UpdateUserDetail(data);

                                                    #endregion


                                                    _logUtils.WriteTextToFileForPeyGhanaMobMoneLogs("PayBankGhanamobilemone :- InvoiceNumber - " + invoiceNumber.InvoiceNumber + " " + req);
                                                    //here to get psaymenturl
                                                    responseString = await GethashorUrl(req, "SendBankFlutter");//same url use for GhanaMobileMoney

                                                    if (responseString != "")
                                                    {
                                                        var TransactionInitial = await _mobileMoneyRepository.GetTransactionInitiateRequest(transationInitiate.Id);
                                                        TransactionInitial.JsonResponse = "GhanaMobileMoney Response" + responseString;
                                                        TransactionInitial = await _mobileMoneyRepository.UpdateTransactionInitiateRequest(TransactionInitial);
                                                        LogTransactionTypes.Response.SaveTransactionLog(LogTransactionNameTypes.PayMoney + subcategory.CategoryName, responseString, "Aggregator Url : ", sender.WalletUserId);

                                                        var _responseModel = JsonConvert.DeserializeObject<AddMoneyAggregatorResponse>(responseString);

                                                        JavaScriptSerializer js = new JavaScriptSerializer();
                                                        dynamic blogObject = js.Deserialize<dynamic>(responseString);
                                                        var txnreverifystatus = blogObject["data"]["status"];//stagin

                                                        if (_responseModel.status == "success" && (txnreverifystatus == "NEW" || txnreverifystatus == "SUCCESSFUL" || txnreverifystatus == "PENDING" || txnreverifystatus == "FAILED"))
                                                        {

                                                            _responseModel.FormatedTransactionDate = string.Format("{0:d}", DateTime.Now) + "" + string.Format("{0:T}", DateTime.Now);

                                                            _responseModel.AccountNo = customer;
                                                            _responseModel.MobileNo = customer;

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
                                                            tran.TransactionId = invoiceNumber.InvoiceNumber;
                                                            response.TransactionId = invoiceNumber.InvoiceNumber;
                                                            int _TransactionStatus = 0;
                                                            if (txnreverifystatus == "SUCCESSFUL")
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
                                                                    body = body.Replace("$$customer$$", customer);

                                                                    body = body.Replace("$$amount$$", "XOF " + _commission.TransactionAmount);
                                                                    body = body.Replace("$$ServiceTaxAmount$$", "XOF " + _commission.CommissionAmount);
                                                                    body = body.Replace("$$AmountWithCommission$$", "XOF " + _commission.AmountWithCommission);
                                                                    body = body.Replace("$$TransactionId$$", invoiceNumber.InvoiceNumber);

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
                                                            else if (txnreverifystatus == "NEW" || txnreverifystatus == "PENDING")
                                                            {
                                                                _TransactionStatus = (int)TransactionStatus.Pending;
                                                            }
                                                            else if (txnreverifystatus == "FAILED")
                                                            {
                                                                var refundAmt = Convert.ToDecimal(data.CurrentBalance) + _commission.AmountWithCommission;
                                                                data.CurrentBalance = Convert.ToString(refundAmt);
                                                                await _walletUserRepository.UpdateUserDetail(data);

                                                                var _transactionInitial = await _mobileMoneyRepository.GetTransactionInitiateRequest(transationInitiate.Id);

                                                                _transactionInitial.AfterTransactionBalance = data.CurrentBalance;
                                                                _transactionInitial.ReceiverCurrentBalance = data.CurrentBalance;
                                                                await _mobileMoneyRepository.UpdateTransactionInitiateRequest(_transactionInitial);

                                                                _TransactionStatus = (int)TransactionStatus.Failed;
                                                            }
                                                            //else if (_responseModel.StatusCode == AggregatorySTATUSCODES.EXCEPTION)
                                                            //{
                                                            //    _TransactionStatus = (int)TransactionStatus.Failed;
                                                            //}
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

                                                            tran.OperatorType = "GhanaMobileMoney";

                                                            tran.IsdCode = null;
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
                                                                if (txnreverifystatus == "NEW" || txnreverifystatus == "PENDING")
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
                                                                tran = await _mobileMoneyRepository.MobileMoneyService(tran);
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                _logUtils.WriteTextToFileForWTxnTableLogs("MobileMoneyservice :-GhanaMobileMobileService InvoiceNumber :" + invoiceNumber.InvoiceNumber + " " + ex.StackTrace + " " + ex.Message);
                                                            }
                                                            //calling pay method insert data in Database
                                                            // await _walletUserRepository.UpdateUserDetail(data);

                                                        }


                                                        else
                                                        {
                                                            response.RstKey = 6;
                                                            response.Message = _responseModel.Message;
                                                        }

                                                    }
                                                    else
                                                    {
                                                        var refundAmt = Convert.ToDecimal(data.CurrentBalance) + _commission.AmountWithCommission;
                                                        _logUtils.WriteTextToFileForWTxnTableLogs("GhanaMobileMobileService :- InvoiceNumber :" + invoiceNumber.InvoiceNumber + ";refundAmt :" + refundAmt + ";CurrentBalance :" + data.CurrentBalance);
                                                        data.CurrentBalance = Convert.ToString(refundAmt);
                                                        await _walletUserRepository.UpdateUserDetail(data);

                                                        var _transactionInitial = await _mobileMoneyRepository.GetTransactionInitiateRequest(transationInitiate.Id);

                                                        _transactionInitial.AfterTransactionBalance = data.CurrentBalance;
                                                        _transactionInitial.ReceiverCurrentBalance = data.CurrentBalance;
                                                        await _mobileMoneyRepository.UpdateTransactionInitiateRequest(_transactionInitial);
                                                        response.RstKey = 8;
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
                                response.AccountNo = customer;
                                response.DocStatus = IsdocVerified;
                                response.DocumetStatus = (int)sender.DocumetStatus;
                                response.CurrentBalance = sender.CurrentBalance;
                                response.MobileNo = customer;
                                response.ToMobileNo = customer;
                                response.StatusCode = AggregatorySTATUSCODES.SUCCESSFUL;
                                response.Amount = request.Amount;
                                response.TransactionDate = DateTime.UtcNow;
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
            }
            catch (Exception ex)
            {
                "MobileMoneyController".ErrorLog("MobileMoneyController.cs", "GhanaMobileMobileService", response + " " + ex.StackTrace + " " + ex.Message);

            }
            return response;


        }





        public async Task<string> GethashorUrl(string jsonReq, string flag)
        {

            string resBody = "";

            using (HttpClient client = new HttpClient())
            {
                // Call asynchronous network methods in a try/catch block to handle exceptions
                try
                {
                    if (flag == "SendBankFlutter")
                    {
                        var content = new StringContent(jsonReq, Encoding.UTF8, "application/json");
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CommonSetting.flutterFLWSECKey);

                        HttpResponseMessage response = await client.PostAsync(CommonSetting.flutterSendBankUrl, content);
                        response.EnsureSuccessStatusCode();
                        resBody = await response.Content.ReadAsStringAsync();

                    }

                }
                catch (HttpRequestException e)
                {
                    if (flag == "SendBankFlutter")
                    { e.Message.ErrorLog("SendBankFlutter", e.StackTrace + " " + e.Message); }
                }
                return resBody;

            }
        }


    }
}
