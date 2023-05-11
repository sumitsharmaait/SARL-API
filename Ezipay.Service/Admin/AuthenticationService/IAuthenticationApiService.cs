using Ezipay.ViewModel.AdminViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.AdminService.AuthenticationService
{
    public interface IAuthenticationApiService
    {
        Task<LoginResponse> Login(LoginRequest request);

        Task<ChangePasswordResponse> ChangePassword(ChangePasswordRequest request, string token);
        Task<NavigationResponse> NavigationList(NavigationsRequest request);
        Task<bool> Logout();
        Task<string> CrrentUserDetail();
    }
}
