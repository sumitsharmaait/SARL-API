using Ezipay.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.AirtimeViewModel;

namespace Ezipay.Repository.MobileMoneyRepo
{
    public interface IMobileMoneyRepository
    {
        Task<WalletTransaction> MobileMoneyService(WalletTransaction request);
        Task<WalletUser> GetData(long walletuserid);
        Task<TransactionInitiateRequest> SaveTransactionInitiateRequest(TransactionInitiateRequest request);
        Task<TransactionInitiateRequest> GetTransactionInitiateRequest(long id);
        Task<TransactionInitiateRequest> UpdateTransactionInitiateRequest(TransactionInitiateRequest request);
        Task<AdminMobileMoneyLimitResponse> VerifyMobileMoneyLimit(AdminMobileMoneyLimitRequest request);

        Task<int> SaveMobileMoneySenderDetailsRequest(PayMoneyAggregatoryRequest request);
        Task<MobileMoneySenderDetail> VerifySenderIdNumberExistorNot(MobileMoneySenderDetailrequest request);

    }
}
