using Ezipay.ViewModel.AirtimeViewModel;
using Ezipay.ViewModel.InternatinalRechargeViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.InternatinalRechargeServ
{
    public interface IInternatinalRechargeService
    {
        Task<InternationalAirtimeResponse> GetProductList(InternationalAirtimeRequest request);
        Task<AddMoneyAggregatorResponse> InternationalAirtimeServices(RechargeAirtimeInternationalAggregatorRequest request,string sessionToken, long WalletUserId = 0);
        //Task<InternationalDTHResponse> GetCountryList();
        //Task<InternationalDTHResponse> GetServiceList(GetServiceListRequest request);
        //Task<InternationalDTHResponse> GetOperatorList(GetServiceListRequest request);
        //Task<InternationalDTHProductResponse> GetProductList(GetServiceListRequest request);
        //Task<AddMoneyAggregatorResponse> InternationalDTHServices(RechargeDthInternationalAggregatorRequest request, long WalletUserId = 0);

        Task<InternationalDTHResponse> GetCountryList();
        Task<InternationalDTHResponse> GetServiceList(GetServiceListRequest request);
        Task<InternationalDTHResponse> GetOperatorList(GetServiceListRequest request);
        Task<InternationalDTHProductResponse> GetProductList(GetServiceListRequest request);
        Task<AddMoneyAggregatorResponse> InternationalDTHServices(RechargeDthInternationalAggregatorRequest request, long WalletUserId = 0);

    }
}
