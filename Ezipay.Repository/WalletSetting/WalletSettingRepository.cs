using Ezipay.Repository.WalletSetting;
using Ezipay.Database;
using Ezipay.Utility.Extention;
using System;
using System.Linq;

namespace Ezipay.Repository.WalletSetting
{
    /// <summary>
    /// WalletTransactionRepository
    /// </summary>
    public class WalletSettingRepository : IWalletSettingRepository
    {
        public WalletSettingRepository()
        {

        }

        /// <summary>
        /// Check if transaction is allowed
        /// </summary>
        /// 
        /// <returns></returns>
        public bool IsTransactionAllowed()
        {
            bool objResponse = false;
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    var setting = db.WalletSettings.Where(w => w.SettingKey == "TransactionsEnabled").FirstOrDefault();
                    if (setting != null)
                    {
                        var enabled = Convert.ToInt32(setting.SettingValue);
                        
                        objResponse = Convert.ToBoolean(enabled);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("WalletSettingRepository.cs", "IsTransactionAllowed", ex.Message);
            }
            return objResponse;
        }
        
    }
}
