
using Ezipay.Database;
using Ezipay.ViewModel.AirtimeViewModel;
using Ezipay.ViewModel.TransferToBankViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.TransferTobankService
{
    public interface ITransferToBankServices
    {
        Task<List<BankListList>> GetBankList();
       
        Task<List<IsdCodesResponse1>> GetTransferttobankCountryList();
        //Task<TransferFundResponse> PayMoneyTransferToBank(TransferFundRequest request, string sessionToken);
        Task<AddMoneyAggregatorResponse> PayMoneyTransferToBank(PayMoneyAggregatoryRequest request, long WalletUserId = 0);


        Task<List<senderIdTypetbl>> GetsenderidtypeList();
    }
}
