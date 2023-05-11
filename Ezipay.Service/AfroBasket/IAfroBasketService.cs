using Ezipay.ViewModel.AfroBasketViewModel;
using Ezipay.ViewModel.ThridPartyApiVIewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.AfroBasket
{
    public interface IAfroBasketService
    {      
        Task<AfroBasketVerificationResponse> PaymentWalletVerification(AfroBasketVerificationRequest requestModel);     
        Task<AfroBasketPaymentVerifyResponse> PaymentByUserWallet(AfroBasketVerifyRequest requestModel, string token);
        Task<VerifyResponse> DataVerification(VerifyAfroBasketRequest xmlRquest);
        Task<AfroBasketLoginResponse> AfroBasketLogin(string token);
    }
}
