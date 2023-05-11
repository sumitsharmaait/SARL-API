using Ezipay.Database;
using Ezipay.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Ezipay.Repository
{
    public class AppDownloadLogRepository : IAppDownloadLogRepository
    {
        public async Task<AppDownloadSearchResponse> GetDownloadLogList(AppDownloadSearchVM request)
        {
            var result = new AppDownloadSearchResponse();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                result.DataList = await (from d in db.AppDownloadLogs
                                         where d.Status == 0 && d.IsDeleted == false
                                         select new AppDownloadLogVM
                                         {
                                             Id = d.Id,
                                             DeviceUniqueId = d.DeviceId,
                                             DeviceToken = d.DeviceToken,
                                             DeviceType = d.DeviceType
                                         })
                                          .OrderByDescending(x => x.Id)
                                          .Skip((request.PageNumber - 1) * request.PageSize)
                                          .Take(request.PageSize).ToListAsync();

                result.TotalCount = await (from d in db.AppDownloadLogs
                                           where d.Status == 0
                                           && d.IsDeleted == false
                                           select d.Id
                                          ).CountAsync();

            }
            return result;
        }

        public async Task<int> Insert(AppDownloadLog entity)
        {
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                db.AppDownloadLogs.Add(entity);
                return await db.SaveChangesAsync();
            }
        }

        public async Task<bool> IsDeviceIdExist(string deviceId, string deviceToken)
        {
            bool result = false;
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                result = await db.AppDownloadLogs.AnyAsync(x => x.DeviceId == deviceId);
                if (result)
                {
                    var data = await db.AppDownloadLogs.Where(x => x.DeviceId == deviceId).FirstOrDefaultAsync();
                    if (data != null)
                    {
                        data.DeviceToken = deviceToken;
                        await db.SaveChangesAsync();
                    }
                }
            }
            return result;
        }



        public async Task<AppDownloadSearchResponse> GetActiveUserForNotification(AppDownloadSearchVM request)
        {
            var result = new AppDownloadSearchResponse();
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                result.DataList = await (from d in db.WalletUsers
                                         where d.IsDeleted == false
                                         select new AppDownloadLogVM
                                         {
                                             Id = d.WalletUserId,
                                             DeviceUniqueId = d.DeviceToken,
                                             DeviceToken = d.DeviceToken,
                                             DeviceType = (int)d.DeviceType
                                         })
                                          .OrderByDescending(x => x.Id)
                                          .Skip((request.PageNumber - 1) * request.PageSize)
                                          .Take(request.PageSize).ToListAsync();

                result.TotalCount = await (from d in db.WalletUsers
                                           where d.IsDeleted == false
                                           select d.WalletUserId
                                          ).CountAsync();

            }
            return result;
        }

        public async Task<int> InsertNotificationalertforweb(Notificationalert entity)
        {
            using (var db = new DB_9ADF60_ewalletEntities())
            {
                db.Notificationalerts.Add(entity);
                return await db.SaveChangesAsync();
            }
        }


        public async Task<List<SendNotificationResponse>> GetCurrentWebNotification()
        {

            //using (var db = new DB_9ADF60_ewalletEntities())
            //{
            //    return await (from d in db.Notificationalerts
            //                  select new SendNotificationResponse
            //                  {
            //                      Id = d.Id,
            //                      Title = d.Title,
            //                      MessageBody = d.MessageBody,
            //                      FileUpload=d.FileUpload,
            //                      CreatedDate = d.CreatedDate

            //                  }).OrderByDescending(x => x.Id).Take(5).ToListAsync();
            //}
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                return await context.Database.SqlQuery<SendNotificationResponse>
                        ("EXEC usp_GetNotificationalert").ToListAsync();
            }
        }

        public async Task<List<CountNotificationRequest>> GetCountCurrentWebNotification(CountNotificationRequest re)
        {
            DateTime date = DateTime.Now.Date;

            var result = new List<CountNotificationRequest>();

            using (var db = new DB_9ADF60_ewalletEntities())
            {

                result = await db.Database.SqlQuery<CountNotificationRequest>
                        ("EXEC usp_Getnotificationdata @walletuserid", new SqlParameter("@walletuserid", re.WalletUserId)).ToListAsync();


                var result1 = new List<CountNotificationRequest>();
                // var aa = result.Where(x => x.statusflag == null).Map(x => x.statusflag).ToList();

                result1 = (from a in result
                          where a.statusflag == null
                          select a).ToList();
                //foreach (var item in result)
                //{
                //    if (item.statusflag == null)
                //    {
                //        result = await (from d in db.Notificationalerts
                //                         //where DbFunctions.TruncateTime(d.CreatedDate) == date && d.Id == item.Id
                //                         where d.Id == item.Id
                //                         select new CountNotificationRequest
                //                         {
                //                             Id = d.Id
                //                         }).ToListAsync();
                //    }
                //}

                if (result != null)
                {
                    return result1; //

                }
            }
            return null; //change tbl

        }

        public async Task<int> UpdateCurrentWebNotification(notificationupdateRequest entity)
        {

            int[] i = entity.Id; // initialization
            foreach (var item in i)
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    var entity1 = new notificationupdate
                    {
                        NotificationId = item,
                        statusflag = entity.statusflag,
                        walletuserid = entity.walletuserid,
                        Createddate = DateTime.UtcNow
                    };
                    db.notificationupdates.Add(entity1);
                    await db.SaveChangesAsync();
                }
            }
            return 1;


        }



        //public async Task<SendNotificationResponse> GetCountCurrentWebNotification()
        //{
        //    DateTime date = DateTime.Now.Date;
        //    var result = new SendNotificationResponse();
        //    using (var db = new DB_9ADF60_ewalletEntities())
        //    {
        //        result.TotalCount = await (from d in db.notificationupdates
        //                                   join x in db.Notificationalerts
        //                                   on d.NotificationId equals x.Id
        //                                   select d).CountAsync();
        //        if (result.TotalCount == 0)
        //        {
        //            result.TotalCount = await (from d in db.Notificationalerts
        //                                       where DbFunctions.TruncateTime(d.CreatedDate) == date
        //                                       select d.Id).CountAsync();
        //        }
        //    }
        //    return result;

        //}
    }
}
