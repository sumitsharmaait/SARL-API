using Ezipay.ViewModel.AdminViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.AdminRepo.Banner
{
    public interface IBannerRepository
    {
        Task<int> InsertBanner(Database.Banner entity);
        Task<List<BannerResponse>> GetBanner();
        Task<Database.Banner> GetById(int id);
        Task<bool> Update(Database.Banner entity);
    }
}
