using Ezipay.Repository.AdminRepo.Banner;
using Ezipay.ViewModel.AdminViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.Admin.Banner
{
    public class BannerService : IBannerService
    {
        private readonly IBannerRepository _bannerRepository;

        public BannerService()
        {
            _bannerRepository = new BannerRepository();
        }

        public async Task<bool> DeleteBanner(int id)
        {
            bool result = false;

            var entity = await _bannerRepository.GetById(id);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.IsDeleted = true;
                result = await _bannerRepository.Update(entity);
            }

            return result;
        }

        public async Task<List<BannerResponse>> GetBanner()
        {
            return await _bannerRepository.GetBanner();
        }

        public async Task<bool> InsertBanner(BannerRequest request)
        {
            var result = false;
            var entity = new Database.Banner
            {
                BannerImage = request.BannerImage,
                BannerUrl = request.BannerUrl,
                Title = request.Title,
                Description = request.Description,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                WalletUserId = 0
            };

            int rowAffected = await _bannerRepository.InsertBanner(entity);
            if (rowAffected > 0)
            {
                result = true;
            }

            return result;
        }
    }
}
