using Ezipay.Database;
using Ezipay.ViewModel.AirtimeViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.AirtimeRepo
{
    public interface IAirtimeRepository
    {
        Task<WalletTransaction> AirtimeServices(WalletTransaction Request, long WalletUserId = 0);
        Task<TransactionInitiateRequest> SaveTransactionInitiateRequest(TransactionInitiateRequest request);
        Task<TransactionInitiateRequest> GetTransactionInitiateRequest(long request);
        Task<TransactionInitiateRequest> UpdateTransactionInitiateRequest(TransactionInitiateRequest request);
    }
}
