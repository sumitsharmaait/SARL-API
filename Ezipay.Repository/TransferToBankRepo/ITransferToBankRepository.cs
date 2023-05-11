using Ezipay.Database;
using Ezipay.ViewModel.AirtimeViewModel;
using Ezipay.ViewModel.TransferToBankViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.TransferToBankRepo
{
    public interface ITransferToBankRepository
    {
        
        Task<WalletService> GetWalletServices(string BankCode);
        Task<WalletService> SaveNewBanks(WalletService request);
        Task<TransferToBankResponseModel> PayMoneyTransferToBank(TransferToBankBeneficiaryNameRequest request);
        Task<WalletService> GetBankDetail(string DestBankCode);
        // ICredentials GetCredential(string passurl, string Username, string Password);
       
        Task<TransactionInitiateRequest> UpdateTransactionInitiateRequest(TransactionInitiateRequest request);
        Task<WalletTransaction> SaveWalletTransaction(WalletTransaction request);


        Task<List<IsdCodesResponse1>> GetTransferttobankCountryList();
        Task<List<BankListList>> GetBankList();
        Task<TransferToBankRequest1> SaveTransactionTransferToBankRequest(TransferToBankRequest1 request);
        Task<TransactionInitiateRequest> SaveTransactionInitiateRequest(TransactionInitiateRequest request);
        Task<TransferToBankResponse1> SaveTransactionTransferToBankResponse(TransferToBankResponse1 response);
        Task<int> UpdateCurrentBalance(string UpdatedCurrentBalance, long walletuserid);


        Task<WalletTransaction> WalletTransactionSave(WalletTransaction request);
        Task<TransactionInitiateRequest> GetTransactionInitiateRequest(long id);
        Task<int> SaveSenderDetailsRequest(PayMoneyAggregatoryRequest request);
        Task<List<senderIdTypetbl>> GetsenderidtypeList();
    }
}
