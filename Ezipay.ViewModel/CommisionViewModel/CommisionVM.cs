using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.ViewModel.CommisionViewModel
{
    public class CalculateCommissionResponse
    {
        public CalculateCommissionResponse()
        {
            this.CommissionAmount = 0;
            this.CommisionPercent = 0;
            this.MerchantCommissionAmount = 0;
            this.MerchantCommissionRate = 0;
            this.CommissionId = 0;
            this.MerchantCommissionId = 0;
            this.AmountWithCommission = 0;
            this.TransactionAmount = 0;
            this.UpdatedCurrentBalance = 0;
            this.FlatCharges = 0;
            this.BenchmarkCharges = 0;
            this.DocumentStatus = 0;
        }
        public int CommissionId { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal CommisionPercent { get; set; }
        public long MerchantCommissionId { get; set; }
        public decimal MerchantCommissionAmount { get; set; }
        public decimal MerchantCommissionRate { get; set; }
        public decimal TransactionAmount { get; set; }
        public decimal AmountWithCommission { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal UpdatedCurrentBalance { get; set; }
        public int ServiceTaxId { get; set; }
        public decimal ServiceTaxAmount { get; set; }
        public decimal ServiceTaxRate { get; set; }
        public decimal FlatCharges { get; set; }
        public decimal BenchmarkCharges { get; set; }
        public int DocumentStatus { get; set; }
    }

    public class CommissionRequest
    {
        public int WalletServiceId { get; set; }
        public decimal CommisionPercent { get; set; }
        public decimal FlatCharges { get; set; }
        public decimal BenchmarkCharges { get; set; }
        public decimal VATCharges { get; set; }
        public long AdminId { get; set; } //log key
    }
    public class WalletServicesList
    {
        public WalletServicesList()
        {
            this.CommisionPercent = 0;
            this.ServiceCategoryId = 0;
            this.ServiceName = string.Empty;
            this.WalletServiceId = 0;
            this.BenchmarkCharges = 0;
            // this.DocumentStatus = 0;
            this.FlatCharges = 0;
        }
        public int WalletServiceId { get; set; }
        public string ServiceName { get; set; }
        public int ServiceCategoryId { get; set; }
        public decimal CommisionPercent { get; set; }
        public decimal FlatCharges { get; set; }
        public decimal BenchmarkCharges { get; set; }
        public decimal VatCharges { get; set; }
        public int TotalCount { get; set; }
        public string CountryName { get; set; }
    }
    public class SubCategoryResponse : MainCategoryResponse
    {
        public long SubCategoryId { get; set; }
    }


    public class MainCategoryResponse : SubCategoryRequest
    {
        public string CategoryName { get; set; }
    }

    public class SubCategoryRequest
    {
        public long? MainCategoryId { get; set; }
    }

    public class WalletServicesRequest : SearchRequest
    {
        public long SubcategoryId { get; set; }
    }

    public class SearchRequest
    {
        public SearchRequest()
        {
            this.SearchText = "";
            this.PageNumber = 1;
            this.PageSize = 10;
        }
        public string SearchText { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class CalculateCommissionRequest
    {        
        public int WalletServiceId { get; set; }
        public decimal TransactionAmount { get; set; }
        public bool IsRoundOff { get; set; }
        public decimal CurrentBalance { get; set; }
    }
    public class InvoiceNumberResponse
    {
        public InvoiceNumberResponse()
        {
            this.InvoiceNumber = string.Empty;
            this.AutoDigit = string.Empty;


        }
        public long Id { get; set; }
        public string InvoiceNumber { get; set; }
        public string AutoDigit { get; set; }
    }
    public class commissionOnAmountModel
    {
        public decimal Percentage { get; set; }
        public int WalletServiceId { get; set; }
        public string WalletServiceName { get; set; }
        public long ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string BankCode { get; set; }
        public long? MerchantId { get; set; }
        public decimal FlatCharges { get; set; }
        public decimal BenchmarkCharges { get; set; }
        public decimal VATCharges { get; set; }
        public string IsdCode { get; set; }
        public decimal AmountInDollar { get; set; }
        public decimal AmountInNGN { get; set; }
        public decimal AmountInEuro { get; set; }

        public decimal AmountInSendNGN { get; set; }
        public decimal AmountInSendGH { get; set; }
    }

    public class ManageWalletServicesList
    {
        public ManageWalletServicesList()
        {
            this.CommisionPercent = 0;
            this.ServiceCategoryId = 0;
            this.ServiceName = string.Empty;
            this.WalletServiceId = 0;
            this.BenchmarkCharges = 0;
            // this.DocumentStatus = 0;
            this.FlatCharges = 0;
        }
        public int WalletServiceId { get; set; }
        public string ServiceName { get; set; }
        public int ServiceCategoryId { get; set; }
        public decimal CommisionPercent { get; set; }
        public decimal FlatCharges { get; set; }
        public decimal BenchmarkCharges { get; set; }
        public int TotalCount { get; set; }
        public string Bank_Code { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
    public class UpdateWalletServicesRequest
    {
        public long WalletServiceId { get; set; }
        public bool IsActive { get; set; }
        public long AdminId { get; set; }
    }
}
