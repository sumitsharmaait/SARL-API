using Ezipay.ViewModel.common;
using Ezipay.ViewModel.TokenViewModel;
using Ezipay.ViewModel.WalletUserVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.TokenService
{
    public interface ITokenService
    {
        Task<TokenResponse> GenerateToken(TokenRequest request);
        int ValidateAuthenticaion(ServiceAuthenticationRequest objReq);
        TempSessionResponse KeysBySessionToken();
        TempSessionResponse KeysByTempToken();
        Task<TempTokenResponse> GenerateTempToken(TempTokenRequest request);
    }
}
