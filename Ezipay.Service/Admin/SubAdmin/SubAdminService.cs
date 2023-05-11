using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ezeePay.Utility.Enums;
using Ezipay.Database;
using Ezipay.Repository.AdminRepo;
using Ezipay.Repository.AdminRepo.SubAdmin;
using Ezipay.Repository.TokenRepo;
using Ezipay.Utility.common;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.common;

namespace Ezipay.Service.Admin.SubAdmin
{
    public class SubAdminService : ISubAdminService
    {
        private IUserApiRepository _userApiRepository;
        private ISubAdminRepository _subAdminRepository;

        public SubAdminService()
        {
            _userApiRepository = new UserApiRepository();
            _subAdminRepository = new SubAdminRepository();
        }

        public async Task<bool> DeleteSubadmin(UserDeleteRequest request)
        {
            return await _subAdminRepository.DeleteSubadmin(request);
        }

        public async Task<bool> EnableDisableSubAdmin(SubAdminManageRequest request)
        {
            var user = await _userApiRepository.GetUserById(request.UserId);
            if (user != null)
            {
                user.IsActive = request.IsActive;
                user.UpdatedDate = DateTime.UtcNow;
                int rowAffected = await _userApiRepository.UpdateUser(user);
                if (rowAffected > 0)
                {
                    var repo = new TokenRepository();
                    repo.RemoveLoginSession(request.UserId);
                    repo.SendLogoutPush(request.UserId);
                    return true;
                }
            }
            return false;
        }

        public async Task<SubadminListResponse> GetSubAdmins(SubadminListRequest request)
        {
            var result = new SubadminListResponse();

            result.SubadminList = await _subAdminRepository.GetSubAdminList(request);
            result.SubadminList.ForEach(x =>
            {
                x.NavigationList = _subAdminRepository.GetNavigationBySubAdmin(x.SubadminId);
            });

            if (result.SubadminList.Count > 0)
            {
                result.TotalCount = result.SubadminList[0].TotalCount;
            }
            result.CompleteNavigationList = _subAdminRepository.GetNavigationBySubAdmin(0);
            result.SubadminList.ForEach(x =>
            {
                if (!string.IsNullOrWhiteSpace(x.Functions))
                {
                    var list = x.Functions.Split(',').Select(a => Convert.ToInt64(a)).ToList();
                    list.ForEach(f =>
                    {
                        x.FunctionList.Add(
                            new ModuleFunctionModel
                            {
                                Id = f,
                                FunctionName = Enum.GetName(typeof(EnumModuleFunctionType), f)
                            });
                    });
                }
                // x.FunctionList = GetModuleFunction(x.Id, dbConnection);
            });
            return result;
        }

        //public async Task<SubadminSaveResponse> SaveSubAdmins(SubAdminRequest request)
        //{
        //    var response = new SubadminSaveResponse();

        //    var adminKeyPair = AES256.AdminKeyPair;
        //    string Mobile = AES256.Encrypt(adminKeyPair.PublicKey, request.MobileNo);
        //    string EmailId = AES256.Encrypt(adminKeyPair.PublicKey, request.EmailId);

        //    int validKey = await _subAdminRepository.isEmailOrPhoneExist(request.SubadminId, Mobile, EmailId);

        //    if (validKey == 0)
        //    {
        //        var req = new AdminPermission { };
        //        await _subAdminRepository.SaveSubAdmin(request);
        //        response.statusCode = (int)UserExistanceStatus.BothNotExist;
        //    }
        //    else
        //    {
        //        if (validKey == 1)
        //        {
        //            response.statusCode = (int)UserExistanceStatus.MobileExist;
        //        }
        //        else
        //        {
        //            response.statusCode = (int)UserExistanceStatus.EmailExist;
        //        }
        //    }

        //    return response;
        //}

        public async Task<SubadminSaveResponse> SaveSubAdmins(SubAdminRequest request)
        {
            var response = new SubadminSaveResponse();
            var req = new List<AdminPermission>();
            var _walletUser = new WalletUser();
            var hashedObject = new EncryptionSha256Response();
            var adminKeyPair = AES256.AdminKeyPair;
            string Mobile = AES256.Encrypt(adminKeyPair.PublicKey, request.MobileNo);
            string EmailId = AES256.Encrypt(adminKeyPair.PublicKey, request.EmailId);
            if (request.Password != null)
            {
                 hashedObject = SHA256ALGO.HashPassword(request.Password);
            }           
            var userKeyPair = AES256.UserKeyPair();

            int validKey = await _subAdminRepository.isEmailOrPhoneExist(request.SubadminId, Mobile, EmailId);

            if (validKey == 0)
            {
                _walletUser.UserType = (int)WalletUserTypes.Subadmin;
                _walletUser.StdCode = request.IsdCode;
                _walletUser.CurrencyId = (int)CurrencyTypes.Ghanaian_Cedi;
                _walletUser.EmailId = EmailId;
                _walletUser.FirstName = AES256.Encrypt(userKeyPair.PublicKey, request.FirstName);
                _walletUser.LastName = AES256.Encrypt(userKeyPair.PublicKey, request.LastName);
                _walletUser.HashedPassword = hashedObject.HashedPassword;
                _walletUser.HashedSalt = hashedObject.SlatBytes;
                _walletUser.MobileNo = Mobile;
                _walletUser.PrivateKey = userKeyPair.PrivateKey;
                _walletUser.PublicKey = userKeyPair.PublicKey;
                _walletUser.IsOtpVerified = true;
                _walletUser.CurrentBalance = "0";
                _walletUser.Otp = string.Empty;
                _walletUser.QrCode = string.Empty;
                _walletUser.AdminUserId = 0;
                _walletUser.IsEmailVerified = true;
                _walletUser.DeviceToken = string.Empty;
                _walletUser.DeviceType = 3;
                _walletUser.IsActive = true;
                _walletUser.IsDeleted = false;
                _walletUser.CreatedDate = DateTime.UtcNow;
                _walletUser.UpdatedDate = DateTime.UtcNow;
                _walletUser.IsFirstTimeUser = false;
                _walletUser.IsTemporaryPassword = false;
                _walletUser.ProfileImage = string.Empty;
                _walletUser.IsNotification = true;
                _walletUser.UserAddress = string.Empty;

                request.NavigationList.ForEach(x =>
                {
                    var fnList = x.FunctionList.Select(f => f.Id).ToList();
                    string fnStr = string.Join(",", fnList);
                    req.Add(
                     new AdminPermission
                     {
                         UserId = request.SubadminId,
                         NavigationId = x.NavigationId,
                         Functions = fnStr,
                         IsActive = true,
                         IsDeleted = false,
                         CreatedDate = DateTime.UtcNow,
                         UpdatedDate = DateTime.UtcNow
                     }
                    );
                });

                var reqadminpasswordhistory = new AdminPasswordHistory
                {
                    WalletUserId = 0,
                    Password = hashedObject.HashedPassword,
                    Hashed = hashedObject.SlatBytes,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };

                await _subAdminRepository.SaveSubAdmin(req, _walletUser, Convert.ToInt32(req[0].UserId), reqadminpasswordhistory);


                response.statusCode = (int)UserExistanceStatus.BothNotExist;
                response.RstKey = 1;
            }
            else
            {
                if (validKey == 1)
                {
                    response.statusCode = (int)UserExistanceStatus.MobileExist;
                    response.RstKey = 2;
                }
                else
                {
                    response.statusCode = (int)UserExistanceStatus.EmailExist;
                    response.RstKey = 3;
                }
            }

            return response;
        }
    }
}
