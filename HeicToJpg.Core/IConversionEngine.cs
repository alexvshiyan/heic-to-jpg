namespace HeicToJpg.Core;

public interface IConversionEngine
{
    /// <summary>
    /// Converts a HEIC file to JPEG. Returns the output file path.
    /// </summary>
    string Convert(string sourcePath, ConversionConfig? config = null);
}
