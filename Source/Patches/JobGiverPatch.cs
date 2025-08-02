using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;

namespace LordKuper.OutfitManager.Patches;

/// <summary>
///     Harmony patch for <see cref="JobGiver_OptimizeApparel.ApparelScoreRaw" /> to inject custom apparel scoring logic.
/// </summary>
[HarmonyPatch(typeof(JobGiver_OptimizeApparel), nameof(JobGiver_OptimizeApparel.ApparelScoreRaw))]
[UsedImplicitly]
internal static class JobGiverPatch
{
    /// <summary>
    ///     Transpiler that injects a call to <see cref="ApparelScoring.GetPawnApparelWorkScore" /> into the apparel score
    ///     calculation.
    /// </summary>
    /// <param name="instructions">The original IL instructions.</param>
    /// <returns>The modified IL instructions with the custom scoring logic injected.</returns>
    [HarmonyTranspiler]
    [UsedImplicitly]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var specialScoreMethod = AccessTools.Method(typeof(Apparel), nameof(Apparel.GetSpecialApparelScoreOffset));
        var insertionIndex = -1;
        var code = new List<CodeInstruction>(instructions);
        for (var i = 0; i < code.Count - 4; i++)
        {
            if (code[i].opcode == OpCodes.Ldloc_0 && code[i + 1].opcode == OpCodes.Ldarg_1 &&
                code[i + 2].opcode == OpCodes.Callvirt && specialScoreMethod.Equals(code[i + 2].operand) &&
                code[i + 3].opcode == OpCodes.Add && code[i + 4].opcode == OpCodes.Stloc_0)
            {
                insertionIndex = i;
                break;
            }
        }
        if (insertionIndex == -1)
        {
            Logger.LogError("Could not apply JobGiver patch.");
            return code;
        }
        Logger.LogMessage("Applying JobGiver patch.");
        var newInstructions = new List<CodeInstruction>
        {
            new(OpCodes.Ldloc_0), new(OpCodes.Ldarg_0),
            new(OpCodes.Ldarg_1),
            new(CodeInstruction.Call(typeof(ApparelScoring), nameof(ApparelScoring.GetPawnApparelWorkScore))),
            new(OpCodes.Add), new(OpCodes.Stloc_0)
        };
        code.InsertRange(insertionIndex, newInstructions);
        return code;
    }
}