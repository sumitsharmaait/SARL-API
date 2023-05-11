using Ezipay.Database;
using Ezipay.Utility.Extention;
using Ezipay.ViewModel.FlightHotelViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.FlightHotelRepo
{
    public class FlightBookingPaymentRepository : IFlightBookingPaymentRepository
    {

        public async Task<bool> IsSession(string token)
        {
            bool IsSuccess = false;
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    var user =await db.SessionTokens.Where(x => x.TokenValue == token).FirstOrDefaultAsync();
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
            //}
            return result;
        }

        public async Task<FlightBookingData> SaveFlightData(FlightBookingData request)
        {

            using (var db = new DB_9ADF60_ewalletEntities())
            {
                db.FlightBookingDatas.Add(request);
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

        public async Task<FlightBookingData> GetFlightData(string securityCode)
        {
            var result = new FlightBookingData();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                result = await db.FlightBookingDatas.Where(x => x.SecurityCode == securityCode).FirstOrDefaultAsync();
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

    }
}
