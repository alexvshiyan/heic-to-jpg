using HeicToJpg.Core;
using Xunit;

namespace HeicToJpg.Tests;

public class ErrorClassifierTests
{
    // ── UnauthorizedAccessException ───────────────────────────────────────────

    [Fact]
    public void GetUserMessage_UnauthorizedAccess_ReturnsPermissionMessage()
    {
        var ex = new UnauthorizedAccessException("access denied");
        Assert.Equal("No write permission", ErrorClassifier.GetUserMessage(ex));
    }

    // ── IOException: sharing / lock violations ────────────────────────────────

    [Fact]
    public void GetUserMessage_SharingViolation_ReturnsLockedMessage()
    {
        var ex = new IOException("locked", unchecked((int)0x80070020)); // ERROR_SHARING_VIOLATION
        Assert.Equal("File is locked or in use", ErrorClassifier.GetUserMessage(ex));
    }

    [Fact]
    public void GetUserMessage_LockViolation_ReturnsLockedMessage()
    {
        var ex = new IOException("locked", unchecked((int)0x80070021)); // ERROR_LOCK_VIOLATION
        Assert.Equal("File is locked or in use", ErrorClassifier.GetUserMessage(ex));
    }

    // ── IOException: disk full ────────────────────────────────────────────────

    [Fact]
    public void GetUserMessage_DiskFull_ReturnsDiskSpaceMessage()
    {
        var ex = new IOException("disk full", unchecked((int)0x80070070)); // ERROR_DISK_FULL
        Assert.Equal("No disk space", ErrorClassifier.GetUserMessage(ex));
    }

    [Fact]
    public void GetUserMessage_HandleDiskFull_ReturnsDiskSpaceMessage()
    {
        var ex = new IOException("disk full", unchecked((int)0x80070027)); // ERROR_HANDLE_DISK_FULL
        Assert.Equal("No disk space", ErrorClassifier.GetUserMessage(ex));
    }

    // ── Generic IOException falls through to ex.Message ──────────────────────

    [Fact]
    public void GetUserMessage_GenericIoException_ReturnsRawMessage()
    {
        var ex = new IOException("some other io error");
        Assert.Equal("some other io error", ErrorClassifier.GetUserMessage(ex));
    }

    // ── MagickException (via actual corrupted-file conversion) ────────────────

    [Fact]
    public void GetUserMessage_CorruptedFileConversion_ReturnsCorruptedMessage()
    {
        var dir     = Path.Combine(Path.GetTempPath(), "ErrClsTest_" + Guid.NewGuid().ToString("N"));
        var corrupt = Path.Combine(dir, "bad.heic");
        try
        {
            Directory.CreateDirectory(dir);
            File.WriteAllBytes(corrupt, new byte[] { 0x00, 0x11, 0x22, 0x33 });

            var ex = Record.Exception(() =>
                new MagickConversionEngine().Convert(corrupt, new ConversionConfig()));

            Assert.NotNull(ex);
            Assert.Equal("File is corrupted or not a valid HEIC",
                ErrorClassifier.GetUserMessage(ex));
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    // ── Unknown exception falls through to ex.Message ────────────────────────

    [Fact]
    public void GetUserMessage_UnknownException_ReturnsRawMessage()
    {
        var ex = new InvalidOperationException("something went wrong");
        Assert.Equal("something went wrong", ErrorClassifier.GetUserMessage(ex));
    }
}
