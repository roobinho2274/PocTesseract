using OcrCore.Services.Interfaces;
using OcrCore.Util;

namespace OcrCore.Services.Implmentation;

public class OcrExtractor : IOcrExtractor
{
    private readonly IExtractTextService _extract;

    public OcrExtractor(IExtractTextService extract)
    {
        _extract = extract;
    }

    public string ExtractCleanText(string pdfPath, OcrOptions? options)
    {
        options ??= new OcrOptions();

        var raw = _extract.ExtractTextSmart(pdfPath, options);
        return CleanTextUtil.CleanText(raw);
    }
}
