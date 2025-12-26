using System.Text;

using Microsoft.AspNetCore.Mvc;

using OcrCore.Services.Implmentation;
using OcrCore.Services.Interfaces;

using OcrServer.Contracts;

namespace OcrServer.Controllers;

/// <summary>
/// Envia um PDF e retorna o texto extraído (OCR + texto nativo).
/// </summary>
/// <param name="file">Arquivo PDF</param>
/// <param name="langs">Idiomas do Tesseract (ex: por+eng)</param>
/// <param name="dpi">DPI do OCR (ex: 300)</param>
/// <returns>Arquivo TXT</returns>
[ApiController]
[Route("api/[controller]")]
public class OcrController : ControllerBase
{
    private readonly IOcrExtractor _ocr;

    public OcrController(IOcrExtractor ocr)
    {
        _ocr = ocr;
    }

    [HttpPost("pdf")]
    [RequestSizeLimit(200_000_000)]
    public async Task<IActionResult> Pdf(
        [FromForm] OcrPdfRequest ocrPdfRequest )
    {
        if (ocrPdfRequest.File is null || ocrPdfRequest.File.Length == 0)
            return BadRequest("Arquivo não enviado.");

        if (!ocrPdfRequest.File.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Envie um PDF.");

        var opts = new OcrOptions();
        if (!string.IsNullOrWhiteSpace(ocrPdfRequest.Langs))
            opts.Langs = ocrPdfRequest.Langs!;
        if (ocrPdfRequest.Dpi.HasValue && ocrPdfRequest.Dpi.Value > 0)
            opts.Dpi = ocrPdfRequest.Dpi.Value;

        // arquivo temporário
        var tempPdf = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.pdf");
        await using (var fs = System.IO.File.Create(tempPdf))
            await ocrPdfRequest.File.CopyToAsync(fs);

        try
        {
            var text = _ocr.ExtractCleanText(tempPdf, opts);

            var bytes = Encoding.UTF8.GetBytes(text);
            var outName = Path.GetFileNameWithoutExtension(ocrPdfRequest.File.FileName) + ".txt";

            return File(bytes, "text/plain; charset=utf-8", outName);
        }
        finally
        {
            System.IO.File.Delete(tempPdf);
        }
    }
}
