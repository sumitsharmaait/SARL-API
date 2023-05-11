using Ezipay.ViewModel.PayMoneyViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.PayMoney
{
    public interface IPayMoneyService
    {
        Task<WalletTransactionResponse> PayMoney(WalletTransactionRequest request,string sessionToken);
    }
}
