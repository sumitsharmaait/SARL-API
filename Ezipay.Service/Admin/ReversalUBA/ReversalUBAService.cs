using Ezipay.Repository.AdminRepo.ReversalUBA;
using Ezipay.ViewModel.AdminViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.Admin.ReversalUBA
{
    public class ReversalUBAService: IReversalUBAService
    {
        private readonly IReversalUBARepository _ReversalUBARepository;

        public ReversalUBAService()
        {
            _ReversalUBARepository = new ReversalUBARepository();
        }
        
        public async Task<List<UBATxnVerificationResponse>> Getresponse(UBATxnVerificationRequest request)
        {
           // request.CardNumber;

            string url = "http://172.19.2.214/centralised/api/up/transactionVerification/{transId}/{opco}/{passKey}";
            return await _ReversalUBARepository.Getresponse(request);
        }


    }
}
