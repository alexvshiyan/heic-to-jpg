using System.Text.Json;

namespace HeicToJpg.Core;

public class ConversionConfig
{
    public int JpegQuality { get; set; } = 85;
    public bool PreserveExif { get; set; } = true;

    private static readonly string ConfigPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "HeicToJpg", "config.json");

    public static ConversionConfig Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);
                return JsonSerializer.Deserialize<ConversionConfig>(json) ?? new ConversionConfig();
            }
        }
        catch { }
        return new ConversionConfig();
    }
}
