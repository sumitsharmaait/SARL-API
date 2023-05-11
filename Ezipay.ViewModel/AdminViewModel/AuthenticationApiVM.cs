using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ezipay.ViewModel.AdminViewModel
{
    public class LoginResponse
    {
        public LoginResponse()
        {
            this.AdminId = 0;
            this.LastName = this.FirstName = this.Email = string.Empty;
            this.IsSuccess = false;
            this.PrivateKey = string.Empty;
            this.PublicKey = string.Empty;
            this.RstKey = 0;
            this.PasswordExpiryDay = 0;
        }
        public long AdminId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Token { get; set; }
        public bool IsSuccess { get; set; }
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
        public int RstKey { get; set; }
        public int PasswordExpiryDay { get; set; }
    }
    public class LoginRequest
    {
        [Required]
        public string EmailId { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string DeviceUniqueId { get; set; }

    }
    public class ChangePasswordRequest
    {

        public ChangePasswordRequest()
        {
            this.CurrentPassword = this.NewPassword = string.Empty;
        }
        [Required]
        public string CurrentPassword { get; set; }
        [Required]
        public string NewPassword { get; set; }
        [CompareAttribute("NewPassword")]
        [Required]
        public string ConfirmPassword { get; set; }

        public long AdminId { get; set; } //log key
    }
    public class NavigationResponse
    {
        public List<Navigations> NavigationList { get; set; }
    }

    public class ChangePasswordResponse
    {
        public ChangePasswordResponse()
        {
            this.IsSuccess = false;
            this.Message = string.Empty;
        }

        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public int RstKey { get; set; }
    }

    public class Navigations
    {
        public Navigations()
        {
            FunctionList = new List<ModuleFunctionModel>();
        }
        public long NavigationId { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string NavigationName { get; set; }
        public string Icon { get; set; }
        public string NgRoutes { get; set; }
        public string NgIcon { get; set; }
        public string Functions { get; set; }
        public List<ModuleFunctionModel> FunctionList { get; set; }
    }

    public class NavigationsRequest
    {
        public long AdminId { get; set; }

    }
    public class GetPasswordExpiryResponse
    {
        public string WalletUserId { get; set; }
        public int PasswordDays { get; set; }
    }
}
