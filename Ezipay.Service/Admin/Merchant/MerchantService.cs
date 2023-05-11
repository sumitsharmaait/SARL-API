using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ezeePay.Utility.CommonClass;
using ezeePay.Utility.Enums;
using Ezipay.Database;
using Ezipay.Repository.AdminRepo;
using Ezipay.Repository.AdminRepo.Merchant;
using Ezipay.Repository.UserRepo;
using Ezipay.Utility.common;
using Ezipay.Utility.SendEmail;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.CheckPasswordViewModel;
using Ezipay.ViewModel.SendEmailViewModel;
using Ezipay.ViewModel.WalletUserVM;
using EziPay.AWSUtils;
using NPOI.HPSF;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using QRCoder;

namespace Ezipay.Service.Admin.Merchant
{
    public class MerchantService : IMerchantService
    {
        private IUserApiRepository _userApiRepository;
        private IMerchantRepository _merchantRepository;
        private IS3Uploader _iS3Uploader;
        private HSSFWorkbook _hssfWorkbook;
        private IWalletUserRepository _walletUserRepository;
        private ISendEmails _sendEmails;

        public MerchantService()
        {
            _walletUserRepository = new WalletUserRepository();
            _userApiRepository = new UserApiRepository();
            _merchantRepository = new MerchantRepository();
            _iS3Uploader = new S3Uploader();
            _sendEmails = new SendEmails();
        }
        public async Task<MerchantListResponse> GetMerchantList(MerchantListRequest request)
        {
            var result = new MerchantListResponse();
            result.MerchantList = await _merchantRepository.GetMerchantList(request);
            if (result.MerchantList.Count > 0)
            {
                result.TotalCount = result.MerchantList[0].TotalCount;
            }
            return result;
        }

        public async Task<MerchantSaveResponse> SaveMerchant(MerchantRequest request)
        {
            var result = new MerchantSaveResponse();

            //var logoRes = await SaveMerchantImage(collection, request.ImageName);
            //request.LogoUrl = logoRes.ImageName;

            if (request.MerchantId == 0)
            {
                result.statusCode = await InsertMerchant(request);
            }
            else
            {
                result.statusCode = await UpdateMerchant(request);
            }

            return result;
        }

        private async Task<int> UpdateMerchant(MerchantRequest request)
        {
            int result = 0;
            var adminKeyPair = AES256.AdminKeyPair;
            string mobile = AES256.Encrypt(adminKeyPair.PublicKey, request.MobileNo);
            string emailId = AES256.Encrypt(adminKeyPair.PublicKey, request.EmailId);
            var hashedObject = SHA256ALGO.HashPassword(request.Password);
            var userKeyPair = AES256.UserKeyPair();

            var _walletUser = await _userApiRepository.GetUserById(request.MerchantId);
            if (_walletUser != null)
            {
                if (_walletUser.EmailId != emailId)
                {
                    bool isEmailExist = await _userApiRepository.CheckEmail(emailId, mobile);
                    if (isEmailExist)
                    {
                        result = (int)UserSignUpStatus.DuplicateUser;
                        return result;
                    }
                }

                #region Wallet User
                String timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                _walletUser.FirstName = AES256.Encrypt(userKeyPair.PublicKey, request.FirstName);
                _walletUser.LastName = AES256.Encrypt(userKeyPair.PublicKey, request.LastName);
                _walletUser.PublicKey = userKeyPair.PublicKey;
                _walletUser.PrivateKey = userKeyPair.PrivateKey;
                _walletUser.MobileNo = mobile;
                _walletUser.EmailId = emailId;
                _walletUser.StdCode = request.IsdCode;
                _walletUser.UserAddress = request.Address;
                _walletUser.ProfileImage =  request.LogoUrl;

                if (string.IsNullOrWhiteSpace(_walletUser.QrCode))
                {
                    var req = new QrCodeRequest
                    {
                        QrCode = request.IsdCode + "," + request.MobileNo
                    };
                    var qrCode = await GenerateQrCode(req);
                    _walletUser.QrCode = qrCode.QrCodeImage;
                }

                #endregion

                #region Wallet Service
                var walletService = await _merchantRepository.GetWalletServiceByUserId(_walletUser.WalletUserId);
                if (walletService != null)
                {
                    walletService.ServiceName = request.Company;
                }
                #endregion

                #region MerchantCommisionMaster
                List<MerchantCommisionMaster> commisionList = new List<MerchantCommisionMaster>();
                MerchantCommisionMaster objCommission = null;
                if (walletService != null)
                {
                    commisionList = await _merchantRepository.GetCommissionByServiceId(walletService.WalletServiceId);
                    commisionList.ForEach(x =>
                    {
                        x.IsActive = false;
                    });

                    objCommission = new MerchantCommisionMaster();
                    objCommission.CommisionPercent = Convert.ToDecimal(request.CommissionPercent);
                    objCommission.CreatedBy = 0;
                    objCommission.CreatedDate = DateTime.UtcNow;
                    objCommission.IsActive = true;
                    objCommission.UpdatedDate = DateTime.UtcNow;
                    objCommission.IsDeleted = false;
                    objCommission.WalletServiceId = walletService.WalletServiceId;
                }


                #endregion

                return await _merchantRepository.UpdateMerchant(_walletUser, walletService, objCommission, commisionList);
            }

            return result;
        }

        private async Task<int> InsertMerchant(MerchantRequest request)
        {
            int result = 0;

            var adminKeyPair = AES256.AdminKeyPair;
            string mobile = AES256.Encrypt(adminKeyPair.PublicKey, request.MobileNo);
            string emailId = AES256.Encrypt(adminKeyPair.PublicKey, request.EmailId);
            var hashedObject = SHA256ALGO.HashPassword(request.Password);
            var userKeyPair = AES256.UserKeyPair();

            bool isEmailExist = await _userApiRepository.CheckEmail(emailId, mobile);
            if (isEmailExist)
            {
                result = (int)UserSignUpStatus.DuplicateUser;
                return result;
            }

            var req = new QrCodeRequest
            {
                QrCode = request.IsdCode + "," + request.MobileNo
            };
            var qrCode = await GenerateQrCode(req);

            #region WalletUser
            WalletUser _walletUser = new WalletUser();
            _walletUser.UserType = (int)WalletUserTypes.Merchant;
            _walletUser.ProfileImage = string.Empty;
            _walletUser.AdminUserId = 0;
            _walletUser.CreatedDate = DateTime.UtcNow;
            _walletUser.UpdatedDate = DateTime.UtcNow;
            _walletUser.StdCode = request.IsdCode;
            _walletUser.IsActive = true;
            _walletUser.IsDisabledTransaction = false;
            _walletUser.IsDeleted = false;
            _walletUser.IsOtpVerified = true;
            _walletUser.IsEmailVerified = true;
            _walletUser.Otp = string.Empty;
            _walletUser.QrCode = string.Empty;
            _walletUser.IsNotification = true;
            _walletUser.CurrencyId = (int)CurrencyTypes.Ghanaian_Cedi;
            _walletUser.CurrentBalance = "0";
            _walletUser.DeviceToken = string.Empty;
            _walletUser.DeviceType = (int)DeviceTypes.Web;
            _walletUser.EmailId = emailId;
            _walletUser.FirstName = AES256.Encrypt(userKeyPair.PublicKey, request.FirstName);
            _walletUser.LastName = AES256.Encrypt(userKeyPair.PublicKey, request.LastName);
            _walletUser.HashedPassword = hashedObject.HashedPassword;
            _walletUser.HashedSalt = hashedObject.SlatBytes;
            _walletUser.StdCode = request.IsdCode;
            _walletUser.MobileNo = mobile;
            _walletUser.PrivateKey = userKeyPair.PrivateKey;
            _walletUser.PublicKey = userKeyPair.PublicKey;
            _walletUser.IsFirstTimeUser = true;
            _walletUser.EmailAuthToken = null;
            _walletUser.DocumetStatus = 2;
            _walletUser.QrCode = qrCode.QrCodeImage;
            _walletUser.ProfileImage = request.LogoUrl;
            _walletUser.UserAddress = request.Address;
            #endregion

            #region WalletService
            WalletService _walletService = new WalletService();
            _walletService.IsActive = true;
            _walletService.IsDeleted = false;
            _walletService.CreatedDate = DateTime.UtcNow;
            _walletService.UpdatedDate = DateTime.UtcNow;
            _walletService.ServiceName = request.Company;
            _walletService.ServiceCategoryId = Convert.ToInt32(WalletTransactionSubTypes.Merchants);
            #endregion

            #region MerchantCommisionMaster

            MerchantCommisionMaster _commisionMaster = new MerchantCommisionMaster();
            _commisionMaster.CommisionPercent = Convert.ToDecimal(request.CommissionPercent);
            _commisionMaster.CreatedDate = DateTime.UtcNow;
            _commisionMaster.IsActive = true;
            _commisionMaster.IsDeleted = false;
            _commisionMaster.WalletServiceId = _walletService.WalletServiceId;

            #endregion

            return await _merchantRepository.InsertMerchant(_walletUser, _walletService, _commisionMaster);

        }

        public async Task<QrCodeData> GenerateQrCode(QrCodeRequest request)
        {
            var qr = new QrCodeData();
            bool isUploaded = false;
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(request.QrCode, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(qrCodeData);
            var qrCodeImage = qrCode.GetGraphic(20, Color.Black, Color.White, (Bitmap)Bitmap.FromFile(CommonSetting.LogoPath));

            using (var outStream = new MemoryStream())
            {
                qrCodeImage.Save(outStream, System.Drawing.Imaging.ImageFormat.Png);
                qrCodeImage.Dispose();
                string imageName = Guid.NewGuid().ToString() + ".png";
                isUploaded = await _iS3Uploader.UploadImage(outStream, imageName, CommonSetting.AWS_BUCKET);

                if (isUploaded == true)
                {
                    qr.QrCodeUrl = CommonSetting.imageUrl + imageName;
                    qr.QrCodeImage = imageName;
                }
            }

            return qr;
        }

        public async Task<MerchantLogoUploadResponse> SaveMerchantImage(HttpFileCollectionBase collection, string logoImg)
        {
            bool isSaved = false;
            MerchantLogoUploadResponse logoUploadResponse = new MerchantLogoUploadResponse();
            logoUploadResponse.StatusCode = isSaved;
            logoUploadResponse.ImageName = string.Empty;
            if (collection != null && collection.Count > 0)
            {
                HttpPostedFileBase image = collection["merchantLogo"];
                string imageName = "";
                if (image != null)
                {

                    string[] ImageArrayName = image.FileName.Split('.');
                    string ext = ImageArrayName[1];
                    imageName = Guid.NewGuid().ToString("n") + "." + ext;
                    Stream requestStream = collection[0].InputStream;
                    string bucketFolderName = ConfigurationManager.AppSettings["AWSBucket"];
                    if (!string.IsNullOrEmpty(logoImg))
                    {
                        if (_iS3Uploader.DeleteObject(bucketFolderName, logoImg))
                            isSaved = await _iS3Uploader.UploadImage(requestStream, imageName, bucketFolderName);
                    }
                    else
                    {
                        isSaved = await _iS3Uploader.UploadImage(requestStream, imageName, bucketFolderName);
                    }

                    logoUploadResponse.StatusCode = isSaved;
                    logoUploadResponse.ImageName = imageName;
                }

            }

            return logoUploadResponse;

        }

        public async Task<bool> EnableDisableMerchant(MerchantManageRequest request)
        {
            bool result = false;

            var user = await _userApiRepository.GetUserById(request.UserId);
            if (user != null)
            {
                user.IsActive = request.IsActive;
                user.UpdatedDate = DateTime.UtcNow;

                if (await _userApiRepository.UpdateUser(user) > 0)
                {
                    result = true;
                }
            }
            return result;
        }

        public async Task<bool> DeleteMarchant(MarchantDeleteRequest request)
        {
            bool result = false;

            var user = await _userApiRepository.GetUserById(request.UserId);
            if (user != null)
            {
                user.IsDeleted = true;
                user.UpdatedDate = DateTime.UtcNow;

                if (await _userApiRepository.UpdateUser(user) > 0)
                {
                    result = true;
                }
            }
            return result;
        }

        public async Task<bool> EnableDisableTransaction(MerchantEnableTransactionRequest request)
        {
            bool result = false;

            var user = await _userApiRepository.GetUserById(request.UserId);
            if (user != null)
            {
                user.IsDisabledTransaction = request.IsDisabledTransaction;
                user.UpdatedDate = DateTime.UtcNow;

                if (await _userApiRepository.UpdateUser(user) > 0)
                {
                    result = true;
                }
            }
            return result;
        }

        public async Task<ViewMarchantTransactionResponse> ViewMerchantTransactions(ViewMarchantTransactionRequest request)
        {
            var result = new ViewMarchantTransactionResponse();
            result.TransactionList = await _merchantRepository.ViewMerchantTransactions(request);
            if (result.TransactionList.Count > 0)
            {
                result.TotalCount = result.TransactionList[0].TotalCount;
            }

            if (request.FromDate != null && request.ToDate != null)
            {
                result.DateFrom = request.DateFrom.Value.ToString("yyyy-MM-dd");
                result.DateTo = request.DateTo.Value.ToString("yyyy-MM-dd");
            }
            return result;
        }

        public async Task<MemoryStream> ExportMerchantListReport(DownloadLogReportRequest request)
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

                var response = await _merchantRepository.DownLoadMerchantLogList(request);

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
                C05.SetCellValue("MerchantAmount");
                C05.CellStyle = style1;

                var C06 = R0.CreateCell(6);
                C06.SetCellValue("PaidToEzeepay");
                C06.CellStyle = style1;

                int i = 1;
                foreach (var item in response.MerchantList)
                {

                    IRow row = sheet1.CreateRow(i);

                    var C0 = row.CreateCell(0);
                    C0.SetCellValue(item.MerchantId.ToString().Count().ToString());

                    var C1 = row.CreateCell(1);
                    C1.SetCellValue(item.CreatedOn);

                    var C2 = row.CreateCell(2);
                    C2.SetCellValue(item.FirstName + " " + item.LastName);

                    var c3 = row.CreateCell(3);
                    c3.SetCellValue(item.EmailId.ToString());

                    var c4 = row.CreateCell(4);
                    c4.SetCellValue(item.MobileNo);

                    var c5 = row.CreateCell(5);
                    c5.SetCellValue(item.MerchantAmount.ToString());

                    var c6 = row.CreateCell(6);
                    c6.SetCellValue(item.PaidToEzeepay.ToString());

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

        public async Task<MerchantSaveResponse> MerchantOnBoardRequest(MerchantRequest request)
        {
            var result = new MerchantSaveResponse();

            var adminKeyPair = AES256.AdminKeyPair;
            string mobile = AES256.Encrypt(adminKeyPair.PublicKey, request.MobileNo);
            string emailId = AES256.Encrypt(adminKeyPair.PublicKey, request.EmailId);
            var hashedObject = SHA256ALGO.HashPassword(request.Password);
            var userKeyPair = AES256.UserKeyPair();

            bool isEmailExist = await _userApiRepository.CheckEmail(emailId, mobile);
            if (isEmailExist)
            {
                result.statusCode = (int)UserSignUpStatus.DuplicateUser;
                return result;
            }

            var req = new QrCodeRequest
            {
                QrCode = request.IsdCode + "," + request.MobileNo
            };
            var qrCode = await GenerateQrCode(req);

            #region WalletUser
            WalletUser _walletUser = new WalletUser();
            _walletUser.UserType = (int)WalletUserTypes.Merchant;
            _walletUser.ProfileImage = string.Empty;
            _walletUser.AdminUserId = 0;
            _walletUser.CreatedDate = DateTime.UtcNow;
            _walletUser.UpdatedDate = DateTime.UtcNow;
            _walletUser.StdCode = request.IsdCode;
            _walletUser.IsActive = true;
            _walletUser.IsDisabledTransaction = false;
            _walletUser.IsDeleted = false;
            _walletUser.IsOtpVerified = false;
            _walletUser.IsEmailVerified = true;
            _walletUser.Otp = string.Empty;
            _walletUser.QrCode = string.Empty;
            _walletUser.IsNotification = true;
            _walletUser.CurrencyId = (int)CurrencyTypes.Ghanaian_Cedi;
            _walletUser.CurrentBalance = "0";
            _walletUser.DeviceToken = string.Empty;
            _walletUser.DeviceType = (int)DeviceTypes.Web;
            _walletUser.EmailId = emailId;
            _walletUser.FirstName = AES256.Encrypt(userKeyPair.PublicKey, request.FirstName);
            _walletUser.LastName = AES256.Encrypt(userKeyPair.PublicKey, request.LastName);
            _walletUser.HashedPassword = hashedObject.HashedPassword;
            _walletUser.HashedSalt = hashedObject.SlatBytes;
            _walletUser.StdCode = request.IsdCode;
            _walletUser.MobileNo = mobile;
            _walletUser.PrivateKey = userKeyPair.PrivateKey;
            _walletUser.PublicKey = userKeyPair.PublicKey;
            _walletUser.BusinessLicense = request.BusinessLicense;
            _walletUser.IsFirstTimeUser = true;
            _walletUser.EmailAuthToken = null;
            _walletUser.DocumetStatus = (int)DocumentStatus.Pending;
            _walletUser.ProfileImage =request.LogoUrl;
            _walletUser.UserAddress = request.Address;
            _walletUser.MerchantType = (int)EnumMerchantType.OnBoard;
            _walletUser.Latitude = request.Latitude;
            _walletUser.Longitude = request.Longitude;
            _walletUser.TinNumber = request.TinNumber;
            _walletUser.VatNumber = request.VatNumber;
            _walletUser.PostalCode = request.PostalCode;
            _walletUser.QrCode = qrCode.QrCodeImage;
            #endregion

            string ATMCard = AES256.Encrypt(adminKeyPair.PublicKey, request.ATMCard.Trim());
            string IdCard = AES256.Encrypt(adminKeyPair.PublicKey, request.IdCard.Trim());

            var docEntity = new UserDocument
            {
                IdProofImage = IdCard,
                CardImage = ATMCard,
                DocumentStatus = (int)DocumentStatus.Pending,
                CreateOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow,
            };

            var docs = new List<MerchantDocument>();
            request.Documents.ForEach(x =>
            {
                docs.Add(new MerchantDocument
                {
                    DocImage = x.DocName,
                    DocType = x.DocType,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedOn = DateTime.UtcNow,
                    UpdatedOn = DateTime.UtcNow,
                });
            });

            string bankName = AES256.Encrypt(adminKeyPair.PublicKey, request.BankName);
            string accountNumber = AES256.Encrypt(adminKeyPair.PublicKey, request.AccountNumber);
            string bankCode = AES256.Encrypt(adminKeyPair.PublicKey, request.BankCode);

            var bankEntity = new BankDetail
            {
                BankName = bankName,
                AccountNumber = accountNumber,
                BankCode = bankCode,
                IsActive = true,
                IsDeleted = false,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow,
            };

            #region WalletService
            WalletService _walletService = new WalletService();
            _walletService.IsActive = true;
            _walletService.IsDeleted = false;
            _walletService.CreatedDate = DateTime.UtcNow;
            _walletService.UpdatedDate = DateTime.UtcNow;
            _walletService.ServiceName = request.Company;
            _walletService.ServiceCategoryId = Convert.ToInt32(WalletTransactionSubTypes.Merchants);
            #endregion

            #region MerchantCommisionMaster

            MerchantCommisionMaster _commisionMaster = new MerchantCommisionMaster();
            _commisionMaster.CommisionPercent = Convert.ToDecimal(request.CommissionPercent);
            _commisionMaster.CreatedDate = DateTime.UtcNow;
            _commisionMaster.IsActive = true;
            _commisionMaster.IsDeleted = false;
            _commisionMaster.WalletServiceId = _walletService.WalletServiceId;

            #endregion

            var userApiKey = new UserApiKey
            {
                WalletUserId = _walletUser.WalletUserId,
                IsActive = true,
                IsDeleted = false,
                CreateDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            result.statusCode = await _merchantRepository.OnBoardRequest(_walletUser, docEntity, docs, bankEntity, _walletService, _commisionMaster, userApiKey);

            var _EmailVerification = new EmailVerification();
            _EmailVerification.EmailId = _walletUser.EmailId;
            _EmailVerification.CreatedDate = DateTime.UtcNow;
            _EmailVerification.IsVerified = false;
            _EmailVerification.VerificationDate = DateTime.UtcNow;
            _EmailVerification.WalletUserId = 0;
            _EmailVerification.IsMailSent = true;
            _EmailVerification = await _walletUserRepository.InsertEmailVerification(_EmailVerification);
            if (_EmailVerification != null)
            {
                string uniqueToken = RandomAlphaNumerals(15) + "_" + _EmailVerification.EmailVerificationId.ToString();
                string VerifyMailLink = CommonSetting.VerifyMailLink + "/" + HttpUtility.UrlEncode(uniqueToken);
                string filename = CommonSetting.EmailVerificationTemplate;
                var body = _sendEmails.ReadEmailformats(filename);
                string Body = string.Format(body, VerifyMailLink);
                //  body = body.Replace("$$IsVerified$$", VerifyMailLink);
                //Send Email to user on register
                var emailModel = new EmailModel
                {
                    TO = request.EmailId,
                    Subject = ResponseMessages.USER_REGISTERED,//"Registered successfully",
                    Body = Body
                };
                _sendEmails.SendEmail(emailModel);
            }


            return result;
        }

        string RandomAlphaNumerals(int stringLength)
        {
            Random random = new Random();


            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, stringLength)
              .Select(s => s[random.Next(s.Length)]).ToArray());

        }

        public async Task<int> SaveStore(AddStoreRequest request)
        {
            int result = 0;
            var merchant = await _userApiRepository.GetUserById(request.WalletUserId);
            if (merchant != null)
            {
                if (request.StoreId == 0)
                {
                    var adminKeyPair = AES256.AdminKeyPair;
                    string mobileNumber = merchant.StdCode + "," + AES256.Decrypt(adminKeyPair.PublicKey, merchant.MobileNo);

                    var storeEntity = new MerchantStore
                    {
                        StoreName = request.StoreName,
                        WalletUserId = merchant.WalletUserId,
                        Location = request.Location,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedDate = DateTime.UtcNow
                    };
                    result = await _merchantRepository.InsertStore(storeEntity);
                    if (result > 0)
                    {
                        var req = new QrCodeRequest
                        {
                            QrCode = mobileNumber + "_" + storeEntity.Id
                        };
                        var qrCode = await GenerateQrCode(req);
                        storeEntity.QrCode = qrCode.QrCodeImage;
                        await _merchantRepository.UpdateStore(storeEntity);
                    }
                }
                else
                {
                    var store = await _merchantRepository.GetStoreById(request.StoreId);
                    if (store != null)
                    {
                        store.StoreName = request.StoreName;
                        store.Location = request.Location;
                        store.UpdatedDate = DateTime.UtcNow;
                        result = await _merchantRepository.UpdateStore(store);
                    }
                }
            }
            return result;
        }

        public async Task<List<StoreResponse>> GetStores(StoreSearchRequest request)
        {
            return await _merchantRepository.GetStores(request);
        }

        public async Task<int> DeleteStore(StoreDeleteRequest requestModel)
        {
            int result = 0;

            var store = await _merchantRepository.GetStoreById(requestModel.StoreId);
            if (store != null)
            {
                store.IsDeleted = true;
                store.UpdatedDate = DateTime.UtcNow;

                result = await _merchantRepository.UpdateStore(store);

            }
            return result;
        }

        public async Task<int> EnableDisableStore(StoreManageRequest request)
        {
            int result = 0;

            var store = await _merchantRepository.GetStoreById(request.StoreId);
            if (store != null)
            {
                store.IsActive = request.IsActive;
                store.UpdatedDate = DateTime.UtcNow;

                result = await _merchantRepository.UpdateStore(store);

            }
            return result;
        }
    }
}
