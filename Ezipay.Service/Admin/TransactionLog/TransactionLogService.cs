using Ezipay.Repository.AdminRepo.TransactionLog;
using Ezipay.Repository.CardPayment;
using Ezipay.Repository.CommisionRepo;
using Ezipay.Repository.UserRepo;
using Ezipay.Utility.common;
using Ezipay.Utility.ExcelGenerate;
using Ezipay.Utility.Extention;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.CheckPasswordViewModel;
using Ezipay.ViewModel.CommisionViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Ezipay.Service.Admin.TransactionLog
{
    public class TransactionLogService : ITransactionLogService
    {
        private readonly ITransactionLogRepository _transactionLogRepository;
        private IGenerateLogReport _generateLogReport;
        private ICardPaymentRepository _cardPaymentRepository;
        private ISetCommisionRepository _setCommisionRepository;
        private IWalletUserRepository _walletUserRepository;
        public TransactionLogService()
        {
            _transactionLogRepository = new TransactionLogRepository();
            _generateLogReport = new GenerateLogReport();
            _walletUserRepository = new WalletUserRepository();
            _cardPaymentRepository = new CardPaymentRepository();
            _setCommisionRepository = new SetCommisionRepository();
        }

        public async Task<TransactionLogsResponse> GetNewTransactionLogs(TransactionLogsRequest request)
        {
            var result = new TransactionLogsResponse();
            result.TransactionLogslist = await _transactionLogRepository.GetNewTransactionLogs(request);
            if (result.TransactionLogslist.Count > 0)
            {
                result.TotalCount = result.TransactionLogslist[0].TotalCount;
            }
            return result;
        }

        public async Task<TransactionLogResponse> GetTransactionLogs(TransactionLogRequest request)
        {
            var result = new TransactionLogResponse();
            result.TransactionLogs = await _transactionLogRepository.GetTransactionLogs(request);
            if (result.TransactionLogs.Count > 0)
            {
                result.TotalCount = result.TransactionLogs[0].TotalCount;
            }
            return result;
        }

        public async Task<MemoryStream> GenerateLogReport(DownloadLogReportRequest request)
        {
            var response = new TransactionLogsResponce();
            var result = new MemoryStream();
            try
            {
                response = await _transactionLogRepository.GenerateLogReport(request);
                result = await _generateLogReport.ExportReport(response);
            }
            catch (Exception ex)
            {
                //  ex.Message.ErrorLog("WalletTransactionRepository.cs", "ViewPaymentRequests");
            }
            return result;

        }

        public async Task<List<CardtxndetailsResponse>> Getcardtxndetails(CardtxndetailsRequest request)
        {
            var result = new List<CardtxndetailsResponse>();
            var objList = await _transactionLogRepository.Getcardtxndetails(request);

            foreach (var item in objList)
            {
                var obj = new CardtxndetailsResponse();
                obj.CreatedDate = item.CreatedDate;
                obj.WalletUserId = item.WalletUserId;
                obj.EmailId = item.EmailId;
                obj.RequestedBankTxnId = item.RequestedBankTxnId;
                obj.ResponseBankTxnId = item.ResponseBankTxnId;
                obj.CardNo = item.CardNo;
                obj.InvoiceNumber = item.InvoiceNumber;

                if (objList.IndexOf(item) == 0)
                {
                    obj.TotalCount = item.Id;
                }
                result.Add(obj);
            }

            return result;
        }


        public async Task<MemoryStream> GenerateLogReport1(DownloadLogReportRequest1 request)
        {
            var response = new MonthlyreportResponce();
            var result = new MemoryStream();
            try
            {
                response = await _transactionLogRepository.GenerateLogReport1(request);
                result = await _generateLogReport.ExportReport1(response);
            }
            catch (Exception ex)
            {
                //  ex.Message.ErrorLog("WalletTransactionRepository.cs", "ViewPaymentRequests");
            }
            return result;

        }


        public async Task<List<Fluttertxnresponse>> UpdateFlutterCheckTxn()
        {
            var result = false;
            var result1 = new List<Fluttertxnresponse>();
            //list of user daywise of callback not response
           


            var getInitialTransaction = await _cardPaymentRepository.GetTransactionInitiateRequestjsonresponse();
            for (int i = 0; i < getInitialTransaction.Count; i++)
            {
                //foreach (var getInitialTransaction[i] in getInitialTransaction)
                //{
                //check wallettxn exist
                int GetWalletTransactionsexist = await _cardPaymentRepository.GetWalletTransactionsexist(getInitialTransaction[i].WalletUserId, getInitialTransaction[i].InvoiceNumber);
                //check already refund or not
                int checkrefundtoinvoiceno = await _cardPaymentRepository.Checkrefundtoinvoiceno(141, getInitialTransaction[i].WalletUserId, getInitialTransaction[i].InvoiceNumber);

                if (getInitialTransaction[i].AfterTransactionBalance == "" && getInitialTransaction[i].JsonResponse == "" && GetWalletTransactionsexist == 0 && checkrefundtoinvoiceno == 0)
                {  //reverify txn 
                    var responseData2 = await GethashorUrl(getInitialTransaction[i].InvoiceNumber, "flutterUrlverify");

                    JavaScriptSerializer js = new JavaScriptSerializer();
                    dynamic blogObject = js.Deserialize<dynamic>(responseData2);

                    var txnreverifystatus = blogObject["data"]["status"];
                    var txncurrency = blogObject["data"]["currency"];
                    //jo successful h wo hi update hogi
                    if (getInitialTransaction[i].InvoiceNumber != null && txnreverifystatus == "successful" && txncurrency == "XOF")
                    {
                        var obj = new Fluttertxnresponse();
                        obj.TransactionType = true;
                        obj.InvoiceNo = getInitialTransaction[i].InvoiceNumber;
                        obj.UserId = Convert.ToInt32(getInitialTransaction[i].WalletUserId);

                        int rowAffected = await _transactionLogRepository.UpdateFlutterCheckTxn(obj);
                        if (rowAffected > 0)
                        {
                            result = true;
                            obj.totalCount = i;
                            result1.Add(obj);
                        }

                    }


                }
                else if(GetWalletTransactionsexist == 1) //successful txn only updatfe flag on webhooktable no refund
                {
                    int ii = await _transactionLogRepository.UpdateFlagonwebhookflutter(getInitialTransaction[i].InvoiceNumber);
                }
            }
            //successful txn only updatfe flag on webhooktable no refund
            var iii = await _cardPaymentRepository.Updatewebhookflutterflagsuccestxn();

            return result1;
        }

        public async Task<List<WalletTxnResponse>> FlutterCheckTxnNotCaptureOurSide(string InvoiceNumber)
        {
            var response1 = new List<WalletTxnResponse>();
            var _commission = new CalculateCommissionResponse();
            var _commissionRequest = new CalculateCommissionRequest();
            var response = new WalletTxnResponse();
            try
            {

                var getInitialTransaction = await _cardPaymentRepository.GetTransactionInitiateRequest(InvoiceNumber);
                long userId = Convert.ToInt32(getInitialTransaction.WalletUserId);

                var senderdata = await _walletUserRepository.GetUserDetailById(userId);
                int GetWalletTransactionsexist = await _cardPaymentRepository.GetWalletTransactionsexist(getInitialTransaction.WalletUserId, InvoiceNumber);
                int checkrefundtoinvoiceno = await _cardPaymentRepository.Checkrefundtoinvoiceno(141, getInitialTransaction.WalletUserId, InvoiceNumber);

                int WalletServiceId = await _cardPaymentRepository.GetServiceId();
                if (getInitialTransaction.AfterTransactionBalance == "" && getInitialTransaction.JsonResponse == "" && GetWalletTransactionsexist == 0 && checkrefundtoinvoiceno == 0)
                {
                    if (WalletServiceId > 0)
                    {
                        #region Calculate Commission on request amount
                        _commissionRequest.IsRoundOff = true;
                        _commissionRequest.TransactionAmount = Convert.ToDecimal(getInitialTransaction.RequestedAmount);
                        _commissionRequest.WalletServiceId = WalletServiceId;
                        _commission = await _setCommisionRepository.CalculateCommission(_commissionRequest);
                        #endregion
                    }
                    decimal amountWithCommision = decimal.Parse(string.Format("{0:0,0}", _commission.AmountWithCommission));    // "1,234,257";
                    var responseData2 = await GethashorUrl(InvoiceNumber, "flutterUrlverify");

                    JavaScriptSerializer js = new JavaScriptSerializer();
                    dynamic blogObject = js.Deserialize<dynamic>(responseData2);


                    var txnreverifystatus = blogObject["data"]["status"];
                    var txncurrency = blogObject["data"]["currency"];

                    //check txn verify flutter --when not succesfujl statsu got from txn verify & suceeful get from cllback 
                    if (InvoiceNumber != null && txnreverifystatus != "successful" && txncurrency == "XOF")
                    {

                        response.InvoiceNo = InvoiceNumber;
                        response.TotalAmount = "0";
                        response.WalletTxnStatus = "not successfull on flutter :" + txnreverifystatus;
                        response.WalletUserId = getInitialTransaction.WalletUserId.ToString();
                        response.CreatedDate = getInitialTransaction.CreatedDate;
                        response.EmailId = senderdata.EmailId; //use for user EmailId 

                    }
                    //after verify flutter txn then crediht to user --when succesfujl statsu got from both
                    else if (InvoiceNumber != null && txnreverifystatus == "successful" && txncurrency == "XOF")
                    {
                        response.InvoiceNo = InvoiceNumber;
                        response.TotalAmount = Convert.ToString(amountWithCommision);
                        response.WalletTxnStatus = "successfull on flutter :" + txnreverifystatus;
                        response.WalletUserId = getInitialTransaction.WalletUserId.ToString();
                        response.CreatedDate = getInitialTransaction.CreatedDate;
                        response.EmailId = senderdata.EmailId; //use for user EmailId 
                    }
                    else
                    {
                        response.InvoiceNo = InvoiceNumber;
                        response.TotalAmount = "0";

                        response.WalletTxnStatus = "check txn on postmen";
                        response.WalletUserId = getInitialTransaction.WalletUserId.ToString();
                        response.CreatedDate = getInitialTransaction.CreatedDate;
                        response.EmailId = senderdata.EmailId; //use for user EmailId 
                    }
                }
                else
                {
                    response.TotalAmount = "0";
                    response.WalletTxnStatus = "already txn refund plz check log";

                }
                response1.Add(response);
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("TxnUpdateService", "FlutterCheckTxnNotCaptureOurSide", InvoiceNumber);
            }


            return response1;
        }

        public async Task<MemoryStream> GenerateLogReportInfo()
        {
            var response = new TransactionLogsResponse2();
            var result = new MemoryStream();
            try
            {
                response = await _transactionLogRepository.GenerateLogReportInfo();
                result = await _generateLogReport.ExportReportInfo(response);
            }
            catch (Exception ex)
            {
                //  ex.Message.ErrorLog("WalletTransactionRepository.cs", "ViewPaymentRequests");
            }
            return result;

        }

        public async Task<bool> UpdateFlutterCheckTxnNotCaptureOurSide(WalletTxnRequest request)
        {
            var result = false;

            int rowAffected = await _transactionLogRepository.UpdateFlutterCheckTxnNotCaptureOurSide(request);
            if (rowAffected > 0)
            {
                result = true;
            }

            return result;
        }
        public async Task<string> GethashorUrl(string jsonReq, string flag)
        {

            string resBody = "";

            using (HttpClient client = new HttpClient())
            {
                // Call asynchronous network methods in a try/catch block to handle exceptions
                try
                {
                    if (flag == "flutterUrlverify")
                    {

                        // client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
                        //var content = new StringContent(null, Encoding.UTF8, "application/json");
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CommonSetting.flutterFLWSECKey);


                        var url = CommonSetting.flutterverifypaymentUrl + jsonReq;
                        HttpResponseMessage response = await client.GetAsync(url);
                        response.EnsureSuccessStatusCode();
                        resBody = await response.Content.ReadAsStringAsync();

                    }

                }
                catch (HttpRequestException e)
                {

                    if (flag == "adminflutterUrlverify")
                    { e.Message.ErrorLog("adminflutterUrlverify", e.StackTrace + " " + e.Message); }
                }
                return resBody;

            }
        }



    }
}
