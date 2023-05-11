using Ezipay.Database;
using Ezipay.ViewModel.AdminViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.AdminRepo.Banner
{
    public class BannerRepository : IBannerRepository
    {
        public async Task<List<BannerResponse>> GetBanner()
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                return await context.Database.SqlQuery<BannerResponse>
                        ("EXEC usp_GetBanners").ToListAsync();
            }
        }

        public async Task<Database.Banner> GetById(int id)
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                return await context.Banners.Where(x => x.Id == id).FirstOrDefaultAsync();
            }
        }

        public async Task<int> InsertBanner(Database.Banner entity)
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                context.Banners.Add(entity);
                return await context.SaveChangesAsync();
            }
        }

        public async Task<bool> Update(Database.Banner entity)
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                context.Entry(entity).State = EntityState.Modified;
                return await context.SaveChangesAsync() > 0;
            }
        }
    }
}
