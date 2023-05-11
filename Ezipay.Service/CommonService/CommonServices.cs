using ezeePay.Utility.CommonClass;
using ezeePay.Utility.Enums;
using Ezipay.Database;
using Ezipay.Repository.CommonRepo;
using Ezipay.Repository.MasterData;
using Ezipay.Repository.TokenRepo;
using Ezipay.Repository.UserRepo;
using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.Utility.SendEmail;
using Ezipay.ViewModel.AirtimeViewModel;
using Ezipay.ViewModel.BannerViewModel;
using Ezipay.ViewModel.CheckPasswordViewModel;
using Ezipay.ViewModel.common;
using Ezipay.ViewModel.SendEmailViewModel;
using Ezipay.ViewModel.WalletUserVM;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Ezipay.Utility.common.AppSetting;

namespace Ezipay.Service.CommonService
{
    public class CommonServices : ICommonServices
    {
        // private HSSFWorkbook _hssfWorkbook;
        private ICommonRepository _commonRepository;
        private ITokenRepository _tokenRepository;
        private IWalletUserRepository _walletUserRepository;
        private ISendEmails _sendEmails;
        private IMasterDataRepository _masterDataRepository;

        public CommonServices()
        {
            _commonRepository = new CommonRepository();
            _tokenRepository = new TokenRepository();
            _walletUserRepository = new WalletUserRepository();
            _sendEmails = new SendEmails();
            _masterDataRepository = new MasterDataRepository();
        }

        public async Task<bool> CheckPassword(string password, string token)
        {
            bool IsMatch = false;
            var response = new CheckLoginResponse();
            try
            {

                if (!string.IsNullOrEmpty(password))
                {
                    response = await _commonRepository.CheckPassword(token); ////
                    if (response != null && !string.IsNullOrEmpty(response.HashedPassword))
                    {
                        var hashedObject = SHA256ALGO.HashPasswordDecryption(password, response.HashedSalt);
                        if (hashedObject.HashedPassword == response.HashedPassword)
                        {
                            IsMatch = true;
                        }
                        else
                        {
                            IsMatch = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("CommonServices.cs", "CheckPassword");
            }
            return IsMatch;

        }
        //public async Task<List<FeedbackTypeResponse>> FeedBackTypes()
        //{
        //    var response = new List<FeedbackTypeResponse>();
        //    try
        //    {
        //        response = await _commonRepository.FeedBackTypes();
        //    }
        //    catch (Exception ex)
        //    {

        //        ex.Message.ErrorLog("CommonRepository.cs", "FeedBackTypes");
        //    }
        //    return response;
        //}

        //public async Task<bool> SaveFeedBack(FeedBackRequest request)
        //{
        //    bool response = false;
        //    try
        //    {
        //        var adminKeyPair = AES256.AdminKeyPair;
        //        var KeyPair = _tokenRepository.KeysBySessionToken();
        //        var AdminEmail = ConfigurationManager.AppSettings["AdminEmail"].ToString();

        //        var result = await _walletUserRepository.UserProfile(KeyPair.Token);

        //        // long WalletUserId = (long)await db.SessionTokens.Where(x => x.TokenValue == KeyPair.Token).Select(x => x.WalletUserId).FirstOrDefaultAsync();
        //        // var sender = new AppUserRepository().UserProfile();

        //        Feedback _feedBack = new Feedback();
        //        _feedBack.UserId = result.WalletUserId;
        //        _feedBack.IsActive = true;
        //        _feedBack.IsDeleted = false;
        //        _feedBack.CreatedDate = DateTime.UtcNow;
        //        _feedBack.UpdatedDate = DateTime.UtcNow;
        //        _feedBack.FeedbackId = request.FeedbackTypeId;
        //        _feedBack.FeedBackMessage = request.FeedBackMessage;
        //        await _commonRepository.SaveFeedBack(_feedBack);
        //        response = true;
        //        var req = new EmailModel
        //        {
        //            TO = AdminEmail,
        //            Body = "Hi,<br/>" + request.FeedBackMessage + " <br/><br/>" + (result.FirstName + " " + result.LastName).Trim() + "<br/>" + DateTime.UtcNow.ToString("dd-MMM-yyyy"),
        //            Subject = "Feedback"
        //        };
        //        _sendEmails.SendEmail(req);

        //        //_IMessageService.SendMail(AdminEmail, "Feedback", "Hi,<br/>" + request.FeedBackMessage + " <br/><br/>" + (result.FirstName + " " + result.LastName).Trim() + "<br/>" + DateTime.UtcNow.ToString("dd-MMM-yyyy"));

        //    }
        //    catch (Exception ex)
        //    {

        //        ex.Message.ErrorLog("UserProfileRepository.cs", "ChangePassword", request);
        //    }
        //    return response;
        //}

        //public async Task<bool> ChangeNotification()
        //{
        //    string token = GlobalData.Key;
        //    var result = await _walletUserRepository.UserProfile(token);
        //    var _walletUser = await _walletUserRepository.GetCurrentUser(result.WalletUserId);
        //    if (_walletUser != null)
        //    {
        //        if (_walletUser.IsNotification == true)
        //        {
        //            _walletUser.IsNotification = false;
        //        }
        //        else
        //        {

        //            _walletUser.IsNotification = true;
        //        }
        //        await _commonRepository.ChangeNotification(_walletUser);
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}


        //public async Task<bool> SendRequest()
        //{
        //    bool IsSent = false;
        //    try
        //    {

        //        string token = GlobalData.Key;
        //        var _response = await _walletUserRepository.UserProfile(token);
        //        if (_response != null)
        //        {
        //            var requestNumber = await _masterDataRepository.GetInvoiceNumber(6);

        //            string FirstName = _response.FirstName;
        //            string LastName = _response.LastName;
        //            string EmailId = _response.EmailId;
        //            string mobileNo = _response.MobileNo;
        //            string sdtcode = _response.StdCode;
        //            string createdDate = Convert.ToString(DateTime.Now);
        //            var TempBody = System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/EmailTemplate/CallBackEmailTemplate.html");
        //            string body = string.Format(TempBody, FirstName, LastName, mobileNo, EmailId, createdDate);
        //            string fromEmail = ConfigurationManager.AppSettings["CallBackFromEmailId"];
        //            string fromPassword = ConfigurationManager.AppSettings["CallBackFromPassword"];
        //            string toEmail = ConfigurationManager.AppSettings["CallBackToEmailId"];
        //            string subject = "Callback Request ID " + requestNumber.AutoDigit;
        //            // IsSent = CommonMethod.CommonSendEmail(fromEmail, fromPassword, toEmail, subject, Body);

        //            var req = new EmailModel
        //            {
        //                TO = toEmail,
        //                Body = body,
        //                Subject = subject
        //            };
        //            _sendEmails.SendEmail(req);
        //            if (_sendEmails != null)
        //            {
        //                IsSent = true;
        //            }
        //            else
        //            {
        //                IsSent = false;
        //            }
        //            if (IsSent == true)
        //            {
        //                Callback _callback = new Callback();
        //                _callback.FirstName = _response.FirstName;
        //                _callback.LastName = _response.LastName;
        //                _callback.EmailId = _response.EmailId;
        //                var requestNewId = await _masterDataRepository.GetInvoiceNumber(6);
        //                _callback.RequestNumber = requestNewId.AutoDigit;
        //                _callback.CreatedDate = DateTime.Now;
        //                _callback.UpdatedDate = DateTime.Now;
        //                _callback.IsActive = true;
        //                _callback.MobileNo = _response.MobileNo;
        //                _callback.IsDeleted = false;
        //                _callback.Status = (int)CallBackRequestStatus.Pending;

        //                _callback = await _commonRepository.SendRequest(_callback);
        //                if (_callback.CallbackId > 0)
        //                {
        //                    CallbackListTracking callbackListTracking = new CallbackListTracking();

        //                    callbackListTracking.CallBackId = _callback.CallbackId;
        //                    callbackListTracking.Status = (int)CallBackRequestStatus.Pending; ;
        //                    callbackListTracking.CeatedBy = _response.WalletUserId;
        //                    callbackListTracking.CreatedOn = DateTime.UtcNow;
        //                    await _commonRepository.InsertCallbackListTracking(callbackListTracking);
        //                    IsSent = true;
        //                    //InsertCallbackListTracking(db, _callback.CallbackId, (int)CallBackRequestStatus.Pending, _response.WalletUserId);
        //                }
        //                else
        //                {
        //                    IsSent = false;
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {


        //    }
        //    return IsSent;
        //}

        //public async Task<List<BannerVM>> GetBanner()
        //{
        //    var response = new List<BannerVM>();

        //    response = await _commonRepository.GetBanner();

        //    return response;
        //}

        //public async Task<UserDocumentResponse> ViewDocument(UserDocumentRequest request)
        //{
        //    var response = new UserDocumentResponse();
        //    var AdminKeys = AES256.AdminKeyPair;
        //    response = await _commonRepository.ViewDocument(request);
        //    response.WalletUserId = response.WalletUserId;
        //    response.DocumentStatus = response.DocumentStatus;
        //    response.CardImage = AES256.Decrypt(AdminKeys.PrivateKey, response.CardImage);
        //    response.IdProofImage = AES256.Decrypt(AdminKeys.PrivateKey, response.IdProofImage);
        //    return response;
        //}

        //public async Task<List<RecentReceiverResponse>> RecentReceiver(RecentReceiverRequest request)
        //{
        //    var result = new List<RecentReceiverResponse>();
        //    return result = await _commonRepository.RecentReceiver(request);
        //}

        //public String MD5Hash(MobileMoneyAggregatoryRequest request)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    //Url for Payment
        //    sb.Append(request.apiKey);
        //    sb.Append(request.customer);
        //    sb.Append(request.amount);
        //    sb.Append(request.invoiceNo);
        //    sb.Append(ThirdPartyAggragatorSettings.secretKey);
        //    StringBuilder hash = new StringBuilder();
        //    MD5CryptoServiceProvider md5provider = new MD5CryptoServiceProvider();
        //    byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(sb.ToString()));

        //    for (int i = 0; i < bytes.Length; i++)
        //    {
        //        hash.Append(bytes[i].ToString("x2"));
        //    }
        //    return hash.ToString();
        //}



        //public async Task<MemoryStream> ExportReport(DownloadLogReportRequest request)
        //{
        //    InitializeWorkbook();
        //    await GenerateData(request);
        //    return GetExcelStream();
        //}

        //MemoryStream GetExcelStream()
        //{
        //    //Write the stream data of workbook to the root directory
        //    MemoryStream file = new MemoryStream();
        //    _hssfWorkbook.Write(file);
        //    return file;
        //}

        //private async Task GenerateData(DownloadLogReportRequest request)
        //{

        //    try
        //    {
        //        ICellStyle style1 = _hssfWorkbook.CreateCellStyle();
        //        style1.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
        //        style1.FillPattern = FillPattern.SolidForeground;

        //        var response = await _userRepository.GenerateLogReport(request);

        //        ISheet sheet1 = _hssfWorkbook.CreateSheet("EzipayLog");
        //        sheet1.SetColumnWidth(0, 1500);
        //        sheet1.SetColumnWidth(1, 4000);
        //        sheet1.SetColumnWidth(2, 4000);
        //        sheet1.SetColumnWidth(3, 8000);
        //        sheet1.SetColumnWidth(4, 8000);
        //        sheet1.SetColumnWidth(5, 8000);
        //        sheet1.SetColumnWidth(6, 4000);
        //        sheet1.SetColumnWidth(7, 8000);
        //        sheet1.SetColumnWidth(8, 4000);
        //        sheet1.SetColumnWidth(9, 4000);
        //        sheet1.SetColumnWidth(10, 8000);
        //        sheet1.SetColumnWidth(11, 4000);
        //        sheet1.SetColumnWidth(12, 4000);
        //        sheet1.SetColumnWidth(13, 4000);
        //        sheet1.SetColumnWidth(14, 15000);
        //        sheet1.SetColumnWidth(15, 4000);
        //        //----------Create Header-----------------
        //        var R0 = sheet1.CreateRow(0);

        //        var C00 = R0.CreateCell(0);
        //        C00.SetCellValue("S.No");
        //        C00.CellStyle = style1;

        //        var C01 = R0.CreateCell(1);
        //        C01.SetCellValue("WalletTransactionId");
        //        C01.CellStyle = style1;

        //        var C02 = R0.CreateCell(2);
        //        C02.SetCellValue("Transactionid");
        //        C02.CellStyle = style1;

        //        var C03 = R0.CreateCell(3);
        //        C03.SetCellValue("Date");
        //        C03.CellStyle = style1;

        //        var C04 = R0.CreateCell(4);
        //        C04.SetCellValue("Time");
        //        C04.CellStyle = style1;

        //        var C05 = R0.CreateCell(5);
        //        C05.SetCellValue("CategoryName");
        //        C05.CellStyle = style1;

        //        var C06 = R0.CreateCell(6);
        //        C06.SetCellValue("ServiceName");
        //        C06.CellStyle = style1;

        //        var C07 = R0.CreateCell(7);
        //        C07.SetCellValue("TransactionType");
        //        C07.CellStyle = style1;

        //        var C08 = R0.CreateCell(8);
        //        C08.SetCellValue("TotalAmount");
        //        C08.CellStyle = style1;

        //        var C09 = R0.CreateCell(9);
        //        C09.SetCellValue("CommisionAmount");
        //        C09.CellStyle = style1;

        //        var C10 = R0.CreateCell(10);
        //        C10.SetCellValue("WalletAmount");
        //        C10.CellStyle = style1;

        //        var C11 = R0.CreateCell(11);
        //        C11.SetCellValue("Name");
        //        C11.CellStyle = style1;

        //        var C12 = R0.CreateCell(12);
        //        C12.SetCellValue("AccountNo");
        //        C12.CellStyle = style1;

        //        var C13 = R0.CreateCell(13);
        //        C13.SetCellValue("TransactionStatus");
        //        C13.CellStyle = style1;

        //        var C14 = R0.CreateCell(14);
        //        C14.SetCellValue("Comment");
        //        C14.CellStyle = style1;

        //        var C15 = R0.CreateCell(15);
        //        C15.SetCellValue("Walletuserid");
        //        C15.CellStyle = style1;
        //        int i = 1;
        //        foreach (var item in response.TransactionLogslist)
        //        {
        //            IRow row = sheet1.CreateRow(i);

        //            var C0 = row.CreateCell(0);
        //            C0.SetCellValue(item.transactionid.Count().ToString());

        //            var C1 = row.CreateCell(1);
        //            C1.SetCellValue(item.WalletTransactionId);

        //            var C2 = row.CreateCell(2);
        //            C2.SetCellValue(item.transactionid);

        //            var c3 = row.CreateCell(3);
        //            c3.SetCellValue(item.Date.ToString());

        //            var c4 = row.CreateCell(4);
        //            c4.SetCellValue(item.Time);

        //            var c5 = row.CreateCell(5);
        //            c5.SetCellValue(item.categoryname.ToString());

        //            var c6 = row.CreateCell(6);
        //            c6.SetCellValue(item.servicename.ToString());

        //            var c7 = row.CreateCell(7);
        //            c7.SetCellValue(item.transactionType);

        //            var c8 = row.CreateCell(8);
        //            c8.SetCellValue(item.totalAmount);

        //            var c9 = row.CreateCell(9);
        //            c9.SetCellValue(item.commisionAmount);


        //            var c10 = row.CreateCell(10);
        //            c10.SetCellValue(item.walletAmount.ToString());

        //            var c11 = row.CreateCell(11);
        //            c11.SetCellValue(item.name.ToString());

        //            var c12 = row.CreateCell(12);
        //            c12.SetCellValue(item.accountNo);

        //            var c13 = row.CreateCell(13);
        //            c13.SetCellValue(item.transactionStatus);

        //            var c14 = row.CreateCell(14);
        //            c14.SetCellValue(item.comments.ToString());

        //            var c15 = row.CreateCell(15);
        //            c15.SetCellValue(item.walletuserid);


        //            i++;
        //        }

        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}

        //void InitializeWorkbook()
        //{
        //    _hssfWorkbook = new HSSFWorkbook();

        //    ////create a entry of DocumentSummaryInformation
        //    DocumentSummaryInformation dsi = PropertySetFactory.CreateDocumentSummaryInformation();
        //    dsi.Company = "NPOI Team";
        //    _hssfWorkbook.DocumentSummaryInformation = dsi;

        //    ////create a entry of SummaryInformation
        //    SummaryInformation si = PropertySetFactory.CreateSummaryInformation();
        //    si.Subject = "NPOI SDK Example";
        //    _hssfWorkbook.SummaryInformation = si;
        //}

        public String SHA1Hash(string request)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder hash = new StringBuilder();
            //Url for Payment
            hash.Append(request);
            SHA1 sHA1 = SHA1.Create();
            byte[] vs = sHA1.ComputeHash(new UTF8Encoding().GetBytes(hash.ToString()));

            for (int i = 0; i < vs.Length; i++)
            {
                sb.Append(vs[i].ToString("x2"));
            }
            return sb.ToString();
        }

        public async Task<bool> IsUserValid(string token, long walletUserId, decimal RequestedAmount)
        {
            var sessionToken = new SessionToken();
            bool isValid = false;
            var d = GlobalData.RoleId;
            sessionToken = await _commonRepository.IsValidToken(token);
            if (d == 1)
            {
                DateTime a = Convert.ToDateTime(sessionToken.ExpiryTime);
                DateTime b = DateTime.UtcNow;
                var time = b.Subtract(a).TotalMinutes;

                if (sessionToken != null && sessionToken.WalletUserId == walletUserId && time <= 2)
                {
                    var result = await _commonRepository.GetWalletUserById(walletUserId);
                    if (result.IsActive == true && result.IsDeleted == false && Convert.ToDecimal(result.CurrentBalance) >= Convert.ToDecimal(RequestedAmount) && result.IsEmailVerified == true)
                    {
                        isValid = true;
                    }
                    else
                    {
                        isValid = false;
                    }

                    await _commonRepository.UpdateTokenTime(token);
                }
            }
            else
            {
                if (sessionToken != null && sessionToken.WalletUserId == walletUserId)
                {
                    var result = await _commonRepository.GetWalletUserById(walletUserId);
                    if (result.IsActive == true && result.IsDeleted == false && Convert.ToDecimal(result.CurrentBalance) >= Convert.ToDecimal(RequestedAmount) && result.IsEmailVerified == true)
                    {
                        isValid = true;
                    }
                    else
                    {
                        isValid = false;
                    }
                }
            }
            return isValid;
        }

    }
}
