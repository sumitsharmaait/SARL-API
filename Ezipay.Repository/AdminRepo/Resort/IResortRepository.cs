using Ezipay.Database;
using Ezipay.ViewModel.ResortViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.AdminRepo.Resort
{
    public interface IResortRepository
    {
        Task<bool> InsertHotel(HotelMaster request);
        Task<List<HotelMasterResponse>> GetHotels();
        Task<bool> DeleteHotel(int id);
        Task<bool> UpdateHotel(HotelMaster request);
        Task<bool> SaveHotelBooking(HotelBooking request);
        Task<HotelMaster> GetHotelById(long id);
    }
}
