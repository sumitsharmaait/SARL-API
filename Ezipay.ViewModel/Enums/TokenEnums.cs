using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezeePay.Utility.Enums
{
    public enum TokenType
    {
        TempToken = 1,
        Session = 2,
        Any=3
    }
    public enum TokenStatusCode
    {
        Invalid = 0,
        Success = 1,
        Failed = 2

    }
}
