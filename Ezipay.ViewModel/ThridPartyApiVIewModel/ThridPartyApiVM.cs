using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.ViewModel.ThridPartyApiVIewModel
{
    public class TransactionStatusResponse
    {
        public string StatusCode { get; set; }
        public string Message { get; set; }
        public string TransactionId { get; set; }
        public string InvoiceNo { get; set; }
        public string OperatorType { get; set; }

    }
    public class UpdateTransactionResponse
    {
        public int status { get; set; }
        public bool isSuccess { get; set; }
        public string Message { get; set; }
    }
    public class FlightBookingPassRequest
    {
        public string Password { get; set; }
    }
    public class UpdateTransactionRequest
    {
        public string gu_transaction_id { get; set; }
        public string status { get; set; }
        public string partner_transaction_id { get; set; }
        public double commission { get; set; }
        public string MerchantId { get; set; }
        public string TransactionId { get; set; }
        public string Message { get; set; }
        public string StatusCode { get; set; }
        public string OperatorType { get; set; }
        public string InvoiceNo { get; set; }
        public string UpdatebyAdminWalletID { get; set; }
    }

    public class TVVerifyModel
    {
        public int StatusCode { get; set; }
        public string AccountName { get; set; }
        public string Currency { get; set; }
        public string AccountStatus { get; set; }
        public string DateCreated { get; set; }
        public string DateModified { get; set; }
    }
    public class AccountModel
    {
        public string Account { get; set; }
        public string Channel { get; set; }

    }
    public class DemoVerifyModel
    {
        public int field1 { get; set; }
        public string field6 { get; set; }
        public string field8 { get; set; }
        public string field9 { get; set; }
        public string field10 { get; set; }
        public string field11 { get; set; }
    }
    public class FlightBookingResponse
    {
        public FlightBookingResponse()
        {
            this.RstKey = 0;
            this.Message = string.Empty;
            this.DocStatus = false;
            this.DocumetStatus = 0;
            this.IsEmailVerified = false;
        }

        public int RstKey { get; set; }
        public string Message { get; set; }
        public int DocumetStatus { get; set; }
        public bool? DocStatus { get; set; }
        public bool IsEmailVerified { get; set; }
        public string responseString { get; set; }
        // public int Status { get; set; }
        //  public string MessageHotel { get; set; }
    }
    public class FlightAndAfroRequest
    {
        public FlightAndAfroRequest()
        {
        }
        public string agentcode { get; set; }
        public string tokenID { get; set; }
        public string tgt { get; set; }
        public string saltkey { get; set; }
        public string countrycode { get; set; }
    }
    [DataContract(Namespace = "")]
    public class VerifyRequest
    {
        [DataMember(Order = 1)]
        public string merchantcode { get; set; }
        [DataMember(Order = 2)]
        public string agentcode { get; set; }
        [DataMember(Order = 3)]
        public string tokenid { get; set; }
        [DataMember(Order = 4)]
        public string checksum { get; set; }
    }
    public class GetFlightBookingRequest
    {
        public GetFlightBookingRequest()
        {
        }
        public string agentcode { get; set; }
        public string tokenID { get; set; }
        public string merchantcode { get; set; }
        public string saltkey { get; set; }

    }
    public class VerifyResponse
    {
        public string address { get; set; }
        public string companyname { get; set; }
        public string city { get; set; }
        public string contactphone { get; set; }
        public string country { get; set; }
        public string emailid { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string mobile { get; set; }
        public string state { get; set; }
        public string title { get; set; }
        public string zip { get; set; }
        public int statusCode { get; set; }
        public string statusMessage { get; set; }
        public string merchantcode { get; set; }
        public string kcode { get; set; }
        public string tokenID { get; set; }
        public string checksum { get; set; }
        public string CurrentBalance { get; set; }
    }
    public class FinalCheckSum
    {
        public FinalCheckSum()
        {
        }
        public string statusCode { get; set; }
        public string statusMessage { get; set; }
        public string merchantCode { get; set; }
        public string agentCode { get; set; }
        public string tokenId { get; set; }
        public string saltKey { get; set; }
    }
}
