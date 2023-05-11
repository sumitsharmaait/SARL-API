using Ezipay.Repository.AdminRepo.Report;
using Ezipay.Utility.Extention;
using Ezipay.ViewModel.AdminViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.Admin.Report
{
    public class ReportService : IReportService
    {
        private IReportRepository _reportRepository;
        public ReportService()
        {
            _reportRepository = new ReportRepository();
        }

        /// <summary>
        /// Get Business list
        /// </summary>
        /// <returns></returns>
        public async Task<List<BusinessResponse>> BusinessList()
        {
            var response = new List<BusinessResponse>();
            try
            {
                response = await _reportRepository.BusinessList();
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("ReportService.cs", "BusinessList");
            }
            return response;
        }

        /// <summary>
        /// Get list of category
        /// </summary>
        /// <returns></returns>
        public async Task<List<CategoryResponse>> CategoryList(SubCategoryRequest request)
        {
            var response = new List<CategoryResponse>();
            try
            {
                response = await _reportRepository.CategoryList(request);
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("ReportRepository.cs", "CategoryList");
            }
            return response;
        }

        /// <summary>
        /// TransactionSummaryByService
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ReportResponse> TransactionSummaryByService(ReportRequest request)
        {

            var response = new ReportResponse();
            try
            {
                response = await _reportRepository.TransactionSummaryByService(request);
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("ReportRepository.cs", "TransactionByService");
            }
            return response;


        }
    }
}
