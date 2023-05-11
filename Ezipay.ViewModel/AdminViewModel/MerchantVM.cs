using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ezipay.ViewModel.AdminViewModel
{
    public class MerchantListRequest : SearchRequest
    {
        public int Type { get; set; }

    }
    public class MerchantListResponse
    {
        public MerchantListResponse()
        {
            TotalCount = 0;
            MerchantList = new List<MerchantList>();
        }
        public int TotalCount { get; set; }
        public List<MerchantList> MerchantList { get; set; }
    }

    public class MerchantList
    {
        public long MerchantId { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }
        public string MobileNo { get; set; }
        public decimal CommissionPercent { get; set; }
        public decimal MerchantAmount { get; set; }
        public decimal PaidToEzeepay { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDisabledTransaction { get; set; }
        public int TotalCount { get; set; }
        public string IsdCode { get; set; }
        public string Address { get; set; }
        public string LogoUrl { get; set; }
        public string ImageName { get; set; }
        public string Password { get; set; }
        public string Company { get; set; }
        public string Otp { get; set; }
        public string BusinessLicense { get; set; }
        public string VatNumber { get; set; }
        public string TinNumber { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string BankCode { get; set; }
    }

    public class MerchantRequest
    {
        public long MerchantId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }
        public string MobileNo { get; set; }
        public decimal CommissionPercent { get; set; }
        public string IsdCode { get; set; }
        public string Address { get; set; }
        public string LogoUrl { get; set; }
        public string ATMCard { get; set; }
        public string IdCard { get; set; }
        public string ImageName { get; set; }
        public string Password { get; set; }
        public string Company { get; set; }
        public string Otp { get; set; }
        public int AddrsFileCount { get; set; }
        public int ShareholderIdFileCount { get; set; }
        public int ShareholderImageFileCount { get; set; }
        public List<DocModel> Documents { get; set; }
        public string BusinessLicense { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string BankCode { get; set; }
        public string VatNumber { get; set; }
        public string TinNumber { get; set; }
        public string PostalCode { get; set; }
        public long AdminId { get; set; } //log key
    }

    public class DocModel
    {
        public int DocType { get; set; }
        public string DocName { get; set; }
    }

    public class MerchantSaveResponse
    {

        public MerchantSaveResponse()
        {
            this.statusCode = 0;
        }
        public int statusCode;

    }
    public class MerchantLogoUploadResponse
    {

        public MerchantLogoUploadResponse()
        {
            this.StatusCode = false;
            this.ImageName = string.Empty;
        }

        public bool StatusCode;
        public string ImageName;
    }

    public class ViewMarchantTransactionRequest : SearchRequest
    {
        public long UserId { get; set; }
        public Nullable<DateTime> DateFrom { get; set; }
        public Nullable<DateTime> DateTo { get; set; }
        public int TransactionType { get; set; }

    }

    public class ViewMarchantTransactionResponse
    {
        public ViewMarchantTransactionResponse()
        {
            TransactionList = new List<TransactionDetails>();
        }
        public int TotalCount { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public List<TransactionDetails> TransactionList { get; set; }
    }

    public class TransactionDetails
    {
        public int Id { get; set; }
        public string RowLabel { get; set; }
        public string TransactionType { get; set; }
        public string TransactionAmount { get; set; }
        public int TransactionCode { get; set; }
        public string FromMobileNo { get; set; }
        public string ToMobileNo { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Comments { get; set; }
        public long WalletTransactionId { get; set; }
        public int TotalCount { get; set; }
        public int DataType { get; set; }
        public int Pagesize { get; set; }
        public string BankTransactionId { get; set; }
        public int TransactionTypeInfo { get; set; }
        public string AccountNo { get; set; }
        public int TransactionStatus { get; set; }
        public string ServiceName { get; set; }
        //public string SubCategory { get; set; }
        //public string MainCategory { get; set; }
        public long receiverId { get; set; }
        public long senderId { get; set; }
        public string TotalAmount { get; set; }
        public string CommisionAmount { get; set; }
        public string AfterTransactionBalance { get; set; }
    }

    public class MarchantDeleteRequest
    {
        [Required]
        public long UserId { get; set; }
        public long AdminId { get; set; } //log key
    }

    public class MerchantManageRequest
    {
        [Required]
        public long UserId { get; set; }
        [Required]
        public bool IsActive { get; set; }

        public bool Delete { get; set; }
        public long AdminId { get; set; } //log key
    }

    public class MerchantDeleteRequest
    {
        [Required]
        public long UserId { get; set; }
        [Required]
        public bool Delete { get; set; }
        public long AdminId { get; set; } //log key
    }
    public class MerchantEnableTransactionRequest
    {
        [Required]
        public long UserId { get; set; }
        [Required]
        public bool IsDisabledTransaction { get; set; }
        public long AdminId { get; set; } //log key
    }

    public class AddStoreRequest
    {
        public long WalletUserId { get; set; }
        public long StoreId { get; set; }
        public string StoreName { get; set; }
        public string Location { get; set; }
        public long AdminId { get; set; } //log key
    }

    public class StoreResponse
    {
        public long WalletUserId { get; set; }
        public long StoreId { get; set; }
        public string StoreName { get; set; }
        public string Location { get; set; }
        public string QrCode { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }

    }

    public class StoreSearchRequest
    {
        [Required]
        public long WalletUserId { get; set; }
        public long AdminId { get; set; } //log key
    }

    public class StoreDeleteRequest
    {
        [Required]
        public long StoreId { get; set; }
        public long AdminId { get; set; } //log key
    }

    public class StoreManageRequest
    {
        [Required]
        public long StoreId { get; set; }
        [Required]
        public bool IsActive { get; set; }
        public bool Delete { get; set; }
        public long AdminId { get; set; } //log key
    }
}
