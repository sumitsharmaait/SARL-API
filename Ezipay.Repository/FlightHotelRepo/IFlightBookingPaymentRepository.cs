using Ezipay.Database;
using Ezipay.ViewModel.FlightHotelViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.FlightHotelRepo
{
    public interface IFlightBookingPaymentRepository
    {
        Task<bool> IsSession(string token);
        Task<ViewUserList> GetUserDetailByEmail(string emailId);
        Task<FlightBookingData> SaveFlightData(FlightBookingData request);
        Task<SessionToken> GetSessionToken(string token);
        Task<FlightBookingData> GetFlightData(string securityCode);
        Task<WalletTransaction> SaveWalletTransaction(WalletTransaction request);
    }
}
