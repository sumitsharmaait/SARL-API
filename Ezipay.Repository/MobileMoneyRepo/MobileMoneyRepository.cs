using Ezipay.Database;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.AirtimeViewModel;
using System;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Ezipay.Repository.MobileMoneyRepo
{
    public class MobileMoneyRepository : IMobileMoneyRepository
    {
        public async Task<WalletTransaction> MobileMoneyService(WalletTransaction request)
        {

            using (var db = new DB_9ADF60_ewalletEntities())
            {
                db.WalletTransactions.Add(request);
                await db.SaveChangesAsync();
            }

            return request;
        }
        public async Task<WalletUser> GetData(long walletuserid)
        {
            var result = new WalletUser();

            using (var db = new DB_9ADF60_ewalletEntities())
            {
                result = await db.WalletUsers.Where(x => x.WalletUserId == walletuserid).FirstOrDefaultAsync();
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

        public async Task<AdminMobileMoneyLimitResponse> VerifyMobileMoneyLimit(AdminMobileMoneyLimitRequest request)
        {
            var response = new AdminMobileMoneyLimitResponse();
            string val = request.MinimumAmount;
            double dValue = double.Parse(val);
            var MinimumAmount = Convert.ToInt32(dValue);
            try
            {

                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    //if (MinimumAmount > 0)
                    //{
                    response = await db.Database.SqlQuery<AdminMobileMoneyLimitResponse>
                        ("exec usp_CheckAdminMobileMoneyLimit @MinimumAmountFlag,@ServiceCode",
                       new SqlParameter("@MinimumAmountFlag", MinimumAmount),
                       new SqlParameter("@ServiceCode", request.Service)
                     ).FirstOrDefaultAsync();
                    //}
                }
            }
            catch (Exception ex)
            {
            
            }

            return response;
        }

        public async Task<int> SaveMobileMoneySenderDetailsRequest(PayMoneyAggregatoryRequest request)
        {
            int res = 0;
            MobileMoneySenderDetail ObjDetail = new MobileMoneySenderDetail();
            try
            {

                ObjDetail.WalletuserId = request.WalletUserId;
                ObjDetail.SenderIdNumber = request.SenderIdNumber.ToUpper();
                ObjDetail.SenderIdType = request.SenderIdType;
               
                ObjDetail.SenderDateofbirth = Convert.ToDateTime(request.SenderDateofbirth);
                ObjDetail.SenderAddress = request.SenderAddress;
                ObjDetail.SenderCity = request.SenderCity;
                ObjDetail.ReceiverFirstName = request.ReceiverFirstName;
                ObjDetail.ReceiverLastName = request.ReceiverLastName;
                ObjDetail.Createddate = DateTime.Now;

                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    db.MobileMoneySenderDetails.Add(ObjDetail);
                    res = await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                return 0;
            }
            return res;
        }

        public async Task<MobileMoneySenderDetail> VerifySenderIdNumberExistorNot(MobileMoneySenderDetailrequest request)
        {
            var response = new MobileMoneySenderDetail();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    response = await db.MobileMoneySenderDetails.Where(x => x.WalletuserId == request.WalletuserId && x.SenderIdNumber == request.SenderIdNumber).OrderByDescending(x => x.Id).FirstOrDefaultAsync();
                }
            }
            catch (Exception ex)
            {
                // ex.Message.ErrorLog("MasterDataRepository.cs", "GetAllTransactionsAddMoney");
            }
            return response;
        }

    }
}
