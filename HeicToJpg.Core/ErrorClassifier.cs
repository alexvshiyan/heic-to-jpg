using ImageMagick;

namespace HeicToJpg.Core;

public static class ErrorClassifier
{
    public static string GetUserMessage(Exception ex) => ex switch
    {
        UnauthorizedAccessException           => "No write permission",
        IOException ioe when IsLocked(ioe)    => "File is locked or in use",
        IOException ioe when IsDiskFull(ioe)  => "No disk space",
        MagickException                       => "File is corrupted or not a valid HEIC",
        _                                     => ex.Message
    };

    // ERROR_SHARING_VIOLATION (32) / ERROR_LOCK_VIOLATION (33)
    private static bool IsLocked(IOException ex)
    {
        var code = ex.HResult & 0xFFFF;
        return code is 32 or 33;
    }

    // ERROR_DISK_FULL (112) / ERROR_HANDLE_DISK_FULL (39)
    private static bool IsDiskFull(IOException ex)
    {
        var code = ex.HResult & 0xFFFF;
        return code is 112 or 39;
    }
}
