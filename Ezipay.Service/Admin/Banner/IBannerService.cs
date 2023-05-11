using Ezipay.ViewModel.AdminViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.Admin.Banner
{
    public interface IBannerService
    {
        Task<bool> InsertBanner(BannerRequest request);
        Task<List<BannerResponse>> GetBanner();
        Task<bool> DeleteBanner(int id);
    }
}
