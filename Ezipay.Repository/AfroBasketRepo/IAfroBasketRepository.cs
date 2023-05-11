using Ezipay.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.AfroBasketRepo
{
    public interface IAfroBasketRepository
    {
        Task<bool> IsSession(string token);
        Task<ViewUserList> GetUserDetailByEmail(string emailId);
        Task<AfroBasketData> SaveAfroBasketData(AfroBasketData request);
        Task<SessionToken> GetSessionToken(string token);
        Task<AfroBasketData> GetAfroBasketData(string securityCode);
        Task<WalletTransaction> SaveWalletTransaction(WalletTransaction request);
        Task<int> AfroBasketBooking(AfroBasketVerifyData afroBasketVerifyData);
        Task<AfroBasketVerifyData> GetUserDetailById(long userId, string token);
        Task<int> AfroBasketLogin(AfroBasketVerifyData _afroBasketVerifyData);

        Task<TransactionInitiateRequest> SaveTransactionInitiateRequest(TransactionInitiateRequest request);
        Task<TransactionInitiateRequest> GetTransactionInitiateRequest(long InvoiceNumber);
        Task<TransactionInitiateRequest> UpdateTransactionInitiateRequest(TransactionInitiateRequest request);
        Task<WalletUser> GetWalletUser(long walletUserId, string EmailId);
    }
}
