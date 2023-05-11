using ezeePay.Utility.Enums;
using Ezipay.Database;
using Ezipay.Utility.Extention;
using Ezipay.ViewModel.CardPaymentViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.MerchantPaymentRepo
{
    public class MerchantPaymentRepository : IMerchantPaymentRepository
    {
        public async Task<SetTransactionLimit> GetTransactionLimitForPayment(long walletUserId)
        {
            var response = new SetTransactionLimit();
            var userId = Convert.ToString(walletUserId);
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    response = await db.SetTransactionLimits.Where(x => x.UserId == userId).FirstOrDefaultAsync();
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
            catch (Exception ex)
            {
                "MerchantPaymentController".ErrorLog("MerchantPaymentRepo.cs", "MerchantPaymentRepoSaveWalletTransaction", request.InvoiceNo + " " + ex.StackTrace + " " + ex.Message);
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
                    //   EntityState.Modified.(request)
                    db.Entry(request).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                }
            }
            catch
            {

            }
            return request;
        }
    }
}
