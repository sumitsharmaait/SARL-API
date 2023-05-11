namespace ezeePay.Utility.Enums
{
    public enum WalletTransactionStatus
    {
        SUCCESS = 1,
        FAILED = 2,
        SENDER_NOT_EXIST = 3,
        RECEIVER_NOT_EXIST = 4,
        SELF_WALLET = 5,
        OTHER_ERROR = 6
    }
    public enum WalletTransactionDetailTypes
    {
        DEBIT = 1,
        CREDIT = 2
    }
    public enum TransactionDetailType
    {
        Debit = 1,
        Credit = 2,
    }
    public enum WalletTransactionType
    {

        PAID = 1,
        RECEIVED = 2,
        ADDED = 3,
        All = 4
    }
    public enum DownloadFileType
    {
        PDF = 1,
        EXCEL = 2
    }
    public enum TransactionTypeInfo
    {
        AddedByCard = 1,
        AddedByMobileMoney = 2,
        Receive = 3,
        PaidByPayServices = 4,
        EWalletToEwalletTransactionsPayMoney = 5,
        EWalletToEwalletTransactionsMakePaymentRequest = 6,
        EWalletToEwalletTransactionsMerchantPayment = 7,
        EWalletToBankTransactions = 8,
        AddByBankToWallet = 9,
        Payforfilgth = 10,
        LendingAppTransaction = 11,
        Resort,
        AfroBasket,
        CashIn,
        CashOut,
        PaidByPayServicesMobileMoney,
        CashDepositToBank,
    }

    public enum TransactionStatus
    {
        NoResponse = 0,
        Completed = 1,
        Pending = 2,
        Rejected = 3,
        UnderProcess = 4,
        Failed = 5
    }
    public enum WalletTransactionSubTypes
    {
        Credit_TO_Debit_Cards = 1,
        Internet_Banking = 2,
        Mobile_Money_AddMoney = 3,
        EWallet_To_Ewallet_Transactions_PayMoney = 4,
        EWallet_To_Ewallet_Transactions_MakePaymentRequest = 5,
        EWallet_To_Bank_Transactions = 6,
        Telecom = 7,
        ISP = 8,
        Utility = 9,
        Mobile_Money_Pay_Services = 10,
        Merchants = 11,
        Cash_Deposit_To_Bank = 14,
        
        SeerbitCard = 16

    }
}
