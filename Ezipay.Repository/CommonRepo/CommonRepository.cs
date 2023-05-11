using ezeePay.Utility.Enums;
using Ezipay.Database;
using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.ViewModel.AirtimeViewModel;
using Ezipay.ViewModel.BannerViewModel;
using Ezipay.ViewModel.BillViewModel;
using Ezipay.ViewModel.CardPaymentViewModel;
using Ezipay.ViewModel.CheckPasswordViewModel;
using Ezipay.ViewModel.common;
using Ezipay.ViewModel.PayMoneyViewModel;
using Ezipay.ViewModel.WalletUserVM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.CommonRepo
{
    public class CommonRepository : ICommonRepository
    {

        public async Task<SetTransactionLimit> GetTransactionLimitForPayment(long walletUserId)
        {
            var response = new SetTransactionLimit();
            var userId = Convert.ToString(walletUserId);
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    response = await db.SetTransactionLimits.Where(x => x.UserId == userId).FirstOrDefaultAsync();
                }
            }
            catch
            {

            }
            return response;
        }

        public async Task<TransactionHistoryAddMoneyReponse> GetAllTransactionByDate(long WalletUserId)
        {
            var response = new TransactionHistoryAddMoneyReponse();
            var userId = Convert.ToString(WalletUserId);
            try
            {

                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    if (WalletUserId > 0)
                    {
                        response = await db.Database.SqlQuery<TransactionHistoryAddMoneyReponse>("exec usp_GetAllTransactionByDate @UserId",
                         new SqlParameter("@UserId", userId)
                         ).FirstOrDefaultAsync();
                    }
                }
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("MasterDataRepository.cs", "GetAllTransactionsAddMoney");
            }

            return response;
        }

        public async Task<int> GetWalletServiceId(int walletTransactionSubTypes)
        {
            int WalletServiceId = 0;
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                WalletServiceId = (int)await db.WalletServices.Where(x => x.ServiceCategoryId == walletTransactionSubTypes).Select(x => x.WalletServiceId).FirstOrDefaultAsync();
            }
            return WalletServiceId;
        }

        public async Task<int> GetMerchantId()
        {
            int MerchantCommissionServiceId = 0;
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                MerchantCommissionServiceId = (int)await db.WalletServices.Where(x => x.ServiceCategoryId == (int)WalletTransactionSubTypes.Merchants).Select(x => x.WalletServiceId).FirstOrDefaultAsync();
            }
            return MerchantCommissionServiceId;
        }

        public async Task<bool> IsMerchant(long walletUserId, int walletServiceId)
        {
            var response = new bool();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                response = await db.WalletServices.AnyAsync(x => x.MerchantId == walletUserId && x.WalletServiceId == walletServiceId);
            }
            return response;
        }

        public async Task<bool> IsService(long MerchantCommissionServiceId, long WalletUserId)
        {
            bool response = false;
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    response = await db.WalletServices.AnyAsync(x => x.WalletServiceId == MerchantCommissionServiceId && x.MerchantId == WalletUserId);
                }
            }
            catch
            {

            }
            return response;
        }


        public async Task<MerchantCommisionMaster> MerchantCommisionMasters(long MerchantCommissionServiceId)
        {
            var response = new MerchantCommisionMaster();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    response = await db.MerchantCommisionMasters.Where(x => x.WalletServiceId == MerchantCommissionServiceId && (bool)x.IsActive).FirstOrDefaultAsync();
                }
            }
            catch
            {

            }
            return response;
        }

        public async Task<WalletTransaction> SaveWalletTransaction(WalletTransaction request)
        {
            // var response = new WalletTransaction();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    db.WalletTransactions.Add(request);
                    await db.SaveChangesAsync();
                }
            }
            catch
            {

            }
            return request;
        }


        public async Task<WalletTransactionDetail> SaveWalletTransactionDetail(WalletTransactionDetail request)
        {
            // var response = new WalletTransactionDetail();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    db.WalletTransactionDetails.Add(request);
                    await db.SaveChangesAsync();
                }
            }
            catch
            {

            }
            return request;
        }

        public async Task<CommisionHistory> SaveCommisionHistory(CommisionHistory request)
        {
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    db.CommisionHistories.Add(request);
                    await db.SaveChangesAsync();
                }
            }
            catch
            {

            }
            return request;
        }

        public async Task<WalletUser> UpdateWalletUser(WalletUser request)
        {
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    int a = await db.SaveChangesAsync();
                }
            }
            catch
            {

            }
            return request;
        }

        public async Task<WalletUser> GetWalletUserByUserType(int userType, long walletUserId)
        {
            var response = new WalletUser();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    response = await db.WalletUsers.Where(x => x.WalletUserId == walletUserId && x.UserType == userType).FirstOrDefaultAsync();
                }
            }
            catch
            {

            }
            return response;
        }

        public async Task<int> GetWalletServiceId(int walletTransactionSubTypes, long walletUserId)
        {
            int WalletServiceId = 0;
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                WalletServiceId = (int)await db.WalletServices.Where(x => x.MerchantId == walletUserId && x.ServiceCategoryId == walletTransactionSubTypes).Select(x => x.WalletServiceId).FirstOrDefaultAsync();
            }
            return WalletServiceId;
        }

        public async Task<CheckLoginResponse> CheckPassword(string token)////
        {
            bool IsMatch = false;
            var response = new CheckLoginResponse();
            string TokenValue = "";
            try
            {
                ////string userToken = GlobalData.Key;
                string userToken = token;
                if (userToken != null)
                {
                    if (!string.IsNullOrEmpty(userToken))
                    {
                        TokenValue = userToken.ToString();
                    }
                }
                var AdminKeys = AES256.AdminKeyPair;
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    response = await db.Database.SqlQuery<CheckLoginResponse>("exec usp_UserDetailByToken @TokenValue",
                        new SqlParameter("@TokenValue", TokenValue)
                            ).FirstOrDefaultAsync();

                }
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("CommonRepository.cs", "CheckPassword");
            }

            return response;

        }

        public async Task<WalletUser> GetWalletUserById(long walletUserId)
        {
            var response = new WalletUser();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    response = await db.WalletUsers.FirstOrDefaultAsync(x => x.WalletUserId == walletUserId);
                }
            }
            catch
            {

            }
            return response;
        }


        public async Task<List<FeedbackTypeResponse>> FeedBackTypes()
        {
            var response = new List<FeedbackTypeResponse>();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    response = await db.Database.SqlQuery<FeedbackTypeResponse>("exec usp_FeedBackTypes").ToListAsync();
                }
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("CommonRepository.cs", "FeedBackTypes");
            }
            return response;
        }

        public async Task<bool> SaveFeedBack(Feedback request)
        {
            bool response = false;
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    db.Feedbacks.Add(request);
                    await db.SaveChangesAsync();

                }
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("UserProfileRepository.cs", "ChangePassword", request);
            }
            return response;
        }

        public async Task<bool> ChangeNotification(WalletUser walletUser)
        {
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                db.Entry(walletUser).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return true;
            }
        }


        public async Task<Callback> SendRequest(Callback callback)
        {
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    db.Callbacks.Add(callback);
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {


            }
            return callback;
        }

        public async Task<bool> InsertCallbackListTracking(CallbackListTracking callbackListTracking)
        {
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    db.CallbackListTrackings.Add(callbackListTracking);
                    int flag = await db.SaveChangesAsync();
                }

            }
            catch (Exception ex)
            {


            }
            return true;
        }

        public async Task<List<BannerVM>> GetBanner()
        {
            var response = new List<BannerVM>();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                response = await db.Database.SqlQuery<BannerVM>("exec usp_GetBannersFor_User").ToListAsync();
            }

            return response;
        }

        public async Task<UserDocumentResponse> ViewDocument(UserDocumentRequest request)
        {
            var response = new UserDocumentResponse();
            var AdminKeys = AES256.AdminKeyPair;
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                //var MerchantDetail = db.UserDocuments.Where(x => x.WalletUserId == request.WalletUserId).FirstOrDefault();
                response = await db.Database.SqlQuery<UserDocumentResponse>
                                        ("EXEC usp_GetDocumentDetails @WalletUserId",
                                        new SqlParameter("@WalletUserId", request.WalletUserId)
                                        ).FirstOrDefaultAsync();
            }
            return response;
        }

        public async Task<List<RecentReceiverResponse>> RecentReceiver(RecentReceiverRequest request)
        {
            var result = new List<RecentReceiverResponse>();
            try
            {
                long sender = Convert.ToInt32(request.SenderId);
                long ServiceId = Convert.ToInt32(request.ServiceId);
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    result = await db.Database.SqlQuery<RecentReceiverResponse>("exec usp_GetRecent_Receiver @SenderId,@ServiceId",
                        new object[]
                        {
                        new SqlParameter("@SenderId",sender),
                        new SqlParameter("@ServiceId",ServiceId),
                        }
                        ).ToListAsync();
                }
            }
            catch (Exception ex)
            {

            }
            return result;
        }

        public async Task<DetailForBillPaymentVM> GetDetailForBillPayment(PayMoneyAggregatoryRequest request)
        {
            var result = new DetailForBillPaymentVM();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandText = "usp_GetDetailForBillPayment";
                cmd.CommandType = CommandType.StoredProcedure;

                SqlParameter p1 = new SqlParameter("@WalletUserId", request.WalletUserId);
                cmd.Parameters.Add(p1);
                SqlParameter p2 = new SqlParameter("@channel", request.channel);
                cmd.Parameters.Add(p2);
                SqlParameter p3 = new SqlParameter("@ISD", request.ISD);
                cmd.Parameters.Add(p3);
                SqlParameter p4 = new SqlParameter("@ServiceCategoryId", request.ServiceCategoryId);
                cmd.Parameters.Add(p4);


                try
                {

                    db.Database.Connection.Open();
                    // Run the sproc
                    var reader = cmd.ExecuteReader();

                    // Read Blogs from the first result set
                    result.sender = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<WalletUser>(reader).FirstOrDefault();

                    reader.NextResult();
                    result.WalletService = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<WalletService>(reader).FirstOrDefault();

                    reader.NextResult();
                    result.SubCategory = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<SubCategory>(reader).FirstOrDefault();

                    reader.NextResult();
                    result.IsdocVerified = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<bool>(reader).FirstOrDefault();

                    reader.NextResult();
                    result.transactionLimit = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<TransactionLimitResponse>(reader).FirstOrDefault();

                    reader.NextResult();
                    result.transactionHistory = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<TransactionHistoryAddMoneyReponse>(reader).FirstOrDefault();
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    db.Database.Connection.Close();
                }

            }

            return result;
        }

        public async Task<SessionToken> IsValidToken(string token)
        {
            var result = new SessionToken();
            using (var db = new DB_9ADF60_ewalletEntities())
            {

                result = await db.SessionTokens.Where(x => x.TokenValue == token && x.IsDeleted == false).FirstOrDefaultAsync();
            }
            return result;
        }
        public async Task<SessionToken> UpdateTokenTime(string token)
        {
            var result = new SessionToken();
            using (var db = new DB_9ADF60_ewalletEntities())
            {

                result = await db.SessionTokens.Where(x => x.TokenValue == token && x.IsDeleted == false).FirstOrDefaultAsync();
                result.ExpiryTime = DateTime.UtcNow;
                db.Entry(result).State = EntityState.Modified;
                db.SaveChanges();
            }
            return result;
        }
    }
}
