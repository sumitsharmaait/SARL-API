using Ezipay.Database;
using Ezipay.ViewModel.TokenViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.TokenRepo
{
    public interface ITokenRepository
    {
        Task<int> GetPreviousSessionToken(long walletUserId, DateTime expiredOn);

        Task<TempTokenResponse> GenerateTempToken(TempTokenRequest request);

        Task<string> GetDeviceUniqueIdByTempToken(string TokenValue);

        int ValidateAuthenticaion(ServiceAuthenticationRequest objReq);

        Task<string> ValidateMacAddress(string DeviceUniqueId);

        Task<SessionResponse> GetWalletUserIdBySession();

        TempSessionResponse KeysByTempToken();

        Task<TempSessionResponse> KeysByTempToken(string token);

        TempSessionResponse KeysBySessionToken();

        Task<TempSessionResponse> KeysBySessionToken(string token);

        Task<bool> WebLogout(ChatModel model);

        void RemoveLoginSession(long WalletUserId);

        Task<TokenResponse> GenerateToken(TokenRequest request);
    }
}
