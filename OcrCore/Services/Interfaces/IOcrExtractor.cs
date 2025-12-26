using OcrCore.Services.Implmentation;

namespace OcrCore.Services.Interfaces;

public interface IOcrExtractor
{
    string ExtractCleanText(string pdfPath, OcrOptions? options = null);
}
