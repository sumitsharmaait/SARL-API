using System;
using System.Collections.Generic;
using System.Configuration;

namespace Ezipay.ViewModel.TransferToBankViewModel
{
    public class BankListModel
    {
        public BankListModel()
        {
            this.BankCode = string.Empty;
            this.BankName = string.Empty;
            this.IsAdditional = false;
        }
        public string BankCode { get; set; }
        public string BankName { get; set; }
        public bool IsAdditional { get; set; }
        public bool IsActive { get; set; }
    }
    public class BankListList
    {
        public BankListList()
        {
            this.bankname = string.Empty;
            this.bankcode = string.Empty;
            this.creditusername = string.Empty;
            this.accountNumber = string.Empty;

        }
        public string bankname { get; set; }
        public string bankcode { get; set; }
        public string creditusername { get; set; }
        public string accountNumber { get; set; }

    }
    public class BankListResponse
    {
        public int RstKey { get; set; }
        public List<BankListModel> bankListModels { get; set; }


    }
    public class IsdCodesResponse1
    {
        public IsdCodesResponse1()
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
      //public decimal AmountInNGN { get; set; }
    }




    public class TransferFundRequest
    {
        public TransferFundRequest()
        {
            this.drAccount = "1520000043";
            this.crAccount = string.Empty;
            this.crAccountName = string.Empty;
            this.amount = string.Empty;
            this.bankCode = string.Empty;
            this.bankName = string.Empty;

            this.categoryCode = "L2OB";
            this.walletNo = "5504468164";
            this.narration = "Transfer";
            this.Password = string.Empty;
            this.remarks = string.Empty;



        }
        public string drAccount { get; set; }
        public string crAccount { get; set; }
        public string crAccountName { get; set; }
        public string amount { get; set; }
        public string bankCode { get; set; }
        public string bankName { get; set; }
        public string remarks { get; set; }
        public string categoryCode { get; set; }
        public string walletNo { get; set; }
        public string narration { get; set; }
        public string Password { get; set; }
        public string Countrycode { get; set; }
        public long WalletUserId { get; set; }
        public string serviceCategory { get; set; }

    }


    public class TransferFundResponse
    {
        //
        
        public int RstKey { get; set; }
        public string id { get; set; }
        public string message { get; set; }
        public int Status { get; set; }

        public string amount { get; set; }
        public string responseDescription { get; set; }
        public DateTime transactionDate { get; set; }
        public string transactionStatus { get; set; }
        public string responsecode { get; set; }
        public DateTime requestdatetime { get; set; }
        public string drAccount { get; set; }
        public string crAccount { get; set; }
        public string reference { get; set; }
        public string narration { get; set; }
        public string remarks { get; set; }
        public string crAccountName { get; set; }
        public string bankname { get; set; }
        public string CurrentBalance        { get; set; }
        public bool DocStatus { get; set; }
        //    "bankname": "Cowries",
        //    "transactionDate": "2021-02-01T15:33:16.340+0000",
        //    "transactionstatus": "success",
        //    "responsecode": "00",
        //    "requestdatetime": "2021-02-01 16:33:08",
        //    "draccount": "1520000043",
        //    "craccount": "1520000047",
        //    "reference": "9ce5e87e-8ce1-4347-b405-a9a4e81daa49",
        //    "narration": "Transfer from 1520000043 to 1520000047",
        //    "remarks": "Open API check",
        //    "craccountname": "OLUWATOYOSI nil OYEGOKE",


        //        {
        //    "id": 26,
        //    "amount": 100,
        //    "sweep": null,
        //    "settled": null,
        //    "settlementType": null,
        //    "numberOfTrial": null,
        //    "responseDescription": "Approved or completed successfully",
        //    "transactionDate": "2021-02-01T15:33:16.340+0000",
        //    "transactionstatus": "success",
        //    "responsecode": "00",
        //    "requestdatetime": "2021-02-01 16:33:08",
        //    "draccount": "1520000043",
        //    "craccount": "1520000047",
        //    "reference": "9ce5e87e-8ce1-4347-b405-a9a4e81daa49",
        //    "narration": "Transfer from 1520000043 to 1520000047",
        //    "remarks": "Open API check",
        //    "craccountname": "OLUWATOYOSI nil OYEGOKE",
        //    "bankname": "Cowries",
        //    "draccountname": "unknown"
        //}


    }




    public class TransferToBankResponseModel
    {
        //
        public string DestAcctNumber { get; set; }
        public string DestBankCode { get; set; }
        public decimal Amount { get; set; }
        public string TrackingNum { get; set; }
        public DateTime TransactionDate { get; set; }
        public string CurrentBalance { get; set; }
        public bool DocStatus { get; set; }
        public int DocumetStatus { get; set; }
        public int RstKey { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }

    }

    public class TransferToBankBeneficiaryNameRequest
    {
        public TransferToBankBeneficiaryNameRequest()
        {
            this.SrcAcctNumber = ConfigurationManager.AppSettings["TransferToBankSourceAccountNo"];
            this.SrcAcctName = ConfigurationManager.AppSettings["TransferToBankSourceAccountName"];
            this.DestAcctNumber = string.Empty;
            this.ServiceTransId = string.Empty;
            this.DestBankCode = string.Empty;
            this.Password = string.Empty;
            this.IsAdditional = false;
            this.MobileNo = string.Empty;
            this.ISDCode = string.Empty;
        }
        public string Amount { get; set; }
        public string SrcAcctNumber { get; set; }
        public string SrcAcctName { get; set; }
        public string DestAcctNumber { get; set; }
        public string DestAcctName { get; set; }
        public string ServiceTransId { get; set; }
        public string DestBankCode { get; set; }
        public bool IsAdditional { get; set; }
        public string Password { get; set; }
        public string MobileNo { get; set; }
        public string ISDCode { get; set; }
    }
    public class TransferToBankBeneficiaryNameResponse
    {
        public string TransStatus { get; set; }
        public string BasisCode { get; set; }

        public TransferToBankBeneficiaryName gipTransaction { get; set; }
    }
    public class beneficiaryRequest
    {
        public string SrcAcctNumber { get; set; }
        public string SrcAcctName { get; set; }
        public string DestAcctNumber { get; set; }
        //  public string DestAcctName { get; set; }
        public string ServiceTransId { get; set; }
        public string DestBankCode { get; set; }
    }
    public class TransferToBankBeneficiaryName
    {
        public TransferToBankBeneficiaryName()
        {
            this.Amount = string.Empty;
            this.TrackingNum = string.Empty;
            this.FunctionCode = string.Empty;
            this.OrigineBank = string.Empty;
            this.DestBank = string.Empty;
            this.SessionId = string.Empty;
            this.ChannelCode = string.Empty;
            this.NameToDebit = string.Empty;
            this.AccountToDebit = string.Empty;
            this.NameToCredit = string.Empty;
            this.AccountToCredit = string.Empty;
        }
        public string Amount { get; set; }
        public string dateTime { get; set; }
        public string TrackingNum { get; set; }
        public string FunctionCode { get; set; }
        public string OrigineBank { get; set; }
        public string DestBank { get; set; }
        public string SessionId { get; set; }
        public string ChannelCode { get; set; }
        public string NameToDebit { get; set; }
        public string AccountToDebit { get; set; }
        public string NameToCredit { get; set; }
        public string AccountToCredit { get; set; }
        public string Narration { get; set; }
        public string ActCode { get; set; }
        public string AprvCode { get; set; }
    }
    public class TransferToBankResponseGTBankModel
    {
        public string transactionCode { get; set; }
        public int Status { get; set; }
        public string Message { get; set; }
    }
    public class TransferToBankRequestGTBankModel
    {
        public string Msisdn { get; set; }
        public string Amount { get; set; }
        public string ReferecnceID { get; set; }
        public string AccountNum { get; set; }
        public string narration { get; set; }
    }

    public class SubmitCreditRequest
    {
        public string SrcAcctNumber { get; set; }
        public string SrcAcctName { get; set; }
        public string DestAcctNumber { get; set; }
        public string DestAcctName { get; set; }
        public string ServiceTransId { get; set; }
        public string DestBankCode { get; set; }
        public string SessionId { get; set; }
        public decimal Amount { get; set; }
        public string Narration { get; set; }
    }

    public class TransferToBankSubmitCreditResponce
    {
        public string TransStatus { get; set; }
        public string BasisCode { get; set; }
        public SubmitCreditModel gipTransaction { get; set; }
    }
    public class SubmitCreditModel
    {
        public SubmitCreditModel()
        {
            this.Amount = 0;
            this.TrackingNum = string.Empty;
            this.FunctionCode = string.Empty;
            this.OrigineBank = string.Empty;
            this.DestBank = string.Empty;
            this.SessionID = string.Empty;
            this.ChannelCode = string.Empty;
            this.NameToDebit = string.Empty;
            this.AccountToDebit = string.Empty;
            this.NameToCredit = string.Empty;
            this.AccountToCredit = string.Empty;
            this.Narration = string.Empty;
            this.ActCode = string.Empty;
            this.AprvCode = string.Empty;
        }
        public decimal? Amount { get; set; }
        public string dateTime { get; set; }
        public string TrackingNum { get; set; }
        public string FunctionCode { get; set; }
        public string OrigineBank { get; set; }
        public string DestBank { get; set; }
        public string SessionID { get; set; }
        public string ChannelCode { get; set; }
        public string NameToDebit { get; set; }
        public string AccountToDebit { get; set; }
        public string NameToCredit { get; set; }
        public string AccountToCredit { get; set; }
        public string Narration { get; set; }
        public string ActCode { get; set; }
        public string AprvCode { get; set; }

    }


    
}
