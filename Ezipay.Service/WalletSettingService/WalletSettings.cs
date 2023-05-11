
using Ezipay.Repository.WalletSetting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.WalletSettingService
{
    public class WalletSettings : IWalletSettings
    {
        private IWalletSettingRepository _walletSettingRepository;

        public WalletSettings()
        {
            _walletSettingRepository = new WalletSettingRepository();
        }
        public bool IsTransactionAllowed()
        {
            return _walletSettingRepository.IsTransactionAllowed();
        }
    }
}
