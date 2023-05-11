using Ezipay.Database;
using Ezipay.ViewModel.AdminViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.AdminRepo.AuthenticationApiRepo
{
    public interface IAuthenticationApiRepository
    {
        Task<WalletUser> Login(LoginRequest request);
        Task<ChangePasswordResponse> ChangePassword(ChangePasswordRequest request, string TokenValue);
        Task<bool> GetPasswords(long walletUserId, string password);
        Task<NavigationResponse> NavigationList(NavigationsRequest request);
        Task<bool> Logout(string token);
        Task<string> CrrentUserDetail();
        Task<GetPasswordExpiryResponse> GetPasswordExpiry(long request);

        Task<int> InsertWrongPassword(WrongPassword wrongPassword);
        Task<WrongPassword> GetWrongPasswordCount(long WalletUserId);
        Task<int> DeleteWrongPassword(long WalletUserId);

    }
}
