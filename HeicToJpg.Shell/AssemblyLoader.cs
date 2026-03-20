using System.Reflection;
using System.Runtime.CompilerServices;

namespace HeicToJpg.Shell
{
    internal static class AssemblyLoader
    {
        [ModuleInitializer]
        internal static void Initialize()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
        }

        private static Assembly? ResolveAssembly(object? sender, ResolveEventArgs args)
        {
            var shortName = new AssemblyName(args.Name).Name;
            var dir = Path.GetDirectoryName(typeof(AssemblyLoader).Assembly.Location)!;
            var path = Path.Combine(dir, shortName + ".dll");
            return File.Exists(path) ? Assembly.LoadFrom(path) : null;
        }
    }
}
