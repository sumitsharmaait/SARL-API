using Ezipay.Database;
using Ezipay.ViewModel.AdminViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Ezipay.Repository.AdminRepo.TxnUpdate
{
    public class TxnUpdateRepository : ITxnUpdateRepository
    {


        public async Task<List<WalletTransaction>> GetWalletTxnPendingList()
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                return await context.WalletTransactions.Where(x => x.TransactionStatus == 2 && x.TransactionType == "CREDIT").OrderByDescending(x => x.CreatedDate).ToListAsync();
            }
        }

        public async Task<int> UpdatePendingWalletTxn(WalletTxnRequest Request)
        {
            var objResponse = new CreditDebitResponse();
            try
            {
                using (var db1 = new DB_9ADF60_ewalletEntities())
                {
                    var txnstatus = db1.WalletTransactions.Where(x => x.WalletTransactionId == Request.WalletTxnid && x.TransactionStatus == 2).FirstOrDefault();
                    if (txnstatus != null)
                    {
                        var entity = new Database.WalletTxnUpdateList
                        {
                            WalletTxn = Request.WalletTxnid,
                            UpdatebyAdminWalletID = Request.UpdatebyAdminWalletID,
                            WalletTxnStatus = Request.Txnstatus.ToString(),
                            CreatedDate = DateTime.UtcNow

                        };
                        db1.WalletTxnUpdateLists.Add(entity);
                        int i = await db1.SaveChangesAsync();
                        if (i == 1)
                        {
                            int serviceId = db1.WalletServices.Where(x => x.ServiceCategoryId == 4).Select(x => x.WalletServiceId).FirstOrDefault();
                            if (serviceId > 0)
                            {
                                var adminUser = db1.WalletUsers.Where(x => x.UserType == 2).FirstOrDefault();
                                if (adminUser != null)
                                {


                                    objResponse = await db1.Database.SqlQuery<CreditDebitResponse>
                                                                                          ("EXEC usp_CreditDebitUser @SenderId,@ReceiverId,@TransactionAmount,@Reason,@ServiceId,@TransactionDate,@IsCredit",
                                                                                          new SqlParameter("@SenderId", Request.TransactionType ? adminUser.WalletUserId : Request.UserId),
                                                                                            new SqlParameter("@ReceiverId", Request.TransactionType ? Request.UserId : adminUser.WalletUserId),
                                                                                            new SqlParameter("@TransactionAmount", Request.Amount),
                                                                                            new SqlParameter("@Reason", Request.Reason),
                                                                                            new SqlParameter("@ServiceId", serviceId),
                                                                                            new SqlParameter("@TransactionDate", DateTime.UtcNow),
                                                                                            new SqlParameter("@IsCredit", Request.TransactionType)
                                                                                          ).FirstOrDefaultAsync();
                                }
                                else
                                {
                                    return -1;
                                }
                            }
                            else
                            {
                                return -1;
                            }
                            //
                            if (objResponse.RstKey == 1)
                            {
                                var Data = db1.WalletTransactions.Where(x => x.WalletTransactionId == Request.WalletTxnid && x.TransactionStatus == 2).FirstOrDefault();
                                if (Data != null)
                                {
                                    Data.TransactionStatus = Request.Txnstatus;
                                    Data.UpdatedDate = DateTime.UtcNow;
                                    db1.Entry(Data).State = EntityState.Modified;
                                    await db1.SaveChangesAsync();
                                    return 1;
                                }
                                else
                                {
                                    return -1;
                                }

                            }
                            else
                            {
                                return -1;
                            }

                        }
                        else
                        {
                            return -1;
                        }

                    }
                    else
                    {
                        return -1;
                    }
                }
            }

            catch (Exception ex)
            {

                return -1;

            }


        }

        public async Task<int> UpdateBankPendingWalletTxn(WalletTxnRequest Request)
        {
            if (Request.Txnstatus == 1) //when aaacceept
            {
                var objResponse = new CreditDebitResponse();
                try
                {
                    using (var db1 = new DB_9ADF60_ewalletEntities())
                    {
                        var txnstatus = db1.WalletTransactions.Where(x => x.WalletTransactionId == Request.WalletTxnid
                        && x.TransactionStatus == 2).FirstOrDefault();
                        if (txnstatus != null)
                        {
                            var entity = new Database.WalletTxnUpdateList
                            {
                                WalletTxn = Request.WalletTxnid,
                                UpdatebyAdminWalletID = Request.UpdatebyAdminWalletID,
                                WalletTxnStatus = Request.Txnstatus.ToString(),
                                CreatedDate = DateTime.UtcNow

                            };
                            db1.WalletTxnUpdateLists.Add(entity);
                            int i = await db1.SaveChangesAsync();
                            if (i == 1)
                            {
                                int serviceId = db1.WalletServices.Where(x => x.ServiceCategoryId == 4).Select(x => x.WalletServiceId).FirstOrDefault();
                                if (serviceId > 0)
                                {
                                    var adminUser = db1.WalletUsers.Where(x => x.UserType == 2).FirstOrDefault();
                                    if (adminUser != null)
                                    {

                                        Request.Reason = "credited amount :- " + txnstatus.WalletAmount + " against Txn Id :- " + txnstatus.InvoiceNo + " & txn done on :- " + txnstatus.CreatedDate;
                                        objResponse = await db1.Database.SqlQuery<CreditDebitResponse>
                                                                                              ("EXEC usp_CreditDebitUser @SenderId,@ReceiverId,@TransactionAmount,@Reason,@ServiceId,@TransactionDate,@IsCredit",
                                                                                              new SqlParameter("@SenderId", Request.TransactionType ? adminUser.WalletUserId : txnstatus.SenderId),
                                                                                                new SqlParameter("@ReceiverId", Request.TransactionType ? txnstatus.SenderId : adminUser.WalletUserId),
                                                                                                new SqlParameter("@TransactionAmount", txnstatus.WalletAmount),
                                                                                                new SqlParameter("@Reason", Request.Reason),
                                                                                                new SqlParameter("@ServiceId", serviceId),
                                                                                                new SqlParameter("@TransactionDate", DateTime.UtcNow),
                                                                                                new SqlParameter("@IsCredit", Request.TransactionType)
                                                                                              ).FirstOrDefaultAsync();

                                        //emailuser
                                    }
                                    else
                                    {
                                        return -1;
                                    }
                                }
                                else
                                {
                                    return -1;
                                }
                                //
                                if (objResponse.RstKey == 1)
                                {

                                    var Data = db1.WalletTransactions.Where(x => x.WalletTransactionId == Request.WalletTxnid && x.TransactionStatus == 2).FirstOrDefault();
                                    if (Data != null)
                                    {
                                        Data.TransactionStatus = Request.Txnstatus;
                                        Data.UpdatedDate = DateTime.UtcNow;
                                        db1.Entry(Data).State = EntityState.Modified;
                                        await db1.SaveChangesAsync();

                                        return 1;


                                    }
                                    else
                                    {
                                        return -1;
                                    }

                                }
                                else
                                {
                                    return -1;
                                }

                            }
                            else
                            {
                                return -1;
                            }

                        }
                        else
                        {
                            return -1;
                        }
                    }
                }

                catch (Exception ex)
                {

                    return -1;

                }

            }

            else //fail
            {
                try
                {
                    using (var db1 = new DB_9ADF60_ewalletEntities())
                    {
                        var txnstatus = db1.WalletTransactions.Where(x => x.WalletTransactionId == Request.WalletTxnid
                        && x.TransactionStatus == 2).FirstOrDefault();
                        if (txnstatus != null)
                        {
                            var entity = new Database.WalletTxnUpdateList
                            {
                                WalletTxn = Request.WalletTxnid,
                                UpdatebyAdminWalletID = Request.UpdatebyAdminWalletID,
                                WalletTxnStatus = Request.Txnstatus.ToString(),
                                CreatedDate = DateTime.UtcNow

                            };
                            db1.WalletTxnUpdateLists.Add(entity);
                            int i = await db1.SaveChangesAsync();

                            if (i == 1)
                            {
                                txnstatus.TransactionStatus = Request.Txnstatus;
                                txnstatus.UpdatedDate = DateTime.UtcNow;
                                //txnstatus.Comments = "fail amount :- " + txnstatus.TotalAmount + " against Txn Id :- " + txnstatus.InvoiceNo + " & txn done on :- " + txnstatus.CreatedDate;
                                db1.Entry(txnstatus).State = EntityState.Modified;
                                await db1.SaveChangesAsync();
                                return 1;
                            }
                            else
                            {
                                return -1;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                    return -1;

                }
                return -1;
            }


        }


      
    }
}