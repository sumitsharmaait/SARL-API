using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.WalletSetting
{
    public interface IWalletSettingRepository
    {
        /// <summary>
        /// Check if transaction is allowed
        /// </summary>
        /// 
        /// <returns></returns>
        bool IsTransactionAllowed();
    }
}
