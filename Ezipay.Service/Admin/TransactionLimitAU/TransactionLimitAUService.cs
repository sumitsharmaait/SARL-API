using Ezipay.Repository.AdminRepo.TransactionLimitAU;
using Ezipay.ViewModel.AdminViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ezipay.Service.Admin.TransactionLimitAU
{
    public class TransactionLimitAUService : ITransactionLimitAUService
    {
        private readonly ITransactionLimitAURespository _TransactionLimitAURespository;

        public TransactionLimitAUService()
        {
            _TransactionLimitAURespository = new TransactionLimitAURespository();
        }


        public async Task<bool> InsertTransactionLimitAU(TransactionLimitAURequest request)
        {
            var result = false;

            var entity = new Database.TransactionLimitAU
            {
                FromDateTime = request.FromDateTime,
                ToDateTime = request.ToDateTime,
                Message = request.Message,
                Amount = request.Amount,
                Createddate = DateTime.UtcNow

            };

            int rowAffected = await _TransactionLimitAURespository.InsertTransactionLimitAU(entity);
            if (rowAffected > 0)
            {
                result = true;
            }

            return result;
        }



        public async Task<List<TransactionLimitAUResponse>> GetTransactionLimitAUResponseList()
        {
            return await _TransactionLimitAURespository.GetTransactionLimitAUResponseList();
        }


        public async Task<TransactionLimitAUResponse> GetTransactionLimitAUMessage()
        {
            return await _TransactionLimitAURespository.GetTransactionLimitAUMessage();
        }

        public async Task<TransactionLimitAUResponse> CheckTransactionLimitAU(string walletuserid)
        {
            return await _TransactionLimitAURespository.CheckTransactionLimitAU(walletuserid);
        }


    }
}
