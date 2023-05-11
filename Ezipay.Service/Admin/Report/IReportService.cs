using Ezipay.ViewModel.AdminViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.Admin.Report
{
    public interface IReportService
    {
        Task<List<BusinessResponse>> BusinessList();
        Task<List<CategoryResponse>> CategoryList(SubCategoryRequest request);
        Task<ReportResponse> TransactionSummaryByService(ReportRequest request);
    }
}
