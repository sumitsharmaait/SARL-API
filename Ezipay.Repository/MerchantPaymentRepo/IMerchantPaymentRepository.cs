using Ezipay.Database;
using Ezipay.ViewModel.CardPaymentViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.MerchantPaymentRepo
{
    public interface IMerchantPaymentRepository
    {
        Task<SetTransactionLimit> GetTransactionLimitForPayment(long walletUserId);
        Task<WalletTransaction> SaveWalletTransaction(WalletTransaction request);
        Task<WalletTransactionDetail> SaveWalletTransactionDetail(WalletTransactionDetail request);

        Task<CommisionHistory> SaveCommisionHistory(CommisionHistory request);
        Task<WalletUser> UpdateWalletUser(WalletUser request);             
    }
}
