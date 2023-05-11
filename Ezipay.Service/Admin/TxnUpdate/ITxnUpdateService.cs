using Ezipay.Database;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.DashBoardViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.Admin.TxnUpdate
{
    public interface ITxnUpdateService
    {
        Task<List<WalletTxnResponse>> GetWalletTxnPendingList();
      
        
        Task<bool> UpdatePendingWalletTxn(WalletTxnRequest request);
        Task<bool> UpdateBankPendingWalletTxn(WalletTxnRequest request);
       
        //Task<bool> InsertAdminMobileMoneyLimit(AdminMobileMoneyLimitRequest request);
    }
}
