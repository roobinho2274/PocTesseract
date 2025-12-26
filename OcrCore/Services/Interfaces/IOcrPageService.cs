using OcrCore.Services.Implmentation;

using PdfiumDocument = PDFiumSharp.PdfDocument;


namespace OcrCore.Services.Interfaces;

public interface IOcrPageService
{
    string OcrPage(PdfiumDocument doc, int pageIndex, OcrOptions options);

}