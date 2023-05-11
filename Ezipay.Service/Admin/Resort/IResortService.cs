using Ezipay.Database;
using Ezipay.ViewModel.PayMoneyViewModel;
using Ezipay.ViewModel.ResortViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Ezipay.Service.Admin.Resort
{
    public interface IResortService
    {
        Task<bool> InsertHotel(HotelRequest request);
        Task<string> SaveImage(HttpPostedFileBase image, string PreviousImage);
        Task<List<HotelMasterResponse>> GetHotels();
        Task<bool> DeleteHotel(int id);
        Task<bool> UpdateHotel(HotelMaster request);
        Task<WalletTransactionResponse> HotelBook(HotelBookingRequest request, string token);
    }
}
