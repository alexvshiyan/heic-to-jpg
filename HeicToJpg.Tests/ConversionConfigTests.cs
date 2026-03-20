using HeicToJpg.Core;
using Xunit;

namespace HeicToJpg.Tests;

public class ConversionConfigTests : IDisposable
{
    private readonly string _dir;
    private readonly string _configPath;

    public ConversionConfigTests()
    {
        _dir = Path.Combine(Path.GetTempPath(), "CfgTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
        _configPath = Path.Combine(_dir, "config.json");
    }

    public void Dispose() => Directory.Delete(_dir, recursive: true);

    // ── default creation ──────────────────────────────────────────────────────

    [Fact]
    public void Load_FileNotExists_ReturnsDefaults()
    {
        var cfg = ConversionConfig.LoadFrom(_configPath);

        Assert.Equal(85, cfg.JpegQuality);
        Assert.True(cfg.PreserveExif);
    }

    [Fact]
    public void Load_FileNotExists_CreatesConfigFile()
    {
        ConversionConfig.LoadFrom(_configPath);

        Assert.True(File.Exists(_configPath));
    }

    [Fact]
    public void Load_FileNotExists_CreatedFileContainsDefaults()
    {
        ConversionConfig.LoadFrom(_configPath);

        var saved = ConversionConfig.LoadFrom(_configPath);
        Assert.Equal(85, saved.JpegQuality);
        Assert.True(saved.PreserveExif);
    }

    // ── loading stored values ─────────────────────────────────────────────────

    [Fact]
    public void Load_ValidFile_ReturnsStoredQuality()
    {
        File.WriteAllText(_configPath,
            """{"JpegQuality":70,"PreserveExif":true}""");

        Assert.Equal(70, ConversionConfig.LoadFrom(_configPath).JpegQuality);
    }

    [Fact]
    public void Load_ValidFile_PreserveExifFalseRoundTrips()
    {
        File.WriteAllText(_configPath,
            """{"JpegQuality":85,"PreserveExif":false}""");

        Assert.False(ConversionConfig.LoadFrom(_configPath).PreserveExif);
    }

    // ── boundary values ───────────────────────────────────────────────────────

    [Fact]
    public void Load_Quality1_IsAccepted()
    {
        File.WriteAllText(_configPath, """{"JpegQuality":1,"PreserveExif":true}""");

        Assert.Equal(1, ConversionConfig.LoadFrom(_configPath).JpegQuality);
    }

    [Fact]
    public void Load_Quality100_IsAccepted()
    {
        File.WriteAllText(_configPath, """{"JpegQuality":100,"PreserveExif":true}""");

        Assert.Equal(100, ConversionConfig.LoadFrom(_configPath).JpegQuality);
    }

    // ── invalid quality falls back to default ─────────────────────────────────

    [Fact]
    public void Load_QualityZero_UsesDefault85()
    {
        File.WriteAllText(_configPath, """{"JpegQuality":0,"PreserveExif":true}""");

        Assert.Equal(85, ConversionConfig.LoadFrom(_configPath).JpegQuality);
    }

    [Fact]
    public void Load_QualityAbove100_UsesDefault85()
    {
        File.WriteAllText(_configPath, """{"JpegQuality":101,"PreserveExif":true}""");

        Assert.Equal(85, ConversionConfig.LoadFrom(_configPath).JpegQuality);
    }

    [Fact]
    public void Load_QualityNegative_UsesDefault85()
    {
        File.WriteAllText(_configPath, """{"JpegQuality":-1,"PreserveExif":true}""");

        Assert.Equal(85, ConversionConfig.LoadFrom(_configPath).JpegQuality);
    }

    // ── corrupt / malformed JSON ──────────────────────────────────────────────

    [Fact]
    public void Load_CorruptJson_ReturnsDefaults()
    {
        File.WriteAllText(_configPath, "not json {{{{");

        var cfg = ConversionConfig.LoadFrom(_configPath);
        Assert.Equal(85, cfg.JpegQuality);
        Assert.True(cfg.PreserveExif);
    }
}
