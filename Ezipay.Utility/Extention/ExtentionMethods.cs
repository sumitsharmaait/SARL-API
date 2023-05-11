using Ezipay.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ezipay.Utility.Extention
{

    public static class ExtensionMethods
    {
        /// <summary>
        /// Exception Log Method without Reqest Model
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="className"></param>
        /// <param name="methodName"></param>
        public static void ErrorLog(this String errorMessage, string className, string methodName)
        {
            try
            {
                using (var DBContext = new DB_9ADF60_ewalletEntities())
                {

                    ErrorLog error = new ErrorLog();
                    error.ErrorMessage = errorMessage;
                    error.ClassName = className;
                    error.MethodName = methodName;
                    error.JsonData = string.Empty;
                    error.CreatedDate = DateTime.UtcNow;
                    DBContext.ErrorLogs.Add(error);
                    DBContext.SaveChanges();

                }
            }
            catch
            {


            }
        }

        /// <summary>
        /// Exception Log Method with Reqest Model
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="className"></param>
        /// <param name="methodName"></param>
        /// <param name="requestJson"></param>
        public static void ErrorLog(this String errorMessage, string className, string methodName, object requestJson)
        {
            try
            {
                using (var DBContext = new DB_9ADF60_ewalletEntities())
                {

                    ErrorLog error = new ErrorLog();
                    error.ErrorMessage = errorMessage;
                    error.ClassName = className;
                    error.MethodName = methodName;
                    error.JsonData = JsonConvert.SerializeObject(requestJson);
                    error.CreatedDate = DateTime.UtcNow;
                    DBContext.ErrorLogs.Add(error);
                    DBContext.SaveChanges();

                }
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("ExtensionMethods.cs", "ErrorLog with Request Model");
            }
        }

        /// <summary>
        /// Exception Log Method with Reqest Model
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="className"></param>
        /// <param name="methodName"></param>
        /// <param name="requestJson"></param>
        public static void ErrorLog(this String errorMessage, string className, string methodName, string requestJson)
        {
            try
            {
                using (var DBContext = new DB_9ADF60_ewalletEntities())
                {

                    ErrorLog error = new ErrorLog();
                    error.ErrorMessage = errorMessage;
                    error.ClassName = className;
                    error.MethodName = methodName;
                    error.JsonData = requestJson;
                    error.CreatedDate = DateTime.UtcNow;
                    DBContext.ErrorLogs.Add(error);
                    DBContext.SaveChanges();

                }
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("ExtensionMethods.cs", "ErrorLog with Request Model");
            }
        }

        public static void webhookflutterLog(this String errorMessage, string className, string methodName, string requestJson)
        {
            try
            {
                using (var DBContext = new DB_9ADF60_ewalletEntities())
                {

                    webhookflutter error = new webhookflutter();
                    error.ErrorMessage = errorMessage;
                    error.ClassName = className;
                    error.MethodName = methodName;
                    error.JsonData = requestJson;
                    error.flag = "0";
                    error.CreatedDate = DateTime.UtcNow;
                    DBContext.webhookflutters.Add(error);
                    DBContext.SaveChanges();

                }
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("ExtensionMethods.cs", "webhookflutterLog with Request Model");
            }
        }
        /// <summary>
        /// Validate email id
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        public static bool IsValidEmailAddress(this string emailAddress)
        {
            return new System.ComponentModel.DataAnnotations
                                .EmailAddressAttribute()
                                .IsValid(emailAddress);
        }

        /// <summary>
        /// IgnoreDecimal
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        public static string IgnoreDecimal(this decimal amount)
        {
            var digits = Convert.ToString(amount).Split('.');
            if (digits != null && digits.Length > 1)
            {
                return digits[0];
            }
            else
            {
                return Convert.ToString(amount);
            }
        }

        /// <summary>
        /// IgnoreDecimal
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        public static string IgnoreDecimal(this string amount)
        {
            var digits = amount.Split('.');
            if (digits != null && digits.Length > 1)
            {
                if (Convert.ToInt32(digits[1]) <= 0)
                {
                    return digits[0];
                }
                else
                {
                    if (digits[1].Length > 2)
                    {
                        return digits[0] + "." + digits[1].Substring(0, 2);
                    }
                    else
                    {
                        return digits[0];
                    }
                }
            }
            else
            {
                return Convert.ToString(amount);
            }
        }

        /// <summary>
        /// Remove Text from amount
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        public static string RemoveText(this string amount)
        {
            try
            {
                var rangeString = "[a-zA-Z]";
                return Regex.Replace(amount, rangeString, string.Empty);
            }
            catch
            {

                return amount;
            }
        }

        /// <summary>
        /// Amount is zero
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        public static bool IsZero(this string Amount)
        {

            try
            {
                return Convert.ToDecimal(Amount) == 0;
            }
            catch
            {

                return true;
            }
        }

        /// <summary>
        /// Amount is zero
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        public static bool IsValidTransactionId(this string TransactionId)
        {

            try
            {
                if (TransactionId.Length > 0)
                {
                    bool res = !(Convert.ToDouble(TransactionId) == 0);
                    return res;
                }
                else
                {
                    return true;
                }

            }
            catch
            {

                return false;
            }
        }

        /// <summary>
        /// Is TwoDigitDecimal
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        public static bool IsTwoDigitDecimal(this string Amount)
        {
            try
            {
                var amt = Amount.Split('.');
                if (amt.Length > 1)
                {
                    return !(amt[1].Length > 2);
                }
                else
                {
                    return true;

                }
            }
            catch
            {

                return false;
            }
        }

        /// <summary>
        /// IgnoreZero
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        public static string IgnoreZero(this string MobileNo)
        {
            try
            {

                if (MobileNo.Substring(0, 1) == "0")
                {
                    return MobileNo.Substring(1, MobileNo.Length - 1);
                }
                else
                {
                    return MobileNo;
                }
            }
            catch
            {

                return MobileNo;
            }
        }

        /// <summary>
        /// SaveTransactionLog
        /// </summary>
        /// <param name="LogType"></param>
        /// <param name="TransactionName"></param>
        /// <param name="jsonValue"></param>
        public static void SaveTransactionLog(this string LogType, string TransactionName, string jsonValue, string Detail,long walletUserId)
        {
            try
            {
                using (var DBContext = new DB_9ADF60_ewalletEntities())
                {

                    TransactionLog log = new TransactionLog();
                    log.LogDate = DateTime.UtcNow;
                    log.LogType = LogType;
                    log.TransactionName = TransactionName;
                    log.WalletUserId = walletUserId;
                    log.LogJson = jsonValue;
                    if (!string.IsNullOrEmpty(Detail))
                    {
                        log.Detail = Detail;
                    }
                    else
                    {
                        log.Detail = string.Empty;
                    }
                    DBContext.TransactionLogs.Add(log);
                    DBContext.SaveChanges();

                }
            }
            catch
            {


            }

        }

        /// <summary>
        /// SaveTransactionLog
        /// </summary>
        /// <param name="LogType"></param>
        /// <param name="TransactionName"></param>
        /// <param name="jsonValue"></param>
        public static void SaveTransactionLog(this string LogType, string TransactionName, object jsonValue, string Detail)
        {
            try
            {
                using (var DBContext = new DB_9ADF60_ewalletEntities())
                {

                    TransactionLog log = new TransactionLog();
                    log.LogDate = DateTime.UtcNow;
                    log.LogType = LogType;
                    log.TransactionName = TransactionName;
                    log.LogJson = JsonConvert.SerializeObject(jsonValue);
                    if (!string.IsNullOrEmpty(Detail))
                    {
                        log.Detail = Detail;
                    }
                    else
                    {
                        log.Detail = string.Empty;
                    }
                    DBContext.TransactionLogs.Add(log);
                    DBContext.SaveChanges();

                }
            }
            catch
            {


            }
        }
    }

    public static class LogTransactionNameTypes
    {
        public static string PayMoney = "PayMoney by ";
        public static string AddMoney = "Add Money by ";
        public static string WalletTransaction = "Wallet Transaction by ";
        public static string UpdateTransaction = "Update Transaction Status call back ";
        public static string TransferToBank = "Wallet to bank Transaction ";
        

    }

    public static class LogTransactionTypes
    {
        public static string Request = "Request";
        public static string Response = "Response";
    }

}
