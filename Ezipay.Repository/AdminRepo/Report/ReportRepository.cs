using Ezipay.Database;
using Ezipay.Utility.Extention;
using Ezipay.ViewModel.AdminViewModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.AdminRepo.Report
{
    public class ReportRepository : IReportRepository
    {
        /// <summary>
        /// Get Business list
        /// </summary>
        /// <returns></returns>
        public async Task<List<BusinessResponse>> BusinessList()
        {
            List<BusinessResponse> response = new List<BusinessResponse>();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    response = await db.Database.SqlQuery<BusinessResponse>
                                    ("EXEC usp_SubCategoryForReport").ToListAsync();
                }
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog("ReportRepository.cs", "BusinessList");
            }
            return response;
        }

        /// <summary>
        /// Get list of category
        /// </summary>
        /// <returns></returns>
        public async Task<List<CategoryResponse>> CategoryList(SubCategoryRequest request)
        {
            List<CategoryResponse> response = new List<CategoryResponse>();
            try
            {
                using (var db = new DB_9ADF60_ewalletEntities())
                {
                    response = await db.Database.SqlQuery<CategoryResponse>
                                     ("EXEC usp_WalletServiceList @SubcategoryId",
                                     new SqlParameter("@SubcategoryId", request.BusinessId)
                                     ).ToListAsync();
                }
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
                using (var db = new DB_9ADF60_ewalletEntities())
                {

                    if (request.DateFrom == DateTime.MinValue || request.DateTo == DateTime.MinValue || request.DateFrom == null || request.DateTo == null)
                    {
                        response = await db.Database.SqlQuery<ReportResponse>
                                     ("EXEC usp_WalletSummaryByService @WalletServiceId",
                                     new SqlParameter("@WalletServiceId", request.CategoryId)

                                     ).FirstOrDefaultAsync();
                    }
                    else
                    {
                        response = await db.Database.SqlQuery<ReportResponse>
                                         ("EXEC usp_WalletSummaryByService @WalletServiceId,@DateFrom,@DateTo",
                                         new SqlParameter("@WalletServiceId", request.CategoryId),
                                         new SqlParameter("@DateFrom", request.DateFrom),
                                         new SqlParameter("@DateTo", request.DateTo)
                                         ).FirstOrDefaultAsync();
                    }

                }
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("ReportRepository.cs", "TransactionByService");
            }
            return response;


        }

    }
}
