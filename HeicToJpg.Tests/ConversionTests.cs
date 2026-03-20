using ImageMagick;
using HeicToJpg.Core;
using Xunit;

namespace HeicToJpg.Tests;

/// <summary>
/// Each test gets its own temp work directory so conversions never touch the
/// Assets folder and cleanup is automatic via IDisposable.
/// </summary>
public sealed class ConversionTests : IDisposable
{
    private readonly MagickConversionEngine _engine = new();
    private readonly ConversionConfig _config = new() { PreserveExif = true, JpegQuality = 85 };

    // Resolved once per test class instance
    private readonly string _assetsDir =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");

    // Isolated temp directory for output files
    private readonly string _workDir;

    public ConversionTests()
    {
        _workDir = Path.Combine(
            Path.GetTempPath(),
            "HeicToJpgTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_workDir);
    }

    public void Dispose() =>
        Directory.Delete(_workDir, recursive: true);

    // ── helpers ──────────────────────────────────────────────────────────────

    /// <summary>Copies a named asset into the work dir and returns its path.</summary>
    private string PrepareAsset(string assetFileName)
    {
        var dest = Path.Combine(_workDir, assetFileName);
        File.Copy(Path.Combine(_assetsDir, assetFileName), dest);
        return dest;
    }

    // ── 1. Output file exists ─────────────────────────────────────────────────

    [Theory]
    [InlineData("sample1.heic")]
    [InlineData("sample2.heic")]
    [InlineData("sample3.heic")]
    public void Convert_ValidHeic_OutputFileExists(string assetName)
    {
        var input = PrepareAsset(assetName);

        var output = _engine.Convert(input, _config);

        Assert.True(File.Exists(output), $"Expected output file at {output}");
    }

    // ── 2. Output is a valid JPEG ─────────────────────────────────────────────

    [Theory]
    [InlineData("sample1.heic")]
    [InlineData("sample2.heic")]
    [InlineData("sample3.heic")]
    public void Convert_ValidHeic_OutputHasJpegHeader(string assetName)
    {
        var input  = PrepareAsset(assetName);
        var output = _engine.Convert(input, _config);

        var header = new byte[3];
        using var fs = File.OpenRead(output);
        fs.Read(header, 0, header.Length);

        // JPEG magic bytes: FF D8 FF
        Assert.Equal(0xFF, header[0]);
        Assert.Equal(0xD8, header[1]);
        Assert.Equal(0xFF, header[2]);
    }

    // ── 3. EXIF metadata preserved ────────────────────────────────────────────

    [Fact]
    public void Convert_PreserveExifTrue_ExifProfilePresentInOutput()
    {
        var input  = PrepareAsset("sample1.heic");
        var output = _engine.Convert(input, _config);

        using var image = new MagickImage(output);
        var exif = image.GetExifProfile();

        Assert.NotNull(exif);
        Assert.NotEmpty(exif.Values);
    }

    [Fact]
    public void Convert_PreserveExifFalse_ExifProfileAbsentInOutput()
    {
        var input  = PrepareAsset("sample1.heic");
        var config = new ConversionConfig { PreserveExif = false, JpegQuality = 85 };

        var output = _engine.Convert(input, config);

        using var image = new MagickImage(output);
        Assert.Null(image.GetExifProfile());
    }

    // ── 4. Cyrillic characters in path ────────────────────────────────────────

    [Fact]
    public void Convert_CyrillicDirectoryAndFilename_OutputFileExists()
    {
        var cyrillicDir = Path.Combine(_workDir, "Тест_конвертации");
        Directory.CreateDirectory(cyrillicDir);

        var input  = Path.Combine(cyrillicDir, "Фото_тест.heic");
        File.Copy(Path.Combine(_assetsDir, "sample1.heic"), input);

        var output = _engine.Convert(input, _config);

        Assert.True(File.Exists(output));
        Assert.Equal(".jpg", Path.GetExtension(output), StringComparer.OrdinalIgnoreCase);
    }

    // ── 5. Corrupted input throws a handled exception ─────────────────────────

    [Fact]
    public void Convert_CorruptedFile_ThrowsException()
    {
        var corrupt = Path.Combine(_workDir, "corrupt.heic");
        File.WriteAllBytes(corrupt, new byte[] { 0x00, 0x11, 0x22, 0x33, 0x44 });

        var ex = Record.Exception(() => _engine.Convert(corrupt, _config));

        Assert.NotNull(ex);
    }

    [Fact]
    public void Convert_EmptyFile_ThrowsException()
    {
        var empty = Path.Combine(_workDir, "empty.heic");
        File.WriteAllBytes(empty, Array.Empty<byte>());

        var ex = Record.Exception(() => _engine.Convert(empty, _config));

        Assert.NotNull(ex);
    }
}
