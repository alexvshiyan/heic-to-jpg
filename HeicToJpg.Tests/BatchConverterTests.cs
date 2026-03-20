using HeicToJpg.Core;
using Xunit;

namespace HeicToJpg.Tests;

/// <summary>
/// Fake engine that lets each test control per-file success/failure behaviour.
/// </summary>
internal sealed class FakeEngine : IConversionEngine
{
    private readonly Dictionary<string, Func<string>> _handlers = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Register a successful output for this filename.</summary>
    public FakeEngine Succeeds(string inputName, string outputPath)
    {
        _handlers[inputName] = () => outputPath;
        return this;
    }

    /// <summary>Register a thrown exception for this filename.</summary>
    public FakeEngine Throws(string inputName, Exception ex)
    {
        _handlers[inputName] = () => throw ex;
        return this;
    }

    public string Convert(string sourcePath, ConversionConfig? config = null)
    {
        var name = Path.GetFileName(sourcePath);
        if (_handlers.TryGetValue(name, out var handler))
            return handler();
        return Path.ChangeExtension(sourcePath, ".jpg");
    }
}

public class BatchConverterTests
{
    private static BatchConverter Make(FakeEngine engine) => new(engine);

    // ── all succeed ───────────────────────────────────────────────────────────

    [Fact]
    public void Convert_AllSucceed_AllResultsSuccessful()
    {
        var engine = new FakeEngine()
            .Succeeds("a.heic", "a.jpg")
            .Succeeds("b.heic", "b.jpg");

        var results = Make(engine).Convert(new[] { "a.heic", "b.heic" });

        Assert.Equal(2, results.Count);
        Assert.All(results, r => Assert.True(r.Success));
        Assert.Equal("a.jpg", results[0].OutputName);
        Assert.Equal("b.jpg", results[1].OutputName);
    }

    // ── one fails, rest continue ──────────────────────────────────────────────

    [Fact]
    public void Convert_OneFileFails_RemainingFilesStillConverted()
    {
        var engine = new FakeEngine()
            .Succeeds("good.heic", "good.jpg")
            .Throws("bad.heic",   new IOException("locked", unchecked((int)0x80070020)))
            .Succeeds("also-good.heic", "also-good.jpg");

        var results = Make(engine).Convert(new[] { "good.heic", "bad.heic", "also-good.heic" });

        Assert.Equal(3, results.Count);
        Assert.True(results[0].Success);
        Assert.False(results[1].Success);
        Assert.True(results[2].Success);  // continued after failure
    }

    // ── error messages are classified ─────────────────────────────────────────

    [Fact]
    public void Convert_LockedFile_ResultContainsClassifiedMessage()
    {
        var engine = new FakeEngine()
            .Throws("locked.heic", new IOException("sharing violation", unchecked((int)0x80070020)));

        var results = Make(engine).Convert(new[] { "locked.heic" });

        Assert.Equal("File is locked or in use", results[0].ErrorMessage);
    }

    [Fact]
    public void Convert_NoPermission_ResultContainsClassifiedMessage()
    {
        var engine = new FakeEngine()
            .Throws("denied.heic", new UnauthorizedAccessException());

        var results = Make(engine).Convert(new[] { "denied.heic" });

        Assert.Equal("No write permission", results[0].ErrorMessage);
    }

    [Fact]
    public void Convert_DiskFull_ResultContainsClassifiedMessage()
    {
        var engine = new FakeEngine()
            .Throws("big.heic", new IOException("disk full", unchecked((int)0x80070070)));

        var results = Make(engine).Convert(new[] { "big.heic" });

        Assert.Equal("No disk space", results[0].ErrorMessage);
    }

    // ── all fail ──────────────────────────────────────────────────────────────

    [Fact]
    public void Convert_AllFail_AllResultsHaveErrors()
    {
        var engine = new FakeEngine()
            .Throws("x.heic", new UnauthorizedAccessException())
            .Throws("y.heic", new UnauthorizedAccessException());

        var results = Make(engine).Convert(new[] { "x.heic", "y.heic" });

        Assert.Equal(2, results.Count);
        Assert.All(results, r => Assert.False(r.Success));
        Assert.All(results, r => Assert.NotNull(r.ErrorMessage));
    }

    // ── result shape ──────────────────────────────────────────────────────────

    [Fact]
    public void Convert_Success_OutputNameIsFilenameOnly()
    {
        var engine = new FakeEngine()
            .Succeeds("photo.heic", @"C:\some\path\photo.jpg");

        var results = Make(engine).Convert(new[] { @"C:\input\photo.heic" });

        Assert.Equal("photo.jpg", results[0].OutputName);
    }

    [Fact]
    public void Convert_Failure_OutputNameIsNull()
    {
        var engine = new FakeEngine()
            .Throws("photo.heic", new UnauthorizedAccessException());

        var results = Make(engine).Convert(new[] { "photo.heic" });

        Assert.Null(results[0].OutputName);
    }
}
