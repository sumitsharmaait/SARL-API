//using Amazon.DynamoDBv2.DocumentModel;

using iText.Layout.Element;
using iText.StyledXmlParser.Jsoup.Nodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static System.Net.Mime.MediaTypeNames;

namespace Ezipay.Utility.PDFGenerate
{
    public class PdfGenerater:IPdfGenerater
    {
        //public MemoryStream GeneratePDF(string bodyContent)
        //{
        //    var Chart1 = new System.Web.UI.DataVisualization.Charting.Chart();
        //    MemoryStream workStream = new MemoryStream();
        //    Document document = new Document();
        //    iTextSharp.text.html.simpleparser.HTMLWorker htmlparser = new iTextSharp.text.html.simpleparser.HTMLWorker(document);
        //    PdfWriter.GetInstance(document, workStream).CloseStream = false;
        //    document.Open();
        //    string filePath = HttpContext.Current.Server.MapPath("~/WebImages/");

        //    //------------For Logo----------------
        //    Image logo = Image.GetInstance(filePath + "logo-txt.png");
        //    logo.ScaleToFit(80f, 80f);
        //    //Give space before image
        //    logo.SpacingBefore = 1f;
        //    //Give some space after the image
        //    logo.SpacingAfter = 1f;
        //    logo.Alignment = Element.ALIGN_CENTER;
        //    document.Add(logo);

        //    Paragraph p = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 1)));
        //    document.Add(p);

        //    //------Render Table Data--------------
        //    StringReader sr = new StringReader(bodyContent);
        //    htmlparser.Parse(sr);

        //    document.Close();
        //    byte[] byteInfo = workStream.ToArray();
        //    workStream.Write(byteInfo, 0, byteInfo.Length);
        //    workStream.Position = 0;
        //    return workStream;
        //}
    }
}
