using Ezipay.ViewModel.AdminViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.Admin.TransactionLimitAU
{
    public interface ITransactionLimitAUService
    {
        Task<bool> InsertTransactionLimitAU(TransactionLimitAURequest request);

        Task<List<TransactionLimitAUResponse>> GetTransactionLimitAUResponseList();

        Task<TransactionLimitAUResponse> GetTransactionLimitAUMessage();
        Task<TransactionLimitAUResponse> CheckTransactionLimitAU(string walletuserid);

    }
}
