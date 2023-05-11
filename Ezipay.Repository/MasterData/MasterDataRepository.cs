using ezeePay.Utility.Enums;
using Ezipay.Database;
using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.ViewModel.CardPaymentViewModel;
using Ezipay.ViewModel.ChannelViewModel;
using Ezipay.ViewModel.CheckPasswordViewModel;
using Ezipay.ViewModel.CommisionViewModel;
using Ezipay.ViewModel.MasterDataViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Ezipay.Repository.MasterData
{
    public class MasterDataRepository : IMasterDataRepository
    {

        public async Task<List<IsdCodesResponse>> IsdCodes()
        {
            List<IsdCodesResponse> response = new List<IsdCodesResponse>();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    response = await db.Database.SqlQuery<IsdCodesResponse>("exec usp_IsdCodes").ToListAsync();
                    if (response != null && response.Count > 0)
                    {
                        foreach (var item in response)
                        {

                            item.CountryFlag = CommonSetting.flagImageurl + item.CountryFlag;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("AppUserRepository.cs", "IsdCodes");
            }
            return response;

        }
        public async Task<Country> IsdCodesby(string IsdCode)
        {
            var response = new Country();
            try
            {               
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    response = await db.Countries.Where(x => x.IsdCode == IsdCode).FirstOrDefaultAsync();
                }
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("AppUserRepository.cs", "IsdCodes");
            }
            return response;

        }


        public async Task<List<MainCategoryResponse>> MainCategory()
        {
            var response = new List<MainCategoryResponse>();

            using (var db = new DB_9ADF60_ewalletEntities())
            {
                response = await db.Database.SqlQuery<MainCategoryResponse>("EXEC usp_MainCategory").ToListAsync();
            }
            return response;
        }

        public async Task<List<SubCategoryResponse>> SubCategory(SubCategoryRequest request)
        {
            var response = new List<SubCategoryResponse>();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                if (request.MainCategoryId > 0)
                {

                    response = await db.Database.SqlQuery<SubCategoryResponse>("EXEC usp_SubCategory @MainCategoryId", new SqlParameter("@MainCategoryId",
                        request.MainCategoryId)).ToListAsync();
                }
                else
                {
                    response = await db.Database.SqlQuery<SubCategoryResponse>("exec usp_GetSubCategoryList").ToListAsync();
                }
            }
            return response;
        }

        public async Task<List<WalletServicesList>> WalletServices(WalletServicesRequest request)
        {
            var response = new List<WalletServicesList>();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                response = await db.Database.SqlQuery<WalletServicesList>("EXEC usp_WalletService @SubcategoryId,@SearchText,@PageNo,@PageSize",
                                new SqlParameter("@SubcategoryId", request.SubcategoryId),
                                new SqlParameter("@SearchText", request.SearchText),
                                new SqlParameter("@PageNo", request.PageNumber),
                                new SqlParameter("@PageSize", request.PageSize)
                                ).ToListAsync();
            }
            return response;
        }

        public async Task<WalletService> GetWalletServicesByIdOrChannel(string channel, long ServiceCategoryId, string ISD)
        {
            var WalletService = new WalletService();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                //WalletService = await db.WalletServices.Where(x => x.ServiceName == channel && x.ServiceCategoryId == ServiceCategoryId).FirstOrDefaultAsync();
                WalletService = db.WalletServices.Where(x => x.ServiceName == channel && x.IsdCode == ISD && x.ServiceCategoryId == ServiceCategoryId).FirstOrDefault();
            }
            return WalletService;
        }

        public async Task<SubCategory> GetWalletSubCategoriesById(long ServiceCategoryId)
        {
            var subcategory = new SubCategory();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                subcategory = await db.SubCategories.Where(x => x.SubCategoryId == ServiceCategoryId).FirstOrDefaultAsync();
            }
            return subcategory;
        }

        public async Task<InvoiceNumberResponse> GetInvoiceNumber(int digit = 6)
        {

            InvoiceNumberResponse res = new InvoiceNumberResponse();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                try
                {

                    var Invoice = await db.Database.SqlQuery<InvoiceNumberResponse>("EXEC usp_GetInvoiceNumber").FirstOrDefaultAsync();
                    if (Invoice != null)
                    {
                        res.Id = Invoice.Id;
                        res.InvoiceNumber = Invoice.InvoiceNumber;
                        int letters = res.Id.ToString().Length;

                        if (letters < 6)
                        {
                            int power = digit - res.Id.ToString().Length;
                            var str = Math.Pow(10, power).ToString().Replace("1", "");
                            res.AutoDigit = str + res.Id.ToString();
                        }
                        else
                        {
                            res.AutoDigit = res.Id.ToString();
                        }

                    }
                }
                catch (Exception)
                {
                    res.InvoiceNumber = CommonSetting.GetUniqueNumber();

                }
                return res;
            }

        }

        public async Task<List<ChannelResponce>> GetChannels(ChannelRequest request)
        {
            var response = new List<ChannelResponce>();

            try
            {

                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    if (request.ServiceCategoryId > 0)
                    {
                        //response = await db.Database.SqlQuery<ChannelResponce>("exec Usp_GetChannels @ServiceCategoryId",
                        // new SqlParameter("@ServiceCategoryId", request.ServiceCategoryId)
                        // ).ToListAsync();
                        response = await db.Database.SqlQuery<ChannelResponce>("exec Usp_GetChannels_Test @ServiceCategoryId,@IsdCode",
                           new SqlParameter("@ServiceCategoryId", request.ServiceCategoryId),
                           new SqlParameter("@IsdCode", request.IsdCode)
                           ).ToListAsync();
                    }
                }
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("MasterDataRepository.cs", "GetChannels");
            }

            return response;
        }

        public async Task<AddMoneyTransavtionLimitResponse> GetTransactionLimitAddMoney(string WalletUserId)
        {
            var response = new AddMoneyTransavtionLimitResponse();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                response = await db.Database.SqlQuery<AddMoneyTransavtionLimitResponse>("exec usp_SelectAddMoneyLimit @userid",
                    new object[]
                    {
                        new SqlParameter("@userid",WalletUserId)
                    }
                    ).FirstOrDefaultAsync();
            }
            return response;
        }

        public TransactionHistoryAddMoneyReponse GetAllTransactionsAddMoney(long WalletUserId)
        {
            var response = new TransactionHistoryAddMoneyReponse();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    if (WalletUserId > 0)
                    {
                        response = db.Database.SqlQuery<TransactionHistoryAddMoneyReponse>("exec usp_GetAllTransactionByDateForAddMoneyReq @WalletUserId",
                         new SqlParameter("@WalletUserId", WalletUserId)
                         ).FirstOrDefault();

                        //response.ForEach(x =>
                        //{
                        //    x.DocumentStatus = UserDetail.DocumetStatus;
                        //});                       
                    }
                }
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("MasterDataRepository.cs", "GetAllTransactionsAddMoney");
            }

            return response;
        }

        public async Task<CommisionMaster> GetCommisionByServiceId(long serviceId)
        {
            var response = new CommisionMaster();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                response = await db.CommisionMasters.Where(x => x.WalletServiceId == serviceId && (bool)x.IsActive).FirstOrDefaultAsync();
            }
            return response;
        }

        public async Task<AppServiceRepositoryResponse> AppServices()
        {
            var response = new AppServiceRepositoryResponse();
            var WalletServices = new List<WalletServiceResponse>();
            var PayServices = new List<WalletServiceResponse>();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                WalletServices = await db.Database.SqlQuery<WalletServiceResponse>("EXEC usp_WalletServices @ServiceCategoryId",
                    new SqlParameter("@ServiceCategoryId", (int)AppServiceTypes.WALLET_SERVICE)).ToListAsync();
                if (WalletServices != null && WalletServices.Count > 0)
                {
                    response.WalletServices = WalletServices;
                }

                PayServices = await db.Database.SqlQuery<WalletServiceResponse>("EXEC usp_WalletServices @ServiceCategoryId",
                     new SqlParameter("@ServiceCategoryId", (int)AppServiceTypes.PAY_SERVICE)).ToListAsync();
                if (PayServices != null && PayServices.Count > 0)
                {
                    response.PayServices = PayServices;
                }
                if (WalletServices == null && PayServices == null)
                {
                    response.IsSuccess = false;
                }
                else
                {
                    response.IsSuccess = true;
                }

            }
            return response;

        }

        public async Task<List<MerchantsResponse>> Merchant(long WalletUserId)
        {
            List<MerchantsResponse> response = new List<MerchantsResponse>();

            using (var db = new DB_9ADF60_ewalletEntities())
            {
                response = await db.Database.SqlQuery<MerchantsResponse>("EXEC usp_Merchants @UserType,@UserId",
                    new SqlParameter("@UserType", (int)WalletUserTypes.Merchant),
                     new SqlParameter("@UserId", WalletUserId)
                    ).ToListAsync();
            }

            return response;
        }

        public async Task<List<commissionOnAmountModel>> ServiceCommissionListForWeb(ChannelRequest request)
        {
            var response = new List<commissionOnAmountModel>();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                try
                {

                    response = await db.Database.SqlQuery<commissionOnAmountModel>("EXEC usp_GetCommisions @ServiceCategoryId",
                        new SqlParameter("@ServiceCategoryId", request.ServiceCategoryId)).ToListAsync();

                }
                catch (Exception)
                {

                }
                return response;

            }
        }

        public async Task<List<FAQResponse>> FAQ()
        {
            var response = new List<FAQResponse>();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    response = await db.Database.SqlQuery<FAQResponse>("exec usp_FAQ").ToListAsync();
                    if (response != null && response.Count > 0)
                    {
                        foreach (var item in response)
                        {
                            var details = db.Database.SqlQuery<FaqDetailResponse>("exec usp_FaqDetails @FaqId", new SqlParameter("@FaqId", item.FaqId)).ToList();
                            if (details != null && details.Count > 0)
                            {
                                item.FaqDetails = details;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("UserProfileRepository.cs", "FAQ");
            }
            return response;
        }

        public usp_GetCurrencyRate_Result GetCurrencyRate()
        {
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                return db.usp_GetCurrencyRate().FirstOrDefault();
            }
        }

        public async Task<List<IsdCodesResponse>> IsdCodesFrancCountry()
        {
            //  MessageService messageService = new MessageService();
            List<IsdCodesResponse> response = new List<IsdCodesResponse>();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    response = await db.Database.SqlQuery<IsdCodesResponse>("exec usp_IsdCodesForFrance").ToListAsync();
                    if (response != null && response.Count > 0)
                    {
                        foreach (var item in response)
                        {

                            item.CountryFlag = CommonSetting.flagImageurl + item.CountryFlag;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("AppUserRepository.cs", "IsdCodesForFrance");
            }
            return response;
        }

        public async Task<List<IspChannelResponse>> GetChannelsForISP(ChannelRequest request)
        {
            List<IspChannelResponse> responce = new List<IspChannelResponse>();
            var chenRes = new List<ChannelProductResponce>();
            try
            {

                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    //  var UserDetail = new AppUserRepository().UserProfile();
                    if ((request.ServiceCategoryId == 1) || (request.ServiceCategoryId == 3))
                    {
                        decimal amtCedi = 0;
                        decimal amtDoller = 0;
                        if (request.ServiceCategoryId == 1)
                        {
                            var res = db.usp_GetCurrencyRate().FirstOrDefault();
                            amtCedi = Convert.ToDecimal(res.CfaRate);
                            amtDoller = Convert.ToDecimal(res.DollarRate);
                            responce = await db.Database.SqlQuery<IspChannelResponse>("exec [Usp_GetChannels_Test] @ServiceCategoryId",
                               new SqlParameter("@ServiceCategoryId", request.ServiceCategoryId)
                               ).ToListAsync();
                        }
                        if (request.ServiceCategoryId == 3 && request.IsdCode != null)// && request.IsdCode == "+229")
                        {

                            responce = db.Database.SqlQuery<IspChannelResponse>("exec Usp_GetChannels_Test @ServiceCategoryId,@IsdCode",
                            new SqlParameter("@ServiceCategoryId", request.ServiceCategoryId),
                            new SqlParameter("@IsdCode", request.IsdCode)
                            ).ToList();
                        }
                        else if (request.ServiceCategoryId == 3 && request.IsdCode == "+225")
                        {
                            responce = db.Database.SqlQuery<IspChannelResponse>("exec Usp_GetChannels_Test @ServiceCategoryId,@IsdCode",
                                 new SqlParameter("@ServiceCategoryId", request.ServiceCategoryId),
                                 new SqlParameter("@IsdCode", request.IsdCode)
                                 ).ToList();
                        }

                        responce.ForEach(x =>
                        {
                            //x.DocumentStatus = UserDetail.DocumetStatus;
                            x.AmountInCedi = amtCedi;
                            x.AmountInDoller = amtDoller;
                        });
                        return responce;
                    }
                    else if (request.ServiceCategoryId == 12 && (request.IsdCode == "+221" || request.IsdCode == "+223" || request.IsdCode == "+225" || request.IsdCode == "+226" || request.IsdCode == "+245" || request.IsdCode == "+227" || request.IsdCode == "+229" || request.IsdCode == "+225" || request.IsdCode == "+228"))
                    {
                        responce = db.Database.SqlQuery<IspChannelResponse>("exec Usp_GetChannels_Test @ServiceCategoryId,@IsdCode",
                         new SqlParameter("@ServiceCategoryId", request.ServiceCategoryId),
                         new SqlParameter("@IsdCode", request.IsdCode)
                         ).ToList();

                    }
                    else if (request.ServiceCategoryId == 7 && (request.IsdCode == "+245" || request.IsdCode == "+227" || request.IsdCode == "+229" || request.IsdCode == "+225" || request.IsdCode == "+228"))
                    {
                        if (request.IsdCode == "+225")
                        {
                            responce = db.Database.SqlQuery<IspChannelResponse>("exec Usp_GetChannels_Test @ServiceCategoryId,@IsdCode",
                                        new SqlParameter("@ServiceCategoryId", request.ServiceCategoryId),
                                        new SqlParameter("@IsdCode", request.IsdCode)
                                        ).ToList();

                            //  responce.RemoveAt(3);
                        }

                    }
                    else if (request.ServiceCategoryId == 7 || request.ServiceCategoryId == 6)
                    {
                        responce = db.Database.SqlQuery<IspChannelResponse>("exec Usp_GetChannels_Test @ServiceCategoryId,@IsdCode",
                           new SqlParameter("@ServiceCategoryId", request.ServiceCategoryId),
                           new SqlParameter("@IsdCode", request.IsdCode)
                           ).ToList();

                    }
                    else if (request.ServiceCategoryId == 10 && request.IsdCode != "")
                    {
                        responce = db.Database.SqlQuery<IspChannelResponse>("exec Usp_GetChannels_Test @ServiceCategoryId,@IsdCode",
                           new SqlParameter("@ServiceCategoryId", request.ServiceCategoryId),
                           new SqlParameter("@IsdCode", request.IsdCode)
                           ).ToList();
                    }
                }
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("WalletTransactionRepository.cs", "CheckDataExistence");
            }
            return responce;
        }

        public async Task<WalletService> GetWalletServicesByIdOrChannel(string channel, long ServiceCategoryId)
        {
            var WalletService = new WalletService();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                WalletService = await db.WalletServices.Where(x => x.ServiceName == channel && x.ServiceCategoryId == ServiceCategoryId).FirstOrDefaultAsync();
            }
            return WalletService;
        }


        public async Task<List<ViewModel.AdminViewModel.DuplicateCardNoVMResponse>> GetcardnoList(long Walletuserid)
        {
            var list = new List<ViewModel.AdminViewModel.DuplicateCardNoVMResponse>();

            using (var context = new DB_9ADF60_ewalletEntities())
            {
                //first take current user id submitted
                list = await (from e in context.CardNoDuplicates
                              where e.WalletUserId == Walletuserid
                              select new ViewModel.AdminViewModel.DuplicateCardNoVMResponse
                              {
                                  Id = e.Id,
                                  WalletUserId = e.WalletUserId,
                                  CardNo = e.CardNo

                              }).ToListAsync();


                return list;
                // return null;
            }
        }


        public async Task<List<ManageWalletServicesList>> ManageWalletServices(WalletServicesRequest request)
        {
            var response = new List<ManageWalletServicesList>();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                response = await db.Database.SqlQuery<ManageWalletServicesList>("EXEC usp_ManageWalletService @SubcategoryId,@SearchText,@PageNo,@PageSize",
                                new SqlParameter("@SubcategoryId", request.SubcategoryId),
                                new SqlParameter("@SearchText", request.SearchText),
                                new SqlParameter("@PageNo", request.PageNumber),
                                new SqlParameter("@PageSize", request.PageSize)
                                ).ToListAsync();
            }
            return response;
        }
        public async Task<WalletService> GetWalletServicesForUpdate(long walletServiceId)
        {
            var WalletService = new WalletService();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                WalletService = await db.WalletServices.Where(x => x.WalletServiceId == walletServiceId).FirstOrDefaultAsync();
            }
            return WalletService;
        }
        public async Task<WalletService> UpdateWalletServicesStatus(WalletService walletService)
        {
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                db.Entry(walletService).State = EntityState.Modified;
                int s = await db.SaveChangesAsync();
                return walletService;
            }

        }


        public async Task<List<IsdCodesResponse>> IsdCodesForXAFCountry()
        {
            //  MessageService messageService = new MessageService();
            List<IsdCodesResponse> response = new List<IsdCodesResponse>();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    response = await db.Database.SqlQuery<IsdCodesResponse>("exec usp_IsdCodesForXAF").ToListAsync();
                    if (response != null && response.Count > 0)
                    {
                        foreach (var item in response)
                        {

                            item.CountryFlag = CommonSetting.flagImageurl + item.CountryFlag;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("AppUserRepository.cs", "IsdCodesForXAFCountry");
            }
            return response;
        }

        public async Task<List<IsdCodesResponse>> IsdCodesAddMonMobMonCountry()
        {
            //  MessageService messageService = new MessageService();
            List<IsdCodesResponse> response = new List<IsdCodesResponse>();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    response = await db.Database.SqlQuery<IsdCodesResponse>("exec usp_IsdCodesAddMonMobMonCountry").ToListAsync();
                    if (response != null && response.Count > 0)
                    {
                        foreach (var item in response)
                        {

                            item.CountryFlag = CommonSetting.flagImageurl + item.CountryFlag;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("AppUserRepository.cs", "IsdCodesAddMonMobMonCountry");
            }
            return response;
        }

        public async Task<List<IsdCodesResponse>> IsdCodesPayGhanaMobMonCountry()
        {
           
            List<IsdCodesResponse> response = new List<IsdCodesResponse>();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    response = await db.Database.SqlQuery<IsdCodesResponse>("exec usp_IsdCodesPayGhanaMobMon").ToListAsync();
                    if (response != null && response.Count > 0)
                    {
                        foreach (var item in response)
                        {

                            item.CountryFlag = CommonSetting.flagImageurl + item.CountryFlag;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("AppUserRepository.cs", "IsdCodesPayGhanaMobMonCountry");
            }
            return response;
        }


    }
}
