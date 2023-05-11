using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.ViewModel.ChannelViewModel
{
    public class ChannelResponce
    {
        public int WalletServiceId { get; set; }
        public string ServiceName { get; set; }
        public string DisplayServiceName { get; set; }
        public int ServiceCategoryId { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public long MerchantId { get; set; }
        public decimal CommisionPercent { get; set; }
        public decimal FlatCharges { get; set; }
        public decimal BenchmarkCharges { get; set; }
        public int DocumentStatus { get; set; }
        public string channel { get; set; }

    }
    public class ChannelRequest
    {
        public int ServiceCategoryId { get; set; }
        public string IsdCode { get; set; }
    }

    public class IspChannelResponse
    {
        public IspChannelResponse()
        {
            channelProductResponces = new List<ChannelProductResponce>();
            this.productTypeId = "";
        }
        public string productTypeId { get; set; }
        public int WalletServiceId { get; set; }
        public string ServiceName { get; set; }
        public string DisplayServiceName { get; set; }
        public int ServiceCategoryId { get; set; }
        public string ImageUrl { get; set; }
        public string JsonData { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public long MerchantId { get; set; }
        public decimal CommisionPercent { get; set; }
        public decimal FlatCharges { get; set; }
        public decimal BenchmarkCharges { get; set; }
        public int DocumentStatus { get; set; }
        public int OperatorId { get; set; }
        public object productDataJson { get; set; }
        public int productId { get; set; }
        public decimal AmountInCedi { get; set; }
        public decimal AmountInDoller { get; set; }
        public string channel { get; set; }
        public List<ChannelProductResponce> channelProductResponces { get; set; }
    }

    public class ChannelProductResponce
    {
        public ChannelProductResponce()
        {
            this.productTypeId = string.Empty;
        }
        public int productId { get; set; }
        public string ProductName { get; set; }
        public int OperatorId { get; set; }
        public string PriceType { get; set; }
        public string MinAmount { get; set; }
        public string MaxAmount { get; set; }
        public string FixAmount { get; set; }
        public string productTypeId { get; set; }
    }
    public class FinalResponse
    {
        public string ServiceName { get; set; }
        public int WalletServiceId { get; set; }
        public string ImageUrl { get; set; }
        public int ProductId { get; set; }
        public int OperatorId { get; set; }
        public string JsonData { get; set; }
    }
    public class OperatorRequest
    {
        public OperatorRequest()
        {
            auth = new Auth();
            version = "5";
            command = string.Empty;
            country = string.Empty;
            operatorData = string.Empty;
        }
        public Auth auth { get; set; }
        public string version { get; set; }
        public string command { get; set; }
        public string country { get; set; }
        public string operatorData { get; set; }
    }
    public class Auth
    {
        public Auth()
        {
            username = string.Empty;
            salt = string.Empty;
            password = string.Empty;
        }
        public string username { get; set; }
        public string salt { get; set; }
        public string password { get; set; }
        //public string signature { get; set; }
    }
    public class ProductType
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class Min
    {
        public string @operator { get; set; }
        public string user { get; set; }
    }

    public class Max
    {
        public string @operator { get; set; }
        public string user { get; set; }
    }

    public class Price
    {
        public Min min { get; set; }
        public Max max { get; set; }
        public string @operator { get; set; }
    }

    public class ProductDataResponse
    {
        public string id { get; set; }
        public ProductType productType { get; set; }
        public string priceType { get; set; }
        public string name { get; set; }
        public Price price { get; set; }
    }

    public class ProductDetail
    {
        public string id { get; set; }
        public ProductType productType { get; set; }
        public string priceType { get; set; }
        public string name { get; set; }
        public Price price { get; set; }
    }

    //public class FinalResponse
    //{
    //    public string ServiceName { get; set; }
    //    public int WalletServiceId { get; set; }
    //    public string ImageUrl { get; set; }
    //    public int ProductId { get; set; }
    //    public int OperatorId { get; set; }
    //    public string JsonData { get; set; }
    //}
}
