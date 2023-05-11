using Ezipay.ViewModel.MerchantPaymentViewModel;
using Ezipay.ViewModel.PayMoneyViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.MerchantPayment
{
    public interface IMerchantPaymentService
    {
        Task<WalletTransactionResponse> MerchantPayment(MerchantTransactionRequest request, string token, long WalletUserId = 0 );
        Task<WalletTransactionResponse> MerchantPaymentEzipayPartner(MerchantTransactionForThirdPartyRequest requestModel);
    }
}
