using PDFiumSharp;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Tesseract;
using PdfiumDocument = PDFiumSharp.PdfDocument;
using PigDocument = UglyToad.PdfPig.PdfDocument;

class Program
{
    static int Main(string[] args)
    {
        var startTime = DateTime.UtcNow;
        string rootFolderPath = Path.Combine(AppContext.BaseDirectory, "PDFsParaProcessar");

        if (!Directory.Exists(rootFolderPath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"Erro: A pasta especificada não existe: {rootFolderPath}");
            Console.ResetColor();
            return 1;
        }

        Console.WriteLine($"Iniciando busca por arquivos .pdf em: {rootFolderPath}");

        var pdfFiles = Directory.GetFiles(rootFolderPath, "*.pdf", SearchOption.AllDirectories);

        if (pdfFiles.Length == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Nenhum arquivo .pdf encontrado na pasta ou subpastas.");
            Console.ResetColor();
            return 0;
        }

        Console.WriteLine($"Total de {pdfFiles.Length} arquivos .pdf encontrados. Iniciando processamento...");
        Console.WriteLine("------------------------------------------------------------");

        int successCount = 0;
        int failureCount = 0;

        foreach (var pdfPath in pdfFiles)
        {
            Console.WriteLine($"Processando: {pdfPath}");
            try
            {
                var txtPath = Path.ChangeExtension(pdfPath, ".txt");

                var rawText = ExtractTextSmart(pdfPath, ocrLangs: "por+eng+chi_sim+chi_sim_vert", ocrDpi: 300);

                var cleanedText = CleanText(rawText);

                File.WriteAllText(txtPath, cleanedText, new UTF8Encoding(false));

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"   -> SUCESSO! Salvo em: {txtPath}");
                Console.ResetColor();
                successCount++;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                var detailedError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Console.Error.WriteLine($"   -> FALHA DETALHADA: {detailedError}");
                Console.ResetColor();
                failureCount++;
            }
        }
        var endTime = DateTime.UtcNow;

        Console.WriteLine("------------------------------------------------------------");
        Console.WriteLine($"Processamento concluído em {endTime - startTime}");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Arquivos processados com sucesso: {successCount}");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Arquivos com falha: {failureCount}");
        Console.ResetColor();

        return failureCount > 0 ? 2 : 0;
    }
    static string ExtractTextSmart(string pdfPath, string ocrLangs = "por+eng+chi_sim+chi_sim_vert", int ocrDpi = 600)
    {
        var sb = new StringBuilder();

        using var pig = PigDocument.Open(pdfPath);
        using var pdfium = new PdfiumDocument(pdfPath);

        for (int i = 0; i < pig.NumberOfPages; i++)
        {
            var page = pig.GetPage(i + 1);
            bool hasText = !string.IsNullOrWhiteSpace(page.Text);

            if (hasText && page.Text.Length > 100)
            {
                sb.AppendLine(page.Text);
                sb.AppendLine();
            }
            else
            {
                var ocr = OcrPage(pdfium, i, ocrLangs, ocrDpi);
                sb.AppendLine(ocr);
                sb.AppendLine();
            }
        }
        return sb.ToString();
    }

    static string OcrPage(PdfiumDocument doc, int pageIndex, string langs, int dpi)
    {
        using var page = doc.Pages[pageIndex];

        double inchW = page.Width / 72.0;
        double inchH = page.Height / 72.0;
        int pxW = Math.Max(1, (int)Math.Round(inchW * dpi));
        int pxH = Math.Max(1, (int)Math.Round(inchH * dpi));

        using var fb = new PDFiumBitmap(pxW, pxH, hasAlpha: true);

        int renderFlags = 2 | 64;

        page.Render(fb, 0, PDFiumSharp.RenderingFlags.LcdText | PDFiumSharp.RenderingFlags.DontCatch);

        using var engine = new TesseractEngine(@"./tessdata", langs, EngineMode.Default);

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

    static string CleanText(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var t = input;

        t = t.Normalize(NormalizationForm.FormKC);
        t = t.Replace("ﬁ", "fi").Replace("ﬂ", "fl");
        t = Regex.Replace(t, @"[^\P{C}\t\r\n]", "");
        t = Regex.Replace(t, @"(\p{L})-\r?\n(\p{L})", "$1$2");
        t = t.Replace("\r\n", "\n").Replace("\r", "\n");
        t = Regex.Replace(t, @"[ \t]+", " ");
        t = Regex.Replace(t, @"\n{3,}", "\n\n");
        t = Regex.Replace(t, @"[^\p{L}\p{N}\p{P}\p{Z}\n]", "");
        t = Regex.Replace(t, @"(^|\n)[\p{P}\p{S}]{4,}(\n|$)", "\n");
        t = Regex.Replace(t, @"\s+([,.;:!?])", "$1");
        t = Regex.Replace(t, @"([,.;:!?]){3,}", "$1$1");
        return t.Trim();
    }
}