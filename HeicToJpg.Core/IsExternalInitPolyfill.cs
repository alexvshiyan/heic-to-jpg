// 'record' types use init-only setters which require this type.
// It exists in .NET 5+ but not .NET Framework 4.8 — the C# compiler
// only needs the name to be present.
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
