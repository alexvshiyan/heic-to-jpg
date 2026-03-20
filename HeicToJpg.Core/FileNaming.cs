namespace HeicToJpg.Core;

public static class FileNaming
{
    /// <summary>
    /// Returns a non-existing file path using Windows Explorer-style collision
    /// avoidance: "name.jpg", "name (1).jpg", "name (2).jpg", …
    /// </summary>
    public static string ResolveOutputPath(string desiredPath)
    {
        if (!File.Exists(desiredPath))
            return desiredPath;

        var dir  = Path.GetDirectoryName(desiredPath) ?? string.Empty;
        var stem = Path.GetFileNameWithoutExtension(desiredPath);
        var ext  = Path.GetExtension(desiredPath);

        for (int n = 1; ; n++)
        {
            var candidate = Path.Combine(dir, $"{stem} ({n}){ext}");
            if (!File.Exists(candidate))
                return candidate;
        }
    }
}
