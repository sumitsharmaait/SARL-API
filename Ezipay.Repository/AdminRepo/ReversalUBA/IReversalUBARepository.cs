using Ezipay.ViewModel.AdminViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.AdminRepo.ReversalUBA
{
    public interface IReversalUBARepository
    {
        Task<List<UBATxnVerificationResponse>> Getresponse(UBATxnVerificationRequest request);
    }
}
