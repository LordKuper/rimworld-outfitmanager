using System;
using System.IO;
using System.Reflection;

namespace LordKuper.OutfitManager.Tests;

/// <summary>
///     Resolves RimWorld assemblies by reading the AssemblyMetadata "RimWorldManagedDir" attribute
///     and registering an AppDomain.AssemblyResolve handler that loads assemblies from that directory.
///     This fixture runs at the namespace root level before any tests in the LordKuper.OutfitManager.Tests
///     namespace tree.
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
        var rimWorldManagedDirAttribute = testAssembly.GetCustomAttribute<AssemblyMetadataAttribute>();
        if (rimWorldManagedDirAttribute?.Key != "RimWorldManagedDir" ||
            string.IsNullOrEmpty(rimWorldManagedDirAttribute.Value))
        {
            throw new InvalidOperationException(
                "RimWorldAssemblyResolverFixture: AssemblyMetadata 'RimWorldManagedDir' not found or empty. " +
                "Check the test project file for <AssemblyMetadata Include=\"RimWorldManagedDir\" Value=\"...\" />.");
        }

        var rimWorldManagedDir = rimWorldManagedDirAttribute.Value;

        if (!Directory.Exists(rimWorldManagedDir))
        {
            throw new DirectoryNotFoundException(
                $"RimWorldAssemblyResolverFixture: RimWorldManagedDir '{rimWorldManagedDir}' does not exist. " +
                "Check that RIMWORLD_DIR environment variable is set correctly.");
        }

        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
        {
            var assemblyName = args.Name.Split(',')[0];
            var assemblyPath = Path.Combine(rimWorldManagedDir, $"{assemblyName}.dll");

            if (File.Exists(assemblyPath))
            {
                return Assembly.LoadFrom(assemblyPath);
            }

            return null;
        };
    }
}
