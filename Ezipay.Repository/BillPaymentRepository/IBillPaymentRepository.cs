using Ezipay.Database;
using Ezipay.ViewModel.AirtimeViewModel;
using Ezipay.ViewModel.BillViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.BillPaymentRepository
{
    public interface IBillPaymentRepository
    {
        Task<WalletTransaction> InsertWalletTransaction(WalletTransaction walletTransaction);
        Task<DetailForBillPaymentVM> GetDetailForBillPayment(BillPayMoneyAggregatoryRequest request);
        Task<TransactionInitiateRequest> SaveTransactionInitiateRequest(TransactionInitiateRequest request);
        Task<TransactionInitiateRequest> GetTransactionInitiateRequest(long request);
        Task<TransactionInitiateRequest> UpdateTransactionInitiateRequest(TransactionInitiateRequest request);
    }
}
