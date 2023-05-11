using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ezipay.Database;
using Ezipay.ViewModel.AdminViewModel;

namespace Ezipay.Repository.AdminRepo.SubAdmin
{
    public interface ISubAdminRepository
    {
        Task<List<SubadminList>> GetSubAdminList(SubadminListRequest request);
        List<NavigationList> GetNavigationBySubAdmin(long subadminId);
        Task<int> isEmailOrPhoneExist(long subadminId, string mobileNo, string emailId);
        Task SaveSubAdmin(List<AdminPermission> request, WalletUser walletUser, long userId, AdminPasswordHistory adminPassword);
        Task<bool> DeleteSubadmin(UserDeleteRequest request);
        //Task<bool> UpdateSubAdmin(long userId, WalletUser walletUser);
        //Task<WalletUser> GetSubAdminById(long userId);
    }
}
