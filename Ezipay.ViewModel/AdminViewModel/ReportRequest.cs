using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.ViewModel.AdminViewModel
{
    public class ReportRequest
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int CategoryId { get; set; }
        public long AdminId { get; set; } //log key
    }
    public class SubCategoryRequest
    {
        public int BusinessId { get; set; }
        public long AdminId { get; set; } //log key
    }
    public class ReportResponse
    {
        public int SelectedBusinessId { get; set; }
        public int SelectedCategoryId { get; set; }

        public decimal TotalRevenue { get; set; }
        public decimal TotalProfit { get; set; }
        public int TotalTransaction { get; set; }

    }

    public class BusinessResponse
    {
        public long BusinessId { get; set; }
        public string BusinessName { get; set; }
    }

    public class CategoryResponse
    {
        public int WalletServiceId { get; set; }
        public string ServiceName { get; set; }
    }
}
