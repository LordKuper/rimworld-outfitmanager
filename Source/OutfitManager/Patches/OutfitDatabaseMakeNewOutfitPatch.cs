using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;

namespace OutfitManager.Patches
{
    [HarmonyPatch(typeof(OutfitDatabase), nameof(OutfitDatabase.MakeNewOutfit))]
    public static class OutfitDatabaseMakeNewOutfitPatch
    {
        [UsedImplicitly]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var oldConstructor = AccessTools.Constructor(typeof(Outfit), new[] {typeof(int), typeof(string)});
            var newConstructor = AccessTools.Constructor(typeof(ExtendedOutfit), new[] {typeof(int), typeof(string)});
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Newobj && oldConstructor.Equals(instruction.operand))
                {
                    instruction.operand = newConstructor;
                }
                yield return instruction;
            }
        }
    }
}