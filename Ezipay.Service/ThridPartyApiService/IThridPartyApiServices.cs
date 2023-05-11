using Ezipay.ViewModel.AirtimeViewModel;
using Ezipay.ViewModel.CommisionViewModel;
using Ezipay.ViewModel.ThridPartyApiVIewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.ThridPartyApiService
{
    public interface IThridPartyApiServices
    {
        Task<UpdateTransactionResponse> UpdateTransactionStatus(UpdateTransactionRequest request);
        Task<List<commissionOnAmountModel>> ServiceCommissionList();
        Task<FlightBookingResponse> FlightHotelBooking(string token);
        Task<object> DataVerification(VerifyRequest xmlRquest);
        Task<string> GetFee(PayMoneyAggregatoryRequest Request);
        Task<TransactionStatusResponse> GetTransactionStatus();
    }
}
