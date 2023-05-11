using Ezipay.ViewModel.AdminViewModel;
using NPOI.HPSF;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ezipay.Utility.ExcelGenerate
{
    public class GenerateLogReport : IGenerateLogReport
    {
        // private ITransactionLogRepository transactionLogRepository;
        public GenerateLogReport()
        {
            // _userRepository = new UserRepository();
        }

        private HSSFWorkbook _hssfWorkbook;
        public async Task<MemoryStream> ExportReport(TransactionLogsResponce request)
        {
            InitializeWorkbook();
            await GenerateData(request);
            return GetExcelStream();
        }

        public async Task<MemoryStream> ExportReport1(MonthlyreportResponce request)
        {
            InitializeWorkbook();
            await GenerateData1(request);
            return GetExcelStream();
        }

        public async Task<MemoryStream> ExportReportInfo(TransactionLogsResponse2 request)
        {
            InitializeWorkbook();
            await GenerateDataInfo(request);
            return GetExcelStream();
        }

        MemoryStream GetExcelStream()
        {
            //Write the stream data of workbook to the root directory
            MemoryStream file = new MemoryStream();
            _hssfWorkbook.Write(file);
            return file;
        }

        private async Task GenerateData(TransactionLogsResponce response)
        {

            try
            {
                ICellStyle style1 = _hssfWorkbook.CreateCellStyle();
                style1.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
                style1.FillPattern = FillPattern.SolidForeground;

                //var response = await _userRepository.GenerateLogReport(request);

                ISheet sheet1 = _hssfWorkbook.CreateSheet("EzipayLog");
                sheet1.SetColumnWidth(0, 1500);
                sheet1.SetColumnWidth(1, 4000);
                sheet1.SetColumnWidth(2, 4000);
                sheet1.SetColumnWidth(3, 8000);
                sheet1.SetColumnWidth(4, 8000);
                sheet1.SetColumnWidth(5, 8000);
                sheet1.SetColumnWidth(6, 4000);
                sheet1.SetColumnWidth(7, 8000);
                sheet1.SetColumnWidth(8, 4000);
                sheet1.SetColumnWidth(9, 4000);
                sheet1.SetColumnWidth(10, 8000);
                sheet1.SetColumnWidth(11, 4000);
                sheet1.SetColumnWidth(12, 4000);
                sheet1.SetColumnWidth(13, 4000);
                sheet1.SetColumnWidth(14, 15000);
                sheet1.SetColumnWidth(15, 4000);
                sheet1.SetColumnWidth(16, 8000); //ReceiverCountryName
                sheet1.SetColumnWidth(17, 8000); //SenderCountryName
                //----------Create Header-----------------
                var R0 = sheet1.CreateRow(0);

                var C00 = R0.CreateCell(0);
                C00.SetCellValue("S.No");
                C00.CellStyle = style1;

                var C01 = R0.CreateCell(1);
                C01.SetCellValue("WalletTransactionId");
                C01.CellStyle = style1;

                var C02 = R0.CreateCell(2);
                C02.SetCellValue("Transactionid");
                C02.CellStyle = style1;

                var C03 = R0.CreateCell(3);
                C03.SetCellValue("Date");
                C03.CellStyle = style1;

                var C04 = R0.CreateCell(4);
                C04.SetCellValue("Time");
                C04.CellStyle = style1;

                var C05 = R0.CreateCell(5);
                C05.SetCellValue("CategoryName");
                C05.CellStyle = style1;

                var C06 = R0.CreateCell(6);
                C06.SetCellValue("ServiceName");
                C06.CellStyle = style1;

                var C07 = R0.CreateCell(7);
                C07.SetCellValue("TransactionType");
                C07.CellStyle = style1;

                var C08 = R0.CreateCell(8);
                C08.SetCellValue("TotalAmount");
                C08.CellStyle = style1;

                var C09 = R0.CreateCell(9);
                C09.SetCellValue("CommisionAmount");
                C09.CellStyle = style1;

                var C10 = R0.CreateCell(10);
                C10.SetCellValue("WalletAmount");
                C10.CellStyle = style1;

                var C11 = R0.CreateCell(11);
                C11.SetCellValue("Name");
                C11.CellStyle = style1;

                var C12 = R0.CreateCell(12);
                C12.SetCellValue("AccountNo");
                C12.CellStyle = style1;

                var C13 = R0.CreateCell(13);
                C13.SetCellValue("TransactionStatus");
                C13.CellStyle = style1;

                var C14 = R0.CreateCell(14);
                C14.SetCellValue("Comment");
                C14.CellStyle = style1;

                var C15 = R0.CreateCell(15);
                C15.SetCellValue("Walletuserid");
                C15.CellStyle = style1;

                var C16 = R0.CreateCell(16);
                C16.SetCellValue("ReceiverCountryName");
                C16.CellStyle = style1;

                var C17 = R0.CreateCell(17);
                C17.SetCellValue("SenderCountryName");
                C17.CellStyle = style1;

                int i = 1;
                foreach (var item in response.TransactionLogslist)
                {
                    IRow row = sheet1.CreateRow(i);

                    var C0 = row.CreateCell(0);
                    C0.SetCellValue(i.ToString());

                    var C1 = row.CreateCell(1);
                    C1.SetCellValue(item.WalletTransactionId);

                    var C2 = row.CreateCell(2);
                    C2.SetCellValue(item.transactionid);

                    var c3 = row.CreateCell(3);
                    c3.SetCellValue(item.Date.ToString());

                    var c4 = row.CreateCell(4);
                    c4.SetCellValue(item.Time);

                    var c5 = row.CreateCell(5);
                    c5.SetCellValue(item.categoryname.ToString());

                    var c6 = row.CreateCell(6);
                    c6.SetCellValue(item.servicename.ToString());

                    var c7 = row.CreateCell(7);
                    c7.SetCellValue(item.transactionType);

                    var c8 = row.CreateCell(8);
                    c8.SetCellValue(item.totalAmount);

                    var c9 = row.CreateCell(9);
                    c9.SetCellValue(item.commisionAmount);


                    var c10 = row.CreateCell(10);
                    c10.SetCellValue(item.walletAmount.ToString());

                    var c11 = row.CreateCell(11);
                    c11.SetCellValue(item.name.ToString());

                    var c12 = row.CreateCell(12);
                    c12.SetCellValue(item.accountNo);

                    var c13 = row.CreateCell(13);
                    c13.SetCellValue(item.transactionStatus);

                    var c14 = row.CreateCell(14);
                    c14.SetCellValue(item.comments.ToString());

                    var c15 = row.CreateCell(15);
                    c15.SetCellValue(item.walletuserid);

                    var c16 = row.CreateCell(16);
                    c16.SetCellValue(item.ReceiverCountryName);

                    var c17 = row.CreateCell(17);
                    c17.SetCellValue(item.SenderCountryName);

                    i++;
                }

            }
            catch (Exception ex)
            {

            }
        }


        private async Task GenerateData1(MonthlyreportResponce response)
        {

            try
            {
                ICellStyle style1 = _hssfWorkbook.CreateCellStyle();
                style1.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
                style1.FillPattern = FillPattern.SolidForeground;
                style1.Alignment = HorizontalAlignment.Center;
                style1.VerticalAlignment = VerticalAlignment.Center;
                //var response = await _userRepository.GenerateLogReport(request);

                ISheet sheet1 = _hssfWorkbook.CreateSheet("EzipayLog");
                sheet1.SetColumnWidth(0, 1500); //s.no.
                sheet1.SetColumnWidth(1, 4000); //
                sheet1.SetColumnWidth(2, 8000);
                sheet1.SetColumnWidth(3, 8000);
                sheet1.SetColumnWidth(4, 8000);
                sheet1.SetColumnWidth(5, 8000);
                sheet1.SetColumnWidth(6, 8000);
                sheet1.SetColumnWidth(7, 8000);
                sheet1.SetColumnWidth(8, 8000);
                sheet1.SetColumnWidth(9, 8000);
                sheet1.SetColumnWidth(10, 8000);
                sheet1.SetColumnWidth(11, 8000);
                sheet1.SetColumnWidth(12, 8000);
                sheet1.SetColumnWidth(13, 8000);
                sheet1.SetColumnWidth(14, 8000);
                sheet1.SetColumnWidth(15, 8000);
                sheet1.SetColumnWidth(16, 8000); //ReceiverCountryName
                sheet1.SetColumnWidth(17, 8000); //SenderCountryName
                sheet1.SetColumnWidth(18, 8000); //ReceiverCountryName
                sheet1.SetColumnWidth(19, 8000); //SenderCountryName
                sheet1.SetColumnWidth(20, 8000); //ReceiverCountryName
                sheet1.SetColumnWidth(21, 8000); //SenderCountryName
                sheet1.SetColumnWidth(22, 8000); //ReceiverCountryName
                sheet1.SetColumnWidth(23, 8000); //SenderCountryName
                sheet1.SetColumnWidth(24, 8000); //ReceiverCountryName
                sheet1.SetColumnWidth(25, 8000); //SenderCountryName
                sheet1.SetColumnWidth(26, 8000); //ReceiverCountryName
                sheet1.SetColumnWidth(27, 8000); //SenderCountryName
                sheet1.SetColumnWidth(28, 8000); //ReceiverCountryName
                sheet1.SetColumnWidth(29, 8000); //SenderCountryName
                sheet1.SetColumnWidth(30, 8000); //ReceiverCountryName
                sheet1.SetColumnWidth(31, 8000); //SenderCountryName
                sheet1.SetColumnWidth(33, 8000); //SenderCountryName

                //----------Create Header-----------------
                var R0 = sheet1.CreateRow(0);

                var C00 = R0.CreateCell(0);
                C00.SetCellValue("S.No.");
                C00.CellStyle = style1;

                var C01 = R0.CreateCell(1);
                C01.SetCellValue("Particulars");
                C01.CellStyle = style1;

                var C02 = R0.CreateCell(2);
                C02.SetCellValue("01-" + response.Monthlyreportlist[0].Month + "-" + response.Monthlyreportlist[0].Yr);
                C02.CellStyle = style1;

                var C03 = R0.CreateCell(3);
                C03.SetCellValue("02-" + response.Monthlyreportlist[0].Month + "-" + response.Monthlyreportlist[0].Yr);
                C03.CellStyle = style1;

                var C04 = R0.CreateCell(4);
                C04.SetCellValue("03-" + response.Monthlyreportlist[0].Month + "-" + response.Monthlyreportlist[0].Yr);
                C04.CellStyle = style1;

                var C05 = R0.CreateCell(5);
                C05.SetCellValue("04-" + response.Monthlyreportlist[0].Month + "-" + response.Monthlyreportlist[0].Yr);
                C05.CellStyle = style1;

                var C06 = R0.CreateCell(6);
                C06.SetCellValue("05-" + response.Monthlyreportlist[0].Month + "-" + response.Monthlyreportlist[0].Yr);
                C06.CellStyle = style1;

                var C07 = R0.CreateCell(7);
                C07.SetCellValue("06-" + response.Monthlyreportlist[0].Month + "-" + response.Monthlyreportlist[0].Yr);
                C07.CellStyle = style1;

                var C08 = R0.CreateCell(8);
                C08.SetCellValue("07-" + response.Monthlyreportlist[0].Month + "-" + response.Monthlyreportlist[0].Yr);
                C08.CellStyle = style1;

                var C09 = R0.CreateCell(9);
                C09.SetCellValue("08-" + response.Monthlyreportlist[0].Month + "-" + response.Monthlyreportlist[0].Yr);
                C09.CellStyle = style1;

                var C10 = R0.CreateCell(10);
                C10.SetCellValue("09-" + response.Monthlyreportlist[0].Month + "-" + response.Monthlyreportlist[0].Yr);
                C10.CellStyle = style1;

                var C11 = R0.CreateCell(11);
                C11.SetCellValue("10-" + response.Monthlyreportlist[0].Month + "-" + response.Monthlyreportlist[0].Yr);
                C11.CellStyle = style1;

                var C12 = R0.CreateCell(12);
                C12.SetCellValue("11-" + response.Monthlyreportlist[0].Month + "-" + response.Monthlyreportlist[0].Yr);
                C12.CellStyle = style1;

                var C13 = R0.CreateCell(13);
                C13.SetCellValue("12-" + response.Monthlyreportlist[0].Month + "-" + response.Monthlyreportlist[0].Yr);
                C13.CellStyle = style1;

                var C14 = R0.CreateCell(14);
                C14.SetCellValue("13-" + response.Monthlyreportlist[0].Month + "-" + response.Monthlyreportlist[0].Yr);
                C14.CellStyle = style1;

                var C15 = R0.CreateCell(15);
                C15.SetCellValue("14-" + response.Monthlyreportlist[0].Month + "-" + response.Monthlyreportlist[0].Yr);
                C15.CellStyle = style1;

                var C16 = R0.CreateCell(16);
                C16.SetCellValue("15-" + response.Monthlyreportlist[0].Month + "-" + response.Monthlyreportlist[0].Yr);
                C16.CellStyle = style1;

                var C17 = R0.CreateCell(17);
                C17.SetCellValue("16-" + response.Monthlyreportlist[0].Month + "-" + response.Monthlyreportlist[0].Yr);
                C17.CellStyle = style1;
                //

                var C18 = R0.CreateCell(18);
                C18.SetCellValue("17-" + response.Monthlyreportlist[0].Month + "-" + response.Monthlyreportlist[0].Yr);
                C18.CellStyle = style1;

                var C19 = R0.CreateCell(19);
                C19.SetCellValue("18-" + response.Monthlyreportlist[0].Month + "-" + response.Monthlyreportlist[0].Yr);
                C19.CellStyle = style1;

                var C20 = R0.CreateCell(20);
                C20.SetCellValue("19-" + response.Monthlyreportlist[0].Month + "-" + response.Monthlyreportlist[0].Yr);
                C20.CellStyle = style1;

                var C21 = R0.CreateCell(21);
                C21.SetCellValue("20-" + response.Monthlyreportlist[0].Month + "-" + response.Monthlyreportlist[0].Yr);
                C21.CellStyle = style1;

                var C22 = R0.CreateCell(22);
                C22.SetCellValue("21-" + response.Monthlyreportlist[0].Month + "-" + response.Monthlyreportlist[0].Yr);
                C22.CellStyle = style1;

                var C23 = R0.CreateCell(23);
                C23.SetCellValue("22-" + response.Monthlyreportlist[0].Month + "-" + response.Monthlyreportlist[0].Yr);
                C23.CellStyle = style1;

                var C24 = R0.CreateCell(24);
                C24.SetCellValue("23-" + response.Monthlyreportlist[0].Month + "-" + response.Monthlyreportlist[0].Yr);
                C24.CellStyle = style1;

                var C25 = R0.CreateCell(25);
                C25.SetCellValue("24-" + response.Monthlyreportlist[0].Month + "-" + response.Monthlyreportlist[0].Yr);
                C25.CellStyle = style1;

                var C26 = R0.CreateCell(26);
                C26.SetCellValue("25-" + response.Monthlyreportlist[0].Month + "-" + response.Monthlyreportlist[0].Yr);
                C26.CellStyle = style1;

                var C27 = R0.CreateCell(27);
                C27.SetCellValue("26-" + response.Monthlyreportlist[0].Month + "-" + response.Monthlyreportlist[0].Yr);
                C27.CellStyle = style1;

                var C28 = R0.CreateCell(28);
                C28.SetCellValue("27-" + response.Monthlyreportlist[0].Month + "-" + response.Monthlyreportlist[0].Yr);
                C28.CellStyle = style1;

                var C29 = R0.CreateCell(29);
                C29.SetCellValue("28-" + response.Monthlyreportlist[0].Month + "-" + response.Monthlyreportlist[0].Yr);
                C29.CellStyle = style1;

                var C30 = R0.CreateCell(30);
                C30.SetCellValue("29-" + response.Monthlyreportlist[0].Month + "-" + response.Monthlyreportlist[0].Yr);
                C30.CellStyle = style1;

                if (response.Monthlyreportlist[0].Month != "February")
                {
                    var C31 = R0.CreateCell(31);
                    C31.SetCellValue("30-" + response.Monthlyreportlist[0].Month + "-" + response.Monthlyreportlist[0].Yr);
                    C31.CellStyle = style1;

                    if (response.Monthlyreportlist[0].Month == "January" || response.Monthlyreportlist[0].Month == "March" || response.Monthlyreportlist[0].Month == "May" || response.Monthlyreportlist[0].Month == "July" || response.Monthlyreportlist[0].Month == "August" || response.Monthlyreportlist[0].Month == "October" || response.Monthlyreportlist[0].Month == "December")
                    {

                        var C32 = R0.CreateCell(32);
                        C32.SetCellValue("31-" + response.Monthlyreportlist[0].Month + "-" + response.Monthlyreportlist[0].Yr);
                        C32.CellStyle = style1;
                    }
                }
                var C33 = R0.CreateCell(33);
                C33.SetCellValue("TOTAL");
                C33.CellStyle = style1;


                int i = 1;
                foreach (var item in response.Monthlyreportlist)
                {
                    IRow row = sheet1.CreateRow(i);

                    var C0 = row.CreateCell(0);
                    C0.SetCellValue(i.ToString());

                    var C1 = row.CreateCell(1);
                    C1.SetCellValue(item.Particulars);

                    var c2 = row.CreateCell(2);
                    c2.SetCellValue(item.F2);

                    var c3 = row.CreateCell(3);
                    c3.SetCellValue(item.F3);

                    var c4 = row.CreateCell(4);
                    c4.SetCellValue(item.F4);

                    var c5 = row.CreateCell(5);
                    c5.SetCellValue(item.F5);

                    var c6 = row.CreateCell(6);
                    c6.SetCellValue(item.F6);

                    var c7 = row.CreateCell(7);
                    c7.SetCellValue(item.F7);

                    var c8 = row.CreateCell(8);
                    c8.SetCellValue(item.F8);


                    var c9 = row.CreateCell(9);
                    c9.SetCellValue(item.F9);

                    var c10 = row.CreateCell(10);
                    c10.SetCellValue(item.F10);

                    var c11 = row.CreateCell(11);
                    c11.SetCellValue(item.F11);

                    var c12 = row.CreateCell(12);
                    c12.SetCellValue(item.F12);

                    var c13 = row.CreateCell(13);
                    c13.SetCellValue(item.F13);

                    var c14 = row.CreateCell(14);
                    c14.SetCellValue(item.F14);

                    var c15 = row.CreateCell(15);
                    c15.SetCellValue(item.F15);

                    var c16 = row.CreateCell(16);
                    c16.SetCellValue(item.F16);


                    var c17 = row.CreateCell(17);
                    c17.SetCellValue(item.F17);


                    var c18 = row.CreateCell(18);
                    c18.SetCellValue(item.F18);

                    var c19 = row.CreateCell(19);
                    c19.SetCellValue(item.F19);

                    var c20 = row.CreateCell(20);
                    c20.SetCellValue(item.F20);

                    var c21 = row.CreateCell(21);
                    c21.SetCellValue(item.F21);

                    var c22 = row.CreateCell(22);
                    c22.SetCellValue(item.F22);

                    var c23 = row.CreateCell(23);
                    c23.SetCellValue(item.F23);

                    var c24 = row.CreateCell(24);
                    c24.SetCellValue(item.F24);

                    var c25 = row.CreateCell(25);
                    c25.SetCellValue(item.F25);


                    var c26 = row.CreateCell(26);
                    c26.SetCellValue(item.F26);

                    var c27 = row.CreateCell(27);
                    c27.SetCellValue(item.F27);

                    var c28 = row.CreateCell(28);
                    c28.SetCellValue(item.F28);

                    var c29 = row.CreateCell(29);
                    c29.SetCellValue(item.F29);

                    var c30 = row.CreateCell(30);
                    c30.SetCellValue(item.F30);

                    if (response.Monthlyreportlist[0].Month != "February")
                    {
                        var c31 = row.CreateCell(31); //30th
                        c31.SetCellValue(item.F31);

                        if (response.Monthlyreportlist[0].Month == "January" || response.Monthlyreportlist[0].Month == "March" || response.Monthlyreportlist[0].Month == "May" || response.Monthlyreportlist[0].Month == "July" || response.Monthlyreportlist[0].Month == "August" || response.Monthlyreportlist[0].Month == "October" || response.Monthlyreportlist[0].Month == "December")
                        {
                            var c32 = row.CreateCell(32); //31th
                            c32.SetCellValue(item.F32);
                        }
                    }
                    var c33 = row.CreateCell(33);
                    c33.SetCellValue(item.TOTAL);

                    //var c35 = row.CreateCell(35);
                    //c35.SetCellValue(item.F34);

                    i++;
                }

            }
            catch (Exception ex)
            {

            }
        }


        private async Task GenerateDataInfo(TransactionLogsResponse2 response)
        {

            try
            {
                ICellStyle style1 = _hssfWorkbook.CreateCellStyle();
                style1.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
                style1.FillPattern = FillPattern.SolidForeground;

                //var response = await _userRepository.GenerateLogReport(request);

                ISheet sheet1 = _hssfWorkbook.CreateSheet("UserInfo");
                sheet1.SetColumnWidth(0, 1500);
                sheet1.SetColumnWidth(1, 8000);
                sheet1.SetColumnWidth(2, 8000);
                sheet1.SetColumnWidth(3, 8000);
                sheet1.SetColumnWidth(4, 8000);
                sheet1.SetColumnWidth(5, 8000);
                sheet1.SetColumnWidth(6, 8000);
                sheet1.SetColumnWidth(7, 8000);
                sheet1.SetColumnWidth(8, 8000);
                sheet1.SetColumnWidth(9, 8000);
                sheet1.SetColumnWidth(10, 8000);
                sheet1.SetColumnWidth(11, 8000);
                sheet1.SetColumnWidth(12, 8000);
                sheet1.SetColumnWidth(13, 8000);
                sheet1.SetColumnWidth(14, 8000);
                sheet1.SetColumnWidth(15, 8000);
                sheet1.SetColumnWidth(16, 8000);
                sheet1.SetColumnWidth(17, 8000);
                //----------Create Header-----------------
                var R0 = sheet1.CreateRow(0);

                var C00 = R0.CreateCell(0);
                C00.SetCellValue("WalletUserId");
                C00.CellStyle = style1;

                var C01 = R0.CreateCell(1);
                C01.SetCellValue("FirstName");
                C01.CellStyle = style1;

                var C02 = R0.CreateCell(2);
                C02.SetCellValue("LastName");
                C02.CellStyle = style1;

                var C03 = R0.CreateCell(3);
                C03.SetCellValue("EmailId");
                C03.CellStyle = style1;

                var C04 = R0.CreateCell(4);
                C04.SetCellValue("StdCode");
                C04.CellStyle = style1;

                var C05 = R0.CreateCell(5);
                C05.SetCellValue("MobileNo");
                C05.CellStyle = style1;

                var C06 = R0.CreateCell(6);
                C06.SetCellValue("IsActive");
                C06.CellStyle = style1;

                var C07 = R0.CreateCell(7);
                C07.SetCellValue("CreatedDate");
                C07.CellStyle = style1;

                var C08 = R0.CreateCell(8);
                C08.SetCellValue("UserType");
                C08.CellStyle = style1;

                var C09 = R0.CreateCell(9);
                C09.SetCellValue("Country");
                C09.CellStyle = style1;

                var C10 = R0.CreateCell(10);
                C10.SetCellValue("Currentbalance");
                C10.CellStyle = style1;

                var C11 = R0.CreateCell(11);
                C11.SetCellValue("DeviceType");
                C11.CellStyle = style1;

                var C12 = R0.CreateCell(12);
                C12.SetCellValue("IsDeleted");
                C12.CellStyle = style1;

                var C13 = R0.CreateCell(13);
                C13.SetCellValue("DocumetStatus");
                C13.CellStyle = style1;

                var C14 = R0.CreateCell(14);
                C14.SetCellValue("IsEmailVerified");
                C14.CellStyle = style1;

                var C15 = R0.CreateCell(15);
                C15.SetCellValue("IsOtpVerified");
                C15.CellStyle = style1;

                int i = 1;
                foreach (var item in response.TransactionLogslist2)
                {
                    IRow row = sheet1.CreateRow(i);
                                        


        var C0 = row.CreateCell(0);
                    C0.SetCellValue(item.WalletUserId);

                    var C1 = row.CreateCell(1);
                    C1.SetCellValue(item.FirstName);

                    var C2 = row.CreateCell(2);
                    C2.SetCellValue(item.LastName);

                    var c3 = row.CreateCell(3);
                    c3.SetCellValue(item.EmailId);

                    var c4 = row.CreateCell(4);
                    c4.SetCellValue(item.StdCode);

                    var c5 = row.CreateCell(5);
                    c5.SetCellValue(item.MobileNo.ToString());

                    var c6 = row.CreateCell(6);
                    c6.SetCellValue(item.IsActive.ToString());

                    var c7 = row.CreateCell(7);
                    c7.SetCellValue(item.CreatedDate.ToString());

                    var c8 = row.CreateCell(8);
                    c8.SetCellValue(item.UserType.ToString());

                    var c9 = row.CreateCell(9);
                    c9.SetCellValue(item.Country);


                    var c10 = row.CreateCell(10);
                    c10.SetCellValue(item.Currentbalance.ToString());

                    var c11 = row.CreateCell(11);
                    c11.SetCellValue(item.DeviceType.ToString());

                    var c12 = row.CreateCell(12);
                    c12.SetCellValue(item.IsDeleted.ToString());

                    var c13 = row.CreateCell(13);
                    c13.SetCellValue(item.DocumetStatus.ToString());

                    var c14 = row.CreateCell(14);
                    c14.SetCellValue(item.IsEmailVerified.ToString());

                    var c15 = row.CreateCell(15);
                    c15.SetCellValue(item.IsOtpVerified.ToString());


                    i++;
                }

            }
            catch (Exception ex)
            {

            }
        }
        void InitializeWorkbook()
        {
            _hssfWorkbook = new HSSFWorkbook();

            ////create a entry of DocumentSummaryInformation
            DocumentSummaryInformation dsi = PropertySetFactory.CreateDocumentSummaryInformation();
            dsi.Company = "NPOI Team";
            _hssfWorkbook.DocumentSummaryInformation = dsi;

            ////create a entry of SummaryInformation
            SummaryInformation si = PropertySetFactory.CreateSummaryInformation();
            si.Subject = "NPOI SDK Example";
            _hssfWorkbook.SummaryInformation = si;
        }
    }
}
