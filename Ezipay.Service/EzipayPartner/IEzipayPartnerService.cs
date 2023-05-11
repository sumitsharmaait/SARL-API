using Ezipay.ViewModel.AfroBasketViewModel;
using Ezipay.ViewModel.ThridPartyApiVIewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.EzipayPartner
{
    public interface IEzipayPartnerService
    {
        Task<AfroBasketVerificationResponse> PaymentWalletVerification(AfroBasketVerificationRequest requestModel);
        Task<AfroBasketPaymentVerifyResponse> PaymentByUserWallet(AfroBasketVerifyRequest requestModel, string headerToken);
        Task<VerifyResponse> DataVerification(VerifyAfroBasketRequest xmlRquest);
        Task<AfroBasketLoginResponse> LoginWithEzipayPartner(EzipayPartnerLoginRequest request, string sessionToken);
        Task<GetUserCurrentBalanceResponse> GetWalletUser(GetUserCurrentBalanceRequest request);
    }
}
