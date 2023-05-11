using ezeePay.Utility.CommonClass;
using ezeePay.Utility.Enums;
using Ezipay.Database;
using Ezipay.Repository.AfroBasketRepo;
using Ezipay.Repository.MasterData;
using Ezipay.Repository.TokenRepo;
using Ezipay.Repository.UserRepo;
using Ezipay.Service.UserService;
using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.ViewModel.AfroBasketViewModel;
using Ezipay.ViewModel.CommisionViewModel;
using Ezipay.ViewModel.common;
using Ezipay.ViewModel.ThridPartyApiVIewModel;
using Ezipay.ViewModel.TokenViewModel;
using Ezipay.ViewModel.WalletUserVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Ezipay.Service.AfroBasket
{
    public class AfroBasketService : IAfroBasketService
    {
        private IMasterDataRepository _masterDataRepository;
        private IWalletUserRepository _walletUserRepository;
        private IAfroBasketRepository _afroBasketRepository;
        private IWalletUserService _walletUserService;
        public AfroBasketService()
        {
            _masterDataRepository = new MasterDataRepository();
            _walletUserRepository = new WalletUserRepository();
            _afroBasketRepository = new AfroBasketRepository();
            _walletUserService = new WalletUserService();
        }
        /// <summary>
        /// Payment Wallet Verification
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        public async Task<AfroBasketVerificationResponse> PaymentWalletVerification(AfroBasketVerificationRequest requestModel)
        {
            var response = new AfroBasketVerificationResponse();
            var afroBasket = new AfroBasketData();
            var userData = new ViewUserList();
            // var userData = new AppUserRepository().GetUserDetailById(requestModel.UserId);
            try
            {

                userData = await _afroBasketRepository.GetUserDetailByEmail(requestModel.UserId);// db.usp_getUserByEmailId(requestModel.UserId).FirstOrDefault();
                if (userData != null)
                {
                    decimal currentbalance = Convert.ToDecimal(userData.Currentbalance);
                    decimal requestedAmount = Convert.ToDecimal(requestModel.Amount);

                    //System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                    //dtDateTime = dtDateTime.AddSeconds(requestModel.TimeStamp).ToLocalTime();

                    //var istdate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                    //var d = istdate.ToString("ddMMyyyyHHmmss");
                    //if (dtDateTime != null)
                    //{
                    // var diffInSeconds = (DateTime.UtcNow - dtDateTime).TotalSeconds;
                    if (requestedAmount > 0)
                    {
                        if (currentbalance > 0 && currentbalance >= requestedAmount)
                        {
                            var invoiceNumber = await _masterDataRepository.GetInvoiceNumber();
                            string securityCode = MD5Encryption(invoiceNumber.InvoiceNumber);

                            #region Map session related values
                            var sessionToken = await new TokenRepository().GenerateToken(new TokenRequest { DeviceUniqueId = userData.DeviceToken, WalletUserId = userData.WalletUserId });
                            if (sessionToken != null)
                            {
                                response.SessionId = sessionToken.Token;
                                response.SecurityCode = securityCode;
                                var otpReq = new OtpRequest
                                {
                                    IsdCode = userData.StdCode,
                                    MobileNo = userData.MobileNo
                                };
                                // var OtpResponse = _IAppUser.SendOtp(otpReq);
                                afroBasket.UserId = userData.WalletUserId;
                                afroBasket.MobileNo = userData.MobileNo;
                                afroBasket.SecurityCode = invoiceNumber.InvoiceNumber;
                                afroBasket.SessionId = sessionToken.Token;
                                afroBasket.TimeStamp = requestModel.TimeStamp.ToString();
                                afroBasket.Amount = requestedAmount;
                                afroBasket.Otp = "0000";//OtpResponse.Otp;
                                afroBasket.CreatedDate = DateTime.UtcNow;
                                afroBasket.UpdatedDate = DateTime.UtcNow;

                                afroBasket = await _afroBasketRepository.SaveAfroBasketData(afroBasket);
                                if (afroBasket != null)
                                {
                                    response.IsSuccess = true;
                                    //response.Message = "OTP sent successfully.";
                                    response.Message = "Verify successfully.";
                                    response.StatusCode = (int)WalletTransactionStatus.SUCCESS;
                                }
                                else
                                {
                                    response.IsSuccess = false;
                                    response.Message = "User not exist.";
                                    response.StatusCode = (int)WalletTransactionStatus.FAILED;
                                }

                            }
                            #endregion
                        }
                        else
                        {
                            response.IsSuccess = false;
                            response.Message = "Insufficient balance in your wallet.";
                            response.StatusCode = (int)WalletTransactionStatus.FAILED;
                        }
                    }
                    else
                    {
                        response.IsSuccess = false;
                        response.Message = "Request time out.";
                        response.StatusCode = (int)WalletTransactionStatus.FAILED;
                    }
                    //}
                    //else
                    //{
                    //    response.IsSuccess = false;
                    //    response.Message = "Time stamp is not in currect";
                    //    response.StatusCode = (int)WalletTransactionStatus.FAILED;
                    //}
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "User not exist.";
                    response.StatusCode = (int)WalletTransactionStatus.FAILED;
                }

            }
            catch (Exception ex)
            {

            }
            //-------------------
            return response;
        }

        /// <summary>
        /// Payment By UserWallet
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        public async Task<AfroBasketPaymentVerifyResponse> PaymentByUserWallet(AfroBasketVerifyRequest requestModel, string token)
        {
            //-----------------           
            var response = new AfroBasketPaymentVerifyResponse();
            string securityCode = MD5Decrypt(requestModel.SecurityCode);
            //HttpRequestMessage httpRequestMessage = HttpContext.Current.Items["MS_HttpRequestMessage"] as HttpRequestMessage;
            ////string token = GlobalData.Key;//httpRequestMessage.Headers.GetValues("Token").First();

            var iSSessionValid = await _afroBasketRepository.GetSessionToken(token); //db.SessionTokens.Where(x => x.TokenValue == token).FirstOrDefault();
            if (iSSessionValid == null)
            {
                return response;
            }
            var isValid = await _afroBasketRepository.GetAfroBasketData(securityCode); //db.FlightBookingDatas.Where(x => x.SecurityCode == securityCode).FirstOrDefault();

            //var userData = new AppUserRepository().GetUserDetailById((long)isValid.UserId);
            var userData = await _afroBasketRepository.GetUserDetailByEmail(requestModel.UserId);//db.usp_getUserByEmailId(requestModel.UserId).FirstOrDefault();
            var currentUserData = await _walletUserRepository.GetCurrentUser(userData.WalletUserId);//db.WalletUsers.Where(x => x.WalletUserId == userData.WalletUserId).FirstOrDefault();
            decimal balance = Convert.ToDecimal(currentUserData.CurrentBalance);
            decimal requestedAmount = Convert.ToDecimal(isValid.Amount);

            //System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            //dtDateTime = dtDateTime.AddSeconds(requestModel.TimeStamp).ToLocalTime();

            //var istdate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            //var d = istdate.ToString("ddMMyyyyHHmmss");

            //var diffInSeconds = (DateTime.UtcNow - dtDateTime).TotalSeconds;

            //if (securityCode == isValid.SecurityCode && diffInSeconds <= 90)
            if (requestModel != null)
            {
                if (balance > 0 && balance >= requestedAmount && userData.EmailId == requestModel.UserId && requestedAmount > 0)
                {
                    currentUserData.CurrentBalance = Convert.ToString(Math.Round((Convert.ToDecimal(userData.Currentbalance)) - requestedAmount, 2));

                    var result = await _walletUserRepository.UpdateUserDetail(currentUserData);//db.SaveChanges();
                    if (result != null)
                    {
                        response.Amount = requestedAmount.ToString();
                        response.UserId = userData.WalletUserId;
                        response.IsSuccess = true;
                        response.Message = "Payment done successfully.";
                        response.StatusCode = (int)WalletTransactionStatus.SUCCESS;

                        var tran = new WalletTransaction();
                        tran.Comments = "Pay for AfroBasket";
                        tran.InvoiceNo = isValid.SecurityCode;
                        tran.TotalAmount = isValid.Amount.ToString();
                        tran.TransactionType = AggragatorServiceType.DEBIT;
                        tran.IsBankTransaction = false;
                        tran.BankBranchCode = string.Empty;
                        tran.BankTransactionId = string.Empty;
                        tran.CommisionId = 0;
                        tran.WalletAmount = isValid.Amount.ToString();
                        tran.ServiceTaxRate = 0;
                        tran.ServiceTax = "0";
                        tran.WalletServiceId = 160;
                        tran.SenderId = userData.WalletUserId;
                        tran.ReceiverId = userData.WalletUserId;
                        tran.AccountNo = string.Empty;
                        tran.TransactionId = "0";
                        tran.IsAdminTransaction = false;
                        tran.IsActive = true;
                        tran.IsDeleted = false;
                        tran.CreatedDate = DateTime.UtcNow;
                        tran.UpdatedDate = DateTime.UtcNow;
                        tran.TransactionTypeInfo = (int)TransactionTypeInfo.AfroBasket;
                        tran.TransactionStatus = (int)TransactionStatus.Completed;
                        tran.MerchantCommissionAmount = "0";
                        tran.CommisionAmount = "0";
                        tran.VoucherCode = string.Empty;
                        tran.MerchantCommissionId = 0;
                        tran.UpdatedOn = DateTime.Now;
                        tran.BenchmarkCharges = "0";
                        tran.FlatCharges = "0";
                        tran.CommisionPercent = "0";
                        await _afroBasketRepository.SaveWalletTransaction(tran);
                        //db.WalletTransactions.Add(tran);
                        //db.SaveChanges();
                    }
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "insufficient fund please add money in your wallet.";
                    response.StatusCode = (int)WalletTransactionStatus.FAILED;
                }
            }
            else
            {
                response.IsSuccess = false;
                response.Message = "Request time out.";
                response.StatusCode = (int)WalletTransactionStatus.FAILED;
                bool isSession = await IsSession();
            }
            return response;
        }

        public async Task<bool> IsSession()
        {
            bool IsSuccess = false;
            HttpRequestMessage httpRequestMessage = HttpContext.Current.Items["MS_HttpRequestMessage"] as HttpRequestMessage;
            string token = httpRequestMessage.Headers.GetValues("Token").First();

            try
            {
                IsSuccess = await _afroBasketRepository.IsSession(token);
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("FlightBookingPaymentService.cs", "IsSession", IsSuccess);
            }

            return IsSuccess;
        }

        public string MD5Encryption(string request)
        {
            string res = "";
            string hash = "appventurez2019";
            byte[] data = UTF8Encoding.UTF8.GetBytes(request);
            ////Url for Payment           
            //string hashedData = request;        
            //using (SHA512 sha512Hash = SHA512.Create())
            //{
            //    //From String to byte array
            //    byte[] sourceBytes = Encoding.UTF8.GetBytes(hashedData);
            //    byte[] hashBytes = sha512Hash.ComputeHash(sourceBytes);

            //    return GetStringFromHash(hashBytes);
            //}
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                byte[] keys = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(hash));
                using (TripleDESCryptoServiceProvider tp = new TripleDESCryptoServiceProvider() { Key = keys, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 })
                {
                    ICryptoTransform crypto = tp.CreateEncryptor();
                    byte[] results = crypto.TransformFinalBlock(data, 0, data.Length);
                    res = Convert.ToBase64String(results, 0, results.Length);
                }
            }
            return res;
        }

        public string MD5Decrypt(string request)
        {
            string res = "";
            string hash = "appventurez2019";
            byte[] data = Convert.FromBase64String(request);
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                byte[] keys = md5.ComputeHash(Encoding.UTF8.GetBytes(hash));
                using (TripleDESCryptoServiceProvider tp = new TripleDESCryptoServiceProvider() { Key = keys, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 })
                {
                    ICryptoTransform crypto = tp.CreateDecryptor();
                    byte[] result = crypto.TransformFinalBlock(data, 0, data.Length);
                    res = UTF8Encoding.UTF8.GetString(result);
                }
            }
            return res;
        }

        public async Task<VerifyResponse> DataVerification(VerifyAfroBasketRequest request)
        {
            var subReq = new VerifyResponse();
            try
            {

                if (request != null)
                {
                    var re = new AfroBasketVerifyData();
                    re.AgentCode = Convert.ToInt32(request.agentcode);
                    re.CheckSum = request.checksum;
                    re.Tgt = request.merchantcode;
                    re.TokenId = request.tokenid;
                    re.IsDeleted = true;
                    await _afroBasketRepository.AfroBasketBooking(re);

                }

                var invoiceNumber = await _masterDataRepository.GetInvoiceNumber();
                var _basketRequest = new GetFlightBookingRequest();
                _basketRequest.agentcode = Convert.ToString(request.agentcode);
                _basketRequest.tokenID = request.tokenid;
                _basketRequest.merchantcode = request.merchantcode;
                _basketRequest.saltkey = "hsyd2KDJ29";


                var checkSumm = new CommonMethods().Sha512Ha(_basketRequest);
                if (checkSumm != null)
                {
                    var res = new AfroBasketVerifyData();
                    res.AgentCode = Convert.ToInt32(request.agentcode);
                    res.CheckSum = checkSumm;
                    res.Tgt = request.merchantcode;
                    res.TokenId = request.tokenid;
                    res.IsDeleted = false;
                    res.IsActive = false;
                    //save detail
                    await _afroBasketRepository.AfroBasketBooking(res);
                }
                // var subReq = new VerifyResponse();
                long userId = Convert.ToInt32(request.agentcode);
                var data = await _afroBasketRepository.GetUserDetailById(userId, request.tokenid);
                var userprofile = await _walletUserRepository.GetUserDetailById(userId);
                if (request.checksum != null && userprofile.WalletUserId > 0 && data != null)
                {

                    if (data != null)
                    {
                        var requset = new FinalCheckSum
                        {
                            agentCode = userId.ToString(),
                            statusMessage = "Success",
                            merchantCode = request.merchantcode,
                            tokenId = request.tokenid,
                            statusCode = 0.ToString(),
                            saltKey = _basketRequest.saltkey
                        };

                        var finalCheckSumm = new CommonMethods().Sha512Final(requset);
                        subReq.statusCode = 0;
                        subReq.statusMessage = "Success";
                        subReq.checksum = finalCheckSumm;
                        subReq.merchantcode = request.merchantcode;
                        subReq.tokenID = request.tokenid;
                        subReq.kcode = userId.ToString();
                        subReq.firstname = userprofile.FirstName;
                        subReq.lastname = userprofile.LastName;
                        subReq.emailid = userprofile.EmailId;
                        subReq.contactphone = userprofile.MobileNo;
                        subReq.city = "GHANA";
                        subReq.mobile = userprofile.MobileNo;
                        subReq.state = "GHANA";
                        subReq.title = "Mr/Mrs.";
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
                else if (request.agentcode == null || request.checksum == null || request.merchantcode == null || request.tokenid == null)
                {
                    subReq.statusCode = 1;
                    subReq.statusMessage = "Invalid json.";
                }
                else if (checkSumm != request.checksum)
                {
                    subReq.statusCode = 1;
                    subReq.statusMessage = "Checksum Mismatch.";
                }
                else if (userId != Convert.ToInt32(request.agentcode) && data.TokenId != request.tokenid)
                {
                    subReq.statusCode = 1;
                    subReq.statusMessage = "Verification failed.";
                }
                else if (data == null)
                {
                    subReq.statusCode = 1;
                    subReq.statusMessage = "Verification failed.";
                }
                else
                {
                    subReq.statusCode = 1;
                    subReq.statusMessage = "Unexpected Error.";
                }
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("AfroBasketService", "DataVerification", ex);
            }

            return subReq;
        }


        public async Task<AfroBasketLoginResponse> AfroBasketLogin(string token) ///
        {
            CalculateCommissionResponse _commission = new CalculateCommissionResponse();
            CalculateCommissionRequest _commissionRequest = new CalculateCommissionRequest();
            var response = new AfroBasketLoginResponse();
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
                                var _afroBasketVerifyData = new AfroBasketVerifyData
                                {
                                    AgentCode = sender.WalletUserId,
                                    Tgt = req.tgt,
                                    TokenId = invoiceNumber.InvoiceNumber,
                                    CheckSum = checkSumm,
                                    CreatedDate = DateTime.UtcNow,
                                    UpdatedDate = DateTime.UtcNow,
                                    IsActive = true,
                                    IsDeleted = false,
                                };

                                int result = await _afroBasketRepository.AfroBasketLogin(_afroBasketVerifyData);


                                #endregion


                                if (result > 0)
                                {
                                    apiUrl = "";//ThirdPartyAggragatorSettings.AfroLogin + "agentcode=" + req.agentcode + "&" + "tokenid=" + req.tokenID + "&" + "tgt=" + req.tgt + "&" + "Checksum=" + checkSumm;
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
    }
}
