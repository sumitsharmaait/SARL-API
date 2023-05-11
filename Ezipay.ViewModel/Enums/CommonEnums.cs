using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezeePay.Utility.Enums
{
    public enum AppServiceTypes
    {
        WALLET_SERVICE = 1,
        PAY_SERVICE = 2
    }
    public enum CurrencyTypes
    {
        Ghanaian_Cedi = 1
    }
    public enum CallBackRequestStatus
    {
        Pending = 1,
        Accepted,
        Completed,
        Rejected
    }
    public enum EnumModuleFunctionType
    {
        [Description("View")]
        View = 1,
        [Description("Add")]
        Add,
        [Description("Edit")]
        Edit,
        [Description("Block")]
        Block,
        [Description("Unblock")]
        Unblock,
        [Description("Delete")]
        Delete,
        [Description("Export")]
        Export
    }
}
