using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.CheckPasswordViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Utility.ExcelGenerate
{
    public interface IGenerateLogReport
    {
        Task<MemoryStream> ExportReport(TransactionLogsResponce request);


        Task<MemoryStream> ExportReport1(MonthlyreportResponce request);

        Task<MemoryStream> ExportReportInfo(TransactionLogsResponse2 request);
    }
}
