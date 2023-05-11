using ezeePay.Utility.Enums;
using Ezipay.Database;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.AirtimeViewModel;
using Ezipay.ViewModel.CardPaymentViewModel;
using Ezipay.ViewModel.MerchantPaymentViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Ezipay.Repository.CardPayment
{
    public class CardPaymentRepository : ICardPaymentRepository
    {
        public async Task<CardAddMoneyResponse> CardPayment(CardPaymentRequest request, long walletUserId)
        {
            var response = new CardAddMoneyResponse();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                var CurrentUser = await db.WalletUsers.Where(x => x.WalletUserId == walletUserId).FirstOrDefaultAsync();
                //var _PayRequest = db.PayMoneyRequests.Where(x => x.PayMoneyRequestId == request.PayMoneyRequestId).FirstOrDefault();
                //new change
                var transactionLimit = db.usp_SelectAddMoneyLimit(Convert.ToString(CurrentUser.WalletUserId)).FirstOrDefault();
                int limit = transactionLimit != null ? Convert.ToInt32(transactionLimit.TransactionLimitForAddMoney) : 0;

                var transactionHistory = db.usp_GetAllTransactionByDateForAddMoneyReq(Convert.ToString(CurrentUser.WalletUserId)).FirstOrDefault();
                int totalAmountTransfered = transactionHistory != null ? Convert.ToInt32(transactionHistory.totalAmount) : 0;
                //=======

                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {

                        db.CardPaymentRequests.Add(request);
                        db.SaveChanges();
                    }

                    catch
                    {

                        tran.Rollback();
                    }
                }
            }
            return response;
        }

        public async Task<int> GetServiceId()
        {
            int WalletServiceId = 0;
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                WalletServiceId = (int)await db.WalletServices.Where(x => x.ServiceCategoryId == (int)WalletTransactionSubTypes.Credit_TO_Debit_Cards).Select(x => x.WalletServiceId).FirstOrDefaultAsync();
            }
            return WalletServiceId;
        }

        public string MerchantContent(MerchantTransactionRequest request, string TransactionId, string OrderNo, long? WalletUserId, int? TransactionStatus)
        {
            string result = string.Empty;
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    AddDuringPayRecord _record = new AddDuringPayRecord();
                    _record.amount = request.Amount;
                    _record.channel = string.Empty;
                    _record.chennelId = 0;
                    _record.Comment = !string.IsNullOrEmpty(request.Comment) ? request.Comment : string.Empty;
                    _record.customer = string.Empty;
                    _record.invoiceNo = string.Empty;
                    _record.IsAddDuringPay = true;
                    _record.IsMerchant = true;
                    _record.MerchantId = request.MerchantId;
                    _record.ISD = string.Empty;
                    _record.OrderNo = string.Empty;
                    _record.serviceCategory = string.Empty;
                    _record.Password = !string.IsNullOrEmpty(request.Password) ? request.Password : string.Empty;
                    _record.ServiceCategoryId = 0;
                    _record.TransactionNo = TransactionId;
                    _record.TransactionStatus = TransactionStatus;
                    _record.WalletUserId = WalletUserId;
                    _record.UpdatedDate = DateTime.UtcNow;
                    _record.CreatedDate = DateTime.UtcNow;
                    _record.IsDeleted = false;
                    _record.IsActive = true;
                    db.AddDuringPayRecords.Add(_record);
                    db.SaveChanges();
                    result = "Success";
                }
            }
            catch (Exception)
            {


            }
            return result;
        }

        public string PayMoneyContent(PayMoneyContent request, string TransactionId, string OrderNo, long? WalletUserId, int? TransactionStatus)
        {
            string result = string.Empty;
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {

                    AddDuringPayRecord _record = new AddDuringPayRecord();
                    _record.amount = request.amount;
                    _record.channel = request.channel;
                    _record.chennelId = request.chennelId;
                    _record.Comment = !string.IsNullOrEmpty(request.Comment) ? request.Comment : string.Empty;
                    _record.customer = request.customer;
                    _record.invoiceNo = !string.IsNullOrEmpty(request.invoiceNo) ? request.invoiceNo : string.Empty;
                    _record.IsAddDuringPay = true;
                    _record.IsMerchant = false;
                    _record.ISD = request.ISD;
                    _record.OrderNo = OrderNo;
                    _record.MerchantId = 0;
                    _record.serviceCategory = !string.IsNullOrEmpty(request.serviceCategory) ? request.serviceCategory : string.Empty;
                    _record.ServiceCategoryId = request.ServiceCategoryId;
                    _record.Password = !string.IsNullOrEmpty(request.Password) ? request.Password : string.Empty;
                    _record.TransactionNo = TransactionId;
                    _record.TransactionStatus = TransactionStatus;
                    _record.WalletUserId = WalletUserId;
                    _record.UpdatedDate = DateTime.UtcNow;
                    _record.CreatedDate = DateTime.UtcNow;
                    _record.IsDeleted = false;
                    _record.IsActive = true;
                    db.AddDuringPayRecords.Add(_record);
                    db.SaveChanges();
                    result = "Success";



                }
            }
            catch (Exception)
            {


            }
            return result;
        }


        public async Task<WalletService> GetWalletService(string serviceName, int serviceCategoryId)
        {
            var response = new WalletService();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    response = await db.WalletServices.Where(x => x.ServiceName == serviceName && x.ServiceCategoryId == serviceCategoryId).FirstOrDefaultAsync();
                }
            }
            catch
            {

            }
            return response;
        }


        public async Task<AddDuringPayRecord> MerchantContent(AddDuringPayRecord request)
        {
            var response = new AddDuringPayRecord();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    db.AddDuringPayRecords.Add(request);
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
            }
            return request;
        }

        public async Task<AddDuringPayRecord> PayMoneyContent(AddDuringPayRecord request)
        {
            var response = new AddDuringPayRecord();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    db.AddDuringPayRecords.Add(request);
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
            }
            return request;
        }

        public async Task<WalletTransaction> MobileMoneyForAddServices(WalletTransaction request, long WalletUserId = 0)
        {
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    db.WalletTransactions.Add(request);
                    int res = await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {

            }
            return request;
        }

        public async Task<AddDuringPayRecord> GetAddDuringPayRecord(int AddDuringPayRecordId, int TransactionStatus)
        {
            var response = new AddDuringPayRecord();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    response = await db.AddDuringPayRecords.Where(x => x.AddDuringPayRecordId == AddDuringPayRecordId && x.TransactionStatus == TransactionStatus).FirstOrDefaultAsync();
                }
            }
            catch
            {

            }
            return response;
        }

        public async Task<AddDuringPayRecord> UpdateAddDuringPayRecord(AddDuringPayRecord request)
        {
            var response = new AddDuringPayRecord();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    await db.SaveChangesAsync();
                }
            }
            catch
            {

            }
            return request;
        }

        public async Task<CardPaymentRequest> SaveCardPaymentRequest(CardPaymentRequest request)
        {
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                db.CardPaymentRequests.Add(request);
                await db.SaveChangesAsync();
            }
            return request;
        }

        public async Task<WalletUser> GetAdminUser()
        {
            var result = new WalletUser();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                result = await db.WalletUsers.Where(x => x.UserType == (int)WalletUserTypes.AdminUser).FirstOrDefaultAsync();
            }
            return result;
        }


        public async Task<CardPaymentRequest> GetCardPaymentRequest(string vpc_OrderInfo, string vpc_MerchTxnRef)
        {
            var result = new CardPaymentRequest();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                result = await db.CardPaymentRequests.Where(x => x.OrderNo == vpc_OrderInfo).FirstOrDefaultAsync();
            }
            return result;
        }


        public async Task<CardPaymentResponse> SaveCardPaymentResponse(CardPaymentResponse request)
        {
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                db.CardPaymentResponses.Add(request);
                await db.SaveChangesAsync();
            }
            return request;
        }

        public async Task<int> GetWalletService()
        {
            int response = 0;
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    response = (int)await db.WalletServices.Where(x => x.ServiceCategoryId == (int)WalletTransactionSubTypes.Credit_TO_Debit_Cards).Select(x => x.WalletServiceId).FirstOrDefaultAsync();
                }
            }
            catch
            {

            }
            return response;
        }


        public async Task<bool> IsWalletTransactions(long WalletUserId, string vpc_TransactionNo)
        {
            bool result = false;
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                result = !await db.WalletTransactions.AnyAsync(x => x.ReceiverId == WalletUserId && x.TransactionId == vpc_TransactionNo);
            }
            return result;
        }

        public async Task<WalletTransaction> SaveWalletTransactions(WalletTransaction request)
        {
            var result = new WalletTransaction();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                db.WalletTransactions.Add(request);
                await db.SaveChangesAsync();
            }
            return request;
        }

        public async Task<WalletTransactionDetail> SaveWalletTransactionDetails(WalletTransactionDetail request)
        {
            var result = new WalletTransaction();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                db.WalletTransactionDetails.Add(request);
                await db.SaveChangesAsync();
            }
            return request;
        }

        public async Task<AddDuringPayRecord> AddDuringPayRecords(string vpc_OrderInfo, string vpc_MerchTxnRef)
        {
            var result = new AddDuringPayRecord();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                result = await db.AddDuringPayRecords.Where(x => x.OrderNo == vpc_OrderInfo && x.TransactionNo == vpc_MerchTxnRef && x.TransactionStatus == (int)TransactionStatus.Pending).FirstOrDefaultAsync();
            }
            return result;
        }

        public async Task<PayMoneyAggregatoryRequest> AddDuringPayRecord(string vpc_OrderInfo, string vpc_MerchTxnRef)
        {
            var result = new PayMoneyAggregatoryRequest();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                // result= db.AddDuringPayRecords.Where(x => x.OrderNo ==vpc_OrderInfo && x.TransactionNo ==vpc_MerchTxnRef && x.TransactionStatus == (int)TransactionStatus.Pending).FirstOrDefault();

                result = await db.AddDuringPayRecords.Where(x => x.OrderNo == vpc_OrderInfo && x.TransactionNo == vpc_MerchTxnRef && x.TransactionStatus == (int)TransactionStatus.Pending).Select(x => new PayMoneyAggregatoryRequest
                {
                    Amount = x.amount,
                    channel = x.channel,
                    chennelId = x.chennelId ?? 0,
                    Comment = x.Comment,
                    customer = x.customer,
                    invoiceNo = x.invoiceNo,
                    IsAddDuringPay = x.IsAddDuringPay ?? false,
                    ISD = x.ISD,
                    serviceCategory = x.serviceCategory,
                    ServiceCategoryId = x.ServiceCategoryId ?? 0,
                    IsMerchant = x.IsMerchant ?? false,
                    MerchantId = x.MerchantId ?? 0
                }).FirstOrDefaultAsync();
            }
            return result;
        }


        public async Task<TransactionInitiateRequest> SaveTransactionInitiateRequest(TransactionInitiateRequest request)
        {
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    db.TransactionInitiateRequests.Add(request);
                    int res = await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {

            }
            return request;
        }

        public async Task<TransactionInitiateRequest> GetTransactionInitiateRequest(string InvoiceNumber)
        {
            var result = new TransactionInitiateRequest();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    result = await db.TransactionInitiateRequests.Where(x => x.InvoiceNumber == InvoiceNumber).FirstOrDefaultAsync();
                }
            }
            catch (Exception ex)
            {

            }
            return result;
        }
        public async Task<TransactionInitiateRequest> GetTxnInitiateRequest(string UserReferanceNumber)
        {
            var result = new TransactionInitiateRequest();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    result = await db.TransactionInitiateRequests.Where(x => x.UserReferanceNumber == UserReferanceNumber).FirstOrDefaultAsync();
                }
            }
            catch (Exception ex)
            {

            }
            return result;
        }
        public async Task<TransactionInitiateRequest> GetTransactionInitiateRequest(long id)
        {
            var result = new TransactionInitiateRequest();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    result = await db.TransactionInitiateRequests.Where(x => x.Id == id).FirstOrDefaultAsync();
                }
            }
            catch (Exception ex)
            {

            }
            return result;
        }

        public async Task<TransactionInitiateRequest> UpdateTransactionInitiateRequest(TransactionInitiateRequest request)
        {
            var result = new TransactionInitiateRequest();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    db.Entry(request).State = EntityState.Modified;
                    int s = await db.SaveChangesAsync();
                    return request;
                }
            }
            catch (Exception ex)
            {

            }
            return result;
        }

        public async Task<int> IsduplicateOrNotTransactionNo(string vpc_TransactionNo)
        {
            try
            {
                int Data = 0;

                await Task.Run(() =>
                {
                    using (var db = new DB_9ADF60_ewalletEntities())
                    {
                        Data = db.CardPaymentResponses.Where(x => x.vpc_TransactionNo == vpc_TransactionNo).Count();
                    }

                });

                if (Data == 1)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
            catch (Exception ex)
            {
                return -1;
            }

        }

        public async Task<int> SaveNewCardNo(long WalletUserId, string CardNo, string NewCardImage, string flag)
        {
            try
            {

                if (flag == "Check")
                {
                    using (var context = new DB_9ADF60_ewalletEntities())
                    {
                        var Data = context.Cardnewaddrequests.Where(x => x.WalletUserId == WalletUserId && x.CardNo == CardNo.Trim()).FirstOrDefault();
                        if (Data == null)
                        {
                            return 1;
                        }
                        else
                        {
                            return -1;
                        }

                    }
                }
                else
                {
                    using (var context = new DB_9ADF60_ewalletEntities())
                    {

                        var entity = new Cardnewaddrequest
                        {
                            WalletUserId = WalletUserId,
                            CardNo = CardNo,
                            Emailsend = true,
                            NewCardImage = NewCardImage,
                            CreatedDate = DateTime.UtcNow

                        };
                        context.Cardnewaddrequests.Add(entity);
                        int i = await context.SaveChangesAsync();

                        return i;

                    }
                }
            }
            catch (Exception ex)
            {
                return -1;
            }

        }


        public async Task<int> SaveCardNo(string InvoiceNumber, long WalletUserId, string CardNo, string PayTranId, string Requestedamount, string Totalamount, string EmailId)
        {
            try
            {

                using (var context = new DB_9ADF60_ewalletEntities())
                {
                    var entity = new Carduseinaddmoney
                    {
                        WalletUserId = WalletUserId,
                        EmailId = EmailId,
                        CardNo = CardNo,
                        RequestedBankTxnId = PayTranId,
                        ResponseBankTxnId = null,
                        RequestedamountXOF = Requestedamount,
                        InvoiceNumber = InvoiceNumber,
                        TotalamountXOF = Totalamount,
                        CreatedDate = DateTime.UtcNow
                    };
                    context.Carduseinaddmoneys.Add(entity);
                    await context.SaveChangesAsync();

                    return 1;

                }
            }
            catch (Exception ex)
            {
                return -1;
            }

        }

        public async Task<int> UpdateNewCardNoResponseBankCode(string InvoiceNumber, long WalletUserId, string transactionID)
        {
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    var Data = db.Carduseinaddmoneys.Where(x => x.WalletUserId == WalletUserId && x.InvoiceNumber == InvoiceNumber).FirstOrDefault();
                    if (Data != null)
                    {
                        Data.ResponseBankTxnId = transactionID;
                        Data.UpdateDate = DateTime.UtcNow;

                        db.Entry(Data).State = EntityState.Modified;
                        await db.SaveChangesAsync();
                    }
                    else
                    {
                        return -1;
                    }
                    return 1;
                }


            }
            catch (Exception ex)
            {
                return -1;
            }

        }


        public async Task<ThirdPartyPaymentByCard> SaveThirdPartyPaymentByCard(ThirdPartyPaymentByCard request)
        {
            var response = new ThirdPartyPaymentByCard();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    db.ThirdPartyPaymentByCards.Add(request);
                    await db.SaveChangesAsync();
                }
            }
            catch
            {

            }
            return request;
        }

        public async Task<MasterCardPaymentRequest> SaveMasterCardPaymentRequest(MasterCardPaymentRequest request)
        {
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                db.MasterCardPaymentRequests.Add(request);
                await db.SaveChangesAsync();
            }
            return request;
        }


        public async Task<MasterCardPaymentRequest> GetMasterCardPaymentRequest(string SuccessIndicator)
        {
            var result = new MasterCardPaymentRequest();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                result = await db.MasterCardPaymentRequests.Where(x => x.SuccessIndicator == SuccessIndicator).FirstOrDefaultAsync();
            }
            return result;
        }

        public async Task<int> GetWalletTransactionsexist(long? WalletUserId, string InvoiceNo)
        {
            int result = 0;
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                result = await db.WalletTransactions.Where(x => x.SenderId == WalletUserId && x.InvoiceNo == InvoiceNo).CountAsync();
            }
            return result;
        }
        public async Task<int> Checkrefundtoinvoiceno(long? WalletsenderId, long? WalletUserId, string InvoiceNo)
        {
            int result = 0;
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                result = await db.WalletTransactions.Where(x => x.SenderId == WalletsenderId && x.ReceiverId == WalletUserId && x.TransactionId.Contains(InvoiceNo)).CountAsync();
            }
            return result;
        }

        public async Task<List<TItxnresponse>> GetTransactionInitiateRequestjsonresponse()
        {
            var result = new List<TItxnresponse>();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    //result = await db.TransactionInitiateRequests.Where(x => x.JsonResponse == "" && x.CreatedDate.Value.Date == DateTime.Now.Date).ToListAsync();


                    result = await (from d in db.TransactionInitiateRequests
                                    join x in db.webhookflutters
                                      on d.InvoiceNumber equals x.JsonData
                                    where d.JsonResponse == "" && x.flag == "0" && x.MethodName == "webhookXOFFlutter"
                                    //where  DbFunctions.TruncateTime(x.CreatedDate) == dt.Date
                                    // where d.JsonResponse == "" && DbFunctions.TruncateTime(x.CreatedDate) == dt
                                    select new TItxnresponse
                                    {
                                        Id = d.Id,
                                        WalletUserId = d.WalletUserId,
                                        InvoiceNumber = d.InvoiceNumber,
                                        JsonResponse = d.JsonResponse,
                                        AfterTransactionBalance = d.AfterTransactionBalance

                                    }).ToListAsync();


                }

            }
            catch (Exception ex)
            {

            }
            return result;
        }


        public async Task<int> Updatewebhookflutterflagsuccestxn()
        {
            var result = new List<TItxnresponse>();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    result = await (from d in db.TransactionInitiateRequests
                                    join x in db.webhookflutters
                                      on d.InvoiceNumber equals x.JsonData
                                    where d.JsonResponse != "" && x.flag == "0" && x.MethodName == "webhookXOFFlutter"
                                    select new TItxnresponse
                                    {
                                        Id = d.Id,
                                        WalletUserId = d.WalletUserId,
                                        InvoiceNumber = d.InvoiceNumber,
                                        JsonResponse = d.JsonResponse,
                                        AfterTransactionBalance = d.AfterTransactionBalance

                                    }).ToListAsync();

                    for (int i = 0; i < result.Count; i++)
                    {
                        var InvoiceNumber = result[i].InvoiceNumber;


                        var Data = db.webhookflutters.Where(x => x.JsonData == InvoiceNumber && x.flag == "0" && x.MethodName == "webhookXOFFlutter").FirstOrDefault();
                        if (Data != null)
                        {
                            Data.flag = "1";
                            db.Entry(Data).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                        }
                        else
                        {
                            return -1;
                        }

                    }
                    return 1;
                }


            }
            catch (Exception ex)
            {
                return -1;
            }
        }


        public async Task<int> Updatewebhookflutterflagsuccestxninvoiceno(string InvoiceNumber)
        {
            var result = new List<TItxnresponse>();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {

                    var Data = db.webhookflutters.Where(x => x.JsonData == InvoiceNumber && x.flag == "0" && x.MethodName == "webhookXOFFlutter").FirstOrDefault();
                    if (Data != null)
                    {
                        Data.flag = "1";
                        db.Entry(Data).State = EntityState.Modified;
                        await db.SaveChangesAsync();
                    }
                    else
                    {
                        return -1;
                    }


                    return 1;
                }


            }
            catch (Exception ex)
            {
                return -1;
            }



        }


        public async Task<WalletTransaction> GetWalletTransaction(long? WalletUserId, string TransactionId)
        {
            var response = new WalletTransaction();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    response = await db.WalletTransactions.Where(x => x.TransactionId == TransactionId && x.TransactionStatus == 2 && x.SenderId == WalletUserId).FirstOrDefaultAsync();
                }
            }
            catch
            {

            }
            return response;
        }



        public async Task<WalletTransaction> UpdateWalletTransaction(WalletTransaction request)
        {
            var response = new WalletTransaction();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    db.Entry(request).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                }
            }
            catch
            {

            }
            return response;

        }


        public async Task<Carduseinaddmoney> GetMasterCarduseinaddmoneyPaymentRequest(long WalletUserId, string InvoiceNumber)
        {
            var result = new Carduseinaddmoney();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                result = await db.Carduseinaddmoneys.Where(x => x.WalletUserId == WalletUserId && x.InvoiceNumber == InvoiceNumber).FirstOrDefaultAsync();
            }
            return result;
        }


        public async Task<int> CheckBeninUserTxnPerde(long walletUserId)
        {
            int i = 0;
           
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    i = await db.Database.SqlQuery<int>("EXEC sp_beninuseraddmoneonetxn @walletuserid, @flag",
                                new SqlParameter("@walletuserid", walletUserId),
                                new SqlParameter("@flag", "addmone")
                                   ).FirstOrDefaultAsync();
                    return i;
                }
            }
            catch (Exception ex)
            {

               
            }
            return i;

        }

    }
}
