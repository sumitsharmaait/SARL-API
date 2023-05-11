using Ezipay.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.AfroBasketRepo
{
    public class AfroBasketRepository:IAfroBasketRepository
    {
        public async Task<bool> IsSession(string token)
        {
            bool IsSuccess = false;
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    var user = await db.SessionTokens.Where(x => x.TokenValue == token).FirstOrDefaultAsync();
                    if (user != null)
                    {
                        user.IsDeleted = true;
                        db.SaveChanges();
                        IsSuccess = true;
                    }
                    else
                    {
                        IsSuccess = true;
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return IsSuccess;
        }

        public async Task<ViewUserList> GetUserDetailByEmail(string emailId)
        {
            var result = new ViewUserList();
            //using (var db = new DB_9ADF60_ewalletEntities())
            //{
            //    var res = db.usp_getUserByEmailId(emailId).FirstOrDefault();
            //    result.Country = res.Country;
            //    result.Currentbalance = res.Currentbalance;
            //    result.WalletUserId = res.WalletUserId;
            //    result.DeviceToken = res.DeviceToken;
            //    result.MobileNo = res.MobileNo;
            //    result.StdCode = res.StdCode;
            //    result.EmailId = res.EmailId;
            //}
            return result;
        }

        public async Task<AfroBasketData> SaveAfroBasketData(AfroBasketData request)
        {

            using (var db = new DB_9ADF60_ewalletEntities())
            {
                db.AfroBasketDatas.Add(request);
                await db.SaveChangesAsync();
            }
            return request;
        }

        public async Task<SessionToken> GetSessionToken(string token)
        {
            var result = new SessionToken();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                result = await db.SessionTokens.Where(x => x.TokenValue == token).FirstOrDefaultAsync();
            }
            return result;
        }

        public async Task<AfroBasketData> GetAfroBasketData(string securityCode)
        {
            var result = new AfroBasketData();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                result = await db.AfroBasketDatas.Where(x => x.SecurityCode == securityCode).FirstOrDefaultAsync();
            }
            return result;
        }

        public async Task<WalletTransaction> SaveWalletTransaction(WalletTransaction request)
        {

            using (var db = new DB_9ADF60_ewalletEntities())
            {
                db.WalletTransactions.Add(request);
                await db.SaveChangesAsync();
            }
            return request;
        }

        public async Task<int> AfroBasketBooking(AfroBasketVerifyData afroBasketVerifyData)
        {
            int result = 0;
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                db.AfroBasketVerifyDatas.Add(afroBasketVerifyData);
                result = await db.SaveChangesAsync();
            }
            return result;
        }

        public async Task<AfroBasketVerifyData> GetUserDetailById(long userId, string token)
        {
            var result = new AfroBasketVerifyData();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                result = await db.AfroBasketVerifyDatas.Where(x => x.AgentCode == userId && x.TokenId == token).FirstOrDefaultAsync();
            }
            return result;
        }

        public async Task<int> AfroBasketLogin(AfroBasketVerifyData _afroBasketVerifyData)
        {
            int result = 0;
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                db.AfroBasketVerifyDatas.Add(_afroBasketVerifyData);
                result = await db.SaveChangesAsync();
            }
            return result;
        }


        public async Task<TransactionInitiateRequest> SaveTransactionInitiateRequest(TransactionInitiateRequest request)
        {
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    db.TransactionInitiateRequests.Add(request);
                    int res = await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {

            }
            return request;
        }

        public async Task<TransactionInitiateRequest> GetTransactionInitiateRequest(long InvoiceNumber)
        {
            var result = new TransactionInitiateRequest();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    result = await db.TransactionInitiateRequests.Where(x => x.Id == InvoiceNumber).FirstOrDefaultAsync();
                }
            }
            catch (Exception ex)
            {

            }
            return result;
        }

        public async Task<TransactionInitiateRequest> UpdateTransactionInitiateRequest(TransactionInitiateRequest request)
        {
            var result = new TransactionInitiateRequest();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    db.Entry(request).State = EntityState.Modified;
                    int s = await db.SaveChangesAsync();
                    return request;
                }
            }
            catch (Exception ex)
            {

            }
            return result;
        }


        public async Task<WalletUser> GetWalletUser(long walletUserId, string EmailId)
        {
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                return await db.WalletUsers.Where(x => x.WalletUserId == walletUserId
                     && x.IsActive == true && x.IsDeleted == false && x.IsEmailVerified == true
                     && x.IsOtpVerified == true && x.EmailId == EmailId).FirstOrDefaultAsync();
            }
        }
    }
}
