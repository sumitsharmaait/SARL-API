using ezeePay.Utility.Enums;
using Ezipay.Database;
using Ezipay.Repository.FlightHotelRepo;
using Ezipay.Repository.MasterData;
using Ezipay.Repository.TokenRepo;
using Ezipay.Repository.UserRepo;
using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.ViewModel.FlightHotelViewModel;
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

namespace Ezipay.Service.FlightHotelService
{
    public class FlightBookingPaymentService : IFlightBookingPaymentService
    {
        private IFlightBookingPaymentRepository _flightBookingPaymentRepository;
        private IMasterDataRepository _masterDataRepository;
        private IWalletUserRepository _walletUserRepository;
        public FlightBookingPaymentService()
        {
            _flightBookingPaymentRepository = new FlightBookingPaymentRepository();
            _masterDataRepository = new MasterDataRepository();
            _walletUserRepository = new WalletUserRepository();
        }
        /// <summary>
        /// Payment Wallet Verification
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        public async Task<FlightBookingVerificationResponse> PaymentWalletVerification(FlightBookingVerificationRequest requestModel)
        {
            var response = new FlightBookingVerificationResponse();
            var flightBookingData = new FlightBookingData();
            var userData = new ViewUserList();
            // var userData = new AppUserRepository().GetUserDetailById(requestModel.UserId);
            try
            {



                userData = await _flightBookingPaymentRepository.GetUserDetailByEmail(requestModel.UserId);// db.usp_getUserByEmailId(requestModel.UserId).FirstOrDefault();
                if (userData != null)
                {
                    decimal currentbalance = Convert.ToDecimal(userData.Currentbalance);
                    decimal requestedAmount = Convert.ToDecimal(requestModel.Amount);

                    System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                    dtDateTime = dtDateTime.AddSeconds(requestModel.TimeStamp).ToLocalTime();

                    //var istdate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                    //var d = istdate.ToString("ddMMyyyyHHmmss");
                    if (dtDateTime != null)
                    {
                        var diffInSeconds = (DateTime.UtcNow - dtDateTime).TotalSeconds;
                        if (diffInSeconds <= 90)
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
                                    flightBookingData.UserId = userData.WalletUserId;
                                    flightBookingData.MobileNo = userData.MobileNo;
                                    flightBookingData.SecurityCode = invoiceNumber.InvoiceNumber;
                                    flightBookingData.SessionId = sessionToken.Token;
                                    flightBookingData.TimeStamp = requestModel.TimeStamp.ToString();
                                    flightBookingData.Amount = requestedAmount;
                                    flightBookingData.Otp = "0000";//OtpResponse.Otp;
                                    flightBookingData.CreatedDate = DateTime.UtcNow;
                                    flightBookingData.UpdateDate = DateTime.UtcNow;
                                    //db.FlightBookingDatas.Add(flightBookingData);
                                    //int result = db.SaveChanges();
                                    flightBookingData = await _flightBookingPaymentRepository.SaveFlightData(flightBookingData);
                                    if (flightBookingData != null)
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
                    }
                    else
                    {
                        response.IsSuccess = false;
                        response.Message = "Time stamp is not in currect";
                        response.StatusCode = (int)WalletTransactionStatus.FAILED;
                    }
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
        public async Task<FlightBookingPaymentVerifyResponse> PaymentByUserWallet(FlightBookingVerifyRequest requestModel)
        {
            //-----------------           
            var response = new FlightBookingPaymentVerifyResponse();
            string securityCode = MD5Decrypt(requestModel.SecurityCode);
            HttpRequestMessage httpRequestMessage = HttpContext.Current.Items["MS_HttpRequestMessage"] as HttpRequestMessage;
            string token = httpRequestMessage.Headers.GetValues("Token").First();

            var iSSessionValid = await _flightBookingPaymentRepository.GetSessionToken(token); //db.SessionTokens.Where(x => x.TokenValue == token).FirstOrDefault();
            if (iSSessionValid == null)
            {
                return response;
            }
            var isValid = await _flightBookingPaymentRepository.GetFlightData(securityCode); //db.FlightBookingDatas.Where(x => x.SecurityCode == securityCode).FirstOrDefault();

            //var userData = new AppUserRepository().GetUserDetailById((long)isValid.UserId);
            var userData = await _flightBookingPaymentRepository.GetUserDetailByEmail(requestModel.UserId);//db.usp_getUserByEmailId(requestModel.UserId).FirstOrDefault();
            var currentUserData = await _walletUserRepository.GetCurrentUser(userData.WalletUserId);//db.WalletUsers.Where(x => x.WalletUserId == userData.WalletUserId).FirstOrDefault();
            decimal balance = Convert.ToDecimal(currentUserData.CurrentBalance);
            decimal requestedAmount = Convert.ToDecimal(isValid.Amount);

            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(requestModel.TimeStamp).ToLocalTime();

            var istdate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            var d = istdate.ToString("ddMMyyyyHHmmss");

            var diffInSeconds = (DateTime.UtcNow - dtDateTime).TotalSeconds;

            if (securityCode == isValid.SecurityCode && diffInSeconds <= 90)
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
                        tran.Comments = "Flight booking";
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
                        tran.WalletServiceId = 159;
                        tran.SenderId = userData.WalletUserId;
                        tran.ReceiverId = userData.WalletUserId;
                        tran.AccountNo = string.Empty;
                        tran.TransactionId = "0";
                        tran.IsAdminTransaction = false;
                        tran.IsActive = true;
                        tran.IsDeleted = false;
                        tran.CreatedDate = DateTime.UtcNow;
                        tran.UpdatedDate = DateTime.UtcNow;
                        tran.TransactionTypeInfo = (int)TransactionTypeInfo.Payforfilgth;
                        tran.TransactionStatus = (int)TransactionStatus.Completed;
                        tran.MerchantCommissionAmount = "0";
                        tran.CommisionAmount = "0";
                        tran.VoucherCode = string.Empty;
                        tran.MerchantCommissionId = 0;
                        tran.UpdatedOn = DateTime.Now;
                        tran.BenchmarkCharges = "0";
                        tran.FlatCharges = "0";
                        tran.CommisionPercent = "0";
                        await _flightBookingPaymentRepository.SaveWalletTransaction(tran);
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
                bool isSession =await IsSession();
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
                IsSuccess = await _flightBookingPaymentRepository.IsSession(token);
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
    }
}
