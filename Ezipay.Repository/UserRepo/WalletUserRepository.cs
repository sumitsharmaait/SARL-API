using ezeePay.Utility.CommonClass;
using ezeePay.Utility.Enums;
using Ezipay.Database;
using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.Utility.SendOtp;
using Ezipay.ViewModel;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.AirtimeViewModel;
using Ezipay.ViewModel.SendEmailViewModel;
using Ezipay.ViewModel.SendOtpViewModel;
using Ezipay.ViewModel.WalletUserVM;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Ezipay.Repository.UserRepo
{
    public class WalletUserRepository : IWalletUserRepository
    {
        public async Task<WalletUser> SignUp(WalletUser request)
        {
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                db.WalletUsers.Add(request);
                await (db.SaveChangesAsync());
                return request;
            }
        }

        public async Task<WalletUser> GetUserDetailById(long WalletUserId = 0)
        {
            string TokenValue = string.Empty;
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    if (WalletUserId > 0)
                    {
                        var AdminKeys = AES256.AdminKeyPair;
                        var user = await db.WalletUsers.Where(x => x.WalletUserId == WalletUserId).FirstOrDefaultAsync();
                        if (user != null)
                        {
                            user.FirstName = AES256.Decrypt(user.PrivateKey, user.FirstName);
                            user.LastName = AES256.Decrypt(user.PrivateKey, user.LastName);
                            user.StdCode = user.StdCode;
                            user.MobileNo = AES256.Decrypt(AdminKeys.PrivateKey, user.MobileNo);
                            user.EmailId = AES256.Decrypt(AdminKeys.PrivateKey, user.EmailId).Trim().ToLower();
                            user.QrCode = user.StdCode + "," + user.MobileNo;
                            user.CurrentBalance = user.CurrentBalance;
                            user.DeviceToken = user.DeviceToken;
                            user.DeviceType = user.DeviceType;
                            user.WalletUserId = user.WalletUserId;
                            user.IsDisabledTransaction = user.IsDisabledTransaction;

                            return user;
                        }
                        else
                        {
                            return null;
                        }

                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {

                //ex.Message.ErrorLog("AppUserRepository.cs", "UserDetail");
                return null;
            }


        }

        public async Task<string> IsUserMerchant(long WalletUserId = 0)
        {
            string serviceName = "";
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                var ws = await db.WalletServices.Where(x => x.MerchantId == WalletUserId).FirstOrDefaultAsync();
                if (ws != null)
                    serviceName = ws.ServiceName;
            }
            return serviceName;
        }

        public async Task<WalletUser> GetUserPushDetailById(string deviceKey, int deviceType)
        {


            string TokenValue = string.Empty;
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    if (deviceKey != null && deviceType > 0)
                    {
                        var AdminKeys = AES256.AdminKeyPair;
                        var receiver = await db.WalletUsers.Where(x => x.DeviceToken == deviceKey && x.DeviceType == deviceType).FirstOrDefaultAsync();
                        if (receiver != null)
                        {
                            receiver.FirstName = AES256.Decrypt(receiver.PrivateKey, receiver.FirstName);
                            receiver.LastName = AES256.Decrypt(receiver.PrivateKey, receiver.LastName);
                            receiver.StdCode = receiver.StdCode;
                            receiver.MobileNo = AES256.Decrypt(AdminKeys.PrivateKey, receiver.MobileNo);
                            receiver.EmailId = AES256.Decrypt(AdminKeys.PrivateKey, receiver.EmailId).Trim().ToLower();
                            receiver.QrCode = receiver.StdCode + "," + receiver.MobileNo;
                            receiver.CurrentBalance = receiver.CurrentBalance;
                            receiver.DeviceToken = receiver.DeviceToken;
                            receiver.DeviceType = receiver.DeviceType;
                            receiver.DeviceToken = receiver.DeviceToken;
                            return receiver;
                        }
                        else
                        {
                            return null;
                        }

                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {

                //ex.Message.ErrorLog("AppUserRepository.cs", "UserDetail");
                return null;
            }


        }

        public async Task<UserExistanceResponse> CredentialsExistance(UserExistanceRequest request)
        {
            var response = new UserExistanceResponse();
            var adminKeyPair = AES256.AdminKeyPair;

            request.MobileNo = AES256.Encrypt(adminKeyPair.PublicKey, request.MobileNo);

            request.EmailId = AES256.Encrypt(adminKeyPair.PublicKey, request.EmailId.Trim().ToLower());
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                bool isMatched = await db.WalletUsers.AnyAsync(x => x.EmailId.Trim() == request.EmailId && x.MobileNo.Trim() == request.MobileNo.Trim());
                if (!isMatched)
                {
                    if (!db.WalletUsers.Any(x => x.EmailId.Trim() == request.EmailId && x.MobileNo.Trim() == request.MobileNo.Trim()))
                    {
                        if (!db.WalletUsers.Any(x => x.EmailId.Trim() == request.EmailId.Trim()))
                        {
                            if (!db.WalletUsers.Any(x => x.MobileNo.Trim() == request.MobileNo.Trim()))
                            {
                                //BothNotExist
                                response.RstKey = (int)UserExistanceStatus.BothNotExist;
                            }
                            else
                            {
                                //MobileExist
                                response.RstKey = (int)UserExistanceStatus.MobileExist;
                            }
                        }
                        else
                        {
                            //EmailExist
                            response.RstKey = (int)UserExistanceStatus.EmailExist;
                        }
                    }
                    else
                    {
                        //BothExist
                        response.RstKey = (int)UserExistanceStatus.BothExist;
                    }
                }
                else
                {
                    response.RstKey = (int)UserExistanceStatus.UserRegistered;
                }
            }
            return response;
        }

        public async Task<UserExistanceResponse> CredentialsExistanceForMobileNumber(UserExistanceRequest request)
        {
            var response = new UserExistanceResponse();
            var adminKeyPair = AES256.AdminKeyPair;
            request.MobileNo = AES256.Encrypt(adminKeyPair.PublicKey, request.MobileNo);
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                var aa = await db.WalletUsers.Where(x => x.MobileNo.Trim() == request.MobileNo.Trim() && x.IsOtpVerified == true).CountAsync();
                if (aa > 0)
                // if (db.WalletUsers.Any(x => x.MobileNo.Trim() == request.MobileNo.Trim()))
                {                    //MobileExist
                    response.RstKey = (int)UserExistanceStatus.MobileExist;
                }
                else
                {
                    //BothNotExist
                    response.RstKey = (int)UserExistanceStatus.BothNotExist;
                }

            }
            return response;
        }

        public async Task<OtpResponse> SendOtp(SendOtpRequest request)
        {
            string customer = request.MobileNo.Substring(0, 1);

            if (request.IsdCode != "+225") //civ con.change 
            {
                if (customer == "0")
                {
                    customer = request.MobileNo.Remove(0, 1);
                }
                else
                {
                    customer = request.MobileNo;
                }
            }
            else
            {
                customer = request.MobileNo;
            }

            OtpResponse response = new OtpResponse();
            try
            {

                if (!string.IsNullOrEmpty(request.Otp))
                {

                    using (var db = new DB_9ADF60_ewalletEntities())
                    {
                        string MobileNo = (request.IsdCode + customer).ToString().Trim();
                        string isdCode = request.IsdCode.Remove(0, 1);
                        // var model = new SendMessageRequest { MobileNo = customer, ISD = request.IsdCode, Message = ResponseMessages.OTP_MESSAGE + request.Otp };
                        //var modelTeleSign = new SendOtpTeleSignRequest { phone_number = isdCode + customer, verify_code = request.Otp, template =  ResponseMessages.OTP_MESSAGE + " " + request.Otp + " ID: " + CommonSetting.AutoReadOtpId };
                        //telesign
                        //var model = new SendMessageRequest { MobileNo = customer, ISD = request.IsdCode, Message = "<#> " + ResponseMessages.OTP_MESSAGE + request.Otp + " ID: " + CommonSetting.AutoReadOtpId };

                        //route
                        var model = new SendMessageRequest { MobileNo = customer, ISD = isdCode, Message = ResponseMessages.OTP_MESSAGE + request.Otp + " ID: " + CommonSetting.AutoReadOtpId };
                        DateTime time = System.DateTime.Now;
                        var OtpExist = await db.OneTimePasswords.Where(x => x.MobileNo == MobileNo && DbFunctions.TruncateTime(x.CreatedDate) == time.Date).FirstOrDefaultAsync();
                        if (OtpExist != null)
                        {
                            if (OtpExist.OtpCounter != CommonSetting.OtpLimit)
                            {
                                OtpExist.Otp = request.Otp;
                                OtpExist.OtpCounter = OtpExist.OtpCounter + 1;

                                 var otpResponse = new SendMessage().SendMessgeWithISDCode(model);

                                //var otpResponse = new SendMessage().CallBackTeleSign(modelTeleSign);
                                if (otpResponse)
                                {
                                    db.SaveChanges();
                                    response.IsSuccess = true;
                                    response.Message = ResponseMessages.OTP_SENT;
                                    response.StatusCode = (int)WalletTransactionStatus.SUCCESS;
                                }
                                response.IsSuccess = false;
                                response.Message = ResponseMessages.OTP_NOT_SENT;
                                response.StatusCode = (int)WalletTransactionStatus.FAILED;
                                // response.OtpId = OtpExist.OtpId;
                            }
                            else
                            {
                                response.IsSuccess = false;
                                response.Message = ResponseMessages.OTP_SENT_RANGE;
                                response.StatusCode = 0;
                            }
                        }
                        else
                        {
                            var OtpNew = new OneTimePassword();

                            OtpNew.CreatedDate = DateTime.UtcNow;
                            OtpNew.MobileNo = MobileNo;
                            OtpNew.OtpCounter = 1;
                            OtpNew.Otp = request.Otp;

                            var otpResponse = new SendMessage().SendMessgeWithISDCode(model);
                            //var otpResponse = await new SendMessage().SendOtpdimoco(modelTeleSign);

                            if (otpResponse)
                            {
                                db.OneTimePasswords.Add(OtpNew);
                                db.SaveChanges();
                                response.IsSuccess = true;
                                response.Message = ResponseMessages.OTP_SENT;
                                response.StatusCode = (int)WalletTransactionStatus.SUCCESS;
                            }
                            else
                            {
                                response.IsSuccess = false;
                                response.Message = ResponseMessages.OTP_NOT_SENT;
                                response.StatusCode = (int)WalletTransactionStatus.FAILED;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("WalletUserRepository.cs", "SendOtp", request);
            }
            return response;
        }

        public async Task<OneTimePassword> GetOtpForCallBack(SendOtpCallBackRequest request)
        {

            var res = new OneTimePassword();
            string customer = request.MobileNo.Substring(0, 1);


            if (request.IsdCode != "+225") //civ con.change 
            {
                if (customer == "0")
                {
                    customer = request.MobileNo.Remove(0, 1);
                }
                else
                {
                    customer = request.MobileNo;
                }
            }
            else
            {
                customer = request.MobileNo;
            }
            using (var db = new DB_9ADF60_ewalletEntities())
            {

                // save callback counter 
                var res1 = await db.OneTimePasswords.Where(x => x.MobileNo == request.IsdCode + customer).OrderByDescending(x => x.OtpId).FirstAsync();

                if (res1.Callbackcounter == null)
                {
                    res1.Callbackcounter = 0;
                }

                res1.Callbackcounter = res1.Callbackcounter + 1;
                db.SaveChanges();

                //chk again latest otp 
                res = await db.OneTimePasswords.Where(x => x.MobileNo == request.IsdCode + customer).OrderByDescending(x => x.OtpId).FirstAsync();

            }
            return res;
        }

        public async Task<UserExistanceResponse> VerifyOtp(VerifyOtpRequest request)
        {
            UserExistanceResponse response = new UserExistanceResponse();

            try
            {
                string customer = request.MobileNo.Substring(0, 1);

                if (request.IsdCode != "+225") //civ con.change 
                {
                    if (customer == "0")
                    {
                        customer = request.MobileNo.Remove(0, 1);
                    }
                    else
                    {
                        customer = request.MobileNo;
                    }
                }
                else
                {
                    customer = request.MobileNo;
                }
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    var res = await db.OneTimePasswords.AnyAsync(x => x.MobileNo == (request.IsdCode + customer) && x.Otp == request.Otp);
                    if (!res)
                    {
                        response.RstKey = 2;
                    }
                    else
                    {
                        response.RstKey = 1;

                    }
                    //update walletuser table with isdcode & mobile 
                    if (response.RstKey == 1)
                    {
                        var adminKeyPair = AES256.AdminKeyPair;
                        request.MobileNo = AES256.Encrypt(adminKeyPair.PublicKey, request.MobileNo);
                        var IsExist = await db.WalletUsers.Where(x => x.StdCode == request.IsdCode && x.MobileNo == request.MobileNo).FirstOrDefaultAsync();

                        if (!string.IsNullOrEmpty(IsExist.MobileNo)) //not empty mobile no
                        {
                            IsExist.MobileNo = request.MobileNo;
                            IsExist.StdCode = request.IsdCode;
                            IsExist.IsOtpVerified = true;
                            IsExist.UpdatedDate = DateTime.Now;

                            db.SaveChanges();
                            //var IsExist1 = await db.WalletUsers.Where(x => x.StdCode == request.IsdCode && x.MobileNo == request.MobileNo).FirstOrDefaultAsync();
                            //response.IsMobileVerified = (bool)IsExist1.IsOtpVerified;//take resp. to view 
                        }

                    }


                }
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("WalletUserRepository.cs", "VerifyOtp", request);
                response.RstKey = 2;

            }
            return response;
        }

        public async Task<UserEmailVerifyResponse> VerfiyByEmailId(string token)
        {
            var result = new UserEmailVerifyResponse();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                long EmailVerificationId = Convert.ToInt64(token.Split('_')[1]);

                var user = await db.EmailVerifications.Where(x => x.EmailVerificationId == EmailVerificationId).AsQueryable().FirstOrDefaultAsync();
                if (user != null)
                {
                    var walletusr = db.WalletUsers.Where(x => x.EmailId == user.EmailId).AsQueryable().FirstOrDefault();
                    if (user != null && walletusr != null)
                    {
                        if (user.IsVerified == true)
                        {
                            result.VerficationStatus = false;
                            result.RstKey = 1;
                            //objResponse.VerficationMessage = ResponseMessages.ALREADY_VERIFIED;
                        }
                        else
                        {
                            user.IsVerified = true;
                            walletusr.IsEmailVerified = true;
                            user.VerificationDate = DateTime.UtcNow;
                            db.SaveChanges();

                            result.VerficationStatus = true;
                            //objResponse.VerficationMessage = ResponseMessages.VERFIED_SUCCESSFULLY;
                            result.RstKey = 2;
                        }
                    }
                    else
                    {
                        result.VerficationStatus = false;
                        // objResponse.VerficationMessage = ResponseMessages.NO_EMAIL_RECORD_FOUND;
                        result.RstKey = 3;
                    }
                }
                else
                {
                    result.VerficationStatus = false;
                    //objResponse.VerficationMessage = ResponseMessages.NO_EMAIL_RECORD_FOUND;
                    result.RstKey = 4;
                }
            }
            return result;


        }

        public async Task<WalletUser> Login(UserLoginRequest request)
        {
            WalletUser response = new WalletUser();


            using (var db = new DB_9ADF60_ewalletEntities())
            {

                if (!string.IsNullOrEmpty(request.SecretKey))
                {
                    response = await db.WalletUsers.Where(x => x.EmailId == request.SecretKey.ToLower() || x.MobileNo == request.SecretKey).FirstOrDefaultAsync();
                }
            }
            return response;
        }

        public async Task<WalletUser> UpdateUserDetail(WalletUser request)
        {
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                db.Entry(request).State = EntityState.Modified;
                int s = await db.SaveChangesAsync();
                return request;
            }
        }

        public async Task<bool> Logout(string token)
        {
            bool IsSuccess = false;
            try
            {
                using (DB_9ADF60_ewalletEntities db = new DB_9ADF60_ewalletEntities())
                {
                    var user = await db.SessionTokens.Where(x => x.TokenValue == token).FirstOrDefaultAsync();
                    if (user != null)
                    {
                        user.IsDeleted = true;
                        db.SaveChanges();
                        IsSuccess = true;
                    }
                    else
                    {
                        IsSuccess = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("AppUserRepository.cs", "Logout");
            }
            return IsSuccess;
        }

        public async Task<bool> IsDocVerified(long WalletUserId, int documetStatus)
        {

            using (DB_9ADF60_ewalletEntities db = new DB_9ADF60_ewalletEntities())
            {
                var checkDoc = await (from s in db.CardPaymentRequests
                                      join sa in db.CardPaymentResponses on s.CardPaymentRequestId equals sa.CardPaymentRequestId
                                      where s.WalletUserId == WalletUserId
                                      select new { sa.vpc_AuthorizeId }).AnyAsync();

                if (checkDoc && (documetStatus == (int)DocumentStatus.NoDocuments || documetStatus == (int)DocumentStatus.Pending || documetStatus == (int)DocumentStatus.Rejected || documetStatus == (int)DocumentStatus.NotOk))
                {
                    return false;
                }
                else
                {
                    return true;
                }


            }

        }

        public async Task<bool> IsDocVerifiedMOMO(int documetStatus)
        {
            using (DB_9ADF60_ewalletEntities db = new DB_9ADF60_ewalletEntities())
            {

                if (documetStatus == (int)DocumentStatus.NoDocuments || documetStatus == (int)DocumentStatus.Pending || documetStatus == (int)DocumentStatus.Rejected || documetStatus == (int)DocumentStatus.NotOk)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }


        public async Task<UserDetailResponse> UserProfile(string TokenValue)
        {
            var response = new UserDetailResponse();
            try
            {
                if (!string.IsNullOrEmpty(TokenValue))
                {
                    // var status = IsDocVerified();

                    var AdminKeys = AES256.AdminKeyPair;
                    using (var db = new DB_9ADF60_ewalletEntities())
                    {
                        // var res = IsDocVerified();
                        response = db.Database.SqlQuery<UserDetailResponse>("exec usp_UserDetailByToken @TokenValue", new SqlParameter("@TokenValue", TokenValue)).Select(
                            x => new UserDetailResponse
                            {
                                WalletUserId = Convert.ToInt32(x.WalletUserId),
                                CurrentBalance = x.CurrentBalance,
                                EarnedPoints = x.EarnedPoints >= 0 ? x.EarnedPoints : 0,
                                EarnedAmount = x.EarnedAmount >= 0 ? x.EarnedAmount : 0,
                                FirstName = AES256.Decrypt(x.PrivateKey, x.FirstName),
                                LastName = AES256.Decrypt(x.PrivateKey, x.LastName),
                                StdCode = x.StdCode,
                                MobileNo = AES256.Decrypt(AdminKeys.PrivateKey, x.MobileNo),
                                EmailId = AES256.Decrypt(AdminKeys.PrivateKey, x.EmailId).Trim().ToLower(),
                                QrCode = x.StdCode + "," + AES256.Decrypt(AdminKeys.PrivateKey, x.MobileNo),// CommonSetting.S3ServiceURL + x.QrCode,
                                PrivateKey = x.PrivateKey,
                                PublicKey = x.PublicKey,
                                IsEmailVerified = x.IsEmailVerified,
                                IsMobileNoVerified = x.IsMobileNoVerified,
                                ProfileImage = x.ProfileImage,
                                IsNotification = x.IsNotification,
                                DeviceToken = x.DeviceToken,
                                DeviceType = x.DeviceType,
                                DocumetStatus = x.DocumetStatus,
                                IsDisabledTransaction = x.IsDisabledTransaction,
                                IsActive = x.IsActive,
                                UserType1 = x.UserType1
                            }
                            ).FirstOrDefault();
                        //firsft add monefy without kyc 
                        //var checkDoc = await db.CardPaymentRequests.AnyAsync(x => x.WalletUserId == response.WalletUserId);
                        //if (checkDoc && (response.DocumetStatus == (int)DocumentStatus.NoDocuments || response.DocumetStatus == (int)DocumentStatus.Pending || response.DocumetStatus == (int)DocumentStatus.Rejected || response.DocumetStatus == (int)DocumentStatus.NotOk))
                        //{
                        //    checkDoc = false;
                        //}
                        //else
                        //{
                        //    checkDoc = true;
                        //}
                        //change only check doc status ,first kyc then other 
                        //bool checkDoc;
                        //if (response.DocumetStatus == (int)DocumentStatus.NoDocuments ||
                        //    response.DocumetStatus == (int)DocumentStatus.Pending || 
                        //    response.DocumetStatus == (int)DocumentStatus.Rejected || 
                        //    response.DocumetStatus == (int)DocumentStatus.NotOk)
                        //{
                        //    checkDoc = false;
                        //}
                        //else
                        //{
                        //    checkDoc = true;
                        //}
                        //response.DocStatus = checkDoc;
                    }
                }
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("WalletUserRepository.cs", "UserProfile");
            }
            return response;
        }

        public async Task<WalletUser> GetCurrentUser(long walletId)
        {
            WalletUser response = new WalletUser();
            try
            {
                if (walletId > 0)
                {
                    // var status = IsDocVerified();

                    var AdminKeys = AES256.AdminKeyPair;
                    using (var db = new DB_9ADF60_ewalletEntities())
                    {
                        response = await db.WalletUsers.Where(x => x.WalletUserId == walletId).FirstOrDefaultAsync();
                    }
                }
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("WalletUserRepository.cs", "GetCurrentUser");
            }
            return response;
        }

        public async Task<WalletUser> GetUserDetailByMobile(string mobileNumber)
        {
            var response = new WalletUser();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    response = await db.WalletUsers.Where(x => x.StdCode + x.MobileNo == mobileNumber).FirstOrDefaultAsync();
                }
            }
            catch
            {

            }
            return response;
        }

        public async Task<UserDetailByQrCodeResponse> UserDetailById(UserDetailByQrCodeRequest request)
        {
            UserDetailByQrCodeResponse response = new UserDetailByQrCodeResponse();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    if (!string.IsNullOrEmpty(request.QrCode))
                    {
                        var user = await db.WalletUsers.Where(x => x.QrCode == request.QrCode).FirstOrDefaultAsync();
                        {
                            response.UserName = user.FirstName + (!string.IsNullOrEmpty(user.LastName) ? " " + user.LastName : "");
                        }
                    }
                    else if (!string.IsNullOrEmpty(request.MobileNo))
                    {
                        var user = db.WalletUsers.Where(x => x.MobileNo == request.MobileNo).FirstOrDefault();
                        {
                            response.UserName = user.FirstName + (!string.IsNullOrEmpty(user.LastName) ? " " + user.LastName : "");
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("WalletUserRepository.cs", "UserDetailById", request);
            }
            return response;

        }

        public async Task<WalletUser> GetWalletUserByUserType(int userType, long walletUserId)
        {
            var response = new WalletUser();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    response = await db.WalletUsers.Where(x => x.WalletUserId == walletUserId && x.UserType == userType).FirstOrDefaultAsync();
                }
            }
            catch
            {

            }
            return response;
        }

        public async Task<bool> DocumentUpload(DocumentUploadRequest request, long WalletUserId, string IdCard, string ATMCard)
        {
            var response = new Documentresponse();
            bool result = false;
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    var walletUser = await db.WalletUsers.Where(x => x.WalletUserId == WalletUserId).FirstOrDefaultAsync();
                    if (walletUser != null)
                    {
                        var usrDoc = db.UserDocuments.Where(x => x.WalletUserId == WalletUserId).FirstOrDefault();
                        if (usrDoc == null)
                        {
                            UserDocument userDocument = new UserDocument();
                            userDocument.WalletUserId = WalletUserId;
                            userDocument.IdProofImage = IdCard;
                            userDocument.CardImage = ATMCard;
                            userDocument.DocumentStatus = (int)DocumentStatus.Pending;
                            userDocument.CreateOn = DateTime.UtcNow;
                            userDocument.UpdatedOn = DateTime.UtcNow;
                            db.UserDocuments.Add(userDocument);
                            walletUser.DocumetStatus = (int)DocumentStatus.Pending;
                            db.SaveChanges();
                            result = true;
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(ATMCard))
                            {
                                usrDoc.CardImage = ATMCard;
                            }
                            if (!string.IsNullOrWhiteSpace(IdCard))
                            {
                                usrDoc.IdProofImage = IdCard;
                            }
                            walletUser.DocumetStatus = (int)DocumentStatus.Pending;
                            usrDoc.DocumentStatus = (int)DocumentStatus.Pending;
                            db.SaveChanges();
                            result = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("AppUserRepository.cs", "UpdateUserProfile", request);
                result = false;
            }
            return result;

        }

        public async Task<EmailVerification> InsertEmailVerification(EmailVerification request)
        {
            var result = new EmailVerification();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    result = db.EmailVerifications.Add(request);
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("WalletUserRepository.cs", "UserDetailById", request);
            }
            return request;

        }

        public async Task<bool> UpdateUserProfile(UpdateProfileRequest request, long WalletUserId)
        {
            var AdminKeys = AES256.AdminKeyPair;
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    // var _UserProfile = UserProfile();
                    var _walletUser = await db.WalletUsers.Where(x => x.WalletUserId == WalletUserId).FirstOrDefaultAsync();
                    if (_walletUser != null)
                    {
                        if (!string.IsNullOrEmpty(request.FirstName))
                        {
                            _walletUser.FirstName = AES256.Encrypt(_walletUser.PublicKey, request.FirstName);
                        }
                        if (!string.IsNullOrEmpty(request.LastName))
                        {
                            _walletUser.LastName = AES256.Encrypt(_walletUser.PublicKey, request.LastName);
                        }
                        if (!string.IsNullOrEmpty(request.EmailId))
                        {
                            _walletUser.EmailId = AES256.Encrypt(_walletUser.PublicKey, request.EmailId);
                        }
                        if (!string.IsNullOrEmpty(request.ProfileImage))
                        {
                            _walletUser.ProfileImage = request.ProfileImage;
                        }

                        db.Entry(_walletUser).State = EntityState.Modified;
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("WalletUserRepository.cs", "UpdateUserProfile", request);

                return false;

            }
        }

        public async Task<ChangePasswordResponse> ChangePassword(ChangePasswordRequest request, string TokenValue)
        {
            var response = new ChangePasswordResponse();
            try
            {
                var adminKeyPair = AES256.AdminKeyPair;
                //HttpRequestMessage httpRequestMessage = HttpContext.Current.Items["MS_HttpRequestMessage"] as HttpRequestMessage;               
                if (!string.IsNullOrEmpty(TokenValue))
                {
                    using (var db = new DB_9ADF60_ewalletEntities())
                    {
                        long WalletUserId = (long)await db.SessionTokens.Where(x => x.TokenValue == TokenValue).Select(x => x.WalletUserId).FirstOrDefaultAsync();
                        var user = db.WalletUsers.Where(x => x.WalletUserId == WalletUserId).FirstOrDefault();
                        //Matched with Email
                        if (user != null)
                        {
                            var hashedObject = SHA256ALGO.HashPasswordDecryption(request.CurrentPassword, user.HashedSalt);

                            if (hashedObject.HashedPassword == user.HashedPassword)
                            {
                                var hashedObjectNew = SHA256ALGO.HashPassword(request.NewPassword);
                                user.HashedPassword = hashedObjectNew.HashedPassword;
                                user.HashedSalt = hashedObjectNew.SlatBytes;
                                if (user.IsTemporaryPassword == true)
                                {
                                    user.IsTemporaryPassword = false;
                                    // user.OtpHashedSalt = null;
                                    //  user.Otp = string.Empty;
                                }
                                db.SaveChanges();
                                //response.IsSuccess = true;
                                //response.Message = "Password changed successfully.";
                                response.RstKey = 1;
                            }
                            else
                            {

                                if (!string.IsNullOrEmpty(user.Otp) && user.OtpHashedSalt != null)
                                {
                                    hashedObject = SHA256ALGO.HashPasswordDecryption(request.CurrentPassword, user.OtpHashedSalt);
                                    if (hashedObject.HashedPassword == user.Otp)
                                    {

                                        var hashedObjectNew = SHA256ALGO.HashPassword(request.NewPassword);
                                        user.HashedPassword = hashedObjectNew.HashedPassword;
                                        user.HashedSalt = hashedObjectNew.SlatBytes;
                                        if (user.IsTemporaryPassword == true)
                                        {
                                            user.IsTemporaryPassword = false;
                                            user.OtpHashedSalt = null;
                                            user.Otp = string.Empty;
                                        }
                                        db.SaveChanges();
                                        //response.IsSuccess = true;
                                        //response.Message = "Password changed successfully.";
                                        response.RstKey = 1;
                                        //new TokenRepository().SendLogoutPush(WalletUserId);

                                    }
                                    else
                                    {
                                        response.RstKey = 2;
                                        //response.Message = "Please enter correct old password.";
                                    }
                                }
                                else
                                {
                                    // response.Message = "Please enter correct old password.";
                                    response.RstKey = 2;
                                }
                            }

                        }
                        else
                        {
                            //Email id not exist


                        }
                    }
                }
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("UserProfileRepository.cs", "ChangePassword", request);
            }
            return response;

        }

        public async Task<bool> ForgotPassword(ForgotPasswordRequest request, string Otp)
        {
            var adminKeyPair = AES256.AdminKeyPair;
            var emailId = AES256.Encrypt(adminKeyPair.PublicKey, request.EmailId.Trim().ToLower());
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    var user = await db.WalletUsers.Where(x => x.EmailId == emailId).FirstOrDefaultAsync();
                    //Matched with Email
                    if (user != null)
                    {
                        // string Otp = Common.Password();
                        var hashedObject = SHA256ALGO.HashPassword(Otp);
                        user.Otp = hashedObject.HashedPassword;
                        user.OtpHashedSalt = hashedObject.SlatBytes;
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("AppUserRepository.cs", "ForgotPassword", request);
                if (ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message))
                {
                    ex.InnerException.Message.ErrorLog("AppUserRepository.cs", "ForgotPassword", request);
                }
                return false;
            }
        }

        public async Task<IsFirstTransactionResponse> IsFirstTransaction(long userId)
        {
            var response = new IsFirstTransactionResponse();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    response = await db.Database.SqlQuery<IsFirstTransactionResponse>("exec usp_IsFirstTransaction @UserId",

                        new object[]
                        {
                            new SqlParameter("@UserId",userId)
                        }
                        ).FirstOrDefaultAsync();
                }
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("AppUserRepository.cs", "IsFirstTransaction");
            }
            return response;

        }

        public async Task<SessionInfoResponse> GetWalletSessionInfo(long walletUserId)
        {
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                return await db.WalletUsers.Where(x => x.WalletUserId == walletUserId
                     && x.IsActive == true && x.IsDeleted == false && x.IsEmailVerified == true
                     && x.IsOtpVerified == true)
                    .Select(x => new SessionInfoResponse
                    {
                        CurrentBalance = x.CurrentBalance
                    }).FirstOrDefaultAsync();
            }
        }

        public async Task<WalletUser> GetWalletUser(long walletUserId)
        {
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                return await db.WalletUsers.Where(x => x.WalletUserId == walletUserId
                     && x.IsActive == true && x.IsDeleted == false && x.IsEmailVerified == true
                     && x.IsOtpVerified == true).FirstOrDefaultAsync();
            }
        }

        public async Task<int> InsertWalletTransaction(WalletTransaction transEntity)
        {
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                db.WalletTransactions.Add(transEntity);
                return await db.SaveChangesAsync();

            }
        }

        public async Task<int> SaveData(ShareAndEarnDetail shareAndEarnDetail)
        {
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                db.ShareAndEarnDetails.Add(shareAndEarnDetail);
                return await db.SaveChangesAsync();
            }
        }

        public async Task<ApiKeysResponse> GetApiKeysData(long walletUserId)
        {
            var res = new ApiKeysResponse();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                var data = await db.UserApiKeys.Where(x => x.WalletUserId == walletUserId).FirstOrDefaultAsync();
                if (data != null)
                {
                    res.WalletUserId = (long)data.WalletUserId;
                    res.ApiKey = data.ApiKey;
                    res.MerchantKey = data.MerchantKey;
                }
            }
            return res;
        }

        public async Task<WalletUser> GetCurrentUserByEmailId(string emailid)
        {
            WalletUser response = new WalletUser();
            try
            {
                if (emailid != null)
                {
                    // var status = IsDocVerified();

                    var AdminKeys = AES256.AdminKeyPair;
                    using (var db = new DB_9ADF60_ewalletEntities())
                    {
                        response = await db.WalletUsers.Where(x => x.EmailId == emailid).FirstOrDefaultAsync();
                    }
                }
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("WalletUserRepository.cs", "GetCurrentUser");
            }
            return response;
        }

        public async Task<UserApiKey> GetMerchantApiKey(string apkiKey, string merchantKey)
        {
            var response = new UserApiKey();
            try
            {
                if (apkiKey != null && merchantKey != null)
                {
                    using (var db = new DB_9ADF60_ewalletEntities())
                    {
                        response = await db.UserApiKeys.Where(x => x.ApiKey == apkiKey && x.MerchantKey == merchantKey).FirstOrDefaultAsync();
                    }
                }
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("WalletUserRepository.cs", "GetCurrentUser");
            }
            return response;
        }

        public async Task<WalletUser> Authentication(AuthenticationRequest request)
        {
            WalletUser response = new WalletUser();


            using (var db = new DB_9ADF60_ewalletEntities())
            {

                if (!string.IsNullOrEmpty(request.emailMobile))
                {
                    response = await db.WalletUsers.Where(x => x.EmailId == request.emailMobile.ToLower() || x.MobileNo == request.emailMobile).FirstOrDefaultAsync();
                }
            }
            return response;
        }

        public async Task<ShareAndEarnDetail> GetReferalUrl(long request)
        {
            var res = new ShareAndEarnDetail();
            try
            {
                
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                  
                    res = await db.ShareAndEarnDetails.Where(x => x.SenderId == request).FirstOrDefaultAsync();
                }
            }
            catch (Exception ex)
            {

            }
            return res;
        }


        public async Task<int> SaveUserReferalData(UserReferalWallet request)
        {

            using (var db = new DB_9ADF60_ewalletEntities())
            {
                db.UserReferalWallets.Add(request);
                return await db.SaveChangesAsync();
            }
        }

        public async Task<int> InsertEarnedHistory(RedeemPointsHistory transEntity)
        {
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                db.RedeemPointsHistories.Add(transEntity);
                return await db.SaveChangesAsync();

            }
        }

        public async Task<AddCashDepositToBankResponse> AddCashDepositToBankServices(AddCashDepositToBankRequest Request)
        {
            var response = new AddCashDepositToBankResponse();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    response = await db.Database.SqlQuery<AddCashDepositToBankResponse>
                        ("exec usp_UpdateCashDepositToBank @DepositorCashAmount,@DepositorCountry,@WalletUserId,@DepositorName,@DepositorSlipImage,@TotalDepositorAmount",
                     new SqlParameter("@DepositorCashAmount", Request.DepositorCashAmount),
                     new SqlParameter("@DepositorCountry", Request.DepositorCountry),
                     new SqlParameter("@WalletUserId", Request.WalletUserId),
                     new SqlParameter("@DepositorName", Request.DepositorName),
                     new SqlParameter("@DepositorSlipImage", Request.DepositorSlipImage),
                     new SqlParameter("@TotalDepositorAmount", Request.TotalDepositorAmount)

                     ).FirstOrDefaultAsync();
                }
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("WalletUserRepository.cs", "AddCashDepositToBankServices");
            }
            return response;

        }

        public UserDetailResponse GetUserProfileForTransaction(string TokenValue)
        {
            UserDetailResponse response = new UserDetailResponse();
            try
            {
                if (!string.IsNullOrEmpty(TokenValue))
                {
                    // var status = IsDocVerified();

                    var AdminKeys = AES256.AdminKeyPair;
                    using (var db = new DB_9ADF60_ewalletEntities())
                    {
                        // var res = IsDocVerified();
                        response = db.Database.SqlQuery<UserDetailResponse>("exec usp_UserDetailByToken @TokenValue", new SqlParameter("@TokenValue", TokenValue)).Select(
                            x => new UserDetailResponse
                            {
                                WalletUserId = Convert.ToInt32(x.WalletUserId),
                                CurrentBalance = x.CurrentBalance,
                                EarnedPoints = x.EarnedPoints >= 0 ? x.EarnedPoints : 0,
                                EarnedAmount = x.EarnedAmount >= 0 ? x.EarnedAmount : 0,
                                FirstName = AES256.Decrypt(x.PrivateKey, x.FirstName),
                                LastName = AES256.Decrypt(x.PrivateKey, x.LastName),
                                StdCode = x.StdCode,
                                MobileNo = AES256.Decrypt(AdminKeys.PrivateKey, x.MobileNo),
                                EmailId = AES256.Decrypt(AdminKeys.PrivateKey, x.EmailId).Trim().ToLower(),
                                QrCode = x.StdCode + "," + AES256.Decrypt(AdminKeys.PrivateKey, x.MobileNo),// CommonSetting.S3ServiceURL + x.QrCode,
                                PrivateKey = x.PrivateKey,
                                PublicKey = x.PublicKey,
                                IsEmailVerified = x.IsEmailVerified,
                                IsMobileNoVerified = x.IsMobileNoVerified,
                                ProfileImage = x.ProfileImage,
                                IsNotification = x.IsNotification,
                                DeviceToken = x.DeviceToken,
                                DeviceType = x.DeviceType,
                                DocumetStatus = x.DocumetStatus,
                                IsDisabledTransaction = x.IsDisabledTransaction,
                            }
                            ).FirstOrDefault();
                    }
                }
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("WalletUserRepository.cs", "UserProfile");
            }
            return response;
        }



        public async Task<OtpResponse> WalletSendOtp(SendOtpRequest request, long userId)
        {
            string customer = request.MobileNo.Substring(0, 1);

            if (request.IsdCode != "+225") //civ con.change 
            {
                if (customer == "0")
                {
                    customer = request.MobileNo.Remove(0, 1);
                }
                else
                {
                    customer = request.MobileNo;
                }
            }
            else
            {
                customer = request.MobileNo;
            }

            OtpResponse response = new OtpResponse();
            try
            {

                if (!string.IsNullOrEmpty(request.Otp))
                {

                    using (var db = new DB_9ADF60_ewalletEntities())
                    {
                        DateTime time = DateTime.Now;

                        //count verify mobile_no. of a user

                        var CountVerifiedMobileNo1 = await db.WalletMoMoOTPs.Where(x => x.WalletUserId == userId
                                                           && x.IsOtpVerified == true).Select(x => x.OtpCounter).ToListAsync();




                        var VerifiedMobileNoExistCheck = await db.WalletMoMoOTPs.Where(x => x.WalletUserId == userId 
                                                           && x.IsOtpVerified == true && x.MobileNo == customer && x.IsdCode == request.IsdCode).FirstOrDefaultAsync();


                        if (VerifiedMobileNoExistCheck != null) //per user 
                        {
                            response.IsSuccess = false;
                            response.Message = ResponseMessages.Verify_already_mobileno;
                            response.StatusCode = 7;
                        }
                        else if (CountVerifiedMobileNo1.Sum() >= 3) //per user 
                        {
                            response.IsSuccess = false;
                            response.Message = ResponseMessages.Verify_mobileno_SENT_RANGE;
                            response.StatusCode = 5;
                        }                        
                        else
                        {
                            //uss din ke user ke counter check kregein & yeh bhi dekna hoga ki kitne verified + unverifeid = otp counter
                            //per day /user

                            var CountCheck = await db.WalletMoMoOTPs.Where(x => x.WalletUserId == userId && x.IsOtpVerified == false
                            && DbFunctions.TruncateTime(x.CreatedDate) == time.Date).Select(x => x.OtpCounter).ToListAsync();

                            
                            var calci1 = CountVerifiedMobileNo1.Sum() + CountCheck.Sum();
                            if (calci1 >= 3) // otp check by user send verified + unverified = 3 otp attempt
                            {
                                response.IsSuccess = false;
                                response.Message = ResponseMessages.OTP_Limit_OVER;
                                response.StatusCode = 6;

                            }

                            else
                            {
                                string MobileNo = (request.IsdCode + customer).ToString().Trim();
                                string isdCode = request.IsdCode.Remove(0, 1);

                                // var modelTeleSign = new SendOtpTeleSignRequest { phone_number = isdCode + customer, verify_code = request.Otp, template = ResponseMessages.OTP_MESSAGE + " " + request.Otp + " ID: " + CommonSetting.AutoReadOtpId };
                                // var model = new SendMessageRequest { MobileNo = customer, ISD = request.IsdCode, Message = "<#> " + ResponseMessages.OTP_MESSAGE + request.Otp + " ID: " + CommonSetting.AutoReadOtpId };

                                var model = new SendMessageRequest { MobileNo = customer, ISD = isdCode, Message = ResponseMessages.OTP_MESSAGE + request.Otp + " ID: " + CommonSetting.AutoReadOtpId };
                                var OtpExist = await db.WalletMoMoOTPs.Where(x => x.MobileNo == customer && x.IsdCode == request.IsdCode && x.WalletUserId == userId && DbFunctions.TruncateTime(x.CreatedDate) == time.Date).FirstOrDefaultAsync();
                                if (OtpExist != null) //ek no. pe 1 otp ya 3 otp per user per day
                                {

                                    if (OtpExist.OtpCounter != CommonSetting.OtpLimit)
                                    {
                                        OtpExist.Otp = request.Otp;
                                        OtpExist.OtpCounter = OtpExist.OtpCounter + 1;

                                        var otpResponse = new SendMessage().SendMessgeWithISDCode(model);

                                        if (otpResponse)
                                        {
                                            db.SaveChanges();
                                            response.IsSuccess = true;
                                            response.Message = ResponseMessages.OTP_SENT;
                                            response.StatusCode = (int)WalletTransactionStatus.SUCCESS;
                                        }
                                        else
                                        {
                                            response.IsSuccess = false;
                                            response.Message = ResponseMessages.OTP_NOT_SENT;
                                            response.StatusCode = (int)WalletTransactionStatus.FAILED;
                                        }

                                    }
                                    else
                                    {
                                        response.IsSuccess = false;
                                        response.Message = ResponseMessages.OTP_Limit_OVER;
                                        response.StatusCode = 6;
                                    }
                                }
                                else
                                {
                                    var OtpNew = new WalletMoMoOTP();

                                    OtpNew.CreatedDate = DateTime.UtcNow;
                                    OtpNew.IsdCode = request.IsdCode;
                                    OtpNew.MobileNo = customer;
                                    OtpNew.OtpCounter = 1;
                                    OtpNew.Otp = request.Otp;
                                    OtpNew.WalletUserId = userId;
                                    OtpNew.IsOtpVerified = false;

                                    var otpResponse = new SendMessage().SendMessgeWithISDCode(model);

                                    if (otpResponse)
                                    {
                                        db.WalletMoMoOTPs.Add(OtpNew);
                                        db.SaveChanges();

                                        response.IsSuccess = true;
                                        response.Message = ResponseMessages.OTP_SENT;
                                        response.StatusCode = (int)WalletTransactionStatus.SUCCESS;
                                    }
                                    else
                                    {
                                        response.IsSuccess = false;
                                        response.Message = ResponseMessages.OTP_NOT_SENT;
                                        response.StatusCode = (int)WalletTransactionStatus.FAILED;
                                    }

                                }
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("WalletUserRepository.cs", "WalletSendOtp", request);
            }
            return response;
        }

        public async Task<UserExistanceResponse> WalletVerifyOtp(VerifyOtpRequest request, long userId)
        {
            UserExistanceResponse response = new UserExistanceResponse();

            try
            {
                string customer = request.MobileNo.Substring(0, 1);

                if (request.IsdCode != "+225") //civ con.change 
                {
                    if (customer == "0")
                    {
                        customer = request.MobileNo.Remove(0, 1);
                    }
                    else
                    {
                        customer = request.MobileNo;
                    }
                }
                else
                {
                    customer = request.MobileNo;
                }
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    string MobileNo = (request.IsdCode + customer).ToString().Trim();
                    var res = await db.WalletMoMoOTPs.AnyAsync(x => x.MobileNo == customer && x.IsdCode == request.IsdCode && x.WalletUserId == userId && x.Otp == request.Otp);
                    if (!res)
                    {
                        response.RstKey = 2;
                    }
                    else
                    {
                        response.RstKey = 1;

                    }
                    //update walletuser table with isdcode & mobile 
                    if (response.RstKey == 1)
                    {
                        var IsExist = await db.WalletMoMoOTPs.Where(x => x.MobileNo == customer && x.IsdCode == request.IsdCode && x.WalletUserId == userId).FirstOrDefaultAsync();

                        if (!string.IsNullOrEmpty(IsExist.MobileNo)) //not empty mobile no
                        {
                            IsExist.IsOtpVerified = true;
                            db.SaveChanges();
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("WalletUserRepository.cs", "WalletVerifyOtp", request);
                response.RstKey = 2;

            }
            return response;
        }

        public async Task<List<MobileNoListResponse>> GetMobileNoList(long Walletuserid)
        {
            var list = new List<MobileNoListResponse>();

            using (var context = new DB_9ADF60_ewalletEntities())
            {

                //first take current user id submitted
                list = await (from e in context.WalletMoMoOTPs
                              where e.WalletUserId == Walletuserid && e.IsOtpVerified == true
                              select new MobileNoListResponse
                              {
                                  WalletUserId = e.WalletUserId,
                                  MobileNo = e.MobileNo

                              }).ToListAsync();


                return list;
                // return null;
            }



        }



        public async Task<balance161022> GetUserbalancefreezeById(long WalletUserId = 0)
        {

            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    if (WalletUserId > 0)
                    {

                        var user = await db.balance161022.Where(x => x.walletuserid == WalletUserId).FirstOrDefaultAsync();
                        if (user != null)
                        {

                            user.currentbalance = user.currentbalance;


                            return user;
                        }
                        else
                        {
                            return null;
                        }

                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {

                //ex.Message.ErrorLog("AppUserRepository.cs", "UserDetail");
                return null;
            }


        }
    }
}
