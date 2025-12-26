using System.Text;

using OcrCore.Services.Interfaces;

using PdfiumDocument = PDFiumSharp.PdfDocument;
using PigDocument = UglyToad.PdfPig.PdfDocument;

namespace OcrCore.Services.Implmentation;

public sealed class ExtractTextService : IExtractTextService
{
    private readonly IOcrPageService _ocrPage;

    public ExtractTextService(IOcrPageService ocrPage)
    {
        _ocrPage = ocrPage;
    }

    public string ExtractTextSmart(string pdfPath, OcrOptions options)
    {
        var sb = new StringBuilder();

        using var pig = PigDocument.Open(pdfPath);
        using var pdfium = new PdfiumDocument(pdfPath);

        for (int i = 0; i < pig.NumberOfPages; i++)
        {
            var page = pig.GetPage(i + 1);
            bool hasText = !string.IsNullOrWhiteSpace(page.Text);

            if (hasText && page.Text.Length > options.MinTextLengthToSkipOcr)
            {
                sb.AppendLine(page.Text);
                sb.AppendLine();
            }
            else
            {
                var ocr = _ocrPage.OcrPage(pdfium, i, options);
                sb.AppendLine(ocr);
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }
}
