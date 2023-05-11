using Ezipay.Database;
using Ezipay.ViewModel.CommisionViewModel;
using Ezipay.ViewModel.ThridPartyApiVIewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.ThridPartyApiRepo
{
    public class ThridPartyApiRepository : IThridPartyApiRepository
    {
        public async Task<WalletTransaction> GetWalletTransaction(string TransactionId, string OperatorType = null, string InvoiceNo = null)
        {
            var response = new WalletTransaction();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    response = await db.WalletTransactions.Where(x => (x.TransactionId == TransactionId && x.OperatorType == OperatorType && x.TransactionStatus == 2) || x.InvoiceNo == InvoiceNo).FirstOrDefaultAsync();
                }
            }
            catch
            {

            }
            return response;
        }
        public async Task<WalletTransaction> GetSochitelWalletTransaction(string TransactionId, string InvoiceNo)
        {
            var response = new WalletTransaction();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    response = await db.WalletTransactions.Where(x => x.TransactionId == TransactionId && x.InvoiceNo == InvoiceNo && x.TransactionStatus == 2).FirstOrDefaultAsync();
                }
            }
            catch
            {

            }
            return response;
        }
        public async Task<WalletService> GetWalletService(int WalletServiceId)
        {
            var response = new WalletService();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    response = await db.WalletServices.Where(x => x.WalletServiceId == WalletServiceId).FirstOrDefaultAsync();
                }
            }
            catch
            {

            }
            return response;
        }

        public async Task<AddDuringPayRecord> GetAddDuringPayRecord(string transactionId, int TransactionStatus)
        {
            var response = new AddDuringPayRecord();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    response = await db.AddDuringPayRecords.Where(x => x.TransactionNo == transactionId && x.TransactionStatus == TransactionStatus).FirstOrDefaultAsync();
                }
            }
            catch
            {

            }
            return response;
        }

        public async Task<WalletTransaction> UpdateWalletTransaction(WalletTransaction request)
        {
            var response = new WalletTransaction();
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
            return response;

        }


        public async Task<List<commissionOnAmountModel>> ServiceCommissionList()
        {
            var response = new List<commissionOnAmountModel>();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                try
                {

                    response = await db.Database.SqlQuery<commissionOnAmountModel>("EXEC usp_CommissionListOfAllServices").ToListAsync();
                }
                catch (Exception)
                {

                }
                return response;

            }
        }


        public async Task<int> FlightHotelBooking(FlightHotelData flightHotelData)
        {
            int result = 0;
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                db.FlightHotelDatas.Add(flightHotelData);
                result = await db.SaveChangesAsync();
            }
            return result;
        }

        public async Task<FlightHotelData> GetUserDetailById(long userId, string token)
        {
            var result = new FlightHotelData();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                result = await db.FlightHotelDatas.Where(x => x.AgentCode == userId && x.TokenId == token).FirstOrDefaultAsync();
            }
            return result;
        }


        public async Task<List<WalletTransaction>> GetPendingTransactions()
        {
            var response = new List<WalletTransaction>();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    response = await db.WalletTransactions.Where(x => x.TransactionStatus == 2 && x.TransactionId != "").ToListAsync();
                }
            }
            catch
            {

            }
            return response;
        }

        public async Task<WalletTransaction> UpdateStatusOfPendingTransactions(WalletTransaction wallet)
        {
            //  var response = new WalletTransaction();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    db.Entry(wallet).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            catch
            {

            }
            return wallet;
        }

        //public async Task<int> WalletTxnUpdateList(string TransactionId, string InvoiceNo, string UpdatebyAdminWalletID)
        //{
        //    int result = 0;
        //    using (var db = new DB_9ADF60_ewalletEntities())
        //    {
        //        var Data = db.WalletTransactions.Where(x => x.TransactionId == TransactionId && x.InvoiceNo == InvoiceNo && x.TransactionStatus == 2).FirstOrDefault();
        //        var entity = new Database.WalletTxnUpdateList
        //        {
        //            WalletTxn = Data.WalletTransactionId,
        //            UpdatebyAdminWalletID = UpdatebyAdminWalletID,
        //            WalletTxnStatus = "1",
        //            CreatedDate = DateTime.UtcNow

        //        };

        //        db.WalletTxnUpdateLists.Add(entity);
        //        result = await db.SaveChangesAsync();
        //    }
        //    return result;

        //}

        public async Task<int> WalletTxnUpdateList(string TransactionId, string InvoiceNo, string UpdatebyAdminWalletID, string StatusCode)
        {
            int result = 0;
            string WalletTxnStatus1 = null;
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                var Data = db.WalletTransactions.Where(x => (x.TransactionId == TransactionId || x.InvoiceNo == InvoiceNo) && x.TransactionStatus == 2).FirstOrDefault();

                if (StatusCode == "200")
                {
                    WalletTxnStatus1 = "1";
                }
                else if (StatusCode == "300")
                {
                    WalletTxnStatus1 = "2";
                }
                else if (StatusCode == "404")
                {
                    WalletTxnStatus1 = "5";
                }

                var entity = new Database.WalletTxnUpdateList
                {
                    WalletTxn = Data.WalletTransactionId,
                    UpdatebyAdminWalletID = UpdatebyAdminWalletID,
                    WalletTxnStatus = WalletTxnStatus1,
                    CreatedDate = DateTime.UtcNow

                };

                db.WalletTxnUpdateLists.Add(entity);
                result = await db.SaveChangesAsync();
            }
            return result;

        }

    }
}
