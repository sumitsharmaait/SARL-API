using ezeePay.Utility.Enums;
using Ezipay.Database;
using Ezipay.Repository.AdminRepo.Resort;
using Ezipay.Repository.PayMoneyRepo;
using Ezipay.Repository.UserRepo;
using Ezipay.Service.UserService;
using Ezipay.Utility.AWSS3;
using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.Utility.SendEmail;
using Ezipay.ViewModel.PayMoneyViewModel;
using Ezipay.ViewModel.ResortViewModel;
using Ezipay.ViewModel.SendEmailViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Ezipay.Service.Admin.Resort
{
    public class ResortService : IResortService
    {
        private IS3Uploader _iS3Uploader;
        private IResortRepository _resortRepository;
        private IWalletUserService _walletUserService;
        private IWalletUserRepository _walletUserRepository;
        private IPayMoneyRepository _payMoneyRepository;
        private ISendEmails _sendEmails;
        public ResortService()
        {
            _iS3Uploader = new S3Uploader();
            _resortRepository = new ResortRepository();
            _walletUserService = new WalletUserService();
            _walletUserRepository = new WalletUserRepository();
            _payMoneyRepository = new PayMoneyRepository();
            _sendEmails = new SendEmails();
        }

        public async Task<string> SaveImage(HttpPostedFileBase image, string PreviousImage)
        {
            //UserDetailResponse response = new UserDetailResponse();

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

                    IsSuccess = _iS3Uploader.UploadImages(requestStream, imageName);

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

        public async Task<bool> InsertHotel(HotelRequest request)
        {
            bool response = false;
            var req = new HotelMaster
            {
                HotelName = request.HotelName,
                HotelImage = request.HotelImage,
                CostOfRooms = request.CostOfRooms,
                NoOfRooms = request.NoOfRooms,
                Location = request.Location,
                MaxGuest = request.MaxGuest,
                PdfUrl = request.PdfUrl,
                AvailableRooms = request.AvailableRooms,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                UpDatedDate = DateTime.UtcNow,
                //WalletUserId = 0
            };
            if (request.Id == 0)
            {
                response = await _resortRepository.InsertHotel(req);
            }
            else
            {
                response = await _resortRepository.UpdateHotel(req);
            }
            return response;
        }

        public async Task<List<HotelMasterResponse>> GetHotels()
        {
            var result = new List<HotelMasterResponse>();
            result = await _resortRepository.GetHotels();

            return result;
        }

        public async Task<bool> DeleteHotel(int id)
        {
            bool response = false;
            response = await _resortRepository.DeleteHotel(id);
            return response;
        }

        public async Task<bool> UpdateHotel(HotelMaster request)
        {
            bool response = false;
            response = await _resortRepository.UpdateHotel(request);
            return response;
        }

        public async Task<WalletTransactionResponse> HotelBook(HotelBookingRequest request, string token)////
        {
            var res = new WalletTransactionResponse();
            var _hotel = new HotelMaster();
            var data = await _walletUserService.UserProfile(token);
            var sender = await _walletUserRepository.GetCurrentUser(data.WalletUserId);
            _hotel = await _resortRepository.GetHotelById(request.HotelId);
            var Isdocverified = await _walletUserRepository.IsDocVerifiedMOMO(data.DocumetStatus);
            decimal currentBalance = Convert.ToDecimal(sender.CurrentBalance);
            decimal roomCost = Convert.ToDecimal(request.CostOfRoom);
            string ServiceName = string.Empty;
            int WalletServiceId = 0;
            int MerchantCommissionServiceId = 0;
            int _TypeInfo = 0;
            int _PushType = 0;
            if (sender.IsEmailVerified == true)
            {
                if (sender.IsDisabledTransaction == false)
                {
                    if (Isdocverified == true)
                    {
                        if (!sender.CurrentBalance.IsZero() && Convert.ToDecimal(sender.CurrentBalance) >= (Convert.ToDecimal(request.CostOfRoom)))//+ Convert.ToDecimal(commission.CommissionAmount)))
                        {
                            #region Save Transaction
                            var tran = new WalletTransaction();
                            tran.Comments = _hotel.HotelName;
                            tran.InvoiceNo = string.Empty;
                            tran.TotalAmount = request.CostOfRoom;
                            tran.TransactionType = AggragatorServiceType.DEBIT;
                            tran.IsBankTransaction = false;
                            tran.BankBranchCode = string.Empty;
                            tran.BankTransactionId = "0";
                            tran.CommisionId = 0;
                            tran.WalletAmount =request.CostOfRoom;
                            tran.ServiceTaxRate = 0;
                            tran.ServiceTax = "0";
                            tran.WalletServiceId = 151;
                            tran.SenderId = sender.WalletUserId;
                            tran.ReceiverId = request.HotelId;
                            tran.AccountNo ="To Hotel";
                            tran.TransactionId = "0";
                            tran.IsAdminTransaction = false;
                            tran.IsActive = true;
                            tran.IsDeleted = false;
                            tran.CreatedDate = DateTime.UtcNow;
                            tran.UpdatedDate = DateTime.UtcNow;
                            tran.TransactionTypeInfo = (int)TransactionTypeInfo.Resort;
                            tran.TransactionStatus = (int)TransactionStatus.Completed;
                            tran.MerchantCommissionAmount = "0";//!string.IsNullOrEmpty(merchantCommission.CommissionAmount) ? merchantCommission.CommissionAmount : "0";
                            tran.CommisionAmount = "0"; //!string.IsNullOrEmpty(commission.CommissionAmount) ? commission.CommissionAmount : "0";
                            tran.VoucherCode = string.Empty;
                            tran.MerchantCommissionId = 0;
                            tran.UpdatedOn = DateTime.Now;
                            tran.BenchmarkCharges = "0";
                            tran.FlatCharges = "0";
                            tran.CommisionPercent = "0";

                            tran = await _payMoneyRepository.SaveWalletTransaction(tran);
                            #endregion
                           

                            #region Update Sender Balance
                            sender.CurrentBalance = Convert.ToString(Math.Round((Convert.ToDecimal(sender.CurrentBalance) - (Convert.ToDecimal(request.CostOfRoom))), 2));
                            #endregion



                            //  db.SaveChanges();                           
                            await _payMoneyRepository.UpdateWalletUser(sender);

                            var req = new HotelBooking
                            {
                                HotelId = request.HotelId,
                                CostOfRoom = Convert.ToDecimal(request.CostOfRoom),
                                FirstName = request.FirstName,
                                LastName = request.LastName,
                                NoOfGuest = request.NoOfGuest,
                                WalletUserId = request.WalletUserId,
                                Address = request.Address,
                                CreatedDate = DateTime.UtcNow,
                                UpdatedDate = DateTime.UtcNow,
                                IsActive = true,
                                IsDeleted = false
                            };
                            bool result = await _resortRepository.SaveHotelBooking(req);
                            if (result)
                            {
                                res.CurrentBalance = sender.CurrentBalance;
                                res.StatusCode = (int)WalletTransactionStatus.SUCCESS;
                                res.TransactionDate = DateTime.UtcNow;
                                res.TransactionAmount = request.CostOfRoom;
                                res.TransactionId = tran.WalletTransactionId;
                                res.Message = "Pay Money successfully.";
                                res.ToMobileNo = "To Hotel";
                                res.TransactionAmount = request.CostOfRoom;
                                res.Amount= request.CostOfRoom;
                                res.RstKey = 1;

                                try
                                {//--------sending mail on success transaction--------
                                   // var receiverdetail = await _walletUserRepository.GetUserDetailById(receiver.WalletUserId);
                                    var senderdetail = await _walletUserRepository.GetUserDetailById(sender.WalletUserId);
                                    string filename = CommonSetting.resortBooking;
                                    var body = _sendEmails.ReadEmailformats(filename);
                                    body = body.Replace("$$FirstName$$", data.FirstName + " " + data.LastName);
                                    body = body.Replace("$$DisplayContent$$", "Resort Booking");
                                    body = body.Replace("$$customer$$", _hotel.HotelName);
                                    body = body.Replace("$$amount$$", "XOF " + request.CostOfRoom);                        
                                    body = body.Replace("$$TransactionId$$", Convert.ToString(tran.WalletTransactionId));
                                    body = body.Replace("$$HotelBookingId$$", Convert.ToString(req.Id));

                                    var _emailModel = new EmailModel
                                    {
                                        TO = senderdetail.EmailId,
                                        Subject = "Hotel Booking Successfull",
                                        Body = body
                                    };
                                    _sendEmails.SendEmail(_emailModel);
                                }
                                catch
                                {

                                }
                            }
                        }
                    }
                }
            }
            return res;
        }
    }
}
