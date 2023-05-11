using Ezipay.Database;
using Ezipay.ViewModel.AdminViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.AdminRepo.TxnUpdate
{
    public interface ITxnUpdateRepository
    {
        Task<List<WalletTransaction>> GetWalletTxnPendingList();
        Task<int> UpdatePendingWalletTxn(WalletTxnRequest entity);
        Task<int> UpdateBankPendingWalletTxn(WalletTxnRequest request);
       
    }
}
