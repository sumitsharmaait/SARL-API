using Ezipay.ViewModel.AdminViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ezipay.Repository.AdminRepo.TransactionLimitAU
{
    public interface ITransactionLimitAURespository
    {
        Task<int> InsertTransactionLimitAU(Database.TransactionLimitAU entity);
      
        Task<List<TransactionLimitAUResponse>> GetTransactionLimitAUResponseList();


        Task<TransactionLimitAUResponse> GetTransactionLimitAUMessage();

        
        Task<TransactionLimitAUResponse> CheckTransactionLimitAU(string walletuserid);

    }
}
