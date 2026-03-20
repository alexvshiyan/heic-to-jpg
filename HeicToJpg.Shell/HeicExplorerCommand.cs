using System.Runtime.InteropServices;
using HeicToJpg.Core;

namespace HeicToJpg.Shell;

[ComVisible(true)]
[Guid("4B8E3F21-A5D7-4C9E-B823-F9D156A7E302")]
[ClassInterface(ClassInterfaceType.None)]
public sealed class HeicExplorerCommand : IExplorerCommand
{
    private static readonly Guid CommandId = new("4B8E3F21-A5D7-4C9E-B823-F9D156A7E302");
    private static readonly string LogPath = Path.Combine(Path.GetTempPath(), "HeicShellDebug.log");

    private const int SIGDN_FILESYSPATH = unchecked((int)0x80058000);
    private const int S_OK      = 0;
    private const int S_FALSE   = 1;
    private const int E_NOTIMPL = unchecked((int)0x80004001);

    static HeicExplorerCommand()
    {
        Log("IExplorerCommand CLASS LOADED by process: " +
            System.Diagnostics.Process.GetCurrentProcess().ProcessName +
            $" | Thread apartment: {System.Threading.Thread.CurrentThread.GetApartmentState()}");
    }

    public int GetTitle(IShellItemArray? psiItemArray, out IntPtr ppszName)
    {
        Log("GetTitle called");
        ppszName = Marshal.StringToCoTaskMemUni("Convert to JPEG");
        return S_OK;
    }

    public int GetIcon(IShellItemArray? psiItemArray, out IntPtr ppszIcon)
    {
        ppszIcon = IntPtr.Zero;
        return S_FALSE;
    }

    public int GetToolTip(IShellItemArray? psiItemArray, out IntPtr ppszInfotip)
    {
        ppszInfotip = IntPtr.Zero;
        return S_FALSE;
    }

    public int GetCanonicalName(out Guid pguidCommandName)
    {
        pguidCommandName = CommandId;
        return S_OK;
    }

    public int GetState(IShellItemArray? psiItemArray, bool fOkToBeSlow, out uint pCmdState)
    {
        Log($"GetState called (fOkToBeSlow={fOkToBeSlow})");
        pCmdState = 0; // ECS_ENABLED
        return S_OK;
    }

    public int Invoke(IShellItemArray? psiItemArray, IntPtr pbc)
    {
        Log("Invoke called");
        if (psiItemArray == null) return S_OK;

        var paths = GetPaths(psiItemArray);
        Task.Run(() => ConvertFiles(paths));
        return S_OK;
    }

    public int GetFlags(out uint pFlags)
    {
        pFlags = 0; // ECF_DEFAULT
        return S_OK;
    }

    public int EnumSubCommands(out IntPtr ppEnum)
    {
        ppEnum = IntPtr.Zero;
        return E_NOTIMPL;
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static void Log(string message)
    {
        try { File.AppendAllText(LogPath, $"[{DateTime.Now:HH:mm:ss.fff}] [ECH] {message}{Environment.NewLine}"); }
        catch { }
    }

    private static string[] GetPaths(IShellItemArray psiItemArray)
    {
        psiItemArray.GetCount(out uint count);
        var paths = new string[count];
        for (uint i = 0; i < count; i++)
        {
            psiItemArray.GetItemAt(i, out IShellItem item);
            item.GetDisplayName(SIGDN_FILESYSPATH, out IntPtr pszName);
            paths[i] = Marshal.PtrToStringUni(pszName) ?? string.Empty;
            Marshal.FreeCoTaskMem(pszName);
        }
        return paths;
    }

    private static void ConvertFiles(string[] paths)
    {
        var engine = new MagickConversionEngine();
        var config = ConversionConfig.Load();
        foreach (var path in paths)
        {
            try { engine.Convert(path, config); }
            catch { }
        }
    }
}
