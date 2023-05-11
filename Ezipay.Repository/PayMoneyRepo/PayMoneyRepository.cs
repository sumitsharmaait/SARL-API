using ezeePay.Utility.Enums;
using Ezipay.Database;
using Ezipay.Utility.Extention;
using Ezipay.ViewModel.CardPaymentViewModel;
using Ezipay.ViewModel.PayMoneyViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.PayMoneyRepo
{
    public class PayMoneyRepository : IPayMoneyRepository
    {
        public async Task<WalletTransactionResponse> PayMoney(WalletTransactionRequest request)
        {
            var response = new WalletTransactionResponse();

            return response;
        }

        public async Task<TransactionLimitResponse> GetTransactionLimitForPayment(long walletUserId)
        {
            var result = new TransactionLimitResponse();
            var userId = Convert.ToString(walletUserId);
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    //response = db.SetTransactionLimits.Where(x => x.UserId == userId).FirstOrDefault();
                    result = await db.Database.SqlQuery<TransactionLimitResponse>("exec usp_SelectTransactionLimit @userid",
                  new object[]
                  {
                        new SqlParameter("@userid",userId)
                  }
                  ).FirstOrDefaultAsync();
                }
            }
            catch
            {

            }
            return result;
        }

        public async Task<TotalTransactionCountResponse> GetTotalTransactionCount(long walletUserId)
        {
            var result = new TotalTransactionCountResponse();
            var userId = Convert.ToString(walletUserId);
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    //response = db.SetTransactionLimits.Where(x => x.UserId == userId).FirstOrDefault();
                    result = await db.Database.SqlQuery<TotalTransactionCountResponse>("exec usp_GetTotalTransaction @WalletuserId",
                  new object[]
                  {
                        new SqlParameter("@WalletuserId",userId)
                  }
                  ).FirstOrDefaultAsync();
                }
            }
            catch(Exception ex)
            {

            }
            return result;
        }

        public async Task<TransactionHistoryAddMoneyReponse> GetAllTransactionByDate(long WalletUserId)
        {
            var response = new TransactionHistoryAddMoneyReponse();
            var userId = Convert.ToString(WalletUserId);
            try
            {

                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    if (WalletUserId > 0)
                    {
                        response = await db.Database.SqlQuery<TransactionHistoryAddMoneyReponse>("exec usp_GetAllTransactionByDate @UserId",
                         new SqlParameter("@UserId", userId)
                         ).FirstOrDefaultAsync();
                    }
                }
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("MasterDataRepository.cs", "GetAllTransactionsAddMoney");
            }

            return response;
        }

        public async Task<int> GetServiceId()
        {
            int WalletServiceId = 0;
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                WalletServiceId = (int)await db.WalletServices.Where(x => x.ServiceCategoryId == (int)WalletTransactionSubTypes.EWallet_To_Ewallet_Transactions_PayMoney).Select(x => x.WalletServiceId).FirstOrDefaultAsync();
            }
            return WalletServiceId;
        }

        public async Task<int> GetMerchantId()
        {
            int MerchantCommissionServiceId = 0;
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                MerchantCommissionServiceId = (int)await db.WalletServices.Where(x => x.ServiceCategoryId == (int)WalletTransactionSubTypes.Merchants).Select(x => x.WalletServiceId).FirstOrDefaultAsync();
            }
            return MerchantCommissionServiceId;
        }

        public async Task<bool> IsMerchant(long walletUserId)
        {
            var response = new bool();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                response = await db.WalletServices.AnyAsync(x => x.MerchantId == walletUserId);
            }
            return response;
        }

        public async Task<bool> IsService(long MerchantCommissionServiceId, long WalletUserId)
        {
            bool response = false;
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    response = await db.WalletServices.AnyAsync(x => x.WalletServiceId == MerchantCommissionServiceId && x.MerchantId == WalletUserId);
                }
            }
            catch
            {

            }
            return response;
        }


        public async Task<MerchantCommisionMaster> MerchantCommisionMasters(long MerchantCommissionServiceId)
        {
            var response = new MerchantCommisionMaster();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    response = await db.MerchantCommisionMasters.Where(x => x.WalletServiceId == MerchantCommissionServiceId && (bool)x.IsActive).FirstOrDefaultAsync();
                }
            }
            catch
            {

            }
            return response;
        }

        public async Task<WalletTransaction> SaveWalletTransaction(WalletTransaction request)
        {
            // var response = new WalletTransaction();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    db.WalletTransactions.Add(request);
                    await db.SaveChangesAsync();
                }
            }
            catch
            {

            }
            return request;
        }


        public async Task<WalletTransactionDetail> SaveWalletTransactionDetail(WalletTransactionDetail request)
        {
            // var response = new WalletTransactionDetail();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    db.WalletTransactionDetails.Add(request);
                    await db.SaveChangesAsync();
                }
            }
            catch
            {

            }
            return request;
        }

        public async Task<CommisionHistory> SaveCommisionHistory(CommisionHistory request)
        {
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    db.CommisionHistories.Add(request);
                    await db.SaveChangesAsync();
                }
            }
            catch
            {

            }
            return request;
        }

        public async Task<WalletUser> UpdateWalletUser(WalletUser request)
        {
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    db.Entry(request).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                }
            }
            catch
            {

            }
            return request;
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

        public async Task<TransactionInitiateRequest> GetTransactionInitiateRequestMerchantDetail(long Id, string InvoiceNumber)
        {
            var result = new TransactionInitiateRequest();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    result = await db.TransactionInitiateRequests.Where(x => x.Id == Id && x.InvoiceNumber == InvoiceNumber).FirstOrDefaultAsync();
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
    }
}
