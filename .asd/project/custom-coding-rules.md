---
responsibility:
  owns: project-owner custom rules read during impl and impl-review phases
  excludes: universal rules, design-only rules
  delegates_to: custom-common-rules.md (all phases), custom-design-rules.md (design/design-review)
---

# Custom Coding Rules

Inherited from the `LordKuper.Common` parent library and the LordKuper RimWorld-mod house standard (shared with `rimworld-equipmentmanager` / `rimworld-workmanager`), adapted to OutfitManager. These are the **target** standard; OM is mid-migration from the legacy stack, so call out any gap rather than silently assuming the target is already in place.

## Nullability

- **Standard: `<Nullable>enable</Nullable>` for all projects.** This is NOT yet enabled on `OutfitManager.csproj` (legacy). Enabling it (and the test project) is target migration work; until then, new code should still be written nullable-clean so the eventual switch is painless.
- Use C# nullable reference types (`T?` + guards) for nullability intent. JetBrains nullability attributes (`[CanBeNull]`/`[NotNull]`/`[ItemNotNull]`) MUST NOT be used. Non-nullability JetBrains attributes (`[UsedImplicitly]`, `[Pure]`) are still allowed and already in use (`OutfitManagerMod`, `JobGiverPatch`).
- Scribe-serialized collection fields are declared nullable (`List<…>? = []` / `Dictionary<…>? = new()`) and restored with `??=` in `Initialize()`, because `Scribe_Collections.Look` writes `null` back on empty load. Scalar reference fields set by `Scribe`/reflection use `= null!`.
- Do NOT disable the nullable context anywhere. Do NOT mask nullable warnings with `#pragma warning disable`.

## Zero warnings

- Target: source builds with `TreatWarningsAsErrors=true` and `WarningLevel 9999` in both Debug and Release. Code MUST compile warning-clean. A warning fails the build. `Microsoft.CodeAnalysis.NetAnalyzers` findings count as warnings.

## Build / lint flow

- Before `build`: run `jb-cleanup` (applies the solution code-cleanup profile).
- After `lint`: run `jb-inspect`, then verify `TestResults/jb-inspect.sarif` has no `error` or `warning` severity entries.
- Commands defined in `.asd/project/commands.yaml` (`jb-cleanup`, `jb-inspect`). These target the modernized `.slnx` stack; if OM is still legacy when a sprint runs, the migration to that stack is part of the work before these commands apply.

## Analyzer / linter suppressions

- Suppress findings only as a last resort — fix the real issue first.
- Prefer attribute-based suppression (`[SuppressMessage]`, `[UsedImplicitly]`, `[Pure]`) over comment pragmas (`#pragma warning disable`, `// ReSharper disable`). Use comments only when no attribute applies.
- Every suppression MUST carry a real reason saying *why*. "false positive" / "by design" alone is not enough.

## Harmony transpiler safety

- `JobGiverPatch` is a **transpiler** that pattern-matches a specific IL sequence in `JobGiver_OptimizeApparel.ApparelScoreRaw`. When that pattern is not found it logs an error and returns the original IL (fail-soft) rather than throwing — preserve that behaviour. Any change to the matched opcode sequence MUST keep the fail-soft path and log via the project `Logger`.
- Prefer the narrowest, most stable injection point. Do not broaden the patch surface without strong reason.

## Self-contained code — no design-doc references

- The codebase (Source AND Tests) MUST be self-sufficient without the ASD design docs. Code and comments MUST NOT reference or quote ASD artifacts: ADR, PRD, acceptance criteria (`AC-N`), improvement items (`IMP-N`), `Task N`, sprint ids, or rule-doc filenames (`custom-*-rules.md`).
- Explain the *why* directly in the comment instead of citing a doc.
- The only exception: forward-looking `TODO`/`FIXME` comments MAY reference a sprint/issue for future work.

## Logging

- Use the project `Logger` (`Source/Logger.cs`), which wraps `LordKuper.Common.Logger` with the mod id (`LogMessage` / `LogWarning` / `LogError`). Actionable, gated, no spam. Prefer it over raw `Verse.Log.*`.

## Testing (NUnit + FluentAssertions)

- There is **no test project yet**; creating `Source/OutfitManager.Tests/` on the NUnit 4.x + FluentAssertions 7.x stack is target migration work. The rules below apply to any test code added.
- Test framework is **NUnit 4.x** (`[Test]`, `[TestCase]`, `[TestFixture]`, `[SetUp]`/`[TearDown]`, `[SetUpFixture]`/`[OneTimeSetUp]`); runner is `NUnit3TestAdapter`; host is `Microsoft.NET.Test.Sdk`. FluentAssertions is pinned to 7.x (Apache-2.0); never float to 8.x (commercial license).
- **All assertions MUST use FluentAssertions (`.Should()`) wherever possible.** Do NOT use NUnit `Assert.*` / `Assert.That` / `ClassicAssert.*` for value, state, or reference checks. Map every common shape to its FluentAssertions form:
  - equality/identity → `actual.Should().Be(expected)` / `.BeSameAs(...)` / `.NotBeNull()`
  - booleans → `flag.Should().BeTrue()/BeFalse()`
  - collections → `coll.Should().BeEmpty()/HaveCount(n)/Contain(x)/BeEquivalentTo(...)/NotContain(...)`
  - exceptions → `act.Should().Throw<T>()` / `.NotThrow()` (wrap the call in an `Action`/`Func`), NOT `Assert.Throws`
  - numeric tolerance → `value.Should().BeApproximately(expected, precision)`
  - reference null-state → `obj.Should().BeNull()/NotBeNull()`
  - The only non-FluentAssertions test constructs allowed are NUnit structural attributes (`[Test]`, `[TestCase]`, `[SetUp]`, `[Ignore("reason")]`, etc.) and genuine non-assertion control flow. `Assert.Fail`/`Assert.Pass`/`Assert.Inconclusive` are NOT assertions of behavior — prefer a real `.Should()` assertion; use `[Ignore]` for legitimately un-runnable tests rather than an empty/`Assert.Pass` body.
  - "Wherever possible" carve-out: only when a check genuinely has no FluentAssertions equivalent (extremely rare) may a non-FA construct be used, with an inline comment stating why.
- Use global `<Using Include="NUnit.Framework" />` and `<Using Include="FluentAssertions" />` rather than per-file `using` directives.
- **Static state isolation** — tests mutating global/cached/static state (e.g. `ApparelCache`, mod `Settings`) MUST save/restore via per-test `[SetUp]` (snapshot) / `[TearDown]` (restore) on a shared base class. Use per-test `[SetUp]`/`[TearDown]` (not per-class `[OneTimeSetUp]`) so each test gets true isolation. NUnit runs non-parallel by default; mark static-touching classes `[NonParallelizable]` and never add `[assembly: Parallelizable]`.
- RimWorld-typed test types require the RimWorld `AppDomain.AssemblyResolve` handler registered before any such type loads, via a namespace-less (global) `[SetUpFixture]`. Do not duplicate or bypass it.
- Do not depend on test execution order.
- RimWorld APIs requiring live game context must be abstracted or guarded; don't call them directly in unit tests without isolation.
