using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezeePay.Utility.Enums
{
    public enum TransactionKeyStatus
    {
        EXPIRED=0,
        VALID = 1,
        INVALID_KEY=2,
        INVALID_PARAMETERS=3,
        INVALID_REQUEST=4
    }
    
}
