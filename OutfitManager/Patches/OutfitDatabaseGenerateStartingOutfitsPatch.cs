using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Harmony;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace OutfitManager.Patches
{
    [HarmonyPatch(typeof(OutfitDatabase), "GenerateStartingOutfits")]
    public static class OutfitDatabaseGenerateStartingOutfitsPatch
    {
        private static void ConfigureOutfitFiltered(ExtendedOutfit outfit, IEnumerable<StatPriority> priorities,
            Func<ThingDef, bool> filter)
        {
            outfit.filter.SetDisallowAll();
            outfit.filter.SetAllow(SpecialThingFilterDefOf.AllowDeadmansApparel, false);
            foreach (var current in DefDatabase<ThingDef>.AllDefs.Where(filter))
            {
                outfit.filter.SetAllow(current, true);
            }
            outfit.AddStatPriorities(priorities);
        }

        internal static void GenerateStartingOutfits(OutfitDatabase db, bool vanilla = true)
        {
            #if DEBUG
            Log.Message("OutfitManager: Discovered outfit stats -----");
            foreach (var stat in OutfitStatHelper.AllAvailableStats)
            {
                Log.Message($"OutfitManager: {stat.defName} - {stat.LabelCap}");
            }
            Log.Message("OutfitManager: -----------------------------");
            Log.Message("OutfitManager: Generating starting outfits");
            #endif
            if (vanilla)
            {
                MakeOutfit(db, "Anything", true).AddStatPriorities(StatPriorityHelper.BaseWorkerStatPriorities);
                ConfigureOutfitFiltered(MakeOutfit(db, "Worker", true), StatPriorityHelper.BaseWorkerStatPriorities,
                    d => d.apparel?.defaultOutfitTags?.Contains("Worker") ?? false);
            }
            MakeOutfit(db, "Doctor").AddStatPriorities(StatPriorityHelper.DoctorWorkTypeStatPriorities);
            MakeOutfit(db, "Warden").AddStatPriorities(StatPriorityHelper.WardenWorkTypeStatPriorities);
            MakeOutfit(db, "Handler").AddStatPriorities(StatPriorityHelper.HandlingWorkTypeStatPriorities);
            MakeOutfit(db, "Cook").AddStatPriorities(StatPriorityHelper.CookingWorkTypeStatPriorities);
            MakeOutfit(db, "Hunter").AddStatPriorities(StatPriorityHelper.HuntingWorkTypeStatPriorities);
            MakeOutfit(db, "Builder").AddStatPriorities(StatPriorityHelper.ConstructionWorkTypeStatPriorities);
            MakeOutfit(db, "Grower").AddStatPriorities(StatPriorityHelper.GrowingWorkTypeStatPriorities);
            MakeOutfit(db, "Miner").AddStatPriorities(StatPriorityHelper.MiningWorkTypeStatPriorities);
            MakeOutfit(db, "Smith").AddStatPriorities(StatPriorityHelper.SmithingWorkTypeStatPriorities);
            MakeOutfit(db, "Tailor").AddStatPriorities(StatPriorityHelper.TailoringWorkTypeStatPriorities);
            MakeOutfit(db, "Artist").AddStatPriorities(StatPriorityHelper.ArtWorkTypeStatPriorities);
            MakeOutfit(db, "Crafter").AddStatPriorities(StatPriorityHelper.CraftingWorkTypeStatPriorities);
            MakeOutfit(db, "Hauler").AddStatPriorities(StatPriorityHelper.HaulingWorkTypeStatPriorities);
            MakeOutfit(db, "Cleaner").AddStatPriorities(StatPriorityHelper.CleaningWorkTypeStatPriorities);
            MakeOutfit(db, "Researcher").AddStatPriorities(StatPriorityHelper.ResearchWorkTypeStatPriorities);
            MakeOutfit(db, "Brawler").AddStatPriorities(StatPriorityHelper.BrawlerStatPriorities);
            if (vanilla)
            {
                MakeOutfit(db, "Soldier").AddStatPriorities(StatPriorityHelper.SoldierStatPriorities);
                ConfigureOutfitFiltered(MakeOutfit(db, "Nudist", true), StatPriorityHelper.BaseWorkerStatPriorities,
                    d => d.apparel?.bodyPartGroups.All(g =>
                             !new[] {BodyPartGroupDefOf.Legs, BodyPartGroupDefOf.Torso}.Contains(g)) ?? false);
            }
        }

        private static ExtendedOutfit MakeOutfit(OutfitDatabase database, string name, bool autoWorkPriorities = false)
        {
            #if DEBUG
            Log.Message($"Outfit Manager: Creating outfit {name}");
            #endif
            if (!(database.MakeNewOutfit() is ExtendedOutfit outfit))
            {
                Log.Error("Outfit Manager: outfit is not of type ExtendedOutfit");
                return null;
            }
            outfit.label = ("Outfit" + name).Translate();
            outfit.AutoWorkPriorities = autoWorkPriorities;
            return outfit;
        }

        [UsedImplicitly]
        [SuppressMessage("Design", "CA1031:Do not catch general exception types")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("Code Quality", "IDE0051:Remove unused private members")]
        private static bool Prefix(OutfitDatabase __instance)
        {
            try
            {
                GenerateStartingOutfits(__instance);
            }
            catch (Exception e)
            {
                Log.Error("OutfitManager: Could not generate outfits - " + e);
            }
            return false;
        }
    }
}