using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.ViewModel.CardPaymentViewModel
{
    public class CardPaymentHelper
    {
        public enum ResponseDescriptionTypes
        {
            Success = 0,
            UnknownError = 1

            /*
                 case "0":
                     getResponseDescription = "Transaction Successful"; break;
                 case "1":
                     getResponseDescription = "Unknown Error"; break;
                 case "2":
                     getResponseDescription = "Bank Declined Transaction"; break;
                 case "3":
                     getResponseDescription = "No Reply from Bank"; break;
                 case "4":
                     getResponseDescription = "Expired Card"; break;
                 case "5":
                     getResponseDescription = "Insufficient Funds"; break;
                 case "6":
                     getResponseDescription = "Error Communicating with Bank"; break;
                 case "7":
                     getResponseDescription = "Payment Server System Error"; break;
                 case "8":
                     getResponseDescription = "Transaction Type Not Supported"; break;
                 case "9":
                     getResponseDescription = "Bank declined transaction (Do not contact Bank)"; break;
                 case "A":
                     getResponseDescription = "Transaction Aborted"; break;
                 case "C":
                     getResponseDescription = "Transaction Cancelled"; break;
                 case "D":
                     getResponseDescription = "Deferred transaction received and is awaiting processing"; break;
                 case "F":
                     getResponseDescription = "3D Secure Authentication failed"; break;
                 case "I":
                     getResponseDescription = "Card Security Code verification failed"; break;
                 case "L":
                     getResponseDescription = "Shopping Transaction Locked"; break;
                 case "N":
                     getResponseDescription = "Cardholder is not enrolled in Authentication scheme"; break;
                 case "P":
                     getResponseDescription = "Transaction is still being processed"; break;
                 case "R":
                     getResponseDescription = "Transaction not processed - Reached limit of retry attempts allowed"; break;
                 case "S":
                     getResponseDescription = "Duplicate SessionID (OrderInfo)"; break;
                 case "T":
                     getResponseDescription = "Address Verification Failed"; break;
                 case "U":
                     getResponseDescription = "Card Security Code Failed"; break;
                 case "V":
                     getResponseDescription = "Address Verification and Card Security Code Failed"; break;
                 case "?":
                     getResponseDescription = "Transaction status is unknown"; break;

                 default: getResponseDescription = "Unable to be determined"; break;
             */

        }
        public string ResponseDescription(string txnResponseCode)
        {
            string getResponseDescription = string.Empty;
            switch (txnResponseCode)
            {
                case "0":
                    getResponseDescription = "Transaction Successful"; break;
                case "1":
                    getResponseDescription = "Unknown Error"; break;
                case "2":
                    getResponseDescription = "Bank Declined Transaction"; break;
                case "3":
                    getResponseDescription = "No Reply from Bank"; break;
                case "4":
                    getResponseDescription = "Expired Card"; break;
                case "5":
                    getResponseDescription = "Insufficient Funds"; break;
                case "6":
                    getResponseDescription = "Error Communicating with Bank"; break;
                case "7":
                    getResponseDescription = "Payment Server System Error"; break;
                case "8":
                    getResponseDescription = "Transaction Type Not Supported"; break;
                case "9":
                    getResponseDescription = "Bank declined transaction (Do not contact Bank)"; break;
                case "A":
                    getResponseDescription = "Transaction Aborted"; break;
                case "B":
                    getResponseDescription = "Your Payment was not successful"; break;
                case "C":
                    getResponseDescription = "Transaction Cancelled"; break;
                case "D":
                    getResponseDescription = "Deferred transaction received and is awaiting processing"; break;
                case "F":
                    getResponseDescription = "3D Secure Authentication failed"; break;
                case "I":
                    getResponseDescription = "Card Security Code verification failed"; break;
                case "L":
                    getResponseDescription = "Shopping Transaction Locked"; break;
                case "N":
                    getResponseDescription = "Cardholder is not enrolled in Authentication scheme"; break;
                case "P":
                    getResponseDescription = "Transaction is still being processed"; break;
                case "R":
                    getResponseDescription = "Transaction not processed - Reached limit of retry attempts allowed"; break;
                case "S":
                    getResponseDescription = "Duplicate SessionID (OrderInfo)"; break;
                case "T":
                    getResponseDescription = "Address Verification Failed"; break;
                case "U":
                    getResponseDescription = "Card Security Code Failed"; break;
                case "V":
                    getResponseDescription = "Address Verification and Card Security Code Failed"; break;
                case "?":
                    getResponseDescription = "Transaction status is unknown"; break;

                default: getResponseDescription = "Unable to be determined"; break;

            }
            return getResponseDescription;
        }
        public string get3DSstatusDescription(string statusResponse)
        {
            string get3DSstatusDescription = string.Empty;


            if (string.IsNullOrEmpty(statusResponse) || statusResponse == "No Value Returned")
            {
                get3DSstatusDescription = "3DS not supported or there was no 3DS data provided";
            }
            else
            {
                switch (statusResponse)
                {
                    case "Y":
                        get3DSstatusDescription = "Cardholder successfully authenticated"; break;
                    case "E":
                        get3DSstatusDescription = "Cardholder not enrolled"; break;
                    case "N":
                        get3DSstatusDescription = "Cardholder not verified"; break;
                    case "U":
                        get3DSstatusDescription = "System Error at the Issuer"; break;
                    case "F":
                        get3DSstatusDescription = "Formatting error in the the 3D Secure request"; break;
                    case "A":
                        get3DSstatusDescription = "3D Secure merchant ID and password authentication Failed"; break;
                    case "D":
                        get3DSstatusDescription = "Error communicating with the Directory Server"; break;
                    case "C":
                        get3DSstatusDescription = "The card type is not supported for authentication"; break;
                    case "S":
                        get3DSstatusDescription = "The Issuers signature on the response could not be validated"; break;
                    case "P":
                        get3DSstatusDescription = "Error parsing input from Issuer"; break;
                    case "I":
                        get3DSstatusDescription = "Internal Payment Server system error"; break;
                    case "T":
                        get3DSstatusDescription = "Timed out while performing authentication"; break;
                    default:
                        get3DSstatusDescription = "Unable to be determined"; break;
                }
            }
            return get3DSstatusDescription;
        }
        public string PaymentUrl(CardPaymentWebRequest request)
        {


            StringBuilder sb = new StringBuilder();
            //Url for Payment
            sb.Append(request.virtualPaymentClientURL);
            sb.Append("?Title=" + request.Title);
            sb.Append("&vpc_Version=" + request.vpc_Version);
            sb.Append("&vpc_Locale=" + request.vpc_Locale);
            sb.Append("&vpc_Command=" + request.vpc_Command);
            sb.Append("&vpc_AccessCode=" + request.vpc_AccessCode);
            sb.Append("&vpc_MerchTxnRef=" + request.vpc_MerchTxnRef);
            sb.Append("&vpc_Merchant=" + request.vpc_Merchant);
            sb.Append("&vpc_OrderInfo=" + request.vpc_OrderInfo);
            sb.Append("&vpc_OperatorId=" + request.vpc_OperatorId);
            sb.Append("&vpc_Amount=" + request.vpc_Amount);
            sb.Append("&vpc_Currency=" + request.vpc_Currency);
            sb.Append("&vpc_ReturnURL=" + request.vpc_ReturnURL);
            sb.Append("&AgainLink=" + request.AgainLink);
            // sb.Append("&vpc_SecureHash=" + MD5Hash(request));
            sb.Append("&vpc_SecureHash=" + SHA2Hash(request));

            return sb.ToString();
        }
        public string MD5Hash(CardPaymentWebRequest request)
        {
            StringBuilder sb = new StringBuilder();
            //Url for Payment
            sb.Append(request.vpc_SecureHash);
            sb.Append(request.AgainLink);
            sb.Append(request.Title);
            sb.Append(request.vpc_AccessCode);
            sb.Append(request.vpc_Amount);
            sb.Append(request.vpc_Command);
            sb.Append(request.vpc_Currency);
            sb.Append(request.vpc_Locale);
            sb.Append(request.vpc_MerchTxnRef);
            sb.Append(request.vpc_Merchant);
            sb.Append(request.vpc_OrderInfo);
            sb.Append(request.vpc_ReturnURL);
            sb.Append(request.vpc_Version);
            StringBuilder hash = new StringBuilder();
            MD5CryptoServiceProvider md5provider = new MD5CryptoServiceProvider();
            byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(sb.ToString()));

            for (int i = 0; i < bytes.Length; i++)
            {
                hash.Append(bytes[i].ToString("x2"));
            }
            return hash.ToString().ToUpper();
        }
        public string SHA2Hash(CardPaymentWebRequest request)
        {
            StringBuilder sb = new StringBuilder();
            //Url for Payment
            sb.Append(request.vpc_SecureHash);
            sb.Append(request.AgainLink);
            sb.Append(request.Title);
            sb.Append(request.vpc_AccessCode);
            sb.Append(request.vpc_Amount);
            sb.Append(request.vpc_Command);
            sb.Append(request.vpc_Currency);
            sb.Append(request.vpc_Locale);
            sb.Append(request.vpc_MerchTxnRef);
            sb.Append(request.vpc_OperatorId);
            sb.Append(request.vpc_Merchant);
            sb.Append(request.vpc_OrderInfo);
            sb.Append(request.vpc_ReturnURL);
            sb.Append(request.vpc_Version);
            StringBuilder hash = new StringBuilder();
            SHA256CryptoServiceProvider sha2provider = new SHA256CryptoServiceProvider();
            byte[] bytes = sha2provider.ComputeHash(new UTF8Encoding().GetBytes(sb.ToString()));

            for (int i = 0; i < bytes.Length; i++)
            {
                hash.Append(bytes[i].ToString("x2"));
            }
            return hash.ToString();
        }
    }
}
