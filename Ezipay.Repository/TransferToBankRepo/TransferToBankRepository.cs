using Ezipay.Database;
using Ezipay.Utility.common;
using Ezipay.ViewModel.AirtimeViewModel;
using Ezipay.ViewModel.TransferToBankViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.TransferToBankRepo
{
    public class TransferToBankRepository : ITransferToBankRepository
    {
        public async Task<List<IsdCodesResponse1>> GetTransferttobankCountryList()
        {
          //  decimal amtXOFtoNGNRate = 0;
            

            List<IsdCodesResponse1> response = new List<IsdCodesResponse1>();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                  //  var res = db.usp_GetCurrencyRate().FirstOrDefault();
                   // amtXOFtoNGNRate = Convert.ToDecimal(res.NGNRate);
                  

                    response = await db.Database.SqlQuery<IsdCodesResponse1>("exec usp_GetTransferToBankCountry").ToListAsync();
                    if (response != null && response.Count > 0)
                    {
                        foreach (var item in response)
                        {
                            item.CountryFlag = CommonSetting.flagImageurl + item.CountryFlag;
                        }
                    }

                    //response.ForEach(x =>
                    //{
                        
                    //    x.AmountInNGN = amtXOFtoNGNRate;
                        
                    //});
                }
            }
            catch (Exception ex)
            {

                //ex.Message.ErrorLog("AppUserRepository.cs", "IsdCodes");
            }
            return response;
        }


        public async Task<List<BankListList>> GetBankList() //only active bank list here
        {
            var response = new List<BankListList>();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    response = await (from b in db.BankLists
                                      where b.IsActive== true
                                      select new BankListList
                                      {
                                          bankcode = b.BankCode,
                                          bankname = b.BankName
                                        
                                      }).ToListAsync();                   
                }
            }
            catch
            {

            }
            return response;
        }

        public async Task<WalletService> GetBankDetail(string bankCode)
        {
            var result = new WalletService();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                result = await db.WalletServices.Where(x => x.ServiceCategoryId == 6 && x.BankCode == bankCode && x.IsActive == true).FirstOrDefaultAsync();
            }
            return result;
        }

        public async Task<TransferToBankRequest1> SaveTransactionTransferToBankRequest(TransferToBankRequest1 request)
        {
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    db.TransferToBankRequest1.Add(request);
                    int res = await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {

            }
            return request;
        }

        public async Task<TransferToBankResponse1> SaveTransactionTransferToBankResponse(TransferToBankResponse1 response)
        {
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    db.TransferToBankResponse1.Add(response);
                    int res = await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {

            }
            return response;
        }

        public async Task<WalletService> GetWalletServices(string BankCode)
        {
            var response = new WalletService();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    response = await db.WalletServices.Where(x => x.BankCode == BankCode).FirstOrDefaultAsync();
                }
            }
            catch
            {

            }
            return response;
        }

        public async Task<WalletService> SaveNewBanks(WalletService request)
        {
            var response = new WalletService();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    db.WalletServices.Add(request);
                    await db.SaveChangesAsync();
                }
            }
            catch
            {

            }
            return response;
        }

        public async Task<TransferToBankResponseModel> PayMoneyTransferToBank(TransferToBankBeneficiaryNameRequest request)
        {

            var result = new TransferToBankResponseModel();
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
        public async Task<WalletTransaction> SaveWalletTransaction(WalletTransaction request)
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


        public async Task<int> UpdateCurrentBalance(string UpdatedCurrentBalance, long walletuserid)
        {
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    var Data = db.WalletUsers.Where(x => x.WalletUserId == walletuserid).FirstOrDefault();
                    if (Data != null)
                    {
                        Data.CurrentBalance = UpdatedCurrentBalance;
                        Data.UpdatedDate = DateTime.UtcNow;

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


        public async Task<WalletTransaction> WalletTransactionSave(WalletTransaction request)
        {

            using (var db = new DB_9ADF60_ewalletEntities())
            {
                db.WalletTransactions.Add(request);
                await db.SaveChangesAsync();
            }

            return request;
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




        public async Task<int> SaveSenderDetailsRequest(PayMoneyAggregatoryRequest request)
        {
            int res = 0;
            MobileMoneySenderDetail ObjDetail = new MobileMoneySenderDetail();
            try
            {

                ObjDetail.WalletuserId = request.WalletUserId;
                ObjDetail.SenderIdNumber = request.SenderIdNumber.ToUpper();
                ObjDetail.SenderIdType = request.SenderIdType;

                ObjDetail.SenderDateofbirth = Convert.ToDateTime(request.SenderDateofbirth);
                ObjDetail.SenderAddress = request.SenderAddress;
                ObjDetail.SenderCity = request.SenderCity;
                ObjDetail.ReceiverFirstName = request.ReceiverFirstName;
                ObjDetail.ReceiverLastName = request.ReceiverLastName;
                ObjDetail.Createddate = DateTime.Now;

                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    db.MobileMoneySenderDetails.Add(ObjDetail);
                    res = await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                return 0;
            }
            return res;
        }

        public async Task<List<senderIdTypetbl>> GetsenderidtypeList()
        {
            var response = new List<senderIdTypetbl>();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    response = await db.senderIdTypetbls.ToListAsync();
                }
            }
            catch
            {

            }
            return response;
        }
    }
}
