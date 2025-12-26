using OcrCore.Services.Interfaces;

namespace OcrCore.Services.Implmentation;
using System.Runtime.InteropServices;

using PDFiumSharp;

using Tesseract;

using PdfiumDocument = PDFiumSharp.PdfDocument;

public class OcrPageService : IOcrPageService
{
    public string OcrPage(PdfiumDocument doc, int pageIndex, OcrOptions options)
    {
        using var page = doc.Pages[pageIndex];

        double inchW = page.Width / 72.0;
        double inchH = page.Height / 72.0;
        int pxW = Math.Max(1, (int)Math.Round(inchW * options.Dpi));
        int pxH = Math.Max(1, (int)Math.Round(inchH * options.Dpi));

        using var fb = new PDFiumBitmap(pxW, pxH, hasAlpha: true);
        page.Render(fb, 0, RenderingFlags.LcdText | RenderingFlags.DontCatch);

        using var engine = new TesseractEngine(options.TessdataPath, options.Langs, EngineMode.Default);

        using var pix = Pix.Create(fb.Width, fb.Height, 32);
        var pixData = pix.GetData();

        for (int y = 0; y < fb.Height; y++)
        {
            IntPtr srcLine = fb.Scan0 + (y * fb.Stride);
            IntPtr dstLine = pixData.Data + (y * fb.Stride);

            byte[] lineBuffer = new byte[fb.Stride];
            Marshal.Copy(srcLine, lineBuffer, 0, fb.Stride);
            Marshal.Copy(lineBuffer, 0, dstLine, fb.Stride);
        }

        using var result = engine.Process(pix, PageSegMode.Auto);
        return result.GetText();
    }
}
