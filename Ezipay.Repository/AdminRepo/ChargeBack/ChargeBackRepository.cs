using Ezipay.Database;
using Ezipay.Utility.Extention;
using Ezipay.ViewModel.AdminViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Ezipay.Repository.AdminRepo.ChargeBack
{
    public class ChargeBackRepository : IChargeBackRepository
    {
        public async Task<List<Database.ChargeBack>> GetChargeBackList()
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                return await context.ChargeBacks.Where(x => x.DeleteFlag == "N").OrderByDescending(x => x.Createddate).ToListAsync();
            }
        }

        public async Task<int> InsertChargeBackDetail(ChargeBackRequest request)
        {
            try
            {
                if (request.DeleteFlag == "Y") //user get delete by admin after debit ChargeBack through user amount-only flag update not delete properly        
                {
                    using (var db = new DB_9ADF60_ewalletEntities())
                    {

                        var Data = db.ChargeBacks.Where(x => x.Walletuserid == request.Walletuserid && x.id==request.id).FirstOrDefault();
                        if (Data != null)
                        {
                           //Data.Walletuserid = request.Walletuserid;
                            Data.DeleteFlag = "Y";
                            Data.Deleteby = request.Deleteby;
                            Data.Deleteddate = DateTime.UtcNow;
                            Data.Comment = request.Comment;
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
                else//insert into chargeback record by admin
                {
                    using (var db = new DB_9ADF60_ewalletEntities())
                    {
                       var count = await db.ChargeBacks.Where(x => x.DeleteFlag == "N" && x.Walletuserid == request.Walletuserid).CountAsync();
                        if (count == 0)
                        {
                            var Data = db.ViewUserLists.Where(x => x.WalletUserId == request.Walletuserid).FirstOrDefault();
                            var entity1 = new Database.ChargeBack
                            {
                                Walletuserid = request.Walletuserid,
                                UserEmail = Data.EmailId,
                                UserMobileNo = Data.MobileNo,
                                IsActiveStatus = false,
                                CurrentBalance = Data.Currentbalance,
                                AmountLimit = request.Amount.Trim(),
                                Createdby = request.Createdby,
                                Createddate = DateTime.UtcNow,
                                DeleteFlag = "N",
                                Deleteby = null,
                                Deleteddate = null,
                                Comment = request.Comment
                            };

                            db.ChargeBacks.Add(entity1);
                            await db.SaveChangesAsync();
                            return 1;
                        }
                        else
                        {
                            return -1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("ChargeBackRepository.cs", "InsertChargeBackDetail");
                return -1;

            }


        }

        public async Task<List<Database.ChargeBack>> GetChargeBackListById(ChargeBackRequest request)
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                return await context.ChargeBacks.Where(x => x.DeleteFlag == "N" && x.Walletuserid == request.Walletuserid).OrderByDescending(x => x.Createddate).ToListAsync();
            }
        }


        ////
        ///
        public async Task<List<Database.freezeuser>> GetfreezeList()
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                return await context.freezeusers.OrderByDescending(x => x.Createddate).ToListAsync();
            }
        }

        public async Task<int> InsertfreezeDetail(freezeRequest request)
        {
            try
            {
                if (request.DeleteFlag == "Y") //user get delete by admin after debit ChargeBack through user amount-only flag update not delete properly        
                {
                    using (var db = new DB_9ADF60_ewalletEntities())
                    {

                        var Data = db.freezeusers.Where(x => x.Walletuserid == request.Walletuserid && x.id == request.id).FirstOrDefault();
                        if (Data != null)
                        {
                            //Data.Walletuserid = request.Walletuserid;
                            Data.UnFreezeComment = request.UnFreezeComment.Trim();
                            Data.DeleteFlag = "Y";
                            Data.Deleteby = request.Deleteby;
                            Data.Deleteddate = DateTime.UtcNow;

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
                else//insert into record by admin
                {
                    using (var db = new DB_9ADF60_ewalletEntities())
                    {
                        var count = await db.freezeusers.Where(x => x.DeleteFlag == "N" && x.Walletuserid == request.Walletuserid).CountAsync();
                        if (count == 0)
                        {
                            var Data = db.ViewUserLists.Where(x => x.WalletUserId == request.Walletuserid).FirstOrDefault();
                            var entity1 = new Database.freezeuser
                            {
                                Walletuserid = request.Walletuserid,
                                AmountLimit = request.Amount.Trim(),
                                Createdby = request.Createdby,
                                Createddate = DateTime.UtcNow,
                                DeleteFlag = "N",
                                Deleteby = null,
                                Deleteddate = null,
                                UserEmail = Data.EmailId,
                                FreezeComment = request.FreezeComment,
                                UnFreezeComment = null
                            };

                            db.freezeusers.Add(entity1);
                            await db.SaveChangesAsync();
                            return 1;
                        }
                        else
                        {
                            return -1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("ChargeBackRepository.cs", "InsertfreezeDetail");
                return -1;
            }
        }


        public async Task<List<Database.freezeuser>> GetfreezeById(long Walletuserid)
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                return await context.freezeusers.Where(x => x.DeleteFlag == "N" && x.Walletuserid == Walletuserid).OrderByDescending(x => x.Createddate).ToListAsync();
            }
        }


    }
}
