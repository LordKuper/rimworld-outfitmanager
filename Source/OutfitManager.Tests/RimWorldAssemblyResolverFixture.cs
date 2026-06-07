using System;
using System.IO;
using System.Linq;
using System.Reflection;

/// <summary>
///     Resolves RimWorld assemblies by reading the AssemblyMetadata "RimWorldManagedDir" attribute
///     and registering an AppDomain.AssemblyResolve handler that loads assemblies from that directory.
///     Declared at the global (namespace-less) scope so NUnit runs this fixture before any test type
///     in any namespace loads — ensuring the RimWorld assembly resolver is registered before the CLR
///     attempts to load any RimWorld-typed class.
/// </summary>
[SetUpFixture]
public class RimWorldAssemblyResolverFixture
{
    /// <summary>
    ///     Registers the AppDomain.AssemblyResolve handler to load RimWorld assemblies from the configured
    ///     RimWorldManagedDir.
    /// </summary>
    [OneTimeSetUp]
    public void RegisterAssemblyResolver()
    {
        var testAssembly = typeof(RimWorldAssemblyResolverFixture).Assembly;
        var rimWorldManagedDirAttribute = testAssembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(a => a.Key == "RimWorldManagedDir")
            ?? throw new InvalidOperationException(
                "RimWorldAssemblyResolverFixture: AssemblyMetadata 'RimWorldManagedDir' not found. " +
                "Check the test project file for <AssemblyMetadata Include=\"RimWorldManagedDir\" Value=\"...\" />.");

        if (string.IsNullOrEmpty(rimWorldManagedDirAttribute.Value))
            throw new InvalidOperationException(
                "RimWorldAssemblyResolverFixture: AssemblyMetadata 'RimWorldManagedDir' is empty. " +
                "Check that the RIMWORLD_DIR environment variable is set correctly.");

        var rimWorldManagedDir = rimWorldManagedDirAttribute.Value;

        if (!Directory.Exists(rimWorldManagedDir))
            throw new DirectoryNotFoundException(
                $"RimWorldAssemblyResolverFixture: RimWorldManagedDir '{rimWorldManagedDir}' does not exist. " +
                "Check that the RIMWORLD_DIR environment variable is set correctly.");

        AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
        {
            var assemblyName = args.Name.Split(',')[0];
            var assemblyPath = Path.Combine(rimWorldManagedDir, $"{assemblyName}.dll");
            return File.Exists(assemblyPath) ? Assembly.LoadFrom(assemblyPath) : null;
        };
    }
}
