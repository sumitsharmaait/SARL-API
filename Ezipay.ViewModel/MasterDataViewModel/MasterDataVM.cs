using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.ViewModel.MasterDataViewModel
{
    public class IsdCodesResponse
    {
        public IsdCodesResponse()
        {
            this.CountryId = 0;
            this.CountryCode = string.Empty;
            this.IsdCode = string.Empty;
            this.Name = string.Empty;
            this.CountryFlag = string.Empty;
        }
        public int CountryId { get; set; }
        public string CountryCode { get; set; }
        public string Name { get; set; }
        public string IsdCode { get; set; }
        public string CountryFlag { get; set; }
    }

    public class NGNBankResponse
    {
        public NGNBankResponse()
        {          
            this.status = string.Empty;
            this.message = string.Empty;
            this.code = string.Empty;
            this.name = string.Empty;
        }
       
        public string status { get; set; }
        public string message { get; set; }
        public string code { get; set; }
        public string name { get; set; }
    }


    public class AppServiceResponse
    {
        public AppServiceResponse()
        {
            this.WalletServices = new List<WalletServiceResponse>();
            this.PayServices = new List<WalletServiceResponse>();
        }
        public List<WalletServiceResponse> WalletServices { get; set; }
        public List<WalletServiceResponse> PayServices { get; set; }
    }
    public class AppServiceRepositoryResponse : AppServiceResponse
    {
        public AppServiceRepositoryResponse()
        {
            this.IsSuccess = false;
        }
        public bool IsSuccess { get; set; }
    }
    public class WalletServiceResponse : AppServiceRequest
    {
        public WalletServiceResponse()
        {
            this.ServiceName = string.Empty;
            this.ServiceCategoryId = 0;
            this.WalletServiceId = 0;
        }
        public string ServiceName { get; set; }
        public long ServiceCategoryId { get; set; }
    }
    public class AppServiceRequest
    {
        public int WalletServiceId { get; set; }
    }

    public class MerchantsResponse
    {
        public MerchantsResponse()
        {
            this.Name = string.Empty;
            this.ProfileImage = string.Empty;
        }
        public long MerchantId { get; set; }
        public string Name { get; set; }
        public string ProfileImage { get; set; }
        public decimal CommisionPercent { get; set; }
        public decimal FlatCharges { get; set; }
        public decimal BenchmarkCharges { get; set; }
    }

    
    public class CurrencyvalueRequestById
    {       
        public long Walletuserid { get; set; }
        public string CurrencyName { get; set; }
    }
    public class CurrencyvalueResponseById
    {        
        public decimal finalAmt { get; set; }
        public string txtfinalAmt { get; set; }
    }
}
