using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using HeicToJpg.Core;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;
using WinRtXml = Windows.Data.Xml.Dom;
using WinRtNotif = Windows.UI.Notifications;

namespace HeicToJpg.Shell;

[ComVisible(true)]
[Guid("B3D5F8A2-7C14-4E9B-A612-0D3F8B5E2C1A")]
[COMServerAssociation(AssociationType.ClassOfExtension, ".heic")]
public class HeicContextMenuHandler : SharpContextMenu
{
    private static readonly string LogPath =
        Path.Combine(Path.GetTempPath(), "HeicShellDebug.log");

    static HeicContextMenuHandler()
    {
        WriteLog("CLASS LOADED by process: " + System.Diagnostics.Process.GetCurrentProcess().ProcessName);
    }

    protected override bool CanShowMenu()
    {
        return SelectedItemPaths.All(p =>
            Path.GetExtension(p).Equals(".heic", StringComparison.OrdinalIgnoreCase));
    }

    protected override ContextMenuStrip CreateMenu()
    {
        var menu = new ContextMenuStrip();
        var item = new ToolStripMenuItem { Text = "Convert to JPEG" };
        item.Click += OnConvertClick;
        menu.Items.Add(item);
        return menu;
    }

    private void OnConvertClick(object? sender, EventArgs e)
    {
        var paths = SelectedItemPaths.ToList();

        if (paths.Count > 1)
            ShowToast("Convert to JPEG", $"Converting {paths.Count} files\u2026");

        var results = new BatchConverter(new MagickConversionEngine())
            .Convert(paths, ConversionConfig.Load());

        foreach (var r in results)
            if (r.Success) WriteLog($"OK:   {r.InputName} -> {r.OutputName}");
            else           WriteLog($"FAIL: {r.InputName} -> {r.ErrorMessage}");

        ShowSummaryToast(results);
    }

    private static void ShowSummaryToast(IReadOnlyList<BatchConverter.FileResult> results)
    {
        var ok   = results.Where(r => r.Success).ToList();
        var fail = results.Where(r => !r.Success).ToList();

        string body;
        string? detail = null;

        if (fail.Count == 0)
        {
            body = ok.Count == 1
                ? $"{ok[0].OutputName} saved"
                : $"{ok.Count} converted";
        }
        else if (ok.Count == 0)
        {
            body   = fail.Count == 1
                ? $"Error: {fail[0].ErrorMessage}"
                : $"{fail.Count} failed";
            detail = fail.Count > 1 ? $"{fail[0].InputName}: {fail[0].ErrorMessage}" : null;
        }
        else
        {
            body   = $"{ok.Count} converted, {fail.Count} failed";
            detail = $"{fail[0].InputName}: {fail[0].ErrorMessage}";
        }

        ShowToast("Convert to JPEG", body, detail);
    }

    private static void WriteLog(string message)
    {
        try
        {
            File.AppendAllText(LogPath,
                $"[{DateTime.Now:HH:mm:ss.fff}] {message}{Environment.NewLine}",
                System.Text.Encoding.UTF8);
        }
        catch { }
    }

    private const string ToastAppId = "HeicToJpg.Converter";

    private static void ShowToast(string title, string body, string? detail = null)
    {
        try
        {
            var detailLine = detail is null
                ? string.Empty
                : $"<text>{SecurityElement.Escape(detail)}</text>";

            var xml = "<toast><visual><binding template=\"ToastGeneric\">" +
                      "<text>" + SecurityElement.Escape(title) + "</text>" +
                      "<text>" + SecurityElement.Escape(body)  + "</text>" +
                      detailLine +
                      "</binding></visual></toast>";

            var doc = new WinRtXml.XmlDocument();
            doc.LoadXml(xml);

            var toast = new WinRtNotif.ToastNotification(doc);
            WinRtNotif.ToastNotificationManager
                .CreateToastNotifier(ToastAppId)
                .Show(toast);

            WriteLog($"Toast shown: {title} | {body}");
        }
        catch (Exception ex)
        {
            WriteLog($"Toast failed: {ex.GetType().Name}: {ex.Message}");
        }
    }
}
