---
responsibility:
  owns: project-vetted reference for one technology version (apis used, version specifics, project conventions)
  excludes: adr rationale, code, full stack overview, build commands
  delegates_to: stack.html (overview), adr/ (decisions), commands.yaml (commands)
---

# .NET SDK @ 10.0.300

## Canonical source
- Official docs: https://learn.microsoft.com/dotnet/core/releases-and-support
- Last verified: 2026-06-07

## API surface used in project
- `dotnet build`: builds `Source/OutfitManager.slnx` (`-c Release`). Drives MSBuild and the bundled Roslyn compiler.
- `dotnet test`: builds and runs `Source/OutfitManager.slnx` tests (NUnit via NUnit3TestAdapter, hosted by Microsoft.NET.Test.Sdk) — once the test project exists.
- `dotnet format`: linting — `dotnet format Source/OutfitManager.slnx --verify-no-changes` for CI verification; `dotnet format Source/OutfitManager.slnx` to apply.
- Bundled MSBuild + NuGet restore: resolve `PackageReference` (Lib.Harmony, Microsoft.CodeAnalysis.NetAnalyzers, and test packages — target state) and `HintPath` references (RimWorld `Assembly-CSharp`, Unity modules, `LordKuper.Common`).
- Commands are the SSoT in `.asd/project/commands.yaml`; this doc does not duplicate or override them.

## Version-specific notes
- .NET 10 is a Long Term Support (LTS) release, supported until November 2028.
- The SDK uses feature bands: the third version segment groups in hundreds. `10.0.300` is the `10.0.3xx` feature band. Installing a higher patch in the same band (e.g. `10.0.301`) removes the prior one; a different band (`10.0.1xx`) installs side by side. As of mid-2026 the released 10.0 SDK servicing patches sit in the `10.0.3xx` band; treat the exact latest patch as machine-resolved.
- No `global.json` is present in the repo, so the build uses whichever compatible 10.0 SDK is installed on the machine (currently 10.0.300). If reproducibility across machines becomes a requirement, pin via `global.json` — that is a decision for an ADR, not assumed here.
- Builds the `.slnx` XML solution format (`Source/OutfitManager.slnx`), a recent solution format supported by current SDK/MSBuild. OM currently ships the legacy `Source/OutfitManager.sln`; the migration to `.slnx` is part of the legacy modernization.
- The SDK is a multi-targeting host: it itself runs on the .NET 10 runtime but compiles the project's `net48` target framework. There is no .NET 10 runtime dependency in the shipped mod assembly — output is a net48 assembly loaded by RimWorld into `1.6/Assemblies/`.

## Deprecations and breaking changes from prior version
- The repo will move from a `.sln` to a `.slnx` solution as part of the legacy migration; `Source/OutfitManager.sln` is replaced by `Source/OutfitManager.slnx`, which `commands.yaml` will reference. Older tooling that only understands `.sln` will not open this solution.
- The current legacy `.csproj` is a non-SDK project (`ToolsVersion 15.0`, explicit `Compile Include` list, `packages.config`); modernizing to SDK-style is a prerequisite for the `.slnx` + `PackageReference` flow this SDK assumes.
- Newer SDK bands may enable additional default analyzer rules and code-style behaviors, but analyzer rule selection here is governed by the pinned `Microsoft.CodeAnalysis.NetAnalyzers` 9.0.0 package, not the SDK band (see that tech-reference).

## Project conventions
- All four required commands target the `.slnx` solution, not individual csproj files (target state).
- Build/lint flow: run `jb cleanupcode` before `dotnet build`; run `dotnet format ... --verify-no-changes` for lint, then `jb inspectcode` after, verifying the SARIF has no error/warning entries.
- Release builds use `DebugType=portable`; warnings are errors in every configuration (target — the legacy csproj currently uses `DebugType pdbonly` and does not enforce warnings-as-errors).

## Known issues and workarounds
- 10.x toolchain is newer than common training/reference baselines (MEDIUM risk): MSBuild, NuGet, and the bundled Roslyn may behave or default differently than older SDKs. Verify behavior against the installed SDK rather than assuming pre-10.x defaults; check `dotnet --version` / `dotnet --info` when diagnosing build differences.
- Without a `global.json`, a contributor with a different installed SDK band can get different MSBuild/analyzer defaults. Workaround if this bites: add a `global.json` pinning the band (ADR-level decision).
- `CheckSdkVulnerabilities` MSBuild property can be set to `true` to emit a build warning (NETSDK1239) when running on an end-of-life SDK; not currently enabled here.
