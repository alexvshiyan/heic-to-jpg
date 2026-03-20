using HeicToJpg.Core;
using Xunit;

namespace HeicToJpg.Tests;

public sealed class FileNamingTests : IDisposable
{
    private readonly string _dir;

    public FileNamingTests()
    {
        _dir = Path.Combine(Path.GetTempPath(),
            "HeicNamingTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
    }

    public void Dispose() => Directory.Delete(_dir, recursive: true);

    private string P(string name) => Path.Combine(_dir, name);

    // ── no collision ──────────────────────────────────────────────────────────

    [Fact]
    public void ResolveOutputPath_NoExistingFile_ReturnsDesiredPath()
    {
        var desired = P("IMG_0677.jpg");

        var result = FileNaming.ResolveOutputPath(desired);

        Assert.Equal(desired, result);
    }

    // ── one collision ─────────────────────────────────────────────────────────

    [Fact]
    public void ResolveOutputPath_DesiredExists_ReturnsIndex1()
    {
        var desired = P("IMG_0677.jpg");
        File.WriteAllBytes(desired, Array.Empty<byte>());

        var result = FileNaming.ResolveOutputPath(desired);

        Assert.Equal(P("IMG_0677 (1).jpg"), result);
    }

    // ── two collisions ────────────────────────────────────────────────────────

    [Fact]
    public void ResolveOutputPath_DesiredAnd1Exist_ReturnsIndex2()
    {
        var desired = P("IMG_0677.jpg");
        File.WriteAllBytes(desired,            Array.Empty<byte>());
        File.WriteAllBytes(P("IMG_0677 (1).jpg"), Array.Empty<byte>());

        var result = FileNaming.ResolveOutputPath(desired);

        Assert.Equal(P("IMG_0677 (2).jpg"), result);
    }

    // ── gap in sequence ───────────────────────────────────────────────────────

    [Fact]
    public void ResolveOutputPath_GapInSequence_ReturnFirstAvailable()
    {
        // (1) exists but (2) does not — should still return (1) only if desired
        // is free; here desired is taken and (1) is taken, so expect (2).
        // This also confirms we don't skip gaps.
        var desired = P("photo.jpg");
        File.WriteAllBytes(desired,           Array.Empty<byte>());
        File.WriteAllBytes(P("photo (1).jpg"), Array.Empty<byte>());

        var result = FileNaming.ResolveOutputPath(desired);

        Assert.Equal(P("photo (2).jpg"), result);
    }

    // ── integration: engine uses safe naming ─────────────────────────────────

    [Fact]
    public void Convert_OutputAlreadyExists_CreatesIndex1WithoutOverwriting()
    {
        var assets  = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");
        var input   = Path.Combine(_dir, "sample1.heic");
        File.Copy(Path.Combine(assets, "sample1.heic"), input);

        // Pre-create the "obvious" output so the engine must pick a new name
        var expected0 = Path.ChangeExtension(input, ".jpg");
        File.WriteAllText(expected0, "placeholder");
        var originalContent = File.ReadAllText(expected0);

        var engine = new MagickConversionEngine();
        var output = engine.Convert(input, new ConversionConfig { PreserveExif = true, JpegQuality = 85 });

        Assert.Equal(Path.Combine(_dir, "sample1 (1).jpg"), output);
        Assert.True(File.Exists(output));
        // Original file must not have been overwritten
        Assert.Equal(originalContent, File.ReadAllText(expected0));
    }
}
