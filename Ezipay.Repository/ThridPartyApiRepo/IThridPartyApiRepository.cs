using Ezipay.Database;
using Ezipay.ViewModel.CommisionViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.ThridPartyApiRepo
{
    public interface IThridPartyApiRepository
    {
        Task<WalletTransaction> GetWalletTransaction(string TransactionId, string OperatorType=null,string InvoiceNo=null);
        Task<WalletTransaction> GetSochitelWalletTransaction(string TransactionId, string InvoiceNo);
        Task<WalletService> GetWalletService(int WalletServiceId);

        Task<AddDuringPayRecord> GetAddDuringPayRecord(string transactionId, int TransactionStatus);
        Task<int> WalletTxnUpdateList(string TransactionId, string InvoiceNo, string UpdatebyAdminWalletID, string StatusCode);
        Task<WalletTransaction> UpdateWalletTransaction(WalletTransaction request);
        Task<List<commissionOnAmountModel>> ServiceCommissionList();
        Task<int> FlightHotelBooking(FlightHotelData flightHotelData);
        Task<FlightHotelData> GetUserDetailById(long userId, string token);
        Task<List<WalletTransaction>> GetPendingTransactions();
        Task<WalletTransaction> UpdateStatusOfPendingTransactions(WalletTransaction walletTransaction);

    }
}
