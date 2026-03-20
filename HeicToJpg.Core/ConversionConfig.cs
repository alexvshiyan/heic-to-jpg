using System.Text.Json;

namespace HeicToJpg.Core;

public class ConversionConfig
{
    public int JpegQuality { get; set; } = 85;
    public bool PreserveExif { get; set; } = true;

    private const int DefaultQuality = 85;

    private static readonly string ConfigPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "HeicToJpg", "config.json");

    public static ConversionConfig Load() => LoadFrom(ConfigPath);

    public static ConversionConfig LoadFrom(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                WriteDefault(path);
                return new ConversionConfig();
            }

            var json = File.ReadAllText(path, System.Text.Encoding.UTF8);
            var cfg = JsonSerializer.Deserialize<ConversionConfig>(json) ?? new ConversionConfig();
            if (cfg.JpegQuality < 1 || cfg.JpegQuality > 100)
                cfg.JpegQuality = DefaultQuality;
            return cfg;
        }
        catch
        {
            return new ConversionConfig();
        }
    }

    private static void WriteDefault(string path)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var json = JsonSerializer.Serialize(new ConversionConfig(),
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json, System.Text.Encoding.UTF8);
        }
        catch { }
    }
}
