using Ezipay.Database;
using Ezipay.ViewModel.AirtimeViewModel;
using Ezipay.ViewModel.BillViewModel;
using Ezipay.ViewModel.CardPaymentViewModel;
using Ezipay.ViewModel.PayMoneyViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.BillPaymentRepository
{
    public class BillsPaymentRepository : IBillPaymentRepository
    {
        public async Task<DetailForBillPaymentVM> GetDetailForBillPayment(BillPayMoneyAggregatoryRequest request)
        {
            var result = new DetailForBillPaymentVM();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandText = "usp_GetDetailForBillPayment";
                cmd.CommandType = CommandType.StoredProcedure;

                SqlParameter p1 = new SqlParameter("@WalletUserId", request.WalletUserId);
                cmd.Parameters.Add(p1);
                SqlParameter p2 = new SqlParameter("@channel", request.channel);
                cmd.Parameters.Add(p2);
                SqlParameter p3 = new SqlParameter("@ISD", request.ISD);
                cmd.Parameters.Add(p3);
                SqlParameter p4 = new SqlParameter("@ServiceCategoryId", request.ServiceCategoryId);
                cmd.Parameters.Add(p4);


                try
                {

                    db.Database.Connection.Open();
                    // Run the sproc
                    var reader = cmd.ExecuteReader();

                    // Read Blogs from the first result set
                    result.sender = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<WalletUser>(reader).FirstOrDefault();

                    reader.NextResult();
                    result.WalletService = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<WalletService>(reader).FirstOrDefault();

                    reader.NextResult();
                    result.SubCategory = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<SubCategory>(reader).FirstOrDefault();

                    reader.NextResult();
                    result.IsdocVerified = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<bool>(reader).FirstOrDefault();

                    reader.NextResult();
                    result.transactionLimit = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<TransactionLimitResponse>(reader).FirstOrDefault();

                    reader.NextResult();
                    result.transactionHistory = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<TransactionHistoryAddMoneyReponse>(reader).FirstOrDefault();
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    db.Database.Connection.Close();
                }

            }

            return result;
        }

        public async Task<WalletTransaction> InsertWalletTransaction(WalletTransaction walletTransaction)
        {
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                db.WalletTransactions.Add(walletTransaction);
                await db.SaveChangesAsync();
            }
            return walletTransaction;
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


        public async Task<TransactionInitiateRequest> GetTransactionInitiateRequest(long id)
        {
            var result = new TransactionInitiateRequest();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    result = await db.TransactionInitiateRequests.Where(x => x.Id == id).FirstOrDefaultAsync();
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
