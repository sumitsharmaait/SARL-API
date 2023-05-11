using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezeePay.Utility.Enums
{
    public enum DeviceTypes
    {
        ANDROID = 1,
        IOS = 2,
        Web = 3,
        Admin
    }

    public enum PushType
    {
        PAYMONEY = 1,
        MAKEPAYMENTREQUEST = 2,
        ACCEPTMAKEPAYMENTREQUEST = 3,
        REJECTMAKEPAYMENTREQUEST = 4,
        MERCHANTPAYMENT = 5,
        ADDMONEY = 6,
        TRANSFERTOBANK = 7,
        PAYSERVICES = 8,
        LOGOUT = 9,
        ANONYMOUSUSER = 10
    }

    public enum UserExistanceStatus
    {
        BothNotExist = 1,
        MobileExist = 2,
        EmailExist = 3,
        BothExist = 4,
        ExceptionOccured = 5,
        UserRegistered
    }

    public enum WalletLoginStatus
    {
        Success = 1,
        InvalidPassword = 2,
        InvalidCredentials = 3
    }

    public enum WalletUserTypes
    {
        AppUser = 1,
        AdminUser = 2,
        Merchant = 3,
        Subadmin = 4
    }

    public enum EnumMerchantType
    {
        Normal,
        OnBoard
    }

    public enum UserSignUpStatus
    {
        Registered = 1,
        DuplicateUser = 2,
        NotRegistered = 3,
        NotVerified = 4,
        MailNotSent = 5
    }
    public enum UserProfileUpdated
    {
        EmailAlreadyExist = 1,
        EmailSend = 2,
        ProfileUpdated,
        Profile_Not_Updated,
        EmailAlreadyverified,
        NameUpdated,
    }
    public enum OtpStatus
    {
        INVALID_OTP = 1,
        VALID_OTP = 2
    }
    public enum LoginTypes
    {
        Tutorial = 1,
        Home = 2,
        ChangePassword = 3
    }
    public enum LoginStatusType
    {
        SUCCESS = 1,
        FAILED = 2,
        EMAILNOTEXIST = 3,
        EMAILNOTVERIFIED = 4,
        INACTIVE = 5,
        INVALID_USER_TYPE = 6,
        USERNOTEXIST = 7
    }
    public enum DocumentStatus
    {
        NoDocuments,
        Pending,
        Verified,
        Rejected,
        NotOk
    }

    public enum EnumDocType
    {
        Address = 1,
        ShareholderId = 2,
        ShareholderImage = 3
    }
}
