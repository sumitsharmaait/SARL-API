using Ezipay.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.PaymentGetway
{
    public interface IPaymentGetwayService
    {
        Task<SessionInfoResponse> GetWalletSessionInfo(SessionInfoRequest request);
        Task<int> PayMoney(PGPayMoneyVM request);
        Task<CashInCashOutResponse> CashInCashOut(CashInCashOutRequest request);
    }
}
