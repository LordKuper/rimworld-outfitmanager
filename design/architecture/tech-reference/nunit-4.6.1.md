---
responsibility:
  owns: project-vetted reference for one technology version (apis used, version specifics, project conventions)
  excludes: adr rationale, code, full stack overview, build commands
  delegates_to: stack.html (overview), adr/ (decisions), commands.yaml (commands)
---

# NUnit @ 4.6.1

## Canonical source
- Official docs: https://docs.nunit.org/
- Breaking changes (4.0): https://docs.nunit.org/articles/nunit/release-notes/breaking-changes.html
- Last verified: 2026-06-07

## API surface used in project
OM currently has **no test project** — this reference describes the target test setup once `Source/OutfitManager.Tests/` is created during the legacy migration. The intended import is global via `<Using Include="NUnit.Framework" />` in `Source/OutfitManager.Tests/OutfitManager.Tests.csproj` (no per-file `using NUnit.Framework;` directive). Expected attribute surface:
- `[TestFixture]`: marks a test class.
- `[Test]`: marks a parameterless test method; example: `[Test] public void WorkTypeScoreFactor_DefaultsToTwo()`.
- `[TestCase]`: parameterized test cases — useful for OM's normalized-weight math (`WorkTypeHelper.GetNormalizedWorkTypeWeights`) and score clamping.
- `[SetUp]` / `[TearDown]`: per-test snapshot/restore of OM's mutable static state (the `Settings` class holds static `_workTypeRules`, `_workTypeScoreFactor`, `_isInitialized`; `ApparelScoring` holds a static `_isInitialized` and a static `ConditionalWeakTable` cache).
- `[SetUpFixture]` + `[OneTimeSetUp]`: assembly/namespace-scoped one-time setup — required to register a RimWorld `AssemblyResolve` handler before any RimWorld-typed test loads (mirrors EM's `RimWorldAssemblyResolverFixture`).
- `[NonParallelizable]`: forces serial execution for tests that mutate `Settings`/`ApparelScoring` static state.

## Version-specific notes
- 4.6.1 is the confirmed latest release as of 2026-05-19.
- NUnit 4.x minimum supported targets are .NET Framework 4.6.2 and .NET 6.0. OM's `net48` target is supported.
- Test discovery/execution requires the VSTest adapter `NUnit3TestAdapter` (despite the "3" in its name it runs NUnit 4 tests) plus `Microsoft.NET.Test.Sdk`. See the `nunit3testadapter-6.2.0` and `microsoft-net-test-sdk-18.6.0` references.

## Deprecations and breaking changes from prior version (3.x -> 4.x)
- Classic asserts moved to a legacy library/namespace: `Assert.AreEqual`, `Assert.IsTrue`, etc. are now `NUnit.Framework.Legacy.ClassicAssert.*`. Standalone helpers `CollectionAssert`, `StringAssert`, `DirectoryAssert`, `FileAssert` also moved to `NUnit.Framework.Legacy`. Not relevant in practice because the project asserts with FluentAssertions, not NUnit asserts (see Project conventions).
- `Assert.That` overloads taking a format string + `params object[]` were removed in favor of a `FormattableString`-based overload.
- The constraint model (`Assert.That(actual, Is...)`) is the 4.x-preferred assertion model; classic syntax is legacy.
- Minimum framework versions raised (see Version-specific notes).

## Project conventions
- Assertions use FluentAssertions `.Should()`, NOT NUnit `Assert.*`. The NUnit 3->4 assert migration is therefore moot for new code; do not introduce `Assert.*` or `ClassicAssert.*`. See the `fluentassertions-7.2.2` reference.
- Static-state isolation is mandatory for any test that touches mutable static state. OM's `Settings` and `ApparelScoring` are static-heavy, so derive such tests from a `StateIsolationTestBase`-style base that snapshots/restores the relevant private static fields, and mark them `[NonParallelizable]`.
- RimWorld and Unity assemblies are not NuGet packages; they live in the local RimWorld `Managed` directory. A global `[SetUpFixture]` must register an `AppDomain.CurrentDomain.AssemblyResolve` handler in `[OneTimeSetUp]` before any RimWorld-typed test loads. Keep all test classes under the `OutfitManager.Tests` namespace (or sub-namespaces) so the fixture's setup runs first.
- Test naming convention: `Method_Scenario_ExpectedOutcome`.

## Known issues and workarounds
- If a state-isolation base reflects a static field that no longer exists in production code, the reflection helper should throw `InvalidOperationException` by design ("test infrastructure may be out of sync with production code"). Keep the reflected field names (`Settings._workTypeRules`, `ApparelScoring._isInitialized`, etc.) in sync with production when those types change.
