using Ezipay.ViewModel.AdminViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.Admin.ReversalUBA
{
    public interface IReversalUBAService
    {
        Task<List<UBATxnVerificationResponse>> Getresponse(UBATxnVerificationRequest request);
    }
}
