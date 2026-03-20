using ImageMagick;

namespace HeicToJpg.Core;

public class MagickConversionEngine : IConversionEngine
{
    public string Convert(string sourcePath, ConversionConfig? config = null)
    {
        config ??= ConversionConfig.Load();

        var outputPath = FileNaming.ResolveOutputPath(
            Path.ChangeExtension(sourcePath, ".jpg"));

        using var image = new MagickImage(sourcePath);

        if (!config.PreserveExif)
            image.Strip();

        image.Format = MagickFormat.Jpeg;
        image.Quality = (uint)config.JpegQuality;
        image.Write(outputPath);

        return outputPath;
    }
}
