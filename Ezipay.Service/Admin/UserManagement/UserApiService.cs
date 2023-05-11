using ezeePay.Utility.CommonClass;
using ezeePay.Utility.Enums;
using Ezipay.Database;
using Ezipay.Repository.AdminRepo;
using Ezipay.Repository.PushNotificationRepo;
using Ezipay.Repository.TokenRepo;
using Ezipay.Repository.UserRepo;
using Ezipay.Service.UserService;
using Ezipay.Utility.common;
using Ezipay.Utility.SendEmail;
using Ezipay.Utility.SendPush;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.CheckPasswordViewModel;
using Ezipay.ViewModel.SendEmailViewModel;
using Ezipay.ViewModel.SendPushViewModel;
using Ezipay.ViewModel.WalletUserVM;
using EziPay.AWSUtils;
using Newtonsoft.Json;
using NPOI.HPSF;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Ezipay.Service.AdminService
{
    public class UserApiService : IUserApiService
    {
        private IUserApiRepository _userApiRepository;
        private ITokenRepository _tokenRepository;
        private ISendPushNotification _sendPushNotification;
        private IWalletUserRepository _walletUserRepository;
       // private IWalletUserService _walletUserService;
        private IS3Uploader _iS3Uploader;
        private HSSFWorkbook _hssfWorkbook;
        private ISendEmails _sendEmails;

        public UserApiService()
        {
            _userApiRepository = new UserApiRepository();
            _tokenRepository = new TokenRepository();
            _sendPushNotification = new SendPushNotification();
            _walletUserRepository = new WalletUserRepository();
           // _walletUserService = new WalletUserService();
            _iS3Uploader = new S3Uploader();
            _sendEmails = new SendEmails();
        }

        public async Task<UserListResponse> UserList(UserListRequest request)
        {
            return await _userApiRepository.UserList(request);
        }

        public async Task<int> EnableDisableUser(UserManageRequest request)
        {
            int response = 0;
            if (request.Status == 1)
            {
                var userData = await _walletUserRepository.GetUserDetailById(request.UserId);
                if (userData != null)
                {
                    var i = await _userApiRepository.SaveBlockUnblockDetails(request);
                    if (i == 1)
                    {
                        var result = await _userApiRepository.EnableDisableUser(request);
                        if (!request.IsActive)
                        {
                            var req = new SendPushRequest
                            {
                                DeviceToken = userData.DeviceToken,
                                DeviceType = userData.DeviceType,
                                MobileNo = userData.MobileNo,
                                WalletUserId = userData.WalletUserId
                            };
                            //var repo = new TokenRepository();
                            if (result == true)
                            {
                                response = 1;
                            }
                            else
                            {
                                response = 2;
                            }
                            _tokenRepository.RemoveLoginSession(request.UserId);
                            _sendPushNotification.SendLogoutPush(req, userData.DeviceToken);
                            //
                            try
                            {
                                //--------send mail--------                                                      
                                var body = _sendEmails.ReadEmailformats("block.html");
                                body = body.Replace("$$FirstLastName$$", userData.FirstName + " " + userData.LastName);

                                var req1 = new EmailModel
                                {
                                    TO = userData.EmailId,
                                    Subject = "Account Block",
                                    Body = body
                                };
                                _sendEmails.SendEmail(req1);

                            }
                            catch
                            {

                            }

                            //when user is in ChargeBack in list admin panel
                            if (request.AdminId == 0 && request.Flag != null)
                            {
                                try
                                {
                                    //--------send mail--------                                                      
                                    var body = _sendEmails.ReadEmailformats("Chargeblock.html");
                                    body = body.Replace("$$EmailID$$", userData.EmailId);
                                    body = body.Replace("$$WalletID$$", userData.WalletUserId.ToString());
                                    body = body.Replace("$$ChargebackAdminID$$", request.Flag);
                                    var req1 = new EmailModel
                                    {
                                         TO = "support@ezipaysarl.com",
                                        //TO = userData.EmailId, //test
                                        Subject = "SARL Account Blocked Chargeback",
                                        Body = body
                                    };
                                    _sendEmails.SendEmail(req1);

                                }
                                catch
                                {

                                }


                            }

                        }
                        else
                        {
                            response = 2;

                        }
                    }
                    else
                    {
                        response = 0;
                    }
                }
                else
                {
                    response = 0;
                }
            }
            else if (request.Status == 2)
            {

                var result = await _userApiRepository.Delete(request);
                if (result == true)
                {
                    response = 3;
                }
                else
                {
                    response = 2;
                }
            }
            return response;
        }

        //public async Task<bool> Delete(UserDeleteRequest request)
        //{
        //    bool objResponse = false;

        //    objResponse = await _userApiRepository.Delete(request);
        //    if (objResponse == true)
        //    {
        //        objResponse = true;
        //    }

        //    return objResponse;
        //}

        public async Task<UserEmailVerifyResponse> VerfiyByEmailId(string token)
        {

            var objResponse = new UserEmailVerifyResponse();
            objResponse = await _userApiRepository.VerfiyByEmailId(token);
            long EmailVerificationId = Convert.ToInt64(token.Split('_')[1]);
            if (objResponse != null)
            {
                if (objResponse != null && objResponse != null)
                {
                    if (objResponse.VerficationStatus == false)
                    {
                        objResponse.VerficationStatus = false;
                        objResponse.VerficationMessage = ResponseMessages.ALREADY_VERIFIED;
                    }
                    else
                    {
                        objResponse.VerficationStatus = true;
                        objResponse.VerficationMessage = ResponseMessages.VERFIED_SUCCESSFULLY;
                    }
                }
            }
            else
            {
                objResponse.VerficationStatus = false;
                objResponse.VerficationMessage = ResponseMessages.NO_EMAIL_RECORD_FOUND;
            }

            return objResponse;
        }

        public async Task<ViewUserTransactionResponse> UserTransactions(ViewUserTransactionRequest request)
        {
            var objResponse = new ViewUserTransactionResponse();
            try
            {
                objResponse = await _userApiRepository.UserTransactions(request);
            }
            catch (Exception ex)
            {

            }

            return objResponse;
        }

        public async Task<CreditDebitResponse> CreditDebitUserAccount(CreditDebitRequest request)
        {
            var objResponse = new CreditDebitResponse();

            if (request != null)
            {

                if (request.CashDepositFlag == "CashDeposit")
                {
                    objResponse = await _userApiRepository.CreditDebitUserAccount(request, (int)WalletTransactionSubTypes.Cash_Deposit_To_Bank, (int)WalletUserTypes.AdminUser);
                }
                else
                {
                    objResponse = await _userApiRepository.CreditDebitUserAccount(request, (int)WalletTransactionSubTypes.EWallet_To_Ewallet_Transactions_PayMoney, (int)WalletUserTypes.AdminUser);
                }

                if (objResponse.RstKey == 1)
                {
                    objResponse.RstKey = (int)TransactionStatus.Completed;
                }

            }
            else
            {
                objResponse.RstKey = (int)TransactionStatus.Failed;
            }
            #region PushNotification

            if (objResponse.RstKey == (int)TransactionStatus.Completed)
            {
                // var ReceiverDetail = db.WalletUsers.Where(x => x.WalletUserId == request.UserId).FirstOrDefault();
                var ReceiverDetail = await _walletUserRepository.GetCurrentUser(request.UserId);
                if (ReceiverDetail != null && ReceiverDetail.WalletUserId > 0)
                {
                    var adminKeyPair = AES256.AdminKeyPair;
                    var ReceiverFirstName = AES256.Decrypt(ReceiverDetail.PrivateKey, ReceiverDetail.FirstName);
                    var ReceiverLastName = AES256.Decrypt(ReceiverDetail.PrivateKey, ReceiverDetail.LastName);
                    var ReceiverMobileNo = AES256.Decrypt(adminKeyPair.PrivateKey, ReceiverDetail.MobileNo);
                    var ReceiverEmail = AES256.Decrypt(adminKeyPair.PrivateKey, ReceiverDetail.EmailId);
                    var ReceiverId = (int)ReceiverDetail.WalletUserId;



                    var pushModel = new CreditDebitUpdateModel();
                    if (request.TransactionType)
                    {
                        pushModel.alert = request.Amount + " XOF has been credited to your account by Admin";
                        pushModel.pushType = (int)PushType.ADDMONEY;
                    }
                    else
                    {
                        pushModel.alert = request.Amount + " XOF has been debited to your account by Admin";
                        pushModel.pushType = (int)PushType.PAYMONEY;
                    }

                    pushModel.Amount = Convert.ToString(Math.Round(request.Amount, 2));

                    pushModel.CurrentBalance = Convert.ToString(Math.Round(Convert.ToDecimal(ReceiverDetail.CurrentBalance), 2));

                    pushModel.MobileNo = ReceiverMobileNo;
                    pushModel.SenderName = "Admin";


                    PushNotificationModel push = new PushNotificationModel();
                    push.deviceType = (int)ReceiverDetail.DeviceType;

                    push.SenderId = ReceiverDetail.WalletUserId;

                    if ((int)ReceiverDetail.DeviceType == (int)DeviceTypes.ANDROID || (int)ReceiverDetail.DeviceType == (int)DeviceTypes.Web)
                    {
                        push.deviceKey = ReceiverDetail.DeviceToken;
                        PushPayload<CreditDebitUpdateModel> aps = new PushPayload<CreditDebitUpdateModel>();
                        PushPayloadData<CreditDebitUpdateModel> _data = new PushPayloadData<CreditDebitUpdateModel>();
                        _data.notification = pushModel;
                        aps.data = _data;
                        aps.to = ReceiverDetail.DeviceToken;
                        aps.collapse_key = string.Empty;
                        push.payload = pushModel;
                        push.message = JsonConvert.SerializeObject(aps);

                    }
                    if ((int)ReceiverDetail.DeviceType == (int)DeviceTypes.IOS)
                    {
                        NotificationJsonResponse<PayMoneyIOSPushModel> aps = new NotificationJsonResponse<PayMoneyIOSPushModel>();
                        PayMoneyIOSPushModel _iosPushModel = new PayMoneyIOSPushModel();
                        _iosPushModel.alert = pushModel.alert;
                        _iosPushModel.Amount = pushModel.Amount;
                        _iosPushModel.CurrentBalance = pushModel.CurrentBalance;
                        _iosPushModel.MobileNo = pushModel.MobileNo;
                        _iosPushModel.SenderName = pushModel.SenderName;
                        _iosPushModel.pushType = 0;
                        aps.aps = _iosPushModel;
                        push.deviceKey = ReceiverDetail.DeviceToken.ToLower();
                        push.payload = _iosPushModel;
                        push.message = JsonConvert.SerializeObject(aps);
                    }

                    _sendPushNotification.sendPushNotification(push);
                }
            }
            //else
            //{
            //    objResponse.RstKey = (int)TransactionStatus.Failed;
            //}
            #endregion
            return objResponse;
        }

        public async Task<bool> ManageTransaction(SetTransactionLimitRequest request)
        {
            return await _userApiRepository.ManageTransaction(request);
        }

        public async Task<UserTransactionLimitDetailsResponse> GetTransactionLimitDetails(UserDetailsRequest request)
        {
            var response = new UserTransactionLimitDetailsResponse();

            try
            {
                response = await _userApiRepository.GetTransactionLimitDetails(request);
            }
            catch (Exception ex)
            {

            }
            return response;
        }

        public async Task<UserDetailsResponse> UserDetails(UserDetailsRequest request)
        {
            var response = new UserDetailsResponse();

            try
            {
                response = await _userApiRepository.UserDetails(request);
            }
            catch (Exception ex)
            {

            }
            return response;
        }

        public async Task<DownloadReportResponse> DownloadReportWithData(DownloadReportApiRequest request)
        {
            return await _userApiRepository.DownloadReportWithData(request);
        }

        public async Task<UserListResponse> DeletedUserList(UserListRequest request)
        {
            var response = new UserListResponse();

            try
            {
                response.UserList = await _userApiRepository.DeletedUserList(request);
                if (response.UserList != null && response.UserList.Count > 0)
                {
                    response = new UserListResponse
                    {
                        TotalCount = response.UserList.FirstOrDefault().TotalCount,
                        UserList = response.UserList,

                    };
                }
                else
                {
                    response.UserList = new List<UserList>();
                }
            }
            catch (Exception ex)
            {

            }
            return response;
        }

        public async Task<string> SaveImage(HttpPostedFileBase image, string PreviousImage)
        {
            UserDetailResponse response = new UserDetailResponse();

            string imageName = "";
            bool IsSuccess = false;

            try
            {
                if (image != null)
                {

                    string[] ImageArrayName = image.FileName.Split('.');
                    string ext = ImageArrayName[1];
                    imageName = Guid.NewGuid().ToString("n") + "." + ext;
                    Stream requestStream = image.InputStream;
                    string bucketFolderName = ConfigurationManager.AppSettings["AWSBucket"];

                    IsSuccess = await _iS3Uploader.UploadImage(requestStream, imageName, bucketFolderName);

                }
                else
                {

                }
            }
            catch (Exception ex)
            {
            }
            return IsSuccess ? imageName : "";
        }

        public async Task<bool> SaveUserDocument(DocumentUploadRequest request, int type)
        {
            var adminKeyPair = AES256.AdminKeyPair;
            string ATMCard = AES256.Encrypt(adminKeyPair.PublicKey, request.ATMCard.Trim());
            string IdCard = AES256.Encrypt(adminKeyPair.PublicKey, request.IdCard.Trim());
            bool result = false;

            var walletUser = await _userApiRepository.GetUserById(request.UserId);
            if (walletUser != null)
            {
                var usrDoc = await _userApiRepository.GetUserDocumentByUserId(request.UserId);
                if (usrDoc == null)
                {
                    var entity = new UserDocument
                    {
                        WalletUserId = request.UserId,
                        IdProofImage = IdCard,
                        CardImage = ATMCard,
                        DocumentStatus = type == 1 ? (int)DocumentStatus.Pending : (int)DocumentStatus.Verified,
                        CreateOn = DateTime.UtcNow,
                        UpdatedOn = DateTime.UtcNow,
                    };
                    int res = await _userApiRepository.InsertDocument(entity);
                    if (res > 0)
                    {
                        walletUser.DocumetStatus = type == 1 ? (int)DocumentStatus.Pending : (int)DocumentStatus.Verified;
                        int res2 = await _userApiRepository.UpdateUser(walletUser);
                        result = true;
                    }
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

                    usrDoc.DocumentStatus = type == 1 ? (int)DocumentStatus.Pending : (int)DocumentStatus.Verified;
                    usrDoc.UpdatedOn = DateTime.UtcNow;

                    int rowAffected = await _userApiRepository.UpdateDocument(usrDoc);
                    if (rowAffected > 0)
                    {
                        walletUser.DocumetStatus = type == 1 ? (int)DocumentStatus.Pending : (int)DocumentStatus.Verified;
                        await _userApiRepository.UpdateUser(walletUser);
                        result = true;
                    }
                }
            }
            return result;
        }

        public async Task<UserDocumentDetailsResponse> ViewDocumentDetails(UserDetailsRequest request)
        {
            var result = new UserDocumentDetailsResponse();
            var adminKeys = AES256.AdminKeyPair;
            if (request.UserId > 0)
            {
                var data = await _userApiRepository.GetUserDocuments(request.UserId);
                var docData = await _userApiRepository.GetUserById(request.UserId);
                result.WalletUserId = docData.WalletUserId;
                if (data != null)
                {
                    result.CardImage = AES256.Decrypt(adminKeys.PrivateKey, data.CardImage);
                    result.IdProofImage = AES256.Decrypt(adminKeys.PrivateKey, data.IdProofImage);
                    result.DocumentStatus = data.DocumentStatus;
                    result.WalletUserId = data.WalletUserId;
                }
                else if (docData != null) //take document status from walletuser
                {
                    result.DocumentStatus = docData.DocumetStatus ?? 0;

                }

                if (docData != null && docData.UserType == (int)WalletUserTypes.Merchant)
                {
                    result.Documents = await _userApiRepository.GetMerchantDocuments(docData.WalletUserId);
                    //result.Documents.ForEach(x =>
                    //{
                    //    x.DocName = ConfigurationManager.AppSettings["AWSurl"] + "/"
                    //    + ConfigurationManager.AppSettings["AWSBucket"] + "/" + x.DocName;
                    //});
                }
            }
            return result;
        }

        public async Task<int> ChangeUserDocumentStatus(DocumentChangeRequest request)
        {
            int result = 0;
            var walletUser = await _userApiRepository.GetUserById(request.UserId);
            var usrDoc = await _userApiRepository.GetUserDocumentByUserId(request.UserId);
            //var status = usrDoc != null ? usrDoc.DocumentStatus: 0;
            if (walletUser != null || usrDoc != null)
            {
                if (usrDoc != null && (usrDoc.DocumentStatus == (int)DocumentStatus.Pending || usrDoc.DocumentStatus == (int)DocumentStatus.Verified || usrDoc.DocumentStatus == (int)DocumentStatus.NoDocuments))
                {
                    walletUser.DocumetStatus = request.Status;
                    usrDoc.DocumentStatus = request.Status;

                    int rowAffected = await _userApiRepository.UpdateDocument(usrDoc);
                    int rowAffected2 = await _userApiRepository.UpdateUser(walletUser);
                    if (rowAffected > 0 && rowAffected2 > 0)
                    {
                        result = 1;
                    }
                }
                else if (walletUser.DocumetStatus == (int)DocumentStatus.NoDocuments || walletUser.DocumetStatus == (int)DocumentStatus.Verified && usrDoc == null)
                {
                    //this condition for direct approved
                    walletUser.DocumetStatus = request.Status;
                    int rowAffected2 = await _userApiRepository.UpdateUser(walletUser);
                    if (rowAffected2 > 0)
                    {
                        result = 1;
                    }
                    else
                    {
                        result = 2;
                    }
                }
                else
                {
                    result = 2;
                }
            }
            #region Push notification

            if (walletUser.WalletUserId > 0 && walletUser.DeviceToken != null && request.Status != 0 && request.Status != 1)
            {
                PayMoneyPushModel pushModel = new PayMoneyPushModel();
                pushModel.TransactionDate = DateTime.UtcNow;
                pushModel.TransactionId = "";
                if (request.Status == (int)DocumentStatus.NotOk)
                {
                    pushModel.alert = "Sorry your uploaded documents were not visible as per our requirements. Please upload again.";

                }
                else if (request.Status == (int)DocumentStatus.Verified)
                {
                    pushModel.alert = "Congratulations your uploaded documents have been approved. Please proceed with your transactions.";

                }
                else
                {
                    pushModel.alert = "Sorry your uploaded documents have been rejected,please contact to administrator ";
                }
                pushModel.Amount = walletUser.CurrentBalance;
                pushModel.CurrentBalance = walletUser.CurrentBalance;
                pushModel.MobileNo = walletUser.MobileNo;
                pushModel.SenderName = "";
                pushModel.pushType = 0;
                PushNotificationModel push = new PushNotificationModel();
                push.deviceType = (int)walletUser.DeviceType;
                push.deviceKey = walletUser.DeviceToken;
                if ((int)walletUser.DeviceType == (int)DeviceTypes.ANDROID || (int)walletUser.DeviceType == (int)DeviceTypes.Web)
                {
                    PushPayload<PayMoneyPushModel> aps = new PushPayload<PayMoneyPushModel>();
                    PushPayloadData<PayMoneyPushModel> _data = new PushPayloadData<PayMoneyPushModel>();
                    _data.notification = pushModel;
                    aps.data = _data;
                    aps.to = walletUser.DeviceToken;
                    aps.collapse_key = string.Empty;
                    push.message = JsonConvert.SerializeObject(aps);
                    push.payload = pushModel;

                }
                if ((int)walletUser.DeviceType == (int)DeviceTypes.IOS)
                {
                    NotificationJsonResponse<PayMoneyIOSPushModel> aps = new NotificationJsonResponse<PayMoneyIOSPushModel>();
                    PayMoneyIOSPushModel _iosPushModel = new PayMoneyIOSPushModel();
                    _iosPushModel.alert = pushModel.alert;
                    _iosPushModel.pushType = pushModel.pushType;
                    aps.aps = _iosPushModel;

                    push.message = JsonConvert.SerializeObject(aps);
                }
                if (!string.IsNullOrEmpty(push.message))
                {
                    new PushNotificationRepository().sendPushNotification(push);
                }
            }
            #endregion
            return result;
        }

        public async Task<UserListResponse> PendingKycUserList(UserListRequest request)
        {
            var result = new UserListResponse();
            result.UserList = await _userApiRepository.PendingKycUserList(request);
            if (result.UserList.Count > 0)
            {
                result.TotalCount = result.UserList[0].TotalCount;
            }
            return result;
        }

        public async Task<MemoryStream> ExportUserListReport(DownloadLogReportRequest request)
        {
            InitializeWorkbook();
            await GenerateData(request);
            return GetExcelStream();
        }

        MemoryStream GetExcelStream()
        {
            //Write the stream data of workbook to the root directory
            MemoryStream file = new MemoryStream();
            _hssfWorkbook.Write(file);
            return file;
        }

        private async Task GenerateData(DownloadLogReportRequest request)
        {

            try
            {
                ICellStyle style1 = _hssfWorkbook.CreateCellStyle();
                style1.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
                style1.FillPattern = FillPattern.SolidForeground;
                string documentStatus = "";
                var response = await _userApiRepository.GenerateUserList(request);

                ISheet sheet1 = _hssfWorkbook.CreateSheet("EzipayLog");
                sheet1.SetColumnWidth(0, 1500);
                sheet1.SetColumnWidth(1, 4000);
                sheet1.SetColumnWidth(2, 4000);
                sheet1.SetColumnWidth(3, 8000);
                sheet1.SetColumnWidth(4, 8000);
                sheet1.SetColumnWidth(5, 8000);
                sheet1.SetColumnWidth(6, 4000);
                sheet1.SetColumnWidth(7, 8000);
                //----------Create Header-----------------
                var R0 = sheet1.CreateRow(0);

                var C00 = R0.CreateCell(0);
                C00.SetCellValue("S.No");
                C00.CellStyle = style1;

                var C01 = R0.CreateCell(1);
                C01.SetCellValue("Created Date");
                C01.CellStyle = style1;

                var C02 = R0.CreateCell(2);
                C02.SetCellValue("Name");
                C02.CellStyle = style1;

                var C03 = R0.CreateCell(3);
                C03.SetCellValue("Email Id");
                C03.CellStyle = style1;

                var C04 = R0.CreateCell(4);
                C04.SetCellValue("MobileNo");
                C04.CellStyle = style1;

                var C05 = R0.CreateCell(5);
                C05.SetCellValue("Country");
                C05.CellStyle = style1;

                var C06 = R0.CreateCell(6);
                C06.SetCellValue("Currentbalance");
                C06.CellStyle = style1;

                var C07 = R0.CreateCell(7);
                C07.SetCellValue("DocumetStatus");
                C07.CellStyle = style1;

                int i = 1;
                foreach (var item in response.UserList)
                {
                    if (item.DocumetStatus == 0)
                    {
                        documentStatus = "Not uploaded";
                    }
                    else if (item.DocumetStatus == 1)
                    {
                        documentStatus = "Pending";
                    }
                    else if (item.DocumetStatus == 2)
                    {
                        documentStatus = "Verified";
                    }
                    else if (item.DocumetStatus == 3)
                    {
                        documentStatus = "Rejected";
                    }
                    IRow row = sheet1.CreateRow(i);

                    var C0 = row.CreateCell(0);
                    C0.SetCellValue(item.UserId.ToString().Count().ToString());

                    var C1 = row.CreateCell(1);
                    C1.SetCellValue(item.CreatedDate);

                    var C2 = row.CreateCell(2);
                    C2.SetCellValue(item.Name);

                    var c3 = row.CreateCell(3);
                    c3.SetCellValue(item.EmailId.ToString());

                    var c4 = row.CreateCell(4);
                    c4.SetCellValue(item.MobileNo);

                    var c5 = row.CreateCell(5);
                    c5.SetCellValue(item.Country.ToString());

                    var c6 = row.CreateCell(6);
                    c6.SetCellValue(item.Currentbalance.ToString());

                    var c7 = row.CreateCell(7);
                    c7.SetCellValue(documentStatus);

                    i++;
                }

            }
            catch (Exception ex)
            {

            }
        }

        void InitializeWorkbook()
        {
            _hssfWorkbook = new HSSFWorkbook();

            ////create a entry of DocumentSummaryInformation
            DocumentSummaryInformation dsi = PropertySetFactory.CreateDocumentSummaryInformation();
            dsi.Company = "NPOI Team";
            _hssfWorkbook.DocumentSummaryInformation = dsi;

            ////create a entry of SummaryInformation
            SummaryInformation si = PropertySetFactory.CreateSummaryInformation();
            si.Subject = "NPOI SDK Example";
            _hssfWorkbook.SummaryInformation = si;
        }

        public async Task<int> VerifyEmail(DocumentChangeRequest request)
        {
            int result = 0;
            var walletUser = await _userApiRepository.GetUserById(request.UserId);
            //var status = usrDoc != null ? usrDoc.DocumentStatus: 0;
            if (walletUser != null)
            {
                if (walletUser != null)
                {
                    walletUser.IsEmailVerified = true;

                    int rowAffected2 = await _userApiRepository.UpdateUser(walletUser);
                    if (rowAffected2 > 0)
                    {
                        result = 1;
                    }
                }
                else
                {
                    result = 2;
                }
            }
            #region Push notification

            if (walletUser.WalletUserId > 0 && walletUser.DeviceToken != null && request.Status != 0 && request.Status != 1)
            {
                PayMoneyPushModel pushModel = new PayMoneyPushModel();
                pushModel.TransactionDate = DateTime.UtcNow;
                pushModel.TransactionId = "";
                if (request.Status == (int)DocumentStatus.NotOk)
                {
                    pushModel.alert = "Sorry your uploaded documents were not visible as per our requirements. Please upload again.";

                }
                else if (request.Status == (int)DocumentStatus.Verified)
                {
                    pushModel.alert = "Congratulations your uploaded documents have been approved. Please proceed with your transactions.";

                }
                else
                {
                    pushModel.alert = "Sorry your uploaded documents have been rejected,please contact to administrator ";
                }
                pushModel.Amount = walletUser.CurrentBalance;
                pushModel.CurrentBalance = walletUser.CurrentBalance;
                pushModel.MobileNo = walletUser.MobileNo;
                pushModel.SenderName = "";
                pushModel.pushType = 0;
                PushNotificationModel push = new PushNotificationModel();
                push.deviceType = (int)walletUser.DeviceType;
                push.deviceKey = walletUser.DeviceToken;
                if ((int)walletUser.DeviceType == (int)DeviceTypes.ANDROID || (int)walletUser.DeviceType == (int)DeviceTypes.Web)
                {
                    PushPayload<PayMoneyPushModel> aps = new PushPayload<PayMoneyPushModel>();
                    PushPayloadData<PayMoneyPushModel> _data = new PushPayloadData<PayMoneyPushModel>();
                    _data.notification = pushModel;
                    aps.data = _data;
                    aps.to = walletUser.DeviceToken;
                    aps.collapse_key = string.Empty;
                    push.message = JsonConvert.SerializeObject(aps);
                    push.payload = pushModel;

                }
                if ((int)walletUser.DeviceType == (int)DeviceTypes.IOS)
                {
                    NotificationJsonResponse<PayMoneyIOSPushModel> aps = new NotificationJsonResponse<PayMoneyIOSPushModel>();
                    PayMoneyIOSPushModel _iosPushModel = new PayMoneyIOSPushModel();
                    _iosPushModel.alert = pushModel.alert;
                    _iosPushModel.pushType = pushModel.pushType;
                    aps.aps = _iosPushModel;

                    push.message = JsonConvert.SerializeObject(aps);
                }
                if (!string.IsNullOrEmpty(push.message))
                {
                    new PushNotificationRepository().sendPushNotification(push);
                }
            }
            #endregion
            return result;
        }




        public async Task<List<DuplicateCardNoVMResponse>> GetduplicatecardnoList(string Cardno, long Walletuserid)
        {
            return await _userApiRepository.GetduplicatecardnoList(Cardno, Walletuserid);
        }

        public async Task<bool> Insertduplicatecardno(DuplicateCardNoVMRequest request)
        {
            var result = false;
            int result1 = await _userApiRepository.Insertduplicatecardno(request);
            if (result1 == 1)
            {
                result = true;
            }

            return result;
        }




        public async Task<UserBlockUnblockDetailResponse> EnableDisableUserList(UserListRequest request)
        {
            return await _userApiRepository.EnableDisableUserList(request);
        }

    }
}
