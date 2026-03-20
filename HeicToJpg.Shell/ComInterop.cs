using System.Runtime.InteropServices;

namespace HeicToJpg.Shell;

// ── Interfaces we CONSUME (received from Windows, [ComImport]) ──────────────

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE")]
public interface IShellItem
{
    [PreserveSig] int BindToHandler(IntPtr pbc, ref Guid bhid, ref Guid riid, out IntPtr ppv);
    [PreserveSig] int GetParent(out IShellItem ppsi);
    [PreserveSig] int GetDisplayName(int sigdnName, out IntPtr ppszName);
    [PreserveSig] int GetAttributes(uint sfgaoMask, out uint psfgaoAttribs);
    [PreserveSig] int Compare(IShellItem psi, uint hint, out int piOrder);
}

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("B63EA76D-9D54-473A-A26D-72969EEF4B0B")]
public interface IShellItemArray
{
    [PreserveSig] int BindToHandler(IntPtr pbc, ref Guid bhid, ref Guid riid, out IntPtr ppv);
    [PreserveSig] int GetPropertyStore(int flags, ref Guid riid, out IntPtr ppv);
    [PreserveSig] int GetPropertyDescriptionList(IntPtr keyType, ref Guid riid, out IntPtr ppv);
    [PreserveSig] int GetAttributes(uint dwAttribFlags, uint sfgaoMask, out uint psfgaoAttribs);
    [PreserveSig] int GetCount(out uint pdwNumItems);
    [PreserveSig] int GetItemAt(uint dwIndex, out IShellItem ppsi);
    [PreserveSig] int EnumItems(out IntPtr ppenumShellItems);
}

// ── Interfaces we IMPLEMENT (exposed to Windows, no [ComImport]) ─────────────

[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("58BA0CB0-18AF-4f22-B834-EF0D03905832")]
public interface IExplorerCommand
{
    [PreserveSig] int GetTitle(IShellItemArray? psiItemArray, out IntPtr ppszName);
    [PreserveSig] int GetIcon(IShellItemArray? psiItemArray, out IntPtr ppszIcon);
    [PreserveSig] int GetToolTip(IShellItemArray? psiItemArray, out IntPtr ppszInfotip);
    [PreserveSig] int GetCanonicalName(out Guid pguidCommandName);
    [PreserveSig] int GetState(IShellItemArray? psiItemArray, [MarshalAs(UnmanagedType.Bool)] bool fOkToBeSlow, out uint pCmdState);
    [PreserveSig] int Invoke(IShellItemArray? psiItemArray, IntPtr pbc);
    [PreserveSig] int GetFlags(out uint pFlags);
    [PreserveSig] int EnumSubCommands(out IntPtr ppEnum); // always E_NOTIMPL
}
