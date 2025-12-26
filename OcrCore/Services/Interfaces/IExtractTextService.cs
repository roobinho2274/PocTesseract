using OcrCore.Services.Implmentation;

namespace OcrCore.Services.Interfaces;

public interface IExtractTextService
{
    string ExtractTextSmart(string pdfPath, OcrOptions options);
}
