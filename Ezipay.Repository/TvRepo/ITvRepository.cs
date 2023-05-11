using Ezipay.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.TvRepo
{
    public interface ITvRepository
    {
        Task<WalletTransaction> TvService(WalletTransaction Request, long WalletUserId = 0);
    }
}
