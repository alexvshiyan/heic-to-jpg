// ModuleInitializerAttribute exists in .NET 5+ but not .NET Framework 4.8.
// The C# compiler only requires the type to exist by its full name.
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    internal sealed class ModuleInitializerAttribute : Attribute { }
}
