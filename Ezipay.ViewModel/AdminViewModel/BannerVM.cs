using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.ViewModel.AdminViewModel
{
    public class BannerResponse
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string BannerImage { get; set; }
        public string BannerUrl { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
    }

    public class BannerRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string BannerImage { get; set; }
        public string BannerUrl { get; set; }
        public long AdminId { get; set; } //log key
    }


    public class DeleteBannerRequest
    {
        public int id { get; set; }
        public long AdminId { get; set; } //log key
    }
}
