using Ezipay.Database;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.AirtimeViewModel;
using Ezipay.ViewModel.CardPaymentViewModel;
using Ezipay.ViewModel.MerchantPaymentViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.CardPayment
{
    public interface ICardPaymentRepository
    {
        Task<CardAddMoneyResponse> CardPayment(CardPaymentRequest request, long walletUserId);
        Task<int> GetServiceId();
        string MerchantContent(MerchantTransactionRequest request, string TransactionId, string OrderNo, long? WalletUserId, int? TransactionStatus);

        string PayMoneyContent(PayMoneyContent request, string TransactionId, string OrderNo, long? WalletUserId, int? TransactionStatus);

        Task<WalletService> GetWalletService(string serviceName, int serviceCategoryId);

        Task<AddDuringPayRecord> MerchantContent(AddDuringPayRecord request);

        Task<AddDuringPayRecord> PayMoneyContent(AddDuringPayRecord request);

        Task<WalletTransaction> MobileMoneyForAddServices(WalletTransaction Request, long WalletUserId = 0);
        Task<AddDuringPayRecord> GetAddDuringPayRecord(int AddDuringPayRecordId, int TransactionStatus);

        Task<AddDuringPayRecord> UpdateAddDuringPayRecord(AddDuringPayRecord request);
        Task<CardPaymentRequest> SaveCardPaymentRequest(CardPaymentRequest request);

        Task<CardPaymentRequest> GetCardPaymentRequest(string vpc_OrderInfo, string vpc_MerchTxnRef);       
        Task<CardPaymentResponse> SaveCardPaymentResponse(CardPaymentResponse request);
        Task<int> GetWalletService();
        Task<WalletUser> GetAdminUser();
        Task<bool> IsWalletTransactions(long WalletUserId, string vpc_TransactionNo);
        Task<WalletTransaction> SaveWalletTransactions(WalletTransaction request);
        Task<WalletTransactionDetail> SaveWalletTransactionDetails(WalletTransactionDetail request);
        Task<AddDuringPayRecord> AddDuringPayRecords(string vpc_OrderInfo, string vpc_MerchTxnRef);
        Task<PayMoneyAggregatoryRequest> AddDuringPayRecord(string vpc_OrderInfo, string vpc_MerchTxnRef);
        Task<TransactionInitiateRequest> SaveTransactionInitiateRequest(TransactionInitiateRequest request);
        Task<TransactionInitiateRequest> GetTransactionInitiateRequest(long id);
        Task<TransactionInitiateRequest> GetTransactionInitiateRequest(string InvoiceNumber);
        Task<TransactionInitiateRequest> UpdateTransactionInitiateRequest(TransactionInitiateRequest request);
        Task<int> IsduplicateOrNotTransactionNo(string vpc_TransactionNo);

        Task<int> SaveNewCardNo(long WalletUserId, string CardNo, string NewCardImage, string flag);

        Task<int> SaveCardNo(string InvoiceNumber,long WalletUserId, string CardNo, string PayTranId, string Requestedamount, string Totalamount, string EmailId);        
        Task<int> UpdateNewCardNoResponseBankCode(string InvoiceNumber, long WalletUserId, string transactionID);

        Task<ThirdPartyPaymentByCard> SaveThirdPartyPaymentByCard(ThirdPartyPaymentByCard request);
        Task<MasterCardPaymentRequest> SaveMasterCardPaymentRequest(MasterCardPaymentRequest request);

        Task<MasterCardPaymentRequest> GetMasterCardPaymentRequest(string SuccessIndicator);
        
        Task<int> GetWalletTransactionsexist(long? WalletUserId, string InvoiceNo);
        Task<int> Checkrefundtoinvoiceno(long? WalletsenderId, long? WalletUserId, string InvoiceNo);
        Task<TransactionInitiateRequest> GetTxnInitiateRequest(string UserReferanceNumber);
        Task<List<TItxnresponse>> GetTransactionInitiateRequestjsonresponse();
        Task<int> Updatewebhookflutterflagsuccestxn();
        Task<int> Updatewebhookflutterflagsuccestxninvoiceno(string InvoiceNumber);
        Task<WalletTransaction> GetWalletTransaction(long? WalletUserId, string TransactionId);
        Task<WalletTransaction> UpdateWalletTransaction(WalletTransaction request);
        Task<Carduseinaddmoney> GetMasterCarduseinaddmoneyPaymentRequest(long WalletUserId, string InvoiceNumber);

        Task<int> CheckBeninUserTxnPerde(long walletUserId);
    }
}
