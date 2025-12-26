using System.Text;
using System.Text.RegularExpressions;

namespace OcrCore.Util;

public static class CleanTextUtil
{
    public static string CleanText(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;
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
