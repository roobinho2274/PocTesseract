namespace OcrCore.Services.Implmentation;

public sealed class OcrOptions
{
    public string Langs { get; set; } = "por+eng+chi_sim+chi_sim_vert";
    public int Dpi { get; set; } = 300;
    public string TessdataPath { get; set; } = "./tessdata";
    public int MinTextLengthToSkipOcr { get; set; } = 100;
}
