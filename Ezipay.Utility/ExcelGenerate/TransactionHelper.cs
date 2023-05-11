//using Amazon.DynamoDBv2.DocumentModel;
using Ezipay.Utility.Extention;
using Ezipay.ViewModel.AdminViewModel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;

namespace Ezipay.Utility.ExcelGenerate
{
    public class TransactionHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>      
        public MemoryStream GenerateStreamFromString(List<ReportData> list)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("<table  border='1' bgcolor='#ffffff' bordercolor='#000000' cellspacing='0' cellpadding='0' style='font-size:10.0pt; font-family:Calibri; background:white;'>");
            stringBuilder.Append("<tbody>");

            stringBuilder.Append("<tr>");
            stringBuilder.Append("<th>Service Name</th>");
            stringBuilder.Append("<th>Bank Transaction Id</th>");
            stringBuilder.Append("<th>TransactionType</th>");
            stringBuilder.Append("<th>Mobile Number</th>");
            stringBuilder.Append("<th>Comment (if available)</th>");
            stringBuilder.Append("<th>WalletTransactionId</th>");
            stringBuilder.Append("<th>CreatedDate</th>");
            stringBuilder.Append("<th>Amount</th>");
            stringBuilder.Append("</tr>");
            stringBuilder.Append(System.Environment.NewLine);
            foreach (var item in list)
            {
                stringBuilder.Append("<tr>");
                stringBuilder.Append("<td>" + ColumnValue(item.ServiceName) + "</td>");
                stringBuilder.Append("<td>" + ColumnValue(item.BankTransactionId) + "</td>");
                stringBuilder.Append("<td>" + ColumnValue(item.TransactionType) + "</td>");
                stringBuilder.Append("<td>" + ColumnValue(item.ToMobileNo) + "</td>");
                stringBuilder.Append("<td>" + ColumnValue(item.Comments) + "</td>");
                stringBuilder.Append("<td>" + item.WalletTransactionId + "</td>");
                stringBuilder.Append("<td>" + item.CreatedDate.ToString("dd-MMM-yyyy") + "</td>");
                stringBuilder.Append("<td>" + ColumnValue(item.TransactionAmount) + "</td>");
                stringBuilder.Append("</tr>");
                stringBuilder.Append(System.Environment.NewLine);

            }
            stringBuilder.Append("</tbody>");
            stringBuilder.Append("</table>");
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(stringBuilder.ToString());
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        string ColumnValue(string Value)
        {
            if (string.IsNullOrEmpty(Value) || (!string.IsNullOrEmpty(Value) && Value == "0"))
            {
                Value = "-";
            }
            return Value;
        }

        public string TD(string Value, bool IsEmptyValue = false)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("<td style='border:none'>");
            if (!IsEmptyValue)
            {

                if (string.IsNullOrEmpty(Value) || (!string.IsNullOrEmpty(Value) && Value == "0"))
                {
                    Value = "-";
                }
            }
            else
            {

                Value = "";
            }
            stringBuilder.Append(Value);
            stringBuilder.Append("</td>");
            return stringBuilder.ToString();
        }

        public string BlankRow()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("<tr >");
            stringBuilder.Append("<th></th>");
            stringBuilder.Append("<th></th>");
            stringBuilder.Append("<th></th>");
            stringBuilder.Append("<th></th>");
            stringBuilder.Append("<th></th>");
            stringBuilder.Append("<th></th>");
            stringBuilder.Append("<th></th>");
            stringBuilder.Append("<th></th>");
            stringBuilder.Append("</tr>");
            return stringBuilder.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public MemoryStream WritePdfForTransactionList(List<ReportData> model, string UserName)
        {

            try
            {
                MemoryStream workStream = new MemoryStream();

                using (var document = new iTextSharp.text.Document(PageSize.A4))
                {

                    iTextSharp.text.html.simpleparser.HTMLWorker htmlparser = new iTextSharp.text.html.simpleparser.HTMLWorker(document);
                    iTextSharp.text.pdf.PdfWriter.GetInstance(document, workStream).CloseStream = false; document.Open();


                    //iTextSharp.text.Font mainFont = new iTextSharp.text.Font(iTextSharp.text.Font.NORMAL, 10);
                    int mainFont = 10;
                    #region Table
                    iTextSharp.text.pdf.PdfPTable table = new iTextSharp.text.pdf.PdfPTable(8);
                    table.TotalWidth = PageSize.A4.Width;
                    table.PaddingTop = 5f;



                    table.DefaultCell.Border = 1;
                    //table.TotalWidth = 600f;
                    table.DefaultCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;

                    table.DefaultCell.Padding = 4;
                    float[] widths = new float[] { 350f, 350f, 350f, 350f, 350f, 350f, 350f, 350f };
                    // table.SetWidths(widths);


                    iTextSharp.text.pdf.PdfPTable innerTable = new iTextSharp.text.pdf.PdfPTable(3);

                    iTextSharp.text.Image myImage = iTextSharp.text.Image.GetInstance(ConfigurationManager.AppSettings["LogoLocation"]);
                    PdfPCell logoCell = new PdfPCell(myImage);
                    logoCell.Colspan = 3;
                    logoCell.Border = 0;
                    logoCell.Padding = 10;

                    innerTable.DefaultCell.BackgroundColor = BaseColor.WHITE;
                    var head = Cell("", mainFont, IsBorder: false, IsBlank: true);
                    head.Colspan = 3;
                    innerTable.AddCell(logoCell);
                    innerTable.AddCell(head);
                    innerTable.AddCell(Cell("", mainFont, IsBorder: false, FontType: 1, IsBlank: true));
                    innerTable.AddCell(Cell("Name", mainFont, IsBorder: false));
                    innerTable.AddCell(Cell(UserName, mainFont, IsBorder: false));

                    innerTable.AddCell(Cell("", mainFont, IsBorder: false, IsBlank: true));
                    innerTable.AddCell(Cell("Date", mainFont, IsBorder: false));
                    innerTable.AddCell(Cell(DateTime.UtcNow.ToString("dd-MMM-yyyy"), mainFont, IsBorder: false));
                    var blankCell = Cell("", mainFont, IsBorder: false, IsBlank: true);
                    blankCell.Colspan = 3;
                    innerTable.AddCell(blankCell);
                    var pCell = new PdfPCell(innerTable);
                    pCell.Colspan = 8;
                    table.AddCell(pCell);

                    mainFont = 10;
                    AddHeaderCell(table, "Service Name", 1);
                    AddHeaderCell(table, "Bank Transaction Id", 1);
                    AddHeaderCell(table, "TransactionType", 1);
                    AddHeaderCell(table, "Mobile Number", 1);
                    AddHeaderCell(table, "Comment (Optional)", 1);
                    AddHeaderCell(table, "WalletTransactionId", 1);
                    AddHeaderCell(table, "CreatedDate", 1);
                    AddHeaderCell(table, "Amount", 1);



                    if (model != null && model.Count > 0)
                    {

                        foreach (var item in model)
                        {
                            table.AddCell(Cell(item.ServiceName, mainFont));
                            table.AddCell(Cell(item.BankTransactionId, mainFont));
                            table.AddCell(Cell(item.TransactionType, mainFont));
                            table.AddCell(Cell(item.ToMobileNo, mainFont));
                            table.AddCell(Cell(item.Comments, mainFont));
                            table.AddCell(Cell(item.WalletTransactionId.ToString(), mainFont));
                            table.AddCell(Cell(item.CreatedDate.ToString("dd-MMM-yyyy"), mainFont));
                            table.AddCell(Cell(item.TransactionAmount, mainFont));


                        }
                    }


                    document.Add(table);
                    #endregion
                    document.Close();
                    byte[] byteInfo = workStream.ToArray();
                    workStream.Write(byteInfo, 0, byteInfo.Length);
                    workStream.Position = 0;
                    //res.Create(workStream);
                    //return res;
                    return workStream;
                }

            }
            catch (Exception ex)
            {
                ex.InnerException.ToString().ErrorLog("Exception GenerationReportController.cs", "WritePdfForManageCountry");
                throw;
            }

        }


        public MemoryStream WritePdfForTransactionListPerUser(List<UserTxnReportData> model, long WalletUserId)
        {

            try
            {
                MemoryStream workStream = new MemoryStream();

                using (var document = new Document(PageSize.A4, 5f, 5f, 20f, 5f))
                {

                    //iTextSharp.text.html.simpleparser.HTMLWorker htmlparser = new iTextSharp.text.html.simpleparser.HTMLWorker(document);
                    PdfWriter.GetInstance(document, workStream).CloseStream = false;
                    document.Open();


                    //iTextSharp.text.Font mainFont = new iTextSharp.text.Font(iTextSharp.text.Font.NORMAL, 10);			
                    int mainFont = 10;
                    #region Table			
                    PdfPTable table = new PdfPTable(10);

                    table.WidthPercentage = 100;
                    table.DefaultCell.Border = 1;

                    table.DefaultCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;


                    PdfPTable innerTable = new PdfPTable(2);

                    iTextSharp.text.Image myImage = iTextSharp.text.Image.GetInstance(ConfigurationManager.AppSettings["LogoLocation"]);
                    PdfPCell logoCell = new PdfPCell(myImage);
                    logoCell.Colspan = 12;
                    logoCell.Border = 0;

                    innerTable.DefaultCell.BackgroundColor = BaseColor.WHITE;
                    var head = Cell("", mainFont, IsBorder: true, IsBlank: true);
                    head.Colspan = 12;
                    innerTable.AddCell(logoCell);

                    innerTable.AddCell(head);
                    if (model != null && model.Count > 0)
                    {
                        int n = 0;
                        foreach (var item in model)
                        {                            
                            if (item.Walletuserid.Trim() == WalletUserId.ToString())
                            {
                                n = n + 1;
                                if (n == 1)
                                {
                                    innerTable.AddCell(Cell("User Wallet ID :	", mainFont, IsBorder: false));
                                    innerTable.AddCell(Cell(item.Walletuserid, mainFont, IsBorder: false));

                                    innerTable.AddCell(Cell("User Full Name :	", mainFont, IsBorder: false));
                                    innerTable.AddCell(Cell(item.FullName, mainFont, IsBorder: false));

                                    innerTable.AddCell(Cell("Email ID: ", mainFont, IsBorder: false));
                                    innerTable.AddCell(Cell(item.Emailid, mainFont, IsBorder: false));

                                    innerTable.AddCell(Cell("Mobile Number: ", mainFont, IsBorder: false));
                                    innerTable.AddCell(Cell(item.IsdMobileno, mainFont, IsBorder: false));


                                    innerTable.AddCell(Cell("User Country: ", mainFont, IsBorder: false));
                                    innerTable.AddCell(Cell(item.UserCountry, mainFont, IsBorder: false));


                                    innerTable.AddCell(Cell("User Current Balance: ", mainFont, IsBorder: false));
                                    innerTable.AddCell(Cell(item.Currentbalance, mainFont, IsBorder: false));


                                    innerTable.AddCell(Cell("From Date :	", mainFont, IsBorder: false));
                                    innerTable.AddCell(Cell(item.FromDate.ToString("dd-MMM-yyyy"), mainFont, IsBorder: false));


                                    innerTable.AddCell(Cell("To Date :	", mainFont, IsBorder: false));
                                    innerTable.AddCell(Cell(item.ToDate.ToString("dd-MMM-yyyy"), mainFont, IsBorder: false));


                                    innerTable.AddCell(Cell("Generated Date :", mainFont, IsBorder: false));
                                    innerTable.AddCell(Cell(DateTime.UtcNow.ToString("dd-MMM-yyyy"), mainFont, IsBorder: false));

                                }
                            }
                        }
                    }

                        var blankCell = Cell("", mainFont, IsBorder: true, IsBlank: true);
                        blankCell.Colspan = 12;
                        innerTable.AddCell(blankCell);
                        var pCell = new PdfPCell(innerTable);
                        pCell.Colspan = 10;
                        table.AddCell(pCell);
                        float[] widths = new float[] { 40f, 70f, 40f, 30f, 30f, 35f, 45f, 35f, 45f, 35f };
                        table.SetWidths(widths);
                        mainFont = 10;

                        AddHeaderCell(table, "Transaction Date", 1);
                        AddHeaderCell(table, "Narration", 1);
                        AddHeaderCell(table, "Account No.", 1);
                        AddHeaderCell(table, "Transaction Type", 1);
                        AddHeaderCell(table, "Transaction Status", 1);
                        AddHeaderCell(table, "Before Txn Balance", 1);
                        AddHeaderCell(table, "Requested Amount", 1);
                        AddHeaderCell(table, "Commision Amount", 1);
                        AddHeaderCell(table, "Total Amount", 1);
                        AddHeaderCell(table, "After Txn Balance", 1);
                        int i = 0;
                        if (model != null && model.Count > 0)
                        {
                            foreach (var item in model)
                            {
                                i = i + 1;


                                table.AddCell(Cell(item.TransactionDate.ToString("dd-MMM-yyyy"), mainFont));
                                table.AddCell(Cell(item.Mainservice + "-" + item.SubServiceName + "-" + item.SubServiceCategoryName, mainFont));
                                table.AddCell(Cell(item.AccountNo, mainFont));
                                table.AddCell(Cell(item.TransactionType, mainFont));
                                table.AddCell(Cell(item.TransactionStatus, mainFont));

                                table.AddCell(Cell(item.BeforeTxnBalance, mainFont));
                                table.AddCell(Cell(item.Requestedamount, mainFont));
                                table.AddCell(Cell(item.CommisionAmount, mainFont));
                                table.AddCell(Cell(item.TotalAmount, mainFont));
                                table.AddCell(Cell(item.AfterTxnBalance, mainFont));

                            }
                        }
                        document.Add(table);
                        #endregion
                        document.Close();

                        byte[] byteInfo = workStream.ToArray();
                        workStream.Write(byteInfo, 0, byteInfo.Length);
                        workStream.Position = 0;
                        //res.Create(workStream);			
                        //return res;			
                        return workStream;
                    }

                }
            catch (Exception ex)
            {
                ex.InnerException.ToString().ErrorLog("Exception GenerationReportController.cs", "WritePdfForManageCountry");
                throw;
            }

        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="mainFont"></param>
        /// <param name="isArabic"></param>
        /// <returns></returns>
        public PdfPCell Cell(string text, int FontSize, bool isArabic = false, int rowspan = 1, bool IsBorder = true, int FontType = 0, bool IsBlank = false)
        {
            BaseFont bfTimes = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, false);
            iTextSharp.text.Font times = new iTextSharp.text.Font(bfTimes, FontSize, FontType, iTextSharp.text.BaseColor.BLACK);


            if (!IsBlank && string.IsNullOrEmpty(text) || (!string.IsNullOrEmpty(text) && text == "0"))
            {
                text = "-";
            }
            PdfPCell cell = new PdfPCell(new Phrase(text, times));

            cell.Rowspan = rowspan;
            if (!IsBorder)
            {
                cell.Border = 0;
                cell.BorderColor = BaseColor.WHITE;


            }
            cell.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
            cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
            cell.Padding = 4;
            return cell;
        }
        private static void AddHeaderCell(PdfPTable table, string text, int rowspan)
        {
            BaseFont bfTimes = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, false);
            iTextSharp.text.Font times = new iTextSharp.text.Font(bfTimes, 10, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.WHITE);

            PdfPCell cell = new PdfPCell(new Phrase(text, times));
            cell.BackgroundColor = BaseColor.BLUE;

            cell.Rowspan = rowspan;
            cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
            cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
            table.AddCell(cell);
        }





    }
}
