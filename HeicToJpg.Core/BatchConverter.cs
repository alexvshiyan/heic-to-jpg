namespace HeicToJpg.Core;

public class BatchConverter
{
    public sealed record FileResult(
        string InputName,
        string? OutputName,
        string? ErrorMessage)
    {
        public bool Success => ErrorMessage is null;
    }

    private readonly IConversionEngine _engine;

    public BatchConverter(IConversionEngine engine) => _engine = engine;

    /// <summary>
    /// Converts every path in <paramref name="inputPaths"/>, continuing on
    /// individual failures. Returns one result per input file.
    /// </summary>
    public IReadOnlyList<FileResult> Convert(
        IEnumerable<string> inputPaths,
        ConversionConfig?   config = null)
    {
        config ??= ConversionConfig.Load();
        var results = new List<FileResult>();

        foreach (var path in inputPaths)
        {
            try
            {
                var outputPath = _engine.Convert(path, config);
                results.Add(new FileResult(
                    Path.GetFileName(path),
                    Path.GetFileName(outputPath),
                    null));
            }
            catch (Exception ex)
            {
                results.Add(new FileResult(
                    Path.GetFileName(path),
                    null,
                    ErrorClassifier.GetUserMessage(ex)));
            }
        }

        return results;
    }
}
