using Ezipay.Database;
using Ezipay.ViewModel.ResortViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.AdminRepo.Resort
{
    public class ResortRepository : IResortRepository
    {
        public async Task<bool> InsertHotel(HotelMaster request)
        {
            bool result = false;
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                db.HotelMasters.Add(request);
                int res = await db.SaveChangesAsync();
                if (res > 0)
                {
                    result = true;
                }
            }
            return result;
        }
        public async Task<List<HotelMasterResponse>> GetHotels()
        {
            var response = new List<HotelMasterResponse>();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                response = await db.Database.SqlQuery<HotelMasterResponse>("exec usp_GetHotels").ToListAsync();
            }

            return response;
        }
        public async Task<bool> DeleteHotel(int id)
        {
            bool response = false;
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                var result = await db.HotelMasters.Where(x => x.Id == id).FirstOrDefaultAsync();
                result.IsActive = false;
                result.IsDeleted = true;
                int res = db.SaveChanges();
                if (res > 0)
                {
                    response = true;
                }
            }

            return response;
        }

        public async Task<bool> UpdateHotel(HotelMaster request)
        {
            bool response = false;
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                var data = db.HotelMasters.Where(x => x.Id == request.Id).FirstOrDefault();
                if (data.NoOfRooms > request.AvailableRooms)
                {
                    data.CostOfRooms = request.CostOfRooms;
                    data.AvailableRooms = request.AvailableRooms;
                    data.MaxGuest = request.MaxGuest;
                    data.IsActive = true;
                    data.IsDeleted = false;
                    data.CreatedDate = DateTime.UtcNow;
                    data.UpDatedDate = DateTime.UtcNow;
                    //WalletUserId = 0                
                    int res = db.SaveChanges();
                    if (res > 0)
                    {
                        response = true;
                    }
                }
            }
            return response;
        }

        public async Task<bool> SaveHotelBooking(HotelBooking request)
        {
            bool response = false;
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                db.HotelBookings.Add(request);
                int result = await db.SaveChangesAsync();
                if (result > 0)
                {
                    response = true;
                }
            }
            return response;
        }

        public async Task<HotelMaster> GetHotelById(long id)
        {
            var response = new HotelMaster();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                response =await db.HotelMasters.Where(x => x.Id ==id).FirstOrDefaultAsync();                
            }
            return response;
        }
    }
}